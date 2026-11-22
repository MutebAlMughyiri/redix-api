// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package fixed owns the numbers kept with four decimals: every amount, price and quantity (BR-13, decimal(19,4) in SQL Server).
package fixed

import (
	"database/sql/driver"
	"errors"
	"fmt"
	"math"
	"math/big"
	"slices"
	"strings"
)

// decimals is how many decimals a number keeps, and scale ten to that.
const (
	decimals = 4
	scale    = 10000
)

// Number is a number with four decimals, kept as an integer count of ten-thousandths, so adding never loses a halala.
type Number int64

// String writes the number with its four decimals, as .NET writes a decimal(19,4): 700.0000, -12.5000.
func (n Number) String() string {
	sign, value := "", int64(n)
	if value < 0 {
		sign, value = "-", -value
	}
	return fmt.Sprintf("%s%d.%04d", sign, value/scale, value%scale)
}

// Plain writes the number as a person types it back into a field: no trailing zeros and no point when it is whole, as 700, 12.5, -0.0001. The
// one writer of a figure a form starts with, Add entry's, the count's and a contract's (redix-api #170).
func (n Number) Plain() string { return strings.TrimSuffix(strings.TrimRight(n.String(), "0"), ".") }

// Parse reads a decimal such as "700", "12.5" or "-0.0001"; more than four decimals round half away from zero, as SQL Server stores them.
func Parse(text string) (Number, error) {
	rat, ok := new(big.Rat).SetString(strings.TrimSpace(text))
	if !ok {
		return 0, fmt.Errorf("fixed: %q is not a number", text)
	}
	rat.Mul(rat, big.NewRat(scale, 1))
	whole := new(big.Int).Quo(rat.Num(), rat.Denom())
	rest := new(big.Rat).Sub(rat, new(big.Rat).SetInt(whole))
	if rest.Abs(rest).Cmp(big.NewRat(1, 2)) >= 0 {
		whole.Add(whole, big.NewInt(int64(rat.Sign())))
	}
	if !whole.IsInt64() {
		return beyond(rat.Sign()), errTooLarge
	}
	return Number(whole.Int64()), nil
}

// errTooLarge is a number past what a Number holds.
var errTooLarge = errors.New("fixed: the number is past what is kept")

// beyond is the number that stands for one past what is kept, on the side of sign: its last decimals make it no acceptable amount, so a request
// typing such a number is refused by its rule's own code - invalidAmount, invalidValue - as .NET refused a decimal past Money.Largest.
func beyond(sign int) Number {
	if sign < 0 {
		return math.MinInt64
	}
	return math.MaxInt64
}

// MarshalJSON writes the number as a JSON number with four decimals.
func (n Number) MarshalJSON() ([]byte, error) { return []byte(n.String()), nil }

// UnmarshalJSON reads a JSON number; one past what is kept reads as beyond, for the request's rule to refuse.
func (n *Number) UnmarshalJSON(data []byte) error {
	parsed, err := Parse(string(data))
	*n = parsed
	if errors.Is(err, errTooLarge) {
		return nil
	}
	return err
}

// Scan reads the integer the column keeps.
func (n *Number) Scan(src any) error {
	value, ok := src.(int64)
	if !ok {
		return fmt.Errorf("fixed: cannot read %T", src)
	}
	*n = Number(value)
	return nil
}

// Value is the integer the column keeps.
func (n Number) Value() (driver.Value, error) { return int64(n), nil }

// Float is the number as a double, as a workbook keeps a number.
func (n Number) Float() float64 { return float64(n) / scale }

// IsAcceptableMoney says whether the number may be an amount of money: at most two decimals (Money.IsAcceptable); every number kept is within
// the largest amount.
func (n Number) IsAcceptableMoney() bool { return n%100 == 0 }

// Total is a number as .NET's decimal wrote it, with its scale: 0 while nothing was counted into it - a decimal's default has no decimals - four
// decimals once a kept number was, as SQL Server's sum of a decimal(19,4) keeps its scale, and as many as it was typed with when it comes straight
// from a request; adding or taking away keeps the wider scale of the two, as decimal arithmetic does. A balance nothing moved writes 0, one moved
// back to nothing 0.0000, and a typed 700.25 writes 700.25.
type Total struct {
	sum      Number
	decimals int
}

// Counted is n, a kept number, as a total.
func Counted(n Number) Total { return Total{sum: n, decimals: decimals} }

// Plus is the total with other added.
func (t Total) Plus(other Total) Total {
	return Total{sum: t.sum + other.sum, decimals: max(t.decimals, other.decimals)}
}

// Minus is the total with other taken away.
func (t Total) Minus(other Total) Total {
	return Total{sum: t.sum - other.sum, decimals: max(t.decimals, other.decimals)}
}

// Number is the total's value.
func (t Total) Number() Number { return t.sum }

// MarshalJSON writes the total with its scale.
func (t Total) MarshalJSON() ([]byte, error) {
	written := t.sum.String()
	written = written[:len(written)-(decimals-t.decimals)]
	return []byte(strings.TrimSuffix(written, ".")), nil
}

// UnmarshalJSON reads a total as it was written, with its scale, so a kept periodic report writes every figure again as it was issued (BR-26).
func (t *Total) UnmarshalJSON(data []byte) error {
	var typed Typed
	err := typed.UnmarshalJSON(data)
	*t = typed.Total()
	return err
}

// Typed is a number as a request typed it, with the decimals it was typed with, which .NET's decimal kept and wrote back in an answer built from it.
type Typed struct {
	Number
	decimals int
}

// UnmarshalJSON reads a JSON number and how many decimals it was written with, at most four.
func (t *Typed) UnmarshalJSON(data []byte) error {
	if err := t.Number.UnmarshalJSON(data); err != nil {
		return err
	}
	if dot := strings.IndexByte(string(data), '.'); dot >= 0 {
		t.decimals = min(len(strings.TrimRight(strings.SplitN(string(data[dot+1:]), "e", 2)[0], "E")), decimals)
	}
	return nil
}

// Total is the typed number as a total of its own scale.
func (t Typed) Total() Total { return Total{sum: t.Number, decimals: t.decimals} }

// IsBeyond says whether the number stands for one past what is kept.
func (n Number) IsBeyond() bool { return n == math.MaxInt64 || n == math.MinInt64 }

// Product is a times b as .NET's decimal multiplied them: the scale is the two scales added, at most four - 380 times 2.5 is 950.0 - and the value
// rounded half away from zero to four decimals, as SQL Server keeps it.
func Product(a, b Typed) Total {
	exact := new(big.Rat).Mul(big.NewRat(int64(a.Number), scale), big.NewRat(int64(b.Number), scale))
	product, err := Parse(exact.FloatString(decimals + 2))
	if err != nil {
		return Total{sum: beyond(exact.Sign()), decimals: decimals}
	}
	return Total{sum: product, decimals: min(a.decimals+b.decimals, decimals)}
}

// Written is a number as .NET wrote a decimal it computed, kept as the text it wrote.
type Written string

// MarshalJSON writes the number as it was written.
func (w Written) MarshalJSON() ([]byte, error) { return []byte(w), nil }

// UnmarshalJSON keeps the number as it was written.
func (w *Written) UnmarshalJSON(data []byte) error {
	*w = Written(data)
	return nil
}

// Percent is part as a percentage of whole as .NET's decimal reckoned decimal.Round(part / whole * 100, decimals): the quotient carries the fewest
// decimals that make it exact - 100/400 is 0.25 - which the percentage keeps, 25.00, up to decimals, past which it is rounded, half away from zero
// when away, to the even digit otherwise, as decimal.Round's default does. 0 for a whole of nothing.
func Percent(part, whole Number, places int, away bool) Written {
	if whole == 0 {
		return "0"
	}
	quotient := big.NewRat(int64(part), int64(whole))
	exact := -1
	for kept := 0; kept <= 28; kept++ {
		if new(big.Rat).Mul(quotient, ten(kept)).IsInt() {
			exact = kept
			break
		}
	}
	percent := new(big.Rat).Mul(quotient, big.NewRat(100, 1))
	if exact >= 0 && exact <= places {
		return Written(percent.FloatString(exact))
	}
	scaled := new(big.Rat).Mul(percent, ten(places))
	whole64 := new(big.Int).Quo(scaled.Num(), scaled.Denom())
	rest := new(big.Rat).Abs(new(big.Rat).Sub(scaled, new(big.Rat).SetInt(whole64)))
	half := rest.Cmp(big.NewRat(1, 2))
	if half > 0 || (half == 0 && (away || whole64.Bit(0) == 1)) {
		whole64.Add(whole64, big.NewInt(int64(percent.Sign())))
	}
	return Written(new(big.Rat).Quo(new(big.Rat).SetInt(whole64), ten(places)).FloatString(places))
}

func ten(power int) *big.Rat {
	return new(big.Rat).SetInt(new(big.Int).Exp(big.NewInt(10), big.NewInt(int64(power)), nil))
}

// TypedIn is value read as a number a person typed, as decimal.TryParse read it with NumberStyles.Number: digits with a sign, a point and
// thousands separators, and the decimals it was written with. The one reader of typed text, an entry's column's and the web's amount
// (redix-api #166).
func TypedIn(value string) (Typed, bool) {
	var number Typed
	var cleaned []byte
	for _, letter := range []byte(value) {
		if letter != ',' && letter != ' ' {
			cleaned = append(cleaned, letter)
		}
	}
	if len(cleaned) == 0 || slices.ContainsFunc(cleaned, func(letter byte) bool { return letter == 'e' || letter == 'E' }) {
		return number, false
	}
	return number, number.UnmarshalJSON(cleaned) == nil
}
