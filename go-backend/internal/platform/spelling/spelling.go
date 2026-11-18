// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package spelling is what a search treats alike in Arabic (ArabicSpelling, FR-43): أ إ آ with ا, ة with ه, ى with ي, ؤ with و, ئ with ي, and no
// tashkeel. A table keeps a normalized copy of what is searched beside it, filled when it is saved, and a search normalizes what was typed the
// same way.
package spelling

import (
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

var letters = map[rune]rune{'أ': 'ا', 'إ': 'ا', 'آ': 'ا', 'ة': 'ه', 'ى': 'ي', 'ؤ': 'و', 'ئ': 'ي'}

// The tashkeel, fathatan to sukun.
const (
	firstTashkeel = 'ً'
	lastTashkeel  = 'ْ'
)

// Normalize is text with one form of each letter and no tashkeel; anything else stays as it is.
func Normalize(text string) string {
	return strings.Map(func(letter rune) rune {
		if letter >= firstTashkeel && letter <= lastTashkeel {
			return -1
		}
		if one, found := letters[letter]; found {
			return one
		}
		return letter
	}, text)
}

// Holds says whether typed, read as this rule reads it and in any case, is in one of words, or in amount written plain (1234.5) or grouped
// (1,234.50); true when nothing was typed. An amount of 0 is no amount, so it is never matched (ArabicSpelling.Holds, redix-app #104): the one way a
// list searched on the API matches what was typed.
func Holds(typed string, amount fixed.Number, words ...string) bool {
	if amount != 0 {
		words = append(words, plain(amount), grouped(amount))
	}
	return strings.Contains(searchable(strings.Join(words, " ")), searchable(typed))
}

func searchable(text string) string { return strings.ToLower(Normalize(strings.TrimSpace(text))) }

// plain is amount as "0.##" wrote it: no trailing zeros, at most two decimals.
func plain(amount fixed.Number) string {
	written := amount.String()
	written = written[:len(written)-2]
	return strings.TrimSuffix(strings.TrimRight(written, "0"), ".")
}

// grouped is amount as "#,0.00" wrote it: thousands grouped, two decimals.
func grouped(amount fixed.Number) string {
	written := amount.String()
	written = written[:len(written)-2]
	sign := ""
	if strings.HasPrefix(written, "-") {
		sign, written = "-", written[1:]
	}
	whole, decimals, _ := strings.Cut(written, ".")
	var parts []string
	for len(whole) > 3 {
		parts = append([]string{whole[len(whole)-3:]}, parts...)
		whole = whole[:len(whole)-3]
	}
	return sign + strings.Join(append([]string{whole}, parts...), ",") + "." + decimals
}
