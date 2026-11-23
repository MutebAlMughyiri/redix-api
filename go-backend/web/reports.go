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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
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

// itemCount is how many items the side's categories hold.
func (shown itemReports) itemCount() int {
	count := 0
	for _, category := range shown.categories {
		count += len(category.Items)
	}
	return count
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

// table is the table ?table= names, when the item holds it: the one the page shows alone.
func (shown itemReportShown) table() (entries.TableReport, bool) {
	id, _ := uuid.Parse(shown.query.Get("table"))
	at := slices.IndexFunc(shown.report.Subgroups, func(table entries.TableReport) bool { return table.ID == id })
	if id == uuid.Nil || at < 0 {
		return entries.TableReport{}, false
	}
	return shown.report.Subgroups[at], true
}

// contract is the contract a contract's report holds, ?contract= having asked for it: the one the page shows alone.
func (shown itemReportShown) contract() (entries.ContractReport, bool) {
	if shown.query.Get("contract") == "" || len(shown.report.Contracts) != 1 {
		return entries.ContractReport{}, false
	}
	return shown.report.Contracts[0], true
}

// recordQuery is what the page was opened for, its item, contract and table, as the search toggle asks for the page again.
func (shown itemReportShown) recordQuery() string {
	return url.Values{"item": {shown.query.Get("item")}, "contract": {shown.query.Get("contract")}, "table": {shown.query.Get("table")}}.Encode()
}

// side is the side the item is on, Expenses or Revenue - the capital among the revenue -, which the way back opens.
func (shown itemReportShown) side() reportSide {
	if shown.report.Type.IsRevenue() {
		return revenueSide
	}
	return expensesSide
}

// entryCount is how many entries the item's tables and contracts hold.
func (shown itemReportShown) entryCount() int {
	count := 0
	for _, table := range shown.report.Subgroups {
		count += len(table.Rows)
	}
	for _, contract := range shown.report.Contracts {
		count += len(contract.Payments)
	}
	return count
}

// kept is what the search keeps of one table's rows.
func (shown itemReportShown) kept(rows []entries.ReportRow) []entries.ReportRow {
	return slices.DeleteFunc(slices.Clone(rows), func(row entries.ReportRow) bool { return !searchHolds(shown.text, reportRowText(row)) })
}

// lastDate is the day of the latest of rows, or a dash for none.
func lastDate(rows []entries.ReportRow) string {
	if len(rows) == 0 {
		return "—"
	}
	return slices.MaxFunc(rows, func(a, b entries.ReportRow) int { return a.Date.Compare(b.Date.Time) }).Date.Format(time.DateOnly)
}

// reportValue is a row's value in column, the at-th of its table, an amount column's written as money, as the app's report writes it.
func reportValue(row entries.ReportRow, at int, column entries.ReportColumn) string {
	if at >= len(row.Values) {
		return ""
	}
	if number, err := fixed.Parse(row.Values[at]); err == nil && column.DataType == structure.Amount {
		return statement.Money(number)
	}
	return row.Values[at]
}

// rowsOf is what the search keeps of the rows of the table called name, and whether the table shows, as the app's search groups them.
func (shown itemReportShown) rowsOf(name string, rows []entries.ReportRow) ([]entries.ReportRow, bool) {
	return searchGroup(shown.text, name, rows, reportRowText)
}

// reportRowText is what the search finds in a report's row: its values, its date and its amount, as the app's _textOf.
func reportRowText(row entries.ReportRow) string {
	return strings.Join(append(slices.Clone(row.Values), row.Date.Format(time.DateOnly), row.Amount.Plain(), statement.Money(row.Amount)), " ")
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
	remaining := ""
	if !contract.IsOpenValue {
		remaining = p.words.Of("reports.remaining") + ": " + contractRemainingOf(contract)
	}
	return parts(p.words.Of("reports.contractValue")+": "+contractValueOf(p, contract), p.words.Of("reports.paid")+": "+money(contract.Paid), remaining)
}

// contractValueOf is a contract's value, or that it is open.
func contractValueOf(p page, contract entries.ContractReport) string {
	if contract.IsOpenValue {
		return p.words.Of("reports.openValue")
	}
	return statement.Money(contract.Value)
}

// contractRemainingOf is what remains of a contract, in brackets once overrun, or a dash for an open-value contract.
func contractRemainingOf(contract entries.ContractReport) string {
	if contract.IsOpenValue {
		return "—"
	}
	return money(contract.Remaining)
}

// reportLine is a line of a periodic report's section: its label and amount, strong for a total, as the app's report draws it.
type reportLine struct {
	id, label string
	amount    fixed.Total
	strong    bool
}

// reportSection is a section of a periodic report: its title and its lines.
type reportSection struct {
	name, title string
	lines       []reportLine
}

// periodicSections are a periodic report's sections in the app's order (PeriodicReportScreen): the accounts and what they hold, the expenses
// against the previous report's, the revenue by category, the expenses by category, the surplus with what waits, and the partners' capital.
func periodicSections(p page, report reports.Snapshot) []reportSection {
	line := func(section, name, label string, amount fixed.Total) reportLine {
		return reportLine{id: "periodicReport.line." + section + "." + name, label: label, amount: amount}
	}
	strong := func(id, label string, amount fixed.Total) reportLine {
		return reportLine{id: id, label: label, amount: amount, strong: true}
	}
	accounts := reportSection{name: "accounts", title: p.words.Of("accounts.title")}
	for at, account := range report.Accounts {
		accounts.lines = append(accounts.lines, line("accounts", strconv.Itoa(at), account.Name, account.Balance))
	}
	accounts.lines = append(accounts.lines, strong("periodicReport.line.accounts.total", p.words.Of("reconciliations.actualTotal"), report.AccountsTotal))
	revenue := reportSection{name: "revenue", title: p.words.Of("summary.revenue")}
	for at, category := range report.RevenueCategories {
		revenue.lines = append(revenue.lines, line("revenue", strconv.Itoa(at), category.Name, category.Total))
	}
	revenue.lines = append(revenue.lines, strong("periodicReport.line.revenue.total", p.words.Of("summary.revenue"), report.Revenue),
		line("revenue", "withoutSales", p.words.Of("summary.surplusWithoutSales"), report.RevenueWithoutSales))
	categories := reportSection{name: "categories", title: p.words.Of("summary.byCategory")}
	for at, category := range report.Categories {
		categories.lines = append(categories.lines, line("categories", strconv.Itoa(at), category.Name, category.Total))
	}
	categories.lines = append(categories.lines, strong("periodicReport.line.categories.total", p.words.Of("summary.expenses"), report.Expenses))
	surplus := reportSection{name: "surplus", title: p.words.Of("reports.surplus"), lines: []reportLine{
		strong("periodicReport.surplus", p.words.Of("reports.surplus"), report.Surplus),
		line("surplus", "withoutSales", p.words.Of("summary.surplusWithoutSales"), report.SurplusWithoutSales),
		line("surplus", "reconciliation", p.words.Of("reconciliations.difference"), report.ReconciliationDifference),
	}}
	if report.PendingCount > 0 {
		surplus.lines = append(surplus.lines, line("surplus", "pendingExpenses",
			p.words.Of("reports.pendingExpenses")+" ("+strconv.FormatInt(report.PendingCount, 10)+")", report.PendingAmount))
	}
	if report.PendingRevenueCount > 0 {
		surplus.lines = append(surplus.lines, line("surplus", "pendingRevenue",
			p.words.Of("reports.pendingRevenue")+" ("+strconv.FormatInt(report.PendingRevenueCount, 10)+")", report.PendingRevenueAmount))
	}
	partners := reportSection{name: "partners", title: p.words.Of("reports.partners")}
	for at, partner := range report.Partners {
		partners.lines = append(partners.lines, line("partners", strconv.Itoa(at), partner.Name+" · "+shareOf(partner.SharePercent), partner.Balance))
	}
	partners.lines = append(partners.lines, strong("periodicReport.line.partners.total", p.words.Of("capital.total"), report.CapitalTotal))
	return []reportSection{accounts, {name: "expenses", title: p.words.Of("summary.expenses"), lines: []reportLine{
		line("expenses", "previous", p.words.Of("reports.previous"), report.PreviousExpenses),
		line("expenses", "current", p.words.Of("reports.current"), report.Expenses),
		strong("periodicReport.difference", p.words.Of("reports.difference"), report.ExpensesDifference),
	}}, revenue, categories, surplus, partners}
}

// periodicReportShown is a periodic report as it was issued, as the caller may see it, and the day it was issued in the project's time zone.
type periodicReportShown struct {
	report   reports.Snapshot
	found    bool
	whole    bool
	location *time.Location
}

// periodicReportTab is the periodic report ?number= in its tab, as the app's screen shows it (UC-053), through the rule the app's route answers
// (reports.PeriodicReportOf), for whoever sees every table.
func (s site) periodicReportTab(r *http.Request, p page) (templ.Component, error) {
	shown := periodicReportShown{whole: p.caller.Access.SeesEveryTable(p.project.ID)}
	if !shown.whole {
		return periodicReportPage(p, shown), nil
	}
	read := s.Database.Queries()
	number, _ := strconv.ParseInt(r.URL.Query().Get("number"), 10, 64)
	report, found, err := reports.PeriodicReportOf(r.Context(), read, p.project.ID, number, p.caller.Access)
	zone, zoneErr := zones.Of(r.Context(), read, p.project.ID)
	shown.report, shown.found, shown.location = report, found, zones.Location(zone)
	return periodicReportPage(p, shown), errors.Join(err, zoneErr)
}

// mayTakeReportFiles says whether p's caller may take a report's PDF: GenerateReports, a finished project's too, as the app's file buttons and the
// routes' gate say (redix-api #131).
func (p page) mayTakeReportFiles() bool { return p.may(access.GenerateReports) }

// periodicFile hands the periodic report the path numbers as a PDF or a workbook, through the rules the app's routes answer
// (reports.PeriodicPdfOf, reports.PeriodicWorkbookOf). 403 without GenerateReports or every table; 404 for a number not issued.
func (s site) periodicFile(w http.ResponseWriter, r *http.Request) {
	p, ok := s.filing(w, r)
	if !ok {
		return
	}
	if !p.caller.Access.SeesEveryTable(p.project.ID) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	number, _ := strconv.ParseInt(r.PathValue("number"), 10, 64)
	isPdf := r.PathValue("format") == "pdf"
	var file []byte
	var found bool
	var err error
	if isPdf {
		file, found, err = reports.PeriodicPdfOf(r.Context(), s.Database.Queries(), s.Typst, p.project.ID, number, p.caller)
	} else {
		file, found, err = reports.PeriodicWorkbookOf(r, s.Database.Queries(), s.Links, p.project.ID, number, p.caller)
	}
	switch {
	case err != nil:
		failed(w)
	case !found:
		http.NotFound(w, r)
	default:
		web.SendFile(w, fileTypes[isPdf], reports.PeriodicFileName(number, isPdf), file)
	}
}

// reportFile hands an item's report, or a contract's, as a PDF or a workbook, through the rules the app's routes answer (entries.ItemReportIn,
// entries.ContractReportOf, reports.ReportPdfOf, reports.ReportWorkbookOf). 403 without GenerateReports; 404 for one the caller may not see.
func (s site) reportFile(w http.ResponseWriter, r *http.Request) {
	p, ok := s.filing(w, r)
	if !ok {
		return
	}
	c := changes.Change{Ctx: r.Context(), Q: s.Database.Queries(), ProjectID: p.project.ID, Access: p.caller.Access}
	id, _ := uuid.Parse(r.PathValue("id"))
	var report entries.ItemReport
	var err error
	if r.PathValue("kind") == "contract" {
		report, err = entries.ContractReportOf(c, id)
	} else {
		report, err = entries.ItemReportIn(c, id)
	}
	isPdf := r.PathValue("format") == "pdf"
	var file []byte
	switch {
	case err == nil && isPdf:
		file, err = reports.ReportPdfOf(s.Typst, report)
	case err == nil:
		file, err = reports.ReportWorkbookOf(r, c, s.Links, report)
	}
	if refused := (changes.Refusal{}); errors.As(err, &refused) && refused.Status == http.StatusNotFound {
		http.NotFound(w, r)
		return
	}
	if err != nil {
		failed(w)
		return
	}
	web.SendFile(w, fileTypes[isPdf], r.PathValue("kind")+"."+r.PathValue("format"), file)
}

// fileTypes are a report file's media types: a PDF's, and a workbook's.
var fileTypes = map[bool]string{true: "application/pdf", false: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}

// filing is the page a report file is asked from, for whoever may take report files; false once it has answered 404 outside a project or for a
// format there is none of, or 403.
func (s site) filing(w http.ResponseWriter, r *http.Request) (page, bool) {
	p, ok := s.working(w, r)
	if !ok {
		return p, false
	}
	if format := r.PathValue("format"); !p.inProject() || (format != "pdf" && format != "xlsx") {
		http.NotFound(w, r)
		return p, false
	}
	if !p.mayTakeReportFiles() {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return p, false
	}
	return p, true
}

// fileAddress is where the report shown is taken as a file: a contract's for a query naming one, otherwise its item's.
func (shown itemReportShown) fileAddress() string {
	if contractID, _ := uuid.Parse(shown.query.Get("contract")); contractID != uuid.Nil {
		return "/report-file/contract/" + contractID.String()
	}
	return "/report-file/item/" + shown.report.ItemID.String()
}

// reportFormat is a report file's format: its word's name and its path.
type reportFormat struct{ name, path string }

// reportFormats are a report's files, in the app's order: its PDF, then its Excel.
var reportFormats = []reportFormat{{"pdf", "pdf"}, {"excel", "xlsx"}}
