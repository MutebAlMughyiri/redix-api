// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"bytes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"os"
	"slices"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/ledongthuc/pdf"
)

// The PDFs as .NET's ReportFilesTests checked them: their pages, and every figure their workbook shows, read back from the PDF Typst made.

// typst makes the tests' PDFs with REDIX_Typst__Path, as the server reads it, or typst on the path.
func typst() statement.Typst {
	if path := os.Getenv("REDIX_Typst__Path"); path != "" {
		return statement.Typst{Path: path}
	}
	return statement.Typst{Path: "typst"}
}

// pagesOf is each page's text with no spacing, so a figure Typst set apart from its brackets still reads whole.
func pagesOf(t *testing.T, document statement.Document) []string {
	t.Helper()
	content, err := typst().Pdf(document)
	if err != nil {
		t.Fatal(err)
	}
	reader, err := pdf.NewReader(bytes.NewReader(content), int64(len(content)))
	if err != nil {
		t.Fatal(err)
	}
	var pages []string
	for number := 1; number <= reader.NumPage(); number++ {
		text, err := reader.Page(number).GetPlainText(nil)
		if err != nil {
			t.Fatal(err)
		}
		pages = append(pages, strings.Join(strings.Fields(text), ""))
	}
	return pages
}

func TestAC_45_1_ThePdfHasThePeriodicSummaryThenAPagePerItem(t *testing.T) {
	capital := capitalOf(partnerMoney{"سالم", 500})
	report := reportOf(capital, item("الرسوم", "الرسوم والفواتير", subgroup("", row(100, "R1"))), item("بلاط", "التشطيب", subgroup("", row(30, "B1"))))
	pages := pagesOf(t, periodicDocumentOf(report, waitingWords{}, zones.Riyadh))
	if len(pages) != 4 {
		t.Errorf("the PDF has %d pages, want the summary and three items", len(pages))
	}
}

func TestRow_130_EveryFigureOfTheWorkbooksItemSheetsRasAlMalAmongThemIsOnThePeriodicReportsPdf(t *testing.T) {
	capital := capitalOf(partnerMoney{"سالم", 1500.25}, partnerMoney{"خالد", 499.75})
	tiles := item("بلاط", "التشطيب", subgroup("", row(1234.5, "B1"), newRow(70, "B2")), subgroup("الفلة الجنوبية", row(310, "B3")))
	tiles.Contracts = []entries.ContractReport{payments("مؤسسة الدهان", 1000, false, row(400, ""), outsideRow(90))}
	report := reportOf(capital, tiles)
	pages := pagesOf(t, periodicDocumentOf(report, waitingWords{}, zones.Riyadh))
	for index, sheetItem := range report.Items {
		sheet := sheetOf(sheetItem, nil)
		page := pages[index+1]
		for _, table := range sheet.Tables {
			for _, line := range table.Rows {
				if figure := statement.Money(amountOf(line.Amount)); !strings.Contains(page, figure) {
					t.Errorf("%s's page lacks %s", sheet.Name, figure)
				}
			}
		}
	}
	for _, figure := range []string{"1,500.25", "499.75", "2,000.00", "75.01%", "24.99%", "100.00%", "1,304.50", "490.00", "600.00", "2,104.50"} {
		if !slices.ContainsFunc(pages, func(page string) bool { return strings.Contains(page, figure) }) {
			t.Errorf("the PDF lacks %s", figure)
		}
	}
}

func TestR129_R4_ThePdfPrintsTheColumnsTheSheetPrints(t *testing.T) {
	tiles := item("بلاط", "التشطيب", subgroup("", row(100, "B1")))
	sheet := sheetOf(tiles, nil)
	page := statement.PdfPage(sheet)
	var printed []string
	for _, block := range page.Blocks {
		if block.Kind == "table" {
			printed = block.Header
		}
	}
	want := []string{"رقم القيد"}
	for _, column := range sheet.Tables[0].Columns {
		want = append(want, column.Header)
	}
	want = append(want, sheet.Tables[0].Amount.Header)
	if !slices.Equal(printed, want) {
		t.Errorf("the PDF's table is headed %q, the sheet's %q", printed, want)
	}
}

func TestAC_54_6_G6_ThePdfShowsTheEntrysNumber(t *testing.T) {
	numbered := row(100, "بلاط")
	numbered.Number = 312
	pages := pagesOf(t, statement.Document{Pages: []statement.Page{statement.PdfPage(sheetOf(item("البلاط", "التشطيب", subgroup("", numbered)), nil))}})
	if !strings.Contains(pages[0], "312") {
		t.Errorf("the PDF lacks the entry's number: %s", pages[0])
	}
}

func TestFR31_TheContractsPdfShowsEachContractsValuePaidAndWhatIsLeftAndAnOverrun(t *testing.T) {
	list := []contracts.View{{SupplierName: "مؤسسة الدهان", ItemName: "بويه", Value: amountOf(1000), Paid: countedOf(1200), Remaining: countedOf(-200), IsOverrun: true},
		{SupplierName: "الحارس", ItemName: "الحراسة", IsOpenValue: true, Paid: countedOf(300)}}
	pages := pagesOf(t, contractsDocumentOf("مشروع A", list))
	for _, figure := range []string{"1,000.00", "1,200.00", "(200.00)", "300.00"} {
		if !strings.Contains(pages[0], figure) {
			t.Errorf("the contracts PDF lacks %s", figure)
		}
	}
	document := contractsDocumentOf("مشروع A", list)
	rows := document.Pages[0].Blocks[0].Rows
	if !strings.HasSuffix(rows[0].Cells[0].Text, " · متجاوز") || !rows[0].Cells[0].Strong || rows[1].Cells[2].Text != "مفتوحة" || rows[1].Cells[4].Text != "" {
		t.Errorf("an overrun is not marked, or an open value shows a figure: %+v", rows)
	}
}

func TestAC_45_2_TheSummaryPageCarriesTheReportsFigures(t *testing.T) {
	report := reportOf(item("بلاط", "التشطيب", subgroup("", row(115, "B1"))))
	report.Categories = []categoryTotalView{category("التشطيب", structure.Both, report.Items[0])}
	report.Accounts = []reportAccount{{Name: "حساب المؤسسة", Balance: countedOf(885)}}
	report.AccountsTotal, report.PreviousExpenses, report.ExpensesDifference = countedOf(885), countedOf(15), countedOf(100)
	report.PendingCount, report.PendingAmount = 2, countedOf(7.5)
	document := periodicDocumentOf(report, waitingWords{expenses: "قيود بانتظار الاعتماد"}, zones.Riyadh)
	pages := pagesOf(t, document)
	for _, figure := range []string{"885.00", "15.00", "115.00", "100.00", "7.50"} {
		if !strings.Contains(pages[0], figure) {
			t.Errorf("the summary lacks %s", figure)
		}
	}
	if !slices.ContainsFunc(document.Pages[0].Blocks, func(block statement.Block) bool {
		return block.Text == "قيود بانتظار الاعتماد (2)" && block.Figure == "7.50"
	}) {
		t.Error("the summary does not say how many entries wait, and for how much")
	}
}
