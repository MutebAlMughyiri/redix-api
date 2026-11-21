// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// accountGroup is a type of account as Accounts groups them: its name, the word of one account of it, and the word of its group.
type accountGroup struct{ name, word, heading string }

// accountGroups are the types of account in the order the app groups them: banks, then cash custodies.
var accountGroups = []accountGroup{{"bank", "accounts.typeBank", "accounts.banks"}, {"cashCustody", "accounts.typeCashCustody", "accounts.custodies"}}

// groupOf is the group of account's type.
func groupOf(account accounts.View) accountGroup {
	for _, group := range accountGroups {
		if group.name == account.Type.Name() {
			return group
		}
	}
	return accountGroups[0]
}

// accountsShown is what Accounts shows: the project's accounts as the caller sees them, and the last count of their actual balances.
type accountsShown struct {
	accounts []accounts.View
	latest   accounts.Reconciliation
}

// in is the accounts of group, in the order the API lists them.
func (a accountsShown) in(group accountGroup) []accounts.View {
	var kept []accounts.View
	for _, account := range a.accounts {
		if account.Type.Name() == group.name {
			kept = append(kept, account)
		}
	}
	return kept
}

// details is account's line as the app's row says it: an active one's type, custodian and what the last count found in it; a disabled one says
// so beside that count.
func (a accountsShown) details(p page, account accounts.View) string {
	counted := ""
	for _, balance := range a.latest.Balances {
		if balance.AccountID == account.ID && balance.WasChecked {
			counted = p.words.Of("accounts.counted", statement.Money(balance.ActualBalance.Number()))
		}
	}
	if !account.IsActive {
		return parts(p.words.Of("accounts.disabled"), counted)
	}
	return parts(p.words.Of(groupOf(account).word), account.CustodianName, counted)
}

// accountsOf is Accounts for p's caller, through the rules the app's routes answer (accounts.ListOf, accounts.LatestOf). The error is the
// database's.
func (s site) accountsOf(ctx context.Context, p page) (accountsShown, error) {
	read := s.Database.Queries()
	listed, err := accounts.ListOf(ctx, read, p.project.ID, p.caller, false)
	latest, latestErr := accounts.LatestOf(ctx, read, p.project.ID, nil)
	return accountsShown{accounts: listed, latest: latest}, errors.Join(err, latestErr)
}

// accountsTab is Accounts in its tab (UC-015, redix-api #168): the last count's figures, then the accounts, banks first, each opening in the
// details panel.
func (s site) accountsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.accountsOf(r.Context(), p)
	return accountsList(p, shown), err
}

// accountOf is the account accountID among shown, and whether it is one.
func (a accountsShown) accountOf(accountID uuid.UUID) (accounts.View, bool) {
	for _, account := range a.accounts {
		if account.ID == accountID {
			return account, true
		}
	}
	return accounts.View{}, false
}

// accountDetails is an account in the details panel: its type, custodian, balance and last count, with Disable or Enable and Delete for whoever
// manages the project's settings, Delete asked first and gray with its reason for an account in use (redix-api #168). 404 for one not in the list.
func (s site) accountDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	shown, err := s.accountsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	accountID, _ := uuid.Parse(r.PathValue("id"))
	account, found := shown.accountOf(accountID)
	if !found || !p.inProject() {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, accountPanel(p, shown, account, said{asking: r.URL.Query().Get("asking")}))
}

// accountActions are an account's actions in the details panel, each the rule the app's route runs.
var accountActions = map[string]func(c changes.Change, accountID uuid.UUID) error{
	"disable": func(c changes.Change, accountID uuid.UUID) error { return accounts.Activate(c, accountID, false) },
	"enable":  func(c changes.Change, accountID uuid.UUID) error { return accounts.Activate(c, accountID, true) },
	"delete":  accounts.Delete,
}

// accountAction disables, enables or deletes the account the path names, through the rule the app's route runs, behind its gate - 403 without
// ManageSettings - in one change: the panel again saying what it did or why not, the account's row beside it; a deleted account leaves the
// list. A finished project changes nothing (AC-04.1); 404 for an action there is none of.
func (s site) accountAction(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	act, known := accountActions[r.PathValue("action")]
	if !p.inProject() || !known {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	accountID, _ := uuid.Parse(r.PathValue("id"))
	shown := said{}
	if p.finished() {
		shown.refusal = "projects.projectFinished"
	} else {
		err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error { return act(c, accountID) })
		var refused changes.Refusal
		switch {
		case errors.As(err, &refused):
			shown.refusal = "accounts." + refused.Code
		case err != nil:
			failed(w)
			return
		case r.PathValue("action") == "delete":
			shown.notice = "accounts.deleted"
		}
	}
	listed, err := s.accountsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	account, found := listed.accountOf(accountID)
	if !found {
		draw(w, r, http.StatusOK, accountGone(p, accountID, shown))
		return
	}
	draw(w, r, http.StatusOK, accountPanel(p, listed, account, shown), accountBeside(p, listed, account))
}

// mayKeepAccounts says whether p's caller may change the project's accounts: ManageSettings, in a project not finished.
func (p page) mayKeepAccounts() bool { return !p.finished() && p.may(access.ManageSettings) }
