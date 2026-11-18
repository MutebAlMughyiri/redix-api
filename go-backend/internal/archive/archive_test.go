// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package archive_test

import (
	"bytes"
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"mime/multipart"
	"os"
	"slices"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/archive"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
	"github.com/google/uuid"
)

// project is an approved project holding the sample project file's books and one document, its manager signed in.
type project struct {
	*testapi.API
	t                *testing.T
	token, admin, id string
}

func (a *project) path() string { return "/projects/" + a.id }

// person is a user called name, in no project yet, and a token of theirs.
func (a *project) person(name string) (uuid.UUID, string) {
	id := uuid.Must(uuid.NewV7())
	a.Exec(`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
		VALUES (?, ?, ?, '', ?, 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`, id, id.String(), name, credentials.Hash("Patrol-chosen-2"))
	return id, a.Token(id, false)
}

// started is a new approved project called name, by the same manager, holding the account the sample file names.
func (a *project) started(name string) string {
	a.t.Helper()
	_, answer := a.Ask("POST", "/projects", `{"name":"`+name+`","startDate":"2026-11-01","units":[]}`, a.token)
	id := testapi.Field(answer, "id").(string)
	a.Ask("POST", "/projects/"+id+"/approve", ``, a.admin)
	a.Ask("POST", "/projects/"+id+"/accounts", `{"name":"حساب المؤسسة","type":"bank"}`, a.token)
	return id
}

// loaded is a project holding the sample file's categories, tables, entries, balances and report, loaded through UC-071, and one document.
func loaded(t *testing.T) *project {
	a := &project{API: testapi.New(t), t: t}
	a.Clock.Set(time.Date(2026, 11, 18, 10, 0, 0, 0, time.UTC))
	_, a.token = a.person("محمد")
	adminID, admin := a.person("admin")
	a.Exec(`INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, 'ApproveProjects')`, uuid.NewString(), adminID)
	a.admin = admin
	a.id = a.started("مشروع الاختبار")
	sample, err := os.ReadFile("testdata/project.json")
	if err != nil {
		t.Fatal(err)
	}
	if code, answer := a.sent(a.path()+"/project-file", "project.json", sample, a.token, nil); code != 200 {
		t.Fatalf("loading the sample: %d %v", code, answer)
	}
	a.keep("العقد")
	return a
}

// keep adds a document called name to the project.
func (a *project) keep(name string) {
	a.t.Helper()
	if code, answer := a.sent(a.path()+"/documents", name+".pdf", []byte("%PDF-1.4 "+name), a.token, map[string]string{"name": name}); code >= 300 {
		a.t.Fatalf("keeping %s: %d %v", name, code, answer)
	}
}

// formOf is a form holding content as its file called name, and fields beside it, as the app sends a file, and its content type.
func formOf(name string, content []byte, fields map[string]string) (string, string) {
	var body bytes.Buffer
	form := multipart.NewWriter(&body)
	for field, value := range fields {
		form.WriteField(field, value)
	}
	part, _ := form.CreateFormFile("file", name)
	part.Write(content)
	form.Close()
	return body.String(), form.FormDataContentType()
}

// sent asks path with content as the form's file called name and fields beside it, as the app sends a file: the status and the answer.
func (a *project) sent(path, name string, content []byte, token string, fields map[string]string) (int, map[string]any) {
	a.t.Helper()
	body, contentType := formOf(name, content, fields)
	answered := a.Send("POST", path, body, "Content-Type", contentType, "Authorization", "Bearer "+token)
	var answer map[string]any
	json.Unmarshal(answered.Body.Bytes(), &answer)
	return answered.Code, answer
}

// archived is the archive of the project id names, as token's holder takes it.
func (a *project) archived(id, token string) (int, []byte) {
	a.t.Helper()
	answered := a.Send("GET", "/projects/"+id+"/archive/json", ``, "Authorization", "Bearer "+token)
	return answered.Code, answered.Body.Bytes()
}

// taken is the project's archive, taken by its manager.
func (a *project) taken() []byte {
	a.t.Helper()
	code, archived := a.archived(a.id, a.token)
	if code != 200 {
		a.t.Fatalf("archive: %d %s", code, archived)
	}
	return archived
}

// retrieved sends archived to the project's retrieve, a preview when preview, as token's holder.
func (a *project) retrieved(archived []byte, preview bool, token string) (int, map[string]any) {
	a.t.Helper()
	path := a.path() + "/retrieve"
	if preview {
		path += "?preview=true"
	}
	return a.sent(path, "archive.json", archived, token, nil)
}

// lines is how many History lines the project has.
func (a *project) lines() int {
	return a.Count("SELECT count(*) FROM history WHERE project_id = ?", a.id)
}

// textOf is a row as the text of its values by column, one string a row's equal has too.
func textOf(columns []string, values []any) string {
	text := map[string]string{}
	for i, column := range columns {
		text[column] = fmt.Sprint(values[i])
	}
	written, _ := json.Marshal(text)
	return string(written)
}

// rowsNow is every row the project holds of each table a retrieve replaces, each as its text, sorted.
func (a *project) rowsNow() map[string][]string {
	a.t.Helper()
	kept := map[string][]string{}
	for _, t := range archive.Retrieved() {
		found, err := a.Database.Read.Query("SELECT * FROM "+t[0]+" WHERE "+t[1], sql.Named("project", a.id))
		if err != nil {
			a.t.Fatal(err)
		}
		columns, _ := found.Columns()
		kept[t[0]] = []string{}
		for found.Next() {
			values := make([]any, len(columns))
			pointers := make([]any, len(columns))
			for i := range values {
				pointers[i] = &values[i]
			}
			found.Scan(pointers...)
			kept[t[0]] = append(kept[t[0]], textOf(columns, values))
		}
		found.Close()
		slices.Sort(kept[t[0]])
	}
	return kept
}

// rowsIn is the same of an archive's rows.
func rowsIn(described map[string]any) map[string][]string {
	tables := described["tables"].(map[string]any)
	kept := map[string][]string{}
	for _, t := range archive.Retrieved() {
		kept[t[0]] = []string{}
		for _, listed := range tables[t[0]].([]any) {
			var columns []string
			var values []any
			for column, value := range listed.(map[string]any) {
				columns, values = append(columns, column), append(values, value)
			}
			kept[t[0]] = append(kept[t[0]], textOf(columns, values))
		}
		slices.Sort(kept[t[0]])
	}
	return kept
}

// describedIn is the archive, its numbers kept as written.
func describedIn(t *testing.T, archived []byte) map[string]any {
	t.Helper()
	decoder := json.NewDecoder(bytes.NewReader(archived))
	decoder.UseNumber()
	var described map[string]any
	if err := decoder.Decode(&described); err != nil {
		t.Fatal(err)
	}
	return described
}

// redescribed is the archive as change makes it.
func redescribed(t *testing.T, archived []byte, change func(map[string]any)) []byte {
	t.Helper()
	described := describedIn(t, archived)
	change(described)
	written, _ := json.Marshal(described)
	return written
}

// rowsOfTable is the archive's rows of table.
func rowsOfTable(described map[string]any, table string) []any {
	return described["tables"].(map[string]any)[table].([]any)
}

// UC-073 AC-73.7: every table of the database is the project's own, kept, or shared with its reason, so none is left out of an archive unseen.
func TestAC73_7_EveryTableOfTheDatabaseIsTheProjectsKeptOrShared(t *testing.T) {
	a := testapi.New(t)
	found, err := a.Database.Read.Query("SELECT name FROM sqlite_master WHERE type = 'table'")
	if err != nil {
		t.Fatal(err)
	}
	defer found.Close()
	seen := 0
	for found.Next() {
		var name string
		found.Scan(&name)
		seen++
		if !strings.HasPrefix(name, "lk_") && !archive.Placed(name) {
			t.Errorf("%s is neither the project's own, kept, nor shared", name)
		}
	}
	if seen < 60 {
		t.Errorf("read %d tables", seen)
	}
}

// UC-073 AC-73.1, R15: the archive is one JSON file holding every row of the project's tables as stored; a bill or a PDF is its attachment's
// row, its file staying in the storage; taking it is a History line in the taker's name.
func TestAC73_1_AnArchiveHoldsEveryRowOfTheProjectAndNamesItsFilesAndWhoTookIt(t *testing.T) {
	a := loaded(t)
	answered := a.Send("GET", a.path()+"/archive/json", ``, "Authorization", "Bearer "+a.token)
	if answered.Code != 200 || answered.Header().Get("Content-Type") != "application/json" ||
		!strings.Contains(answered.Header().Get("Content-Disposition"), ".json") {
		t.Fatalf("archive: %d %v", answered.Code, answered.Header())
	}
	archived := answered.Body.Bytes()
	described := describedIn(t, archived)
	project, by := described["project"].(map[string]any), described["archivedBy"].(map[string]any)
	release, _ := a.Database.Release(context.Background())
	if described["format"] != "redix.archive" || described["version"] != json.Number("1") || described["release"] != json.Number(fmt.Sprint(release)) ||
		project["id"] != a.id || project["name"] != "مشروع الاختبار" || by["name"] != "محمد" || described["archivedAt"] != "2026-11-18T10:00:00Z" {
		t.Errorf("the archive: %v", described)
	}
	if tables := described["tables"].(map[string]any); len(tables) != archive.Archived() || len(tables) < 44 {
		t.Errorf("%d tables", len(tables))
	}
	if !slices.Equal(rowsIn(described)["entries"], a.rowsNow()["entries"]) || len(rowsIn(described)["entries"]) == 0 {
		t.Errorf("entries: %d in the archive", len(rowsIn(described)["entries"]))
	}
	if rows := rowsOfTable(described, "history"); len(rows) != a.lines()-1 {
		t.Errorf("History: %d lines archived of %d", len(rows), a.lines())
	}
	attachment := rowsOfTable(described, "attachments")[0].(map[string]any)
	if attachment["file_name"] != "العقد.pdf" || !strings.HasPrefix(attachment["storage_path"].(string), a.id+"/") || bytes.Contains(archived, []byte("%PDF-1.4")) {
		t.Errorf("the document: %v, its file inside the archive: %v", attachment, bytes.Contains(archived, []byte("%PDF-1.4")))
	}
	if got := a.Count("SELECT count(*) FROM history WHERE project_id = ? AND action = 'Archive' AND table_name = 'Projects'", a.id); got != 1 {
		t.Errorf("%d Archive lines", got)
	}
}

// UC-073 AC-73.9, AC-73.3, AC-73.4: archived, changed, retrieved, the project's own rows are the archive's again, while History, the issued
// report, the entry counter and the members stay as they are, and «سجل المشروع» says it was retrieved.
func TestAC73_9_RetrievedTheProjectIsItsArchiveAgainWhileHistoryReportsAndMembersStay(t *testing.T) {
	a := loaded(t)
	archived := a.taken()
	taken := a.rowsNow()
	counter := a.Count("SELECT last_entry_number FROM projects WHERE id = ?", a.id)
	reports := a.Count("SELECT count(*) FROM periodic_reports WHERE project_id = ?", a.id)
	if !slices.Equal(rowsIn(describedIn(t, archived))["entries"], taken["entries"]) {
		t.Fatal("the archive does not hold the project's entries")
	}

	a.Ask("POST", a.path()+"/categories", `{"name":"الحديد"}`, a.token)
	_, listed := a.Ask("GET", a.path()+"/documents", ``, a.token)
	document := testapi.Field(listed.([]any)[0], "id").(string)
	a.Ask("DELETE", a.path()+"/documents/"+document, ``, a.token)
	a.keep("المخطط")
	member, _ := a.person("أحمد")
	a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ReadProject')`, uuid.NewString(), a.id, member)
	if changed := a.rowsNow(); slices.Equal(changed["categories"], taken["categories"]) || slices.Equal(changed["documents"], taken["documents"]) {
		t.Fatal("the project did not change")
	}
	lines := a.lines()

	code, answer := a.retrieved(archived, false, a.token)
	if code != 200 {
		t.Fatalf("retrieve: %d %v", code, answer)
	}
	now := a.rowsNow()
	for _, table := range archive.Retrieved() {
		if !slices.Equal(now[table[0]], taken[table[0]]) {
			t.Errorf("%s: %d rows, the archive's %d", table[0], len(now[table[0]]), len(taken[table[0]]))
		}
	}
	after := answer["after"].(map[string]any)
	if answer["archivedAt"] != "2026-11-18T10:00:00Z" || after["entries"] != float64(len(taken["entries"])) || answer["now"] != nil {
		t.Errorf("the answer: %v", answer)
	}
	if a.lines() != lines+1 || a.Count("SELECT count(*) FROM history WHERE project_id = ? AND action = 'Retrieve'", a.id) != 1 {
		t.Errorf("History: %d lines, %d before", a.lines(), lines)
	}
	if a.Count("SELECT count(*) FROM project_permissions WHERE user_id = ?", member) != 1 {
		t.Error("the member added since the archive lost their access (R9)")
	}
	if a.Count("SELECT last_entry_number FROM projects WHERE id = ?", a.id) != counter || a.Count("SELECT count(*) FROM periodic_reports WHERE project_id = ?", a.id) != reports {
		t.Error("the entry counter or the issued report moved (R10, R11)")
	}
	_, history := a.Ask("GET", a.path()+"/history", ``, a.token)
	newest := history.([]any)[0]
	if testapi.Field(newest, "action") != "retrieved" || testapi.Field(newest, "about") != "2026-11-18T10:00:00Z" || testapi.Field(newest, "by") != "محمد" {
		t.Errorf("«سجل المشروع»'s newest line: %v", newest)
	}
	if code, _ := a.archived(a.id, a.token); code != 200 {
		t.Errorf("archived again: %d", code)
	}
}

// UC-073 AC-73.2: a preview says when and by whom the archive was taken and seven counts now and after, and changes nothing.
func TestAC73_2_APreviewCountsNowAndAfterAndChangesNothing(t *testing.T) {
	a := loaded(t)
	archived := a.taken()
	a.keep("المخطط")
	rows, lines := a.rowsNow(), a.lines()

	code, answer := a.retrieved(archived, true, a.token)
	now, after := answer["now"].(map[string]any), answer["after"].(map[string]any)
	if code != 200 || answer["archivedBy"] != "محمد" || answer["archivedAt"] != "2026-11-18T10:00:00Z" || len(now) != 7 || len(after) != 7 ||
		now["attachments"] != float64(2) || after["attachments"] != float64(1) || now["entries"] != after["entries"] || after["entries"] == float64(0) {
		t.Fatalf("preview: %d %v", code, answer)
	}
	for _, table := range archive.Retrieved() {
		if !slices.Equal(a.rowsNow()[table[0]], rows[table[0]]) {
			t.Errorf("%s changed", table[0])
		}
	}
	if a.lines() != lines {
		t.Errorf("History: %d lines, %d before", a.lines(), lines)
	}
}

// UC-073 AC-73.6, D7: an archive taken on an earlier release is brought to the server's by its own migrations, and retrieves.
func TestAC73_6_AnArchiveOfAnEarlierReleaseIsBroughtUp(t *testing.T) {
	a := loaded(t)
	archived := redescribed(t, a.taken(), func(described map[string]any) {
		described["release"] = 8
		tables := described["tables"].(map[string]any)
		var held []any
		for _, permission := range tables["project_permissions"].([]any) {
			if permission.(map[string]any)["permission"] != "ManageProjectFiles" {
				held = append(held, permission)
			}
		}
		tables["project_permissions"] = held
	})
	taken := a.rowsNow()
	a.Ask("POST", a.path()+"/categories", `{"name":"الحديد"}`, a.token)

	if code, answer := a.retrieved(archived, false, a.token); code != 200 {
		t.Fatalf("retrieve: %d %v", code, answer)
	}
	if !slices.Equal(a.rowsNow()["categories"], taken["categories"]) || !slices.Equal(a.rowsNow()["entries"], taken["entries"]) {
		t.Error("the project is not its archive")
	}
}

// UC-073 AC-73.5, R12, R14: a file that is no archive, an archive of a newer release, or one naming what this server lacks is refused, and the
// project is as it was.
func TestAC73_5_AnArchiveThatCannotRetrieveIsRefusedAndNothingIsSaved(t *testing.T) {
	a := loaded(t)
	archived := a.taken()
	firstEntry := func(change func(map[string]any)) []byte {
		return redescribed(t, archived, func(d map[string]any) { change(rowsOfTable(d, "entries")[0].(map[string]any)) })
	}
	cases := []struct {
		name    string
		content []byte
		status  int
		code    string
	}{
		{"a file that is no JSON", []byte("not an archive"), 400, "notAnArchive"},
		{"JSON that is no archive", []byte(`{"name": "not an archive"}`), 400, "notAnArchive"},
		{"another format", redescribed(t, archived, func(d map[string]any) { d["format"] = "redix.project" }), 400, "notAnArchive"},
		{"a table no release holds", redescribed(t, archived, func(d map[string]any) { d["tables"].(map[string]any)["nonsense"] = []any{} }), 400, "notAnArchive"},
		{"a column no release holds", firstEntry(func(entry map[string]any) { entry["nonsense"] = 1 }), 400, "notAnArchive"},
		{"text where a whole number goes", firstEntry(func(entry map[string]any) { entry["amount"] = "much" }), 400, "notAnArchive"},
		{"a fraction where a whole number goes", firstEntry(func(entry map[string]any) { entry["amount"] = 1.5 }), 400, "notAnArchive"},
		{"a number where text goes", firstEntry(func(entry map[string]any) { entry["description"] = 5 }), 400, "notAnArchive"},
		{"a value neither text nor a number", firstEntry(func(entry map[string]any) { entry["description"] = true }), 400, "notAnArchive"},
		{"text its column's check refuses", firstEntry(func(entry map[string]any) { entry["description"] = strings.Repeat("ط", 2001) }), 400, "notAnArchive"},
		{"a row of nothing", redescribed(t, archived, func(d map[string]any) {
			tables := d["tables"].(map[string]any)
			tables["units"] = append(tables["units"].([]any), map[string]any{})
		}), 400, "notAnArchive"},
		{"a newer release", redescribed(t, archived, func(d map[string]any) { d["release"] = 99 }), 400, "newerArchive"},
		{"a person the server lacks", firstEntry(func(entry map[string]any) { entry["created_by_user_id"] = uuid.NewString() }), 409, "archiveDoesNotFit"},
	}
	rows, lines := a.rowsNow(), a.lines()
	if answered := a.Send("POST", a.path()+"/retrieve", `{}`, "Authorization", "Bearer "+a.token); answered.Code != 400 {
		t.Errorf("no file: %d", answered.Code)
	}
	for _, c := range cases {
		code, answer := a.retrieved(c.content, false, a.token)
		if code != c.status || answer["code"] != c.code {
			t.Errorf("%s: %d %v, want %d %s", c.name, code, answer, c.status, c.code)
		}
	}
	for _, table := range archive.Retrieved() {
		if !slices.Equal(a.rowsNow()[table[0]], rows[table[0]]) {
			t.Errorf("%s changed", table[0])
		}
	}
	if a.lines() != lines {
		t.Errorf("History: %d lines, %d before", a.lines(), lines)
	}
}

// UC-073 R12: another project's archive is refused, naming that project.
func TestAC73_5_AnotherProjectsArchiveIsRefusedNamingIt(t *testing.T) {
	a := loaded(t)
	other := a.started("مشروع آخر")
	_, archived := a.archived(other, a.token)
	if code, answer := a.retrieved(archived, true, a.token); code != 409 || answer["code"] != "archiveOfAnotherProject" || answer["project"] != "مشروع آخر" {
		t.Errorf("another project's archive: %d %v", code, answer)
	}
}

// UC-073 R17: a finished project is archived, and its retrieve answers projectFinished.
func TestAC73_5_AFinishedProjectIsArchivedNeverRetrieved(t *testing.T) {
	a := loaded(t)
	if code, answer := a.Ask("POST", a.path()+"/close", ``, a.token); code != 200 {
		t.Fatalf("close: %d %v", code, answer)
	}
	archived := a.taken()
	if code, answer := a.retrieved(archived, true, a.token); code != 409 || answer["code"] != "projectFinished" {
		t.Errorf("retrieve in a finished project: %d %v", code, answer)
	}
}

// UC-073 AC-73.8: without «ملفات المشروع» neither the archive, the retrieve nor UC-071's load answers.
func TestAC73_8_WithoutManageProjectFilesNoRouteOfTheProjectsFilesAnswers(t *testing.T) {
	a := loaded(t)
	archived := a.taken()
	member, token := a.person("أحمد")
	a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ReadProject'), (?, ?, ?, 'ManageProject')`,
		uuid.NewString(), a.id, member, uuid.NewString(), a.id, member)
	if code, _ := a.archived(a.id, token); code != 403 {
		t.Errorf("archive: %d", code)
	}
	if code, _ := a.retrieved(archived, true, token); code != 403 {
		t.Errorf("retrieve: %d", code)
	}
	if code, _ := a.sent(a.path()+"/project-file", "project.json", []byte(`{}`), token, nil); code != 403 {
		t.Errorf("load: %d", code)
	}
}

// lean is a project put straight into the database, holding one category and its manager's «ملفات المشروع» alone, so failing each call of its
// archive and its retrieve stays quick.
func lean(t *testing.T) *project {
	a := &project{API: testapi.New(t), t: t}
	manager, token := a.person("محمد")
	a.token, a.id = token, uuid.Must(uuid.NewV7()).String()
	a.Exec(`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template, last_entry_number)
		VALUES (?, 'مشروع صغير', '2026-11-01', 0, '', '', 0, 0)`, a.id)
	a.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ManageProjectFiles')`, uuid.NewString(), a.id, manager)
	a.Exec(`INSERT INTO categories (id, project_id, name, sort_order, is_active) VALUES (?, ?, 'الأرض', 0, 1)`, uuid.NewString(), a.id)
	return a
}

// redix-api #141 D1: the database failing at any call of the archive, the preview or the retrieve answers 500, keeps no History line and leaves
// the project as it was.
func TestAFailingDatabaseAtAnyCallOfTheArchiveOrTheRetrieveAnswers500AndChangesNothing(t *testing.T) {
	a := lean(t)
	body, contentType := formOf("archive.json", a.taken(), nil)
	rows := a.rowsNow()
	for _, ask := range [][2]string{{"GET", a.path() + "/archive/json"}, {"POST", a.path() + "/retrieve?preview=true"}, {"POST", a.path() + "/retrieve"}} {
		for n := int64(1); ; n++ {
			lines := a.lines()
			answered, failed := a.SendFailing(n, ask[0], ask[1], body, "Content-Type", contentType, "Authorization", "Bearer "+a.token)
			if !failed {
				break
			}
			if answered.Code != 500 || a.lines() != lines {
				t.Errorf("%s %s, the database failing at its call %d: %d, %d History lines, %d before", ask[0], ask[1], n, answered.Code, a.lines(), lines)
			}
		}
	}
	for _, table := range archive.Retrieved() {
		if !slices.Equal(a.rowsNow()[table[0]], rows[table[0]]) {
			t.Errorf("%s changed", table[0])
		}
	}
}
