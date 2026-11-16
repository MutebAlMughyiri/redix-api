// Package fixed owns the numbers kept with four decimals: every amount, price and quantity (BR-13, decimal(19,4) in SQL Server).
package fixed

import (
	"database/sql/driver"
	"fmt"
	"math/big"
	"strings"
)

// scale is ten to the number of decimals kept.
const scale = 10000

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
