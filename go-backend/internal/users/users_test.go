// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users_test

import (
	"encoding/json"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

var (
	project = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a1")
	other   = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a2")
	partner = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000b1")
)

// api is the API through testapi, on a database holding an approved project with a partner, and its manager, who manages its users.
type api struct {
	*testapi.API
	t       *testing.T
	manager uuid.UUID
}

func newAPI(t *testing.T) api {
	t.Helper()
	a := api{API: testapi.New(t), t: t}
	a.Exec(`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template) VALUES (?, 'مشروع A', '2026-11-01', 0, '', '', 0)`, project)
	a.Exec(`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template) VALUES (?, 'أخرى', '2026-11-01', 0, '', '', 0)`, other)
	a.Exec(`INSERT INTO partners (id, project_id, name, sort_order, is_deleted) VALUES (?, ?, 'شريك', 0, 0)`, partner, project)
	a.manager = a.person("manager", "Manager", project, access.ReadProject, access.ManageUsers)
	return a
}

// person adds someone, with permissions in projectID, straight in the database, as the .NET factory's seeds did.
func (a api) person(username, displayName string, projectID uuid.UUID, permissions ...access.Permission) uuid.UUID {
	id := uuid.Must(uuid.NewV7())
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES (?, ?, ?, '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', ?)`, id, username, displayName, credentials.Hash("Patrol-chosen-2"), username+"@example.com")
	for _, permission := range permissions {
		a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?)`, uuid.NewString(), projectID, id, string(permission))
	}
	return id
}

func (a api) as(userID uuid.UUID) string { return a.Token(userID, false) }

func (a api) ask(method, path, body, token string) (int, any) {
	return a.Ask(method, path, body, token)
}

func (a api) exec(statement string, args ...any) { a.Exec(statement, args...) }

func field(answer any, name string) any { return testapi.Field(answer, name) }

func (a api) users() string { return "/projects/" + project.String() + "/users" }

func TestFR02_AManagerAddsSomeoneWithReadProjectTheTicksAPartnerAndARoleName(t *testing.T) {
	a := newAPI(t)
	code, answer := a.ask("POST", a.users(), `{"displayName":" B ","username":" b ","mobile":"","email":"b@example.com","temporaryPassword":"Temporary-1",
		"permissions":["recordEntries","seeChosenCapital"],"partnerIds":["`+partner.String()+`"],"roleName":" مشرف ميداني "}`, a.as(a.manager))
	if code != 201 || field(answer, "username") != "b" || field(answer, "displayName") != "B" || field(answer, "mustChangePassword") != true {
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
		fields := map[string]any{"displayName": "B", "username": "b", "mobile": "", "email": "b@example.com", "temporaryPassword": "Temporary-1"}
		if err := json.Unmarshal([]byte(change), &fields); err != nil {
			t.Fatal(err)
		}
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
	person := a.person("b", "B", project, access.ReadProject)
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
	person := a.person("b", "B", project, access.ReadProject, access.RecordEntries)
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
	person := a.person("b", "B", project, access.ReadProject)
	theirs := a.as(person)
	code, answer := a.ask("POST", "/users/"+person.String()+"/reset-password", `{"temporaryPassword":"Temporary-2"}`, a.as(a.manager))
	if code != 200 || field(answer, "mustChangePassword") != true {
		t.Fatalf("%d %v", code, answer)
	}
	if code, _ := a.ask("GET", "/projects/"+project.String()+"/members", ``, theirs); code != 401 {
		t.Errorf("their device after the reset: %d", code)
	}
}

// redix-api #182: the users list says when each person was last seen - their latest sign-in or renewal, on any device -, the API's never before
// their first; the answer to a change says it too.
func TestIssue182_EachPersonSaysWhenTheyWereLastSeenOrNeverBeforeTheirFirstSignIn(t *testing.T) {
	a := newAPI(t)
	person := a.person("b", "B", project, access.ReadProject)
	seen := func() map[string]any {
		_, answer := a.ask("GET", "/projects/"+project.String()+"/users", ``, a.as(a.manager))
		lastSeen := map[string]any{}
		for _, one := range answer.([]any) {
			lastSeen[field(one, "id").(string)] = field(one, "lastSeenAt")
		}
		return lastSeen
	}
	if before := seen(); before[person.String()] != "1900-01-01T00:00:00Z" {
		t.Errorf("before anyone signs in: %v", before)
	}
	signIn := func() string {
		if code, answer := a.ask("POST", "/auth/sign-in", `{"username":"b","password":"Patrol-chosen-2"}`, ``); code != 200 {
			t.Fatalf("signing in: %d %v", code, answer)
		}
		at, _ := json.Marshal(moment.Moment{Time: a.Clock.Now()})
		return strings.Trim(string(at), `"`)
	}
	first := signIn()
	if after := seen(); after[person.String()] != first {
		t.Errorf("after the first sign-in at %s: %v", first, after)
	}
	a.Clock.Advance(3 * time.Hour)
	latest := signIn()
	if after := seen(); after[person.String()] != latest {
		t.Errorf("after the second sign-in at %s: %v", latest, after)
	}
	if _, changed := a.ask("PUT", "/projects/"+project.String()+"/users/"+person.String()+"/role-name", `{"roleName":"مراقب"}`, a.as(a.manager)); field(changed, "lastSeenAt") != latest {
		t.Errorf("the answer to a change: %v", changed)
	}
}
