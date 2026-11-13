namespace Redix.Api.Reports;

/// <summary>
/// What every sheet of one statement workbook shares (FR-46, redix-api #129): the names already taken, the hidden «القوائم» its list cells read, the footer every sheet prints,
/// and الخلاصة, which each sheet links back to when the workbook has one.
/// </summary>
public sealed class StatementBook
{
    /// <summary>A workbook whose sheets print <paramref name="footer"/> at their foot, right; and link back to <paramref name="summaryName"/>, empty for a workbook without a summary.</summary>
    public StatementBook(string footer, string summaryName = "")
    {
        Footer = footer;
        SummaryName = summaryName;
        Lists = new ChoiceLists(Taken);
    }

    /// <summary>The sheet names taken so far.</summary>
    public HashSet<string> Taken { get; } = [];

    /// <summary>The lists its tables' cells choose from, written last.</summary>
    public ChoiceLists Lists { get; }

    /// <summary>What every sheet prints at its foot, right: the project, and the report's number when it has one.</summary>
    public string Footer { get; }

    /// <summary>The summary's sheet, which every other sheet links back to; empty when the workbook has none.</summary>
    public string SummaryName { get; }
}
