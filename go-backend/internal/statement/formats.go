package statement

import (
	"fmt"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

// Money is an amount as a report file prints it (ReportFormats.Money, BR-13): 1,234.56, a negative in brackets, rounded half away from zero from
// the kept decimals.
func Money(amount fixed.Number) string {
	negative := amount < 0
	if negative {
		amount = -amount
	}
	cents := (int64(amount) + 50) / 100
	text := fmt.Sprintf("%s.%02d", grouped(cents/100), cents%100)
	if negative {
		return "(" + text + ")"
	}
	return text
}

// grouped is whole with a comma between each three digits.
func grouped(whole int64) string {
	digits := strconv.FormatInt(whole, 10)
	var text strings.Builder
	for index, digit := range digits {
		if index > 0 && (len(digits)-index)%3 == 0 {
			text.WriteByte(',')
		}
		text.WriteRune(digit)
	}
	return text.String()
}

// moneyText is a workbook amount as MoneyFormat shows it, for a column's fit.
func moneyText(amount float64) string { return Money(numberOf(amount)) }
