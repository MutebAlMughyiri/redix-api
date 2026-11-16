package users

import (
	"context"
	"database/sql"
	"encoding/json"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history/historytest"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

var (
	project = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a1")
	other   = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a2")
	partner = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000b1")
)

type api struct {
	t        *testing.T
	handler  http.Handler
	database db.DB
	tokens   credentials.Tokens
	manager  uuid.UUID
}

// newAPI is the users routes on a database holding an approved project with a partner, and its manager, who manages its users.
func newAPI(t *testing.T) api {
	t.Helper()
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { database.Close() })
	fake := clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))
	tokens := credentials.NewTokens([]byte("test-key-for-go-backend-only-32b"), fake)
	book := access.NewBook(database)
	router := web.NewRouter("")
	Map(router, access.NewGate(tokens, database, book, slog.New(slog.NewTextHandler(io.Discard, nil))), database, fake)
	a := api{t: t, handler: problem.Traced(router), database: database, tokens: tokens}
	a.exec(`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template) VALUES (?, 'غصون', '2026-11-01', 0, '', '', 0)`, project)
	a.exec(`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template) VALUES (?, 'أخرى', '2026-11-01', 0, '', '', 0)`, other)
	a.exec(`INSERT INTO partners (id, project_id, name, sort_order, is_deleted) VALUES (?, ?, 'شريك', 0, 0)`, partner, project)
	a.manager = a.person("manager", "Manager", project, access.ReadProject, access.ManageUsers)
	return a
}

func (a api) exec(statement string, args ...any) {
	a.t.Helper()
	err := a.database.InTx(context.Background(), func(tx *sql.Tx) error { _, err := tx.Exec(statement, args...); return err })
	if err != nil {
		a.t.Fatal(err)
	}
}

// person adds someone, with permissions in projectID, straight in the database, as the .NET factory's seeds did.
func (a api) person(username, displayName string, projectID uuid.UUID, permissions ...access.Permission) uuid.UUID {
	id := uuid.Must(uuid.NewV7())
	a.exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES (?, ?, ?, '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', ?)`, id, username, displayName, credentials.Hash("Patrol-chosen-2"), username+"@example.com")
	for _, permission := range permissions {
		a.exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?)`, uuid.NewString(), projectID, id, string(permission))
	}
	return id
}

// as is the bearer token of userID on a device of their own.
func (a api) as(userID uuid.UUID) string {
	device := uuid.Must(uuid.NewV7())
	a.exec(`INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at)
		VALUES (?, ?, ?, '2026-11-17T10:00:00.0000000Z', 0, 0, '2026-11-16T10:00:00.0000000Z')`, device, userID, device.String())
	token, _ := a.tokens.Create(credentials.Session{UserID: userID, Username: "u", DeviceID: device})
	return token
}

func (a api) ask(method, path, body, token string) (int, any) {
	a.t.Helper()
	request := httptest.NewRequest(method, path, strings.NewReader(body))
	request.Header.Set("Content-Type", "application/json")
	request.Header.Set("Authorization", "Bearer "+token)
	recorder := httptest.NewRecorder()
	historytest.Check(a.t, a.database, func() { a.handler.ServeHTTP(recorder, request) })
	var answer any
	json.Unmarshal(recorder.Body.Bytes(), &answer)
	return recorder.Code, answer
}

func field(answer any, name string) any {
	object, _ := answer.(map[string]any)
	return object[name]
}

func (a api) users() string { return "/projects/" + project.String() + "/users" }

func TestFR02_AManagerAddsSomeoneWithReadProjectTheTicksAPartnerAndARoleName(t *testing.T) {
	a := newAPI(t)
	code, answer := a.ask("POST", a.users(), `{"displayName":" نواف ","username":" nawaf ","mobile":"","email":"nawaf@example.com","temporaryPassword":"Temporary-1",
		"permissions":["recordEntries","seeChosenCapital"],"partnerIds":["`+partner.String()+`"],"roleName":" مشرف ميداني "}`, a.as(a.manager))
	if code != 201 || field(answer, "username") != "nawaf" || field(answer, "displayName") != "نواف" || field(answer, "mustChangePassword") != true {
		t.Fatalf("%d %v", code, answer)
	}
	projects := field(answer, "projects").([]any)
	held := projects[0].(map[string]any)
	if got := held["permissions"].([]any); len(got) != 3 || got[0] != "readProject" || got[1] != "seeChosenCapital" || got[2] != "recordEntries" {
		t.Errorf("permissions in .NET's order: %v", got)
	}
	if held["roleName"] != "مشرف ميداني" || len(held["partnerIds"].([]any)) != 1 || held["isInactive"] != false {
		t.Errorf("%v", held)
	}
}

func TestFR02_ANewPersonIsRefusedForEachReason(t *testing.T) {
	a := newAPI(t)
	base := func(change string) string {
		fields := map[string]any{"displayName": "نواف", "username": "nawaf", "mobile": "", "email": "nawaf@example.com", "temporaryPassword": "Temporary-1"}
		json.Unmarshal([]byte(change), &fields)
		body, _ := json.Marshal(fields)
		return string(body)
	}
	token := a.as(a.manager)
	for change, want := range map[string]string{
		`{"username":""}`:                                      "invalidUsername",
		`{"temporaryPassword":"short"}`:                        "weakPassword",
		`{"displayName":""}`:                                   "invalidName",
		`{"mobile":"٠٥٠"}`:                                     "invalidMobile",
		`{"email":""}`:                                         "emailInvalid",
		`{"username":"MANAGER"}`:                               "usernameTaken",
		`{"email":"manager@example.com"}`:                      "emailTaken",
		`{"permissions":["approveProjects"]}`:                  "invalidPermission",
		`{"permissions":["seeAllCapital","seeChosenCapital"]}`: "invalidPermission",
		`{"roleName":"` + strings.Repeat("م", 51) + `"}`:       "invalidRoleName",
		`{"permissions":["seeChosenCapital"],"partnerIds":["` + uuid.NewString() + `"]}`: "unknownPartner",
	} {
		if code, answer := a.ask("POST", a.users(), base(change), token); code < 400 || field(answer, "code") != want {
			t.Errorf("%s: %d %v, want %s", change, code, answer, want)
		}
	}
}

func TestBR33_SomeoneWhoDoesNotManageTheProjectsUsersIsRefused(t *testing.T) {
	a := newAPI(t)
	reader := a.person("reader", "Reader", project, access.ReadProject)
	if code, _ := a.ask("GET", a.users(), ``, a.as(reader)); code != 403 {
		t.Errorf("list: %d", code)
	}
	if code, _ := a.ask("GET", "/projects/"+project.String()+"/members", ``, a.as(reader)); code != 200 {
		t.Errorf("members: %d", code)
	}
}

func TestD7_MembersComeInArabicDictionaryOrderTheInactiveLeftOut(t *testing.T) {
	a := newAPI(t)
	a.person("ahmed", "أحمد", project, access.ReadProject)
	a.person("ibrahim", "ابراهيم", project, access.ReadProject)
	gone := a.person("amna", "آمنة", project, access.ReadProject)
	a.exec(`INSERT INTO inactive_members (id, project_id, user_id) VALUES (?, ?, ?)`, uuid.NewString(), project, gone)
	_, answer := a.ask("GET", "/projects/"+project.String()+"/members", ``, a.as(a.manager))
	var names []string
	for _, member := range answer.([]any) {
		names = append(names, member.(map[string]any)["displayName"].(string))
	}
	if strings.Join(names, " ") != "Manager ابراهيم أحمد" {
		t.Errorf("%v", names)
	}
	_, listed := a.ask("GET", a.users(), ``, a.as(a.manager))
	if last := listed.([]any)[3].(map[string]any); last["displayName"] != "آمنة" {
		t.Errorf("the inactive one is not last: %v", last["displayName"])
	}
}

func TestBR31_SeeingAllCapitalTakesSeeingChosenPartnersAwayAndItsPartners(t *testing.T) {
	a := newAPI(t)
	person := a.person("nawaf", "نواف", project, access.ReadProject)
	path := a.users() + "/" + person.String()
	token := a.as(a.manager)
	if code, answer := a.ask("POST", path+"/partners/"+partner.String(), ``, token); code != 200 {
		t.Fatalf("choose: %d %v", code, answer)
	}
	code, answer := a.ask("POST", path+"/permissions/SeeAllCapital", ``, token)
	held := field(answer, "projects").([]any)[0].(map[string]any)
	if code != 200 || len(held["partnerIds"].([]any)) != 0 || strings.Contains(strings.Join(texts(held["permissions"]), " "), "seeChosenCapital") {
		t.Errorf("%d %v", code, held)
	}
	if code, _ := a.ask("POST", path+"/partners/"+uuid.NewString(), ``, token); code != 404 {
		t.Errorf("a partner not the project's: %d", code)
	}
}

func texts(list any) []string {
	var all []string
	for _, item := range list.([]any) {
		all = append(all, item.(string))
	}
	return all
}

func TestBR29_TheLastActiveManagerKeepsManagingUsers(t *testing.T) {
	a := newAPI(t)
	path := a.users() + "/" + a.manager.String()
	token := a.as(a.manager)
	for _, request := range [][2]string{{"DELETE", path + "/permissions/manageUsers"}, {"DELETE", path + "/permissions/readProject"}, {"POST", path + "/disable"}} {
		if code, answer := a.ask(request[0], request[1], ``, token); code != 409 || field(answer, "code") != "lastManager" {
			t.Errorf("%v: %d %v", request, code, answer)
		}
	}
	a.person("second", "Second", project, access.ReadProject, access.ManageUsers)
	if code, _ := a.ask("POST", path+"/disable", ``, token); code != 200 {
		t.Errorf("with a second manager: %d", code)
	}
}

func TestUC126_TakingReadProjectTakesSomeoneOutWithTheirRoleNameAndBeingInactive(t *testing.T) {
	a := newAPI(t)
	person := a.person("nawaf", "نواف", project, access.ReadProject, access.RecordEntries)
	path := a.users() + "/" + person.String()
	token := a.as(a.manager)
	a.ask("PUT", path+"/role-name", `{"roleName":"مشرف"}`, token)
	a.ask("POST", path+"/disable", ``, token)
	code, answer := a.ask("DELETE", path+"/permissions/readProject", ``, token)
	if code != 200 || len(field(answer, "projects").([]any)) != 0 {
		t.Errorf("%d %v", code, answer)
	}
	if code, _ := a.ask("POST", path+"/enable", ``, token); code != 404 {
		t.Errorf("someone no longer in the project: %d", code)
	}
}

func TestUC007_EditingSomeoneInAProjectTheCallerDoesNotManageIsRefused(t *testing.T) {
	a := newAPI(t)
	shared := a.person("shared", "Shared", project, access.ReadProject)
	a.exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ReadProject')`, uuid.NewString(), other, shared)
	code, answer := a.ask("PUT", "/users/"+shared.String(), `{"displayName":"x","mobile":"","email":"x@example.com"}`, a.as(a.manager))
	if code != 403 || field(answer, "code") != "otherProject" {
		t.Errorf("%d %v", code, answer)
	}
	if code, _ := a.ask("PUT", "/users/"+uuid.NewString(), `{"displayName":"x","mobile":"","email":"x@example.com"}`, a.as(a.manager)); code != 404 {
		t.Errorf("nobody: %d", code)
	}
}

func TestUC007_AResetPasswordMustBeChangedAndSignsOutEveryDevice(t *testing.T) {
	a := newAPI(t)
	person := a.person("nawaf", "نواف", project, access.ReadProject)
	theirs := a.as(person)
	code, answer := a.ask("POST", "/users/"+person.String()+"/reset-password", `{"temporaryPassword":"Temporary-2"}`, a.as(a.manager))
	if code != 200 || field(answer, "mustChangePassword") != true {
		t.Fatalf("%d %v", code, answer)
	}
	if code, _ := a.ask("GET", "/projects/"+project.String()+"/members", ``, theirs); code != 401 {
		t.Errorf("their device after the reset: %d", code)
	}
}
