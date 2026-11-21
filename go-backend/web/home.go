// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/reports"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// newestShown is how many of the newest entries the home lists, as the app shows them by default (redix-app #105).
const newestShown = 10

// homeTab is the home of p's project, the app's home in the content area (UC-049, redix-api #165): its summary, the custody p's caller holds, and
// the newest entries; the project list while they choose a project. Every figure comes from the owner the app's routes call. The error is the
// database's.
func (s site) homeTab(r *http.Request, p page) (templ.Component, error) {
	if !p.inProject() {
		return projectList(p), nil
	}
	read, projectID := s.Database.Queries(), p.project.ID
	summary, err := reports.SummaryOf(r.Context(), read, projectID, p.caller.Access.Unseen(projectID))
	listed, accountsErr := accounts.ListOf(r.Context(), read, projectID, p.caller, false)
	newest, newestErr := entries.Newest(r.Context(), read, projectID, p.caller, newestShown)
	if err := errors.Join(err, accountsErr, newestErr); err != nil {
		return nil, err
	}
	var custody []accounts.View
	for _, account := range listed {
		if account.IsMine && account.IsActive {
			custody = append(custody, account)
		}
	}
	return home(p, summary, custody, newest), nil
}

// waitsForMe says whether the summary shows what waits for p's caller's approval: something waits, and they may approve it, as the app's
// summary card shows it.
func (p page) waitsForMe(summary reports.Summary) bool {
	return summary.Waiting.ExpensesCount+summary.Waiting.RevenueCount > 0 && p.may(access.ApproveEntries)
}

// money is an amount as the app and the report files write it (BR-13): 1,234.56, a negative in brackets.
func money(total fixed.Total) string { return statement.Money(total.Number()) }

// entryTitle is what an entry is about on a list: its partner for capital, its item otherwise, as the app titles its cards.
func entryTitle(row entries.Row) string {
	if row.PartnerName != "" {
		return row.PartnerName
	}
	return row.ItemName
}

// share is part's share of whole as a whole percent, for a category's bar.
func share(part, whole fixed.Total) int {
	if whole.Number() == 0 {
		return 0
	}
	return int(part.Number() * 100 / whole.Number())
}
