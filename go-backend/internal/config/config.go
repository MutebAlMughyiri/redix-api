// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package config serves the values the app reads at start, before anyone signs in: how long it waits for an answer, the VAT rate, and anything
// later made configurable (UC-070). None is ever a secret: secrets stay in environment variables.
package config

import (
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// Map routes GET /config, open to everyone.
func Map(router *web.Router, database db.DB) {
	values := database.Queries()
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
