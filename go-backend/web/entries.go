// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
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

// kindFilter is Entries' kind filter as the approved design draws it (redix-api #183): the kinds, each chosen when query holds it.
func kindFilter(p page, query url.Values) filter {
	kinds := filter{id: "entries.kindChoice", label: "entries.kind", several: true}
	for _, kind := range entryKinds {
		kinds.choices = append(kinds.choices, filterChoice{id: "entries.kind." + kind.name, name: "kinds", value: kind.name, word: p.words.Of(kind.word),
			chosen: slices.Contains(query["kinds"], kind.name)})
	}
	return kinds
}

// statusFilter is Entries' status filter as the approved design draws it (redix-api #183): the statuses, each chosen when query holds it.
func statusFilter(p page, query url.Values) filter {
	statuses := filter{id: "entries.statusChoice", label: "entries.status", several: true}
	for _, status := range entryStatuses {
		statuses.choices = append(statuses.choices, filterChoice{id: "entries.status." + status, name: "statuses", value: status, word: p.words.Of("entries." + status),
			chosen: slices.Contains(query["statuses"], status)})
	}
	return statuses
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
	p, ok := s.working(w, r)
	if !ok {
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
	draw(w, r, http.StatusOK, entryPanel(p, row, said{asking: r.URL.Query().Get("asking")}))
}

// said is what the details panel says beside an entry: what an action just did, why it could not, or what an action asks first - "remove" its
// question, "reject" its reason.
type said struct {
	notice, refusal, asking string
}

// entryAction runs an action on the entry the path names - approve, send or remove - through the rule the app's route runs, behind the same gate,
// in one change, and answers the panel again saying what it did or why it could not, the entry's row in the list beside it; a removed entry
// leaves the list (redix-api #166). 404 for an action there is none of, or with no project; 403 without the permission the action needs; in a
// finished project the panel says it cannot change (AC-04.1).
func (s site) entryAction(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	needs, known := actionNeeds[action]
	if !p.inProject() || !known {
		http.NotFound(w, r)
		return
	}
	if !p.may(needs) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	entryID, _ := uuid.Parse(r.PathValue("id"))
	if p.finished() {
		s.answerEntry(w, r, p, entryID, said{refusal: "projects.projectFinished"})
		return
	}
	shown := said{notice: map[string]string{"send": "expenses.sent", "remove": "swipe.removed"}[action]}
	err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		switch action {
		case "approve":
			_, notApproved, err := entries.ApproveEach(c, p.caller, []uuid.UUID{entryID})
			if err == nil && len(notApproved) > 0 {
				shown = said{refusal: "approvals." + notApproved[0].Code}
			}
			return err
		case "send":
			return entries.Submit(c, p.caller, entryID)
		case "reject":
			return entries.Reject(c, p.caller, entryID, rejectionReason(r))
		default:
			return entries.Remove(c, p.caller, entryID)
		}
	})
	var refused changes.Refusal
	if errors.As(err, &refused) {
		shown, err = said{refusal: refusalWord(refused, action)}, nil
	}
	if action == "reject" && shown.refusal == "approvals.reasonRequired" {
		shown.asking = "reject"
	}
	if err != nil {
		failed(w)
		return
	}
	s.answerEntry(w, r, p, entryID, shown)
}

// answerEntry answers the details panel with the entry entryID as it now stands and what shown says, its row in the list beside it; once the
// entry has left the list, the panel says so and the row leaves too.
func (s site) answerEntry(w http.ResponseWriter, r *http.Request, p page, entryID uuid.UUID, shown said) {
	row, found, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, entryID)
	if err != nil {
		failed(w)
		return
	}
	if !found {
		draw(w, r, http.StatusOK, entryGone(p, entryID, shown))
		return
	}
	draw(w, r, http.StatusOK, entryPanel(p, row, shown), rowBeside(p, row))
}

// actionNeeds is the permission each of an entry's actions needs before its own rule runs, as the app's route gate asks it (access.Gate.Changes):
// approving and rejecting need ApproveEntries, sending RecordEntries, and removing only that the project is read, the entry deciding the rest.
var actionNeeds = map[string]access.Permission{"approve": access.ApproveEntries, "reject": access.ApproveEntries, "send": access.RecordEntries,
	"remove": access.ReadProject}

// refusalWord is the word of a refusal of an entry's action, as the app says it: an entry no longer there, one the caller may not take away, or
// the refusal's own code - under Approvals' words for a rejection, as the app's reason sheet says it.
func refusalWord(refused changes.Refusal, action string) string {
	switch {
	case refused.Status == http.StatusNotFound:
		return "approvals.unknownEntry"
	case refused.Code == "":
		return "entries.notYours"
	case action == "reject":
		return "approvals." + refused.Code
	default:
		return "entries." + refused.Code
	}
}

// fileWord is the word of a file's role, as the app's entry names its files: فاتورة, سند استلام, إيصال تحويل, ملف.
func fileWord(role string) string {
	return "entries.file" + strings.ToUpper(role[:1]) + role[1:]
}

// quickReasons are the reasons the app's reason sheet offers a rejection, by their words.
var quickReasons = []string{"approvals.reasonUnclear", "approvals.reasonAmount", "approvals.reasonItem", "approvals.reasonDuplicate"}

// rowID is the id of an entry's row in the list in front: Approvals' card, or the Entries' and the home's row, which an action's answer finds.
func (p page) rowID(entryID uuid.UUID) string {
	if p.view.Active == "approvals" {
		return "approvals.card." + entryID.String()
	}
	return "entries.row." + entryID.String()
}

// mayChange says whether p's caller may change row, as the entry's own rule decides it (entries.Row.Changeable); never in a finished project.
func (p page) mayChange(row entries.Row) bool { return !p.finished() && row.Changeable }

// mayRemove says whether p's caller may take row away, as entries.Remove decides it: an approved one with RemoveApprovedEntries, any other with
// DeleteEntries when the entry's own rule allows changing it; never in a finished project.
func (p page) mayRemove(row entries.Row) bool {
	if row.Status.Name() == "approved" {
		return !p.finished() && p.may(access.RemoveApprovedEntries)
	}
	return !p.finished() && p.may(access.DeleteEntries) && row.Changeable
}
