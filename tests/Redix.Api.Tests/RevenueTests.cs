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

/// <summary>UC-047, UC-048, UC-056: revenue, sales invoices and the VAT statement. No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class RevenueTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static DateOnly QuarterOf(Ledger ledger) => Invoice.QuarterStartOf(ledger.Today);

    private static Task<HttpResponseMessage> RevenueAsync(Ledger ledger, RevenueRequest request) => ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", request, Json);

    private static async Task<HttpResponseMessage> SaleAsync(Ledger ledger, SalesInvoiceRequest request) =>
        await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/sales-invoices", request with { ItemId = await ledger.RevenueItemAsync(ItemType.Sale) }, Json);

    private static SalesInvoiceRequest Sale(Ledger ledger, string number = "S-1", decimal amount = 1000, decimal discount = 0, SaleTaxType tax = SaleTaxType.Vat) =>
        new("أحمد", "", number, ledger.Today, amount, discount, tax, ledger.AccountId);

    private static Task<ProjectFigures> FiguresAsync(Ledger ledger) => ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));

    private static async Task<VatStatementView> StatementAsync(Ledger ledger)
    {
        var start = QuarterOf(ledger);
        return (await ledger.Client.GetFromJsonAsync<VatStatementView>($"/projects/{ledger.ProjectId}/vat-statement?from={start:yyyy-MM-dd}&to={VatStatement.NextQuarter(start).AddDays(-1):yyyy-MM-dd}", Json))!;
    }

    private static async Task ApprovedPurchaseAsync(Ledger ledger, decimal amount, string number)
    {
        var purchase = ledger.Purchase(amount, number) with { Submit = true, AttachmentId = await ledger.FileAsync() };
        await ledger.RecordAsync(purchase);
        await ledger.SetStatusAsync(purchase.Id, EntryStatus.Approved);
    }

    [Fact]
    public async Task AC_28_1_ARefundRaisesRevenueAndSurplusAndShowsItsDifferenceFromTheStatement()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 1150, "2617");
        var before = await FiguresAsync(ledger);

        (await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 140, ledger.AccountId, "") { PeriodStart = QuarterOf(ledger) })).EnsureSuccessStatusCode();

        var after = await FiguresAsync(ledger);
        Assert.Equal(before.Revenue + 140, after.Revenue);
        Assert.Equal(before.Surplus + 140, after.Surplus);
        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((150m, -10m), (row.StatementRefund, row.RefundDifference));
    }

    [Fact]
    public async Task ARefundIsForAQuarter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 10, ledger.AccountId, "") { PeriodStart = QuarterOf(ledger).AddDays(1) });

        Assert.Equal("invalidPeriod", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task OtherRevenueIsForItsOwnDayUnlessToldAndReachesItsAccount()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        (await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, "إيجار"))).EnsureSuccessStatusCode();

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((ledger.Today, ledger.Today, "حساب المؤسسة"), (row.PeriodStart, row.PeriodEnd, row.AccountName));
        Assert.Equal(70m, (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(account => account.Id == ledger.AccountId).Balance);
    }

    [Fact]
    public async Task OnlyWhoeverMovesMoneyRecordsRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("users.roleField");

        var refused = await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, ""), Json);

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [Fact]
    public async Task AC_42_1_ASaleWithVatReachesTheSalesSectionAndChangesTheNetDue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 1150, "2617");

        (await SaleAsync(ledger, Sale(ledger, amount: 2000, discount: 1000))).EnsureSuccessStatusCode();

        var quarter = Assert.Single((await StatementAsync(ledger)).Quarters);
        var sale = Assert.Single(quarter.Sales);
        Assert.Equal((1000m, 150m, 1150m), (sale.AfterDiscount, sale.Tax, sale.Total));
        Assert.Equal((150m, 150m, 0m), (quarter.SalesVat, quarter.PurchasesVat, quarter.NetDue));
    }

    [Fact]
    public async Task ASaleWithTheRealEstateTaxShowsItsTaxAndStaysOutOfTheNetVat()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        (await SaleAsync(ledger, Sale(ledger, tax: SaleTaxType.RealEstateTransaction))).EnsureSuccessStatusCode();

        var quarter = Assert.Single((await StatementAsync(ledger)).Quarters);
        Assert.Equal((0m, 50m, 0m), (quarter.SalesVat, quarter.RealEstateTax, quarter.NetDue));
        Assert.Equal(1000m, (await FiguresAsync(ledger)).Sales);
    }

    [Fact]
    public async Task AC_42_1_ASaleAlreadyRecordedAsRevenueCountsOnce()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var recorded = await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.Sale), ledger.Today, 1000, ledger.AccountId, ""));
        var entry = await recorded.Content.ReadFromJsonAsync<EntryStatusView>(Json);

        (await SaleAsync(ledger, Sale(ledger, tax: SaleTaxType.RealEstateTransaction) with { SaleEntryId = entry!.Id })).EnsureSuccessStatusCode();

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
        Assert.Equal("invalidVatNumber", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2") with { BuyerVatNumber = "12345" })));
        Assert.Equal("invalidInvoiceNumber", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, " "))));
        Assert.Equal("invalidAmount", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2", 0))));
        Assert.Equal("invalidDiscount", await Ledger.CodeOf(await SaleAsync(ledger, Sale(ledger, "S-2", 100, 200))));
    }

    [Fact]
    public async Task ReversingASaleTakesItOutOfTheStatementAndTheRevenue()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var answer = await SaleAsync(ledger, Sale(ledger));
        var entry = await answer.Content.ReadFromJsonAsync<EntryStatusView>(Json);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entry!.Id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(0m, (await FiguresAsync(ledger)).Sales);
        Assert.Equal(0m, Assert.Single((await StatementAsync(ledger)).Quarters).SalesVat);
    }

    [Fact]
    public async Task AC_32_1_TheStatementAddsTheKeptDecimalsAndGeneratesItsDescription()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 100.01m, "A1");
        await ApprovedPurchaseAsync(ledger, 100.01m, "A2");

        var quarter = Assert.Single((await StatementAsync(ledger)).Quarters);

        Assert.Equal(2, quarter.Purchases.Count);
        Assert.Equal(26.0896m, quarter.PurchasesVat);
        Assert.Equal(1, quarter.Purchases[0].Row);
        Assert.EndsWith("/ شركة النخيل / فA1", quarter.Purchases[0].Description);
    }

    [Fact]
    public async Task AC_32_2_ACustomPeriodAcrossQuartersHasASectionPerQuarter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var statement = await ledger.Client.GetFromJsonAsync<VatStatementView>($"/projects/{ledger.ProjectId}/vat-statement?from=2026-08-15&to=2027-01-10", Json);

        Assert.Equal([new DateOnly(2026, 7, 1), new DateOnly(2026, 10, 1), new DateOnly(2027, 1, 1)], statement!.Quarters.Select(quarter => quarter.Start));
        Assert.Equal(new DateOnly(2026, 9, 30), statement.Quarters[0].End);
    }

    [Fact]
    public async Task APeriodEndingBeforeItStartsIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/vat-statement?from=2026-08-15&to=2026-08-14");

        Assert.Equal("invalidPeriod", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task AC_32_3_WhatWaitsHasNoVatNumberOrNoInvoiceIsLeftOutAndListed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var waiting = ledger.Purchase(115, "W1") with { Submit = true, AttachmentId = await ledger.FileAsync() };
        await ledger.RecordAsync(waiting);
        await ledger.RecordAsync(ledger.WithoutInvoice() with { Submit = true });
        await ApprovedPurchaseAsync(ledger, 115, "N1");
        await factory.WithDatabaseAsync(db => db.Suppliers.Where(row => row.Id == ledger.SupplierId).ExecuteUpdateAsync(row => row.SetProperty(supplier => supplier.VatNumber, "")));

        var statement = await StatementAsync(ledger);

        Assert.Empty(Assert.Single(statement.Quarters).Purchases);
        Assert.Equal(["noVatNumber", "pending", "withoutInvoice"], statement.Exceptions.Select(row => row.Reason).Order());
    }

    [Fact]
    public async Task UC_058_ARowWithNoFileIsListed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var noFile = ledger.Purchase(115, "F1");
        await ledger.RecordAsync(noFile);
        await ledger.SetStatusAsync(noFile.Id, EntryStatus.Approved);

        var statement = await StatementAsync(ledger);

        Assert.Single(Assert.Single(statement.Quarters).Purchases);
        Assert.Equal("noFile", Assert.Single(statement.Exceptions).Reason);
    }

    [Fact]
    public async Task BR_17_AnInvoiceDeclaredLaterLeavesItsQuarterAndIsListedInTheLaterOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 115, "L1");
        var entryId = await ledger.ReadAsync(db => db.Invoices.Where(row => row.Number == "L1" && db.Entries.Any(entry => entry.Id == row.EntryId && entry.ProjectId == ledger.ProjectId)).Select(row => row.EntryId).SingleAsync());
        var next = VatStatement.NextQuarter(QuarterOf(ledger));

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/vat-period", new VatPeriodRequest(next), Json)).EnsureSuccessStatusCode();

        Assert.Empty(Assert.Single((await StatementAsync(ledger)).Quarters).Purchases);
        var later = await ledger.Client.GetFromJsonAsync<VatStatementView>($"/projects/{ledger.ProjectId}/vat-statement?from={next:yyyy-MM-dd}&to={VatStatement.NextQuarter(next).AddDays(-1):yyyy-MM-dd}", Json);
        Assert.Single(Assert.Single(later!.Quarters).Purchases);
        Assert.Equal("declaredLater", Assert.Single(later.Exceptions).Reason);
    }

    [Fact]
    public async Task AnInvoiceCannotBeDeclaredInAnEarlierQuarter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedPurchaseAsync(ledger, 115, "L1");
        var entryId = await ledger.ReadAsync(db => db.Invoices.Where(row => row.Number == "L1" && db.Entries.Any(entry => entry.Id == row.EntryId && entry.ProjectId == ledger.ProjectId)).Select(row => row.EntryId).SingleAsync());

        var refused = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/vat-period", new VatPeriodRequest(QuarterOf(ledger).AddMonths(-3)), Json);

        Assert.Equal("invalidPeriod", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task FR_28_RevenueGoesOnARevenueItemAndNeverOnAnExpenseOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await RevenueAsync(ledger, new RevenueRequest(ledger.ItemId, ledger.Today, 70, ledger.AccountId, ""));
        (await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 70, ledger.AccountId, "إيجار"))).EnsureSuccessStatusCode();

        Assert.Equal("unknownItem", await Ledger.CodeOf(refused));
        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<RevenueRowView>>($"/projects/{ledger.ProjectId}/revenue", Json))!);
        Assert.Equal((ItemType.OtherRevenue, "إيرادات أخرى"), (row.Type, row.ItemName));
        Assert.Equal(70m, (await FiguresAsync(ledger)).OtherRevenue);
    }

    [Fact]
    public async Task AC_46_7_TheDashboardKeepsRevenueCategoriesApartFromExpenseOnes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await RevenueAsync(ledger, new RevenueRequest(await ledger.RevenueItemAsync(ItemType.VatRefund), ledger.Today, 40, ledger.AccountId, "") { PeriodStart = QuarterOf(ledger) })).EnsureSuccessStatusCode();

        var dashboard = (await ledger.Client.GetFromJsonAsync<DashboardView>($"/projects/{ledger.ProjectId}/dashboard", Json))!;

        Assert.Equal(["استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], dashboard.RevenueCategories.Select(category => category.Name));
        Assert.Equal(40m, dashboard.RevenueCategories[0].Total);
        Assert.DoesNotContain(dashboard.Categories.SelectMany(category => category.Items), item => item.Type.IsRevenue());
    }
}
