using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Redix.Api.Reports;

/// <summary>The periodic report as a PDF laid out as report 23 (UC-054, FR-45) and as a workbook with live totals (UC-055, FR-46). Words are the statement's own Arabic, as report 23 prints them.</summary>
public static class PeriodicReportFiles
{
    /// <summary>A4 portrait, right to left: the summary, the partners, then a page per item; new rows shaded green (BR-26).</summary>
    public static byte[] Pdf(PeriodicReportSnapshot report)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document =>
        {
            Page(document, page => Summary(page, report));
            Page(document, page => Partners(page, report));
            foreach (var item in report.Items)
            {
                Page(document, page => ItemReportFiles.ItemPage(page, item));
            }
        }).GeneratePdf();
    }

    /// <summary>One page of a report: A4 portrait, right to left, in the embedded Arabic font, numbered at its foot.</summary>
    public static void Page(IDocumentContainer document, Action<ColumnDescriptor> content) => document.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(1.2f, Unit.Centimetre);
        page.ContentFromRightToLeft();
        page.DefaultTextStyle(style => style.FontFamily(ReportFormats.FontFamily).FontSize(9));
        page.Content().Column(content);
        page.Footer().AlignCenter().Text(text => text.CurrentPageNumber());
    });

    /// <summary>A line of a label and an amount.</summary>
    public static void Line(ColumnDescriptor column, string label, decimal amount, bool isStrong = false) => column.Item().Row(row =>
    {
        var text = row.RelativeItem().Text(label);
        var figure = row.ConstantItem(120).AlignLeft().Text(ReportFormats.Money(amount));
        if (isStrong)
        {
            text.Bold();
            figure.Bold();
        }
    });

    private static void Summary(ColumnDescriptor page, PeriodicReportSnapshot report)
    {
        page.Spacing(4);
        page.Item().Text($"{report.ProjectName} · تقرير رقم {report.Number} · {ReportFormats.Time(report.IssuedAt)}").FontSize(14).Bold();
        page.Item().PaddingTop(6).Text("أرصدة الحسابات").Bold();
        foreach (var account in report.Accounts)
        {
            Line(page, account.Name, account.Balance);
        }
        Line(page, "إجمالي الحسابات", report.AccountsTotal, true);
        page.Item().PaddingTop(6).Text("المصروفات").Bold();
        Line(page, "مصروفات التقرير السابق", report.PreviousExpenses);
        Line(page, "مصروفات التقرير الحالي", report.Expenses);
        Line(page, "الفرق", report.ExpensesDifference, true);
        page.Item().PaddingTop(6).Text("الإيرادات").Bold();
        Line(page, "رأس المال", report.Capital);
        Line(page, "استرجاع الضرائب", report.VatRefunds);
        Line(page, "البيع", report.Sales);
        if (report.OtherRevenue != 0)
        {
            Line(page, "إيرادات أخرى", report.OtherRevenue);
        }
        Line(page, "مجموع الإيرادات", report.Revenue, true);
        Line(page, "المجموع بدون البيع", report.RevenueWithoutSales);
        page.Item().PaddingTop(6).Text("المصروفات حسب التصنيف").Bold();
        foreach (var category in report.Categories)
        {
            Line(page, category.Name, category.Total);
        }
        Line(page, "مجموع المصروفات", report.Expenses, true);
        page.Item().PaddingTop(6).Text("الفائض").Bold();
        Line(page, "الفائض", report.Surplus, true);
        Line(page, "الفائض بدون البيع", report.SurplusWithoutSales);
        Line(page, "التصفية", report.Liquidation);
        Line(page, "فرق المطابقة", report.ReconciliationDifference, true);
        if (report.PendingCount > 0)
        {
            Line(page, $"بانتظار الاعتماد ({report.PendingCount})", report.PendingAmount);
        }
        foreach (var category in report.Categories.Where(category => category.Items.Any(item => item.Total != 0)))
        {
            page.Item().PaddingTop(6).Text($"تفاصيل {category.Name}").Bold();
            foreach (var item in category.Items.Where(item => item.Total != 0))
            {
                Line(page, item.Name, item.Total);
            }
        }
    }

    private static void Partners(ColumnDescriptor page, PeriodicReportSnapshot report)
    {
        page.Spacing(4);
        page.Item().Text("الشركاء ورأس المال").FontSize(14).Bold();
        page.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });
            Header(table, "الشريك", "الرصيد", "النسبة %");
            foreach (var partner in report.Partners)
            {
                Cell(table, partner.Name);
                Cell(table, ReportFormats.Money(partner.Balance));
                Cell(table, partner.SharePercent.ToString("0.0000000000", System.Globalization.CultureInfo.InvariantCulture));
            }
            Cell(table, "الإجمالي", true);
            Cell(table, ReportFormats.Money(report.CapitalTotal), true);
            Cell(table, "100.0000000000", true);
        });
        foreach (var partner in report.Partners.Where(partner => partner.Movements.Count > 0))
        {
            page.Item().PaddingTop(8).Text($"حركات {partner.Name}").Bold();
            foreach (var movement in partner.Movements)
            {
                Line(page, $"{ReportFormats.Date(movement.Date)} · {(movement.Description.Length > 0 ? movement.Description : movement.Type.ToString())}", movement.Amount);
            }
        }
    }

    /// <summary>A table's header row.</summary>
    public static void Header(TableDescriptor table, params string[] titles)
    {
        table.Header(header =>
        {
            foreach (var title in titles)
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text(title).Bold();
            }
        });
    }

    /// <summary>A cell of a table, shaded green when it is new (BR-26).</summary>
    public static void Cell(TableDescriptor table, string text, bool isStrong = false, bool isNew = false)
    {
        var cell = table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Background(isNew ? Colors.Green.Lighten4 : Colors.White).Padding(3).Text(text);
        if (isStrong)
        {
            cell.Bold();
        }
    }

    /// <summary>A summary sheet, a partners sheet with a link to each receipt, and a sheet per item; every total a live formula, new rows green (FR-46).</summary>
    public static byte[] Excel(PeriodicReportSnapshot report, string receiptsAddress)
    {
        using var workbook = new XLWorkbook();
        var names = new HashSet<string>();
        var summary = workbook.Worksheets.Add(ReportFormats.SheetName("الملخص", names));
        var partners = workbook.Worksheets.Add(ReportFormats.SheetName("الشركاء", names));
        var itemTotals = new Dictionary<Guid, string>();
        foreach (var item in report.Items)
        {
            var sheet = workbook.Worksheets.Add(ReportFormats.SheetName(item.ItemName, names));
            itemTotals[item.ItemId] = $"'{sheet.Name}'!{ItemReportFiles.FillSheet(sheet, item)}";
        }
        FillSummary(summary, report, itemTotals);
        FillPartners(partners, report, receiptsAddress);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void FillSummary(IXLWorksheet sheet, PeriodicReportSnapshot report, Dictionary<Guid, string> itemTotals)
    {
        sheet.RightToLeft = true;
        sheet.Cell(1, 1).Value = $"{report.ProjectName} · تقرير رقم {report.Number} · {ReportFormats.Time(report.IssuedAt)}";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        var row = 3;
        void Put(string label, Action<IXLCell> value)
        {
            sheet.Cell(row, 1).Value = label;
            value(sheet.Cell(row, 2));
            sheet.Cell(row, 2).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            row++;
        }

        var accountsFirst = row;
        foreach (var account in report.Accounts)
        {
            Put(account.Name, cell => cell.Value = account.Balance);
        }
        var accountsTotal = $"B{row}";
        Put("إجمالي الحسابات", cell => cell.FormulaA1 = row > accountsFirst ? $"SUM(B{accountsFirst}:B{row - 1})" : "0");
        var categoriesFirst = row + 1;
        row++;
        foreach (var category in report.Categories)
        {
            var parts = category.Items.Where(item => itemTotals.ContainsKey(item.Id)).Select(item => itemTotals[item.Id]).ToList();
            Put(category.Name, cell => cell.FormulaA1 = parts.Count > 0 ? string.Join("+", parts) : "0");
        }
        var expenses = $"B{row}";
        Put("مجموع المصروفات", cell => cell.FormulaA1 = row > categoriesFirst ? $"SUM(B{categoriesFirst}:B{row - 1})" : "0");
        Put("مصروفات التقرير السابق", cell => cell.Value = report.PreviousExpenses);
        var previous = $"B{row - 1}";
        Put("الفرق", cell => cell.FormulaA1 = $"{expenses}-{previous}");
        row++;
        var revenueFirst = row;
        Put("رأس المال", cell => cell.Value = report.Capital);
        Put("استرجاع الضرائب", cell => cell.Value = report.VatRefunds);
        Put("إيرادات أخرى", cell => cell.Value = report.OtherRevenue);
        var sales = $"B{row}";
        Put("البيع", cell => cell.Value = report.Sales);
        var revenue = $"B{row}";
        Put("مجموع الإيرادات", cell => cell.FormulaA1 = $"SUM(B{revenueFirst}:B{row - 1})");
        Put("المجموع بدون البيع", cell => cell.FormulaA1 = $"{revenue}-{sales}");
        var surplus = $"B{row}";
        Put("الفائض", cell => cell.FormulaA1 = $"{revenue}-{expenses}");
        Put("الفائض بدون البيع", cell => cell.FormulaA1 = $"{surplus}-{sales}");
        Put("التصفية", cell => cell.Value = report.Liquidation);
        Put("فرق المطابقة", cell => cell.FormulaA1 = $"{accountsTotal}-{surplus}");
        if (report.PendingCount > 0)
        {
            Put($"بانتظار الاعتماد ({report.PendingCount})", cell => cell.Value = report.PendingAmount);
        }
        sheet.Columns().AdjustToContents();
    }

    private static void FillPartners(IXLWorksheet sheet, PeriodicReportSnapshot report, string receiptsAddress)
    {
        sheet.RightToLeft = true;
        string[] titles = ["الشريك", "الرصيد", "النسبة %"];
        for (var column = 0; column < titles.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = titles[column];
            sheet.Cell(1, column + 1).Style.Font.Bold = true;
        }
        var row = 2;
        foreach (var partner in report.Partners)
        {
            sheet.Cell(row, 1).Value = partner.Name;
            sheet.Cell(row, 2).Value = partner.Balance;
            sheet.Cell(row, 2).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
            sheet.Cell(row, 3).FormulaA1 = $"IF(B{report.Partners.Count + 2}=0,0,B{row}/B{report.Partners.Count + 2}*100)";
            sheet.Cell(row, 3).Style.NumberFormat.Format = "0.0000000000";
            row++;
        }
        sheet.Cell(row, 1).Value = "الإجمالي";
        sheet.Cell(row, 2).FormulaA1 = row > 2 ? $"SUM(B2:B{row - 1})" : "0";
        sheet.Cell(row, 2).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        row += 2;
        foreach (var partner in report.Partners.Where(partner => partner.Movements.Count > 0))
        {
            sheet.Cell(row, 1).Value = $"حركات {partner.Name}";
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            foreach (var movement in partner.Movements)
            {
                sheet.Cell(row, 1).Value = movement.Date.ToDateTime(TimeOnly.MinValue);
                sheet.Cell(row, 1).Style.DateFormat.Format = ReportFormats.ExcelDate;
                sheet.Cell(row, 2).Value = movement.Amount;
                sheet.Cell(row, 2).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
                sheet.Cell(row, 3).Value = movement.Description;
                if (movement.AttachmentId.Length > 0)
                {
                    sheet.Cell(row, 4).Value = "الإيصال";
                    sheet.Cell(row, 4).SetHyperlink(new XLHyperlink(receiptsAddress + movement.AttachmentId));
                }
                row++;
            }
        }
        sheet.Columns().AdjustToContents();
    }
}
