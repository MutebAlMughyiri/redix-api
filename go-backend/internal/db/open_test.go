// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db_test

import (
	"context"
	"path/filepath"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/dbtest"
)

// Open answers a failure of the database as it opens - migrating the schema, preparing the named queries on either pool - with an error, or,
// where goose itself takes a failed read of its version table as no table yet, with a database that works; never with one that does not
// (redix-api #141 D1).
func TestOpenAnswersEveryFailureOfTheDatabase(t *testing.T) {
	path := filepath.Join(t.TempDir(), "redix.db")
	failure := dbtest.Watch(t, path)
	for n := int64(1); ; n++ {
		failure.At(n)
		database, err := db.OpenWith(context.Background(), dbtest.Driver, path)
		if !failure.Failed() {
			if err != nil {
				t.Fatalf("nothing failed, and Open answered %v", err)
			}
			database.Close()
			t.Logf("Open made %d calls, each answered when it failed", n-1)
			return
		}
		if err == nil {
			failure.At(0)
			var tables int
			if err := database.Read.QueryRow("SELECT count(*) FROM sqlite_schema WHERE type = 'table'").Scan(&tables); err != nil || tables < 59 {
				t.Fatalf("call %d failed, and Open answered a database holding %d tables (%v)", n, tables, err)
			}
			database.Close()
		}
	}
}

func TestOpenThroughADriverNoOneRegisteredFails(t *testing.T) {
	if _, err := db.OpenWith(context.Background(), "no-such-driver", filepath.Join(t.TempDir(), "redix.db")); err == nil {
		t.Fatal("Open through a driver no one registered answered no error")
	}
}
