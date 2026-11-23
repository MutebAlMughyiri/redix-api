// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"net/http"
	"net/url"
	"strconv"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// approvalFilter is one of Approvals' filters, as the app's: its name, its word, and the kind of entry it keeps, every kind for "".
type approvalFilter struct{ name, word, kind string }

// approvalFilters are Approvals' filters, in the app's order: everything, purchases, contractor payments.
var approvalFilters = []approvalFilter{{"all", "approvals.all", ""}, {"purchases", "approvals.purchases", "purchase"},
	{"contractorPayments", "approvals.contractorPayments", "contractorPayment"}}

// kindFilter is Approvals' kind filter as the approved design draws it (redix-api #183): everything, purchases and contractor payments, each
// with how many wait, the one chosen.
func (a approvals) kindFilter(p page) filter {
	kinds := filter{id: "approvals.filterChoice", label: "entries.kind"}
	for _, kind := range approvalFilters {
		kinds.choices = append(kinds.choices, filterChoice{id: "approvals." + kind.name, name: "filter", value: kind.name, word: p.words.Of(kind.word) + " · " + a.count(kind),
			chosen: kind == a.filter})
	}
	return kinds
}

// approvalKindFilter is the approval history's kind filter as the approved design draws it (redix-api #183): everything, purchases and
// contractor payments, the one query holds chosen.
func approvalKindFilter(p page, shown history) filter {
	kinds := filter{id: "approvalHistory.kindChoice", label: "entries.kind", choices: []filterChoice{{id: "approvalHistory.all", name: "kind", word: p.words.Of("history.all"),
		chosen: shown.query.Get("kind") == ""}}}
	for _, kind := range approvalFilters[1:] {
		kinds.choices = append(kinds.choices, filterChoice{id: "approvalHistory.kind." + kind.kind, name: "kind", value: kind.kind, word: p.words.Of(kind.word),
			chosen: shown.query.Get("kind") == kind.kind})
	}
	return kinds
}

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
	return searchHolds(a.text, strconv.FormatInt(entry.Number, 10), entry.ItemName, entry.SupplierName, entry.AuthorName, entry.InvoiceNumber,
		entry.Description, statement.Money(entry.Amount))
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

// approvalHistoryTab is the approval history in its tab (the owner, 12 Nov 2026; redix-api #167): what was approved and rejected, by whom and
// when, newest first, searched and narrowed by kind and person as the app's.
func (s site) approvalHistoryTab(r *http.Request, p page) (templ.Component, error) {
	history, err := s.approvalHistoryOf(r.Context(), p, r.URL.Query())
	return approvalHistoryPage(p, history), err
}

// approvalHistoryMore is the next page of the approval history, added after the last line, with Show more again while there is more.
func (s site) approvalHistoryMore(w http.ResponseWriter, r *http.Request) {
	s.historyMore(w, r, "approvalHistory", s.approvalHistoryOf)
}
