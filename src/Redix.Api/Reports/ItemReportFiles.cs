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

    /// <summary>One item's report as a workbook, its sheet as the periodic report's.</summary>
    public static byte[] Excel(ItemReportView item)
    {
        using var workbook = StatementStyle.NewWorkbook();
        FillSheet(StatementStyle.AddSheet(workbook, item.ItemName, []), item);
        return StatementStyle.Bytes(workbook);
    }

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

    /// <summary>
    /// Fills a sheet as the statement's item sheet: the purchases in one table oldest first, the item's own report columns before the cost, and their total; then a block per contract with its value,
    /// its payments, what remains after each, and their total; then the total of all. New rows are green (BR-26). Answers the address of the total of all, for the summary to point at (FR-46).
    /// </summary>
    public static string FillSheet(IXLWorksheet sheet, ItemReportView item)
    {
        string[] titles = [.. Titles[..^1], .. item.Columns, Titles[^1]];
        double[] widths = [4.25, 39.13, 10.38, 11.13, 11.5, 8.75, 5.25, .. item.Columns.Select(_ => 12.0), 10.88];
        for (var column = 0; column < widths.Length; column++)
        {
            sheet.Column(column + 1).Width = widths[column];
        }
        var cost = titles.Length;
        var costLetter = sheet.Cell(1, cost).Address.ColumnLetter;
        var parts = new List<string>();
        var row = 1;
        var purchases = item.Subgroups.SelectMany(subgroup => subgroup.Rows.Select(line => (Line: line, Item: subgroup.Name.Length > 0 ? $"{item.ItemName} · {subgroup.Name}" : item.ItemName))).OrderBy(purchase => purchase.Line.Date).ToList();
        if (purchases.Count > 0 || item.Contracts.Count == 0)
        {
            var header = sheet.Range(row, 1, row, cost).AsHeader();
            for (var column = 0; column < cost; column++)
            {
                header.Cell(1, column + 1).Value = titles[column];
            }
            row++;
            var number = 1;
            foreach (var (line, itemName) in purchases)
            {
                sheet.Cell(row, 1).SetCentred(number++);
                sheet.Cell(row, 2).Value = string.Join(" · ", new[] { line.Description, line.SupplierName }.Where(text => text.Length > 0));
                sheet.Cell(row, 3).SetCentred(line.InvoiceNumber);
                sheet.Cell(row, 4).SetDate(line.Date);
                sheet.Cell(row, 5).SetCentred(itemName);
                if (line.Quantity != 0)
                {
                    sheet.Cell(row, 6).SetMoney(line.UnitPrice).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(row, 7).Value = line.Quantity;
                    sheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                for (var value = 0; value < line.Values.Count; value++)
                {
                    sheet.Cell(row, 8 + value).SetCentred(line.Values[value]);
                }
                sheet.Cell(row, cost).SetMoney(line.Amount);
                Mark(sheet.Range(row, 1, row, cost).AsBody(), line.IsNew);
                row++;
            }
            var total = sheet.Range(row, 1, row, 2).Merge();
            total.FirstCell().Value = "الاجمالي";
            sheet.Cell(row, cost).SetMoneyFormula(row > 2 ? $"SUM({costLetter}2:{costLetter}{row - 1})" : "0");
            sheet.Range(row, 1, row, cost).AsTotal();
            parts.Add($"{costLetter}{row}");
            row += 3;
        }
        foreach (var contract in item.Contracts)
        {
            parts.Add(Contract(sheet, row, contract));
            row += contract.Payments.Count + 4;
        }
        var all = sheet.Range(row, 1, row, 2).Merge();
        all.FirstCell().Value = "اجمالي الكل";
        sheet.Cell(row, cost).SetMoneyFormula(parts.Count > 0 ? string.Join("+", parts) : "0");
        sheet.Range(row, 1, row, cost).AsTotal();
        return $"{costLetter}{row}";
    }

    /// <summary>
    /// Writes a contract's block from <paramref name="start"/>, as the statement's payment blocks: its title and value, a header, each payment with what remains of a fixed value after it (a payment
    /// outside the contract leaves that as it was, BR-04), and the payments' total down the side. Answers the total's address.
    /// </summary>
    private static string Contract(IXLWorksheet sheet, int start, ContractReportView contract)
    {
        var title = sheet.Range(start, 1, start, 2).Merge();
        title.FirstCell().SetCentred($"دفعات {contract.ContractorName}");
        var value = sheet.Range(start, 3, start, 5).Merge();
        if (contract.IsOpenValue)
        {
            value.FirstCell().SetCentred("قيمة مفتوحة");
        }
        else
        {
            value.FirstCell().SetMoney(contract.Value).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        sheet.Range(start, 1, start, 5).Style.Fill.SetBackgroundColor(StatementStyle.Grey).Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        var header = sheet.Range(start + 1, 1, start + 1, 5).AsHeader();
        header.Cell(1, 2).Value = "الوصف";
        header.Cell(1, 3).Value = "التاريخ";
        header.Cell(1, 4).Value = "الدفعة";
        header.Cell(1, 5).Value = "المتبقي";
        var row = start + 2;
        var remaining = $"C{start}";
        var number = 1;
        foreach (var payment in contract.Payments)
        {
            sheet.Cell(row, 1).SetCentred(number++);
            sheet.Cell(row, 2).Value = string.Join(" · ", new[] { payment.Description, payment.IsOutsideContract ? "خارج العقد" : "" }.Concat(payment.Values).Where(text => text.Length > 0));
            sheet.Cell(row, 3).SetDate(payment.Date);
            sheet.Cell(row, 4).SetMoney(payment.Amount);
            if (!contract.IsOpenValue)
            {
                sheet.Cell(row, 5).SetMoneyFormula(payment.IsOutsideContract ? remaining : $"{remaining}-D{row}");
                remaining = $"E{row}";
            }
            Mark(sheet.Range(row, 1, row, 5).AsBody(), payment.IsNew);
            row++;
        }
        var total = sheet.Range(start, 6, Math.Max(row - 1, start + 1), 6).Merge();
        total.FirstCell().SetMoneyFormula(row > start + 2 ? $"SUM(D{start + 2}:D{row - 1})" : "0");
        total.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        return $"F{start}";
    }

    private static void Mark(IXLRange row, bool isNew)
    {
        if (isNew)
        {
            row.AsNew();
        }
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
