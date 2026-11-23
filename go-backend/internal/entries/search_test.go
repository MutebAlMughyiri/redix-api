// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries_test

import (
	"encoding/json"
	"slices"
	"strings"
	"testing"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// sale is the project's sale item and its general table, which revenue goes on.
func (a project) sale() (item, table string) {
	a.t.Helper()
	project := strings.TrimPrefix(a.path, "/projects/")
	if err := a.Database.Read.QueryRow(`SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND items.type = 3`,
		project).Scan(&item); err != nil {
		a.t.Fatal(err)
	}
	if err := a.Database.Read.QueryRow(`SELECT id FROM subgroups WHERE item_id = ?`, item).Scan(&table); err != nil {
		a.t.Fatal(err)
	}
	return item, table
}

// sent records amount on item's table, today, and sends it for approval: an expense through the project's payment method, revenue with none. It
// is the entry's id.
func (a project) sent(item, table string, amount int) string {
	a.t.Helper()
	body := map[string]any{"id": uuid.Must(uuid.NewV7()), "itemId": item, "tableId": table, "date": a.Clock.Now().Format("2006-01-02"), "amount": amount,
		"accountId": a.account, "description": "", "columnValues": map[string]string{}, "submit": true}
	if item == a.item {
		body["paymentMethodId"] = a.method
	}
	sent, _ := json.Marshal(body)
	code, answer := a.Ask("POST", a.path+"/entries", string(sent), a.token)
	if code != 201 {
		a.t.Fatalf("recording %d: %d %v", amount, code, answer)
	}
	return testapi.Field(answer, "id").(string)
}

// listed is each entry the list at query answers token, as its number and kind, in the list's order.
func (a project) listed(query, token string) []string {
	a.t.Helper()
	var shown []string
	for _, line := range a.lines(a.path+"/entries"+query, token) {
		number, _ := json.Marshal(line["number"])
		shown = append(shown, string(number)+" "+line["kind"].(string))
	}
	return shown
}

// redix-app #104: the entries list gives only what the search keeps - the kinds asked for, the text an entry holds -, newest first, a page of
// take at a time.
func TestApp104_TheListGivesOnlyWhatTheSearchKeepsTheNewestFirstAPageAtATime(t *testing.T) {
	a := approved(t, started)
	saleItem, saleTable := a.sale()
	a.sent(saleItem, saleTable, 100)
	a.sent(saleItem, saleTable, 200)
	for _, amount := range []int{300, 400, 500} {
		a.sent(a.item, a.table, amount)
	}
	for query, want := range map[string][]string{
		"": {"5 purchase", "4 purchase", "3 purchase", "2 revenue", "1 revenue"},
		"?kinds=purchase&kinds=contractorPayment&take=2": {"5 purchase", "4 purchase"},
		"?text=200": {"2 revenue"},
	} {
		if got := a.listed(query, a.token); !slices.Equal(got, want) {
			t.Errorf("%q lists %v, want %v", query, got, want)
		}
	}
}

// redix-app #104: where entries stand, whose they are and their period are read from the query, for whoever reads the project and sees its
// reports.
func TestApp104_WhereEntriesStandWhoseTheyAreAndTheirPeriodAreReadFromTheQuery(t *testing.T) {
	a := approved(t, started)
	saleItem, saleTable := a.sale()
	approvedRevenue := a.sent(saleItem, saleTable, 100)
	if code, answer := a.Ask("POST", a.path+"/entries/approve", `{"entryIds":["`+approvedRevenue+`"]}`, a.token); code != 200 {
		t.Fatalf("approving: %d %v", code, answer)
	}
	a.sent(a.item, a.table, 300)
	_, reader := a.member("B", "ReadProject", "SeeReports")
	today, tomorrow := a.Clock.Now().Format("2006-01-02"), a.Clock.Now().AddDate(0, 0, 1).Format("2006-01-02")
	for query, want := range map[string][]string{
		"":                                {"2 purchase", "1 revenue"},
		"?statuses=pendingApproval":       {"2 purchase"},
		"?from=" + today + "&to=" + today: {"2 purchase", "1 revenue"},
		"?from=" + tomorrow:               nil,
		"?mine=true":                      nil,
	} {
		if got := a.listed(query, reader); !slices.Equal(got, want) {
			t.Errorf("%q lists %v, want %v", query, got, want)
		}
	}
}
