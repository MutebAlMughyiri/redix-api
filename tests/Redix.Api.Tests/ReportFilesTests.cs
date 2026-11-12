using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Reports;
using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>UC-053 to UC-058: the periodic report and the report files. No figure here is real.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class ReportFilesTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private async Task<Guid> ApprovedAsync(Ledger ledger, EntryRequest request)
    {
        await ledger.RecordAsync(request with { Submit = true, AttachmentId = await ledger.FileAsync() });
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        await factory.WithDatabaseAsync(db => db.Entries.Where(entry => entry.Id == request.Id).ExecuteUpdateAsync(entry => entry.SetProperty(row => row.ApprovedAt, factory.Time.GetUtcNow().UtcDateTime)));
        return request.Id;
    }

    private static async Task<PeriodicReportRowView> IssueAsync(Ledger ledger) =>
        (await (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/periodic-reports", null)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<PeriodicReportRowView>(Json))!;

    private static async Task<PeriodicReportSnapshot> ReadAsync(HttpClient client, Guid projectId, int number) =>
        (await client.GetFromJsonAsync<PeriodicReportSnapshot>($"/projects/{projectId}/periodic-reports/{number}", Json))!;

    [Fact]
    public void APartnersMovementWithNoDescriptionIsNamedInArabicNeverByItsCode()
    {
        foreach (var type in Enum.GetValues<CapitalMovementType>())
        {
            var name = ReportFormats.Movement(type);

            Assert.Equal(Translations.TranslationSeed.Arabic($"capital.{JsonNamingPolicy.CamelCase.ConvertName(type.ToString())}"), name);
            Assert.NotEqual(type.ToString(), name);
        }
    }

    [Fact]
    public async Task AC_29_3_ReportsAreNumberedWithNoGapsAndTheNextSaysWhatWaits()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(70) with { Submit = true });

        var next = await ledger.Client.GetFromJsonAsync<NextReportView>($"/projects/{ledger.ProjectId}/periodic-reports/next", Json);
        Assert.Equal((1, 1, 70m), (next!.Number, next.Waiting.ExpensesCount, next.Waiting.ExpensesAmount));

        Assert.Equal(1, (await IssueAsync(ledger)).Number);
        Assert.Equal(2, (await IssueAsync(ledger)).Number);
        Assert.Equal([2, 1], (await ledger.Client.GetFromJsonAsync<List<PeriodicReportRowView>>($"/projects/{ledger.ProjectId}/periodic-reports", Json))!.Select(row => row.Number));
    }

    [Fact]
    public async Task Row_147_WhatWaitsIsExpensesAndRevenueWithPartnersMoneyApartAndTheSummaryTheNextAndTheIssuedReportAgree()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        await ledger.ApproveAsync(await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 1000)));
        await ledger.RecordAsync(ledger.Purchase(5) with { Submit = true });
        (await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 300))).EnsureSuccessStatusCode();
        (await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 100, isWithdrawal: true))).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.RevenueAsync(ItemType.OtherRevenue, 70, ""), Json)).EnsureSuccessStatusCode();

        var summary = await ledger.Client.GetFromJsonAsync<SummaryView>($"/projects/{ledger.ProjectId}/summary", Json);
        var next = await ledger.Client.GetFromJsonAsync<NextReportView>($"/projects/{ledger.ProjectId}/periodic-reports/next", Json);
        await IssueAsync(ledger);
        var report = await ReadAsync(ledger.Client, ledger.ProjectId, 1);

        var expected = new WaitingFigures(1, 5m, 3, 270m);
        Assert.Equal(expected, summary!.Waiting);
        Assert.Equal(expected, next!.Waiting);
        Assert.Equal((1, 5m, 3, 270m), (report.PendingCount, report.PendingAmount, report.PendingRevenueCount, report.PendingRevenueAmount));
        Assert.Equal((1000m, 0m, 1000m), (report.Revenue, report.Expenses, report.Surplus));
    }

    [Fact]
    public async Task Row_147_TheWorkbookShowsWaitingExpensesBesideTheirDifferenceAndWaitingRevenueUnderTheSurplus()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        await ledger.RecordAsync(ledger.Purchase(5) with { Submit = true });
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 300, notes: ""), Json)).EnsureSuccessStatusCode();
        await IssueAsync(ledger);

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/xlsx")));

        var summary = workbook.Worksheet("الخلاصة");
        IXLCell Labelled(string start) => summary.Column(1).CellsUsed().Single(cell => cell.GetString().StartsWith(start, StringComparison.Ordinal));
        var expenses = Labelled("مصروفات بانتظار الاعتماد");
        var revenue = Labelled("إيرادات ورأس مال بانتظار الاعتماد");
        Assert.Equal(("مصروفات بانتظار الاعتماد (1)", 8, 5m), (expenses.GetString(), expenses.Address.RowNumber, expenses.CellRight().GetValue<decimal>()));
        Assert.Equal(("إيرادات ورأس مال بانتظار الاعتماد (1)", Labelled("الفائض\n").Address.RowNumber + 1, 300m), (revenue.GetString(), revenue.Address.RowNumber, revenue.CellRight().GetValue<decimal>()));
    }

    [Fact]
    public async Task App_119_D4_ACreditNotePrintsAsItsOwnMinusRowInTheStatementsFormat()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(1150));
        await ApprovedAsync(ledger, ledger.Purchase(320) with { IsCreditNote = true });
        await IssueAsync(ledger);

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/xlsx")));

        var minus = workbook.Worksheets.Where(sheet => sheet.Name != "الخلاصة").SelectMany(sheet => sheet.CellsUsed()).Where(cell => cell.Value.IsNumber && cell.Value.GetNumber() == -320).ToList();
        Assert.NotEmpty(minus);
        Assert.All(minus, cell => Assert.Equal(("#,##0.00;(#,##0.00)", "(320.00)"), (cell.Style.NumberFormat.Format, cell.GetFormattedString())));
        Assert.Contains(workbook.Worksheets.Where(sheet => sheet.Name != "الخلاصة").SelectMany(sheet => sheet.CellsUsed()), cell => cell.Value.IsNumber && cell.Value.GetNumber() == 830);
    }

    [Fact]
    public async Task AC_29_2_AnEntryApprovedLaterLeavesTheIssuedReportAndIsNewInTheNext()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(100));
        await IssueAsync(ledger);
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var late = await ApprovedAsync(ledger, ledger.Purchase(40));

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
        await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 1000, notes: ""), Json));
        await ApprovedAsync(ledger, ledger.Purchase(300));
        await ledger.RecordAsync(ledger.Purchase(5) with { Submit = true });

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
        var (reader, readerId) = await ledger.MemberAsync("Reader");
        await PartnerAsync(ledger, "القارئ", readerId);
        await PartnerAsync(ledger, "غيره");
        await IssueAsync(ledger);

        var seen = await ReadAsync(reader, ledger.ProjectId, 1);

        Assert.Equal("القارئ", Assert.Single(seen.Partners).Name);
        Assert.Equal(2, (await ReadAsync(ledger.Client, ledger.ProjectId, 1)).Partners.Count);
        Assert.Equal(HttpStatusCode.Forbidden, (await reader.GetAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/pdf")).StatusCode);
    }

    [Fact]
    public async Task AC_45_1_ThePdfIsAPdfWithAPagePerItemAfterTheSummary()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115));
        await IssueAsync(ledger);

        var pdf = await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/pdf");

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.Equal(2, Regex.Matches(Encoding.Latin1.GetString(pdf), @"/Type\s*/Page(?!s)").Count);
    }

    [Fact]
    public async Task AC_46_1_TheWorkbookIsLaidOutAsTheStatementAndEveryTotalIsLive()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partner, 1000, notes: ""), Json));
        await ApprovedAsync(ledger, ledger.Purchase(115) with { Product = "P1" });
        await IssueAsync(ledger);

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/periodic-reports/1/xlsx")));
        workbook.RecalculateAllFormulas();

        var itemSheet = workbook.Worksheet(2);
        Assert.Equal(["الخلاصة", itemSheet.Name, "رأس المال"], workbook.Worksheets.Select(sheet => sheet.Name));
        Assert.All(workbook.Worksheets, sheet => Assert.True(sheet.RightToLeft && sheet.PageSetup.PagesWide == 1 && sheet.PageSetup.PaperSize == XLPaperSize.A4Paper));
        var summary = workbook.Worksheet("الخلاصة");
        Assert.Equal(("رقم التقرير:", "1", "غصون 6"), (summary.Cell("A2").GetString(), summary.Cell("B2").GetString(), summary.Cell("A1").GetString()));
        Assert.Equal(("ايرادات", "مصروفات", "رأس المال", "'رأس المال'!D4"), (summary.Cell("A9").GetString(), summary.Cell("D9").GetString(), summary.Cell("A10").GetString(), summary.Cell("B10").FormulaA1));
        Assert.Equal(("مجموع الايرادات", 1000m, 115m, 885m), (summary.Cell("A14").GetString(), summary.Cell("B14").GetValue<decimal>(), summary.Cell("E15").GetValue<decimal>(), summary.Cell("B18").GetValue<decimal>()));
        Assert.Equal(("تفاصيل العظم:", "B31"), (summary.Cell("A21").GetString(), summary.Cell("E12").FormulaA1));
        Assert.Equal(["استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], new[] { 11, 12, 13 }.Select(line => summary.Cell(line, 1).GetString()));
        Assert.Contains(summary.CellsUsed(), cell => cell.HasFormula && cell.FormulaA1.Contains($"'{itemSheet.Name}'!"));
        var capital = workbook.Worksheet("رأس المال");
        Assert.Equal(("شريك", "إضافة مبلغ", 1000m), (capital.Cell("A1").GetString(), capital.Cell("B3").GetString(), capital.Cell("D3").GetValue<decimal>()));
        Assert.Equal(("الاجمالي", 1000m), (capital.Cell("A4").GetString(), capital.Cell("D4").GetValue<decimal>()));
        var row = itemSheet.CellsUsed().First(cell => cell.GetString().StartsWith("P1 ·")).Address.RowNumber;
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

        using var workbook = new XLWorkbook(new MemoryStream(PeriodicReportWorkbook.Of(report)));

        Assert.Equal(["السباكة · العظم", "السباكة · التشطيب"], workbook.Worksheets.Skip(1).Select(sheet => sheet.Name));
    }

    [Fact]
    public void AC_46_7_UC_109_TheRevenueSideIsEachRevenueCategoryInTheStructuresOrderRasAlMalFirstEachOnItsOwnSheetAndSalesAreMuted()
    {
        var capital = Capital(("سالم", 500));
        var refunds = Item("استرجاع الضرائب", "استرجاع ضرائب", Subgroup("", Row(200, ""), Row(100, "", isNew: true))) with { Type = ItemType.VatRefund };
        var sales = Item("البيع", "فلوس البيع المودعة بالحساب", Subgroup("", Row(1000, "S-1"))) with { Type = ItemType.Sale };
        var report = Report([capital, refunds, sales]) with
        {
            RevenueCategories = [Category("رأس المال", ItemType.Capital, capital), Category("استرجاع ضرائب", ItemType.VatRefund, refunds), Category("فلوس البيع المودعة بالحساب", ItemType.Sale, sales)],
        };

        using var workbook = Workbook(report);

        var summary = workbook.Worksheet("الخلاصة");
        var refundSheet = workbook.Worksheet("استرجاع الضرائب");
        Assert.Equal(("المبلغ", 300m), (refundSheet.Cell("D2").GetString(), refundSheet.Cell("D5").GetValue<decimal>()));
        Assert.Equal(StatementStyle.Green, refundSheet.Cell("B4").Style.Fill.BackgroundColor);
        Assert.Equal(("رأس المال", "'رأس المال'!D4", 500m), (summary.Cell("A10").GetString(), summary.Cell("B10").FormulaA1, summary.Cell("B10").GetValue<decimal>()));
        Assert.Equal(("استرجاع ضرائب", "'استرجاع الضرائب'!D5", 300m), (summary.Cell("A11").GetString(), summary.Cell("B11").FormulaA1, summary.Cell("B11").GetValue<decimal>()));
        Assert.Equal(("فلوس البيع المودعة بالحساب", StatementStyle.Muted), (summary.Cell("A12").GetString(), summary.Cell("A12").Style.Font.FontColor));
        Assert.Equal(("مجموع الايرادات", 1800m, 800m), (summary.Cell("A13").GetString(), summary.Cell("B13").GetValue<decimal>(), summary.Cell("B15").GetValue<decimal>()));
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
    public void AC_46_6_UC_109_RasAlMalIsAnItemWhoseTablesAreItsPartnersEachOnesShareUnderTheirTotalAndTheWholeCapitalUnderThem()
    {
        var capital = Capital(("سالم", 150), ("خالد", 50));

        using var workbook = Workbook(Report([capital]) with { RevenueCategories = [Category("رأس المال", ItemType.Capital, capital)] });

        var sheet = workbook.Worksheet("رأس المال");
        Assert.Equal(("سالم", "الاجمالي", 150m), (sheet.Cell("A1").GetString(), sheet.Cell("A4").GetString(), sheet.Cell("D4").GetValue<decimal>()));
        Assert.Equal(("النسبة", 0.75m, "خالد", 0.25m), (sheet.Cell("A5").GetString(), sheet.Cell("D5").GetValue<decimal>(), sheet.Cell("A7").GetString(), sheet.Cell("D11").GetValue<decimal>()));
        Assert.Equal(("اجمالي الكل", 200m, "نسبة الكل", 1m), (sheet.Cell("A13").GetString(), sheet.Cell("D13").GetValue<decimal>(), sheet.Cell("A14").GetString(), sheet.Cell("D14").GetValue<decimal>()));
        Assert.Equal("'رأس المال'!D13", workbook.Worksheet("الخلاصة").Cell("B10").FormulaA1);
    }

    [Fact]
    public void BR_31_UC_109_AReportForSomeoneWithChosenPartnersHoldsTheirLinesAndNoItemReportWhichOnlyTheFilesTake()
    {
        var khaled = Guid.NewGuid();
        var report = Report([Capital(("سالم", 150), ("خالد", 50))]) with { Partners = [new ReportPartnerView("سالم", Guid.NewGuid(), 150, 75), new ReportPartnerView("خالد", khaled, 50, 25)] };

        var seen = report.SeenBy(new PartnerSight(false, [khaled]));

        Assert.Equal("خالد", Assert.Single(seen.Partners).Name);
        Assert.Empty(seen.Items);
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
        var workbook = new XLWorkbook(new MemoryStream(PeriodicReportWorkbook.Of(report)));
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
        new("غصون 6", 1, new DateTime(2026, 11, 5), 0, items.Sum(item => item.Total), 0, 0, 0, 0, 0, 0, [], [], 0, 0, 0, 0, 0, [], items, [], 0, 0);

    private static ItemReportView Item(string name, string category, params SubgroupReportView[] subgroups) => new(Guid.NewGuid(), name, category, subgroups.Sum(subgroup => subgroup.Total), [], [.. subgroups], []);

    private static SubgroupReportView Subgroup(string name, params ItemEntryRow[] rows) => new(Guid.NewGuid(), name, rows.Sum(row => row.Amount), [.. rows]);

    private static CategoryTotalView Category(string name, ItemType type, params ItemReportView[] items) =>
        new(Guid.NewGuid(), name, items.Sum(item => item.Total), [.. items.Select(item => new ItemTotalView(item.ItemId, item.ItemName, item.Total, type))]);

    /// <summary>رأس المال as an item (redix-api #109): a table per partner holding one contribution, its sheet giving each one's share.</summary>
    private static ItemReportView Capital(params (string Name, decimal Amount)[] partners) =>
        Item("رأس المال", "رأس المال", [.. partners.Select(partner => Subgroup(partner.Name, Row(partner.Amount, "")))]) with { Type = ItemType.Capital, ShowsTablePercentages = true };

    private static ItemEntryRow Row(decimal amount, string invoiceNumber, bool isNew = false, bool isOutside = false) =>
        new(Guid.NewGuid(), new DateOnly(2026, 9, 1), "دفعة", "", invoiceNumber, 0, 0, amount, isOutside, [], isNew);

    [Fact]
    public async Task AC_47_1_D19_TheVatWorkbookWorksVatOutOfEachTotalAtTheConfiguredRate()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115));
        var start = VatStatement.QuarterStartOf(ledger.Today);
        var address = $"/projects/{ledger.ProjectId}/vat-statement/xlsx?from={start:yyyy-MM-dd}&to={VatStatement.NextQuarter(start).AddDays(-1):yyyy-MM-dd}";

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync(address)));

        var sheet = Assert.Single(workbook.Worksheets);
        Assert.StartsWith("ضريبة الربع", sheet.Name);
        Assert.True(sheet.Cell(1, 1).IsMerged());
        Assert.Equal((115, "شركة النخيل"), (sheet.Cell(3, 5).GetValue<int>(), sheet.Cell(3, 2).GetString()));
        Assert.Equal("E3*15/(100+15)", sheet.Cell(3, 6).FormulaA1);
        Assert.Equal("E3-F3", sheet.Cell(3, 7).FormulaA1);
        Assert.Equal("SUM(F3:F3)", sheet.Cell(4, 6).FormulaA1);
    }

    [Fact]
    public async Task D18_TheRateIsTheConfigValueOfVatRatePercent()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115));
        var start = VatStatement.QuarterStartOf(ledger.Today);
        var address = $"/projects/{ledger.ProjectId}/vat-statement/xlsx?from={start:yyyy-MM-dd}&to={VatStatement.NextQuarter(start).AddDays(-1):yyyy-MM-dd}";
        await factory.WithDatabaseAsync(db => db.ConfigValues.Where(row => row.Key == "vatRatePercent").ExecuteUpdateAsync(row => row.SetProperty(value => value.Value, "5")));

        try
        {
            using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync(address)));
            Assert.Equal("E3*5/(100+5)", workbook.Worksheet(1).Cell(3, 6).FormulaA1);
        }
        finally
        {
            await factory.WithDatabaseAsync(db => db.ConfigValues.Where(row => row.Key == "vatRatePercent").ExecuteUpdateAsync(row => row.SetProperty(value => value.Value, "15")));
        }
    }

    [Fact]
    public void Row_130_EveryFigureOfTheWorkbooksItemSheetsRasAlMalAmongThemIsOnThePeriodicReportsPdf()
    {
        var report = Report([Tiles(), Capital(("متعب", 3000), ("نواف", 1000))]);

        using var workbook = Workbook(report);

        var figures = FiguresOf(workbook);
        Assert.Contains("75.00%", figures);
        Assert.Contains("30,000.00", figures);
        AssertOnThePdf(figures, PeriodicReportFiles.Pdf(report));
    }

    [Fact]
    public void Row_130_EveryFigureOfAnItemsWorkbookIsOnItsPdf()
    {
        var item = Tiles();

        using var workbook = new XLWorkbook(new MemoryStream(ItemReportFiles.Excel(item)));
        workbook.RecalculateAllFormulas();

        var figures = FiguresOf(workbook);
        Assert.Contains("4,000.00", figures);
        AssertOnThePdf(figures, ItemReportFiles.Pdf(item));
    }

    /// <summary>An item with a general table, a named one with a new row, and a contract with a payment outside it, whose sheet calculates each table's percentage.</summary>
    private static ItemReportView Tiles()
    {
        var contract = new ContractReportView(Guid.NewGuid(), "مقاول البلاط", 34000, false, 30000, 4000, false, 30500, [Row(30000, ""), Row(500, "", isOutside: true)]);
        var item = Item("البلاط", "التشطيب", Subgroup("", Row(115, "")), Subgroup("الدور الأرضي", Row(200, ""), Row(85.5m, "", isNew: true)));
        return item with { Contracts = [contract], Total = item.Total + 30500, ShowsTablePercentages = true };
    }

    /// <summary>Every figure in the amount column of every sheet but الخلاصة, as the statement writes it: money as 1,234.56, a share as 12.34%.</summary>
    private static List<string> FiguresOf(XLWorkbook workbook) =>
        [.. workbook.Worksheets.Where(sheet => sheet.Name != "الخلاصة").SelectMany(sheet => sheet.Column(sheet.LastColumnUsed()!.ColumnNumber()).CellsUsed())
            .Where(cell => cell.Value.IsNumber)
            .Select(cell => cell.Style.NumberFormat.Format.Contains('%')
                ? ((decimal)cell.Value.GetNumber() * 100).ToString("0.00", CultureInfo.InvariantCulture) + "%"
                : ReportFormats.Money((decimal)cell.Value.GetNumber()))];

    /// <summary>Every one of <paramref name="figures"/> is a word of <paramref name="pdf"/>, as many times as the workbook shows it, so no figure can stand in for another.</summary>
    private static void AssertOnThePdf(List<string> figures, byte[] pdf)
    {
        using var document = UglyToad.PdfPig.PdfDocument.Open(pdf);
        var words = document.GetPages().SelectMany(page => page.GetWords()).GroupBy(word => word.Text).ToDictionary(group => group.Key, group => group.Count());
        Assert.All(figures.GroupBy(figure => figure), figure => Assert.True(words.GetValueOrDefault(figure.Key) >= figure.Count(), $"{figure.Key}: {figure.Count()} in the workbook, {words.GetValueOrDefault(figure.Key)} on the PDF"));
    }

    [Fact]
    public async Task AC_33_1_TheItemAndContractsReportsExport()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(115));

        var pdf = await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}/pdf");
        using var item = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}/xlsx")));
        using var contracts = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/contracts/xlsx")));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.Single(item.Worksheets);
        Assert.Equal("العقود", contracts.Worksheet(1).Name);
    }

    [Fact]
    public async Task Findings131_TheContractsWorkbookIsOneStatementTableOfWhatEachContractHasLeft()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new Contracts.AddContractRequest(ledger.SupplierId, ledger.ItemId, 1000m, false), Json)).EnsureSuccessStatusCode();

        using var workbook = new XLWorkbook(new MemoryStream(await ledger.Client.GetByteArrayAsync($"/projects/{ledger.ProjectId}/reports/contracts/xlsx")));
        var sheet = workbook.Worksheet(1);

        Assert.Equal("العقود", sheet.Cell(1, 1).GetString());
        Assert.Equal(["المقاول", "البند", "قيمة العقد", "المدفوع", "المتبقي"], Enumerable.Range(2, 5).Select(column => sheet.Cell(2, column).GetString()));
        Assert.Equal((1000m, 0m, 1000m), (sheet.Cell(3, 4).GetValue<decimal>(), sheet.Cell(3, 5).GetValue<decimal>(), sheet.Cell(3, 6).GetValue<decimal>()));
        Assert.Equal(("الاجمالي", "SUM(F3:F3)"), (sheet.Cell(4, 1).GetString(), sheet.Cell(4, 6).FormulaA1));
    }

    /// <summary>Adds the partner <paramref name="name"/>, chosen for <paramref name="seenBy"/> when given (redix-api #106).</summary>
    private static async Task<Guid> PartnerAsync(Ledger ledger, string name, Guid seenBy = default)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name), Json)).EnsureSuccessStatusCode();
        var id = (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
        if (seenBy != default)
        {
            await ledger.ChooseAsync(seenBy, id);
        }
        return id;
    }
}
