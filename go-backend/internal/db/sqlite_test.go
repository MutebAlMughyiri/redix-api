// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"context"
	"errors"
	"path/filepath"
	"strings"
	"testing"
)

func open(t *testing.T) DB {
	t.Helper()
	db, err := Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { _ = db.Close() })
	return db
}

func count(t *testing.T, db DB, query string) int {
	t.Helper()
	var n int
	if err := db.Read.QueryRow(query).Scan(&n); err != nil {
		t.Fatal(err)
	}
	return n
}

func TestR19_ANewDatabaseHoldsEveryTableIndexAndTrigger(t *testing.T) {
	db := open(t)
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> 'goose_db_version'"); got != 66 {
		t.Errorf("tables: %d, want 66", got)
	}
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'index' AND sql IS NOT NULL"); got != 117 {
		t.Errorf("indexes: %d, want 117", got)
	}
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> 'goose_db_version' AND sql NOT LIKE '%) STRICT'"); got != 0 {
		t.Errorf("tables not STRICT: %d", got)
	}
}

// redix-api #87 R5: every foreign key leads an index, so a join or a delete through it never reads the whole table, and every whole-number column
// named for an enum (a kind, status, type, action, role or purpose) keys its lookup table. A key or an enum column added without them fails here.
func TestR87_5_EveryForeignKeyLeadsAnIndexAndEveryEnumColumnKeysItsLookupTable(t *testing.T) {
	db := open(t)
	for query, problem := range map[string]string{
		`SELECT m.name || '.' || f."from" FROM sqlite_schema m, pragma_foreign_key_list(m.name) f WHERE m.type = 'table' AND NOT EXISTS (
			SELECT 1 FROM pragma_index_list(m.name) l, pragma_index_info(l.name) i WHERE i.seqno = 0 AND i.name = f."from")`: "a foreign key no index leads with",
		`SELECT m.name || '.' || c.name FROM sqlite_schema m, pragma_table_info(m.name) c WHERE m.type = 'table' AND m.name NOT LIKE 'lk_%'
			AND c.type = 'INTEGER' AND (c.name LIKE '%kind' OR c.name LIKE '%status' OR c.name LIKE '%type' OR c.name LIKE '%action'
			OR c.name LIKE '%role' OR c.name LIKE '%purpose') AND NOT EXISTS (
			SELECT 1 FROM pragma_foreign_key_list(m.name) f WHERE f."from" = c.name AND f."table" LIKE 'lk_%')`: "an enum column with no lookup table",
	} {
		rows, err := db.Read.Query(query)
		if err != nil {
			t.Fatal(err)
		}
		for rows.Next() {
			var column string
			if err := rows.Scan(&column); err != nil {
				t.Fatal(err)
			}
			t.Errorf("%s: %s", problem, column)
		}
		if err := errors.Join(rows.Err(), rows.Close()); err != nil {
			t.Fatal(err)
		}
	}
}

func TestR31_ANewDatabaseStartsWithItsWordsAndLookups(t *testing.T) {
	db := open(t)
	if got := count(t, db, "SELECT count(*) FROM translations"); got != 2008 {
		t.Errorf("words: %d, want 2008", got)
	}
	if got := count(t, db, "SELECT count(*) FROM lk_entry_kind"); got != 5 {
		t.Errorf("entry kinds: %d, want 5", got)
	}
}

func TestR12_EveryConnectionKeepsThePragmas(t *testing.T) {
	db := open(t)
	var mode string
	if err := db.Read.QueryRow("PRAGMA journal_mode").Scan(&mode); err != nil || mode != "wal" {
		t.Errorf("journal_mode: %q %v", mode, err)
	}
	for pragma, want := range map[string]int{"foreign_keys": 1, "busy_timeout": 5000, "synchronous": 1} {
		if got := count(t, db, "PRAGMA "+pragma); got != want {
			t.Errorf("%s: %d, want %d", pragma, got, want)
		}
	}
}

func TestR13_TheReadPoolCannotWrite(t *testing.T) {
	db := open(t)
	if _, err := db.Read.Exec("DELETE FROM config_values"); err == nil {
		t.Error("the read pool deleted rows")
	}
}

func TestAC21_1_HistoryIsNeverChangedOrDeleted(t *testing.T) {
	db := open(t)
	ctx := context.Background()
	for _, setUp := range []string{
		`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at)
			VALUES ('0193a1b2-0000-7000-8000-000000000001', 'dev-admin', 'Dev', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-16T10:00:00.0000000Z')`,
		`INSERT INTO history (at, project_id, user_id, action, table_name, record_id, before, after)
			VALUES ('2026-11-16T10:00:00.0000000Z', '00000000-0000-0000-0000-000000000000', '0193a1b2-0000-7000-8000-000000000001',
			(SELECT name FROM lk_history_action ORDER BY id LIMIT 1), 'Users', '0193a1b2-0000-7000-8000-000000000001', '{}', '{}')`,
	} {
		if err := db.Exec(ctx, setUp); err != nil {
			t.Fatal(err)
		}
	}
	for _, change := range []string{"UPDATE history SET table_name = 'Accounts'", "DELETE FROM history"} {
		if err := db.Exec(ctx, change); err == nil || !strings.Contains(err.Error(), "append-only") {
			t.Errorf("%s: %v", change, err)
		}
	}
}
