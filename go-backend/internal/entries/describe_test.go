// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"slices"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

// UC-069 R4 (D9): every thing names its record from History's own values - its own field, or the records its values point at - so a line says
// what it was done to with no table read.
func TestR4_EveryThingNamesItsRecordFromHistorysOwnValues(t *testing.T) {
	account, other, partner, supplier, item, entry, file, contract := uuid.New(), uuid.New(), uuid.New(), uuid.New(), uuid.New(), uuid.New(), uuid.New(), uuid.New()
	sara := uuid.New()
	line := func(table string, id uuid.UUID, after string) store.History {
		return store.History{TableName: table, RecordID: id, Action: "Create", Before: `{}`, After: after}
	}
	of := func(id uuid.UUID) string { return `"` + id.String() + `"` }
	known := knowledgeOf([]store.History{
		line("Accounts", account, `{"Name":"الراجحي"}`),
		line("Accounts", other, `{"Name":"الصندوق"}`),
		line("Partners", partner, `{"Name":"زيد"}`),
		line("Suppliers", supplier, `{"Name":"مؤسسة البناء"}`),
		line("Items", item, `{"Name":"التسليح"}`),
		line("Entries", entry, `{"Number":312,"Description":"خرسانة","Amount":4250.0000}`),
		line("Attachments", file, `{"FileName":"invoice.jpg"}`),
		line("Contracts", contract, `{"SupplierId":`+of(supplier)+`,"ItemId":`+of(item)+`,"Value":10000.0000}`),
	}, map[uuid.UUID]string{sara: "سارة"})
	money := func(text string) fixed.Number {
		number, _ := fixed.Parse(text)
		return number
	}
	for _, c := range []struct {
		line store.History
		want described
	}{
		{line("Accounts", account, `{}`), described{name: "الراجحي"}},
		{line("AccountCustodians", account, `{"UserId":`+of(sara)+`}`), described{name: "الراجحي", about: "سارة"}},
		{line("Transfers", uuid.New(), `{"FromAccountId":`+of(account)+`,"ToAccountId":`+of(other)+`,"Amount":5000.0000}`),
			described{from: "الراجحي", to: "الصندوق", amount: money("5000")}},
		{line("Reconciliations", uuid.New(), `{}`), described{}},
		{line("PartnerUsers", partner, `{"UserId":`+of(sara)+`}`), described{name: "زيد", about: "سارة"}},
		{line("Contracts", contract, `{}`), described{name: "مؤسسة البناء", about: "التسليح", amount: money("10000")}},
		{line("ContractAttachments", contract, `{"AttachmentId":`+of(file)+`}`), described{name: "مؤسسة البناء", about: "التسليح", amount: money("10000")}},
		{line("SupplierDefaultItems", supplier, `{"ItemId":`+of(item)+`}`), described{name: "مؤسسة البناء"}},
		{line("Subgroups", uuid.New(), `{"Name":"الأعمدة","ItemId":`+of(item)+`}`), described{name: "الأعمدة", about: "التسليح"}},
		{line("Entries", entry, `{}`), described{name: "312", about: "خرسانة", amount: money("4250")}},
		{line("ExpenseDetails", entry, `{"ItemId":`+of(item)+`}`), described{name: "312", about: "خرسانة", amount: money("4250")}},
		{line("Invoices", entry, `{"Number":"INV-9"}`), described{name: "INV-9", about: "خرسانة"}},
		{line("EntryAttachments", entry, `{"AttachmentId":`+of(file)+`}`), described{name: "invoice.jpg", about: "خرسانة"}},
		{line("PeriodicReports", uuid.New(), `{"Number":7}`), described{name: "7"}},
		{line("ProjectPermissions", uuid.New(), `{"UserId":`+of(sara)+`,"Permission":15}`), described{name: "approveEntries", about: "سارة"}},
		{line("InactiveMembers", uuid.New(), `{"UserId":`+of(sara)+`}`), described{name: "سارة"}},
		{line("ProjectRoleNames", uuid.New(), `{"UserId":`+of(sara)+`,"RoleName":"محاسب"}`), described{name: "محاسب", about: "سارة"}},
		{line("ChosenPartners", uuid.New(), `{"UserId":`+of(sara)+`,"PartnerId":`+of(partner)+`}`), described{name: "زيد", about: "سارة"}},
		{line("ProjectTimeZones", uuid.New(), `{"TimeZone":"Asia/Dubai"}`), described{name: "Asia/Dubai"}},
		{line("ProjectStructureSources", uuid.New(), `{"SourceProjectId":`+of(uuid.New())+`}`), described{}},
	} {
		var got described
		things[c.line.TableName].describe(&got, known.ownValues(c.line), known)
		if got != c.want {
			t.Errorf("%s: %+v, want %+v", c.line.TableName, got, c.want)
		}
	}
}

// UC-069 R2 (D4): a file kept shows nowhere on its own, and a part shows only when its owner wrote nothing in the same request; a line written
// before History kept the request's trace id belongs with the lines of its moment and its person (D3).
func TestR2_AFileNeverShowsAloneAndAPartOnlyWithoutItsOwner(t *testing.T) {
	sara, ali := uuid.New(), uuid.New()
	first, second := moment.Of(time.Date(2026, 11, 5, 23, 7, 21, 993673700, time.UTC)), moment.Of(time.Date(2026, 11, 5, 23, 7, 22, 205843400, time.UTC))
	rows := []store.History{
		{ID: 1, TableName: "Attachments", TraceID: "a"},
		{ID: 2, TableName: "Entries", TraceID: "b"},
		{ID: 3, TableName: "ExpenseDetails", TraceID: "b"},
		{ID: 4, TableName: "EntryAttachments", TraceID: "c"},
		{ID: 5, TableName: "Units", TraceID: ""},
		{ID: 6, TableName: "Users", TraceID: "d"},
		{ID: 7, TableName: "Entries", At: first, UserID: sara},
		{ID: 8, TableName: "ExpenseDetails", At: first, UserID: sara},
		{ID: 9, TableName: "ExpenseDetails", At: second, UserID: sara},
		{ID: 10, TableName: "ExpenseDetails", At: first, UserID: ali},
	}
	var kept []int64
	for _, row := range shown(rows) {
		kept = append(kept, row.ID)
	}
	if want := []int64{2, 4, 5, 7, 9, 10}; !slices.Equal(kept, want) {
		t.Errorf("the lines that stand for an action: %v, want %v", kept, want)
	}
}
