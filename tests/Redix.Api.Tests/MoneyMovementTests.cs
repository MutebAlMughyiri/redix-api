using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Reports;
using Redix.Api.Revenue;
using Redix.Api.Structure;
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
        (await client.PostAsJsonAsync($"/projects/{projectId}/partners", new PartnerRequest(name, await Ledger.CapitalItemOfAsync(client, projectId)) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await client.GetFromJsonAsync<List<PartnerView>>($"/projects/{projectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    private static Task<HttpResponseMessage> MoveAsync(Ledger ledger, EntryRequest request) => ledger.PostAsync(request);

    private static Task<EntryRequest> ContributionAsync(Ledger ledger, Guid partnerId, decimal amount) => ledger.PartnerEntryAsync(partnerId, amount, notes: "زاد رصيده");

    private static async Task ContributeAsync(Ledger ledger, Guid partnerId, decimal amount) => await ledger.ApproveAsync(await MoveAsync(ledger, await ContributionAsync(ledger, partnerId, amount)));

    private static async Task<CapitalView> CapitalAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<CapitalView>($"/projects/{projectId}/capital", Json))!;

    private static Task<decimal> SurplusAsync(Ledger ledger) => ledger.ReadAsync(async db => (await ProjectFigures.OfAsync(ledger.ProjectId, db)).Surplus);

    private static async Task ReverseAsync(HttpClient client, Guid projectId, Guid entryId) =>
        (await client.PostAsJsonAsync($"/projects/{projectId}/entries/{entryId}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

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
        var balance = Assert.Single(view.Balances);
        Assert.Equal((ledger.AccountId, false), (balance.AccountId, balance.WasChecked));
    }

    [Fact]
    public async Task AnAccountAddedAfterTheLatestCheckSaysTheCheckDidNotHoldIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 0m)]), Json);
        var added = await AccountAsync(ledger, "كاش");

        var view = await ledger.Client.GetFromJsonAsync<ReconciliationView>($"/projects/{ledger.ProjectId}/reconciliations/latest", Json);

        Assert.True(Assert.Single(view!.Balances, row => row.AccountId == ledger.AccountId).WasChecked);
        Assert.False(Assert.Single(view.Balances, row => row.AccountId == added).WasChecked);
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
    public async Task Simple_Entry_AMovementTakesItsReceiptWhenThereIsOneAndNeedsNone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var bare = await ledger.PartnerEntryAsync(partner, 100);

        await ledger.ApproveAsync(await MoveAsync(ledger, bare));
        await ledger.ApproveAsync(await MoveAsync(ledger, bare with { Id = Guid.CreateVersion7(), AttachmentId = await ledger.FileAsync() }));

        Assert.Equal(200m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
    }

    [Fact]
    public async Task AWithdrawalTakesFromTheCapitalAndTheAccount()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        await ContributeAsync(ledger, partner, 1000);

        await ledger.ApproveAsync(await MoveAsync(ledger, await ledger.PartnerEntryAsync(partner, 300, isWithdrawal: true, notes: "زاد رصيده")));

        Assert.Equal(700m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
        Assert.Equal(700m, await BalanceAsync(ledger, ledger.AccountId));
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
        await ledger.ApproveAsync(await MoveAsync(ledger, await ledger.PartnerEntryAsync(third, 500, isWithdrawal: true, notes: "زاد رصيده")));

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
    public async Task BR_31_UC_109_RasAlMalsReportIsATablePerPartnerEveryOneForWhoeverSeesAllCapitalTheirOwnForAPartnerNoneForAnyoneElseAndTheWholeCapitalForAll()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (partnerReader, partnerReaderId) = await ledger.MemberAsync("Reader");
        var (otherReader, _) = await ledger.MemberAsync("Reader");
        var own = await PartnerAsync(ledger.Client, ledger.ProjectId, "القارئ", partnerReaderId);
        var other = await PartnerAsync(ledger.Client, ledger.ProjectId, "غيره");
        await ContributeAsync(ledger, own, 100);
        await ContributeAsync(ledger, other, 300);
        await ledger.ApproveAsync(await MoveAsync(ledger, await ledger.PartnerEntryAsync(own, 40, isWithdrawal: true, notes: "زاد رصيده")));
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);

        async Task<ItemReportView> ReportAsync(HttpClient client) => (await client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{capital}", Json))!;

        var all = await ReportAsync(ledger.Client);
        Assert.Equal(360m, all.Total);
        Assert.Equal([(own, "القارئ", 60m), (other, "غيره", 300m)], all.Subgroups.Select(table => (table.Id, table.Name, table.Total)));
        Assert.Equal([-40m, 100m], all.Subgroups[0].Rows.Select(row => row.Amount).Order());
        var mine = await ReportAsync(partnerReader);
        Assert.Equal((360m, own), (mine.Total, Assert.Single(mine.Subgroups).Id));
        var none = await ReportAsync(otherReader);
        Assert.Equal(360m, none.Total);
        Assert.Empty(none.Subgroups);
        var dashboard = (await otherReader.GetFromJsonAsync<DashboardView>($"/projects/{ledger.ProjectId}/dashboard", Json))!;
        Assert.Equal(360m, dashboard.RevenueCategories.SelectMany(category => category.Items).Single(item => item.Id == capital).Total);
    }

    [Fact]
    public async Task BR_15_UC_109_ADisabledRasAlMalTakesNoMovementYetItsPartnerIsStillRenamed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/items/{capital}/disable", null)).EnsureSuccessStatusCode();

        var moved = await MoveAsync(ledger, await ContributionAsync(ledger, partner, 100));
        var renamed = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{partner}", new PartnerRequest("شريك أول", capital), Json);

        Assert.Equal((HttpStatusCode.BadRequest, "unknownTable"), (moved.StatusCode, await Ledger.CodeOf(moved)));
        renamed.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UC_109_AMovementsTextIsItsEntrysNotesSoChangingTheNotesChangesWhatTheCapitalAndItsReportShow()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        await ContributeAsync(ledger, partner, 100);
        var entryId = (await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{partner}/movements", Json))!.Single().Id;

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/notes", new ChangeNotesRequest("تحويل من البنك"), Json)).EnsureSuccessStatusCode();

        Assert.Equal("تحويل من البنك", (await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{partner}/movements", Json))!.Single().Description);
        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{capital}", Json))!;
        Assert.EndsWith("تحويل من البنك", report.Subgroups.Single().Rows.Single().Description);
    }

    [Fact]
    public async Task AMovementShowsWhatItDidToTheCapitalAndItsReceipt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var file = await ledger.FileAsync();
        await ledger.ApproveAsync(await MoveAsync(ledger, (await ledger.PartnerEntryAsync(partner, 100, isWithdrawal: true, notes: "سحب")) with { AttachmentId = file }));

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{partner}/movements", Json))!);

        Assert.Equal((-100m, $"{file}", "invoice.jpg", "حساب المؤسسة"), (row.Amount, row.AttachmentId, row.FileName, row.AccountName));
    }

    [Fact]
    public async Task R10_AMovementWaitsForApprovalAndCountsOnlyOnceApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");

        var sent = (await (await MoveAsync(ledger, await ContributionAsync(ledger, partner, 1000))).Content.ReadFromJsonAsync<EntryStatusView>(Json))!;

        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
        Assert.Equal(0m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
        Assert.Equal(0m, await BalanceAsync(ledger, ledger.AccountId));
        Assert.Empty((await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{partner}/movements", Json))!);

        await ledger.ApproveAsync(sent.Id);

        Assert.Equal(1000m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
        Assert.Equal(1000m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task R10_ARejectedMovementNeverCounts()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");
        var id = (await (await MoveAsync(ledger, await ContributionAsync(ledger, partner, 1000))).Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reject", new RejectEntryRequest("المبلغ لا يطابق الإيصال"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await CapitalAsync(ledger.Client, ledger.ProjectId)).Total);
        Assert.Equal(0m, await BalanceAsync(ledger, ledger.AccountId));
    }

    [Fact]
    public async Task R16_WhatWaitsNamesAMovementsPartnerAndARevenueEntrysItem()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "نواف");
        await MoveAsync(ledger, await ContributionAsync(ledger, partner, 1000));
        var item = await ledger.RevenueItemAsync(ItemType.VatRefund);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.RevenueOnAsync(item, 140, ""), Json)).EnsureSuccessStatusCode();

        var waiting = (await ledger.Client.GetFromJsonAsync<List<PendingEntryView>>($"/projects/{ledger.ProjectId}/entries/pending", Json))!;

        var capital = waiting.Single(row => row.Kind == EntryKind.CapitalIn);
        Assert.Equal(("نواف", "", 1000m), (capital.PartnerName, capital.ItemName, capital.Amount));
        var revenue = waiting.Single(row => row.Kind == EntryKind.Revenue);
        var itemName = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories", Json))!.SelectMany(category => category.Items).Single(row => row.Id == item).Name;
        Assert.Equal((itemName, "", 140m), (revenue.ItemName, revenue.PartnerName, revenue.Amount));
    }

    [Fact]
    public async Task R16_AMovementSentTellsWhoeverApprovesAndNotWhoSentIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (finance, _) = await ledger.MemberAsync("Finance");
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");

        await MoveAsync(ledger, await ContributionAsync(ledger, partner, 1000));

        Assert.Equal(NotificationKind.EntrySubmitted, Assert.Single((await finance.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!).Kind);
        Assert.Empty((await ledger.Client.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!);
    }

    [Fact]
    public async Task R4_R6_A_110_APartnerEntryIsAContributionOrAWithdrawalAndTheOldRoutesAreGone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "شريك");

        foreach (var route in new[] { "capital-movements", "capital-movements/carry", "revenue", "expenses", "contractor-payments" })
        {
            var gone = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/{route}", new { partnerId = partner, type = "contribution", date = ledger.Today, amount = 100, accountId = ledger.AccountId, description = "" });
            Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
        }
        Assert.Empty(await ledger.ReadAsync(db => db.CapitalMovementDetails.Where(row => row.PartnerId == partner).ToListAsync()));
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
