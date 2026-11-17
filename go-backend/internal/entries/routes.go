package entries

import (
	"database/sql"
	"errors"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
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
	router.Handle("GET", project+"/entries/attachable", gate.Needs(access.RecordEntries, s.attachable))
	router.Handle("POST", project+"/entries/{entryId:guid}/invoice", gate.Changes(access.RecordEntries, s.attach))
	router.Handle("GET", project+"/reports/items/{itemId:guid}", gate.Needs(access.SeeReports, s.itemReportRoute))
	router.Handle("GET", project+"/reports/contracts/{contractId:guid}", gate.Needs(access.SeeReports, s.contractReportRoute))
	router.Handle("GET", project+"/accounts/history", reads(s.accountHistory))
	router.Handle("GET", project+"/partners/history", reads(s.partnerHistory))
	router.Handle("GET", project+"/approvals/history", gate.Needs(access.ApproveEntries, s.approvalHistory))
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

func (s Routes) now() moment.Moment { return moment.Of(s.clock.Now()) }

// entry is the route's entry when it is the project's and not removed (Entry.OfProjectAsync, BR-33, redix-api #119): 404 otherwise.
func (s Routes) entry(w http.ResponseWriter, r *http.Request) (store.Entry, bool) {
	entry, err := changes.Found(s.database.Queries().EntryOf(r.Context(), store.EntryOfParams{ID: web.ID(r, "entryId"), ProjectID: web.ID(r, "projectId")}))
	if err == nil && entry.Status == removed {
		err = changes.NotFound
	}
	return entry, !changes.Answered(w, r, err)
}

// submit sends a draft or a rejected entry for approval and answers it (UC-028): notYourEntry (403) for one the caller may not handle (BR-42);
// entryNotSubmittable (409) for one already sent; entryIncomplete (400) with every required column left empty under missing (FR-41).
func (s Routes) submit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	entry, found := s.entry(w, r)
	if !found {
		return
	}
	if !mayHandle(entry, caller) {
		problem.Refuse(w, r, http.StatusForbidden, "notYourEntry")
		return
	}
	if entry.Status != draft && entry.Status != rejected {
		problem.Refuse(w, r, http.StatusConflict, "entryNotSubmittable")
		return
	}
	missing, err := s.missingOf(r, entry)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if len(missing) > 0 {
		problem.RefuseOver(w, r, http.StatusBadRequest, "entryIncomplete", "missing", missing)
		return
	}
	sent := s.writer.Write(w, r, caller, func(c changes.Change) error {
		after := entry
		after.Status = pendingApproval
		if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
			return err
		}
		was, is := history.Changed(fields(entry), fields(after))
		if err := c.Record(history.Submit, "Entries", entry.ID, was, is); err != nil {
			return err
		}
		return sendOff(c, after, true)
	})
	if sent {
		s.answerView(w, r, entry.ID)
	}
}

// missingOf is the required columns the entry gives no value, among those of its item its kind is asked (EntriesEndpoints.MissingOfAsync): an
// expense's or revenue's item, or the project's capital item for a partner's money.
func (s Routes) missingOf(r *http.Request, entry store.Entry) ([]string, error) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: entry.ProjectID}
	known, err := factsOf(c, []store.Entry{entry})
	if err != nil {
		return nil, err
	}
	itemID := known.expenses[entry.ID].ItemID
	if row, isRevenue := known.revenues[entry.ID]; isRevenue {
		itemID = row.ItemID
	}
	items, err := c.Q.ItemsOf(c.Ctx, entry.ProjectID)
	if err != nil {
		return nil, err
	}
	for _, item := range items {
		if (entry.Kind == capitalIn || entry.Kind == capitalOut) && item.Type == int64(structure.Capital) {
			itemID = item.ID
		}
	}
	columns, err := c.Q.ColumnsOfItem(c.Ctx, itemID)
	if err != nil {
		return nil, err
	}
	var asks form = revenueForm{}
	switch entry.Kind {
	case purchase:
		asks = purchaseForm{}
	case contractorPayment:
		asks = contractForm{}
	}
	var asked []store.ItemColumn
	for _, column := range columns {
		if !column.IsHidden && asks.asks(column.Key) {
			asked = append(asked, column)
		}
	}
	values := web.Ordered{}
	values.UnmarshalJSON([]byte(entry.ColumnValues))
	return missingOf(entry.Description, values.Values, asked), nil
}

// describe changes an entry's description, allowed even once it is approved (AC-50.2, BR-11): notYourEntry (403), invalidDescription (400).
// Its line is written whether or not anything changed, as .NET's AddHistory wrote it.
func (s Routes) describe(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed struct {
		Description string `json:"description"`
	}
	if !web.Read(w, r, &typed) {
		return
	}
	entry, found := s.entry(w, r)
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
	entry, found := s.entry(w, r)
	if !found {
		return
	}
	if entry.Status == approved {
		if !caller.Access.Holds(entry.ProjectID, access.RemoveApprovedEntries) {
			problem.Status(w, r, http.StatusForbidden)
			return
		}
		s.writer.Done(w, r, caller, func(c changes.Change) error { return setStatus(c, entry, removed, history.Delete) })
		return
	}
	if !caller.Access.Holds(entry.ProjectID, access.DeleteEntries) {
		problem.Status(w, r, http.StatusForbidden)
		return
	}
	if status, code := refusalOfChange(entry, caller); code != "" {
		problem.Refuse(w, r, status, code)
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error { return Delete(c, entry) })
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
	if err == nil && entry.Status != removed {
		err = changes.NotFound
	}
	if changes.Answered(w, r, err) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error { return setStatus(c, entry, approved, history.Restore) })
}

// approve approves the entries named, each on its own (FR-19, AC-19.2): what cannot be approved comes back by id with its reason - unknownEntry,
// entryNotPending - and nothing is approved in silence. The first expense on a supplier waiting for approval approves it too (AC-44.1).
func (s Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed struct {
		EntryIDs []uuid.UUID `json:"entryIds"`
	}
	if !web.Read(w, r, &typed) {
		return
	}
	type failure struct {
		EntryID uuid.UUID `json:"entryId"`
		Code    string    `json:"code"`
	}
	view := struct {
		Approved []uuid.UUID `json:"approved"`
		Failed   []failure   `json:"failed"`
	}{Approved: []uuid.UUID{}, Failed: []failure{}}
	done := s.writer.Write(w, r, caller, func(c changes.Change) error {
		var named []uuid.UUID
		for _, id := range typed.EntryIDs {
			if !slices.Contains(named, id) {
				named = append(named, id)
			}
		}
		for _, id := range named {
			entry, err := c.Q.EntryOf(c.Ctx, store.EntryOfParams{ID: id, ProjectID: c.ProjectID})
			switch {
			case errors.Is(err, sql.ErrNoRows):
				view.Failed = append(view.Failed, failure{id, "unknownEntry"})
			case err != nil:
				return err
			case entry.Status != pendingApproval:
				view.Failed = append(view.Failed, failure{id, "entryNotPending"})
			default:
				if err := Approve(c, entry, c.Now); err != nil {
					return err
				}
				view.Approved = append(view.Approved, id)
			}
		}
		return nil
	})
	if done {
		web.JSON(w, r, http.StatusOK, view)
	}
}

// Approve approves entry, one waiting, as counting from approvedAt (ApprovalsEndpoints.ApproveAsync, BR-26, BR-43): its decision, its line, who
// entered it told unless they decided it themselves, and its supplier approved when it waits.
func Approve(c changes.Change, entry store.Entry, approvedAt moment.Moment) error {
	after := entry
	after.Status, after.ApprovedAt = approved, approvedAt
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

// reject rejects an entry, saying why (AC-19.1): it returns to who entered it, to fix and send again. reasonRequired (400) for no reason or one
// past 500 characters, entryNotPending (409) for one nobody waits on.
func (s Routes) reject(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed struct {
		Reason string `json:"reason"`
	}
	if !web.Read(w, r, &typed) {
		return
	}
	entry, found := s.entry(w, r)
	if !found {
		return
	}
	reason := strings.TrimSpace(typed.Reason)
	if reason == "" || tooLong(reason, approvalReasonMaxLength) {
		problem.Refuse(w, r, http.StatusBadRequest, "reasonRequired")
		return
	}
	if entry.Status != pendingApproval {
		problem.Refuse(w, r, http.StatusConflict, "entryNotPending")
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		if err := setStatus(c, entry, rejected, history.Reject); err != nil {
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

// seenBy is the project's entries caller may see, newest first, never a removed one (EntriesEndpoints.SeenByAsync): a draft only for who entered it
// (BR-42), and a partner's money only for whoever sees that partner (BR-31).
func (s Routes) seenBy(c changes.Change, caller access.Caller) ([]store.Entry, facts, error) {
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return nil, facts{}, err
	}
	known, err := factsOf(c, all)
	if err != nil {
		return nil, facts{}, err
	}
	seesAll, chosen := caller.Access.Holds(c.ProjectID, access.SeeAllCapital), caller.Access.ChosenIn(c.ProjectID)
	if !caller.Access.Holds(c.ProjectID, access.SeeChosenCapital) {
		chosen = nil
	}
	var seen []store.Entry
	for _, entry := range all {
		partner, isCapital := known.capital[entry.ID]
		if (entry.Status != draft || entry.CreatedByUserID == caller.UserID) && (seesAll || !isCapital || slices.Contains(chosen, partner)) {
			seen = append(seen, entry)
		}
	}
	return seen, known, nil
}

// rows is the entries keep keeps of those caller sees, as the lists show them (EntriesEndpoints.RowsAsync).
func (s Routes) rows(r *http.Request, caller access.Caller, keep func(store.Entry) bool) ([]rowView, error) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	seen, known, err := s.seenBy(c, caller)
	if err != nil {
		return nil, err
	}
	rejections, err := c.Q.RejectionsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return nil, err
	}
	lastReason := map[uuid.UUID]string{}
	for _, row := range rejections {
		lastReason[row.EntryID] = row.Reason
	}
	views := []rowView{}
	for _, entry := range seen {
		if !keep(entry) {
			continue
		}
		view := known.row(entry, caller.UserID)
		if entry.Status == rejected {
			view.RejectionReason = lastReason[entry.ID]
		}
		views = append(views, view)
	}
	return views, nil
}

func (s Routes) mine(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	views, err := s.rows(r, caller, func(entry store.Entry) bool { return entry.CreatedByUserID == caller.UserID })
	s.answer(w, r, views, err)
}

// row is one entry as the lists show it (findings row 201): 404 for one the caller does not see (BR-31, BR-42), or none.
func (s Routes) row(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	views, err := s.rows(r, caller, func(entry store.Entry) bool { return entry.ID == web.ID(r, "entryId") })
	if err == nil && len(views) != 1 {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views[0])
}

// list is the entries of the project the caller may see, newest first (Muteb, 8 Nov 2026), those a search keeps when anything narrows them
// (redix-app #104), the newest take of them when take is above 0 (redix-app #105), and with attachable only those a shared file can go on
// (redix-app #115 D6).
func (s Routes) list(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	query := r.URL.Query()
	search := searchOf(query["kinds"], query["statuses"], query.Get("text"), query.Get("mine"), query.Get("from"), query.Get("to"), query.Get("number"))
	attachableOnly := strings.EqualFold(query.Get("attachable"), "true")
	views, err := s.rows(r, caller, func(entry store.Entry) bool { return !attachableOnly || attachableBy(entry, caller) })
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	kept := []rowView{}
	for _, view := range views {
		if search.keeps(view) {
			kept = append(kept, view)
		}
	}
	if take, _ := strconv.Atoi(query.Get("take")); take > 0 && take < len(kept) {
		kept = kept[:take]
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
func (s Routes) pending(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
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
	views := []pendingView{}
	for _, entry := range waiting {
		views = append(views, known.pending(entry))
	}
	web.JSON(w, r, http.StatusOK, views)
}
