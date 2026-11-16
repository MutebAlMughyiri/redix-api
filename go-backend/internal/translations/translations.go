// Package translations serves the words the app shows, one language at a time (UC-001).
package translations

import (
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"net/http"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// Map routes GET /translations/{language}, open without sign-in because the sign-in screen needs its words.
func Map(router *web.Router, database db.DB) {
	words := store.New(database.Read)
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
	body, _ := json.Marshal(byKey)
	sum := sha256.Sum256(body)
	etag := `"` + strings.ToUpper(hex.EncodeToString(sum[:])) + `"`
	w.Header().Set("ETag", etag)
	if r.Header.Get("If-None-Match") == etag {
		w.WriteHeader(http.StatusNotModified)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.Write(body)
}
