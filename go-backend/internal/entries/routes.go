// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"context"
	"database/sql"
	"errors"
	"math"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/ledger"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// approvalReasonMaxLength is the longest reason for a rejection (ApprovalsEndpoints.ReasonMaxLength).
const approvalReasonMaxLength = 500

// Routes keeps the ledger.
type Routes struct {
	database db.DB
	clock    clock.Clock
	writer   changes.Writer
}

// Map routes the ledger. Recording and changing an entry ask only that the caller reads the project, since which table they may enter is the
// table's to say; sending and describing need RecordEntries; deciding needs ApproveEntries; deleting follows the entry, DeleteEntries before
// approval and RemoveApprovedEntries after it (redix-api #119).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, clock: clock, writer: changes.Writer{Database: database, Clock: clock}}
	const project = "/projects/{projectId:guid}"
	reads := func(handler access.Handler) http.HandlerFunc { return gate.Needs(access.ReadProject, handler) }
	router.Handle("GET", project+"/entry-tables", reads(s.entryTables))
	router.Handle("GET", project+"/items/{itemId:guid}/tables", reads(s.itemTables))
	router.Handle("GET", project+"/tables", reads(s.sightTables))
	router.Handle("GET", project+"/entries/attachable", gate.Needs(access.RecordEntries, s.attachable))
	router.Handle("POST", project+"/entries/{entryId:guid}/invoice", gate.Changes(access.RecordEntries, s.attach))
	router.Handle("GET", project+"/reports/items/{itemId:guid}", gate.Needs(access.SeeReports, s.itemReportRoute))
	router.Handle("GET", project+"/reports/contracts/{contractId:guid}", gate.Needs(access.SeeReports, s.contractReportRoute))
	router.Handle("GET", project+"/accounts/history", reads(s.accountHistory))
	router.Handle("GET", project+"/partners/history", reads(s.partnerHistory))
	router.Handle("GET", project+"/approvals/history", gate.Needs(access.ApproveEntries, s.approvalHistory))
	router.Handle("GET", project+"/history", gate.Needs(access.SeeProjectHistory, s.projectHistory))
	router.Handle("GET", "/me/history", gate.Working(s.myHistory))
	router.Handle("POST", project+"/entries", gate.Changes(access.ReadProject, s.record))
	router.Handle("PUT", project+"/entries/{entryId:guid}", gate.Changes(access.ReadProject, s.change))
	router.Handle("GET", project+"/entries/{entryId:guid}", reads(s.read))
	router.Handle("GET", project+"/entries/{entryId:guid}/row", reads(s.row))
	router.Handle("GET", project+"/entries/mine", reads(s.mine))
	router.Handle("GET", project+"/entries", reads(s.list))
	router.Handle("POST", project+"/entries/{entryId:guid}/submit", gate.Changes(access.RecordEntries, s.submit))
	router.Handle("PUT", project+"/entries/{entryId:guid}/description", gate.Changes(access.RecordEntries, s.describe))
	router.Handle("DELETE", project+"/entries/{entryId:guid}", gate.Changes(access.ReadProject, s.delete))
	router.Handle("POST", project+"/entries/{entryId:guid}/restore", gate.Changes(access.RemoveApprovedEntries, s.restore))
	router.Handle("GET", project+"/entries/pending", gate.Needs(access.ApproveEntries, s.pending))
	router.Handle("POST", project+"/entries/approve", gate.Changes(access.ApproveEntries, s.approve))
	router.Handle("POST", project+"/entries/{entryId:guid}/reject", gate.Changes(access.ApproveEntries, s.reject))
}

// entry is the route's entry when it is the project's, not removed and of a table caller sees (Entry.OfProjectAsync, BR-33, redix-api #119,
// #106 R9): 404 otherwise.
func (s Routes) entry(w http.ResponseWriter, r *http.Request, caller access.Caller) (store.Entry, bool) {
	entry, err := seenEntry(changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}, caller, web.ID(r, "entryId"))
	return entry, !changes.Answered(w, r, err)
}

// seenEntry is the entry entryID of c's project when caller sees it: changes.NotFound for one removed, one of a table they do not see (#106 R9),
// or none.
func seenEntry(c changes.Change, caller access.Caller, entryID uuid.UUID) (store.Entry, error) {
	entry, err := changes.Found(c.Q.EntryOf(c.Ctx, store.EntryOfParams{ID: entryID, ProjectID: c.ProjectID}))
	if err == nil && entry.Status == ledger.Removed {
		err = changes.NotFound
	}
	if err == nil {
		err = seen(c.Ctx, c.Q, caller.Access, entry)
	}
	return entry, err
}

// seen is NotFound for an entry of a table held does not see (#106 R9); an entry with no table, a capital movement, is the capital ticks'.
func seen(ctx context.Context, q *store.Queries, held access.Access, entry store.Entry) error {
	tables, err := q.TableOfEntry(ctx, entry.ID)
	if err == nil && slices.ContainsFunc(tables, func(table uuid.UUID) bool { return !held.Sees(entry.ProjectID, table) }) {
		return changes.NotFound
	}
	return err
}

// submit sends a draft or a rejected entry for approval and answers it (UC-028): notYourEntry (403) for one the caller may not handle (BR-42);
// entryNotSubmittable (409) for one already sent; entryIncomplete (400) with every required column left empty under missing (FR-41).
func (s Routes) submit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	entryID := web.ID(r, "entryId")
	if s.writer.Write(w, r, caller, func(c changes.Change) error { return Submit(c, caller, entryID) }) {
		s.answerView(w, r, entryID)
	}
}

// Submit sends the entry entryID of c's project for approval as caller, as submit answers it (UC-028): the rule both doors call (redix-api
// #166). It refuses notYourEntry (403), entryNotSubmittable (409) and entryIncomplete (400) as submit describes, and changes.NotFound for an
// entry caller does not see.
func Submit(c changes.Change, caller access.Caller, entryID uuid.UUID) error {
	entry, err := seenEntry(c, caller, entryID)
	if err != nil {
		return err
	}
	if !mayHandle(entry, caller) {
		return changes.Refuse(http.StatusForbidden, "notYourEntry")
	}
	if entry.Status != ledger.Draft && entry.Status != ledger.Rejected {
		return changes.Refuse(http.StatusConflict, "entryNotSubmittable")
	}
	missing, err := missingOfEntry(c, entry)
	if err != nil {
		return err
	}
	if len(missing) > 0 {
		return changes.RefuseOver(http.StatusBadRequest, "entryIncomplete", "missing", missing)
	}
	after := entry
	after.Status = ledger.PendingApproval
	if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
		return err
	}
	was, is := history.Changed(fields(entry), fields(after))
	if err := c.Record(history.Submit, "Entries", entry.ID, was, is); err != nil {
		return err
	}
	return sendOff(c, after, true)
}

// itemOf is the item entry went into, as known holds it: an expense's or a revenue's, or the project's capital item for a partner's money.
func itemOf(c changes.Change, entry store.Entry, known facts) (uuid.UUID, error) {
	itemID := known.expenses[entry.ID].ItemID
	if row, isRevenue := known.revenues[entry.ID]; isRevenue {
		itemID = row.ItemID
	}
	if entry.Kind != ledger.CapitalIn && entry.Kind != ledger.CapitalOut {
		return itemID, nil
	}
	items, err := c.Q.ItemsOf(c.Ctx, entry.ProjectID)
	for _, item := range items {
		if item.Type == int64(structure.Capital) {
			itemID = item.ID
		}
	}
	return itemID, err
}

// missingOfEntry is the required columns the entry gives no value, among those of its item its kind is asked (EntriesEndpoints.MissingOfAsync): an
// expense's or revenue's item, or the project's capital item for a partner's money.
func missingOfEntry(c changes.Change, entry store.Entry) ([]string, error) {
	known, err := factsOf(c, []store.Entry{entry})
	if err != nil {
		return nil, err
	}
	itemID, err := itemOf(c, entry, known)
	if err != nil {
		return nil, err
	}
	columns, err := c.Q.ColumnsOfItem(c.Ctx, itemID)
	if err != nil {
		return nil, err
	}
	asks := asksOf(entry.Kind)
	var asked []store.ItemColumn
	for _, column := range columns {
		if !column.IsHidden && asks.asks(column.Key) {
			asked = append(asked, column)
		}
	}
	values := web.Ordered{}
	if err := values.UnmarshalJSON([]byte(entry.ColumnValues)); err != nil {
		return nil, err
	}
	return missingOf(entry.Description, values.Values, asked), nil
}

// describeEntryRequest is the body PUT /projects/{projectId}/entries/{entryId}/description reads (UC-033).
type describeEntryRequest struct {
	Description string `json:"description"`
}

// describe changes an entry's description, allowed even once it is approved (AC-50.2, BR-11): notYourEntry (403), invalidDescription (400).
// Its line is written whether or not anything changed, as .NET's AddHistory wrote it.
func (s Routes) describe(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed describeEntryRequest
	if !web.Read(w, r, &typed) {
		return
	}
	entry, found := s.entry(w, r, caller)
	if !found {
		return
	}
	if !mayHandle(entry, caller) {
		problem.Refuse(w, r, http.StatusForbidden, "notYourEntry")
		return
	}
	description := strings.TrimSpace(typed.Description)
	if tooLong(description, descriptionMaxLength) {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidDescription")
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		after := entry
		after.Description = description
		if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
			return err
		}
		was, is := history.Changed(fields(entry), fields(after))
		return c.Record(history.Update, "Entries", entry.ID, was, is)
	})
}

// delete deletes an entry (UC-033): one never approved goes with all it holds, for whoever deletes entries, with the refusals of a change; an
// approved one is removed softly for whoever removes approved entries (redix-api #119). Without the permission the entry asks for, 403.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	entryID := web.ID(r, "entryId")
	s.writer.Done(w, r, caller, func(c changes.Change) error { return Remove(c, caller, entryID) })
}

// forbidden is the refusal of an entry the caller sees but may not take away: 403 with no code, as the gate refuses a permission.
var forbidden = changes.Refusal{Status: http.StatusForbidden}

// Remove takes the entry entryID of c's project away as caller, as delete answers it (UC-033): the rule both doors call (redix-api #166). An
// approved one is removed softly for whoever removes approved entries (redix-api #119); any other goes with all it holds for whoever deletes
// entries, with the refusals of a change; 403 without the permission the entry asks for; changes.NotFound for one caller does not see.
func Remove(c changes.Change, caller access.Caller, entryID uuid.UUID) error {
	entry, err := seenEntry(c, caller, entryID)
	if err != nil {
		return err
	}
	if entry.Status == ledger.Approved {
		if !caller.Access.Holds(entry.ProjectID, access.RemoveApprovedEntries) {
			return forbidden
		}
		return setStatus(c, entry, ledger.Removed, history.Delete)
	}
	if !caller.Access.Holds(entry.ProjectID, access.DeleteEntries) {
		return forbidden
	}
	if status, code := refusalOfChange(entry, caller); code != "" {
		return changes.Refuse(status, code)
	}
	return Delete(c, entry)
}

// Delete removes an entry never approved, with all it holds - its rows, its files, its decisions and what anyone was told of it - History keeping
// what it held (EntryChanges.DeleteAsync): the one way an entry goes, for its own route and a partner's delete alike.
func Delete(c changes.Change, entry store.Entry) error {
	files, err := filesOf(c, entry.ID)
	if err != nil {
		return err
	}
	if err := removeRows(c, entry.ID, files); err != nil {
		return err
	}
	if err := c.Q.RemoveDecisions(c.Ctx, entry.ID); err != nil {
		return err
	}
	told, err := c.Q.NotificationsAbout(c.Ctx, entry.ID)
	if err != nil {
		return err
	}
	for _, id := range told {
		if err := c.Q.RemoveNotificationEntry(c.Ctx, id); err != nil {
			return err
		}
		if err := c.Q.RemoveNotification(c.Ctx, id); err != nil {
			return err
		}
	}
	if err := c.Q.RemoveEntry(c.Ctx, entry.ID); err != nil {
		return err
	}
	return c.Record(history.Delete, "Entries", entry.ID, fields(entry), nil)
}

// setStatus moves entry to status with its line of action.
func setStatus(c changes.Change, entry store.Entry, status int64, action history.Action) error {
	after := entry
	after.Status = status
	if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
		return err
	}
	was, is := history.Changed(fields(entry), fields(after))
	return c.Record(action, "Entries", entry.ID, was, is)
}

// restore brings back an approved entry removed softly, approved again with all it held (redix-api #119): 404 for one not removed.
func (s Routes) restore(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	entry, err := changes.Found(s.database.Queries().EntryOf(r.Context(), store.EntryOfParams{ID: web.ID(r, "entryId"), ProjectID: web.ID(r, "projectId")}))
	if err == nil && entry.Status != ledger.Removed {
		err = changes.NotFound
	}
	if err == nil {
		err = seen(r.Context(), s.database.Queries(), caller.Access, entry)
	}
	if changes.Answered(w, r, err) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error { return setStatus(c, entry, ledger.Approved, history.Restore) })
}

// approveEntriesRequest is the body POST /projects/{projectId}/entries/approve reads (UC-032).
type approveEntriesRequest struct {
	EntryIDs []uuid.UUID `json:"entryIds"`
}

// ApprovalFailure is an entry that could not be approved, with its reason: part of what POST /projects/{projectId}/entries/approve answers (UC-032),
// and what the web's approval says.
type ApprovalFailure struct {
	EntryID uuid.UUID `json:"entryId"`
	Code    string    `json:"code"`
}

// approvalView is what POST /projects/{projectId}/entries/approve answers (UC-032).
type approvalView struct {
	Approved []uuid.UUID       `json:"approved"`
	Failed   []ApprovalFailure `json:"failed"`
}

// approve approves the entries named, each on its own (FR-19, AC-19.2): what cannot be approved comes back by id with its reason - unknownEntry,
// entryNotPending - and nothing is approved in silence. The first expense on a supplier waiting for approval approves it too (AC-44.1).
func (s Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed approveEntriesRequest
	if !web.Read(w, r, &typed) {
		return
	}
	view := approvalView{}
	done := s.writer.Write(w, r, caller, func(c changes.Change) error {
		var err error
		view.Approved, view.Failed, err = ApproveEach(c, caller, typed.EntryIDs)
		return err
	})
	if done {
		web.JSON(w, r, http.StatusOK, view)
	}
}

// ApproveEach approves the entries ids name, each on its own, as approve answers them (FR-19, AC-19.2): the rule both doors call (redix-api
// #166). It is those approved, and those that could not be with their reason - unknownEntry, entryNotPending; the error is the database's.
func ApproveEach(c changes.Change, caller access.Caller, ids []uuid.UUID) ([]uuid.UUID, []ApprovalFailure, error) {
	var named, approved []uuid.UUID
	var failed []ApprovalFailure
	for _, id := range ids {
		if !slices.Contains(named, id) {
			named = append(named, id)
		}
	}
	for _, id := range named {
		entry, err := c.Q.EntryOf(c.Ctx, store.EntryOfParams{ID: id, ProjectID: c.ProjectID})
		if err == nil {
			err = seen(c.Ctx, c.Q, caller.Access, entry)
		}
		switch {
		case errors.Is(err, sql.ErrNoRows) || errors.Is(err, changes.NotFound):
			failed = append(failed, ApprovalFailure{id, "unknownEntry"})
		case err != nil:
			return nil, nil, err
		case entry.Status != ledger.PendingApproval:
			failed = append(failed, ApprovalFailure{id, "entryNotPending"})
		default:
			if err := Approve(c, entry, c.Now); err != nil {
				return nil, nil, err
			}
			approved = append(approved, id)
		}
	}
	return approved, failed, nil
}

// Approve approves entry, one waiting, as counting from approvedAt (ApprovalsEndpoints.ApproveAsync, BR-26, BR-43): its decision, its line, who
// entered it told unless they decided it themselves, and its supplier approved when it waits.
func Approve(c changes.Change, entry store.Entry, approvedAt moment.Moment) error {
	after := entry
	after.Status, after.ApprovedAt = ledger.Approved, approvedAt
	if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
		return err
	}
	if err := c.Q.AddDecision(c.Ctx, store.AddDecisionParams{EntryID: entry.ID, Action: approvedBy, UserID: c.Actor, At: c.Now}); err != nil {
		return err
	}
	was, is := history.Changed(fields(entry), fields(after))
	if err := c.Record(history.Approve, "Entries", entry.ID, was, is); err != nil {
		return err
	}
	if entry.CreatedByUserID != c.Actor {
		if err := tell(c, entry, entry.CreatedByUserID, notifications.EntryApproved); err != nil {
			return err
		}
	}
	return approveItsSupplier(c, entry)
}

// approveItsSupplier approves the supplier of the entry - its purchase's, or its contract's contractor - when it waits (AC-44.1).
func approveItsSupplier(c changes.Change, entry store.Entry) error {
	facts, err := factsOf(c, []store.Entry{entry})
	if err != nil {
		return err
	}
	supplier := facts.supplierOf(entry)
	if supplier.ID == uuid.Nil || supplier.Status != 1 {
		return nil
	}
	after, err := c.Q.SupplierOf(c.Ctx, store.SupplierOfParams{ID: supplier.ID, ProjectID: c.ProjectID})
	if err != nil {
		return err
	}
	before := after
	after.Status = 0
	if err := c.Q.SaveSupplier(c.Ctx, store.SaveSupplierParams(after)); err != nil {
		return err
	}
	return c.Record(history.Update, "Suppliers", after.ID, map[string]any{"Status": before.Status}, map[string]any{"Status": after.Status})
}

// rejectEntryRequest is the body POST /projects/{projectId}/entries/{entryId}/reject reads (UC-032).
type rejectEntryRequest struct {
	Reason string `json:"reason"`
}

// reject rejects an entry, saying why (AC-19.1): it returns to who entered it, to fix and send again. reasonRequired (400) for no reason or one
// past 500 characters, entryNotPending (409) for one nobody waits on.
func (s Routes) reject(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed rejectEntryRequest
	if !web.Read(w, r, &typed) {
		return
	}
	entry, found := s.entry(w, r, caller)
	if !found {
		return
	}
	reason := strings.TrimSpace(typed.Reason)
	if reason == "" || tooLong(reason, approvalReasonMaxLength) {
		problem.Refuse(w, r, http.StatusBadRequest, "reasonRequired")
		return
	}
	if entry.Status != ledger.PendingApproval {
		problem.Refuse(w, r, http.StatusConflict, "entryNotPending")
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		if err := setStatus(c, entry, ledger.Rejected, history.Reject); err != nil {
			return err
		}
		if err := c.Q.AddDecision(c.Ctx, store.AddDecisionParams{EntryID: entry.ID, Action: rejectedBy, UserID: c.Actor, At: c.Now, Reason: reason}); err != nil {
			return err
		}
		if entry.CreatedByUserID == c.Actor {
			return nil
		}
		return tell(c, entry, entry.CreatedByUserID, notifications.EntryRejected)
	})
}

// seenBy is the project's entries caller may see, newest first, never a removed one, with their facts.
func (s Routes) seenBy(c changes.Change, caller access.Caller) ([]store.Entry, facts, error) {
	c.Access = caller.Access
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return nil, facts{}, err
	}
	known, err := factsOf(c, all)
	if err != nil {
		return nil, facts{}, err
	}
	return known.seenAmong(caller, c.ProjectID, all), known, nil
}

// seenAmong is those of entries caller may see, in their order (EntriesEndpoints.SeenByAsync): a draft only for who entered it (BR-42), and a
// partner's money only for whoever sees that partner (BR-31).
func (f facts) seenAmong(caller access.Caller, projectID uuid.UUID, entries []store.Entry) []store.Entry {
	seesAll, chosen := caller.Access.Holds(projectID, access.SeeAllCapital), caller.Access.ChosenIn(projectID)
	if !caller.Access.Holds(projectID, access.SeeChosenCapital) {
		chosen = nil
	}
	var seen []store.Entry
	for _, entry := range entries {
		partner, isCapital := f.capital[entry.ID]
		if (entry.Status != ledger.Draft || entry.CreatedByUserID == caller.UserID) && (seesAll || !isCapital || slices.Contains(chosen, partner)) &&
			f.tableSeen(caller.Access, projectID, entry.ID) {
			seen = append(seen, entry)
		}
	}
	return seen
}

// entryBatch is how many entries a list reads at a time, newest first, with their facts, until it holds what it shows; its first batch is as
// many as it shows, so a page whose entries are all seen and kept reads no more (#87).
const entryBatch = 500

// newest is a moment after every entry's, where a list begins.
var newest = moment.Of(time.Date(9999, 12, 31, 0, 0, 0, 0, time.UTC))

// everyRow shows every row a list keeps.
func everyRow(Row) bool { return true }

// Listed is the entries of projectID that caller sees and query's search keeps - kinds, statuses, text, mine, from, to, number, as the app's
// list sends them - newest first, take of them, every one when take is 0: the list both doors show (UC-028, redix-app #104, redix-api #166). The
// error is the database's.
func Listed(ctx context.Context, read *store.Queries, projectID uuid.UUID, caller access.Caller, query url.Values, take int) ([]Row, error) {
	return rowsOf(ctx, read, projectID, caller, func(store.Entry, facts) bool { return true }, searchOf(query).keeps, take)
}

// Newest is the newest take entries of projectID that caller sees, as the lists show them: the home's latest entries, on the phone and on the web
// (redix-api #165). The error is the database's.
func Newest(ctx context.Context, read *store.Queries, projectID uuid.UUID, caller access.Caller, take int) ([]Row, error) {
	return rowsOf(ctx, read, projectID, caller, func(store.Entry, facts) bool { return true }, everyRow, take)
}

// rowsOf is the entries of projectID keep keeps of those caller sees, as the lists show them and show keeps (EntriesEndpoints.RowsAsync), newest
// first: read a batch at a time with that batch's facts, until take are kept, every one when take is 0 (#87).
func rowsOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, caller access.Caller, keep func(store.Entry, facts) bool, show func(Row) bool, take int) ([]Row, error) {
	c := changes.Change{Ctx: ctx, Q: read, ProjectID: projectID, Access: caller.Access}
	known, err := sharedFactsOf(c)
	if err != nil {
		return nil, err
	}
	var views []Row
	after := store.EntriesBeforeParams{ProjectID: c.ProjectID, CreatedAt: newest, Number: math.MaxInt64, Take: entryBatch}
	if take > 0 {
		after.Take = int64(min(take, entryBatch))
	}
	for {
		batch, err := c.Q.EntriesBefore(c.Ctx, after)
		if err != nil || len(batch) == 0 {
			return views, err
		}
		after.CreatedAt, after.Number, after.Take = batch[len(batch)-1].CreatedAt, batch[len(batch)-1].Number, entryBatch
		if err := known.readEntries(c, batch); err != nil {
			return nil, err
		}
		seen := known.seenAmong(caller, c.ProjectID, batch)
		rejections, err := c.Q.RejectionsOf(c.Ctx, entriesNamed(seen))
		if err != nil {
			return nil, err
		}
		lastReason := map[uuid.UUID]string{}
		for _, row := range rejections {
			lastReason[row.EntryID] = row.Reason
		}
		for _, entry := range seen {
			if !keep(entry, known) {
				continue
			}
			view := known.row(entry, caller.UserID)
			_, refused := refusalOfChange(entry, caller)
			view.Changeable = refused == ""
			if entry.Status == ledger.Rejected {
				view.RejectionReason = lastReason[entry.ID]
			}
			if !show(view) {
				continue
			}
			views = append(views, view)
			if len(views) == take {
				return views, nil
			}
		}
	}
}

// mine is where the caller's own entries stand, newest first (UC-030).
func (s Routes) mine(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	views, err := rowsOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), caller, func(entry store.Entry, _ facts) bool { return entry.CreatedByUserID == caller.UserID }, everyRow, 0)
	s.answer(w, r, views, err)
}

// row is one entry as the lists show it (findings row 201): 404 for one the caller does not see (BR-31, BR-42), or none.
func (s Routes) row(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	view, found, err := RowOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), caller, web.ID(r, "entryId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !found {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	web.JSON(w, r, http.StatusOK, view)
}

// RowOf is the entry entryID of projectID as the lists show it, when caller sees it (BR-31, BR-42): the row the app reads and the web's details
// panel shows (findings row 201, redix-api #166); false for one they do not see, or none. The error is the database's.
func RowOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, caller access.Caller, entryID uuid.UUID) (Row, bool, error) {
	views, err := rowsOf(ctx, read, projectID, caller, func(entry store.Entry, _ facts) bool { return entry.ID == entryID }, everyRow, 1)
	if err != nil || len(views) != 1 {
		return Row{}, false, err
	}
	return views[0], true, nil
}

// list is the entries of the project the caller may see, newest first (the owner, 8 Nov 2026), those a search keeps when anything narrows them
// (redix-app #104), the newest take of them when take is above 0 (redix-app #105), and with attachable only those a shared file can go on
// (redix-app #115 D6).
func (s Routes) list(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	query := r.URL.Query()
	search := searchOf(query)
	attachableOnly := strings.EqualFold(query.Get("attachable"), "true")
	take, _ := strconv.Atoi(query.Get("take"))
	kept, err := rowsOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), caller, func(entry store.Entry, _ facts) bool { return !attachableOnly || attachableBy(entry, caller) }, search.keeps,
		max(take, 0))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, kept)
}

func (s Routes) answer(w http.ResponseWriter, r *http.Request, views any, err error) {
	if !changes.Answered(w, r, err) {
		web.JSON(w, r, http.StatusOK, views)
	}
}

// read is one entry of the project with what its kind has (EntriesEndpoints.Read): 404 for one the caller may not see (BR-31), or none.
func (s Routes) read(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	seen, _, err := s.seenBy(c, caller)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !slices.ContainsFunc(seen, func(entry store.Entry) bool { return entry.ID == web.ID(r, "entryId") }) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	s.answerView(w, r, web.ID(r, "entryId"))
}

// answerView answers the entry entryID with what its kind has.
func (s Routes) answerView(w http.ResponseWriter, r *http.Request, entryID uuid.UUID) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	entry, err := c.Q.EntryOf(c.Ctx, store.EntryOfParams{ID: entryID, ProjectID: c.ProjectID})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	known, err := factsOf(c, []store.Entry{entry})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, known.view(entry))
}

// pending is what waits for a decision in the project, oldest first, each with its file when it has one (AC-18.1).
func (s Routes) pending(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
	waiting, err := c.Q.PendingEntriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	known, err := factsOf(c, waiting)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	var views []pendingView
	for _, entry := range waiting {
		if known.tableSeen(caller.Access, c.ProjectID, entry.ID) {
			views = append(views, known.pending(entry))
		}
	}
	web.JSON(w, r, http.StatusOK, views)
}
