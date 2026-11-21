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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// approvalFilter is one of Approvals' filters, as the app's: its name, its word, and the kind of entry it keeps, every kind for "".
type approvalFilter struct{ name, word, kind string }

// approvalFilters are Approvals' filters, in the app's order: everything, purchases, contractor payments.
var approvalFilters = []approvalFilter{{"all", "approvals.all", ""}, {"purchases", "approvals.purchases", "purchase"},
	{"contractorPayments", "approvals.contractorPayments", "contractorPayment"}}

// approvals is what Approvals shows: what waits that the caller sees, the filter and the search chosen, and why each entry that could not be
// approved was not.
type approvals struct {
	waiting []entries.Pending
	filter  approvalFilter
	text    string
	failed  map[uuid.UUID]string
}

// keeps says whether entry is among those the filter and the search keep: the search finds the entry's number, its item, its supplier, who
// entered it, its invoice, its description and its amount.
func (a approvals) keeps(entry entries.Pending, filter approvalFilter) bool {
	if filter.kind != "" && entry.Kind.Name() != filter.kind {
		return false
	}
	text := strings.ToLower(a.text)
	return text == "" || slices.ContainsFunc([]string{strconv.FormatInt(entry.Number, 10), entry.ItemName, entry.SupplierName, entry.AuthorName, entry.InvoiceNumber,
		entry.Description, statement.Money(entry.Amount)}, func(field string) bool { return strings.Contains(strings.ToLower(field), text) })
}

// shown is what the filter and the search keep, oldest first.
func (a approvals) shown() []entries.Pending {
	var kept []entries.Pending
	for _, entry := range a.waiting {
		if a.keeps(entry, a.filter) {
			kept = append(kept, entry)
		}
	}
	return kept
}

// count is how many entries filter keeps with the search, as the app's filter says beside its name.
func (a approvals) count(filter approvalFilter) string {
	count := 0
	for _, entry := range a.waiting {
		if a.keeps(entry, filter) {
			count++
		}
	}
	return strconv.Itoa(count)
}

// approvalsOf is Approvals for p's caller with what query chose, through the rule the app's route answers (entries.PendingOf). The error is the
// database's.
func (s site) approvalsOf(ctx context.Context, p page, query url.Values) (approvals, error) {
	waiting, err := entries.PendingOf(ctx, s.Database.Queries(), p.project.ID, p.caller)
	shown := approvals{waiting: waiting, filter: approvalFilters[0], text: strings.TrimSpace(query.Get("text"))}
	for _, filter := range approvalFilters {
		if filter.name == query.Get("filter") {
			shown.filter = filter
		}
	}
	return shown, err
}

// approvalsTab is Approvals in its tab (UC-032, redix-api #167): what waits, filtered and searched as the app's, each a row that opens in the
// details panel, with a tick for Approve selected.
func (s site) approvalsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.approvalsOf(r.Context(), p, r.URL.Query())
	return approvalsList(p, shown), err
}

// approveSelected approves the entries ticked, each on its own, through the rule the app's route runs (entries.ApproveEach, FR-19, AC-19.2), in
// one change: Approvals again, without those approved, saying beside each other why it was not. 403 without ApproveEntries, as the route's
// gate refuses it; a finished project approves nothing (AC-04.1).
func (s site) approveSelected(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ApproveEntries) || p.finished() {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	var ticked []uuid.UUID
	for _, value := range r.PostForm["entry"] {
		if id, err := uuid.Parse(value); err == nil {
			ticked = append(ticked, id)
		}
	}
	var notApproved []entries.ApprovalFailure
	err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		var err error
		_, notApproved, err = entries.ApproveEach(c, p.caller, ticked)
		return err
	})
	if err != nil {
		failed(w)
		return
	}
	shown, err := s.approvalsOf(r.Context(), p, r.PostForm)
	if err != nil {
		failed(w)
		return
	}
	shown.failed = map[uuid.UUID]string{}
	for _, failure := range notApproved {
		shown.failed[failure.EntryID] = "approvals." + failure.Code
	}
	draw(w, r, http.StatusOK, approvalsList(p, shown))
}

// rejectionReason is the reason a rejection gives: what was typed, or the quick reason chosen when nothing was (the app's ReasonSheet).
func rejectionReason(r *http.Request) string {
	if typed := strings.TrimSpace(r.PostFormValue("reason")); typed != "" {
		return typed
	}
	return r.PostFormValue("quick")
}

// approvalHistory is a page of the approval history as the app's screen shows it: its lines, the project's members to narrow by, what the query
// chose, and the project's time zone its times are told in.
type approvalHistory struct {
	lines    []entries.ApprovalLine
	members  []store.MembersRow
	query    url.Values
	location *time.Location
}

// more is the query of the page after this one - its last line's - when this page is full; "" when it is the last.
func (h approvalHistory) more() string {
	if len(h.lines) < entries.PageSize {
		return ""
	}
	next := url.Values{"before": {strconv.FormatInt(h.lines[len(h.lines)-1].ID, 10)}, "kind": {h.query.Get("kind")}, "by": {h.query.Get("by")},
		"text": {h.query.Get("text")}}
	return next.Encode()
}

// what is what line says was decided, as the app's line says it: the entry's item, supplier and amount, and a rejection's reason.
func (h approvalHistory) what(p page, line entries.ApprovalLine) string {
	key := "history.entryApproved"
	if line.Kind.Name() == "rejected" {
		key = "history.entryRejected"
	}
	return p.words.Of(key, parts(line.Item, line.Party, statement.Money(line.Amount)), line.Reason)
}

// whoAndWhen is who decided line and when, in the project's time: an approval says who approved it.
func (h approvalHistory) whoAndWhen(p page, line entries.ApprovalLine) string {
	who := line.By
	if line.Kind.Name() == "approved" {
		who = p.words.Of("history.approvedBy", line.By)
	}
	return parts(who, line.At.In(h.location).Format("2006-01-02 15:04"))
}

// parts is the non-empty of values, each apart from the next by a middle dot, as the app's partsLine joins them.
func parts(values ...string) string {
	return strings.Join(slices.DeleteFunc(values, func(value string) bool { return value == "" }), " · ")
}

// approvalHistoryOf is the page of the approval history query asks, through the rule the app's route answers (entries.ApprovalHistoryOf),
// with the project's members and time zone. The error is the database's.
func (s site) approvalHistoryOf(ctx context.Context, p page, query url.Values) (approvalHistory, error) {
	read := s.Database.Queries()
	lines, err := entries.ApprovalHistoryOf(ctx, read, p.project.ID, p.caller, query)
	members, membersErr := read.Members(ctx, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	return approvalHistory{lines: lines, members: members, query: query, location: zones.Location(zone)}, errors.Join(err, membersErr, zoneErr)
}

// approvalHistoryTab is the approval history in its tab (the owner, 12 Nov 2026; redix-api #167): what was approved and rejected, by whom and
// when, newest first, searched and narrowed by kind and person as the app's.
func (s site) approvalHistoryTab(r *http.Request, p page) (templ.Component, error) {
	history, err := s.approvalHistoryOf(r.Context(), p, r.URL.Query())
	return approvalHistoryPage(p, history), err
}

// approvalHistoryMore is the next page of the approval history, added after the last line, with Show more again while there is more.
func (s site) approvalHistoryMore(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.offers("approvalHistory") {
		http.NotFound(w, r)
		return
	}
	history, err := s.approvalHistoryOf(r.Context(), p, r.URL.Query())
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, approvalLines(p, history), moreHistory(p, history, true))
}
