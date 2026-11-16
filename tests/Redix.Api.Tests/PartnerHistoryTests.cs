using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Partners;

namespace Redix.Api.Tests;

/// <summary>redix-app #101: the partners' history, the accounts' history for partners, seen through BR-31.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class PartnerHistoryTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<List<PartnerHistoryLine>> HistoryAsync(HttpClient client, Ledger ledger, string query = "") =>
        (await client.GetFromJsonAsync<List<PartnerHistoryLine>>($"/projects/{ledger.ProjectId}/partners/history{query}", Json))!;

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name, Guid userId = default)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    private static async Task MoveAsync(Ledger ledger, Guid partnerId, decimal amount, bool isWithdrawal = false) =>
        await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partnerId, amount, isWithdrawal), Json));

    [Fact]
    public async Task UC_101_EachPartnerActionAndApprovedCapitalMovementIsALineNewestFirst()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        await factory.WithDatabaseAsync(db => db.Users.Where(user => user.Id == memberId).ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "نواف")));
        var khaled = await PartnerAsync(ledger, "خالد م");
        (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{khaled}", new PartnerRequest("خالد"), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{khaled}", new PartnerRequest("خالد") { UserId = memberId }, Json)).EnsureSuccessStatusCode();
        await MoveAsync(ledger, khaled, 1000);
        await MoveAsync(ledger, khaled, 200, isWithdrawal: true);
        var nour = await PartnerAsync(ledger, "نور");
        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/partners/{nour}")).EnsureSuccessStatusCode();

        var lines = await HistoryAsync(ledger.Client, ledger);

        Assert.Equal(
            [PartnerHistoryKind.PartnerDeleted, PartnerHistoryKind.PartnerCreated, PartnerHistoryKind.CapitalOut, PartnerHistoryKind.CapitalIn, PartnerHistoryKind.PartnerAccount, PartnerHistoryKind.PartnerChanged, PartnerHistoryKind.PartnerCreated],
            lines.Select(line => line.Kind));
        Assert.Equal(("نور", "خالد"), (lines[0].Partner, lines[^1].Partner));
        Assert.Equal("خالد م", lines.Single(line => line.Kind == PartnerHistoryKind.PartnerChanged).NameWas);
        Assert.Equal("نواف", lines.Single(line => line.Kind == PartnerHistoryKind.PartnerAccount).Login);
        var capitalIn = lines.Single(line => line.Kind == PartnerHistoryKind.CapitalIn);
        Assert.Equal((1000m, "خالد"), (capitalIn.Amount, capitalIn.Partner));
        Assert.NotEmpty(capitalIn.Account);
    }

    [Fact]
    public async Task AddingAPartnerRecordsThePartnerAndTheirLoginAloneWhateverElseTheSaveCarries()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        var account = ledger.AccountId;
        var accountLines = await ledger.ReadAsync(db => db.History.CountAsync(row => row.RecordId == account));
        var partner = Guid.Empty;

        await factory.WithDatabaseAsync(async db =>
        {
            (await db.Accounts.SingleAsync(row => row.Id == account)).Name = "حساب يُحفظ مع الشريك";
            Assert.Equal("", await PartnersEndpoints.AddAsync(ledger.ProjectId, "خالد", memberId, memberId, factory.Time.GetUtcNow().UtcDateTime, db));
            partner = await db.Partners.Where(row => row.ProjectId == ledger.ProjectId && row.Name == "خالد").Select(row => row.Id).SingleAsync();
        });

        var created = await ledger.ReadAsync(db => db.History.Where(row => row.RecordId == partner).Select(row => row.TableName).ToListAsync());
        Assert.Equal(["PartnerUsers", "Partners"], created.Order(StringComparer.Ordinal));
        Assert.Equal(accountLines, await ledger.ReadAsync(db => db.History.CountAsync(row => row.RecordId == account)));
        Assert.Equal("حساب يُحفظ مع الشريك", await ledger.ReadAsync(db => db.Accounts.Where(row => row.Id == account).Select(row => row.Name).SingleAsync()));
    }

    [Fact]
    public async Task R214_APartnerAddedWithALoginIsOneLineAndTakingTheLoginAwayIsALineWithNone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        var khaled = await PartnerAsync(ledger, "خالد", memberId);

        (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{khaled}", new PartnerRequest("خالد"), Json)).EnsureSuccessStatusCode();

        var lines = await HistoryAsync(ledger.Client, ledger);
        Assert.Equal([(PartnerHistoryKind.PartnerAccount, ""), (PartnerHistoryKind.PartnerCreated, "")], lines.Select(line => (line.Kind, line.Login)));
    }

    [Fact]
    public async Task R104_WhatWasTypedKeepsTheLinesWhosePartnerOrAmountHoldItWhicheverWayArabicIsSpelled()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var khaled = await PartnerAsync(ledger, "خالد");
        var nour = await PartnerAsync(ledger, "نور");
        await MoveAsync(ledger, khaled, 1500);
        await MoveAsync(ledger, nour, 700);

        var named = await HistoryAsync(ledger.Client, ledger, $"?text={Uri.EscapeDataString("نُور")}");
        var byAmount = await HistoryAsync(ledger.Client, ledger, "?text=1,500");

        Assert.Equal([(PartnerHistoryKind.CapitalIn, "نور"), (PartnerHistoryKind.PartnerCreated, "نور")], named.Select(line => (line.Kind, line.Partner)));
        Assert.Equal([(PartnerHistoryKind.CapitalIn, "خالد")], byAmount.Select(line => (line.Kind, line.Partner)));
    }

    [Fact]
    public async Task UC_101_OnePartnersLinesAndWhatICanBeKeptAlone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var khaled = await PartnerAsync(ledger, "خالد");
        var saad = await PartnerAsync(ledger, "سعد");
        await MoveAsync(ledger, saad, 500);

        var khaledsLines = await HistoryAsync(ledger.Client, ledger, $"?partner={khaled}");
        var mine = await HistoryAsync(ledger.Client, ledger, "?mine=true");

        Assert.Equal(["خالد"], khaledsLines.Select(line => line.Partner).Distinct());
        Assert.Equal(3, mine.Count);
    }

    [Fact]
    public async Task UC_101_BR_31_SomeoneWithChosenPartnersSeesOnlyTheirLinesAndSomeoneWithNeitherSeesNone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (chosenClient, chosenId) = await ledger.MemberAsync("Reader");
        var (otherClient, _) = await ledger.MemberAsync("Reader");
        var khaled = await PartnerAsync(ledger, "خالد");
        var saad = await PartnerAsync(ledger, "سعد");
        await MoveAsync(ledger, saad, 500);
        await ledger.ChooseAsync(chosenId, khaled);

        var seenByChosen = await HistoryAsync(chosenClient, ledger);
        var unseen = await chosenClient.GetAsync($"/projects/{ledger.ProjectId}/partners/history?partner={saad}");

        Assert.Equal(["خالد"], seenByChosen.Select(line => line.Partner).Distinct());
        Assert.Empty(await HistoryAsync(otherClient, ledger));
        Assert.Equal((HttpStatusCode.BadRequest, "unknownPartner"), (unseen.StatusCode, await Ledger.CodeOf(unseen)));
    }
}
