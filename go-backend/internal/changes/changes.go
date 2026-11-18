// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package changes is what every route that changes something inside a project shares (ProjectChanges): one transaction for the whole change,
// the refusal it answers with when it rolls back, and a History line for every row it adds, changes or removes - #86 "UC-069: History of every
// action, with its time, user and record" decided on 6 Nov 2026 that every action is recorded.
package changes

import (
	"context"
	"database/sql"
	"errors"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Change is one write to a project: its queries, and who makes it in which project and when, which each History line it adds names.
type Change struct {
	Ctx       context.Context
	Q         *store.Queries
	ProjectID uuid.UUID
	Actor     uuid.UUID
	Now       moment.Moment
	// Access is what the actor holds, for an owner whose change depends on it.
	Access access.Access
}

// Save writes a row by write and records it: created when isNew, otherwise only what changed between before and after - the C# names of the
// fields - and a row that did not change is neither written nor recorded.
func (c Change) Save(table string, id uuid.UUID, isNew bool, before, after map[string]any, write func() error) error {
	action := history.Create
	if isNew {
		before = nil
	} else {
		action = history.Update
		if before, after = history.Changed(before, after); len(after) == 0 {
			return nil
		}
	}
	if err := write(); err != nil {
		return err
	}
	return history.Write(c.Ctx, c.Q, c.Now, c.Actor, action, history.Change{Table: table, RecordID: id, ProjectID: c.ProjectID, Before: before, After: after})
}

// Remove deletes a row by write and records what it held.
func (c Change) Remove(table string, id uuid.UUID, before map[string]any, write func() error) error {
	if err := write(); err != nil {
		return err
	}
	return history.Write(c.Ctx, c.Q, c.Now, c.Actor, history.Delete, history.Change{Table: table, RecordID: id, ProjectID: c.ProjectID, Before: before})
}

// Record adds a History line of action for the row id of table as it stands, whatever was written: where .NET's AddHistory wrote a line of its
// own choosing that a route's history reads back - an account disabled again, a custodian under the account's change - Go writes the same.
func (c Change) Record(action history.Action, table string, id uuid.UUID, before, after map[string]any) error {
	return history.Write(c.Ctx, c.Q, c.Now, c.Actor, action, history.Change{Table: table, RecordID: id, ProjectID: c.ProjectID, Before: before, After: after})
}

// Refusal is a change refused: the status it answers and its code, and what it was refused over, under Name, when it carries something; one with
// no code answers its status alone, as a row not found does.
type Refusal struct {
	Status int
	Code   string
	Name   string
	Detail any
}

func (r Refusal) Error() string { return "changes: refused " + r.Code }

// NotFound is a row the project does not hold, 404.
var NotFound = Refusal{Status: http.StatusNotFound}

// Refuse is the refusal code answered with status.
func Refuse(status int, code string) error { return Refusal{Status: status, Code: code} }

// RefuseOver is the refusal code answered with status, carrying detail under name.
func RefuseOver(status int, code, name string, detail any) error {
	return Refusal{Status: status, Code: code, Name: name, Detail: detail}
}

// Found is a row a lookup found, or NotFound for none.
func Found[T any](row T, err error) (T, error) {
	if errors.Is(err, sql.ErrNoRows) {
		return row, NotFound
	}
	return row, err
}

// Unknown is a lookup's err, with a row that is not there refused as code (400): a request naming something the project does not hold. Any other
// failure stays the failure it is, so a failing database answers 500, never a refusal.
func Unknown(err error, code string) error {
	if errors.Is(err, sql.ErrNoRows) {
		return Refuse(http.StatusBadRequest, code)
	}
	return err
}

// Answered answers err - a refusal with its status and code, NotFound 404, any other failure 500 - and says whether there was one to answer.
// Every route answers a failure through it, so a failing database is never told as a row that is not there.
func Answered(rw http.ResponseWriter, r *http.Request, err error) bool {
	var refused Refusal
	switch {
	case err == nil:
		return false
	case errors.As(err, &refused) && refused.Code == "":
		problem.Status(rw, r, refused.Status)
	case errors.As(err, &refused) && refused.Name != "":
		problem.RefuseOver(rw, r, refused.Status, refused.Code, refused.Name, refused.Detail)
	case errors.As(err, &refused):
		problem.Refuse(rw, r, refused.Status, refused.Code)
	default:
		problem.Status(rw, r, http.StatusInternalServerError)
	}
	return true
}

// Writer runs the changes of a domain's routes.
type Writer struct {
	Database db.DB
	Clock    clock.Clock
}

// Write runs work as one change to the route's project and says whether it was done; otherwise it has answered: a refusal with its status and
// code, any other failure 500. Nothing work wrote is kept when it fails.
func (w Writer) Write(rw http.ResponseWriter, r *http.Request, caller access.Caller, work func(Change) error) bool {
	c := Change{Ctx: r.Context(), ProjectID: web.ID(r, "projectId"), Actor: caller.UserID, Now: moment.Of(w.Clock.Now()), Access: caller.Access}
	err := w.Database.InTx(r.Context(), func(q *store.Queries) error {
		c.Q = q
		return work(c)
	})
	return !Answered(rw, r, err)
}

// Done runs work as Write does and answers 200 with no body once it is done.
func (w Writer) Done(rw http.ResponseWriter, r *http.Request, caller access.Caller, work func(Change) error) {
	if w.Write(rw, r, caller, work) {
		rw.WriteHeader(http.StatusOK)
	}
}

// nameRequest is the body of every route that only names something, read by Named.
type nameRequest struct {
	Name string `json:"name"`
}

// Named reads a request holding a name and answers invalidName (400) for one the rule refuses (ProjectChanges.WithNameAsync); false once it has
// answered.
func Named(w http.ResponseWriter, r *http.Request) (string, bool) {
	var request nameRequest
	if !web.Read(w, r, &request) {
		return "", false
	}
	name, ok := naming.Named(request.Name)
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, naming.Invalid)
	}
	return name, ok
}
