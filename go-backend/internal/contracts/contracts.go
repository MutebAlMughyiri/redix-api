// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package contracts keeps what each contractor was promised in a project, for an item (UC-020, FR-07), to follow what is paid and what remains
// (BR-04). There is no delete, and every change is in History, because the remaining of a contract is money (ContractsEndpoints, redix-api #140).
package contracts

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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// notesMaxLength is the longest notes a contract keeps (Contract.NotesMaxLength).
const notesMaxLength = 1000

// The statuses of an entry the money of a contract counts (EntryStatus).
const (
	pendingApproval int64 = 1
	approved        int64 = 2
)

// Money is what the ledger holds inside a contract (ContractMoney, BR-04): what was paid, which counts only once approved (BR-10), and what waits
// for approval beside it.
type Money struct {
	Paid, Pending fixed.Total
}

// MoneyOf is what the ledger holds inside each contract of the project, leaving out the entry leftOut, the one being changed; a contract with
// nothing is not among them, and holds nothing.
func MoneyOf(ctx context.Context, q *store.Queries, projectID, leftOut uuid.UUID) (map[uuid.UUID]Money, error) {
	rows, err := q.ContractMoney(ctx, store.ContractMoneyParams{ProjectID: projectID, LeftOut: leftOut})
	money := map[uuid.UUID]Money{}
	for _, row := range rows {
		held := money[row.ContractID]
		if row.Status == approved {
			held.Paid = held.Paid.Plus(fixed.Counted(fixed.Number(row.Total)))
		} else {
			held.Pending = held.Pending.Plus(fixed.Counted(fixed.Number(row.Total)))
		}
		money[row.ContractID] = held
	}
	return money, err
}

// Remaining is the contract's value less what was paid, below zero once overrun; nothing for an open-value contract, which has no remaining
// (Contract.RemainingAfter, BR-04).
func Remaining(contract store.Contract, paid fixed.Total) fixed.Total {
	if contract.IsOpenValue {
		return fixed.Total{}
	}
	return fixed.Counted(contract.Value).Minus(paid)
}

// Overrun says whether paid goes beyond the contract's value, which shows an alert; never for an open-value contract (Contract.IsOverrunBy).
func Overrun(contract store.Contract, paid fixed.Total) bool {
	return !contract.IsOpenValue && paid.Number() > contract.Value
}

// Routes keeps a project's contracts.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the contracts: reading them for whoever reads the project, changing them for whoever holds ManageSettings, in a project not finished
// (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const contracts = "/projects/{projectId:guid}/contracts"
	keeps := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", contracts, gate.Needs(access.ReadProject, s.list))
	router.Handle("POST", contracts, keeps(s.add))
	router.Handle("PUT", contracts+"/{contractId:guid}", keeps(s.amend))
	router.Handle("POST", contracts+"/{contractId:guid}/disable", keeps(s.activate(false)))
	router.Handle("POST", contracts+"/{contractId:guid}/enable", keeps(s.activate(true)))
}

// documentView is a document as the app shows it (AttachmentView).
type documentView struct {
	ID          uuid.UUID `json:"id"`
	FileName    string    `json:"fileName"`
	ContentType string    `json:"contentType"`
	SizeBytes   int64     `json:"sizeBytes"`
}

// View is a contract as the app shows it (ContractView).
// View is a contract as its list and the contracts report show it, with its value, paid and remaining (ContractView, BR-04).
type View struct {
	ID           uuid.UUID      `json:"id"`
	ItemID       uuid.UUID      `json:"itemId"`
	SupplierName string         `json:"supplierName"`
	ItemName     string         `json:"itemName"`
	Value        fixed.Number   `json:"value"`
	IsOpenValue  bool           `json:"isOpenValue"`
	UnitPrice    fixed.Number   `json:"unitPrice"`
	Quantity     fixed.Number   `json:"quantity"`
	Notes        string         `json:"notes"`
	Paid         fixed.Total    `json:"paid"`
	Pending      fixed.Total    `json:"pending"`
	Remaining    fixed.Total    `json:"remaining"`
	IsOverrun    bool           `json:"isOverrun"`
	IsActive     bool           `json:"isActive"`
	SupplierID   uuid.UUID      `json:"supplierId"`
	HasPayments  bool           `json:"hasPayments"`
	Documents    []documentView `json:"documents"`
}

// List is every contract of the project, disabled ones included, in the order they were added, each with its value, paid and remaining (BR-04,
// ContractsEndpoints.List): the one owner of the contracts screen and the contracts report.
func List(ctx context.Context, q *store.Queries, projectID uuid.UUID) ([]View, error) {
	rows, err := q.ContractsOf(ctx, projectID)
	documents, documentsErr := q.ContractDocuments(ctx, projectID)
	money, moneyErr := MoneyOf(ctx, q, projectID, uuid.Nil)
	withPayments, paymentsErr := q.ContractsWithPayments(ctx, projectID)
	if err := errors.Join(err, documentsErr, moneyErr, paymentsErr); err != nil {
		return nil, err
	}
	views := []View{}
	for _, row := range rows {
		contract := store.Contract{ID: row.ID, ProjectID: row.ProjectID, ItemID: row.ItemID, SortOrder: row.SortOrder, SupplierID: row.SupplierID, Value: row.Value,
			IsOpenValue: row.IsOpenValue, UnitPrice: row.UnitPrice, Quantity: row.Quantity, Notes: row.Notes, IsActive: row.IsActive}
		held := money[row.ID]
		view := View{ID: row.ID, ItemID: row.ItemID, SupplierName: row.SupplierName, ItemName: row.ItemName, Value: row.Value, IsOpenValue: row.IsOpenValue,
			UnitPrice: row.UnitPrice, Quantity: row.Quantity, Notes: row.Notes, Paid: held.Paid, Pending: held.Pending, Remaining: Remaining(contract, held.Paid),
			IsOverrun: Overrun(contract, held.Paid), IsActive: row.IsActive, SupplierID: row.SupplierID, HasPayments: slices.Contains(withPayments, row.ID),
			Documents: []documentView{}}
		for _, document := range documents {
			if document.ContractID == row.ID {
				view.Documents = append(view.Documents, documentView{ID: document.ID, FileName: document.FileName, ContentType: document.ContentType, SizeBytes: document.SizeBytes})
			}
		}
		views = append(views, view)
	}
	return views, nil
}

func (s Routes) list(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	views, err := List(r.Context(), s.database.Queries(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// Terms is what can change on a contract, which is what the app sends to amend it (ContractTerms).
type Terms struct {
	Value         fixed.Number `json:"value"`
	IsOpenValue   bool         `json:"isOpenValue"`
	UnitPrice     fixed.Number `json:"unitPrice,omitzero"`
	Quantity      fixed.Number `json:"quantity,omitzero"`
	Notes         string       `json:"notes,omitzero"`
	AttachmentIDs []uuid.UUID  `json:"attachmentIds,omitzero"`
}

// refusal is why a contract cannot have these terms (Contract.RefusalOf): invalidValue for a value not above zero unless the contract is of open
// value, invalidUnitPrice or invalidQuantity below zero, any amount with more than two decimals, invalidNotes past 1,000 characters; "" when it can.
func (t Terms) refusal() string {
	switch {
	case !t.IsOpenValue && (t.Value <= 0 || !t.Value.IsAcceptableMoney()):
		return "invalidValue"
	case t.UnitPrice < 0 || !t.UnitPrice.IsAcceptableMoney():
		return "invalidUnitPrice"
	case t.Quantity < 0 || !t.Quantity.IsAcceptableMoney():
		return "invalidQuantity"
	case utf8.RuneCountInString(strings.TrimSpace(t.Notes)) > notesMaxLength:
		return "invalidNotes"
	}
	return ""
}

// into is contract with these terms (Contract.SetTerms): an open-value contract keeps no value.
func (t Terms) into(contract store.Contract) store.Contract {
	contract.Value, contract.IsOpenValue, contract.UnitPrice, contract.Quantity, contract.Notes = t.Value, t.IsOpenValue, t.UnitPrice, t.Quantity, strings.TrimSpace(t.Notes)
	if t.IsOpenValue {
		contract.Value = 0
	}
	return contract
}

// refusalOfParties is why supplierID and itemID cannot be a contract's: unknownSupplier for a contractor who is not an active supplier of the
// project, itemNotOffered for an item that takes no contractor payments.
func refusalOfParties(c changes.Change, supplierID, itemID uuid.UUID) error {
	active, err := c.Q.ActiveSupplierExists(c.Ctx, store.ActiveSupplierExistsParams{ID: supplierID, ProjectID: c.ProjectID})
	if err != nil || !active {
		return orRefused(err, "unknownSupplier")
	}
	offered, err := structure.Offers(c.Ctx, c.Q, c.ProjectID, itemID, structure.ContractorPayments)
	if err != nil || !offered {
		return orRefused(err, "itemNotOffered")
	}
	return nil
}

// refusalOfDocuments is unknownAttachment for a document that is no file of the project.
func refusalOfDocuments(c changes.Change, attachmentIDs []uuid.UUID) error {
	for _, id := range attachmentIDs {
		kept, err := c.Q.AttachmentExists(c.Ctx, store.AttachmentExistsParams{ID: id, ProjectID: c.ProjectID})
		if err != nil || !kept {
			return orRefused(err, "unknownAttachment")
		}
	}
	return nil
}

// orRefused is err when there is one, and the refusal code (400) otherwise.
func orRefused(err error, code string) error {
	if err != nil {
		return err
	}
	return changes.Refuse(http.StatusBadRequest, code)
}

func fields(row store.Contract) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "ItemId": row.ItemID, "SortOrder": row.SortOrder, "SupplierId": row.SupplierID, "Value": row.Value,
		"IsOpenValue": row.IsOpenValue, "UnitPrice": row.UnitPrice, "Quantity": row.Quantity, "Notes": row.Notes, "IsActive": row.IsActive}
}

// keep writes the contract and its line of action, which .NET's AddHistory wrote whether or not anything changed, then attaches what it does not
// carry yet of attachmentIDs, each with its line.
func keep(c changes.Change, action history.Action, before, after store.Contract, attachmentIDs []uuid.UUID) error {
	if err := c.Q.SaveContract(c.Ctx, store.SaveContractParams(after)); err != nil {
		return err
	}
	was, is := history.Changed(fields(before), fields(after))
	if action == history.Create {
		was, is = nil, fields(after)
	}
	if err := c.Record(action, "Contracts", after.ID, was, is); err != nil {
		return err
	}
	held, err := c.Q.DocumentsOfContract(c.Ctx, after.ID)
	if err != nil {
		return err
	}
	for _, id := range attachmentIDs {
		if slices.Contains(held, id) {
			continue
		}
		held = append(held, id)
		if err := c.Q.AddContractDocument(c.Ctx, store.AddContractDocumentParams{ContractID: after.ID, AttachmentID: id}); err != nil {
			return err
		}
		if err := c.Record(history.Attach, "ContractAttachments", after.ID, nil, map[string]any{"AttachmentId": id}); err != nil {
			return err
		}
	}
	return nil
}

// addContractRequest is the body POST /projects/{projectId}/contracts (UC-020) reads.
type addContractRequest struct {
	SupplierID uuid.UUID `json:"supplierId"`
	ItemID     uuid.UUID `json:"itemId"`
	Terms
}

// add adds a contract last among its item's tables, refused as refusalOfParties, Terms.refusal and refusalOfDocuments say (400).
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addContractRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		_, err := Add(c, request.SupplierID, request.ItemID, request.Terms)
		return err
	})
}

// Add adds the contract of the supplier for the item on terms, last among the item's tables (ContractsEndpoints.AddAsync), refused as
// refusalOfParties, Terms.refusal and refusalOfDocuments say (400): the one way a contract is added, by hand or by a project file's load.
func Add(c changes.Change, supplierID, itemID uuid.UUID, terms Terms) (uuid.UUID, error) {
	if err := refusalOfParties(c, supplierID, itemID); err != nil {
		return uuid.Nil, err
	}
	if refused := terms.refusal(); refused != "" {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, refused)
	}
	if err := refusalOfDocuments(c, terms.AttachmentIDs); err != nil {
		return uuid.Nil, err
	}
	place, err := structure.NextTablePlace(c.Ctx, c.Q, itemID)
	if err != nil {
		return uuid.Nil, err
	}
	contract := terms.into(store.Contract{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, ItemID: itemID, SortOrder: place, SupplierID: supplierID,
		IsActive: true})
	return contract.ID, keep(c, history.Create, store.Contract{}, contract, terms.AttachmentIDs)
}

// amendContractRequest is the body PUT /projects/{projectId}/contracts/{contractId} reads (UC-020).
type amendContractRequest struct {
	Terms
	SupplierID uuid.UUID `json:"supplierId,omitzero"`
	ItemID     uuid.UUID `json:"itemId,omitzero"`
}

// amend amends a contract's Terms and adds to its documents, and changes its contractor or item while no payment names it, or answers
// contractHasPayments (409); refused as add is.
func (s Routes) amend(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request amendContractRequest
	if !web.Read(w, r, &request) {
		return
	}
	if refused := request.refusal(); refused != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refused)
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		if err := refusalOfDocuments(c, request.AttachmentIDs); err != nil {
			return err
		}
		before, err := changes.Found(c.Q.ContractOf(c.Ctx, store.ContractOfParams{ID: web.ID(r, "contractId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		after := request.into(before)
		if request.SupplierID != uuid.Nil {
			after.SupplierID = request.SupplierID
		}
		if request.ItemID != uuid.Nil {
			after.ItemID = request.ItemID
		}
		if after.SupplierID != before.SupplierID || after.ItemID != before.ItemID {
			withPayments, err := c.Q.ContractsWithPayments(c.Ctx, c.ProjectID)
			if err != nil {
				return err
			}
			if slices.Contains(withPayments, before.ID) {
				return changes.Refuse(http.StatusConflict, "contractHasPayments")
			}
			if err := refusalOfParties(c, after.SupplierID, after.ItemID); err != nil {
				return err
			}
		}
		if after.ItemID != before.ItemID {
			if after.SortOrder, err = structure.NextTablePlace(c.Ctx, c.Q, after.ItemID); err != nil {
				return err
			}
		}
		return keep(c, history.Update, before, after, request.AttachmentIDs)
	})
}

// activate disables a contract, so no new payment names it and it stays in the reports, or brings it back.
func (s Routes) activate(active bool) access.Handler {
	action := history.Disable
	if active {
		action = history.Enable
	}
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.writer.Done(w, r, caller, func(c changes.Change) error {
			before, err := changes.Found(c.Q.ContractOf(c.Ctx, store.ContractOfParams{ID: web.ID(r, "contractId"), ProjectID: c.ProjectID}))
			if err != nil {
				return err
			}
			after := before
			after.IsActive = active
			return keep(c, action, before, after, nil)
		})
	}
}
