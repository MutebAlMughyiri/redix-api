// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

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
	"devices":               "a session's device: signing in, renewing and signing out are recorded against the user (AuthEndpoints)",
	"email_codes":           "an emailed code is a means, not a record: the reset or the new email it leads to is recorded (EmailCode)",
	"history":               "History itself",
	"notification_entries":  "the entry a notification opens, told with it",
	"notifications":         "what someone is told is not a change to the books: what they are told of has its own line (NotificationsEndpoints)",
	"goose_db_version":      "the schema's own version",
	"sqlite_sequence":       "SQLite's own counter for History's ids",
	"user_default_projects": "the project a person opens straight into is their own choice, never recorded (ProjectsEndpoints.SetDefault)",
}

// owner is the record whose History line also stands for a row of another table, and the column of that row naming it.
type owner struct {
	table  string
	column string
}

// recordedWithTheirOwner are the tables whose rows .NET recorded, as they were added or removed, only through the line of a record owning them:
// the units and partners a project is started with, under its RegisterProject line (Project.New, PartnersEndpoints.DeclaredWith), and an account's
// custodian, under the account's line (AccountsEndpoints.Add, Delete); and what each account held at a reconciliation, under its line
// (ReconciliationsEndpoints.EnterAsync); and which accounts a payment method pays from, under the line of the method or of the account added or
// removed with them, or the method's own line when what it pays from changes (redix-app #109). A row of one added or removed passes when the same
// write recorded one of its owners; a row changed still needs its own line.
var recordedWithTheirOwner = map[string][]owner{
	"units":                   {{"Projects", "project_id"}},
	"partners":                {{"Projects", "project_id"}},
	"account_custodians":      {{"Accounts", "account_id"}},
	"reconciliation_balances": {{"Reconciliations", "reconciliation_id"}},
	"entry_decisions":         {{"Entries", "entry_id"}},
	"partner_users":           {{"Partners", "partner_id"}},
	"payment_method_accounts": {{"PaymentMethods", "payment_method_id"}, {"Accounts", "account_id"}},
}

// counters are the columns that move with no History line of their own: a project's last entry number, which each entry's own line carries
// (Entry.Number, FR-54).
var counters = map[string]string{"projects": "last_entry_number"}

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

	lines, approved, retrieved := map[string]bool{}, false, false
	found, err := database.Read.Query("SELECT table_name, record_id, action FROM history WHERE id > ?", lastLine)
	if err != nil {
		t.Fatal(err)
	}
	for found.Next() {
		var table, record, action string
		found.Scan(&table, &record, &action)
		lines[table+" "+record] = true
		approved = approved || (table == "Projects" && action == "ApproveProject")
		// A retrieve puts back thousands of rows under one Retrieve line on the project: the archive it puts back is each row's record, so a row
		// added, changed or removed passes when the same write recorded one (UC-073 D9).
		retrieved = retrieved || (table == "Projects" && action == "Retrieve")
	}
	found.Close()
	for _, table := range tables {
		after := rowsOf(t, database, table)
		for _, id := range changed(before[table], after) {
			was, existed := before[table][id]
			now, kept := after[id]
			withItsOwner := false
			for _, owned := range recordedWithTheirOwner[table] {
				withItsOwner = withItsOwner || !existed && lines[owned.table+" "+now.values[owned.column]] || !kept && lines[owned.table+" "+was.values[owned.column]]
			}
			copied := copiedOnApproval[table] && !existed && approved
			record := now.record
			if !kept {
				record = was.record
			}
			if !lines[historyName(table)+" "+record] && !withItsOwner && !copied && !retrieved {
				t.Errorf("History net: %s %s changed with no History line", table, id)
			}
		}
	}
}

// row is one row as the net compares it: the text of all its values, each value by its column, and the record its own History line names - the
// first column of its key, as .NET named a link's line by the row it links from.
type row struct {
	text   string
	values map[string]string
	record string
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

// rowsOf is every row of table by its primary key - its id, or the pair of ids a link table is keyed by - as the text of all its values.
func rowsOf(t testing.TB, database db.DB, table string) map[string]row {
	t.Helper()
	key := keyOf(t, database, table)
	rows, err := database.Read.Query("SELECT * FROM " + table)
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	columns, _ := rows.Columns()
	if len(key) == 0 {
		key = columns[:1]
	}
	kept := map[string]row{}
	for rows.Next() {
		values := make([]any, len(columns))
		pointers := make([]any, len(columns))
		for i := range values {
			pointers[i] = &values[i]
		}
		rows.Scan(pointers...)
		read := row{values: map[string]string{}}
		for i, column := range columns {
			read.values[column] = fmt.Sprint(values[i])
			if counters[table] != column {
				read.text += fmt.Sprint(values[i]) + "|"
			}
		}
		parts := make([]string, 0, len(key))
		for _, column := range key {
			parts = append(parts, read.values[column])
		}
		read.record = parts[0]
		kept[strings.Join(parts, " ")] = read
	}
	return kept
}

// keyOf is the columns of table's primary key, in their order.
func keyOf(t testing.TB, database db.DB, table string) []string {
	t.Helper()
	rows, err := database.Read.Query("SELECT name FROM pragma_table_info(?) WHERE pk > 0 ORDER BY pk", table)
	if err != nil {
		t.Fatal(err)
	}
	defer rows.Close()
	var key []string
	for rows.Next() {
		var column string
		rows.Scan(&column)
		key = append(key, column)
	}
	return key
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
