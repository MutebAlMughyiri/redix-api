// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/spelling"
	"github.com/google/uuid"
)

// facts is what a list of a project's entries shows of each, read at once (EntryFacts): the one owner of joining an entry to its names, its
// invoice, its file and what its account holds.
type facts struct {
	expenses       map[uuid.UUID]store.ExpenseDetail
	purchases      map[uuid.UUID]store.PurchaseDetail
	payments       map[uuid.UUID]store.ContractorPaymentDetail
	revenues       map[uuid.UUID]store.RevenueDetail
	capital        map[uuid.UUID]uuid.UUID
	invoices       map[uuid.UUID]string
	files          map[uuid.UUID][]store.EntryFilesOfRow
	partners       map[uuid.UUID]string
	items          map[uuid.UUID]string
	accounts       map[uuid.UUID]string
	paymentMethods map[uuid.UUID]string
	suppliers      map[uuid.UUID]store.Supplier
	contractors    map[uuid.UUID]uuid.UUID
	authors        map[uuid.UUID]string
	balances       map[uuid.UUID]fixed.Total
	waitingOut     map[uuid.UUID]fixed.Total
	// tables is each entry's table, its contract or its group, from the view entry_tables (#106 G1); a capital movement has none.
	tables map[uuid.UUID]uuid.UUID
}

// factsOf reads the facts of entries with what every entry's facts share (#87).
func factsOf(c changes.Change, entries []store.Entry) (facts, error) {
	f, err := sharedFactsOf(c)
	if err != nil {
		return f, err
	}
	return f, f.readEntries(c, entries)
}

// sharedFactsOf reads what every entry's facts share: the project's names, its accounts' balances, and the money waiting to leave each account,
// whose entries' own facts it reads too. A list reads it once, then each batch's own (#87).
func sharedFactsOf(c changes.Change) (facts, error) {
	f, err := namesOf(c)
	if err != nil {
		return f, err
	}
	waiting, err := c.Q.PendingEntriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return f, err
	}
	if err := f.readEntries(c, waiting); err != nil {
		return f, err
	}
	if f.balances, err = accounts.Balances(c.Ctx, c.Q, c.ProjectID, c.Access.Unseen(c.ProjectID)); err != nil {
		return f, err
	}
	for _, entry := range waiting {
		if moved := signed(entry.Kind, fixed.Counted(entry.Amount)); moved.Number() < 0 && f.tableSeen(c.Access, c.ProjectID, entry.ID) {
			f.waitingOut[entry.AccountID] = f.waitingOut[entry.AccountID].Minus(moved)
		}
	}
	return f, nil
}

// namesOf reads the names an entry's facts give - the project's partners, items, accounts, payment methods, suppliers and contracts' contractors,
// and every person's - with no money and no entry's own facts yet: what a history screen reads once, then the entries of each batch (#87).
func namesOf(c changes.Change) (facts, error) {
	q, ctx, projectID := c.Q, c.Ctx, c.ProjectID
	f := facts{expenses: map[uuid.UUID]store.ExpenseDetail{}, purchases: map[uuid.UUID]store.PurchaseDetail{}, payments: map[uuid.UUID]store.ContractorPaymentDetail{},
		revenues: map[uuid.UUID]store.RevenueDetail{}, capital: map[uuid.UUID]uuid.UUID{}, invoices: map[uuid.UUID]string{}, files: map[uuid.UUID][]store.EntryFilesOfRow{},
		partners: map[uuid.UUID]string{}, items: map[uuid.UUID]string{}, accounts: map[uuid.UUID]string{}, paymentMethods: map[uuid.UUID]string{},
		suppliers: map[uuid.UUID]store.Supplier{}, contractors: map[uuid.UUID]uuid.UUID{}, authors: map[uuid.UUID]string{}, waitingOut: map[uuid.UUID]fixed.Total{},
		tables: map[uuid.UUID]uuid.UUID{}}
	partners, err := q.PartnersOf(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range partners {
		f.partners[row.ID] = row.Name
	}
	items, err := q.ItemsOf(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range items {
		f.items[row.ID] = row.Name
	}
	accountRows, err := q.AccountsOf(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range accountRows {
		f.accounts[row.ID] = row.Name
	}
	methods, err := q.PaymentMethodsOf(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range methods {
		f.paymentMethods[row.ID] = row.Name
	}
	suppliers, err := q.ProjectSuppliers(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range suppliers {
		f.suppliers[row.ID] = row
	}
	contracts, err := q.ContractsOf(ctx, projectID)
	if err != nil {
		return f, err
	}
	for _, row := range contracts {
		f.contractors[row.ID] = row.SupplierID
	}
	names, err := q.UserNames(ctx)
	if err != nil {
		return f, err
	}
	for _, row := range names {
		f.authors[row.ID] = row.DisplayName
	}
	return f, nil
}

// readEntries adds to f the own facts of entries - their tables, details, invoices and files - named in one JSON list (#87). An entry read again
// is read whole again.
func (f facts) readEntries(c changes.Change, entries []store.Entry) error {
	q, ctx := c.Q, c.Ctx
	named := entriesNamed(entries)
	for _, entry := range entries {
		delete(f.files, entry.ID)
	}
	tables, err := q.EntryTablesOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range tables {
		f.tables[row.EntryID] = row.TableID
	}
	expenses, err := q.ExpenseDetailsOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range expenses {
		f.expenses[row.EntryID] = row
	}
	purchases, err := q.PurchaseDetailsOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range purchases {
		f.purchases[row.EntryID] = row
	}
	payments, err := q.PaymentDetailsOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range payments {
		f.payments[row.EntryID] = row
	}
	revenues, err := q.RevenueDetailsOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range revenues {
		f.revenues[row.EntryID] = row
	}
	capital, err := q.CapitalDetailsOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range capital {
		f.capital[row.EntryID] = row.PartnerID
	}
	invoices, err := q.InvoicesOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range invoices {
		f.invoices[row.EntryID] = row.Number
	}
	files, err := q.EntryFilesOf(ctx, named)
	if err != nil {
		return err
	}
	for _, row := range files {
		f.files[row.EntryID] = append(f.files[row.EntryID], row)
	}
	return nil
}

// entriesNamed is the ids of entries as the JSON list the per-entry queries read (#87).
func entriesNamed(entries []store.Entry) string {
	ids := make([]uuid.UUID, 0, len(entries))
	for _, entry := range entries {
		ids = append(ids, entry.ID)
	}
	return db.IDList(ids)
}

func (f facts) itemNameOf(entry store.Entry) string {
	if row, isRevenue := f.revenues[entry.ID]; isRevenue {
		return f.items[row.ItemID]
	}
	return f.items[f.expenses[entry.ID].ItemID]
}

// supplierOf is the entry's supplier, or its contract's contractor; an empty one when it has none.
func (f facts) supplierOf(entry store.Entry) store.Supplier {
	if row, bought := f.purchases[entry.ID]; bought {
		return f.suppliers[row.SupplierID]
	}
	if row, paid := f.payments[entry.ID]; paid {
		return f.suppliers[f.contractors[row.ContractID]]
	}
	return store.Supplier{}
}

// tableOf is the table the entry is in: its contract, its partner, its revenue group, or its expense group.
func (f facts) tableOf(entry store.Entry) uuid.UUID {
	for _, id := range []uuid.UUID{f.payments[entry.ID].ContractID, f.capital[entry.ID], f.revenues[entry.ID].SubgroupID, f.expenses[entry.ID].SubgroupID} {
		if id != uuid.Nil {
			return id
		}
	}
	return uuid.Nil
}

// accountWaitingOf is what waits to leave the entry's account: every entry taking money out of it that waits for approval, and the entry itself
// while it is a draft (the owner, 10 Nov 2026).
func (f facts) accountWaitingOf(entry store.Entry) fixed.Total {
	waiting := f.waitingOut[entry.AccountID]
	if entry.Status == draft && signed(entry.Kind, fixed.Counted(entry.Amount)).Number() < 0 {
		return waiting.Plus(fixed.Counted(entry.Amount))
	}
	return waiting.Plus(fixed.Total{})
}

// accountShortByOf is how much more what waits to leave the entry's account takes than it holds, for an entry taking money out not yet decided;
// nothing for any other (R5, R6, D4, D6).
func (f facts) accountShortByOf(entry store.Entry) fixed.Total {
	takesOut := signed(entry.Kind, fixed.Counted(entry.Amount)).Number() < 0
	if !takesOut || (entry.Status != draft && entry.Status != pendingApproval) {
		return fixed.Total{}
	}
	short := f.accountWaitingOf(entry).Minus(f.balances[entry.AccountID])
	if short.Number() > 0 {
		return short
	}
	return fixed.Total{}
}

func (f facts) firstFile(entry store.Entry) store.EntryFilesOfRow {
	if files := f.files[entry.ID]; len(files) > 0 {
		return files[0]
	}
	return store.EntryFilesOfRow{}
}

// rowView is an entry on a list (EntryRowView).
type rowView struct {
	ID                uuid.UUID    `json:"id"`
	Number            int64        `json:"number"`
	Kind              named        `json:"kind"`
	Status            named        `json:"status"`
	RejectionReason   string       `json:"rejectionReason"`
	EntryDate         moment.Date  `json:"entryDate"`
	Amount            fixed.Number `json:"amount"`
	ItemName          string       `json:"itemName"`
	SupplierName      string       `json:"supplierName"`
	SupplierVatNumber string       `json:"supplierVatNumber"`
	InvoiceNumber     string       `json:"invoiceNumber"`
	AuthorName        string       `json:"authorName"`
	IsMine            bool         `json:"isMine"`
	PartnerName       string       `json:"partnerName"`
	Description       string       `json:"description"`
	AccountName       string       `json:"accountName"`
	PaymentMethodName string       `json:"paymentMethodName"`
	AccountShortBy    fixed.Total  `json:"accountShortBy"`
}

func (f facts) row(entry store.Entry, userID uuid.UUID) rowView {
	supplier := f.supplierOf(entry)
	return rowView{ID: entry.ID, Number: entry.Number, Kind: kindOf(entry.Kind), Status: statusOf(entry.Status), EntryDate: entry.EntryDate, Amount: entry.Amount,
		ItemName: f.itemNameOf(entry), SupplierName: supplier.Name, SupplierVatNumber: supplier.VatNumber, InvoiceNumber: f.invoices[entry.ID],
		AuthorName: f.authors[entry.CreatedByUserID], IsMine: entry.CreatedByUserID == userID, PartnerName: f.partners[f.capital[entry.ID]],
		Description: entry.Description, AccountName: f.accounts[entry.AccountID], PaymentMethodName: f.paymentMethods[f.expenses[entry.ID].PaymentMethodID],
		AccountShortBy: f.accountShortByOf(entry)}
}

// pendingView is an entry in Finance's inbox (PendingEntryView).
type pendingView struct {
	ID             uuid.UUID    `json:"id"`
	Number         int64        `json:"number"`
	Kind           named        `json:"kind"`
	EntryDate      moment.Date  `json:"entryDate"`
	Amount         fixed.Number `json:"amount"`
	ItemName       string       `json:"itemName"`
	SupplierName   string       `json:"supplierName"`
	InvoiceNumber  string       `json:"invoiceNumber"`
	AccountName    string       `json:"accountName"`
	Description    string       `json:"description"`
	AttachmentID   string       `json:"attachmentId"`
	ContentType    string       `json:"contentType"`
	FileName       string       `json:"fileName"`
	AuthorName     string       `json:"authorName"`
	PartnerName    string       `json:"partnerName"`
	AccountBalance fixed.Total  `json:"accountBalance"`
	AccountWaiting fixed.Total  `json:"accountWaiting"`
	AccountShortBy fixed.Total  `json:"accountShortBy"`
}

func (f facts) pending(entry store.Entry) pendingView {
	file := f.firstFile(entry)
	view := pendingView{ID: entry.ID, Number: entry.Number, Kind: kindOf(entry.Kind), EntryDate: entry.EntryDate, Amount: entry.Amount, ItemName: f.itemNameOf(entry),
		SupplierName: f.supplierOf(entry).Name, InvoiceNumber: f.invoices[entry.ID], AccountName: f.accounts[entry.AccountID], Description: entry.Description,
		ContentType: file.ContentType, FileName: file.FileName, AuthorName: f.authors[entry.CreatedByUserID], PartnerName: f.partners[f.capital[entry.ID]],
		AccountBalance: f.balances[entry.AccountID], AccountWaiting: f.accountWaitingOf(entry), AccountShortBy: f.accountShortByOf(entry)}
	if file.AttachmentID != uuid.Nil {
		view.AttachmentID = file.AttachmentID.String()
	}
	return view
}

// entryView is an entry and everything its kind has (EntryView): what it does not have is empty, with an empty entryId.
type entryView struct {
	Entry struct {
		ID              uuid.UUID     `json:"id"`
		ProjectID       uuid.UUID     `json:"projectId"`
		Number          int64         `json:"number"`
		Kind            named         `json:"kind"`
		Status          named         `json:"status"`
		EntryDate       moment.Date   `json:"entryDate"`
		Amount          fixed.Number  `json:"amount"`
		AccountID       uuid.UUID     `json:"accountId"`
		Description     string        `json:"description"`
		ColumnValues    string        `json:"columnValues"`
		CreatedByUserID uuid.UUID     `json:"createdByUserId"`
		CreatedAt       moment.Moment `json:"createdAt"`
		ApprovedAt      moment.Moment `json:"approvedAt"`
	} `json:"entry"`
	TableID uuid.UUID `json:"tableId"`
	Expense struct {
		EntryID         uuid.UUID `json:"entryId"`
		ItemID          uuid.UUID `json:"itemId"`
		SubgroupID      uuid.UUID `json:"subgroupId"`
		PaymentMethodID uuid.UUID `json:"paymentMethodId"`
	} `json:"expense"`
	Purchase struct {
		EntryID    uuid.UUID `json:"entryId"`
		SupplierID uuid.UUID `json:"supplierId"`
	} `json:"purchase"`
	ContractorPayment struct {
		EntryID           uuid.UUID `json:"entryId"`
		ContractID        uuid.UUID `json:"contractId"`
		IsOutsideContract bool      `json:"isOutsideContract"`
	} `json:"contractorPayment"`
	Invoice struct {
		EntryID uuid.UUID `json:"entryId"`
		Number  string    `json:"number"`
	} `json:"invoice"`
	Attachments []fileView `json:"attachments"`
}

// fileView is a file an entry carries (EntryAttachmentView, redix-app #115).
type fileView struct {
	AttachmentID uuid.UUID     `json:"attachmentId"`
	Role         named         `json:"role"`
	FileName     string        `json:"fileName"`
	ContentType  string        `json:"contentType"`
	UploadedAt   moment.Moment `json:"uploadedAt"`
}

func (f facts) view(entry store.Entry) entryView {
	var view entryView
	view.Entry.ID, view.Entry.ProjectID, view.Entry.Number, view.Entry.Kind, view.Entry.Status = entry.ID, entry.ProjectID, entry.Number, kindOf(entry.Kind), statusOf(entry.Status)
	view.Entry.EntryDate, view.Entry.Amount, view.Entry.AccountID, view.Entry.Description = entry.EntryDate, entry.Amount, entry.AccountID, entry.Description
	view.Entry.ColumnValues, view.Entry.CreatedByUserID, view.Entry.CreatedAt, view.Entry.ApprovedAt = entry.ColumnValues, entry.CreatedByUserID, entry.CreatedAt, entry.ApprovedAt
	view.TableID = f.tableOf(entry)
	if row, has := f.expenses[entry.ID]; has {
		view.Expense.EntryID, view.Expense.ItemID, view.Expense.SubgroupID, view.Expense.PaymentMethodID = row.EntryID, row.ItemID, row.SubgroupID, row.PaymentMethodID
	}
	if row, has := f.purchases[entry.ID]; has {
		view.Purchase.EntryID, view.Purchase.SupplierID = row.EntryID, row.SupplierID
	}
	if row, has := f.payments[entry.ID]; has {
		view.ContractorPayment.EntryID, view.ContractorPayment.ContractID, view.ContractorPayment.IsOutsideContract = row.EntryID, row.ContractID, row.IsOutsideContract
	}
	if number, has := f.invoices[entry.ID]; has {
		view.Invoice.EntryID, view.Invoice.Number = entry.ID, number
	}
	view.Attachments = []fileView{}
	for _, file := range f.files[entry.ID] {
		view.Attachments = append(view.Attachments, fileView{AttachmentID: file.AttachmentID, Role: named{file.Role, roles}, FileName: file.FileName,
			ContentType: file.ContentType, UploadedAt: file.UploadedAt})
	}
	return view
}

// search is what narrows the list of entries (EntrySearch, redix-app #104): text, the caller's own, kinds, statuses, a day range and a number.
type search struct {
	text           string
	mine           bool
	kinds          []string
	statuses       []string
	from, to       time.Time
	number         int64
	narrowsNothing bool
}

func searchOf(kinds, statuses []string, text, mine, from, to, number string) search {
	s := search{text: strings.TrimSpace(text), mine: strings.EqualFold(mine, "true"), kinds: kinds, statuses: statuses}
	s.from, _ = time.Parse("2006-01-02", from)
	s.to, _ = time.Parse("2006-01-02", to)
	s.number, _ = strconv.ParseInt(number, 10, 64)
	s.narrowsNothing = s.text == "" && !s.mine && len(kinds) == 0 && len(statuses) == 0 && s.from.IsZero() && s.to.IsZero() && s.number == 0
	return s
}

func (s search) keeps(row rowView) bool {
	if s.narrowsNothing {
		return true
	}
	kind, _ := row.Kind.MarshalJSON()
	status, _ := row.Status.MarshalJSON()
	return (!s.mine || row.IsMine) && (s.number == 0 || row.Number == s.number) && (len(s.kinds) == 0 || slices.Contains(s.kinds, strings.Trim(string(kind), `"`))) &&
		(len(s.statuses) == 0 || slices.Contains(s.statuses, strings.Trim(string(status), `"`))) && !row.EntryDate.Before(s.from) &&
		(s.to.IsZero() || !row.EntryDate.After(s.to)) &&
		spelling.Holds(s.text, row.Amount, row.ItemName, row.PartnerName, row.SupplierName, row.AuthorName, row.InvoiceNumber)
}

// tableSeen says whether held sees entryID's table in projectID: always for an entry with none, a capital movement, whose partner the capital ticks
// decide (#106 R14).
func (f facts) tableSeen(held access.Access, projectID, entryID uuid.UUID) bool {
	table, has := f.tables[entryID]
	return !has || held.Sees(projectID, table)
}
