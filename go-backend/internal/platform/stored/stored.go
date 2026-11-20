// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package stored reads back a value the API stored as JSON where nothing is the normal answer for a value not kept - a History line's values,
// a column's choices (redix-api #76 row 282, the owner 20 Nov 2026): a value not kept reads as nothing, and one that cannot be read reads as
// nothing too, but is written to the log, so a screen shows what it can and the log says what it could not.
package stored

import (
	"encoding/json"
	"fmt"
	"log/slog"
)

// Read reads kept into into; into stays as it was for a value not kept, and for one it cannot read, which is logged.
func Read(kept []byte, into any) {
	if err := json.Unmarshal(kept, into); err != nil && len(kept) > 0 {
		slog.Warn("a stored JSON value could not be read", "into", fmt.Sprintf("%T", into), "error", err)
	}
}
