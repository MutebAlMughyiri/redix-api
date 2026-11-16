// Package attachments keeps a project's files and serves them only to people with access to that project (UC-021, FR-09). Each file lives outside
// the database, under the attachments folder, and is never deleted (NFR-11) (AttachmentsEndpoints, Attachment; redix-api #140).
package attachments

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"io"
	"mime"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// The limits of a file (Attachment, NFR-06).
const (
	imageLimitBytes   = 1_000_000
	anyFileLimitBytes = 25_000_000
	fileNameMaxLength = 255
	pdfType           = "application/pdf"
)

// kinds are the files read from their first bytes rather than from what the sender says: a picture or a PDF.
var kinds = []struct {
	signature   []byte
	contentType string
	extension   string
}{
	{[]byte{0xFF, 0xD8, 0xFF}, "image/jpeg", ".jpg"},
	{[]byte{0x89, 0x50, 0x4E, 0x47}, "image/png", ".png"},
	{[]byte{0x25, 0x50, 0x44, 0x46}, pdfType, ".pdf"},
}

func kindOf(content []byte) (string, string) {
	for _, kind := range kinds {
		if bytes.HasPrefix(content, kind.signature) {
			return kind.contentType, kind.extension
		}
	}
	return "", ""
}

// Routes keeps a project's files.
type Routes struct {
	database db.DB
	writer   changes.Writer
	folder   string
	links    Links
}

// Map routes the files: reading one for whoever reads the project, keeping one for whoever attaches files, an entry's or a document's.
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock, folder string, links Links) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}, folder: folder, links: links}
	router.Handle("GET", "/files/{link}", s.readByLink)
	const project = "/projects/{projectId:guid}"
	router.Handle("GET", project+"/attachments/{attachmentId:guid}", gate.Needs(access.ReadProject, s.read))
	router.Handle("POST", project+"/attachments", gate.Changes(access.AttachFiles, s.upload(false)))
	router.Handle("POST", project+"/attachments/document", gate.Changes(access.AttachFiles, s.upload(true)))
}

// read is the file of an attachment of the project, by its name; one of another project is not found (BR-33).
func (s Routes) read(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	attachment, err := s.database.Queries().AttachmentOf(r.Context(), store.AttachmentOfParams{ID: web.ID(r, "attachmentId"), ProjectID: web.ID(r, "projectId")})
	s.send(w, r, attachment, err)
}

// readByLink is the file a signed link opens, with no sign-in (FileLinks, Muteb, 16 Nov 2026); 404 for a link expired, changed or never made.
func (s Routes) readByLink(w http.ResponseWriter, r *http.Request) {
	attachment, err := s.database.Queries().AttachmentWithID(r.Context(), s.links.AttachmentOf(r.PathValue("link")))
	s.send(w, r, attachment, err)
}

// send sends the attachment's file under its own name; 404 when it was not found, or its file is not kept.
func (s Routes) send(w http.ResponseWriter, r *http.Request, attachment store.Attachment, err error) {
	if err != nil {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	content, err := os.ReadFile(filepath.Join(s.folder, filepath.FromSlash(attachment.StoragePath)))
	if err != nil {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	w.Header().Set("Content-Type", attachment.ContentType)
	w.Header().Set("Content-Disposition", "attachment; filename="+asciiName(attachment.FileName)+"; filename*=UTF-8''"+url.PathEscape(attachment.FileName))
	w.WriteHeader(http.StatusOK)
	w.Write(content)
}

// asciiName is a file's name as the plain filename of a Content-Disposition carries it: what is not ASCII as underscores.
func asciiName(name string) string {
	return strings.Map(func(letter rune) rune {
		if letter > 0x7E || letter < 0x20 {
			return '_'
		}
		return letter
	}, name)
}

// view is an attachment as the app shows it (AttachmentView).
type view struct {
	ID          uuid.UUID `json:"id"`
	FileName    string    `json:"fileName"`
	ContentType string    `json:"contentType"`
	SizeBytes   int64     `json:"sizeBytes"`
}

// upload keeps a file, and answers it: an entry's, or with anyFile a document's, refused as Keep says (400). The same file from the same person
// answers the one already kept (200), so a retried upload keeps it once (UC-029); a new one answers 201.
func (s Routes) upload(anyFile bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		content, name, ok := Sent(w, r)
		if !ok {
			return
		}
		var kept store.Attachment
		isNew := false
		done := s.writer.Write(w, r, caller, func(c changes.Change) error {
			var err error
			kept, isNew, err = Keep(c, s.folder, content, name, anyFile)
			return err
		})
		if !done {
			return
		}
		answer := view{ID: kept.ID, FileName: kept.FileName, ContentType: kept.ContentType, SizeBytes: kept.SizeBytes}
		if !isNew {
			web.JSON(w, r, http.StatusOK, answer)
			return
		}
		w.Header().Set("Location", "/projects/"+kept.ProjectID.String()+"/attachments/"+kept.ID.String())
		web.JSON(w, r, http.StatusCreated, answer)
	}
}

// Sent is the file a form sent as "file", and its name; false once it has answered 400 for none.
func Sent(w http.ResponseWriter, r *http.Request) ([]byte, string, bool) {
	file, header, err := r.FormFile("file")
	if err != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return nil, "", false
	}
	defer file.Close()
	content, err := io.ReadAll(file)
	if err != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return nil, "", false
	}
	return content, header.Filename, true
}

// Keep keeps content, sent as fileName, for the project, its file written under folder and its row added with its line; the same file from the same
// person answers the one already kept, and false (AttachmentsEndpoints.KeepAsync): the one way a file is kept, an entry's and a document's alike.
// An entry's must be a picture under 1 MB or a PDF - invalidFileType, imageTooLarge - and with anyFile any file of at most 25 MB, fileTooLarge.
func Keep(c changes.Change, folder string, content []byte, fileName string, anyFile bool) (store.Attachment, bool, error) {
	contentType, extension := kindOf(content)
	switch {
	case anyFile && len(content) > anyFileLimitBytes:
		return store.Attachment{}, false, changes.Refuse(http.StatusBadRequest, "fileTooLarge")
	case !anyFile && contentType == "":
		return store.Attachment{}, false, changes.Refuse(http.StatusBadRequest, "invalidFileType")
	case !anyFile && contentType != pdfType && len(content) >= imageLimitBytes:
		return store.Attachment{}, false, changes.Refuse(http.StatusBadRequest, "imageTooLarge")
	}
	name := filepath.Base(strings.ReplaceAll(strings.TrimSpace(fileName), "\\", "/"))
	if runes := []rune(name); len(runes) > fileNameMaxLength {
		name = string(runes[len(runes)-fileNameMaxLength:])
	}
	if contentType == "" {
		contentType = typeNamedBy(name)
	}
	hash := sha256.Sum256(content)
	sent, err := c.Q.AttachmentSent(c.Ctx, store.AttachmentSentParams{ProjectID: c.ProjectID, UserID: c.Actor, Sha256: hex.EncodeToString(hash[:])})
	if err != nil || len(sent) > 0 {
		if len(sent) > 0 {
			return sent[0], false, err
		}
		return store.Attachment{}, false, err
	}
	id := uuid.Must(uuid.NewV7())
	kept := store.Attachment{ID: id, ProjectID: c.ProjectID, StoragePath: c.ProjectID.String() + "/" + id.String() + extension, FileName: name, ContentType: contentType,
		SizeBytes: int64(len(content)), Sha256: hex.EncodeToString(hash[:]), UploadedByUserID: c.Actor, UploadedAt: c.Now}
	path := filepath.Join(folder, filepath.FromSlash(kept.StoragePath))
	if err := os.MkdirAll(filepath.Dir(path), 0o750); err != nil {
		return kept, false, err
	}
	if err := os.WriteFile(path, content, 0o640); err != nil {
		return kept, false, err
	}
	if err := c.Q.AddAttachment(c.Ctx, store.AddAttachmentParams(kept)); err != nil {
		return kept, false, err
	}
	return kept, true, c.Record(history.Attach, "Attachments", kept.ID, nil, map[string]any{"ProjectId": kept.ProjectID, "StoragePath": kept.StoragePath,
		"FileName": kept.FileName, "ContentType": kept.ContentType, "SizeBytes": kept.SizeBytes, "Sha256": kept.Sha256,
		"UploadedByUserId": kept.UploadedByUserID, "UploadedAt": kept.UploadedAt})
}

// typeNamedBy is what a file is by its extension, so the phone knows which app opens it; application/octet-stream for one none names.
func typeNamedBy(name string) string {
	if byExtension := mime.TypeByExtension(strings.ToLower(filepath.Ext(name))); byExtension != "" {
		return strings.SplitN(byExtension, ";", 2)[0]
	}
	return "application/octet-stream"
}
