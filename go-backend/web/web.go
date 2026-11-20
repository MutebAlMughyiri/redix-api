// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package web is REDIX's web front end, the second door into the same rules (redix-api #80, #161): pages this binary draws inside one shell, the
// base design the owner approved on 21 Nov 2026, each calling the owner the app's route calls. Every web file lives in this folder; the backend's
// packages hold none (#161 S14).
package web

import (
	"net/http"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
)

// Version is the web's version, SemVer, raised by hand with each release as pubspec raises the app's (#161 S12).
const Version = "1.0.0"

// Parts are what the pages run on: the same database, sessions and gate as the app's API.
type Parts struct {
	Database db.DB
	Sessions auth.Sessions
	Gate     *access.Gate
	Clock    clock.Clock
}

// site draws the pages.
type site struct {
	Parts
	assets assets
}

// Routes is every page of the web, behind Go's own cross-origin check, which refuses a form another site posts (#161 R17).
func Routes(parts Parts) http.Handler {
	s := site{Parts: parts, assets: loadAssets()}
	mux := http.NewServeMux()
	mux.HandleFunc("GET /{$}", s.home)
	mux.HandleFunc("GET /assets/{file}", s.assets.serve)
	mux.HandleFunc("POST /sign-in", s.signIn)
	mux.HandleFunc("POST /change-password", s.changePassword)
	mux.HandleFunc("POST /sign-out", s.signOut)
	mux.HandleFunc("GET /tab/{name}", s.openTab)
	mux.HandleFunc("DELETE /tab/{name}", s.closeTab)
	mux.HandleFunc("POST /view", s.keepView)
	mux.HandleFunc("GET /language/{language}", s.chooseLanguage)
	return http.NewCrossOriginProtection().Handler(mux)
}

// draw answers status with components, one after another: a page holds a person's data, so no answer is kept by a cache.
func draw(w http.ResponseWriter, r *http.Request, status int, components ...templ.Component) {
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.Header().Set("Cache-Control", "no-store")
	w.WriteHeader(status)
	for _, component := range components {
		_ = component.Render(r.Context(), w)
	}
}

// failed answers a database that failed, as the API answers it: 500.
func failed(w http.ResponseWriter) {
	http.Error(w, http.StatusText(http.StatusInternalServerError), http.StatusInternalServerError)
}
