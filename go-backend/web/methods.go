// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
)

// methodsShown is Payment methods as the app's screen shows them (AC-40.1): every method in order, with the accounts it pays from, and the
// project's accounts to tick.
type methodsShown struct {
	all      []settings.PaymentMethod
	accounts []accounts.View
}

// methodOf is the method with methodID.
func (s methodsShown) methodOf(methodID uuid.UUID) (settings.PaymentMethod, bool) {
	at := slices.IndexFunc(s.all, func(method settings.PaymentMethod) bool { return method.ID == methodID })
	if at < 0 {
		return settings.PaymentMethod{}, false
	}
	return s.all[at], true
}

// activeAccounts are the accounts a method's dialog ticks: the active ones, as the app's AccountTicks lists them.
func (s methodsShown) activeAccounts() []accounts.View {
	return slices.DeleteFunc(slices.Clone(s.accounts), func(account accounts.View) bool { return !account.IsActive })
}

// paysFromLine is what a method's row says it pays from, as the app's row does: the accounts' names, apart by the Arabic comma.
func (s methodsShown) paysFromLine(p page, method settings.PaymentMethod) string {
	var names []string
	for _, account := range s.accounts {
		if slices.Contains(method.AccountIDs, account.ID) {
			names = append(names, account.Name)
		}
	}
	return p.words.Of("paymentMethods.paysFrom") + ": " + strings.Join(names, "، ")
}

// mayKeepMethods says whether p's caller may change the payment methods: ManageSettings, in a project not finished, as the app's
// mayManageSettings (AC-04.1).
func (p page) mayKeepMethods() bool { return !p.finished() && p.may(access.ManageSettings) }

// methodsOf is Payment methods, read through the rules the app's routes answer (settings.MethodsOf, accounts.ListOf). The error is the
// database's.
func (s site) methodsOf(ctx context.Context, p page) (methodsShown, error) {
	read := s.Database.Queries()
	methods, err := settings.MethodsOf(ctx, read, p.project.ID, false)
	listed, accountsErr := accounts.ListOf(ctx, read, p.project.ID, p.caller, false)
	return methodsShown{all: methods, accounts: listed}, errors.Join(err, accountsErr)
}

// paymentMethodsTab is Payment methods in its tab (AC-40.1, redix-api #174), for whoever manages the project's settings.
func (s site) paymentMethodsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.methodsOf(r.Context(), p)
	return methodsList(p, shown), err
}

// methodDetails is a payment method in the details panel, or its form as the app's dialog asks it: New payment method (id new), Rename
// (asking=rename) and Delete asked first (asking=delete). 404 without ManageSettings, or for a method the project does not hold.
func (s site) methodDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.may(access.ManageSettings) {
		http.NotFound(w, r)
		return
	}
	shown, err := s.methodsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	if r.PathValue("id") == "new" {
		every := settings.PaymentMethod{}
		for _, account := range shown.activeAccounts() {
			every.AccountIDs = append(every.AccountIDs, account.ID)
		}
		draw(w, r, http.StatusOK, methodForm(p, shown, every, "", ""))
		return
	}
	methodID, _ := uuid.Parse(r.PathValue("id"))
	method, found := shown.methodOf(methodID)
	switch {
	case !found:
		http.NotFound(w, r)
	case r.URL.Query().Get("asking") == "rename":
		draw(w, r, http.StatusOK, methodForm(p, shown, method, method.Name, ""))
	default:
		draw(w, r, http.StatusOK, methodPanel(p, shown, method, said{asking: r.URL.Query().Get("asking")}))
	}
}

// tickedAccounts is the accounts a method's form ticked.
func tickedAccounts(r *http.Request) []uuid.UUID {
	var ids []uuid.UUID
	for _, id := range r.PostForm["account"] {
		if parsed, err := uuid.Parse(id); err == nil {
			ids = append(ids, parsed)
		}
	}
	return ids
}

// keepMethod keeps New payment method (POST /payment-method/new/add), Rename (…/{id}/rename), Disable, Enable or Delete (…/{id}/disable,
// …/enable, …/delete) through the rule the app's route runs, and answers the method's panel, Payment methods beside it, or Payment methods alone
// once deleted; refused, the form or the panel says why in the app's words. 403 without ManageSettings; 404 for an action there is none of, or for
// a method the project does not hold; a finished project changes nothing (AC-04.1).
func (s site) keepMethod(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action, adding := r.PathValue("action"), r.PathValue("id") == "new"
	known := (adding && action == "add") || (!adding && slices.Contains([]string{"rename", "disable", "enable", "delete"}, action))
	if !p.inProject() || r.ParseForm() != nil || !known {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.methodsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	methodID, _ := uuid.Parse(r.PathValue("id"))
	method, found := shown.methodOf(methodID)
	if !adding && !found {
		http.NotFound(w, r)
		return
	}
	refusal := "paymentMethods.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
			switch action {
			case "add":
				methodID, err = settings.CreatePaymentMethod(c, r.PostFormValue("name"), settings.PaysFromAccounts(tickedAccounts(r)))
			case "rename":
				err = settings.RenamePaymentMethod(c, methodID, r.PostFormValue("name"), settings.PaysFromAccounts(tickedAccounts(r)))
			case "delete":
				err = settings.DeletePaymentMethod(c, methodID)
			default:
				err = settings.ActivatePaymentMethod(c, methodID, action == "enable")
			}
			return err
		})
		if refusal, err = refusalOf("paymentMethods", err); err != nil {
			failed(w)
			return
		}
	}
	if refusal != "" {
		if adding || action == "rename" {
			method.AccountIDs = tickedAccounts(r)
			draw(w, r, http.StatusOK, methodForm(p, shown, method, r.PostFormValue("name"), refusal))
			return
		}
		draw(w, r, http.StatusOK, methodPanel(p, shown, method, said{refusal: refusal}))
		return
	}
	if shown, err = s.methodsOf(r.Context(), p); err != nil {
		failed(w)
		return
	}
	if action == "delete" {
		draw(w, r, http.StatusOK, methodsBeside(p, shown))
		return
	}
	method, _ = shown.methodOf(methodID)
	draw(w, r, http.StatusOK, methodPanel(p, shown, method, said{}), methodsBeside(p, shown))
}

// methodFormPath is where a method's form keeps it: New payment method adds one, Rename names the one it has.
func methodFormPath(method settings.PaymentMethod) string {
	if method.ID == uuid.Nil {
		return "/payment-method/new/add"
	}
	return "/payment-method/" + method.ID.String() + "/rename"
}
