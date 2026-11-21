// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"net/url"
	"strconv"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
)

// entriesShown is how many entries a page of the list holds; Show more adds as many again, as the app's list pages (redix-app #105). A
// computer's screen holds more rows than a phone's ten.
const entriesShown = 30

// entryKinds and entryStatuses are the kinds and statuses the list's filters offer, by the names the API knows them, with their words, as the
// app's chips name them (redix-app #104).
var (
	entryKinds = []struct{ name, word string }{{"purchase", "entries.purchases"}, {"contractorPayment", "entries.contractorPayments"},
		{"revenue", "categories.tableRevenue"}, {"capitalIn", "capital.contribution"}, {"capitalOut", "capital.withdrawal"}}
	entryStatuses = []string{"pendingApproval", "approved", "rejected", "draft"}
)

// entriesTab is the project's entries as the app lists them (UC-028, redix-api #166): searched and filtered by the query, as the app's list asks
// the API, a page at a time; the project list while the caller chooses a project. The error is the database's.
func (s site) entriesTab(r *http.Request, p page) (templ.Component, error) {
	if !p.inProject() {
		return projectList(p), nil
	}
	query := r.URL.Query()
	take, _ := strconv.Atoi(query.Get("take"))
	take = max(take, entriesShown)
	rows, err := entries.Listed(r.Context(), s.Database.Queries(), p.project.ID, p.caller, query, take+1)
	if err != nil {
		return nil, err
	}
	return entriesList(p, query, rows[:min(len(rows), take)], len(rows) > take, take), nil
}

// narrows says whether query narrows the list at all, so Clear has something to clear.
func narrows(query url.Values) bool {
	for _, name := range []string{"text", "mine", "kinds", "statuses", "from", "to", "number"} {
		if query.Get(name) != "" {
			return true
		}
	}
	return false
}

// takeMore is the query of the list's next page: the same filters, as many entries again.
func takeMore(take int) string { return `{"take":"` + strconv.Itoa(take+entriesShown) + `"}` }

// entryDetails is an entry of the project in the details panel, as the lists show it (redix-api #166): 404 for one the caller does not see, and
// the sign-in form in the whole shell for a session that ended.
func (s site) entryDetails(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	caller, signedIn, err := s.session(w, r)
	if err != nil {
		failed(w)
		return
	}
	if !signedIn {
		p.content = signInForm(p, ended(r), "")
		reshell(w, r, p)
		return
	}
	if p, err = s.open(r, p, caller); err != nil {
		failed(w)
		return
	}
	entryID, _ := uuid.Parse(r.PathValue("id"))
	row, found, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, entryID)
	if err != nil {
		failed(w)
		return
	}
	if !found || !p.inProject() {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, entryPanel(p, row))
}
