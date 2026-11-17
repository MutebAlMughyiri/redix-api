package settings_test

import (
	"encoding/json"
	"slices"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

type api struct {
	*testapi.API
	t       *testing.T
	token   string
	project string
}

// approved is an API with one approved project, its structure and payment methods copied from the template, and its manager's token.
func approved(t *testing.T) api {
	a := api{API: testapi.New(t), t: t}
	manager, admin := uuid.Must(uuid.NewV7()), uuid.Must(uuid.NewV7())
	for _, id := range []uuid.UUID{manager, admin} {
		a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
			VALUES (?, ?, 'x', '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, id.String(), credentials.Hash("Patrol-chosen-2"))
	}
	a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), admin)
	a.token = a.Token(manager, false)
	code, answer := a.Ask("POST", "/projects", `{"name":"غصون","startDate":"2026-11-01","units":[]}`, a.token)
	if code != 201 {
		t.Fatalf("start: %d %v", code, answer)
	}
	a.project = "/projects/" + testapi.Field(answer, "id").(string)
	if code, _ := a.Ask("POST", a.project+"/approve", ``, a.Token(admin, false)); code != 200 {
		t.Fatalf("approve: %d", code)
	}
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

func (a api) account(name string) string {
	a.t.Helper()
	a.ask("POST", "/accounts", `{"name":"`+name+`","type":"bank"}`, 200)
	var id string
	if err := a.Database.Read.QueryRow("SELECT id FROM accounts WHERE name = ?", name).Scan(&id); err != nil {
		a.t.Fatal(err)
	}
	return id
}

// methods is the project's payment methods, each with the accounts it pays from.
func (a api) methods() map[string][]string {
	a.t.Helper()
	answer := a.ask("GET", "/payment-methods", ``, 200)
	methods := map[string][]string{}
	for _, method := range answer.([]any) {
		ids := []string{}
		for _, id := range testapi.Field(method, "accountIds").([]any) {
			ids = append(ids, id.(string))
		}
		slices.Sort(ids)
		methods[testapi.Field(method, "id").(string)] = ids
	}
	return methods
}

// added adds the payment method sent, as its dialog does, and is its id.
func (a api) added(sent, name string) string {
	a.t.Helper()
	a.ask("POST", "/payment-methods", sent, 200)
	for _, method := range a.ask("GET", "/payment-methods", ``, 200).([]any) {
		if testapi.Field(method, "name") == name {
			return testapi.Field(method, "id").(string)
		}
	}
	a.t.Fatalf("no method named %s", name)
	return ""
}

func sorted(ids ...string) []string {
	slices.Sort(ids)
	return ids
}

func body(name string, accountIDs ...string) string {
	sent, _ := json.Marshal(map[string]any{"name": name, "accountIds": accountIDs})
	return string(sent)
}

// R3 (Muteb, 12 Nov 2026): a payment method pays from the accounts its dialog ticks, and a change to them is the method's History line.
func TestR3_AMethodPaysFromTheAccountsItsDialogTicks(t *testing.T) {
	a := approved(t)
	bank, custody := a.account("حساب المؤسسة"), a.account("عهدة متعب")
	method := a.added(body("نقداً", custody), "نقداً")
	if got := a.methods()[method]; !slices.Equal(got, []string{custody}) {
		t.Fatalf("the new method pays from %v, not the custody alone", got)
	}
	a.ask("PUT", "/payment-methods/"+method, body("نقداً", custody, bank), 200)
	if got := a.methods()[method]; !slices.Equal(got, sorted(custody, bank)) {
		t.Fatalf("after ticking the bank it pays from %v", got)
	}
	if lines := a.Count("SELECT count(*) FROM history WHERE table_name = 'PaymentMethods' AND action = 'Update' AND record_id = ?", method); lines != 1 {
		t.Fatalf("%d History lines for what the method pays from, want 1", lines)
	}
	a.ask("PUT", "/payment-methods/"+method, `{"name":"نقدي"}`, 200)
	if got := a.methods()[method]; !slices.Equal(got, sorted(custody, bank)) {
		t.Fatalf("a rename that names no accounts changed them to %v", got)
	}
}

// R4 and D1b (Muteb, 17 Nov 2026): a method naming no accounts pays from every one, and an account added later is paid from by every method of its
// project, the ones copied from the template among them.
func TestR4_D1b_EveryMethodPaysFromEveryAccountUntilOneIsUnticked(t *testing.T) {
	a := approved(t)
	first := a.account("حساب المؤسسة")
	method := a.added(`{"name":"تحويل بنكي"}`, "تحويل بنكي")
	if got := a.methods()[method]; !slices.Equal(got, []string{first}) {
		t.Fatalf("a method naming no accounts pays from %v, not every account", got)
	}
	second := a.account("حساب المشروع")
	for id, accounts := range a.methods() {
		if !slices.Equal(accounts, sorted(first, second)) {
			t.Errorf("method %s pays from %v, not both accounts", id, accounts)
		}
	}
	a.ask("DELETE", "/accounts/"+second, ``, 200)
	if got := a.methods()[method]; !slices.Equal(got, []string{first}) {
		t.Fatalf("a deleted account is still paid from: %v", got)
	}
	a.ask("DELETE", "/payment-methods/"+method, ``, 200)
	if _, kept := a.methods()[method]; kept {
		t.Fatal("the deleted method is still listed")
	}
}

// R10: a method pays from at least one account while the project holds any, and only from the project's own; one with no account yet takes a
// method naming none, which the first account added is then ticked on (D1b).
func TestR10_AMethodPaysFromAtLeastOneOfTheProjectsAccounts(t *testing.T) {
	a := approved(t)
	early := a.added(body("مدى"), "مدى")
	if got := a.methods()[early]; len(got) != 0 {
		t.Fatalf("a method of a project with no account pays from %v", got)
	}
	first := a.account("حساب المؤسسة")
	if got := a.methods()[early]; !slices.Equal(got, []string{first}) {
		t.Fatalf("the first account added is not paid from by the early method: %v", got)
	}
	if refused := a.ask("POST", "/payment-methods", body("نقداً"), 400); testapi.Field(refused, "code") != "paysFromNothing" {
		t.Fatalf("an empty list was answered %v", refused)
	}
	if refused := a.ask("POST", "/payment-methods", body("نقداً", uuid.NewString()), 400); testapi.Field(refused, "code") != "unknownAccount" {
		t.Fatalf("an account of no project was answered %v", refused)
	}
}

// R5: the API refuses an expense whose account its payment method does not pay from, and takes it once the account is ticked.
func TestR5_AnExpenseGoesOnlyThroughAnAccountItsMethodPaysFrom(t *testing.T) {
	a := approved(t)
	bank, custody := a.account("حساب المؤسسة"), a.account("عهدة متعب")
	category := testapi.Field(a.ask("POST", "/categories", `{"name":"الحديد"}`, 201), "id").(string)
	item := testapi.Field(a.ask("POST", "/categories/"+category+"/items", `{"name":"التسليح","type":"purchases"}`, 201), "id").(string)
	var table string
	if err := a.Database.Read.QueryRow("SELECT id FROM subgroups WHERE item_id = ?", item).Scan(&table); err != nil {
		t.Fatal(err)
	}
	method := a.added(body("نقداً", custody), "نقداً")
	expense := func() string {
		sent, _ := json.Marshal(map[string]any{"id": uuid.Must(uuid.NewV7()), "itemId": item, "tableId": table, "date": "2026-11-16", "amount": 100,
			"accountId": bank, "paymentMethodId": method, "description": "", "columnValues": map[string]string{}})
		return string(sent)
	}
	if refused := a.ask("POST", "/entries", expense(), 400); testapi.Field(refused, "code") != "accountNotForMethod" {
		t.Fatalf("paying cash from the bank was answered %v", refused)
	}
	a.ask("PUT", "/payment-methods/"+method, body("نقداً", custody, bank), 200)
	a.ask("POST", "/entries", expense(), 201)
}
