// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package notifications keeps what each person is told (NotificationsEndpoints, redix-api #140).
package notifications

import (
	"context"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Kind is what happened, as the database keeps it (NotificationKind).
type Kind int64

// The kinds of thing a person is told.
const (
	EntrySubmitted Kind = iota
	EntryApproved
	EntryRejected
	ProjectApproved
	ProjectRejected
	// PermissionRequested tells whoever manages a project's users of a request waiting; PermissionRequestApproved and PermissionRequestRejected
	// tell the person who asked (redix-api #112 R6).
	PermissionRequested
	PermissionRequestApproved
	PermissionRequestRejected
)

// TellManagers tells everyone who manages the users of projectID what the admin decided, at at (NotificationsEndpoints.TellManagersAsync).
func TellManagers(ctx context.Context, q *store.Queries, projectID uuid.UUID, kind Kind, at moment.Moment) error {
	managers, err := q.ManagersOf(ctx, projectID)
	if err != nil {
		return err
	}
	for _, userID := range managers {
		if err := q.AddNotification(ctx, store.AddNotificationParams{ID: uuid.Must(uuid.NewV7()), UserID: userID, ProjectID: projectID, Kind: int64(kind), At: at}); err != nil {
			return err
		}
	}
	return nil
}

var kinds = web.Enum{"entrySubmitted", "entryApproved", "entryRejected", "projectApproved", "projectRejected", "permissionRequested",
	"permissionRequestApproved", "permissionRequestRejected"}

// Name is the kind's name, as the API writes it and the app's words key it: notifications.entrySubmitted.
func (k Kind) Name() string { return kinds[k] }

// MarshalJSON writes the kind by its name.
func (k Kind) MarshalJSON() ([]byte, error) { return kinds.Write(int64(k)) }

// View is one thing a person was told, as GET /notifications answers it (UC-036) and the web's Notifications lists it.
type View struct {
	ID          uuid.UUID     `json:"id"`
	Kind        Kind          `json:"kind"`
	ProjectID   uuid.UUID     `json:"projectId"`
	ProjectName string        `json:"projectName"`
	EntryID     uuid.UUID     `json:"entryId"`
	At          moment.Moment `json:"at"`
	IsRead      bool          `json:"isRead"`
}

// Map routes what the signed-in person was told, for them alone (UC-036, FR-20).
func Map(router *web.Router, gate *access.Gate, database db.DB) {
	// What the caller was told, the latest 100, newest first, about projects they still have (UC-036).
	router.Handle("GET", "/notifications", gate.Working(func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		views, err := ListOf(r.Context(), database.Queries(), caller.UserID)
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		web.JSON(w, r, http.StatusOK, views)
	}))
	// Marks one of the caller's notifications as seen (UC-036).
	router.Handle("POST", "/notifications/{id:guid}/read", gate.Working(func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		marked, err := MarkRead(r.Context(), database, web.ID(r, "id"), caller.UserID)
		switch {
		case err != nil:
			problem.Status(w, r, http.StatusInternalServerError)
		case !marked:
			problem.Status(w, r, http.StatusNotFound)
		default:
			w.WriteHeader(http.StatusOK)
		}
	}))
}

// ListOf is what userID was told, the latest 100, newest first, about projects they still have, as GET /notifications answers it: the rule both
// doors call, the app's route and the web's Notifications (redix-api #175). The error is the database's.
func ListOf(ctx context.Context, read *store.Queries, userID uuid.UUID) ([]View, error) {
	rows, err := read.NotificationsOf(ctx, userID)
	if err != nil {
		return nil, err
	}
	var views []View
	for _, row := range rows {
		views = append(views, View{ID: row.ID, Kind: Kind(row.Kind), ProjectID: row.ProjectID, ProjectName: row.ProjectName, EntryID: row.EntryID, At: row.At, IsRead: row.IsRead})
	}
	return views, nil
}

// MarkRead marks notificationID, one of userID's, seen, as POST /notifications/{id}/read does: the rule both doors call, the app's route and the
// web's Notifications (redix-api #175). marked is false for none of theirs; the error is the database's.
func MarkRead(ctx context.Context, database db.DB, notificationID, userID uuid.UUID) (marked bool, err error) {
	err = database.InTx(ctx, func(q *store.Queries) error {
		count, err := q.MarkRead(ctx, store.MarkReadParams{ID: notificationID, UserID: userID})
		marked = count > 0
		return err
	})
	return marked, err
}
