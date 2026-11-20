// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package access

import (
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

var (
	openDevice    = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000d1")
	revokedDevice = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000d2")
)

type gated struct {
	handler  http.Handler
	tokens   credentials.Tokens
	database db.DB
}

func gatedRoutes(t *testing.T) gated {
	t.Helper()
	database, book := world(t)
	exec(t, database, device(openDevice, 0), device(revokedDevice, 1))
	tokens := credentials.NewTokens([]byte("test-key-for-go-backend-only-32b"), clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC)))
	gate := NewGate(tokens, database, book, slog.New(slog.NewTextHandler(io.Discard, nil)))
	ok := func(w http.ResponseWriter, r *http.Request, caller Caller) { _, _ = w.Write([]byte(caller.Username)) }
	router := web.NewRouter("")
	router.Handle("GET", "/me", gate.SignedIn(ok))
	router.Handle("GET", "/work", gate.Working(ok))
	router.Handle("GET", "/projects/{projectId:guid}/entries", gate.Needs(RecordEntries, ok))
	router.Handle("POST", "/projects/{projectId:guid}/entries", gate.Changes(RecordEntries, ok))
	return gated{handler: problem.Traced(router), tokens: tokens, database: database}
}

func device(id uuid.UUID, revoked int) string {
	return `INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at) VALUES ('` + id.String() + `', '` +
		user.String() + `', '` + id.String()[24:] + `', '2026-12-16T10:00:00.0000000Z', ` + string(rune('0'+revoked)) + `, 0, '2026-11-16T10:00:00.0000000Z')`
}

func (g gated) ask(path, authorization string) *httptest.ResponseRecorder {
	request := httptest.NewRequest(http.MethodGet, path, nil)
	if authorization != "" {
		request.Header.Set("Authorization", authorization)
	}
	recorder := httptest.NewRecorder()
	g.handler.ServeHTTP(recorder, request)
	return recorder
}

func (g gated) bearer(deviceID uuid.UUID, mustChange bool) string {
	token, _ := g.tokens.Create(credentials.Session{UserID: user, Username: "manager", DeviceID: deviceID, MustChangePassword: mustChange})
	return "Bearer " + token
}

func TestAC02_1_NoTokenIsAskedToSignIn(t *testing.T) {
	got := gatedRoutes(t).ask("/work", "")
	if got.Code != 401 || got.Header().Get("WWW-Authenticate") != "Bearer" || got.Header().Get("Content-Type") != "application/problem+json" {
		t.Errorf("%d %q %s", got.Code, got.Header().Get("WWW-Authenticate"), got.Body)
	}
}

func TestAC02_1_ATokenThatIsNotValidIsNamedInTheChallenge(t *testing.T) {
	got := gatedRoutes(t).ask("/work", "Bearer not-a-token")
	if got.Code != 401 || got.Header().Get("WWW-Authenticate") != `Bearer error="invalid_token"` {
		t.Errorf("%d %q", got.Code, got.Header().Get("WWW-Authenticate"))
	}
}

func TestSessionPolicies_ARevokedDeviceIsRefusedAtItsNextRequest(t *testing.T) {
	routes := gatedRoutes(t)
	if got := routes.ask("/work", routes.bearer(revokedDevice, false)); got.Code != 401 {
		t.Errorf("revoked device: %d", got.Code)
	}
	if got := routes.ask("/work", routes.bearer(openDevice, false)); got.Code != 200 || got.Body.String() != "manager" {
		t.Errorf("open device: %d %s", got.Code, got.Body)
	}
}

func TestAC02_1_ATemporaryPasswordMayOnlyBeChanged(t *testing.T) {
	routes := gatedRoutes(t)
	temporary := routes.bearer(openDevice, true)
	if got := routes.ask("/me", temporary); got.Code != 200 {
		t.Errorf("signed in: %d", got.Code)
	}
	if got := routes.ask("/work", temporary); got.Code != 403 || got.Header().Get("Content-Type") != "application/problem+json" {
		t.Errorf("working: %d %s", got.Code, got.Body)
	}
}

func TestFR03_ARouteOfAProjectNeedsItsPermissionThere(t *testing.T) {
	routes := gatedRoutes(t)
	working := routes.bearer(openDevice, false)
	for project, want := range map[uuid.UUID]int{active: 200, pending: 403, inactive: 403, uuid.New(): 403} {
		if got := routes.ask("/projects/"+project.String()+"/entries", working); got.Code != want {
			t.Errorf("%s: %d, want %d", project, got.Code, want)
		}
	}
}

func TestR4_ARouteValueThatDoesNotFitIsNotFoundBeforeAnyoneIsAsked(t *testing.T) {
	if got := gatedRoutes(t).ask("/projects/12/entries", ""); got.Code != 404 {
		t.Errorf("%d", got.Code)
	}
}

func TestAC04_1_AFinishedProjectRefusesEveryChangeButStillReads(t *testing.T) {
	routes := gatedRoutes(t)
	working := routes.bearer(openDevice, false)
	request := httptest.NewRequest(http.MethodPost, "/projects/"+active.String()+"/entries", nil)
	request.Header.Set("Authorization", working)
	before := httptest.NewRecorder()
	routes.handler.ServeHTTP(before, request)
	if before.Code != 200 {
		t.Fatalf("an active project's change: %d", before.Code)
	}
	exec(t, routes.database, "UPDATE projects SET status = 1 WHERE id = '"+active.String()+"'")
	after := httptest.NewRecorder()
	routes.handler.ServeHTTP(after, request)
	if after.Code != 409 || !strings.Contains(after.Body.String(), `"code":"projectFinished"`) {
		t.Errorf("a finished project's change: %d %s", after.Code, after.Body)
	}
	if got := routes.ask("/projects/"+active.String()+"/entries", working); got.Code != 200 {
		t.Errorf("reading a finished project: %d", got.Code)
	}
}
