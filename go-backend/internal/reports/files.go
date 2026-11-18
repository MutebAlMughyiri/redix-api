// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"errors"
	"net/http"
	"slices"
	"strconv"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// The report files besides the periodic report's (FR-33, UC-057, UC-058, ReportFilesEndpoints): an item's report, one contract's and the contracts
// report, each as a workbook, for whoever holds GenerateReports; a file's رأس المال holds only the partners its reader may see (redix-api #131 D1, D2,
// BR-31). Their PDFs are still on .NET.

const (
	excelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
	// contractTitlePrefix starts the title of a contract's table of payments (Contract.TableTitlePrefix).
	contractTitlePrefix = "دفعات "
)

// sheetOf is an item's sheet (ItemReportFiles.SheetOf, FR-46): a table per subgroup that has entries, titled with its name or, for the general
// one, the item's; a table of payments per contract, its agreed amount under it (AC-46.2); for رأس المال a table per partner; all in the order the
// item's tables were put in (AC-46.8). Every table prints its own columns (redix-api #129, R1), its amount column last; the item's choice says
// whether each table's percentage is calculated (AC-46.9). A row's «المرفق» opens its entry's file by its link in fileLinks; none in a PDF.
func sheetOf(item entries.ItemReport, fileLinks map[uuid.UUID]string) statement.Sheet {
	type placed struct {
		sortOrder int64
		table     statement.Table
	}
	var tables []placed
	for _, subgroup := range item.Subgroups {
		title := subgroup.Name
		if title == "" {
			title = item.ItemName
		}
		tables = append(tables, placed{subgroup.SortOrder, tableOf(title, subgroup.Columns, subgroup.AmountName, subgroup.Rows, 0, fileLinks)})
	}
	for _, contract := range item.Contracts {
		agreed := contract.Value.Float()
		if contract.IsOpenValue {
			agreed = 0
		}
		tables = append(tables, placed{contract.SortOrder, tableOf(contractTitlePrefix+contract.ContractorName, contract.Columns, contract.AmountName, contract.Payments, agreed, fileLinks)})
	}
	slices.SortStableFunc(tables, func(a, b placed) int { return int(a.sortOrder - b.sortOrder) })
	sheet := statement.Sheet{Name: item.ItemName, ShowsEveryTable: item.ShowsEveryTable, Whole: item.Total.Number().Float()}
	for _, table := range tables {
		sheet.Tables = append(sheet.Tables, table.table)
	}
	if len(sheet.Tables) == 0 {
		sheet.Tables = []statement.Table{tableOf(item.ItemName, nil, "", nil, 0, fileLinks)}
	}
	if item.ShowsTablePercentages {
		sheet.PercentLabel = "النسبة"
	}
	return sheet
}

func tableOf(title string, columns []entries.ReportColumn, amountName string, rows []entries.ReportRow, agreed float64, fileLinks map[uuid.UUID]string) statement.Table {
	table := statement.Table{Title: title, Amount: statement.Column{Header: amountName}, AgreedAmount: agreed, NumberHeader: "رقم القيد"}
	for _, column := range columns {
		table.Columns = append(table.Columns, columnOf(column))
	}
	for _, row := range rows {
		line := statement.Row{Amount: row.Amount.Float(), IsNew: row.IsNew, CountsAgainstAgreement: !row.IsOutsideContract, FileLink: fileLinks[row.EntryID]}
		if row.Number > 0 {
			line.Number = row.Number
		}
		for index, column := range columns {
			value := ""
			if index < len(row.Values) {
				value = row.Values[index]
			}
			line.Cells = append(line.Cells, cellOf(column, value))
		}
		table.Rows = append(table.Rows, line)
	}
	return table
}

// columnOf is a column of an item as its table prints it: a date as a day, an amount as money, a text as it is, anything else centred; a list with
// the choices its cells take (redix-api #129).
func columnOf(column entries.ReportColumn) statement.Column {
	kind := statement.Centred
	switch column.DataType {
	case structure.Date:
		kind = statement.Day
	case structure.Amount:
		kind = statement.Price
	case structure.Text:
		kind = statement.Text
	}
	printed := statement.Column{Header: column.Name, Kind: kind, AllowsBlank: !column.IsRequired, IsFile: column.Key == "attachment"}
	for _, choice := range column.Choices {
		printed.Choices = append(printed.Choices, statement.Choice{Name: choice.Name, Sign: signText(choice.Sign)})
	}
	return printed
}

// signText is a choice's sign as the workbook writes it: + or −, nothing for none.
func signText(sign structure.Sign) string {
	switch sign {
	case structure.Plus:
		return "+"
	case structure.Minus:
		return "−"
	}
	return ""
}

// cellOf is a value as its cell holds it: a day, a number, or the text kept; nil when none was given.
func cellOf(column entries.ReportColumn, value string) any {
	if value == "" {
		return nil
	}
	if day, err := time.Parse("2006-01-02", value); err == nil && column.DataType == structure.Date {
		return day
	}
	if number, err := strconv.ParseFloat(value, 64); err == nil && (column.DataType == structure.Number || column.DataType == structure.Amount) {
		return number
	}
	return value
}

// workbookOf is one item's report as a workbook of the project projectName, its sheet as the periodic report's, its lists on «القوائم» (redix-api
// #129), each «المرفق» opening its entry's file by its link in fileLinks (ItemReportFiles.Excel).
func workbookOf(projectName string, item entries.ItemReport, fileLinks map[uuid.UUID]string) ([]byte, error) {
	book := statement.NewBook(projectName, "")
	book.Add(sheetOf(item, fileLinks))
	book.Lists.AddTo(book)
	return book.Bytes()
}

// contractsWorkbookOf is the contracts report as a workbook of the project projectName: one statement table (UC-067, findings row 131) of each
// contract's contractor, item, value and what was paid, its amount what is left of it, an overrun negative and an open value nothing.
func contractsWorkbookOf(projectName string, list []contracts.View) ([]byte, error) {
	table := statement.Table{Title: "العقود", Columns: []statement.Column{{Header: "المقاول"}, {Header: "البند"}, {Header: "قيمة العقد", Kind: statement.Price},
		{Header: "المدفوع", Kind: statement.Price}}, Amount: statement.Column{Header: "المتبقي", Kind: statement.Price}}
	for index, contract := range list {
		var value any = contract.Value.Float()
		remaining := contract.Remaining.Number().Float()
		if contract.IsOpenValue {
			value, remaining = "مفتوحة", 0
		}
		table.Rows = append(table.Rows, statement.Row{Number: index + 1, Cells: []any{contract.SupplierName, contract.ItemName, value, contract.Paid.Number().Float()},
			Amount: remaining, CountsAgainstAgreement: true})
	}
	book := statement.NewBook(projectName, "")
	book.Add(statement.Sheet{Name: "العقود", Tables: []statement.Table{table}, ShowsEveryTable: true})
	return book.Bytes()
}

// itemFile is an item's report as a PDF, or as a workbook; رأس المال's holds only the partners the caller may see, its total the whole capital
// (BR-31).
func (s Routes) itemFile(isPdf bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.sendItemFile(w, r, caller, isPdf)
	}
}

func (s Routes) sendItemFile(w http.ResponseWriter, r *http.Request, caller access.Caller, isPdf bool) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
	item, err := changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: web.ID(r, "itemId"), ProjectID: c.ProjectID}))
	if changes.Answered(w, r, err) {
		return
	}
	report, err := entries.ItemReportOf(c, item, func(partnerID uuid.UUID) bool { return caller.Access.SeesCapitalOf(c.ProjectID, partnerID) })
	s.sendReport(w, r, c, report, "item", isPdf, err)
}

// contractFile is one contract's report as a PDF or a workbook, from the item report's own writers (Muteb, 12 Nov 2026); 404 for a contract of
// another project.
func (s Routes) contractFile(isPdf bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
		report, err := entries.ContractReportOf(c, web.ID(r, "contractId"))
		if changes.Answered(w, r, err) {
			return
		}
		s.sendReport(w, r, c, report, "contract", isPdf, nil)
	}
}

// sendReport sends report as name.pdf, or as name.xlsx whose «المرفق» cells open their files by links on the address r came to (Muteb, 16 Nov
// 2026).
func (s Routes) sendReport(w http.ResponseWriter, r *http.Request, c changes.Change, report entries.ItemReport, name string, isPdf bool, err error) {
	if err == nil && isPdf {
		file, err := s.typst.Pdf(statement.Document{Pages: []statement.Page{statement.PdfPage(sheetOf(report, nil))}})
		s.sendFile(w, r, file, pdfType, name+".pdf", err)
		return
	}
	project, projectErr := c.Q.ProjectWithID(c.Ctx, c.ProjectID)
	links, linksErr := s.links.OfEntries(c.Ctx, c.Q, c.ProjectID, report.EntryIDs(), r)
	if err != nil || projectErr != nil || linksErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	file, err := workbookOf(project.Name, report, links)
	s.sendFile(w, r, file, excelType, name+".xlsx", err)
}

// contractsWorkbook is the contracts report as a workbook.
func (s Routes) contractsWorkbook(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	if name, list, found := s.contractsOf(w, r); found {
		file, err := contractsWorkbookOf(name, list)
		s.sendFile(w, r, file, excelType, "contracts.xlsx", err)
	}
}

// contractsOf is the project's name and its contracts, which the contracts report shows; false once it has answered 404 for a project that does not
// exist, or 500.
func (s Routes) contractsOf(w http.ResponseWriter, r *http.Request) (string, []contracts.View, bool) {
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	project, err := changes.Found(read.ProjectWithID(r.Context(), projectID))
	if errors.Is(err, changes.NotFound) {
		problem.Status(w, r, http.StatusNotFound)
		return "", nil, false
	}
	list, listErr := contracts.List(r.Context(), read, projectID)
	if err != nil || listErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return "", nil, false
	}
	return project.Name, list, true
}

// sendFile sends file as an attachment named name through web.SendFile, the one way the API hands a file it makes (redix-api #76 row 274).
func (s Routes) sendFile(w http.ResponseWriter, r *http.Request, file []byte, contentType, name string, err error) {
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.SendFile(w, contentType, name, file)
}
