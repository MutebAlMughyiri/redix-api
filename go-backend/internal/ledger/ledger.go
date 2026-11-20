// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package ledger names what an entry is and where it stands, as the database keeps them, and how each kind moves its account's money: the one
// owner every package that reads entries asks, so none keeps a copy of its own (EntryKind, EntryStatus, redix-api #87 G4).
package ledger

import (
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// What an entry is (EntryKind).
const (
	Purchase int64 = iota
	ContractorPayment
	CapitalIn
	CapitalOut
	Revenue
)

// Kinds names the kinds as the API writes and reads them.
var Kinds = web.Enum{"purchase", "contractorPayment", "capitalIn", "capitalOut", "revenue"}

// Where an entry stands (EntryStatus).
const (
	Draft int64 = iota
	PendingApproval
	Approved
	Rejected
	Removed
)

// Statuses names the statuses as the API writes and reads them.
var Statuses = web.Enum{"draft", "pendingApproval", "approved", "rejected", "removed"}

// Signed is an amount of kind as it moves its account: capital in and revenue bring money in, the rest take it out (EntryKinds.Signed, BR-10).
func Signed(kind int64, amount fixed.Total) fixed.Total {
	if kind == CapitalIn || kind == Revenue {
		return amount
	}
	return fixed.Total{}.Minus(amount)
}
