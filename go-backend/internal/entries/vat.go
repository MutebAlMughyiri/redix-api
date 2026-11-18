// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"slices"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
)

// VatRow is one purchase on the VAT statement: its day, its party with the party's VAT number, what it was for, and its total.
type VatRow struct {
	Date                          moment.Date
	Party, VatNumber, Description string
	Total                         fixed.Number
}

// VatQuarter is one quarter of the statement, from its first day to its last.
type VatQuarter struct {
	Start, End time.Time
	Purchases  []VatRow
}

// VatQuarters is the كشف الضريبة (UC-056, VatStatement.OfAsync): the quarters from the one holding first to the one holding last, each with the
// approved purchases dated inside the period, oldest first, each a total with its party's VAT number; a purchase not described says its item.
// REDIX keeps no VAT: the workbook works it out of each total at the configured rate (D19).
func VatQuarters(c changes.Change, first, last time.Time) ([]VatQuarter, error) {
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return nil, err
	}
	var purchases []store.Entry
	for _, entry := range all {
		if entry.Kind == purchase && entry.Status == approved && !entry.EntryDate.Before(first) && !entry.EntryDate.After(last) {
			purchases = append(purchases, entry)
		}
	}
	slices.SortStableFunc(purchases, func(a, b store.Entry) int {
		if !a.EntryDate.Equal(b.EntryDate.Time) {
			return a.EntryDate.Compare(b.EntryDate.Time)
		}
		return int(a.Number - b.Number)
	})
	known, err := factsOf(c, purchases)
	if err != nil {
		return nil, err
	}
	var quarters []VatQuarter
	for start := time.Date(first.Year(), (first.Month()-1)/3*3+1, 1, 0, 0, 0, 0, time.UTC); !start.After(last); start = start.AddDate(0, 3, 0) {
		quarter := VatQuarter{Start: start, End: start.AddDate(0, 3, -1), Purchases: []VatRow{}}
		for _, entry := range purchases {
			if entry.EntryDate.Before(quarter.Start) || entry.EntryDate.After(quarter.End) {
				continue
			}
			supplier, description := known.supplierOf(entry), entry.Description
			if description == "" {
				description = known.itemNameOf(entry)
			}
			quarter.Purchases = append(quarter.Purchases, VatRow{Date: entry.EntryDate, Party: supplier.Name, VatNumber: supplier.VatNumber, Description: description,
				Total: entry.Amount})
		}
		quarters = append(quarters, quarter)
	}
	return quarters, nil
}
