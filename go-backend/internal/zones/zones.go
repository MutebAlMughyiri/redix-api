// Package zones holds the time zones a project may be in, and the one answer to which a project is in (redix-app #107; D3, Muteb 17 Nov 2026):
// one short list, each zone by its IANA name and the word naming its city. The zone database is built into the binary, so a server without zone
// files still knows them. Every moment stays UTC; only an entry's calendar day is counted in its project's zone.
package zones

import (
	"context"
	"database/sql"
	"errors"
	"time"
	_ "time/tzdata"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/google/uuid"
)

// Zone is one time zone a project may be in.
type Zone struct {
	// Name is the IANA name, as a project keeps it: Asia/Riyadh.
	Name string
	// WordKey is the word naming its city in the app's languages.
	WordKey string
}

// Riyadh is the zone of every project whose manager chose none (D1).
const Riyadh = "Asia/Riyadh"

// All are the zones offered, Saudi Arabia first.
var All = []Zone{
	{"Asia/Riyadh", "timeZone.riyadh"},
	{"Asia/Dubai", "timeZone.dubai"},
	{"Asia/Kuwait", "timeZone.kuwait"},
	{"Asia/Bahrain", "timeZone.bahrain"},
	{"Asia/Qatar", "timeZone.doha"},
	{"Asia/Muscat", "timeZone.muscat"},
	{"Africa/Cairo", "timeZone.cairo"},
	{"Asia/Amman", "timeZone.amman"},
	{"Europe/Istanbul", "timeZone.istanbul"},
	{"Europe/London", "timeZone.london"},
	{"Europe/Paris", "timeZone.paris"},
	{"Asia/Kolkata", "timeZone.newDelhi"},
	{"Asia/Karachi", "timeZone.karachi"},
	{"Asia/Dhaka", "timeZone.dhaka"},
	{"Asia/Manila", "timeZone.manila"},
	{"Asia/Shanghai", "timeZone.beijing"},
	{"America/New_York", "timeZone.newYork"},
	{"America/Los_Angeles", "timeZone.losAngeles"},
}

// Offered is whether name is on the list.
func Offered(name string) bool {
	for _, zone := range All {
		if zone.Name == name {
			return true
		}
	}
	return false
}

// Location is the zone named name, Riyadh's for a name not on the list. Every zone on it loads from the database built in, as a test proves.
func Location(name string) *time.Location {
	if !Offered(name) {
		name = Riyadh
	}
	location, _ := time.LoadLocation(name)
	return location
}

// OffsetMinutes is how far the zone named name is ahead of UTC at now, in minutes: what the app adds to its corrected clock (G1).
func OffsetMinutes(name string, now time.Time) int {
	_, seconds := now.In(Location(name)).Zone()
	return seconds / 60
}

// Today is the calendar day it is at now in the zone named name: an entry's today in its project (BR-12).
func Today(name string, now time.Time) time.Time {
	local := now.In(Location(name))
	return time.Date(local.Year(), local.Month(), local.Day(), 0, 0, 0, 0, time.UTC)
}

// Of is the zone the project is in: the one its manager chose, Riyadh for one that chose none.
func Of(ctx context.Context, q *store.Queries, projectID uuid.UUID) (string, error) {
	name, err := q.ProjectTimeZone(ctx, projectID)
	if errors.Is(err, sql.ErrNoRows) {
		return Riyadh, nil
	}
	return name, err
}
