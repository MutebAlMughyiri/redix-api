// Package historytest is the net under History (redix-api #140, Muteb's choice of 16 Nov 2026): around any write a test makes, it checks that
// every row the write added, changed or removed has a new History line naming its table and id, so a forgotten line fails a test instead of
// leaving a silent gap. It runs only in tests; the server pays nothing for it.
package historytest

import (
	"fmt"
	"slices"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

// exempt are the tables whose rows change with no History line, as in .NET, each with why.
var exempt = map[string]string{
	"devices":               "a session's device: signing in, renewing and signing out are recorded against the user (AuthEndpoints)",
	"email_codes":           "an emailed code is a means, not a record: the reset or the new email it leads to is recorded (EmailCode)",
	"history":               "History itself",
	"notifications":         "what someone is told is not a change to the books: what they are told of has its own line (NotificationsEndpoints)",
	"goose_db_version":      "the schema's own version",
	"sqlite_sequence":       "SQLite's own counter for History's ids",
	"user_default_projects": "the project a person opens straight into is their own choice, never recorded (ProjectsEndpoints.SetDefault)",
}

// createdWithTheirProject are the tables whose new rows .NET recorded only through their project's RegisterProject line: the units and partners a
// project is started with (Project.New, PartnersEndpoints.DeclaredWith). A new row of one passes when the same write registered its project.
var createdWithTheirProject = map[string]bool{"units": true, "partners": true}

// copiedOnApproval are the tables whose new rows .NET recorded only through the ApproveProject line of the project they were copied into: its
// structure, its payment methods and its general cash supplier (ProjectStructure.ApproveAsync). A new row of one passes when the same write
// approved a project.
var copiedOnApproval = map[string]bool{"default_columns": true, "categories": true, "items": true, "subgroups": true, "item_columns": true,
	"payment_methods": true, "suppliers": true}

// Check runs write, then fails t for each row of a table not exempt that write added, changed or removed with no new History line naming it.
func Check(t testing.TB, database db.DB, write func()) {
	t.Helper()
	tables := keptTables(t, database)
	before := map[string]map[string]row{}
	for _, table := range tables {
		before[table] = rowsOf(t, database, table)
	}
	var lastLine int64
	database.Read.QueryRow("SELECT COALESCE(MAX(id), 0) FROM history").Scan(&lastLine)

	write()

	lines, registered, approved := map[string]bool{}, map[string]bool{}, false
	found, err := database.Read.Query("SELECT table_name, record_id, action FROM history WHERE id > ?", lastLine)
	if err != nil {
		t.Fatal(err)
	}
	for found.Next() {
		var table, record, action string
		found.Scan(&table, &record, &action)
		lines[table+" "+record] = true
		if table == "Projects" && action == "RegisterProject" {
			registered[record] = true
		}
		approved = approved || (table == "Projects" && action == "ApproveProject")
	}
	found.Close()
	for _, table := range tables {
		after := rowsOf(t, database, table)
		for _, id := range changed(before[table], after) {
			_, existed := before[table][id]
			withItsProject := createdWithTheirProject[table] && !existed && registered[after[id].project]
			copied := copiedOnApproval[table] && !existed && approved
			if !lines[historyName(table)+" "+id] && !withItsProject && !copied {
				t.Errorf("History net: %s %s changed with no History line", table, id)
			}
		}
	}
}

// row is one row as the net compares it: the text of all its values, and its project when it has a project_id.
type row struct {
	text    string
	project string
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
func rowsOf(t testing.TB, database db.DB, table string) map[string]row {
	t.Helper()
	rows, err := database.Read.Query("SELECT * FROM " + table)
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	columns, _ := rows.Columns()
	project := slices.Index(columns, "project_id")
	kept := map[string]row{}
	for rows.Next() {
		values := make([]any, len(columns))
		pointers := make([]any, len(columns))
		for i := range values {
			pointers[i] = &values[i]
		}
		rows.Scan(pointers...)
		read := row{text: fmt.Sprint(values...)}
		if project >= 0 {
			read.project = fmt.Sprint(values[project])
		}
		kept[fmt.Sprint(values[0])] = read
	}
	return kept
}

func changed(before, after map[string]row) []string {
	var ids []string
	for id, now := range after {
		if before[id].text != now.text {
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
