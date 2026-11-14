using System.Globalization;
using ClosedXML.Excel;
using QuestPDF.Fluent;

namespace Redix.Api.Reports;

/// <summary>
/// A statement sheet on a PDF page, laid out as <see cref="StatementWriter"/> lays it in the workbook (findings row 130, Muteb 8 Nov 2026): the same tables, columns, rows and lines, so
/// the two files never show a sheet in two shapes. A PDF has no formulas, so every figure the workbook leaves to a formula is worked out here.
/// </summary>
public static class StatementPdf
{
    /// <summary>How wide the first column is, in points: room for «رقم القيد» and an entry's number of five digits (AC-54.6).</summary>
    private const float NumberWidth = 40;

    /// <summary>
    /// Draws <paramref name="sheet"/> on <paramref name="page"/>: its name, then each table with its title, rows and الاجمالي, المتبقي and مبلغ الاتفاق when an amount was agreed, and its
    /// percentage when the sheet calculates one; with more than one table, اجمالي الكل and نسبة الكل.
    /// </summary>
    /// <param name="page">The page's column.</param>
    /// <param name="sheet">The sheet, as the workbook writes it.</param>
    public static void Draw(ColumnDescriptor page, StatementSheet sheet)
    {
        page.Spacing(6);
        page.Item().Text(sheet.Name).FontSize(13).Bold();
        var totals = sheet.Tables.Select(table => table.Rows.Sum(row => row.Amount)).ToList();
        var whole = sheet.ShowsEveryTable ? totals.Sum() : sheet.Whole;
        var hasWhole = sheet.Tables.Count > 1 || !sheet.ShowsEveryTable;
        var percents = hasWhole && sheet.PercentLabel.Length > 0;
        foreach (var (table, total) in sheet.Tables.Zip(totals))
        {
            Table(page, table);
            PeriodicReportFiles.Line(page, "الاجمالي", total, true);
            if (table.AgreedAmount != 0)
            {
                PeriodicReportFiles.Line(page, "المتبقي", table.AgreedAmount - table.Rows.Where(row => row.CountsAgainstAgreement).Sum(row => row.Amount), true);
                PeriodicReportFiles.Line(page, "مبلغ الاتفاق", table.AgreedAmount, true);
            }
            if (percents)
            {
                PeriodicReportFiles.Line(page, sheet.PercentLabel, Percent(whole == 0 ? 0 : total / whole), true);
            }
        }
        if (hasWhole)
        {
            PeriodicReportFiles.Line(page, "اجمالي الكل", whole, true);
        }
        if (percents)
        {
            PeriodicReportFiles.Line(page, "نسبة الكل", Percent(whole == 0 ? 0 : 1), true);
        }
    }

    /// <summary>A table's title, then its header and its rows, each under its number (AC-54.6), the first column the widest and the amount the last; new rows shaded green (BR-26).</summary>
    private static void Table(ColumnDescriptor page, StatementTable table)
    {
        page.Item().PaddingTop(4).Text(table.Title).Bold();
        page.Item().Table(grid =>
        {
            grid.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(NumberWidth);
                columns.RelativeColumn(4);
                for (var column = 1; column <= table.Columns.Count; column++)
                {
                    columns.RelativeColumn(2);
                }
            });
            PeriodicReportFiles.Header(grid, [table.NumberHeader, .. table.Columns.Select(column => column.Header), table.Amount.Header]);
            foreach (var row in table.Rows)
            {
                PeriodicReportFiles.Cell(grid, Text(CellKind.Centred, row.Number), isNew: row.IsNew);
                for (var column = 0; column < table.Columns.Count; column++)
                {
                    PeriodicReportFiles.Cell(grid, Text(table.Columns[column].Kind, row.Cells[column]), isNew: row.IsNew);
                }
                PeriodicReportFiles.Cell(grid, ReportFormats.Money(row.Amount), isNew: row.IsNew);
            }
        });
    }

    /// <summary>A cell's value as the workbook shows it: a day as the statement writes one, a price as money, anything else as it is.</summary>
    private static string Text(CellKind kind, XLCellValue value)
    {
        if (value.IsBlank)
        {
            return "";
        }
        if (kind == CellKind.Date)
        {
            return ReportFormats.Date(DateOnly.FromDateTime(value.GetDateTime()));
        }
        return kind == CellKind.Price ? ReportFormats.Money((decimal)value.GetNumber()) : value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>A share as the workbook's 0.00% writes it.</summary>
    private static string Percent(decimal share) => (share * 100).ToString("0.00", CultureInfo.InvariantCulture) + "%";
}
