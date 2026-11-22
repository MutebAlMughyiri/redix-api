// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/documents"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// documentsShown is Documents as the app's screen shows them (UC-064): the project's documents, those in no group first, then each group in
// order, the search typed, and the project's time zone their dates are read in.
type documentsShown struct {
	all      []documents.View
	text     string
	location *time.Location
}

// shown is what the search keeps: as the app's, it finds a document's name, group and file's name.
func (s documentsShown) shown() []documents.View {
	return slices.DeleteFunc(slices.Clone(s.all), func(document documents.View) bool {
		return !searchHolds(s.text, document.Name, document.Group, document.FileName)
	})
}

// startsGroup says whether the document at of shown is the first of a group, which the app heads with the group's name.
func startsGroup(shown []documents.View, at int) bool {
	return shown[at].Group != "" && (at == 0 || shown[at-1].Group != shown[at].Group)
}

// groups are the groups the documents are in, which Add document offers as the app's field does.
func (s documentsShown) groups() []string {
	var groups []string
	for _, document := range s.all {
		if document.Group != "" && !slices.Contains(groups, document.Group) {
			groups = append(groups, document.Group)
		}
	}
	return groups
}

// documentOf is the document with documentID.
func (s documentsShown) documentOf(documentID uuid.UUID) (documents.View, bool) {
	at := slices.IndexFunc(s.all, func(document documents.View) bool { return document.ID == documentID })
	if at < 0 {
		return documents.View{}, false
	}
	return s.all[at], true
}

// fileLine is what a document's row says under its name, as the app's: its file's name and the day it was kept.
func (s documentsShown) fileLine(document documents.View) string {
	return parts(document.FileName, document.UploadedAt.In(s.location).Format(time.DateOnly))
}

// isPicture says whether a file of contentType is a picture, which the app shows to zoom into and a panel shows as it is.
func isPicture(contentType string) bool { return strings.HasPrefix(contentType, "image/") }

// documentsOf is Documents with the search text, read through the rule the app's route answers (documents.ListOf). The error is the database's.
func (s site) documentsOf(ctx context.Context, p page, text string) (documentsShown, error) {
	read := s.Database.Queries()
	all, err := documents.ListOf(ctx, read, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	return documentsShown{all: all, text: strings.TrimSpace(text), location: zones.Location(zone)}, errors.Join(err, zoneErr)
}

// documentsTab is Documents in its tab (UC-064, redix-api #175 D3), for everyone who reads the project.
func (s site) documentsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.documentsOf(r.Context(), p, r.URL.Query().Get("text"))
	return documentsList(p, shown), err
}

// documentDetails is a document in the details panel, Remove asked first (asking=remove) as the app's swipe asks it, or Add document's form
// (id new). 404 for a document the project does not hold, or Add document without ManageProject.
func (s site) documentDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	adding := r.PathValue("id") == "new"
	if !p.inProject() || (adding && !p.mayManageProject()) {
		http.NotFound(w, r)
		return
	}
	shown, err := s.documentsOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	if adding {
		draw(w, r, http.StatusOK, documentForm(p, shown, "", "", ""))
		return
	}
	documentID, _ := uuid.Parse(r.PathValue("id"))
	document, found := shown.documentOf(documentID)
	if !found {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, documentPanel(p, shown, document, said{asking: r.URL.Query().Get("asking")}))
}

// addDocument keeps Add document (POST /document/new/add): the file the form sent under its name and group, through the rule the app's route
// runs, and answers the new document's panel saying it was saved, Documents beside it; refused, the form says why in the app's words, as a form
// with no file does. 403 without ManageProject; a finished project keeps nothing (AC-04.1).
func (s site) addDocument(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageProject) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	content, fileName, err := attachments.FormFile(r)
	refusal, documentID := "documents.projectFinished", uuid.Nil
	switch {
	case err != nil:
		refusal = "documents.fileRequired"
	case !p.finished():
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
			documentID, err = documents.Add(c, s.AttachmentsFolder, content, fileName, r.PostFormValue("name"), r.PostFormValue("group"))
			return err
		})
		if refusal, err = refusalOf("documents", err); err != nil {
			failed(w)
			return
		}
	}
	shown, err := s.documentsOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusOK, documentForm(p, shown, r.PostFormValue("name"), r.PostFormValue("group"), refusal))
		return
	}
	document, _ := shown.documentOf(documentID)
	draw(w, r, http.StatusOK, documentPanel(p, shown, document, said{notice: "documents.saved"}), documentsBeside(p, shown))
}

// removeDocument takes a document off the list (POST /document/{id}/remove) through the rule the app's route runs, and answers the panel closed
// with Documents beside it; refused, the panel says why. 403 without ManageProject; 404 for a document the project does not hold; a finished
// project changes nothing (AC-04.1).
func (s site) removeDocument(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageProject) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.documentsOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	documentID, _ := uuid.Parse(r.PathValue("id"))
	document, found := shown.documentOf(documentID)
	if !found {
		http.NotFound(w, r)
		return
	}
	refusal := "documents.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			return documents.Remove(c, documentID)
		})
		if refusal, err = refusalOf("documents", err); err != nil {
			failed(w)
			return
		}
	}
	if refusal != "" {
		draw(w, r, http.StatusOK, documentPanel(p, shown, document, said{refusal: refusal}))
		return
	}
	if shown, err = s.documentsOf(r.Context(), p, ""); err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, documentsBeside(p, shown))
}

// attachmentFile is a file the project keeps (GET /attachment/{id}), handed over under its own name through the rule the app's route reads it
// with (attachments.FileOf); 404 for one of another project, or whose file is not kept (BR-33).
func (s site) attachmentFile(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	attachmentID, _ := uuid.Parse(r.PathValue("id"))
	attachment, content, err := attachments.FileOf(r.Context(), s.Database.Queries(), s.AttachmentsFolder, p.project.ID, attachmentID)
	switch {
	case errors.Is(err, changes.NotFound):
		http.NotFound(w, r)
	case err != nil:
		failed(w)
	default:
		web.SendFile(w, attachment.ContentType, attachment.FileName, content)
	}
}
