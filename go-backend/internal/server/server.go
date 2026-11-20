// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package server wires every domain's routes into one router: the one the binary serves, and the one the replays of .NET's recorded answers drive
// (redix-api #140), so the two can never differ; and beside it the web's pages, on the same rules (#161).
package server

import (
	"log/slog"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/apidoc"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/archive"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/config"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/documents"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/email"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/legal"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projectfile"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/reports"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/sync"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/translations"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	site "github.com/MutebAlMughyiri/redix-api/go-backend/web"
)

// Parts are what the routes run on.
type Parts struct {
	Database          db.DB
	Clock             clock.Clock
	SigningKey        []byte
	Sender            email.Sender
	EmailCodesPerHour int
	// RegistrationsPerHour is how often one address may register a project an hour (REDIX_Registration__PermitsPerHour).
	RegistrationsPerHour int
	// Documented serves the OpenAPI document and Swagger UI, as Dev and QA do (redix-api #142 R4).
	Documented bool
	WebFolder  string
	// AttachmentsFolder keeps the files people send (REDIX_Attachments__Folder, NFR-10).
	AttachmentsFolder string
	// TypstPath is the typst binary that makes the report PDFs (REDIX_Typst__Path, #140 D2).
	TypstPath string
	Log       *slog.Logger
}

// Doors are the two ways into REDIX, on one set of rules (redix-api #161 S14): the app's API, answering JSON, and the web's pages, answering HTML.
// Both stand on the same sessions, gate and access book, so a write either makes is seen by both.
type Doors struct {
	API   *web.Router
	Pages http.Handler
}

// Routes is every route of the API, and every page of the web, on parts.
func Routes(parts Parts) Doors {
	database, now := parts.Database, parts.Clock
	links := attachments.NewLinks(parts.SigningKey, now)
	tokens := credentials.NewTokens(parts.SigningKey, now)
	book := access.NewBook(database)
	gate := access.NewGate(tokens, database, book, parts.Log)
	sessions := auth.NewSessions(database, tokens, book, now)

	router := web.NewRouter(parts.WebFolder)
	// Healthy while the API runs and reaches its database, otherwise 503 Unhealthy (UC-001).
	router.Handle("GET", "/health", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		if database.Read.PingContext(r.Context()) != nil {
			w.WriteHeader(http.StatusServiceUnavailable)
			_, _ = w.Write([]byte("Unhealthy"))
			return
		}
		_, _ = w.Write([]byte("Healthy"))
	})
	translations.Map(router, database)
	legal.Map(router)
	if parts.Documented {
		apidoc.Map(router)
	}
	config.Map(router, database)
	auth.MapSessions(router, gate, sessions)
	auth.MapCodes(router, gate, database, now, parts.Sender, limits.PerHour(parts.EmailCodesPerHour, now), parts.Log)
	users.Map(router, gate, database, now)
	projects.Map(router, gate, database, book, now, limits.PerHour(parts.RegistrationsPerHour, now))
	structure.Map(router, gate, database, now)
	settings.Map(router, gate, database, now)
	suppliers.Map(router, gate, database, now)
	accounts.Map(router, gate, database, now)
	contracts.Map(router, gate, database, now)
	entries.Map(router, gate, database, now)
	notifications.Map(router, gate, database)
	partners.Map(router, gate, database, now)
	attachments.Map(router, gate, database, now, parts.AttachmentsFolder, links)
	documents.Map(router, gate, database, now, parts.AttachmentsFolder)
	reports.Map(router, gate, database, now, links, statement.Typst{Path: parts.TypstPath})
	projectfile.Map(router, gate, database, now)
	archive.Map(router, gate, database, now)
	sync.Map(router, gate)
	return Doors{API: router, Pages: site.Routes(site.Parts{Database: database, Sessions: sessions, Gate: gate, Clock: now})}
}
