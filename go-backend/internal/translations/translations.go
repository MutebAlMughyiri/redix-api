// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package translations serves the words the app shows, one language at a time (UC-001).
package translations

import (
	"crypto/sha256"
	"encoding/hex"
	"net/http"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// Fill is word with each {n} in it filled with fill[n], as the app fills a word: «بانتظار اعتمادك: {0}». The one filler an email and the web
// pages share.
func Fill(word string, fill ...string) string {
	for n, value := range fill {
		word = strings.ReplaceAll(word, "{"+strconv.Itoa(n)+"}", value)
	}
	return word
}

// Map routes GET /translations/{language}, open without sign-in because the sign-in screen needs its words.
func Map(router *web.Router, database db.DB) {
	words := database.Queries()
	// Every word of a language in one bundle with its ETag, for anyone (UC-001).
	router.Handle("GET", "/translations/{language}", func(w http.ResponseWriter, r *http.Request) {
		bundle(w, r, words)
	})
}

// bundle answers every word of a language as one JSON object keyed by word key, with an ETag; 304 while the bundle is unchanged, 404 for a
// language with no words (TranslationsEndpoint.GetBundle).
func bundle(w http.ResponseWriter, r *http.Request, words *store.Queries) {
	rows, err := words.WordsIn(r.Context(), r.PathValue("language"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if len(rows) == 0 {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	byKey := make(map[string]string, len(rows))
	for _, row := range rows {
		byKey[row.Key] = row.Value
	}
	body, _ := encode.JSON(byKey)
	sum := sha256.Sum256(body)
	etag := `"` + strings.ToUpper(hex.EncodeToString(sum[:])) + `"`
	w.Header().Set("ETag", etag)
	if r.Header.Get("If-None-Match") == etag {
		w.WriteHeader(http.StatusNotModified)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	_, _ = w.Write(body)
}
