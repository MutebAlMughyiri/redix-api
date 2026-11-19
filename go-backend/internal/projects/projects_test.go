// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package projects_test

import (
	"fmt"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

type api struct {
	*testapi.API
	t *testing.T
}

func newAPI(t *testing.T) api { return api{API: testapi.New(t), t: t} }

// person adds someone straight in the database, an admin when admin is true, and is their token.
func (a api) person(username string, admin bool) (uuid.UUID, string) {
	id := uuid.Must(uuid.NewV7())
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES (?, ?, ?, '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, username, username, credentials.Hash("Patrol-chosen-2"))
	if admin {
		a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), id)
	}
	return id, a.Token(id, false)
}

// started is a project the token's person starts, approved straight in the database when approved is true.
func (a api) started(token string, approved bool) string {
	a.t.Helper()
	code, answer := a.Ask("POST", "/projects", `{"name":"مشروع A","startDate":"2026-11-01","units":["أ","ب"],"partners":["شريك"]}`, token)
	if code != 201 {
		a.t.Fatalf("start: %d %v", code, answer)
	}
	id := testapi.Field(answer, "id").(string)
	if approved {
		a.Exec("UPDATE projects SET status = 0 WHERE id = ?", id)
	}
	return id
}

func TestUC004_StartingAProjectMakesItsStarterItsManagerWaitingForTheAdmin(t *testing.T) {
	a := newAPI(t)
	starter, token := a.person("starter", false)
	answered := a.Send("POST", "/projects", `{"name":" مشروع A ","startDate":"2026-11-01","units":[" أ ","ب"],"description":" وصف ","partners":["شريك"]}`,
		"Authorization", "Bearer "+token)
	if answered.Code != 201 || !strings.HasPrefix(answered.Header().Get("Location"), "/projects/") {
		t.Fatalf("%d %s", answered.Code, answered.Body)
	}
	body := answered.Body.String()
	for _, want := range []string{`"name":"مشروع A"`, `"status":"pendingApproval"`, `"description":"وصف"`, `"permissions":[]`, `"holdsEntries":false`, `"name":"أ"`} {
		if !strings.Contains(body, want) {
			t.Errorf("%s not in %s", want, body)
		}
	}
	if got := a.Count("SELECT count(*) FROM project_permissions WHERE user_id = ?", starter); got != 18 {
		t.Errorf("the starter holds %d permissions, want 18, SeeProjectHistory and ManageProjectFiles among them (UC-069 D7, redix-api #130 D8)", got)
	}
	if got := a.Count("SELECT count(*) FROM partners"); got != 1 {
		t.Errorf("partners: %d", got)
	}
}

func TestUC004_AProjectIsRefusedForEachReason(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	for body, want := range map[string]string{
		`{"name":"","startDate":"2026-11-01","units":[]}`:                                                          "invalidName",
		`{"name":"مشروع A","startDate":"2026-11-01","units":["أ"," أ "]}`:                                          "unitNameTaken",
		`{"name":"مشروع A","startDate":"2026-11-01","units":[""]}`:                                                 "invalidName",
		`{"name":"مشروع A","startDate":"2026-11-01","units":[],"partners":[""]}`:                                   "invalidName",
		`{"name":"مشروع A","startDate":"2026-11-01","units":[],"description":"` + strings.Repeat("و", 2001) + `"}`: "invalidDescription",
		`{"name":"مشروع A","startDate":"2026-11-01","units":[],"structureSourceId":"` + uuid.NewString() + `"}`:    "unknownSource",
	} {
		if code, answer := a.Ask("POST", "/projects", body, token); code != 400 || testapi.Field(answer, "code") != want {
			t.Errorf("%.60s: %d %v, want %s", body, code, answer, want)
		}
	}
}

func TestUC066_SomeoneWithNoAccountRegistersAProjectAndTheAdminSeesItWaiting(t *testing.T) {
	a := newAPI(t)
	code, answer := a.Ask("POST", "/projects/register", `{"name":"مشروع","startDate":"2026-11-05","units":["أ"],"displayName":"B","username":"b",
		"mobile":"0500000000","password":"Register-1"}`, "")
	if code != 201 || testapi.Field(answer, "status") != "pendingApproval" {
		t.Fatalf("%d %v", code, answer)
	}
	if code, answer := a.Ask("POST", "/projects/register", `{"name":"مشروع","startDate":"2026-11-05","units":[],"displayName":"x","username":"B",
		"mobile":"","password":"Register-1"}`, ""); code != 409 || testapi.Field(answer, "code") != "usernameTaken" {
		t.Errorf("a taken username: %d %v", code, answer)
	}
	_, admin := a.person("admin", true)
	_, waiting := a.Ask("GET", "/projects/registered", "", admin)
	listed := waiting.([]any)
	if len(listed) != 1 || testapi.Field(listed[0], "managerUsername") != "b" || testapi.Field(listed[0], "managerMobile") != "0500000000" {
		t.Errorf("%v", waiting)
	}
	_, someone := a.person("someone", false)
	if code, _ := a.Ask("GET", "/projects/registered", "", someone); code != 403 {
		t.Errorf("someone not the admin: %d", code)
	}
}

func TestUC004_TheListHoldsWhatTheyMayReadAndWhatTheyStarted(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	pending := a.started(token, false)
	approved := a.started(token, true)
	_, answer := a.Ask("GET", "/projects", "", token)
	listed := answer.([]any)
	if len(listed) != 2 || testapi.Field(listed[0], "id") != approved || testapi.Field(listed[1], "id") != pending {
		t.Fatalf("the active first: %v", answer)
	}
	if permissions := testapi.Field(listed[0], "permissions").([]any); len(permissions) != 18 {
		t.Errorf("in the approved project: %v", permissions)
	}
	_, other := a.person("other", false)
	if _, answer := a.Ask("GET", "/projects", "", other); len(answer.([]any)) != 0 {
		t.Errorf("someone in neither: %v", answer)
	}
}

func TestFR04_AManagerRenamesDescribesAndKeepsTheUnits(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	project := "/projects/" + a.started(token, true)
	if code, answer := a.Ask("PUT", project, `{"name":" مشروع B "}`, token); code != 200 || testapi.Field(answer, "name") != "مشروع B" {
		t.Errorf("rename: %d %v", code, answer)
	}
	if code, answer := a.Ask("PUT", project+"/description", `{"description":"وصف"}`, token); code != 200 || testapi.Field(answer, "description") != "وصف" {
		t.Errorf("describe: %d %v", code, answer)
	}
	code, answer := a.Ask("POST", project+"/units", `{"name":"ج"}`, token)
	units := testapi.Field(answer, "units").([]any)
	if code != 200 || len(units) != 3 || testapi.Field(units[2], "name") != "ج" {
		t.Fatalf("add a unit: %d %v", code, answer)
	}
	if code, answer := a.Ask("POST", project+"/units", `{"name":"أ"}`, token); code != 409 || testapi.Field(answer, "code") != "unitNameTaken" {
		t.Errorf("a taken unit name: %d %v", code, answer)
	}
	unit := testapi.Field(units[0], "id").(string)
	if code, answer := a.Ask("PUT", project+"/units/"+unit, `{"name":"د"}`, token); code != 200 || testapi.Field(testapi.Field(answer, "units").([]any)[0], "name") != "د" {
		t.Errorf("rename a unit: %d %v", code, answer)
	}
	if code, answer := a.Ask("DELETE", project+"/units/"+unit, ``, token); code != 200 || len(testapi.Field(answer, "units").([]any)) != 2 {
		t.Errorf("remove a unit: %d %v", code, answer)
	}
	if code, _ := a.Ask("DELETE", project+"/units/"+uuid.NewString(), ``, token); code != 404 {
		t.Errorf("a unit it does not have: %d", code)
	}
}

func TestAC04_1_AClosedProjectRefusesEveryChange(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	project := "/projects/" + a.started(token, true)
	if code, answer := a.Ask("POST", project+"/close", ``, token); code != 200 || testapi.Field(answer, "status") != "finished" {
		t.Fatalf("close: %d %v", code, answer)
	}
	for _, change := range [][3]string{{"PUT", project, `{"name":"x"}`}, {"POST", project + "/units", `{"name":"x"}`}, {"POST", project + "/close", ``}} {
		if code, answer := a.Ask(change[0], change[1], change[2], token); code != 409 || testapi.Field(answer, "code") != "projectFinished" {
			t.Errorf("%v: %d %v", change, code, answer)
		}
	}
}

func TestAC37_1_SignInOpensTheDefaultProjectUntilItIsTakenAway(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	id := a.started(token, true)
	if code, answer := a.Ask("POST", "/projects/"+id+"/default", ``, token); code != 200 || testapi.Field(answer, "defaultProjectId") != id {
		t.Errorf("set: %d %v", code, answer)
	}
	if code, answer := a.Ask("DELETE", "/projects/default", ``, token); code != 200 || testapi.Field(answer, "defaultProjectId") != "" {
		t.Errorf("remove: %d %v", code, answer)
	}
}

// template is the project a structure is copied from when none was chosen (migration 00004_template).
const template = "f11ea9ed-0623-ebf7-abff-3f4e8714248a"

// structureOf counts what project holds of each part of a structure, and its general cash supplier.
func (a api) structureOf(project string) map[string]int {
	items := "SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ?"
	return map[string]int{
		"default columns": a.Count("SELECT count(*) FROM default_columns WHERE project_id = ?", project),
		"categories":      a.Count("SELECT count(*) FROM categories WHERE project_id = ?", project),
		"items":           a.Count("SELECT count(*) FROM ("+items+")", project),
		"subgroups":       a.Count("SELECT count(*) FROM subgroups WHERE item_id IN ("+items+")", project),
		"item columns":    a.Count("SELECT count(*) FROM item_columns WHERE item_id IN ("+items+")", project),
		"payment methods": a.Count("SELECT count(*) FROM payment_methods WHERE project_id = ?", project),
		"general cash":    a.Count("SELECT count(*) FROM suppliers WHERE project_id = ? AND is_general_cash = 1 AND name = 'مورد نقدي عام'", project),
	}
}

func TestBR34_ApprovingCopiesTheTemplateTellsTheManagerAndOpensThePermissions(t *testing.T) {
	a := newAPI(t)
	starter, token := a.person("starter", false)
	adminID, admin := a.person("admin", true)
	project := a.started(token, false)
	_, someone := a.person("someone", false)
	if code, _ := a.Ask("POST", "/projects/"+project+"/approve", ``, someone); code != 403 {
		t.Errorf("someone not the admin: %d", code)
	}
	if code, _ := a.Ask("POST", "/projects/"+uuid.NewString()+"/approve", ``, admin); code != 404 {
		t.Errorf("no such project: %d", code)
	}
	answered := a.Send("POST", "/projects/"+project+"/approve", ``, "Authorization", "Bearer "+admin)
	if answered.Code != 200 || answered.Body.Len() != 0 {
		t.Fatalf("approve: %d %s", answered.Code, answered.Body)
	}
	inheritedColumns := a.Count(`SELECT count(*) FROM items JOIN categories ON categories.id = items.category_id
		JOIN default_columns ON default_columns.project_id = categories.project_id AND default_columns.item_type = items.type WHERE categories.project_id = ?`, template)
	want := map[string]int{"default columns": 53, "categories": 9, "items": 36, "subgroups": 36, "item columns": inheritedColumns, "payment methods": 3, "general cash": 1}
	if got := a.structureOf(project); fmt.Sprint(got) != fmt.Sprint(want) {
		t.Errorf("copied %v, want %v", got, want)
	}
	if got := a.Count("SELECT count(*) FROM suppliers WHERE project_id = ? AND created_by_user_id = ?", project, adminID); got != 1 {
		t.Errorf("the general cash supplier is not in the admin's name")
	}
	if got := a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND project_id = ? AND kind = 3 AND is_read = 0", starter, project); got != 1 {
		t.Errorf("the manager was told %d times", got)
	}
	_, listed := a.Ask("GET", "/projects", "", token)
	if testapi.Field(listed.([]any)[0], "status") != "active" || len(testapi.Field(listed.([]any)[0], "permissions").([]any)) != 18 {
		t.Errorf("after approval: %v", listed)
	}
	if code, answer := a.Ask("POST", "/projects/"+project+"/approve", ``, admin); code != 409 || testapi.Field(answer, "code") != "notPending" {
		t.Errorf("approved twice: %d %v", code, answer)
	}
}

func TestFR05_AChosenSourceGivesItsActiveStructureAndTheTemplatesPaymentMethods(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	_, admin := a.person("admin", true)
	source := a.started(token, false)
	a.Ask("POST", "/projects/"+source+"/approve", ``, admin)
	a.Exec("UPDATE categories SET is_active = 0 WHERE id = (SELECT id FROM categories WHERE project_id = ? ORDER BY sort_order LIMIT 1)", source)
	a.Exec(`UPDATE items SET is_active = 0 WHERE id = (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id
		WHERE categories.project_id = ? AND categories.is_active = 1 ORDER BY items.id LIMIT 1)`, source)
	a.Exec(`UPDATE item_columns SET is_hidden = 1 WHERE id = (SELECT item_columns.id FROM item_columns JOIN items ON items.id = item_columns.item_id
		JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND categories.is_active = 1 AND items.is_active = 1
		ORDER BY item_columns.id LIMIT 1)`, source)
	a.Exec("UPDATE payment_methods SET name = 'غير القالب' WHERE project_id = ?", source)
	activeItems := `SELECT items.id FROM items JOIN categories ON categories.id = items.category_id
		WHERE categories.project_id = ? AND categories.is_active = 1 AND items.is_active = 1`
	want := map[string]int{
		"default columns": 53,
		"categories":      a.Count("SELECT count(*) FROM categories WHERE project_id = ? AND is_active = 1", source),
		"items":           a.Count("SELECT count(*) FROM ("+activeItems+")", source),
		"subgroups":       a.Count("SELECT count(*) FROM subgroups WHERE item_id IN ("+activeItems+")", source),
		"item columns":    a.Count("SELECT count(*) FROM item_columns WHERE is_hidden = 0 AND item_id IN ("+activeItems+")", source),
		"payment methods": 3,
		"general cash":    1,
	}
	code, answer := a.Ask("POST", "/projects", `{"name":"مشروع B","startDate":"2026-11-10","units":[],"structureSourceId":"`+source+`"}`, token)
	if code != 201 {
		t.Fatalf("start from a source: %d %v", code, answer)
	}
	copied := testapi.Field(answer, "id").(string)
	a.Ask("POST", "/projects/"+copied+"/approve", ``, admin)
	if got := a.structureOf(copied); fmt.Sprint(got) != fmt.Sprint(want) {
		t.Errorf("copied %v, want %v", got, want)
	}
	if got := a.Count("SELECT count(*) FROM payment_methods WHERE project_id = ? AND name = 'غير القالب'", copied); got != 0 {
		t.Errorf("the payment methods came from the source, not the template")
	}
}

func TestAC52_2_TheAdminRejectsAProjectSayingWhy(t *testing.T) {
	a := newAPI(t)
	starter, token := a.person("starter", false)
	_, admin := a.person("admin", true)
	project := a.started(token, false)
	for _, reason := range []string{`"  "`, `"` + strings.Repeat("س", 501) + `"`} {
		if code, answer := a.Ask("POST", "/projects/"+project+"/reject", `{"reason":`+reason+`}`, admin); code != 400 || testapi.Field(answer, "code") != "invalidReason" {
			t.Errorf("reason %.20s: %d %v", reason, code, answer)
		}
	}
	if code, _ := a.Ask("POST", "/projects/"+project+"/reject", `{}`, admin); code != 400 {
		t.Errorf("no reason: %d", code)
	}
	if code, _ := a.Ask("POST", "/projects/"+project+"/reject", `{"reason":" ليس مشروعا عقاريا "}`, admin); code != 200 {
		t.Fatalf("reject: %d", code)
	}
	_, listed := a.Ask("GET", "/projects", "", token)
	if testapi.Field(listed.([]any)[0], "status") != "rejected" || testapi.Field(listed.([]any)[0], "rejectionReason") != "ليس مشروعا عقاريا" {
		t.Errorf("after rejection: %v", listed)
	}
	if got := a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND kind = 4", starter); got != 1 {
		t.Errorf("the manager was told %d times", got)
	}
	if got := a.structureOf(project)["categories"]; got != 0 {
		t.Errorf("a rejected project got %d categories", got)
	}
	if code, answer := a.Ask("POST", "/projects/"+project+"/reject", `{"reason":"مرة ثانية"}`, admin); code != 409 || testapi.Field(answer, "code") != "notPending" {
		t.Errorf("rejected twice: %d %v", code, answer)
	}
}
