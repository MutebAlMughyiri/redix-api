using ClosedXML.Excel;

namespace Redix.Api.Reports;

/// <summary>
/// The one layout of a statement sheet other than the summary (FR-46). Tables go one under another with a blank row between. Each table has a title row, a header and its numbered rows,
/// then its lines, each on a row of its own: الاجمالي; المتبقي and مبلغ الاتفاق when an agreed amount was filled in; its percentage when the sheet calculates one. With more than one table come
/// اجمالي الكل and, with percentages, نسبة الكل. Every amount sits in the sheet's last column. A builder says what a table holds; only this places a cell.
/// </summary>
public static class StatementWriter
{
    private const double NumberWidth = 4.25;

    /// <summary>
    /// Adds <paramref name="content"/> to <paramref name="workbook"/> as a sheet, its name unique among <paramref name="taken"/>, and answers the address of its last total as another sheet's
    /// formula names it: اجمالي الكل, or its one table's الاجمالي.
    /// </summary>
    public static string Add(XLWorkbook workbook, StatementSheet content, HashSet<string> taken)
    {
        var widest = content.Tables.MaxBy(table => table.Columns.Count)!;
        var sheet = StatementStyle.AddSheet(workbook, content.Name, taken, NumberWidth);
        var last = widest.Columns.Count + 2;
        var hasWhole = content.Tables.Count > 1;
        var totals = new List<string>();
        var percents = new List<IXLCell>();
        var row = 1;
        foreach (var table in content.Tables)
        {
            var (total, first) = Table(sheet, ref row, last, table);
            if (table.AgreedAmount != 0)
            {
                var agreed = Line(sheet, row + 1, last, "مبلغ الاتفاق").SetMoney(table.AgreedAmount).Address.ToStringRelative();
                Line(sheet, row, last, "المتبقي").SetMoneyFormula($"{agreed}-{InsideOf(table, total, first, last)}");
                row += 2;
            }
            if (hasWhole && content.PercentLabel.Length > 0)
            {
                percents.Add(Line(sheet, row++, last, content.PercentLabel));
            }
            totals.Add(total);
            row++;
        }
        if (!hasWhole)
        {
            StatementStyle.Fit(sheet);
            return StatementStyle.Reference(sheet, totals[0]);
        }
        var whole = Line(sheet, row, last, "اجمالي الكل").SetMoneyFormula(string.Join("+", totals)).Address.ToStringRelative();
        for (var index = 0; index < percents.Count; index++)
        {
            percents[index].SetPercentFormula($"IF({whole}=0,0,{totals[index]}/{whole})");
        }
        if (percents.Count > 0)
        {
            Line(sheet, row + 1, last, "نسبة الكل").SetPercentFormula(string.Join("+", percents.Select(cell => cell.Address.ToStringRelative())));
        }
        StatementStyle.Fit(sheet);
        return StatementStyle.Reference(sheet, whole);
    }

    /// <summary>Writes <paramref name="table"/>'s title, header, rows and الاجمالي from <paramref name="row"/>, leaving it on the row after; answers the address of الاجمالي and the row of the first line.</summary>
    private static (string Total, int First) Table(IXLWorksheet sheet, ref int row, int last, StatementTable table)
    {
        sheet.Range(row, 1, row, last).Merge().AsTitle().FirstCell().Value = table.Title;
        row++;
        var stretch = last - 2 - table.Columns.Count;
        sheet.Range(row, 1, row, last).AsHeader();
        for (var column = 0; column < table.Columns.Count; column++)
        {
            Span(sheet, row, column, stretch).Value = table.Columns[column].Header;
        }
        sheet.Cell(row, last).Value = table.Amount.Header;
        row++;
        var first = row;
        foreach (var line in table.Rows)
        {
            sheet.Cell(row, 1).SetCentred(row - first + 1);
            for (var column = 0; column < table.Columns.Count; column++)
            {
                Write(Span(sheet, row, column, stretch), table.Columns[column].Kind, line.Cells[column]);
            }
            sheet.Cell(row, last).SetMoney(line.Amount);
            var body = sheet.Range(row, 1, row, last).AsBody();
            if (line.IsNew)
            {
                body.AsNew();
            }
            row++;
        }
        var letter = XLHelper.GetColumnLetterFromNumber(last);
        var total = Line(sheet, row, last, "الاجمالي").SetMoneyFormula(row > first ? $"SUM({letter}{first}:{letter}{row - 1})" : "0").Address.ToStringRelative();
        row++;
        return (total, first);
    }

    /// <summary>The payments that count against the agreed amount: the total less those outside the contract (BR-04), whose rows start at <paramref name="first"/>.</summary>
    private static string InsideOf(StatementTable table, string total, int first, int last)
    {
        var outside = table.Rows.Select((line, index) => (line, index)).Where(pair => !pair.line.CountsAgainstAgreement).Select(pair => $"{XLHelper.GetColumnLetterFromNumber(last)}{first + pair.index}").ToList();
        return outside.Count > 0 ? $"({total}-({string.Join("+", outside)}))" : total;
    }

    /// <summary>The cell of <paramref name="column"/> on <paramref name="row"/>, after the number; the first column takes <paramref name="stretch"/> more, so the amount stays in the last.</summary>
    private static IXLCell Span(IXLWorksheet sheet, int row, int column, int stretch)
    {
        var from = column == 0 ? 2 : column + 2 + stretch;
        var to = column == 0 ? 2 + stretch : from;
        return to > from ? sheet.Range(row, from, row, to).Merge().FirstCell() : sheet.Cell(row, from);
    }

    private static void Write(IXLCell cell, CellKind kind, XLCellValue value)
    {
        if (value.IsBlank)
        {
            return;
        }
        if (kind == CellKind.Date)
        {
            cell.SetDate(DateOnly.FromDateTime(value.GetDateTime()));
            return;
        }
        cell.Value = value;
        if (kind == CellKind.Centred)
        {
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        if (kind == CellKind.Price)
        {
            cell.Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }

    /// <summary>A line under a table: its label across every column but the last, grey and bold; answers the cell its figure goes in.</summary>
    private static IXLCell Line(IXLWorksheet sheet, int row, int last, string label)
    {
        sheet.Range(row, 1, row, last - 1).Merge().FirstCell().Value = label;
        sheet.Range(row, 1, row, last).AsTotal();
        return sheet.Cell(row, last);
    }
}
