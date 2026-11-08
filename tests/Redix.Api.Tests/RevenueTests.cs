using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
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

    private static Task<HttpResponseMessage> RevenueAsync(Ledger ledger, RevenueRequest request) => ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", request, Json);

    private static async Task<HttpResponseMessage> SaleAsync(Ledger ledger, SalesInvoiceRequest request) =>
        await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/sales-invoices", request with { ItemId = await ledger.RevenueItemAsync(ItemType.Sale) }, Json);

    private static SalesInvoiceRequest Sale(Ledger ledger, string number = "S-1", decimal total = 1150) =>
        new("أحمد", "", number, ledger.Today, total, ledger.AccountId);

    private static Task<ProjectFigures> FiguresAsync(Ledger ledger) => ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));

    private static Task<List<VatQuarterView>> StatementAsync(Ledger ledger, DateOnly from, DateOnly to) => ledger.ReadAsync(db => VatStatement.OfAsync(ledger.ProjectId, from, to, db));

    private static Task<List<VatQuarterView>> StatementAsync(Ledger ledger) => StatementAsync(ledger, QuarterOf(ledger), VatStatement.NextQuarter(QuarterOf(ledger)).AddDays(-1));

    private static async Task ApprovedPurchaseAsync(Ledger ledger, decimal amount, string product = "")
    {
        var purchase = ledger.Purchase(amount) with { Product = product, Submit = true };
        await ledger.RecordAsync(purchase);
        await ledger.SetStatusAsync(purchase.Id, EntryStatus.Approved);
    }

    [Fact]
    public async Task D23_ARefundIsPlainRevenueThatRaisesRevenueAndSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = await FiguresAsync(ledger);

        await ledger.ApproveAsync(await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 140, ledger.AccountId, "")));

        var after = await FiguresAsync(ledger);
        Assert.Equal(before.Revenue + 140, after.Revenue);
        Assert.Equal(before.Surplus + 140, after.Surplus);
        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((ledger.Today, ledger.Today, 140m), (row.PeriodStart, row.PeriodEnd, row.Amount));
    }

    [Fact]
    public async Task APeriodEndingBeforeItStartsIsRefusedForRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 10, ledger.AccountId, "") { PeriodStart = ledger.Today, PeriodEnd = ledger.Today.AddDays(-1) });

        Assert.Equal("invalidPeriod", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task OtherRevenueIsForItsOwnDayUnlessToldAndReachesItsAccount()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        await ledger.ApproveAsync(await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, "إيجار")));

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((ledger.Today, ledger.Today, "حساب المؤسسة"), (row.PeriodStart, row.PeriodEnd, row.AccountName));
        Assert.Equal(70m, (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(account => account.Id == ledger.AccountId).Balance);
    }

    [Fact]
    public async Task R10_RevenueWaitsForApprovalAndCountsOnlyOnceApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = await FiguresAsync(ledger);

        var sent = (await (await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, "إيجار"))).Content.ReadFromJsonAsync<EntryStatusView>(Json))!;

        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
        Assert.Equal(before.Revenue, (await FiguresAsync(ledger)).Revenue);
        Assert.Empty((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);

        await ledger.ApproveAsync(sent.Id);

        Assert.Equal(before.Revenue + 70, (await FiguresAsync(ledger)).Revenue);
    }

    [Fact]
    public async Task D10_ASalesInvoiceWaitsForApprovalAndARejectedOneNeverCounts()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var sent = await SaleAsync(ledger, Sale(ledger, total: 1150));
        var id = (await sent.Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;
        Assert.Equal(0m, (await FiguresAsync(ledger)).Sales);
        Assert.Empty(Assert.Single(await StatementAsync(ledger)).Sales);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reject", new RejectEntryRequest("رقم الفاتورة خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await FiguresAsync(ledger)).Sales);
    }

    [Fact]
    public async Task OnlyWhoeverMovesMoneyRecordsRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");

        var refused = await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, ""), Json);

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [Fact]
    public async Task AC_42_1_D22_ASaleIsATotalThatReachesTheSalesSection()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        await ledger.ApproveAsync(await SaleAsync(ledger, Sale(ledger, total: 1150) with { BuyerVatNumber = MigratedApiFactory.NewVatNumber() }));

        var sale = Assert.Single(Assert.Single(await StatementAsync(ledger)).Sales);
        Assert.Equal(("أحمد", "S-1", 1150m), (sale.Party, sale.Description, sale.Total));
        Assert.Equal(1150m, (await FiguresAsync(ledger)).Sales);
    }

    [Fact]
    public async Task AC_42_1_ASaleAlreadyRecordedAsRevenueCountsOnce()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var recorded = await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.Sale), ledger.Today, 1000, ledger.AccountId, ""));
        var entry = await recorded.Content.ReadFromJsonAsync<EntryStatusView>(Json);

        (await SaleAsync(ledger, Sale(ledger, total: 1000) with { SaleEntryId = entry!.Id })).EnsureSuccessStatusCode();
        await ledger.ApproveAsync(entry.Id);

        Assert.Equal(1000m, (await FiguresAsync(ledger)).Sales);
        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal(("أحمد", "S-1"), (row.BuyerName, row.InvoiceNumber));
        Assert.Equal("unknownSale", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2") with { SaleEntryId = entry.Id })));
    }

    [Fact]
    public async Task WhatASalesInvoiceCannotBeIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await SaleAsync(ledger, Sale(ledger, "S-1"))).EnsureSuccessStatusCode();

        Assert.Equal("duplicateInvoice", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-1"))));
        Assert.Equal("invalidBuyerName", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2") with { BuyerName = " " })));
        Assert.Equal("invalidVatNumber", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2") with { BuyerVatNumber = "12345a" })));
        Assert.Equal("invalidInvoiceNumber", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, " "))));
        Assert.Equal("invalidAmount", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2", 0))));
    }

    [Fact]
    public async Task ReversingASaleTakesItOutOfTheStatementAndTheRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await ledger.ApproveAsync(await SaleAsync(ledger, Sale(ledger)));

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await FiguresAsync(ledger)).Sales);
        Assert.Equal(0m, Assert.Single(await StatementAsync(ledger)).Sales.Sum(row => row.Total));
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
    public async Task FR_28_RevenueGoesOnARevenueItemAndNeverOnAnExpenseOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await RevenueAsync(ledger, new RevenueRequest(ledger.ItemId, ledger.Today, 70, ledger.AccountId, ""));
        await ledger.ApproveAsync(await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, "إيجار")));

        Assert.Equal("unknownItem", await Ledger.CodeOf(refused));
        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((ItemType.OtherRevenue, "إيرادات أخرى"), (row.Type, row.ItemName));
        Assert.Equal(70m, (await FiguresAsync(ledger)).OtherRevenue);
    }

    [Fact]
    public async Task AC_46_7_TheDashboardKeepsRevenueCategoriesApartFromExpenseOnes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.ApproveAsync(await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 40, ledger.AccountId, "") { PeriodStart = QuarterOf(ledger) }));

        var dashboard = (await ledger.Client.GetFromJsonAsync<DashboardView>($"/projects/{ledger.ProjectId}/dashboard", Json))!;

        Assert.Equal(["استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], dashboard.RevenueCategories.Select(category => category.Name));
        Assert.Equal(40m, dashboard.RevenueCategories[0].Total);
        Assert.DoesNotContain(dashboard.Categories.SelectMany(category => category.Items), item => item.Type.IsRevenue());
    }
}
