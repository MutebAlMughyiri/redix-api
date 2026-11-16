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

func TestATotalWritesWhatDotNetWrote(t *testing.T) {
	nothing := Total{}
	moved := Counted(1000000)
	for total, want := range map[Total]string{nothing: "0", moved: "100.0000", moved.Minus(moved): "0.0000", nothing.Plus(nothing): "0", nothing.Minus(moved): "-100.0000"} {
		if written, _ := total.MarshalJSON(); string(written) != want {
			t.Errorf("%+v wrote %s, want %s", total, written, want)
		}
	}
}

func TestATypedNumberIsWrittenBackWithTheDecimalsItWasTypedWith(t *testing.T) {
	for typed, want := range map[string]string{"700.25": "700.25", "0": "0", "12.5000": "12.5000", "-3.1": "-3.1"} {
		var number Typed
		if err := number.UnmarshalJSON([]byte(typed)); err != nil {
			t.Fatal(err)
		}
		if written, _ := number.Total().MarshalJSON(); string(written) != want {
			t.Errorf("%s was written back as %s, want %s", typed, written, want)
		}
	}
}
