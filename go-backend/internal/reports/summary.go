// Package reports keeps what is read of a project's figures: its summary (UC-049, FR-34) and the reports built from it (Reports; redix-api
// #140).
package reports

import (
	"context"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// Routes reads a project's figures.
type Routes struct {
	database db.DB
}

// Map routes the summary, for whoever reads the project.
func Map(router *web.Router, gate *access.Gate, database db.DB) {
	s := Routes{database: database}
	router.Handle("GET", "/projects/{projectId:guid}/summary", gate.Needs(access.ReadProject, s.summary))
}

// Waiting is what waits for approval, as two figures (WaitingFigures, findings row 147): expenses, which lower the surplus once approved, and
// revenue with the partners' money, which raises it.
type Waiting struct {
	ExpensesCount  int64       `json:"expensesCount"`
	ExpensesAmount fixed.Total `json:"expensesAmount"`
	RevenueCount   int64       `json:"revenueCount"`
	RevenueAmount  fixed.Total `json:"revenueAmount"`
}

// WaitingOf is what waits in the project now; the one owner of "what waits" for the summary and the periodic report.
func WaitingOf(ctx context.Context, q *store.Queries, projectID uuid.UUID) (Waiting, error) {
	rows, err := q.WaitingByKind(ctx, projectID)
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

// ItemTotals is what each item of the project came to (ItemTotals, BR-02, BR-05); an item with nothing is not among them, and comes to nothing.
func ItemTotals(ctx context.Context, q *store.Queries, projectID uuid.UUID) (map[uuid.UUID]fixed.Total, error) {
	rows, err := q.ItemTotalsOf(ctx, projectID)
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

// summary is revenue, expenses, the surplus, the reconciliation difference, what waits, each category's total with its items' - the capital among
// the revenue (redix-api #109) - and the completion (UC-049, UC-063), read on every request so an approval shows at once (AC-34.1).
func (s Routes) summary(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	figures, err := accounts.FiguresOf(r.Context(), read, projectID)
	at, difference, reconciledErr := accounts.Reconciled(r.Context(), read, projectID)
	waiting, waitingErr := WaitingOf(r.Context(), read, projectID)
	totals, totalsErr := ItemTotals(r.Context(), read, projectID)
	categories, categoriesErr := read.CategoriesOf(r.Context(), projectID)
	items, itemsErr := read.ItemsOf(r.Context(), projectID)
	for _, failed := range []error{err, reconciledErr, waitingErr, totalsErr, categoriesErr, itemsErr} {
		if failed != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
	}
	side := func(isRevenue bool) []categoryTotalView {
		lines := []categoryTotalView{}
		for _, category := range categories {
			line := categoryTotalView{ID: category.ID, Name: category.Name, Items: []itemTotalView{}}
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
	web.JSON(w, r, http.StatusOK, struct {
		Revenue                  fixed.Total         `json:"revenue"`
		Expenses                 fixed.Total         `json:"expenses"`
		Surplus                  fixed.Total         `json:"surplus"`
		WasReconciled            bool                `json:"wasReconciled"`
		ReconciliationDifference fixed.Total         `json:"reconciliationDifference"`
		Waiting                  Waiting             `json:"waiting"`
		CompletionPercent        fixed.Written       `json:"completionPercent"`
		Categories               []categoryTotalView `json:"categories"`
		RevenueCategories        []categoryTotalView `json:"revenueCategories"`
	}{figures.Revenue(), figures.Expenses, figures.Surplus(), !at.IsZero(), difference, waiting,
		fixed.Percent(figures.Expenses.Number(), figures.Revenue().Number(), 2, false), side(false), side(true)})
}
