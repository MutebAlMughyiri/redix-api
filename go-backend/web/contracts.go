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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
)

// contractFilter is one of Contracts' filters, as the app's ContractFilter keeps contracts.
type contractFilter struct {
	name, word string
	keeps      func(contract contracts.View) bool
}

// contractFilters are Contracts' filters, in the app's order: every contract, those overrun, those of open value.
var contractFilters = []contractFilter{
	{"all", "contracts.filterAll", func(contracts.View) bool { return true }},
	{"overrun", "contracts.filterOverrun", func(contract contracts.View) bool { return contract.IsOverrun }},
	{"openValue", "contracts.filterOpenValue", func(contract contracts.View) bool { return contract.IsOpenValue }},
}

// contractsShown is Contracts as the app's screen shows it (UC-020): every contract, disabled ones included, in the order added, the filter
// chosen and the search typed.
type contractsShown struct {
	all    []contracts.View
	filter contractFilter
	text   string
}

// shown is what the filter and the search keep: as the app's, the search finds a contract's contractor, item, notes and value.
func (s contractsShown) shown() []contracts.View {
	return slices.DeleteFunc(slices.Clone(s.all), func(contract contracts.View) bool {
		return !s.filter.keeps(contract) || !searchHolds(s.text, contract.SupplierName, contract.ItemName, contract.Notes, contract.Value.Plain(),
			statement.Money(contract.Value))
	})
}

// count is how many contracts filter keeps, as the app's filter says beside its name.
func (s contractsShown) count(filter contractFilter) int {
	count := 0
	for _, contract := range s.all {
		if filter.keeps(contract) {
			count++
		}
	}
	return count
}

// contractOf is the contract contractID, and whether the project holds one.
func (s contractsShown) contractOf(contractID uuid.UUID) (contracts.View, bool) {
	at := slices.IndexFunc(s.all, func(contract contracts.View) bool { return contract.ID == contractID })
	if at < 0 {
		return contracts.View{}, false
	}
	return s.all[at], true
}

// contractStanding is the word of where contract stands, as the app's chip says it (Contract.standingWordKey).
func contractStanding(contract contracts.View) string {
	switch {
	case !contract.IsActive:
		return "contracts.disabled"
	case contract.IsOverrun:
		return "contracts.overrun"
	case contract.IsOpenValue:
		return "contracts.openValue"
	case contract.Paid.Number() == 0:
		return "contracts.notStarted"
	}
	return "contracts.running"
}

// contractValue is a contract's value as the app's card writes it: open for an open-value contract.
func contractValue(p page, contract contracts.View) string {
	if contract.IsOpenValue {
		return p.words.Of("contracts.open")
	}
	return statement.Money(contract.Value)
}

// contractRemaining is what remains of a contract as the app's card writes it: nothing for an open-value contract.
func contractRemaining(contract contracts.View) string {
	if contract.IsOpenValue {
		return "—"
	}
	return statement.Money(contract.Remaining.Number())
}

// contractTerms is a contract's unit price and quantity as the app's card writes them: price × quantity, or the price alone.
func contractTerms(contract contracts.View) string {
	switch {
	case contract.UnitPrice > 0 && contract.Quantity > 0:
		return contract.UnitPrice.Plain() + " × " + contract.Quantity.Plain()
	case contract.UnitPrice > 0:
		return contract.UnitPrice.Plain()
	}
	return ""
}

// contractWaiting is the app's line under a contract with payments waiting for approval: what waits and, but for an open-value contract, what
// will remain once it is approved.
func contractWaiting(p page, contract contracts.View) string {
	waiting := p.words.Of("contracts.pending") + " " + statement.Money(contract.Pending.Number())
	if contract.IsOpenValue {
		return waiting
	}
	return parts(waiting, statement.Money(contract.Value-contract.Paid.Number()-contract.Pending.Number())+" "+p.words.Of("contracts.afterApproval"))
}

// contractsOf is Contracts as p's caller sees it with what query chose, through the rule the app's route answers (contracts.List). The error is
// the database's.
func (s site) contractsOf(ctx context.Context, p page, query url.Values) (contractsShown, error) {
	all, err := contracts.List(ctx, s.Database.Queries(), p.project.ID)
	shown := contractsShown{all: all, filter: contractFilters[0], text: strings.TrimSpace(query.Get("text"))}
	for _, filter := range contractFilters {
		if filter.name == query.Get("filter") {
			shown.filter = filter
		}
	}
	return shown, err
}

// contractsTab is Contracts in its tab (UC-020, redix-api #170): everyone in the project reads the contracts, with their value, paid and
// remaining; whoever keeps the settings adds, amends, disables and enables them.
func (s site) contractsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.contractsOf(r.Context(), p, r.URL.Query())
	return contractsList(p, shown), err
}

// mayKeepContracts says whether p's caller may change the contracts: ManageSettings, in a project not finished, as the app's screen offers New
// contract and each card's menu.
func (p page) mayKeepContracts() bool { return !p.finished() && p.may(access.ManageSettings) }

// contractTyped is a contract as its form types it, with the documents it adds; an empty contractor or item keeps the contract's own.
type contractTyped struct {
	contractor, item                                  uuid.UUID
	contractorText, value, unitPrice, quantity, notes string
	openValue                                         bool
	documents                                         []store.Attachment
}

// contractTypedIn is the contract form's fields in form, or editing's own terms when the form was not drawn yet.
func contractTypedIn(form url.Values, editing contracts.View) contractTyped {
	if !form.Has("value") {
		return contractTyped{openValue: editing.IsOpenValue, notes: editing.Notes, value: typedFigure(editing.Value), unitPrice: typedFigure(editing.UnitPrice),
			quantity: typedFigure(editing.Quantity)}
	}
	contractor, _ := uuid.Parse(form.Get("contractor"))
	item, _ := uuid.Parse(form.Get("item"))
	return contractTyped{contractor: contractor, item: item, contractorText: form.Get("contractorText"), value: form.Get("value"),
		unitPrice: form.Get("unitPrice"), quantity: form.Get("quantity"), notes: form.Get("notes"), openValue: form.Get("openValue") == "true"}
}

// typedFigure is number as the app's dialog starts its field: blank for nothing.
func typedFigure(number fixed.Number) string {
	if number == 0 {
		return ""
	}
	return number.Plain()
}

// terms is what typed asks of the contract, or the refusal word of a figure that is no number, as the app's dialog refuses it before sending.
func (typed contractTyped) terms() (contracts.Terms, string) {
	terms := contracts.Terms{IsOpenValue: typed.openValue, Notes: typed.notes}
	for _, document := range typed.documents {
		terms.AttachmentIDs = append(terms.AttachmentIDs, document.ID)
	}
	figures := []struct {
		text, refusal string
		into          *fixed.Number
		optional      bool
	}{{typed.value, "contracts.invalidValue", &terms.Value, typed.openValue}, {typed.unitPrice, "contracts.invalidUnitPrice", &terms.UnitPrice, true},
		{typed.quantity, "contracts.invalidQuantity", &terms.Quantity, true}}
	for _, figure := range figures {
		number, isNumber := fixed.TypedIn(figure.text)
		switch {
		case isNumber:
			*figure.into = number.Number
		case !figure.optional || strings.TrimSpace(figure.text) != "":
			return terms, figure.refusal
		}
	}
	return terms, ""
}

// contractFormOf answers the details panel with New contract's form - or Edit's, for editing - as typed: the contractors the search finds, the
// items that take contractor payments, and why it could not be kept.
func (s site) contractFormOf(w http.ResponseWriter, r *http.Request, p page, editing contracts.View, typed contractTyped, refusal string) {
	read := s.Database.Queries()
	contractors, err := suppliers.Search(r.Context(), read, p.project.ID, p.caller, typed.contractorText)
	items, itemsErr := structure.OfferedOf(r.Context(), read, p.project.ID, structure.ContractorPayments)
	if errors.Join(err, itemsErr) != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, contractForm(p, editing, typed, contractors, items, refusal))
}

// contractDetails is a contract in the details panel, or Edit's form for ?asking=edit; 404 for one the project does not hold.
func (s site) contractDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	shown, err := s.contractsOf(r.Context(), p, nil)
	if err != nil {
		failed(w)
		return
	}
	contractID, _ := uuid.Parse(r.PathValue("id"))
	contract, found := shown.contractOf(contractID)
	if !found || !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if r.URL.Query().Get("asking") == "edit" && p.mayKeepContracts() {
		s.contractFormOf(w, r, p, contract, contractTypedIn(r.URL.Query(), contract), "")
		return
	}
	draw(w, r, http.StatusOK, contractPanel(p, contract, ""))
}

// newContractForm is New contract in the details panel, for whoever keeps the settings (UC-020, redix-api #170), drawn again as the contractor
// search changes.
func (s site) newContractForm(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.mayKeepContracts() {
		http.NotFound(w, r)
		return
	}
	s.contractFormOf(w, r, p, contracts.View{}, contractTypedIn(r.URL.Query(), contracts.View{}), "")
}

// contractChanges are what the details panel does to a contract the path names, each the rule the app's route runs.
var contractChanges = map[string]func(c changes.Change, contractID uuid.UUID, typed contractTyped, terms contracts.Terms) error{
	"edit": func(c changes.Change, contractID uuid.UUID, typed contractTyped, terms contracts.Terms) error {
		return contracts.Amend(c, contractID, typed.contractor, typed.item, terms)
	},
	"disable": func(c changes.Change, contractID uuid.UUID, _ contractTyped, _ contracts.Terms) error {
		return contracts.Activate(c, contractID, false)
	},
	"enable": func(c changes.Change, contractID uuid.UUID, _ contractTyped, _ contracts.Terms) error {
		return contracts.Activate(c, contractID, true)
	},
}

// keepContract keeps the form posted, a new contract through contracts.Add or the contract the path names through contracts.Amend, or disables
// or enables that contract through contracts.Activate - the rules the app's routes run -, in one change: the panel shows the contract and
// Contracts beside it follows; refused, the panel says why in the app's words. 403 without ManageSettings; a finished project changes nothing
// (AC-04.1).
func (s site) keepContract(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	act, known := contractChanges[action]
	if !p.inProject() || parsedForm(r) != nil || (action != "" && !known) {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.contractsOf(r.Context(), p, nil)
	if err != nil {
		failed(w)
		return
	}
	contract := contracts.View{}
	if action != "" {
		contractID, _ := uuid.Parse(r.PathValue("id"))
		var found bool
		if contract, found = shown.contractOf(contractID); !found {
			http.NotFound(w, r)
			return
		}
	}
	typed, terms, refusal := contractTypedIn(r.PostForm, contract), contracts.Terms{}, ""
	asked := action == "" || action == "edit"
	if typed.documents, err = s.carriedDocuments(r, p); err != nil {
		failed(w)
		return
	}
	if asked {
		terms, refusal = typed.terms()
	}
	if asked && refusal == "" && !p.finished() {
		if typed.documents, refusal, err = s.keepContractDocuments(r, p, typed.documents); err != nil {
			failed(w)
			return
		}
		terms, _ = typed.terms()
	}
	if p.finished() {
		refusal = "projects.projectFinished"
	}
	if refusal == "" {
		contractID := contract.ID
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			if known {
				return act(c, contractID, typed, terms)
			}
			added, err := contracts.Add(c, typed.contractor, typed.item, terms)
			contractID = added
			return err
		})
		var refused changes.Refusal
		switch {
		case errors.As(err, &refused):
			refusal = "contracts." + refused.Code
		case err != nil:
			failed(w)
			return
		}
		contract.ID = contractID
	}
	if refusal != "" && asked {
		s.contractFormOf(w, r, p, contract, typed, refusal)
		return
	}
	if shown, err = s.contractsOf(r.Context(), p, nil); err != nil {
		failed(w)
		return
	}
	contract, _ = shown.contractOf(contract.ID)
	draw(w, r, http.StatusOK, contractPanel(p, contract, refusal), contractsBeside(p, shown))
}

// carriedDocuments are the documents a refused contract form carries, kept already and still to be added, as the app's dialog holds those it
// added; the error is the database's.
func (s site) carriedDocuments(r *http.Request, p page) ([]store.Attachment, error) {
	var carried []store.Attachment
	for _, id := range r.PostForm["document"] {
		attachmentID, _ := uuid.Parse(id)
		document, err := changes.Found(s.Database.Queries().AttachmentOf(r.Context(), store.AttachmentOfParams{ID: attachmentID, ProjectID: p.project.ID}))
		switch {
		case err == nil:
			carried = append(carried, document)
		case !errors.Is(err, changes.NotFound):
			return nil, err
		}
	}
	return carried, nil
}

// keepContractDocuments keeps each file the form sent as a document, for whoever may attach files, through the rule the app's POST
// /projects/{id}/attachments/document runs (attachments.Keep), in a change of their own before the contract's, as the app keeps each once
// chosen: the documents the contract is to add - those carried, then those kept - and why a file was refused (fileTooLarge); the error is the
// database's.
func (s site) keepContractDocuments(r *http.Request, p page, carried []store.Attachment) ([]store.Attachment, string, error) {
	files, err := attachments.FormFiles(r)
	if err != nil || !p.may(access.AttachFiles) {
		return carried, "", nil
	}
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		for _, file := range files {
			if len(file.Content) == 0 {
				continue
			}
			kept, _, err := attachments.Keep(c, s.AttachmentsFolder, file.Content, file.Name, true)
			if err != nil {
				return err
			}
			carried = append(carried, kept)
		}
		return nil
	})
	refusal, err := refusalOf("attachments", err)
	return carried, refusal, err
}
