// Package notifications keeps what each person is told (NotificationsEndpoints, redix-api #140).
package notifications

import (
	"context"

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
