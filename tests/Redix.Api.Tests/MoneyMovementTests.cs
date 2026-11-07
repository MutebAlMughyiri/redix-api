using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-040 to UC-045: money that moves without an expense, and a partner's capital. No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class MoneyMovementTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<Guid> AccountAsync(Ledger ledger, string name, AccountType type = AccountType.Bank, decimal opening = 0)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest(name, type, opening), Json)).EnsureSuccessStatusCode();
        return (await AccountsAsync(ledger)).Single(row => row.Name == name).Id;
    }

    private static async Task<List<AccountView>> AccountsAsync(Ledger ledger) => (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!;

    private static async Task<decimal> BalanceAsync(Ledger ledger, Guid accountId) => (await AccountsAsync(ledger)).Single(row => row.Id == accountId).Balance;

    private static async Task<Guid> PartnerAsync(HttpClient client, Guid projectId, string name, Guid userId = default)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await client.GetFromJsonAsync<List<PartnerView>>($"/projects/{projectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    private static Task<HttpResponseMessage> MoveAsync(Ledger ledger, CapitalMovementRequest request) => ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/capital-movements", request, Json);

    private static CapitalMovementRequest Contribution(Ledger ledger, Guid partnerId, decimal amount) =>
        new(partnerId, CapitalMovementType.Contribution, ledger.Today, amount, ledger.AccountId, "زاد رصيده") { CashReason = "نقداً" };

    private static async Task ContributeAsync(Ledger ledger, Guid partnerId, decimal amount) => (await MoveAsync(ledger, Contribution(ledger, partnerId, amount))).EnsureSuccessStatusCode();

    private static async Task<CapitalView> CapitalAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<CapitalView>($"/projects/{projectId}/capital", Json))!;

    private static Task<decimal> SurplusAsync(Ledger ledger) => ledger.ReadAsync(async db => (await ProjectFigures.OfAsync(ledger.ProjectId, db)).Surplus);

    private static Task<HttpResponseMessage> TransferAsync(Ledger ledger, Guid from, Guid to, decimal amount) =>
        ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/transfers", new TransferRequest(ledger.Today, from, to, amount, "تغذية العهدة"), Json);

    private static async Task ReverseAsync(HttpClient client, Guid projectId, Guid entryId) =>
        (await client.PostAsJsonAsync($"/projects/{projectId}/entries/{entryId}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

    [Fact]
    public async Task AC_23_1_ATransferMovesBothBalancesAndLeavesTheSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش", opening: 100);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);
        var surplus = await SurplusAsync(ledger);

        (await TransferAsync(ledger, ledger.AccountId, other, 250)).EnsureSuccessStatusCode();

        Assert.Equal(750m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Equal(350m, await BalanceAsync(ledger, other));
        Assert.Equal(surplus, await SurplusAsync(ledger));
    }

    [Fact]
    public async Task ATransferIsApprovedAtOnceAndListedOnce()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش");

        (await TransferAsync(ledger, ledger.AccountId, other, 250)).EnsureSuccessStatusCode();

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<TransferView>>($"/projects/{ledger.ProjectId}/transfers", Json))!);
        Assert.Equal((250m, "حساب المؤسسة", "كاش", "تغذية العهدة"), (row.Amount, row.FromAccountName, row.ToAccountName, row.Description));
        Assert.All(await ledger.ReadAsync(db => db.Entries.Where(entry => entry.ProjectId == ledger.ProjectId).ToListAsync()), entry => Assert.Equal(EntryStatus.Approved, entry.Status));
    }

    [Fact]
    public async Task WhatATransferCannotBeIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش");

        Assert.Equal("sameAccount", await Ledger.CodeOf(await TransferAsync(ledger, other, other, 10)));
        Assert.Equal("invalidAmount", await Ledger.CodeOf(await TransferAsync(ledger, ledger.AccountId, other, 0)));
        Assert.Equal("unknownAccount", await Ledger.CodeOf(await TransferAsync(ledger, ledger.AccountId, Guid.NewGuid(), 10)));
    }

    [Fact]
    public async Task OnlyWhoeverMovesMoneyEntersOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش");
        var (field, _) = await ledger.MemberAsync("Field");

        var refused = await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/transfers", new TransferRequest(ledger.Today, ledger.AccountId, other, 10, ""), Json);

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [Fact]
    public async Task ReversingATransferReversesBothSides()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش");
        (await TransferAsync(ledger, ledger.AccountId, other, 250)).EnsureSuccessStatusCode();
        var row = (await ledger.Client.GetFromJsonAsync<List<TransferView>>($"/projects/{ledger.ProjectId}/transfers", Json))!.Single();

        await ReverseAsync(ledger.Client, ledger.ProjectId, row.Id);

        Assert.Equal(0m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Equal(0m, await BalanceAsync(ledger, other));
    }

    [Fact]
    public async Task AC_24_1_TheDifferenceIsTheActualBalancesLessTheSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await AccountAsync(ledger, "كاش");
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);

        var answer = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 700.25m), new(other, 200.50m)]), Json);

        var view = await answer.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ReconciliationView>(Json);
        Assert.Equal((1000m, 900.75m, -99.25m), (view!.Surplus, view.ActualTotal, view.Difference));
    }

    [Fact]
    public async Task BeforeAnyReconciliationTheAccountsShowWithNothingEntered()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var view = await ledger.Client.GetFromJsonAsync<ReconciliationView>($"/projects/{ledger.ProjectId}/reconciliations/latest", Json);

        Assert.Equal(0m, view!.ActualTotal);
        Assert.Equal(ledger.AccountId, Assert.Single(view.Balances).AccountId);
    }

    [Fact]
    public async Task AReconciliationNeedsEveryActiveAccountAndNoOtherProjects()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await AccountAsync(ledger, "كاش");

        Assert.Equal("accountMissing", await Ledger.CodeOf(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 1)]), Json)));
        Assert.Equal("unknownAccount", await Ledger.CodeOf(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(Guid.CreateVersion7(), 1)]), Json)));
    }

    [Fact]
    public async Task AC_25_1_AMovementWithNoReceiptIsRefusedAndCashWithAReasonIsNot()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var bare = new CapitalMovementRequest(partner, CapitalMovementType.Contribution, ledger.Today, 100, ledger.AccountId, "");

        Assert.Equal("receiptRequired", await Ledger.CodeOf(await MoveAsync(ledger, bare)));
        (await MoveAsync(ledger, bare with { CashReason = "دفع نقداً في الموقع" })).EnsureSuccessStatusCode();
        (await MoveAsync(ledger, bare with { AttachmentId = await ledger.FileAsync() })).EnsureSuccessStatusCode();

        Assert.Equal(200m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
    }

    [Fact]
    public async Task AWithdrawalTakesFromTheCapitalAndTheAccount()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);

        (await MoveAsync(ledger, Contribution(ledger, partner, 300) with { Type = CapitalMovementType.Withdrawal })).EnsureSuccessStatusCode();

        Assert.Equal(700m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
        Assert.Equal(700m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task SomethingInKindNeedsItsReason()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var inKind = new CapitalMovementRequest(partner, CapitalMovementType.InKind, ledger.Today, 100, ledger.AccountId, "") { AttachmentId = await ledger.FileAsync() };

        Assert.Equal("receiptRequired", await Ledger.CodeOf(await MoveAsync(ledger, inKind)));
        (await MoveAsync(ledger, inKind with { CashReason = "أرض" })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ASettlementTakesAllTheyHoldAndTheyLeave()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);
        var settlement = Contribution(ledger, partner, 400) with { Type = CapitalMovementType.SettlementAndExit };

        Assert.Equal("settlementNotFull", await Ledger.CodeOf(await MoveAsync(ledger, settlement)));
        (await MoveAsync(ledger, settlement with { Amount = 1000 })).EnsureSuccessStatusCode();

        var line = Assert.Single((await CapitalAsync(ledger.Client, ledger.ProjectId)).Partners);
        Assert.Equal((0m, false), (line.Balance, line.IsActive));
        Assert.Equal("partnerLeft", await Ledger.CodeOf(await MoveAsync(ledger, Contribution(ledger, partner, 10))));
    }

    [Fact]
    public async Task ReversingASettlementBringsThePartnerBack()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);
        var answer = await MoveAsync(ledger, Contribution(ledger, partner, 1000) with { Type = CapitalMovementType.SettlementAndExit });
        var settled = await answer.Content.ReadFromJsonAsync<EntryStatusView>(Json);

        await ReverseAsync(ledger.Client, ledger.ProjectId, settled!.Id);

        var line = Assert.Single((await CapitalAsync(ledger.Client, ledger.ProjectId)).Partners);
        Assert.Equal((1000m, true), (line.Balance, line.IsActive));
    }

    [Fact]
    public async Task ACarryHasItsOwnRoute()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");

        Assert.Equal("useCarry", await Ledger.CodeOf(await MoveAsync(ledger, Contribution(ledger, partner, 10) with { Type = CapitalMovementType.CarryIn })));
    }

    private async Task<(Ledger Source, Guid SourcePartner, ProjectView Target, Guid TargetPartner, Guid TargetAccount)> TwoProjectsAsync()
    {
        var source = await Ledger.WithFinanceAsync(factory);
        var sourcePartner = await PartnerAsync(source.Client, source.ProjectId, "شريك");
        await ContributeAsync(source, sourcePartner, 1000);
        var target = await factory.ApprovedProjectAsync(source.Client, "غصون 7", "الفلة");
        var user = await source.ReadAsync(db => db.ProjectPermissions.Where(row => row.ProjectId == source.ProjectId).Select(row => row.UserId).FirstAsync());
        await factory.GiveAsync(user, target.Id, Holds.Of("Finance"));
        var targetPartner = await PartnerAsync(source.Client, target.Id, "شريك");
        (await source.Client.PostAsJsonAsync($"/projects/{target.Id}/accounts", new AddAccountRequest("حساب", AccountType.Bank, 0), Json)).EnsureSuccessStatusCode();
        var account = (await source.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{target.Id}/accounts", Json))!.Single(row => row.Type == AccountType.Bank);
        return (source, sourcePartner, target, targetPartner, account.Id);
    }

    [Fact]
    public async Task AC_26_1_ACarryLowersOneAndRaisesTheOtherWithNoReceipt()
    {
        var (source, sourcePartner, target, targetPartner, targetAccount) = await TwoProjectsAsync();

        var answer = await source.Client.PostAsJsonAsync($"/projects/{source.ProjectId}/capital-movements/carry", new CarryRequest(sourcePartner, source.AccountId, target.Id, targetPartner, targetAccount, source.Today, 400, ""), Json);

        answer.EnsureSuccessStatusCode();
        Assert.Equal(600m, (await CapitalAsync(source.Client, source.ProjectId)).Partners.Single().Balance);
        Assert.Equal(400m, (await CapitalAsync(source.Client, target.Id)).Partners.Single().Balance);
    }

    [Fact]
    public async Task ReversingACarryReversesBothProjects()
    {
        var (source, sourcePartner, target, targetPartner, targetAccount) = await TwoProjectsAsync();
        var answer = await source.Client.PostAsJsonAsync($"/projects/{source.ProjectId}/capital-movements/carry", new CarryRequest(sourcePartner, source.AccountId, target.Id, targetPartner, targetAccount, source.Today, 400, ""), Json);
        var carried = await answer.Content.ReadFromJsonAsync<EntryStatusView>(Json);

        await ReverseAsync(source.Client, source.ProjectId, carried!.Id);

        Assert.Equal(1000m, (await CapitalAsync(source.Client, source.ProjectId)).Partners.Single().Balance);
        Assert.Equal(0m, (await CapitalAsync(source.Client, target.Id)).Partners.Single().Balance);
    }

    [Fact]
    public async Task ACarryNeedsMoneyRightsInBothProjectsAndARunningTarget()
    {
        var (source, sourcePartner, target, targetPartner, targetAccount) = await TwoProjectsAsync();
        var request = new CarryRequest(sourcePartner, source.AccountId, target.Id, targetPartner, targetAccount, source.Today, 400, "");

        var elsewhere = await Ledger.WithFinanceAsync(factory);
        var refused = await source.Client.PostAsJsonAsync($"/projects/{source.ProjectId}/capital-movements/carry", request with { TargetProjectId = elsewhere.ProjectId }, Json);
        Assert.Equal("targetProjectRefused", await Ledger.CodeOf(refused));

        var same = await source.Client.PostAsJsonAsync($"/projects/{source.ProjectId}/capital-movements/carry", request with { TargetProjectId = source.ProjectId }, Json);
        Assert.Equal("sameProject", await Ledger.CodeOf(same));

        await factory.WithDatabaseAsync(db => db.Projects.Where(row => row.Id == target.Id).ExecuteUpdateAsync(row => row.SetProperty(project => project.Status, ProjectStatus.Finished)));
        var finished = await source.Client.PostAsJsonAsync($"/projects/{source.ProjectId}/capital-movements/carry", request, Json);
        Assert.Equal("targetProjectFinished", await Ledger.CodeOf(finished));
    }

    [Fact]
    public async Task AC_25_2_AC_27_1_EachBalanceIsTheirMovementsAndTheSharesAddUpTo100()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await PartnerAsync(ledger.Client, ledger.ProjectId, "الأول");
        var second = await PartnerAsync(ledger.Client, ledger.ProjectId, "الثاني");
        var third = await PartnerAsync(ledger.Client, ledger.ProjectId, "الثالث");
        await ContributeAsync(ledger, first, 2000);
        await ContributeAsync(ledger, second, 1000);
        await ContributeAsync(ledger, third, 500);
        (await MoveAsync(ledger, Contribution(ledger, third, 500) with { Type = CapitalMovementType.Withdrawal })).EnsureSuccessStatusCode();

        var capital = await CapitalAsync(ledger.Client, ledger.ProjectId);

        Assert.Equal(3000m, capital.Total);
        Assert.Equal(66.6666666667m, capital.Partners.Single(row => row.Id == first).SharePercent);
        Assert.Equal(33.3333333333m, capital.Partners.Single(row => row.Id == second).SharePercent);
        Assert.Equal(0m, capital.Partners.Single(row => row.Id == third).SharePercent);
        Assert.Equal(100m, capital.Partners.Sum(row => row.SharePercent));
    }

    [Fact]
    public async Task BR_31_APartnerSeesTheTotalAndTheirOwnLineAndNoOneElses()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (reader, readerId) = await ledger.MemberAsync("Reader");
        var own = await PartnerAsync(ledger.Client, ledger.ProjectId, "القارئ", readerId);
        var other = await PartnerAsync(ledger.Client, ledger.ProjectId, "غيره");
        await ContributeAsync(ledger, own, 100);
        await ContributeAsync(ledger, other, 300);

        var capital = await CapitalAsync(reader, ledger.ProjectId);

        Assert.Equal(400m, capital.Total);
        var line = Assert.Single(capital.Partners);
        Assert.Equal((own, true, 25m), (line.Id, line.IsMine, line.SharePercent));
        Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync($"/projects/{ledger.ProjectId}/partners/{own}/movements")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await reader.GetAsync($"/projects/{ledger.ProjectId}/partners/{other}/movements")).StatusCode);
    }

    [Fact]
    public async Task AMovementShowsWhatItDidToTheCapitalAndItsReceipt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var file = await ledger.FileAsync();
        (await MoveAsync(ledger, new CapitalMovementRequest(partner, CapitalMovementType.Withdrawal, ledger.Today, 100, ledger.AccountId, "سحب") { AttachmentId = file })).EnsureSuccessStatusCode();

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{partner}/movements", Json))!);

        Assert.Equal((-100m, $"{file}", "حساب المؤسسة"), (row.Amount, row.AttachmentId, row.AccountName));
    }

    [Fact]
    public async Task R99_APartnerAddedMakesNoAccountSoTheAccountsAreOnlyThoseAddedInAccounts()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = (await AccountsAsync(ledger)).Select(row => row.Id).ToList();

        await PartnerAsync(ledger.Client, ledger.ProjectId, "نواف");

        Assert.Equal(before, (await AccountsAsync(ledger)).Select(row => row.Id));
    }
}
