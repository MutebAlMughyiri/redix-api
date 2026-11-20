// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package auth

import (
	"context"
	"path/filepath"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
)

var now = moment.Of(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))

func emptyDatabase(t *testing.T) db.DB {
	t.Helper()
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { _ = database.Close() })
	return database
}

func count(t *testing.T, database db.DB, query string) int {
	t.Helper()
	var n int
	if err := database.Read.QueryRow(query).Scan(&n); err != nil {
		t.Fatal(err)
	}
	return n
}

func TestBR29_AnEmptyDatabaseGetsItsAdminWhoMustChangeThePasswordAndMayApproveProjects(t *testing.T) {
	database := emptyDatabase(t)
	if err := SeedAdmin(context.Background(), database, "real-admin", "Temporary-1", now); err != nil {
		t.Fatal(err)
	}
	var hash string
	var mustChange bool
	if err := database.Read.QueryRow("SELECT password_hash, must_change_password FROM users WHERE username = 'real-admin'").Scan(&hash, &mustChange); err != nil {
		t.Fatal(err)
	}
	if !mustChange || credentials.Check(hash, "Temporary-1") != credentials.Right {
		t.Errorf("must change %v, password checks %v", mustChange, credentials.Check(hash, "Temporary-1"))
	}
	if got := count(t, database, "SELECT count(*) FROM system_permissions WHERE permission = 'ApproveProjects'"); got != 1 {
		t.Errorf("approvers: %d", got)
	}
}

func TestBR29_ADatabaseWithUsersGetsNoSecondAdmin(t *testing.T) {
	database := emptyDatabase(t)
	if err := SeedAdmin(context.Background(), database, "real-admin", "Temporary-1", now); err != nil {
		t.Fatal(err)
	}
	if err := SeedAdmin(context.Background(), database, "other-admin", "Temporary-2", now); err != nil {
		t.Fatal(err)
	}
	if got := count(t, database, "SELECT count(*) FROM users"); got != 1 {
		t.Errorf("users: %d", got)
	}
}

func TestBR29_NoAdminIsMadeWithoutAUsername(t *testing.T) {
	database := emptyDatabase(t)
	if err := SeedAdmin(context.Background(), database, "", "Temporary-1", now); err != nil {
		t.Fatal(err)
	}
	if got := count(t, database, "SELECT count(*) FROM users"); got != 0 {
		t.Errorf("users: %d", got)
	}
}
