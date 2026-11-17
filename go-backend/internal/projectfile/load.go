package projectfile

import (
	"encoding/json"
	"errors"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"net/http"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/reports"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
	"github.com/google/uuid"
)

// Routes loads project files.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the load of a project file, for the manager who created the project, who holds ManageProject (D1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	router.Handle("POST", "/projects/{projectId:guid}/project-file", gate.Changes(access.ManageProject, s.load))
}

// owner is whose rule refused a part of a project file, which says whose words the app shows for its code (ProjectFileOwner, D14).
type owner string

const (
	ownerFile           owner = "file"
	ownerEntry          owner = "entry"
	ownerStructure      owner = "structure"
	ownerContract       owner = "contract"
	ownerPartner        owner = "partner"
	ownerSupplier       owner = "supplier"
	ownerPaymentMethod  owner = "paymentMethod"
	ownerReconciliation owner = "reconciliation"
)

// refusal is one refusal of a load (ProjectFileRefusal, AC-53.5, D14): where it is, by the file's category, item and table and the row's place
// counted from 1 (0 for the part itself), whose rule refused it and its code, with the column, the value and the column's name a code names.
type refusal struct {
	Category  string `json:"category"`
	Item      string `json:"item"`
	Table     string `json:"table"`
	Row       int    `json:"row"`
	Owner     owner  `json:"owner"`
	Code      string `json:"code"`
	ColumnKey string `json:"columnKey"`
	Value     string `json:"value"`
	Column    string `json:"column"`
}

// registers is the project's accounts, suppliers and payment methods by name, which a row's list values name.
type registers struct {
	accounts, suppliers, paymentMethods map[string]uuid.UUID
}

// place is a table of the file with the item and the table of the project it goes into.
type place struct {
	category string
	item     store.Item
	table    fileTable
	tableID  uuid.UUID
}

func (p place) refusal(row int, by owner, code string) refusal {
	return refusal{Category: p.category, Item: p.item.Name, Table: p.table.Name, Row: row, Owner: by, Code: code}
}

// load loads a project file into an approved project that holds no entry yet (FR-53, D4). A file that is not a project file answers
// notAProjectFile, a newer one newerVersion (AC-53.9), and a project holding an entry projectHasEntries (409, AC-53.3), each before anything
// changes. Then its missing categories, items, tables, contracts, partners, suppliers and payment methods are added (D5, D6, D13), every row is
// recorded through Add entry's own checks and approved in the caller's name (D7, AC-53.4), counting from the file's report time (D12), what each
// account really held becomes one reconciliation (D8, AC-53.8), and the file's report is issued with its number and time (D9, AC-53.7); a number
// the project already issued is reportAlreadyIssued. Any refusal, a row's or a part's, answers rowsRefused with every one of them under rows, and
// nothing is saved (D3, D14, AC-53.5).
func (s Routes) load(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	content, _, sent := attachments.Sent(w, r)
	if !sent {
		return
	}
	projectFile, code := read(content)
	if code != "" {
		problem.Refuse(w, r, http.StatusBadRequest, code)
		return
	}
	holds, err := s.database.Queries().ProjectHoldsEntries(r.Context(), web.ID(r, "projectId"))
	switch {
	case err != nil:
		problem.Status(w, r, http.StatusInternalServerError)
		return
	case holds:
		problem.Refuse(w, r, http.StatusConflict, "projectHasEntries")
		return
	}
	var loaded struct {
		Entries int   `json:"entries"`
		Tables  int   `json:"tables"`
		Report  int64 `json:"report"`
	}
	if !s.writer.Write(w, r, caller, func(c changes.Change) error {
		var refused []refusal
		issued, err := c.Q.PeriodicReportSnapshot(c.Ctx, store.PeriodicReportSnapshotParams{ProjectID: c.ProjectID, Number: projectFile.Report.Number})
		if err != nil {
			return err
		}
		if projectFile.Report.Number > 0 && len(issued) > 0 {
			refused = append(refused, refusal{Owner: ownerFile, Code: "reportAlreadyIssued", Value: strconv.FormatInt(projectFile.Report.Number, 10)})
		}
		known, err := registersOf(c, projectFile, &refused)
		if err != nil {
			return err
		}
		places, err := placesOf(c, projectFile, &refused)
		if err != nil {
			return err
		}
		balances := balancesOf(projectFile, known, &refused)
		kept, err := record(c, places, projectFile, known, &refused)
		if err != nil {
			return err
		}
		if len(refused) > 0 {
			return changes.RefuseOver(http.StatusBadRequest, "rowsRefused", "rows", refused)
		}
		zone, err := zones.Of(c.Ctx, c.Q, c.ProjectID)
		if err != nil {
			return err
		}
		approvedAt := c.Now
		if at, _ := projectFile.Report.at(zone); projectFile.Report.Number > 0 {
			approvedAt = at
		}
		tables := map[uuid.UUID]bool{}
		for _, row := range kept {
			if err := entries.Approve(c, row.entry, approvedAt); err != nil {
				return err
			}
			tables[row.tableID] = true
		}
		if len(projectFile.Accounts) > 0 {
			if code, err := codeOf(accounts.Enter(c, balances)); err != nil || code != "" {
				return orRefused(err, []refusal{{Owner: ownerReconciliation, Code: code}})
			}
		}
		if projectFile.Report.Number > 0 {
			at, _ := projectFile.Report.at(zone)
			if _, err := reports.Issue(c, projectFile.Report.Number, at); err != nil {
				return err
			}
		}
		loaded.Entries, loaded.Tables, loaded.Report = len(kept), len(tables), projectFile.Report.Number
		return nil
	}) {
		return
	}
	web.JSON(w, r, http.StatusOK, loaded)
}

// codeOf is the code of an owner's refusal, "" when it did not refuse; any other failure stays an error.
func codeOf(err error) (string, error) {
	var refused changes.Refusal
	if errors.As(err, &refused) {
		return refused.Code, nil
	}
	return "", err
}

// orRefused is err, or the load refused over rows.
func orRefused(err error, rows []refusal) error {
	if err != nil {
		return err
	}
	return changes.RefuseOver(http.StatusBadRequest, "rowsRefused", "rows", rows)
}

// registersOf is the project's accounts, suppliers and payment methods by name, once the suppliers and payment methods of the file's lists it
// lacks are added through their owners (D6). An account is never added: the file carries no type or custodian.
func registersOf(c changes.Change, projectFile file, refused *[]refusal) (registers, error) {
	known := registers{accounts: map[string]uuid.UUID{}, suppliers: map[string]uuid.UUID{}, paymentMethods: map[string]uuid.UUID{}}
	supplierRows, err := c.Q.SuppliersOf(c.Ctx, store.SuppliersOfParams{ProjectID: c.ProjectID})
	if err != nil {
		return known, err
	}
	for _, row := range supplierRows {
		if _, seen := known.suppliers[row.Name]; !seen {
			known.suppliers[row.Name] = row.ID
		}
	}
	for _, name := range labelsOf(projectFile, "supplier") {
		if _, found := known.suppliers[name]; found {
			continue
		}
		added, err := suppliers.Add(c, suppliers.Request{Name: name})
		code, err := codeOf(err)
		switch {
		case err != nil:
			return known, err
		case code != "":
			*refused = append(*refused, refusal{Owner: ownerSupplier, Code: code, Value: name})
		default:
			known.suppliers[added.Name] = added.ID
		}
	}
	methods, err := c.Q.PaymentMethodsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return known, err
	}
	for _, method := range methods {
		if _, seen := known.paymentMethods[method.Name]; !seen {
			known.paymentMethods[method.Name] = method.ID
		}
	}
	for _, name := range labelsOf(projectFile, "paymentMethod") {
		if _, found := known.paymentMethods[name]; found {
			continue
		}
		if _, named := naming.Named(name); !named {
			*refused = append(*refused, refusal{Owner: ownerPaymentMethod, Code: naming.Invalid, Value: name})
			continue
		}
		if known.paymentMethods[name], err = settings.AddPaymentMethod(c, name, settings.EveryAccount); err != nil {
			return known, err
		}
	}
	accountRows, err := c.Q.AccountsOf(c.Ctx, c.ProjectID)
	for _, account := range accountRows {
		if _, seen := known.accounts[account.Name]; !seen {
			known.accounts[account.Name] = account.ID
		}
	}
	return known, err
}

// labelsOf is the labels of the options of the file's lists of key, trimmed, each once, none empty.
func labelsOf(projectFile file, key string) []string {
	var labels []string
	for _, list := range projectFile.Lists {
		for _, option := range list.Options {
			if label := strings.TrimSpace(option.Label); list.Key == key && label != "" && !slices.Contains(labels, label) {
				labels = append(labels, label)
			}
		}
	}
	return labels
}

// placesOf is every table of the file with the item and the table of the project it goes into: a category and an item found by name, or added at
// the end (D5, D13); a table found by name within its item, or added through its owner - a subgroup, a contract with the contractor its name
// gives after «دفعات», or a partner (D6). A part its owner refuses is listed, and its rows with it.
func placesOf(c changes.Change, projectFile file, refused *[]refusal) ([]place, error) {
	var places []place
	for _, fileCategory := range projectFile.Categories {
		categoryName, named := naming.Named(fileCategory.Name)
		if !named {
			*refused = append(*refused, refusal{Category: fileCategory.Name, Owner: ownerStructure, Code: naming.Invalid})
			continue
		}
		categoryID, err := categoryOf(c, categoryName)
		if err != nil {
			return nil, err
		}
		for _, fileItem := range fileCategory.Items {
			item, code, err := itemOf(c, categoryID, fileItem)
			if err != nil {
				return nil, err
			}
			if code != "" {
				*refused = append(*refused, refusal{Category: categoryName, Item: fileItem.Name, Owner: ownerStructure, Code: code})
				continue
			}
			for _, table := range fileItem.Tables {
				by, tableID, code, err := tableOf(c, item, table)
				if err != nil {
					return nil, err
				}
				at := place{category: categoryName, item: item, table: table, tableID: tableID}
				if code != "" {
					*refused = append(*refused, at.refusal(0, by, code))
				} else {
					places = append(places, at)
				}
			}
		}
	}
	return places, nil
}

// categoryOf is the project's category named name, or one added at the end.
func categoryOf(c changes.Change, name string) (uuid.UUID, error) {
	categories, err := c.Q.CategoriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return uuid.Nil, err
	}
	for _, category := range categories {
		if category.Name == name {
			return category.ID, nil
		}
	}
	added, err := structure.AddCategory(c, name)
	return added.ID, err
}

// itemOf is the item of fileItem's name in the category, or one added with its type; the owner's refusal otherwise.
func itemOf(c changes.Change, categoryID uuid.UUID, fileItem fileItem) (store.Item, string, error) {
	name, named := naming.Named(fileItem.Name)
	if !named {
		return store.Item{}, naming.Invalid, nil
	}
	items, err := c.Q.ItemsOfCategory(c.Ctx, categoryID)
	if err != nil {
		return store.Item{}, "", err
	}
	for _, item := range items {
		if item.Name == name {
			return item, "", nil
		}
	}
	added, _, err := structure.AddItem(c, categoryID, name, fileItem.Type)
	code, err := codeOf(err)
	return added, code, err
}

// tableOf is the table of item that table is, found by name or added through its owner; that owner and its refusal otherwise.
func tableOf(c changes.Change, item store.Item, table fileTable) (owner, uuid.UUID, string, error) {
	name, named := naming.Named(table.Name)
	if !named {
		return ownerStructure, uuid.Nil, naming.Invalid, nil
	}
	switch table.Kind {
	case structure.ContractorPayments:
		return contractOf(c, item, strings.TrimSpace(strings.TrimPrefix(name, contractTitlePrefix)), table.ContractAmount)
	case structure.Capital:
		return partnerOf(c, name)
	}
	subgroupName := name
	if name == item.Name {
		subgroupName = ""
	}
	subgroups, err := c.Q.SubgroupsOfItem(c.Ctx, item.ID)
	if err != nil {
		return ownerStructure, uuid.Nil, "", err
	}
	for _, subgroup := range subgroups {
		if subgroup.Name == subgroupName {
			return ownerStructure, subgroup.ID, "", nil
		}
	}
	added, err := structure.AddSubgroup(c, item, name)
	code, err := codeOf(err)
	return ownerStructure, added, code, err
}

// contractTitlePrefix starts the name of a contract's table of payments, the contractor's name after it (Contract.TableTitlePrefix).
const contractTitlePrefix = "دفعات "

// contractOf is the item's contract with contractor, or one added for the value agreed, an open value when the file holds none (D6); the
// contractor is added as a supplier when the project lacks one of that name.
func contractOf(c changes.Change, item store.Item, contractor string, agreed json.RawMessage) (owner, uuid.UUID, string, error) {
	held, err := c.Q.ContractsOfItem(c.Ctx, item.ID)
	if err != nil {
		return ownerContract, uuid.Nil, "", err
	}
	supplierRows, err := c.Q.SuppliersOf(c.Ctx, store.SuppliersOfParams{ProjectID: c.ProjectID})
	if err != nil {
		return ownerContract, uuid.Nil, "", err
	}
	supplierID := uuid.Nil
	for _, row := range supplierRows {
		if row.Name == contractor && supplierID == uuid.Nil {
			supplierID = row.ID
		}
	}
	for _, contract := range held {
		if slices.ContainsFunc(supplierRows, func(row store.SuppliersOfRow) bool { return row.ID == contract.SupplierID && row.Name == contractor }) {
			return ownerContract, contract.ID, "", nil
		}
	}
	if supplierID == uuid.Nil {
		added, err := suppliers.Add(c, suppliers.Request{Name: contractor})
		if code, err := codeOf(err); err != nil || code != "" {
			return ownerSupplier, uuid.Nil, code, err
		}
		supplierID = added.ID
	}
	var terms contracts.Terms
	if json.Unmarshal(agreed, &terms.Value) != nil {
		terms.IsOpenValue, terms.Value = true, 0
	}
	added, err := contracts.Add(c, supplierID, item.ID, terms)
	code, err := codeOf(err)
	return ownerContract, added, code, err
}

// partnerOf is the project's partner named name, or one added with no login (D6).
func partnerOf(c changes.Change, name string) (owner, uuid.UUID, string, error) {
	held, err := c.Q.PartnersOf(c.Ctx, c.ProjectID)
	if err != nil {
		return ownerPartner, uuid.Nil, "", err
	}
	for _, partner := range held {
		if partner.Name == name && !partner.IsDeleted {
			return ownerPartner, partner.ID, "", nil
		}
	}
	added, err := partners.Add(c, name, uuid.Nil)
	code, err := codeOf(err)
	return ownerPartner, added, code, err
}

// balancesOf is what each account of the file really held, by the project's account of its name (D8); an account the project lacks is listed as
// accountNotInProject (D6).
func balancesOf(projectFile file, known registers, refused *[]refusal) []accounts.Actual {
	var balances []accounts.Actual
	for _, account := range projectFile.Accounts {
		accountID, found := known.accounts[strings.TrimSpace(account.Name)]
		if !found {
			*refused = append(*refused, refusal{Owner: ownerFile, Code: "accountNotInProject", Value: account.Name})
			continue
		}
		var held fixed.Typed
		json.Unmarshal(account.ActualBalance, &held)
		balances = append(balances, accounts.Actual{AccountID: accountID, ActualBalance: held})
	}
	return balances
}

// kept is an entry the load recorded, with its table.
type kept struct {
	entry   store.Entry
	tableID uuid.UUID
}

// asked is a row of the file as Add entry is asked to record it, with its place in the file and in its table.
type asked struct {
	inFile, number int
	at             place
	request        entries.Request
}

// record records every row of every table through Add entry's own checks, telling no decider (D7): a table not offered to the caller is listed
// once, and each row refused is listed with its table, its place counted from 1 as the sheet's «#» counts it, and its code, in the file's order.
// The rows are recorded by their date, then in the file's order, so each «رقم القيد» follows the calendar (redix-api #128 D6).
func record(c changes.Change, places []place, projectFile file, known registers, refused *[]refusal) ([]kept, error) {
	options := map[string][2]string{}
	for _, list := range projectFile.Lists {
		for _, option := range list.Options {
			if _, seen := options[option.ID]; !seen {
				options[option.ID] = [2]string{list.Key, strings.TrimSpace(option.Label)}
			}
		}
	}
	inside, err := c.Q.ArabicWord(c.Ctx, "contractorPayments.inside")
	outside, outsideErr := c.Q.ArabicWord(c.Ctx, "contractorPayments.outside")
	if err := errors.Join(err, outsideErr); err != nil {
		return nil, err
	}
	type found struct {
		inFile  int
		refusal refusal
	}
	var rows []asked
	var refusals []found
	inFile := 0
	for _, at := range places {
		columns, err := entries.AskedColumns(c, at.item.ID, at.tableID)
		if code, err := codeOf(err); err != nil {
			return nil, err
		} else if code != "" {
			refusals = append(refusals, found{inFile, at.refusal(0, ownerEntry, code)})
			inFile++
			continue
		}
		for index, row := range at.table.Rows {
			request, refused := requestOf(row, at, columns, options, known, [2]string{inside, outside})
			if refused.Code != "" {
				refused.Category, refused.Item, refused.Table, refused.Row, refused.Owner = at.category, at.item.Name, at.table.Name, index+1, ownerFile
				refusals = append(refusals, found{inFile, refused})
			} else {
				rows = append(rows, asked{inFile: inFile, number: index + 1, at: at, request: request})
			}
			inFile++
		}
	}
	slices.SortStableFunc(rows, func(a, b asked) int { return a.request.Date.Compare(b.request.Date.Time) })
	var recorded []kept
	for _, row := range rows {
		entry, err := entries.Record(c, row.request, false)
		var refusedEntry changes.Refusal
		switch {
		case errors.As(err, &refusedEntry):
			columnKey, _ := refusedEntry.Detail.(string)
			at := row.at.refusal(row.number, ownerEntry, refusedEntry.Code)
			at.ColumnKey = columnKey
			refusals = append(refusals, found{row.inFile, at})
		case err != nil:
			return nil, err
		default:
			recorded = append(recorded, kept{entry: entry, tableID: row.at.tableID})
		}
	}
	slices.SortStableFunc(refusals, func(a, b found) int { return a.inFile - b.inFile })
	for _, one := range refusals {
		*refused = append(*refused, one.refusal)
	}
	return recorded, nil
}

// requestOf is what row asks Add entry to record, sent for approval, in the table at: each value under its column's key, a list's option by its
// label. An option the file's lists do not hold, or a choice the table's list does not have, answers unknownChoice, naming the value and the
// column (G4); an account the project lacks accountNotInProject (D6); a file named attachmentNotInFile, since a file carries none (G3).
// Everything else is Add entry's to check.
func requestOf(row fileRow, at place, columns []store.ItemColumn, options map[string][2]string, known registers, places [2]string) (entries.Request, refusal) {
	request := entries.Request{ID: uuid.Must(uuid.NewV7()), ItemID: at.item.ID, TableID: at.tableID, Submit: true, ColumnValues: web.Ordered{Values: map[string]string{}}}
	refuse := func(code, key, value, column string) (entries.Request, refusal) {
		return entries.Request{}, refusal{Code: code, ColumnKey: key, Value: value, Column: column}
	}
	for _, key := range row.keys {
		column := fileColumn{Key: key}
		for _, one := range at.table.Columns {
			if one.Key == key {
				column = one
				break
			}
		}
		value := textOf(row.values[key])
		if column.Type == structure.List && value != "" {
			option, found := options[value]
			if !found || option[0] != column.List {
				return refuse("unknownChoice", key, value, column.Name)
			}
			value = option[1]
		}
		switch {
		case key == "description":
			request.Description = value
		case key == "date":
			if day, err := time.Parse("2006-01-02", value); err == nil {
				request.Date = moment.Date{Time: day}
			}
		case key == "total" || key == "amount":
			if json.Unmarshal([]byte(strings.ReplaceAll(value, ",", "")), &request.Amount) != nil {
				request.Amount = fixed.Typed{}
			}
		case key == "account" && value != "":
			accountID, found := known.accounts[value]
			if !found {
				return refuse("accountNotInProject", key, value, column.Name)
			}
			request.AccountID = accountID
		case key == "supplier":
			request.SupplierID = known.suppliers[value]
		case key == "paymentMethod":
			request.PaymentMethodID = known.paymentMethods[value]
		case key == "outsideContract" && value != "":
			if value != places[0] && value != places[1] {
				return refuse("unknownChoice", key, value, column.Name)
			}
			request.IsOutsideContract = value == places[1]
		case key == "attachment" && value != "":
			return refuse("attachmentNotInFile", key, value, column.Name)
		case key == "account" || key == "outsideContract" || key == "attachment":
		default:
			var choices []structure.Choice
			for _, asked := range columns {
				if asked.Key == key && structure.DataType(asked.DataType) == structure.List {
					choices = append(choices, structure.ChoicesIn(asked)...)
				}
			}
			if index := slices.IndexFunc(choices, func(choice structure.Choice) bool { return choice.Name == value }); len(choices) > 0 && value != "" {
				if index < 0 {
					return refuse("unknownChoice", key, value, column.Name)
				}
				value = choices[index].ID.String()
			}
			request.ColumnValues.Keys = append(request.ColumnValues.Keys, key)
			request.ColumnValues.Values[key] = value
		}
	}
	return request, refusal{}
}
