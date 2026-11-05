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
        Assert.Equal(("ايرادات", "مصروفات", "رأس المال", "'راس المال'!C3"), (summary.Cell("A9").GetString(), summary.Cell("D9").GetString(), summary.Cell("A10").GetString(), summary.Cell("B10").FormulaA1));
        Assert.Equal((1000m, 115m, 885m), (summary.Cell("B13").GetValue<decimal>(), summary.Cell("E15").GetValue<decimal>(), summary.Cell("B18").GetValue<decimal>()));
        Assert.Equal(("تفاصيل تكاليف العظم:", "B31"), (summary.Cell("A21").GetString(), summary.Cell("E12").FormulaA1));
        Assert.Contains(summary.CellsUsed(), cell => cell.HasFormula && cell.FormulaA1.Contains($"'{itemSheet.Name}'!"));
        var capital = workbook.Worksheet("راس المال");
        Assert.Equal(("شريك", 1000m, 100m), (capital.Cell("B2").GetString(), capital.Cell("C2").GetValue<decimal>(), capital.Cell("D2").GetValue<decimal>()));
        Assert.Equal(("إضافة مبلغ", 1000m), (capital.Cell("H3").GetString(), capital.Cell("K4").GetValue<decimal>()));
        var row = itemSheet.CellsUsed().First(cell => cell.GetString() == "P1").Address.RowNumber;
        Assert.True(itemSheet.Cell(row, 6).IsEmpty() && itemSheet.Cell(row, 7).IsEmpty(), "a price and a quantity nobody entered stay blank, as in the PDF");
        Assert.Equal(("الوصف", 115m), (itemSheet.Cell("B1").GetString(), itemSheet.Cell(row + 1, 8).GetValue<decimal>()));
    }

    [Fact]
    public void AC_46_2_AContractIsABlockWithItsValueAndWhatRemainsAfterEachPayment()
    {
        var item = new ItemReportView(Guid.NewGuid(), "بويه", "التشطيب", 700, [],
            [new SubgroupReportView(Guid.NewGuid(), "", 100, [Row(100, "P1", isNew: true)])],
            [new ContractReportView(Guid.NewGuid(), "مؤسسة الدهان", 1000, false, 500, 500, false, 600, [Row(400, ""), Row(100, "", isOutside: true), Row(100, "")])]);

        using var workbook = new XLWorkbook(new MemoryStream(ItemReportFiles.Excel(item)));
        workbook.RecalculateAllFormulas();

        var sheet = workbook.Worksheet(1);
        Assert.Equal(("الاجمالي", 100m), (sheet.Cell("A3").GetString(), sheet.Cell("H3").GetValue<decimal>()));
        Assert.Equal(StatementStyle.Green, sheet.Cell("B2").Style.Fill.BackgroundColor);
        Assert.Equal(("دفعات بويه مؤسسة الدهان", 1000m), (sheet.Cell("A6").GetString(), sheet.Cell("C6").GetValue<decimal>()));
        Assert.Equal([600m, 600m, 500m], new[] { 8, 9, 10 }.Select(line => sheet.Cell(line, 5).GetValue<decimal>()));
        Assert.Equal((600m, 700m), (sheet.Cell("F6").GetValue<decimal>(), sheet.Cell("H13").GetValue<decimal>()));
        Assert.Equal("اجمالي الكل", sheet.Cell("A13").GetString());
    }

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
