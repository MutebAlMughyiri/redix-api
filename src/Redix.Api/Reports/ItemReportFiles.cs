using ClosedXML.Excel;
using QuestPDF.Fluent;
using Redix.Api.Contracts;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>An item's report and the contracts report as files (FR-33), and the item page and sheet the periodic report shares (FR-45, FR-46).</summary>
public static class ItemReportFiles
{
    private static readonly string[] Titles = ["", "الوصف", "رقم الفاتورة", "التاريخ", "البند", "سعر الحبة", "العدد", "التكلفة"];

    /// <summary>One item's report as a PDF.</summary>
    public static byte[] Pdf(ItemReportView item)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document => PeriodicReportFiles.Page(document, page => ItemPage(page, item))).GeneratePdf();
    }

    private static readonly StatementColumn Cost = new("التكلفة", CellKind.Text);

    private static readonly StatementColumn Payment = new("الدفعة", CellKind.Text);

    private static readonly List<StatementColumn> PaymentColumns = [new("الوصف", CellKind.Text), new("التاريخ", CellKind.Date)];

    private static readonly StatementColumn Received = new("المبلغ", CellKind.Text);

    private static readonly List<StatementColumn> RevenueColumns = [new("الوصف", CellKind.Text), new("رقم الفاتورة", CellKind.Centred), new("التاريخ", CellKind.Date)];

    /// <summary>One item's report as a workbook, its sheet as the periodic report's.</summary>
    public static byte[] Excel(ItemReportView item)
    {
        using var workbook = StatementStyle.NewWorkbook();
        StatementWriter.Add(workbook, SheetOf(item), []);
        return StatementStyle.Bytes(workbook);
    }

    /// <summary>
    /// An item's sheet (FR-46): a table of purchases per subgroup that has any, titled with its name or, for the general one, the item's, the item's own report columns before the cost (AC-46.8);
    /// and a table of payments per contract, its agreed amount under it (AC-46.2); all in the order the item's tables were put in. A revenue item's tables list what came in: the description and buyer, the invoice, the day and the
    /// amount. The item's choice says whether each table's percentage is calculated (AC-46.9).
    /// </summary>
    public static StatementSheet SheetOf(ItemReportView item)
    {
        if (item.Type.IsRevenue())
        {
            List<StatementTable> received = [.. item.Subgroups.Select(subgroup => new StatementTable(subgroup.Name.Length > 0 ? subgroup.Name : item.ItemName, RevenueColumns, Received, subgroup.Rows.Select(RevenueRow).ToList(), 0))];
            return new StatementSheet(item.ItemName, received.Count > 0 ? received : [new StatementTable(item.ItemName, RevenueColumns, Received, [], 0)], item.ShowsTablePercentages ? "النسبة" : "");
        }
        List<StatementColumn> purchaseColumns =
        [
            new("الوصف", CellKind.Text),
            new("رقم الفاتورة", CellKind.Centred),
            new("التاريخ", CellKind.Date),
            new("سعر الحبة", CellKind.Price),
            new("العدد", CellKind.Centred),
            .. item.Columns.Select(name => new StatementColumn(name, CellKind.Centred)),
        ];
        List<StatementTable> tables =
        [
            .. item.Subgroups.Select(subgroup => (subgroup.SortOrder, Table: new StatementTable(subgroup.Name.Length > 0 ? subgroup.Name : item.ItemName, purchaseColumns, Cost, subgroup.Rows.Select(PurchaseRow).ToList(), 0)))
                .Concat(item.Contracts.Select(contract => (contract.SortOrder, Table: new StatementTable($"دفعات {contract.ContractorName}", PaymentColumns, Payment, contract.Payments.Select(PaymentRow).ToList(), contract.IsOpenValue ? 0 : contract.Value))))
                .OrderBy(table => table.SortOrder)
                .Select(table => table.Table),
        ];
        return new StatementSheet(item.ItemName, tables.Count > 0 ? tables : [new StatementTable(item.ItemName, purchaseColumns, Cost, [], 0)], item.ShowsTablePercentages ? "النسبة" : "");
    }

    private static StatementRow PurchaseRow(ItemEntryRow line) => new(
        [
            string.Join(" · ", new[] { line.Description, line.SupplierName }.Where(text => text.Length > 0)),
            line.InvoiceNumber,
            line.Date.ToDateTime(TimeOnly.MinValue),
            line.Quantity == 0 ? Blank.Value : line.UnitPrice,
            line.Quantity == 0 ? Blank.Value : line.Quantity,
            .. line.Values.Select(value => (XLCellValue)value),
        ],
        line.Amount, "", line.IsNew, true);

    private static StatementRow RevenueRow(ItemEntryRow line) => new(
        [string.Join(" · ", new[] { line.Description, line.SupplierName }.Where(text => text.Length > 0)), line.InvoiceNumber, line.Date.ToDateTime(TimeOnly.MinValue)],
        line.Amount, "", line.IsNew, true);

    private static StatementRow PaymentRow(ItemEntryRow payment) => new(
        [
            string.Join(" · ", new[] { payment.Description, payment.IsOutsideContract ? "خارج العقد" : "" }.Concat(payment.Values).Where(text => text.Length > 0)),
            payment.Date.ToDateTime(TimeOnly.MinValue),
        ],
        payment.Amount, "", payment.IsNew, !payment.IsOutsideContract);

    /// <summary>An item's page: a table per subgroup in the statement's columns, then each contract with its value in the head, its payments, their total, what remains and the total of all; new rows green (FR-45).</summary>
    public static void ItemPage(ColumnDescriptor page, ItemReportView item)
    {
        page.Spacing(6);
        page.Item().Text($"{item.CategoryName} · {item.ItemName}").FontSize(13).Bold();
        foreach (var subgroup in item.Subgroups)
        {
            page.Item().Text(subgroup.Name.Length > 0 ? subgroup.Name : "عام").Bold();
            Table(page, item, subgroup.Rows, subgroup.Total);
        }
        foreach (var contract in item.Contracts)
        {
            page.Item().Text($"{contract.ContractorName} · قيمة العقد {(contract.IsOpenValue ? "مفتوحة" : ReportFormats.Money(contract.Value))}").Bold();
            Table(page, item, contract.Payments, contract.PaymentsTotal);
            if (!contract.IsOpenValue)
            {
                PeriodicReportFiles.Line(page, "المتبقي", contract.Remaining, true);
            }
        }
        PeriodicReportFiles.Line(page, "اجمالي الكل", item.Total, true);
    }

    private static void Table(ColumnDescriptor page, ItemReportView item, List<ItemEntryRow> rows, decimal total) => page.Item().Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(22);
            columns.RelativeColumn(4);
            columns.RelativeColumn(2);
            columns.RelativeColumn(2);
            columns.RelativeColumn(2);
            columns.RelativeColumn(2);
            columns.RelativeColumn(1);
            columns.RelativeColumn(2);
        });
        PeriodicReportFiles.Header(table, Titles);
        var number = 1;
        foreach (var row in rows)
        {
            foreach (var text in Cells(number++, row, item.ItemName))
            {
                PeriodicReportFiles.Cell(table, text, isNew: row.IsNew);
            }
        }
        for (var blank = 0; blank < Titles.Length - 1; blank++)
        {
            PeriodicReportFiles.Cell(table, blank == 1 ? "المجموع" : "", true);
        }
        PeriodicReportFiles.Cell(table, ReportFormats.Money(total), true);
    });

    private static string[] Cells(int number, ItemEntryRow row, string itemName) =>
    [
        $"{number}",
        string.Join(" · ", new[] { row.Description, row.SupplierName }.Concat(row.Values).Where(text => text.Length > 0)),
        row.InvoiceNumber,
        ReportFormats.Date(row.Date),
        itemName,
        row.UnitPrice == 0 ? "" : ReportFormats.Money(row.UnitPrice),
        row.Quantity == 0 ? "" : $"{row.Quantity:0.##}",
        ReportFormats.Money(row.Amount),
    ];

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

    /// <summary>The contracts report as a workbook: one statement table (UC-067, findings row 131) of each contract's contractor, item, value and what was paid, its amount what is
    /// left of it, an overrun negative and an open value nothing.</summary>
    public static byte[] ContractsExcel(List<ContractView> contracts)
    {
        using var workbook = new XLWorkbook();
        var table = new StatementTable(
            "العقود",
            [new StatementColumn("المقاول", CellKind.Text), new StatementColumn("البند", CellKind.Text), new StatementColumn("قيمة العقد", CellKind.Price), new StatementColumn("المدفوع", CellKind.Price)],
            new StatementColumn("المتبقي", CellKind.Price),
            contracts.Select(contract => new StatementRow([contract.SupplierName, contract.ItemName, contract.IsOpenValue ? "مفتوحة" : contract.Value, contract.Paid], contract.IsOpenValue ? 0 : contract.Remaining, "", false, true)).ToList(),
            0);
        StatementWriter.Add(workbook, new StatementSheet("العقود", [table], ""), []);
        return StatementStyle.Bytes(workbook);
    }
}
