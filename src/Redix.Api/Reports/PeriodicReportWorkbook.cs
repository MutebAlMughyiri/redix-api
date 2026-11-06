using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;
using Redix.Api.Accounts;
using Redix.Api.Partners;
using Redix.Api.Revenue;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>
/// The periodic report as a workbook laid out as the project's own statement (UC-055, FR-46): الخلاصة with the revenue on one side and the expenses on the other, then a sheet for every
/// other line of it, each written by <see cref="StatementWriter"/>: راس المال with a table per partner, a sheet per kind of revenue, and a sheet per item. Every line of the summary points at
/// its own sheets, and no sheet serves two lines (AC-46.10). Every total is a live formula, and new rows are green (BR-26).
/// </summary>
public static class PeriodicReportWorkbook
{
    private const int Revenue = 1;
    private const int Expenses = 4;

    private static readonly (RevenueType Type, string Name)[] RevenueSheets = [(RevenueType.Sale, "البيع"), (RevenueType.VatRefund, "استرجاع الضرائب"), (RevenueType.Other, "إيرادات أخرى")];

    private static readonly StatementColumn MovementAmount = new("المبلغ", 13, CellKind.Text);

    private static readonly List<StatementColumn> MovementColumns = [new("النوع", 11, CellKind.Centred), new("التاريخ", 13, CellKind.Date), new("الوصف", 27.25, CellKind.Linked)];

    private static readonly StatementColumn RevenueAmount = new("المبلغ", 13, CellKind.Text);

    private static readonly List<StatementColumn> RevenueColumns = [new("الوصف", 39.13, CellKind.Text), new("التاريخ", 11.13, CellKind.Date)];

    /// <summary>The workbook of <paramref name="report"/>, each partner's receipt a link under <paramref name="receiptsAddress"/>.</summary>
    public static byte[] Of(PeriodicReportSnapshot report, string receiptsAddress)
    {
        using var workbook = StatementStyle.NewWorkbook();
        var names = new HashSet<string>();
        var summary = StatementStyle.AddSheet(workbook, "الخلاصة", names, 21.5, 13.13, 5.25, 22, 15.5, 15.63);
        var capitalTotal = StatementWriter.Add(workbook, CapitalOf(report, receiptsAddress), names);
        var revenueTotals = RevenueSheets.Where(kind => kind.Type != RevenueType.Other || report.RevenueRows.Any(row => row.Type == RevenueType.Other))
            .ToDictionary(kind => kind.Type, kind => StatementWriter.Add(workbook, RevenueOf(kind.Name, report.RevenueRows.Where(row => row.Type == kind.Type)), names));
        var repeated = report.Items.GroupBy(item => item.ItemName).Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet();
        var itemTotals = report.Items.ToDictionary(item => item.ItemId, item => StatementWriter.Add(workbook, ItemReportFiles.SheetOf(item) with { Name = repeated.Contains(item.ItemName) ? $"{item.ItemName} · {item.CategoryName}" : item.ItemName }, names));
        FillSummary(summary, report, capitalTotal, revenueTotals, itemTotals);
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

    private static StatementTable Movements(string title, IEnumerable<StatementRow> rows) => new(title, MovementColumns, MovementAmount, rows.ToList(), Agreement.None);

    private static StatementRow MovementRow(CapitalMovementView movement, string receiptsAddress) => new(
        [
            TranslationSeed.Arabic($"capital.{JsonNamingPolicy.CamelCase.ConvertName(movement.Type.ToString())}"),
            movement.Date.ToDateTime(TimeOnly.MinValue),
            movement.Description.Length > 0 || movement.AttachmentId.Length == 0 ? movement.Description : "الإيصال",
        ],
        movement.Amount, movement.AttachmentId.Length > 0 ? receiptsAddress + movement.AttachmentId : "", false, true);

    /// <summary>A sheet of one kind of revenue, named <paramref name="name"/>: its entries oldest first, new ones green (BR-26).</summary>
    private static StatementSheet RevenueOf(string name, IEnumerable<ReportRevenueView> rows) =>
        new(name, [new StatementTable(name, RevenueColumns, RevenueAmount, rows.Select(row => new StatementRow([row.Description, row.Date.ToDateTime(TimeOnly.MinValue)], row.Amount, "", row.IsNew, true)).ToList(), Agreement.None)], "");

    private static void FillSummary(IXLWorksheet sheet, PeriodicReportSnapshot report, string capitalTotal, Dictionary<RevenueType, string> revenueTotals, Dictionary<Guid, string> itemTotals)
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
        Line(sheet, row++, Revenue, "استرجاع ضرائب", revenueTotals[RevenueType.VatRefund]).AsBody();
        if (revenueTotals.TryGetValue(RevenueType.Other, out var other))
        {
            Line(sheet, row++, Revenue, "إيرادات أخرى", other).AsBody();
        }
        var sales = row++;
        Line(sheet, sales, Revenue, "فلوس البيع المودعة بالحساب", revenueTotals[RevenueType.Sale]).AsMutedTotal();
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
