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

    /// <summary>A table this many columns wide, its number and its amount counted, prints across the page (redix-api #129).</summary>
    private const int LandscapeWidth = 8;

    /// <summary>What a sheet says to go back to the summary, as the approved workbook writes it.</summary>
    private const string BackToSummary = "← الخلاصة";

    /// <summary>
    /// Adds <paramref name="content"/> to the workbook of <paramref name="book"/> as a sheet, its name unique among the book's, and answers its name and the address of its last total
    /// as another sheet's formula names it: اجمالي الكل, or its one table's الاجمالي. Each list column's cells read their choices from the book's «القوائم», and the sheet prints its
    /// tables alone: across the page when they are eight columns wide or more, «← الخلاصة» two columns after them and outside what prints (redix-api #129).
    /// </summary>
    public static PlacedSheet Add(XLWorkbook workbook, StatementSheet content, StatementBook book)
    {
        var widest = content.Tables.MaxBy(table => table.Columns.Count)!;
        var sheet = StatementStyle.AddSheet(workbook, content.Name, book.Taken, NumberWidth);
        var last = widest.Columns.Count + 2;
        var hasWhole = content.Tables.Count > 1 || !content.ShowsEveryTable;
        var totals = new List<string>();
        var percents = new List<IXLCell>();
        var row = 1;
        foreach (var table in content.Tables)
        {
            var (total, first) = Table(sheet, ref row, last, table, book.Lists);
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
        var lastTotal = totals[0];
        if (hasWhole)
        {
            var whole = Line(sheet, row, last, "اجمالي الكل");
            lastTotal = (content.ShowsEveryTable ? whole.SetMoneyFormula(string.Join("+", totals)) : whole.SetMoney(content.Whole)).Address.ToStringRelative();
            for (var index = 0; index < percents.Count; index++)
            {
                percents[index].SetPercentFormula($"IF({lastTotal}=0,0,{totals[index]}/{lastTotal})");
            }
            if (percents.Count > 0)
            {
                Line(sheet, ++row, last, "نسبة الكل").SetPercentFormula(string.Join("+", percents.Select(cell => cell.Address.ToStringRelative())));
            }
        }
        else
        {
            row -= 2;
        }
        if (book.SummaryName.Length > 0)
        {
            sheet.Cell(1, last + 2).LinkTo(book.SummaryName).Value = BackToSummary;
        }
        StatementStyle.Fit(sheet);
        StatementStyle.Print(sheet, book.Footer);
        sheet.PageSetup.PageOrientation = last >= LandscapeWidth ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
        sheet.PageSetup.PrintAreas.Add(1, 1, row, last);
        return new PlacedSheet(sheet.Name, StatementStyle.Reference(sheet.Name, lastTotal));
    }

    /// <summary>
    /// Writes <paramref name="table"/>'s title, header, rows and الاجمالي from <paramref name="row"/>, leaving it on the row after; answers the address of الاجمالي and the row of the first
    /// line. A list column's cells take a validation reading its block of <paramref name="lists"/>, an empty cell allowed where entry allows one (redix-api #129).
    /// </summary>
    private static (string Total, int First) Table(IXLWorksheet sheet, ref int row, int last, StatementTable table, ChoiceLists lists)
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
        for (var column = 1; column < table.Columns.Count && row > first; column++)
        {
            var listed = table.Columns[column];
            if (listed.Choices.Count > 0)
            {
                var place = Span(sheet, first, column, stretch).Address.ColumnNumber;
                var validation = sheet.Range(first, place, row - 1, place).CreateDataValidation();
                validation.List(lists.RangeOf(listed.Header, listed.Choices), true);
                validation.IgnoreBlanks = listed.AllowsBlank;
            }
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

/// <summary>A sheet as its workbook placed it: its name, which a link opens, and the address of its last total, which the summary's line reads.</summary>
/// <param name="Name">The sheet's name in its workbook.</param>
/// <param name="Total">Its last total, as another sheet's formula names it.</param>
public record PlacedSheet(string Name, string Total)
{
    /// <summary>No sheet: what an item without one reads, its total nothing, so no reader holds a null.</summary>
    public static readonly PlacedSheet None = new("", "0");
}
