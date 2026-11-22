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

	"github.com/a-h/templ"

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
	if (!p.inProject() && name != "myHistory") || !p.offers(name) {
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

// partnerHistoryOf is the page of the partner history query asks, through the rule the app's route answers (entries.PartnerHistoryOf): each line
// as the app's says it, an approved movement's saying who approved it. A partner the caller does not see lists nothing. The error is the
// database's.
func (s site) partnerHistoryOf(ctx context.Context, p page, query url.Values) (history, error) {
	shown, err := s.historyOf(ctx, p, "partnerHistory", "/partner-history", query)
	lines, linesErr := entries.PartnerHistoryOf(ctx, s.Database.Queries(), p.project.ID, p.caller, query)
	var refused changes.Refusal
	if errors.As(linesErr, &refused) {
		linesErr = nil
	}
	shown.full = len(lines) == entries.PageSize
	for _, line := range lines {
		kind := line.Kind.Name()
		shown.rows = append(shown.rows, historyRow{id: line.ID, kind: kind, what: p.words.Of("history."+kind, partnerLineValues(p, line)...),
			who: shown.when(p, line.By, kind == "capitalIn" || kind == "capitalOut", line.At)})
	}
	return shown, errors.Join(err, linesErr)
}

// partnerLineValues are what fills a partner history line's word, as the app's PartnerHistoryKind fills it.
func partnerLineValues(p page, line entries.PartnerLine) []string {
	switch line.Kind.Name() {
	case "partnerChanged":
		if line.NameWas == "" {
			return []string{line.Partner, ""}
		}
		return []string{line.Partner, p.words.Of("history.nameWas", line.NameWas)}
	case "partnerAccount":
		if line.Login == "" {
			return []string{line.Partner, p.words.Of("partners.noLogin")}
		}
		return []string{line.Partner, line.Login}
	case "capitalIn", "capitalOut":
		return []string{statement.Money(line.Amount.Number()), line.Partner, line.Account}
	}
	return []string{line.Partner}
}

// projectAreas and loginAreas are the kinds of line the project history and My history narrow to, in the app's order (HistoryArea.inAProject,
// HistoryArea.ofALogin).
var (
	projectAreas = []string{"entries", "accounts", "partners", "suppliers", "structure", "project", "people"}
	loginAreas   = []string{"signIn", "password", "biometrics", "details"}
)

// projectHistoryOf is the page of «سجل المشروع» query asks, through the rule the app's route answers (entries.ProjectHistoryOf): each line as
// the app's says it, only for whoever may see its record (UC-069). The error is the database's.
func (s site) projectHistoryOf(ctx context.Context, p page, query url.Values) (history, error) {
	shown, err := s.historyOf(ctx, p, "projectHistory", "/project-history", query)
	lines, linesErr := entries.ProjectHistoryOf(ctx, s.Database.Queries(), p.project.ID, p.caller, query)
	shown.full = len(lines) == entries.PageSize
	for _, line := range lines {
		shown.rows = append(shown.rows, historyRow{id: line.ID, kind: line.Action.Name(), what: projectLineText(p, line, shown.location),
			who: shown.when(p, line.By, false, line.At)})
	}
	return shown, errors.Join(err, linesErr)
}

// projectLineText is what a line of «سجل المشروع» says, as the app's ProjectHistoryLine.textIn: what was done to what, a permission by its own
// word; what it was about, a retrieval's archive time in the project's; the name it had, what it went from and to, and its amount.
func projectLineText(p page, line entries.ProjectLine, location *time.Location) string {
	name := line.Name
	if line.Thing.Name() == "permission" {
		name = p.words.Of("permissions." + line.Name)
	}
	about, nameWas, fromTo, amount := line.About, "", "", ""
	if at, err := time.Parse(time.RFC3339Nano, about); err == nil && line.Action.Name() == "retrieved" {
		about = at.In(location).Format("2006-01-02 15:04")
	}
	if line.NameWas != "" {
		nameWas = p.words.Of("history.nameWas", line.NameWas)
	}
	if line.From != "" {
		fromTo = p.words.Of("history.fromTo", line.From, line.To)
	}
	if line.Amount != 0 {
		amount = statement.Money(line.Amount)
	}
	done := strings.Join(slices.DeleteFunc([]string{p.words.Of("history.action." + line.Action.Name()), p.words.Of("history.thing." + line.Thing.Name()), name},
		func(part string) bool { return part == "" }), " ")
	return parts(done, about, nameWas, fromTo, amount)
}

// myHistoryOf is the page of «سجلي» query asks, through the rule the app's route answers (entries.MyHistoryOf): every line of the caller's own
// login, whoever wrote it, in the project's time when one is chosen, Riyadh's otherwise. The error is the database's.
func (s site) myHistoryOf(ctx context.Context, p page, query url.Values) (history, error) {
	read := s.Database.Queries()
	zone, err := "", error(nil)
	if p.inProject() {
		zone, err = zones.Of(ctx, read, p.project.ID)
	}
	shown := history{name: "myHistory", query: query, next: "/my-history", location: zones.Location(zone)}
	lines, linesErr := entries.MyHistoryOf(ctx, read, p.caller, query)
	shown.full = len(lines) == entries.PageSize
	for _, line := range lines {
		shown.rows = append(shown.rows, historyRow{id: line.ID, kind: line.Kind.Name(), what: parts(p.words.Of("history.login."+line.Kind.Name()), line.About),
			who: shown.when(p, line.By, false, line.At)})
	}
	return shown, errors.Join(err, linesErr)
}

// projectHistoryTab is «سجل المشروع» in its tab (UC-069, redix-api #172), for whoever may see the project's history.
func (s site) projectHistoryTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.projectHistoryOf(r.Context(), p, r.URL.Query())
	return projectHistoryPage(p, shown), err
}

// projectHistoryMore is the next page of «سجل المشروع», added after the last line.
func (s site) projectHistoryMore(w http.ResponseWriter, r *http.Request) {
	s.historyMore(w, r, "projectHistory", s.projectHistoryOf)
}

// myHistoryTab is «سجلي» in its tab (UC-069 R6, redix-api #172), opened from the top bar, for everyone signed in.
func (s site) myHistoryTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.myHistoryOf(r.Context(), p, r.URL.Query())
	return myHistoryPage(p, shown), err
}

// myHistoryMore is the next page of «سجلي», added after the last line.
func (s site) myHistoryMore(w http.ResponseWriter, r *http.Request) {
	s.historyMore(w, r, "myHistory", s.myHistoryOf)
}
