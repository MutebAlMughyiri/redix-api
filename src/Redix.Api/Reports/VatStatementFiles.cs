using System.Globalization;
using ClosedXML.Excel;
using Redix.Api.Revenue;

namespace Redix.Api.Reports;

/// <summary>
/// The كشف الضريبة as the accountant's workbook: a sheet per quarter, its purchases, each row's VAT and amount before VAT as formulas on its total at the configured rate, and the net due at the foot (redix-api #96, D5, D19). Every total gives its VAT, whether its party has a VAT number or not: an empty number means nothing (Muteb, 7 Nov 2026).
/// </summary>
public static class VatStatementFiles
{
    private static readonly string[] Ordinals = ["الأول", "الثاني", "الثالث", "الرابع"];

    private static readonly string[] Titles = ["التاريخ", "اسم الشركة", "البيان", "الرقم الضريبي", "إجمالي قيمة الفاتورة", "الضريبة", "أصل المبلغ قبل الضريبة"];

    /// <summary>The workbook of <paramref name="quarters"/>, at <paramref name="ratePercent"/>: 15 for 15%.</summary>
    public static byte[] Excel(List<VatQuarterView> quarters, decimal ratePercent)
    {
        using var workbook = new XLWorkbook();
        var names = new HashSet<string>();
        foreach (var quarter in quarters)
        {
            var ordinal = Ordinals[(quarter.Start.Month - 1) / 3];
            var sheet = workbook.Worksheets.Add(ReportFormats.SheetName($"ضريبة الربع {ordinal} {quarter.Start.Year}", names));
            FillQuarter(sheet, quarter, ordinal, ratePercent.ToString(CultureInfo.InvariantCulture));
        }
        return StatementStyle.Bytes(workbook);
    }

    private static void FillQuarter(IXLWorksheet sheet, VatQuarterView quarter, string ordinal, string rate)
    {
        sheet.RightToLeft = true;
        sheet.Cell(1, 1).Value = $"ضريبة المشتريات عن الربع {ordinal} سنة {quarter.Start.Year}";
        sheet.Range(1, 1, 1, Titles.Length).Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        for (var column = 0; column < Titles.Length; column++)
        {
            sheet.Cell(2, column + 1).Value = Titles[column];
        }
        sheet.Row(2).Style.Font.Bold = true;
        var purchasesTotal = Section(sheet, 3, quarter.Purchases, rate, "الإجمالي");
        var foot = purchasesTotal + 2;
        sheet.Cell(foot, 1).Value = "إجمالي ضريبة المشتريات";
        sheet.Cell(foot, 6).FormulaA1 = $"F{purchasesTotal}";
        sheet.Cell(foot, 6).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        sheet.Range(foot, 1, foot, Titles.Length).Style.Font.Bold = true;
        sheet.Columns().AdjustToContents();
    }

    /// <summary>Writes <paramref name="rows"/> from <paramref name="first"/>, then their totals; answers the totals' row.</summary>
    private static int Section(IXLWorksheet sheet, int first, List<VatRow> rows, string rate, string label)
    {
        var row = first;
        foreach (var line in rows)
        {
            sheet.Cell(row, 1).Value = line.Date.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(row, 1).Style.DateFormat.Format = ReportFormats.ExcelVatDate;
            sheet.Cell(row, 2).Value = line.Party;
            sheet.Cell(row, 3).Value = line.Description;
            sheet.Cell(row, 4).Value = line.VatNumber;
            sheet.Cell(row, 5).Value = line.Total;
            sheet.Cell(row, 6).FormulaA1 = $"E{row}*{rate}/(100+{rate})";
            sheet.Cell(row, 7).FormulaA1 = $"E{row}-F{row}";
            sheet.Range(row, 5, row, 7).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            row++;
        }
        sheet.Cell(row, 1).Value = label;
        foreach (var column in new[] { "E", "F", "G" })
        {
            var cell = sheet.Cell($"{column}{row}");
            cell.FormulaA1 = row > first ? $"SUM({column}{first}:{column}{row - 1})" : "0";
            cell.Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        }
        sheet.Range(row, 1, row, Titles.Length).Style.Font.Bold = true;
        return row;
    }
}
