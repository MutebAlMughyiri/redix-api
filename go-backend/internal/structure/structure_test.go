// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package structure_test

import (
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

type api struct {
	*testapi.API
	t       *testing.T
	token   string
	project string
}

// approved is an API with one approved project, its structure copied from the template, and its manager's token.
func approved(t *testing.T) api {
	a := api{API: testapi.New(t), t: t}
	a.token, a.project = a.ApprovedProject()
	return a
}

func (a api) ask(method, path, body string, want int) any {
	a.t.Helper()
	code, answer := a.Ask(method, a.project+path, body, a.token)
	if code != want {
		a.t.Fatalf("%s %s: %d %v, want %d", method, path, code, answer, want)
	}
	return answer
}

func (a api) lines(table, action string) int {
	return a.Count("SELECT count(*) FROM history WHERE table_name = ? AND action = ?", table, action)
}

func TestUC069_EveryChangeToTheStructureIsInTheHistory(t *testing.T) {
	a := approved(t)
	category := testapi.Field(a.ask("POST", "/categories", `{"name":"الحديد"}`, 201), "id").(string)
	item := testapi.Field(a.ask("POST", "/categories/"+category+"/items", `{"name":"التسليح","type":"purchases"}`, 201), "id").(string)
	a.ask("POST", "/items/"+item+"/subgroups", `{"name":"مواد"}`, 200)
	a.ask("PUT", "/categories/"+category, `{"name":"الحديد والتسليح"}`, 200)
	a.ask("POST", "/items/"+item+"/disable", ``, 200)
	a.ask("POST", "/items/"+item+"/columns", `{"name":"المقاس","dataType":"text","isRequired":false,"showInReports":true,"choices":[]}`, 200)
	columns := a.Count("SELECT count(*) FROM item_columns WHERE item_id = ?", item)
	for _, want := range []struct {
		table, action string
		count         int
	}{
		{"Categories", "Create", 1}, {"Categories", "Update", 1}, {"Items", "Create", 1}, {"Items", "Update", 1},
		{"Subgroups", "Create", 2}, {"ItemColumns", "Create", columns},
	} {
		if got := a.lines(want.table, want.action); got != want.count {
			t.Errorf("%s %s: %d lines, want %d", want.table, want.action, got, want.count)
		}
	}
	a.ask("POST", "/items/"+item+"/disable", ``, 200)
	if got := a.lines("Items", "Update"); got != 1 {
		t.Errorf("disabling twice wrote %d update lines, want 1: what did not change is not recorded", got)
	}
}

func TestFR38_AChangedTypeTakesTheTemplateOfItsTypeAndKeepsWhatAPersonAddedAfterIt(t *testing.T) {
	a := approved(t)
	category := testapi.Field(a.ask("POST", "/categories", `{"name":"الحديد"}`, 201), "id").(string)
	item := testapi.Field(a.ask("POST", "/categories/"+category+"/items", `{"name":"التسليح","type":"purchases"}`, 201), "id").(string)
	a.ask("POST", "/items/"+item+"/columns", `{"name":"المقاس","dataType":"text","isRequired":false,"showInReports":true,"choices":[]}`, 200)
	a.ask("PUT", "/items/"+item+"/type", `{"type":"contractorPayments"}`, 200)
	columns := a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any)
	template := a.ask("GET", "/default-columns/contractorPayments", ``, 200).([]any)
	if len(columns) != len(template)+1 || testapi.Field(columns[len(columns)-1], "name") != "المقاس" {
		t.Fatalf("columns after the change: %v", columns)
	}
	for i, column := range template {
		if testapi.Field(columns[i], "key") != testapi.Field(column, "key") {
			t.Errorf("column %d is %v, want the template's %v", i, testapi.Field(columns[i], "key"), testapi.Field(column, "key"))
		}
	}
	if code, answer := a.Ask("PUT", a.project+"/items/"+item+"/type", `{"type":"capital"}`, a.token); code != 409 || testapi.Field(answer, "code") != "capitalItemExists" {
		t.Errorf("a second capital item: %d %v", code, answer)
	}
}

func TestAC41_3_ATemplateColumnReachesExistingItemsOnlyWhenApplied(t *testing.T) {
	a := approved(t)
	category := testapi.Field(a.ask("POST", "/categories", `{"name":"الحديد"}`, 201), "id").(string)
	item := testapi.Field(a.ask("POST", "/categories/"+category+"/items", `{"name":"التسليح","type":"both"}`, 201), "id").(string)
	before := len(a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any))
	a.ask("POST", "/default-columns/both", `{"name":"الوزن","dataType":"number","isRequired":false,"showInReports":true,"choices":[],"applyToAllItems":false}`, 200)
	if got := len(a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any)); got != before {
		t.Errorf("not applied, the item has %d columns, want %d", got, before)
	}
	a.ask("POST", "/default-columns/both", `{"name":"الطول","dataType":"number","isRequired":false,"showInReports":true,"choices":[],"applyToAllItems":true}`, 200)
	columns := a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any)
	if len(columns) != before+1 || testapi.Field(columns[before], "name") != "الطول" {
		t.Fatalf("applied: %v", columns)
	}
	template := a.ask("GET", "/default-columns/both", ``, 200).([]any)
	added := testapi.Field(template[len(template)-1], "id").(string)
	a.ask("DELETE", "/default-columns/both/"+added+"?applyToAllItems=true", ``, 200)
	if got := len(a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any)); got != before {
		t.Errorf("after the delete applied to all, the item has %d columns, want %d", got, before)
	}
	if code, _ := a.Ask("DELETE", a.project+"/default-columns/both/"+added, ``, a.token); code != 400 {
		t.Errorf("a delete without applyToAllItems: %d, want 400", code)
	}
}

func TestR129_ATableTakesOneListWhoseChoicesGiveASign(t *testing.T) {
	a := approved(t)
	category := testapi.Field(a.ask("POST", "/categories", `{"name":"الحديد"}`, 201), "id").(string)
	item := testapi.Field(a.ask("POST", "/categories/"+category+"/items", `{"name":"التسليح","type":"purchases"}`, 201), "id").(string)
	code, answer := a.Ask("POST", a.project+"/items/"+item+"/columns",
		`{"name":"اتجاه","dataType":"list","isRequired":false,"showInReports":true,"choices":[{"name":"له","sign":"plus"},{"name":"عليه","sign":"minus"}]}`, a.token)
	if code != 400 || testapi.Field(answer, "code") != "signedListExists" {
		t.Errorf("a second signed list on a purchase item, which has نوع القيد: %d %v", code, answer)
	}
	a.ask("POST", "/items/"+item+"/columns", `{"name":"اللون","dataType":"list","isRequired":false,"showInReports":true,"choices":[{"name":" أحمر "},{"name":"أحمر"},{"name":""}]}`, 200)
	columns := a.ask("GET", "/items/"+item+"/columns", ``, 200).([]any)
	choices := testapi.Field(columns[len(columns)-1], "choices").([]any)
	if len(choices) != 1 || testapi.Field(choices[0], "name") != "أحمر" || testapi.Field(choices[0], "sign") != "none" {
		t.Errorf("the choices kept: %v", choices)
	}
}
