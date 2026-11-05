using ClosedXML.Excel;

namespace Redix.Api.Reports;

/// <summary>
/// How a statement workbook looks, as the project's own statement does: Arial 10, right to left, A4 one page wide so it exports to PDF as the statement does, grey header rows and totals, hair lines,
/// and green for cash and for what is new. The one owner, so the periodic report and an item's report look alike.
/// </summary>
public static class StatementStyle
{
    /// <summary>The grey of header rows and totals.</summary>
    public static readonly XLColor Grey = XLColor.FromHtml("#D9D9D9");

    /// <summary>The green of cash held by a person, of the accounts' total, and of a row new since the last report (BR-26).</summary>
    public static readonly XLColor Green = XLColor.FromHtml("#B6D7A8");

    /// <summary>The grey text of a line kept for reference: the sales and what leaves them out.</summary>
    public static readonly XLColor Muted = XLColor.FromHtml("#B7B7B7");

    private const string Font = "Arial";

    private const double FontSize = 10;

    /// <summary>An empty workbook in the statement's font.</summary>
    public static XLWorkbook NewWorkbook()
    {
        var workbook = new XLWorkbook();
        workbook.Style.Font.SetFontName(Font).Font.SetFontSize(FontSize).Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        return workbook;
    }

    /// <summary>Adds a sheet named <paramref name="name"/>, unique among <paramref name="taken"/>: right to left, A4 portrait one page wide, its columns <paramref name="widths"/> characters wide.</summary>
    public static IXLWorksheet AddSheet(XLWorkbook workbook, string name, HashSet<string> taken, params double[] widths)
    {
        var sheet = workbook.Worksheets.Add(ReportFormats.SheetName(name, taken));
        sheet.Style.Font.SetFontName(Font).Font.SetFontSize(FontSize).Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        sheet.RightToLeft = true;
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        sheet.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        sheet.PageSetup.FitToPages(1, 0);
        for (var column = 0; column < widths.Length; column++)
        {
            sheet.Column(column + 1).Width = widths[column];
        }
        return sheet;
    }

    /// <summary>The address of <paramref name="cell"/> on <paramref name="sheet"/>, as another sheet's formula names it.</summary>
    public static string Reference(IXLWorksheet sheet, string cell) => $"'{sheet.Name.Replace("'", "''")}'!{cell}";

    /// <summary>The workbook as a file.</summary>
    public static byte[] Bytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>A header row: grey, a hair line under it, centred.</summary>
    public static IXLRange AsHeader(this IXLRange range)
    {
        range.Style.Fill.BackgroundColor = Grey;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        return range;
    }

    /// <summary>Lines of a table: hair lines all round.</summary>
    public static IXLRange AsBody(this IXLRange range)
    {
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        return range;
    }

    /// <summary>A total: bold on grey.</summary>
    public static IXLRange AsTotal(this IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = Grey;
        return range;
    }

    /// <summary>A total kept for reference: bold grey text on grey.</summary>
    public static IXLRange AsMutedTotal(this IXLRange range)
    {
        range.AsTotal().Style.Font.FontColor = Muted;
        return range;
    }

    /// <summary>Marks a row new since the last report: green (BR-26).</summary>
    public static IXLRange AsNew(this IXLRange range)
    {
        range.Style.Fill.BackgroundColor = Green;
        return range;
    }

    /// <summary>Writes <paramref name="amount"/> as an amount: separators, two decimals, a negative in brackets, to the left.</summary>
    public static IXLCell SetMoney(this IXLCell cell, decimal amount)
    {
        cell.Value = amount;
        return cell.AsMoney();
    }

    /// <summary>Writes <paramref name="formula"/> shown as an amount.</summary>
    public static IXLCell SetMoneyFormula(this IXLCell cell, string formula)
    {
        cell.FormulaA1 = formula;
        return cell.AsMoney();
    }

    /// <summary>Writes <paramref name="date"/> as the statement does, centred.</summary>
    public static IXLCell SetDate(this IXLCell cell, DateOnly date)
    {
        cell.Value = date.ToDateTime(TimeOnly.MinValue);
        cell.Style.DateFormat.Format = ReportFormats.ExcelDate;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        return cell;
    }

    /// <summary>Writes <paramref name="value"/>, centred.</summary>
    public static IXLCell SetCentred(this IXLCell cell, XLCellValue value)
    {
        cell.Value = value;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        return cell;
    }

    private static IXLCell AsMoney(this IXLCell cell)
    {
        cell.Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        return cell;
    }
}
