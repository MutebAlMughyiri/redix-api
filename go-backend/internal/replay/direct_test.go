// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package replay

import (
	"bufio"
	"bytes"
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"os"
	"slices"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

// directRow is a row a test wrote straight into .NET's database, as the recorder kept it: its table as schema.Name, whether it was added,
// modified or deleted, and each column's value as the SQLite schema keeps it.
type directRow struct {
	Table  string                     `json:"table"`
	State  string                     `json:"state"`
	Values map[string]json.RawMessage `json:"values"`
}

// sqlServerOwn are the test classes about SQL Server itself - its migrations' steps, its lookup tables' keys - whose writes have no SQLite twin;
// each is ported by hand.
var sqlServerOwn = []string{"MigrationStepsTests", "LookupTablesTests"}

// replayable says whether every direct write of the test is one the replay can make again: rows the recorder kept, in a test not about SQL
// Server's own workings.
func replayable(name string, steps []step) bool {
	if !slices.ContainsFunc(steps, func(s step) bool { return s.Step == "direct" }) {
		return true
	}
	return !slices.Contains(sqlServerOwn, strings.SplitN(name, ".", 2)[0]) && !slices.ContainsFunc(steps, func(s step) bool { return s.Step == "direct" && s.Rows == nil })
}

// applyDirect writes s's rows into the replay's database as the test wrote them into .NET's, each .NET id among them made Go's.
func applyDirect(t *testing.T, database db.DB, names mapping, s step) {
	t.Helper()
	sources, err := sourcesOf("../db/migrations/00001_schema.sql")
	if err != nil {
		t.Fatal(err)
	}
	err = database.Load(context.Background(), func(tx *sql.Tx) error {
		for _, row := range s.Rows {
			at := slices.IndexFunc(sources, func(source source) bool { return source.Schema+"."+source.Name == row.Table })
			if at < 0 {
				return fmt.Errorf("%s has no table", row.Table)
			}
			if err := writeRow(tx, names, sources[at].Table, row); err != nil {
				return fmt.Errorf("%s: %w", row.Table, err)
			}
		}
		return nil
	})
	if err != nil {
		t.Fatalf("direct write: %v", err)
	}
}

// writeRow adds, changes or removes row in table, its columns matched by name without underscores or case, Group as group_name; the rowversion
// SQLite has no place for is left out (N5), and so is a whole-number key EF had not been given yet - zero or its temporary negative - which SQLite
// numbers as SQL Server did. A row to remove that is already gone - the request before it removed it - is the state the write wanted.
func writeRow(tx *sql.Tx, names mapping, table string, row directRow) error {
	columns, keys, err := columnsOfTable(tx, table)
	if err != nil {
		return err
	}
	values := map[string]any{}
	for source, raw := range row.Values {
		if source == "RowVersion" {
			continue
		}
		at := slices.IndexFunc(columns, func(column string) bool {
			return plainName(column) == plainName(source) || (column == "group_name" && source == "Group")
		})
		if at < 0 {
			return fmt.Errorf("the column %s has no place", source)
		}
		value, err := valueOf(names, raw)
		if err != nil {
			return err
		}
		values[columns[at]] = value
	}
	switch row.State {
	case "added":
		var named []string
		var given []any
		for _, column := range columns {
			if number, isNumber := values[column].(int64); isNumber && number <= 0 && slices.Contains(keys, column) {
				continue
			}
			if value, held := values[column]; held {
				named = append(named, column)
				given = append(given, value)
			}
		}
		_, err = tx.Exec(fmt.Sprintf("INSERT INTO %s (%s) VALUES (%s)", table, strings.Join(named, ", "), strings.TrimSuffix(strings.Repeat("?, ", len(named)), ", ")),
			given...)
		return err
	case "modified", "deleted":
		where, keyValues, err := locate(tx, names, table, keys, values, row.Values)
		if err != nil && row.State == "deleted" {
			return nil
		}
		if err != nil {
			return err
		}
		if row.State == "deleted" {
			_, err = tx.Exec(fmt.Sprintf("DELETE FROM %s WHERE %s", table, where), keyValues...)
			return err
		}
		var sets []string
		var given []any
		for _, column := range columns {
			if value, held := values[column]; held && !slices.Contains(keys, column) {
				sets = append(sets, column+" = ?")
				given = append(given, value)
			}
		}
		_, err = tx.Exec(fmt.Sprintf("UPDATE %s SET %s WHERE %s", table, strings.Join(sets, ", "), where), append(given, keyValues...)...)
		return err
	}
	return fmt.Errorf("a row %s", row.State)
}

// locate is the condition that finds row's one row in table: by its key, or - when the key holds an id of .NET's no answer showed, as a column
// the approval copied - by a unique index whose every column the row names, after which the replay knows the two ids as one. A row neither finds
// stops the replay, so a write never matches nothing unnoticed.
func locate(tx *sql.Tx, names mapping, table string, keys []string, values map[string]any, recorded map[string]json.RawMessage) (string, []any, error) {
	candidates := [][]string{keys}
	indexes, err := tx.Query("SELECT name FROM pragma_index_list(?) WHERE \"unique\" = 1 AND origin <> 'pk'", table)
	if err != nil {
		return "", nil, err
	}
	var indexNames []string
	for indexes.Next() {
		var name string
		if err := indexes.Scan(&name); err != nil {
			_ = indexes.Close()
			return "", nil, err
		}
		indexNames = append(indexNames, name)
	}
	if err := indexes.Close(); err != nil {
		return "", nil, err
	}
	for _, index := range indexNames {
		columns, err := tx.Query("SELECT name FROM pragma_index_info(?) ORDER BY seqno", index)
		if err != nil {
			return "", nil, err
		}
		var unique []string
		for columns.Next() {
			var name string
			if err := columns.Scan(&name); err != nil {
				_ = columns.Close()
				return "", nil, err
			}
			unique = append(unique, name)
		}
		if err := columns.Close(); err != nil {
			return "", nil, err
		}
		candidates = append(candidates, unique)
	}
	for _, candidate := range candidates {
		var where []string
		var args []any
		for _, column := range candidate {
			value, held := values[column]
			if !held || value == nil {
				where = nil
				break
			}
			where = append(where, column+" = ?")
			args = append(args, value)
		}
		if len(where) == 0 {
			continue
		}
		condition := strings.Join(where, " AND ")
		found, err := tx.Query(fmt.Sprintf("SELECT %s FROM %s WHERE %s", strings.Join(keys, ", "), table, condition), args...)
		if err != nil {
			return "", nil, err
		}
		var rows [][]any
		for found.Next() {
			key := make([]any, len(keys))
			pointers := make([]any, len(keys))
			for index := range key {
				pointers[index] = &key[index]
			}
			if err := found.Scan(pointers...); err != nil {
				_ = found.Close()
				return "", nil, err
			}
			rows = append(rows, key)
		}
		if err := found.Close(); err != nil {
			return "", nil, err
		}
		if len(rows) != 1 {
			continue
		}
		for index, key := range keys {
			var netValue string
			if json.Unmarshal(recorded[sourceOf(recorded, key)], &netValue) == nil {
				if goValue, isText := rows[0][index].(string); isText && goValue != netValue {
					names[netValue] = goValue
				}
			}
		}
		return condition, args, nil
	}
	return "", nil, fmt.Errorf("no row of %s is the one written", table)
}

// sourceOf is the recorded column a SQLite column was written from.
func sourceOf(recorded map[string]json.RawMessage, column string) string {
	for source := range recorded {
		if plainName(source) == plainName(column) || (column == "group_name" && source == "Group") {
			return source
		}
	}
	return ""
}

// columnsOfTable is the table's columns and its key's.
func columnsOfTable(tx *sql.Tx, table string) ([]string, []string, error) {
	rows, err := tx.Query("SELECT name, pk FROM pragma_table_info(?) ORDER BY cid", table)
	if err != nil {
		return nil, nil, err
	}
	defer func() { _ = rows.Close() }()
	var columns []string
	keys := map[int]string{}
	for rows.Next() {
		var name string
		var key int
		if err := rows.Scan(&name, &key); err != nil {
			return nil, nil, err
		}
		columns = append(columns, name)
		if key > 0 {
			keys[key] = name
		}
	}
	ordered := make([]string, len(keys))
	for place, name := range keys {
		ordered[place-1] = name
	}
	return columns, ordered, rows.Err()
}

func plainName(name string) string { return strings.ToLower(strings.ReplaceAll(name, "_", "")) }

// valueOf is a recorded value as SQLite takes it: a text with each .NET id made Go's, a whole number as an integer, null as NULL.
func valueOf(names mapping, raw json.RawMessage) (any, error) {
	decoder := json.NewDecoder(bytes.NewReader(raw))
	decoder.UseNumber()
	var value any
	if err := decoder.Decode(&value); err != nil {
		return nil, err
	}
	switch typed := value.(type) {
	case string:
		return names.into(typed), nil
	case json.Number:
		if whole, err := typed.Int64(); err == nil {
			return whole, nil
		}
		return typed.Float64()
	}
	return value, nil
}

// source is a table of the schema and the SQL Server table .NET kept it in, schema and name (redix-api #140 R19): what a row .NET's tests wrote
// is found by.
type source struct {
	Table, Schema, Name string
}

// sourcesOf is every table of the schema at path with the SQL Server table it was generated from, read from the line naming it above each
// CREATE TABLE.
func sourcesOf(path string) ([]source, error) {
	schema, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	var sources []source
	named := ""
	lines := bufio.NewScanner(strings.NewReader(string(schema)))
	for lines.Scan() {
		line := lines.Text()
		if name, isSource := strings.CutPrefix(line, "-- "); isSource && strings.Count(name, ".") == 1 && !strings.Contains(name, " ") {
			named = name
			continue
		}
		if table, isTable := strings.CutPrefix(line, "CREATE TABLE "); isTable && named != "" {
			schemaName, name, _ := strings.Cut(named, ".")
			sources = append(sources, source{Table: strings.TrimSuffix(table, " ("), Schema: schemaName, Name: name})
		}
		named = ""
	}
	return sources, lines.Err()
}
