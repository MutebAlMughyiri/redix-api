// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package access

import (
	"context"
	"encoding/json"
	"path/filepath"
	"slices"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/google/uuid"
)

var (
	user     = uuid.MustParse("0193a1b2-0000-7000-8000-000000000001")
	active   = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a1")
	pending  = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a2")
	inactive = uuid.MustParse("0193a1b2-0000-7000-8000-0000000000a3")
)

// world is a person holding RecordEntries in an active project, a pending one and one they are inactive in, and ApproveProjects everywhere.
func world(t *testing.T) (db.DB, *Book) {
	t.Helper()
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { _ = database.Close() })
	exec(t, database,
		`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at)
		 VALUES ('`+user.String()+`', 'manager', 'Manager', '', '', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-16T10:00:00.0000000Z')`,
		project(active, 0), project(pending, 2), project(inactive, 0),
		permission(active, "RecordEntries"), permission(pending, "RecordEntries"), permission(inactive, "RecordEntries"),
		`INSERT INTO inactive_members (id, project_id, user_id) VALUES ('`+uuid.NewString()+`', '`+inactive.String()+`', '`+user.String()+`')`,
		`INSERT INTO system_permissions (id, user_id, permission) VALUES ('`+uuid.NewString()+`', '`+user.String()+`', 'ApproveProjects')`)
	return database, NewBook(database)
}

func project(id uuid.UUID, status int) string {
	return `INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template) VALUES ('` + id.String() + `', 'P ` + id.String()[34:] + `', '2026-11-01', ` + string(rune('0'+status)) + `, '', '', 0)`
}

func permission(projectID uuid.UUID, name string) string {
	return `INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES ('` + uuid.NewString() + `', '` + projectID.String() + `', '` + user.String() + `', '` + name + `')`
}

func exec(t *testing.T, database db.DB, statements ...string) {
	t.Helper()
	for _, statement := range statements {
		if err := database.Exec(context.Background(), statement); err != nil {
			t.Fatal(err)
		}
	}
}

func TestBR33_APermissionCountsOnlyInTheProjectItWasTickedIn(t *testing.T) {
	_, book := world(t)
	access, err := book.Of(context.Background(), user)
	if err != nil {
		t.Fatal(err)
	}
	if !access.Holds(active, RecordEntries) || access.Holds(uuid.New(), RecordEntries) || access.Holds(active, ApproveEntries) {
		t.Errorf("%+v", access)
	}
}

func TestBR34_NoPermissionCountsInAProjectNotYetApproved(t *testing.T) {
	_, book := world(t)
	access, _ := book.Of(context.Background(), user)
	if access.Holds(pending, RecordEntries) {
		t.Error("a pending project's permission counted")
	}
}

func TestIssue126D1_SomeoneInactiveInAProjectHoldsNothingThere(t *testing.T) {
	_, book := world(t)
	access, _ := book.Of(context.Background(), user)
	if access.Holds(inactive, RecordEntries) {
		t.Error("an inactive member's permission counted")
	}
}

func TestBR29_ASystemPermissionCountsEverywhere(t *testing.T) {
	_, book := world(t)
	access, _ := book.Of(context.Background(), user)
	if !access.Holds(active, ApproveProjects) || !access.Holds(uuid.Nil, ApproveProjects) {
		t.Error("ApproveProjects did not count everywhere")
	}
}

func TestBR32_AChangeCountsAtTheNextRequestWithNothingToRemember(t *testing.T) {
	database, book := world(t)
	ctx := context.Background()
	if kept, _ := book.Of(ctx, user); kept.Holds(active, ApproveEntries) {
		t.Fatal("held before it was given")
	}
	exec(t, database, permission(active, "ApproveEntries"))
	if fresh, _ := book.Of(ctx, user); !fresh.Holds(active, ApproveEntries) {
		t.Error("the change did not count at the next read")
	}
}

// FR-03: a permission is read by its name in any case, and a name no permission has, or a number, is none.
func TestFR03_APermissionIsReadByItsNameAndNothingElse(t *testing.T) {
	var read []Permission
	if err := json.Unmarshal([]byte(`["manageUsers","ManageUsers"]`), &read); err != nil || !slices.Equal(read, []Permission{ManageUsers, ManageUsers}) {
		t.Errorf("by name: %v %v", read, err)
	}
	for _, refused := range []string{`["flyPlanes"]`, `[13]`} {
		if err := json.Unmarshal([]byte(refused), &read); err == nil {
			t.Errorf("%s is read as %v", refused, read)
		}
	}
}
