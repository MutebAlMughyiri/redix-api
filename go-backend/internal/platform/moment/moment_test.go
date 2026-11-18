// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package moment

import (
	"encoding/json"
	"testing"
	"time"
)

func TestG10_AMomentIsKeptAsISOTextWithSevenDecimalsAndReadBackEqual(t *testing.T) {
	m := Of(time.Date(2026, 11, 16, 10, 12, 13, 123456789, time.FixedZone("Riyadh", 3*3600)))
	kept, _ := m.Value()
	if kept != "2026-11-16T07:12:13.1234567Z" {
		t.Errorf("kept as %v", kept)
	}
	var read Moment
	if err := read.Scan(kept); err != nil || !read.Equal(m.Time) {
		t.Errorf("read back %v %v", read, err)
	}
}

func TestG8_AMomentIsWrittenAsDotNetWritesAUtcDateTime(t *testing.T) {
	for given, want := range map[time.Time]string{
		time.Date(2026, 11, 16, 10, 12, 13, 0, time.UTC):         `"2026-11-16T10:12:13Z"`,
		time.Date(2026, 11, 16, 10, 12, 13, 500000000, time.UTC): `"2026-11-16T10:12:13.5Z"`,
		time.Date(2026, 11, 16, 10, 12, 13, 123456700, time.UTC): `"2026-11-16T10:12:13.1234567Z"`,
		Never.Time: `"1900-01-01T00:00:00Z"`,
	} {
		got, _ := json.Marshal(Of(given))
		if string(got) != want {
			t.Errorf("%v: %s, want %s", given, got, want)
		}
	}
}

func TestG8_AMomentIsReadWithItsOffsetOrAsUtcWithout(t *testing.T) {
	for text, want := range map[string]time.Time{
		`"2026-11-16T10:12:13Z"`:      time.Date(2026, 11, 16, 10, 12, 13, 0, time.UTC),
		`"2026-11-16T13:12:13+03:00"`: time.Date(2026, 11, 16, 10, 12, 13, 0, time.UTC),
		`"2026-11-16T10:12:13.5"`:     time.Date(2026, 11, 16, 10, 12, 13, 500000000, time.UTC),
	} {
		var got Moment
		if err := json.Unmarshal([]byte(text), &got); err != nil || !got.Equal(want) {
			t.Errorf("%s: %v %v", text, got, err)
		}
	}
}

func TestADateIsWrittenAndKeptAsDotNetWritesADateOnly(t *testing.T) {
	d := Date{time.Date(2026, 11, 16, 0, 0, 0, 0, time.UTC)}
	written, _ := json.Marshal(d)
	kept, _ := d.Value()
	if string(written) != `"2026-11-16"` || kept != "2026-11-16" {
		t.Errorf("written %s, kept %v", written, kept)
	}
	var read Date
	if err := json.Unmarshal(written, &read); err != nil || !read.Equal(d.Time) {
		t.Errorf("read back %v %v", read, err)
	}
}
