// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package notifications keeps what each person is told (NotificationsEndpoints, redix-api #140).
package notifications

import (
	"context"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
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

var kinds = web.Enum{"entrySubmitted", "entryApproved", "entryRejected", "projectApproved", "projectRejected"}

func (k Kind) MarshalJSON() ([]byte, error) { return kinds.Write(int64(k)) }

// notificationView is part of what GET /notifications answers (UC-036).
type notificationView struct {
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
		rows, err := database.Queries().NotificationsOf(r.Context(), caller.UserID)
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		views := []notificationView{}
		for _, row := range rows {
			views = append(views, notificationView{ID: row.ID, Kind: Kind(row.Kind), ProjectID: row.ProjectID, ProjectName: row.ProjectName, EntryID: row.EntryID, At: row.At, IsRead: row.IsRead})
		}
		web.JSON(w, r, http.StatusOK, views)
	}))
	// Marks one of the caller's notifications as seen (UC-036).
	router.Handle("POST", "/notifications/{id:guid}/read", gate.Working(func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		var marked int64
		err := database.InTx(r.Context(), func(q *store.Queries) error {
			var err error
			marked, err = q.MarkRead(r.Context(), store.MarkReadParams{ID: web.ID(r, "id"), UserID: caller.UserID})
			return err
		})
		switch {
		case err != nil:
			problem.Status(w, r, http.StatusInternalServerError)
		case marked == 0:
			problem.Status(w, r, http.StatusNotFound)
		default:
			w.WriteHeader(http.StatusOK)
		}
	}))
}
