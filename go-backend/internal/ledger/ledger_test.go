// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package ledger_test

import (
	"path/filepath"
	"slices"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/dbtest"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/ledger"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// BR-10: capital in and revenue bring money into their account, a purchase, a contractor's payment and capital out take it out.
func TestBR10_EachKindMovesItsAccountsMoneyItsWay(t *testing.T) {
	amount := fixed.Counted(fixed.Number(250000))
	for kind, in := range map[int64]bool{ledger.Purchase: false, ledger.ContractorPayment: false, ledger.CapitalIn: true, ledger.CapitalOut: false,
		ledger.Revenue: true} {
		if got := ledger.Signed(kind, amount); (got.Number() > 0) != in || got.Number() != map[bool]fixed.Number{true: 250000, false: -250000}[in] {
			t.Errorf("%s: %d", ledger.Kinds[kind], got.Number())
		}
	}
}

// redix-api #87 G4: the kinds and statuses are the database's numbers, each named as its lookup table names it, the API's name being that name
// in camel case.
func TestR87_G4_KindsAndStatusesAreTheDatabasesNumbersAndNames(t *testing.T) {
	database, _ := dbtest.Open(t, filepath.Join(t.TempDir(), "redix.db"))
	for table, names := range map[string]web.Enum{"lk_entry_kind": ledger.Kinds, "lk_entry_status": ledger.Statuses} {
		rows, err := database.Read.Query("SELECT id, name FROM " + table + " ORDER BY id")
		if err != nil {
			t.Fatal(err)
		}
		var kept []string
		for rows.Next() {
			var id int64
			var name string
			if err := rows.Scan(&id, &name); err != nil {
				t.Fatal(err)
			}
			if id != int64(len(kept)) {
				t.Errorf("%s: %q is %d, not %d", table, name, id, len(kept))
			}
			kept = append(kept, web.Camel(name))
		}
		if err := rows.Close(); err != nil {
			t.Fatal(err)
		}
		if !slices.Equal(kept, names) {
			t.Errorf("%s holds %q, the ledger %q", table, kept, names)
		}
	}
}
