// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package settings keeps a project's payment methods (UC-014, FR-40): added, renamed, disabled and enabled, and deleted while no expense was paid
// with them (PaymentMethodsEndpoints, redix-api #140).
package settings

import (
	"context"
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

// PaymentMethod is a payment method as the app shows it (PaymentMethodView), with the accounts it pays from (redix-app #109).
type PaymentMethod struct {
	ID         uuid.UUID   `json:"id"`
	Name       string      `json:"name"`
	IsActive   bool        `json:"isActive"`
	AccountIDs []uuid.UUID `json:"accountIds"`
}

// PaysFrom is the accounts a request names a method paying from, and whether it named any list at all: one naming none keeps what a method pays
// from, and a new method then pays from every account of its project (R4).
type PaysFrom struct {
	ids   []uuid.UUID
	named bool
}

// UnmarshalJSON reads the accounts named, and that the request named a list at all.
func (p *PaysFrom) UnmarshalJSON(data []byte) error {
	p.named = true
	return json.Unmarshal(data, &p.ids)
}

// PaysFromAccounts is a method paying from exactly ids, as a dialog's ticks name them.
func PaysFromAccounts(ids []uuid.UUID) PaysFrom { return PaysFrom{ids: ids, named: true} }

// IsZero is a request that named no accounts.
func (p PaysFrom) IsZero() bool { return !p.named }

// methodRequest is a payment method as its dialog sends it: its name, and the accounts it pays from.
type methodRequest struct {
	Name     string   `json:"name"`
	PaysFrom PaysFrom `json:"accountIds,omitzero"`
}

// list is the project's payment methods in order: every one, or only the active ones an entry may choose from (AC-40.1).
func (s Routes) list(activeOnly bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, _ access.Caller) {
		views, err := MethodsOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), activeOnly)
		if !changes.Answered(w, r, err) {
			web.JSON(w, r, http.StatusOK, views)
		}
	}
}

// MethodsOf is projectID's payment methods as list answers them, only the active ones when activeOnly, each with the accounts it pays from: the
// rule both doors call, the app's routes and the web's Add entry (redix-api #166). The error is the database's.
func MethodsOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, activeOnly bool) ([]PaymentMethod, error) {
	methods, err := read.PaymentMethodsOf(ctx, projectID)
	links, linksErr := read.PaymentMethodAccountsOf(ctx, projectID)
	if err := errors.Join(err, linksErr); err != nil {
		return nil, err
	}
	var views []PaymentMethod
	for _, method := range methods {
		if method.IsActive || !activeOnly {
			views = append(views, PaymentMethod{ID: method.ID, Name: method.Name, IsActive: method.IsActive, AccountIDs: accountsOf(links, method.ID)})
		}
	}
	return views, nil
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
	var request methodRequest
	if web.Read(w, r, &request) {
		s.writer.Done(w, r, caller, func(c changes.Change) error {
			_, err := CreatePaymentMethod(c, request.Name, request.PaysFrom)
			return err
		})
	}
}

// CreatePaymentMethod adds the payment method typed as name, paying from the accounts from names, as add does: invalidName for a name not 1 to
// 100 letters, then the refusals of AddPaymentMethod. The rule both doors call, the app's route and the web's New payment method (redix-api
// #174).
func CreatePaymentMethod(c changes.Change, name string, from PaysFrom) (uuid.UUID, error) {
	named, ok := naming.Named(name)
	if !ok {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return AddPaymentMethod(c, named, from)
}

// AddPaymentMethod adds the payment method named name at the end of the change's project's list (PaymentMethodsEndpoints.AddAsync), paying from
// the accounts from names, or every account of the project when it names none: the one way a payment method is added, by hand or by a project
// file's load.
func AddPaymentMethod(c changes.Change, name string, from PaysFrom) (uuid.UUID, error) {
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
var EveryAccount = PaysFrom{}

// setPaysFrom makes the method pay from exactly the accounts from names, or every account of the project when it names none and the method is
// new (redix-app #109, R4): paysFromNothing for an empty list while the project holds accounts (R10), unknownAccount for an account it does not
// hold. A project with no account yet takes a method naming none, and every account added later is ticked on it (D1b). A change to what a
// method kept pays from is its own History line, under the method.
func setPaysFrom(c changes.Change, methodID uuid.UUID, from PaysFrom, isNew bool) error {
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

// methodIn is methodID in the change's project, or changes.NotFound for one it does not hold.
func methodIn(c changes.Change, methodID uuid.UUID) (store.PaymentMethod, error) {
	return changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: methodID, ProjectID: c.ProjectID}))
}

// rename changes a payment method's name and, when its dialog sends them, the accounts it pays from, as RenamePaymentMethod does.
func (s Routes) rename(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request methodRequest
	if web.Read(w, r, &request) {
		s.writer.Done(w, r, caller, func(c changes.Change) error {
			return RenamePaymentMethod(c, web.ID(r, "paymentMethodId"), request.Name, request.PaysFrom)
		})
	}
}

// RenamePaymentMethod names methodID name and makes it pay from the accounts from names, when it names any, as rename does: the rule both doors
// call, the app's route and the web's Rename (redix-api #174). invalidName for a name not 1 to 100 letters; NotFound for a method the project
// does not hold; then the refusals of setPaysFrom.
func RenamePaymentMethod(c changes.Change, methodID uuid.UUID, name string, from PaysFrom) error {
	named, ok := naming.Named(name)
	if !ok {
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	before, err := methodIn(c, methodID)
	if err != nil {
		return err
	}
	after := before
	after.Name = named
	if err := save(c, before, after); err != nil {
		return err
	}
	return setPaysFrom(c, before.ID, from, false)
}

// activate disables a payment method, so it leaves entry and stays on the entries that used it (AC-40.1), or brings it back, as
// ActivatePaymentMethod does.
func (s Routes) activate(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.writer.Done(w, r, caller, func(c changes.Change) error { return ActivatePaymentMethod(c, web.ID(r, "paymentMethodId"), active) })
	}
}

// ActivatePaymentMethod disables methodID, so it leaves entry and stays on the entries that used it (AC-40.1), or brings it back when active:
// the rule both doors call, the app's route and the web's Payment methods (redix-api #174). NotFound for a method the project does not hold.
func ActivatePaymentMethod(c changes.Change, methodID uuid.UUID, active bool) error {
	before, err := methodIn(c, methodID)
	if err != nil {
		return err
	}
	after := before
	after.IsActive = active
	return save(c, before, after)
}

// delete deletes a payment method no expense was paid with: paymentMethodUsed (409) for one an expense keeps, which is disabled instead, as
// DeletePaymentMethod does.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.writer.Done(w, r, caller, func(c changes.Change) error { return DeletePaymentMethod(c, web.ID(r, "paymentMethodId")) })
}

// DeletePaymentMethod deletes methodID while no expense was paid with it, as delete does: the rule both doors call, the app's route and the web's
// Payment methods (redix-api #174). NotFound for a method the project does not hold; paymentMethodUsed (409) for one an expense keeps, which is
// disabled instead.
func DeletePaymentMethod(c changes.Change, methodID uuid.UUID) error {
	method, err := methodIn(c, methodID)
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
}
