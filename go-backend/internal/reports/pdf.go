// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"context"
	"errors"
	"fmt"
	"net/http"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// The reports as PDFs (UC-054, FR-33, FR-45, PeriodicReportFiles): the periodic report's summary laid out as report 23, then the capital and every
// item drawn from the very tables its workbook writes (findings row 130); an item's or a contract's report alone; and the contracts with what each
// has left. Words are the statement's own Arabic, as report 23 prints them.

const pdfType = "application/pdf"

// periodicDocumentOf is the periodic report as a PDF's pages: the summary, then a page per item, رأس المال among them.
func periodicDocumentOf(report Snapshot, words waitingWords, zone string) statement.Document {
	document := statement.Document{Pages: []statement.Page{summaryPage(report, words, zone)}}
	for _, item := range report.Items {
		document.Pages = append(document.Pages, statement.PdfPage(sheetOf(item, nil)))
	}
	return document
}

// summaryPage is the report's summary as report 23 lays it out: the accounts, the expenses against the previous report's, the revenue, the expenses
// by category, the surplus and the reconciliation, what waits, and each category's items that hold anything.
func summaryPage(report Snapshot, words waitingWords, zone string) statement.Page {
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

// periodicPdf is the report as a PDF laid out as report 23 (UC-054), its capital showing only the partners the caller may see (redix-api #131 D1,
// BR-31); 404 for a number not issued.
func (s Routes) periodicPdf(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	number := web.Int(r, "number")
	file, found, err := PeriodicPdfOf(r.Context(), s.database.Queries(), s.typst, web.ID(r, "projectId"), number, caller)
	if err == nil && !found {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	s.sendFile(w, r, file, pdfType, PeriodicFileName(number, true), err)
}

// PeriodicPdfOf is projectID's report numbered number as a PDF laid out as report 23 (UC-054), its capital showing only the partners caller may
// see (redix-api #131 D1, BR-31), as periodicPdf answers it: the rule both doors call, the app's route and the web's (redix-api #171). found is
// false for a number not issued; the error is the database's or typst's.
func PeriodicPdfOf(ctx context.Context, read *store.Queries, typst statement.Typst, projectID uuid.UUID, number int64, caller access.Caller) ([]byte, bool, error) {
	report, found, err := issuedOf(ctx, read, projectID, number)
	if err != nil || !found {
		return nil, found, err
	}
	words, wordsErr := waitingWordsOf(ctx, read)
	zone, zoneErr := zones.Of(ctx, read, projectID)
	if err := errors.Join(wordsErr, zoneErr); err != nil {
		return nil, true, err
	}
	file, err := typst.Pdf(periodicDocumentOf(forFilesOf(report, caller, projectID), words, zone))
	return file, true, err
}

// PeriodicFileName is the name the report numbered number's file is handed under, its PDF or its workbook.
func PeriodicFileName(number int64, isPdf bool) string {
	if isPdf {
		return fmt.Sprintf("report-%d.pdf", number)
	}
	return fmt.Sprintf("report-%d.xlsx", number)
}

// waitingWords are the labels of what waits, from the translations.
func waitingWordsOf(ctx context.Context, read *store.Queries) (waitingWords, error) {
	expenses, err := read.ArabicWord(ctx, "reports.pendingExpenses")
	revenue, revenueErr := read.ArabicWord(ctx, "reports.pendingRevenue")
	if err == nil {
		err = revenueErr
	}
	return waitingWords{expenses: expenses, revenue: revenue}, err
}
