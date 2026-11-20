// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package archive

import (
	"bytes"
	"context"
	"database/sql"
	"encoding/json"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// What names an archive (AC-73.1); a file naming another is notAnArchive.
const (
	format  = "redix.archive"
	version = 1
)

// file is the archive: when and by whom it was taken, at which release of the database, and every row of the project's tables as stored, each
// table by its own name (D4). A bill or a PDF is its attachment's row - its file's name and storage path - and the file itself stays in the
// storage the server's configuration names (the owner, 18 Nov 2026), never inside the archive.
type file struct {
	Format     string           `json:"format"`
	Version    int              `json:"version"`
	Release    int64            `json:"release"`
	Project    named            `json:"project"`
	ArchivedAt moment.Moment    `json:"archivedAt"`
	ArchivedBy named            `json:"archivedBy"`
	Tables     map[string][]row `json:"tables"`
}

// named is a record by its id and its name.
type named struct {
	ID   uuid.UUID `json:"id"`
	Name string    `json:"name"`
}

// Routes are UC-073's: taking a project's archive and retrieving one.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the archive and the retrieve, for whoever holds ManageProjectFiles, «ملفات المشروع» (D2); a finished project is archived, never
// retrieved (R17).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	router.Handle("GET", "/projects/{projectId:guid}/archive/json", gate.Needs(access.ManageProjectFiles, s.archive))
	router.Handle("POST", "/projects/{projectId:guid}/retrieve", gate.Changes(access.ManageProjectFiles, s.retrieve))
}

// archive hands over the project's archive (AC-73.1): one JSON file holding every row of its 44 tables, read in one snapshot; a History line,
// Archive, names who took it (R15).
func (s Routes) archive(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	archived, err := s.taken(r.Context(), web.ID(r, "projectId"), caller.UserID)
	var content []byte
	if err == nil {
		content, err = json.MarshalIndent(archived, "", " ")
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if s.writer.Write(w, r, caller, func(c changes.Change) error { return c.Record(history.Archive, "Projects", c.ProjectID, nil, nil) }) {
		web.SendFile(w, "application/json", archived.Project.Name+".json", content)
	}
}

// taken is the archive of projectID taken by userID, its rows read in one snapshot.
func (s Routes) taken(ctx context.Context, projectID, userID uuid.UUID) (file, error) {
	archived := file{Format: format, Version: version, Project: named{ID: projectID}, ArchivedAt: moment.Of(s.writer.Clock.Now()), ArchivedBy: named{ID: userID},
		Tables: map[string][]row{}}
	release, err := s.database.Release(ctx)
	if err != nil {
		return file{}, err
	}
	archived.Release = release
	if err := s.database.Read.QueryRowContext(ctx, "SELECT display_name FROM users WHERE id = ?", userID).Scan(&archived.ArchivedBy.Name); err != nil {
		return file{}, err
	}
	snapshot, err := s.database.Read.BeginTx(ctx, &sql.TxOptions{ReadOnly: true})
	if err != nil {
		return file{}, err
	}
	defer func() { _ = snapshot.Rollback() }()
	for _, t := range tables {
		if archived.Tables[t.name], err = rowsOf(ctx, snapshot, t, projectID); err != nil {
			return file{}, err
		}
	}
	archived.Project.Name = archived.Tables["projects"][0]["name"].(string)
	return archived, nil
}

// opened is the archive content holds, or the code a file that is no archive is refused with.
func opened(content []byte) (file, string) {
	decoder := json.NewDecoder(bytes.NewReader(content))
	decoder.UseNumber()
	var archived file
	if decoder.Decode(&archived) != nil || archived.Format != format || archived.Version != version || archived.Release < 1 {
		return file{}, "notAnArchive"
	}
	return archived, ""
}
