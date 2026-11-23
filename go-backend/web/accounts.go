// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// accountGroup is a type of account as Accounts groups them: its name, the word of one account of it, and the word of its group.
type accountGroup struct{ name, word, heading string }

// accountGroups are the types of account in the order the app groups them: banks, then cash custodies.
var accountGroups = []accountGroup{{"bank", "accounts.typeBank", "accounts.banks"}, {"cashCustody", "accounts.typeCashCustody", "accounts.custodies"}}

// groupOf is the group of account's type: a cash custody's, or the banks'.
func groupOf(account accounts.View) accountGroup {
	if account.Type == accounts.CashCustody {
		return accountGroups[1]
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
	if r.URL.Query().Get("asking") == "edit" && p.mayKeepAccounts() {
		s.accountFormOf(w, r, p, account, accountTyped{name: account.Name, kind: account.Type.Name(), custodian: custodianOf(account)}, "")
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

// accountFilter is the account history's account filter as the approved design draws it (redix-api #183): every account, then each, the one
// shown's query holds chosen.
func (s accountsShown) accountFilter(p page, shown history) filter {
	all := filter{id: "accountHistory.accountChoice", label: "expenses.account", choices: []filterChoice{{id: "accountHistory.accountChoice.option.none", name: "account",
		word: p.words.Of("history.all"), chosen: shown.query.Get("account") == ""}}}
	for _, account := range s.accounts {
		all.choices = append(all.choices, filterChoice{id: "accountHistory.account." + account.ID.String(), name: "account", value: account.ID.String(), word: account.Name,
			chosen: account.ID.String() == shown.query.Get("account")})
	}
	return all
}

// mayKeepAccounts says whether p's caller may change the project's accounts: ManageSettings, in a project not finished.
func (p page) mayKeepAccounts() bool { return !p.finished() && p.may(access.ManageSettings) }

// accountTyped is New account's or Edit's form as typed: the name, the type - bank or cashCustody, chosen only for a new one - and who holds it.
type accountTyped struct {
	name, kind string
	custodian  uuid.UUID
}

// accountTypedOf is the form r posted.
func accountTypedOf(r *http.Request) accountTyped {
	custodian, _ := uuid.Parse(r.PostFormValue("custodian"))
	return accountTyped{name: r.PostFormValue("name"), kind: r.PostFormValue("type"), custodian: custodian}
}

// accountTypeOf is the type a form names: a cash custody, or a bank, the type's zero, for any other.
func accountTypeOf(kind string) accounts.AccountType {
	if kind == "cashCustody" {
		return accounts.CashCustody
	}
	return 0
}

// custodianOf is who holds account, none for a bank.
func custodianOf(account accounts.View) uuid.UUID {
	custodian, _ := uuid.Parse(account.CustodianID)
	return custodian
}

// accountFormOf answers the details panel with New account's form - or Edit's, for editing - as typed, with the project's members to hand it to
// and why it could not be kept.
func (s site) accountFormOf(w http.ResponseWriter, r *http.Request, p page, editing accounts.View, typed accountTyped, refusal string) {
	members, err := s.Database.Queries().Members(r.Context(), p.project.ID)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, accountForm(p, editing, typed, members, refusal))
}

// newAccountForm is New account in the details panel, for whoever manages the project's settings (UC-015, redix-api #168).
func (s site) newAccountForm(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.mayKeepAccounts() {
		http.NotFound(w, r)
		return
	}
	s.accountFormOf(w, r, p, accounts.View{}, accountTyped{kind: "bank"}, "")
}

// keepAccount keeps the form posted: a new account through accounts.Add, or the account the path names through accounts.Edit - the rules the
// app's routes run - in one change. Kept, the panel shows the account and the list beside it holds it; refused, the form again says why in the
// app's words. 403 without ManageSettings; a finished project keeps nothing (AC-04.1).
func (s site) keepAccount(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.accountsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	typed, editing := accountTypedOf(r), accounts.View{}
	if r.PathValue("id") != "" {
		accountID, _ := uuid.Parse(r.PathValue("id"))
		var found bool
		if editing, found = shown.accountOf(accountID); !found {
			http.NotFound(w, r)
			return
		}
	}
	if p.finished() {
		s.accountFormOf(w, r, p, editing, typed, "projects.projectFinished")
		return
	}
	accountID := editing.ID
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		if editing.ID != uuid.Nil {
			return accounts.Edit(c, editing.ID, typed.name, typed.custodian)
		}
		var err error
		accountID, err = accounts.Add(c, typed.name, accountTypeOf(typed.kind), typed.custodian)
		return err
	})
	var refused changes.Refusal
	if errors.As(err, &refused) {
		s.accountFormOf(w, r, p, editing, typed, "accounts."+refused.Code)
		return
	}
	if err != nil {
		failed(w)
		return
	}
	if shown, err = s.accountsOf(r.Context(), p); err != nil {
		failed(w)
		return
	}
	account, _ := shown.accountOf(accountID)
	draw(w, r, http.StatusOK, accountPanel(p, shown, account, said{}), listBeside(p, shown))
}

// active is the accounts money may move between: the active ones.
func (a accountsShown) active() []accounts.View {
	var kept []accounts.View
	for _, account := range a.accounts {
		if account.IsActive {
			kept = append(kept, account)
		}
	}
	return kept
}

// mayTransfer says whether p's caller may move money between shown's accounts, as the app's Transfer button allows: MoveMoney, in a project not
// finished, with two active accounts at least.
func (p page) mayTransfer(shown accountsShown) bool {
	return !p.finished() && p.may(access.MoveMoney) && len(shown.active()) > 1
}

// transferTyped is Transfer's form as typed: from and to, the amount, the date and the notes.
type transferTyped struct {
	from, to            uuid.UUID
	amount, date, notes string
}

// transferTypedOf is the form query holds, from the first active account when it names none, dated today in the project's zone when it has no
// date.
func (s site) transferTypedOf(r *http.Request, p page, shown accountsShown, query url.Values) (transferTyped, error) {
	from, _ := uuid.Parse(query.Get("from"))
	to, _ := uuid.Parse(query.Get("to"))
	typed := transferTyped{from: from, to: to, amount: query.Get("amount"), date: query.Get("date"), notes: query.Get("notes")}
	if _, found := shown.accountOf(typed.from); !found && len(shown.active()) > 0 {
		typed.from = shown.active()[0].ID
	}
	var err error
	if typed.date == "" {
		typed.date, err = s.today(r.Context(), p)
	}
	return typed, err
}

// transferForm answers the details panel with Transfer as query holds it and why it could not be made; 404 for whoever may not transfer.
func (s site) transferFormOf(w http.ResponseWriter, r *http.Request, p page, query url.Values, refusal string) {
	shown, err := s.accountsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	if !p.inProject() || !p.mayTransfer(shown) {
		http.NotFound(w, r)
		return
	}
	typed, err := s.transferTypedOf(r, p, shown, query)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, transferForm(p, shown, typed, refusal))
}

// transferDetails is Transfer in the details panel, drawn again with what was typed when the account it moves from changes (FR-23).
func (s site) transferDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	s.transferFormOf(w, r, p, r.URL.Query(), "")
}

// transfer moves the amount the form names, through the rule the app's route runs (accounts.Transfer, FR-23), in one change: the panel closes
// and Accounts beside it shows the balances after; refused, the form again says why in the app's words. 403 without MoveMoney, as the route's
// gate refuses it.
func (s site) transfer(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.MoveMoney) || p.finished() {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	from, _ := uuid.Parse(r.PostFormValue("from"))
	to, _ := uuid.Parse(r.PostFormValue("to"))
	amount, _ := fixed.TypedIn(r.PostFormValue("amount"))
	date, _ := time.Parse(time.DateOnly, r.PostFormValue("date"))
	request := accounts.TransferRequest{FromAccountID: from, ToAccountID: to, Amount: amount.Number, Date: moment.Date{Time: date}, Notes: r.PostFormValue("notes")}
	err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error { return accounts.Transfer(c, request) })
	var refused changes.Refusal
	if errors.As(err, &refused) {
		s.transferFormOf(w, r, p, r.PostForm, "transfers."+refused.Code)
		return
	}
	if err != nil {
		failed(w)
		return
	}
	shown, err := s.accountsOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, listBeside(p, shown))
}

// mayCount says whether p's caller may update the actual balances, as the app's Update and its route's gate allow: MoveMoney, in a project not
// finished, seeing every table, since the check is one document for everyone (access.Whole, #106 R12).
func (p page) mayCount() bool {
	return !p.finished() && p.may(access.MoveMoney) && p.caller.Access.SeesEveryTable(p.project.ID)
}

// count is Update the actual balances as typed: the last count, each account's balance as typed - the last count's when nothing was typed -,
// and why it could not be kept.
type count struct {
	latest  accounts.Reconciliation
	typed   map[uuid.UUID]string
	refusal string
}

// countOf is the count query holds over the project's last one, as the app's screen fills its fields from it. The error is the database's.
func (s site) countOf(r *http.Request, p page, query url.Values) (count, error) {
	latest, err := accounts.LatestOf(r.Context(), s.Database.Queries(), p.project.ID, nil)
	shown := count{latest: latest, typed: map[uuid.UUID]string{}}
	for _, balance := range latest.Balances {
		key := "balance." + balance.AccountID.String()
		switch {
		case query.Has(key):
			shown.typed[balance.AccountID] = query.Get(key)
		case balance.WasChecked:
			shown.typed[balance.AccountID] = balance.ActualBalance.Number().Plain()
		}
	}
	return shown, err
}

// total is the sum of what is typed, as the app's screen adds its fields; what is no number adds nothing.
func (c count) total() fixed.Total {
	sum := fixed.Total{}
	for _, text := range c.typed {
		if typed, isNumber := fixed.TypedIn(text); isNumber {
			sum = sum.Plus(typed.Total())
		}
	}
	return sum
}

// difference is the total typed less the surplus the books make, as the app's screen shows it.
func (c count) difference() fixed.Total { return c.total().Minus(c.latest.Surplus) }

// countTab is Update the actual balances in its tab (FR-24, redix-api #168), for whoever may update them.
func (s site) countTab(r *http.Request, p page) (templ.Component, error) {
	if !p.mayCount() {
		return countNotOffered(p), nil
	}
	shown, err := s.countOf(r, p, r.URL.Query())
	return countPage(p, shown), err
}

// countFigures is the count's total and difference as typed, drawn again as a balance changes.
func (s site) countFigures(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.mayCount() {
		http.NotFound(w, r)
		return
	}
	shown, err := s.countOf(r, p, r.URL.Query())
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, countFiguresOf(p, shown))
}

// keepCount keeps the balances typed as one check, through the rule the app's route runs (accounts.Enter, FR-24): the count's tab closes and
// Accounts comes to the front with the new figures; a balance left empty or refused, the page again says why in the app's words. 403 for
// whoever may not update them.
func (s site) keepCount(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.mayCount() {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.countOf(r, p, r.PostForm)
	if err != nil {
		failed(w)
		return
	}
	var balances []accounts.Actual
	for _, balance := range shown.latest.Balances {
		typed, isNumber := fixed.TypedIn(shown.typed[balance.AccountID])
		if !isNumber {
			shown.refusal = "reconciliations.accountMissing"
			draw(w, r, http.StatusOK, countPage(p, shown))
			return
		}
		balances = append(balances, accounts.Actual{AccountID: balance.AccountID, ActualBalance: typed})
	}
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error { return accounts.Enter(c, balances) })
	var refused changes.Refusal
	if errors.As(err, &refused) {
		shown.refusal = "reconciliations." + refused.Code
		draw(w, r, http.StatusOK, countPage(p, shown))
		return
	}
	if err != nil {
		failed(w)
		return
	}
	s.turn(w, r, func(v view) view { return v.closing("reconcile").opening("accounts") })
}

// accountHistoryTab is the account history in its tab (redix-app #89, redix-api #168): accounts added, changed, disabled, enabled or deleted, a
// custodian handed on, transfers, approved entries and checks, newest first, searched and narrowed by account and person as the app's.
func (s site) accountHistoryTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.accountHistoryOf(r.Context(), p, r.URL.Query())
	if err != nil {
		return nil, err
	}
	listed, err := s.accountsOf(r.Context(), p)
	return accountHistoryPage(p, shown, listed), err
}

// accountHistoryMore is the next page of the account history, added after the last line.
func (s site) accountHistoryMore(w http.ResponseWriter, r *http.Request) {
	s.historyMore(w, r, "accountHistory", s.accountHistoryOf)
}
