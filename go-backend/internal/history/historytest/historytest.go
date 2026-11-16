// Package historytest is the net under History (redix-api #140, Muteb's choice of 16 Nov 2026): around any write a test makes, it checks that
// every row the write added, changed or removed has a new History line naming its table and id, so a forgotten line fails a test instead of
// leaving a silent gap. It runs only in tests; the server pays nothing for it.
package historytest

import (
	"fmt"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

// exempt are the tables whose rows change with no History line, as in .NET, each with why.
var exempt = map[string]string{
	"devices":          "a session's device: signing in, renewing and signing out are recorded against the user (AuthEndpoints)",
	"email_codes":      "an emailed code is a means, not a record: the reset or the new email it leads to is recorded (EmailCode)",
	"history":          "History itself",
	"goose_db_version": "the schema's own version",
	"sqlite_sequence":  "SQLite's own counter for History's ids",
}

// Check runs write, then fails t for each row of a table not exempt that write added, changed or removed with no new History line naming it.
func Check(t testing.TB, database db.DB, write func()) {
	t.Helper()
	tables := keptTables(t, database)
	before := map[string]map[string]string{}
	for _, table := range tables {
		before[table] = rowsOf(t, database, table)
	}
	var lastLine int64
	database.Read.QueryRow("SELECT COALESCE(MAX(id), 0) FROM history").Scan(&lastLine)

	write()

	lines := map[string]bool{}
	found, err := database.Read.Query("SELECT table_name, record_id FROM history WHERE id > ?", lastLine)
	if err != nil {
		t.Fatal(err)
	}
	for found.Next() {
		var table, record string
		found.Scan(&table, &record)
		lines[table+" "+record] = true
	}
	found.Close()
	for _, table := range tables {
		after := rowsOf(t, database, table)
		for _, id := range changed(before[table], after) {
			if !lines[historyName(table)+" "+id] {
				t.Errorf("History net: %s %s changed with no History line", table, id)
			}
		}
	}
}

func keptTables(t testing.TB, database db.DB) []string {
	t.Helper()
	rows, err := database.Read.Query("SELECT name FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'lk\\_%' ESCAPE '\\'")
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	var tables []string
	for rows.Next() {
		var name string
		rows.Scan(&name)
		if _, skip := exempt[name]; !skip {
			tables = append(tables, name)
		}
	}
	return tables
}

// rowsOf is every row of table by its first column, its id, as the text of all its values.
func rowsOf(t testing.TB, database db.DB, table string) map[string]string {
	t.Helper()
	rows, err := database.Read.Query("SELECT * FROM " + table)
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	columns, _ := rows.Columns()
	kept := map[string]string{}
	for rows.Next() {
		values := make([]any, len(columns))
		pointers := make([]any, len(columns))
		for i := range values {
			pointers[i] = &values[i]
		}
		rows.Scan(pointers...)
		kept[fmt.Sprint(values[0])] = fmt.Sprint(values...)
	}
	return kept
}

func changed(before, after map[string]string) []string {
	var ids []string
	for id, row := range after {
		if before[id] != row {
			ids = append(ids, id)
		}
	}
	for id := range before {
		if _, kept := after[id]; !kept {
			ids = append(ids, id)
		}
	}
	return ids
}

// historyName is a table's name as History keeps it, the name .NET gave it: project_permissions is ProjectPermissions.
func historyName(table string) string {
	parts := strings.Split(table, "_")
	for i, part := range parts {
		parts[i] = strings.ToUpper(part[:1]) + part[1:]
	}
	return strings.Join(parts, "")
}
