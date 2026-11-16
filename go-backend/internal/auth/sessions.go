package auth

import (
	"context"
	"database/sql"
	"errors"
	"net/http"

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

// Sessions opens, renews, secures and ends sessions (AuthEndpoints): sign-in (UC-002), renewal with a device's refresh token, changing the password
// (UC-003), sign-out (UC-062), and a phone that uses biometrics (UC-011).
type Sessions struct {
	database db.DB
	tokens   credentials.Tokens
	book     *access.Book
	clock    clock.Clock
}

// MapSessions routes POST /auth/sign-in and /auth/refresh, open without a session; /auth/change-password and /auth/sign-out, open to any session;
// and /auth/biometrics, open to a working one.
func MapSessions(router *web.Router, gate *access.Gate, database db.DB, tokens credentials.Tokens, book *access.Book, clock clock.Clock) {
	s := Sessions{database: database, tokens: tokens, book: book, clock: clock}
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

// signIn opens a session on a new device (FR-01, AC-01.1) and records it in History. A wrong username and a wrong password both answer
// invalidCredentials; a locked account answers accountLocked (AC-01.2), and a refusal is recorded against the user whose name was typed.
func (s Sessions) signIn(w http.ResponseWriter, r *http.Request) {
	var request struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(s.clock.Now())
	user, err := s.database.Queries().UserNamed(r.Context(), request.Username)
	if errors.Is(err, sql.ErrNoRows) {
		problem.Refuse(w, r, http.StatusUnauthorized, invalidCredentials)
		return
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	refusal, left := attempt(user, request.Password, now)
	if refusal != "" {
		s.write(w, r, func(ctx context.Context, q *store.Queries) error {
			return saveAttempt(ctx, q, user, left, refusedAction(user, left), now)
		}, func() { problem.Refuse(w, r, http.StatusUnauthorized, refusal) })
		return
	}
	device := store.Device{ID: uuid.Must(uuid.NewV7()), UserID: user.ID}
	refreshToken := renew(&device, now)
	s.write(w, r, func(ctx context.Context, q *store.Queries) error {
		err := q.AddDevice(ctx, store.AddDeviceParams{ID: device.ID, UserID: user.ID, RefreshTokenHash: device.RefreshTokenHash, RefreshExpiresAt: device.RefreshExpiresAt, LastSeenAt: now})
		if err != nil {
			return err
		}
		return saveAttempt(ctx, q, user, left, history.SignIn, now)
	}, func() { s.answer(w, r, left, device, refreshToken) })
}

// refresh renews a session with its device's refresh token, which it replaces (#126 R6). An unknown, revoked or expired one answers sessionExpired.
func (s Sessions) refresh(w http.ResponseWriter, r *http.Request) {
	var request struct {
		RefreshToken string `json:"refreshToken"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(s.clock.Now())
	read := s.database.Queries()
	device, err := read.DeviceWithRefreshToken(r.Context(), hashOf(request.RefreshToken))
	if errors.Is(err, sql.ErrNoRows) || (err == nil && !canRefresh(device, now)) {
		problem.Refuse(w, r, http.StatusUnauthorized, sessionExpired)
		return
	}
	user, userErr := read.UserWithID(r.Context(), device.UserID)
	if err != nil || userErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	refreshToken := renew(&device, now)
	s.write(w, r, func(ctx context.Context, q *store.Queries) error { return renewDevice(ctx, q, device) },
		func() { s.answer(w, r, user, device, refreshToken) })
}

// changePassword replaces the signed-in user's password (UC-003, AC-02.1), records it, and signs out the user's other devices; this one stays signed
// in with a renewed session. A wrong current password answers invalidCredentials and counts toward the lock; a new one the policy refuses, or the
// current one again, answers weakPassword.
func (s Sessions) changePassword(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		CurrentPassword string `json:"currentPassword"`
		NewPassword     string `json:"newPassword"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(s.clock.Now())
	read := s.database.Queries()
	user, err := read.UserWithID(r.Context(), caller.UserID)
	device, deviceErr := read.DeviceWithID(r.Context(), caller.DeviceID)
	if err != nil || deviceErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	refusal, left := attempt(user, request.CurrentPassword, now)
	if refusal != "" {
		s.write(w, r, func(ctx context.Context, q *store.Queries) error {
			return saveAttempt(ctx, q, user, left, refusedAction(user, left), now)
		}, func() { problem.Refuse(w, r, http.StatusBadRequest, refusal) })
		return
	}
	if !mayReplace(request.NewPassword, request.CurrentPassword, user.Username) {
		problem.Refuse(w, r, http.StatusBadRequest, weakPassword)
		return
	}
	left.PasswordHash = credentials.Hash(request.NewPassword)
	left.MustChangePassword = false
	refreshToken := renew(&device, now)
	s.write(w, r, func(ctx context.Context, q *store.Queries) error {
		if err := q.RevokeOtherDevices(ctx, store.RevokeOtherDevicesParams{UserID: user.ID, KeptDeviceID: device.ID}); err != nil {
			return err
		}
		if err := renewDevice(ctx, q, device); err != nil {
			return err
		}
		return saveAttempt(ctx, q, user, left, history.ChangePassword, now)
	}, func() { s.answer(w, r, left, device, refreshToken) })
}

// signOut ends the session on this device, and only this one (UC-062), and records it.
func (s Sessions) signOut(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	now := moment.Of(s.clock.Now())
	s.write(w, r, func(ctx context.Context, q *store.Queries) error {
		if err := q.RevokeDevice(ctx, caller.DeviceID); err != nil {
			return err
		}
		return history.Write(ctx, q, now, caller.UserID, history.SignOut, history.Change{Table: "Users", RecordID: caller.UserID})
	}, func() { w.WriteHeader(http.StatusNoContent) })
}

// setBiometrics turns fingerprint or face on or off for this phone (UC-011, FR-35) and records it. The check runs on the phone; the server only
// remembers it, so the session stops timing out; the token is renewed, so turning it off starts the 12 hours from now.
func (s Sessions) setBiometrics(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		On bool `json:"on"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(s.clock.Now())
	read := s.database.Queries()
	user, err := read.UserWithID(r.Context(), caller.UserID)
	device, deviceErr := read.DeviceWithID(r.Context(), caller.DeviceID)
	if err != nil || deviceErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	device.UsesBiometrics = request.On
	refreshToken := renew(&device, now)
	action := history.TurnOffBiometrics
	if request.On {
		action = history.TurnOnBiometrics
	}
	s.write(w, r, func(ctx context.Context, q *store.Queries) error {
		if err := renewDevice(ctx, q, device); err != nil {
			return err
		}
		return history.Write(ctx, q, now, user.ID, action, history.Change{Table: "Users", RecordID: user.ID})
	}, func() { s.answer(w, r, user, device, refreshToken) })
}

// write runs change in one write transaction, then answers with answered; a failed write answers 500.
func (s Sessions) write(w http.ResponseWriter, r *http.Request, change func(context.Context, *store.Queries) error, answered func()) {
	err := s.database.InTx(r.Context(), func(q *store.Queries) error { return change(r.Context(), q) })
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	answered()
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

// answer is the session of user on device with refreshToken and a new access token (Session.OfAsync). The default project is named only while the
// user may still read it, so one check covers every way of losing it, being made inactive included (AC-37.2, #126).
func (s Sessions) answer(w http.ResponseWriter, r *http.Request, user store.User, device store.Device, refreshToken string) {
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
	accessToken, expiresAt := s.tokens.Create(credentials.Session{UserID: user.ID, Username: user.Username, DeviceID: device.ID, MustChangePassword: user.MustChangePassword})
	web.JSON(w, r, http.StatusOK, session{
		AccessToken: accessToken, AccessTokenExpiresAt: moment.Of(expiresAt), RefreshToken: refreshToken, MustChangePassword: user.MustChangePassword,
		Permissions: held.Everywhere(), DefaultProjectID: defaultProjectID, UsesBiometrics: device.UsesBiometrics,
		UserID: user.ID, Username: user.Username, DisplayName: user.DisplayName, Email: user.Email,
	})
}
