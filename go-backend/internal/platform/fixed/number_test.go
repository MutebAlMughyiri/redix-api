// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

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

func TestANumberPastWhatIsKeptIsNoAcceptableAmount(t *testing.T) {
	for _, typed := range []string{"1000000000000001", "-1000000000000001"} {
		var number Number
		if err := number.UnmarshalJSON([]byte(typed)); err != nil || number.IsAcceptableMoney() {
			t.Errorf("%s read as %v, %v: want no error and no acceptable amount", typed, number, err)
		}
	}
}

func TestAPercentIsWrittenAsDotNetsDecimalReckonedIt(t *testing.T) {
	for _, c := range []struct {
		part, whole Number
		places      int
		away        bool
		want        Written
	}{
		{1000000, 4000000, 10, true, "25.00"}, {3000000, 3000000, 10, true, "100"}, {20000000, 30000000, 10, true, "66.6666666667"},
		{10000000, 30000000, 10, true, "33.3333333333"}, {0, 30000000, 10, true, "0"}, {5, 0, 10, true, "0"}, {10000, 80000, 2, false, "12.50"},
		{1, 3, 2, false, "33.33"},
	} {
		if got := Percent(c.part, c.whole, c.places, c.away); got != c.want {
			t.Errorf("Percent(%d, %d) = %s, want %s", c.part, c.whole, got, c.want)
		}
	}
}

func TestBR26_AKeptTotalIsReadBackWithTheScaleItWasWrittenWith(t *testing.T) {
	for kept := range map[string]bool{"0": true, "100.0000": true, "-700.25": true, "0.0000": true} {
		var total Total
		if err := json.Unmarshal([]byte(kept), &total); err != nil {
			t.Fatal(err)
		}
		if written, _ := json.Marshal(total); string(written) != kept {
			t.Errorf("%s was read back as %s", kept, written)
		}
	}
}

func TestBR26_AKeptShareIsReadBackAsItWasWritten(t *testing.T) {
	var share Written
	if err := json.Unmarshal([]byte("33.3333333333"), &share); err != nil {
		t.Fatal(err)
	}
	if written, _ := json.Marshal(share); string(written) != "33.3333333333" {
		t.Errorf("the share was read back as %s", written)
	}
}
