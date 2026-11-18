// Package documents keeps a project's documents (UC-064): the manager keeps any file under a name, in a group when he gives one, and everyone who
// reads the project opens them through the attachment store (DocumentsEndpoints, redix-api #140).
package documents

import (
	"net/http"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// The limits of a document (Document.NameMaxLength, GroupMaxLength).
const (
	nameMaxLength  = 200
	groupMaxLength = 100
)

// Routes keeps a project's documents.
type Routes struct {
	database db.DB
	writer   changes.Writer
	folder   string
}

// Map routes the documents: the list for whoever reads the project, the changes for whoever manages it, in a project not finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock, folder string) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}, folder: folder}
	const documents = "/projects/{projectId:guid}/documents"
	router.Handle("GET", documents, gate.Needs(access.ReadProject, s.list))
	router.Handle("POST", documents, gate.Changes(access.ManageProject, s.add))
	router.Handle("DELETE", documents+"/{documentId:guid}", gate.Changes(access.ManageProject, s.remove))
}

// documentView is part of what GET /projects/{projectId}/documents answers (UC-064).
type documentView struct {
	ID           uuid.UUID     `json:"id"`
	Name         string        `json:"name"`
	Group        string        `json:"group"`
	AttachmentID uuid.UUID     `json:"attachmentId"`
	FileName     string        `json:"fileName"`
	ContentType  string        `json:"contentType"`
	UploadedAt   moment.Moment `json:"uploadedAt"`
}

// list is the project's documents: those in no group first, then each group in order, newest first within each.
func (s Routes) list(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	rows, err := s.database.Queries().DocumentsOf(r.Context(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []documentView{}
	for _, row := range rows {
		views = append(views, documentView{ID: row.ID, Name: row.Name, Group: row.GroupName, AttachmentID: row.AttachmentID, FileName: row.FileName, ContentType: row.ContentType,
			UploadedAt: row.UploadedAt})
	}
	web.JSON(w, r, http.StatusOK, views)
}

type idView struct {
	ID uuid.UUID `json:"id"`
}

func fields(document store.Document) map[string]any {
	return map[string]any{"ProjectId": document.ProjectID, "AttachmentId": document.AttachmentID, "Name": document.Name, "Group": document.GroupName,
		"UploadedByUserId": document.UploadedByUserID, "UploadedAt": document.UploadedAt}
}

// add keeps a file of any kind as a document of the project under its name and group: invalidName for a name empty or past 200 letters,
// invalidGroup past 100, fileTooLarge past 25 MB (400); documentAlreadyKept (409) for a file already a document.
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	content, fileName, ok := attachments.Sent(w, r)
	if !ok {
		return
	}
	name, group := strings.TrimSpace(r.FormValue("name")), strings.TrimSpace(r.FormValue("group"))
	if name == "" || utf8.RuneCountInString(name) > nameMaxLength {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidName")
		return
	}
	if utf8.RuneCountInString(group) > groupMaxLength {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidGroup")
		return
	}
	var document store.Document
	added := s.writer.Write(w, r, caller, func(c changes.Change) error {
		attachment, _, err := attachments.Keep(c, s.folder, content, fileName, true)
		if err != nil {
			return err
		}
		kept, err := c.Q.DocumentKept(c.Ctx, store.DocumentKeptParams{ProjectID: c.ProjectID, AttachmentID: attachment.ID})
		if err != nil || kept {
			if kept {
				return changes.Refuse(http.StatusConflict, "documentAlreadyKept")
			}
			return err
		}
		document = store.Document{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, AttachmentID: attachment.ID, Name: name, GroupName: group,
			UploadedByUserID: c.Actor, UploadedAt: c.Now}
		if err := c.Q.AddDocument(c.Ctx, store.AddDocumentParams(document)); err != nil {
			return err
		}
		return c.Record(history.Create, "Documents", document.ID, nil, fields(document))
	})
	if added {
		web.JSON(w, r, http.StatusOK, idView{document.ID})
	}
}

// remove takes a document off the project's list; its file stays in the attachment store, which never deletes (NFR-11). 404 for one outside the
// project.
func (s Routes) remove(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	documentID := web.ID(r, "documentId")
	removed := s.writer.Write(w, r, caller, func(c changes.Change) error {
		document, err := changes.Found(c.Q.DocumentOf(c.Ctx, store.DocumentOfParams{ID: documentID, ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		return c.Remove("Documents", document.ID, fields(document), func() error { return c.Q.RemoveDocument(c.Ctx, document.ID) })
	})
	if removed {
		web.JSON(w, r, http.StatusOK, idView{documentID})
	}
}
