// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
)

// suppliersShown is Suppliers as the app's screen shows it (UC-017): the register, disabled suppliers included, by name, and the search typed.
type suppliersShown struct {
	all  []suppliers.View
	text string
}

// shown is what the search keeps: as the app's, it finds a supplier's name, VAT number, mobile and default item.
func (s suppliersShown) shown() []suppliers.View {
	return slices.DeleteFunc(slices.Clone(s.all), func(supplier suppliers.View) bool {
		return !searchHolds(s.text, supplier.Name, supplier.VatNumber, supplier.Mobile, supplier.DefaultItemName)
	})
}

// supplierOf is the supplier supplierID of the register, and whether it holds one.
func (s suppliersShown) supplierOf(supplierID uuid.UUID) (suppliers.View, bool) {
	at := slices.IndexFunc(s.all, func(supplier suppliers.View) bool { return supplier.ID == supplierID })
	if at < 0 {
		return suppliers.View{}, false
	}
	return s.all[at], true
}

// supplierDetails is what the register says about supplier, as the app's row says it (Supplier.detailsIn): that it is disabled or waits for
// approval, its VAT number, its default item.
func supplierDetails(p page, supplier suppliers.View) string {
	standing := ""
	switch {
	case !supplier.IsActive:
		standing = p.words.Of("suppliers.disabled")
	case supplier.Pending():
		standing = p.words.Of("suppliers.pendingApproval")
	}
	return parts(standing, supplier.VatNumber, supplier.DefaultItemName)
}

// suppliersOf is the register as Suppliers shows it to p's caller with the search query typed, through the rule the app's route answers
// (suppliers.ListOf). The error is the database's.
func (s site) suppliersOf(ctx context.Context, p page, query url.Values) (suppliersShown, error) {
	all, err := suppliers.ListOf(ctx, s.Database.Queries(), p.project.ID)
	return suppliersShown{all: all, text: strings.TrimSpace(query.Get("text"))}, err
}

// suppliersTab is Suppliers in its tab (UC-017, redix-api #170): everyone in the project reads the register; whoever keeps the settings adds,
// changes, disables and enables its suppliers.
func (s site) suppliersTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.suppliersOf(r.Context(), p, r.URL.Query())
	return suppliersList(p, shown), err
}

// mayKeepSuppliers says whether p's caller may change the register from Suppliers: ManageSettings, in a project not finished, as the app's
// screen offers New supplier and each row's menu.
func (p page) mayKeepSuppliers() bool { return !p.finished() && p.may(access.ManageSettings) }

// supplierDetailsPanel is a supplier in the details panel, or Edit's form for ?asking=edit; 404 for one the register does not hold.
func (s site) supplierDetailsPanel(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	shown, err := s.suppliersOf(r.Context(), p, nil)
	if err != nil {
		failed(w)
		return
	}
	supplierID, _ := uuid.Parse(r.PathValue("id"))
	supplier, found := shown.supplierOf(supplierID)
	if !found || !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if r.URL.Query().Get("asking") == "edit" && p.mayKeepSuppliers() {
		s.supplierFormOf(w, r, p, supplier, supplierTypedOf(supplier), "", "")
		return
	}
	draw(w, r, http.StatusOK, supplierPanel(p, supplier, ""))
}

// supplierTyped is a supplier as its form types it.
type supplierTyped struct {
	name, vatNumber, mobile string
	defaultItem             uuid.UUID
}

// supplierTypedOf is supplier as its form starts, to change it.
func supplierTypedOf(supplier suppliers.View) supplierTyped {
	defaultItem, _ := uuid.Parse(supplier.DefaultItemID)
	return supplierTyped{name: supplier.Name, vatNumber: supplier.VatNumber, mobile: supplier.Mobile, defaultItem: defaultItem}
}

// request is what typed asks of the register.
func (typed supplierTyped) request() suppliers.Request {
	return suppliers.Request{Name: typed.name, VatNumber: typed.vatNumber, Mobile: typed.mobile, DefaultItemID: typed.defaultItem}
}

// supplierFormOf answers the details panel with New supplier's form - or Edit's, for editing - as typed, the purchase items it may default to,
// why it could not be kept and, for a VAT number another supplier holds, that supplier's name.
func (s site) supplierFormOf(w http.ResponseWriter, r *http.Request, p page, editing suppliers.View, typed supplierTyped, refusal, holder string) {
	items, err := structure.OfferedOf(r.Context(), s.Database.Queries(), p.project.ID, structure.Purchases)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, supplierForm(p, editing, typed, items, refusal, holder))
}

// newSupplierForm is New supplier in the details panel, for whoever keeps the settings (UC-017, redix-api #170).
func (s site) newSupplierForm(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.mayKeepSuppliers() {
		http.NotFound(w, r)
		return
	}
	s.supplierFormOf(w, r, p, suppliers.View{}, supplierTyped{}, "", "")
}

// supplierChanges are what the details panel does to a supplier the path names, each the rule the app's route runs.
var supplierChanges = map[string]func(c changes.Change, supplierID uuid.UUID, typed supplierTyped) error{
	"edit": func(c changes.Change, supplierID uuid.UUID, typed supplierTyped) error {
		return suppliers.Edit(c, supplierID, typed.request())
	},
	"disable": func(c changes.Change, supplierID uuid.UUID, _ supplierTyped) error {
		return suppliers.Activate(c, supplierID, false)
	},
	"enable": func(c changes.Change, supplierID uuid.UUID, _ supplierTyped) error {
		return suppliers.Activate(c, supplierID, true)
	},
}

// keepSupplier keeps the form posted, a new supplier through suppliers.Add or the supplier the path names through suppliers.Edit, or disables
// or enables that supplier through suppliers.Activate - the rules the app's routes run -, in one change: the panel shows the supplier and
// Suppliers beside it follows; refused, the panel says why in the app's words. 403 without ManageSettings; a finished project changes nothing
// (AC-04.1).
func (s site) keepSupplier(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	act, known := supplierChanges[action]
	if !p.inProject() || r.ParseForm() != nil || (action != "" && !known) {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.suppliersOf(r.Context(), p, nil)
	if err != nil {
		failed(w)
		return
	}
	defaultItem, _ := uuid.Parse(r.PostFormValue("defaultItem"))
	typed := supplierTyped{name: r.PostFormValue("name"), vatNumber: r.PostFormValue("vatNumber"), mobile: r.PostFormValue("mobile"), defaultItem: defaultItem}
	supplier := suppliers.View{}
	if action != "" {
		supplierID, _ := uuid.Parse(r.PathValue("id"))
		var found bool
		if supplier, found = shown.supplierOf(supplierID); !found {
			http.NotFound(w, r)
			return
		}
	}
	refusal, holder := "", ""
	if p.finished() {
		refusal = "projects.projectFinished"
	} else {
		supplierID := supplier.ID
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			if known {
				return act(c, supplierID, typed)
			}
			added, err := suppliers.Add(c, typed.request())
			supplierID = added.ID
			return err
		})
		var refused changes.Refusal
		switch {
		case errors.As(err, &refused):
			refusal = "suppliers." + refused.Code
			if existing, holds := refused.Detail.(suppliers.View); holds {
				holder = existing.Name
			}
		case err != nil:
			failed(w)
			return
		}
		supplier.ID = supplierID
	}
	if refusal != "" && (action == "" || action == "edit") {
		s.supplierFormOf(w, r, p, supplier, typed, refusal, holder)
		return
	}
	if shown, err = s.suppliersOf(r.Context(), p, nil); err != nil {
		failed(w)
		return
	}
	supplier, _ = shown.supplierOf(supplier.ID)
	draw(w, r, http.StatusOK, supplierPanel(p, supplier, refusal), suppliersBeside(p, shown))
}

// keepsUnoffered says whether the form must offer the default item the supplier being edited has though entry no longer offers it - a disabled
// item -, so that saving keeps it, as the app's dialog keeps an item not picked again.
func keepsUnoffered(items []structure.CategoryView, editing suppliers.View, typed supplierTyped) bool {
	if editing.DefaultItemID == "" || editing.DefaultItemID != typed.defaultItem.String() {
		return false
	}
	return !slices.ContainsFunc(items, func(category structure.CategoryView) bool {
		return slices.ContainsFunc(category.Items, func(item structure.ItemView) bool { return item.ID == typed.defaultItem })
	})
}
