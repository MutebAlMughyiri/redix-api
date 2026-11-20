// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries_test

import (
	"slices"
	"strconv"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/google/uuid"
)

var started = time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC)

// member adds a person called name to the project, holding permissions, and is their id and a token of theirs.
func (a project) member(name string, permissions ...string) (uuid.UUID, string) {
	id := uuid.Must(uuid.NewV7())
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES (?, ?, ?, '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, id.String(), name, credentials.Hash("Patrol-chosen-2"))
	for _, permission := range permissions {
		a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?)`, uuid.NewString(), strings.TrimPrefix(a.path, "/projects/"),
			id, permission)
	}
	return id, a.Token(id, false)
}

// lines is the page a history at path answers token, newest first.
func (a project) lines(path, token string) []map[string]any {
	a.t.Helper()
	code, answer := a.Ask("GET", path, ``, token)
	if code != 200 {
		a.t.Fatalf("%s answered %d: %v", path, code, answer)
	}
	var lines []map[string]any
	for _, line := range answer.([]any) {
		lines = append(lines, line.(map[string]any))
	}
	return lines
}

// said is each line of the page a history at path answers token, as what was done to what, and its name when it has one: "created unit B3".
func (a project) said(path, token string) []string {
	a.t.Helper()
	var said []string
	for _, line := range a.lines(path, token) {
		words := line["action"].(string) + " " + line["thing"].(string)
		if name := line["name"].(string); name != "" {
			words += " " + name
		}
		said = append(said, words)
	}
	return said
}

// UC-069 R1 (D1): «سجل المشروع» answers whoever holds SeeProjectHistory, which whoever starts a project holds, and refuses anyone else in it.
func TestR1_TheProjectHistoryOpensBySeeProjectHistoryWhichAProjectsStarterHolds(t *testing.T) {
	a := approved(t, started)
	if code, _ := a.Ask("GET", a.path+"/history", ``, a.token); code != 200 {
		t.Errorf("the starter: %d", code)
	}
	_, reader := a.member("قارئ", "ReadProject")
	if code, _ := a.Ask("GET", a.path+"/history", ``, reader); code != 403 {
		t.Errorf("someone without the permission: %d", code)
	}
}

// UC-069 R2 (D4): one request is one line, the line of the record it acted on: starting a project is its registration, not its permissions; an
// item is one line with its subgroup and columns; an expense one line with its details; a column added to an item on its own is a line.
func TestR2_OneRequestIsOneLineOfTheRecordItActedOn(t *testing.T) {
	a := approved(t, started)
	a.dated("2026-11-16")
	a.Ask("POST", a.path+"/items/"+a.item+"/columns", `{"name":"المقاس","dataType":"text","isRequired":false,"showInReports":true,"choices":[]}`, a.token)
	want := []string{"created column المقاس", "drafted entry 1", "created item التسليح", "created category الحديد", "created account حساب المؤسسة",
		"approved project مشروع A", "registered project مشروع A"}
	if got := a.said(a.path+"/history", a.token); !slices.Equal(got, want) {
		t.Errorf("the project's history:\n got %q\nwant %q", got, want)
	}
}

// UC-069 R3 (R8): a line shows only to whoever may see its record: someone else's draft never, a partner only with its capital in sight
// (BR-31), a permission only with ManageUsers.
func TestR3_ALineShowsOnlyToWhoeverMaySeeItsRecord(t *testing.T) {
	a := approved(t, started)
	a.dated("2026-11-16")
	saraID, sara := a.member("سارة", "ReadProject", "SeeProjectHistory")
	a.Ask("POST", a.path+"/users/"+saraID.String()+"/permissions/approveEntries", ``, a.token)
	a.Ask("POST", a.path+"/partners", `{"name":"زيد"}`, a.token)
	hidden := []string{"created partner زيد", "given permission approveEntries", "drafted entry 1"}
	manager, others := a.said(a.path+"/history", a.token), a.said(a.path+"/history", sara)
	for _, line := range hidden {
		if !slices.Contains(manager, line) {
			t.Errorf("the manager, who may see it, is not shown %q: %q", line, manager)
		}
		if slices.Contains(others, line) {
			t.Errorf("someone who may not see it is shown %q", line)
		}
	}
	if !slices.Contains(others, "created item التسليح") {
		t.Errorf("what everyone sees is missing: %q", others)
	}
}

// UC-069 R4 (D9): a line names its record from History's own values, so a renamed unit's line says its old name and a deleted unit's still
// names it.
func TestR4_ALineNamesItsRecordEvenAfterItIsDeleted(t *testing.T) {
	a := approved(t, started)
	a.Ask("POST", a.path+"/units", `{"name":"B3"}`, a.token)
	var unit string
	if err := a.Database.Read.QueryRow("SELECT id FROM units WHERE name = 'B3'").Scan(&unit); err != nil {
		t.Fatal(err)
	}
	a.Ask("PUT", a.path+"/units/"+unit, `{"name":"B4"}`, a.token)
	a.Ask("DELETE", a.path+"/units/"+unit, ``, a.token)
	lines := a.lines(a.path+"/history", a.token)
	if got := a.said(a.path+"/history", a.token)[:3]; !slices.Equal(got, []string{"deleted unit B4", "changed unit B4", "created unit B3"}) {
		t.Errorf("the unit's lines: %q", got)
	}
	if lines[1]["nameWas"] != "B3" {
		t.Errorf("the rename says its old name: %v", lines[1])
	}
}

// UC-069 R3, R4: a rejection's line says its reason; an entry deleted since shows only to whoever recorded it, since nothing else says whose
// it was to see.
func TestR3_ARejectionSaysItsReasonAndADeletedEntryShowsOnlyToItsRecorder(t *testing.T) {
	a := approved(t, started)
	a.dated("2026-11-16")
	_, sara := a.member("سارة", "ReadProject", "SeeProjectHistory")
	var entry string
	if err := a.Database.Read.QueryRow("SELECT id FROM entries").Scan(&entry); err != nil {
		t.Fatal(err)
	}
	a.Ask("POST", a.path+"/entries/"+entry+"/submit", ``, a.token)
	a.Ask("POST", a.path+"/entries/"+entry+"/reject", `{"reason":"الفاتورة غير واضحة"}`, a.token)
	if line := a.lines(a.path+"/history", a.token)[0]; line["action"] != "rejected" || line["about"] != "الفاتورة غير واضحة" {
		t.Errorf("the rejection: %v", line)
	}
	if !slices.Contains(a.said(a.path+"/history", sara), "rejected entry 1") {
		t.Errorf("a sent entry is everyone's to see: %q", a.said(a.path+"/history", sara))
	}
	a.Ask("DELETE", a.path+"/entries/"+entry, ``, a.token)
	if got := a.said(a.path+"/history", a.token)[0]; got != "deleted entry 1" {
		t.Errorf("the recorder sees the deletion: %q", got)
	}
	if slices.ContainsFunc(a.said(a.path+"/history", sara), func(line string) bool { return strings.HasSuffix(line, "entry 1") }) {
		t.Errorf("someone else sees a deleted entry: %q", a.said(a.path+"/history", sara))
	}
}

// UC-069 R5: a history keeps one kind of record, one person's lines, and those holding what was typed.
func TestR5_AHistoryIsFilteredByKindAndPersonAndSearched(t *testing.T) {
	a := approved(t, started)
	saraID, sara := a.member("سارة", "ReadProject", "SeeProjectHistory", "ManageProject")
	a.Ask("POST", a.path+"/units", `{"name":"B5"}`, sara)
	if got := a.said(a.path+"/history?area=accounts", a.token); !slices.Equal(got, []string{"created account حساب المؤسسة"}) {
		t.Errorf("the accounts alone: %q", got)
	}
	if got := a.said(a.path+"/history?by="+saraID.String(), a.token); !slices.Equal(got, []string{"created unit B5"}) {
		t.Errorf("Sara's alone: %q", got)
	}
	if got := a.said(a.path+"/history?text=الحديد", a.token); !slices.Equal(got, []string{"created category الحديد"}) {
		t.Errorf("what holds the typed word: %q", got)
	}
	if got := a.said(a.path+"/history?area=anything", a.token); len(got) != 6 {
		t.Errorf("any other kind keeps every kind: %q", got)
	}
}

// UC-069 R6 (R9, R14): «سجلي» is every line of one's own login, whoever wrote it, and never another's or a hash.
func TestR6_MyHistoryIsMyOwnLoginsLinesAlone(t *testing.T) {
	a := approved(t, started)
	saraID, _ := a.member("سارة", "ReadProject")
	a.member("علي", "ReadProject")
	a.Ask("POST", "/auth/sign-in", `{"username":"`+saraID.String()+`","password":"wrong-password-1"}`, ``)
	a.Ask("POST", "/auth/sign-in", `{"username":"`+saraID.String()+`","password":"Patrol-chosen-2"}`, ``)
	a.Ask("POST", "/users/"+saraID.String()+"/reset-password", `{"temporaryPassword":"Second-temp0rary-1"}`, a.token)
	lines := a.lines("/me/history", a.Token(saraID, false))
	var kinds []string
	for _, line := range lines {
		kinds = append(kinds, line["kind"].(string)+" by "+line["by"].(string))
	}
	if want := []string{"passwordReset by x", "signIn by سارة", "signInFailed by سارة"}; !slices.Equal(kinds, want) {
		t.Errorf("Sara's own history:\n got %q\nwant %q", kinds, want)
	}
	if password := a.lines("/me/history?area=password", a.Token(saraID, false)); len(password) != 1 || password[0]["kind"] != "passwordReset" {
		t.Errorf("the password's lines alone: %v", password)
	}
	if typed := a.lines("/me/history?text=x", a.Token(saraID, false)); len(typed) != 1 || typed[0]["by"] != "x" {
		t.Errorf("what holds the typed word: %v", typed)
	}
	if got := a.said(a.path+"/history", a.token); slices.ContainsFunc(got, func(line string) bool { return strings.Contains(line, "sign") }) {
		t.Errorf("a login's line in the project's history: %q", got)
	}
	if _, answer := a.Ask("GET", "/me/history", ``, a.token); len(answer.([]any)) != 0 {
		t.Errorf("the manager reads Sara's lines as their own: %v", answer)
	}
	sent := a.Send("GET", "/me/history", ``, "Authorization", "Bearer "+a.Token(saraID, false))
	if strings.Contains(sent.Body.String(), "Hash") || strings.Contains(sent.Body.String(), "$") {
		t.Errorf("a hash in my history: %s", sent.Body.String())
	}
}

// UC-069 R7 (R11, D3): every line keeps the trace id of the request that wrote it, so two requests in the same instant are two lines.
func TestR7_EveryLineKeepsTheTraceIdOfTheRequestThatWroteIt(t *testing.T) {
	a := approved(t, started)
	sent := a.Send("POST", a.path+"/units", `{"name":"B6"}`, "Authorization", "Bearer "+a.token, "traceparent", "00-11111111111111111111111111111111-2222222222222222-01")
	var kept string
	if err := a.Database.Read.QueryRow("SELECT trace_id FROM history WHERE table_name = 'Units' ORDER BY id DESC LIMIT 1").Scan(&kept); err != nil {
		t.Fatal(err)
	}
	if kept != sent.Header().Get("X-Trace-Id") || !strings.HasPrefix(kept, "00-11111111111111111111111111111111-") {
		t.Errorf("the line's trace id %q, the answer's %q", kept, sent.Header().Get("X-Trace-Id"))
	}
	a.Ask("POST", a.path+"/units", `{"name":"B7"}`, a.token)
	a.Ask("POST", a.path+"/units", `{"name":"B8"}`, a.token)
	if got := a.said(a.path+"/history", a.token)[:2]; !slices.Equal(got, []string{"created unit B8", "created unit B7"}) {
		t.Errorf("two requests at one instant: %q", got)
	}
}

// redix-api #87 R8: the project history is read a batch at a time, and its pages still hold every line the person may see, once each, newest
// first: a request of 701 lines spanning the end of a batch is one line, as its account's line stands for its custody lines; a page that begins
// inside a request leaves that request's custody lines hidden as the whole request does.
func TestR87_8_APagedHistoryShowsEveryLineOnceAcrossBatches(t *testing.T) {
	a := approved(t, started)
	base := len(a.everyPage(a.path + "/history"))
	a.longHistory()

	all := a.everyPage(a.path + "/history")
	for _, line := range all {
		if line["thing"] == "custody" {
			t.Errorf("a custody line shows although its request's account line stands for it: %v", line["id"])
		}
	}
	if want := base + 300 + 1 + 450 + 1 + 450; len(all) != want {
		t.Errorf("%d lines across the pages, want %d", len(all), want)
	}
	var mid int64
	if err := a.Database.Read.QueryRow(`SELECT max(id) FROM history WHERE trace_id = 'mid'`).Scan(&mid); err != nil {
		t.Fatal(err)
	}
	for _, line := range a.lines(a.path+"/history?before="+strconv.FormatInt(mid, 10), a.token) {
		if line["thing"] == "custody" {
			t.Errorf("a page beginning inside a request shows its custody line %v", line["id"])
		}
	}
}

// redix-api #87 R8: the partner history, read through its records' own lines a batch at a time, pages every line about its partners once,
// newest first, however many other lines lie between them.
func TestR87_8_APartnerHistoryPagesItsOwnLinesAcrossOtherLines(t *testing.T) {
	a := approved(t, started)
	a.Ask("POST", a.path+"/partners", `{"name":"زيد"}`, a.token)
	var partner string
	if err := a.Database.Read.QueryRow("SELECT id FROM partners WHERE name = 'زيد'").Scan(&partner); err != nil {
		t.Fatal(err)
	}
	base := len(a.everyPage(a.path + "/partners/history"))
	renamed := func(count int) {
		a.Exec(`WITH RECURSIVE n(i) AS (SELECT 1 UNION ALL SELECT i + 1 FROM n WHERE i < ?)
			INSERT INTO history (at, project_id, user_id, action, table_name, record_id, before, after, trace_id)
			SELECT made.at, made.project_id, made.user_id, 'Update', 'Partners', made.record_id, '{"Name":"زيد"}', '{"Name":"زيد"}', 'renamed-' || hex(randomblob(8))
			FROM (SELECT * FROM history WHERE table_name = 'Partners' AND record_id = ? LIMIT 1) AS made, n`, count, partner)
	}
	renamed(40)
	a.longHistory()
	renamed(40)
	a.longHistory()
	renamed(40)
	if got := len(a.everyPage(a.path + "/partners/history")); got != base+120 {
		t.Errorf("%d lines across the pages, want %d", got, base+120)
	}
}

// everyPage is every line of the history at from, page after page, each page after the last line of the one before; it fails the test when a
// line repeats or comes out of order (#87 R8).
func (a project) everyPage(from string) []map[string]any {
	a.t.Helper()
	var all []map[string]any
	for path := from; ; {
		page := a.lines(path, a.token)
		for _, line := range page {
			if len(all) > 0 && line["id"].(float64) >= all[len(all)-1]["id"].(float64) {
				a.t.Fatalf("line %v repeats or breaks the order after %v", line["id"], all[len(all)-1]["id"])
			}
			all = append(all, line)
		}
		if len(page) < 50 {
			return all
		}
		path = from + "?before=" + strconv.FormatInt(int64(page[len(page)-1]["id"].(float64)), 10)
	}
}

// longHistory writes 1,932 lines into the project's History, more than three batches: single requests around a request of 701 lines (an account's
// line and 700 custody lines) spanning a batch's end, and a request of 31 a page can begin inside, its account's line written last.
func (a project) longHistory() {
	a.t.Helper()
	project := strings.TrimPrefix(a.path, "/projects/")
	var manager string
	if err := a.Database.Read.QueryRow(`SELECT user_id FROM history WHERE project_id = ? LIMIT 1`, project).Scan(&manager); err != nil {
		a.t.Fatal(err)
	}
	write := func(count int, table, trace string) {
		a.Exec(`WITH RECURSIVE n(i) AS (SELECT 1 UNION ALL SELECT i + 1 FROM n WHERE i < ?)
			INSERT INTO history (at, project_id, user_id, action, table_name, record_id, before, after, trace_id)
			SELECT '2026-11-16T10:00:00.0000000Z', ?, ?, 'Update', ?, ?, '{}', '{"Name":"حساب المؤسسة","UserId":"`+manager+`"}',
				CASE WHEN ? = '' THEN 'single-' || hex(randomblob(8)) ELSE ? END FROM n`, count, project, manager, table, a.account, trace, trace)
	}
	write(300, "Accounts", "")
	write(1, "Accounts", "big")
	write(700, "AccountCustodians", "big")
	write(450, "Accounts", "")
	write(30, "AccountCustodians", "mid")
	write(1, "Accounts", "mid")
	write(450, "Accounts", "")
}

// redix-api #141 D1, #87: each history screen read a batch at a time answers 500 when the database fails at any of its calls - a batch, the rest
// of a request, what History knows of a record - and writes no History line.
func TestR87_AFailingDatabaseAtAnyCallOfAHistoryScreenAnswers500(t *testing.T) {
	a := approved(t, started)
	a.longHistory()
	for _, path := range []string{a.path + "/history", a.path + "/history?before=1500", a.path + "/accounts/history", a.path + "/partners/history",
		a.path + "/approvals/history", "/me/history"} {
		for n := int64(1); ; n++ {
			before := a.Count("SELECT count(*) FROM history")
			answered, failed := a.SendFailing(n, "GET", path, ``, "Authorization", "Bearer "+a.token)
			if !failed {
				break
			}
			if answered.Code != 500 || a.Count("SELECT count(*) FROM history") != before {
				t.Errorf("%s, the database failing at its call %d: %d", path, n, answered.Code)
			}
		}
	}
}
