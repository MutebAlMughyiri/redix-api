package reports

import (
	"fmt"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// The reports as PDFs (UC-054, FR-33, FR-45, PeriodicReportFiles): the periodic report's summary laid out as report 23, then the capital and every
// item drawn from the very tables its workbook writes (findings row 130); an item's or a contract's report alone; and the contracts with what each
// has left. Words are the statement's own Arabic, as report 23 prints them.

const pdfType = "application/pdf"

// periodicDocumentOf is the periodic report as a PDF's pages: the summary, then a page per item, رأس المال among them.
func periodicDocumentOf(report snapshot, words waitingWords, zone string) statement.Document {
	document := statement.Document{Pages: []statement.Page{summaryPage(report, words, zone)}}
	for _, item := range report.Items {
		document.Pages = append(document.Pages, statement.PdfPage(sheetOf(item, nil)))
	}
	return document
}

// summaryPage is the report's summary as report 23 lays it out: the accounts, the expenses against the previous report's, the revenue, the expenses
// by category, the surplus and the reconciliation, what waits, and each category's items that hold anything.
func summaryPage(report snapshot, words waitingWords, zone string) statement.Page {
	issuedAt := report.IssuedAt.In(zones.Location(zone))
	page := statement.Page{Title: fmt.Sprintf("%s · تقرير رقم %d · %s", report.ProjectName, report.Number, issuedAt.Format("02.01.2006 15:04")), TitleSize: 14}
	add := func(blocks ...statement.Block) { page.Blocks = append(page.Blocks, blocks...) }
	add(statement.Heading("أرصدة الحسابات", 6))
	for _, account := range report.Accounts {
		add(statement.AmountLine(account.Name, account.Balance.Number(), false))
	}
	add(statement.AmountLine("إجمالي الحسابات", report.AccountsTotal.Number(), true))
	add(statement.Heading("المصروفات", 6), statement.AmountLine("مصروفات التقرير السابق", report.PreviousExpenses.Number(), false),
		statement.AmountLine("مصروفات التقرير الحالي", report.Expenses.Number(), false), statement.AmountLine("الفرق", report.ExpensesDifference.Number(), true))
	add(statement.Heading("الإيرادات", 6))
	for _, category := range report.RevenueCategories {
		add(statement.AmountLine(category.Name, category.Total.Number(), false))
	}
	add(statement.AmountLine("مجموع الإيرادات", report.Revenue.Number(), true), statement.AmountLine("المجموع بدون البيع", report.RevenueWithoutSales.Number(), false))
	add(statement.Heading("المصروفات حسب التصنيف", 6))
	for _, category := range report.Categories {
		add(statement.AmountLine(category.Name, category.Total.Number(), false))
	}
	add(statement.AmountLine("مجموع المصروفات", report.Expenses.Number(), true))
	add(statement.Heading("الفائض", 6), statement.AmountLine("الفائض", report.Surplus.Number(), true),
		statement.AmountLine("الفائض بدون البيع", report.SurplusWithoutSales.Number(), false),
		statement.AmountLine("فرق المطابقة", report.ReconciliationDifference.Number(), true))
	if report.PendingCount > 0 {
		add(statement.AmountLine(fmt.Sprintf("%s (%d)", words.expenses, report.PendingCount), report.PendingAmount.Number(), false))
	}
	if report.PendingRevenueCount > 0 {
		add(statement.AmountLine(fmt.Sprintf("%s (%d)", words.revenue, report.PendingRevenueCount), report.PendingRevenueAmount.Number(), false))
	}
	for _, category := range report.Categories {
		var lines []statement.Block
		for _, item := range category.Items {
			if item.Total.Number() != 0 {
				lines = append(lines, statement.AmountLine(item.Name, item.Total.Number(), false))
			}
		}
		if len(lines) > 0 {
			add(statement.Heading("تفاصيل "+category.Name, 6))
			add(lines...)
		}
	}
	return page
}

// contractsDocumentOf is the contracts report as a PDF: each contract's value, paid, remaining, and an alert where more was paid (FR-31, FR-33).
func contractsDocumentOf(projectName string, list []contracts.View) statement.Document {
	table := statement.Block{Kind: "table", Gap: 6, Widths: []statement.TableWidth{{Shares: 3}, {Shares: 2}, {Shares: 2}, {Shares: 2}, {Shares: 2}},
		Header: []string{"المقاول", "البند", "قيمة العقد", "المدفوع", "المتبقي"}, Rows: []statement.PdfRow{}}
	for _, contract := range list {
		contractor, value, remaining := contract.SupplierName, statement.Money(contract.Value), statement.Money(contract.Remaining.Number())
		if contract.IsOverrun {
			contractor += " · متجاوز"
		}
		if contract.IsOpenValue {
			value, remaining = "مفتوحة", ""
		}
		table.Rows = append(table.Rows, statement.PdfRow{Cells: []statement.PdfCell{{Text: contractor, Strong: contract.IsOverrun}, {Text: contract.ItemName},
			{Text: value}, {Text: statement.Money(contract.Paid.Number())}, {Text: remaining, Strong: contract.IsOverrun}}})
	}
	return statement.Document{Pages: []statement.Page{{Title: projectName + " · العقود والمتبقي", TitleSize: 13, Blocks: []statement.Block{table}}}}
}

// periodicPdf is the report as a PDF laid out as report 23 (UC-054), its capital showing only the partners the caller may see (redix-api #131 D1,
// BR-31); 404 for a number not issued.
func (s Routes) periodicPdf(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	report, found, err := s.issued(r, web.Int(r, "number"))
	if err == nil && !found {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	words, wordsErr := s.waitingWords(r)
	zone, zoneErr := zones.Of(r.Context(), s.database.Queries(), web.ID(r, "projectId"))
	if err != nil || wordsErr != nil || zoneErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	file, err := s.typst.Pdf(periodicDocumentOf(forFilesOf(report, caller, web.ID(r, "projectId")), words, zone))
	s.sendFile(w, r, file, pdfType, fmt.Sprintf("report-%d.pdf", report.Number), err)
}

// contractsPdf is the contracts report as a PDF.
func (s Routes) contractsPdf(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	if name, list, found := s.contractsOf(w, r); found {
		file, err := s.typst.Pdf(contractsDocumentOf(name, list))
		s.sendFile(w, r, file, pdfType, "contracts.pdf", err)
	}
}

// waitingWords are the labels of what waits, from the translations.
func (s Routes) waitingWords(r *http.Request) (waitingWords, error) {
	read := s.database.Queries()
	expenses, err := read.ArabicWord(r.Context(), "reports.pendingExpenses")
	revenue, revenueErr := read.ArabicWord(r.Context(), "reports.pendingRevenue")
	if err == nil {
		err = revenueErr
	}
	return waitingWords{expenses: expenses, revenue: revenue}, err
}
