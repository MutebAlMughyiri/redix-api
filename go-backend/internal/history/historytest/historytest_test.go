// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package historytest

import (
	"context"
	"fmt"
	"path/filepath"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
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
	exec(t, database, `INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at)
		VALUES (?, 'manager', 'Manager', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-16T10:00:00.0000000Z')`, user.String())
	return database
}

func exec(t *testing.T, database db.DB, statement string, args ...any) {
	t.Helper()
	if err := database.Exec(context.Background(), statement, args...); err != nil {
		t.Fatal(err)
	}
}

func TestTheNetCatchesAChangedRowWithNoHistoryLine(t *testing.T) {
	database := withUser(t)
	net := &caught{T: t}
	Check(net, database, func() {
		exec(t, database, "UPDATE users SET display_name = 'نواف'")
	})
	if len(net.failures) != 1 {
		t.Errorf("%v", net.failures)
	}
}

func TestTheNetPassesAChangeWithItsLineAndAnExemptTableWithout(t *testing.T) {
	database := withUser(t)
	net := &caught{T: t}
	Check(net, database, func() {
		exec(t, database, "UPDATE users SET display_name = 'نواف'")
		exec(t, database, `INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at)
			VALUES (?, ?, 'HASH', '2026-11-17T10:00:00.0000000Z', 0, 0, '2026-11-16T10:00:00.0000000Z')`, uuid.NewString(), user.String())
		err := database.InTx(context.Background(), func(q *store.Queries) error {
			return history.Write(context.Background(), q, moment.Of(time.Now()), user, history.Update, history.Change{Table: "Users", RecordID: user,
				Before: map[string]any{"DisplayName": "Manager"}, After: map[string]any{"DisplayName": "نواف"}})
		})
		if err != nil {
			t.Fatal(err)
		}
	})
	if len(net.failures) != 0 {
		t.Errorf("%v", net.failures)
	}
}
