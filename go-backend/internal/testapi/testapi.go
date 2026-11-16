// Package testapi is the one way a domain's tests ask the API (redix-api #140): the whole router as the binary serves it, on a fresh database,
// every request under the History net, so no test can send one without it. A guard test keeps every domain's tests going through here.
package testapi

import (
	"context"
	"database/sql"
	"encoding/json"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"regexp"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history/historytest"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/server"
	"github.com/google/uuid"
)

// signingKey signs the tests' tokens.
var signingKey = []byte("test-key-for-go-backend-only-32b")

// API is the API on a fresh database, standing at 10:00 UTC on 16 Nov 2026 until a test moves its clock.
type API struct {
	t        *testing.T
	Database db.DB
	Clock    *clock.Fake
	Emails   *Emails
	handler  http.Handler
	tokens   credentials.Tokens
}

// New is the API on a fresh database of t's own.
func New(t *testing.T) *API {
	t.Helper()
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { database.Close() })
	fake := clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))
	emails := &Emails{}
	routes := server.Routes(server.Parts{Database: database, Clock: fake, SigningKey: signingKey, Sender: emails, EmailCodesPerHour: 100000,
		RegistrationsPerHour: 100000, Log: slog.New(slog.NewTextHandler(io.Discard, nil))})
	return &API{t: t, Database: database, Clock: fake, Emails: emails, handler: problem.Traced(routes), tokens: credentials.NewTokens(signingKey, fake)}
}

// Send asks method on path with body as JSON, and headers as name and value pairs, under the History net.
func (a *API) Send(method, path, body string, headers ...string) *httptest.ResponseRecorder {
	a.t.Helper()
	request := httptest.NewRequest(method, path, strings.NewReader(body))
	request.Header.Set("Content-Type", "application/json")
	for i := 0; i+1 < len(headers); i += 2 {
		request.Header.Set(headers[i], headers[i+1])
	}
	recorder := httptest.NewRecorder()
	historytest.Check(a.t, a.Database, func() { a.handler.ServeHTTP(recorder, request) })
	return recorder
}

// Ask is Send as token - none when "" - with the status and the JSON answer.
func (a *API) Ask(method, path, body, token string) (int, any) {
	a.t.Helper()
	var headers []string
	if token != "" {
		headers = []string{"Authorization", "Bearer " + token}
	}
	answered := a.Send(method, path, body, headers...)
	var answer any
	json.Unmarshal(answered.Body.Bytes(), &answer)
	return answered.Code, answer
}

// Exec writes straight into the database, as a test sets up what it needs, as the .NET factory's seeds did.
func (a *API) Exec(statement string, args ...any) {
	a.t.Helper()
	err := a.Database.InTx(context.Background(), func(tx *sql.Tx) error { _, err := tx.Exec(statement, args...); return err })
	if err != nil {
		a.t.Fatal(err)
	}
}

// Token is a bearer token of userID on a device of their own, open for 12 hours.
func (a *API) Token(userID uuid.UUID, mustChangePassword bool) string {
	a.t.Helper()
	device := uuid.Must(uuid.NewV7())
	a.Exec(`INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at)
		VALUES (?, ?, ?, '2026-11-17T10:00:00.0000000Z', 0, 0, '2026-11-16T10:00:00.0000000Z')`, device, userID, device.String())
	token, _ := a.tokens.Create(credentials.Session{UserID: userID, Username: "u", DeviceID: device, MustChangePassword: mustChangePassword})
	return token
}

// Count is the number a query of one count answers.
func (a *API) Count(query string, args ...any) int {
	a.t.Helper()
	var n int
	if err := a.Database.Read.QueryRow(query, args...).Scan(&n); err != nil {
		a.t.Fatal(err)
	}
	return n
}

// Field is name in an answer that is a JSON object; nil otherwise.
func Field(answer any, name string) any {
	object, _ := answer.(map[string]any)
	return object[name]
}

// Emails keeps every email the API sends, in order, instead of sending it.
type Emails struct {
	mu   sync.Mutex
	sent []sentEmail
}

type sentEmail struct{ to, body string }

// Deliver keeps the email.
func (e *Emails) Deliver(_ context.Context, to, _, body string) error {
	e.mu.Lock()
	defer e.mu.Unlock()
	e.sent = append(e.sent, sentEmail{to: to, body: body})
	return nil
}

// Count is how many emails were sent.
func (e *Emails) Count() int {
	e.mu.Lock()
	defer e.mu.Unlock()
	return len(e.sent)
}

var sixDigits = regexp.MustCompile(`\b\d{6}\b`)

// LastCodeTo is the six-digit code of the last email sent to address, or "" when none was.
func (e *Emails) LastCodeTo(address string) string {
	e.mu.Lock()
	defer e.mu.Unlock()
	for i := len(e.sent) - 1; i >= 0; i-- {
		if e.sent[i].to == address {
			return sixDigits.FindString(e.sent[i].body)
		}
	}
	return ""
}
