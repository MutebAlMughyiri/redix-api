// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package incidents keeps what every request did and every failure was, so a problem in production is found from its trace id (redix-api
// #150): one log line per request, and every answer of 400 or more, refusals included (D1), stored with its trace id, route, code, who asked and,
// for a panic, its stack. Incidents live in a SQLite file of their own beside the database: a failing database must not lose the incident
// about it, and the replay that fails the database call by call must not count their writes. They are kept 90 days (R12). A password, a token,
// an emailed code or a file's content is never stored (R11): only the request's method, route and status, and what the code says.
package incidents

import (
	"context"
	"database/sql"
	"fmt"
	"log/slog"
	"net/http"
	"runtime/debug"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/google/uuid"
	_ "modernc.org/sqlite"
)

// kept is how long an incident stays (R12).
const kept = 90 * 24 * time.Hour

// schema is the one table, made when the file is opened.
const schema = `CREATE TABLE IF NOT EXISTS incidents (
    id INTEGER PRIMARY KEY,
    at TEXT NOT NULL,
    trace_id TEXT CHECK (length(trace_id) <= 55) NOT NULL,
    method TEXT CHECK (length(method) <= 10) NOT NULL,
    route TEXT CHECK (length(route) <= 300) NOT NULL,
    status INTEGER NOT NULL,
    code TEXT CHECK (length(code) <= 60) NOT NULL,
    user_id TEXT CHECK (length(user_id) <= 36) NOT NULL,
    project_id TEXT CHECK (length(project_id) <= 36) NOT NULL,
    duration_ms INTEGER NOT NULL,
    stack TEXT CHECK (length(stack) <= 20000) NOT NULL
) STRICT;
CREATE INDEX IF NOT EXISTS ix_incidents_at ON incidents (at);
CREATE INDEX IF NOT EXISTS ix_incidents_trace_id ON incidents (trace_id);`

// Store is the incidents file.
type Store struct{ db *sql.DB }

// Open opens, or makes, the incidents file at path.
func Open(path string) (*Store, error) {
	db, err := sql.Open("sqlite", "file:"+path+"?_pragma=journal_mode(WAL)&_pragma=busy_timeout(5000)")
	if err != nil {
		return nil, err
	}
	db.SetMaxOpenConns(1)
	if _, err := db.Exec(schema); err != nil {
		db.Close()
		return nil, fmt.Errorf("the incidents file %s: %w", path, err)
	}
	return &Store{db: db}, nil
}

// Close closes the file.
func (s *Store) Close() error { return s.db.Close() }

// Incident is one failed request.
type Incident struct {
	At         moment.Moment
	TraceID    string
	Method     string
	Route      string
	Status     int
	Code       string
	UserID     string
	ProjectID  string
	DurationMs int64
	Stack      string
}

// add keeps an incident, and lets those past their 90 days go.
func (s *Store) add(ctx context.Context, incident Incident) error {
	_, err := s.db.ExecContext(ctx, `INSERT INTO incidents (at, trace_id, method, route, status, code, user_id, project_id, duration_ms, stack)
		VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`, incident.At, incident.TraceID, incident.Method, incident.Route, incident.Status, incident.Code,
		incident.UserID, incident.ProjectID, incident.DurationMs, clip(incident.Stack, 20000))
	if err != nil {
		return err
	}
	_, err = s.db.ExecContext(ctx, `DELETE FROM incidents WHERE at < ?`, moment.Of(incident.At.Add(-kept)))
	return err
}

// routeOf is the request's path with each id or number it carries named {id}, so the failures of one route group together, and never its
// query, which can hold a code or what was searched.
func routeOf(r *http.Request) string {
	segments := strings.Split(r.URL.Path, "/")
	for i, segment := range segments {
		if _, err := uuid.Parse(segment); (err == nil && len(segment) == 36) || (segment != "" && strings.Trim(segment, "0123456789") == "") {
			segments[i] = "{id}"
		}
	}
	return clip(strings.Join(segments, "/"), 300)
}

func clip(text string, length int) string {
	if len(text) > length {
		return text[:length]
	}
	return text
}

// answered keeps the status a handler wrote.
type answered struct {
	http.ResponseWriter
	status int
}

func (a *answered) WriteHeader(status int) {
	if a.status == 0 {
		a.status = status
	}
	a.ResponseWriter.WriteHeader(status)
}

func (a *answered) Write(body []byte) (int, error) {
	if a.status == 0 {
		a.status = http.StatusOK
	}
	return a.ResponseWriter.Write(body)
}

// Unwrap lets http.ResponseController reach the writer beneath, to flush an answer before its work goes on (as forgot-password does).
func (a *answered) Unwrap() http.ResponseWriter { return a.ResponseWriter }

// Recorded wraps the API, inside problem.Traced: it answers a panic as 500 with its code and trace id (R6), writes one log line per request
// (R5), and keeps every answer of 400 or more as an incident (R3, D1). The log line comes first, so an incident the file cannot take is still in
// the log (R8).
func Recorded(next http.Handler, store *Store, log *slog.Logger, clock clock.Clock) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		started := clock.Now()
		writer := &answered{ResponseWriter: w}
		var stack string
		func() {
			defer func() {
				if failure := recover(); failure != nil {
					stack = fmt.Sprintf("%v\n%s", failure, debug.Stack())
					if writer.status == 0 {
						problem.Status(writer, r, http.StatusInternalServerError)
					}
				}
			}()
			next.ServeHTTP(writer, r)
		}()
		if writer.status == 0 {
			writer.status = http.StatusOK
		}
		facts := problem.FactsOf(r.Context())
		incident := Incident{At: moment.Of(started), TraceID: problem.TraceOf(r.Context()), Method: r.Method, Route: routeOf(r), Status: writer.status,
			Code: facts.Code, UserID: facts.UserID, ProjectID: facts.ProjectID, DurationMs: clock.Now().Sub(started).Milliseconds(), Stack: stack}
		attributes := []any{"method", incident.Method, "route", incident.Route, "status", incident.Status, "code", incident.Code,
			"durationMs", incident.DurationMs, "userId", incident.UserID, "projectId", incident.ProjectID, "traceId", incident.TraceID}
		if stack != "" {
			attributes = append(attributes, "stack", stack)
		}
		log.Log(r.Context(), levelOf(incident.Status), "request", attributes...)
		if incident.Status < 400 {
			return
		}
		if err := store.add(context.WithoutCancel(r.Context()), incident); err != nil {
			log.Error("an incident was not kept", "traceId", incident.TraceID, "error", err)
		}
	})
}

// levelOf is how loud a request's log line is: an error for a failure of the API's own, a warning for a refusal, information otherwise.
func levelOf(status int) slog.Level {
	if status >= 500 {
		return slog.LevelError
	}
	if status >= 400 {
		return slog.LevelWarn
	}
	return slog.LevelInfo
}
