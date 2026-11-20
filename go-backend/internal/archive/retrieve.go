// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package archive

import (
	"context"
	"database/sql"
	"errors"
	"net/http"
	"slices"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// parts are what a preview counts, now and after, each a table of tables (D8).
var parts = []string{"entries", "contracts", "accounts", "suppliers", "partners", "items", "attachments"}

// retrieval is what a retrieve answers: when the archive was taken, and how many of each part the project holds after it; a preview also says
// by whom it was taken and how many it holds now (AC-73.2).
type retrieval struct {
	ArchivedAt moment.Moment    `json:"archivedAt"`
	ArchivedBy string           `json:"archivedBy,omitempty"`
	Now        map[string]int64 `json:"now,omitempty"`
	After      map[string]int64 `json:"after"`
}

// retrieve makes the project's own rows its archive's (AC-73.3): every retrieved table emptied of the project's rows and filled with the
// archive's, each row as it was, in one transaction, with one History line, Retrieve. A bill's or a PDF's row names its file in the storage, which
// a retrieve never touches. ?preview=true says what it would do and changes nothing (AC-73.2). An archive of another project or of a newer
// release, or naming what this server lacks, is refused, and nothing is saved (AC-73.5).
func (s Routes) retrieve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	content, _, sent := attachments.Sent(w, r)
	if !sent {
		return
	}
	ctx, projectID := r.Context(), web.ID(r, "projectId")
	archived, code := opened(content)
	release, err := s.database.Release(ctx)
	switch {
	case err != nil:
		problem.Status(w, r, http.StatusInternalServerError)
		return
	case code != "":
		problem.Refuse(w, r, http.StatusBadRequest, code)
		return
	case archived.Project.ID != projectID:
		problem.RefuseOver(w, r, http.StatusConflict, "archiveOfAnotherProject", "project", archived.Project.Name)
		return
	case archived.Release > release:
		problem.Refuse(w, r, http.StatusBadRequest, "newerArchive")
		return
	}
	rows, err := broughtUp(ctx, archived)
	if changes.Answered(w, r, err) {
		return
	}
	after := map[string]int64{}
	for _, part := range parts {
		after[part] = int64(len(rows[part]))
	}
	if r.URL.Query().Get("preview") == "true" {
		now, err := s.counted(ctx, projectID)
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		web.JSON(w, r, http.StatusOK, retrieval{ArchivedAt: archived.ArchivedAt, ArchivedBy: archived.ArchivedBy.Name, Now: now, After: after})
		return
	}
	err = s.database.InSQL(ctx, func(tx *sql.Tx, q *store.Queries) error {
		if err := override(ctx, tx, projectID, rows); err != nil {
			return err
		}
		c := changes.Change{Ctx: ctx, Q: q, ProjectID: projectID, Actor: caller.UserID, Now: moment.Of(s.writer.Clock.Now())}
		return c.Record(history.Retrieve, "Projects", projectID, nil, map[string]any{"ArchivedAt": archived.ArchivedAt, "ArchivedByUserId": archived.ArchivedBy.ID})
	})
	if !changes.Answered(w, r, err) {
		web.JSON(w, r, http.StatusOK, retrieval{ArchivedAt: archived.ArchivedAt, After: after})
	}
}

// broughtUp is the archive's rows of the tables a retrieve replaces, as the server's release keeps them (D7): put into a scratch database at the
// archive's release, which the server's own migrations then bring up, its data migrations reaching the archive's rows too. A table, a column or a
// value the archive's release does not hold is notAnArchive (400); rows a migration cannot bring up are archiveDoesNotFit (409).
func broughtUp(ctx context.Context, archived file) (map[string][]row, error) {
	scratch, err := db.OpenScratch(ctx, archived.Release)
	if err != nil {
		return nil, err
	}
	defer func() { _ = scratch.Close() }()
	if err := filled(ctx, scratch, archived); err != nil {
		return nil, err
	}
	if scratch.Up(ctx) != nil {
		return nil, changes.Refuse(http.StatusConflict, "archiveDoesNotFit")
	}
	brought := map[string][]row{}
	for _, t := range retrieved {
		if brought[t.name], err = rowsOf(ctx, scratch, t, archived.Project.ID); err != nil {
			return nil, err
		}
	}
	return brought, nil
}

// notAnArchive is a file whose tables, columns or values its release does not hold.
var notAnArchive = changes.Refuse(http.StatusBadRequest, "notAnArchive")

// filled puts every row of the archive into scratch, or refuses it as notAnArchive.
func filled(ctx context.Context, scratch db.Scratch, archived file) error {
	tx, err := scratch.BeginTx(ctx, nil)
	if err != nil {
		return err
	}
	defer func() { _ = tx.Rollback() }()
	for name, rows := range archived.Tables {
		if _, known := tableNamed(name); !known {
			return notAnArchive
		}
		columns, err := columnsOf(ctx, tx, name)
		if err != nil {
			return err
		}
		for _, r := range rows {
			if put(ctx, tx, name, r, columns) != nil {
				return notAnArchive
			}
		}
	}
	return tx.Commit()
}

// override makes the project's own rows the archive's (AC-73.3): every retrieved table emptied of the project's rows, the rows naming others
// first, then filled from rows, each row as it was. A row this server's database does not take, naming a person it lacks, is archiveDoesNotFit,
// and the transaction keeps nothing.
func override(ctx context.Context, tx *sql.Tx, projectID uuid.UUID, rows map[string][]row) error {
	project := sql.Named("project", projectID)
	for _, t := range slices.Backward(retrieved) {
		if _, err := tx.ExecContext(ctx, "DELETE FROM "+t.name+" WHERE "+t.scope, project); err != nil {
			return err
		}
	}
	for _, t := range retrieved {
		columns, err := columnsOf(ctx, tx, t.name)
		if err != nil {
			return err
		}
		for _, r := range rows[t.name] {
			if err := put(ctx, tx, t.name, r, columns); errors.Is(err, errUnfit) || db.Refuses(err) {
				return changes.Refuse(http.StatusConflict, "archiveDoesNotFit")
			} else if err != nil {
				return err
			}
		}
	}
	return nil
}

// errUnfit is a row whose names or values its table's columns do not take.
var errUnfit = errors.New("archive: a row its table does not take")

// put puts r into table through tx as columns take it, or answers errUnfit (G8).
func put(ctx context.Context, tx *sql.Tx, table string, r row, columns map[string]bool) error {
	statement, values, fits := insertOf(table, r, columns)
	if !fits {
		return errUnfit
	}
	_, err := tx.ExecContext(ctx, statement, values...)
	return err
}

// counted is how many of each part the project holds now.
func (s Routes) counted(ctx context.Context, projectID uuid.UUID) (map[string]int64, error) {
	counts := map[string]int64{}
	for _, part := range parts {
		t, _ := tableNamed(part)
		var count int64
		if err := s.database.Read.QueryRowContext(ctx, "SELECT count(*) FROM "+t.name+" WHERE "+t.scope, sql.Named("project", projectID)).Scan(&count); err != nil {
			return nil, err
		}
		counts[part] = count
	}
	return counts, nil
}
