// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package documents_test

import (
	"slices"
	"strings"
	"testing"
	"time"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// kept is a project its manager keeps documents in.
type kept struct {
	*testapi.API
	t           *testing.T
	token, path string
}

func newKept(t *testing.T) kept {
	api := testapi.New(t)
	token, path := api.ApprovedProject()
	return kept{API: api, t: t, token: token, path: path}
}

// wordFile is a Word file of its own, one no other document holds.
func wordFile() []byte {
	return append([]byte{0x50, 0x4B, 0x03, 0x04}, uuid.Must(uuid.NewV7()).String()...)
}

// add keeps content, a file called fileName, as the document name in group, as token, and is the status and the code the API answered.
func (k kept) add(token, name, group string, content []byte, fileName string) (int, any) {
	k.t.Helper()
	code, answer := k.AskWithFile(k.path+"/documents", fileName, content, token, map[string]string{"name": name, "group": group})
	return code, testapi.Field(answer, "code")
}

// listed is every document token reads, as the list answers them.
func (k kept) listed(token string) []map[string]any {
	k.t.Helper()
	code, answer := k.Ask("GET", k.path+"/documents", ``, token)
	if code != 200 {
		k.t.Fatalf("the list: %d %v", code, answer)
	}
	var documents []map[string]any
	for _, one := range answer.([]any) {
		documents = append(documents, one.(map[string]any))
	}
	return documents
}

// UC-064: the manager keeps documents, their names and groups trimmed; everyone who reads the project reads them, those in no group first, then
// by group, the newest first in each, and opens their files; nobody else keeps one.
func TestUC064_TheManagerKeepsDocumentsAndEveryoneReadsThemUngroupedFirstThenByGroup(t *testing.T) {
	k := newKept(t)
	for _, document := range [][2]string{{"المخطط المعماري", "مخططات"}, {"رخصة البناء", " تراخيص "}, {"  عقد المقاول الرئيسي ", ""}, {"مخطط الموقع العام", "مخططات"}} {
		if code, refused := k.add(k.token, document[0], document[1], wordFile(), "contract.docx"); code != 200 {
			t.Fatalf("keeping %s: %d %v", document[0], code, refused)
		}
		k.Clock.Advance(time.Minute)
	}
	reader, token := k.Person("Reader")
	k.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, 'ReadProject')`, uuid.NewString(),
		strings.TrimPrefix(k.path, "/projects/"), reader)
	seen := k.listed(token)
	var order []string
	for _, document := range seen {
		order = append(order, document["name"].(string)+" | "+document["group"].(string))
	}
	if want := []string{"عقد المقاول الرئيسي | ", "رخصة البناء | تراخيص", "مخطط الموقع العام | مخططات", "المخطط المعماري | مخططات"}; !slices.Equal(order, want) {
		t.Errorf("the reader reads %v, want %v", order, want)
	}
	if code, _ := k.Ask("GET", k.path+"/attachments/"+seen[0]["attachmentId"].(string), ``, token); code != 200 {
		t.Errorf("the reader opens the file: %d", code)
	}
	if code, _ := k.add(token, "مستند", "", wordFile(), "a.docx"); code != 403 {
		t.Errorf("the reader keeps a document: %d, want 403", code)
	}
}

// UC-064: a document is any file, its type named by its extension, while an entry's invoice stays a picture or a PDF.
func TestUC064_ADocumentIsAnyFileNamedByItsExtensionWhileAnInvoiceStaysAPictureOrAPdf(t *testing.T) {
	k := newKept(t)
	k.add(k.token, "جدول الكميات", "", wordFile(), "contract.docx")
	k.add(k.token, "مخطط أوتوكاد", "", append([]byte{0x41, 0x43, 0x31, 0x30}, uuid.Must(uuid.NewV7()).String()...), "site.dwg")
	types := map[string]string{}
	for _, document := range k.listed(k.token) {
		types[document["fileName"].(string)] = document["contentType"].(string)
	}
	if types["site.dwg"] != "application/octet-stream" || types["contract.docx"] != "application/vnd.openxmlformats-officedocument.wordprocessingml.document" {
		t.Errorf("the documents' types: %v", types)
	}
	if code, answer := k.AskWithFile(k.path+"/attachments", "invoice.docx", wordFile(), k.token, nil); code != 400 || testapi.Field(answer, "code") != "invalidFileType" {
		t.Errorf("a Word file as an invoice: %d %v", code, answer)
	}
}

// UC-064: a document needs a name of 200 letters at most, a group of 100 at most and a file of 25 MB at most, and one file is kept once.
func TestUC064_ADocumentNeedsANameAGroupOf100LettersAtMostAFileOf25MbAtMostAndIsKeptOnce(t *testing.T) {
	k := newKept(t)
	for _, refused := range []struct {
		name, group string
		content     []byte
		code        string
	}{
		{" ", "", wordFile(), "invalidName"},
		{strings.Repeat("م", 201), "", wordFile(), "invalidName"},
		{"مستند", strings.Repeat("م", 101), wordFile(), "invalidGroup"},
		{"مستند", "", make([]byte, 25_000_001), "fileTooLarge"},
	} {
		if code, answered := k.add(k.token, refused.name, refused.group, refused.content, "a.zip"); code != 400 || answered != refused.code {
			t.Errorf("%q in %q: %d %v, want %s", refused.name, refused.group, code, answered, refused.code)
		}
	}
	file := wordFile()
	if code, _ := k.add(k.token, "مستند", "", file, "a.docx"); code != 200 {
		t.Fatalf("a document: %d", code)
	}
	if code, refused := k.add(k.token, "مستند آخر", "", file, "a.docx"); code != 409 || refused != "documentAlreadyKept" {
		t.Errorf("the same file again: %d %v", code, refused)
	}
}

// UC-064: a removed document leaves the list and its file stays; the old blueprints route is gone.
func TestUC064_ARemovedDocumentLeavesTheListButItsFileStays(t *testing.T) {
	k := newKept(t)
	k.add(k.token, "مخطط الكهرباء", "مخططات", wordFile(), "contract.docx")
	documents := k.listed(k.token)
	if len(documents) != 1 {
		t.Fatalf("%d documents", len(documents))
	}
	if code, _ := k.Ask("DELETE", k.path+"/documents/"+documents[0]["id"].(string), ``, k.token); code != 200 {
		t.Fatalf("removing: %d", code)
	}
	if left := k.listed(k.token); len(left) != 0 {
		t.Errorf("%d documents left", len(left))
	}
	if code, _ := k.Ask("GET", k.path+"/attachments/"+documents[0]["attachmentId"].(string), ``, k.token); code != 200 {
		t.Errorf("the removed document's file: %d", code)
	}
	if code, _ := k.Ask("GET", k.path+"/blueprints", ``, k.token); code != 404 {
		t.Errorf("the old blueprints route: %d", code)
	}
}

// UC-064: keeping, listing and removing documents answer 500 when the database fails at any of their calls, and keep nothing half done.
func TestUC064_AFailingDatabaseAtAnyCallAnswers500(t *testing.T) {
	k := newKept(t)
	k.add(k.token, "مستند", "", wordFile(), "a.docx")
	document := k.listed(k.token)[0]["id"].(string)
	body, contentType := k.Form("b.docx", wordFile(), map[string]string{"name": "مستند آخر", "group": ""})
	for _, ask := range [][3]string{{"GET", k.path + "/documents", ""}, {"POST", k.path + "/documents", body}, {"DELETE", k.path + "/documents/" + document, ""}} {
		for n := int64(1); ; n++ {
			answered, failed := k.SendFailing(n, ask[0], ask[1], ask[2], "Content-Type", contentType, "Authorization", "Bearer "+k.token)
			if !failed {
				break
			}
			if answered.Code != 500 {
				t.Errorf("%s %s with the database failing at its call %d answered %d", ask[0], ask[1], n, answered.Code)
			}
		}
	}
}
