package reports

import (
	"encoding/json"
	"errors"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// The numbered periodic report that replaces the manual statement (UC-053, FR-29, PeriodicReportsEndpoints): issued once from what is approved,
// and kept as issued, every figure, so nothing approved later changes it (BR-26). Its PDF (UC-054) and workbook (UC-055) are still on .NET.

type reportAccount struct {
	Name    string               `json:"name"`
	Type    accounts.AccountType `json:"type"`
	Balance fixed.Total          `json:"balance"`
}

type reportPartner struct {
	Name         string        `json:"name"`
	ID           uuid.UUID     `json:"id"`
	Balance      fixed.Total   `json:"balance"`
	SharePercent fixed.Written `json:"sharePercent"`
}

// snapshot is everything a periodic report shows, as it was when issued (PeriodicReportSnapshot, BRS section 6.1). It is kept as the JSON it
// answers with, and read back with every number in the scale it was written with; a report .NET kept, its kinds written as numbers, reads the same.
type snapshot struct {
	ProjectName              string               `json:"projectName"`
	Number                   int64                `json:"number"`
	IssuedAt                 moment.Moment        `json:"issuedAt"`
	PreviousExpenses         fixed.Total          `json:"previousExpenses"`
	Expenses                 fixed.Total          `json:"expenses"`
	Capital                  fixed.Total          `json:"capital"`
	VatRefunds               fixed.Total          `json:"vatRefunds"`
	Sales                    fixed.Total          `json:"sales"`
	OtherRevenue             fixed.Total          `json:"otherRevenue"`
	Surplus                  fixed.Total          `json:"surplus"`
	SurplusWithoutSales      fixed.Total          `json:"surplusWithoutSales"`
	Categories               []categoryTotalView  `json:"categories"`
	Accounts                 []reportAccount      `json:"accounts"`
	AccountsTotal            fixed.Total          `json:"accountsTotal"`
	ReconciliationDifference fixed.Total          `json:"reconciliationDifference"`
	PendingCount             int64                `json:"pendingCount"`
	PendingAmount            fixed.Total          `json:"pendingAmount"`
	CapitalTotal             fixed.Total          `json:"capitalTotal"`
	Partners                 []reportPartner      `json:"partners"`
	Items                    []entries.ItemReport `json:"items"`
	RevenueCategories        []categoryTotalView  `json:"revenueCategories"`
	PendingRevenueCount      int64                `json:"pendingRevenueCount"`
	PendingRevenueAmount     fixed.Total          `json:"pendingRevenueAmount"`
	ExpensesDifference       fixed.Total          `json:"expensesDifference"`
	Revenue                  fixed.Total          `json:"revenue"`
	RevenueWithoutSales      fixed.Total          `json:"revenueWithoutSales"`
}

// reportRow is a report issued (PeriodicReportRowView).
type reportRow struct {
	Number   int64         `json:"number"`
	IssuedAt moment.Moment `json:"issuedAt"`
	Expenses fixed.Total   `json:"expenses"`
}

// latestOf is the project's latest report; while none was issued, one numbered 0, of no expenses, issued before anything was approved.
func latestOf(c changes.Change) (store.LatestPeriodicReportRow, error) {
	latest, err := c.Q.LatestPeriodicReport(c.Ctx, c.ProjectID)
	for _, report := range latest {
		return report, err
	}
	return store.LatestPeriodicReportRow{}, err
}

// snapshotOf is the report numbered number, issued now from what is approved: what was approved since the previous one is marked new, whatever
// its date, and the expenses' difference starts from the previous report's (BR-09, BR-26). Every item with a total has its report, the expenses'
// first, then the revenue's.
func snapshotOf(c changes.Change, number int64, previous fixed.Total) (snapshot, error) {
	project, err := c.Q.ProjectWithID(c.Ctx, c.ProjectID)
	figures, figuresErr := accounts.FiguresOf(c.Ctx, c.Q, c.ProjectID)
	capital, lines, capitalErr := partners.Capital(c.Ctx, c.Q, c.ProjectID, func(uuid.UUID) bool { return true })
	expenses, revenue, categoriesErr := categoriesOf(c.Ctx, c.Q, c.ProjectID)
	reconciliation, reconciliationErr := accounts.LatestOf(c.Ctx, c.Q, c.ProjectID, nil)
	waiting, waitingErr := WaitingOf(c.Ctx, c.Q, c.ProjectID)
	if err := errors.Join(err, figuresErr, capitalErr, categoriesErr, reconciliationErr, waitingErr); err != nil {
		return snapshot{}, err
	}
	report := snapshot{ProjectName: project.Name, Number: number, IssuedAt: c.Now, PreviousExpenses: previous, Expenses: figures.Expenses,
		Capital: figures.Capital, VatRefunds: figures.VatRefunds, Sales: figures.Sales, OtherRevenue: figures.OtherRevenue, Surplus: figures.Surplus(),
		SurplusWithoutSales: figures.SurplusWithoutSales(), Categories: expenses, Accounts: []reportAccount{}, AccountsTotal: reconciliation.ActualTotal,
		ReconciliationDifference: reconciliation.Difference, PendingCount: waiting.ExpensesCount, PendingAmount: waiting.ExpensesAmount,
		CapitalTotal: capital, Partners: []reportPartner{}, Items: []entries.ItemReport{}, RevenueCategories: revenue,
		PendingRevenueCount: waiting.RevenueCount, PendingRevenueAmount: waiting.RevenueAmount,
		ExpensesDifference: figures.Expenses.Minus(previous), Revenue: figures.Revenue(), RevenueWithoutSales: figures.Revenue().Minus(figures.Sales)}
	for _, balance := range reconciliation.Balances {
		report.Accounts = append(report.Accounts, reportAccount{Name: balance.AccountName, Type: balance.Type, Balance: balance.ActualBalance})
	}
	for _, line := range lines {
		report.Partners = append(report.Partners, reportPartner{Name: line.Name, ID: line.ID, Balance: line.Balance, SharePercent: line.SharePercent})
	}
	for _, category := range append(expenses, revenue...) {
		for _, total := range category.Items {
			if total.Total.Number() == 0 {
				continue
			}
			item, err := c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: total.ID, ProjectID: c.ProjectID})
			if err != nil {
				return snapshot{}, err
			}
			itemReport, err := entries.ItemReportOf(c, item, func(uuid.UUID) bool { return true })
			if err != nil {
				return snapshot{}, err
			}
			report.Items = append(report.Items, itemReport)
		}
	}
	return report, nil
}

// issue issues the next report now, numbered after the latest so numbering has no gaps (AC-29.3), and answers it as the list shows it.
func (s Routes) issue(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var issued reportRow
	if !s.writer.Write(w, r, caller, func(c changes.Change) error {
		previous, err := latestOf(c)
		if err != nil {
			return err
		}
		report, err := snapshotOf(c, previous.Number+1, fixedTotalOf(previous))
		if err != nil {
			return err
		}
		kept, err := json.Marshal(report)
		if err != nil {
			return err
		}
		row := store.AddPeriodicReportParams{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Number: report.Number, IssuedAt: c.Now,
			IssuedByUserID: c.Actor, ExpensesTotal: report.Expenses.Number(), Snapshot: string(kept)}
		if err := c.Q.AddPeriodicReport(c.Ctx, row); err != nil {
			return err
		}
		issued = reportRow{Number: row.Number, IssuedAt: row.IssuedAt, Expenses: report.Expenses}
		return c.Record(history.Create, "PeriodicReports", row.ID, nil, map[string]any{"ProjectId": row.ProjectID, "Number": row.Number,
			"IssuedAt": row.IssuedAt, "IssuedByUserId": row.IssuedByUserID, "ExpensesTotal": row.ExpensesTotal})
	}) {
		return
	}
	web.JSON(w, r, http.StatusOK, issued)
}

// fixedTotalOf is the previous report's expenses, which the next one's difference starts from (BR-09): kept with four decimals, nothing while no
// report was issued.
func fixedTotalOf(previous store.LatestPeriodicReportRow) fixed.Total {
	if previous.Number == 0 {
		return fixed.Total{}
	}
	return fixed.Counted(previous.ExpensesTotal)
}

// periodicReports is the reports issued, newest first.
func (s Routes) periodicReports(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	rows, err := s.database.Queries().PeriodicReportsOf(r.Context(), web.ID(r, "projectId"))
	issued := []reportRow{}
	for _, row := range rows {
		issued = append(issued, reportRow{Number: row.Number, IssuedAt: row.IssuedAt, Expenses: fixed.Counted(row.ExpensesTotal)})
	}
	s.answer(w, r, issued, err)
}

// nextPeriodicReport is the number the next report takes, and what waits for approval, which it shows as two lines (UC-053 step 2).
func (s Routes) nextPeriodicReport(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	latest, err := latestOf(c)
	waiting, waitingErr := WaitingOf(c.Ctx, c.Q, c.ProjectID)
	s.answer(w, r, struct {
		Number  int64   `json:"number"`
		Waiting Waiting `json:"waiting"`
	}{latest.Number + 1, waiting}, errors.Join(err, waitingErr))
}

// periodicReport is a report as it was issued: every partner for whoever sees all capital, otherwise the total and only the lines of the partners
// the caller sees, and no item's report (BR-31); 404 for a number not issued.
func (s Routes) periodicReport(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	projectID := web.ID(r, "projectId")
	kept, err := s.database.Queries().PeriodicReportSnapshot(r.Context(), store.PeriodicReportSnapshotParams{ProjectID: projectID, Number: web.Int(r, "number")})
	if err == nil && len(kept) == 0 {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	var report snapshot
	if err == nil {
		err = json.Unmarshal([]byte(kept[0]), &report)
	}
	if !caller.Access.Holds(projectID, access.SeeAllCapital) {
		seen := []reportPartner{}
		for _, partner := range report.Partners {
			if caller.Access.SeesCapitalOf(projectID, partner.ID) {
				seen = append(seen, partner)
			}
		}
		report.Partners, report.Items = seen, []entries.ItemReport{}
	}
	s.answer(w, r, report, err)
}

// answer writes view, or 500 when reading it failed.
func (s Routes) answer(w http.ResponseWriter, r *http.Request, view any, err error) {
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, view)
}
