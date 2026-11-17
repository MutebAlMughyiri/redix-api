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

// MapCodes routes POST /auth/forgot-password, limited per address and per login, and /auth/reset-with-code, both open with no session; and
// POST /me/email, limited per address, and /me/email/confirm, for a working session. perAddress allows REDIX_EmailCodes__PermitsPerHour an hour.
func MapCodes(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock, sender email.Sender, perAddress *limits.Window, log *slog.Logger) {
	c := Codes{database: database, clock: clock, sender: sender, perLogin: limits.PerHour(asksPerLoginAnHour, clock), log: log}
	router.Handle("POST", "/auth/forgot-password", perAddress.PerAddress(c.forgotPassword))
	router.Handle("POST", "/auth/reset-with-code", c.resetWithCode)
	router.Handle("POST", "/me/email", gate.Working(func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		perAddress.PerAddress(func(w http.ResponseWriter, r *http.Request) { c.askToConfirm(w, r, caller) })(w, r)
	}))
	router.Handle("POST", "/me/email/confirm", gate.Working(c.confirm))
}

// forgotPassword answers 202 at once, whoever the login names, so the answer says nothing about which accounts exist; then, when it names one with
// an email, it emails a reset code in the asker's language. One login asks at most five times an hour.
func (c Codes) forgotPassword(w http.ResponseWriter, r *http.Request) {
	var request struct {
		Login string `json:"login"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	if !c.perLogin.Allow(strings.ToUpper(strings.TrimSpace(request.Login))) {
		problem.Status(w, r, http.StatusTooManyRequests)
		return
	}
	w.WriteHeader(http.StatusAccepted)
	http.NewResponseController(w).Flush()
	ctx := context.WithoutCancel(r.Context())
	user, found, err := c.accountOf(ctx, request.Login)
	if err != nil {
		c.log.Error("a reset code was not sent", "error", err)
		return
	}
	if !found || user.Email == "" {
		return
	}
	code, err := c.make(ctx, user.ID, resetPassword, user.Email)
	if err == nil {
		err = email.Send(ctx, c.database, c.sender, user.Email, web.Language(r), "email.resetSubject", "email.resetBody", code)
	}
	if err != nil {
		c.log.Error("a reset code was not sent", "userId", user.ID, "error", err)
	}
}

// resetWithCode sets a new password with the code the email brought (AC-02.8): a wrong, used, expired or worn-out code answers codeWrong, a password
// the policy refuses weakPassword. Every device of the account is signed out, the lock lifted, and the reset recorded.
func (c Codes) resetWithCode(w http.ResponseWriter, r *http.Request) {
	var request struct {
		Login       string `json:"login"`
		Code        string `json:"code"`
		NewPassword string `json:"newPassword"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(c.clock.Now())
	user, _, err := c.accountOf(r.Context(), request.Login)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	code, ok, err := c.check(r.Context(), user.ID, resetPassword, strings.TrimSpace(request.Code), now)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, "codeWrong")
		return
	}
	if !users.KeepsPolicy(request.NewPassword, user.Username) {
		problem.Refuse(w, r, http.StatusBadRequest, weakPassword)
		return
	}
	err = c.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := q.SpendCode(r.Context(), store.SpendCodeParams{ID: code.ID, UsedAt: now}); err != nil {
			return err
		}
		reset, err := users.ResetPassword(r.Context(), q, user, request.NewPassword, false)
		if err != nil {
			return err
		}
		return history.Write(r.Context(), q, now, user.ID, history.ResetPassword, users.Change(user, reset))
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusNoContent)
}

// askToConfirm emails a code to the new email the signed-in user typed (#127): an email that is not one answers emailInvalid (400), one another
// account holds emailTaken (409).
func (c Codes) askToConfirm(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		Email string `json:"email"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	address := strings.TrimSpace(request.Email)
	if c.refuseEmail(w, r, address, caller.UserID) {
		return
	}
	code, err := c.make(r.Context(), caller.UserID, confirmEmail, address)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if err := email.Send(r.Context(), c.database, c.sender, address, web.Language(r), "email.confirmSubject", "email.confirmBody", code); err != nil {
		c.log.Error("a confirmation code was not sent", "userId", caller.UserID, "error", err)
	}
	w.WriteHeader(http.StatusAccepted)
}

// confirm makes the email the code was sent to the user's (AC-10.4), unless another account took it meanwhile; a changed email ends every code the
// user still holds.
func (c Codes) confirm(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		Code string `json:"code"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	now := moment.Of(c.clock.Now())
	user, err := c.database.Queries().UserWithID(r.Context(), caller.UserID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	code, ok, err := c.check(r.Context(), user.ID, confirmEmail, strings.TrimSpace(request.Code), now)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, "codeWrong")
		return
	}
	if c.refuseEmail(w, r, code.Email, user.ID) {
		return
	}
	err = c.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := q.SpendCode(r.Context(), store.SpendCodeParams{ID: code.ID, UsedAt: now}); err != nil {
			return err
		}
		changed, err := users.ChangeEmail(r.Context(), q, user, code.Email)
		if err != nil {
			return err
		}
		return history.Write(r.Context(), q, now, user.ID, history.Update, users.Change(user, changed))
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusNoContent)
}

// refuseEmail answers the refusal of an email for userID, when there is one (users.RefusalOfEmail).
func (c Codes) refuseEmail(w http.ResponseWriter, r *http.Request, address string, userID uuid.UUID) bool {
	status, code, err := users.RefusalOfEmail(r.Context(), c.database.Queries(), address, userID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return true
	}
	if code != "" {
		problem.Refuse(w, r, status, code)
		return true
	}
	return false
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
