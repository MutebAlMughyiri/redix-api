using ClosedXML.Excel;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>A sheet of a statement workbook other than the summary (FR-46): its tables one under another, and the label of each table's percentage, empty when it calculates none.</summary>
/// <param name="Name">The sheet's name.</param>
/// <param name="Tables">Its tables, at least one.</param>
/// <param name="PercentLabel">The line under each table giving its share of اجمالي الكل; empty for none (AC-46.9).</param>
public record StatementSheet(string Name, List<StatementTable> Tables, string PercentLabel);

/// <summary>One table of a statement sheet: its title, its columns, its amount, its rows, and what was agreed for it.</summary>
/// <param name="Title">The title row's only text.</param>
/// <param name="Columns">Its columns before the amount, the first being the one that stretches when the sheet is wider than the table.</param>
/// <param name="Amount">Its amount column, always the sheet's last.</param>
/// <param name="Rows">Its rows, in order.</param>
/// <param name="AgreedAmount">What a contract agreed, which puts المتبقي and مبلغ الاتفاق under it (AC-46.2); zero when nothing was agreed, which shows neither.</param>
public record StatementTable(string Title, List<StatementColumn> Columns, StatementColumn Amount, List<StatementRow> Rows, decimal AgreedAmount);

/// <summary>A column of a table.</summary>
/// <param name="Header">Its header.</param>
/// <param name="Kind">How its cells are written.</param>
public record StatementColumn(string Header, CellKind Kind)
{
    /// <summary>The choices its cells take, with their signs, when it is a list (redix-api #129); empty for any other column.</summary>
    public List<ReportChoiceView> Choices { get; init; } = [];

    /// <summary>Whether a cell may be left empty: true unless entry requires the column, so an optional supplier may be none (Muteb, 13 Nov 2026).</summary>
    public bool AllowsBlank { get; init; } = true;
}

/// <summary>A row of a table.</summary>
/// <param name="Cells">Its cells, one for each column before the amount.</param>
/// <param name="Amount">Its amount.</param>
/// <param name="IsNew">True when it was approved after the previous report, shaded green (BR-26).</param>
/// <param name="CountsAgainstAgreement">False for a payment outside its contract, which leaves المتبقي as it was (BR-04).</param>
public record StatementRow(List<XLCellValue> Cells, decimal Amount, bool IsNew, bool CountsAgainstAgreement);

/// <summary>How a cell is written.</summary>
public enum CellKind : byte
{
    /// <summary>As it is.</summary>
    Text,

    /// <summary>Centred.</summary>
    Centred,

    /// <summary>A day, as the statement writes one.</summary>
    Date,

    /// <summary>An amount, centred: the price of one.</summary>
    Price,
}
