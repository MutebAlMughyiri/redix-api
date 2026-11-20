// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package reports keeps what is read of a project's figures: its summary (UC-049, FR-34) and the reports built from it (Reports; redix-api
// #140).
package reports

import (
	"context"
	"errors"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// Routes reads a project's figures, and issues its periodic reports.
type Routes struct {
	database db.DB
	writer   changes.Writer
	links    attachments.Links
	typst    statement.Typst
}

// Map routes the summary, for whoever reads the project, and the periodic reports: read by whoever holds SeeReports, issued by whoever holds
// GenerateReports (redix-api #131 D2).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock, links attachments.Links, typst statement.Typst) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}, links: links, typst: typst}
	const project = "/projects/{projectId:guid}"
	const reports = project + "/periodic-reports"
	files := func(handler access.Handler) http.HandlerFunc { return gate.Needs(access.GenerateReports, handler) }
	router.Handle("GET", "/projects/{projectId:guid}/summary", gate.Needs(access.ReadProject, s.summary))
	router.Handle("GET", reports, gate.Needs(access.SeeReports, access.Whole(s.periodicReports)))
	router.Handle("GET", reports+"/next", gate.Needs(access.SeeReports, access.Whole(s.nextPeriodicReport)))
	router.Handle("GET", reports+"/{number:int}", gate.Needs(access.SeeReports, access.Whole(s.periodicReport)))
	router.Handle("POST", reports, gate.Changes(access.GenerateReports, access.Whole(s.issue)))
	router.Handle("GET", reports+"/{number:int}/xlsx", files(access.Whole(s.periodicWorkbook)))
	router.Handle("GET", reports+"/{number:int}/pdf", files(access.Whole(s.periodicPdf)))
	router.Handle("GET", project+"/reports/items/{itemId:guid}/xlsx", files(s.itemFile(false)))
	router.Handle("GET", project+"/reports/items/{itemId:guid}/pdf", files(s.itemFile(true)))
	router.Handle("GET", project+"/reports/contracts/{contractId:guid}/xlsx", files(s.contractFile(false)))
	router.Handle("GET", project+"/reports/contracts/{contractId:guid}/pdf", files(s.contractFile(true)))
	router.Handle("GET", project+"/reports/contracts/xlsx", files(access.Whole(s.contractsWorkbook)))
	router.Handle("GET", project+"/reports/contracts/pdf", files(access.Whole(s.contractsPdf)))
	router.Handle("GET", project+"/vat-statement/xlsx", files(access.Whole(s.vatWorkbook)))
}

// Waiting is what waits for approval, as two figures (WaitingFigures, findings row 147): expenses, which lower the surplus once approved, and
// revenue with the partners' money, which raises it.
type Waiting struct {
	ExpensesCount  int64       `json:"expensesCount"`
	ExpensesAmount fixed.Total `json:"expensesAmount"`
	RevenueCount   int64       `json:"revenueCount"`
	RevenueAmount  fixed.Total `json:"revenueAmount"`
}

// WaitingOf is what waits in the project now, but the entries of the tables in unseen (#106 R6); the one owner of "what waits" for the summary
// and the periodic report.
func WaitingOf(ctx context.Context, q *store.Queries, projectID uuid.UUID, unseen string) (Waiting, error) {
	rows, err := q.WaitingByKind(ctx, store.WaitingByKindParams{ProjectID: projectID, Unseen: unseen})
	var waiting Waiting
	for _, row := range rows {
		amount := fixed.Counted(fixed.Number(row.Total))
		switch row.Kind {
		case 0, 1:
			waiting.ExpensesCount += row.Entries
			waiting.ExpensesAmount = waiting.ExpensesAmount.Plus(amount)
		case 3:
			waiting.RevenueCount += row.Entries
			waiting.RevenueAmount = waiting.RevenueAmount.Minus(amount)
		default:
			waiting.RevenueCount += row.Entries
			waiting.RevenueAmount = waiting.RevenueAmount.Plus(amount)
		}
	}
	return waiting, err
}

// ItemTotals is what each item of the project came to (ItemTotals, BR-02, BR-05), but the entries of the tables in unseen (#106 R6); an item with
// nothing is not among them, and comes to nothing.
func ItemTotals(ctx context.Context, q *store.Queries, projectID uuid.UUID, unseen string) (map[uuid.UUID]fixed.Total, error) {
	rows, err := q.ItemTotalsOf(ctx, store.ItemTotalsOfParams{ProjectID: projectID, Unseen: unseen})
	totals := map[uuid.UUID]fixed.Total{}
	for _, row := range rows {
		totals[row.ItemID] = fixed.Counted(fixed.Number(row.Total))
	}
	return totals, err
}

type itemTotalView struct {
	ID    uuid.UUID          `json:"id"`
	Name  string             `json:"name"`
	Total fixed.Total        `json:"total"`
	Type  structure.ItemType `json:"type"`
}

type categoryTotalView struct {
	ID    uuid.UUID       `json:"id"`
	Name  string          `json:"name"`
	Total fixed.Total     `json:"total"`
	Items []itemTotalView `json:"items"`
}

// categoriesOf is each category's total with its items', the expenses' side and the revenue's, the capital among the revenue (redix-api #109): the
// one owner of the categories the summary and a periodic report show.
func categoriesOf(ctx context.Context, q *store.Queries, projectID uuid.UUID, unseen string) ([]categoryTotalView, []categoryTotalView, error) {
	totals, err := ItemTotals(ctx, q, projectID, unseen)
	categories, categoriesErr := q.CategoriesOf(ctx, projectID)
	items, itemsErr := q.ItemsOf(ctx, projectID)
	if err := errors.Join(err, categoriesErr, itemsErr); err != nil {
		return nil, nil, err
	}
	side := func(isRevenue bool) []categoryTotalView {
		var lines []categoryTotalView
		for _, category := range categories {
			line := categoryTotalView{ID: category.ID, Name: category.Name}
			for _, item := range items {
				if item.CategoryID == category.ID && structure.ItemType(item.Type).IsRevenue() == isRevenue {
					line.Items = append(line.Items, itemTotalView{ID: item.ID, Name: item.Name, Total: totals[item.ID], Type: structure.ItemType(item.Type)})
					line.Total = line.Total.Plus(totals[item.ID])
				}
			}
			if len(line.Items) > 0 {
				lines = append(lines, line)
			}
		}
		return lines
	}
	return side(false), side(true), nil
}

// summaryView is what GET /projects/{projectId}/summary answers (UC-049).
type summaryView struct {
	Revenue                  fixed.Total         `json:"revenue"`
	Expenses                 fixed.Total         `json:"expenses"`
	Surplus                  fixed.Total         `json:"surplus"`
	WasReconciled            bool                `json:"wasReconciled"`
	ReconciliationDifference fixed.Total         `json:"reconciliationDifference"`
	Waiting                  Waiting             `json:"waiting"`
	CompletionPercent        fixed.Written       `json:"completionPercent"`
	Categories               []categoryTotalView `json:"categories"`
	RevenueCategories        []categoryTotalView `json:"revenueCategories"`
}

// summary is revenue, expenses, the surplus, the reconciliation difference, what waits, each category's total with its items' - the capital among
// the revenue (redix-api #109) - and the completion (UC-049, UC-063), read on every request so an approval shows at once (AC-34.1).
func (s Routes) summary(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	unseen := caller.Access.Unseen(projectID)
	figures, err := accounts.FiguresOf(r.Context(), read, projectID, unseen)
	at, difference, reconciledErr := accounts.Reconciled(r.Context(), read, projectID)
	waiting, waitingErr := WaitingOf(r.Context(), read, projectID, unseen)
	expenses, revenue, categoriesErr := categoriesOf(r.Context(), read, projectID, unseen)
	if errors.Join(err, reconciledErr, waitingErr, categoriesErr) != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, summaryView{figures.Revenue(), figures.Expenses, figures.Surplus(), !at.IsZero(), difference, waiting,
		fixed.Percent(figures.Expenses.Number(), figures.Revenue().Number(), 2, false), expenses, revenue})
}
