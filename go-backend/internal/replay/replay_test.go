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
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"maps"
	"math"
	"mime"
	"net/http"
	"net/http/httptest"
	"net/url"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"sort"
	"strconv"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/xuri/excelize/v2"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/dbtest"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history/historytest"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/server"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
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
	Registrations int               `json:"registrationsPerHour"`
	Rows          []directRow       `json:"rows"`
	file          string
}

// host is one .NET host a recording's requests went to, and the limits it was set up with; a test that built a host of its own gets a router of
// its own.
type host struct {
	file          string
	id            int
	emailCodes    int
	registrations int
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
				hosts = append(hosts, host{file: file, id: s.Host, emailCodes: s.EmailCodes, registrations: s.Registrations})
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

// readTheDatabase are the tests that send an id they read straight from .NET's database, which no answer showed - an item the approval copied,
// read with WithDatabaseAsync - so the replay cannot know which of Go's rows it means; like a direct write, each is ported by hand.
var readTheDatabase = []string{
	"SuppliersTests.FinanceAddsASupplierWithAllItsDetails",
	"SuppliersTests.AnEditChangesASupplierAndLeavesItItsOwnNumber",
	"SuppliersTests.AnEditCanTakeTheDefaultItemAndTheVatNumberAway",
	"CategoriesTests.R10_AnItemOffersAnExpenseItsGeneralTableEachNamedOneAndEachActiveContractInItsSheetsOrder",
}

// ranAtOnce are the tests that send requests at the same moment, whose outcome - which entry took which number - is the order they reached .NET's
// database, which a replay one request at a time cannot repeat; each is ported by hand.
var ranAtOnce = []string{
	"EntryNumberTests.G1_EntriesArrivingAtTheSameMomentEachTakeANumberOfTheirOwn",
	"PasswordResetTests.D3_TriesSentAtOnceCheckNoMoreGuessesThanACodeAllows",
}

// signedByDotNet are the tests that open a link .NET's Data Protection signed, read from a cell of its workbook, which Go's key cannot open by
// design (redix-api #140 G6); Go's links have their own test, in attachments.
var signedByDotNet = []string{
	"ReportFilesTests.Muteb16Nov_AWorkbooksFileCellOpensItsFileWithNoSignInForSevenDaysAndAChangedLinkOpensNothing",
}

// isGone says whether s asked for a route .NET did not have - one removed, or a method a path does not take - which Go's router answers as
// ASP.NET Core's did, 404 or 405, with no route of its own.
func isGone(s step) bool {
	return s.Status == http.StatusNotFound || s.Status == http.StatusMethodNotAllowed
}

// seeds is what a replay can put straight into the database, as the factory's helpers did.
var seeds = []string{"user", "give", "inactive", "approve"}

func TestReplayDotNetsAnswers(t *testing.T) {
	tests, order, hosts := recorded(t)
	discovery, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "routes.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer discovery.Close()
	routes := server.Routes(server.Parts{Database: discovery, Clock: clock.System{}, SigningKey: []byte("k"), Log: quiet})
	var replayed, byHand, waiting, underReview int
	for _, name := range order {
		steps := tests[name]
		switch {
		case slices.ContainsFunc(steps, func(s step) bool { return s.Step == "request" && s.Status == http.StatusInternalServerError }):
			// .NET crashed on some of its requests: the rest are compared, and what Go answers to those waits for Muteb's review (redix-api #76
			// rows 263, 264).
			underReview++
		case !replayable(name, steps), slices.Contains(readTheDatabase, name), slices.Contains(ranAtOnce, name), slices.Contains(signedByDotNet, name):
			byHand++
			continue
		case slices.ContainsFunc(steps, func(s step) bool {
			return (s.Step == "request" && !routes.Handles(s.Method, s.Path) && !isGone(s)) || (s.Step == "seed" && !slices.Contains(seeds, s.Kind))
		}):
			waiting++
			continue
		}
		replayed++
		t.Run(name, func(t *testing.T) {
			t.Parallel()
			replay(t, steps, hosts)
		})
	}
	t.Logf("%d tests replayed against .NET's answers; %d wait for routes not yet on Go; %d are ported by hand - about SQL Server itself, reading its "+
		"database or racing it; "+
		"%d of them with requests .NET crashed on, which wait for review (#76 row 263)", replayed, waiting, byHand, underReview)
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

// count is how many emails were sent.
func (k *kept) count() int {
	k.mu.Lock()
	defer k.mu.Unlock()
	return len(k.sent)
}

// keepFirst forgets every email after the first n, as a request that failed sent them.
func (k *kept) keepFirst(n int) {
	k.mu.Lock()
	defer k.mu.Unlock()
	k.sent = k.sent[:n]
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
	database, failure := dbtest.Open(t, filepath.Join(t.TempDir(), "redix.db"))
	fake := clock.NewFake(steps[0].At)
	if err := auth.SeedAdmin(context.Background(), database, adminUsername, adminPassword, moment.Of(steps[0].At)); err != nil {
		t.Fatal(err)
	}
	emails, files := &kept{}, filepath.Join(t.TempDir(), "attachments")
	handlers := map[int]http.Handler{}
	limitsOf := func(s step) host {
		for _, known := range hosts {
			if known.file == s.file && known.id == s.Host {
				return known
			}
		}
		return unlimited
	}
	handlerOf := func(s step) http.Handler {
		if handler, built := handlers[s.Host]; built {
			return handler
		}
		limits := limitsOf(s)
		handlers[s.Host] = problem.Traced(server.Routes(server.Parts{Database: database, Clock: fake, SigningKey: []byte("replay-key-for-go-backend-only-32"),
			Sender: emails, EmailCodesPerHour: limits.emailCodes, RegistrationsPerHour: limits.registrations, AttachmentsFolder: files, TypstPath: typstPath(), Log: quiet}))
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
		case "direct":
			applyDirect(t, database, names, s)
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
			if s.Status >= 200 && s.Status < 300 && strings.Contains(s.ContentType, "json") && firstWithABody(s) {
				var broken *httptest.ResponseRecorder
				historytest.Check(t, database, func() { broken = send(handlerOf(s), names, withBrokenBody(s)) })
				if broken.Code != http.StatusBadRequest {
					t.Errorf("step %d, %s %s%s: a body that is not JSON was answered %d, not 400", i, s.Method, s.Path, s.Query, broken.Code)
					return
				}
			}
			var got *httptest.ResponseRecorder
			if s.Status != http.StatusInternalServerError && limitsOf(s).limitsNothing() && !slices.Contains(unswept, s.Method+" "+s.Path) && firstOfItsKind(t, s) {
				var answered bool
				if got, answered = failEachCall(t, database, failure, emails, handlerOf(s), names, s, i); !answered {
					return
				}
			} else {
				historytest.Check(t, database, func() { got = send(handlerOf(s), names, s) })
			}
			resolve()
			if s.Status == http.StatusInternalServerError {
				// .NET crashed here (#76 row 263): Go may answer 400 or 404, never crash too.
				if got.Code == http.StatusInternalServerError {
					t.Errorf("step %d, %s %s%s: Go crashed where .NET did (#76 row 263)", i, s.Method, s.Path, s.Query)
					return
				}
				t.Logf("review (#76 row 263): %s %s%s - .NET crashed, Go answered %d", s.Method, pathPattern.ReplaceAllString(s.Path, "{id}"), s.Query, got.Code)
				continue
			}
			if differences := compare(names, s, got); len(differences) > 0 {
				t.Errorf("step %d, %s %s%s:\n  %s", i, s.Method, s.Path, s.Query, strings.Join(differences, "\n  "))
				return
			}
		}
	}
}

// unlimited is a host that limits neither registrations nor emailed codes, as every test's host is unless it is about those limits.
var unlimited = host{emailCodes: 100000, registrations: 100000}

// limitsNothing is whether h limits neither registrations nor emailed codes, so sending to it again uses up nothing a later step counts on.
func (h host) limitsNothing() bool {
	return h.emailCodes == unlimited.emailCodes && h.registrations == unlimited.registrations
}

// failEachCall sends s again and again, the database failing its first call, then its second, and so on, each before anything commits, and wants
// a 500 every time, with nothing changed and no email kept; the first sending in which no call failed is the step's own answer (redix-api #141
// D1). False once it has reported a failure Go answered otherwise.
func failEachCall(t *testing.T, database db.DB, failure *dbtest.Failure, emails *kept, handler http.Handler, names mapping, s step, i int) (*httptest.ResponseRecorder, bool) {
	t.Helper()
	sent := emails.count()
	for n := int64(1); ; n++ {
		var got *httptest.ResponseRecorder
		var failed bool
		historytest.Check(t, database, func() {
			failure.BeforeCommit(n)
			got = send(handler, names, s)
			failed = failure.Failed()
			failure.At(0)
		})
		if !failed {
			return got, true
		}
		emails.keepFirst(sent)
		want, named := answeredOnFailure[s.Method+" "+s.Path]
		if !named {
			want = http.StatusInternalServerError
		}
		if got.Code != want {
			t.Errorf("step %d, %s %s%s: the database failed at its call %d, and Go answered %d %s", i, s.Method, s.Path, s.Query, n, got.Code, got.Body.String())
			return nil, false
		}
	}
}

// answeredOnFailure is what a route answers to a failing database where that is not 500: /health says the database is down.
var answeredOnFailure = map[string]int{"GET /health": http.StatusServiceUnavailable}

// unswept are the routes never sent with their database failing: asking for a reset code spends one of the login's few asks an hour before it
// reads the database, so each failing attempt would spend an ask a later step counts on; it answers before it looks the login up, so a failure
// is only logged, and its own tests fail it on purpose (AC-02.8).
var unswept = []string{"POST /auth/forgot-password"}

// wordsDotNetHad is Go's bundle of words without those added since .NET, which every feature built after it adds (redix-app #109's among them):
// each word .NET had is still compared.
func wordsDotNetHad(net, got any) any {
	netWords, _ := net.(map[string]any)
	goWords, isBundle := got.(map[string]any)
	if !isBundle {
		return got
	}
	kept := map[string]any{}
	for key, value := range goWords {
		if _, had := netWords[key]; had {
			kept[key] = value
		}
	}
	return kept
}

// addedSinceDotNet are the fields Go answers that .NET never had, each added by a feature built after it, so an answer recorded without one still
// matches; each names why it is there.
var addedSinceDotNet = map[string]string{
	"accountIds":       "the accounts a payment method pays from (redix-app #109)",
	"timeZone":         "the time zone a project is in (redix-app #107)",
	"utcOffsetMinutes": "how far a project's zone is ahead of UTC now (redix-app #107)",
}

// withBody holds each route already sent once with a body that is not JSON, before the request it was recorded with: a route .NET answered
// with success refuses such a body with 400 and changes nothing, as ASP.NET Core refused a body it could not read.
var withBody = struct {
	sync.Mutex
	routes map[string]bool
}{routes: map[string]bool{}}

func firstWithABody(s step) bool {
	route := s.Method + " " + pathPattern.ReplaceAllString(s.Path, "{id}")
	withBody.Lock()
	defer withBody.Unlock()
	if withBody.routes[route] {
		return false
	}
	withBody.routes[route] = true
	return true
}

// withBrokenBody is s sent with a body that is not JSON.
func withBrokenBody(s step) step {
	s.Body = json.RawMessage(`"{"`)
	return s
}

// swept holds each kind of request already sent with its database failing call by call, so each kind is swept once across the replay: its
// method, its route, the status .NET answered, the fields its body and query name, and the class of .NET tests it was recorded in, since each
// class takes a route down paths of its own.
var swept = struct {
	sync.Mutex
	kinds map[string]bool
}{kinds: map[string]bool{}}

func firstOfItsKind(t *testing.T, s step) bool {
	var text string
	var fields map[string]json.RawMessage
	json.Unmarshal(s.Body, &text)
	json.Unmarshal([]byte(text), &fields)
	query, _ := url.ParseQuery(strings.TrimPrefix(s.Query, "?"))
	class, _, _ := strings.Cut(strings.TrimPrefix(t.Name(), "TestReplayDotNetsAnswers/"), ".")
	kind := fmt.Sprint(s.Method, " ", pathPattern.ReplaceAllString(s.Path, "{id}"), " ", s.Status, " ", slices.Sorted(maps.Keys(fields)), " ",
		slices.Sorted(maps.Keys(query)), " ", class)
	swept.Lock()
	defer swept.Unlock()
	if swept.kinds[kind] {
		return false
	}
	swept.kinds[kind] = true
	return true
}

// mapping is .NET's value of an id, token, ETag or emailed code, to Go's.
type mapping map[string]string

// into is text with every .NET value it holds replaced by Go's, longest first so no value is cut inside another.
func (m mapping) into(text string) string {
	keys := make([]string, 0, len(m))
	for key := range m {
		if !strings.HasPrefix(key, lineKey) {
			keys = append(keys, key)
		}
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
		if err := database.Exec(context.Background(), statement, args...); err != nil {
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
	case "approve":
		// ApprovedProjectAsync: ProjectStructure.ApproveAsync straight in the database, in the name of the project's first permission holder - no
		// History line and no notification, as the route would write.
		var given struct {
			ProjectID string `json:"projectId"`
		}
		json.Unmarshal(s.Given, &given)
		projectID := uuid.MustParse(names.into(given.ProjectID))
		var manager uuid.UUID
		err := database.Read.QueryRow("SELECT user_id FROM project_permissions WHERE project_id = ? ORDER BY id LIMIT 1", projectID).Scan(&manager)
		if err != nil {
			t.Fatalf("seed approve: %v", err)
		}
		err = database.InTx(context.Background(), func(q *store.Queries) error {
			if err := q.SetProjectStatus(context.Background(), store.SetProjectStatusParams{ID: projectID}); err != nil {
				return err
			}
			if err := structure.Copy(context.Background(), q, projectID); err != nil {
				return err
			}
			return suppliers.AddGeneralCash(context.Background(), q, projectID, manager)
		})
		if err != nil {
			t.Fatalf("seed approve: %v", err)
		}
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
	request := httptest.NewRequest(s.Method, names.into(s.Path+lineQuery(names, s.Query)), bytes.NewReader(body))
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

var guidIn = regexp.MustCompile(`[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}`)

// generated are the other values a server makes up as it writes: a custom column's key, "custom." and a fresh version 7 id (Column.NewCustomKey).
var generated = regexp.MustCompile(`^custom\.[0-9a-f]{32}$`)

// tokenKeys are the values a server makes up whose form says nothing: the tokens of a session, and a sync's stamp, a hash of what it answered.
var tokenKeys = []string{"accessToken", "refreshToken", "stamp"}

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
	differences = append(differences, sameBody(names, netType, s, got)...)
	// The headers come after the body, which has taught the mapping the ids a header may name, such as a new project's in Location.
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
	return differences
}

// sameBody is every difference between .NET's body and Go's: JSON compared value by value, a workbook cell by cell, other text as written, any
// other file not at all.
func sameBody(names mapping, netType string, s step, got *httptest.ResponseRecorder) []string {
	var recorded struct {
		Workbook []recordedSheet `json:"workbook"`
	}
	if json.Unmarshal(s.Answer, &recorded) == nil && recorded.Workbook != nil {
		return sameWorkbook(recorded.Workbook, got.Body.Bytes())
	}
	var answer string
	if json.Unmarshal(s.Answer, &answer) != nil {
		return nil
	}
	if !strings.Contains(netType, "json") {
		if names.into(answer) != got.Body.String() {
			return []string{fmt.Sprintf("body %q, .NET answered %q", got.Body.String(), answer)}
		}
		return nil
	}
	netJSON, goJSON := decode(answer), decode(got.Body.String())
	route := s.Method + " " + pathPattern.ReplaceAllString(s.Path, "{id}")
	if strings.HasPrefix(route, "GET /translations/") {
		goJSON = wordsDotNetHad(netJSON, goJSON)
	}
	if everyones[route] || tiedByChance[route] {
		return sameItems(names, netJSON, goJSON, tiedByChance[route])
	}
	return same(names, "$", "", netJSON, goJSON)
}

// everyones are the routes that list rows of every test - every project waiting for the admin - so .NET's answer held other tests' rows too,
// since its tests share one database, while each replay starts fresh: Go's every item must be .NET's item of the same id, field by field.
var everyones = map[string]bool{"GET /projects/registered": true}

// tiedByChance are the routes whose list .NET ordered on fields two items may share, leaving such items in the order SQL Server read them - for
// version 7 GUIDs, chance - where Go breaks the tie by name: Go's items must be .NET's items, every one, each compared with .NET's of its id.
var tiedByChance = map[string]bool{"GET /projects": true, "GET /projects/{id}/users": true, "GET /projects/{id}/members": true, "GET /projects/{id}/documents": true}

var pathPattern = regexp.MustCompile(`[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}`)

// sameItems compares each of Go's items with .NET's item of the same id; every item says that .NET's answer holds no other.
func sameItems(names mapping, net, got any, every bool) []string {
	netItems, netOK := net.([]any)
	goItems, goOK := got.([]any)
	if !netOK || !goOK {
		return same(names, "$", "", net, got)
	}
	var differences []string
	if every && len(netItems) != len(goItems) {
		differences = append(differences, fmt.Sprintf("$: %d items in .NET's answer, %d in Go's", len(netItems), len(goItems)))
	}
	for i, goItem := range goItems {
		goID, _ := goItem.(map[string]any)["id"].(string)
		var match any
		for _, netItem := range netItems {
			if netID, _ := netItem.(map[string]any)["id"].(string); names.into(netID) == goID {
				match = netItem
			}
		}
		if match == nil {
			differences = append(differences, fmt.Sprintf("$[%d]: %s is not in .NET's answer", i, goID))
			continue
		}
		differences = append(differences, same(names, fmt.Sprintf("$[%d]", i), "", match, goItem)...)
	}
	return differences
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

// sets are the lists of names whose order says nothing: .NET wrote a person's permissions in the order SQL Server read their rows, which for
// version 7 GUIDs - sorted on their last, random bytes - is chance, and the app reads them as a set. They are compared sorted.
var sets = map[string]bool{"permissions": true}

func sorted(items []any) []any {
	kept := slices.Clone(items)
	slices.SortFunc(kept, func(a, b any) int { return strings.Compare(fmt.Sprint(a), fmt.Sprint(b)) })
	return kept
}

// same is every difference between .NET's value and Go's at path, learning a new id or token as it goes.
func same(names mapping, path, key string, net, got any) []string {
	switch net := net.(type) {
	case map[string]any:
		got, ok := got.(map[string]any)
		if !ok {
			return []string{fmt.Sprintf("%s: an object in .NET's answer, %v in Go's", path, got)}
		}
		// A key may hold an id - a sync's lists are keyed by address - so keys are read through the mapping too, those naming fewer ids first, so
		// the lists that show an item's id are compared before the one keyed by it.
		keys := make([]string, 0, len(net))
		for name := range net {
			keys = append(keys, name)
		}
		sort.SliceStable(keys, func(i, j int) bool {
			return len(guidIn.FindAllString(keys[i], -1)) < len(guidIn.FindAllString(keys[j], -1))
		})
		var differences []string
		matched := map[string]bool{}
		for _, name := range keys {
			goName := names.into(name)
			if _, has := got[goName]; !has {
				differences = append(differences, fmt.Sprintf("%s.%s: missing in Go's answer", path, name))
				continue
			}
			matched[goName] = true
			if name == "traceId" {
				continue
			}
			differences = append(differences, same(names, path+"."+name, name, net[name], got[goName])...)
		}
		for name := range got {
			if _, added := addedSinceDotNet[name]; !matched[name] && !added {
				differences = append(differences, fmt.Sprintf("%s.%s: not in .NET's answer", path, name))
			}
		}
		return differences
	case []any:
		got, ok := got.([]any)
		if !ok || len(got) != len(net) {
			return []string{fmt.Sprintf("%s: %d items in .NET's answer, %v in Go's", path, len(net), got)}
		}
		if sets[key] {
			net, got = sorted(net), sorted(got)
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
		case slices.Contains(tokenKeys, key), guid.MatchString(net) && guid.MatchString(got), generated.MatchString(net) && generated.MatchString(got):
			if _, known := names[net]; !known {
				names[net] = got
				return nil
			}
		}
		return []string{fmt.Sprintf("%s: %q, .NET answered %q", path, got, net)}
	default:
		if key == "id" {
			// A number under "id" is a History line's, which .NET numbered across every test of its shared database: learned as a GUID is.
			if known, learned := names[lineKey+fmt.Sprint(net)]; !learned || known == fmt.Sprint(got) {
				names[lineKey+fmt.Sprint(net)] = fmt.Sprint(got)
				return nil
			}
		}
		if fmt.Sprint(net) != fmt.Sprint(got) {
			return []string{fmt.Sprintf("%s: %v, .NET answered %v", path, got, net)}
		}
		return nil
	}
}

// lineKey marks a History line's number in the mapping, kept apart from the text into replaces, since a number may be part of another value.
const lineKey = "line:"

// lineQuery is the request's query with a page's starting line, before, named by Go's number.
func lineQuery(names mapping, query string) string {
	values, err := url.ParseQuery(strings.TrimPrefix(query, "?"))
	if err != nil || values.Get("before") == "" {
		return query
	}
	if goLine, learned := names[lineKey+values.Get("before")]; learned {
		values.Set("before", goLine)
		return "?" + values.Encode()
	}
	return query
}

// recordedSheet is a sheet of a workbook .NET answered, as the recorder kept it: every cell that holds something, its formula or its value as Excel
// keeps it, a day as its serial number.
type recordedSheet struct {
	Name  string `json:"name"`
	Cells []struct {
		At      string `json:"at"`
		Formula string `json:"formula"`
		Value   string `json:"value"`
	} `json:"cells"`
}

// sameWorkbook is every difference between the workbook .NET answered and Go's: the sheets, in order, and each cell's formula or value, a number
// as a number. A cell only Go fills, inside what .NET's sheet covers or just past it, is a difference too. The look is the workbook tests' to check.
func sameWorkbook(net []recordedSheet, content []byte) []string {
	file, err := excelize.OpenReader(bytes.NewReader(content))
	if err != nil {
		return []string{fmt.Sprintf("Go's workbook does not open: %v", err)}
	}
	defer file.Close()
	var names []string
	for _, sheet := range net {
		names = append(names, sheet.Name)
	}
	if goNames := file.GetSheetList(); !slices.Equal(goNames, names) {
		return []string{fmt.Sprintf("the sheets %q, .NET's %q", goNames, names)}
	}
	var differences []string
	for _, sheet := range net {
		kept := map[string]bool{}
		lastRow, lastColumn := 1, 1
		for _, cell := range sheet.Cells {
			kept[cell.At] = true
			column, row, _ := excelize.CellNameToCoordinates(cell.At)
			lastRow, lastColumn = max(lastRow, row), max(lastColumn, column)
			if difference := sameCell(file, sheet.Name, cell.At, cell.Formula, cell.Value); difference != "" {
				differences = append(differences, difference)
			}
		}
		merged, _ := file.GetMergeCells(sheet.Name)
		for row := 1; row <= lastRow+2; row++ {
			for column := 1; column <= lastColumn+2; column++ {
				at, _ := excelize.CoordinatesToCellName(column, row)
				if !kept[at] && !insideMerge(merged, column, row) {
					if difference := sameCell(file, sheet.Name, at, "", ""); difference != "" {
						differences = append(differences, difference)
					}
				}
			}
		}
	}
	return differences
}

// sameCell is how the cell at of Go's sheet differs from .NET's formula and value; "" when it does not.
func sameCell(file *excelize.File, sheet, at, formula, value string) string {
	goFormula, _ := file.GetCellFormula(sheet, at)
	goValue, _ := file.GetCellValue(sheet, at, excelize.Options{RawCellValue: true})
	if goFormula != "" {
		goValue = ""
	}
	netNumber, netErr := strconv.ParseFloat(value, 64)
	goNumber, goErr := strconv.ParseFloat(goValue, 64)
	sameValue := goValue == value || (netErr == nil && goErr == nil && math.Abs(netNumber-goNumber) < 1e-9)
	if goFormula == formula && sameValue {
		return ""
	}
	return fmt.Sprintf("%s!%s holds %q =%q, .NET's %q =%q", sheet, at, goValue, goFormula, value, formula)
}

// insideMerge says whether the cell is a merged range's but its first, which holds the range's content.
func insideMerge(merged []excelize.MergeCell, column, row int) bool {
	for _, merge := range merged {
		left, top, _ := excelize.CellNameToCoordinates(merge.GetStartAxis())
		right, bottom, _ := excelize.CellNameToCoordinates(merge.GetEndAxis())
		if column >= left && column <= right && row >= top && row <= bottom && (column != left || row != top) {
			return true
		}
	}
	return false
}

// typstPath is the typst binary the replay makes PDFs with: REDIX_Typst__Path, as the server reads it, or typst on the path.
func typstPath() string {
	if path := os.Getenv("REDIX_Typst__Path"); path != "" {
		return path
	}
	return "typst"
}
