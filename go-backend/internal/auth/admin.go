// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package auth signs people in and keeps their sessions (UC-002), and makes the first admin (BR-29).
package auth

import (
	"context"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

// SeedAdmin makes the first user, the admin (BR-29), when the database has no user and a username is set: the username from REDIX_Admin__Username,
// the password from the environment's REDIX_{Environment}_Admin__Password (#118), to be changed at first sign-in, and the right to approve projects
// (AdminSeed).
func SeedAdmin(ctx context.Context, database db.DB, username, password string, now moment.Moment) error {
	if username == "" {
		return nil
	}
	return database.InTx(ctx, func(q *store.Queries) error {
		if has, err := q.HasUsers(ctx); err != nil || has {
			return err
		}
		id := uuid.Must(uuid.NewV7())
		err := q.AddUser(ctx, store.AddUserParams{
			ID: id, Username: username, DisplayName: username, PasswordHash: credentials.Hash(password),
			MustChangePassword: true, LockedUntil: moment.Never, CreatedAt: now,
		})
		if err != nil {
			return err
		}
		return q.AddSystemPermission(ctx, store.AddSystemPermissionParams{ID: uuid.Must(uuid.NewV7()), UserID: id, Permission: "ApproveProjects"})
	})
}
