using ClosedXML.Excel;
using QuestPDF.Fluent;
using Redix.Api.Contracts;

namespace Redix.Api.Reports;

/// <summary>An item's report and the contracts report as files (FR-33), and the item page and sheet the periodic report shares (FR-45, FR-46).</summary>
public static class ItemReportFiles
{
    private static readonly string[] Titles = ["م", "الوصف", "رقم الفاتورة", "التاريخ", "البند", "سعر الحبة", "العدد", "التكلفة"];

    /// <summary>One item's report as a PDF.</summary>
    public static byte[] Pdf(ItemReportView item)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document => PeriodicReportFiles.Page(document, page => ItemPage(page, item))).GeneratePdf();
    }

    /// <summary>One item's report as a workbook.</summary>
    public static byte[] Excel(ItemReportView item)
    {
        using var workbook = new XLWorkbook();
        FillSheet(workbook.Worksheets.Add(ReportFormats.SheetName(item.ItemName, [])), item);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
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

    /// <summary>Fills a sheet with an item's tables, each with a SUM row, new rows green, and answers the address of the item's total, a formula of the tables' sums, for the summary to add (FR-46).</summary>
    public static string FillSheet(IXLWorksheet sheet, ItemReportView item)
    {
        sheet.RightToLeft = true;
        sheet.Cell(1, 1).Value = $"{item.CategoryName} · {item.ItemName}";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        var row = 3;
        var sums = new List<string>();
        void Write(string title, List<ItemEntryRow> rows)
        {
            sheet.Cell(row, 1).Value = title;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            for (var column = 0; column < Titles.Length; column++)
            {
                sheet.Cell(row, column + 1).Value = Titles[column];
                sheet.Cell(row, column + 1).Style.Font.Bold = true;
            }
            row++;
            var first = row;
            var number = 1;
            foreach (var line in rows)
            {
                sheet.Cell(row, 1).Value = number++;
                sheet.Cell(row, 2).Value = string.Join(" · ", new[] { line.Description, line.SupplierName }.Concat(line.Values).Where(text => text.Length > 0));
                sheet.Cell(row, 3).Value = line.InvoiceNumber;
                sheet.Cell(row, 4).Value = line.Date.ToDateTime(TimeOnly.MinValue);
                sheet.Cell(row, 4).Style.DateFormat.Format = ReportFormats.ExcelDate;
                sheet.Cell(row, 5).Value = item.ItemName;
                if (line.Quantity != 0)
                {
                    sheet.Cell(row, 6).Value = line.UnitPrice;
                    sheet.Cell(row, 7).Value = line.Quantity;
                }
                sheet.Cell(row, 8).Value = line.Amount;
                sheet.Range(row, 6, row, 8).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
                if (line.IsNew)
                {
                    sheet.Range(row, 1, row, Titles.Length).Style.Fill.BackgroundColor = XLColor.LightGreen;
                }
                row++;
            }
            sheet.Cell(row, 2).Value = "المجموع";
            sheet.Cell(row, 8).FormulaA1 = row > first ? $"SUM(H{first}:H{row - 1})" : "0";
            sheet.Cell(row, 8).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            sheet.Range(row, 1, row, Titles.Length).Style.Font.Bold = true;
            sums.Add($"H{row}");
            row += 2;
        }

        foreach (var subgroup in item.Subgroups)
        {
            Write(subgroup.Name.Length > 0 ? subgroup.Name : "عام", subgroup.Rows);
        }
        foreach (var contract in item.Contracts)
        {
            Write($"{contract.ContractorName} · قيمة العقد {(contract.IsOpenValue ? "مفتوحة" : ReportFormats.Money(contract.Value))}", contract.Payments);
        }
        sheet.Cell(row, 2).Value = "اجمالي الكل";
        sheet.Cell(row, 8).FormulaA1 = sums.Count > 0 ? string.Join("+", sums) : "0";
        sheet.Cell(row, 8).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        sheet.Range(row, 1, row, Titles.Length).Style.Font.Bold = true;
        sheet.Columns().AdjustToContents();
        return $"H{row}";
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
