// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"
	"strconv"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/reports"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// homeTab is the home of p's project, the app's home in the content area (UC-049, redix-api #165): its summary, the custody p's caller holds, and
// the newest entries, as many as Settings chose and as many again for each Show more (pages), as the app's; the project list while they choose
// a project. Every figure comes from the owner the app's routes call. The error is the
// database's.
func (s site) homeTab(r *http.Request, p page) (templ.Component, error) {
	if !p.inProject() {
		return projectList(p), nil
	}
	read, projectID := s.Database.Queries(), p.project.ID
	summary, err := reports.SummaryOf(r.Context(), read, projectID, p.caller.Access.Unseen(projectID))
	listed, accountsErr := accounts.ListOf(r.Context(), read, projectID, p.caller, false)
	pages, _ := strconv.Atoi(r.URL.Query().Get("pages"))
	asked := max(pages, 1) * entriesShownOf(r)
	newest, newestErr := entries.Newest(r.Context(), read, projectID, p.caller, asked)
	if err := errors.Join(err, accountsErr, newestErr); err != nil {
		return nil, err
	}
	var custody []accounts.View
	for _, account := range listed {
		if account.IsMine && account.IsActive {
			custody = append(custody, account)
		}
	}
	more := ""
	if len(newest) >= asked {
		more = "/tab/home?pages=" + strconv.Itoa(max(pages, 1)+1)
	}
	return home(p, summary, custody, newest, more), nil
}

// mayEnterExpenses says whether p's caller may record an entry here now: RecordEntries, in a project not finished, as the app's
// mayEnterExpenses; the home's Add entry and Entries' New entry both ask it.
func (p page) mayEnterExpenses() bool { return !p.finished() && p.may(access.RecordEntries) }

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
