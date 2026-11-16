package historytest

import (
	"context"
	"database/sql"
	"fmt"
	"path/filepath"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

// caught keeps the failures Check reports instead of failing the test.
type caught struct {
	*testing.T
	failures []string
}

func (c *caught) Errorf(format string, args ...any) {
	c.failures = append(c.failures, fmt.Sprintf(format, args...))
}

func (c *caught) Helper() {}

var user = uuid.MustParse("0193a1b2-0000-7000-8000-000000000001")

func withUser(t *testing.T) db.DB {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { database.Close() })
	write(t, database, func(tx *sql.Tx) error {
		_, err := tx.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at)
			VALUES (?, 'manager', 'Manager', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-16T10:00:00.0000000Z')`, user.String())
		return err
	})
	return database
}

func write(t *testing.T, database db.DB, change func(*sql.Tx) error) {
	t.Helper()
	if err := database.InTx(context.Background(), change); err != nil {
		t.Fatal(err)
	}
}

func TestTheNetCatchesAChangedRowWithNoHistoryLine(t *testing.T) {
	database := withUser(t)
	net := &caught{T: t}
	Check(net, database, func() {
		write(t, database, func(tx *sql.Tx) error { _, err := tx.Exec("UPDATE users SET display_name = 'نواف'"); return err })
	})
	if len(net.failures) != 1 {
		t.Errorf("%v", net.failures)
	}
}

func TestTheNetPassesAChangeWithItsLineAndAnExemptTableWithout(t *testing.T) {
	database := withUser(t)
	net := &caught{T: t}
	Check(net, database, func() {
		write(t, database, func(tx *sql.Tx) error {
			if _, err := tx.Exec("UPDATE users SET display_name = 'نواف'"); err != nil {
				return err
			}
			if _, err := tx.Exec(`INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at)
				VALUES (?, ?, 'HASH', '2026-11-17T10:00:00.0000000Z', 0, 0, '2026-11-16T10:00:00.0000000Z')`, uuid.NewString(), user.String()); err != nil {
				return err
			}
			return history.Write(context.Background(), tx, moment.Of(time.Now()), user, history.Update, history.Change{Table: "Users", RecordID: user,
				Before: map[string]any{"DisplayName": "Manager"}, After: map[string]any{"DisplayName": "نواف"}})
		})
	})
	if len(net.failures) != 0 {
		t.Errorf("%v", net.failures)
	}
}
