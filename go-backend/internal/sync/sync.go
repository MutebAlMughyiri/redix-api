// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package sync answers what a phone keeps so it can work without a network (UC-030, FR-16): every list a person may see and needs to enter an
// entry, by the address the app reads it at, so a kept copy answers that same read (SyncEndpoints; redix-api #140).
package sync

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// syncView is what GET /projects/{projectId}/sync answers (UC-030).
type syncView struct {
	Stamp string                     `json:"stamp"`
	Lists map[string]json.RawMessage `json:"lists"`
}

// Map routes the sync, for whoever reads the project. Each list is read through routes itself, as the app reads it, so every list has the one
// owner its own route is.
func Map(router *web.Router, gate *access.Gate) {
	router.Handle("GET", "/projects/{projectId:guid}/sync", gate.Needs(access.ReadProject, func(w http.ResponseWriter, r *http.Request, _ access.Caller) {
		project := "/projects/" + web.ID(r, "projectId").String()
		lists := map[string]json.RawMessage{}
		for _, address := range []string{"/categories/offered?itemType=purchases", "/categories/offered?itemType=contractorPayments", "/entry-tables",
			"/suppliers", "/suppliers/search?text=", "/contracts", "/accounts", "/accounts/offered", "/payment-methods", "/payment-methods/offered", "/entries/mine"} {
			answer, ok := read(router, r, project+address)
			if !ok {
				problem.Status(w, r, http.StatusInternalServerError)
				return
			}
			lists[project+address] = answer
		}
		var categories []struct {
			Items []struct {
				ID string `json:"id"`
			} `json:"items"`
		}
		var paying []struct {
			Items []struct {
				ID string `json:"id"`
			} `json:"items"`
		}
		json.Unmarshal(lists[project+"/categories/offered?itemType=purchases"], &categories)
		json.Unmarshal(lists[project+"/categories/offered?itemType=contractorPayments"], &paying)
		for _, category := range append(categories, paying...) {
			for _, item := range category.Items {
				address := project + "/items/" + item.ID + "/columns"
				if _, seen := lists[address]; seen {
					continue
				}
				answer, ok := read(router, r, address)
				if !ok {
					problem.Status(w, r, http.StatusInternalServerError)
					return
				}
				lists[address] = answer
			}
		}
		written, _ := json.Marshal(lists)
		hash := sha256.Sum256(written)
		stamp := hex.EncodeToString(hash[:])
		if r.URL.Query().Get("since") == stamp {
			lists = map[string]json.RawMessage{}
		}
		web.JSON(w, r, http.StatusOK, syncView{stamp, lists})
	}))
}

// read is what routes answers the caller at address, with the caller's own headers; false for anything but 200.
func read(routes http.Handler, r *http.Request, address string) (json.RawMessage, bool) {
	ask, err := http.NewRequestWithContext(r.Context(), http.MethodGet, address, nil)
	if err != nil {
		return nil, false
	}
	ask.Header = r.Header.Clone()
	answer := &recorded{header: http.Header{}, status: http.StatusOK}
	routes.ServeHTTP(answer, ask)
	return answer.body.Bytes(), answer.status == http.StatusOK
}

// recorded keeps what a route answered.
type recorded struct {
	header http.Header
	status int
	body   bytes.Buffer
}

func (a *recorded) Header() http.Header         { return a.header }
func (a *recorded) Write(b []byte) (int, error) { return a.body.Write(b) }
func (a *recorded) WriteHeader(status int)      { a.status = status }
