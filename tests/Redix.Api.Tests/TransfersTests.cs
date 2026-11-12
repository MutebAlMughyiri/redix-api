using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>
/// redix-app #89: money moved between accounts, balances only entries and transfers move, deleting an account nothing names, and the note on an account that falls short.
/// No figure here is real.
/// </summary>
[Collection(SecondDatabaseCollection.Name)]
public class TransfersTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    public static TheoryData<decimal> NoAmounts => [0m, -5m, 1.234m, Money.Largest + 1];

    private static async Task<Guid> AccountAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest(name, AccountType.Bank), Json)).EnsureSuccessStatusCode();
        return (await AccountsAsync(ledger)).Single(row => row.Name == name).Id;
    }

    private static async Task<List<AccountView>> AccountsAsync(Ledger ledger) => (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!;

    private static async Task<AccountView> AccountOf(Ledger ledger, Guid accountId) => (await AccountsAsync(ledger)).Single(row => row.Id == accountId);

    private static async Task<decimal> BalanceAsync(Ledger ledger, Guid accountId) => (await AccountOf(ledger, accountId)).Balance;

    /// <summary>Puts <paramref name="amount"/> into the ledger's account with approved revenue: money enters an account only through entries and transfers (R10).</summary>
    private static async Task FundAsync(Ledger ledger, decimal amount) => await ledger.ApproveAsync(await ledger.PostAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, amount)));

    private static Task<HttpResponseMessage> TransferAsync(Ledger ledger, Guid from, Guid to, decimal amount, string notes = "", HttpClient? client = null) =>
        (client ?? ledger.Client).PostAsJsonAsync($"/projects/{ledger.ProjectId}/transfers", new TransferRequest(from, to, amount, ledger.Today, notes), Json);

    private static Task<decimal> SurplusAsync(Ledger ledger) => ledger.ReadAsync(async db => (await ProjectFigures.OfAsync(ledger.ProjectId, db)).Surplus);

    private static async Task<PendingEntryView> PendingAsync(Ledger ledger, Guid entryId) =>
        (await ledger.Client.GetFromJsonAsync<List<PendingEntryView>>($"/projects/{ledger.ProjectId}/entries/pending", Json))!.Single(row => row.Id == entryId);

    private static async Task<Guid> SentPurchaseAsync(Ledger ledger, decimal amount)
    {
        var request = ledger.Purchase(amount) with { Submit = true };
        await ledger.RecordAsync(request);
        return request.Id;
    }

    private static async Task AssertRefusedAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task FR_23_ATransferMovesMoneyBetweenTwoAccountsAtOnceAndNeverTheSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 1000m);
        var surplus = await SurplusAsync(ledger);

        (await TransferAsync(ledger, ledger.AccountId, custody, 400m, "تغذية العهدة")).EnsureSuccessStatusCode();

        Assert.Equal(600m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Equal(400m, await BalanceAsync(ledger, custody));
        Assert.Equal(surplus, await SurplusAsync(ledger));
        var kept = await ledger.ReadAsync(db => db.Transfers.SingleAsync(row => row.ProjectId == ledger.ProjectId));
        Assert.Equal((ledger.AccountId, custody, 400m, "تغذية العهدة"), (kept.FromAccountId, kept.ToAccountId, kept.Amount, kept.Notes));
    }

    [Fact]
    public async Task R26_ATransferMayMoveExactlyWhatTheAccountHoldsAndNotOneHalalaMore()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 500m);

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, custody, 500.01m), HttpStatusCode.BadRequest, "insufficientBalance");
        Assert.Equal(500m, await BalanceAsync(ledger, ledger.AccountId));

        (await TransferAsync(ledger, ledger.AccountId, custody, 500m)).EnsureSuccessStatusCode();
        Assert.Equal(0m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Equal(500m, await BalanceAsync(ledger, custody));
    }

    [Theory]
    [MemberData(nameof(NoAmounts))]
    public async Task R14_ATransferOfNoAmountIsRefusedAndNothingMoves(decimal amount)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, custody, amount), HttpStatusCode.BadRequest, "invalidAmount");

        Assert.Equal(100m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.False(await ledger.ReadAsync(db => db.Transfers.AnyAsync(row => row.ProjectId == ledger.ProjectId)));
    }

    [Fact]
    public async Task R14_ATransferToTheSameAccountIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 100m);

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, ledger.AccountId, 50m), HttpStatusCode.BadRequest, "sameAccount");
    }

    [Fact]
    public async Task R14_ATransferTouchingADisabledAccountIsRefusedEitherWay()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);
        (await TransferAsync(ledger, ledger.AccountId, custody, 50m)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/accounts/{custody}/disable", null)).EnsureSuccessStatusCode();

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, custody, 10m), HttpStatusCode.BadRequest, "accountDisabled");
        await AssertRefusedAsync(await TransferAsync(ledger, custody, ledger.AccountId, 10m), HttpStatusCode.BadRequest, "accountDisabled");
        Assert.Equal(50m, await BalanceAsync(ledger, custody));
    }

    [Fact]
    public async Task R14_AnAccountOfAnotherProjectIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 100m);

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, other.AccountId, 10m), HttpStatusCode.BadRequest, "unknownAccount");
        Assert.Equal(0m, await BalanceAsync(other, other.AccountId));
    }

    [Fact]
    public async Task NotesBeyondTheirLengthAreRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);

        await AssertRefusedAsync(await TransferAsync(ledger, ledger.AccountId, custody, 10m, new string('م', Transfer.NotesMaxLength + 1)), HttpStatusCode.BadRequest, "invalidNotes");
        (await TransferAsync(ledger, ledger.AccountId, custody, 10m, new string('م', Transfer.NotesMaxLength))).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task D1_WithoutMoveMoneyATransferIsRefusedAndNothingMoves()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);
        var (field, _) = await ledger.MemberAsync("Field");

        Assert.Equal(HttpStatusCode.Forbidden, (await TransferAsync(ledger, ledger.AccountId, custody, 10m, client: field)).StatusCode);
        Assert.Equal(100m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task R21_ATransferIsInTheHistoryWithWhoAndItsProject()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);

        (await TransferAsync(ledger, ledger.AccountId, custody, 30m)).EnsureSuccessStatusCode();

        var transfer = await ledger.ReadAsync(db => db.Transfers.SingleAsync(row => row.ProjectId == ledger.ProjectId));
        var record = Assert.Single(await factory.HistoryOfAsync(transfer.Id));
        Assert.Equal((HistoryAction.Create, ledger.ProjectId, transfer.CreatedByUserId), (record.Action, record.ProjectId, record.UserId));
    }

    [Fact]
    public async Task R2_ABalanceMovesWithApprovedEntriesBySignAndNotWithADraftOrAWait()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 1000m);
        await ledger.ApproveAsync(await SentPurchaseAsync(ledger, 300m));
        await SentPurchaseAsync(ledger, 50m);
        await ledger.RecordAsync(ledger.Purchase(70m));

        Assert.Equal(700m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task R4_NothingBlocksAnEntryAboveItsAccountsBalanceAndTheAccountGoesBelowZero()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        await ledger.ApproveAsync(await SentPurchaseAsync(ledger, 3000m));

        Assert.Equal(-3000m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task BR_15_AnAccountNothingNamesIsDeletedAndItsHistoryStaysInItsProject()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var spare = await AccountAsync(ledger, "احتياطي");
        Assert.False((await AccountOf(ledger, spare)).IsUsed);

        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/accounts/{spare}")).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await AccountsAsync(ledger), row => row.Id == spare);
        var history = await factory.HistoryOfAsync(spare);
        Assert.Equal([HistoryAction.Create, HistoryAction.Delete], history.Select(row => row.Action));
        Assert.All(history, row => Assert.Equal(ledger.ProjectId, row.ProjectId));
        Assert.Equal("احتياطي", JsonDocument.Parse(history[^1].Before).RootElement.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task BR_15_AnAccountADraftNamesIsUsedAndCannotBeDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(70m));

        await AssertRefusedAsync(await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/accounts/{ledger.AccountId}"), HttpStatusCode.Conflict, "accountInUse");

        Assert.True((await AccountOf(ledger, ledger.AccountId)).IsUsed);
    }

    [Fact]
    public async Task R15_AnAccountATransferNamesIsUsedAndCannotBeDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var custody = await AccountAsync(ledger, "كاش");
        await FundAsync(ledger, 100m);
        (await TransferAsync(ledger, ledger.AccountId, custody, 100m)).EnsureSuccessStatusCode();

        await AssertRefusedAsync(await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/accounts/{custody}"), HttpStatusCode.Conflict, "accountInUse");

        Assert.True((await AccountOf(ledger, custody)).IsUsed);
    }

    [Fact]
    public async Task R15_AnAccountACheckNamesIsUsedAndCannotBeDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var spare = await AccountAsync(ledger, "احتياطي");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 0m), new(spare, 0m)]), Json)).EnsureSuccessStatusCode();

        await AssertRefusedAsync(await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/accounts/{spare}"), HttpStatusCode.Conflict, "accountInUse");
    }

    [Fact]
    public async Task DeletingNeedsWhoeverKeepsTheSettings()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var spare = await AccountAsync(ledger, "احتياطي");
        var (field, _) = await ledger.MemberAsync("Field");

        Assert.Equal(HttpStatusCode.Forbidden, (await field.DeleteAsync($"/projects/{ledger.ProjectId}/accounts/{spare}")).StatusCode);
        Assert.Contains(await AccountsAsync(ledger), row => row.Id == spare);
    }

    [Fact]
    public async Task R21_AddingDisablingAndEnablingAnAccountAreInTheHistory()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var spare = await AccountAsync(ledger, "احتياطي");

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/accounts/{spare}/disable", null)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/accounts/{spare}/enable", null)).EnsureSuccessStatusCode();

        Assert.Equal([HistoryAction.Create, HistoryAction.Disable, HistoryAction.Enable], (await factory.HistoryOfAsync(spare)).Select(row => row.Action));
    }

    [Fact]
    public async Task R5_TheInboxSaysByHowMuchAPurchaseIsAboveItsAccountsBalance()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 2750m);

        var pending = await PendingAsync(ledger, await SentPurchaseAsync(ledger, 3000m));

        Assert.Equal((2750m, 3000m, 250m), (pending.AccountBalance, pending.AccountWaiting, pending.AccountShortBy));
    }

    [Fact]
    public async Task R5_APurchaseItsAccountCoversHasNoShortfall()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 1000m);

        var pending = await PendingAsync(ledger, await SentPurchaseAsync(ledger, 1000m));

        Assert.Equal((1000m, 0m), (pending.AccountBalance, pending.AccountShortBy));
    }

    [Fact]
    public async Task R5_TwoEntriesThatEachFitButNotTogetherAreBothNotedWithTheWholeGap()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 1000m);
        var first = await SentPurchaseAsync(ledger, 600m);
        var second = await SentPurchaseAsync(ledger, 600m);

        foreach (var id in new[] { first, second })
        {
            var pending = await PendingAsync(ledger, id);
            Assert.Equal((1000m, 1200m, 200m), (pending.AccountBalance, pending.AccountWaiting, pending.AccountShortBy));
        }

        await ledger.ApproveAsync(first);
        var left = await PendingAsync(ledger, second);
        Assert.Equal((400m, 600m, 200m), (left.AccountBalance, left.AccountWaiting, left.AccountShortBy));
    }

    [Fact]
    public async Task R5_OneTransferOfTheWholeGapClearsEveryNote()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var store = await AccountAsync(ledger, "الخزنة");
        await FundAsync(ledger, 1200m);
        (await TransferAsync(ledger, ledger.AccountId, store, 200m)).EnsureSuccessStatusCode();
        var first = await SentPurchaseAsync(ledger, 600m);
        var second = await SentPurchaseAsync(ledger, 600m);
        Assert.Equal(200m, (await PendingAsync(ledger, first)).AccountShortBy);

        (await TransferAsync(ledger, store, ledger.AccountId, 200m)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await PendingAsync(ledger, first)).AccountShortBy);
        Assert.Equal(0m, (await PendingAsync(ledger, second)).AccountShortBy);
    }

    [Fact]
    public async Task R5_WhatWaitsOnAnotherAccountIsNotCounted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var store = await AccountAsync(ledger, "الخزنة");
        await FundAsync(ledger, 1000m);
        await ledger.RecordAsync(ledger.Purchase(5000m) with { AccountId = store, Submit = true });

        var pending = await PendingAsync(ledger, await SentPurchaseAsync(ledger, 600m));

        Assert.Equal((600m, 0m), (pending.AccountWaiting, pending.AccountShortBy));
    }

    [Fact]
    public async Task R16_ADraftCountsItselfWithWhatWaitsSoItsSharedLineTellsTheTruth()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FundAsync(ledger, 1000m);
        await SentPurchaseAsync(ledger, 600m);
        var draft = ledger.Purchase(600m);
        await ledger.RecordAsync(draft);

        var row = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!.Single(entry => entry.Id == draft.Id);

        Assert.Equal(200m, row.AccountShortBy);
    }

    [Fact]
    public async Task R5_TheShortCustodyFlow_ATransferTakesTheNoteAwayAndTheApprovalLeavesBothBalancesRight()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var store = await AccountAsync(ledger, "الخزنة");
        await FundAsync(ledger, 1000m);
        (await TransferAsync(ledger, ledger.AccountId, store, 1000m)).EnsureSuccessStatusCode();
        var purchase = await SentPurchaseAsync(ledger, 300m);
        Assert.Equal(300m, (await PendingAsync(ledger, purchase)).AccountShortBy);

        (await TransferAsync(ledger, store, ledger.AccountId, 300m)).EnsureSuccessStatusCode();
        Assert.Equal(0m, (await PendingAsync(ledger, purchase)).AccountShortBy);
        await ledger.ApproveAsync(purchase);

        Assert.Equal(0m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Equal(700m, await BalanceAsync(ledger, store));
    }

    [Fact]
    public async Task D6_MoneyComingInIsNeverShort()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var revenue = await ledger.RevenueAsync(ItemType.OtherRevenue, 5000m);
        await ledger.RecordAsync(revenue);

        Assert.Equal(0m, (await PendingAsync(ledger, revenue.Id)).AccountShortBy);
    }

    [Fact]
    public async Task R16_TheEntriesListCarriesTheShortfallForTheSharedLineAndForgetsItOnceApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var purchase = await SentPurchaseAsync(ledger, 400m);
        async Task<EntryRowView> RowAsync() => (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!.Single(row => row.Id == purchase);

        Assert.Equal(400m, (await RowAsync()).AccountShortBy);

        await ledger.ApproveAsync(purchase);
        Assert.Equal(0m, (await RowAsync()).AccountShortBy);
    }
}
