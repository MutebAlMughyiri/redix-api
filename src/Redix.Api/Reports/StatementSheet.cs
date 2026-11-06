using ClosedXML.Excel;

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
/// <param name="Agreement">What was agreed for it, which puts المتبقي and مبلغ الاتفاق under it (AC-46.2).</param>
public record StatementTable(string Title, List<StatementColumn> Columns, StatementColumn Amount, List<StatementRow> Rows, Agreement Agreement);

/// <summary>A column of a table.</summary>
/// <param name="Header">Its header.</param>
/// <param name="Width">How wide it is, in characters, when its table is the sheet's widest.</param>
/// <param name="Kind">How its cells are written.</param>
public record StatementColumn(string Header, double Width, CellKind Kind);

/// <summary>A row of a table.</summary>
/// <param name="Cells">Its cells, one for each column before the amount.</param>
/// <param name="Amount">Its amount.</param>
/// <param name="Link">Where its <see cref="CellKind.Linked"/> cell leads; empty for nowhere.</param>
/// <param name="IsNew">True when it was approved after the previous report, shaded green (BR-26).</param>
/// <param name="CountsAgainstAgreement">False for a payment outside its contract, which leaves المتبقي as it was (BR-04).</param>
public record StatementRow(List<XLCellValue> Cells, decimal Amount, string Link, bool IsNew, bool CountsAgainstAgreement);

/// <summary>How a cell is written.</summary>
public enum CellKind : byte
{
    /// <summary>As it is.</summary>
    Text,

    /// <summary>As it is, leading to the row's link.</summary>
    Linked,

    /// <summary>Centred.</summary>
    Centred,

    /// <summary>A day, as the statement writes one.</summary>
    Date,

    /// <summary>An amount, centred: the price of one.</summary>
    Price,
}

/// <summary>What was agreed for a table: a contract's value, which puts المتبقي and مبلغ الاتفاق under it, or an open value, which puts مبلغ الاتفاق alone.</summary>
/// <param name="Kind">Whether anything was agreed.</param>
/// <param name="Amount">How much, when fixed.</param>
public record Agreement(AgreementKind Kind, decimal Amount)
{
    /// <summary>Nothing agreed: every table but a contract's.</summary>
    public static readonly Agreement None = new(AgreementKind.None, 0);

    /// <summary>A contract with no fixed value.</summary>
    public static readonly Agreement Open = new(AgreementKind.Open, 0);

    /// <summary>A contract worth <paramref name="amount"/>.</summary>
    public static Agreement Of(decimal amount) => new(AgreementKind.Fixed, amount);
}

/// <summary>Whether anything was agreed for a table.</summary>
public enum AgreementKind : byte
{
    /// <summary>Nothing.</summary>
    None,

    /// <summary>A contract with no fixed value.</summary>
    Open,

    /// <summary>A contract with a value.</summary>
    Fixed,
}
