// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"context"
	"errors"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
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

// Snapshot is everything a periodic report shows, as it was when issued (PeriodicReportSnapshot, BRS section 6.1). It is kept as the JSON it
// answers with, and read back with every number in the scale it was written with; a report .NET kept, its kinds written as numbers, reads the same.
type Snapshot struct {
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
	Categories               []CategoryTotal      `json:"categories"`
	Accounts                 []reportAccount      `json:"accounts"`
	AccountsTotal            fixed.Total          `json:"accountsTotal"`
	ReconciliationDifference fixed.Total          `json:"reconciliationDifference"`
	PendingCount             int64                `json:"pendingCount"`
	PendingAmount            fixed.Total          `json:"pendingAmount"`
	CapitalTotal             fixed.Total          `json:"capitalTotal"`
	Partners                 []reportPartner      `json:"partners"`
	Items                    []entries.ItemReport `json:"items"`
	RevenueCategories        []CategoryTotal      `json:"revenueCategories"`
	PendingRevenueCount      int64                `json:"pendingRevenueCount"`
	PendingRevenueAmount     fixed.Total          `json:"pendingRevenueAmount"`
	ExpensesDifference       fixed.Total          `json:"expensesDifference"`
	Revenue                  fixed.Total          `json:"revenue"`
	RevenueWithoutSales      fixed.Total          `json:"revenueWithoutSales"`
}

// Report is a report issued (PeriodicReportRowView).
type Report struct {
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

// snapshotOf is the report numbered number, issued at from what is approved now: what was approved since the previous one is marked new, whatever
// its date, and the expenses' difference starts from the previous report's (BR-09, BR-26). Every item with a total has its report, the expenses'
// first, then the revenue's.
func snapshotOf(c changes.Change, number int64, at moment.Moment, previous fixed.Total) (Snapshot, error) {
	project, err := changes.Found(c.Q.ProjectWithID(c.Ctx, c.ProjectID))
	figures, figuresErr := accounts.FiguresOf(c.Ctx, c.Q, c.ProjectID, access.Everything)
	capital, lines, capitalErr := partners.Capital(c.Ctx, c.Q, c.ProjectID, func(uuid.UUID) bool { return true })
	expenses, revenue, categoriesErr := categoriesOf(c.Ctx, c.Q, c.ProjectID, access.Everything)
	reconciliation, reconciliationErr := accounts.LatestOf(c.Ctx, c.Q, c.ProjectID, nil)
	waiting, waitingErr := WaitingOf(c.Ctx, c.Q, c.ProjectID, access.Everything)
	if err := errors.Join(err, figuresErr, capitalErr, categoriesErr, reconciliationErr, waitingErr); err != nil {
		return Snapshot{}, err
	}
	report := Snapshot{ProjectName: project.Name, Number: number, IssuedAt: at, PreviousExpenses: previous, Expenses: figures.Expenses,
		Capital: figures.Capital, VatRefunds: figures.VatRefunds, Sales: figures.Sales, OtherRevenue: figures.OtherRevenue, Surplus: figures.Surplus(),
		SurplusWithoutSales: figures.SurplusWithoutSales(), Categories: expenses, AccountsTotal: reconciliation.ActualTotal,
		ReconciliationDifference: reconciliation.Difference, PendingCount: waiting.ExpensesCount, PendingAmount: waiting.ExpensesAmount,
		CapitalTotal: capital, RevenueCategories: revenue,
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
				return Snapshot{}, err
			}
			itemReport, err := entries.ItemReportOf(c, item, func(uuid.UUID) bool { return true })
			if err != nil {
				return Snapshot{}, err
			}
			report.Items = append(report.Items, itemReport)
		}
	}
	return report, nil
}

// issue issues the next report now, numbered after the latest so numbering has no gaps (AC-29.3), and answers it as the list shows it.
func (s Routes) issue(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var issued Report
	if !s.writer.Write(w, r, caller, func(c changes.Change) (err error) {
		issued, err = IssueNext(c)
		return err
	}) {
		return
	}
	web.JSON(w, r, http.StatusOK, issued)
}

// IssueNext issues the change's project's next report now, numbered after the latest so numbering has no gaps (AC-29.3), as issue does: the rule
// both doors call, the app's route and the web's Reports (redix-api #171). The error is the database's.
func IssueNext(c changes.Change) (Report, error) {
	latest, err := latestOf(c)
	if err != nil {
		return Report{}, err
	}
	return Issue(c, latest.Number+1, c.Now)
}

// Issue issues the change's project's report numbered number at the moment at, from what is approved now: a numbered Snapshot that never
// changes (BR-26, PeriodicReportsEndpoints.IssueAsync). What was approved since the previous one is marked new, whatever its date, and the
// expenses' difference starts from the previous report's (BR-09). The route issues the next number now; a project file's load, the file's number
// at the file's time (redix-api #132 D9).
func Issue(c changes.Change, number int64, at moment.Moment) (Report, error) {
	previous, err := latestOf(c)
	if err != nil {
		return Report{}, err
	}
	report, err := snapshotOf(c, number, at, fixedTotalOf(previous))
	if err != nil {
		return Report{}, err
	}
	kept, err := encode.JSON(report)
	if err != nil {
		return Report{}, err
	}
	row := store.AddPeriodicReportParams{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Number: report.Number, IssuedAt: at,
		IssuedByUserID: c.Actor, ExpensesTotal: report.Expenses.Number(), Snapshot: string(kept)}
	if err := c.Q.AddPeriodicReport(c.Ctx, row); err != nil {
		return Report{}, err
	}
	return Report{Number: row.Number, IssuedAt: row.IssuedAt, Expenses: report.Expenses}, c.Record(history.Create, "PeriodicReports", row.ID, nil,
		map[string]any{"ProjectId": row.ProjectID, "Number": row.Number, "IssuedAt": row.IssuedAt, "IssuedByUserId": row.IssuedByUserID,
			"ExpensesTotal": row.ExpensesTotal})
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
	issued, err := Issued(r.Context(), s.database.Queries(), web.ID(r, "projectId"))
	s.answer(w, r, issued, err)
}

// Issued is the reports issued in projectID, newest first, as periodicReports answers them: the rule both doors call, the app's route and the
// web's Reports (redix-api #171). The error is the database's.
func Issued(ctx context.Context, read *store.Queries, projectID uuid.UUID) ([]Report, error) {
	rows, err := read.PeriodicReportsOf(ctx, projectID)
	var issued []Report
	for _, row := range rows {
		issued = append(issued, Report{Number: row.Number, IssuedAt: row.IssuedAt, Expenses: fixed.Counted(row.ExpensesTotal)})
	}
	return issued, err
}

// Next is the number the next report takes, and what waits for approval, as GET /projects/{projectId}/periodic-reports/next answers it (UC-053)
// and the web's Issue asks it.
type Next struct {
	Number  int64   `json:"number"`
	Waiting Waiting `json:"waiting"`
}

// nextPeriodicReport is the number the next report takes, and what waits for approval, which it shows as two lines (UC-053 step 2).
func (s Routes) nextPeriodicReport(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	next, err := NextOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"))
	s.answer(w, r, next, err)
}

// NextOf is the number projectID's next report takes and what waits for approval, as nextPeriodicReport answers it: the rule both doors call, the
// app's route and the web's Reports (redix-api #171). The error is the database's.
func NextOf(ctx context.Context, read *store.Queries, projectID uuid.UUID) (Next, error) {
	latest, err := latestOf(changes.Change{Ctx: ctx, Q: read, ProjectID: projectID})
	if err != nil {
		return Next{}, err
	}
	waiting, err := WaitingOf(ctx, read, projectID, access.Everything)
	return Next{latest.Number + 1, waiting}, err
}

// periodicReport is a report as it was issued: every partner for whoever sees all capital, otherwise the total and only the lines of the partners
// the caller sees, and no item's report (BR-31); 404 for a number not issued.
func (s Routes) periodicReport(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	report, found, err := PeriodicReportOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), web.Int(r, "number"), caller.Access)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !found {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	s.answer(w, r, report, err)
}

// PeriodicReportOf is projectID's report numbered number as it was issued, as periodicReport answers it to whoever holds held: every partner for
// whoever sees all capital, otherwise the total and only the lines of the partners they see, and no item's report (BR-31). The rule both doors
// call, the app's route and the web's report (redix-api #171); found is false for a number not issued, and the error is the database's.
func PeriodicReportOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, number int64, held access.Access) (Snapshot, bool, error) {
	report, found, err := issuedOf(ctx, read, projectID, number)
	if err != nil || !found || held.Holds(projectID, access.SeeAllCapital) {
		return report, found, err
	}
	var seen []reportPartner
	for _, partner := range report.Partners {
		if held.SeesCapitalOf(projectID, partner.ID) {
			seen = append(seen, partner)
		}
	}
	report.Partners, report.Items = seen, nil
	return report, true, nil
}

// answer writes view, or 500 when reading it failed.
func (s Routes) answer(w http.ResponseWriter, r *http.Request, view any, err error) {
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, view)
}
