using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Reports;
using Redix.Api.Revenue;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-047, UC-048, UC-056: revenue, sales invoices and the كشف الضريبة: totals, whose VAT the Excel works out (redix-api #96). No figure here is real.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class RevenueTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static DateOnly QuarterOf(Ledger ledger) => VatStatement.QuarterStartOf(ledger.Today);

    private static Task<HttpResponseMessage> SendAsync(Ledger ledger, EntryRequest request) => ledger.PostAsync(request);

    private static async Task<List<EntryRowView>> EntriesAsync(Ledger ledger) => (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;

    private static Task<ProjectFigures> FiguresAsync(Ledger ledger) => ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));

    private static Task<List<VatQuarterView>> StatementAsync(Ledger ledger, DateOnly from, DateOnly to) => ledger.ReadAsync(db => VatStatement.OfAsync(ledger.ProjectId, from, to, db));

    private static Task<List<VatQuarterView>> StatementAsync(Ledger ledger) => StatementAsync(ledger, QuarterOf(ledger), VatStatement.NextQuarter(QuarterOf(ledger)).AddDays(-1));

    private static Task ApprovedPurchaseAsync(Ledger ledger, decimal amount, string product = "") => ApprovedAsync(ledger, ledger.Purchase(amount) with { Product = product, Submit = true });

    private static async Task ApprovedAsync(Ledger ledger, EntryRequest purchase)
    {
        await ledger.RecordAsync(purchase);
        await ledger.SetStatusAsync(purchase.Id, EntryStatus.Approved);
    }

    [Fact]
    public async Task ARejectedRevenueEntryIsSentAgainAndWaitsOnceMore()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var sent = await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, ""));
        var id = (await sent.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Ledger.Json))!.Id;
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reject", new RejectEntryRequest("المبلغ خطأ"), Ledger.Json)).EnsureSuccessStatusCode();

        var again = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{id}/submit", null);

        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(EntryStatus.PendingApproval, await ledger.ReadAsync(db => db.Entries.Where(entry => entry.Id == id).Select(entry => entry.Status).SingleAsync()));
    }

    [Fact]
    public async Task BR_31_OnePartnersMoneyReadByItsIdIsLeftToWhoeverItIsChosenForAndWhoeverSeesAllCapital()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (reader, readerId) = await ledger.MemberAsync("Reader");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("القارئ"), Ledger.Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("غيره"), Ledger.Json)).EnsureSuccessStatusCode();
        var partners = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Ledger.Json))!;
        await ledger.ChooseAsync(readerId, partners.Single(row => row.Name == "القارئ").Id);
        async Task<Guid> ContributeAsync(string name) =>
            await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partners.Single(row => row.Name == name).Id, 100, notes: ""), Ledger.Json));
        var own = await ContributeAsync("القارئ");
        var other = await ContributeAsync("غيره");

        var ownRead = await reader.GetAsync($"/projects/{ledger.ProjectId}/entries/{own}");
        var otherRead = await reader.GetAsync($"/projects/{ledger.ProjectId}/entries/{other}");
        var financeRead = await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/entries/{other}");

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.OK), (ownRead.StatusCode, otherRead.StatusCode, financeRead.StatusCode));
    }

    [Fact]
    public async Task RevenueOnAnItemOfADisabledCategoryIsRefusedAsTheTableListHidesIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var itemId = await ledger.RevenueItemAsync(ItemType.OtherRevenue);
        var category = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories", Ledger.Json))!.Single(row => row.Items.Any(item => item.Id == itemId));
        var request = await ledger.RevenueOnAsync(itemId, 70, "");
        (await SendAsync(ledger, request)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/categories/{category.Id}/disable", null)).EnsureSuccessStatusCode();

        var refused = await SendAsync(ledger, request with { Id = Guid.CreateVersion7() });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("unknownTable", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task D23_ARefundIsPlainRevenueThatRaisesRevenueAndSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = await FiguresAsync(ledger);

        var id = await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.VatRefund, 140, "")));

        var after = await FiguresAsync(ledger);
        Assert.Equal(before.Revenue + 140, after.Revenue);
        Assert.Equal(before.Surplus + 140, after.Surplus);
        Assert.Equal(EntryKind.Revenue, (await ledger.ReadAsync(db => db.Entries.SingleAsync(entry => entry.Id == id))).Kind);
    }

    [Fact]
    public async Task OtherRevenueReachesItsAccount()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, "إيجار")));

        Assert.Equal(70m, (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(account => account.Id == ledger.AccountId).Balance);
    }

    [Fact]
    public async Task R10_RevenueWaitsForApprovalAndCountsOnlyOnceApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = await FiguresAsync(ledger);

        var sent = (await (await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, "إيجار"))).Content.ReadFromJsonAsync<EntryStatusView>(Json))!;

        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
        Assert.Equal(before.Revenue, (await FiguresAsync(ledger)).Revenue);
        Assert.Equal(EntryStatus.PendingApproval, (await EntriesAsync(ledger)).Single(row => row.Id == sent.Id).Status);

        await ledger.ApproveAsync(sent.Id);

        Assert.Equal(before.Revenue + 70, (await FiguresAsync(ledger)).Revenue);
    }

    [Fact]
    public async Task OnlyWhoeverMovesMoneyRecordsRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");

        var refused = await ledger.PostAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, 70), field);

        Assert.Equal((HttpStatusCode.BadRequest, "unknownTable"), (refused.StatusCode, await Ledger.CodeOf(refused)));
    }

    [Fact]
    public async Task ReversingASaleTakesItOutOfTheRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.Sale, 1150, "")));
        Assert.Equal(1150m, (await FiguresAsync(ledger)).Sales);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await FiguresAsync(ledger)).Sales);
    }

    [Fact]
    public async Task AC_32_1_D19_TheStatementListsApprovedPurchasesWithTheirSupplierAndWhatWasBought()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 100.01m, "بلاط");
        await ApprovedPurchaseAsync(ledger, 100.01m);
        await ledger.RecordAsync(ledger.Purchase(50m) with { Submit = true });

        var quarter = Assert.Single(await StatementAsync(ledger));

        Assert.Equal(2, quarter.Purchases.Count);
        Assert.Equal(200.02m, quarter.Purchases.Sum(row => row.Total));
        var tiles = Assert.Single(quarter.Purchases, row => row.Description == "بلاط");
        Assert.Equal(("شركة النخيل", 15), (tiles.Party, tiles.VatNumber.Length));
    }

    [Fact]
    public async Task UC_026_ACreditNoteAndAReversalTakeAwayFromTheStatementWhatTheirBillsCounted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var bill = ledger.Purchase(115m) with { Submit = true };
        await ApprovedAsync(ledger, bill);
        await ApprovedAsync(ledger, ledger.Purchase(15m) with { IsCreditNote = true, OriginalEntryId = bill.Id, Submit = true });
        var mistaken = ledger.Purchase(50m) with { Submit = true };
        await ApprovedAsync(ledger, mistaken);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{mistaken.Id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        var quarter = Assert.Single(await StatementAsync(ledger));

        Assert.Equal(new[] { -50m, -15m, 50m, 115m }, quarter.Purchases.Select(row => row.Total).Order());
        Assert.Equal(100m, quarter.Purchases.Sum(row => row.Total));
    }

    [Fact]
    public async Task AC_32_2_ACustomPeriodAcrossQuartersHasASectionPerQuarter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var statement = await StatementAsync(ledger, new DateOnly(2026, 8, 15), new DateOnly(2027, 1, 10));

        Assert.Equal([new DateOnly(2026, 7, 1), new DateOnly(2026, 10, 1), new DateOnly(2027, 1, 1)], statement.Select(quarter => quarter.Start));
        Assert.Equal(new DateOnly(2026, 9, 30), statement[0].End);
    }

    [Fact]
    public async Task APeriodEndingBeforeItStartsIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/vat-statement/xlsx?from=2026-08-15&to=2026-08-14");

        Assert.Equal("invalidPeriod", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task FR_28_RevenueGoesOnARevenueItemAndAnExpenseTablesEntryIsAnExpense()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await SendAsync(ledger, await ledger.RevenueOnAsync(ledger.ItemId, 70));
        await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, "إيجار")));

        Assert.Equal("unknownPaymentMethod", await Ledger.CodeOf(refused));
        var row = Assert.Single(await EntriesAsync(ledger), row => row.Kind == EntryKind.Revenue);
        Assert.Equal("إيرادات أخرى", row.ItemName);
        Assert.Equal(70m, (await FiguresAsync(ledger)).OtherRevenue);
    }

    [Fact]
    public async Task OneList_TheEntriesListHoldsRevenueAndPartnersMoneyWithTheirStatus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();
        var partner = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single().Id;
        var revenue = await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, "")));
        var capital = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 500, notes: ""), Json);
        var capitalId = (await capital.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;

        var rows = await EntriesAsync(ledger);

        Assert.Equal((EntryKind.Revenue, EntryStatus.Approved, "إيرادات أخرى", ""), rows.Where(row => row.Id == revenue).Select(row => (row.Kind, row.Status, row.ItemName, row.PartnerName)).Single());
        Assert.Equal((EntryKind.CapitalIn, EntryStatus.PendingApproval, "نواف"), rows.Where(row => row.Id == capitalId).Select(row => (row.Kind, row.Status, row.PartnerName)).Single());
    }

    [Fact]
    public async Task OneList_AWaitingRevenueOrPartnerEntryIsDeletedWithEverythingItHolds()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();
        var partner = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single().Id;
        var revenue = (await (await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, ""))).Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;
        var capital = (await (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 500, notes: ""), Json)).Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;

        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{revenue}")).EnsureSuccessStatusCode();
        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{capital}")).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await EntriesAsync(ledger), row => row.Id == revenue || row.Id == capital);
        Assert.False(await ledger.ReadAsync(db => db.Set<RevenueDetails>().AnyAsync(row => row.EntryId == revenue)));
        Assert.False(await ledger.ReadAsync(db => db.CapitalMovementDetails.AnyAsync(row => row.EntryId == capital)));
    }

    [Fact]
    public async Task BR_31_InTheEntriesListSomeoneSeesTheMoneyOfThePartnerChosenForThemAndNoOneElses()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (reader, readerId) = await ledger.MemberAsync("Reader");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("القارئ"), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("غيره"), Json)).EnsureSuccessStatusCode();
        var partners = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!;
        await ledger.ChooseAsync(readerId, partners.Single(row => row.Name == "القارئ").Id);
        async Task<Guid> ContributeAsync(string name) =>
            await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partners.Single(row => row.Name == name).Id, 100, notes: ""), Json));
        var own = await ContributeAsync("القارئ");
        var other = await ContributeAsync("غيره");

        var seenByReader = (await reader.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;
        var seenByFinance = await EntriesAsync(ledger);

        Assert.Contains(seenByReader, row => row.Id == own);
        Assert.DoesNotContain(seenByReader, row => row.Id == other);
        Assert.Contains(seenByFinance, row => row.Id == own);
        Assert.Contains(seenByFinance, row => row.Id == other);
    }

    [Fact]
    public async Task NoRevenueList_TheRevenueListAndTheSalesInvoiceAreGone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        Assert.False((await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/revenue")).IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/sales-invoices", new { buyerName = "أحمد" })).StatusCode);
    }

    [Fact]
    public async Task BR_06_TheSummaryGivesEveryReaderTheCapitalSoTheRevenueSideAddsUpToTheRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();
        var partner = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single().Id;
        await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 500, notes: ""), Json));
        await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.OtherRevenue, 70, "")));
        var (reader, _) = await ledger.MemberAsync("Reader");

        var summary = (await reader.GetFromJsonAsync<SummaryView>($"/projects/{ledger.ProjectId}/summary", Json))!;

        var capital = summary.RevenueCategories.Single(category => category.Items.Any(item => item.Type == ItemType.Capital));
        Assert.Equal(("رأس المال", 500m, 570m), (capital.Name, capital.Total, summary.Revenue));
        Assert.Equal(summary.Revenue, summary.RevenueCategories.Sum(category => category.Total));
    }

    [Fact]
    public async Task AC_46_7_TheSummaryKeepsRevenueCategoriesApartFromExpenseOnes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.ApproveAsync(await SendAsync(ledger, await ledger.RevenueAsync(ItemType.VatRefund, 40, "")));

        var summary = (await ledger.Client.GetFromJsonAsync<SummaryView>($"/projects/{ledger.ProjectId}/summary", Json))!;

        Assert.Equal(["رأس المال", "استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], summary.RevenueCategories.Select(category => category.Name));
        Assert.Equal((0m, 40m), (summary.RevenueCategories[0].Total, summary.RevenueCategories[1].Total));
        Assert.DoesNotContain(summary.Categories.SelectMany(category => category.Items), item => item.Type.IsRevenue());
    }
}
