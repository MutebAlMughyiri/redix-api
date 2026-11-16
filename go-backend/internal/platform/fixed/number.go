// Package fixed owns the numbers kept with four decimals: every amount, price and quantity (BR-13, decimal(19,4) in SQL Server).
package fixed

import (
	"database/sql/driver"
	"fmt"
	"math/big"
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
		return 0, fmt.Errorf("fixed: %q is too large", text)
	}
	return Number(whole.Int64()), nil
}

// MarshalJSON writes the number as a JSON number with four decimals.
func (n Number) MarshalJSON() ([]byte, error) { return []byte(n.String()), nil }

// UnmarshalJSON reads a JSON number.
func (n *Number) UnmarshalJSON(data []byte) error {
	parsed, err := Parse(string(data))
	*n = parsed
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
