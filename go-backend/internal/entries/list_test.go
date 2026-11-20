// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries_test

import (
	"slices"
	"strconv"
	"strings"
	"testing"
)

// manyEntries writes 1,200 approved purchases into the project, over three batches of the list, most of them sharing their moment with others
// (only three moments among them), so the list's order rests on the number as much as on the moment.
func (a project) manyEntries() {
	a.t.Helper()
	project := strings.TrimPrefix(a.path, "/projects/")
	var manager string
	if err := a.Database.Read.QueryRow(`SELECT user_id FROM history WHERE project_id = ? LIMIT 1`, project).Scan(&manager); err != nil {
		a.t.Fatal(err)
	}
	a.Exec(`WITH RECURSIVE n(i) AS (SELECT 1 UNION ALL SELECT i + 1 FROM n WHERE i < 1200)
		INSERT INTO entries (id, project_id, kind, status, entry_date, amount, account_id, description, created_by_user_id, created_at, approved_at,
			column_values, number)
		SELECT lower(hex(randomblob(4))) || '-0000-7000-8000-' || lower(hex(randomblob(6))), ?, 0, 2, '2026-11-16', 10000, ?, '', ?,
			'2026-11-16T10:0' || (i % 3) || ':00.0000000Z', '2026-11-16T10:00:00.0000000Z', '{}', 1000 + i FROM n`, project, a.account, manager)
}

// redix-api #87 R6, R7: the entries list reads 500 entries at a time with that batch's facts, and answers as it did reading every entry: every
// entry once, newest first and then by number, across batches whose ends fall inside a moment; and take stops anywhere, at a batch's end too.
func TestR87_6_TheEntriesListReadInBatchesKeepsItsOrderAndItsTake(t *testing.T) {
	a := approved(t, started)
	a.manyEntries()
	rows, err := a.Database.Read.Query(`SELECT id FROM entries WHERE project_id = ? AND status <> 4 ORDER BY created_at DESC, number DESC`,
		strings.TrimPrefix(a.path, "/projects/"))
	if err != nil {
		t.Fatal(err)
	}
	var want []string
	for rows.Next() {
		var id string
		if err := rows.Scan(&id); err != nil {
			t.Fatal(err)
		}
		want = append(want, id)
	}
	if err := rows.Close(); err != nil {
		t.Fatal(err)
	}
	listed := func(query string) []string {
		var ids []string
		for _, line := range a.lines(a.path+"/entries"+query, a.token) {
			ids = append(ids, line["id"].(string))
		}
		return ids
	}
	if got := listed(""); !slices.Equal(got, want) {
		t.Fatalf("every entry: %d listed, %d in the project in that order", len(got), len(want))
	}
	for _, take := range []int{1, 499, 500, 501, 1000, 5000} {
		if got := listed("?take=" + strconv.Itoa(take)); !slices.Equal(got, want[:min(take, len(want))]) {
			t.Errorf("take %d: %d listed, not the newest %d", take, len(got), min(take, len(want)))
		}
	}
}

// redix-api #141 D1, #87: the entries lists read a batch at a time answer 500 and write no History line when the database fails at any of their
// calls - a batch, its facts, its rejections, what every batch shares.
func TestR87_AFailingDatabaseAtAnyCallOfABatchedListAnswers500(t *testing.T) {
	a := approved(t, started)
	a.manyEntries()
	a.failsAtEveryCall(a.path+"/entries?take=700", a.path+"/entries/mine")
}
