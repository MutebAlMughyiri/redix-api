// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package fixed

import (
	"math"
	"testing"
)

func TestANumberColumnHoldingAnythingButAnIntegerIsAFailureToRead(t *testing.T) {
	var number Number
	if err := number.Scan("12.5"); err == nil {
		t.Fatal("a text read as a number")
	}
}

// A product too large for a column is beyond, as .NET's decimal overflowed, with the product's sign.
func TestAProductBeyondEveryColumnIsBeyond(t *testing.T) {
	huge := Typed{Number: Number(math.MaxInt64)}
	if product := Product(huge, huge); !product.Number().IsBeyond() || product.Number() < 0 {
		t.Fatalf("the product is %v", product.Number())
	}
	if product := Product(huge, Typed{Number: Number(-math.MaxInt64)}); !product.Number().IsBeyond() || product.Number() > 0 {
		t.Fatalf("the negative product is %v", product.Number())
	}
}
