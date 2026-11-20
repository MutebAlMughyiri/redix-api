// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package history

import (
	"context"
	"path/filepath"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

var user = uuid.MustParse("0193a1b2-0000-7000-8000-000000000001")

func TestUC069_ALineKeepsWhoDidWhatToWhichRecordWithTheValuesItChanged(t *testing.T) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer func() { _ = database.Close() }()
	account := uuid.MustParse("0193a1b2-0000-7000-8000-0000000000c1")
	at := moment.Of(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))
	before, after := Changed(
		map[string]any{"Name": "الصندوق", "UserId": user, "Opening": fixed.Number(0)},
		map[string]any{"Name": "صندوق الموقع", "UserId": user, "Opening": fixed.Number(5000000)})
	err = database.Exec(context.Background(), `INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password,
		failed_sign_ins, locked_until, created_at)
		VALUES ('`+user.String()+`', 'manager', 'Manager', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-16T10:00:00.0000000Z')`)
	if err != nil {
		t.Fatal(err)
	}
	err = database.InTx(context.Background(), func(q *store.Queries) error {
		return Write(context.Background(), q, at, user, Update, Change{Table: "Accounts", RecordID: account, Before: before, After: after})
	})
	if err != nil {
		t.Fatal(err)
	}
	var action, table, kept, made, when string
	if err := database.Read.QueryRow("SELECT action, table_name, before, after, at FROM history").Scan(&action, &table, &kept, &made, &when); err != nil {
		t.Fatal(err)
	}
	if action != "Update" || table != "Accounts" || when != "2026-11-16T10:00:00.0000000Z" {
		t.Errorf("%s %s %s", action, table, when)
	}
	if kept != `{"Name":"الصندوق","Opening":0.0000}` || made != `{"Name":"صندوق الموقع","Opening":500.0000}` {
		t.Errorf("before %s, after %s", kept, made)
	}
}

func TestUC069_OnlyTheValuesThatChangedAreKept(t *testing.T) {
	before, after := Changed(map[string]any{"Name": "الصندوق", "IsActive": true}, map[string]any{"Name": "الصندوق", "IsActive": false})
	if len(before) != 1 || before["IsActive"] != true || len(after) != 1 || after["IsActive"] != false {
		t.Errorf("%v %v", before, after)
	}
}
