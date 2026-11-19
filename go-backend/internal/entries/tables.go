// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"database/sql"
	"errors"
	"net/http"
	"slices"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// What a table of an item's sheet prints (ItemTableKind).
const (
	generalTable int64 = iota
	subgroupTable
	contractTable
	partnerTable
)

var tableKinds = web.Enum{"general", "subgroup", "contract", "partner"}

// The columns only a purchase's template holds, and those only a contractor payment's does (StructureSeed.IsAsked, FR-38).
var (
	onlyPurchases = []string{"supplier", "unitPrice", "quantity", "entryType", "total"}
	onlyPayments  = []string{"outsideContract", "contract", "amount"}
)

// table is a table an entry goes into (EntryTable, redix-api #110): its category and item, its id - a group's, a contract's or a partner's - its
// name in the picker, what it is in the item's sheet and its place there, the item's columns its entry is asked, and its form, what its kind adds.
type table struct {
	category store.Category
	item     store.Item
	id       uuid.UUID
	name     string
	kind     int64
	place    int64
	columns  []store.ItemColumn
	form     form
}

// form is what one kind of table adds to every entry (PurchaseTable, ContractTable, RevenueTable, PartnerTable).
type form interface {
	entryKind(sign structure.Sign) int64
	asks(key string) bool
	total(r Request) fixed.Total
	amount(r Request, sign structure.Sign) fixed.Total
	refusal(c changes.Change, r Request, held access.Access) (string, error)
	keep(c changes.Change, r Request, entry store.Entry, t table) error
	answer(c changes.Change, r Request, entry store.Entry, amount fixed.Total) (any, error)
}

// expense is what the forms of expenses share.
type expense struct{}

func (expense) total(r Request) fixed.Total { return r.Amount.Total() }

func (expense) amount(r Request, sign structure.Sign) fixed.Total {
	return amountOf(r.Amount.Total(), sign)
}

func (expense) refusal(changes.Change, Request, access.Access) (string, error) { return "", nil }

func (expense) answer(_ changes.Change, _ Request, entry store.Entry, amount fixed.Total) (any, error) {
	return recordedView{ID: entry.ID, Status: statusOf(entry.Status), Amount: amount}, nil
}

// amountOf is total, taken away when the choice of the table's signed list is minus, as a credit note takes away what its bill counted
// (EntryTable.AmountOf, redix-api #119 D4, #129).
func amountOf(total fixed.Total, sign structure.Sign) fixed.Total {
	if sign == structure.Minus {
		return fixed.Total{}.Minus(total)
	}
	return total
}

// recordedView is a recorded entry, as the API kept it (RecordedEntryView).
type recordedView struct {
	ID     uuid.UUID   `json:"id"`
	Status named       `json:"status"`
	Amount fixed.Total `json:"amount"`
}

// purchaseForm is a purchase table (PurchaseTable, UC-022, FR-08): a group of an item that takes purchases.
type purchaseForm struct {
	expense
	subgroup store.Subgroup
}

func (purchaseForm) entryKind(structure.Sign) int64 { return purchase }

func (purchaseForm) asks(key string) bool { return !slices.Contains(onlyPayments, key) }

// total is the amount typed; left out, the unit price times the quantity, when the entry gives both (BR-01) and their product is within what is
// kept - none otherwise, as .NET's guard against a product past Money.Largest left none.
func (purchaseForm) total(r Request) fixed.Total {
	if r.Amount.Number != 0 {
		return r.Amount.Total()
	}
	values := r.kept()
	unitPrice, priceOK := numberIn(values.Values["unitPrice"])
	quantity, quantityOK := numberIn(values.Values["quantity"])
	if !priceOK || !quantityOK || unitPrice.Number <= 0 || quantity.Number <= 0 || unitPrice.IsBeyond() || quantity.IsBeyond() {
		return fixed.Total{}
	}
	if product := fixed.Product(unitPrice, quantity); !product.Number().IsBeyond() {
		return product
	}
	return fixed.Total{}
}

func (f purchaseForm) amount(r Request, sign structure.Sign) fixed.Total {
	return amountOf(f.total(r), sign)
}

// refusal is unknownSupplier for a supplier named that the caller may not use (BR-19); naming none is no refusal (redix-api #129).
func (purchaseForm) refusal(c changes.Change, r Request, held access.Access) (string, error) {
	if r.SupplierID == uuid.Nil {
		return "", nil
	}
	supplier, err := c.Q.SupplierOf(c.Ctx, store.SupplierOfParams{ID: r.SupplierID, ProjectID: c.ProjectID})
	if err != nil && !errors.Is(err, sql.ErrNoRows) {
		return "", err
	}
	offered := err == nil && supplier.IsActive && (supplier.Status == 0 || supplier.CreatedByUserID == c.Actor || held.Holds(c.ProjectID, access.ManageSettings))
	if offered {
		return "", nil
	}
	return "unknownSupplier", nil
}

func (f purchaseForm) keep(c changes.Change, r Request, entry store.Entry, t table) error {
	if err := keepExpense(c, r, entry, t.item.ID, f.subgroup.ID); err != nil {
		return err
	}
	if r.SupplierID == uuid.Nil {
		return nil
	}
	return addDetail(c, "PurchaseDetails", entry.ID, map[string]any{"SupplierId": r.SupplierID},
		func() error {
			return c.Q.AddPurchaseDetails(c.Ctx, store.AddPurchaseDetailsParams{EntryID: entry.ID, SupplierID: r.SupplierID})
		})
}

// keepExpense keeps what every expense holds, in its item's group subgroupID: how it was paid.
func keepExpense(c changes.Change, r Request, entry store.Entry, itemID, subgroupID uuid.UUID) error {
	return addDetail(c, "ExpenseDetails", entry.ID, map[string]any{"ItemId": itemID, "SubgroupId": subgroupID, "PaymentMethodId": r.PaymentMethodID},
		func() error {
			return c.Q.AddExpenseDetails(c.Ctx, store.AddExpenseDetailsParams{EntryID: entry.ID, ItemID: itemID, SubgroupID: subgroupID, PaymentMethodID: r.PaymentMethodID})
		})
}

// addDetail writes a row of the entry entryID by write, with its Create line, whose record is the entry, the row's key.
func addDetail(c changes.Change, table string, entryID uuid.UUID, values map[string]any, write func() error) error {
	if err := write(); err != nil {
		return err
	}
	return c.Record(history.Create, table, entryID, nil, values)
}

// contractForm is a contract's payments (ContractTable, FR-12): its entry goes under the item's general group, inside or outside the contract
// (FR-13), and recording it answers the contract's figures with it.
type contractForm struct {
	expense
	contract store.Contract
	general  uuid.UUID
}

func (contractForm) entryKind(structure.Sign) int64 { return contractorPayment }

func (contractForm) asks(key string) bool {
	return key != "supplier" && !slices.Contains(onlyPurchases, key)
}

func (f contractForm) keep(c changes.Change, r Request, entry store.Entry, t table) error {
	if err := keepExpense(c, r, entry, t.item.ID, f.general); err != nil {
		return err
	}
	return addDetail(c, "ContractorPaymentDetails", entry.ID, map[string]any{"ContractId": f.contract.ID, "IsOutsideContract": r.IsOutsideContract},
		func() error {
			return c.Q.AddPaymentDetails(c.Ctx, store.AddPaymentDetailsParams{EntryID: entry.ID, ContractID: f.contract.ID, IsOutsideContract: r.IsOutsideContract})
		})
}

// paymentView is a recorded contractor payment and what its contract stands at with it (ContractorPaymentView, BR-07).
type paymentView struct {
	ID             uuid.UUID   `json:"id"`
	Status         named       `json:"status"`
	Amount         fixed.Total `json:"amount"`
	Paid           fixed.Total `json:"paid"`
	Pending        fixed.Total `json:"pending"`
	RemainingAfter fixed.Total `json:"remainingAfter"`
	IsOverrun      bool        `json:"isOverrun"`
}

func (f contractForm) answer(c changes.Change, r Request, entry store.Entry, amount fixed.Total) (any, error) {
	money, err := contracts.MoneyOf(c.Ctx, c.Q, c.ProjectID, entry.ID)
	held := money[f.contract.ID]
	counted := held.Paid.Plus(held.Pending)
	if !r.IsOutsideContract {
		counted = counted.Plus(amount)
	}
	return paymentView{ID: entry.ID, Status: statusOf(entry.Status), Amount: amount, Paid: held.Paid, Pending: held.Pending,
		RemainingAfter: contracts.Remaining(f.contract, counted), IsOverrun: contracts.Overrun(f.contract, counted)}, err
}

// revenueForm is a revenue item's group (RevenueTable, FR-38, redix-api #101): money coming in.
type revenueForm struct {
	expense
	subgroup store.Subgroup
}

func (revenueForm) entryKind(structure.Sign) int64 { return revenue }

func (revenueForm) asks(string) bool { return true }

func (f revenueForm) keep(c changes.Change, _ Request, entry store.Entry, t table) error {
	return addDetail(c, "RevenueDetails", entry.ID, map[string]any{"ItemId": t.item.ID, "SubgroupId": f.subgroup.ID},
		func() error {
			return c.Q.AddRevenueDetails(c.Ctx, store.AddRevenueDetailsParams{EntryID: entry.ID, ItemID: t.item.ID, SubgroupID: f.subgroup.ID})
		})
}

// partnerForm is a partner, a table of the project's capital (PartnerTable, redix-api #109): money they put in, or take out when the choice of its
// movement type says minus (BR-05, BR-06).
type partnerForm struct {
	expense
	partner store.Partner
}

func (partnerForm) entryKind(sign structure.Sign) int64 {
	if sign == structure.Minus {
		return capitalOut
	}
	return capitalIn
}

func (partnerForm) asks(string) bool { return true }

// amount is the total as typed: what a movement does to the capital is its kind's to say.
func (partnerForm) amount(r Request, _ structure.Sign) fixed.Total { return r.Amount.Total() }

func (f partnerForm) keep(c changes.Change, _ Request, entry store.Entry, _ table) error {
	return addDetail(c, "CapitalMovementDetails", entry.ID, map[string]any{"PartnerId": f.partner.ID},
		func() error {
			return c.Q.AddCapitalDetails(c.Ctx, store.AddCapitalDetailsParams{EntryID: entry.ID, PartnerID: f.partner.ID})
		})
}

// isRevenue says whether the table is on the revenue side (EntryTable.IsRevenue): entered by whoever moves money into any active account, with
// no payment method and no deduction.
func (t table) isRevenue() bool { return structure.ItemType(t.item.Type).IsRevenue() }

// sign is the sign of what r chose in the table's list whose choices carry one (redix-api #129); none when it has no such list or chose nothing.
func (t table) sign(r Request) structure.Sign {
	values := r.kept()
	for _, column := range t.columns {
		if sign, isSigned := structure.SignOfChoice(column, values.Values[column.Key]); isSigned {
			return sign
		}
	}
	return structure.Unsigned
}

// offered is every table of the project whoever holds held may enter into, in the order of the report (EntryTable.OfferedAsync): each active
// category's items, each with its tables in the order its sheet prints them. An expense item's groups and active contracts are offered to whoever
// records entries; a revenue item's groups and a capital item's partners to whoever moves money (BR-30, redix-api #109).
func offered(c changes.Change, held access.Access) ([]table, error) {
	q, ctx, projectID := c.Q, c.Ctx, c.ProjectID
	categories, err := q.CategoriesOf(ctx, projectID)
	if err != nil {
		return nil, err
	}
	items, err := q.ItemsOf(ctx, projectID)
	if err != nil {
		return nil, err
	}
	subgroups, err := q.SubgroupsOf(ctx, projectID)
	if err != nil {
		return nil, err
	}
	columns, err := q.ColumnsOf(ctx, projectID)
	if err != nil {
		return nil, err
	}
	offeredContracts, err := q.OfferedContracts(ctx, projectID)
	if err != nil {
		return nil, err
	}
	partners, err := q.PartnersOf(ctx, projectID)
	if err != nil {
		return nil, err
	}
	records, moves := held.Holds(projectID, access.RecordEntries), held.Holds(projectID, access.MoveMoney)
	var tables []table
	for _, category := range categories {
		if !category.IsActive {
			continue
		}
		for _, item := range items {
			if item.CategoryID != category.ID || !item.IsActive {
				continue
			}
			tables = append(tables, tablesOf(category, item, subgroups, columns, offeredContracts, partners, records, moves)...)
		}
	}
	// A table unticked for the person is never offered: Add entry's picker and recording both read here (#106 R10).
	return slices.DeleteFunc(tables, func(t table) bool { return !held.Sees(c.ProjectID, t.id) }), nil
}

func tablesOf(category store.Category, item store.Item, subgroups []store.Subgroup, columns []store.ItemColumn, offeredContracts []store.OfferedContractsRow,
	partners []store.Partner, records, moves bool) []table {
	itemType := structure.ItemType(item.Type)
	var own []store.Subgroup
	general := uuid.Nil
	for _, subgroup := range subgroups {
		if subgroup.ItemID == item.ID {
			own = append(own, subgroup)
			if subgroup.Name == "" {
				general = subgroup.ID
			}
		}
	}
	shaped := func(id uuid.UUID, name string, kind, place int64, f form) table {
		t := table{category: category, item: item, id: id, name: name, kind: kind, place: place, form: f}
		for _, column := range columns {
			if column.ItemID == item.ID && !column.IsHidden && f.asks(column.Key) {
				t.columns = append(t.columns, column)
			}
		}
		return t
	}
	group := func(subgroup store.Subgroup, f form) table {
		if subgroup.Name == "" {
			return shaped(subgroup.ID, item.Name, generalTable, subgroup.SortOrder, f)
		}
		return shaped(subgroup.ID, subgroup.Name, subgroupTable, subgroup.SortOrder, f)
	}
	var tables []table
	if records && itemType.OfferedFor(structure.Purchases) {
		for _, subgroup := range own {
			tables = append(tables, group(subgroup, purchaseForm{subgroup: subgroup}))
		}
	}
	if records && itemType.OfferedFor(structure.ContractorPayments) {
		for _, row := range offeredContracts {
			if row.ItemID == item.ID {
				contract := store.Contract{ID: row.ID, ProjectID: row.ProjectID, ItemID: row.ItemID, SortOrder: row.SortOrder, SupplierID: row.SupplierID, Value: row.Value,
					IsOpenValue: row.IsOpenValue, UnitPrice: row.UnitPrice, Quantity: row.Quantity, Notes: row.Notes, IsActive: row.IsActive}
				tables = append(tables, shaped(row.ID, row.Contractor, contractTable, row.SortOrder, contractForm{contract: contract, general: general}))
			}
		}
	}
	if moves && itemType.OfferedFor(structure.OtherRevenue) {
		for _, subgroup := range own {
			tables = append(tables, group(subgroup, revenueForm{subgroup: subgroup}))
		}
	}
	if moves && itemType.OfferedFor(structure.Capital) {
		for _, partner := range partners {
			if !partner.IsDeleted {
				tables = append(tables, shaped(partner.ID, partner.Name, partnerTable, partner.SortOrder, partnerForm{partner: partner}))
			}
		}
	}
	slices.SortStableFunc(tables, func(a, b table) int { return int(a.place - b.place) })
	return tables
}

// offeredTable is part of what GET /projects/{projectId}/entry-tables (UC-012) answers.
type offeredTable struct {
	ID         uuid.UUID `json:"id"`
	Kind       named     `json:"kind"`
	Name       string    `json:"name"`
	ColumnKeys []string  `json:"columnKeys"`
}

// offeredItem is part of what GET /projects/{projectId}/entry-tables (UC-012) answers.
type offeredItem struct {
	ID        uuid.UUID      `json:"id"`
	Name      string         `json:"name"`
	Tables    []offeredTable `json:"tables"`
	IsRevenue bool           `json:"isRevenue"`
}

// offeredCategory is part of what GET /projects/{projectId}/entry-tables (UC-012) answers.
type offeredCategory struct {
	ID    uuid.UUID     `json:"id"`
	Name  string        `json:"name"`
	Items []offeredItem `json:"items"`
}

// entryTables is every table the caller may enter into (redix-app #84, redix-api #110), grouped by category and item in the order of the report,
// each with the keys of the columns its entry is asked.
func (s Routes) entryTables(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	tables, err := offered(changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}, caller.Access)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []offeredCategory{}
	for _, t := range tables {
		if len(views) == 0 || views[len(views)-1].ID != t.category.ID {
			views = append(views, offeredCategory{ID: t.category.ID, Name: t.category.Name, Items: []offeredItem{}})
		}
		category := &views[len(views)-1]
		if len(category.Items) == 0 || category.Items[len(category.Items)-1].ID != t.item.ID {
			category.Items = append(category.Items, offeredItem{ID: t.item.ID, Name: t.item.Name, Tables: []offeredTable{}, IsRevenue: t.isRevenue()})
		}
		item := &category.Items[len(category.Items)-1]
		keys := []string{}
		for _, column := range t.columns {
			keys = append(keys, column.Key)
		}
		item.Tables = append(item.Tables, offeredTable{ID: t.id, Kind: named{t.kind, tableKinds}, Name: t.name, ColumnKeys: keys})
	}
	web.JSON(w, r, http.StatusOK, views)
}

// itemTableView is part of what GET /projects/{projectId}/items/{itemId}/tables (UC-012) answers.
type itemTableView struct {
	ID    uuid.UUID `json:"id"`
	Kind  named     `json:"kind"`
	Name  string    `json:"name"`
	Count int       `json:"count"`
	place int64
}

// itemTables is an item's tables in the order its sheet prints them (CategoriesEndpoints.Tables, AC-46.8): its general table named after the
// item, each named subgroup, each contract's payments named after its contractor, and a capital item's partners the caller sees (BR-31), each
// with how many approved entries it holds. 404 for an item the project does not hold.
func (s Routes) itemTables(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	item, err := changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: web.ID(r, "itemId"), ProjectID: c.ProjectID}))
	if changes.Answered(w, r, err) {
		return
	}
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	known, factsErr := factsOf(c, all)
	subgroups, subgroupsErr := c.Q.SubgroupsOfItem(c.Ctx, item.ID)
	contractRows, contractsErr := c.Q.ContractsOf(c.Ctx, c.ProjectID)
	partners, partnersErr := c.Q.PartnersInItem(c.Ctx, item.ID)
	if err != nil || factsErr != nil || subgroupsErr != nil || contractsErr != nil || partnersErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	counts := map[uuid.UUID]int{}
	for _, entry := range all {
		if entry.Status != approved {
			continue
		}
		if row, has := known.expenses[entry.ID]; has && row.ItemID == item.ID && entry.Kind == purchase {
			counts[row.SubgroupID]++
		}
		if row, has := known.revenues[entry.ID]; has && row.ItemID == item.ID {
			counts[row.SubgroupID]++
		}
		if row, has := known.payments[entry.ID]; has {
			counts[row.ContractID]++
		}
		if partner, has := known.capital[entry.ID]; has {
			counts[partner]++
		}
	}
	views := []itemTableView{}
	if item.Type != int64(structure.Capital) {
		for _, subgroup := range subgroups {
			kind, name := subgroupTable, subgroup.Name
			if subgroup.Name == "" {
				kind, name = generalTable, item.Name
			}
			views = append(views, itemTableView{ID: subgroup.ID, Kind: named{kind, tableKinds}, Name: name, place: subgroup.SortOrder})
		}
	}
	for _, contract := range contractRows {
		if contract.ItemID == item.ID {
			views = append(views, itemTableView{ID: contract.ID, Kind: named{contractTable, tableKinds}, Name: contract.SupplierName, place: contract.SortOrder})
		}
	}
	seesAll, chosen := caller.Access.Holds(c.ProjectID, access.SeeAllCapital), caller.Access.ChosenIn(c.ProjectID)
	if !caller.Access.Holds(c.ProjectID, access.SeeChosenCapital) {
		chosen = nil
	}
	for _, partner := range partners {
		if !partner.IsDeleted && (seesAll || slices.Contains(chosen, partner.ID)) {
			views = append(views, itemTableView{ID: partner.ID, Kind: named{partnerTable, tableKinds}, Name: partner.Name, place: partner.SortOrder})
		}
	}
	views = slices.DeleteFunc(views, func(view itemTableView) bool { return !caller.Access.Sees(c.ProjectID, view.ID) })
	slices.SortStableFunc(views, func(a, b itemTableView) int { return int(a.place - b.place) })
	for i := range views {
		views[i].Count = counts[views[i].ID]
	}
	web.JSON(w, r, http.StatusOK, views)
}

// sightTables is the project's tables as «الجداول» ticks them (#106 R4, D8, D9): every group and contract by category and item, in the order of the
// report, each item's in the order its sheet prints them, finished ones included; one the caller does not see is listed only to someone who manages
// the people, who can then tick it back (G2). A partner is the capital ticks' (R14).
func (s Routes) sightTables(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	q, ctx, projectID := s.database.Queries(), r.Context(), web.ID(r, "projectId")
	categories, err := q.CategoriesOf(ctx, projectID)
	items, itemsErr := q.ItemsOf(ctx, projectID)
	subgroups, subgroupsErr := q.SubgroupsOf(ctx, projectID)
	contracts, contractsErr := q.ContractsOf(ctx, projectID)
	if errors.Join(err, itemsErr, subgroupsErr, contractsErr) != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	listed := func(id uuid.UUID) bool {
		return caller.Access.Sees(projectID, id) || caller.Access.Holds(projectID, access.ManageUsers)
	}
	views := []offeredCategory{}
	for _, category := range categories {
		view := offeredCategory{ID: category.ID, Name: category.Name, Items: []offeredItem{}}
		for _, item := range items {
			if item.CategoryID != category.ID || item.Type == int64(structure.Capital) {
				continue
			}
			var placed []itemTableView
			for _, subgroup := range subgroups {
				if subgroup.ItemID == item.ID {
					kind, name := subgroupTable, subgroup.Name
					if subgroup.Name == "" {
						kind, name = generalTable, item.Name
					}
					placed = append(placed, itemTableView{ID: subgroup.ID, Kind: named{kind, tableKinds}, Name: name, place: subgroup.SortOrder})
				}
			}
			for _, contract := range contracts {
				if contract.ItemID == item.ID {
					placed = append(placed, itemTableView{ID: contract.ID, Kind: named{contractTable, tableKinds}, Name: contract.SupplierName, place: contract.SortOrder})
				}
			}
			slices.SortStableFunc(placed, func(a, b itemTableView) int { return int(a.place - b.place) })
			shown := offeredItem{ID: item.ID, Name: item.Name, Tables: []offeredTable{}, IsRevenue: structure.ItemType(item.Type).IsRevenue()}
			for _, table := range placed {
				if listed(table.ID) {
					shown.Tables = append(shown.Tables, offeredTable{ID: table.ID, Kind: table.Kind, Name: table.Name, ColumnKeys: []string{}})
				}
			}
			if len(shown.Tables) > 0 {
				view.Items = append(view.Items, shown)
			}
		}
		if len(view.Items) > 0 {
			views = append(views, view)
		}
	}
	web.JSON(w, r, http.StatusOK, views)
}
