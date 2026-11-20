// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package suppliers

import (
	"context"
	"database/sql"
	"errors"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/google/uuid"
	_ "modernc.org/sqlite"
)

// A VAT number the database cannot check against the project's suppliers is a failure, answered 500, never a supplier let through or a
// refusal (BR-16, changes.Answered). The replay's sweep reaches this only on some runs, so coverage flipped by its block (#76 row 269).
func TestBR16_AVatNumberTheDatabaseCannotCheckIsAFailureNotARefusal(t *testing.T) {
	closed, err := sql.Open("sqlite", ":memory:")
	if err != nil {
		t.Fatal(err)
	}
	_ = closed.Close()
	err = refusalOf(changes.Change{Ctx: context.Background(), Q: store.New(closed), ProjectID: uuid.New()}, uuid.Nil, Request{Name: "Cement Co", VatNumber: "300000000000003"})
	var refused changes.Refusal
	if err == nil || errors.As(err, &refused) {
		t.Errorf("a database that cannot answer gave %v", err)
	}
}
