using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Reports;
using Redix.Api.Revenue;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-053 to UC-058: the periodic report and the report files. No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class ReportFilesTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private async Task<Guid> ApprovedAsync(Ledger ledger, ExpenseRequest request)
    {
        await ledger.RecordAsync(request with { Submit = true, AttachmentId = request.WithoutInvoiceReason.Length > 0 ? Guid.Empty : await ledger.FileAsync() });
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        await factory.WithDatabaseAsync(db => db.Entries.Where(entry => entry.Id == request.Id).ExecuteUpdateAsync(entry => entry.SetProperty(row => row.ApprovedAt, factory.Time.GetUtcNow().UtcDateTime)));
        return request.Id;
    }

    private static async Task<PeriodicReportRowView> IssueAsync(Ledger ledger) =>
        (await (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/periodic-reports", null)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<PeriodicReportRowView>(Json))!;

    private static async Task<PeriodicReportSnapshot> ReadAsync(HttpClient client, Guid projectId, int number) =>
        (await client.GetFromJsonAsync<PeriodicReportSnapshot>($"/projects/{projectId}/periodic-reports/{number}", Json))!;

    [Fact]
    public async Task AC_29_3_ReportsAreNumberedWithNoGapsAndTheNextSaysWhatWaits()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.WithoutInvoice(70) with { Submit = true });

        var next = await ledger.Client.GetFromJsonAsync<NextReportView>($"/projects/{ledger.ProjectId}/periodic-reports/next", Json);
        Assert.Equal((1, 1, 70m), (next!.Number, next.PendingCount, next.PendingAmount));

        Assert.Equal(1, (await IssueAsync(ledger)).Number);
        Assert.Equal(2, (await IssueAsync(ledger)).Number);
        Assert.Equal([2, 1], (await ledger.Client.GetFromJsonAsync<List<PeriodicReportRowView>>($"/projects/{ledger.ProjectId}/periodic-reports", Json))!.Select(row => row.Number));
    }

    [Fact]
    public async Task AC_29_2_AnEntryApprovedLaterLeavesTheIssuedReportAndIsNewInTheNext()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.WithoutInvoice(100));
        await IssueAsync(ledger);
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var late = await ApprovedAsync(ledger, ledger.WithoutInvoice(40));

        await IssueAsync(ledger);

        var first = await ReadAsync(ledger.Client, ledger.ProjectId, 1);
        var second = await ReadAsync(ledger.Client, ledger.ProjectId, 2);
        Assert.Equal(100m, first.Expenses);
        Assert.Equal((100m, 140m, 40m), (second.PreviousExpenses, second.Expenses, second.ExpensesDifference));
        var rows = second.Items.SelectMany(item => item.Subgroups).SelectMany(subgroup => subgroup.Rows).ToList();
        Assert.True(rows.Single(row => row.EntryId == late).IsNew);
        Assert.False(rows.Single(row => row.EntryId != late).IsNew);
    }

    [Fact]
    public async Task TheSummaryAddsUpAsTheStatementDoes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/capital-movements", new CapitalMovementRequest(partner, CapitalMovementType.Contribution, ledger.Today, 1000, ledger.AccountId, "") { CashReason = "نقداً" }, Json)).EnsureSuccessStatusCode();
        await ApprovedAsync(ledger, ledger.WithoutInvoice(300));
        await ledger.RecordAsync(ledger.WithoutInvoice(5) with { Submit = true });

        await IssueAsync(ledger);

        var report = await ReadAsync(ledger.Client, ledger.ProjectId, 1);
        Assert.Equal((1000m, 300m, 700m, 1, 5m), (report.Revenue, report.Expenses, report.Surplus, report.PendingCount, report.PendingAmount));
        Assert.Equal(report.Expenses, report.Categories.Sum(category => category.Total));
        Assert.Equal(1000m, Assert.Single(report.Partners).Balance);
    }

    [Fact]
    public async Task BR_31_EveryoneReadsTheReportAndSeesOnlyTheirOwnPartnerLineButNoOneElseExports()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (reader, readerId) = await ledger.MemberAsync("users.roleReader");
        await PartnerAsync(ledger, "القارئ", readerId);
        await PartnerAsync(ledger, "غيره");
        await IssueAsync(ledger);

        var seen = await ReadAsync(reader, ledger.ProjectId, 1);

        Assert.Equal("القارئ", Assert.Single(seen.Partners).Name);
        Assert.Equal(2, (await ReadAsync(ledger.Client, ledger.ProjectId, 1)).Partners.Count);
        Assert.Equal(HttpStatusCode.Forbidden, (await reader.GetAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/pdf")).StatusCode);
    }

    [Fact]
    public async Task AC_45_1_ThePdfIsAPdfWithAPagePerItemAfterTheSummaryAndPartners()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1"));
        await IssueAsync(ledger);

        var pdf = await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/pdf");

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(System.Text.Encoding.Latin1.GetString(pdf), @"/Type\s*/Page(?!s)").Count);
    }

    [Fact]
    public async Task AC_46_1_TheWorkbookIsLaidOutAsTheStatementAndEveryTotalIsLive()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/capital-movements", new CapitalMovementRequest(partner, CapitalMovementType.Contribution, ledger.Today, 1000, ledger.AccountId, "") { CashReason = "نقداً" }, Json)).EnsureSuccessStatusCode();
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1"));
        await IssueAsync(ledger);

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/xlsx")));
        workbook.RecalculateAllFormulas();

        var itemSheet = workbook.Worksheet(3);
        Assert.Equal(["الخلاصة", "راس المال"], workbook.Worksheets.Take(2).Select(sheet => sheet.Name));
        Assert.All(workbook.Worksheets, sheet => Assert.True(sheet.RightToLeft && sheet.PageSetup.PagesWide == 1 && sheet.PageSetup.PaperSize == XLPaperSize.A4Paper));
        var summary = workbook.Worksheet("الخلاصة");
        Assert.Equal(("رقم التقرير:", "1", "غصون 6"), (summary.Cell("A2").GetString(), summary.Cell("B2").GetString(), summary.Cell("A1").GetString()));
        Assert.Equal(("ايرادات", "مصروفات", "رأس المال", "'راس المال'!E4"), (summary.Cell("A9").GetString(), summary.Cell("D9").GetString(), summary.Cell("A10").GetString(), summary.Cell("B10").FormulaA1));
        Assert.Equal(("مجموع الايرادات", 1000m, 115m, 885m), (summary.Cell("A14").GetString(), summary.Cell("B14").GetValue<decimal>(), summary.Cell("E15").GetValue<decimal>(), summary.Cell("B19").GetValue<decimal>()));
        Assert.Equal(("تفاصيل العظم:", "B32"), (summary.Cell("A22").GetString(), summary.Cell("E12").FormulaA1));
        Assert.Equal(["استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], new[] { 11, 12, 13 }.Select(line => summary.Cell(line, 1).GetString()));
        Assert.Contains(summary.CellsUsed(), cell => cell.HasFormula && cell.FormulaA1.Contains($"'{itemSheet.Name}'!"));
        var capital = workbook.Worksheet("راس المال");
        Assert.Equal(("شريك", "إضافة مبلغ", 1000m), (capital.Cell("A1").GetString(), capital.Cell("B3").GetString(), capital.Cell("E3").GetValue<decimal>()));
        Assert.Equal(("الاجمالي", 1000m), (capital.Cell("A4").GetString(), capital.Cell("E4").GetValue<decimal>()));
        var row = itemSheet.CellsUsed().First(cell => cell.GetString() == "P1").Address.RowNumber;
        Assert.True(itemSheet.Cell(row, 5).IsEmpty() && itemSheet.Cell(row, 6).IsEmpty(), "a price and a quantity nobody entered stay blank, as in the PDF");
        Assert.Equal((itemSheet.Name, "الوصف", 115m), (itemSheet.Cell("A1").GetString(), itemSheet.Cell("B2").GetString(), itemSheet.Cell(row + 1, 7).GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_2_AContractsTotalWhatRemainsAndItsAgreedAmountAreLinesUnderItsTable()
    {
        var item = new ItemReportView(Guid.NewGuid(), "بويه", "التشطيب", 700, [],
            [new SubgroupReportView(Guid.NewGuid(), "", 100, [Row(100, "P1", isNew: true)])],
            [new ContractReportView(Guid.NewGuid(), "مؤسسة الدهان", 1000, false, 500, 500, false, 600, [Row(400, ""), Row(100, "", isOutside: true), Row(100, "")])]);

        using var workbook = ItemWorkbook(item);

        var sheet = workbook.Worksheet(1);
        Assert.Equal(("بويه", "الاجمالي", 100m), (sheet.Cell("A1").GetString(), sheet.Cell("A4").GetString(), sheet.Cell("G4").GetValue<decimal>()));
        Assert.Equal(StatementStyle.Green, sheet.Cell("B3").Style.Fill.BackgroundColor);
        Assert.Equal("دفعات مؤسسة الدهان", sheet.Cell("A6").GetString());
        Assert.True(sheet.Cell("G6").IsEmpty(), "the title row holds only the title");
        Assert.Equal(("الدفعة", 400m), (sheet.Cell("G7").GetString(), sheet.Cell("G8").GetValue<decimal>()));
        Assert.Equal(("الاجمالي", 600m), (sheet.Cell("A11").GetString(), sheet.Cell("G11").GetValue<decimal>()));
        Assert.Equal(("المتبقي", 500m), (sheet.Cell("A12").GetString(), sheet.Cell("G12").GetValue<decimal>()));
        Assert.Equal(("مبلغ الاتفاق", 1000m), (sheet.Cell("A13").GetString(), sheet.Cell("G13").GetValue<decimal>()));
        Assert.Equal(("اجمالي الكل", 700m), (sheet.Cell("A15").GetString(), sheet.Cell("G15").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_2_AContractWithNoAgreedAmountShowsOnlyItsTotal()
    {
        var item = new ItemReportView(Guid.NewGuid(), "الحراسة", "العظم", 200, [], [], [new ContractReportView(Guid.NewGuid(), "الحارس", 0, true, 200, 0, false, 200, [Row(200, "")])]);

        using var workbook = ItemWorkbook(item);

        var sheet = workbook.Worksheet(1);
        Assert.Equal(("الاجمالي", 200m), (sheet.Cell("A4").GetString(), sheet.Cell("D4").GetValue<decimal>()));
        Assert.DoesNotContain(sheet.CellsUsed(), cell => cell.GetString() is "المتبقي" or "مبلغ الاتفاق");
    }

    [Fact]
    public void AC_46_3_ItemsOfOneNameInTwoCategoriesEachNameTheirCategoryOnTheirSheet()
    {
        var report = Report([Item("السباكة", "العظم", Subgroup("", Row(100, "P1"))), Item("السباكة", "التشطيب", Subgroup("", Row(100, "P2")))]);

        using var workbook = new XLWorkbook(new MemoryStream(PeriodicReportWorkbook.Of(report, "")));

        Assert.Equal(["السباكة · العظم", "السباكة · التشطيب"], workbook.Worksheets.Skip(2).Select(sheet => sheet.Name));
    }

    [Fact]
    public void AC_46_7_TheRevenueSideIsCapitalThenEachRevenueCategoryEachOnItsOwnSheetAndSalesAreMuted()
    {
        var refunds = Item("استرجاع الضرائب", "استرجاع ضرائب", Subgroup("", Row(200, ""), Row(100, "", isNew: true))) with { Type = ItemType.VatRefund };
        var sales = Item("البيع", "فلوس البيع المودعة بالحساب", Subgroup("", Row(1000, "S-1"))) with { Type = ItemType.Sale };
        var report = Report([refunds, sales]) with { RevenueCategories = [Category("استرجاع ضرائب", ItemType.VatRefund, refunds), Category("فلوس البيع المودعة بالحساب", ItemType.Sale, sales)] };

        using var workbook = Workbook(report);

        var summary = workbook.Worksheet("الخلاصة");
        var refundSheet = workbook.Worksheet("استرجاع الضرائب");
        Assert.Equal(("المبلغ", 300m), (refundSheet.Cell("E2").GetString(), refundSheet.Cell("E5").GetValue<decimal>()));
        Assert.Equal(StatementStyle.Green, refundSheet.Cell("B4").Style.Fill.BackgroundColor);
        Assert.Equal(("رأس المال", "استرجاع ضرائب", "'استرجاع الضرائب'!E5", 300m), (summary.Cell("A10").GetString(), summary.Cell("A11").GetString(), summary.Cell("B11").FormulaA1, summary.Cell("B11").GetValue<decimal>()));
        Assert.Equal(("فلوس البيع المودعة بالحساب", StatementStyle.Muted), (summary.Cell("A12").GetString(), summary.Cell("A12").Style.Font.FontColor));
        Assert.Equal(("مجموع الايرادات", 1300m, 300m), (summary.Cell("A13").GetString(), summary.Cell("B13").GetValue<decimal>(), summary.Cell("B15").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_5_ASheetOfOneTableHasNoGrandTotalNorPercentageAndTheSummaryPointsAtItsTotal()
    {
        var iron = Item("حديد", "العظم", Subgroup("", Row(300, "H1"))) with { ShowsTablePercentages = true };

        using var workbook = Workbook(Report([iron]) with { Categories = [Category("الحديد", ItemType.Both, iron)] });

        Assert.DoesNotContain(workbook.Worksheet("حديد").CellsUsed(), cell => cell.GetString() is "اجمالي الكل" or "النسبة" or "نسبة الكل");
        Assert.Equal(("'حديد'!G4", 300m), (workbook.Worksheet("الخلاصة").Cell("E10").FormulaA1, workbook.Worksheet("الخلاصة").Cell("E10").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_6_CapitalIsATablePerPartnerWithTheirShareUnderTheirTotalAndTheWholeCapitalForEveryone()
    {
        var khaled = Guid.NewGuid();
        var report = Report([]) with { Capital = 200, CapitalTotal = 200, Partners = [Partner("سالم", Guid.NewGuid(), 150), Partner("خالد", khaled, 50)] };

        using var full = Workbook(report);
        using var own = Workbook(report.ForPartnerOf(khaled));

        var capital = full.Worksheet("راس المال");
        Assert.Equal(("سالم", "الاجمالي", 150m), (capital.Cell("A1").GetString(), capital.Cell("A4").GetString(), capital.Cell("E4").GetValue<decimal>()));
        Assert.Equal(("نسبة المشاركة", 0.75m, "خالد", 0.25m), (capital.Cell("A5").GetString(), capital.Cell("E5").GetValue<decimal>(), capital.Cell("A7").GetString(), capital.Cell("E11").GetValue<decimal>()));
        Assert.Equal(("اجمالي الكل", 200m, "نسبة الكل", 1m), (capital.Cell("A13").GetString(), capital.Cell("E13").GetValue<decimal>(), capital.Cell("A14").GetString(), capital.Cell("E14").GetValue<decimal>()));
        Assert.Equal("'راس المال'!E13", full.Worksheet("الخلاصة").Cell("B10").FormulaA1);
        var seen = own.Worksheet("راس المال");
        Assert.Equal(("خالد", "باقي الشركاء", 150m, 200m), (seen.Cell("A1").GetString(), seen.Cell("A7").GetString(), seen.Cell("E9").GetValue<decimal>(), seen.Cell("E13").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_8_EachSubgroupWithPurchasesIsItsOwnTableTitledWithItsNameTheGeneralOneWithTheItems()
    {
        using var workbook = ItemWorkbook(Item("بلاط", "التشطيب", Subgroup("", Row(100, "B1")), Subgroup("الفلة الجنوبية", Row(50, "B2"))));

        var sheet = workbook.Worksheet(1);
        Assert.Equal(("بلاط", "الفلة الجنوبية"), (sheet.Cell("A1").GetString(), sheet.Cell("A6").GetString()));
        Assert.Equal(("اجمالي الكل", 150m), (sheet.Cell("A11").GetString(), sheet.Cell("G11").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_8_TheSheetPrintsTheItemsTablesInTheOrderTheyWerePutIn()
    {
        var item = new ItemReportView(Guid.NewGuid(), "الحفر", "العظم", 300, [], [new SubgroupReportView(Guid.NewGuid(), "", 100, [Row(100, "H1")]) { SortOrder = 1 }],
            [new ContractReportView(Guid.NewGuid(), "مقاول الحفر", 0, true, 200, 0, false, 200, [Row(200, "")]) { SortOrder = 0 }]);

        using var workbook = ItemWorkbook(item);

        Assert.Equal(("دفعات مقاول الحفر", "الحفر"), (workbook.Worksheet(1).Cell("A1").GetString(), workbook.Worksheet(1).Cell("A6").GetString()));
    }

    [Fact]
    public void AC_46_9_AnItemThatChoosesPercentagesShowsEachTablesShareUnderItsTotalAndTheWholeUnderTheGrandTotal()
    {
        using var workbook = ItemWorkbook(Item("بلاط", "التشطيب", Subgroup("", Row(300, "B1")), Subgroup("الفلة الجنوبية", Row(100, "B2"))) with { ShowsTablePercentages = true });

        var sheet = workbook.Worksheet(1);
        Assert.Equal(("النسبة", 0.75m, "النسبة", 0.25m), (sheet.Cell("A5").GetString(), sheet.Cell("G5").GetValue<decimal>(), sheet.Cell("A11").GetString(), sheet.Cell("G11").GetValue<decimal>()));
        Assert.Equal(("اجمالي الكل", 400m, "نسبة الكل", 1m), (sheet.Cell("A13").GetString(), sheet.Cell("G13").GetValue<decimal>(), sheet.Cell("A14").GetString(), sheet.Cell("G14").GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_10_ACategoryOfOneItemIsOneLineAndOnlyACategoryOfMoreItemsHasADetailTable()
    {
        var fees = Item("الرسوم", "الرسوم والفواتير", Subgroup("", Row(100, "R1")), Subgroup("التأمين", Row(20, "R2")));
        var tiles = Item("بلاط", "تكلفة التشطيب", Subgroup("", Row(30, "B1")));
        var paint = Item("بوية", "تكلفة التشطيب", Subgroup("", Row(20, "P1")));

        using var workbook = Workbook(Report([fees, tiles, paint]) with { Categories = [Category("الرسوم والفواتير", ItemType.Both, fees), Category("تكلفة التشطيب", ItemType.Both, tiles, paint)] });

        var summary = workbook.Worksheet("الخلاصة");
        Assert.Equal(("'الرسوم'!G11", 120m), (summary.Cell("E10").FormulaA1, summary.Cell("E10").GetValue<decimal>()));
        Assert.Equal("تفاصيل تكلفة التشطيب:", Assert.Single(summary.CellsUsed(), cell => cell.GetString().StartsWith("تفاصيل ")).GetString());
    }

    [Fact]
    public void AC_46_11_EveryColumnFitsItsContentAndTextPastFortyFiveCharactersWraps()
    {
        var longText = string.Join(" ", Enumerable.Repeat("دفعات المكتب الهندسي", 6));
        using var workbook = ItemWorkbook(Item("الرسوم", "الرسوم والفواتير", Subgroup("", new ItemEntryRow(Guid.NewGuid(), new DateOnly(2025, 7, 5), longText, "", "A-1", 0, 0, 590400, false, [], false))));

        var sheet = workbook.Worksheet(1);
        Assert.Equal((45d, true), (sheet.Column(2).Width, sheet.Cell("B3").Style.Alignment.WrapText));
        using var file = new ZipArchive(new MemoryStream(ItemReportFiles.Excel(Item("الرسوم", "الرسوم والفواتير", Subgroup("", new ItemEntryRow(Guid.NewGuid(), new DateOnly(2025, 7, 5), longText, "", "A-1", 0, 0, 590400, false, [], false))))));
        using var sheetXml = new StreamReader(file.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        Assert.DoesNotContain("customHeight", sheetXml.ReadToEnd());
        Assert.True(sheet.Column(4).Width >= 10 && sheet.Column(7).Width >= 10, "a date and an amount show whole");
    }

    private static XLWorkbook Workbook(PeriodicReportSnapshot report)
    {
        var workbook = new XLWorkbook(new MemoryStream(PeriodicReportWorkbook.Of(report, "")));
        workbook.RecalculateAllFormulas();
        return workbook;
    }

    private static XLWorkbook ItemWorkbook(ItemReportView item)
    {
        var workbook = new XLWorkbook(new MemoryStream(ItemReportFiles.Excel(item)));
        workbook.RecalculateAllFormulas();
        return workbook;
    }

    private static PeriodicReportSnapshot Report(List<ItemReportView> items) =>
        new("غصون 6", 1, new DateTime(2026, 11, 5), 0, items.Sum(item => item.Total), 0, 0, 0, 0, 0, 0, 0, [], [], 0, 0, 0, 0, 0, [], items, []);

    private static ItemReportView Item(string name, string category, params SubgroupReportView[] subgroups) => new(Guid.NewGuid(), name, category, subgroups.Sum(subgroup => subgroup.Total), [], [.. subgroups], []);

    private static SubgroupReportView Subgroup(string name, params ItemEntryRow[] rows) => new(Guid.NewGuid(), name, rows.Sum(row => row.Amount), [.. rows]);

    private static CategoryTotalView Category(string name, ItemType type, params ItemReportView[] items) =>
        new(Guid.NewGuid(), name, items.Sum(item => item.Total), [.. items.Select(item => new ItemTotalView(item.ItemId, item.ItemName, item.Total, type))]);

    private static ReportPartnerView Partner(string name, Guid userId, decimal balance) =>
        new(name, userId, balance, 0, [new CapitalMovementView(Guid.NewGuid(), CapitalMovementType.Contribution, new DateOnly(2026, 3, 1), balance, "تحويل بنكي", "", "حساب المؤسسة", "", "", false)]);

    private static ItemEntryRow Row(decimal amount, string invoiceNumber, bool isNew = false, bool isOutside = false) =>
        new(Guid.NewGuid(), new DateOnly(2026, 9, 1), "دفعة", "", invoiceNumber, 0, 0, amount, isOutside, [], isNew);

    [Fact]
    public async Task AC_47_1_TheVatWorkbookIsTheAccountantsFile()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1"));
        var start = Invoice.QuarterStartOf(ledger.Today);
        var address = $"/projects/{ledger.ProjectId}/vat-statement/xlsx?from={start:yyyy-MM-dd}&to={VatStatement.NextQuarter(start).AddDays(-1):yyyy-MM-dd}";

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync(address)));

        var sheet = Assert.Single(workbook.Worksheets);
        Assert.StartsWith($"ضريبة الربع {VatStatementFiles.OrdinalOf(start)}", sheet.Name);
        Assert.Equal($"ضريبة المشتريات عن الربع {VatStatementFiles.OrdinalOf(start)} سنة {start.Year}", sheet.Cell(1, 1).GetString());
        Assert.True(sheet.Cell(1, 1).IsMerged());
        Assert.Equal("E3-F3", sheet.Cell(3, 7).FormulaA1);
        Assert.Equal("G3*0.15", sheet.Cell(3, 8).FormulaA1);
        Assert.Equal("G3+H3", sheet.Cell(3, 9).FormulaA1);
        Assert.Equal("SUM(H3:H3)", sheet.Cell(4, 8).FormulaA1);
    }

    [Fact]
    public async Task UC_058_TheZipHoldsEachInvoiceNamedByItsRow()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1"));
        var start = Invoice.QuarterStartOf(ledger.Today);

        using var zip = new ZipArchive(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/vat-statement/zip?from={start:yyyy-MM-dd}&to={VatStatement.NextQuarter(start).AddDays(-1):yyyy-MM-dd}")));

        Assert.Equal($"Q{(start.Month - 1) / 3 + 1}-{start.Year}-001.jpg", Assert.Single(zip.Entries).Name);
    }

    [Fact]
    public async Task AC_33_1_TheItemAndContractsReportsExport()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1"));

        var pdf = await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}/pdf");
        using var item = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}/xlsx")));
        using var contracts = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/contracts/xlsx")));

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.Single(item.Worksheets);
        Assert.Equal("العقود", contracts.Worksheet(1).Name);
    }

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name, Guid userId = default)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }
}
