package fixed

import (
	"encoding/json"
	"testing"
)

func TestBR13_AnAmountIsWrittenWithItsFourDecimalsAsDotNetWritesIt(t *testing.T) {
	for number, want := range map[Number]string{7000000: "700.0000", -125000: "-12.5000", 1: "0.0001", 0: "0.0000", -1: "-0.0001"} {
		got, _ := json.Marshal(number)
		if string(got) != want {
			t.Errorf("%d: %s, want %s", number, got, want)
		}
	}
}

func TestBR13_AnAmountIsReadExactlyAndRoundedPastFourDecimalsAsSqlServerStoresIt(t *testing.T) {
	for text, want := range map[string]Number{"700": 7000000, "12.5": 125000, "-0.0001": -1, "8320.75": 83207500, "0.00005": 1, "-0.00005": -1, "0.00004": 0, "1e2": 1000000} {
		var got Number
		if err := json.Unmarshal([]byte(text), &got); err != nil || got != want {
			t.Errorf("%s: %d %v, want %d", text, got, err, want)
		}
	}
	var refused Number
	if err := json.Unmarshal([]byte(`"abc"`), &refused); err == nil {
		t.Error("a word was read as a number")
	}
}

func TestBR13_AddingAmountsNeverLosesAHalala(t *testing.T) {
	var total Number
	for i := 0; i < 1000; i++ {
		total += 1000
	}
	if total.String() != "100.0000" {
		t.Errorf("a thousand times 0.1000 is %s", total)
	}
}
