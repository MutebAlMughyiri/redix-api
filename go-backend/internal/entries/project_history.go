// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"bytes"
	"cmp"
	"encoding/json"
	"maps"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/spelling"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// «سجل المشروع» and «سجلي» (UC-069, redix-api #86): every action in a project, one line per request, each shown only to whoever may see its
// record; and every line of one's own login.

// What a line of the project history is filtered under (R5), then what a line of one's own history is.
var areas = web.Enum{"entries", "accounts", "partners", "suppliers", "structure", "project", "people", "signIn", "password", "biometrics", "details"}

// What a line of the project history says was done (D5).
var actions = web.Enum{"created", "changed", "deleted", "disabled", "enabled", "drafted", "sent", "approved", "rejected", "restored", "registered",
	"closed", "attached", "given", "taken", "issued", "archived", "retrieved"}

// What a line of the project history says it was done to (D5).
var thingNames = web.Enum{"entry", "invoice", "file", "account", "custody", "transfer", "reconciliation", "paymentMethod", "partner", "partnerLogin", "supplier",
	"contract", "contractDocument", "category", "item", "subgroup", "column", "defaultColumn", "project", "unit", "timeZone", "document",
	"periodicReport", "permission", "person", "roleName", "partnerSight", "permissionRequest", "tableSight"}

// What a line of one's own history says happened to one's login (R6).
var loginKinds = web.Enum{"signIn", "signOut", "signInFailed", "locked", "passwordChanged", "passwordReset", "biometricsOn", "biometricsOff",
	"created", "changed"}

// one is the value of enum named name.
func one(enum web.Enum, name string) named { return named{int64(slices.Index(enum, name)), enum} }

// actionOf is what a line of action says was done; a Create is said as its thing says it (an entry's is a draft, a report's is issued).
var actionOf = map[string]string{"Create": "created", "Update": "changed", "Delete": "deleted", "Disable": "disabled", "Enable": "enabled",
	"Submit": "sent", "Approve": "approved", "Reject": "rejected", "Restore": "restored", "Reverse": "approved", "RegisterProject": "registered",
	"ApproveProject": "approved", "RejectProject": "rejected", "CloseProject": "closed", "Attach": "attached", "AssignProject": "given",
	"RemoveProject": "taken", "Archive": "archived", "Retrieve": "retrieved"}

// thing is what a History table's record is in the project history: what a line calls it and the kind it is filtered under; the tables whose
// line stands for its line when the same request wrote one (R2); what its actions are said as where actionOf does not say them so; who may
// see its lines (R3); and how a line names it, from what History knows of it (R4).
type thing struct {
	name     string
	area     string
	owners   []string
	saidAs   map[string]string
	seen     func(sight, values) bool
	describe func(*described, values, knowledge)
}

// always are owners every request has: a file kept never shows alone, but through what it is attached to (D4).
var always = []string{""}

// things is every History table a project's lines are of; a table missing here shows nothing, and TestR2_EveryProjectTableIsAThing fails.
var things = map[string]thing{
	"Entries":                  {name: "entry", area: "entries", saidAs: map[string]string{"Create": "drafted"}, seen: sight.entry, describe: entry},
	"ExpenseDetails":           {name: "entry", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: entryOfPart},
	"PurchaseDetails":          {name: "entry", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: entryOfPart},
	"ContractorPaymentDetails": {name: "entry", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: entryOfPart},
	"RevenueDetails":           {name: "entry", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: entryOfPart},
	"CapitalMovementDetails":   {name: "entry", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: entryOfPart},
	"Invoices":                 {name: "invoice", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: invoice},
	"EntryAttachments":         {name: "file", area: "entries", owners: []string{"Entries"}, seen: sight.entry, describe: fileOfEntry},
	"Attachments":              {name: "file", area: "entries", owners: always, seen: sight.everyone, describe: nameIn("FileName")},
	"Accounts":                 {name: "account", area: "accounts", seen: sight.everyone, describe: nameIn("Name")},
	"AccountCustodians":        {name: "custody", area: "accounts", owners: []string{"Accounts"}, seen: sight.everyone, describe: custody},
	"Transfers":                {name: "transfer", area: "accounts", seen: sight.everyone, describe: transfer},
	"Reconciliations":          {name: "reconciliation", area: "accounts", seen: sight.everyone, describe: nothing},
	"PaymentMethods":           {name: "paymentMethod", area: "accounts", seen: sight.everyone, describe: nameIn("Name")},
	"Partners":                 {name: "partner", area: "partners", seen: sight.partner, describe: nameIn("Name")},
	"PartnerUsers":             {name: "partnerLogin", area: "partners", owners: []string{"Partners"}, seen: sight.partner, describe: partnerLogin},
	"Suppliers":                {name: "supplier", area: "suppliers", seen: sight.everyone, describe: nameIn("Name")},
	"SupplierDefaultItems": {name: "supplier", area: "suppliers", owners: []string{"Suppliers"},
		saidAs: map[string]string{"Create": "changed", "Update": "changed", "Delete": "changed"}, seen: sight.everyone, describe: supplierOfItem},
	"Contracts":               {name: "contract", area: "suppliers", seen: sight.everyone, describe: contract},
	"ContractAttachments":     {name: "contractDocument", area: "suppliers", owners: []string{"Contracts"}, seen: sight.everyone, describe: contractDocument},
	"Categories":              {name: "category", area: "structure", seen: sight.everyone, describe: nameIn("Name")},
	"Items":                   {name: "item", area: "structure", seen: sight.everyone, describe: nameIn("Name")},
	"Subgroups":               {name: "subgroup", area: "structure", owners: []string{"Items"}, seen: sight.everyone, describe: ofItem},
	"ItemColumns":             {name: "column", area: "structure", owners: []string{"Items", "DefaultColumns"}, seen: sight.everyone, describe: ofItem},
	"DefaultColumns":          {name: "defaultColumn", area: "structure", seen: sight.everyone, describe: nameIn("Name")},
	"Projects":                {name: "project", area: "project", seen: sight.everyone, describe: nameIn("Name")},
	"Units":                   {name: "unit", area: "project", seen: sight.everyone, describe: nameIn("Name")},
	"ProjectTimeZones":        {name: "timeZone", area: "project", seen: sight.everyone, describe: nameIn("TimeZone")},
	"ProjectStructureSources": {name: "project", area: "project", owners: []string{"Projects"}, seen: sight.everyone, describe: nothing},
	"Documents":               {name: "document", area: "project", seen: sight.everyone, describe: nameIn("Name")},
	"PeriodicReports": {name: "periodicReport", area: "project", saidAs: map[string]string{"Create": "issued"}, seen: sight.reports,
		describe: periodicReport},
	"ProjectPermissions": {name: "permission", area: "people", owners: []string{"Projects"}, seen: sight.managingUsers, describe: permission},
	"InactiveMembers":    {name: "person", area: "people", owners: []string{"ProjectPermissions"}, seen: sight.managingUsers, describe: person},
	"ProjectRoleNames":   {name: "roleName", area: "people", owners: []string{"ProjectPermissions"}, seen: sight.managingUsers, describe: roleName},
	"ChosenPartners":     {name: "partnerSight", area: "people", owners: []string{"ProjectPermissions"}, seen: sight.managingUsers, describe: partnerSight},
	"UnseenGroups":       {name: "tableSight", area: "people", owners: []string{"ProjectPermissions"}, seen: sight.managingUsers, describe: tableSight},
	"UnseenContracts":    {name: "tableSight", area: "people", owners: []string{"ProjectPermissions"}, seen: sight.managingUsers, describe: tableSight},
	"PermissionRequests": {name: "permissionRequest", area: "people", saidAs: map[string]string{"Create": "sent"}, seen: sight.managingUsers,
		describe: person},
}

// values is everything History knows of one record: each value its lines kept, the newest winning, so a deleted record keeps its name (R4).
type values map[string]json.RawMessage

func (v values) text(name string) string {
	var text string
	json.Unmarshal(v[name], &text)
	return text
}

func (v values) id(name string) uuid.UUID {
	id, _ := uuid.Parse(v.text(name))
	return id
}

func (v values) number(name string) fixed.Number {
	number, _ := fixed.Parse(string(bytes.Trim(v[name], `"`)))
	return number
}

func (v values) whole(name string) int64 {
	var whole float64
	json.Unmarshal(v[name], &whole)
	return int64(whole)
}

// record names one record as History does: its table and its id.
type record struct {
	table string
	id    uuid.UUID
}

// knowledge is what the lines of a project say of every record, and the names of everyone.
type knowledge struct {
	records map[record]values
	people  map[uuid.UUID]string
}

func (k knowledge) of(table string, id uuid.UUID) values { return k.records[record{table, id}] }

func (k knowledge) nameOf(table string, id uuid.UUID) string { return k.of(table, id).text("Name") }

// knowledgeOf is what rows, oldest first, say of each record: every value a line kept before or after it, the newest winning.
func knowledgeOf(rows []store.History, people map[uuid.UUID]string) knowledge {
	known := knowledge{records: map[record]values{}, people: people}
	for _, row := range rows {
		key := record{row.TableName, row.RecordID}
		if known.records[key] == nil {
			known.records[key] = values{}
		}
		known.records[key].add(row)
	}
	return known
}

// add takes in the values row kept before and after its change, these winning.
func (v values) add(row store.History) {
	for _, kept := range []string{row.Before, row.After} {
		maps.Copy(v, valuesIn(kept))
	}
}

// ownValues are what History knows of row's record, with row's own values over them: a line says what its own change did, and knows the id of
// the record it names.
func (k knowledge) ownValues(row store.History) values {
	own := values{"Id": json.RawMessage(strconv.Quote(row.RecordID.String()))}
	maps.Copy(own, k.of(row.TableName, row.RecordID))
	own.add(row)
	return own
}

// valuesIn are the values a line kept, before or after its change.
func valuesIn(kept string) values {
	var some values
	json.Unmarshal([]byte(kept), &some)
	return some
}

// described is what a line says of its record (D5): its name, what else it is about, its amount (0 for none), the accounts money went between,
// and the name it had before a rename.
type described struct {
	name, about, nameWas, from, to string
	amount                         fixed.Number
}

// nameIn names a record by its own field.
func nameIn(field string) func(*described, values, knowledge) {
	return func(d *described, record values, _ knowledge) { d.name = record.text(field) }
}

func nothing(*described, values, knowledge) {}

func entry(d *described, record values, _ knowledge) {
	if number := record.whole("Number"); number > 0 {
		d.name = strconv.FormatInt(number, 10)
	}
	d.about, d.amount = record.text("Description"), record.number("Amount")
}

// entryOfPart names the entry a part belongs to: a part's line names its entry's id as its own record.
func entryOfPart(d *described, record values, known knowledge) {
	entry(d, known.of("Entries", record.id("Id")), known)
}

// invoice names an invoice number, and the entry it is of, which its line names as its own record.
func invoice(d *described, record values, known knowledge) {
	entryOfPart(d, record, known)
	d.name, d.amount = record.text("Number"), 0
}

// supplierOfItem names the supplier a default item is of, which its line names as its own record.
func supplierOfItem(d *described, record values, known knowledge) {
	d.name = known.nameOf("Suppliers", record.id("Id"))
}

func fileOfEntry(d *described, record values, known knowledge) {
	entryOfPart(d, record, known)
	d.name, d.amount = known.of("Attachments", record.id("AttachmentId")).text("FileName"), 0
}

// custody names the account a custodian line is of, which it names as its own record, and the custodian.
func custody(d *described, record values, known knowledge) {
	d.name, d.about = known.nameOf("Accounts", record.id("Id")), known.people[record.id("UserId")]
}

func transfer(d *described, record values, known knowledge) {
	d.amount = record.number("Amount")
	d.from, d.to = known.nameOf("Accounts", record.id("FromAccountId")), known.nameOf("Accounts", record.id("ToAccountId"))
}

// partnerLogin names the partner a login line is of, which it names as its own record, and the login.
func partnerLogin(d *described, record values, known knowledge) {
	d.name, d.about = known.nameOf("Partners", record.id("Id")), known.people[record.id("UserId")]
}

func contract(d *described, record values, known knowledge) {
	d.name, d.about = known.nameOf("Suppliers", record.id("SupplierId")), known.nameOf("Items", record.id("ItemId"))
	d.amount = record.number("Value")
}

// contractDocument names the contract a document line is of, which it names as its own record.
func contractDocument(d *described, record values, known knowledge) {
	contract(d, known.of("Contracts", record.id("Id")), known)
}

func ofItem(d *described, record values, known knowledge) {
	d.name, d.about = record.text("Name"), known.nameOf("Items", record.id("ItemId"))
}

func periodicReport(d *described, record values, _ knowledge) {
	d.name = strconv.FormatInt(record.whole("Number"), 10)
}

func permission(d *described, record values, known knowledge) {
	if number := record.whole("Permission"); number >= 0 && number < int64(len(access.All)) {
		d.name = web.Camel(string(access.All[number]))
	}
	d.about = known.people[record.id("UserId")]
}

func person(d *described, record values, known knowledge) { d.name = known.people[record.id("UserId")] }

func roleName(d *described, record values, known knowledge) {
	d.name, d.about = record.text("RoleName"), known.people[record.id("UserId")]
}

func partnerSight(d *described, record values, known knowledge) {
	d.name, d.about = known.nameOf("Partners", record.id("PartnerId")), known.people[record.id("UserId")]
}

// tableSight names the table a sight line is of, a group by its name and a contract by its contractor's, and the person (#106).
func tableSight(d *described, record values, known knowledge) {
	table := record.id("TableId")
	d.name = cmp.Or(known.nameOf("Subgroups", table), known.nameOf("Suppliers", known.of("Contracts", table).id("SupplierId")))
	d.about = known.people[record.id("UserId")]
}

// sight is what the caller may see in the project (R3): the entries the entries screens show them, and their permissions.
type sight struct {
	caller    access.Caller
	projectID uuid.UUID
	entries   map[uuid.UUID]bool
}

func (s sight) everyone(values) bool { return true }

// entry: an entry the entries screens show the caller (seenBy), whose id an entry's line and its parts' name as their record; one deleted
// since, only to whoever recorded it, since a deleted entry is a draft or was never approved, and nothing else says whose capital it moved.
func (s sight) entry(record values) bool {
	if seen, kept := s.entries[record.id("Id")]; kept {
		return seen
	}
	return record.id("CreatedByUserId") == s.caller.UserID
}

// partner: a partner's lines, and its login's, which name the partner as their own record, for whoever sees its capital (BR-31).
func (s sight) partner(record values) bool {
	return s.caller.Access.SeesCapitalOf(s.projectID, record.id("Id"))
}

func (s sight) managingUsers(values) bool {
	return s.caller.Access.Holds(s.projectID, access.ManageUsers)
}

func (s sight) reports(values) bool { return s.caller.Access.Holds(s.projectID, access.SeeReports) }

// requestOf is the request that wrote row: its trace id; for a line written before History kept one (D3), the moment and the person, which one
// request's lines share and the next request's never do, a moment being kept to the ten-millionth of a second.
func requestOf(row store.History) string {
	if row.TraceID == "" {
		return row.At.String() + " " + row.UserID.String()
	}
	return row.TraceID
}

// shown keeps of rows the lines that stand for an action (R2): a line goes when the request that wrote it also wrote a line of one of its
// owners, which stands for it.
func shown(rows []store.History) []store.History {
	wrote := map[string]map[string]bool{}
	for _, row := range rows {
		request := requestOf(row)
		if wrote[request] == nil {
			wrote[request] = map[string]bool{"": true}
		}
		wrote[request][row.TableName] = true
	}
	var kept []store.History
	for _, row := range rows {
		kind, isThing := things[row.TableName]
		if isThing && !slices.ContainsFunc(kind.owners, func(owner string) bool { return wrote[requestOf(row)][owner] }) {
			kept = append(kept, row)
		}
	}
	return kept
}

type projectLine struct {
	ID      int64         `json:"id"`
	At      moment.Moment `json:"at"`
	By      string        `json:"by"`
	Area    named         `json:"area"`
	Action  named         `json:"action"`
	Thing   named         `json:"thing"`
	Name    string        `json:"name"`
	About   string        `json:"about"`
	NameWas string        `json:"nameWas"`
	From    string        `json:"from"`
	To      string        `json:"to"`
	Amount  fixed.Number  `json:"amount"`
}

func (l projectLine) holds(text string) bool {
	return spelling.Holds(text, l.Amount, l.By, l.Name, l.About, l.NameWas, l.From, l.To)
}

// projectHistory is a page of «سجل المشروع» (UC-069 R1-R5): every action in the project, one line per request, newest first, each only for
// someone who may see its record; area keeps one kind of record, any other text every kind.
func (s Routes) projectHistory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	area, areaErr := areas.Parse(r.URL.Query().Get("area"))
	seen, known, err := s.seenBy(c, caller)
	all, allErr := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	newest, rowsErr := c.Q.HistoryOfProject(c.Ctx, c.ProjectID)
	decisions, decisionsErr := c.Q.DecisionsOf(c.Ctx, c.ProjectID)
	if err != nil || allErr != nil || rowsErr != nil || decisionsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	oldest := slices.Clone(newest)
	slices.Reverse(oldest)
	records := knowledgeOf(oldest, known.authors)
	sees := sight{caller: caller, projectID: c.ProjectID, entries: map[uuid.UUID]bool{}}
	for _, entry := range all {
		sees.entries[entry.ID] = false
	}
	for _, entry := range seen {
		sees.entries[entry.ID] = true
	}
	var kept []store.History
	for _, row := range shown(newest) {
		kind := things[row.TableName]
		if kind.seen(sees, records.ownValues(row)) && (areaErr != nil || kind.area == areas[area]) {
			kept = append(kept, row)
		}
	}
	lines := page(r, kept, func(row store.History) projectLine {
		kind := things[row.TableName]
		action := actionOf[row.Action]
		if said, saidSo := kind.saidAs[row.Action]; saidSo {
			action = said
		}
		var said described
		kind.describe(&said, records.ownValues(row), records)
		if row.Action == "Update" {
			said.nameWas = valuesIn(row.Before).text("Name")
		}
		if row.Action == "Reject" {
			said.about = reasonOf(decisions, row.RecordID, row.At)
		}
		if row.Action == "Retrieve" {
			said.about = valuesIn(row.After).text("ArchivedAt")
		}
		return projectLine{ID: row.ID, At: row.At, By: known.authors[row.UserID], Area: one(areas, kind.area), Action: one(actions, action),
			Thing: one(thingNames, kind.name), Name: said.name, About: said.about, NameWas: said.nameWas, From: said.from, To: said.to, Amount: said.amount}
	})
	web.JSON(w, r, http.StatusOK, lines)
}

// reasonOf is the reason entryID was rejected with at: the decision of that moment.
func reasonOf(decisions []store.EntryDecision, entryID uuid.UUID, at moment.Moment) string {
	for _, decision := range decisions {
		if decision.EntryID == entryID && decision.Action == rejectedBy && decision.At.Equal(at.Time) {
			return decision.Reason
		}
	}
	return ""
}

// loginKind is what a line of a login says happened to it, and the kind of one's history it is filtered under.
type loginKind struct{ kind, area string }

var loginKindOf = map[string]loginKind{"SignIn": {"signIn", "signIn"}, "SignOut": {"signOut", "signIn"}, "SignInFailed": {"signInFailed", "signIn"},
	"AccountLocked": {"locked", "signIn"}, "ChangePassword": {"passwordChanged", "password"}, "ResetPassword": {"passwordReset", "password"},
	"TurnOnBiometrics": {"biometricsOn", "biometrics"}, "TurnOffBiometrics": {"biometricsOff", "biometrics"}, "Create": {"created", "details"},
	"Update": {"changed", "details"}}

type loginLine struct {
	ID    int64         `json:"id"`
	At    moment.Moment `json:"at"`
	By    string        `json:"by"`
	Area  named         `json:"area"`
	Kind  named         `json:"kind"`
	About string        `json:"about"`
}

func (l loginLine) holds(text string) bool { return spelling.Holds(text, 0, l.By, l.About) }

// myHistory is a page of «سجلي» (UC-069 R6): every line of the caller's own login, whoever wrote it, newest first; what a change set shows
// only as the name, username, mobile and email it gave, never a hash, a code or a token. area keeps one kind, any other text every kind.
func (s Routes) myHistory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	q := s.database.Queries()
	area, areaErr := areas.Parse(r.URL.Query().Get("area"))
	rows, err := q.HistoryOfLogin(r.Context(), caller.UserID)
	people, peopleErr := q.UserNames(r.Context())
	if err != nil || peopleErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	names := map[uuid.UUID]string{}
	for _, person := range people {
		names[person.ID] = person.DisplayName
	}
	var kept []store.History
	for _, row := range rows {
		if kind, known := loginKindOf[row.Action]; known && (areaErr != nil || kind.area == areas[area]) {
			kept = append(kept, row)
		}
	}
	lines := page(r, kept, func(row store.History) loginLine {
		kind, gave := loginKindOf[row.Action], valuesIn(row.After)
		var about []string
		for _, field := range []string{"DisplayName", "Username", "Mobile", "Email"} {
			if text := gave.text(field); text != "" && row.Action == "Update" {
				about = append(about, text)
			}
		}
		return loginLine{ID: row.ID, At: row.At, By: names[row.UserID], Area: one(areas, kind.area), Kind: one(loginKinds, kind.kind),
			About: strings.Join(about, " · ")}
	})
	web.JSON(w, r, http.StatusOK, lines)
}
