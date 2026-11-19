// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries_test

import (
	"encoding/json"
	"slices"
	"strconv"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

// sighted is a project whose item has two groups, its general one and «الأعمدة», with an approved entry of 100 in the first and one of 200 in the
// second, and Sara, who records, approves and sees reports there.
type sighted struct {
	project
	group, seenEntry, unseenEntry string
	sara                          uuid.UUID
	saraToken                     string
}

func newSighted(t *testing.T) sighted {
	a := sighted{project: approved(t, started)}
	if code, answer := a.Ask("POST", a.path+"/items/"+a.item+"/subgroups", `{"name":"الأعمدة"}`, a.token); code >= 300 {
		t.Fatalf("the group: %d %v", code, answer)
	}
	if err := a.Database.Read.QueryRow("SELECT id FROM subgroups WHERE name = 'الأعمدة'").Scan(&a.group); err != nil {
		t.Fatal(err)
	}
	a.sara, a.saraToken = a.member("سارة", "RecordEntries", "ApproveEntries", "SeeReports", "ReadProject")
	a.seenEntry, a.unseenEntry = a.sent(a.table, 100, a.token), a.sent(a.group, 200, a.token)
	if code, answer := a.Ask("POST", a.path+"/entries/approve", `{"entryIds":["`+a.seenEntry+`","`+a.unseenEntry+`"]}`, a.saraToken); code != 200 {
		t.Fatalf("approve: %d %v", code, answer)
	}
	return a
}

// sent records an entry of amount into table and sends it for approval, as token; it is the entry's id.
func (a sighted) sent(table string, amount int, token string) string {
	a.t.Helper()
	id := uuid.Must(uuid.NewV7()).String()
	if code, answer := a.Ask("POST", a.path+"/entries", a.typed(id, table, amount), token); code != 201 {
		a.t.Fatalf("recording into %s: %d %v", table, code, answer)
	}
	return id
}

func (a sighted) typed(id, table string, amount int) string {
	typed, _ := json.Marshal(map[string]any{"id": id, "itemId": a.item, "tableId": table, "date": "2026-11-10", "amount": amount, "accountId": a.account,
		"paymentMethodId": a.method, "description": "", "columnValues": map[string]string{}, "submit": true})
	return string(typed)
}

// tick ticks tables for Sara when seen and unticks them otherwise, in one request, as the manager; it is the status answered.
func (a sighted) tick(seen bool, tables ...string) int {
	code, _ := a.Ask("POST", a.path+"/users/"+a.sara.String()+"/tables", `{"seen":`+strconv.FormatBool(seen)+`,"tableIds":["`+strings.Join(tables, `","`)+`"]}`, a.token)
	return code
}

// listed is the ids of the entries «القيود» shows token.
func (a sighted) listed(token string) []string {
	a.t.Helper()
	code, answer := a.Ask("GET", a.path+"/entries", ``, token)
	if code != 200 {
		a.t.Fatalf("the entries: %d %v", code, answer)
	}
	var ids []string
	for _, entry := range answer.([]any) {
		ids = append(ids, testapi.Field(entry, "id").(string))
	}
	return ids
}

// redix-api #106 R5, R6, R9: before anything is unticked Sara sees both entries; unticked from «الأعمدة», its entry is neither listed nor found,
// to be opened, edited or removed, while the manager still sees both.
func TestR106_R5_R6_R9_AnUntickedTablesEntriesAreNeitherListedNorFound(t *testing.T) {
	a := newSighted(t)
	if listed := a.listed(a.saraToken); len(listed) != 2 {
		t.Fatalf("before any unticking Sara sees %v", listed)
	}
	if code := a.tick(false, a.group); code != 200 {
		t.Fatalf("untick: %d", code)
	}
	if listed := a.listed(a.saraToken); !slices.Equal(listed, []string{a.seenEntry}) {
		t.Errorf("Sara's entries: %v, want only %s", listed, a.seenEntry)
	}
	if listed := a.listed(a.token); len(listed) != 2 {
		t.Errorf("the manager's entries: %v", listed)
	}
	for _, path := range []string{"/entries/" + a.unseenEntry, "/entries/" + a.unseenEntry + "/row"} {
		if code, _ := a.Ask("GET", a.path+path, ``, a.saraToken); code != 404 {
			t.Errorf("GET %s for Sara: %d, want 404", path, code)
		}
	}
	if code, _ := a.Ask("GET", a.path+"/entries/"+a.seenEntry, ``, a.saraToken); code != 200 {
		t.Errorf("the seen entry for Sara: %d", code)
	}
	if code, _ := a.Ask("PUT", a.path+"/entries/"+a.unseenEntry+"/description", `{"description":"x"}`, a.saraToken); code != 404 {
		t.Errorf("describing the unseen entry: %d, want 404", code)
	}
	if code, _ := a.Ask("DELETE", a.path+"/entries/"+a.unseenEntry, ``, a.saraToken); code != 404 {
		t.Errorf("removing the unseen entry: %d, want 404", code)
	}
}

// redix-api #106 R6, D6: the summary, an account's balance and the item report count only what Sara sees, so her figures add up.
func TestR106_R6_TheSummaryTheBalancesAndTheItemReportCountOnlyTheSeenTables(t *testing.T) {
	a := newSighted(t)
	a.tick(false, a.group)
	for token, want := range map[string]float64{a.token: 300, a.saraToken: 100} {
		_, summary := a.Ask("GET", a.path+"/summary", ``, token)
		if got := testapi.Field(summary, "expenses"); got != want {
			t.Errorf("expenses: %v, want %v", got, want)
		}
		_, accounts := a.Ask("GET", a.path+"/accounts", ``, token)
		if got := testapi.Field(accounts.([]any)[0], "balance"); got != -want {
			t.Errorf("the balance: %v, want %v", got, -want)
		}
	}
	_, report := a.Ask("GET", a.path+"/reports/items/"+a.item, ``, a.saraToken)
	for _, table := range testapi.Field(report, "subgroups").([]any) {
		if testapi.Field(table, "id") == a.group {
			t.Errorf("Sara's item report holds the unseen group: %v", report)
		}
	}
	if got := testapi.Field(report, "total"); got != 100.0 {
		t.Errorf("Sara's item total: %v, want 100", got)
	}
}

// redix-api #106 R7, R12: the periodic report, its files and the VAT statement are one document for everyone, so someone narrowed gets none.
func TestR106_R7_R12_TheWholeDocumentsAreForWhoeverSeesEveryTable(t *testing.T) {
	a := newSighted(t)
	a.tick(false, a.group)
	for _, path := range []string{"/periodic-reports", "/periodic-reports/next", "/vat-statement/xlsx", "/reports/contracts/pdf"} {
		if code, _ := a.Ask("GET", a.path+path, ``, a.saraToken); code != 403 {
			t.Errorf("GET %s for Sara: %d, want 403", path, code)
		}
		if code, _ := a.Ask("GET", a.path+path, ``, a.token); code == 403 {
			t.Errorf("GET %s refused the manager", path)
		}
	}
}

// redix-api #106 R6, R8, R10, D1: the item's tables show Sara a table until she is unticked from it, recording into it then is refused, and
// ticking it again brings it and its entry back.
func TestR106_R8_R10_ATableIsOfferedUntilUntickedAndATickBringsItBack(t *testing.T) {
	a := newSighted(t)
	offered := func() int {
		_, tables := a.Ask("GET", a.path+"/items/"+a.item+"/tables", ``, a.saraToken)
		return len(tables.([]any))
	}
	if got := offered(); got != 2 {
		t.Fatalf("tables offered before unticking: %d", got)
	}
	a.tick(false, a.group)
	if got := offered(); got != 1 {
		t.Errorf("tables offered after unticking: %d, want 1", got)
	}
	code, answer := a.Ask("POST", a.path+"/entries", a.typed(uuid.Must(uuid.NewV7()).String(), a.group, 50), a.saraToken)
	if code != 400 || testapi.Field(answer, "code") != "unknownTable" {
		t.Errorf("recording into the unseen group: %d %v", code, answer)
	}
	if code := a.tick(false, a.group); code != 200 {
		t.Errorf("unticking twice: %d", code)
	}
	if code := a.tick(true, a.group); code != 200 {
		t.Errorf("ticking again: %d", code)
	}
	if got, listed := offered(), a.listed(a.saraToken); got != 2 || len(listed) != 2 {
		t.Errorf("after ticking again: %d tables, entries %v", got, listed)
	}
	if code := a.tick(false, uuid.NewString()); code != 404 {
		t.Errorf("unticking a table of no project: %d, want 404", code)
	}
}

// redix-api #106 R11: of the approvers, only those who see an entry's table are told it waits.
func TestR106_R11_OnlyThoseWhoSeeAnEntrysTableAreToldOfIt(t *testing.T) {
	a := newSighted(t)
	a.tick(false, a.group)
	told := func() int {
		return a.Count("SELECT count(*) FROM notifications WHERE user_id = ? AND kind = 0", a.sara)
	}
	before := told()
	a.sent(a.group, 30, a.token)
	if got := told(); got != before {
		t.Errorf("Sara was told of an entry of a table she does not see")
	}
	a.sent(a.table, 30, a.token)
	if got := told(); got != before+1 {
		t.Errorf("Sara was told %d times of an entry she sees, want 1", got-before)
	}
	_, pending := a.Ask("GET", a.path+"/entries/pending", ``, a.saraToken)
	if len(pending.([]any)) != 1 {
		t.Errorf("Sara's waiting entries: %v, want one", pending)
	}
}

// redix-api #106 R3, R13: a person is added with the tables they do not see, the users list says which, and every change to what someone sees is a
// line of «سجل المشروع».
func TestR106_R3_R13_ANewPersonStartsUntickedAndEveryChangeIsAHistoryLine(t *testing.T) {
	a := newSighted(t)
	person := func(username, unseen string) (int, any) {
		return a.Ask("POST", a.path+"/users", `{"displayName":"نواف","username":"`+username+`","mobile":"","email":"`+username+`@example.com","temporaryPassword":"Temporary-1",
			"permissions":["RecordEntries"],"unseenTableIds":["`+unseen+`"]}`, a.token)
	}
	if code, answer := person("nawaf", uuid.NewString()); code != 400 || testapi.Field(answer, "code") != "unknownTable" {
		t.Errorf("a new person unticked from a table of no project: %d %v", code, answer)
	}
	code, added := person("nawaf", a.group)
	if code != 201 {
		t.Fatalf("a new person: %d %v", code, added)
	}
	_, users := a.Ask("GET", a.path+"/users", ``, a.token)
	for _, user := range users.([]any) {
		if testapi.Field(user, "id") != testapi.Field(added, "id") {
			continue
		}
		unseen := testapi.Field(testapi.Field(user, "projects").([]any)[0], "unseenTables").([]any)
		if len(unseen) != 1 || unseen[0] != a.group {
			t.Errorf("the new person's unseen tables: %v", unseen)
		}
	}
	a.tick(false, a.group)
	a.tick(true, a.group)
	said := a.said(a.path+"/history", a.token)
	for _, want := range []string{"taken tableSight الأعمدة", "given tableSight الأعمدة"} {
		if !slices.Contains(said, want) {
			t.Errorf("«سجل المشروع» does not say %q: %v", want, said)
		}
	}
}

// redix-api #106 R4, D9, G2: «الجداول» lists every group and contract to whoever manages the people, one unticked for them too, so they can tick it
// back; anyone else only those they see; and the signed-in person's project says which they do not see.
func TestR106_D9_G2_TheTablesListHoldsEveryTableForAManagerAndTheSeenOnesForAnyoneElse(t *testing.T) {
	a := newSighted(t)
	a.tick(false, a.group)
	listed := func(token string) []string {
		var ids []string
		_, categories := a.Ask("GET", a.path+"/tables", ``, token)
		for _, category := range categories.([]any) {
			for _, item := range testapi.Field(category, "items").([]any) {
				for _, table := range testapi.Field(item, "tables").([]any) {
					ids = append(ids, testapi.Field(table, "id").(string))
				}
			}
		}
		return ids
	}
	if got := listed(a.saraToken); !slices.Contains(got, a.table) || slices.Contains(got, a.group) {
		t.Errorf("Sara's tables hold the general group %v and «الأعمدة» %v", slices.Contains(got, a.table), slices.Contains(got, a.group))
	}
	var manager string
	if err := a.Database.Read.QueryRow("SELECT user_id FROM project_permissions WHERE permission = 'ManageUsers'").Scan(&manager); err != nil {
		t.Fatal(err)
	}
	if code, _ := a.Ask("POST", a.path+"/users/"+manager+"/tables", `{"seen":false,"tableIds":["`+a.group+`"]}`, a.token); code != 200 {
		t.Fatalf("the manager unticking themselves: %d", code)
	}
	if got := listed(a.token); !slices.Contains(got, a.table) || !slices.Contains(got, a.group) {
		t.Errorf("the manager unticked from «الأعمدة» no longer lists it")
	}
	_, projects := a.Ask("GET", "/projects", ``, a.saraToken)
	if unseen := testapi.Field(projects.([]any)[0], "unseenTables").([]any); len(unseen) != 1 || unseen[0] != a.group {
		t.Errorf("Sara's project's unseen tables: %v", unseen)
	}
}

// redix-api #106: every route the sight reaches answers 500 wherever the database fails, and the changes among them write nothing.
func TestR106_EveryRouteTheSightReachesAnswersAFailingDatabase(t *testing.T) {
	a := newSighted(t)
	a.tick(false, a.group)
	sara, manager := "Bearer "+a.saraToken, "Bearer "+a.token
	person := `{"displayName":"نواف","username":"nawaf","mobile":"","email":"nawaf@example.com","temporaryPassword":"Temporary-1","permissions":["RecordEntries"],
		"unseenTableIds":["` + a.group + `"]}`
	steps := []struct{ method, path, body, token string }{
		{"GET", a.path + "/tables", ``, sara},
		{"GET", a.path + "/entries", ``, sara},
		{"GET", a.path + "/entries/" + a.seenEntry, ``, sara},
		{"GET", a.path + "/entries/pending", ``, sara},
		{"GET", a.path + "/approvals/history", ``, sara},
		{"GET", a.path + "/summary", ``, sara},
		{"GET", a.path + "/accounts", ``, sara},
		{"GET", a.path + "/reports/items/" + a.item, ``, sara},
		{"GET", a.path + "/items/" + a.item + "/tables", ``, sara},
		{"GET", "/projects", ``, sara},
		{"PUT", a.path + "/entries/" + a.seenEntry + "/description", `{"description":"x"}`, sara},
		{"POST", a.path + "/entries", a.typed(uuid.Must(uuid.NewV7()).String(), a.table, 10), manager},
		{"POST", a.path + "/users", person, manager},
		{"POST", a.path + "/users/" + a.sara.String() + "/tables", `{"seen":true,"tableIds":["` + a.group + `","` + a.table + `"]}`, manager},
		{"POST", a.path + "/users/" + a.sara.String() + "/tables", `{"seen":false,"tableIds":["` + a.group + `","` + a.table + `"]}`, manager},
		{"POST", a.path + "/users/" + a.sara.String() + "/tables", `{"seen":false,"tableIds":["` + uuid.NewString() + `"]}`, manager},
	}
	for _, step := range steps {
		for n := int64(1); ; n++ {
			before := a.Count("SELECT count(*) FROM history")
			answered, failed := a.SendFailing(n, step.method, step.path, step.body, "Authorization", step.token)
			if !failed {
				break
			}
			if answered.Code != 500 || a.Count("SELECT count(*) FROM history") != before {
				t.Errorf("%s %s, the database failing at its call %d: %d", step.method, step.path, n, answered.Code)
			}
		}
	}
}

// redix-api #106 G1, R4, R9: a contractor's payment is its contract's to see: unticked from the contract, Sara neither lists the payment nor opens
// the contract's report, nor finds it on «الجداول»; ticked again, both are back.
func TestR106_G1_AContractorsPaymentIsItsContractsToSee(t *testing.T) {
	a := newSighted(t)
	_, supplier := a.Ask("POST", a.path+"/suppliers", `{"name":"مؤسسة الإتقان"}`, a.token)
	a.Ask("PUT", a.path+"/items/"+a.item+"/type", `{"type":"both"}`, a.token)
	if code, answer := a.Ask("POST", a.path+"/contracts", `{"supplierId":"`+testapi.Field(supplier, "id").(string)+`","itemId":"`+a.item+`","value":1000,"isOpenValue":false}`,
		a.token); code >= 300 {
		t.Fatalf("the contract: %d %v", code, answer)
	}
	var contract string
	if err := a.Database.Read.QueryRow("SELECT id FROM contracts").Scan(&contract); err != nil {
		t.Fatal(err)
	}
	payment := a.sent(contract, 300, a.token)
	a.Ask("POST", a.path+"/entries/approve", `{"entryIds":["`+payment+`"]}`, a.saraToken)
	if code := a.tick(false, contract); code != 200 {
		t.Fatalf("unticking the contract: %d", code)
	}
	if listed := a.listed(a.saraToken); slices.Contains(listed, payment) {
		t.Errorf("Sara lists the payment of a contract she does not see")
	}
	if code, _ := a.Ask("GET", a.path+"/reports/contracts/"+contract, ``, a.saraToken); code != 404 {
		t.Errorf("the unseen contract's report for Sara: %d, want 404", code)
	}
	if code := a.tick(false, contract); code != 200 {
		t.Errorf("unticking the contract twice: %d", code)
	}
	if code := a.tick(true, contract); code != 200 {
		t.Errorf("ticking the contract again: %d", code)
	}
	if listed := a.listed(a.saraToken); !slices.Contains(listed, payment) {
		t.Errorf("ticked again, Sara does not list the payment")
	}
	_, report := a.Ask("GET", a.path+"/reports/contracts/"+contract, ``, a.saraToken)
	if contracts := testapi.Field(report, "contracts").([]any); len(contracts) != 1 {
		t.Errorf("ticked again, the contract's report for Sara: %v", report)
	}
}
