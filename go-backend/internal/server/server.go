// Package server wires every domain's routes into one router: the one the binary serves, and the one the replays of .NET's recorded answers drive
// (redix-api #140), so the two can never differ.
package server

import (
	"log/slog"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/accounts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/contracts"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/config"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/email"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/settings"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/translations"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
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
	WebFolder            string
	Log                  *slog.Logger
}

// Routes is every route of the API on parts.
func Routes(parts Parts) *web.Router {
	database, now := parts.Database, parts.Clock
	tokens := credentials.NewTokens(parts.SigningKey, now)
	book := access.NewBook(database)
	gate := access.NewGate(tokens, database, book, parts.Log)

	router := web.NewRouter(parts.WebFolder)
	router.Handle("GET", "/health", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		if database.Read.PingContext(r.Context()) != nil {
			w.WriteHeader(http.StatusServiceUnavailable)
			w.Write([]byte("Unhealthy"))
			return
		}
		w.Write([]byte("Healthy"))
	})
	translations.Map(router, database)
	config.Map(router, database)
	auth.MapSessions(router, gate, database, tokens, book, now)
	auth.MapCodes(router, gate, database, now, parts.Sender, limits.PerHour(parts.EmailCodesPerHour, now), parts.Log)
	users.Map(router, gate, database, now)
	projects.Map(router, gate, database, book, now, limits.PerHour(parts.RegistrationsPerHour, now))
	structure.Map(router, gate, database, now)
	settings.Map(router, gate, database, now)
	suppliers.Map(router, gate, database, now)
	accounts.Map(router, gate, database, now)
	contracts.Map(router, gate, database, now)
	return router
}
