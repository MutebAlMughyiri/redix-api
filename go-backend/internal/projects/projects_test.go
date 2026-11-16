package projects_test

import (
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
	code, answer := a.Ask("POST", "/projects", `{"name":"غصون","startDate":"2026-11-01","units":["أ","ب"],"partners":["شريك"]}`, token)
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
	answered := a.Send("POST", "/projects", `{"name":" غصون ","startDate":"2026-11-01","units":[" أ ","ب"],"description":" وصف ","partners":["شريك"]}`,
		"Authorization", "Bearer "+token)
	if answered.Code != 201 || !strings.HasPrefix(answered.Header().Get("Location"), "/projects/") {
		t.Fatalf("%d %s", answered.Code, answered.Body)
	}
	body := answered.Body.String()
	for _, want := range []string{`"name":"غصون"`, `"status":"pendingApproval"`, `"description":"وصف"`, `"permissions":[]`, `"holdsEntries":false`, `"name":"أ"`} {
		if !strings.Contains(body, want) {
			t.Errorf("%s not in %s", want, body)
		}
	}
	if got := a.Count("SELECT count(*) FROM project_permissions WHERE user_id = ?", starter); got != 16 {
		t.Errorf("the starter holds %d permissions, want 16", got)
	}
	if got := a.Count("SELECT count(*) FROM partners"); got != 1 {
		t.Errorf("partners: %d", got)
	}
}

func TestUC004_AProjectIsRefusedForEachReason(t *testing.T) {
	a := newAPI(t)
	_, token := a.person("starter", false)
	for body, want := range map[string]string{
		`{"name":"","startDate":"2026-11-01","units":[]}`:                                                       "invalidName",
		`{"name":"غصون","startDate":"2026-11-01","units":["أ"," أ "]}`:                                          "unitNameTaken",
		`{"name":"غصون","startDate":"2026-11-01","units":[""]}`:                                                 "invalidName",
		`{"name":"غصون","startDate":"2026-11-01","units":[],"partners":[""]}`:                                   "invalidName",
		`{"name":"غصون","startDate":"2026-11-01","units":[],"description":"` + strings.Repeat("و", 2001) + `"}`: "invalidDescription",
		`{"name":"غصون","startDate":"2026-11-01","units":[],"structureSourceId":"` + uuid.NewString() + `"}`:    "unknownSource",
	} {
		if code, answer := a.Ask("POST", "/projects", body, token); code != 400 || testapi.Field(answer, "code") != want {
			t.Errorf("%.60s: %d %v, want %s", body, code, answer, want)
		}
	}
}

func TestUC066_SomeoneWithNoAccountRegistersAProjectAndTheAdminSeesItWaiting(t *testing.T) {
	a := newAPI(t)
	code, answer := a.Ask("POST", "/projects/register", `{"name":"مشروع","startDate":"2026-11-05","units":["أ"],"displayName":"نواف","username":"nawaf",
		"mobile":"0500000000","password":"Register-1"}`, "")
	if code != 201 || testapi.Field(answer, "status") != "pendingApproval" {
		t.Fatalf("%d %v", code, answer)
	}
	if code, answer := a.Ask("POST", "/projects/register", `{"name":"مشروع","startDate":"2026-11-05","units":[],"displayName":"x","username":"NAWAF",
		"mobile":"","password":"Register-1"}`, ""); code != 409 || testapi.Field(answer, "code") != "usernameTaken" {
		t.Errorf("a taken username: %d %v", code, answer)
	}
	_, admin := a.person("admin", true)
	_, waiting := a.Ask("GET", "/projects/registered", "", admin)
	listed := waiting.([]any)
	if len(listed) != 1 || testapi.Field(listed[0], "managerUsername") != "nawaf" || testapi.Field(listed[0], "managerMobile") != "0500000000" {
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
	if permissions := testapi.Field(listed[0], "permissions").([]any); len(permissions) != 16 {
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
	if code, answer := a.Ask("PUT", project, `{"name":" غصون ٦ "}`, token); code != 200 || testapi.Field(answer, "name") != "غصون ٦" {
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
