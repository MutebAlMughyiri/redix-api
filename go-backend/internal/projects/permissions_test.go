// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package projects_test

import (
	"testing"

	"github.com/google/uuid"
)

// BR-32: a permission ticked in a project counts at the next request, and taken away, counts no more - nothing remembers it.
func TestBR32_ANewRowGivesAPermissionAtTheNextRequest(t *testing.T) {
	a := newAPI(t)
	_, starter := a.person("starter", false)
	project := a.started(starter, true)
	member, token := a.person("member", false)
	a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ReadProject')`, uuid.NewString(), project, member)
	addUnit := func() int {
		code, _ := a.Ask("POST", "/projects/"+project+"/units", `{"name":"الفلة الغربية"}`, token)
		return code
	}
	if code := addUnit(); code != 403 {
		t.Errorf("a reader adds a unit: %d, want 403", code)
	}
	a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ManageProject')`, uuid.NewString(), project, member)
	if code := addUnit(); code != 200 {
		t.Errorf("once ManageProject is ticked: %d, want 200", code)
	}
	a.Exec(`DELETE FROM project_permissions WHERE user_id = ? AND permission = 'ManageProject'`, member)
	if code := addUnit(); code != 403 {
		t.Errorf("once it is taken: %d, want 403", code)
	}
}

// BR-29: a permission held everywhere reaches every project; a project that is not there is not found.
func TestBR29_APermissionHeldEverywhereReachesEveryProjectAndAnUnknownOneIsNotFound(t *testing.T) {
	a := newAPI(t)
	_, starter := a.person("starter", false)
	project := a.started(starter, true)
	admin, token := a.person("admin", true)
	a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ManageProject')`, uuid.NewString(), admin)
	if code, answer := a.Ask("POST", "/projects/"+project+"/close", ``, token); code != 200 {
		t.Errorf("closing a project the admin is not in: %d %v", code, answer)
	}
	if code, _ := a.Ask("POST", "/projects/"+uuid.NewString()+"/close", ``, token); code != 404 {
		t.Errorf("closing a project that is not there: %d, want 404", code)
	}
}
