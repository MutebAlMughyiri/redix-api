// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package db

import (
	"strings"
	"testing"

	"github.com/google/uuid"
)

// BR-32: the database itself refuses a permission the code does not know, and the admin's ApproveProjects ticked in a project, so neither can be
// stored by any write; a permission the code knows is kept.
func TestBR32_TheDatabaseRefusesAPermissionTheCodeDoesNotKnowAndTheAdminsInAProject(t *testing.T) {
	db := open(t)
	user, project := uuid.NewString(), uuid.NewString()
	for _, statement := range []string{
		`INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
			VALUES ('` + user + `', 'manager', 'x', '', 'x', 0, 0, '1900-01-01T00:00:00.0000000Z', '2026-11-01T10:00:00.0000000Z', '')`,
		`INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template, last_entry_number)
			VALUES ('` + project + `', 'مشروع A', '2026-11-01', 1, '', '', 0, 0)`,
	} {
		if _, err := db.write.Exec(statement); err != nil {
			t.Fatal(err)
		}
	}
	give := func(permission string) error {
		_, err := db.write.Exec(`INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?)`, uuid.NewString(), project, user, permission)
		return err
	}
	if err := give("ReadProject"); err != nil {
		t.Fatalf("a permission the code knows: %v", err)
	}
	for _, refused := range []string{"NotAPermission", "ApproveProjects"} {
		if err := give(refused); err == nil || !strings.Contains(err.Error(), "CHECK") {
			t.Errorf("%s in a project: %v, want a CHECK refusal", refused, err)
		}
	}
}
