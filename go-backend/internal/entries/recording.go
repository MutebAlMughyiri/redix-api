// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"database/sql"
	"errors"
	"net/http"
	"slices"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"github.com/google/uuid"
)

// refusedOver is a refusal of a recorded entry, which names the column it was refused over, "" for none (Refusal.Of with columnKey).
func refusedOver(status int, code, column string) error {
	return changes.RefuseOver(status, code, "columnKey", column)
}

// record records an entry as a draft, or sent for approval (EntryRecording.Record, UC-022): the same id from whoever recorded it answers that
// entry, so a retried upload lands once (AC-16.3, UC-029); from anyone else entryExists (409), and an empty one invalidEntryId (400).
func (s Routes) record(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed Request
	if !web.Read(w, r, &typed) {
		return
	}
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	earlier, err := read.EntryOf(r.Context(), store.EntryOfParams{ID: typed.ID, ProjectID: projectID})
	if err != nil && !errors.Is(err, sql.ErrNoRows) {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if err == nil && earlier.CreatedByUserID == caller.UserID {
		web.JSON(w, r, http.StatusOK, statusView{ID: earlier.ID, Number: earlier.Number, Status: statusOf(earlier.Status)})
		return
	}
	if typed.ID == uuid.Nil {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidEntryId")
		return
	}
	if exists, err := read.EntryExists(r.Context(), typed.ID); err != nil || exists {
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
		} else {
			problem.Refuse(w, r, http.StatusConflict, "entryExists")
		}
		return
	}
	s.save(w, r, caller, typed, store.Entry{})
}

// statusView is where an entry stands (EntryStatusView): what answers an upload sent again.
type statusView struct {
	ID              uuid.UUID `json:"id"`
	Number          int64     `json:"number"`
	Status          named     `json:"status"`
	RejectionReason string    `json:"rejectionReason"`
}

// change changes an entry before it is approved (EntryRecording.Change, UC-033), through every check a new one passes; it may move to another
// table, of any kind: 404 for one not in the project, then the refusals of refusalOfChange.
func (s Routes) change(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed Request
	if !web.Read(w, r, &typed) {
		return
	}
	existing, found := s.entry(w, r, caller)
	if !found {
		return
	}
	if status, code := refusalOfChange(existing, caller); code != "" {
		problem.Refuse(w, r, status, code)
		return
	}
	typed.ID = existing.ID
	s.save(w, r, caller, typed, existing)
}

// refusalOfChange is why caller cannot change entry (EntryChanges.RefusalOf): entryApproved (409), since what is approved never changes (BR-11);
// notYourEntry (403) for one they may not handle; entryWaitingForDecision (409) for one waiting, unless they manage any entry.
func refusalOfChange(entry store.Entry, caller access.Caller) (int, string) {
	switch {
	case entry.Status == approved:
		return http.StatusConflict, "entryApproved"
	case !mayHandle(entry, caller):
		return http.StatusForbidden, "notYourEntry"
	case entry.Status == pendingApproval && !caller.Access.Holds(entry.ProjectID, access.ManageAnyEntry):
		return http.StatusConflict, "entryWaitingForDecision"
	}
	return 0, ""
}

// mayHandle says whether caller may act on entry at all (EntryChanges.MayHandle): they entered it, or it was sent and they manage any entry; a draft
// is its author's alone until it is sent (BR-42).
func mayHandle(entry store.Entry, caller access.Caller) bool {
	return entry.CreatedByUserID == caller.UserID || (entry.Status != draft && caller.Access.Holds(entry.ProjectID, access.ManageAnyEntry))
}

// save keeps typed as a new entry, or in place of existing (EntryRecording.SaveAsync): unknownTable (400) for a table the caller is not offered,
// then the refusals of refusal, naming the column; 201 with what was kept for a new entry, 200 for a changed one.
func (s Routes) save(w http.ResponseWriter, r *http.Request, caller access.Caller, typed Request, existing store.Entry) {
	var answer any
	if !s.writer.Write(w, r, caller, func(c changes.Change) error {
		t, err := tableOf(c, typed)
		if err != nil {
			return err
		}
		entry, amount, err := entryOf(c, typed, t, existing.ID)
		if err != nil {
			return err
		}
		if existing.ID == uuid.Nil {
			err = add(c, entry, typed, t, true)
		} else {
			entry, err = replace(c, existing, entry, typed, t)
		}
		if err != nil {
			return err
		}
		answer, err = t.form.answer(c, typed, entry, amount)
		return err
	}) {
		return
	}
	if existing.ID == uuid.Nil {
		w.Header().Set("Location", "/projects/"+web.ID(r, "projectId").String()+"/entries/"+typed.ID.String())
		web.JSON(w, r, http.StatusCreated, answer)
		return
	}
	web.JSON(w, r, http.StatusOK, answer)
}

// Record records typed as a new entry of the change's project through every check Add entry passes, sent for approval when typed.Submit
// (EntryRecording.RecordAsync): whoever decides entries is told unless tellDeciders is false, as a project file's load records them (redix-api
// #132 D7). Each refusal names its column under columnKey. The one way an entry is recorded, by hand or by a project file's load.
func Record(c changes.Change, typed Request, tellDeciders bool) (store.Entry, error) {
	t, err := tableOf(c, typed)
	if err != nil {
		return store.Entry{}, err
	}
	entry, _, err := entryOf(c, typed, t, uuid.Nil)
	if err != nil {
		return store.Entry{}, err
	}
	return entry, add(c, entry, typed, t, tellDeciders)
}

// AskedColumns is the columns the table tableID of the item asks an entry, when the actor is offered it (EntryTable.OfferedAsync): unknownTable
// (400) for a table they are not offered.
func AskedColumns(c changes.Change, itemID, tableID uuid.UUID) ([]store.ItemColumn, error) {
	t, err := tableOf(c, Request{ItemID: itemID, TableID: tableID})
	if err != nil {
		return nil, err
	}
	return t.columns, nil
}

// tableOf is the table typed goes into, among those the actor is offered: unknownTable (400) for any other.
func tableOf(c changes.Change, typed Request) (table, error) {
	tables, err := offered(c, c.Access)
	if err != nil {
		return table{}, err
	}
	at := slices.IndexFunc(tables, func(t table) bool { return t.item.ID == typed.ItemID && t.id == typed.TableID })
	if at < 0 {
		return table{}, changes.Refuse(http.StatusBadRequest, "unknownTable")
	}
	return tables[at], nil
}

// entryOf is typed as an entry of t, refused as refusal says, with the amount it comes to; editing is the entry it replaces, uuid.Nil for none.
func entryOf(c changes.Change, typed Request, t table, editing uuid.UUID) (store.Entry, fixed.Total, error) {
	if err := refusal(c, typed, t, c.Access, editing); err != nil {
		return store.Entry{}, fixed.Total{}, err
	}
	sign := t.sign(typed)
	amount := t.form.amount(typed, sign)
	entry := store.Entry{ID: typed.ID, ProjectID: c.ProjectID, Kind: t.form.entryKind(sign), Status: draft, EntryDate: typed.Date, Amount: amount.Number(),
		AccountID: typed.AccountID, Description: typed.description(), ColumnValues: typed.kept().KeptText(), CreatedByUserID: c.Actor, CreatedAt: c.Now,
		ApprovedAt: notApproved}
	if typed.Submit {
		entry.Status = pendingApproval
	}
	return entry, amount, nil
}

// refusal is why typed cannot go into t (EntryRecording.RefusalOfAsync), in .NET's order: the payment - unknownPaymentMethod for an expense,
// accountNotOffered, unknownAttachment; what was typed - invalidDescription, deductionNeedsDescription, the table's own typing, invalidAmount; the
// date, invalidDate (BR-12); the table's own checks; the columns - unknownColumn, invalidColumnValue, requiredColumnMissing on sending (FR-41); and
// invoiceAlreadyAttached (409) for a file on another entry (BR-22). Each names its column, "" for none.
func refusal(c changes.Change, typed Request, t table, held access.Access, editing uuid.UUID) error {
	code, err := refusalOfPayment(c, typed, t, held)
	if err != nil {
		return err
	}
	if code == "" {
		code = refusalOfTyped(typed, t)
	}
	if code == "" {
		project, err := c.Q.ProjectWithID(c.Ctx, c.ProjectID)
		zone, zoneErr := zones.Of(c.Ctx, c.Q, c.ProjectID)
		if err := errors.Join(err, zoneErr); err != nil {
			return err
		}
		if !isAcceptableDate(typed.Date, project.StartDate, zone, c.Now.Time) {
			code = "invalidDate"
		}
	}
	if code == "" {
		if code, err = t.form.refusal(c, typed, held); err != nil {
			return err
		}
	}
	if code != "" {
		return refusedOver(http.StatusBadRequest, code, "")
	}
	if reason, column := refusalOfColumns(typed, t.columns); reason != "" {
		return refusedOver(http.StatusBadRequest, reason, column)
	}
	if typed.AttachmentID == uuid.Nil {
		return nil
	}
	onAnother, err := fileOnAnotherEntry(c, typed.AttachmentID, editing)
	if err != nil || !onAnother {
		return err
	}
	return refusedOver(http.StatusConflict, "invoiceAlreadyAttached", "")
}

func refusalOfPayment(c changes.Change, typed Request, t table, held access.Access) (string, error) {
	if !t.isRevenue() {
		methods, err := c.Q.PaymentMethodsOf(c.Ctx, c.ProjectID)
		if err != nil {
			return "", err
		}
		if !slices.ContainsFunc(methods, func(method store.PaymentMethod) bool { return method.ID == typed.PaymentMethodID && method.IsActive }) {
			return "unknownPaymentMethod", nil
		}
	}
	accounts, err := c.Q.AccountsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return "", err
	}
	chargesAny := t.isRevenue() || held.Holds(c.ProjectID, access.ChargeAnyAccount)
	if !slices.ContainsFunc(accounts, func(account store.AccountsOfRow) bool {
		return account.ID == typed.AccountID && account.IsActive && (chargesAny || (account.CustodianID == c.Actor && account.CustodianID != uuid.Nil))
	}) {
		return "accountNotOffered", nil
	}
	if !t.isRevenue() {
		paysFrom, err := c.Q.MethodPaysFrom(c.Ctx, store.MethodPaysFromParams{PaymentMethodID: typed.PaymentMethodID, AccountID: typed.AccountID})
		if err != nil {
			return "", err
		}
		if !paysFrom {
			return "accountNotForMethod", nil
		}
	}
	if typed.AttachmentID == uuid.Nil {
		return "", nil
	}
	if _, err := c.Q.AttachmentOf(c.Ctx, store.AttachmentOfParams{ID: typed.AttachmentID, ProjectID: c.ProjectID}); err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return "unknownAttachment", nil
		}
		return "", err
	}
	return "", nil
}

func refusalOfTyped(typed Request, t table) string {
	description := typed.description()
	if tooLong(description, descriptionMaxLength) {
		return "invalidDescription"
	}
	total := t.form.total(typed)
	if !t.isRevenue() && total.Number() < 0 && description == "" {
		return "deductionNeedsDescription"
	}
	if t.sign(typed) == structure.Minus && total.Number() < 0 {
		return "invalidAmount"
	}
	if !typed.Amount.IsAcceptableMoney() || total.Number() == 0 || (t.isRevenue() && total.Number() < 0) {
		return "invalidAmount"
	}
	return ""
}

// refusalOfColumns is why typed's column values do not fit columns, those its table asks, or, when sending, a required one is empty (FR-41), and
// the column's key; "" when they fit. Every column whose value the entry keeps is checked the same way, whatever it is (redix-api #129, R5).
func refusalOfColumns(typed Request, columns []store.ItemColumn) (string, string) {
	values := typed.kept()
	for _, key := range values.Keys {
		at := slices.IndexFunc(columns, func(column store.ItemColumn) bool { return column.Key == key && !structure.IsEntryField(column.Key) })
		if at < 0 {
			return "unknownColumn", key
		}
		if !structure.Accepts(columns[at], values.Values[key]) {
			return "invalidColumnValue", key
		}
	}
	if !typed.Submit {
		return "", ""
	}
	if missing := missingOf(typed.description(), values.Values, columns); len(missing) > 0 {
		return "requiredColumnMissing", missing[0]
	}
	return "", ""
}

// missingOf is the keys of the required columns the entry gives no value, in their order (EntryAnswers.MissingOf): a field of the entry's own counts
// as given, the description but for an empty one.
func missingOf(description string, values map[string]string, columns []store.ItemColumn) []string {
	missing := []string{}
	for _, column := range columns {
		_, has := values[column.Key]
		gives := has || (structure.IsEntryField(column.Key) && column.Key != "description") || (column.Key == "description" && description != "")
		if column.IsRequired && !gives {
			missing = append(missing, column.Key)
		}
	}
	return missing
}

// fileOnAnotherEntry says whether the file, or the same bytes, are on an entry of the project other than editing already (BR-22).
func fileOnAnotherEntry(c changes.Change, attachmentID, editing uuid.UUID) (bool, error) {
	attachment, err := c.Q.AttachmentOf(c.Ctx, store.AttachmentOfParams{ID: attachmentID, ProjectID: c.ProjectID})
	if err != nil {
		return false, err
	}
	files, err := c.Q.EntryFilesOf(c.Ctx, c.ProjectID)
	return slices.ContainsFunc(files, func(file store.EntryFilesOfRow) bool {
		return file.EntryID != editing && file.Sha256 == attachment.Sha256
	}), err
}

// add keeps a new entry under its project's next number (FR-54), with its line - sent, or created - each of its rows with its line, and when sent
// its decision and the deciders told, once its rows say its table (EntryRecording.AddAsync, #106 R11).
func add(c changes.Change, entry store.Entry, typed Request, t table, tellDeciders bool) error {
	number, err := c.Q.NextEntryNumber(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	entry.Number = number
	if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(entry)); err != nil {
		return err
	}
	sent := entry.Status == pendingApproval
	action := history.Create
	if sent {
		action = history.Submit
	}
	if err := c.Record(action, "Entries", entry.ID, nil, fields(entry)); err != nil {
		return err
	}
	if err := keepRows(c, typed, entry, t); err != nil || !sent {
		return err
	}
	return sendOff(c, entry, tellDeciders)
}

// keepRows writes what the entry's table adds to it, and its own file.
func keepRows(c changes.Change, typed Request, entry store.Entry, t table) error {
	if err := t.form.keep(c, typed, entry, t); err != nil {
		return err
	}
	if typed.AttachmentID == uuid.Nil {
		return nil
	}
	role := fileRole(entry.Kind)
	return addDetail(c, "EntryAttachments", entry.ID, map[string]any{"AttachmentId": typed.AttachmentID, "Role": role},
		func() error {
			return c.Q.AddEntryFile(c.Ctx, store.AddEntryFileParams{EntryID: entry.ID, AttachmentID: typed.AttachmentID, Role: role})
		})
}

// sendOff records that entry was sent for approval and, when tellDeciders, tells whoever decides entries in its project, but the sender
// (TellDecidersAsync).
func sendOff(c changes.Change, entry store.Entry, tellDeciders bool) error {
	if err := c.Q.AddDecision(c.Ctx, store.AddDecisionParams{EntryID: entry.ID, Action: submitted, UserID: c.Actor, At: c.Now}); err != nil || !tellDeciders {
		return err
	}
	deciders, err := c.Q.HoldersOf(c.Ctx, store.HoldersOfParams{ProjectID: c.ProjectID, Permission: string(access.ApproveEntries)})
	if err != nil {
		return err
	}
	for _, userID := range deciders {
		if userID != c.Actor {
			if err := tell(c, entry, userID, notifications.EntrySubmitted); err != nil {
				return err
			}
		}
	}
	return nil
}

// tell tells userID kind about entry, which tapping it opens (NotificationsEndpoints.About); nobody is told of an entry whose table they do not see
// (#106 R11).
func tell(c changes.Change, entry store.Entry, userID uuid.UUID, kind notifications.Kind) error {
	hidden, err := c.Q.EntryHiddenFrom(c.Ctx, store.EntryHiddenFromParams{EntryID: entry.ID, UserID: userID, ProjectID: entry.ProjectID})
	if err != nil || hidden {
		return err
	}
	id := uuid.Must(uuid.NewV7())
	if err := c.Q.AddNotification(c.Ctx, store.AddNotificationParams{ID: id, UserID: userID, ProjectID: entry.ProjectID, Kind: int64(kind), At: c.Now}); err != nil {
		return err
	}
	return c.Q.AddNotificationEntry(c.Ctx, store.AddNotificationEntryParams{NotificationID: id, EntryID: entry.ID})
}

// replace puts edited in place of existing, keeping who entered it and when, and its status unless it is sent now (EntryChanges.ReplaceAsync);
// of its files only the oldest is replaced, the one the edit form shows and sends back, a file shared onto it later staying (redix-app #115, R5).
func replace(c changes.Change, existing, edited store.Entry, typed Request, t table) (store.Entry, error) {
	files, err := filesOf(c, existing.ID)
	if err != nil {
		return existing, err
	}
	if err := removeRows(c, existing.ID, files[:min(1, len(files))]); err != nil {
		return existing, err
	}
	sending := edited.Status == pendingApproval && existing.Status != pendingApproval
	after := existing
	after.Kind, after.EntryDate, after.Amount, after.AccountID, after.Description, after.ColumnValues = edited.Kind, edited.EntryDate, edited.Amount,
		edited.AccountID, edited.Description, edited.ColumnValues
	if edited.Status == pendingApproval {
		after.Status = pendingApproval
	}
	if err := c.Q.SaveEntry(c.Ctx, store.SaveEntryParams(after)); err != nil {
		return after, err
	}
	action := history.Update
	if sending {
		action = history.Submit
	}
	was, is := history.Changed(fields(existing), fields(after))
	if err := c.Record(action, "Entries", after.ID, was, is); err != nil {
		return after, err
	}
	if err := keepRows(c, typed, after, t); err != nil || !sending {
		return after, err
	}
	return after, sendOff(c, after, true)
}

// filesOf is the entry's files, the oldest first.
func filesOf(c changes.Change, entryID uuid.UUID) ([]store.EntryFilesOfRow, error) {
	files, err := c.Q.EntryFilesOf(c.Ctx, c.ProjectID)
	var kept []store.EntryFilesOfRow
	for _, file := range files {
		if file.EntryID == entryID {
			kept = append(kept, file)
		}
	}
	return kept, err
}

// removeRows removes what the entry holds - of its files only files - each with its Delete line (EntryChanges.RemoveDetailsAsync).
func removeRows(c changes.Change, entryID uuid.UUID, files []store.EntryFilesOfRow) error {
	expenses, err := c.Q.ExpenseDetailsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range expenses {
		if row.EntryID == entryID {
			if err := c.Remove("ExpenseDetails", entryID, map[string]any{"ItemId": row.ItemID, "SubgroupId": row.SubgroupID, "PaymentMethodId": row.PaymentMethodID},
				func() error { return c.Q.RemoveExpenseDetails(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	purchases, err := c.Q.PurchaseDetailsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range purchases {
		if row.EntryID == entryID {
			if err := c.Remove("PurchaseDetails", entryID, map[string]any{"SupplierId": row.SupplierID}, func() error { return c.Q.RemovePurchaseDetails(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	payments, err := c.Q.PaymentDetailsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range payments {
		if row.EntryID == entryID {
			if err := c.Remove("ContractorPaymentDetails", entryID, map[string]any{"ContractId": row.ContractID, "IsOutsideContract": row.IsOutsideContract},
				func() error { return c.Q.RemovePaymentDetails(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	invoices, err := c.Q.InvoicesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range invoices {
		if row.EntryID == entryID {
			if err := c.Remove("Invoices", entryID, map[string]any{"Number": row.Number}, func() error { return c.Q.RemoveInvoice(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	for _, file := range files {
		if err := c.Remove("EntryAttachments", entryID, map[string]any{"AttachmentId": file.AttachmentID, "Role": file.Role},
			func() error {
				return c.Q.RemoveEntryFile(c.Ctx, store.RemoveEntryFileParams{EntryID: entryID, AttachmentID: file.AttachmentID})
			}); err != nil {
			return err
		}
	}
	revenues, err := c.Q.RevenueDetailsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range revenues {
		if row.EntryID == entryID {
			if err := c.Remove("RevenueDetails", entryID, map[string]any{"ItemId": row.ItemID, "SubgroupId": row.SubgroupID},
				func() error { return c.Q.RemoveRevenueDetails(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	capital, err := c.Q.CapitalDetailsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, row := range capital {
		if row.EntryID == entryID {
			if err := c.Remove("CapitalMovementDetails", entryID, map[string]any{"PartnerId": row.PartnerID}, func() error { return c.Q.RemoveCapitalDetails(c.Ctx, entryID) }); err != nil {
				return err
			}
		}
	}
	return nil
}

// numberIn is a column's value read as a number, as decimal.TryParse read it with NumberStyles.Number: digits with a sign, a point and thousands
// separators, and the decimals it was written with.
func numberIn(value string) (fixed.Typed, bool) {
	var number fixed.Typed
	cleaned := []byte{}
	for _, letter := range []byte(value) {
		if letter != ',' && letter != ' ' {
			cleaned = append(cleaned, letter)
		}
	}
	if len(cleaned) == 0 || slices.ContainsFunc(cleaned, func(letter byte) bool { return letter == 'e' || letter == 'E' }) {
		return number, false
	}
	return number, number.UnmarshalJSON(cleaned) == nil
}
