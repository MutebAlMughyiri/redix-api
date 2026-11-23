// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web_test

import (
	"slices"
	"strings"
	"testing"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/web"
)

// gate is a control the web offers only to whoever holds permission, as the app's own (redix-app test/auth/permission_gates_test.dart): the
// element id names in the answer to path, usable when it is drawn and not disabled.
type gate struct {
	permission access.Permission
	path, id   string
}

// usable says whether body draws the element id names, and not disabled.
func usable(body, id string) bool {
	at := strings.Index(body, `id="`+id+`"`)
	if at < 0 {
		return false
	}
	start, end := strings.LastIndex(body[:at], "<"), at+strings.Index(body[at:], ">")
	return !strings.Contains(body[start:end], " disabled")
}

// gates are every control the web offers by a permission: each side-menu item and tab of menuNeeds, then the buttons the pages gray or leave
// out; entries are w's own, a draft, one sent for approval and an approved one.
func (w working) gates(draft, sent, approved string) []gate {
	var gates []gate
	for name, permission := range web.MenuNeeds {
		if web.InSideMenu(name) {
			gates = append(gates, gate{permission, "/", "web.item." + name})
		} else {
			gates = append(gates, gate{permission, "/tab/" + name, "web.tab." + name})
		}
	}
	return append(gates,
		gate{access.ApproveProjects, "/", "sharedSettings.registered"},
		gate{access.ManageProject, "/tab/projectInfo", "projectInfo.rename"},
		gate{access.ManageProject, "/tab/documents", "documents.add"},
		gate{access.ManageSettings, "/tab/accounts", "accounts.add"},
		gate{access.ManageSettings, "/tab/partners", "partners.add"},
		gate{access.ManageSettings, "/tab/suppliers", "suppliers.add"},
		gate{access.ManageSettings, "/tab/contracts", "contracts.add"},
		gate{access.MoveMoney, "/tab/accounts", "accounts.transfer"},
		gate{access.MoveMoney, "/tab/accounts", "accounts.update"},
		gate{access.RecordEntries, "/tab/entries", "entries.new"},
		gate{access.RecordEntries, "/tab/home", "home.addEntry." + w.id},
		gate{access.AttachFiles, "/tab/addEntry", "addEntry.fileSection"},
		gate{access.AddSupplier, "/tab/addEntry", "addEntry.quickAddOpen"},
		gate{access.ApproveEntries, "/details/entry/" + sent, "entry.approve"},
		gate{access.ApproveEntries, "/details/entry/" + sent, "entry.reject"},
		gate{access.ManageAnyEntry, "/details/entry/" + sent, "entry.change"},
		gate{access.DeleteEntries, "/details/entry/" + draft, "entry.remove"},
		gate{access.RemoveApprovedEntries, "/details/entry/" + approved, "entry.remove"},
		gate{access.GenerateReports, "/tab/reports", "reports.issue"},
	)
}

// FR-03: every control the web offers by a permission is usable by whoever holds it and gray or left out without it, the permission taken in
// the project, or everywhere for approving projects; asked again at each request, as the API's gate is (BR-32).
func TestWeb_FR03_EveryControlAPermissionGatesIsUsableWithItAndNotWithout(t *testing.T) {
	w := newWorking(t)
	w.Ask("POST", w.path+"/accounts", `{"name":"حساب احتياطي","type":"bank"}`, w.token)
	draft, sent, approved := w.drafted(100), w.sent(200), w.approved(300)
	w.home()
	for _, gate := range w.gates(draft, sent, approved) {
		table, scope, in := "project_permissions", "project_id = ? AND ", []any{w.id}
		if gate.permission == access.ApproveProjects {
			table, scope, in = "system_permissions", "", nil
		}
		w.Exec(`DELETE FROM `+table+` WHERE `+scope+`user_id = ? AND permission = ?`, append(in, w.manager, gate.permission)...)
		if usable(w.browser.Send("GET", gate.path, nil).Body.String(), gate.id) {
			t.Errorf("%s at %s is usable without %s", gate.id, gate.path, gate.permission)
		}
		columns, values := "project_id, ", "?, "
		if table == "system_permissions" {
			columns, values = "", ""
		}
		w.Exec(`INSERT INTO `+table+` (id, `+columns+`user_id, permission) VALUES (?, `+values+`?, ?)`, append(append([]any{uuid.NewString()}, in...), w.manager, gate.permission)...)
		if !usable(w.browser.Send("GET", gate.path, nil).Body.String(), gate.id) {
			t.Errorf("%s at %s is not usable with %s", gate.id, gate.path, gate.permission)
		}
		if table == "system_permissions" {
			w.Exec(`DELETE FROM system_permissions WHERE user_id = ? AND permission = ?`, w.manager, gate.permission)
		}
	}
}

// FR-03, as the app's findings row 134: every permission grays or leaves out a control of the web, but reading the project, which is
// membership itself, charging any account, which only widens the accounts offered, and the two capital ticks, which narrow or widen what the
// API answers.
func TestWeb_FR03_EveryPermissionGatesAControl(t *testing.T) {
	gated := map[access.Permission]bool{}
	for _, gate := range (working{}).gates("", "", "") {
		gated[gate.permission] = true
	}
	for _, permission := range access.All {
		exempt := slices.Contains([]access.Permission{access.ReadProject, access.ChargeAnyAccount, access.SeeChosenCapital, access.SeeAllCapital}, permission)
		if gated[permission] == exempt {
			t.Errorf("%s gates a control: %v", permission, gated[permission])
		}
	}
}
