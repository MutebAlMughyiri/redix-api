package access

import (
	"log/slog"
	"net/http"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/google/uuid"
)

// Caller is who is asking, and what they may do.
type Caller struct {
	credentials.Session
	Access Access
}

// Handler answers a request from a known caller.
type Handler func(w http.ResponseWriter, r *http.Request, caller Caller)

// Gate decides whether a request may reach its handler, as JwtBearer, SessionPolicies and PermissionHandler did: 401 when no open session sends it,
// 403 when the session may not do this. A route open to anyone is mapped without it.
type Gate struct {
	tokens  credentials.Tokens
	devices *store.Queries
	book    *Book
	log     *slog.Logger
}

// NewGate checks tokens with tokens, devices and access in database through book, and writes each refusal of a permission to log (#120 D6).
func NewGate(tokens credentials.Tokens, database db.DB, book *Book, log *slog.Logger) *Gate {
	return &Gate{tokens: tokens, devices: store.New(database.Read), book: book, log: log}
}

// SignedIn lets through anyone signed in on an open device, a temporary password too: changing it is all such a session may do (MayChangePassword).
func (g *Gate) SignedIn(next Handler) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if caller, ok := g.caller(w, r); ok {
			next(w, r, caller)
		}
	}
}

// Working lets through anyone signed in with no temporary password left to change (AC-02.1), the policy of every route not open to all.
func (g *Gate) Working(next Handler) http.HandlerFunc {
	return g.SignedIn(func(w http.ResponseWriter, r *http.Request, caller Caller) {
		if caller.MustChangePassword {
			problem.Status(w, r, http.StatusForbidden)
			return
		}
		next(w, r, caller)
	})
}

// Needs lets through a working caller who holds permission in the route's {projectId}, or everywhere when the route names no project (FR-03).
func (g *Gate) Needs(permission Permission, next Handler) http.HandlerFunc {
	return g.Working(func(w http.ResponseWriter, r *http.Request, caller Caller) {
		projectID, _ := uuid.Parse(r.PathValue("projectId"))
		if !caller.Access.Holds(projectID, permission) {
			g.log.Info("refused", "userId", caller.UserID, "projectId", projectID, "path", r.URL.Path, "permission", permission)
			problem.Status(w, r, http.StatusForbidden)
			return
		}
		next(w, r, caller)
	})
}

// caller is the session the request's bearer token speaks for, on a device still open, with its access; otherwise it answers 401 with a Bearer
// challenge, naming an invalid token when one was sent.
func (g *Gate) caller(w http.ResponseWriter, r *http.Request) (Caller, bool) {
	scheme, token, sent := strings.Cut(r.Header.Get("Authorization"), " ")
	if !sent || !strings.EqualFold(scheme, "Bearer") || token == "" {
		g.challenge(w, r, "Bearer")
		return Caller{}, false
	}
	session, err := g.tokens.Check(token)
	if err == nil {
		var open bool
		open, err = g.devices.DeviceIsOpen(r.Context(), store.DeviceIsOpenParams{DeviceID: session.DeviceID, UserID: session.UserID})
		if err == nil && !open {
			err = credentials.ErrNotValid
		}
	}
	if err != nil {
		g.challenge(w, r, `Bearer error="invalid_token"`)
		return Caller{}, false
	}
	access, err := g.book.Of(r.Context(), session.UserID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return Caller{}, false
	}
	return Caller{Session: session, Access: access}, true
}

func (g *Gate) challenge(w http.ResponseWriter, r *http.Request, header string) {
	w.Header().Set("WWW-Authenticate", header)
	problem.Status(w, r, http.StatusUnauthorized)
}
