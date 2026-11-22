// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/reports"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// reportsShown is Reports as the app's screen shows it (UC-053): the periodic reports issued, newest first, the number the next one takes and what
// waits, for whoever sees every table; whether Issue is being asked, and what came of it.
type reportsShown struct {
	issued   []reports.Report
	next     reports.Next
	whole    bool
	asking   bool
	location *time.Location
	said     said
}

// waitingCount is how many entries wait for approval, which Issue's question says the report shows apart.
func (s reportsShown) waitingCount() string {
	return strconv.FormatInt(s.next.Waiting.ExpensesCount+s.next.Waiting.RevenueCount, 10)
}

// mayIssue says whether p's caller may issue the next report: GenerateReports, seeing every table, in a project not finished, as the app's
// button and the route's gate say.
func (p page) mayIssue(shown reportsShown) bool {
	return shown.whole && !p.finished() && p.may(access.GenerateReports)
}

// reportsOf is Reports for p's caller, through the rules the app's routes answer (reports.Issued, reports.NextOf). The error is the database's.
func (s site) reportsOf(ctx context.Context, p page, said said) (reportsShown, error) {
	read := s.Database.Queries()
	shown := reportsShown{whole: p.caller.Access.SeesEveryTable(p.project.ID), said: said}
	if !shown.whole {
		return shown, nil
	}
	issued, err := reports.Issued(ctx, read, p.project.ID)
	next, nextErr := reports.NextOf(ctx, read, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	shown.issued, shown.next, shown.location = issued, next, zones.Location(zone)
	return shown, errors.Join(err, nextErr, zoneErr)
}

// reportsTab is Reports in its tab (UC-053, redix-api #171): the periodic reports, Issue asked first, and the item reports of each side.
func (s site) reportsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.reportsOf(r.Context(), p, said{asking: r.URL.Query().Get("asking")})
	shown.asking = shown.said.asking == "issue" && p.mayIssue(shown)
	return reportsPage(p, shown), err
}

// issueReport issues the next report through reports.IssueNext, the rule the app's route runs, and answers Reports saying it was issued, or why
// not in the app's words. 403 without GenerateReports or every table; a finished project issues nothing (AC-04.1).
func (s site) issueReport(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.GenerateReports) || !p.caller.Access.SeesEveryTable(p.project.ID) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown := said{notice: "reports.issued"}
	if p.finished() {
		shown = said{refusal: "projects.projectFinished"}
	} else {
		err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			_, err := reports.IssueNext(c)
			return err
		})
		var refused changes.Refusal
		switch {
		case errors.As(err, &refused):
			shown = said{refusal: "reports." + refused.Code}
		case err != nil:
			failed(w)
			return
		}
	}
	listed, err := s.reportsOf(r.Context(), p, shown)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, reportsPage(p, listed))
}

// reportSide is one side of the item reports, as the app's SummarySide: the expenses or the revenue.
type reportSide struct {
	name, word string
	of         func(summary reports.Summary) ([]reports.CategoryTotal, fixed.Total)
}

// expensesSide and revenueSide are the item reports' sides, each a tab of its own, opened from Reports.
var (
	expensesSide = reportSide{"expenses", "summary.expenses", func(summary reports.Summary) ([]reports.CategoryTotal, fixed.Total) {
		return summary.Categories, summary.Expenses
	}}
	revenueSide = reportSide{"revenue", "summary.revenue", func(summary reports.Summary) ([]reports.CategoryTotal, fixed.Total) {
		return summary.RevenueCategories, summary.Revenue
	}}
)

// itemReports is a side's item reports as the app's ItemReportsScreen shows them (UC-050): each category's total and share of the side, its items
// the search keeps, each opening its report.
type itemReports struct {
	side       reportSide
	categories []reports.CategoryTotal
	total      fixed.Total
	text       string
}

// itemsOf is what the search keeps of category's items, and whether the category shows, as the app's search groups them.
func (shown itemReports) itemsOf(category reports.CategoryTotal) ([]reports.ItemTotal, bool) {
	return searchGroup(shown.text, category.Name, category.Items, func(item reports.ItemTotal) string { return item.Name })
}

// itemReportsIn is side's item reports in its tab, from the summary the app's screen reads (reports.SummaryOf), as the caller sees it (BR-31).
func (s site) itemReportsIn(side reportSide, r *http.Request, p page) (templ.Component, error) {
	summary, err := reports.SummaryOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller.Access.Unseen(p.project.ID))
	categories, total := side.of(summary)
	return itemReportsPage(p, itemReports{side: side, categories: categories, total: total, text: strings.TrimSpace(r.URL.Query().Get("text"))}), err
}

// expensesTab is the expenses' item reports, by category.
func (s site) expensesTab(r *http.Request, p page) (templ.Component, error) {
	return s.itemReportsIn(expensesSide, r, p)
}

// revenueTab is the revenue's item reports, by category, the capital among them.
func (s site) revenueTab(r *http.Request, p page) (templ.Component, error) {
	return s.itemReportsIn(revenueSide, r, p)
}

// itemReportShown is an item's report as the app's ItemReportScreen shows it (UC-050), or a contract's - its item's report holding that contract
// alone -, with the search typed and where it came from.
type itemReportShown struct {
	report entries.ItemReport
	found  bool
	query  url.Values
	text   string
}

// rowsOf is what the search keeps of the rows of the table called name, and whether the table shows, as the app's search groups them.
func (shown itemReportShown) rowsOf(name string, rows []entries.ReportRow) ([]entries.ReportRow, bool) {
	return searchGroup(shown.text, name, rows, reportRowText)
}

// reportRowText is what the search finds in a report's row: its values, its date and its amount, as the app's _textOf.
func reportRowText(row entries.ReportRow) string {
	return strings.Join(append(slices.Clone(row.Values), row.Date.Format(time.DateOnly), row.Amount.Plain(), statement.Money(row.Amount)), " ")
}

// reportRowTitle is a report row's title, its description, as the app's row titles it.
func reportRowTitle(row entries.ReportRow, columns []entries.ReportColumn) string {
	at := slices.IndexFunc(columns, func(column entries.ReportColumn) bool { return column.Key == "description" })
	if at < 0 || at >= len(row.Values) {
		return ""
	}
	return row.Values[at]
}

// reportRowDetails is a report row's other values, each with its column's name, an amount column's written as money, as the app's row says them.
func reportRowDetails(row entries.ReportRow, columns []entries.ReportColumn) string {
	var details []string
	for at, column := range columns {
		if at >= len(row.Values) || row.Values[at] == "" || column.Key == "description" {
			continue
		}
		value := row.Values[at]
		if column.DataType == structure.Amount {
			if number, err := fixed.Parse(value); err == nil {
				value = statement.Money(number)
			}
		}
		details = append(details, column.Name+": "+value)
	}
	return parts(details...)
}

// itemReportTab is an item's report in its tab, or a contract's for ?contract=, through the rules the app's routes answer (entries.ItemReportIn,
// entries.ContractReportOf); an item or contract the caller may not see says so.
func (s site) itemReportTab(r *http.Request, p page) (templ.Component, error) {
	query := r.URL.Query()
	c := changes.Change{Ctx: r.Context(), Q: s.Database.Queries(), ProjectID: p.project.ID, Access: p.caller.Access}
	var report entries.ItemReport
	var err error
	if contractID, _ := uuid.Parse(query.Get("contract")); contractID != uuid.Nil {
		report, err = entries.ContractReportOf(c, contractID)
	} else {
		itemID, _ := uuid.Parse(query.Get("item"))
		report, err = entries.ItemReportIn(c, itemID)
	}
	shown := itemReportShown{report: report, found: err == nil, query: query, text: strings.TrimSpace(query.Get("text"))}
	if refused := (changes.Refusal{}); errors.As(err, &refused) && refused.Status == http.StatusNotFound {
		err = nil
	}
	return itemReportPage(p, shown), err
}

// subgroupName is a report table's name, the item's general table called general, as the app's report names it.
func subgroupName(p page, table entries.TableReport) string {
	if table.Name == "" {
		return p.words.Of("reports.generalSubgroup")
	}
	return table.Name
}

// contractReportLine is what a contract's report says under its contractor: its value, or that it is open, what was paid and, but for an
// open-value contract, what remains.
func contractReportLine(p page, contract entries.ContractReport) string {
	value := p.words.Of("reports.openValue")
	remaining := ""
	if !contract.IsOpenValue {
		value = statement.Money(contract.Value)
		remaining = p.words.Of("reports.remaining") + ": " + money(contract.Remaining)
	}
	return parts(p.words.Of("reports.contractValue")+": "+value, p.words.Of("reports.paid")+": "+money(contract.Paid), remaining)
}
