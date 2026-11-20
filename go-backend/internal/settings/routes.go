// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package settings keeps a project's payment methods (UC-014, FR-40): added, renamed, disabled and enabled, and deleted while no expense was paid
// with them (PaymentMethodsEndpoints, redix-api #140).
package settings

import (
	"encoding/json"
	"errors"
	"net/http"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Routes keeps a project's payment methods.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the payment methods: reading them for whoever reads the project, changing them for whoever holds ManageSettings in a project not
// finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const methods = "/projects/{projectId:guid}/payment-methods"
	manage := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", methods, gate.Needs(access.ReadProject, s.list(false)))
	router.Handle("GET", methods+"/offered", gate.Needs(access.ReadProject, s.list(true)))
	router.Handle("POST", methods, manage(s.add))
	router.Handle("PUT", methods+"/{paymentMethodId:guid}", manage(s.rename))
	router.Handle("POST", methods+"/{paymentMethodId:guid}/disable", manage(s.activate(false)))
	router.Handle("POST", methods+"/{paymentMethodId:guid}/enable", manage(s.activate(true)))
	router.Handle("DELETE", methods+"/{paymentMethodId:guid}", manage(s.delete))
}

// paymentMethodView is a payment method as the app shows it (PaymentMethodView), with the accounts it pays from (redix-app #109).
type paymentMethodView struct {
	ID         uuid.UUID   `json:"id"`
	Name       string      `json:"name"`
	IsActive   bool        `json:"isActive"`
	AccountIDs []uuid.UUID `json:"accountIds"`
}

// paysFrom is the accounts a request names a method paying from, and whether it named any list at all: one naming none keeps what a method pays
// from, and a new method then pays from every account of its project (R4).
type paysFrom struct {
	ids   []uuid.UUID
	named bool
}

func (p *paysFrom) UnmarshalJSON(data []byte) error {
	p.named = true
	return json.Unmarshal(data, &p.ids)
}

// IsZero is a request that named no accounts.
func (p paysFrom) IsZero() bool { return !p.named }

// methodRequest is a payment method as its dialog sends it: its name, and the accounts it pays from.
type methodRequest struct {
	Name     string   `json:"name"`
	PaysFrom paysFrom `json:"accountIds,omitzero"`
}

// read is the request's name and the accounts it names; false once it has answered a body it cannot read or a name it refuses.
func read(w http.ResponseWriter, r *http.Request) (string, paysFrom, bool) {
	var request methodRequest
	if !web.Read(w, r, &request) {
		return "", paysFrom{}, false
	}
	name, ok := naming.Named(request.Name)
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, naming.Invalid)
	}
	return name, request.PaysFrom, ok
}

// list is the project's payment methods in order: every one, or only the active ones an entry may choose from (AC-40.1).
func (s Routes) list(activeOnly bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, _ access.Caller) {
		read, projectID := s.database.Queries(), web.ID(r, "projectId")
		methods, err := read.PaymentMethodsOf(r.Context(), projectID)
		links, linksErr := read.PaymentMethodAccountsOf(r.Context(), projectID)
		if changes.Answered(w, r, errors.Join(err, linksErr)) {
			return
		}
		var views []paymentMethodView
		for _, method := range methods {
			if method.IsActive || !activeOnly {
				views = append(views, paymentMethodView{ID: method.ID, Name: method.Name, IsActive: method.IsActive, AccountIDs: accountsOf(links, method.ID)})
			}
		}
		web.JSON(w, r, http.StatusOK, views)
	}
}

func fields(method store.PaymentMethod) map[string]any {
	return map[string]any{"ProjectId": method.ProjectID, "Name": method.Name, "SortOrder": method.SortOrder, "IsActive": method.IsActive}
}

func save(c changes.Change, before, after store.PaymentMethod) error {
	return c.Save("PaymentMethods", after.ID, before.ID == uuid.Nil, fields(before), fields(after),
		func() error { return c.Q.SavePaymentMethod(c.Ctx, store.SavePaymentMethodParams(after)) })
}

// add adds a payment method as AddPaymentMethod does, paying from the accounts its dialog ticked.
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, from, ok := read(w, r)
	if !ok {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		_, err := AddPaymentMethod(c, name, from)
		return err
	})
}

// AddPaymentMethod adds the payment method named name at the end of the change's project's list (PaymentMethodsEndpoints.AddAsync), paying from
// the accounts from names, or every account of the project when it names none: the one way a payment method is added, by hand or by a project
// file's load.
func AddPaymentMethod(c changes.Change, name string, from paysFrom) (uuid.UUID, error) {
	methods, err := c.Q.PaymentMethodsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return uuid.Nil, err
	}
	place := int64(0)
	for _, method := range methods {
		place = max(place, method.SortOrder+1)
	}
	method := store.PaymentMethod{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Name: name, SortOrder: place, IsActive: true}
	if err := save(c, store.PaymentMethod{}, method); err != nil {
		return uuid.Nil, err
	}
	return method.ID, setPaysFrom(c, method.ID, from, true)
}

// EveryAccount is a payment method paying from every account of its project, as a project file's methods do (R4).
var EveryAccount = paysFrom{}

// setPaysFrom makes the method pay from exactly the accounts from names, or every account of the project when it names none and the method is
// new (redix-app #109, R4): paysFromNothing for an empty list while the project holds accounts (R10), unknownAccount for an account it does not
// hold. A project with no account yet takes a method naming none, and every account added later is ticked on it (D1b). A change to what a
// method kept pays from is its own History line, under the method.
func setPaysFrom(c changes.Change, methodID uuid.UUID, from paysFrom, isNew bool) error {
	if !from.named && !isNew {
		return nil
	}
	accounts, err := c.Q.AccountsOf(c.Ctx, c.ProjectID)
	links, linksErr := c.Q.PaymentMethodAccountsOf(c.Ctx, c.ProjectID)
	if err := errors.Join(err, linksErr); err != nil {
		return err
	}
	held := make([]uuid.UUID, 0, len(accounts))
	for _, account := range accounts {
		held = append(held, account.ID)
	}
	wanted := held
	if from.named {
		if len(from.ids) == 0 && len(held) > 0 {
			return changes.Refuse(http.StatusBadRequest, "paysFromNothing")
		}
		wanted = slices.Compact(slices.SortedFunc(slices.Values(from.ids), func(a, b uuid.UUID) int { return compareIDs(a, b) }))
		for _, id := range wanted {
			if !slices.Contains(held, id) {
				return changes.Refuse(http.StatusBadRequest, "unknownAccount")
			}
		}
	}
	kept := accountsOf(links, methodID)
	for _, id := range wanted {
		if !slices.Contains(kept, id) {
			if err := c.Q.AddPaymentMethodAccount(c.Ctx, store.AddPaymentMethodAccountParams{PaymentMethodID: methodID, AccountID: id}); err != nil {
				return err
			}
		}
	}
	for _, id := range kept {
		if !slices.Contains(wanted, id) {
			if err := c.Q.RemovePaymentMethodAccount(c.Ctx, store.RemovePaymentMethodAccountParams{PaymentMethodID: methodID, AccountID: id}); err != nil {
				return err
			}
		}
	}
	if isNew || slices.Equal(sortedIDs(kept), sortedIDs(wanted)) {
		return nil
	}
	return c.Record(history.Update, "PaymentMethods", methodID, map[string]any{"AccountIds": sortedIDs(kept)}, map[string]any{"AccountIds": sortedIDs(wanted)})
}

// TickOnEveryMethod has every payment method of the change's project pay from the account just added (D1b, the owner 17 Nov 2026), recorded under the
// account's own line.
func TickOnEveryMethod(c changes.Change, accountID uuid.UUID) error {
	methods, err := c.Q.PaymentMethodsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	for _, method := range methods {
		if err := c.Q.AddPaymentMethodAccount(c.Ctx, store.AddPaymentMethodAccountParams{PaymentMethodID: method.ID, AccountID: accountID}); err != nil {
			return err
		}
	}
	return nil
}

// accountsOf is the accounts the method pays from, among the project's links.
func accountsOf(links []store.PaymentMethodAccount, methodID uuid.UUID) []uuid.UUID {
	var ids []uuid.UUID
	for _, link := range links {
		if link.PaymentMethodID == methodID {
			ids = append(ids, link.AccountID)
		}
	}
	return ids
}

func compareIDs(a, b uuid.UUID) int { return strings.Compare(a.String(), b.String()) }

func sortedIDs(ids []uuid.UUID) []string {
	texts := make([]string, 0, len(ids))
	for _, id := range ids {
		texts = append(texts, id.String())
	}
	slices.Sort(texts)
	return texts
}

// change applies apply to the route's payment method: 404 for one the project does not hold.
func (s Routes) change(w http.ResponseWriter, r *http.Request, caller access.Caller, apply func(*store.PaymentMethod)) {
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		before, err := changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: web.ID(r, "paymentMethodId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		after := before
		apply(&after)
		return save(c, before, after)
	})
}

// rename changes a payment method's name and, when its dialog sends them, the accounts it pays from.
func (s Routes) rename(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, from, ok := read(w, r)
	if !ok {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		before, err := changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: web.ID(r, "paymentMethodId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		after := before
		after.Name = name
		if err := save(c, before, after); err != nil {
			return err
		}
		return setPaysFrom(c, before.ID, from, false)
	})
}

// activate disables a payment method, so it leaves entry and stays on the entries that used it (AC-40.1), or brings it back.
func (s Routes) activate(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.change(w, r, caller, func(method *store.PaymentMethod) { method.IsActive = active })
	}
}

// delete deletes a payment method no expense was paid with: paymentMethodUsed (409) for one an expense keeps, which is disabled instead.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		method, err := changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: web.ID(r, "paymentMethodId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		if used, err := c.Q.PaymentMethodUsed(c.Ctx, method.ID); err != nil || used {
			if used {
				return changes.Refuse(http.StatusConflict, "paymentMethodUsed")
			}
			return err
		}
		return c.Remove("PaymentMethods", method.ID, fields(method), func() error {
			if err := c.Q.RemovePaymentMethodAccounts(c.Ctx, method.ID); err != nil {
				return err
			}
			return c.Q.RemovePaymentMethod(c.Ctx, method.ID)
		})
	})
}
