// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"net/http"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Putting a file shared to REDIX on an entry (UC-039, BR-22, redix-app #115): any entry its person may handle, of any kind, an approved one too,
// beside the files it has; only the file goes on, and nothing else about the entry changes (redix-api #96, D15).

// attachableBy says whether caller may put a file on entry (EntryFileEndpoints.AttachableBy): one paying something - no deduction and no credit
// note - not removed, and theirs to handle.
func attachableBy(entry store.Entry, caller access.Caller) bool {
	return entry.Amount > 0 && entry.Status != removed && mayHandle(entry, caller)
}

// attachable is the entries a QR code on a shared file matches (AC-49.2), as the lists show them: those the caller may put a file on whose
// supplier's VAT number is the code's, whose total is within one riyal of its total and whose date within three days of its date.
func (s Routes) attachable(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	query := r.URL.Query()
	total, totalErr := fixed.Parse(query.Get("total"))
	date, dateErr := time.Parse("2006-01-02", query.Get("date"))
	if !query.Has("vatNumber") || totalErr != nil || dateErr != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return
	}
	vatNumber := query.Get("vatNumber")
	const riyal = fixed.Number(10000)
	views, err := s.rows(r, caller, func(entry store.Entry, known facts) bool {
		near := entry.Amount >= total-riyal && entry.Amount <= total+riyal && !entry.EntryDate.Before(date.AddDate(0, 0, -3)) && !entry.EntryDate.After(date.AddDate(0, 0, 3))
		return attachableBy(entry, caller) && near && vatNumber != "" && known.supplierOf(entry).VatNumber == vatNumber
	}, everyRow, 0)
	s.answer(w, r, views, err)
}

// attachInvoiceRequest is the body POST /projects/{projectId}/entries/{entryId}/invoice reads (UC-039).
type attachInvoiceRequest struct {
	AttachmentID uuid.UUID `json:"attachmentId"`
}

// attach puts the file on the entry beside any it has, as its kind's file (D5), and changes nothing else (D15): notYourEntry (403) for one the
// caller may not handle (BR-42), entryNotAttachable (409) for a deduction or a credit note, unknownAttachment (400), and invoiceAlreadyAttached
// (409) for a file already on an entry (BR-22). A removed entry is not found (404), as on every route of an entry (redix-api #119).
func (s Routes) attach(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed attachInvoiceRequest
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
	if !attachableBy(entry, caller) {
		problem.Refuse(w, r, http.StatusConflict, "entryNotAttachable")
		return
	}
	attached := s.writer.Write(w, r, caller, func(c changes.Change) error {
		if _, err := c.Q.AttachmentOf(c.Ctx, store.AttachmentOfParams{ID: typed.AttachmentID, ProjectID: c.ProjectID}); err != nil {
			return changes.Unknown(err, "unknownAttachment")
		}
		onAnother, err := fileOnAnotherEntry(c, typed.AttachmentID, uuid.Nil)
		if err != nil {
			return err
		}
		if onAnother {
			return changes.Refuse(http.StatusConflict, "invoiceAlreadyAttached")
		}
		role := fileRole(entry.Kind)
		return addDetail(c, "EntryAttachments", entry.ID, map[string]any{"AttachmentId": typed.AttachmentID, "Role": role},
			func() error {
				return c.Q.AddEntryFile(c.Ctx, store.AddEntryFileParams{EntryID: entry.ID, AttachmentID: typed.AttachmentID, Role: role})
			})
	})
	if attached {
		web.JSON(w, r, http.StatusOK, statusView{ID: entry.ID, Number: entry.Number, Status: statusOf(entry.Status)})
	}
}
