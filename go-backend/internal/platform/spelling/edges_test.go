package spelling

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

func TestANegativeAmountIsGroupedWithItsSign(t *testing.T) {
	if got := grouped(fixed.Number(-12345670000)); got != "-1,234,567.00" {
		t.Fatal(got)
	}
}
