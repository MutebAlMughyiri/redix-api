// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// historyRow is one line of a history as the web shows it, as the app's HistoryLine says it: what was done, who did it and when, and its kind,
// which marks it.
type historyRow struct {
	id              int64
	kind, what, who string
}

// history is a page of one of the project's histories as the app's history screens show it (redix-api #167, #168): its name - its tab's and the
// prefix of its ids -, its rows, the query that chose them, the people to narrow it by, where its next page is asked, and the project's time
// zone its times are told in. Every history page is drawn from it.
type history struct {
	name     string
	rows     []historyRow
	query    url.Values
	members  []store.MembersRow
	next     string
	full     bool
	location *time.Location
}

// historyOf is a history page named name, its next page asked at next, for query: the project's members and time zone, its rows to come. The
// error is the database's.
func (s site) historyOf(ctx context.Context, p page, name, next string, query url.Values) (history, error) {
	read := s.Database.Queries()
	members, err := read.Members(ctx, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	return history{name: name, query: query, members: members, next: next, location: zones.Location(zone)}, errors.Join(err, zoneErr)
}

// more is where the page after this one is asked when this one is full - the same filters, after its last line -; "" when it is the last.
func (h history) more() string {
	if !h.full || len(h.rows) == 0 {
		return ""
	}
	next := url.Values{}
	for key, values := range h.query {
		if key != "before" {
			next[key] = values
		}
	}
	next.Set("before", strconv.FormatInt(h.rows[len(h.rows)-1].id, 10))
	return h.next + "?" + next.Encode()
}

// when is who did a line and when, in the project's time, as the app's line says it: an approval says who approved it.
func (h history) when(p page, by string, approved bool, at moment.Moment) string {
	if approved {
		by = p.words.Of("history.approvedBy", by)
	}
	return parts(by, at.In(h.location).Format("2006-01-02 15:04"))
}

// parts is the non-empty of values, each apart from the next by a middle dot, as the app's partsLine joins them.
func parts(values ...string) string {
	return strings.Join(slices.DeleteFunc(values, func(value string) bool { return value == "" }), " · ")
}

// historyMore answers the next page of the history its tab names, added after the last line, with Show more again while there is more; 404
// for a history the caller may not read.
func (s site) historyMore(w http.ResponseWriter, r *http.Request, name string, of func(context.Context, page, url.Values) (history, error)) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.offers(name) {
		http.NotFound(w, r)
		return
	}
	shown, err := of(r.Context(), p, r.URL.Query())
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, historyRows(shown), historyMoreButton(p, shown, true))
}

// kindWord is the word of an entry's kind, as the Entries filter names it.
func kindWord(kind string) string {
	for _, known := range entryKinds {
		if known.name == kind {
			return known.word
		}
	}
	return ""
}

// approvalHistoryOf is the page of the approval history query asks, through the rule the app's route answers (entries.ApprovalHistoryOf): each
// decision as the app's line says it - the entry's item, supplier and amount, and a rejection's reason. The error is the database's.
func (s site) approvalHistoryOf(ctx context.Context, p page, query url.Values) (history, error) {
	shown, err := s.historyOf(ctx, p, "approvalHistory", "/approval-history", query)
	lines, linesErr := entries.ApprovalHistoryOf(ctx, s.Database.Queries(), p.project.ID, p.caller, query)
	shown.full = len(lines) == entries.PageSize
	for _, line := range lines {
		key := "history.entryApproved"
		if line.Kind.Name() == "rejected" {
			key = "history.entryRejected"
		}
		shown.rows = append(shown.rows, historyRow{id: line.ID, kind: line.Kind.Name(), what: p.words.Of(key, parts(line.Item, line.Party, statement.Money(line.Amount)), line.Reason),
			who: shown.when(p, line.By, line.Kind.Name() == "approved", line.At)})
	}
	return shown, errors.Join(err, linesErr)
}

// accountHistoryOf is the page of the account history query asks, through the rule the app's route answers (entries.AccountHistoryOf): each line
// as the app's says it, an approved entry's saying who approved it. An account no longer in the project lists nothing. The error is the
// database's.
func (s site) accountHistoryOf(ctx context.Context, p page, query url.Values) (history, error) {
	shown, err := s.historyOf(ctx, p, "accountHistory", "/account-history", query)
	lines, linesErr := entries.AccountHistoryOf(ctx, s.Database.Queries(), p.project.ID, p.caller, query)
	var refused changes.Refusal
	if errors.As(linesErr, &refused) {
		linesErr = nil
	}
	shown.full = len(lines) == entries.PageSize
	for _, line := range lines {
		kind := line.Kind.Name()
		shown.rows = append(shown.rows, historyRow{id: line.ID, kind: kind, what: p.words.Of("history."+kind, accountLineValues(p, line)...),
			who: shown.when(p, line.By, kind == "entryIn" || kind == "entryOut", line.At)})
	}
	return shown, errors.Join(err, linesErr)
}

// accountLineValues are what fills an account history line's word, as the app's AccountHistoryKind fills it.
func accountLineValues(p page, line entries.AccountLine) []string {
	money := statement.Money(line.Amount.Number())
	switch line.Kind.Name() {
	case "accountChanged":
		if line.NameWas == "" {
			return []string{line.Account, ""}
		}
		return []string{line.Account, p.words.Of("history.nameWas", line.NameWas)}
	case "custodianChanged":
		if line.Custodian == "" {
			return []string{line.Account, p.words.Of("accounts.noCustodian")}
		}
		return []string{line.Account, line.Custodian}
	case "transfer":
		return []string{money, line.Account, line.OtherAccount}
	case "entryIn", "entryOut":
		return []string{p.words.Of(kindWord(line.EntryKind.Name())), money, line.Account}
	case "check":
		return []string{money}
	}
	return []string{line.Account}
}
