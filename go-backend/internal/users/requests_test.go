// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users_test

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
)

func (a api) requests() string { return "/projects/" + project.String() + "/permission-requests" }

// held is what userID holds in the project, by name.
func (a api) held(userID string) int {
	return a.Count("SELECT count(*) FROM project_permissions WHERE project_id = ? AND user_id = ?", project, userID)
}

// redix-api #112 R1, R2, R5, R6, R9: a person asks for one permission given and one taken, with a reason; nothing changes; the managers are told,
// the request waits on their list with the person's name, and «سجلي»'s neighbour, «سجل المشروع», holds it.
func TestR112_APersonAsksAndNothingChangesUntilAManagerDecides(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject, access.SeeReports)
	code, answer := a.ask("POST", a.requests(), `{"gives":["recordEntries"],"takes":["seeReports"],"reason":"أسجل المصروفات"}`, a.as(sara))
	if code != 201 || field(answer, "status") != "waiting" || field(answer, "reason") != "أسجل المصروفات" {
		t.Fatalf("ask: %d %v", code, answer)
	}
	if a.held(sara.String()) != 2 {
		t.Error("the request changed a permission before it was approved")
	}
	if got := a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND kind = 5", a.manager); got != 1 {
		t.Errorf("the manager was told %d times", got)
	}
	_, listed := a.ask("GET", a.requests(), ``, a.as(a.manager))
	rows := listed.([]any)
	if len(rows) != 1 || field(rows[0], "displayName") != "سارة" || field(rows[0], "gives").([]any)[0] != "recordEntries" || field(rows[0], "takes").([]any)[0] != "seeReports" {
		t.Errorf("the managers' list: %v", listed)
	}
	if _, mine := a.ask("GET", a.requests()+"/mine", ``, a.as(sara)); len(mine.([]any)) != 1 {
		t.Errorf("mine: %v", mine)
	}
	if code, _ := a.ask("GET", a.requests(), ``, a.as(sara)); code != 403 {
		t.Errorf("the list for someone who does not manage the users: %d", code)
	}
}

// redix-api #125: a request whose kept permissions are not a list of names, a shape the database's json_valid lets through but the API never
// writes, answers 500 on the person's own list, the managers' list and the approval, where it read as no permission; approving it changes nothing.
func TestR125_ARequestKeptInAShapeTheAPINeverWritesAnswers500AndChangesNothing(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject, access.SeeReports)
	code, answer := a.ask("POST", a.requests(), `{"gives":["recordEntries"],"takes":["seeReports"],"reason":"أسجل المصروفات"}`, a.as(sara))
	if code != 201 {
		t.Fatalf("ask: %d %v", code, answer)
	}
	a.Exec(`UPDATE permission_requests SET gives = '{}'`)
	for _, asked := range []struct{ path, token string }{{a.requests() + "/mine", a.as(sara)}, {a.requests(), a.as(a.manager)}} {
		if code, _ := a.ask("GET", asked.path, ``, asked.token); code != 500 {
			t.Errorf("%s: %d, want 500", asked.path, code)
		}
	}
	if code, _ := a.ask("POST", a.requests()+"/"+field(answer, "id").(string)+"/approve", ``, a.as(a.manager)); code != 500 {
		t.Errorf("approve: %d, want 500", code)
	}
	if a.held(sara.String()) != 2 {
		t.Error("approving a request it could not read changed a permission")
	}
}

// redix-api #112 R4, R6, R11: an approved request is applied as ticking is, the person told; a permission ticked by hand meanwhile changes nothing.
func TestR112_AnApprovedRequestIsAppliedAsTickingIsAndThePersonIsTold(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject, access.SeeReports)
	_, asked := a.ask("POST", a.requests(), `{"gives":["recordEntries","moveMoney"],"takes":["seeReports"],"reason":"أسجل المصروفات"}`, a.as(sara))
	a.exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES ('11111111-1111-7111-8111-111111111111', ?, ?, 'MoveMoney')`, project, sara)
	if code, answer := a.ask("POST", a.requests()+"/"+field(asked, "id").(string)+"/approve", ``, a.as(a.manager)); code != 200 {
		t.Fatalf("approve: %d %v", code, answer)
	}
	for permission, want := range map[string]int{"RecordEntries": 1, "MoveMoney": 1, "SeeReports": 0, "ReadProject": 1} {
		if got := a.Count("SELECT count(*) FROM project_permissions WHERE user_id = ? AND permission = ?", sara, permission); got != want {
			t.Errorf("%s: %d, want %d", permission, got, want)
		}
	}
	if _, mine := a.ask("GET", a.requests()+"/mine", ``, a.as(sara)); field(mine.([]any)[0], "status") != "approved" {
		t.Errorf("mine after approval: %v", mine)
	}
	if got := a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND kind = 6", sara); got != 1 {
		t.Errorf("the person was told %d times", got)
	}
	if code, _ := a.ask("POST", a.requests()+"/"+field(asked, "id").(string)+"/approve", ``, a.as(a.manager)); code != 404 {
		t.Errorf("approved twice: %d", code)
	}
}

// redix-api #112 R8: a rejection needs its reason, changes nothing, and the person reads it.
func TestR112_ARejectionCarriesItsReasonAndChangesNothing(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject)
	_, asked := a.ask("POST", a.requests(), `{"gives":["recordEntries"],"reason":"أسجل المصروفات"}`, a.as(sara))
	path := a.requests() + "/" + field(asked, "id").(string) + "/reject"
	if code, answer := a.ask("POST", path, `{"reason":" "}`, a.as(a.manager)); code != 400 || field(answer, "code") != "reasonRequired" {
		t.Errorf("a rejection with no reason: %d %v", code, answer)
	}
	if code, _ := a.ask("POST", path, `{"reason":"ليس الآن"}`, a.as(a.manager)); code != 200 {
		t.Fatalf("reject: %d", code)
	}
	_, mine := a.ask("GET", a.requests()+"/mine", ``, a.as(sara))
	if field(mine.([]any)[0], "status") != "rejected" || field(mine.([]any)[0], "rejectionReason") != "ليس الآن" || a.held(sara.String()) != 1 {
		t.Errorf("mine after the rejection: %v", mine)
	}
	if got := a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND kind = 7", sara); got != 1 {
		t.Errorf("the person was told %d times", got)
	}
}

// redix-api #112 R7: a new request replaces the waiting one, and the person may withdraw theirs.
func TestR112_ANewRequestReplacesTheWaitingOneAndOneIsWithdrawn(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject)
	a.ask("POST", a.requests(), `{"gives":["recordEntries"],"reason":"أولًا"}`, a.as(sara))
	a.ask("POST", a.requests(), `{"gives":["seeReports"],"reason":"ثانيًا"}`, a.as(sara))
	_, listed := a.ask("GET", a.requests(), ``, a.as(a.manager))
	if rows := listed.([]any); len(rows) != 1 || field(rows[0], "reason") != "ثانيًا" {
		t.Errorf("waiting: %v", listed)
	}
	if code, _ := a.ask("DELETE", a.requests()+"/mine", ``, a.as(sara)); code != 200 {
		t.Errorf("withdraw: %d", code)
	}
	if code, _ := a.ask("DELETE", a.requests()+"/mine", ``, a.as(sara)); code != 404 {
		t.Errorf("withdraw with none waiting: %d", code)
	}
	if _, listed := a.ask("GET", a.requests(), ``, a.as(a.manager)); len(listed.([]any)) != 0 {
		t.Errorf("waiting after the withdrawal: %v", listed)
	}
}

// redix-api #112 R10, R8: a request asks only for what changes something, with a reason.
func TestR112_ARequestAsksOnlyForWhatChangesSomething(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject, access.SeeReports)
	for body, code := range map[string]string{
		`{"reason":"لا شيء"}`:                               "nothingAsked",
		`{"gives":["nonsense"],"reason":"x"}`:               "invalidPermission",
		`{"gives":["approveProjects"],"reason":"x"}`:        "invalidPermission",
		`{"takes":["readProject"],"reason":"x"}`:            "invalidPermission",
		`{"gives":["seeReports"],"reason":"x"}`:             "alreadyHeld",
		`{"takes":["moveMoney"],"reason":"x"}`:              "notHeld",
		`{"gives":["moveMoney"],"reason":"  "}`:             "reasonRequired",
		`{"gives":["moveMoney"],"reason":"` + long() + `"}`: "invalidReason",
	} {
		if got, answer := a.ask("POST", a.requests(), body, a.as(sara)); got != 400 || field(answer, "code") != code {
			t.Errorf("%s: %d %v, want %s", body, got, answer, code)
		}
	}
	if _, mine := a.ask("GET", a.requests()+"/mine", ``, a.as(sara)); len(mine.([]any)) != 0 {
		t.Errorf("a refused request was kept: %v", mine)
	}
}

func long() string {
	text := ""
	for len([]rune(text)) <= 500 {
		text += "سبب "
	}
	return text
}

// redix-api #112 R4, BR-29: approving a request that takes managing users from the last who has it is lastManager, and nothing changes.
func TestR112_TheLastManagerKeepsManagingUsersWhateverTheyAsk(t *testing.T) {
	a := newAPI(t)
	_, asked := a.ask("POST", a.requests(), `{"takes":["manageUsers"],"reason":"أتنحى"}`, a.as(a.manager))
	if code, answer := a.ask("POST", a.requests()+"/"+field(asked, "id").(string)+"/approve", ``, a.as(a.manager)); code != 409 || field(answer, "code") != "lastManager" {
		t.Errorf("approve: %d %v", code, answer)
	}
	if a.held(a.manager.String()) != 2 {
		t.Error("the last manager lost a permission")
	}
}

// redix-api #141 D1: the database failing at any call of asking, reading, withdrawing, approving or refusing a request answers 500 and changes
// nothing.
func TestR112_AFailingDatabaseAtAnyCallOfARequestAnswers500AndChangesNothing(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject, access.SeeReports)
	_, asked := a.ask("POST", a.requests(), `{"gives":["recordEntries"],"takes":["seeReports"],"reason":"أسجل المصروفات"}`, a.as(sara))
	decide := a.requests() + "/" + field(asked, "id").(string)
	steps := []struct{ method, path, body, token string }{
		{"GET", a.requests() + "/mine", ``, a.as(sara)},
		{"GET", a.requests(), ``, a.as(a.manager)},
		{"POST", a.requests(), `{"gives":["moveMoney"],"reason":"أنقل المال"}`, a.as(sara)},
		{"POST", decide + "/reject", `{"reason":"ليس الآن"}`, a.as(a.manager)},
	}
	for _, step := range steps {
		for n := int64(1); ; n++ {
			before := a.Count("SELECT count(*) FROM history")
			answered, failed := a.SendFailing(n, step.method, step.path, step.body, "Authorization", "Bearer "+step.token)
			if !failed {
				break
			}
			if answered.Code != 500 || a.Count("SELECT count(*) FROM history") != before {
				t.Errorf("%s %s, the database failing at its call %d: %d", step.method, step.path, n, answered.Code)
			}
		}
	}
	_, latest := a.ask("GET", a.requests()+"/mine", ``, a.as(sara))
	again := a.requests() + "/" + field(latest.([]any)[0], "id").(string)
	for _, step := range []struct{ method, path, token string }{{"POST", again + "/approve", a.as(a.manager)}, {"DELETE", a.requests() + "/mine", a.as(sara)}} {
		for n := int64(1); ; n++ {
			answered, failed := a.SendFailing(n, step.method, step.path, ``, "Authorization", "Bearer "+step.token)
			if !failed {
				break
			}
			if answered.Code != 500 {
				t.Errorf("%s %s, the database failing at its call %d: %d", step.method, step.path, n, answered.Code)
			}
		}
	}
}

// Findings row 289 (the owner, 23 Nov 2026): on a finished project a person still asks, and a manager still approves, as the users they change stay
// manageable there and the app offers both.
func TestR112_D289_RequestsStayOpenOnAFinishedProjectAsTheUsersDo(t *testing.T) {
	a := newAPI(t)
	sara := a.person("sara", "سارة", project, access.ReadProject)
	a.Exec("UPDATE projects SET status = 1 WHERE id = ?", project)
	code, asked := a.ask("POST", a.requests(), `{"gives":["recordEntries"],"reason":"أسجل المصروفات"}`, a.as(sara))
	if code != 201 {
		t.Fatalf("asking on a finished project: %d %v", code, asked)
	}
	if code, answer := a.ask("POST", a.requests()+"/"+field(asked, "id").(string)+"/approve", ``, a.as(a.manager)); code != 200 {
		t.Errorf("approving on a finished project: %d %v", code, answer)
	}
	if a.held(sara.String()) != 2 {
		t.Error("the approval did not tick the permission")
	}
}

