using System.Globalization;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using Redix.Api.Contracts;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>An item's report and the contracts report as files (FR-33), and the item sheet the periodic report shares, drawn the same in its workbook and its PDF (FR-45, FR-46, findings row 130).</summary>
public static class ItemReportFiles
{
    /// <summary>One item's report as a PDF.</summary>
    public static byte[] Pdf(ItemReportView item)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document => PeriodicReportFiles.Page(document, page => StatementPdf.Draw(page, SheetOf(item)))).GeneratePdf();
    }

    private static readonly StatementColumn Description = new("الوصف", CellKind.Text);

    /// <summary>One item's report as a workbook of the project <paramref name="projectName"/>, its sheet as the periodic report's, its lists on «القوائم» (redix-api #129).</summary>
    public static byte[] Excel(string projectName, ItemReportView item)
    {
        using var workbook = StatementStyle.NewWorkbook();
        var book = new StatementBook(projectName);
        StatementWriter.Add(workbook, SheetOf(item), book);
        book.Lists.AddTo(workbook, book.Footer);
        return StatementStyle.Bytes(workbook);
    }

    /// <summary>
    /// An item's sheet (FR-46): a table per subgroup that has entries, titled with its name or, for the general one, the item's; a table of payments per contract, its agreed amount
    /// under it (AC-46.2); and for رأس المال a table per partner; all in the order the item's tables were put in (AC-46.8). Every table prints its own columns (redix-api #129, R1):
    /// «الوصف» first, then its item's columns that print, in their order, each headed by its own name, and its amount column last; nothing else is fixed. The item's choice says
    /// whether each table's percentage is calculated (AC-46.9).
    /// </summary>
    public static StatementSheet SheetOf(ItemReportView item)
    {
        List<StatementTable> tables =
        [
            .. item.Subgroups.Select(subgroup => (subgroup.SortOrder, Table: TableOf(subgroup.Name.Length > 0 ? subgroup.Name : item.ItemName, subgroup.Columns, subgroup.AmountName, subgroup.Rows, 0)))
                .Concat(item.Contracts.Select(contract => (contract.SortOrder, Table: TableOf($"دفعات {contract.ContractorName}", contract.Columns, contract.AmountName, contract.Payments, contract.IsOpenValue ? 0 : contract.Value))))
                .OrderBy(table => table.SortOrder)
                .Select(table => table.Table),
        ];
        return new StatementSheet(item.ItemName, tables.Count > 0 ? tables : [TableOf(item.ItemName, [], "", [], 0)], item.ShowsTablePercentages ? "النسبة" : "");
    }

    private static StatementTable TableOf(string title, List<ReportColumnView> columns, string amountName, List<ItemEntryRow> rows, decimal agreedAmount) => new(
        title,
        [Description, .. columns.Select(ColumnOf)],
        new StatementColumn(amountName, CellKind.Text),
        [.. rows.Select(row => new StatementRow([row.Description, .. columns.Zip(row.Values, CellOf)], row.Amount, row.IsNew, !row.IsOutsideContract))],
        agreedAmount);

    /// <summary>A column of an item as its table prints it: a date as a day, an amount as money, anything else centred; a list with the choices its cells take (redix-api #129).</summary>
    private static StatementColumn ColumnOf(ReportColumnView column) => new(column.Name, column.DataType switch
    {
        ColumnDataType.Date => CellKind.Date,
        ColumnDataType.Amount => CellKind.Price,
        _ => CellKind.Centred,
    })
    {
        Choices = column.Choices,
        AllowsBlank = !column.IsRequired,
    };

    /// <summary>A value as its cell holds it: a day, a number, or the text kept; nothing when none was given.</summary>
    private static XLCellValue CellOf(ReportColumnView column, string value)
    {
        if (value.Length == 0)
        {
            return Blank.Value;
        }
        if (column.DataType == ColumnDataType.Date && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date.ToDateTime(TimeOnly.MinValue);
        }
        return column.DataType is ColumnDataType.Number or ColumnDataType.Amount && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) ? number : value;
    }

    /// <summary>The contracts report as a PDF: each contract's value, paid, remaining, and an alert where more was paid (FR-31, FR-33).</summary>
    public static byte[] ContractsPdf(string projectName, List<ContractView> contracts)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document => PeriodicReportFiles.Page(document, page =>
        {
            page.Spacing(6);
            page.Item().Text($"{projectName} · العقود والمتبقي").FontSize(13).Bold();
            page.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });
                PeriodicReportFiles.Header(table, "المقاول", "البند", "قيمة العقد", "المدفوع", "المتبقي");
                foreach (var contract in contracts)
                {
                    PeriodicReportFiles.Cell(table, contract.IsOverrun ? $"{contract.SupplierName} · متجاوز" : contract.SupplierName, contract.IsOverrun);
                    PeriodicReportFiles.Cell(table, contract.ItemName);
                    PeriodicReportFiles.Cell(table, contract.IsOpenValue ? "مفتوحة" : ReportFormats.Money(contract.Value));
                    PeriodicReportFiles.Cell(table, ReportFormats.Money(contract.Paid));
                    PeriodicReportFiles.Cell(table, contract.IsOpenValue ? "" : ReportFormats.Money(contract.Remaining), contract.IsOverrun);
                }
            });
        })).GeneratePdf();
    }

    /// <summary>The contracts report as a workbook of the project <paramref name="projectName"/>: one statement table (UC-067, findings row 131) of each contract's contractor, item, value
    /// and what was paid, its amount what is left of it, an overrun negative and an open value nothing.</summary>
    public static byte[] ContractsExcel(string projectName, List<ContractView> contracts)
    {
        using var workbook = new XLWorkbook();
        var table = new StatementTable(
            "العقود",
            [new StatementColumn("المقاول", CellKind.Text), new StatementColumn("البند", CellKind.Text), new StatementColumn("قيمة العقد", CellKind.Price), new StatementColumn("المدفوع", CellKind.Price)],
            new StatementColumn("المتبقي", CellKind.Price),
            contracts.Select(contract => new StatementRow([contract.SupplierName, contract.ItemName, contract.IsOpenValue ? "مفتوحة" : contract.Value, contract.Paid], contract.IsOpenValue ? 0 : contract.Remaining, false, true)).ToList(),
            0);
        StatementWriter.Add(workbook, new StatementSheet("العقود", [table], ""), new StatementBook(projectName));
        return StatementStyle.Bytes(workbook);
    }
}
