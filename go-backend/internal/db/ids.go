// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"encoding/json"

	"github.com/google/uuid"
)

// IDList is ids as the JSON list a query reads with json_each, naming the rows it reads (#87). A list of ids always encodes.
func IDList(ids []uuid.UUID) string {
	named, _ := json.Marshal(ids)
	return string(named)
}
