// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package contracts

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

func TestR297_AContractSaysHowItStands(t *testing.T) {
	const value = fixed.Number(1000 * 10000)
	paid := func(riyals int64) fixed.Total { return fixed.Counted(fixed.Number(riyals * 10000)) }
	for _, one := range []struct {
		contract store.Contract
		paid     fixed.Total
		want     string
	}{
		{store.Contract{Value: value, IsActive: false}, paid(2000), "contracts.disabled"},
		{store.Contract{Value: value, IsActive: true}, paid(1001), "contracts.overrun"},
		{store.Contract{IsOpenValue: true, IsActive: true}, paid(5000), "contracts.openValue"},
		{store.Contract{Value: value, IsActive: true}, paid(0), "contracts.notStarted"},
		{store.Contract{Value: value, IsActive: true}, paid(1000), "contracts.running"},
	} {
		if got := standingWordKeyOf(one.contract, one.paid); got != one.want {
			t.Errorf("%+v with %v paid stands %s, want %s", one.contract, one.paid.Number(), got, one.want)
		}
	}
}

func TestR297_WhatRemainsOnceApprovedCountsWhatWaits(t *testing.T) {
	held := Money{Paid: fixed.Counted(fixed.Number(300 * 10000)), Pending: fixed.Counted(fixed.Number(200 * 10000))}
	if got := StandingOf(store.Contract{Value: fixed.Number(1000 * 10000)}, held, fixed.Total{}).RemainingAfter.Number(); got != fixed.Number(500*10000) {
		t.Errorf("a contract of 1000 with 300 paid and 200 waiting keeps %v once approved, want 500", got)
	}
	if got := StandingOf(store.Contract{IsOpenValue: true}, held, fixed.Total{}).RemainingAfter.Number(); got != 0 {
		t.Errorf("an open-value contract keeps %v once approved, want nothing", got)
	}
}
