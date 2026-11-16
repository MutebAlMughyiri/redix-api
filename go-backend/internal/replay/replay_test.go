// Package replay drives the Go API with what .NET's tests asked of the .NET API, and compares the answers with what .NET answered (redix-api #140,
// R4, R23, G17): every recorded test whose routes are all on Go, on a fresh database, with the factory's seeds at .NET's own ids, the clock at each
// request's recorded moment, and .NET's ids, tokens, ETags and emailed codes mapped to Go's. Any other difference fails: a status, a content type,
// a key, a value - and a number as written, so 700.0000 is not 700.
package replay

import (
	"bufio"
	"bytes"
	"compress/gzip"
	"context"
	"database/sql"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"mime"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"sort"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/server"
	"github.com/google/uuid"
)

// The factory's admin, as MigratedApiFactory configured it.
const (
	adminUsername = "admin"
	adminPassword = "Admin-test-password"
)

// step is one line of a recording.
type step struct {
	Test          string            `json:"test"`
	Step          string            `json:"step"`
	At            time.Time         `json:"at"`
	Method        string            `json:"method"`
	Path          string            `json:"path"`
	Query         string            `json:"query"`
	Headers       map[string]string `json:"headers"`
	ContentType   string            `json:"contentType"`
	Body          json.RawMessage   `json:"body"`
	Status        int               `json:"status"`
	AnswerType    string            `json:"answerType"`
	AnswerHeaders map[string]string `json:"answerHeaders"`
	Answer        json.RawMessage   `json:"answer"`
	Kind          string            `json:"kind"`
	Given         json.RawMessage   `json:"given"`
	To            string            `json:"to"`
	Host          int               `json:"host"`
	EmailCodes    int               `json:"emailCodesPerHour"`
	file          string
}

// host is one .NET host a recording's requests went to, and the limit it was set up with; a test that built a host of its own gets a router of its own.
type host struct {
	file       string
	id         int
	emailCodes int
}

// recorded is every recorded test, by name, its steps in order, and every host the recordings name.
func recorded(t *testing.T) (map[string][]step, []string, []host) {
	t.Helper()
	files, _ := filepath.Glob(filepath.Join("..", "..", "testdata", "golden", "*.jsonl.gz"))
	if len(files) == 0 {
		t.Fatal("no recordings in testdata/golden")
	}
	tests, order, hosts := map[string][]step{}, []string{}, []host{}
	for _, file := range files {
		opened, err := os.Open(file)
		if err != nil {
			t.Fatal(err)
		}
		unzipped, err := gzip.NewReader(opened)
		if err != nil {
			t.Fatal(err)
		}
		lines := bufio.NewScanner(unzipped)
		lines.Buffer(make([]byte, 64<<20), 64<<20)
		for lines.Scan() {
			var s step
			if err := json.Unmarshal(lines.Bytes(), &s); err != nil {
				t.Fatal(err)
			}
			s.file = file
			if s.Step == "host" {
				hosts = append(hosts, host{file: file, id: s.Host, emailCodes: s.EmailCodes})
			}
			if s.Test == "" {
				continue
			}
			if _, seen := tests[s.Test]; !seen {
				order = append(order, s.Test)
			}
			tests[s.Test] = append(tests[s.Test], s)
		}
		opened.Close()
	}
	return tests, order, hosts
}

// seeds is what a replay can put straight into the database, as the factory's helpers did.
var seeds = []string{"user", "give", "inactive"}

func TestReplayDotNetsAnswers(t *testing.T) {
	tests, order, hosts := recorded(t)
	discovery, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "routes.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer discovery.Close()
	routes := server.Routes(server.Parts{Database: discovery, Clock: clock.System{}, SigningKey: []byte("k"), Log: quiet})
	var replayed, byHand, waiting int
	for _, name := range order {
		steps := tests[name]
		switch {
		case slices.ContainsFunc(steps, func(s step) bool { return s.Step == "direct" }):
			byHand++
			continue
		case slices.ContainsFunc(steps, func(s step) bool {
			return (s.Step == "request" && !routes.Handles(s.Method, s.Path)) || (s.Step == "seed" && !slices.Contains(seeds, s.Kind))
		}):
			waiting++
			continue
		}
		replayed++
		t.Run(name, func(t *testing.T) { replay(t, steps, hosts) })
	}
	t.Logf("%d tests replayed against .NET's answers; %d wait for routes not yet on Go; %d write the database directly and are ported by hand", replayed, waiting, byHand)
}

var quiet = slog.New(slog.NewTextHandler(io.Discard, nil))

// kept is the replay's email sender: every email, in order.
type kept struct {
	mu   sync.Mutex
	sent []string
}

func (k *kept) Deliver(_ context.Context, to, subject, body string) error {
	k.mu.Lock()
	defer k.mu.Unlock()
	k.sent = append(k.sent, to+"\n"+body)
	return nil
}

// nthTo is the body of the nth email sent to address, counting from 0, or "" while Go has not sent it yet.
func (k *kept) nthTo(address string, n int) string {
	k.mu.Lock()
	defer k.mu.Unlock()
	for _, email := range k.sent {
		if to, body, _ := strings.Cut(email, "\n"); to == address {
			if n == 0 {
				return body
			}
			n--
		}
	}
	return ""
}

// sentCode is one email .NET sent, waiting for Go's same email - the same address, the same place among its emails - to map its code.
type sentCode struct {
	to   string
	nth  int
	code string
}

var code = regexp.MustCompile(`\d{6}`)

func replay(t *testing.T, steps []step, hosts []host) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer database.Close()
	fake := clock.NewFake(steps[0].At)
	if err := auth.SeedAdmin(context.Background(), database, adminUsername, adminPassword, moment.Of(steps[0].At)); err != nil {
		t.Fatal(err)
	}
	emails := &kept{}
	handlers := map[int]http.Handler{}
	handlerOf := func(s step) http.Handler {
		if handler, built := handlers[s.Host]; built {
			return handler
		}
		limit := 100000
		for _, known := range hosts {
			if known.file == s.file && known.id == s.Host {
				limit = known.emailCodes
			}
		}
		handlers[s.Host] = problem.Traced(server.Routes(server.Parts{Database: database, Clock: fake, SigningKey: []byte("replay-key-for-go-backend-only-32"),
			Sender: emails, EmailCodesPerHour: limit, Log: quiet}))
		return handlers[s.Host]
	}
	names := mapping{}
	var pending []sentCode
	sentTo := map[string]int{}
	resolve := func() {
		var left []sentCode
		for _, sent := range pending {
			if goCode := code.FindString(emails.nthTo(sent.to, sent.nth)); goCode != "" {
				names[sent.code] = goCode
			} else {
				left = append(left, sent)
			}
		}
		pending = left
	}
	for i, s := range steps {
		switch s.Step {
		case "seed":
			seed(t, database, names, s)
		case "email":
			var body string
			json.Unmarshal(s.Body, &body)
			if netCode := code.FindString(body); netCode != "" {
				pending = append(pending, sentCode{to: s.To, nth: sentTo[s.To], code: netCode})
			}
			sentTo[s.To]++
			resolve()
		case "request":
			fake.Set(s.At)
			got := send(handlerOf(s), names, s)
			resolve()
			if differences := compare(names, s, got); len(differences) > 0 {
				t.Errorf("step %d, %s %s%s:\n  %s", i, s.Method, s.Path, s.Query, strings.Join(differences, "\n  "))
				return
			}
		}
	}
}

// mapping is .NET's value of an id, token, ETag or emailed code, to Go's.
type mapping map[string]string

// into is text with every .NET value it holds replaced by Go's, longest first so no value is cut inside another.
func (m mapping) into(text string) string {
	keys := make([]string, 0, len(m))
	for key := range m {
		keys = append(keys, key)
	}
	sort.Slice(keys, func(i, j int) bool { return len(keys[i]) > len(keys[j]) })
	for _, key := range keys {
		text = strings.ReplaceAll(text, key, m[key])
	}
	return text
}

func seed(t *testing.T, database db.DB, names mapping, s step) {
	t.Helper()
	write := func(statement string, args ...any) {
		if err := database.InTx(context.Background(), func(tx *sql.Tx) error { _, err := tx.Exec(statement, args...); return err }); err != nil {
			t.Fatalf("seed %s: %v", s.Kind, err)
		}
	}
	switch s.Kind {
	case "user":
		var given struct {
			ID                 uuid.UUID `json:"id"`
			Username           string    `json:"username"`
			Password           string    `json:"password"`
			MustChangePassword bool      `json:"mustChangePassword"`
			IsAdmin            bool      `json:"isAdmin"`
			Email              string    `json:"email"`
			CreatedAt          time.Time `json:"createdAt"`
		}
		json.Unmarshal(s.Given, &given)
		created, _ := moment.Of(given.CreatedAt).Value()
		write(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
			VALUES (?, ?, '', '', ?, ?, 0, '1900-01-01T00:00:00.0000000Z', ?, ?)`, given.ID, given.Username, credentials.Hash(given.Password), given.MustChangePassword, created, given.Email)
		if given.IsAdmin {
			write(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), given.ID)
		}
	case "give":
		var given struct {
			UserID      string `json:"userId"`
			ProjectID   string `json:"projectId"`
			Permissions []int  `json:"permissions"`
		}
		json.Unmarshal(s.Given, &given)
		for _, number := range given.Permissions {
			write(`INSERT INTO project_permissions (id, project_id, user_id, permission) SELECT ?, ?, ?, ? WHERE NOT EXISTS
				(SELECT 1 FROM project_permissions WHERE project_id = ? AND user_id = ? AND permission = ?)`,
				uuid.NewString(), names.into(given.ProjectID), names.into(given.UserID), string(access.All[number]),
				names.into(given.ProjectID), names.into(given.UserID), string(access.All[number]))
		}
	case "inactive":
		var given struct {
			UserID    string `json:"userId"`
			ProjectID string `json:"projectId"`
		}
		json.Unmarshal(s.Given, &given)
		write(`INSERT INTO inactive_members (id, project_id, user_id) VALUES (?, ?, ?)`, uuid.NewString(), names.into(given.ProjectID), names.into(given.UserID))
	}
}

// send asks the Go API what s asked the .NET API, with .NET's values replaced by Go's.
func send(handler http.Handler, names mapping, s step) *httptest.ResponseRecorder {
	var body []byte
	var text string
	if json.Unmarshal(s.Body, &text) == nil {
		body = []byte(names.into(text))
	} else {
		var file struct {
			Base64 string `json:"base64"`
		}
		json.Unmarshal(s.Body, &file)
		body, _ = base64.StdEncoding.DecodeString(file.Base64)
	}
	request := httptest.NewRequest(s.Method, names.into(s.Path+s.Query), bytes.NewReader(body))
	if s.ContentType != "" {
		request.Header.Set("Content-Type", s.ContentType)
	}
	for name, value := range s.Headers {
		request.Header.Set(name, names.into(value))
	}
	recorder := httptest.NewRecorder()
	handler.ServeHTTP(recorder, request)
	return recorder
}

// learned are the values that are Go's own by nature, so they are mapped rather than compared: ids, tokens and ETags.
var guid = regexp.MustCompile(`^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$`)

var tokenKeys = []string{"accessToken", "refreshToken"}

func compare(names mapping, s step, got *httptest.ResponseRecorder) []string {
	var differences []string
	if got.Code != s.Status {
		differences = append(differences, fmt.Sprintf("status %d, .NET answered %d", got.Code, s.Status))
	}
	netType, _, _ := mime.ParseMediaType(s.AnswerType)
	goType, _, _ := mime.ParseMediaType(got.Header().Get("Content-Type"))
	if netType != goType {
		differences = append(differences, fmt.Sprintf("content type %q, .NET answered %q", goType, netType))
	}
	for name, value := range s.AnswerHeaders {
		switch name {
		case "ETag":
			names[value] = got.Header().Get(name)
		default:
			if goValue := got.Header().Get(name); names.into(value) != goValue {
				differences = append(differences, fmt.Sprintf("header %s %q, .NET answered %q", name, goValue, value))
			}
		}
	}
	var answer string
	if json.Unmarshal(s.Answer, &answer) != nil {
		return differences
	}
	if !strings.Contains(netType, "json") {
		if names.into(answer) != got.Body.String() {
			differences = append(differences, fmt.Sprintf("body %q, .NET answered %q", got.Body.String(), answer))
		}
		return differences
	}
	netJSON, goJSON := decode(answer), decode(got.Body.String())
	return append(differences, same(names, "$", "", netJSON, goJSON)...)
}

func decode(text string) any {
	decoder := json.NewDecoder(strings.NewReader(text))
	decoder.UseNumber()
	var value any
	if decoder.Decode(&value) != nil {
		return text
	}
	return value
}

// same is every difference between .NET's value and Go's at path, learning a new id or token as it goes.
func same(names mapping, path, key string, net, got any) []string {
	switch net := net.(type) {
	case map[string]any:
		got, ok := got.(map[string]any)
		if !ok {
			return []string{fmt.Sprintf("%s: an object in .NET's answer, %v in Go's", path, got)}
		}
		var differences []string
		for name, value := range net {
			if _, has := got[name]; !has {
				differences = append(differences, fmt.Sprintf("%s.%s: missing in Go's answer", path, name))
				continue
			}
			if name == "traceId" {
				continue
			}
			differences = append(differences, same(names, path+"."+name, name, value, got[name])...)
		}
		for name := range got {
			if _, has := net[name]; !has {
				differences = append(differences, fmt.Sprintf("%s.%s: not in .NET's answer", path, name))
			}
		}
		return differences
	case []any:
		got, ok := got.([]any)
		if !ok || len(got) != len(net) {
			return []string{fmt.Sprintf("%s: %d items in .NET's answer, %v in Go's", path, len(net), got)}
		}
		var differences []string
		for i := range net {
			differences = append(differences, same(names, fmt.Sprintf("%s[%d]", path, i), key, net[i], got[i])...)
		}
		return differences
	case string:
		got, ok := got.(string)
		switch {
		case !ok:
			return []string{fmt.Sprintf("%s: %q in .NET's answer, %v in Go's", path, net, got)}
		case names.into(net) == got:
			return nil
		case slices.Contains(tokenKeys, key), guid.MatchString(net) && guid.MatchString(got):
			if _, known := names[net]; !known {
				names[net] = got
				return nil
			}
		}
		return []string{fmt.Sprintf("%s: %q, .NET answered %q", path, got, net)}
	default:
		if fmt.Sprint(net) != fmt.Sprint(got) {
			return []string{fmt.Sprintf("%s: %v, .NET answered %v", path, got, net)}
		}
		return nil
	}
}
