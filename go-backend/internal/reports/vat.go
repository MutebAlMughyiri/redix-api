// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"fmt"
	"net/http"
	"strconv"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// The كشف الضريبة as the accountant's workbook (VatStatementFiles, redix-api #96, D5, D19): a sheet per quarter, its purchases, each row's VAT and
// amount before VAT as formulas on its total at the configured rate, and the purchases' VAT at the foot; in Excel's own look, not the statement's (#76
// row 267).

var (
	quarterOrdinals = [4]string{"الأول", "الثاني", "الثالث", "الرابع"}
	vatTitles       = []string{"التاريخ", "اسم الشركة", "البيان", "الرقم الضريبي", "إجمالي قيمة الفاتورة", "الضريبة", "أصل المبلغ قبل الضريبة"}
)

// vatWorkbookOf is the workbook of quarters at ratePercent, as the config value writes it: 15 for 15%.
func vatWorkbookOf(quarters []entries.VatQuarter, ratePercent string) ([]byte, error) {
	book := statement.NewWorkbook()
	for _, quarter := range quarters {
		ordinal := quarterOrdinals[(quarter.Start.Month()-1)/3]
		sheet := book.AddSheet(fmt.Sprintf("ضريبة الربع %s %d", ordinal, quarter.Start.Year()))
		sheet.Cell(1, 1).Set(fmt.Sprintf("ضريبة المشتريات عن الربع %s سنة %d", ordinal, quarter.Start.Year()))
		sheet.Range(1, 1, 1, len(vatTitles)).Merge().Bold().Centre()
		for column, title := range vatTitles {
			sheet.Cell(2, column+1).Set(title)
		}
		sheet.Range(2, 1, 2, len(vatTitles)).Bold()
		total := vatSection(sheet, 3, quarter.Purchases, ratePercent, "الإجمالي")
		foot := total + 2
		sheet.Cell(foot, 1).Set("إجمالي ضريبة المشتريات")
		sheet.Cell(foot, 6).Format(statement.MoneyFormat).Formula = fmt.Sprintf("F%d", total)
		sheet.Range(foot, 1, foot, len(vatTitles)).Bold()
		sheet.Fit()
	}
	return book.Bytes()
}

// vatSection writes rows from first, then their totals; it answers the totals' row.
func vatSection(sheet *statement.Worksheet, first int, rows []entries.VatRow, rate, label string) int {
	row := first
	for _, line := range rows {
		sheet.Cell(row, 1).Set(line.Date.Time).Format(statement.VatDateFormat)
		sheet.Cell(row, 2).Set(line.Party)
		sheet.Cell(row, 3).Set(line.Description)
		sheet.Cell(row, 4).Set(line.VatNumber)
		sheet.Cell(row, 5).Set(line.Total.Float())
		sheet.Cell(row, 6).Formula = fmt.Sprintf("E%d*%s/(100+%s)", row, rate, rate)
		sheet.Cell(row, 7).Formula = fmt.Sprintf("E%d-F%d", row, row)
		for column := 5; column <= 7; column++ {
			sheet.Cell(row, column).Format(statement.MoneyFormat)
		}
		row++
	}
	sheet.Cell(row, 1).Set(label)
	for column, letter := range []string{"E", "F", "G"} {
		sheet.Cell(row, column+5).Format(statement.MoneyFormat).Formula = sumOf(letter, first, row-1)
	}
	sheet.Range(row, 1, row, len(vatTitles)).Bold()
	return row
}

// vatWorkbook is the كشف الضريبة from the query's from to its to as the accountant's workbook, at the rate of the config value vatRatePercent
// (D18, D19); a period ending before it starts answers invalidPeriod.
func (s Routes) vatWorkbook(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	from, fromErr := time.Parse("2006-01-02", r.URL.Query().Get("from"))
	to, toErr := time.Parse("2006-01-02", r.URL.Query().Get("to"))
	switch {
	case fromErr != nil || toErr != nil:
		problem.Status(w, r, http.StatusBadRequest)
		return
	case to.Before(from):
		problem.Refuse(w, r, http.StatusBadRequest, "invalidPeriod")
		return
	}
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	quarters, err := entries.VatQuarters(c, from, to)
	values, valuesErr := c.Q.Values(c.Ctx)
	rate := ""
	for _, value := range values {
		if value.Key == "vatRatePercent" {
			rate = value.Value
		}
	}
	if _, rateErr := strconv.ParseFloat(rate, 64); err != nil || valuesErr != nil || rateErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	file, err := vatWorkbookOf(quarters, rate)
	s.sendFile(w, r, file, excelType, fmt.Sprintf("vat-%s-%s.xlsx", from.Format("2006-01-02"), to.Format("2006-01-02")), err)
}
