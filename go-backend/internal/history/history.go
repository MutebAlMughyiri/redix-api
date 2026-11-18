// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package history writes the History table: who did what to which record, when, and the values it changed (UC-069). It is append-only: the
// database refuses any change to a line once written (AC-21.1).
package history

import (
	"bytes"
	"context"
	"encoding/json"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/google/uuid"
)

// Action is what was done, as LK_HistoryAction names it.
type Action string

// The actions History records.
const (
	SignIn            Action = "SignIn"
	SignOut           Action = "SignOut"
	SignInFailed      Action = "SignInFailed"
	AccountLocked     Action = "AccountLocked"
	ChangePassword    Action = "ChangePassword"
	Create            Action = "Create"
	Update            Action = "Update"
	ResetPassword     Action = "ResetPassword"
	Disable           Action = "Disable"
	Enable            Action = "Enable"
	AssignProject     Action = "AssignProject"
	RemoveProject     Action = "RemoveProject"
	RegisterProject   Action = "RegisterProject"
	ApproveProject    Action = "ApproveProject"
	Approve           Action = "Approve"
	Reject            Action = "Reject"
	RejectProject     Action = "RejectProject"
	TurnOnBiometrics  Action = "TurnOnBiometrics"
	TurnOffBiometrics Action = "TurnOffBiometrics"
	Submit            Action = "Submit"
	Attach            Action = "Attach"
	CloseProject      Action = "CloseProject"
	Delete            Action = "Delete"
	Restore           Action = "Restore"
	Reverse           Action = "Reverse"
	// Archive and Retrieve are a project archived and retrieved from an archive, one line each on the project (UC-073 D9).
	Archive  Action = "Archive"
	Retrieve Action = "Retrieve"
)

// Change is what one action did to one record: its table and id as .NET named them ("Users"), its project (uuid.Nil for none), and the values it
// changed under the C# property names every history screen reads ("Name", "UserId"). Before is empty for a new record, After for a removed one.
type Change struct {
	Table     string
	RecordID  uuid.UUID
	ProjectID uuid.UUID
	Before    map[string]any
	After     map[string]any
}

// Write adds one History line with q, inside the write it records, under the trace id of the request that made it (UC-069 R7), so the lines one
// request wrote are read back as one action.
func Write(ctx context.Context, q *store.Queries, at moment.Moment, userID uuid.UUID, action Action, change Change) error {
	before, err := json.Marshal(orEmpty(change.Before))
	if err != nil {
		return err
	}
	after, err := json.Marshal(orEmpty(change.After))
	if err != nil {
		return err
	}
	return q.AddHistory(ctx, store.AddHistoryParams{
		At: at, ProjectID: change.ProjectID, UserID: userID, Action: string(action),
		TableName: change.Table, RecordID: change.RecordID, Before: string(before), After: string(after), TraceID: problem.TraceOf(ctx),
	})
}

// Changed keeps of before and after only the values that differ, as HistoryRecord kept only the properties a save modified. A new record passes
// its values as After alone, and a removed one as Before alone, as HistoryRecord wrote them.
func Changed(before, after map[string]any) (map[string]any, map[string]any) {
	was, is := map[string]any{}, map[string]any{}
	for name, value := range after {
		old, _ := json.Marshal(before[name])
		now, _ := json.Marshal(value)
		if !bytes.Equal(old, now) {
			was[name], is[name] = before[name], value
		}
	}
	return was, is
}

func orEmpty(values map[string]any) map[string]any {
	if values == nil {
		return map[string]any{}
	}
	return values
}
