// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package projectfile_test

import (
	"fmt"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

// fileWithBalance is a project file holding no rows and one actual balance, of the account called "حساب A", written as balance.
func fileWithBalance(balance string) []byte {
	return fmt.Appendf(nil, `{"format":"redix.project","version":1,"project":"مشروع A","report":{"number":0},
		"accounts":[{"name":"حساب A","actualBalance":%s}],"lists":[],"categories":[]}`, balance)
}

// approvedProject is a new approved project holding the account "حساب A", and its manager's token.
func approvedProject(a *testapi.API) (string, string) {
	_, manager := a.Person("A")
	adminID, admin := a.Person("B")
	a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), adminID)
	_, answer := a.Ask("POST", "/projects", `{"name":"مشروع A","startDate":"2026-11-01","units":[]}`, manager)
	id := testapi.Field(answer, "id").(string)
	a.Ask("POST", "/projects/"+id+"/approve", ``, admin)
	a.Ask("POST", "/projects/"+id+"/accounts", `{"name":"حساب A","type":"bank"}`, manager)
	return id, manager
}

// #125: an actual balance that is not an amount was read as 0; it is refused under the reconciliation's words, naming its account, and nothing
// is saved. The same file with an amount loads its one reconciliation.
func TestAnActualBalanceThatIsNotAnAmountIsRefusedNamingItsAccountAndNothingIsSaved(t *testing.T) {
	a := testapi.New(t)
	id, manager := approvedProject(a)
	path := "/projects/" + id + "/project-file"

	code, answer := a.AskWithFile(path, "project.json", fileWithBalance(`"ten"`), manager, nil)
	rows, _ := answer["rows"].([]any)
	if code != 400 || answer["code"] != "rowsRefused" || len(rows) != 1 {
		t.Fatalf("a balance that is not an amount: %d %v", code, answer)
	}
	if row := rows[0].(map[string]any); row["owner"] != "reconciliation" || row["code"] != "invalidAmount" || row["value"] != "حساب A" {
		t.Errorf("refused as %v, want the reconciliation's invalidAmount naming حساب A", row)
	}
	if kept := a.Count(`SELECT count(*) FROM reconciliations WHERE project_id = ?`, id); kept != 0 {
		t.Errorf("%d reconciliations kept after a refusal", kept)
	}

	if code, answer := a.AskWithFile(path, "project.json", fileWithBalance(`10.5`), manager, nil); code != 200 {
		t.Fatalf("a balance that is an amount: %d %v", code, answer)
	}
	if kept := a.Count(`SELECT count(*) FROM reconciliations WHERE project_id = ?`, id); kept != 1 {
		t.Errorf("%d reconciliations kept, want the file's one", kept)
	}
}
