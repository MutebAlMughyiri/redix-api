// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package config serves the values the app reads at start, before anyone signs in: how long it waits for an answer, the VAT rate, and anything
// later made configurable (UC-070). None is ever a secret: secrets stay in environment variables.
package config

import (
	"context"
	"fmt"
	"net/http"
	"strconv"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// Map routes GET /config, open to everyone.
func Map(router *web.Router, database db.DB) {
	values := database.Queries()
	// Every configuration value by its key, for anyone, so none is a secret (UC-070).
	router.Handle("GET", "/config", func(w http.ResponseWriter, r *http.Request) {
		rows, err := values.Values(r.Context())
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		byKey := make(map[string]string, len(rows))
		for _, row := range rows {
			byKey[row.Key] = row.Value
		}
		web.JSON(w, r, http.StatusOK, byKey)
	})
}

// requestSize is the key of the largest request the API and the web read, in megabytes.
const requestSize = "requestSizeMegabytes"

// RequestSize is the largest request body, in bytes, the API and the web read: the value requestSizeMegabytes, 30 to start with, so the
// largest file a person may send, 25 MB (fileTooLarge), fits with room for its form, and a larger body is refused before any route reads it
// (redix-api #145 R17; the owner, 23 Nov 2026: kept in this table). The server reads it as it starts, so a change holds from the instance's
// next start; a value that is no whole number of megabytes above 0, or none, stops it there, naming the key.
func RequestSize(ctx context.Context, database db.DB) (int64, error) {
	rows, err := database.Queries().Values(ctx)
	if err != nil {
		return 0, err
	}
	for _, row := range rows {
		if row.Key == requestSize {
			megabytes, err := strconv.ParseInt(row.Value, 10, 64)
			if err != nil || megabytes <= 0 {
				return 0, fmt.Errorf("the config value %s is %q, not a whole number of megabytes", requestSize, row.Value)
			}
			return megabytes << 20, nil
		}
	}
	return 0, fmt.Errorf("the config value %s is missing", requestSize)
}
