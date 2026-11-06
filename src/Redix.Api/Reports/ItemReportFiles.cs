using ClosedXML.Excel;
using QuestPDF.Fluent;
using Redix.Api.Contracts;

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

    private static readonly StatementColumn Cost = new("التكلفة", 10.88, CellKind.Text);

    private static readonly StatementColumn Payment = new("الدفعة", 10.88, CellKind.Text);

    private static readonly List<StatementColumn> PaymentColumns = [new("الوصف", 39.13, CellKind.Text), new("التاريخ", 11.13, CellKind.Date)];

    /// <summary>One item's report as a workbook, its sheet as the periodic report's.</summary>
    public static byte[] Excel(ItemReportView item)
    {
        using var workbook = StatementStyle.NewWorkbook();
        StatementWriter.Add(workbook, SheetOf(item), []);
        return StatementStyle.Bytes(workbook);
    }

    /// <summary>
    /// An item's sheet (FR-46): a table of purchases per subgroup that has any, titled with its name or, for the general one, the item's, the item's own report columns before the cost (AC-46.8);
    /// then a table of payments per contract, its agreed amount under it (AC-46.2). The item's choice says whether each table's percentage is calculated (AC-46.9).
    /// </summary>
    public static StatementSheet SheetOf(ItemReportView item)
    {
        List<StatementColumn> purchaseColumns =
        [
            new("الوصف", 39.13, CellKind.Text),
            new("رقم الفاتورة", 10.38, CellKind.Centred),
            new("التاريخ", 11.13, CellKind.Date),
            new("سعر الحبة", 8.75, CellKind.Price),
            new("العدد", 5.25, CellKind.Centred),
            .. item.Columns.Select(name => new StatementColumn(name, 12, CellKind.Centred)),
        ];
        List<StatementTable> tables =
        [
            .. item.Subgroups.Select(subgroup => new StatementTable(subgroup.Name.Length > 0 ? subgroup.Name : item.ItemName, purchaseColumns, Cost, subgroup.Rows.Select(PurchaseRow).ToList(), Agreement.None)),
            .. item.Contracts.Select(contract => new StatementTable($"دفعات {contract.ContractorName}", PaymentColumns, Payment, contract.Payments.Select(PaymentRow).ToList(), contract.IsOpenValue ? Agreement.Open : Agreement.Of(contract.Value))),
        ];
        return new StatementSheet(item.ItemName, tables.Count > 0 ? tables : [new StatementTable(item.ItemName, purchaseColumns, Cost, [], Agreement.None)], item.ShowsTablePercentages ? "النسبة" : "");
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

    /// <summary>The contracts report as a workbook, its remaining a live formula, an overrun in red.</summary>
    public static byte[] ContractsExcel(List<ContractView> contracts)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("العقود");
        sheet.RightToLeft = true;
        string[] titles = ["المقاول", "البند", "قيمة العقد", "المدفوع", "المتبقي"];
        for (var column = 0; column < titles.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = titles[column];
            sheet.Cell(1, column + 1).Style.Font.Bold = true;
        }
        var row = 2;
        foreach (var contract in contracts)
        {
            sheet.Cell(row, 1).Value = contract.SupplierName;
            sheet.Cell(row, 2).Value = contract.ItemName;
            sheet.Cell(row, 3).Value = contract.Value;
            sheet.Cell(row, 4).Value = contract.Paid;
            sheet.Cell(row, 5).FormulaA1 = contract.IsOpenValue ? "0" : $"C{row}-D{row}";
            sheet.Range(row, 3, row, 5).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            if (contract.IsOverrun)
            {
                sheet.Range(row, 1, row, 5).Style.Font.FontColor = XLColor.Red;
            }
            row++;
        }
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
