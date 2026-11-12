using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Reports;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>
/// redix-api #119: an approved entry is removed softly by its own permission (D1), a report issued before keeps it (D2), nothing is reversed any more (D3), and an entry reversed
/// before reads as an ordinary approved one (D5). The credit note, a plain minus row (D4), is in <see cref="CreditNoteTests"/>. No figure here is real.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RemoveApprovedEntryTests(MigratedApiFactory factory)
{
    private static readonly System.Text.Json.JsonSerializerOptions Json = Ledger.Json;

    /// <summary>Someone who reads the project and holds the removal alone.</summary>
    private static async Task<(HttpClient Client, Guid UserId)> RemoverAsync(Ledger ledger)
    {
        var (client, userId) = await ledger.MemberAsync("Reader");
        await ledger.Factory.GiveAsync(userId, ledger.ProjectId, [Permission.RemoveApprovedEntries]);
        return (client, userId);
    }

    private static Task<HttpResponseMessage> DeleteAsync(HttpClient client, Ledger ledger, Guid id) => client.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{id}");

    private static async Task<Guid> ApprovedAsync(Ledger ledger, EntryRequest request) => await ledger.ApproveAsync(await ledger.PostAsync(request with { Submit = true }));

    private static Task<EntryStatus> StatusOfAsync(Ledger ledger, Guid id) => ledger.ReadAsync(db => db.Entries.Where(row => row.Id == id).Select(row => row.Status).SingleAsync());

    private static async Task<T> ReadAsync<T>(Ledger ledger, string path) => (await ledger.Client.GetFromJsonAsync<T>($"/projects/{ledger.ProjectId}/{path}", Json))!;

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name), Json)).EnsureSuccessStatusCode();
        return (await ReadAsync<List<PartnerView>>(ledger, "partners")).Single(row => row.Name == name).Id;
    }

    [Fact]
    public async Task D1_WhoeverHoldsTheRemovalRemovesAnApprovedEntrySoftlyAndTheHistorySaysWhoAndWhen()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, removerId) = await RemoverAsync(ledger);
        var id = await ApprovedAsync(ledger, ledger.Purchase(200m));

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        Assert.Equal(EntryStatus.Removed, await StatusOfAsync(ledger, id));
        Assert.Equal(200m, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == id).Select(row => row.Amount).SingleAsync()));
        Assert.True(await ledger.ReadAsync(db => db.ExpenseDetails.AnyAsync(row => row.EntryId == id)));
        var removal = (await factory.HistoryOfAsync(id)).Last();
        Assert.Equal((HistoryAction.Delete, removerId), (removal.Action, removal.UserId));
        Assert.Contains("\"Status\":2", removal.Before);
        Assert.Contains("\"Status\":4", removal.After);
    }

    [Fact]
    public async Task D1_WithoutTheRemovalAnApprovedEntryStaysWhateverElseTheyHold()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");
        var id = await ApprovedAsync(ledger, ledger.Purchase(200m));

        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteAsync(ledger.Client, ledger, id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteAsync(field, ledger, id)).StatusCode);

        Assert.Equal(EntryStatus.Approved, await StatusOfAsync(ledger, id));
    }

    [Fact]
    public async Task D1_NobodyHoldsTheRemovalUntilItIsTicked()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        Assert.DoesNotContain(Permission.RemoveApprovedEntries, ProjectPermission.OfWhoeverStarts);
        Assert.DoesNotContain(Permission.RemoveApprovedEntries, (await ledger.Client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single(row => row.Id == ledger.ProjectId).Permissions);
    }

    [Fact]
    public async Task D1_TheRemovalAloneDeletesNothingThatIsNotApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var waiting = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(waiting);

        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteAsync(remover, ledger, waiting.Id)).StatusCode);

        Assert.Equal(EntryStatus.PendingApproval, await StatusOfAsync(ledger, waiting.Id));
    }

    [Fact]
    public async Task D1_ARemovedEntryLeavesEveryListAndIsNotFoundByItsId()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var kept = await ApprovedAsync(ledger, ledger.Purchase(100m));
        var id = await ApprovedAsync(ledger, ledger.Purchase(200m));

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        Assert.Equal([kept], (await ReadAsync<List<EntryRowView>>(ledger, "entries")).Select(row => row.Id));
        Assert.Equal([kept], (await ReadAsync<List<EntryRowView>>(ledger, "entries/mine")).Select(row => row.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/entries/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/entries/{id}/row")).StatusCode);
    }

    [Fact]
    public async Task D1_ARemovedEntryIsNotRemovedChangedOrNotedAgain()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var request = ledger.Purchase(200m);
        var id = await ApprovedAsync(ledger, request);
        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.NotFound, (await DeleteAsync(remover, ledger, id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.ChangeAsync(request with { Amount = 9m })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/notes", new ChangeNotesRequest("بعد الحذف"), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{id}/submit", null)).StatusCode);
        Assert.Equal(EntryStatus.Removed, await StatusOfAsync(ledger, id));
    }

    [Fact]
    public async Task D1_ARemovedEntryLeavesTheSummaryItsAccountAndItsItem()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        await ApprovedAsync(ledger, ledger.Purchase(100m));
        var id = await ApprovedAsync(ledger, ledger.Purchase(300m));
        Assert.Equal(400m, (await ReadAsync<SummaryView>(ledger, "summary")).Expenses);

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        var summary = await ReadAsync<SummaryView>(ledger, "summary");
        Assert.Equal(100m, summary.Expenses);
        Assert.Equal(100m, summary.Categories.Sum(category => category.Total));
        Assert.Equal(-100m, (await ReadAsync<List<AccountView>>(ledger, "accounts")).Single(row => row.Id == ledger.AccountId).Balance);
    }

    [Fact]
    public async Task D1_ARemovedPaymentLeavesWhatItsContractPaid()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var item = (await ReadAsync<List<CategoryView>>(ledger, $"categories/offered?itemType={ItemType.ContractorPayments}")).SelectMany(category => category.Items).First();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, item.Id, 34000m, false), Json)).EnsureSuccessStatusCode();
        var contract = (await ReadAsync<List<ContractView>>(ledger, "contracts")).Single();
        var payment = new EntryRequest(Guid.CreateVersion7())
        {
            ItemId = item.Id,
            TableId = contract.Id,
            Date = ledger.Today,
            Amount = 3000m,
            PaymentMethodId = ledger.MethodId,
            AccountId = ledger.AccountId,
            AttachmentId = await ledger.FileAsync(),
        };
        var id = await ApprovedAsync(ledger, payment);
        Assert.Equal(3000m, (await ReadAsync<List<ContractView>>(ledger, "contracts")).Single().Paid);

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        var after = (await ReadAsync<List<ContractView>>(ledger, "contracts")).Single();
        Assert.Equal((0m, 0m, 34000m), (after.Paid, after.Pending, after.Remaining));
    }

    [Fact]
    public async Task D1_ARemovedMovementLeavesItsPartnersCapitalAndNoLongerHoldsThePartner()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var partner = await PartnerAsync(ledger, "شريك");
        var id = await ledger.ApproveAsync(await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 1000m, notes: "زاد رصيده")));
        Assert.Equal(1000m, (await ReadAsync<CapitalView>(ledger, "capital")).Total);

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await ReadAsync<CapitalView>(ledger, "capital")).Total);
        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/partners/{partner}")).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task D1_TheHistoriesLeaveTheRemovedEntrysApproval()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        var id = await ApprovedAsync(ledger, ledger.Purchase(200m));
        Assert.Single(await ReadAsync<List<ApprovalHistoryLine>>(ledger, "approvals/history"));
        Assert.Single(await ReadAsync<List<AccountHistoryLine>>(ledger, "accounts/history"), line => line.Kind == AccountHistoryKind.EntryOut);

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();

        Assert.Empty(await ReadAsync<List<ApprovalHistoryLine>>(ledger, "approvals/history"));
        Assert.DoesNotContain(await ReadAsync<List<AccountHistoryLine>>(ledger, "accounts/history"), line => line.Kind == AccountHistoryKind.EntryOut);
    }

    [Fact]
    public async Task D2_AReportIssuedBeforeKeepsTheRemovedEntryAndTheNextShowsTheDifference()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, _) = await RemoverAsync(ledger);
        await ApprovedAsync(ledger, ledger.Purchase(100m));
        var id = await ApprovedAsync(ledger, ledger.Purchase(40m));
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/periodic-reports", null)).EnsureSuccessStatusCode();
        factory.Time.Advance(TimeSpan.FromMinutes(1));

        (await DeleteAsync(remover, ledger, id)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/periodic-reports", null)).EnsureSuccessStatusCode();

        var first = await ReadAsync<PeriodicReportSnapshot>(ledger, "periodic-reports/1");
        var second = await ReadAsync<PeriodicReportSnapshot>(ledger, "periodic-reports/2");
        Assert.Equal(140m, first.Expenses);
        Assert.Equal((140m, 100m, -40m), (second.PreviousExpenses, second.Expenses, second.ExpensesDifference));
    }

    [Fact]
    public async Task D3_NothingIsReversedAnyMore()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await ApprovedAsync(ledger, ledger.Purchase(200m));

        var answer = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reverse", new RejectEntryRequest("إدخال خاطئ"), Json);

        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
        Assert.Equal(1, await ledger.ReadAsync(db => db.Entries.CountAsync(row => row.ProjectId == ledger.ProjectId)));
    }

    [Fact]
    public async Task D5_AnEntryReversedBeforeReadsAsAnOrdinaryApprovedEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, reverserId) = await ledger.MemberAsync("Reader");
        await ApprovedAsync(ledger, ledger.Purchase(200m));
        var opposite = ledger.Deduction(-200m, "عكس قيد قديم");
        await ledger.RecordAsync(opposite);
        await ledger.SetStatusAsync(opposite.Id, EntryStatus.Approved);
        await factory.WithDatabaseAsync(async db =>
        {
            db.AddHistory(await db.Entries.SingleAsync(row => row.Id == opposite.Id), HistoryAction.Reverse, reverserId, factory.Time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        });

        var lines = (await ReadAsync<List<AccountHistoryLine>>(ledger, "accounts/history")).Where(line => line.EntryKind == EntryKind.Purchase && line.Amount == 200m).ToList();

        Assert.Equal([AccountHistoryKind.EntryIn, AccountHistoryKind.EntryOut], lines.Select(line => line.Kind));
        Assert.Equal(0m, (await ReadAsync<List<AccountView>>(ledger, "accounts")).Single(row => row.Id == ledger.AccountId).Balance);
    }
}
