// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"errors"

	"modernc.org/sqlite"
	sqlite3 "modernc.org/sqlite/lib"
)

// Refuses says whether err is the database refusing a row for one of its constraints - a foreign key, a unique index, a CHECK - rather than
// failing: a retrieve answers the one archiveDoesNotFit, and the other 500 (UC-073 AC-73.5).
func Refuses(err error) bool {
	var refused *sqlite.Error
	return errors.As(err, &refused) && refused.Code()&0xff == sqlite3.SQLITE_CONSTRAINT
}
