// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// entryForm is Add entry as the app's screen offers it (UC-022, redix-api #166): the tables the caller may enter into and the one chosen, its
// columns, the payment methods, the accounts and the suppliers it may use, what was typed, and what the last save said.
type entryForm struct {
	editing   entries.Row
	tables    []entries.OfferedCategory
	item      entries.OfferedItem
	table     entries.OfferedTable
	columns   []structure.ColumnView
	methods   []settings.PaymentMethod
	method    uuid.UUID
	accounts  []accounts.View
	suppliers []suppliers.View
	contract  store.Contract
	standing  contracts.Standing
	typed     url.Values
	said      said
	refused   string
}

// title is the form's title: Add entry, or the change of the entry it edits - Fix for a rejected one - as the app's screen is titled.
func (f entryForm) title() string {
	switch {
	case f.editing.ID == uuid.Nil:
		return "entries.new"
	case f.editing.Status.Name() == "rejected":
		return "entries.fix"
	}
	return "entries.edit"
}

// tab is the tab the form lives in, which draws it again as it changes: Add entry's, or Change's.
func (f entryForm) tab() string {
	if f.editing.ID == uuid.Nil {
		return "/tab/addEntry"
	}
	return "/tab/changeEntry"
}

// action is where the form is saved: a new entry, or the entry it edits.
func (f entryForm) action() string {
	if f.editing.ID == uuid.Nil {
		return "/entry"
	}
	return "/entry/" + f.editing.ID.String()
}

// asksContract says whether the chosen table is a contract's, which asks whether a payment is inside it (FR-13).
func (f entryForm) asksContract() bool { return f.table.Kind.Name() == "contract" }

// asksSupplier says whether the chosen table is a purchase's, which may name its supplier (redix-api #129).
func (f entryForm) asksSupplier() bool { return !f.item.IsRevenue && !f.asksContract() }

// tableValue is how a table is named in the form: its item's id and its own.
func tableValue(item entries.OfferedItem, table entries.OfferedTable) string {
	return item.ID.String() + ":" + table.ID.String()
}

// tableLabel is a table as the app's picker names it (EntryTable.label): the item's own table its item's name, any other its name with its
// item's in brackets, as الجلاي (دفعات العظم).
func tableLabel(item entries.OfferedItem, table entries.OfferedTable) string {
	if table.Kind.Name() == "general" {
		return item.Name
	}
	return table.Name + " (" + item.Name + ")"
}

// entryFormOf is Add entry for p's caller in their project with what typed holds, the first table offered when it names none: the tables, columns,
// methods, accounts and suppliers come from the rules the app's routes answer with. The error is the database's.
func (s site) entryFormOf(ctx context.Context, p page, typed url.Values) (entryForm, error) {
	read := s.Database.Queries()
	c := changes.Change{Ctx: ctx, Q: read, ProjectID: p.project.ID, Actor: p.caller.UserID, Access: p.caller.Access}
	form := entryForm{typed: typed}
	var err error
	if form.tables, err = entries.Offered(c, p.caller.Access); err != nil {
		return form, err
	}
	for _, category := range form.tables {
		for _, item := range category.Items {
			for _, table := range item.Tables {
				if form.table.ID == uuid.Nil || tableValue(item, table) == typed.Get("table") {
					form.item, form.table = item, table
				}
			}
		}
	}
	if form.table.ID == uuid.Nil {
		return form, nil
	}
	rows, err := entries.AskedColumns(c, form.item.ID, form.table.ID)
	if err != nil {
		return form, err
	}
	for _, row := range rows {
		if !row.IsHidden && !structure.IsEntryField(row.Key) && structure.DataType(row.DataType) != structure.File {
			form.columns = append(form.columns, structure.ColumnOf(row))
		}
	}
	held, err := accounts.ListOf(ctx, read, p.project.ID, p.caller, !form.item.IsRevenue)
	if err != nil {
		return form, err
	}
	paysFrom := func(accounts.View) bool { return true }
	if !form.item.IsRevenue {
		if form.methods, err = settings.MethodsOf(ctx, read, p.project.ID, true); err != nil {
			return form, err
		}
		chosen := settings.PaymentMethod{}
		for _, method := range form.methods {
			if chosen.ID == uuid.Nil || method.ID.String() == typed.Get("method") {
				chosen = method
			}
		}
		form.method = chosen.ID
		paysFrom = func(account accounts.View) bool { return slices.Contains(chosen.AccountIDs, account.ID) }
	}
	for _, account := range held {
		if account.IsActive && paysFrom(account) {
			form.accounts = append(form.accounts, account)
		}
	}
	if form.asksSupplier() {
		form.suppliers, err = suppliers.Search(ctx, read, p.project.ID, p.caller, typed.Get("supplierText"))
	}
	if form.asksContract() && err == nil {
		payment, _ := fixed.TypedIn(typed.Get("amount"))
		if typed.Get("outside") == "true" {
			payment = fixed.Typed{}
		}
		leftOut, _ := uuid.Parse(typed.Get("entry"))
		form.contract, form.standing, err = contracts.StandingIn(ctx, read, p.project.ID, form.table.ID, leftOut, payment.Total())
	}
	return form, err
}

// progress is how far the contract is reached with this payment, as a percent from 0 to 100, as the app's bar draws it: full for a contract of
// no value.
func (f entryForm) progress() string {
	if f.contract.Value <= 0 {
		return "100"
	}
	reached := f.contract.Value - f.standing.RemainingAfter.Number()
	return strconv.FormatInt(min(max(int64(reached)*100/int64(f.contract.Value), 0), 100), 10)
}

// figuresFollow makes a contract payment's field draw the contract's figures again as it changes, as the app's figures follow what is typed.
func (f entryForm) figuresFollow() templ.Attributes {
	if !f.asksContract() {
		return templ.Attributes{}
	}
	return templ.Attributes{"hx-get": "/entry-figures", "hx-include": "closest form", "hx-trigger": "change", "hx-target": "[id='addEntry.figures']", "hx-swap": "outerHTML"}
}

// entryFigures is a contract payment's figures as the form now stands: the contract's value, what is paid, what remains after this payment, what
// waits, and whether it goes beyond the value (BR-07, redix-api #166).
func (s site) entryFigures(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	form, err := s.entryFormOf(r.Context(), p, r.URL.Query())
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, contractFigures(p, form))
}

// account is the account the form pays from: the one typed when the form still offers it, the first offered otherwise.
func (f entryForm) account() uuid.UUID {
	for _, account := range f.accounts {
		if account.ID.String() == f.typed.Get("account") {
			return account.ID
		}
	}
	if len(f.accounts) == 0 {
		return uuid.Nil
	}
	return f.accounts[0].ID
}

// request is what the form's typed values ask the API to record as entryID, as the app's screen sends it (AddEntryScreen._body): its table,
// date, amount, account, payment method for an expense, description, column values in the item's order, supplier for a purchase, and inside
// or outside the contract for a contract's payment; sent for approval when send.
func (f entryForm) request(entryID uuid.UUID, send bool) entries.Request {
	typed := f.typed
	date, _ := time.Parse(time.DateOnly, typed.Get("date"))
	amount, _ := fixed.TypedIn(typed.Get("amount"))
	values := web.Ordered{Values: map[string]string{}}
	for _, column := range f.columns {
		values.Keys = append(values.Keys, column.Key)
		values.Values[column.Key] = typed.Get("column." + column.Key)
	}
	request := entries.Request{ID: entryID, ItemID: f.item.ID, TableID: f.table.ID, Date: moment.Date{Time: date}, Amount: amount, AccountID: f.account(),
		Description: typed.Get("description"), ColumnValues: values, Submit: send, IsOutsideContract: f.asksContract() && typed.Get("outside") == "true"}
	if !f.item.IsRevenue {
		request.PaymentMethodID = f.method
	}
	if f.asksSupplier() {
		request.SupplierID, _ = uuid.Parse(typed.Get("supplier"))
	}
	return request
}

// whatIsWrong is the word of what stops the form being saved before it reaches the API, as the app's screen checks it (_whatIsWrong): an amount
// that is no number, or a method that pays from no account the caller may use; "" when nothing does.
func (f entryForm) whatIsWrong() string {
	if typed := strings.TrimSpace(f.typed.Get("amount")); typed != "" {
		if _, isNumber := fixed.TypedIn(typed); !isNumber {
			return "expenses.invalidAmount"
		}
	}
	if !f.item.IsRevenue && f.method != uuid.Nil && len(f.accounts) == 0 {
		return "expenses.noAccountForMethod"
	}
	return ""
}

// refusedColumn is the column a refusal of an entry names (entries' columnKey), "" for one naming none.
func refusedColumn(refused changes.Refusal) string {
	column, _ := refused.Detail.(string)
	if refused.Name != "columnKey" {
		return ""
	}
	return column
}

// today is the day it is in the project's time zone, Add entry's first date (BR-12).
func (s site) today(ctx context.Context, p page) (string, error) {
	zone, err := zones.Of(ctx, s.Database.Queries(), p.project.ID)
	return zones.Today(zone, s.Clock.Now()).Format(time.DateOnly), err
}

// addEntryTab is Add entry in its own tab, drawn again with what was typed whenever the table, the payment method or the supplier search
// changes, so what it offers always follows the API's rules (redix-api #166).
func (s site) addEntryTab(r *http.Request, p page) (templ.Component, error) {
	typed := r.URL.Query()
	if typed.Get("date") == "" {
		today, err := s.today(r.Context(), p)
		if err != nil {
			return nil, err
		}
		typed.Set("date", today)
	}
	form, err := s.entryFormOf(r.Context(), p, typed)
	return addEntry(p, form), err
}

// changeEntryTab is Change or Fix in its own tab: Add entry filled with the entry the query names as it stands (entries.Typed), then drawn
// again with what was typed as it changes, as Add entry is (redix-api #166). An entry that is gone, or one the caller may not change, says so.
func (s site) changeEntryTab(r *http.Request, p page) (templ.Component, error) {
	typed := r.URL.Query()
	entryID, _ := uuid.Parse(typed.Get("entry"))
	row, found, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, entryID)
	if err != nil || !found || !p.mayChange(row) {
		return entryNotChangeable(p), err
	}
	if typed.Get("table") == "" {
		c := changes.Change{Ctx: r.Context(), Q: s.Database.Queries(), ProjectID: p.project.ID, Actor: p.caller.UserID, Access: p.caller.Access}
		saved, err := entries.Typed(c, p.caller, entryID)
		if err != nil {
			return nil, err
		}
		typed = valuesOf(saved)
		typed.Set("entry", entryID.String())
	}
	form, err := s.entryFormOf(r.Context(), p, typed)
	form.editing = row
	return addEntry(p, form), err
}

// valuesOf is an entry's request as the form's fields hold it.
func valuesOf(saved entries.Request) url.Values {
	amount := strings.TrimRight(strings.TrimRight(saved.Amount.String(), "0"), ".")
	values := url.Values{"table": {saved.ItemID.String() + ":" + saved.TableID.String()}, "description": {saved.Description},
		"date": {saved.Date.Format(time.DateOnly)}, "amount": {amount}, "method": {saved.PaymentMethodID.String()}, "account": {saved.AccountID.String()},
		"supplier": {saved.SupplierID.String()}, "outside": {strconv.FormatBool(saved.IsOutsideContract)}}
	for _, key := range saved.ColumnValues.Keys {
		values.Set("column."+key, saved.ColumnValues.Values[key])
	}
	return values
}

// changeEntry changes the entry the path names to the form, through the checks the app's route makes (entries.Change), as a draft or sent for
// approval as the button pressed says (UC-033, redix-api #166).
func (s site) changeEntry(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	entryID, _ := uuid.Parse(r.PathValue("id"))
	row, found, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, entryID)
	if err != nil {
		failed(w)
		return
	}
	if !found || !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	s.save(w, r, p, row, func(c changes.Change, typed entries.Request) error {
		return entries.Change(c, p.caller, entryID, typed)
	})
}

// recordEntry records the form as a new entry through the rule the app's route and a project file's load run (entries.Record), as a draft or
// sent for approval as the button pressed says: the form again, empty but for its table, method and account, saying it was kept, with the
// new entry in the details panel; or the form as typed, saying why not and marking the column refused (UC-022, redix-api #166). A finished
// project records nothing, as the app's route refuses it at its gate (AC-04.1).
func (s site) recordEntry(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	s.save(w, r, p, entries.Row{}, func(c changes.Change, typed entries.Request) error {
		_, err := entries.Record(c, typed, true)
		return err
	})
}

// save saves the form posted to r through keep in one change, as the change of editing or as a new entry when editing is none: the form again
// saying it was kept, with the entry in the details panel - a new entry's form empty but for its table, method, account and date; or the form
// as typed, saying why not and marking the column refused. A finished project saves nothing, as the app's route refuses it at its gate (AC-04.1).
func (s site) save(w http.ResponseWriter, r *http.Request, p page, editing entries.Row, keep func(changes.Change, entries.Request) error) {
	form, err := s.entryFormOf(r.Context(), p, r.PostForm)
	form.editing = editing
	if err != nil {
		failed(w)
		return
	}
	if form.said.refusal = form.whatIsWrong(); p.finished() {
		form.said.refusal = "projects.projectFinished"
	}
	if form.said.refusal != "" {
		draw(w, r, http.StatusOK, addEntry(p, form))
		return
	}
	send, entryID := r.PostForm.Get("submit") == "send", editing.ID
	if entryID == uuid.Nil {
		entryID = uuid.Must(uuid.NewV7())
	}
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		return keep(c, form.request(entryID, send))
	})
	var refused changes.Refusal
	if errors.As(err, &refused) {
		form.said.refusal, form.refused = "expenses."+refused.Code, refusedColumn(refused)
		if refused.Code == "" {
			form.said.refusal = "expenses.refused"
		}
		draw(w, r, http.StatusOK, addEntry(p, form))
		return
	}
	if err != nil {
		failed(w)
		return
	}
	row, _, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, entryID)
	if err != nil {
		failed(w)
		return
	}
	overrun := form.standing.IsOverrun
	if editing.ID == uuid.Nil {
		kept := url.Values{"table": {r.PostForm.Get("table")}, "method": {r.PostForm.Get("method")}, "account": {r.PostForm.Get("account")}, "date": {r.PostForm.Get("date")}}
		if form, err = s.entryFormOf(r.Context(), p, kept); err != nil {
			failed(w)
			return
		}
	} else {
		form.editing = row
	}
	form.said = said{notice: "expenses.saved"}
	if send {
		form.said.notice = "expenses.sent"
	}
	if overrun {
		form.said.notice = "contractorPayments.savedOverrun"
	}
	draw(w, r, http.StatusOK, addEntry(p, form), panelBeside(p, row))
}
