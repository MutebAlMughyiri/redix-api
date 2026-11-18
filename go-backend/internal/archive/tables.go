// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package archive keeps a whole project in one file and puts it back (UC-073, redix-api #130): «أرشفة المشروع» hands over every row of the
// project as stored, with its files; «استرجاع المشروع» makes the project's own rows the archive's.
package archive

import (
	"context"
	"database/sql"
	"encoding/json"
	"strings"

	"github.com/google/uuid"
)

// table is one table holding a project's rows: its name, the condition finding the project's rows in it, and whether a retrieve replaces them
// (AC-73.3) or leaves them as they are (AC-73.4).
type table struct {
	name      string
	scope     string
	retrieved bool
}

// The conditions finding a project's rows, :project being its id.
const (
	ofProject = "project_id = :project"
	ofItem    = "item_id IN (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = :project)"
	ofEntry   = "entry_id IN (SELECT id FROM entries WHERE project_id = :project)"
)

// tables are every table holding a project's rows, each after the tables its rows name: an archive writes them all, and a retrieve empties the
// retrieved ones in the reverse order, then fills them in this one (D5). What stays is what records what happened, History and the issued
// reports, and the project's own row with who belongs to it: a removed member is never let back in by an old archive (R9), and the entry
// counter never gives a number twice (R10).
var tables = []table{
	{name: "projects", scope: "id = :project"},
	{name: "project_time_zones", scope: ofProject},
	{name: "project_structure_sources", scope: ofProject},
	{name: "project_permissions", scope: ofProject},
	{name: "project_role_names", scope: ofProject},
	{name: "inactive_members", scope: ofProject},
	{name: "units", scope: ofProject, retrieved: true},
	{name: "categories", scope: ofProject, retrieved: true},
	{name: "items", scope: "category_id IN (SELECT id FROM categories WHERE project_id = :project)", retrieved: true},
	{name: "subgroups", scope: ofItem, retrieved: true},
	{name: "item_columns", scope: ofItem, retrieved: true},
	{name: "default_columns", scope: ofProject, retrieved: true},
	{name: "accounts", scope: ofProject, retrieved: true},
	{name: "account_custodians", scope: "account_id IN (SELECT id FROM accounts WHERE project_id = :project)", retrieved: true},
	{name: "payment_methods", scope: ofProject, retrieved: true},
	{name: "payment_method_accounts", scope: "payment_method_id IN (SELECT id FROM payment_methods WHERE project_id = :project)", retrieved: true},
	{name: "suppliers", scope: ofProject, retrieved: true},
	{name: "supplier_default_items", scope: "supplier_id IN (SELECT id FROM suppliers WHERE project_id = :project)", retrieved: true},
	{name: "partners", scope: ofProject, retrieved: true},
	{name: "partner_users", scope: ofProject, retrieved: true},
	{name: "chosen_partners", scope: ofProject, retrieved: true},
	{name: "contracts", scope: ofProject, retrieved: true},
	{name: "attachments", scope: ofProject, retrieved: true},
	{name: "contract_attachments", scope: "contract_id IN (SELECT id FROM contracts WHERE project_id = :project)", retrieved: true},
	{name: "documents", scope: ofProject, retrieved: true},
	{name: "entries", scope: ofProject, retrieved: true},
	{name: "expense_details", scope: ofEntry, retrieved: true},
	{name: "purchase_details", scope: ofEntry, retrieved: true},
	{name: "revenue_details", scope: ofEntry, retrieved: true},
	{name: "contractor_payment_details", scope: ofEntry, retrieved: true},
	{name: "capital_movement_details", scope: ofEntry, retrieved: true},
	{name: "invoices", scope: ofEntry, retrieved: true},
	{name: "entry_attachments", scope: ofEntry, retrieved: true},
	{name: "entry_decisions", scope: ofEntry, retrieved: true},
	{name: "transfers", scope: ofProject, retrieved: true},
	{name: "reconciliations", scope: ofProject, retrieved: true},
	{name: "reconciliation_balances", scope: "reconciliation_id IN (SELECT id FROM reconciliations WHERE project_id = :project)", retrieved: true},
	{name: "notifications", scope: ofProject, retrieved: true},
	{name: "notification_entries", scope: "notification_id IN (SELECT id FROM notifications WHERE project_id = :project)", retrieved: true},
	{name: "periodic_reports", scope: ofProject},
	{name: "history", scope: ofProject},
}

// shared are the tables holding no project's rows, each with why: with them and the lookups, lk_, tables names every table of the database
// (AC-73.7).
var shared = map[string]string{
	"users":                 "a person signs in once, whatever projects they belong to",
	"devices":               "a person's signed-in phones",
	"email_codes":           "a code emailed to a person",
	"system_permissions":    "what a person holds everywhere",
	"user_default_projects": "the project a person opens straight into, their own choice",
	"config_values":         "the server's settings",
	"translation_keys":      "the words",
	"translations":          "the words",
	"goose_db_version":      "the schema's own version",
	"sqlite_sequence":       "SQLite's own counter of History's ids",
}

// retrieved are the tables a retrieve replaces, in tables' order.
var retrieved = func() []table {
	var replaced []table
	for _, t := range tables {
		if t.retrieved {
			replaced = append(replaced, t)
		}
	}
	return replaced
}()

// tableNamed is the table of tables called name.
func tableNamed(name string) (table, bool) {
	for _, t := range tables {
		if t.name == name {
			return t, true
		}
	}
	return table{}, false
}

// row is one row by its columns' names, each value as stored: text, or a whole number (D4).
type row map[string]any

// querier is what reads rows: a database, or one of its transactions.
type querier interface {
	QueryContext(ctx context.Context, query string, args ...any) (*sql.Rows, error)
}

// rowsOf is every row t holds of projectID, in the order they were written.
func rowsOf(ctx context.Context, q querier, t table, projectID uuid.UUID) ([]row, error) {
	found, err := q.QueryContext(ctx, "SELECT * FROM "+t.name+" WHERE "+t.scope+" ORDER BY rowid", sql.Named("project", projectID))
	if err != nil {
		return nil, err
	}
	defer found.Close()
	columns, _ := found.Columns()
	rows := []row{}
	for found.Next() {
		values := make([]any, len(columns))
		pointers := make([]any, len(columns))
		for i := range values {
			pointers[i] = &values[i]
		}
		if err := found.Scan(pointers...); err != nil {
			return nil, err
		}
		kept := row{}
		for i, column := range columns {
			kept[column] = values[i]
		}
		rows = append(rows, kept)
	}
	return rows, found.Err()
}

// columnsOf is every column of table by its name, with whether it keeps whole numbers, read from the schema itself.
func columnsOf(ctx context.Context, q querier, table string) (map[string]bool, error) {
	found, err := q.QueryContext(ctx, "SELECT name, type FROM pragma_table_info(?)", table)
	if err != nil {
		return nil, err
	}
	defer found.Close()
	columns := map[string]bool{}
	for found.Next() {
		var name, kind string
		if err := found.Scan(&name, &kind); err != nil {
			return nil, err
		}
		columns[name] = kind == "INTEGER"
	}
	return columns, found.Err()
}

// insertOf is the statement putting r into table, and its values, when every name of r is one of columns and every value suits its column: only
// names the schema gives reach the statement, and values go as its parameters (G8). A number read from the file is whole or refused.
func insertOf(table string, r row, columns map[string]bool) (string, []any, bool) {
	var names, marks []string
	var values []any
	for name, value := range r {
		whole, known := columns[name]
		if !known {
			return "", nil, false
		}
		if number, read := value.(json.Number); read {
			integer, err := number.Int64()
			if err != nil {
				return "", nil, false
			}
			value = integer
		}
		switch value.(type) {
		case int64:
			if !whole {
				return "", nil, false
			}
		case string:
			if whole {
				return "", nil, false
			}
		default:
			return "", nil, false
		}
		names, marks, values = append(names, `"`+name+`"`), append(marks, "?"), append(values, value)
	}
	if len(names) == 0 {
		return "", nil, false
	}
	return "INSERT INTO " + table + " (" + strings.Join(names, ", ") + ") VALUES (" + strings.Join(marks, ", ") + ")", values, true
}
