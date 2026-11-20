// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"encoding/json"

	"github.com/google/uuid"
)

// IDList is ids as the JSON list a query reads with json_each, naming the rows it reads, each once, so no row is read twice (#87). A list of
// ids always encodes.
func IDList(ids []uuid.UUID) string {
	listed := make([]uuid.UUID, 0, len(ids))
	seen := map[uuid.UUID]bool{}
	for _, id := range ids {
		if !seen[id] {
			seen[id] = true
			listed = append(listed, id)
		}
	}
	named, _ := json.Marshal(listed)
	return string(named)
}
