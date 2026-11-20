// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"net/http"
	"slices"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// attachmentWord is what a file's cell says when the entry carries one, as the approved workbook writes it (ReportColumns.AttachmentWord).
const attachmentWord = "فتح المرفق"

// Everything spent on one item (UC-050, FR-30): its purchases by subgroup, then each contract's payments; for a revenue item, its revenue by
// subgroup; for a capital item, each partner's money in and out, a table each (ItemReportEndpoints, redix-api #109).

// ReportChoice is a choice of a list column in a report, with the sign it gives the amount (ReportChoiceView).
type ReportChoice struct {
	Name string         `json:"name"`
	Sign structure.Sign `json:"sign"`
}

// ReportColumn is a column of a table in a report, as the report keeps it (ReportColumnView, redix-api #129).
type ReportColumn struct {
	Key        string             `json:"key"`
	Name       string             `json:"name"`
	DataType   structure.DataType `json:"dataType"`
	IsRequired bool               `json:"isRequired"`
	Choices    []ReportChoice     `json:"choices"`
}

// ReportRow is one entry in an item's report, in its table's columns (ItemEntryRow, R1); Number is 0 in a report issued before entries had one (G6).
type ReportRow struct {
	EntryID           uuid.UUID    `json:"entryId"`
	Date              moment.Date  `json:"date"`
	Amount            fixed.Number `json:"amount"`
	IsOutsideContract bool         `json:"isOutsideContract"`
	Values            []string     `json:"values"`
	IsNew             bool         `json:"isNew"`
	Number            int64        `json:"number"`
}

// TableReport is one table's rows: a subgroup's purchases or revenue, or a partner's money in and out (SubgroupReportView).
type TableReport struct {
	ID         uuid.UUID      `json:"id"`
	Name       string         `json:"name"`
	Total      fixed.Total    `json:"total"`
	Rows       []ReportRow    `json:"rows"`
	SortOrder  int64          `json:"sortOrder"`
	Columns    []ReportColumn `json:"columns"`
	AmountName string         `json:"amountName"`
}

// ContractReport is one contract with its payments (ContractReportView).
type ContractReport struct {
	ID             uuid.UUID      `json:"id"`
	ContractorName string         `json:"contractorName"`
	Value          fixed.Number   `json:"value"`
	IsOpenValue    bool           `json:"isOpenValue"`
	Paid           fixed.Total    `json:"paid"`
	Remaining      fixed.Total    `json:"remaining"`
	IsOverrun      bool           `json:"isOverrun"`
	PaymentsTotal  fixed.Total    `json:"paymentsTotal"`
	Payments       []ReportRow    `json:"payments"`
	SortOrder      int64          `json:"sortOrder"`
	Columns        []ReportColumn `json:"columns"`
	AmountName     string         `json:"amountName"`
}

// ItemReport is everything spent on one item, as its screen, its files and a periodic report show it (ItemReportView).
type ItemReport struct {
	ItemID                uuid.UUID          `json:"itemId"`
	ItemName              string             `json:"itemName"`
	CategoryName          string             `json:"categoryName"`
	Total                 fixed.Total        `json:"total"`
	Subgroups             []TableReport      `json:"subgroups"`
	Contracts             []ContractReport   `json:"contracts"`
	ShowsTablePercentages bool               `json:"showsTablePercentages"`
	Type                  structure.ItemType `json:"type"`
	ShowsEveryTable       bool               `json:"showsEveryTable"`
}

// asksOf is the form whose columns an entry of kind is asked (StructureSeed.IsAsked).
func asksOf(kind int64) form {
	switch kind {
	case purchase:
		return purchaseForm{}
	case contractorPayment:
		return contractForm{}
	default:
		return revenueForm{}
	}
}

func isAmount(key string) bool { return key == "total" || key == "amount" }

// printedOf is the columns of the item a table of entries of kind prints, in their order (ReportColumns.PrintedOf, R1): those it asks that print in
// the reports, but its amount, the table's last column.
func printedOf(columns []store.ItemColumn, kind int64) []store.ItemColumn {
	var printed []store.ItemColumn
	for _, column := range columns {
		if !column.IsHidden && column.ShowInReports && asksOf(kind).asks(column.Key) && !isAmount(column.Key) {
			printed = append(printed, column)
		}
	}
	return printed
}

// amountNameOf is the name of the amount column of the item's tables of kind, their header; "" when it has none (ReportColumns.AmountNameOf).
func amountNameOf(columns []store.ItemColumn, kind int64) string {
	for _, column := range columns {
		if isAmount(column.Key) && asksOf(kind).asks(column.Key) {
			return column.Name
		}
	}
	return ""
}

// registers is what a report's list columns choose from (ReportColumns): the project's suppliers, accounts and payment methods, and a contract's
// two places.
type registers struct {
	names  map[string][]string
	places [2]string
}

func (g registers) viewsOf(columns []store.ItemColumn) []ReportColumn {
	views := []ReportColumn{}
	for _, column := range columns {
		view := ReportColumn{Key: column.Key, Name: column.Name, DataType: structure.DataType(column.DataType), IsRequired: column.IsRequired, Choices: []ReportChoice{}}
		switch column.Key {
		case "supplier", "account", "paymentMethod":
			for _, name := range g.names[column.Key] {
				view.Choices = append(view.Choices, ReportChoice{Name: name})
			}
		case "outsideContract":
			view.Choices = append(view.Choices, ReportChoice{Name: g.places[0]}, ReportChoice{Name: g.places[1]})
		default:
			for _, choice := range structure.ChoicesIn(column) {
				view.Choices = append(view.Choices, ReportChoice{Name: choice.Name, Sign: choice.Sign})
			}
		}
		views = append(views, view)
	}
	return views
}

// valuesOf is what entry shows under each of columns, as text (ReportColumns.ValuesOf): a date as yyyy-MM-dd, a number as it was kept, a list's
// choice by its name.
func (g registers) valuesOf(columns []store.ItemColumn, entry store.Entry, known facts, isOutside bool) ([]string, error) {
	kept := web.Ordered{}
	if err := kept.UnmarshalJSON([]byte(entry.ColumnValues)); err != nil {
		return nil, err
	}
	values := []string{}
	for _, column := range columns {
		value := kept.Values[column.Key]
		switch column.Key {
		case "description":
			value = entry.Description
		case "date":
			value = entry.EntryDate.Format("2006-01-02")
		case "supplier", "contract":
			value = known.supplierOf(entry).Name
		case "account":
			value = known.accounts[entry.AccountID]
		case "paymentMethod":
			value = known.paymentMethods[known.expenses[entry.ID].PaymentMethodID]
		case "attachment":
			value = ""
			if len(known.files[entry.ID]) > 0 {
				value = attachmentWord
			}
		case "outsideContract":
			value = g.places[0]
			if isOutside {
				value = g.places[1]
			}
		default:
			if structure.DataType(column.DataType) == structure.List {
				value = structure.ChoiceNameOf(column, value)
			}
		}
		values = append(values, value)
	}
	return values, nil
}

// ItemReportOf is the report of the item, from what is approved, in date order, what was approved since the latest periodic report marked new
// (BR-26); of a capital item's partners only those shows keeps have their table, its total staying the whole capital (BR-31). Only the tables
// c.Access sees are in it, and only their entries count (#106 R6).
func ItemReportOf(c changes.Change, item store.Item, shows func(uuid.UUID) bool) (ItemReport, error) {
	q, ctx, projectID := c.Q, c.Ctx, c.ProjectID
	all, err := q.EntriesOf(ctx, projectID)
	if err != nil {
		return ItemReport{}, err
	}
	known, err := factsOf(c, all)
	if err != nil {
		return ItemReport{}, err
	}
	all = slices.DeleteFunc(all, func(entry store.Entry) bool { return !known.tableSeen(c.Access, projectID, entry.ID) })
	latest, err := q.LatestPeriodicReport(ctx, projectID)
	if err != nil {
		return ItemReport{}, err
	}
	newSince := notApproved
	for _, report := range latest {
		newSince = report.IssuedAt
	}
	columns, err := q.ColumnsOfItem(ctx, item.ID)
	subgroups, subgroupsErr := q.SubgroupsOfItem(ctx, item.ID)
	contractRows, contractsErr := q.ContractsOf(ctx, projectID)
	partners, partnersErr := q.PartnersInItem(ctx, item.ID)
	category, categoryErr := q.CategoryOf(ctx, store.CategoryOfParams{ID: item.CategoryID, ProjectID: projectID})
	names, namesErr := q.RegisterNames(ctx, projectID)
	inside, insideErr := q.ArabicWord(ctx, "contractorPayments.inside")
	outside, outsideErr := q.ArabicWord(ctx, "contractorPayments.outside")
	money, moneyErr := contracts.MoneyOf(ctx, q, projectID, uuid.Nil)
	for _, failed := range []error{err, subgroupsErr, contractsErr, partnersErr, categoryErr, namesErr, insideErr, outsideErr, moneyErr} {
		if failed != nil {
			return ItemReport{}, failed
		}
	}
	g := registers{names: map[string][]string{}, places: [2]string{inside, outside}}
	for _, row := range names {
		g.names[row.Register] = append(g.names[row.Register], row.Name)
	}
	approvedOldestFirst := slices.Clone(all)
	slices.SortStableFunc(approvedOldestFirst, func(a, b store.Entry) int {
		if !a.EntryDate.Equal(b.EntryDate.Time) {
			return a.EntryDate.Compare(b.EntryDate.Time)
		}
		return int(a.Number - b.Number)
	})
	groupKind := purchase
	if structure.ItemType(item.Type).IsRevenue() {
		groupKind = revenue
	}
	rowOf := func(entry store.Entry, printed []store.ItemColumn, amount fixed.Number) (ReportRow, error) {
		isOutside := known.payments[entry.ID].IsOutsideContract
		values, err := g.valuesOf(printed, entry, known, isOutside)
		return ReportRow{EntryID: entry.ID, Date: entry.EntryDate, Amount: amount, IsOutsideContract: isOutside, Values: values,
			IsNew: entry.ApprovedAt.After(newSince.Time), Number: entry.Number}, err
	}
	report := ItemReport{ItemID: item.ID, ItemName: item.Name, CategoryName: category.Name, Subgroups: []TableReport{}, Contracts: []ContractReport{},
		ShowsTablePercentages: item.ShowsTablePercentages, Type: structure.ItemType(item.Type), ShowsEveryTable: true}
	for _, entry := range approvedOldestFirst {
		if entry.Status != approved {
			continue
		}
		expense, isExpense := known.expenses[entry.ID]
		revenueRow, isRevenue := known.revenues[entry.ID]
		partner, isCapital := known.capital[entry.ID]
		inItem := (isExpense && expense.ItemID == item.ID) || (isRevenue && revenueRow.ItemID == item.ID) ||
			(isCapital && slices.ContainsFunc(partners, func(row store.Partner) bool { return row.ID == partner }))
		if inItem {
			report.Total = report.Total.Plus(signedFor(entry))
		}
	}
	groupColumns, paymentColumns, capitalColumns := printedOf(columns, groupKind), printedOf(columns, contractorPayment), printedOf(columns, capitalIn)
	for _, subgroup := range subgroups {
		table := TableReport{ID: subgroup.ID, Name: subgroup.Name, Rows: []ReportRow{}, SortOrder: subgroup.SortOrder, Columns: g.viewsOf(groupColumns),
			AmountName: amountNameOf(columns, groupKind)}
		for _, entry := range approvedOldestFirst {
			expense, isExpense := known.expenses[entry.ID]
			revenueRow, isRevenue := known.revenues[entry.ID]
			ofGroup := (isExpense && entry.Kind == purchase && expense.ItemID == item.ID && expense.SubgroupID == subgroup.ID) ||
				(isRevenue && revenueRow.ItemID == item.ID && revenueRow.SubgroupID == subgroup.ID)
			if entry.Status == approved && ofGroup {
				row, err := rowOf(entry, groupColumns, entry.Amount)
				if err != nil {
					return ItemReport{}, err
				}
				table.Rows = append(table.Rows, row)
				table.Total = table.Total.Plus(fixed.Counted(entry.Amount))
			}
		}
		if len(table.Rows) > 0 {
			report.Subgroups = append(report.Subgroups, table)
		}
	}
	var itemContracts []store.ContractsOfRow
	for _, row := range contractRows {
		if row.ItemID == item.ID {
			itemContracts = append(itemContracts, row)
		}
	}
	slices.SortStableFunc(itemContracts, func(a, b store.ContractsOfRow) int { return int(a.SortOrder - b.SortOrder) })
	for _, row := range itemContracts {
		contract := store.Contract{ID: row.ID, Value: row.Value, IsOpenValue: row.IsOpenValue}
		paid := money[row.ID].Paid
		view := ContractReport{ID: row.ID, ContractorName: row.SupplierName, Value: row.Value, IsOpenValue: row.IsOpenValue, Paid: paid,
			Remaining: contracts.Remaining(contract, paid), IsOverrun: contracts.Overrun(contract, paid), Payments: []ReportRow{}, SortOrder: row.SortOrder,
			Columns: g.viewsOf(paymentColumns), AmountName: amountNameOf(columns, contractorPayment)}
		for _, entry := range approvedOldestFirst {
			payment, isPayment := known.payments[entry.ID]
			if entry.Status == approved && entry.Kind == contractorPayment && isPayment && payment.ContractID == row.ID && known.expenses[entry.ID].ItemID == item.ID {
				payment, err := rowOf(entry, paymentColumns, entry.Amount)
				if err != nil {
					return ItemReport{}, err
				}
				view.Payments = append(view.Payments, payment)
				view.PaymentsTotal = view.PaymentsTotal.Plus(fixed.Counted(entry.Amount))
			}
		}
		if len(view.Payments) > 0 || row.Value > 0 {
			report.Contracts = append(report.Contracts, view)
		}
	}
	slices.SortStableFunc(partners, func(a, b store.Partner) int { return int(a.SortOrder - b.SortOrder) })
	shown := 0
	for _, partner := range partners {
		if !shows(partner.ID) {
			continue
		}
		shown++
		table := TableReport{ID: partner.ID, Name: partner.Name, Rows: []ReportRow{}, SortOrder: partner.SortOrder, Columns: g.viewsOf(capitalColumns),
			AmountName: amountNameOf(columns, capitalIn)}
		for _, entry := range approvedOldestFirst {
			if mover, isCapital := known.capital[entry.ID]; entry.Status == approved && isCapital && mover == partner.ID {
				amount := signedFor(entry)
				row, err := rowOf(entry, capitalColumns, amount.Number())
				if err != nil {
					return ItemReport{}, err
				}
				table.Rows = append(table.Rows, row)
				table.Total = table.Total.Plus(amount)
			}
		}
		if len(table.Rows) > 0 {
			report.Subgroups = append(report.Subgroups, table)
		}
	}
	report.ShowsEveryTable = shown == len(partners)
	report.Subgroups = slices.DeleteFunc(report.Subgroups, func(table TableReport) bool { return !c.Access.Sees(projectID, table.ID) })
	report.Contracts = slices.DeleteFunc(report.Contracts, func(contract ContractReport) bool { return !c.Access.Sees(projectID, contract.ID) })
	return report, nil
}

// signedFor is what an entry adds to its item's total: its amount, taken away for capital taken out.
func signedFor(entry store.Entry) fixed.Total {
	if entry.Kind == capitalOut {
		return fixed.Total{}.Minus(fixed.Counted(entry.Amount))
	}
	return fixed.Counted(entry.Amount)
}

// itemReportRoute is the report of an item of the project (ItemReportEndpoints.Report): a capital item shows the caller only the partners they see
// (BR-31), its total the whole capital; 404 for an item of another project.
func (s Routes) itemReportRoute(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
	item, err := changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: web.ID(r, "itemId"), ProjectID: c.ProjectID}))
	if changes.Answered(w, r, err) {
		return
	}
	report, err := ItemReportOf(c, item, func(partnerID uuid.UUID) bool { return caller.Access.SeesCapitalOf(c.ProjectID, partnerID) })
	s.answer(w, r, report, err)
}

// contractReportRoute is the report of one contract, as the Contracts screen opens it (the owner, 12 Nov 2026): its item's report holding that
// contract's table alone; 404 for a contract of another project.
func (s Routes) contractReportRoute(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
	report, err := ContractReportOf(c, web.ID(r, "contractId"))
	s.answer(w, r, report, err)
}

// ContractReportOf is the report of the contract contractID (ItemReportEndpoints.OfContractAsync), for its screen and its files: its item's report
// holding that contract's table alone; NotFound for a contract of another project, or one c.Access does not see (#106 R9).
func ContractReportOf(c changes.Change, contractID uuid.UUID) (ItemReport, error) {
	contract, err := changes.Found(c.Q.ContractOf(c.Ctx, store.ContractOfParams{ID: contractID, ProjectID: c.ProjectID}))
	if err == nil && !c.Access.Sees(c.ProjectID, contractID) {
		err = changes.NotFound
	}
	if err != nil {
		return ItemReport{}, err
	}
	item, err := changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: contract.ItemID, ProjectID: c.ProjectID}))
	if err != nil {
		return ItemReport{}, err
	}
	report, err := ItemReportOf(c, item, func(uuid.UUID) bool { return false })
	if err != nil {
		return ItemReport{}, err
	}
	report.Subgroups = []TableReport{}
	kept := []ContractReport{}
	for _, one := range report.Contracts {
		if one.ID == contract.ID {
			kept = append(kept, one)
		}
	}
	report.Contracts = kept
	return report, nil
}

// WithTablesOf is the same report holding only the tables shows keeps, the partners' of رأس المال a reader may see; its total stays the whole (BR-31).
func (r ItemReport) WithTablesOf(shows func(uuid.UUID) bool) ItemReport {
	kept := []TableReport{}
	for _, table := range r.Subgroups {
		if shows(table.ID) {
			kept = append(kept, table)
		}
	}
	if len(kept) < len(r.Subgroups) {
		r.Subgroups, r.ShowsEveryTable = kept, false
	}
	return r
}

// EntryIDs is the entries the report's tables show, every row and payment.
func (r ItemReport) EntryIDs() []uuid.UUID {
	var ids []uuid.UUID
	for _, table := range r.Subgroups {
		for _, row := range table.Rows {
			ids = append(ids, row.EntryID)
		}
	}
	for _, table := range r.Contracts {
		for _, row := range table.Payments {
			ids = append(ids, row.EntryID)
		}
	}
	return ids
}
