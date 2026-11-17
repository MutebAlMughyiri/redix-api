package entries_test

import (
	"encoding/json"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

// project is an approved project, its manager signed in at the moment the test stands at, who may record an expense on an item of its own, through a bank account and a payment method.
type project struct {
	*testapi.API
	t                            *testing.T
	token, path                  string
	item, table, account, method string
}

func approved(t *testing.T, at time.Time) project {
	a := project{API: testapi.New(t), t: t}
	a.Clock.Set(at)
	manager, admin := uuid.Must(uuid.NewV7()), uuid.Must(uuid.NewV7())
	for _, id := range []uuid.UUID{manager, admin} {
		a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
			VALUES (?, ?, 'x', '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, id.String(), credentials.Hash("Patrol-chosen-2"))
	}
	a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), admin)
	a.token = a.Token(manager, false)
	_, answer := a.Ask("POST", "/projects", `{"name":"غصون","startDate":"2026-11-01","units":[]}`, a.token)
	a.path = "/projects/" + testapi.Field(answer, "id").(string)
	a.Ask("POST", a.path+"/approve", ``, a.Token(admin, false))
	a.Ask("POST", a.path+"/accounts", `{"name":"حساب المؤسسة","type":"bank"}`, a.token)
	_, category := a.Ask("POST", a.path+"/categories", `{"name":"الحديد"}`, a.token)
	_, item := a.Ask("POST", a.path+"/categories/"+testapi.Field(category, "id").(string)+"/items", `{"name":"التسليح","type":"purchases"}`, a.token)
	a.item = testapi.Field(item, "id").(string)
	read := a.Database.Read
	if err := read.QueryRow("SELECT id FROM subgroups WHERE item_id = ?", a.item).Scan(&a.table); err != nil {
		t.Fatal(err)
	}
	if err := read.QueryRow("SELECT id FROM accounts").Scan(&a.account); err != nil {
		t.Fatal(err)
	}
	if err := read.QueryRow("SELECT payment_methods.id FROM payment_methods JOIN projects ON projects.id = payment_methods.project_id WHERE projects.is_template = 0").Scan(&a.method); err != nil {
		t.Fatal(err)
	}
	return a
}

// dated records an expense dated date, and is the status and code the API answered.
func (a project) dated(date string) (int, any) {
	sent, _ := json.Marshal(map[string]any{"id": uuid.Must(uuid.NewV7()), "itemId": a.item, "tableId": a.table, "date": date, "amount": 100,
		"accountId": a.account, "paymentMethodId": a.method, "description": "", "columnValues": map[string]string{}})
	code, answer := a.Ask("POST", a.path+"/entries", string(sent), a.token)
	return code, testapi.Field(answer, "code")
}

// BR-12 in the project's zone (redix-app #107): at 17:00 UTC on 16 Nov 2026 it is already the 17th in Shanghai, so a project there takes an entry
// dated the 17th, which a project in Riyadh, at 20:00 on the 16th, refuses as a date after its today.
func TestBR12_AnEntrysTodayIsItsProjectsTodayInItsZone(t *testing.T) {
	at := time.Date(2026, 11, 16, 17, 0, 0, 0, time.UTC)
	riyadh := approved(t, at)
	if code, refused := riyadh.dated("2026-11-17"); code != 400 || refused != "invalidDate" {
		t.Errorf("tomorrow in Riyadh: %d %v", code, refused)
	}
	if code, _ := riyadh.dated("2026-11-16"); code != 201 {
		t.Errorf("today in Riyadh: %d", code)
	}
	shanghai := approved(t, at)
	shanghai.Ask("PUT", shanghai.path+"/time-zone", `{"timeZone":"Asia/Shanghai"}`, shanghai.token)
	if code, refused := shanghai.dated("2026-11-17"); code != 201 {
		t.Errorf("today in Shanghai: %d %v", code, refused)
	}
	if code, refused := shanghai.dated("2026-11-18"); code != 400 || refused != "invalidDate" {
		t.Errorf("tomorrow in Shanghai: %d %v", code, refused)
	}
}
