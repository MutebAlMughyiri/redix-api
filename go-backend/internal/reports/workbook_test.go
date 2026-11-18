// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"bytes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"math"
	"slices"
	"strconv"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
	"github.com/xuri/excelize/v2"
)

// The workbooks' layout, as .NET's ReportFilesTests checked it, each built from a report put together here and read back cell by cell with its
// formulas worked out.

// descriptionColumn is «الوصف», the first column of a table here, as of every table but رأس المال's (redix-api #129).
var descriptionColumn = entries.ReportColumn{Key: "description", Name: "الوصف", DataType: structure.Text, Choices: []entries.ReportChoice{}}

// purchaseColumns are five columns of a purchase table, as an item's own: the description, the date, the supplier, the unit price and the
// quantity.
var purchaseColumns = []entries.ReportColumn{descriptionColumn, {Key: "date", Name: "التاريخ", DataType: structure.Date, IsRequired: true},
	{Key: "supplier", Name: "المورد", DataType: structure.List, Choices: []entries.ReportChoice{{Name: "شركة النخيل"}}},
	{Key: "unitPrice", Name: "سعر الحبة", DataType: structure.Amount}, {Key: "quantity", Name: "العدد", DataType: structure.Number}}

// dateColumns are the two columns revenue, a payment or a partner's money prints here: its description and its date.
var dateColumns = []entries.ReportColumn{descriptionColumn, {Key: "date", Name: "التاريخ", DataType: structure.Date, IsRequired: true}}

func amountOf(amount float64) fixed.Number { return fixed.Number(math.Round(amount * 10000)) }

func countedOf(amount float64) fixed.Total { return fixed.Counted(amountOf(amount)) }

// row is a row of amount described as description in its first column, dated in its second, nothing in the others.
func row(amount float64, description string) entries.ReportRow {
	return entries.ReportRow{EntryID: uuid.New(), Date: moment.Date{Time: time.Date(2026, 9, 1, 0, 0, 0, 0, time.UTC)}, Amount: amountOf(amount),
		Values: []string{description, "2026-09-01", "", "", ""}}
}

func newRow(amount float64, description string) entries.ReportRow {
	line := row(amount, description)
	line.IsNew = true
	return line
}

func outsideRow(amount float64) entries.ReportRow {
	line := row(amount, "")
	line.IsOutsideContract = true
	return line
}

func tableOfRows(name string, columns []entries.ReportColumn, amountName string, rows ...entries.ReportRow) entries.TableReport {
	table := entries.TableReport{ID: uuid.New(), Name: name, Rows: rows, Columns: columns, AmountName: amountName}
	for _, line := range rows {
		table.Total = table.Total.Plus(fixed.Counted(line.Amount))
	}
	return table
}

func subgroup(name string, rows ...entries.ReportRow) entries.TableReport {
	return tableOfRows(name, purchaseColumns, "التكلفة", rows...)
}

func revenueGroup(name string, rows ...entries.ReportRow) entries.TableReport {
	return tableOfRows(name, dateColumns, "المبلغ", rows...)
}

func payments(contractor string, value float64, isOpen bool, rows ...entries.ReportRow) entries.ContractReport {
	contract := entries.ContractReport{ID: uuid.New(), ContractorName: contractor, Value: amountOf(value), IsOpenValue: isOpen, Payments: rows, Columns: dateColumns,
		AmountName: "الدفعة"}
	for _, line := range rows {
		contract.PaymentsTotal = contract.PaymentsTotal.Plus(fixed.Counted(line.Amount))
	}
	return contract
}

func item(name, category string, subgroups ...entries.TableReport) entries.ItemReport {
	report := entries.ItemReport{ItemID: uuid.New(), ItemName: name, CategoryName: category, Subgroups: subgroups, Contracts: []entries.ContractReport{}, ShowsEveryTable: true}
	for _, table := range subgroups {
		report.Total = report.Total.Plus(table.Total)
	}
	return report
}

// capitalOf is رأس المال as an item (redix-api #109): a table per partner holding one contribution, its sheet giving each one's share.
func capitalOf(partners ...struct {
	name   string
	amount float64
}) entries.ItemReport {
	var tables []entries.TableReport
	for _, partner := range partners {
		tables = append(tables, revenueGroup(partner.name, row(partner.amount, "")))
	}
	capital := item("رأس المال", "رأس المال", tables...)
	capital.Type, capital.ShowsTablePercentages = structure.Capital, true
	return capital
}

type partnerMoney = struct {
	name   string
	amount float64
}

func category(name string, kind structure.ItemType, items ...entries.ItemReport) categoryTotalView {
	line := categoryTotalView{ID: uuid.New(), Name: name, Items: []itemTotalView{}}
	for _, one := range items {
		line.Items = append(line.Items, itemTotalView{ID: one.ItemID, Name: one.ItemName, Total: one.Total, Type: kind})
		line.Total = line.Total.Plus(one.Total)
	}
	return line
}

func reportOf(items ...entries.ItemReport) snapshot {
	report := snapshot{ProjectName: "غصون 6", Number: 1, IssuedAt: moment.Of(time.Date(2026, 11, 5, 0, 0, 0, 0, time.UTC)), Items: items}
	for _, one := range items {
		report.Expenses = report.Expenses.Plus(one.Total)
	}
	return report
}

// opened is a workbook read back.
type opened struct {
	t    *testing.T
	file *excelize.File
}

func open(t *testing.T, content []byte, err error) opened {
	t.Helper()
	if err != nil {
		t.Fatal(err)
	}
	file, err := excelize.OpenReader(bytes.NewReader(content))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { file.Close() })
	return opened{t: t, file: file}
}

func itemWorkbook(t *testing.T, report entries.ItemReport) opened {
	content, err := workbookOf("غصون 6", report, nil)
	return open(t, content, err)
}

func periodicWorkbook(t *testing.T, report snapshot) opened {
	content, err := periodicWorkbookOf(report, nil, waitingWords{expenses: "قيود بانتظار الاعتماد", revenue: "ايرادات بانتظار الاعتماد"}, zones.Riyadh)
	return open(t, content, err)
}

func (o opened) first() string { return o.file.GetSheetList()[0] }

// text is what the cell holds, as it was written.
func (o opened) text(sheet, cell string) string {
	value, err := o.file.GetCellValue(sheet, cell, excelize.Options{RawCellValue: true})
	if err != nil {
		o.t.Fatal(err)
	}
	return value
}

// number is the cell's number, its formula worked out.
func (o opened) number(sheet, cell string) float64 {
	o.t.Helper()
	value := o.text(sheet, cell)
	if o.formula(sheet, cell) != "" {
		calculated, err := o.file.CalcCellValue(sheet, cell, excelize.Options{RawCellValue: true})
		if err != nil {
			o.t.Fatalf("%s!%s: %v", sheet, cell, err)
		}
		value = calculated
	}
	number, err := strconv.ParseFloat(value, 64)
	if err != nil {
		o.t.Fatalf("%s!%s holds %q, not a number", sheet, cell, value)
	}
	return number
}

func (o opened) formula(sheet, cell string) string {
	formula, _ := o.file.GetCellFormula(sheet, cell)
	return formula
}

func (o opened) style(sheet, cell string) *excelize.Style {
	id, _ := o.file.GetCellStyle(sheet, cell)
	style, err := o.file.GetStyle(id)
	if err != nil {
		o.t.Fatal(err)
	}
	return style
}

func (o opened) fill(sheet, cell string) string {
	if colours := o.style(sheet, cell).Fill.Color; len(colours) > 0 {
		return strings.ToUpper(strings.TrimPrefix(colours[0], "FF"))
	}
	return ""
}

func (o opened) link(sheet, cell string) string {
	_, target, _ := o.file.GetCellHyperLink(sheet, cell)
	return target
}

// mergeOf is the merged range the cell belongs to; "" for none.
func (o opened) mergeOf(sheet, cell string) string {
	merges, _ := o.file.GetMergeCells(sheet)
	column, row, _ := excelize.CellNameToCoordinates(cell)
	for _, merge := range merges {
		left, top, _ := excelize.CellNameToCoordinates(merge.GetStartAxis())
		right, bottom, _ := excelize.CellNameToCoordinates(merge.GetEndAxis())
		if column >= left && column <= right && row >= top && row <= bottom {
			return merge.GetStartAxis() + ":" + merge.GetEndAxis()
		}
	}
	return ""
}

// find is the one cell of the sheet that holds text.
func (o opened) find(sheet, text string) string {
	o.t.Helper()
	rows, _ := o.file.GetRows(sheet)
	var found []string
	for row, cells := range rows {
		for column, value := range cells {
			if name, _ := excelize.CoordinatesToCellName(column+1, row+1); value == text && o.mergeOf(sheet, name) == "" {
				found = append(found, name)
			}
		}
	}
	if len(found) != 1 {
		o.t.Fatalf("%s holds %q in %v", sheet, text, found)
	}
	return found[0]
}

func (o opened) holds(sheet string, wanted func(string) bool) bool {
	rows, _ := o.file.GetRows(sheet)
	return slices.ContainsFunc(rows, func(cells []string) bool { return slices.ContainsFunc(cells, wanted) })
}

func (o opened) want(got, want any, what string) {
	o.t.Helper()
	if got != want {
		o.t.Errorf("%s: %v, want %v", what, got, want)
	}
}

func TestAC_46_2_AContractsTotalWhatRemainsAndItsAgreedAmountAreLinesUnderItsTable(t *testing.T) {
	paint := item("بويه", "التشطيب", subgroup("", newRow(100, "P1")))
	paint.Contracts = []entries.ContractReport{payments("مؤسسة الدهان", 1000, false, row(400, ""), outsideRow(100), row(100, ""))}
	book := itemWorkbook(t, paint)
	sheet := book.first()
	book.want(book.text(sheet, "A1"), "بويه", "A1")
	book.want(book.text(sheet, "A4"), "الاجمالي", "A4")
	book.want(book.number(sheet, "G4"), 100.0, "G4")
	book.want(book.fill(sheet, "B3"), "B6D7A8", "a new row is green")
	book.want(book.text(sheet, "A6"), "دفعات مؤسسة الدهان", "A6")
	book.want(book.mergeOf(sheet, "G6"), "A6:G6", "the title row holds only the title")
	book.want(book.text(sheet, "G7"), "الدفعة", "G7")
	book.want(book.number(sheet, "G8"), 400.0, "G8")
	book.want(book.text(sheet, "A11"), "الاجمالي", "A11")
	book.want(book.number(sheet, "G11"), 600.0, "G11")
	book.want(book.text(sheet, "A12"), "المتبقي", "A12")
	book.want(book.number(sheet, "G12"), 500.0, "what remains leaves out the payment outside the contract")
	book.want(book.text(sheet, "A13"), "مبلغ الاتفاق", "A13")
	book.want(book.number(sheet, "G13"), 1000.0, "G13")
	book.want(book.text(sheet, "A15"), "اجمالي الكل", "A15")
	book.want(book.number(sheet, "G15"), 700.0, "G15")
}

func TestAC_46_2_AContractWithNoAgreedAmountShowsOnlyItsTotal(t *testing.T) {
	guard := item("الحراسة", "العظم")
	guard.Contracts = []entries.ContractReport{payments("الحارس", 0, true, row(200, ""))}
	book := itemWorkbook(t, guard)
	sheet := book.first()
	book.want(book.text(sheet, "A4"), "الاجمالي", "A4")
	book.want(book.number(sheet, "D4"), 200.0, "D4")
	if book.holds(sheet, func(text string) bool { return text == "المتبقي" || text == "مبلغ الاتفاق" }) {
		t.Error("a contract of no agreed amount shows what remains")
	}
}

func TestAC_46_3_ItemsOfOneNameInTwoCategoriesEachNameTheirCategoryOnTheirSheet(t *testing.T) {
	book := periodicWorkbook(t, reportOf(item("السباكة", "العظم", subgroup("", row(100, "P1"))), item("السباكة", "التشطيب", subgroup("", row(100, "P2")))))
	sheets := book.file.GetSheetList()
	book.want(strings.Join(sheets[1:3], "|"), "السباكة · العظم|السباكة · التشطيب", "the sheets")
}

func TestAC_46_7_UC_109_TheRevenueSideIsEachRevenueCategoryInTheStructuresOrderRasAlMalFirstEachOnItsOwnSheetAndSalesAreMuted(t *testing.T) {
	capital := capitalOf(partnerMoney{"سالم", 500})
	refunds := item("استرجاع الضرائب", "استرجاع ضرائب", revenueGroup("", row(200, ""), newRow(100, "")))
	refunds.Type = structure.VatRefund
	sales := item("البيع", "فلوس البيع المودعة بالحساب", revenueGroup("", row(1000, "S-1")))
	sales.Type = structure.Sale
	report := reportOf(capital, refunds, sales)
	report.RevenueCategories = []categoryTotalView{category("رأس المال", structure.Capital, capital), category("استرجاع ضرائب", structure.VatRefund, refunds),
		category("فلوس البيع المودعة بالحساب", structure.Sale, sales)}
	book := periodicWorkbook(t, report)
	book.want(book.text("استرجاع الضرائب", "D2"), "المبلغ", "the refunds' amount")
	book.want(book.number("استرجاع الضرائب", "D5"), 300.0, "the refunds' total")
	book.want(book.fill("استرجاع الضرائب", "B4"), "B6D7A8", "a new refund is green")
	book.want(book.text(summaryName, "A10"), "رأس المال", "A10")
	book.want(book.formula(summaryName, "B10"), "'رأس المال'!D4", "B10")
	book.want(book.number(summaryName, "B10"), 500.0, "B10")
	book.want(book.text(summaryName, "A11"), "استرجاع ضرائب", "A11")
	book.want(book.formula(summaryName, "B11"), "'استرجاع الضرائب'!D5", "B11")
	book.want(book.number(summaryName, "B11"), 300.0, "B11")
	book.want(book.text(summaryName, "A12"), "فلوس البيع المودعة بالحساب", "A12")
	book.want(strings.ToUpper(book.style(summaryName, "A12").Font.Color), "B7B7B7", "the sales are muted")
	book.want(book.text(summaryName, "A13"), "مجموع الايرادات", "A13")
	book.want(book.number(summaryName, "B13"), 1800.0, "B13")
	book.want(book.number(summaryName, "B15"), 800.0, "the revenue without the sales")
}

func TestR129_TheSheetsFollowTheSummarysReadingOrderAndTheHiddenListsSheetIsLast(t *testing.T) {
	capital := capitalOf(partnerMoney{"سالم", 500})
	fees := item("الرسوم", "الرسوم والفواتير", subgroup("", row(100, "R1")))
	tiles := item("بلاط", "تكلفة التشطيب", subgroup("", row(30, "B1")))
	paint := item("بوية", "تكلفة التشطيب", subgroup("", row(20, "P1")))
	report := reportOf(paint, fees, capital, tiles)
	report.RevenueCategories = []categoryTotalView{category("رأس المال", structure.Capital, capital)}
	report.Categories = []categoryTotalView{category("الرسوم والفواتير", structure.Both, fees), category("تكلفة التشطيب", structure.Both, tiles, paint)}
	book := periodicWorkbook(t, report)
	book.want(strings.Join(book.file.GetSheetList(), "|"), "الخلاصة|رأس المال|الرسوم|بلاط|بوية|القوائم", "the sheets")
	for _, sheet := range book.file.GetSheetList() {
		visible, _ := book.file.GetSheetVisible(sheet)
		book.want(visible, sheet != "القوائم", sheet+" shows")
	}
}

func TestR129_EachSummaryLinesNameOpensItsSheetAndEachSheetLinksBackOutsideWhatPrints(t *testing.T) {
	fees := item("الرسوم", "الرسوم والفواتير", subgroup("", row(100, "R1")))
	tiles := item("بلاط", "تكلفة التشطيب", subgroup("", row(30, "B1")))
	paint := item("بوية", "تكلفة التشطيب", subgroup("", row(20, "P1")))
	report := reportOf(fees, tiles, paint)
	report.Categories = []categoryTotalView{category("الرسوم والفواتير", structure.Both, fees), category("تكلفة التشطيب", structure.Both, tiles, paint)}
	book := periodicWorkbook(t, report)
	book.want(book.text(summaryName, "D10"), "الرسوم والفواتير", "D10")
	book.want(book.link(summaryName, "D10"), "'الرسوم'!A1", "a category of one item opens its sheet")
	book.want(book.link(summaryName, "D11"), "", "a category of more items is no sheet's line")
	book.want(book.link(summaryName, book.find(summaryName, "بوية")), "'بوية'!A1", "an item of the detail block opens its sheet")
	book.want(book.text("الرسوم", "I1"), "← الخلاصة", "the way back")
	book.want(book.link("الرسوم", "I1"), "'الخلاصة'!A1", "the way back opens the summary")
	book.want(printAreaOf(book, "الرسوم"), "'الرسوم'!$A$1:$G$4", "what prints")
}

func printAreaOf(book opened, sheet string) string {
	for _, name := range book.file.GetDefinedName() {
		if name.Name == "_xlnm.Print_Area" && name.Scope == sheet {
			return name.RefersTo
		}
	}
	return ""
}

func TestR129_EverySheetPrintsOnA4OneWideWithTheProjectAndReportNumberAtItsFootAndAcrossThePageFromEightColumns(t *testing.T) {
	wide := append(slices.Clone(purchaseColumns), entries.ReportColumn{Key: "account", Name: "الحساب", DataType: structure.List, IsRequired: true,
		Choices: []entries.ReportChoice{{Name: "حساب المؤسسة"}}})
	narrow := item("الرسوم", "الرسوم والفواتير", subgroup("", row(100, "R1")))
	line := row(30, "B1")
	line.Values = []string{"B1", "2026-09-01", "", "", "", "حساب المؤسسة"}
	broadTable := subgroup("", line)
	broadTable.Columns = wide
	broad := item("بلاط", "التشطيب", broadTable)
	book := periodicWorkbook(t, reportOf(narrow, broad))
	for _, sheet := range book.file.GetSheetList() {
		layout, err := book.file.GetPageLayout(sheet)
		if err != nil || *layout.Size != 9 || *layout.FitToWidth != 1 || *layout.FitToHeight != 0 {
			t.Errorf("%s does not print on A4 one page wide: %+v %v", sheet, layout, err)
		}
		footer, _ := book.file.GetHeaderFooter(sheet)
		book.want(footer.OddFooter, "&L&A&C&P / &N&Rغصون 6 · التقرير 1", sheet+"'s foot")
	}
	portrait, _ := book.file.GetPageLayout("الرسوم")
	landscape, _ := book.file.GetPageLayout("بلاط")
	book.want(*portrait.Orientation, "portrait", "a table of seven columns")
	book.want(*landscape.Orientation, "landscape", "a table of eight columns")
	book.want(printAreaOf(book, "الرسوم"), "'الرسوم'!$A$1:$G$4", "what prints")
}

func TestAC_46_5_ASheetOfOneTableHasNoGrandTotalNorPercentageAndTheSummaryPointsAtItsTotal(t *testing.T) {
	iron := item("حديد", "العظم", subgroup("", row(300, "H1")))
	iron.ShowsTablePercentages = true
	report := reportOf(iron)
	report.Categories = []categoryTotalView{category("الحديد", structure.Both, iron)}
	book := periodicWorkbook(t, report)
	if book.holds("حديد", func(text string) bool {
		return text == "اجمالي الكل" || text == "النسبة" || text == "نسبة الكل"
	}) {
		t.Error("a sheet of one table has a grand total or a percentage")
	}
	book.want(book.formula(summaryName, "E10"), "'حديد'!G4", "E10")
	book.want(book.number(summaryName, "E10"), 300.0, "E10")
}

func TestAC_46_6_UC_109_RasAlMalIsAnItemWhoseTablesAreItsPartnersEachOnesShareUnderTheirTotalAndTheWholeCapitalUnderThem(t *testing.T) {
	capital := capitalOf(partnerMoney{"سالم", 150}, partnerMoney{"خالد", 50})
	report := reportOf(capital)
	report.RevenueCategories = []categoryTotalView{category("رأس المال", structure.Capital, capital)}
	book := periodicWorkbook(t, report)
	const sheet = "رأس المال"
	book.want(book.text(sheet, "A1"), "سالم", "A1")
	book.want(book.text(sheet, "A4"), "الاجمالي", "A4")
	book.want(book.number(sheet, "D4"), 150.0, "D4")
	book.want(book.text(sheet, "A5"), "النسبة", "A5")
	book.want(book.number(sheet, "D5"), 0.75, "D5")
	book.want(book.text(sheet, "A7"), "خالد", "A7")
	book.want(book.number(sheet, "D11"), 0.25, "D11")
	book.want(book.text(sheet, "A13"), "اجمالي الكل", "A13")
	book.want(book.number(sheet, "D13"), 200.0, "D13")
	book.want(book.text(sheet, "A14"), "نسبة الكل", "A14")
	book.want(book.number(sheet, "D14"), 1.0, "D14")
	book.want(book.formula(summaryName, "B10"), "'رأس المال'!D13", "B10")
}

func TestAC_46_8_EachSubgroupWithPurchasesIsItsOwnTableTitledWithItsNameTheGeneralOneWithTheItems(t *testing.T) {
	book := itemWorkbook(t, item("بلاط", "التشطيب", subgroup("", row(100, "B1")), subgroup("الفلة الجنوبية", row(50, "B2"))))
	sheet := book.first()
	book.want(book.text(sheet, "A1"), "بلاط", "A1")
	book.want(book.text(sheet, "A6"), "الفلة الجنوبية", "A6")
	book.want(book.text(sheet, "A11"), "اجمالي الكل", "A11")
	book.want(book.number(sheet, "G11"), 150.0, "G11")
}

func TestAC_46_8_TheSheetPrintsTheItemsTablesInTheOrderTheyWerePutIn(t *testing.T) {
	digging := subgroup("", row(100, "H1"))
	digging.SortOrder = 1
	report := item("الحفر", "العظم", digging)
	contract := payments("مقاول الحفر", 0, true, row(200, ""))
	report.Contracts = []entries.ContractReport{contract}
	book := itemWorkbook(t, report)
	book.want(book.text(book.first(), "A1"), "دفعات مقاول الحفر", "A1")
	book.want(book.text(book.first(), "A6"), "الحفر", "A6")
}

func TestAC_46_9_AnItemThatChoosesPercentagesShowsEachTablesShareUnderItsTotalAndTheWholeUnderTheGrandTotal(t *testing.T) {
	tiles := item("بلاط", "التشطيب", subgroup("", row(300, "B1")), subgroup("الفلة الجنوبية", row(100, "B2")))
	tiles.ShowsTablePercentages = true
	book := itemWorkbook(t, tiles)
	sheet := book.first()
	book.want(book.text(sheet, "A5"), "النسبة", "A5")
	book.want(book.number(sheet, "G5"), 0.75, "G5")
	book.want(book.text(sheet, "A11"), "النسبة", "A11")
	book.want(book.number(sheet, "G11"), 0.25, "G11")
	book.want(book.text(sheet, "A13"), "اجمالي الكل", "A13")
	book.want(book.number(sheet, "G13"), 400.0, "G13")
	book.want(book.text(sheet, "A14"), "نسبة الكل", "A14")
	book.want(book.number(sheet, "G14"), 1.0, "G14")
}

func TestAC_46_10_ACategoryOfOneItemIsOneLineAndOnlyACategoryOfMoreItemsHasADetailTable(t *testing.T) {
	fees := item("الرسوم", "الرسوم والفواتير", subgroup("", row(100, "R1")), subgroup("التأمين", row(20, "R2")))
	tiles := item("بلاط", "تكلفة التشطيب", subgroup("", row(30, "B1")))
	paint := item("بوية", "تكلفة التشطيب", subgroup("", row(20, "P1")))
	report := reportOf(fees, tiles, paint)
	report.Categories = []categoryTotalView{category("الرسوم والفواتير", structure.Both, fees), category("تكلفة التشطيب", structure.Both, tiles, paint)}
	book := periodicWorkbook(t, report)
	book.want(book.formula(summaryName, "E10"), "'الرسوم'!G11", "E10")
	book.want(book.number(summaryName, "E10"), 120.0, "E10")
	details := 0
	rows, _ := book.file.GetRows(summaryName)
	for _, cells := range rows {
		for _, text := range cells {
			if strings.HasPrefix(text, "تفاصيل ") {
				details++
				book.want(text, "تفاصيل تكلفة التشطيب:", "the detail block")
			}
		}
	}
	book.want(details, 1, "detail blocks")
}

func TestAC_46_11_EveryColumnFitsItsContentAndTextPastFortyFiveCharactersWraps(t *testing.T) {
	long := strings.TrimSpace(strings.Repeat("دفعات المكتب الهندسي ", 6))
	line := entries.ReportRow{EntryID: uuid.New(), Date: moment.Date{Time: time.Date(2025, 7, 5, 0, 0, 0, 0, time.UTC)}, Amount: amountOf(590400),
		Values: []string{long, "2025-07-05", "", "", ""}}
	content, err := workbookOf("غصون 6", item("الرسوم", "الرسوم والفواتير", subgroup("", line)), nil)
	book := open(t, content, err)
	sheet := book.first()
	width, _ := book.file.GetColWidth(sheet, "B")
	book.want(width, 45.0, "a long text's column")
	book.want(book.style(sheet, "B3").Alignment.WrapText, true, "a long text wraps")
	dateWidth, _ := book.file.GetColWidth(sheet, "C")
	amountWidth, _ := book.file.GetColWidth(sheet, "G")
	if dateWidth < 10 || amountWidth < 10 {
		t.Errorf("a date and an amount show whole: %v, %v", dateWidth, amountWidth)
	}
	if strings.Contains(string(content), "customHeight") {
		t.Error("a row keeps a height of its own")
	}
}

func TestFindings131_TheContractsWorkbookIsOneStatementTableOfWhatEachContractHasLeft(t *testing.T) {
	content, err := contractsWorkbookOf("غصون 6", []contracts.View{{SupplierName: "شركة النخيل", ItemName: "الحفر", Value: amountOf(1000), Remaining: countedOf(1000)}})
	book := open(t, content, err)
	sheet := book.first()
	book.want(book.text(sheet, "A1"), "العقود", "A1")
	var headers []string
	for column := 2; column <= 6; column++ {
		name, _ := excelize.CoordinatesToCellName(column, 2)
		headers = append(headers, book.text(sheet, name))
	}
	book.want(strings.Join(headers, "|"), "المقاول|البند|قيمة العقد|المدفوع|المتبقي", "the headers")
	book.want(book.number(sheet, "D3"), 1000.0, "D3")
	book.want(book.number(sheet, "E3"), 0.0, "E3")
	book.want(book.number(sheet, "F3"), 1000.0, "F3")
	book.want(book.text(sheet, "A4"), "الاجمالي", "A4")
	book.want(book.formula(sheet, "F4"), "SUM(F3:F3)", "F4")
	book.want(book.text(sheet, "A2"), "", "A2")
	book.want(book.number(sheet, "A3"), 1.0, "A3")
}

func TestAC_54_6_G6_TheWorkbookShowsTheNumberAndAReportIssuedBeforeEntriesHadOneLeavesItEmpty(t *testing.T) {
	numbered := row(100, "بلاط")
	numbered.Number = 312
	book := itemWorkbook(t, item("البلاط", "التشطيب", subgroup("", numbered, row(50, "حديد"))))
	sheet := book.first()
	book.want(book.text(sheet, "A2"), "رقم القيد", "A2")
	book.want(book.number(sheet, "A3"), 312.0, "A3")
	book.want(book.text(sheet, "A4"), "", "a row of a report issued before entries had a number shows none")
}

func TestRow267_TheVatWorkbookIsInExcelsOwnLookCalibri11WithNoPageSetup(t *testing.T) {
	quarter := entries.VatQuarter{Start: time.Date(2026, 10, 1, 0, 0, 0, 0, time.UTC), End: time.Date(2026, 12, 31, 0, 0, 0, 0, time.UTC),
		Purchases: []entries.VatRow{{Date: moment.Date{Time: time.Date(2026, 11, 5, 0, 0, 0, 0, time.UTC)}, Party: "شركة النخيل", VatNumber: "300000000000003",
			Description: "حديد", Total: amountOf(115)}}}
	content, err := vatWorkbookOf([]entries.VatQuarter{quarter}, "15")
	book := open(t, content, err)
	sheet := book.first()
	if font := book.style(sheet, "A1").Font; font.Family != "Calibri" || font.Size != 11 {
		t.Errorf("the VAT workbook's font is %s %v", font.Family, font.Size)
	}
	if props, _ := book.file.GetSheetProps(sheet); props.FitToPage != nil && *props.FitToPage {
		t.Error("the VAT workbook is fitted to a page")
	}
}
