// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package suppliers keeps a project's register of suppliers (UC-017, FR-06), the search of it (UC-018, FR-43), and a supplier added from the
// field (UC-019, FR-44). There is no delete: what is no longer wanted is disabled (BR-15). (SuppliersEndpoints, redix-api #140.)
package suppliers

import (
	"context"
	"net/http"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/spelling"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/google/uuid"
)

// Where a supplier stands (SupplierStatus): anyone may pick an approved one; one added from the field waits for whoever keeps the settings.
const (
	approved int64 = iota
	pendingApproval
)

var statuses = web.Enum{"approved", "pendingApproval"}

const (
	// generalCashName is the supplier every register starts with, for paying cash to no one in particular (Supplier.GeneralCashSupplierName).
	generalCashName    = "مورد نقدي عام"
	vatNumberMaxLength = 20
	searchMinLength    = 3
	searchLimit        = 20
)

// AddGeneralCash starts projectID's register with the general cash supplier, added in the name of createdBy (Supplier.GeneralCashSupplier). Like
// the structure it comes with, it has no History line of its own: the approval's line is the record.
func AddGeneralCash(ctx context.Context, q *store.Queries, projectID, createdBy uuid.UUID) error {
	return q.SaveSupplier(ctx, store.SaveSupplierParams{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, Name: generalCashName,
		NormalizedName: spelling.Normalize(generalCashName), Status: approved, IsActive: true, IsGeneralCash: true, CreatedByUserID: createdBy})
}

// Routes keeps a project's register.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the register: reading and searching it for whoever reads the project, adding to it for whoever holds AddSupplier, and changing it
// for whoever holds ManageSettings, in a project not finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const suppliers = "/projects/{projectId:guid}/suppliers"
	keeps := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", suppliers, gate.Needs(access.ReadProject, s.list))
	router.Handle("GET", suppliers+"/search", gate.Needs(access.ReadProject, s.search))
	router.Handle("POST", suppliers, gate.Changes(access.AddSupplier, s.add))
	router.Handle("PUT", suppliers+"/{supplierId:guid}", keeps(s.edit))
	router.Handle("POST", suppliers+"/{supplierId:guid}/disable", keeps(s.activate(false)))
	router.Handle("POST", suppliers+"/{supplierId:guid}/enable", keeps(s.activate(true)))
}

// supplierView is a supplier as the app shows it (SupplierView); timesUsed counts the caller's purchases naming it, in a search.
type supplierView struct {
	ID              uuid.UUID `json:"id"`
	Name            string    `json:"name"`
	VatNumber       string    `json:"vatNumber"`
	Mobile          string    `json:"mobile"`
	DefaultItemID   string    `json:"defaultItemId"`
	DefaultItemName string    `json:"defaultItemName"`
	Status          status    `json:"status"`
	IsActive        bool      `json:"isActive"`
	TimesUsed       int64     `json:"timesUsed"`
}

type status int64

func (s status) MarshalJSON() ([]byte, error) { return statuses.Write(int64(s)) }

func viewOf(row store.SuppliersOfRow, counted bool) supplierView {
	view := supplierView{ID: row.ID, Name: row.Name, VatNumber: row.VatNumber, Mobile: row.Mobile, DefaultItemName: row.DefaultItemName,
		Status: status(row.Status), IsActive: row.IsActive}
	if row.DefaultItemID != uuid.Nil {
		view.DefaultItemID = row.DefaultItemID.String()
	}
	if counted {
		view.TimesUsed = row.TimesUsed
	}
	return view
}

// list is every supplier of the project, disabled ones included, by name.
func (s Routes) list(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	rows, err := s.database.Queries().SuppliersOf(r.Context(), store.SuppliersOfParams{ProjectID: web.ID(r, "projectId")})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	var views []supplierView
	for _, row := range rows {
		views = append(views, viewOf(row, false))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// search is what the caller's entry may pick (AC-43.1): the active suppliers that are approved, that they added, or any waiting one for whoever
// keeps the settings, whose name - read as spelling reads it - or VAT number holds text; those on most of the caller's purchases first, then by
// name, at most twenty. Fewer than three letters narrow nothing down.
func (s Routes) search(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	projectID := web.ID(r, "projectId")
	rows, err := s.database.Queries().SuppliersOf(r.Context(), store.SuppliersOfParams{ProjectID: projectID, UserID: caller.UserID})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	seesPending := caller.Access.Holds(projectID, access.ManageSettings)
	typed := strings.TrimSpace(r.URL.Query().Get("text"))
	normalized := strings.ToLower(spelling.Normalize(typed))
	narrowsNothing := utf8.RuneCountInString(normalized) < searchMinLength
	var found []store.SuppliersOfRow
	for _, row := range rows {
		offered := row.IsActive && (row.Status == approved || row.CreatedByUserID == caller.UserID || seesPending)
		holds := narrowsNothing || strings.Contains(strings.ToLower(row.NormalizedName), normalized) || strings.Contains(row.VatNumber, typed)
		if offered && holds {
			found = append(found, row)
		}
	}
	slices.SortStableFunc(found, func(a, b store.SuppliersOfRow) int { return int(b.TimesUsed - a.TimesUsed) })
	var views []supplierView
	for _, row := range found[:min(len(found), searchLimit)] {
		views = append(views, viewOf(row, true))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// Request is a supplier as a person types it (SupplierRequest).
type Request struct {
	Name          string    `json:"name"`
	VatNumber     string    `json:"vatNumber,omitzero"`
	Mobile        string    `json:"mobile,omitzero"`
	DefaultItemID uuid.UUID `json:"defaultItemId,omitzero"`
}

// refusalOf is why typed cannot be the supplier supplierID of the change's project, uuid.Nil for a new one; nil when it can (Supplier.RefusalOf):
// invalidName, invalidVatNumber for anything but at most twenty digits (BR-16), invalidMobile, unknownItem for a default item outside the project
// (400); vatNumberTaken (409) for a number another of its suppliers holds, with that supplier as existing.
func refusalOf(c changes.Change, supplierID uuid.UUID, typed Request) error {
	_, named := naming.Named(typed.Name)
	vatNumber := strings.TrimSpace(typed.VatNumber)
	switch {
	case !named:
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	case len(vatNumber) > vatNumberMaxLength || strings.IndexFunc(vatNumber, func(c rune) bool { return c < '0' || c > '9' }) >= 0:
		return changes.Refuse(http.StatusBadRequest, "invalidVatNumber")
	case !users.IsAcceptableMobile(strings.TrimSpace(typed.Mobile)):
		return changes.Refuse(http.StatusBadRequest, "invalidMobile")
	case typed.DefaultItemID != uuid.Nil:
		if _, err := c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: typed.DefaultItemID, ProjectID: c.ProjectID}); err != nil {
			return changes.Unknown(err, "unknownItem")
		}
	}
	if vatNumber == "" {
		return nil
	}
	rows, err := c.Q.SuppliersOf(c.Ctx, store.SuppliersOfParams{ProjectID: c.ProjectID})
	if err != nil {
		return err
	}
	for _, row := range rows {
		if row.VatNumber == vatNumber && row.ID != supplierID {
			return changes.RefuseOver(http.StatusConflict, "vatNumberTaken", "existing", viewOf(row, false))
		}
	}
	return nil
}

func fields(row store.Supplier) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "Name": row.Name, "NormalizedName": row.NormalizedName, "VatNumber": row.VatNumber,
		"Mobile": row.Mobile, "Status": row.Status, "IsActive": row.IsActive, "IsGeneralCash": row.IsGeneralCash, "CreatedByUserId": row.CreatedByUserID}
}

func save(c changes.Change, before, after store.Supplier) error {
	return c.Save("Suppliers", after.ID, before.ID == uuid.Nil, fields(before), fields(after),
		func() error { return c.Q.SaveSupplier(c.Ctx, store.SaveSupplierParams(after)) })
}

// typedInto is supplier as typed changes it: its name, kept with its search form, VAT number and mobile, each trimmed.
func typedInto(supplier store.Supplier, typed Request) store.Supplier {
	supplier.Name, _ = naming.Named(typed.Name)
	supplier.NormalizedName = spelling.Normalize(supplier.Name)
	supplier.VatNumber, supplier.Mobile = strings.TrimSpace(typed.VatNumber), strings.TrimSpace(typed.Mobile)
	return supplier
}

// defaultTo makes itemID the item the supplier's expenses usually go to, uuid.Nil none (Supplier.DefaultTo), with its History line, whose
// record is the supplier, the row's key.
func defaultTo(c changes.Change, supplierID, itemID uuid.UUID) error {
	kept, err := c.Q.DefaultItemOf(c.Ctx, supplierID)
	if err != nil {
		return err
	}
	if itemID == uuid.Nil {
		for _, row := range kept {
			if err := c.Remove("SupplierDefaultItems", supplierID, map[string]any{"ItemId": row.ItemID},
				func() error { return c.Q.RemoveDefaultItem(c.Ctx, supplierID) }); err != nil {
				return err
			}
		}
		return nil
	}
	before := map[string]any{"ItemId": uuid.Nil}
	if len(kept) > 0 {
		before["ItemId"] = kept[0].ItemID
	}
	return c.Save("SupplierDefaultItems", supplierID, len(kept) == 0, before, map[string]any{"ItemId": itemID},
		func() error {
			return c.Q.SaveDefaultItem(c.Ctx, store.SaveDefaultItemParams{SupplierID: supplierID, ItemID: itemID})
		})
}

// Add adds a supplier to the change's project, approved for whoever keeps the settings and otherwise waiting for approval (FR-44,
// SuppliersEndpoints.AddAsync), refused as refusalOf says: the one way a supplier is added, by hand or by a project file's load.
func Add(c changes.Change, request Request) (store.Supplier, error) {
	if err := refusalOf(c, uuid.Nil, request); err != nil {
		return store.Supplier{}, err
	}
	supplier := store.Supplier{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Status: pendingApproval, IsActive: true, CreatedByUserID: c.Actor}
	if c.Access.Holds(c.ProjectID, access.ManageSettings) {
		supplier.Status = approved
	}
	supplier = typedInto(supplier, request)
	if err := save(c, store.Supplier{}, supplier); err != nil {
		return store.Supplier{}, err
	}
	return supplier, defaultTo(c, supplier.ID, request.DefaultItemID)
}

// add adds a supplier as Add does; 201 with it.
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request Request
	if !web.Read(w, r, &request) {
		return
	}
	var supplier store.Supplier
	if !s.writer.Write(w, r, caller, func(c changes.Change) (err error) {
		supplier, err = Add(c, request)
		return err
	}) {
		return
	}
	projectID := web.ID(r, "projectId")
	rows, err := s.database.Queries().SuppliersOf(r.Context(), store.SuppliersOfParams{ProjectID: projectID})
	at := slices.IndexFunc(rows, func(row store.SuppliersOfRow) bool { return row.ID == supplier.ID })
	if err != nil || at < 0 {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Location", "/projects/"+projectID.String()+"/suppliers/"+supplier.ID.String())
	web.JSON(w, r, http.StatusCreated, viewOf(rows[at], false))
}

// edit changes a supplier's name, VAT number, mobile and default item: 404 for one the project does not hold, refused as add is, a supplier
// keeping its own number.
func (s Routes) edit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request Request
	if !web.Read(w, r, &request) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		before, err := changes.Found(c.Q.SupplierOf(c.Ctx, store.SupplierOfParams{ID: web.ID(r, "supplierId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		if err := refusalOf(c, before.ID, request); err != nil {
			return err
		}
		if err := save(c, before, typedInto(before, request)); err != nil {
			return err
		}
		return defaultTo(c, before.ID, request.DefaultItemID)
	})
}

// activate disables a supplier, so it leaves entry and stays in the reports (BR-15), or brings it back: generalCashSupplier (409) for disabling
// the general cash supplier, which an expense with no supplier picks.
func (s Routes) activate(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.writer.Done(w, r, caller, func(c changes.Change) error {
			before, err := changes.Found(c.Q.SupplierOf(c.Ctx, store.SupplierOfParams{ID: web.ID(r, "supplierId"), ProjectID: c.ProjectID}))
			if err != nil {
				return err
			}
			if before.IsGeneralCash && !active {
				return changes.Refuse(http.StatusConflict, "generalCashSupplier")
			}
			after := before
			after.IsActive = active
			return save(c, before, after)
		})
	}
}
