using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>
/// redix-app #89, the account history (H1-H15): what was done to a project's accounts and what moved their money, and, first of all, that no line of another project ever
/// reaches it. No figure here is real.
/// </summary>
[Collection(FourthDatabaseCollection.Name)]
public class AccountHistoryTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static readonly AccountHistoryKind[] EntryKinds = [AccountHistoryKind.EntryIn, AccountHistoryKind.EntryOut, AccountHistoryKind.Reversed];

    private static string PathOf(Ledger ledger, Guid accountId = default, bool mine = false, long before = 0) =>
        $"/projects/{ledger.ProjectId}/accounts/history?account={accountId}&mine={mine}&before={before}";

    private static async Task<List<AccountHistoryLine>> HistoryAsync(Ledger ledger, HttpClient? client = null, Guid accountId = default, bool mine = false, long before = 0) =>
        (await (client ?? ledger.Client).GetFromJsonAsync<List<AccountHistoryLine>>(PathOf(ledger, accountId, mine, before), Json))!;

    private static async Task<Guid> AccountAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest(name, AccountType.Bank), Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == name).Id;
    }

    private static async Task FundAsync(Ledger ledger, decimal amount) => await ledger.ApproveAsync(await ledger.PostAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, amount)));

    private static async Task TransferAsync(Ledger ledger, Guid from, Guid to, decimal amount, HttpClient? client = null) =>
        (await (client ?? ledger.Client).PostAsJsonAsync($"/projects/{ledger.ProjectId}/transfers", new TransferRequest(from, to, amount, ledger.Today, ""), Json)).EnsureSuccessStatusCode();

    private static async Task<Guid> SentPurchaseAsync(Ledger ledger, decimal amount)
    {
        var request = ledger.Purchase(amount) with { Submit = true };
        await ledger.RecordAsync(request);
        return request.Id;
    }

    /// <summary>A project with a line of every kind History shows: an account added, a transfer, an approval, a reversal and a check.</summary>
    private async Task<(Ledger Ledger, Guid Custody)> BusyProjectAsync()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش مع مدير المشروع");
        await FundAsync(ledger, 1000m);
        await TransferAsync(ledger, ledger.AccountId, custody, 400m);
        var purchase = await ledger.ApproveAsync(await SentPurchaseAsync(ledger, 300m));
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{purchase}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 600m), new(custody, 400m)]), Json)).EnsureSuccessStatusCode();
        return (ledger, custody);
    }

    private static async Task<HashSet<long>> RowIdsOfAsync(Ledger ledger) =>
        [.. await ledger.ReadAsync(db => db.History.Where(row => row.ProjectId == ledger.ProjectId).Select(row => row.Id).ToListAsync())];

    [Fact]
    public async Task H1_AMemberSeesEveryKindOfLineWithWhoAndWhenNewestFirst()
    {
        var (ledger, _) = await BusyProjectAsync();
        var (reader, _) = await ledger.MemberAsync("Reader");
        await factory.WithDatabaseAsync(db => db.Users.Where(user => db.History.Any(row => row.ProjectId == ledger.ProjectId && row.UserId == user.Id)).ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "متعب")));

        var lines = await HistoryAsync(ledger, reader);

        Assert.Equal(
            [AccountHistoryKind.Check, AccountHistoryKind.Reversed, AccountHistoryKind.EntryOut, AccountHistoryKind.Transfer, AccountHistoryKind.EntryIn, AccountHistoryKind.AccountCreated, AccountHistoryKind.AccountCreated],
            lines.Select(line => line.Kind));
        Assert.Equal(lines.OrderByDescending(line => line.Id).Select(line => line.Id), lines.Select(line => line.Id));
        var transfer = lines.Single(line => line.Kind == AccountHistoryKind.Transfer);
        Assert.Equal(("حساب المؤسسة", "كاش مع مدير المشروع", 400m), (transfer.Account, transfer.OtherAccount, transfer.Amount));
        Assert.Equal((300m, EntryKind.Purchase, "حساب المؤسسة"), (lines.Single(line => line.Kind == AccountHistoryKind.EntryOut).Amount, lines.Single(line => line.Kind == AccountHistoryKind.EntryOut).EntryKind, lines.Single(line => line.Kind == AccountHistoryKind.EntryOut).Account));
        Assert.Equal(1000m, lines.Single(line => line.Kind == AccountHistoryKind.Check).Amount);
        Assert.All(lines, line => Assert.Equal("متعب", line.By));
        Assert.All(lines, line => Assert.Equal(factory.Time.GetUtcNow().UtcDateTime, line.At, TimeSpan.FromDays(1)));
        Assert.Contains(lines, line => line.Account == "كاش مع مدير المشروع" && line.Kind == AccountHistoryKind.AccountCreated);
    }

    [Fact]
    public async Task H2_AnotherProjectsLinesAtTheSameTimeNeverAppear()
    {
        var (a, _) = await BusyProjectAsync();
        var (b, _) = await BusyProjectAsync();
        var aRows = await RowIdsOfAsync(a);

        var lines = await HistoryAsync(a);

        Assert.All(lines, line => Assert.Contains(line.Id, aRows));
        Assert.NotEmpty(await HistoryAsync(b));
    }

    [Fact]
    public async Task H3_APersonInTwoProjectsSeesEachProjectsLinesOnlyUnderIt()
    {
        var (a, _) = await BusyProjectAsync();
        var (b, _) = await BusyProjectAsync();
        var (both, userId) = await a.MemberAsync("Reader");
        await factory.GiveAsync(userId, b.ProjectId, Holds.Reader);
        var (aRows, bRows) = (await RowIdsOfAsync(a), await RowIdsOfAsync(b));

        var underA = await HistoryAsync(a, both);
        var underB = await HistoryAsync(b, both);

        Assert.All(underA, line => Assert.Contains(line.Id, aRows));
        Assert.All(underB, line => Assert.Contains(line.Id, bRows));
        Assert.Empty(underA.Select(line => line.Id).Intersect(underB.Select(line => line.Id)));
    }

    [Fact]
    public async Task H4_SomeoneNotInTheProjectIsRefused()
    {
        var (a, _) = await BusyProjectAsync();
        var b = await Ledger.WithFinanceAsync(factory);

        Assert.Equal(HttpStatusCode.Forbidden, (await b.Client.GetAsync(PathOf(a))).StatusCode);
    }

    [Fact]
    public async Task H5_FilteringByAnotherProjectsAccountIsRefusedAndListsNothing()
    {
        var (a, _) = await BusyProjectAsync();
        var (b, _) = await BusyProjectAsync();

        var refused = await a.Client.GetAsync(PathOf(a, b.AccountId));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("unknownAccount", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task H6_PagingFromAnIdTakenFromAnotherProjectStillListsOnlyThisProjectsLines()
    {
        var (a, _) = await BusyProjectAsync();
        var (b, _) = await BusyProjectAsync();
        var newestOfB = (await HistoryAsync(b))[0].Id;
        var aRows = await RowIdsOfAsync(a);

        var lines = await HistoryAsync(a, before: newestOfB);

        Assert.NotEmpty(lines);
        Assert.All(lines, line => Assert.Contains(line.Id, aRows));
    }

    [Fact]
    public async Task H7_ADeletedAccountsLinesStayInItsProjectWithItsNameAndNeverInAnother()
    {
        var a = await Ledger.WithFinanceAsync(factory);
        var b = await Ledger.WithFinanceAsync(factory);
        var spare = await AccountAsync(a, "احتياطي");
        (await a.Client.PostAsync($"/projects/{a.ProjectId}/accounts/{spare}/delete", null)).EnsureSuccessStatusCode();

        var lines = await HistoryAsync(a);

        Assert.Equal([AccountHistoryKind.AccountDeleted, AccountHistoryKind.AccountCreated], lines.Where(line => line.Account == "احتياطي").Select(line => line.Kind));
        Assert.DoesNotContain(await HistoryAsync(b), line => line.Account == "احتياطي");
    }

    [Fact]
    public async Task H8_SomeoneRemovedFromTheProjectCanNoLongerReadItsHistory()
    {
        var (a, _) = await BusyProjectAsync();
        var (reader, userId) = await a.MemberAsync("Reader");
        Assert.NotEmpty(await HistoryAsync(a, reader));

        await factory.WithDatabaseAsync(db => db.ProjectPermissions.Where(row => row.UserId == userId && row.ProjectId == a.ProjectId).ExecuteDeleteAsync());

        Assert.Equal(HttpStatusCode.Forbidden, (await reader.GetAsync(PathOf(a))).StatusCode);
    }

    [Fact]
    public async Task H9_APartnersMoneyShowsOnlyToWhoeverSeesAllCapital()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("شريك"), Json)).EnsureSuccessStatusCode();
        var partner = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single().Id;
        await ledger.ApproveAsync(await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 5000m)));
        var (reader, _) = await ledger.MemberAsync("Reader");

        Assert.Contains(await HistoryAsync(ledger), line => line.EntryKind == EntryKind.CapitalIn && EntryKinds.Contains(line.Kind));
        Assert.DoesNotContain(await HistoryAsync(ledger, reader), line => line.EntryKind == EntryKind.CapitalIn && EntryKinds.Contains(line.Kind));
    }

    [Fact]
    public async Task H10_WhatIDidListsOnlyTheAskingPersonsOwnLines()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 1000m);
        var (second, _) = await ledger.MemberAsync("Finance");
        await TransferAsync(ledger, ledger.AccountId, custody, 100m, second);
        await TransferAsync(ledger, ledger.AccountId, custody, 50m);

        var mine = await HistoryAsync(ledger, second, mine: true);
        var all = await HistoryAsync(ledger, second);

        Assert.Equal([100m], mine.Select(line => line.Amount));
        Assert.Equal(2, all.Count(line => line.Kind == AccountHistoryKind.Transfer));
    }

    [Fact]
    public async Task H11_DraftsSendsAndRejectionsAreNotListed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(70m));
        await SentPurchaseAsync(ledger, 80m);
        var rejected = await SentPurchaseAsync(ledger, 90m);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{rejected}/reject", new RejectEntryRequest("غير واضح"), Json)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await HistoryAsync(ledger), line => EntryKinds.Contains(line.Kind));
    }

    [Fact]
    public async Task H13_EachActionWritesItsRowWithTheRightProject_AnEntryItsPartsATransferAndACheck()
    {
        var (ledger, _) = await BusyProjectAsync();
        var other = await Ledger.WithFinanceAsync(factory);

        var rows = await ledger.ReadAsync(db => db.History.Where(row => row.ProjectId == ledger.ProjectId).Select(row => row.TableName).Distinct().ToListAsync());

        Assert.Superset(new HashSet<string> { "Accounts", "Entries", "PurchaseDetails", "ExpenseDetails", "Transfers", "Reconciliations", "EntryLinks" }, rows.ToHashSet());
        Assert.False(await ledger.ReadAsync(db => db.History.AnyAsync(row => row.ProjectId == other.ProjectId && row.TableName == "Transfers")));
    }

    [Fact]
    public async Task OneAccountsHistoryKeepsItsOwnLines()
    {
        var (ledger, custody) = await BusyProjectAsync();

        var lines = await HistoryAsync(ledger, accountId: custody);

        Assert.Equal([AccountHistoryKind.Check, AccountHistoryKind.Transfer, AccountHistoryKind.AccountCreated], lines.Select(line => line.Kind));
    }

    [Fact]
    public async Task R22_APageHoldsFiftyLinesAndTheNextStartsAfterItsLast()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        for (var i = 0; i < 30; i++)
        {
            (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/accounts/{ledger.AccountId}/disable", null)).EnsureSuccessStatusCode();
            (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/accounts/{ledger.AccountId}/enable", null)).EnsureSuccessStatusCode();
        }

        var first = await HistoryAsync(ledger);
        var second = await HistoryAsync(ledger, before: first[^1].Id);

        Assert.Equal(HistoryPage.Size, first.Count);
        Assert.Equal(11, second.Count);
        Assert.Empty(first.Select(line => line.Id).Intersect(second.Select(line => line.Id)));
        Assert.True(first[^1].Id > second[0].Id);
    }

    [Fact]
    public async Task H15_AfterTheRenameTheHistoryTableStillRefusesAnyChangeOrDeletion()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var row = (await factory.HistoryOfAsync(ledger.AccountId)).Single();

        await factory.WithDatabaseAsync(async db =>
        {
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => db.History.Where(kept => kept.Id == row.Id).ExecuteUpdateAsync(kept => kept.SetProperty(history => history.ProjectId, Guid.Empty)));
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() => db.History.Where(kept => kept.Id == row.Id).ExecuteDeleteAsync());
        });
        Assert.Equal(ledger.ProjectId, (await factory.HistoryOfAsync(ledger.AccountId)).Single().ProjectId);
    }
}
