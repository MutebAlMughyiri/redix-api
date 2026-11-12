using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Partners;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>redix-api #100: a partner is deleted softly, only at zero capital with nothing unapproved naming them, and their movements stay in the books.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class PartnerDeleteTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<List<PartnerView>> PartnersAsync(Ledger ledger, HttpClient? client = null) =>
        (await (client ?? ledger.Client).GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!;

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name, Guid userId = default)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await PartnersAsync(ledger)).Single(row => row.Name == name).Id;
    }

    private static Task<HttpResponseMessage> DeleteAsync(Ledger ledger, Guid partnerId, HttpClient? client = null) =>
        (client ?? ledger.Client).DeleteAsync($"/projects/{ledger.ProjectId}/partners/{partnerId}");

    /// <summary>Someone in <paramref name="ledger"/>'s project holding <paramref name="holds"/>, signed in, and their id.</summary>
    private async Task<(HttpClient Client, Guid UserId)> HoldingAsync(Ledger ledger, IEnumerable<Permission> holds)
    {
        var user = await factory.AddUserAsync(Ledger.Password);
        await factory.GiveAsync(user.Id, ledger.ProjectId, holds);
        return (await factory.ClientSignedInAsAsync(user, Ledger.Password), user.Id);
    }

    private static async Task<HttpResponseMessage> MovementAsync(Ledger ledger, Guid partnerId, decimal amount, bool isWithdrawal = false) =>
        await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partnerId, amount, isWithdrawal), Json);

    [Fact]
    public async Task UC_100_APartnerAtZeroCapitalIsDeletedSoftlyLeavingTheListsWhileTheirMovementsAndHistoryStayAndTheirAccountIsFree()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        var haitham = await PartnerAsync(ledger, "هيثم", memberId);
        var khaled = await PartnerAsync(ledger, "خالد");
        await ledger.ApproveAsync(await MovementAsync(ledger, khaled, 300));
        var cameIn = await ledger.ApproveAsync(await MovementAsync(ledger, haitham, 100));
        await ledger.ApproveAsync(await MovementAsync(ledger, haitham, 100, isWithdrawal: true));
        var capitalItem = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);

        Assert.True((await PartnersAsync(ledger)).Single(row => row.Id == haitham).MayDelete);
        (await DeleteAsync(ledger, haitham)).EnsureSuccessStatusCode();

        Assert.Equal(["خالد"], (await PartnersAsync(ledger)).Select(row => row.Name));
        var capital = (await ledger.Client.GetFromJsonAsync<CapitalView>($"/projects/{ledger.ProjectId}/capital", Json))!;
        Assert.Equal(300m, capital.Total);
        Assert.Equal([khaled], capital.Partners.Select(line => line.Id));
        var tables = (await ledger.Client.GetFromJsonAsync<List<ItemTableView>>($"/projects/{ledger.ProjectId}/items/{capitalItem}/tables", Json))!;
        Assert.DoesNotContain(tables, table => table.Id == haitham);
        var entries = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;
        Assert.Equal("هيثم", entries.Single(row => row.Id == cameIn).PartnerName);
        Assert.True(await ledger.ReadAsync(db => db.History.AnyAsync(row => row.RecordId == haitham && row.Action == HistoryAction.Delete)));
        Assert.True(await ledger.ReadAsync(db => db.Partners.Where(row => row.Id == haitham).Select(row => row.IsDeleted).SingleAsync()));
        Assert.Equal(HttpStatusCode.NotFound, (await DeleteAsync(ledger, haitham)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{haitham}", new PartnerRequest("هيثم"), Json)).StatusCode);
        await PartnerAsync(ledger, "هيثم الثاني", memberId);
    }

    [Fact]
    public async Task UC_100_APartnerWithCapitalOrAMovementWaitingIsNotDeletedAndOneNeverUsedIs()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var khaled = await PartnerAsync(ledger, "خالد");
        var saad = await PartnerAsync(ledger, "سعد");
        var nour = await PartnerAsync(ledger, "نور");
        await ledger.ApproveAsync(await MovementAsync(ledger, khaled, 300));
        (await MovementAsync(ledger, saad, 50)).EnsureSuccessStatusCode();

        var listed = (await PartnersAsync(ledger)).ToDictionary(row => row.Id, row => row.MayDelete);
        var withCapital = await DeleteAsync(ledger, khaled);
        var waiting = await DeleteAsync(ledger, saad);

        Assert.Equal((false, false, true), (listed[khaled], listed[saad], listed[nour]));
        Assert.Equal((HttpStatusCode.Conflict, "partnerNotDeletable"), (withCapital.StatusCode, await Ledger.CodeOf(withCapital)));
        Assert.Equal((HttpStatusCode.Conflict, "partnerNotDeletable"), (waiting.StatusCode, await Ledger.CodeOf(waiting)));
        (await DeleteAsync(ledger, nour)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Row_199_SomeoneElsesDraftHoldsThePartnerAndSaysWhoseWhileTheCallersOwnDraftsAreDeletedWithThem()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partnerId = await PartnerAsync(ledger, "شريك المسودات");
        var (other, otherId) = await ledger.MemberAsync("Finance");
        await factory.WithDatabaseAsync(db => db.Users.Where(user => user.Id == otherId).ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "نواف")));
        var othersDraft = await ledger.PartnerEntryAsync(partnerId, 500m) with { Submit = false };
        (await ledger.PostAsync(othersDraft, other)).EnsureSuccessStatusCode();
        var myDraft = await ledger.PartnerEntryAsync(partnerId, 300m) with { Submit = false };
        (await ledger.PostAsync(myDraft)).EnsureSuccessStatusCode();

        var held = (await PartnersAsync(ledger)).Single(row => row.Id == partnerId);
        var refused = await DeleteAsync(ledger, partnerId);

        Assert.False(held.MayDelete);
        Assert.Equal(["نواف"], held.OthersDrafts);
        Assert.Equal([300m], held.MyDrafts.Select(draft => draft.Amount));
        Assert.Equal((HttpStatusCode.Conflict, "partnerNotDeletable"), (refused.StatusCode, await Ledger.CodeOf(refused)));

        (await other.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{othersDraft.Id}")).EnsureSuccessStatusCode();
        var free = (await PartnersAsync(ledger)).Single(row => row.Id == partnerId);
        (await DeleteAsync(ledger, partnerId)).EnsureSuccessStatusCode();

        Assert.True(free.MayDelete);
        Assert.Empty(free.OthersDrafts);
        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(entry => entry.Id == myDraft.Id)));
        Assert.True(await ledger.ReadAsync(db => db.History.AnyAsync(row => row.Action == HistoryAction.Delete && row.RecordId == myDraft.Id)));
    }

    [Fact]
    public async Task Row_202_ADeleteThatTakesTheCallersOwnDraftsNeedsDeleteEntriesTooWhileOneWithNoDraftOfTheirsDoesNot()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (manager, managerId) = await HoldingAsync(ledger, Holds.Finance.Where(permission => permission != Permission.DeleteEntries));
        var drafted = await PartnerAsync(ledger, "شريك مسودتي");
        var free = await PartnerAsync(ledger, "شريك بلا مسودة");
        var draft = await ledger.PartnerEntryAsync(drafted, 300m) with { Submit = false };
        (await ledger.PostAsync(draft, manager)).EnsureSuccessStatusCode();

        var listed = (await PartnersAsync(ledger, manager)).ToDictionary(row => row.Id, row => row.MayDelete);
        var refused = await DeleteAsync(ledger, drafted, manager);

        Assert.Equal((false, true), (listed[drafted], listed[free]));
        Assert.Equal((HttpStatusCode.Conflict, "partnerNotDeletable"), (refused.StatusCode, await Ledger.CodeOf(refused)));
        Assert.True(await ledger.ReadAsync(db => db.Entries.AnyAsync(entry => entry.Id == draft.Id)));
        (await DeleteAsync(ledger, free, manager)).EnsureSuccessStatusCode();

        await factory.GiveAsync(managerId, ledger.ProjectId, [Permission.DeleteEntries]);
        Assert.True((await PartnersAsync(ledger, manager)).Single(row => row.Id == drafted).MayDelete);
        (await DeleteAsync(ledger, drafted, manager)).EnsureSuccessStatusCode();
        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(entry => entry.Id == draft.Id)));
    }

    [Fact]
    public async Task Row_203_WhoseDraftNamesAPartnerIsToldOnlyToWhoeverMayDeletePartners()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partnerId = await PartnerAsync(ledger, "شريك المسودة");
        var (author, authorId) = await ledger.MemberAsync("Finance");
        await factory.WithDatabaseAsync(db => db.Users.Where(user => user.Id == authorId).ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "نواف")));
        (await ledger.PostAsync(await ledger.PartnerEntryAsync(partnerId, 500m) with { Submit = false }, author)).EnsureSuccessStatusCode();
        var (reader, _) = await HoldingAsync(ledger, [Permission.ReadProject, Permission.SeeAllCapital]);

        var toTheManager = (await PartnersAsync(ledger)).Single(row => row.Id == partnerId);
        var toTheReader = (await PartnersAsync(ledger, reader)).Single(row => row.Id == partnerId);

        Assert.Equal(["نواف"], toTheManager.OthersDrafts);
        Assert.Empty(toTheReader.OthersDrafts);
        Assert.False(toTheReader.MayDelete);
    }

    [Fact]
    public async Task UC_100_ADeletedPartnerCannotBeChosenForSomeone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        var nour = await PartnerAsync(ledger, "نور");
        (await DeleteAsync(ledger, nour)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/users/{memberId}/partners/{nour}", null)).StatusCode);
    }
}
