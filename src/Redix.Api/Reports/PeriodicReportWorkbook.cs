using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using Redix.Api.Accounts;
using Redix.Api.Partners;
using Redix.Api.Structure;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>
/// The periodic report as a workbook laid out as the project's own statement (UC-055, FR-46): الخلاصة with the revenue on one side and the expenses on the other, then a sheet for every
/// other line of it, each written by <see cref="StatementWriter"/>: راس المال with a table per partner, then a sheet per item, expense or revenue. Every line of the summary points at
/// its own sheets, and no sheet serves two lines (AC-46.10). Every total is a live formula, and new rows are green (BR-26).
/// </summary>
public static class PeriodicReportWorkbook
{
    private const int Revenue = 1;
    private const int Expenses = 4;

    private static readonly StatementColumn MovementAmount = new("المبلغ", CellKind.Text);

    private static readonly List<StatementColumn> MovementColumns = [new("النوع", CellKind.Centred), new("التاريخ", CellKind.Date), new("الوصف", CellKind.Linked)];

    /// <summary>The workbook of <paramref name="report"/>, each partner's receipt a link under <paramref name="receiptsAddress"/>.</summary>
    public static byte[] Of(PeriodicReportSnapshot report, string receiptsAddress)
    {
        using var workbook = StatementStyle.NewWorkbook();
        var names = new HashSet<string>();
        var summary = StatementStyle.AddSheet(workbook, "الخلاصة", names, 21.5, 13.13, 5.25, 22, 15.5, 15.63);
        var capitalTotal = StatementWriter.Add(workbook, CapitalOf(report, receiptsAddress), names);
        var repeated = report.Items.GroupBy(item => item.ItemName).Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet();
        var itemTotals = report.Items.ToDictionary(item => item.ItemId, item => StatementWriter.Add(workbook, ItemReportFiles.SheetOf(item) with { Name = repeated.Contains(item.ItemName) ? $"{item.ItemName} · {item.CategoryName}" : item.ItemName }, names));
        FillSummary(summary, report, capitalTotal, itemTotals);
        StatementStyle.Fit(summary);
        return StatementStyle.Bytes(workbook);
    }

    /// <summary>
    /// The capital sheet (AC-46.6): a table per partner, their movements oldest first, each receipt a link under <paramref name="receiptsAddress"/>, and their share under their total. Someone
    /// who sees only their own line (BR-31) gets the other partners as one table of one row, so اجمالي الكل is still the project's capital.
    /// </summary>
    private static StatementSheet CapitalOf(PeriodicReportSnapshot report, string receiptsAddress)
    {
        List<StatementTable> tables = [.. report.Partners.Select(partner => Movements(partner.Name, Enumerable.Reverse(partner.Movements).Select(movement => MovementRow(movement, receiptsAddress))))];
        var others = report.CapitalTotal - report.Partners.Sum(partner => partner.Balance);
        if (others != 0)
        {
            tables.Add(Movements("باقي الشركاء", [new StatementRow(["", Blank.Value, "رأس مال باقي الشركاء"], others, "", false, true)]));
        }
        return new StatementSheet("راس المال", tables.Count > 0 ? tables : [Movements("الشركاء", [])], "نسبة المشاركة");
    }

    private static StatementTable Movements(string title, IEnumerable<StatementRow> rows) => new(title, MovementColumns, MovementAmount, rows.ToList(), 0);

    private static StatementRow MovementRow(CapitalMovementView movement, string receiptsAddress) => new(
        [
            ReportFormats.Movement(movement.Type),
            movement.Date.ToDateTime(TimeOnly.MinValue),
            movement.Description.Length > 0 || movement.AttachmentId.Length == 0 ? movement.Description : "الإيصال",
        ],
        movement.Amount, movement.AttachmentId.Length > 0 ? receiptsAddress + movement.AttachmentId : "", false, true);

    /// <summary>
    /// Fills الخلاصة in the statement's layout. Each side lists its categories in order (AC-46.7): the revenue side starts with رأس المال, then every category of revenue items; the expense
    /// side every category of expense items. A category of one item points at that item's sheet, one of more at its detail block (AC-46.10). A line holding only sale items is muted and left
    /// out of مجموع الايرادات بدون البيع.
    /// </summary>
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

        var revenue = top + 2 + report.RevenueCategories.Count;
        var withoutSales = revenue + 2;
        var surplus = withoutSales + 2;
        var details = Details(sheet, [.. report.Categories, .. report.RevenueCategories], itemTotals, [surplus, expensesTotal]);
        string ValueOf(CategoryTotalView category) => category.Items.Count > 1 ? details[category.Id] : category.Items.Select(item => itemTotals.GetValueOrDefault(item.Id, "0")).FirstOrDefault("0");

        Line(sheet, top + 1, Revenue, "رأس المال", capitalTotal).AsBody();
        var row = top + 2;
        foreach (var category in report.RevenueCategories)
        {
            var line = Line(sheet, row++, Revenue, category.Name, ValueOf(category));
            if (category.Items.All(item => item.Type == ItemType.Sale))
            {
                line.AsMutedTotal();
            }
            else
            {
                line.AsBody();
            }
        }
        Line(sheet, revenue, Revenue, "مجموع الايرادات", $"SUM(B{top + 1}:B{revenue - 1})").AsTotal();
        var sales = report.RevenueCategories.SelectMany(category => category.Items).Where(item => item.Type == ItemType.Sale).Select(item => itemTotals.GetValueOrDefault(item.Id, "0")).ToList();
        Line(sheet, withoutSales, Revenue, "مجموع الايرادات بدون البيع", sales.Count > 0 ? $"B{revenue}-({string.Join("+", sales)})" : $"B{revenue}").AsMutedTotal();
        Line(sheet, withoutSales + 1, Revenue, "الفائض بدون البيع", $"B{withoutSales}-E{expensesTotal}").AsMutedTotal();
        Line(sheet, surplus, Revenue, "الفائض\n( المبلغ المفروض بالحساب)", $"B{revenue}-E{expensesTotal}").AsTotal().Style.Alignment.WrapText = true;

        row = top + 1;
        foreach (var category in report.Categories)
        {
            Line(sheet, row++, Expenses, category.Name, ValueOf(category)).AsBody();
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
    /// Writes a block per category of more than one item among <paramref name="categories"/>, each item pointing at its sheet, under whichever side of the summary ends higher, starting from
    /// <paramref name="bottoms"/>: the last row of the revenue side, then of the expenses side. Answers the address of each block's total.
    /// </summary>
    private static Dictionary<Guid, string> Details(IXLWorksheet sheet, List<CategoryTotalView> categories, Dictionary<Guid, string> itemTotals, int[] bottoms)
    {
        var totals = new Dictionary<Guid, string>();
        foreach (var category in categories.Where(category => category.Items.Count > 1))
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
