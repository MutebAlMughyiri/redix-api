// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package auth

import (
	"cmp"
	"context"
	"database/sql"
	"errors"
	"net/http"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/google/uuid"
)

// Sessions opens, renews, secures and ends sessions (AuthEndpoints) for both doors, the app's API and the web's pages (redix-api #161 S1):
// sign-in (UC-002), renewal with a device's refresh token, changing the password (UC-003), sign-out (UC-062), and a phone that uses biometrics
// (UC-011).
type Sessions struct {
	database db.DB
	tokens   credentials.Tokens
	book     *access.Book
	clock    clock.Clock
}

// NewSessions is the sessions of database, whose access tokens tokens signs and whose holdings book keeps.
func NewSessions(database db.DB, tokens credentials.Tokens, book *access.Book, clock clock.Clock) Sessions {
	return Sessions{database: database, tokens: tokens, book: book, clock: clock}
}

// Opened is a session a sign-in, a renewal or a password change opened: the user as it left them, their device, and the device's new refresh token.
type Opened struct {
	User         store.User
	Device       store.Device
	RefreshToken string
}

// MapSessions routes POST /auth/sign-in and /auth/refresh, open without a session; /auth/change-password and /auth/sign-out, open to any session;
// and /auth/biometrics, open to a working one.
func MapSessions(router *web.Router, gate *access.Gate, s Sessions) {
	router.Handle("POST", "/auth/sign-in", s.signIn)
	router.Handle("POST", "/auth/refresh", s.refresh)
	router.Handle("POST", "/auth/change-password", gate.SignedIn(s.changePassword))
	router.Handle("POST", "/auth/sign-out", gate.SignedIn(s.signOut))
	router.Handle("POST", "/auth/biometrics", gate.Working(s.setBiometrics))
}

// session is an open session, as every one of these endpoints answers it (Session).
type session struct {
	AccessToken          string              `json:"accessToken"`
	AccessTokenExpiresAt moment.Moment       `json:"accessTokenExpiresAt"`
	RefreshToken         string              `json:"refreshToken"`
	MustChangePassword   bool                `json:"mustChangePassword"`
	Permissions          []access.Permission `json:"permissions"`
	DefaultProjectID     string              `json:"defaultProjectId"`
	UsesBiometrics       bool                `json:"usesBiometrics"`
	UserID               uuid.UUID           `json:"userId"`
	Username             string              `json:"username"`
	DisplayName          string              `json:"displayName"`
	Email                string              `json:"email"`
}

// signInRequest is the body POST /auth/sign-in (UC-002) reads.
type signInRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

// SignIn opens a session for username with password on a new device: the rule signIn answers the app with, which the web's sign-in shares
// (redix-api #161 S1). It is the session, or the refusal signIn describes; the error is the database's.
func (s Sessions) SignIn(ctx context.Context, username, password string) (Opened, string, error) {
	now := moment.Of(s.clock.Now())
	user, err := s.database.Queries().UserNamed(ctx, username)
	if errors.Is(err, sql.ErrNoRows) {
		return Opened{}, invalidCredentials, nil
	}
	if err != nil {
		return Opened{}, "", err
	}
	refusal, left := attempt(user, password, now)
	if refusal != "" {
		return Opened{}, refusal, s.database.InTx(ctx, func(q *store.Queries) error {
			return saveAttempt(ctx, q, user, left, refusedAction(user, left), now)
		})
	}
	opened := Opened{User: left, Device: store.Device{ID: uuid.Must(uuid.NewV7()), UserID: user.ID}}
	opened.RefreshToken = renew(&opened.Device, now)
	return opened, "", s.database.InTx(ctx, func(q *store.Queries) error {
		err := q.AddDevice(ctx, store.AddDeviceParams{ID: opened.Device.ID, UserID: user.ID, RefreshTokenHash: opened.Device.RefreshTokenHash,
			RefreshExpiresAt: opened.Device.RefreshExpiresAt, LastSeenAt: now})
		if err != nil {
			return err
		}
		return saveAttempt(ctx, q, user, left, history.SignIn, now)
	})
}

// signIn opens a session on a new device (FR-01, AC-01.1) and records it in History. A wrong username and a wrong password both answer
// invalidCredentials; a locked account answers accountLocked (AC-01.2), and a refusal is recorded against the user whose name was typed.
func (s Sessions) signIn(w http.ResponseWriter, r *http.Request) {
	var request signInRequest
	if !web.Read(w, r, &request) {
		return
	}
	opened, refusal, err := s.SignIn(r.Context(), request.Username, request.Password)
	s.reply(w, r, opened, refusal, err, http.StatusUnauthorized)
}

// refreshRequest is the body POST /auth/refresh (UC-002) reads.
type refreshRequest struct {
	RefreshToken string `json:"refreshToken"`
}

// Renew renews a session from its device's refresh token: the rule refresh answers the app with, which the web's pages renew their cookie with
// (redix-api #161 S1). It is the session, or sessionExpired; the error is the database's.
func (s Sessions) Renew(ctx context.Context, refreshToken string) (Opened, string, error) {
	now := moment.Of(s.clock.Now())
	read := s.database.Queries()
	device, err := read.DeviceWithRefreshToken(ctx, hashOf(refreshToken))
	if errors.Is(err, sql.ErrNoRows) || (err == nil && !canRefresh(device, now)) {
		return Opened{}, sessionExpired, nil
	}
	if err != nil {
		return Opened{}, "", err
	}
	user, err := read.UserWithID(ctx, device.UserID)
	if err != nil {
		return Opened{}, "", err
	}
	opened := Opened{User: user, Device: device}
	opened.RefreshToken = renew(&opened.Device, now)
	return opened, "", s.database.InTx(ctx, func(q *store.Queries) error { return renewDevice(ctx, q, opened.Device) })
}

// refresh renews a session with its device's refresh token, which it replaces (#126 R6). An unknown, revoked or expired one answers sessionExpired.
func (s Sessions) refresh(w http.ResponseWriter, r *http.Request) {
	var request refreshRequest
	if !web.Read(w, r, &request) {
		return
	}
	opened, refusal, err := s.Renew(r.Context(), request.RefreshToken)
	s.reply(w, r, opened, refusal, err, http.StatusUnauthorized)
}

// changePasswordRequest is the body POST /auth/change-password (UC-003) reads.
type changePasswordRequest struct {
	CurrentPassword string `json:"currentPassword"`
	NewPassword     string `json:"newPassword"`
}

// ReplacePassword replaces the password of session's user: the rule changePassword answers the app with, which the web's change-password form
// shares (redix-api #161 S1). It is the renewed session, or the refusal changePassword describes; the error is the database's.
func (s Sessions) ReplacePassword(ctx context.Context, session credentials.Session, currentPassword, newPassword string) (Opened, string, error) {
	now := moment.Of(s.clock.Now())
	user, device, err := s.signedIn(ctx, session)
	if err != nil {
		return Opened{}, "", err
	}
	refusal, left := attempt(user, currentPassword, now)
	if refusal != "" {
		return Opened{}, refusal, s.database.InTx(ctx, func(q *store.Queries) error {
			return saveAttempt(ctx, q, user, left, refusedAction(user, left), now)
		})
	}
	if !mayReplace(newPassword, currentPassword, user.Username) {
		return Opened{}, weakPassword, nil
	}
	left.PasswordHash = credentials.Hash(newPassword)
	left.MustChangePassword = false
	opened := Opened{User: left, Device: device}
	opened.RefreshToken = renew(&opened.Device, now)
	return opened, "", s.database.InTx(ctx, func(q *store.Queries) error {
		if err := q.RevokeOtherDevices(ctx, store.RevokeOtherDevicesParams{UserID: user.ID, KeptDeviceID: device.ID}); err != nil {
			return err
		}
		if err := renewDevice(ctx, q, opened.Device); err != nil {
			return err
		}
		return saveAttempt(ctx, q, user, left, history.ChangePassword, now)
	})
}

// changePassword replaces the signed-in user's password (UC-003, AC-02.1), records it, and signs out the user's other devices; this one stays signed
// in with a renewed session. A wrong current password answers invalidCredentials and counts toward the lock; a new one the policy refuses, or the
// current one again, answers weakPassword.
func (s Sessions) changePassword(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request changePasswordRequest
	if !web.Read(w, r, &request) {
		return
	}
	opened, refusal, err := s.ReplacePassword(r.Context(), caller.Session, request.CurrentPassword, request.NewPassword)
	s.reply(w, r, opened, refusal, err, http.StatusBadRequest)
}

// SignOut ends the session on session's device, and only that one: the rule signOut answers the app with, which the web's Sign out shares
// (redix-api #161 S1). The error is the database's.
func (s Sessions) SignOut(ctx context.Context, session credentials.Session) error {
	now := moment.Of(s.clock.Now())
	return s.database.InTx(ctx, func(q *store.Queries) error {
		if err := q.RevokeDevice(ctx, session.DeviceID); err != nil {
			return err
		}
		return history.Write(ctx, q, now, session.UserID, history.SignOut, history.Change{Table: "Users", RecordID: session.UserID})
	})
}

// signOut ends the session on this device, and only this one (UC-062), and records it.
func (s Sessions) signOut(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if s.SignOut(r.Context(), caller.Session) != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusNoContent)
}

// biometricsRequest is the body POST /auth/biometrics (UC-011) reads.
type biometricsRequest struct {
	On bool `json:"on"`
}

// setBiometrics turns fingerprint or face on or off for this phone (UC-011, FR-35) and records it. The check runs on the phone; the server only
// remembers it, so the session stops timing out; the token is renewed, so turning it off starts the 12 hours from now.
func (s Sessions) setBiometrics(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request biometricsRequest
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(s.clock.Now())
	user, device, err := s.signedIn(r.Context(), caller.Session)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	device.UsesBiometrics = request.On
	opened := Opened{User: user, Device: device}
	opened.RefreshToken = renew(&opened.Device, now)
	action := history.TurnOffBiometrics
	if request.On {
		action = history.TurnOnBiometrics
	}
	err = s.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := renewDevice(r.Context(), q, opened.Device); err != nil {
			return err
		}
		return history.Write(r.Context(), q, now, user.ID, action, history.Change{Table: "Users", RecordID: user.ID})
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	s.answer(w, r, opened)
}

// signedIn is session's user and its device; the error is the database's, when either cannot be read.
func (s Sessions) signedIn(ctx context.Context, session credentials.Session) (store.User, store.Device, error) {
	read := s.database.Queries()
	user, err := read.UserWithID(ctx, session.UserID)
	device, deviceErr := read.DeviceWithID(ctx, session.DeviceID)
	return user, device, cmp.Or(err, deviceErr)
}

// reply answers the app with opened, or with refusal as refusedStatus; a failed database answers 500.
func (s Sessions) reply(w http.ResponseWriter, r *http.Request, opened Opened, refusal string, err error, refusedStatus int) {
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, refusedStatus, refusal)
		return
	}
	s.answer(w, r, opened)
}

// AccessToken is a new access token for opened, and when it expires.
func (s Sessions) AccessToken(opened Opened) (string, time.Time) {
	return s.tokens.Create(credentials.Session{UserID: opened.User.ID, Username: opened.User.Username, DeviceID: opened.Device.ID,
		MustChangePassword: opened.User.MustChangePassword})
}

// saveAttempt keeps what an attempt left on the user, and its History line.
func saveAttempt(ctx context.Context, q *store.Queries, before, after store.User, action history.Action, now moment.Moment) error {
	err := q.SaveUser(ctx, store.SaveUserParams{
		ID: after.ID, FailedSignIns: after.FailedSignIns, LockedUntil: after.LockedUntil, PasswordHash: after.PasswordHash, MustChangePassword: after.MustChangePassword,
	})
	if err != nil {
		return err
	}
	return history.Write(ctx, q, now, after.ID, action, users.Change(before, after))
}

func renewDevice(ctx context.Context, q *store.Queries, device store.Device) error {
	return q.RenewDevice(ctx, store.RenewDeviceParams{
		ID: device.ID, RefreshTokenHash: device.RefreshTokenHash, RefreshExpiresAt: device.RefreshExpiresAt, LastSeenAt: device.LastSeenAt, UsesBiometrics: device.UsesBiometrics,
	})
}

// answer is opened's session with a new access token (Session.OfAsync). The default project is named only while the user may still read it, so one
// check covers every way of losing it, being made inactive included (AC-37.2, #126).
func (s Sessions) answer(w http.ResponseWriter, r *http.Request, opened Opened) {
	user, device := opened.User, opened.Device
	held, err := s.book.Of(r.Context(), user.ID)
	chosen, chosenErr := s.database.Queries().DefaultProjectOf(r.Context(), user.ID)
	if err != nil || chosenErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	defaultProject := uuid.Nil
	if len(chosen) > 0 {
		defaultProject = chosen[0]
	}
	defaultProjectID := ""
	if held.Holds(defaultProject, access.ReadProject) {
		defaultProjectID = defaultProject.String()
	}
	accessToken, expiresAt := s.AccessToken(opened)
	web.JSON(w, r, http.StatusOK, session{
		AccessToken: accessToken, AccessTokenExpiresAt: moment.Of(expiresAt), RefreshToken: opened.RefreshToken, MustChangePassword: user.MustChangePassword,
		Permissions: held.Everywhere(), DefaultProjectID: defaultProjectID, UsesBiometrics: device.UsesBiometrics,
		UserID: user.ID, Username: user.Username, DisplayName: user.DisplayName, Email: user.Email,
	})
}
