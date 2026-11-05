using System.IO.Compression;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using Redix.Api.Revenue;

namespace Redix.Api.Reports;

/// <summary>The VAT statement as the accountant's own file (UC-057, FR-47), as a PDF in the same layout, and its invoices as one ZIP (UC-058).</summary>
public static class VatStatementFiles
{
    private static readonly string[] Ordinals = ["الأول", "الثاني", "الثالث", "الرابع"];

    private static readonly string[] Titles = ["التاريخ", "اسم الشركة", "البيان", "الرقم الضريبي", "أصل المبلغ قبل الضريبة قبل الخصم", "خصم", "أصل المبلغ قبل الضريبة بعد الخصم", "الضريبة", "إجمالي قيمة الفاتورة", "رقم الفاتورة"];

    /// <summary>The ordinal word of the quarter starting <paramref name="start"/>.</summary>
    public static string OrdinalOf(DateOnly start) => Ordinals[(start.Month - 1) / 3];

    /// <summary>A sheet per quarter named "ضريبة الربع ...", its merged title, the ten columns in their order with G = E−F, H = G × rate, I = G + H and a SUM row, then the sales section and the summary; dates yyyy-mm-dd, negatives in brackets, total sales yellow and net due red.</summary>
    public static byte[] Excel(VatStatementView statement)
    {
        using var workbook = new XLWorkbook();
        var names = new HashSet<string>();
        foreach (var quarter in statement.Quarters)
        {
            var sheet = workbook.Worksheets.Add(ReportFormats.SheetName($"ضريبة الربع {OrdinalOf(quarter.Start)} {quarter.Start.Year}", names));
            FillQuarter(sheet, quarter);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void FillQuarter(IXLWorksheet sheet, VatQuarterView quarter)
    {
        sheet.RightToLeft = true;
        sheet.Cell(1, 1).Value = $"ضريبة المشتريات عن الربع {OrdinalOf(quarter.Start)} سنة {quarter.Start.Year}";
        sheet.Range(1, 1, 1, Titles.Length).Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        Titles.Select((title, index) => sheet.Cell(2, index + 1).Value = title).ToList();
        sheet.Row(2).Style.Font.Bold = true;
        var row = 3;
        foreach (var line in quarter.Purchases)
        {
            Write(sheet, row++, line.Date, line.CompanyName, line.Description, line.VatNumber, line.BeforeDiscount, line.Discount, line.Rate, line.InvoiceNumber);
        }
        var purchasesTotal = Totals(sheet, row, 3, "الإجمالي");
        row += 2;
        sheet.Cell(row, 1).Value = "ضريبة المبيعات";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        var salesFirst = row;
        foreach (var line in quarter.Sales)
        {
            var rate = line.AfterDiscount == 0 ? 0 : decimal.Round(line.Tax / line.AfterDiscount, 4);
            Write(sheet, row++, line.Date, line.BuyerName, line.TaxType == SaleTaxType.Vat ? "" : "ضريبة التصرفات العقارية", line.VatNumber, line.BeforeDiscount, line.Discount, rate, line.InvoiceNumber);
        }
        var salesTotal = Totals(sheet, row, salesFirst, "إجمالي المبيعات");
        sheet.Range(row, 1, row, Titles.Length).Style.Fill.BackgroundColor = XLColor.Yellow;
        row += 2;
        sheet.Cell(row, 1).Value = "إجمالي ضريبة المبيعات";
        sheet.Cell(row, 8).Value = quarter.SalesVat;
        sheet.Cell(row + 1, 1).Value = "إجمالي ضريبة المشتريات";
        sheet.Cell(row + 1, 8).FormulaA1 = $"H{purchasesTotal}";
        sheet.Cell(row + 2, 1).Value = "الفرق المستحق";
        sheet.Cell(row + 2, 8).FormulaA1 = $"H{row}-H{row + 1}";
        sheet.Range(row + 2, 1, row + 2, Titles.Length).Style.Fill.BackgroundColor = XLColor.Red;
        sheet.Range(row, 8, row + 2, 8).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        sheet.Range(row, 1, row + 2, Titles.Length).Style.Font.Bold = true;
        if (salesTotal == 0)
        {
            sheet.Cell(row, 8).Value = 0;
        }
        sheet.Columns().AdjustToContents();
    }

    private static void Write(IXLWorksheet sheet, int row, DateOnly date, string company, string description, string vatNumber, decimal beforeDiscount, decimal discount, decimal rate, string number)
    {
        sheet.Cell(row, 1).Value = date.ToDateTime(TimeOnly.MinValue);
        sheet.Cell(row, 1).Style.DateFormat.Format = ReportFormats.ExcelVatDate;
        sheet.Cell(row, 2).Value = company;
        sheet.Cell(row, 3).Value = description;
        sheet.Cell(row, 4).Value = vatNumber;
        sheet.Cell(row, 5).Value = beforeDiscount;
        sheet.Cell(row, 6).Value = discount;
        sheet.Cell(row, 7).FormulaA1 = $"E{row}-F{row}";
        sheet.Cell(row, 8).FormulaA1 = $"G{row}*{rate.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        sheet.Cell(row, 9).FormulaA1 = $"G{row}+H{row}";
        sheet.Cell(row, 10).Value = number;
        sheet.Range(row, 5, row, 9).Style.NumberFormat.Format = ReportFormats.ExcelMoney;
    }

    private static int Totals(IXLWorksheet sheet, int row, int first, string label)
    {
        sheet.Cell(row, 1).Value = label;
        foreach (var column in new[] { "E", "F", "G", "H", "I" })
        {
            var cell = sheet.Cell($"{column}{row}");
            cell.FormulaA1 = row > first ? $"SUM({column}{first}:{column}{row - 1})" : "0";
            cell.Style.NumberFormat.Format = ReportFormats.ExcelMoney;
        }
        sheet.Range(row, 1, row, Titles.Length).Style.Font.Bold = true;
        return row;
    }

    /// <summary>The statement as a PDF in the same layout, a page per quarter.</summary>
    public static byte[] Pdf(VatStatementView statement)
    {
        ReportFormats.PreparePdf();
        return Document.Create(document =>
        {
            foreach (var quarter in statement.Quarters)
            {
                PeriodicReportFiles.Page(document, page =>
                {
                    page.Spacing(4);
                    page.Item().Text($"ضريبة المشتريات عن الربع {OrdinalOf(quarter.Start)} سنة {quarter.Start.Year}").FontSize(12).Bold();
                    page.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        PeriodicReportFiles.Header(table, "التاريخ", "اسم الشركة", "البيان", "الرقم الضريبي", "بعد الخصم", "الضريبة", "الإجمالي");
                        foreach (var line in quarter.Purchases)
                        {
                            foreach (var text in new[] { ReportFormats.Date(line.Date), line.CompanyName, line.Description, line.VatNumber, ReportFormats.Money(line.AfterDiscount), ReportFormats.Money(line.Vat), ReportFormats.Money(line.Total) })
                            {
                                PeriodicReportFiles.Cell(table, text);
                            }
                        }
                    });
                    PeriodicReportFiles.Line(page, "بعد الخصم", quarter.PurchasesAfterDiscount, true);
                    PeriodicReportFiles.Line(page, "ضريبة المشتريات", quarter.PurchasesVat, true);
                    PeriodicReportFiles.Line(page, "الإجمالي", quarter.PurchasesTotal, true);
                    PeriodicReportFiles.Line(page, "ضريبة المبيعات", quarter.SalesVat);
                    if (quarter.RealEstateTax != 0)
                    {
                        PeriodicReportFiles.Line(page, "ضريبة التصرفات العقارية", quarter.RealEstateTax);
                    }
                    PeriodicReportFiles.Line(page, "الفرق المستحق", quarter.NetDue, true);
                });
            }
        }).GeneratePdf();
    }

    /// <summary>The statement's invoice files in one ZIP, each named by its quarter and its row, so the accountant finds it beside its line (UC-058). A row with no file is in the exceptions list instead.</summary>
    public static byte[] Zip(VatStatementView statement, Func<string, (string Path, string FileName)> fileOf)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var quarter in statement.Quarters)
            {
                foreach (var line in quarter.Purchases.Where(line => line.AttachmentId.Length > 0))
                {
                    var (path, fileName) = fileOf(line.AttachmentId);
                    zip.CreateEntryFromFile(path, $"Q{(quarter.Start.Month - 1) / 3 + 1}-{quarter.Start.Year}-{line.Row:000}{Path.GetExtension(fileName)}");
                }
            }
        }
        return stream.ToArray();
    }
}
