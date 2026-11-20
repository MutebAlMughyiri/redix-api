// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package accounts

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
	"github.com/google/uuid"
)

// What kind of account holds a project's money (AccountType, FR-22).
const (
	Bank AccountType = iota
	CashCustody
)

var accountTypes = web.Enum{"bank", "cashCustody"}

// AccountType is what an account is: a Bank account, or cash a custodian holds (FR-21).
type AccountType int64

func (t AccountType) MarshalJSON() ([]byte, error) { return accountTypes.Write(int64(t)) }

func (t *AccountType) UnmarshalJSON(data []byte) error {
	value, err := accountTypes.Read(data)
	*t = AccountType(value)
	return err
}

// notesMaxLength is the longest notes a transfer keeps (Transfer.NotesMaxLength).
const notesMaxLength = 1000

// Routes keeps a project's accounts.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the accounts: reading them for whoever reads the project (BR-31), changing them for whoever holds ManageSettings, and moving money
// and checking it for whoever holds MoveMoney, in a project not finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const project = "/projects/{projectId:guid}"
	keeps := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", project+"/accounts", gate.Needs(access.ReadProject, s.list(false)))
	router.Handle("GET", project+"/accounts/offered", gate.Needs(access.ReadProject, s.list(true)))
	router.Handle("POST", project+"/accounts", keeps(s.add))
	router.Handle("PUT", project+"/accounts/{accountId:guid}", keeps(s.edit))
	router.Handle("POST", project+"/accounts/{accountId:guid}/disable", keeps(s.activate(false)))
	router.Handle("POST", project+"/accounts/{accountId:guid}/enable", keeps(s.activate(true)))
	router.Handle("DELETE", project+"/accounts/{accountId:guid}", keeps(s.delete))
	router.Handle("POST", project+"/transfers", gate.Changes(access.MoveMoney, s.transfer))
	router.Handle("GET", project+"/reconciliations/latest", gate.Needs(access.ReadProject, s.latest))
	router.Handle("POST", project+"/reconciliations", gate.Changes(access.MoveMoney, access.Whole(s.enter)))
}

// accountView is an account as the app shows it (AccountView).
type accountView struct {
	ID            uuid.UUID   `json:"id"`
	Type          AccountType `json:"type"`
	Name          string      `json:"name"`
	Balance       fixed.Total `json:"balance"`
	CustodianID   string      `json:"custodianId"`
	CustodianName string      `json:"custodianName"`
	IsMine        bool        `json:"isMine"`
	IsActive      bool        `json:"isActive"`
	IsUsed        bool        `json:"isUsed"`
}

// list is the project's accounts, banks first and then in the order they were added (AC-22.1): every one, or those the caller's entry may be paid
// from - the active ones, all of them with ChargeAnyAccount, otherwise only those they hold (BRS open question 4).
func (s Routes) list(offeredOnly bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		read, projectID := s.database.Queries(), web.ID(r, "projectId")
		accounts, err := read.AccountsOf(r.Context(), projectID)
		balances, balancesErr := Balances(r.Context(), read, projectID, caller.Access.Unseen(projectID))
		used, usedErr := read.AccountsUsed(r.Context(), projectID)
		if err != nil || balancesErr != nil || usedErr != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		chargesAny := caller.Access.Holds(projectID, access.ChargeAnyAccount)
		var views []accountView
		for _, account := range accounts {
			mine := account.CustodianID == caller.UserID && account.CustodianID != uuid.Nil
			if offeredOnly && !(account.IsActive && (chargesAny || mine)) {
				continue
			}
			view := accountView{ID: account.ID, Type: AccountType(account.Type), Name: account.Name, Balance: balances[account.ID],
				CustodianName: account.CustodianName, IsMine: mine, IsActive: account.IsActive, IsUsed: slices.Contains(used, account.ID)}
			if account.CustodianID != uuid.Nil {
				view.CustodianID = account.CustodianID.String()
			}
			views = append(views, view)
		}
		web.JSON(w, r, http.StatusOK, views)
	}
}

// refused answers why an account named name, of its type, held by custodianID, cannot be kept (Account.RefusalOf): invalidName,
// custodianRequired for a cash custody nobody holds, unknownCustodian for a custodian not in the project (400).
func (s Routes) refused(w http.ResponseWriter, r *http.Request, name string, typeOf int64, custodianID uuid.UUID) bool {
	refusal := ""
	if _, named := naming.Named(name); !named {
		refusal = naming.Invalid
	} else if AccountType(typeOf) == CashCustody && custodianID == uuid.Nil {
		refusal = "custodianRequired"
	} else if custodianID != uuid.Nil {
		member, err := s.database.Queries().IsMember(r.Context(), store.IsMemberParams{ProjectID: web.ID(r, "projectId"), UserID: custodianID})
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return true
		}
		if !member {
			refusal = "unknownCustodian"
		}
	}
	if refusal != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
	}
	return refusal != ""
}

func accountFields(row store.Account) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "Type": row.Type, "Name": row.Name, "IsActive": row.IsActive}
}

// custodianFields is who holds an account as History keeps it: the account, the row's key, is the line's record and not among its values, as
// HistoryRecord.Of left the key out.
func custodianFields(userID uuid.UUID) map[string]any { return map[string]any{"UserId": userID} }

// addAccountRequest is the body POST /projects/{projectId}/accounts (UC-015) reads.
type addAccountRequest struct {
	Name        string      `json:"name"`
	Type        AccountType `json:"type"`
	CustodianID uuid.UUID   `json:"custodianId,omitzero"`
}

// add adds an account, which holds nothing until an entry or a transfer moves money into it; its custodian is recorded under its line, as .NET
// recorded it.
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addAccountRequest
	if !web.Read(w, r, &request) || s.refused(w, r, request.Name, int64(request.Type), request.CustodianID) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		name, _ := naming.Named(request.Name)
		account := store.Account{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Type: int64(request.Type), Name: name, IsActive: true}
		if err := c.Q.SaveAccount(c.Ctx, store.SaveAccountParams(account)); err != nil {
			return err
		}
		if request.CustodianID != uuid.Nil {
			if err := c.Q.SaveCustodian(c.Ctx, store.SaveCustodianParams{AccountID: account.ID, UserID: request.CustodianID}); err != nil {
				return err
			}
		}
		if err := settings.TickOnEveryMethod(c, account.ID); err != nil {
			return err
		}
		return c.Record(history.Create, "Accounts", account.ID, nil, accountFields(account))
	})
}

// account is the route's account: 404 for one the project does not hold.
func (s Routes) account(w http.ResponseWriter, r *http.Request) (store.Account, bool) {
	account, err := changes.Found(s.database.Queries().AccountOf(r.Context(), store.AccountOfParams{ID: web.ID(r, "accountId"), ProjectID: web.ID(r, "projectId")}))
	return account, !changes.Answered(w, r, err)
}

// editAccountRequest is the body PUT /projects/{projectId}/accounts/{accountId} reads (UC-015).
type editAccountRequest struct {
	Name        string    `json:"name"`
	CustodianID uuid.UUID `json:"custodianId,omitzero"`
}

// edit changes an account's name and who holds it; its type never changes. It answers as add does; each change is in History as .NET's
// AddHistoryOfChanges wrote it, the custodian's under an update whatever became of its row.
func (s Routes) edit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request editAccountRequest
	if !web.Read(w, r, &request) {
		return
	}
	before, found := s.account(w, r)
	if !found || s.refused(w, r, request.Name, before.Type, request.CustodianID) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		after := before
		after.Name, _ = naming.Named(request.Name)
		if err := c.Save("Accounts", after.ID, false, accountFields(before), accountFields(after),
			func() error { return c.Q.SaveAccount(c.Ctx, store.SaveAccountParams(after)) }); err != nil {
			return err
		}
		return handTo(c, before.ID, request.CustodianID)
	})
}

// handTo makes userID the one who holds the account, uuid.Nil nobody (Account.HandTo).
func handTo(c changes.Change, accountID, userID uuid.UUID) error {
	held, err := c.Q.CustodianOf(c.Ctx, accountID)
	switch {
	case err != nil:
		return err
	case len(held) == 0 && userID == uuid.Nil, len(held) > 0 && held[0].UserID == userID:
		return nil
	case userID == uuid.Nil:
		if err := c.Q.RemoveCustodian(c.Ctx, accountID); err != nil {
			return err
		}
		return c.Record(history.Update, "AccountCustodians", accountID, custodianFields(held[0].UserID), nil)
	}
	if err := c.Q.SaveCustodian(c.Ctx, store.SaveCustodianParams{AccountID: accountID, UserID: userID}); err != nil {
		return err
	}
	if len(held) == 0 {
		return c.Record(history.Update, "AccountCustodians", accountID, nil, custodianFields(userID))
	}
	return c.Record(history.Update, "AccountCustodians", accountID, map[string]any{"UserId": held[0].UserID}, map[string]any{"UserId": userID})
}

// activate disables an account, so it leaves entry and every account picker and stays in the reports (BR-15), or brings it back. Its line is
// written even when nothing changed, as .NET wrote it, since the account history shows it.
func (s Routes) activate(active bool) access.Handler {
	action := history.Disable
	if active {
		action = history.Enable
	}
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		before, found := s.account(w, r)
		if !found {
			return
		}
		s.writer.Done(w, r, caller, func(c changes.Change) error {
			after := before
			after.IsActive = active
			if err := c.Q.SaveAccount(c.Ctx, store.SaveAccountParams(after)); err != nil {
				return err
			}
			was, is := history.Changed(accountFields(before), accountFields(after))
			return c.Record(action, "Accounts", after.ID, was, is)
		})
	}
}

// delete deletes an account nothing names: accountInUse (409) for one an entry in any status, a check or a transfer names, which is disabled
// instead (BR-15). Its history stays.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	account, found := s.account(w, r)
	if !found {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		used, err := c.Q.AccountsUsed(c.Ctx, c.ProjectID)
		if err != nil {
			return err
		}
		if slices.Contains(used, account.ID) {
			return changes.Refuse(http.StatusConflict, "accountInUse")
		}
		if err := c.Q.RemoveCustodian(c.Ctx, account.ID); err != nil {
			return err
		}
		if err := c.Q.RemoveAccountFromPaymentMethods(c.Ctx, account.ID); err != nil {
			return err
		}
		if err := c.Q.RemoveAccount(c.Ctx, account.ID); err != nil {
			return err
		}
		return c.Record(history.Delete, "Accounts", account.ID, accountFields(account), nil)
	})
}

// transferRequest is the body POST /projects/{projectId}/transfers reads (FR-23).
type transferRequest struct {
	FromAccountID uuid.UUID    `json:"fromAccountId"`
	ToAccountID   uuid.UUID    `json:"toAccountId"`
	Amount        fixed.Number `json:"amount"`
	Date          moment.Date  `json:"date"`
	Notes         string       `json:"notes"`
}

// transfer moves an amount from one account of the project to another (FR-23), which counts at once in both balances: unknownAccount for an
// account not in the project, then sameAccount, accountDisabled, invalidAmount for none or more than two decimals, invalidNotes past 1,000
// characters, insufficientBalance for more than the first account holds (400).
func (s Routes) transfer(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request transferRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		from, err := c.Q.AccountOf(c.Ctx, store.AccountOfParams{ID: request.FromAccountID, ProjectID: c.ProjectID})
		to, toErr := c.Q.AccountOf(c.Ctx, store.AccountOfParams{ID: request.ToAccountID, ProjectID: c.ProjectID})
		if err := changes.Unknown(errors.Join(err, toErr), "unknownAccount"); err != nil {
			return err
		}
		balances, err := Balances(c.Ctx, c.Q, c.ProjectID, access.Everything)
		if err != nil {
			return err
		}
		notes := strings.TrimSpace(request.Notes)
		switch {
		case from.ID == to.ID:
			return changes.Refuse(http.StatusBadRequest, "sameAccount")
		case !from.IsActive || !to.IsActive:
			return changes.Refuse(http.StatusBadRequest, "accountDisabled")
		case request.Amount <= 0 || !request.Amount.IsAcceptableMoney():
			return changes.Refuse(http.StatusBadRequest, "invalidAmount")
		case utf8.RuneCountInString(notes) > notesMaxLength:
			return changes.Refuse(http.StatusBadRequest, "invalidNotes")
		case request.Amount > balances[from.ID].Number():
			return changes.Refuse(http.StatusBadRequest, "insufficientBalance")
		}
		transfer := store.AddTransferParams{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, FromAccountID: from.ID, ToAccountID: to.ID, Amount: request.Amount,
			Date: request.Date, Notes: notes, CreatedByUserID: c.Actor, CreatedAt: c.Now}
		if err := c.Q.AddTransfer(c.Ctx, transfer); err != nil {
			return err
		}
		return c.Record(history.Create, "Transfers", transfer.ID, nil, map[string]any{"ProjectId": transfer.ProjectID, "FromAccountId": transfer.FromAccountID,
			"ToAccountId": transfer.ToAccountID, "Amount": transfer.Amount, "Date": transfer.Date, "Notes": transfer.Notes,
			"CreatedByUserId": transfer.CreatedByUserID, "CreatedAt": transfer.CreatedAt})
	})
}

// Reconciliation is the accounts against the books (ReconciliationView).
type Reconciliation struct {
	At          moment.Moment    `json:"at"`
	Surplus     fixed.Total      `json:"surplus"`
	ActualTotal fixed.Total      `json:"actualTotal"`
	Difference  fixed.Total      `json:"difference"`
	Balances    []AccountBalance `json:"balances"`
}

// AccountBalance is one account in a reconciliation: what the books say it holds, and what it really held.
type AccountBalance struct {
	AccountID     uuid.UUID   `json:"accountId"`
	AccountName   string      `json:"accountName"`
	Type          AccountType `json:"type"`
	BooksBalance  fixed.Total `json:"booksBalance"`
	ActualBalance fixed.Total `json:"actualBalance"`
	WasChecked    bool        `json:"wasChecked"`
}

// LatestOf is the surplus now, the latest check's balances and the difference; with no check yet the balances are empty and the difference is
// what the surplus says is missing (UC-041, BR-08). typed are the balances of a check just entered, as they were typed, which .NET's answer
// carried; nil for one read back.
func LatestOf(ctx context.Context, q *store.Queries, projectID uuid.UUID, typed map[uuid.UUID]fixed.Total) (Reconciliation, error) {
	latest, err := q.LatestReconciliation(ctx, projectID)
	if err != nil {
		return Reconciliation{}, err
	}
	view, actual := Reconciliation{}, map[uuid.UUID]fixed.Total{}
	for _, check := range latest {
		view.At = check.At
		balances, err := q.BalancesOf(ctx, check.ID)
		if err != nil {
			return view, err
		}
		for _, row := range balances {
			actual[row.AccountID] = fixed.Counted(row.ActualBalance)
			if as, given := typed[row.AccountID]; given {
				actual[row.AccountID] = as
			}
			view.ActualTotal = view.ActualTotal.Plus(actual[row.AccountID])
		}
	}
	figures, err := FiguresOf(ctx, q, projectID, access.Everything)
	if err != nil {
		return view, err
	}
	accounts, err := q.AccountsOf(ctx, projectID)
	moved, movedErr := Balances(ctx, q, projectID, access.Everything)
	for _, account := range accounts {
		held, checked := actual[account.ID]
		if account.IsActive || checked {
			view.Balances = append(view.Balances, AccountBalance{AccountID: account.ID, AccountName: account.Name, Type: AccountType(account.Type),
				BooksBalance: moved[account.ID], ActualBalance: held, WasChecked: checked})
		}
	}
	view.Surplus = figures.Surplus()
	view.Difference = view.ActualTotal.Minus(view.Surplus)
	return view, errors.Join(err, movedErr)
}

// latest is the project's latest reconciliation: the surplus, the latest balances and their difference (UC-040).
func (s Routes) latest(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	view, err := LatestOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), nil)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, view)
}

// Actual is what an account really held, as it was typed (ActualBalanceRequest).
type Actual struct {
	AccountID     uuid.UUID   `json:"accountId"`
	ActualBalance fixed.Typed `json:"actualBalance"`
}

// Enter keeps what each account really holds as one check of the change's project (FR-24, ReconciliationsEndpoints.EnterAsync): unknownAccount
// for an account not in the project, invalidAmount for more than two decimals, accountMissing for an active account left out (400). The one way a
// check is kept, by hand or by a project file's load.
func Enter(c changes.Change, balances []Actual) error {
	accounts, err := c.Q.AccountsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	given := map[uuid.UUID]bool{}
	for _, row := range balances {
		given[row.AccountID] = true
		if !slices.ContainsFunc(accounts, func(account store.AccountsOfRow) bool { return account.ID == row.AccountID }) {
			return changes.Refuse(http.StatusBadRequest, "unknownAccount")
		}
	}
	for _, row := range balances {
		if !row.ActualBalance.IsAcceptableMoney() {
			return changes.Refuse(http.StatusBadRequest, "invalidAmount")
		}
	}
	for _, account := range accounts {
		if account.IsActive && !given[account.ID] {
			return changes.Refuse(http.StatusBadRequest, "accountMissing")
		}
	}
	check := store.AddReconciliationParams{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, At: c.Now, UserID: c.Actor}
	if err := c.Q.AddReconciliation(c.Ctx, check); err != nil {
		return err
	}
	for _, row := range balances {
		if err := c.Q.AddReconciliationBalance(c.Ctx, store.AddReconciliationBalanceParams{ReconciliationID: check.ID, AccountID: row.AccountID,
			ActualBalance: row.ActualBalance.Number}); err != nil {
			return err
		}
	}
	return c.Record(history.Create, "Reconciliations", check.ID, nil, map[string]any{"ProjectId": check.ProjectID, "At": check.At, "UserId": check.UserID})
}

// reconciliationRequest is the body POST /projects/{projectId}/reconciliations reads (UC-040).
type reconciliationRequest struct {
	Balances []Actual `json:"balances"`
}

// enter keeps a check as Enter does, and answers the accounts against the books, each balance as it was typed.
func (s Routes) enter(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reconciliationRequest
	if !web.Read(w, r, &request) {
		return
	}
	entered := s.writer.Write(w, r, caller, func(c changes.Change) error { return Enter(c, request.Balances) })
	if !entered {
		return
	}
	typed := map[uuid.UUID]fixed.Total{}
	for _, row := range request.Balances {
		typed[row.AccountID] = row.ActualBalance.Total()
	}
	view, err := LatestOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), typed)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, view)
}

// Reconciled is when the project's latest check was made, the zero moment for none yet, and what it differs from the surplus (UC-041, BR-08): what
// the summary and the periodic report show of it.
func Reconciled(ctx context.Context, q *store.Queries, projectID uuid.UUID) (moment.Moment, fixed.Total, error) {
	view, err := LatestOf(ctx, q, projectID, nil)
	if err != nil {
		return moment.Moment{}, fixed.Total{}, err
	}
	return view.At, view.Difference, nil
}
