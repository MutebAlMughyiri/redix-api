// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package zones

import (
	"testing"
	"time"
)

// Every zone offered loads from the database built into the binary, and none but Riyadh's stands in for a name not on the list.
func TestEveryZoneOfferedLoads(t *testing.T) {
	for _, zone := range All {
		if location, err := time.LoadLocation(zone.Name); err != nil || Location(zone.Name).String() != location.String() {
			t.Errorf("%s: %v", zone.Name, err)
		}
	}
	if Location("Mars/Olympus").String() != Riyadh || Offered("Mars/Olympus") {
		t.Error("a zone not on the list was taken")
	}
}

// BR-12 in a project's zone: at 17:00 UTC on 16 Nov 2026 it is already the 17th in Shanghai, still the 16th in New York and in Riyadh at 20:00.
func TestBR12_AProjectsTodayIsTheServersClockInItsZone(t *testing.T) {
	at := time.Date(2026, 11, 16, 17, 0, 0, 0, time.UTC)
	for zone, day := range map[string]int{"Asia/Shanghai": 17, "America/New_York": 16, Riyadh: 16} {
		if got := Today(zone, at); got.Day() != day || got.Location() != time.UTC {
			t.Errorf("%s: %v, not the %dth", zone, got, day)
		}
	}
	if OffsetMinutes(Riyadh, at) != 180 || OffsetMinutes("Asia/Kolkata", at) != 330 || OffsetMinutes("America/Los_Angeles", at) != -480 {
		t.Error(OffsetMinutes(Riyadh, at), OffsetMinutes("Asia/Kolkata", at), OffsetMinutes("America/Los_Angeles", at))
	}
}
