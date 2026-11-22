// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package auth

import (
	"context"
	"crypto/rand"
	"database/sql"
	"errors"
	"fmt"
	"log/slog"
	"math/big"
	"net/http"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/email"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/google/uuid"
)

// What a code is for (EmailCodePurpose).
const (
	resetPassword int64 = 0
	confirmEmail  int64 = 1
)

const (
	// codeLifetime is how long an emailed code may be typed (EmailCode.Lifetime).
	codeLifetime = 15 * time.Minute
	// triesAllowed is how many times a code may be typed, right or wrong (AC-02.8).
	triesAllowed = 5
	// asksPerLoginAnHour is how often one login may ask for a reset code, whatever the address (EmailCode.AsksPerLoginAnHour).
	asksPerLoginAnHour = 5
)

// Codes emails six-digit codes to reset a forgotten password (UC-002, AC-02.8) and to confirm a new email (redix-api #127, AC-10.4).
type Codes struct {
	database db.DB
	clock    clock.Clock
	sender   email.Sender
	perLogin *limits.Window
	log      *slog.Logger
}

// NewCodes is the codes of database, emailed through sender and their failures logged to log; one login asks for a reset code at most five
// times an hour, whichever door it asks at (redix-api #161).
func NewCodes(database db.DB, clock clock.Clock, sender email.Sender, log *slog.Logger) Codes {
	return Codes{database: database, clock: clock, sender: sender, perLogin: limits.PerHour(asksPerLoginAnHour, clock), log: log}
}

// MapCodes routes POST /auth/forgot-password, limited per address and per login, and /auth/reset-with-code, both open with no session; and
// POST /me/email, limited per address, and /me/email/confirm, for a working session. perAddress allows REDIX_EmailCodes__PermitsPerHour an hour.
func MapCodes(router *web.Router, gate *access.Gate, c Codes, perAddress *limits.Window) {
	router.Handle("POST", "/auth/forgot-password", perAddress.PerAddress(c.forgotPassword))
	router.Handle("POST", "/auth/reset-with-code", c.resetWithCode)
	// Sends a six-digit code to the new address, in the language asked; the email changes only once the code confirms it (UC-010).
	router.Handle("POST", "/me/email", gate.Working(func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		perAddress.PerAddress(func(w http.ResponseWriter, r *http.Request) { c.askToConfirm(w, r, caller) })(w, r)
	}))
	router.Handle("POST", "/me/email/confirm", gate.Working(c.confirm))
}

// forgotPasswordRequest is the body POST /auth/forgot-password (UC-002) reads.
type forgotPasswordRequest struct {
	Login string `json:"login"`
}

// forgotPassword answers 202 at once, whoever the login names, so the answer says nothing about which accounts exist; then, when it names one with
// an email, it emails a reset code in the asker's language. One login asks at most five times an hour.
func (c Codes) forgotPassword(w http.ResponseWriter, r *http.Request) {
	var request forgotPasswordRequest
	if !web.Read(w, r, &request) {
		return
	}
	if !c.MayAskReset(request.Login) {
		problem.Status(w, r, http.StatusTooManyRequests)
		return
	}
	w.WriteHeader(http.StatusAccepted)
	_ = http.NewResponseController(w).Flush()
	c.SendResetCode(context.WithoutCancel(r.Context()), request.Login, web.Language(r))
}

// MayAskReset counts an ask for a reset code by login, and says whether it is within the five one login may ask an hour, whatever the address
// (EmailCode.AsksPerLoginAnHour): the limit both doors share (redix-api #161).
func (c Codes) MayAskReset(login string) bool {
	return c.perLogin.Allow(strings.ToUpper(strings.TrimSpace(login)))
}

// SendResetCode emails a reset code in language to the account login names, when it names one with an email: the rule forgotPassword answers the
// app with, which the web's Forgot password shares (redix-api #161). It answers nothing either way, so no answer says which accounts exist; a
// failure is logged.
func (c Codes) SendResetCode(ctx context.Context, login, language string) {
	user, found, err := c.accountOf(ctx, login)
	if err != nil {
		c.log.Error("a reset code was not sent", "error", err)
		return
	}
	if !found || user.Email == "" {
		return
	}
	code, err := c.make(ctx, user.ID, resetPassword, user.Email)
	if err == nil {
		err = email.Send(ctx, c.database, c.sender, user.Email, language, "email.resetSubject", "email.resetBody", code)
	}
	if err != nil {
		c.log.Error("a reset code was not sent", "userId", user.ID, "error", err)
	}
}

// resetWithCodeRequest is the body POST /auth/reset-with-code (UC-002) reads.
type resetWithCodeRequest struct {
	Login       string `json:"login"`
	Code        string `json:"code"`
	NewPassword string `json:"newPassword"`
}

// resetWithCode sets a new password with the code the email brought (AC-02.8): a wrong, used, expired or worn-out code answers codeWrong, a password
// the policy refuses weakPassword. Every device of the account is signed out, the lock lifted, and the reset recorded.
func (c Codes) resetWithCode(w http.ResponseWriter, r *http.Request) {
	var request resetWithCodeRequest
	if !web.Read(w, r, &request) {
		return
	}
	refusal, err := c.ResetWithCode(r.Context(), request.Login, request.Code, request.NewPassword)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
		return
	}
	w.WriteHeader(http.StatusNoContent)
}

// ResetWithCode sets newPassword on the account login names, with the code the email brought: the rule resetWithCode answers the app with, which
// the web's Forgot password shares (redix-api #161). It refuses codeWrong or weakPassword as resetWithCode describes; the error is the database's.
func (c Codes) ResetWithCode(ctx context.Context, login, typed, newPassword string) (string, error) {
	now := moment.Of(c.clock.Now())
	user, _, err := c.accountOf(ctx, login)
	if err != nil {
		return "", err
	}
	code, ok, err := c.check(ctx, user.ID, resetPassword, strings.TrimSpace(typed), now)
	if err != nil {
		return "", err
	}
	if !ok {
		return codeWrong, nil
	}
	if !users.KeepsPolicy(newPassword, user.Username) {
		return weakPassword, nil
	}
	return "", c.database.InTx(ctx, func(q *store.Queries) error {
		if err := q.SpendCode(ctx, store.SpendCodeParams{ID: code.ID, UsedAt: now}); err != nil {
			return err
		}
		reset, err := users.ResetPassword(ctx, q, user, newPassword, false)
		if err != nil {
			return err
		}
		return history.Write(ctx, q, now, user.ID, history.ResetPassword, users.Change(user, reset))
	})
}

// changeEmailRequest is the body POST /me/email reads (UC-010).
type changeEmailRequest struct {
	Email string `json:"email"`
}

// askToConfirm emails a code to the new email the signed-in user typed (#127): an email that is not one answers emailInvalid (400), one another
// account holds emailTaken (409).
func (c Codes) askToConfirm(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request changeEmailRequest
	if !web.Read(w, r, &request) {
		return
	}
	err := c.AskToConfirmEmail(r.Context(), caller.UserID, request.Email, web.Language(r))
	if !changes.Answered(w, r, err) {
		w.WriteHeader(http.StatusAccepted)
	}
}

// AskToConfirmEmail emails userID a code, in language, to confirm typed as their email, as askToConfirm does: the rule both doors call, the
// app's route and the web's My email (redix-api #175). Refused as users.RefusalOfEmail refuses; a code that could not be sent is logged.
func (c Codes) AskToConfirmEmail(ctx context.Context, userID uuid.UUID, typed, language string) error {
	address := strings.TrimSpace(typed)
	status, refusal, err := users.RefusalOfEmail(ctx, c.database.Queries(), address, userID)
	if err != nil {
		return err
	}
	if refusal != "" {
		return changes.Refuse(status, refusal)
	}
	code, err := c.make(ctx, userID, confirmEmail, address)
	if err != nil {
		return err
	}
	if err := email.Send(ctx, c.database, c.sender, address, language, "email.confirmSubject", "email.confirmBody", code); err != nil {
		c.log.Error("a confirmation code was not sent", "userId", userID, "error", err)
	}
	return nil
}

// confirmEmailRequest is the body POST /me/email/confirm (UC-010) reads.
type confirmEmailRequest struct {
	Code string `json:"code"`
}

// confirm makes the email the code was sent to the user's (AC-10.4), unless another account took it meanwhile; a changed email ends every code the
// user still holds.
func (c Codes) confirm(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request confirmEmailRequest
	if !web.Read(w, r, &request) {
		return
	}
	err := c.ConfirmEmail(r.Context(), caller.UserID, request.Code)
	if !changes.Answered(w, r, err) {
		w.WriteHeader(http.StatusNoContent)
	}
}

// ConfirmEmail makes the email the code typed was sent to userID's, as confirm does: the rule both doors call, the app's route and the web's My
// email (redix-api #175). codeWrong (400) for a code that is not the latest, unused and unexpired with a try left; refused as
// users.RefusalOfEmail refuses when another account took the email meanwhile.
func (c Codes) ConfirmEmail(ctx context.Context, userID uuid.UUID, typed string) error {
	now := moment.Of(c.clock.Now())
	user, err := c.database.Queries().UserWithID(ctx, userID)
	if err != nil {
		return err
	}
	code, ok, err := c.check(ctx, user.ID, confirmEmail, strings.TrimSpace(typed), now)
	if err != nil {
		return err
	}
	if !ok {
		return changes.Refuse(http.StatusBadRequest, codeWrong)
	}
	status, refusal, err := users.RefusalOfEmail(ctx, c.database.Queries(), code.Email, user.ID)
	if err != nil {
		return err
	}
	if refusal != "" {
		return changes.Refuse(status, refusal)
	}
	return c.database.InTx(ctx, func(q *store.Queries) error {
		if err := q.SpendCode(ctx, store.SpendCodeParams{ID: code.ID, UsedAt: now}); err != nil {
			return err
		}
		changed, err := users.ChangeEmail(ctx, q, user, code.Email)
		if err != nil {
			return err
		}
		return history.Write(ctx, q, now, user.ID, history.Update, users.Change(user, changed))
	})
}

// accountOf is the account login names: the one with that username, or else the one with that email (AccountOfAsync).
func (c Codes) accountOf(ctx context.Context, login string) (store.User, bool, error) {
	typed := strings.TrimSpace(login)
	read := c.database.Queries()
	user, err := read.UserNamed(ctx, typed)
	if errors.Is(err, sql.ErrNoRows) {
		user, err = read.UserWithEmail(ctx, typed)
	}
	if errors.Is(err, sql.ErrNoRows) {
		return store.User{}, false, nil
	}
	return user, err == nil, err
}

// make replaces the user's earlier codes for purpose with a new six-digit one for address, keeping only its hash, and returns it (EmailCode.MakeAsync).
func (c Codes) make(ctx context.Context, userID uuid.UUID, purpose int64, address string) (string, error) {
	number, err := rand.Int(rand.Reader, big.NewInt(1_000_000))
	if err != nil {
		return "", err
	}
	code := fmt.Sprintf("%06d", number.Int64())
	expiresAt := moment.Of(c.clock.Now().Add(codeLifetime))
	err = c.database.InTx(ctx, func(q *store.Queries) error {
		if err := q.DeleteCodes(ctx, store.DeleteCodesParams{UserID: userID, Purpose: purpose}); err != nil {
			return err
		}
		return q.AddCode(ctx, store.AddCodeParams{ID: uuid.Must(uuid.NewV7()), UserID: userID, Purpose: purpose, Email: address, CodeHash: credentials.Hash(code), ExpiresAt: expiresAt})
	})
	return code, err
}

// check is the user's latest code for purpose when typed is it, unused, unexpired and with a try left; each check counts a try, right or wrong
// (EmailCode.CheckAsync). A database that fails is its error, never a wrong code.
func (c Codes) check(ctx context.Context, userID uuid.UUID, purpose int64, typed string, now moment.Moment) (store.EmailCode, bool, error) {
	last, err := c.database.Queries().LastCode(ctx, store.LastCodeParams{UserID: userID, Purpose: purpose})
	if err != nil || len(last) == 0 {
		return store.EmailCode{}, false, err
	}
	code := last[0]
	if !code.UsedAt.Equal(moment.Never.Time) || !code.ExpiresAt.After(now.Time) || code.Tries >= triesAllowed {
		return store.EmailCode{}, false, nil
	}
	var counted int64
	err = c.database.InTx(ctx, func(q *store.Queries) error {
		var err error
		counted, err = q.CountTry(ctx, store.CountTryParams{ID: code.ID, TriesAllowed: triesAllowed})
		return err
	})
	if err != nil || counted == 0 || credentials.Check(code.CodeHash, typed) == credentials.Wrong {
		return store.EmailCode{}, false, err
	}
	return code, true, nil
}
