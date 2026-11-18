// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"strings"
	"sync"

	"golang.org/x/text/collate"
	"golang.org/x/text/language"
	"modernc.org/sqlite"
)

// Names is the collation every list sorted by a name uses, "ORDER BY name COLLATE names": Arabic dictionary order, where أ إ آ ا count as one
// letter, with English case ignored (redix-api #140, D7).
const Names = "names"

// collators lends one collator to each comparison, since a collator may not be shared by two at once.
var collators = sync.Pool{New: func() any { return collate.New(language.Arabic, collate.IgnoreCase) }}

func init() {
	if err := sqlite.RegisterCollationUtf8(Names, compareNames); err != nil {
		panic(err)
	}
}

// alefs folds the alef forms into one letter: Unicode's tables keep آ أ إ ا apart, while a reader looks أحمد up beside احمد.
var alefs = strings.NewReplacer("أ", "ا", "إ", "ا", "آ", "ا")

// compareNames orders by the names with their alefs folded, then by the names as written, so two that differ only there still have one order.
func compareNames(left, right string) int {
	collator := collators.Get().(*collate.Collator)
	defer collators.Put(collator)
	if order := collator.CompareString(alefs.Replace(left), alefs.Replace(right)); order != 0 {
		return order
	}
	return collator.CompareString(left, right)
}
