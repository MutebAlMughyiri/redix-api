package db

import (
	"context"
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
	t.Cleanup(func() { db.Close() })
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
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> 'goose_db_version'"); got != 59 {
		t.Errorf("tables: %d, want 59", got)
	}
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'index' AND sql IS NOT NULL"); got != 101 {
		t.Errorf("indexes: %d, want 101", got)
	}
	if got := count(t, db, "SELECT count(*) FROM sqlite_schema WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> 'goose_db_version' AND sql NOT LIKE '%) STRICT'"); got != 0 {
		t.Errorf("tables not STRICT: %d", got)
	}
}

func TestR31_ANewDatabaseStartsWithItsWordsAndLookups(t *testing.T) {
	db := open(t)
	if got := count(t, db, "SELECT count(*) FROM translations"); got != 1634 {
		t.Errorf("words: %d, want 1634", got)
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
