// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package web is REDIX's web front end, the second door into the same rules (redix-api #80, #161): pages this binary draws inside one shell, the
// base design the owner approved on 21 Nov 2026, each calling the owner the app's route calls. Every web file lives in this folder; the backend's
// packages hold none (#161 S14).
package web

import (
	"net/http"
	"runtime/debug"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
)

// Version is the web's version, SemVer, raised by hand with each release as pubspec raises the app's (#161 S12).
const Version = "1.0.0"

// build is the web's build beside its version, " (b6ef37d)": the commit this program was built from, which Go stamps into every build made in
// the repository, so a tester tells one deploy from the next with no step to forget (the owner, 21 Nov 2026: "add web build number to the login
// page"). "" in a build with no commit stamped, as a test's.
var build = func() string {
	info, _ := debug.ReadBuildInfo()
	for _, setting := range info.Settings {
		if setting.Key == "vcs.revision" {
			return " (" + setting.Value[:min(7, len(setting.Value))] + ")"
		}
	}
	return ""
}()

// Parts are what the pages run on: the same database, sessions, codes, gate and access book as the app's API, and the same windows that cap how
// often one address asks for an emailed code and registers a project.
type Parts struct {
	Database      db.DB
	Sessions      auth.Sessions
	Codes         auth.Codes
	EmailCodes    *limits.Window
	Registrations *limits.Window
	Gate          *access.Gate
	Book          *access.Book
	Clock         clock.Clock
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
	mux.HandleFunc("GET /sign-in", s.signInAgain)
	mux.HandleFunc("POST /sign-in", s.signIn)
	mux.HandleFunc("GET /forgot-password", s.forgotPassword)
	mux.HandleFunc("POST /forgot-password", s.sendResetCode)
	mux.HandleFunc("POST /reset-with-code", s.resetWithCode)
	mux.HandleFunc("GET /register", s.registerForm)
	mux.HandleFunc("POST /register", s.register)
	mux.HandleFunc("POST /register/{list}", s.listed)
	mux.HandleFunc("POST /change-password", s.changePassword)
	mux.HandleFunc("POST /sign-out", s.signOut)
	mux.HandleFunc("GET /tab/{name}", s.openTab)
	mux.HandleFunc("DELETE /tab/{name}", s.closeTab)
	mux.HandleFunc("POST /view", s.keepView)
	mux.HandleFunc("GET /details/entry/{id}", s.entryDetails)
	mux.HandleFunc("POST /entry/{id}/{action}", s.entryAction)
	mux.HandleFunc("POST /entry", s.recordEntry)
	mux.HandleFunc("POST /entry/{id}", s.changeEntry)
	mux.HandleFunc("GET /entry-figures", s.entryFigures)
	mux.HandleFunc("POST /approvals/approve", s.approveSelected)
	mux.HandleFunc("GET /approval-history", s.approvalHistoryMore)
	mux.HandleFunc("POST /project/{id}", s.chooseProject)
	mux.HandleFunc("POST /project/{id}/default", s.starProject)
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
