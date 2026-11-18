// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package statement

import (
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

func TestBR13_AnAmountPrintsWithSeparatorsTwoDecimalsRoundedHalfAwayFromZeroAndANegativeInBrackets(t *testing.T) {
	for amount, want := range map[fixed.Number]string{12345600: "1,234.56", 0: "0.00", 50: "0.01", 49: "0.00", -12345650: "(1,234.57)", 10000000000: "1,000,000.00"} {
		if got := Money(amount); got != want {
			t.Errorf("%d printed as %s, want %s", amount, got, want)
		}
	}
}

func TestASheetNameHasNoForbiddenLetterAtMost28LettersAndEachIsUnique(t *testing.T) {
	book := NewBook("", "")
	long := strings.Repeat("بلاط", 10)
	for name, want := range map[string]string{"بلاط/حمام": "بلاط حمام", "  ": "-"} {
		if got := book.SheetName(name); got != want {
			t.Errorf("%q named %q, want %q", name, got, want)
		}
	}
	if got := book.SheetName(long); got != strings.Repeat("بلاط", 7) {
		t.Errorf("a long name was cut to %q", got)
	}
	if first, second := book.SheetName("الرسوم"), book.SheetName("الرسوم"); first != "الرسوم" || second != "الرسوم 2" {
		t.Errorf("two sheets of one name were named %q and %q", first, second)
	}
	if lists := book.SheetName("القوائم"); lists != "القوائم 2" {
		t.Errorf("a sheet named as the lists' took %q, which the lists hold from the start", lists)
	}
}
