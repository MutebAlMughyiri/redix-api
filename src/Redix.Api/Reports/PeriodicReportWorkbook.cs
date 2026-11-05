using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using Redix.Api.Accounts;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>
/// The periodic report as a workbook laid out as the project's own statement (UC-055, FR-46): الخلاصة with the revenue on one side and the expenses on the other, راس المال with a block of
/// movements per partner, then a sheet per item. Every total is a live formula that points at the sheet it adds, and new rows are green (BR-26).
/// </summary>
public static class PeriodicReportWorkbook
{
    private const int Revenue = 1;
    private const int Expenses = 4;

    /// <summary>The workbook of <paramref name="report"/>, each partner's receipt a link under <paramref name="receiptsAddress"/>.</summary>
    public static byte[] Of(PeriodicReportSnapshot report, string receiptsAddress)
    {
        using var workbook = StatementStyle.NewWorkbook();
        var names = new HashSet<string>();
        var summary = StatementStyle.AddSheet(workbook, "الخلاصة", names, 21.5, 13.13, 5.25, 22, 15.5, 15.63);
        var capital = StatementStyle.AddSheet(workbook, "راس المال", names, 4.63, 11, 13, 27.25, 13, 2.88, 4.38, 10.63, 11, 27, 13);
        var itemTotals = new Dictionary<Guid, string>();
        var repeated = report.Items.GroupBy(item => item.ItemName).Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet();
        foreach (var item in report.Items)
        {
            var sheet = StatementStyle.AddSheet(workbook, repeated.Contains(item.ItemName) ? $"{item.ItemName} · {item.CategoryName}" : item.ItemName, names);
            itemTotals[item.ItemId] = StatementStyle.Reference(sheet, ItemReportFiles.FillSheet(sheet, item));
        }
        var capitalTotal = StatementStyle.Reference(capital, FillCapital(capital, report, receiptsAddress));
        FillSummary(summary, report, capitalTotal, itemTotals);
        return StatementStyle.Bytes(workbook);
    }

    private static void FillSummary(IXLWorksheet sheet, PeriodicReportSnapshot report, string capitalTotal, Dictionary<Guid, string> itemTotals)
    {
        var title = sheet.Range(1, 1, 1, 5).Merge();
        title.FirstCell().Value = report.ProjectName;
        title.Style.Font.SetBold().Font.SetFontSize(15).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        var issuedAt = ReportFormats.InSaudiArabia(report.IssuedAt);
        Heading(sheet, 2, "رقم التقرير:").SetCentred(report.Number.ToString(CultureInfo.InvariantCulture)).Style.NumberFormat.Format = "@";
        Heading(sheet, 3, "تاريخ التقرير:").SetDate(DateOnly.FromDateTime(issuedAt));
        Heading(sheet, 4, "وقت التقرير:").SetCentred(issuedAt.ToString("hh:mm tt", CultureInfo.InvariantCulture));

        var lastAccount = 1;
        foreach (var account in report.Accounts)
        {
            lastAccount++;
            sheet.Cell(lastAccount, Expenses).Value = account.Name;
            sheet.Cell(lastAccount, Expenses + 1).SetMoney(account.Balance);
            if (account.Type == AccountType.CashCustody)
            {
                sheet.Range(lastAccount, Expenses, lastAccount, Expenses + 1).AsNew();
            }
        }
        lastAccount = Math.Max(2, lastAccount);
        var accountsTitle = sheet.Range(2, 6, 3, 6).Merge();
        accountsTitle.FirstCell().SetCentred("إجمالي\nالحسابات");
        accountsTitle.Style.Fill.SetBackgroundColor(StatementStyle.Grey).Alignment.SetWrapText(true).Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        var accountsTotal = sheet.Range(4, 6, 5, 6).Merge();
        accountsTotal.FirstCell().SetMoneyFormula($"SUM(E2:E{lastAccount})").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        accountsTotal.AsNew().Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

        var top = Math.Max(9, lastAccount + 2);
        Line(sheet, top, Revenue, "ايرادات").AsTotal();
        Line(sheet, top, Expenses, "مصروفات").AsTotal();
        var expensesTotal = top + 1 + report.Categories.Count;

        var row = top + 1;
        Line(sheet, row++, Revenue, "رأس المال", capitalTotal).AsBody();
        Line(sheet, row++, Revenue, "استرجاع ضرائب", report.VatRefunds).AsBody();
        if (report.OtherRevenue != 0)
        {
            Line(sheet, row++, Revenue, "إيرادات أخرى", report.OtherRevenue).AsBody();
        }
        var sales = row++;
        Line(sheet, sales, Revenue, "فلوس البيع المودعة بالحساب", report.Sales).AsMutedTotal();
        var revenue = row;
        Line(sheet, revenue, Revenue, "مجموع الايرادات", $"SUM(B{top + 1}:B{sales})").AsTotal();
        var withoutSales = revenue + 2;
        Line(sheet, withoutSales, Revenue, "مجموع الايرادات بدون البيع", $"B{revenue}-B{sales}").AsMutedTotal();
        var liquidation = withoutSales + 1;
        Line(sheet, liquidation, Revenue, "التصفية", report.Liquidation).AsMutedTotal();
        Line(sheet, liquidation + 1, Revenue, "الفائض بدون البيع", $"B{withoutSales}-E{expensesTotal}").AsMutedTotal();
        var surplus = liquidation + 2;
        Line(sheet, surplus, Revenue, "الفائض\n( المبلغ المفروض بالحساب)", $"B{revenue}-(E{expensesTotal}+B{liquidation})").AsTotal().Style.Alignment.WrapText = true;

        var details = Details(sheet, report, itemTotals, [surplus, expensesTotal]);
        row = top + 1;
        foreach (var category in report.Categories)
        {
            var value = category.Items.Count > 1 ? details[category.Id] : category.Items.Select(item => itemTotals.GetValueOrDefault(item.Id, "0")).FirstOrDefault("0");
            Line(sheet, row++, Expenses, category.Name, value).AsBody();
        }
        Line(sheet, expensesTotal, Expenses, "مجموع المصروفات", expensesTotal > top + 1 ? $"SUM(E{top + 1}:E{expensesTotal - 1})" : "0").AsTotal();

        Line(sheet, 5, Revenue, "مصروفات التقرير السابق", report.PreviousExpenses).FirstCell().CellRight().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        Line(sheet, 6, Revenue, "مصروفات التقرير الحالي", $"E{expensesTotal}").FirstCell().CellRight().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        Line(sheet, 7, Revenue, "فرق المصروفات", "B6-B5").FirstCell().CellRight().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        if (report.PendingCount > 0)
        {
            Line(sheet, 8, Revenue, $"بانتظار الاعتماد ({report.PendingCount})", report.PendingAmount);
        }
        sheet.Cell(6, 6).FormulaA1 = $"ROUND(F4,2)=ROUND(B{surplus},2)";
        sheet.Cell(7, 6).SetMoneyFormula($"F4-B{surplus}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    /// <summary>
    /// Writes a block per category of more than one item, each item pointing at its sheet, under whichever side of the summary ends higher, starting from <paramref name="bottoms"/>: the last row of the revenue
    /// side, then of the expenses side. Answers the address of each block's total.
    /// </summary>
    private static Dictionary<Guid, string> Details(IXLWorksheet sheet, PeriodicReportSnapshot report, Dictionary<Guid, string> itemTotals, int[] bottoms)
    {
        var totals = new Dictionary<Guid, string>();
        foreach (var category in report.Categories.Where(category => category.Items.Count > 1))
        {
            var side = bottoms[0] <= bottoms[1] ? 0 : 1;
            var column = side == 0 ? Revenue : Expenses;
            var start = bottoms[side] + 3;
            var head = sheet.Range(start, column, start, column + 1).Merge();
            head.FirstCell().Value = $"تفاصيل {category.Name}:";
            head.AsTotal().Style.Border.BottomBorder = XLBorderStyleValues.Hair;
            var row = start + 1;
            foreach (var item in category.Items)
            {
                Line(sheet, row++, column, item.Name, itemTotals.GetValueOrDefault(item.Id, "0")).AsBody();
            }
            var letter = sheet.Cell(row, column + 1).Address.ColumnLetter;
            Line(sheet, row, column, "الاجمالي", row > start + 1 ? $"SUM({letter}{start + 1}:{letter}{row - 1})" : "0").AsTotal();
            totals[category.Id] = $"{letter}{row}";
            bottoms[side] = row;
        }
        return totals;
    }

    /// <summary>
    /// Writes the partners' table and a block of movements per partner, the first under the table and each next one under whichever side ends higher, as the statement lays them out, and answers the
    /// address of the capital's total. Someone who sees only their own line
    /// (BR-31) still reads the whole total: the other partners are one line between them.
    /// </summary>
    private static string FillCapital(IXLWorksheet sheet, PeriodicReportSnapshot report, string receiptsAddress)
    {
        sheet.Range(1, 2, 1, 4).AsHeader().Style.Font.Bold = true;
        sheet.Cell(1, 3).Value = "راس المال";
        sheet.Cell(1, 4).Value = "نسبة المشاركة";
        var others = report.CapitalTotal - report.Partners.Sum(partner => partner.Balance);
        var total = 2 + report.Partners.Count + (others != 0 ? 1 : 0);
        int[] bottoms = [total + 1, 0];
        var row = 2;
        foreach (var partner in report.Partners)
        {
            var side = row == 2 || bottoms[0] <= bottoms[1] ? 0 : 1;
            var block = Movements(sheet, bottoms[side] + 1, side == 0 ? 1 : 7, partner, receiptsAddress);
            bottoms[side] = block + 1;
            Share(sheet, row++, total, partner.Name).SetMoneyFormula($"{(side == 0 ? "E" : "K")}{block}");
        }
        if (others != 0)
        {
            Share(sheet, row, total, "باقي الشركاء").SetMoney(others);
        }
        sheet.Cell(total, 2).SetCentred("الإجمالي");
        sheet.Cell(total, 3).SetMoneyFormula(total > 2 ? $"SUM(C2:C{total - 1})" : "0");
        sheet.Cell(total, 4).FormulaA1 = total > 2 ? $"SUM(D2:D{total - 1})" : "0";
        sheet.Cell(total, 4).Style.NumberFormat.Format = "0.00";
        sheet.Range(total, 2, total, 4).AsTotal();
        return $"C{total}";
    }

    /// <summary>A partner's line in the table, its share a formula of the total; answers the cell its capital goes in.</summary>
    private static IXLCell Share(IXLWorksheet sheet, int row, int total, string name)
    {
        sheet.Cell(row, 2).SetCentred(name);
        sheet.Cell(row, 4).FormulaA1 = $"IF(C{total}=0,0,C{row}/C{total}*100)";
        sheet.Cell(row, 4).Style.NumberFormat.Format = "0.0000000000";
        sheet.Range(row, 2, row, 4).AsBody();
        return sheet.Cell(row, 3);
    }

    /// <summary>Writes a partner's block from <paramref name="start"/> at <paramref name="column"/>: their name, then their movements oldest first, then their total; answers the total's row.</summary>
    private static int Movements(IXLWorksheet sheet, int start, int column, ReportPartnerView partner, string receiptsAddress)
    {
        var name = sheet.Range(start, column + 1, start, column + 4).Merge();
        name.FirstCell().SetCentred(partner.Name);
        name.Style.Font.Bold = true;
        sheet.Range(start + 1, column, start + 1, column + 4).AsHeader();
        sheet.Cell(start + 1, column + 2).Value = "التاريخ";
        sheet.Cell(start + 1, column + 3).Value = "الوصف";
        sheet.Cell(start + 1, column + 4).Value = "المبلغ";
        var row = start + 2;
        var number = 1;
        foreach (var movement in Enumerable.Reverse(partner.Movements))
        {
            sheet.Cell(row, column).SetCentred(number++);
            sheet.Cell(row, column + 1).SetCentred(TranslationSeed.Arabic($"capital.{JsonNamingPolicy.CamelCase.ConvertName(movement.Type.ToString())}"));
            sheet.Cell(row, column + 2).SetDate(movement.Date);
            var description = sheet.Cell(row, column + 3);
            description.Value = movement.Description.Length > 0 || movement.AttachmentId.Length == 0 ? movement.Description : "الإيصال";
            if (movement.AttachmentId.Length > 0)
            {
                description.SetHyperlink(new XLHyperlink(receiptsAddress + movement.AttachmentId));
            }
            sheet.Cell(row, column + 4).SetMoney(movement.Amount);
            sheet.Range(row, column, row, column + 4).AsBody();
            row++;
        }
        var letter = sheet.Cell(row, column + 4).Address.ColumnLetter;
        sheet.Cell(row, column).Value = "الاجمالي";
        sheet.Cell(row, column + 4).SetMoneyFormula(row > start + 2 ? $"SUM({letter}{start + 2}:{letter}{row - 1})" : "0");
        sheet.Range(row, column, row, column + 4).AsTotal();
        return row;
    }

    /// <summary>A label in <paramref name="column"/> and an amount beside it; answers the two cells.</summary>
    private static IXLRange Line(IXLWorksheet sheet, int row, int column, string label, decimal amount)
    {
        sheet.Cell(row, column).Value = label;
        sheet.Cell(row, column + 1).SetMoney(amount);
        return sheet.Range(row, column, row, column + 1);
    }

    /// <summary>A label in <paramref name="column"/> and a formula shown as an amount beside it; answers the two cells.</summary>
    private static IXLRange Line(IXLWorksheet sheet, int row, int column, string label, string formula)
    {
        sheet.Cell(row, column).Value = label;
        sheet.Cell(row, column + 1).SetMoneyFormula(formula);
        return sheet.Range(row, column, row, column + 1);
    }

    /// <summary>A label with nothing beside it yet; answers the two cells.</summary>
    private static IXLRange Line(IXLWorksheet sheet, int row, int column, string label)
    {
        sheet.Cell(row, column).Value = label;
        return sheet.Range(row, column, row, column + 1);
    }

    /// <summary>A bold label of the heading, at the top right; answers the bold cell beside it.</summary>
    private static IXLCell Heading(IXLWorksheet sheet, int row, string label)
    {
        sheet.Range(row, 1, row, 2).Style.Font.Bold = true;
        sheet.Cell(row, 1).Value = label;
        return sheet.Cell(row, 2);
    }
}
