package moment

import "testing"

func TestAMomentOrDateColumnHoldingAnythingButTextIsAFailureToRead(t *testing.T) {
	var at Moment
	var day Date
	if at.Scan(int64(1)) == nil || day.Scan(int64(1)) == nil {
		t.Fatal("a number read as a moment or a date")
	}
}
