package reports

import (
	"encoding/json"
	"fmt"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"net/http"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// The periodic report as a workbook laid out as the project's own statement (UC-055, FR-46, PeriodicReportWorkbook): الخلاصة with the revenue on
// one side and the expenses on the other, then a sheet per item - expense, revenue or capital, رأس المال's tables its partners (redix-api #109) -
// in the order the summary reads them, and the hidden «القوائم» last (redix-api #129). Every line of the summary points at its own sheets, and no
// sheet serves two lines (AC-46.10); each line's name opens its sheet, and each sheet has «← الخلاصة» back. Every total is a live formula, new rows
// are green (BR-26), and every sheet prints with the project and the report's number at its foot.

const (
	summaryName = "الخلاصة"
	// revenueSide and expensesSide are the columns each side of the summary starts in.
	revenueSide  = 1
	expensesSide = 4
)

// waitingWords are the labels of what waits, as the summary's lines name them.
type waitingWords struct {
	expenses, revenue string
}

// periodicWorkbookOf is report as a workbook, each «المرفق» opening its entry's file by its link in fileLinks.
func periodicWorkbookOf(report snapshot, fileLinks map[uuid.UUID]string, words waitingWords, zone string) ([]byte, error) {
	book := statement.NewBook(report.ProjectName+" · التقرير "+strconv.FormatInt(report.Number, 10), summaryName)
	summary := book.AddSheet(summaryName, 21.5, 13.13, 5.25, 22, 15.5, 15.63)
	named := map[string]int{}
	for _, item := range report.Items {
		named[item.ItemName]++
	}
	sheets := map[uuid.UUID]statement.Placed{}
	for _, item := range inReadingOrder(report) {
		sheet := sheetOf(item, fileLinks)
		if named[item.ItemName] > 1 {
			sheet.Name = item.ItemName + " · " + item.CategoryName
		}
		sheets[item.ItemID] = book.Add(sheet)
	}
	book.Lists.AddTo(book)
	fillSummary(summary, report, sheets, words, zone)
	summary.Fit()
	summary.Print(book.Footer)
	return book.Bytes()
}

// inReadingOrder is the report's items in the order الخلاصة reads them (redix-api #129): the revenue side's categories, then the expense side's,
// each category's items in order; any other last.
func inReadingOrder(report snapshot) []entries.ItemReport {
	var listed []uuid.UUID
	for _, category := range append(slices.Clone(report.RevenueCategories), report.Categories...) {
		for _, item := range category.Items {
			listed = append(listed, item.ID)
		}
	}
	placeOf := func(item entries.ItemReport) int {
		if place := slices.Index(listed, item.ItemID); place >= 0 {
			return place
		}
		return len(listed)
	}
	ordered := slices.Clone(report.Items)
	slices.SortStableFunc(ordered, func(a, b entries.ItemReport) int { return placeOf(a) - placeOf(b) })
	return ordered
}

// fillSummary fills الخلاصة in the statement's layout. Each side lists its categories in order (AC-46.7): the revenue side every category of
// revenue items, رأس المال first as the structure orders it; the expense side every category of expense items. A category of one item points at
// that item's sheet, one of more at its detail block (AC-46.10). A line holding only sale items is muted and left out of مجموع الايرادات بدون
// البيع.
func fillSummary(sheet *statement.Worksheet, report snapshot, sheets map[uuid.UUID]statement.Placed, words waitingWords, zone string) {
	title := sheet.Range(1, 1, 1, 5).Merge()
	title.First().Set(report.ProjectName)
	title.Bold().Size(15).Centre()
	issuedAt := report.IssuedAt.In(zones.Location(zone))
	heading(sheet, 2, "رقم التقرير:").Centred(strconv.FormatInt(report.Number, 10)).Format("@")
	heading(sheet, 3, "تاريخ التقرير:").Date(time.Date(issuedAt.Year(), issuedAt.Month(), issuedAt.Day(), 0, 0, 0, 0, time.UTC))
	heading(sheet, 4, "وقت التقرير:").Centred(issuedAt.Format("03:04 PM"))
	lastAccount := 1
	for _, account := range report.Accounts {
		lastAccount++
		sheet.Cell(lastAccount, expensesSide).Set(account.Name)
		sheet.Cell(lastAccount, expensesSide+1).Money(account.Balance.Number().Float())
		if account.Type == accounts.CashCustody {
			sheet.Range(lastAccount, expensesSide, lastAccount, expensesSide+1).AsNew()
		}
	}
	lastAccount = max(2, lastAccount)
	accountsTitle := sheet.Range(2, 6, 3, 6).Merge()
	accountsTitle.First().Centred("إجمالي\nالحسابات")
	accountsTitle.Fill(statement.Grey).Wrap().Boxed()
	accountsTotal := sheet.Range(4, 6, 5, 6).Merge()
	accountsTotal.First().MoneyFormula(fmt.Sprintf("SUM(E2:E%d)", lastAccount)).Align("center")
	accountsTotal.AsNew().Boxed()
	top := max(9, lastAccount+2)
	labelled(sheet, top, revenueSide, "ايرادات").AsTotal()
	labelled(sheet, top, expensesSide, "مصروفات").AsTotal()
	expensesTotal := top + 1 + len(report.Categories)
	revenue := top + 1 + len(report.RevenueCategories)
	withoutSales := revenue + 2
	surplus := withoutSales + 2
	details := detailsOf(sheet, append(slices.Clone(report.Categories), report.RevenueCategories...), sheets, [2]int{surplus, expensesTotal})
	valueOf := func(category categoryTotalView) string {
		if len(category.Items) > 1 {
			return details[category.ID]
		}
		for _, item := range category.Items {
			return totalOf(sheets, item.ID)
		}
		return "0"
	}
	categoryLine := func(row, column int, category categoryTotalView) statement.Range {
		line := figured(sheet, row, column, category.Name, valueOf(category))
		if len(category.Items) == 1 {
			linkToSheetOf(line, category.Items[0].ID, sheets)
		}
		return line
	}
	row := top + 1
	for _, category := range report.RevenueCategories {
		line := categoryLine(row, revenueSide, category)
		row++
		if !slices.ContainsFunc(category.Items, func(item itemTotalView) bool { return item.Type != structure.Sale }) {
			line.AsMutedTotal()
		} else {
			line.AsBody()
		}
	}
	figured(sheet, revenue, revenueSide, "مجموع الايرادات", sumOf("B", top+1, revenue-1)).AsTotal()
	var sales []string
	for _, category := range report.RevenueCategories {
		for _, item := range category.Items {
			if item.Type == structure.Sale {
				sales = append(sales, totalOf(sheets, item.ID))
			}
		}
	}
	withoutSalesFormula := fmt.Sprintf("B%d", revenue)
	if len(sales) > 0 {
		withoutSalesFormula = fmt.Sprintf("B%d-(%s)", revenue, strings.Join(sales, "+"))
	}
	figured(sheet, withoutSales, revenueSide, "مجموع الايرادات بدون البيع", withoutSalesFormula).AsMutedTotal()
	figured(sheet, withoutSales+1, revenueSide, "الفائض بدون البيع", fmt.Sprintf("B%d-E%d", withoutSales, expensesTotal)).AsMutedTotal()
	figured(sheet, surplus, revenueSide, "الفائض\n( المبلغ المفروض بالحساب)", fmt.Sprintf("B%d-E%d", revenue, expensesTotal)).AsTotal().Wrap()
	row = top + 1
	for _, category := range report.Categories {
		categoryLine(row, expensesSide, category).AsBody()
		row++
	}
	figured(sheet, expensesTotal, expensesSide, "مجموع المصروفات", sumOf("E", top+1, expensesTotal-1)).AsTotal()
	amounted(sheet, 5, revenueSide, "مصروفات التقرير السابق", report.PreviousExpenses.Number())
	figured(sheet, 6, revenueSide, "مصروفات التقرير الحالي", fmt.Sprintf("E%d", expensesTotal))
	figured(sheet, 7, revenueSide, "فرق المصروفات", "B6-B5")
	for row := 5; row <= 7; row++ {
		sheet.Cell(row, revenueSide+1).Align("center")
	}
	if report.PendingCount > 0 {
		amounted(sheet, 8, revenueSide, fmt.Sprintf("%s (%d)", words.expenses, report.PendingCount), report.PendingAmount.Number())
	}
	if report.PendingRevenueCount > 0 {
		amounted(sheet, surplus+1, revenueSide, fmt.Sprintf("%s (%d)", words.revenue, report.PendingRevenueCount), report.PendingRevenueAmount.Number())
	}
	sheet.Cell(6, 6).Formula = fmt.Sprintf("ROUND(F4,2)=ROUND(B%d,2)", surplus)
	sheet.Cell(7, 6).MoneyFormula(fmt.Sprintf("F4-B%d", surplus)).Align("center")
}

// detailsOf writes a block per category of more than one item among categories, each item pointing at its sheet's total and its name opening the
// sheet, under whichever side of the summary ends higher, starting from bottoms: the last row of the revenue side, then of the expenses side. It
// answers the address of each block's total.
func detailsOf(sheet *statement.Worksheet, categories []categoryTotalView, sheets map[uuid.UUID]statement.Placed, bottoms [2]int) map[uuid.UUID]string {
	totals := map[uuid.UUID]string{}
	for _, category := range categories {
		if len(category.Items) <= 1 {
			continue
		}
		side, column := 0, revenueSide
		if bottoms[0] > bottoms[1] {
			side, column = 1, expensesSide
		}
		start := bottoms[side] + 3
		head := sheet.Range(start, column, start, column+1).Merge()
		head.First().Set("تفاصيل " + category.Name + ":")
		head.AsTotal().UnderlineHair()
		row := start + 1
		for _, item := range category.Items {
			linkToSheetOf(figured(sheet, row, column, item.Name, totalOf(sheets, item.ID)).AsBody(), item.ID, sheets)
			row++
		}
		letter := statement.ColumnLetter(column + 1)
		figured(sheet, row, column, "الاجمالي", sumOf(letter, start+1, row-1)).AsTotal()
		totals[category.ID] = fmt.Sprintf("%s%d", letter, row)
		bottoms[side] = row
	}
	return totals
}

// totalOf is the address of the item's sheet's last total; "0" for an item with no sheet.
func totalOf(sheets map[uuid.UUID]statement.Placed, itemID uuid.UUID) string {
	if placed, found := sheets[itemID]; found {
		return placed.Total
	}
	return statement.NotPlaced.Total
}

// sumOf is the sum of the column's rows from first to last; 0 when there are none.
func sumOf(letter string, first, last int) string {
	if last < first {
		return "0"
	}
	return fmt.Sprintf("SUM(%s%d:%s%d)", letter, first, letter, last)
}

// linkToSheetOf makes the label of line open the sheet of the item, when it has one (redix-api #129).
func linkToSheetOf(line statement.Range, itemID uuid.UUID, sheets map[uuid.UUID]statement.Placed) {
	if placed, found := sheets[itemID]; found {
		line.First().LinkTo(placed.Name)
	}
}

// labelled is a label in column with nothing beside it yet; it answers the two cells.
func labelled(sheet *statement.Worksheet, row, column int, label string) statement.Range {
	sheet.Cell(row, column).Set(label)
	return sheet.Range(row, column, row, column+1)
}

// amounted is a label in column and an amount beside it; it answers the two cells.
func amounted(sheet *statement.Worksheet, row, column int, label string, amount fixed.Number) statement.Range {
	sheet.Cell(row, column+1).Money(amount.Float())
	return labelled(sheet, row, column, label)
}

// figured is a label in column and a formula shown as an amount beside it; it answers the two cells.
func figured(sheet *statement.Worksheet, row, column int, label, formula string) statement.Range {
	sheet.Cell(row, column+1).MoneyFormula(formula)
	return labelled(sheet, row, column, label)
}

// heading is a bold label of the heading, at the top right; it answers the bold cell beside it.
func heading(sheet *statement.Worksheet, row int, label string) *statement.Cell {
	sheet.Range(row, 1, row, 2).Bold()
	sheet.Cell(row, 1).Set(label)
	return sheet.Cell(row, 2)
}

// forFilesOf is the report as the files of caller give it (redix-api #131 D1, BR-31): رأس المال's tables and the partners' lines only of the
// partners they see, as the capital screen shows them, its total still the whole capital. Whoever sees all capital gets every partner.
func forFilesOf(report snapshot, caller access.Caller, projectID uuid.UUID) snapshot {
	if caller.Access.Holds(projectID, access.SeeAllCapital) {
		return report
	}
	sees := func(partnerID uuid.UUID) bool { return caller.Access.SeesCapitalOf(projectID, partnerID) }
	partners := []reportPartner{}
	for _, partner := range report.Partners {
		if sees(partner.ID) {
			partners = append(partners, partner)
		}
	}
	items := []entries.ItemReport{}
	for _, item := range report.Items {
		if item.Type == structure.Capital {
			item = item.WithTablesOf(sees)
		}
		items = append(items, item)
	}
	report.Partners, report.Items = partners, items
	return report
}

// periodicWorkbook is the report as a workbook with live totals (UC-055), its capital showing only the partners the caller may see (redix-api #131
// D1, BR-31); 404 for a number not issued.
func (s Routes) periodicWorkbook(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	report, found, err := s.issued(r, web.Int(r, "number"))
	if err == nil && !found {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	projectID, read := web.ID(r, "projectId"), s.database.Queries()
	seen := forFilesOf(report, caller, projectID)
	var ids []uuid.UUID
	for _, item := range seen.Items {
		ids = append(ids, item.EntryIDs()...)
	}
	links, linksErr := s.links.OfEntries(r.Context(), read, projectID, ids, r)
	words, wordsErr := s.waitingWords(r)
	zone, zoneErr := zones.Of(r.Context(), read, projectID)
	if err != nil || linksErr != nil || wordsErr != nil || zoneErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	file, err := periodicWorkbookOf(seen, links, words, zone)
	s.sendFile(w, r, file, excelType, fmt.Sprintf("report-%d.xlsx", report.Number), err)
}

// issued is the report numbered number as it was issued; not found for a number not issued.
func (s Routes) issued(r *http.Request, number int64) (snapshot, bool, error) {
	kept, err := s.database.Queries().PeriodicReportSnapshot(r.Context(), store.PeriodicReportSnapshotParams{ProjectID: web.ID(r, "projectId"), Number: number})
	if err != nil || len(kept) == 0 {
		return snapshot{}, false, err
	}
	var report snapshot
	return report, true, json.Unmarshal([]byte(kept[0]), &report)
}
