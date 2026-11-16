using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>
/// The periodic report as a PDF (UC-054, FR-45): its summary laid out as report 23, and the capital and every item drawn by <see cref="StatementPdf"/> from the very tables its workbook
/// writes (<see cref="PeriodicReportWorkbook"/>, findings row 130). Words are the statement's own Arabic, as report 23 prints them.
/// </summary>
public static class PeriodicReportFiles
{
    /// <summary>A4 portrait, right to left: the summary, then a page per item, رأس المال among them; new rows shaded green (BR-26).</summary>
    public static byte[] Pdf(PeriodicReportSnapshot report)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document =>
        {
            Page(document, page => Summary(page, report));
            foreach (var item in report.Items)
            {
                Page(document, page => StatementPdf.Draw(page, ItemReportFiles.SheetOf(item, [])));
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
    public static void Line(ColumnDescriptor column, string label, decimal amount, bool isStrong = false) => Line(column, label, ReportFormats.Money(amount), isStrong);

    /// <summary>A line of a label and a figure already written.</summary>
    public static void Line(ColumnDescriptor column, string label, string written, bool isStrong) => column.Item().Row(row =>
    {
        var text = row.RelativeItem().Text(label);
        var figure = row.ConstantItem(120).AlignLeft().Text(written);
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
        foreach (var category in report.RevenueCategories)
        {
            Line(page, category.Name, category.Total);
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
        Line(page, "فرق المطابقة", report.ReconciliationDifference, true);
        if (report.PendingCount > 0)
        {
            Line(page, $"{TranslationSeed.Arabic("reports.pendingExpenses")} ({report.PendingCount})", report.PendingAmount);
        }
        if (report.PendingRevenueCount > 0)
        {
            Line(page, $"{TranslationSeed.Arabic("reports.pendingRevenue")} ({report.PendingRevenueCount})", report.PendingRevenueAmount);
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
}
