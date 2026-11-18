// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package moment owns the times and dates REDIX keeps: a moment in UTC, as datetime2 held it, and a calendar date, as date held it (redix-api #140, G10).
package moment

import (
	"database/sql/driver"
	"fmt"
	"strings"
	"time"
)

// column is how a moment is kept: ISO 8601 UTC with seven decimals, so text order is time order.
const column = "2006-01-02T15:04:05.0000000Z"

// Moment is an instant in UTC.
type Moment struct{ time.Time }

// Never is the moment that means "not happened", as 1900-01-01 meant it in SQL Server (Entry.NotApproved, User.NeverLocked).
var Never = Moment{time.Date(1900, 1, 1, 0, 0, 0, 0, time.UTC)}

// Of is t as a moment in UTC, to the 100 nanoseconds datetime2 kept.
func Of(t time.Time) Moment { return Moment{t.UTC().Truncate(100 * time.Nanosecond)} }

// Scan reads the text the column keeps.
func (m *Moment) Scan(src any) error {
	text, ok := src.(string)
	if !ok {
		return fmt.Errorf("moment: cannot read %T", src)
	}
	parsed, err := time.Parse(column, text)
	m.Time = parsed
	return err
}

// Value is the text the column keeps.
func (m Moment) Value() (driver.Value, error) { return m.UTC().Format(column), nil }

// MarshalJSON writes the moment as .NET writes a UTC DateTime: decimals only as far as they are not zero, then Z; the zero moment as .NET wrote
// default(DateTime), which has no zone - what a view of something not yet done says, as the latest reconciliation before the first.
func (m Moment) MarshalJSON() ([]byte, error) {
	if m.IsZero() {
		return []byte(`"0001-01-01T00:00:00"`), nil
	}
	return []byte(`"` + m.UTC().Format(time.RFC3339Nano) + `"`), nil
}

// UnmarshalJSON reads an ISO 8601 moment; one with no offset is taken as UTC, as the API took it.
func (m *Moment) UnmarshalJSON(data []byte) error {
	text := strings.Trim(string(data), `"`)
	parsed, err := time.Parse(time.RFC3339Nano, text)
	if err != nil {
		parsed, err = time.ParseInLocation("2006-01-02T15:04:05.9999999", text, time.UTC)
	}
	*m = Of(parsed)
	return err
}

// dateLayout is how a date is kept and written.
const dateLayout = "2006-01-02"

// Date is a calendar day with no time, such as an entry's date.
type Date struct{ time.Time }

// Scan reads the text the column keeps.
func (d *Date) Scan(src any) error {
	text, ok := src.(string)
	if !ok {
		return fmt.Errorf("moment: cannot read %T as a date", src)
	}
	parsed, err := time.Parse(dateLayout, text)
	d.Time = parsed
	return err
}

// Value is the text the column keeps.
func (d Date) Value() (driver.Value, error) { return d.Format(dateLayout), nil }

// MarshalJSON writes the date as .NET writes a DateOnly: "2026-11-16".
func (d Date) MarshalJSON() ([]byte, error) { return []byte(`"` + d.Format(dateLayout) + `"`), nil }

// UnmarshalJSON reads "2026-11-16".
func (d *Date) UnmarshalJSON(data []byte) error {
	parsed, err := time.Parse(dateLayout, strings.Trim(string(data), `"`))
	d.Time = parsed
	return err
}
