// Package users keeps the people of REDIX: their details and email, their password when someone resets it, and who is in each project with which
// permissions (UsersEndpoints, ProjectUsersEndpoints). The rules about a user live here; signing in asks them too (redix-api #140).
package users

import (
	"context"
	"net/http"
	"net/mail"
	"regexp"
	"strings"
	"unicode"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

const (
	usernameMaxLength     = 50
	nameMaxLength         = 100
	mobileMaxLength       = 20
	emailMaxLength        = 254
	minimumPasswordLength = 8
)

var acceptableMobile = regexp.MustCompile(`^(\+?[0-9]+)?$`)

// KeepsPolicy says whether password keeps the password policy (PasswordPolicy.Holds): at least eight characters, a letter and a digit, and not the
// username whatever its case.
func KeepsPolicy(password, username string) bool {
	hasLetter := strings.IndexFunc(password, unicode.IsLetter) >= 0
	hasDigit := strings.IndexFunc(password, unicode.IsDigit) >= 0
	return utf8.RuneCountInString(password) >= minimumPasswordLength && hasLetter && hasDigit && !strings.EqualFold(password, strings.TrimSpace(username))
}

// RefusalOfDetails is the refusal of a name and a mobile, both already trimmed (User.RefusalOfDetails): invalidName for an empty or too long
// name, invalidMobile for anything but ASCII digits after one leading plus at most, in 20 characters; "" when both may be kept.
func RefusalOfDetails(displayName, mobile string) string {
	if length := utf8.RuneCountInString(displayName); length == 0 || length > nameMaxLength {
		return "invalidName"
	}
	if len(mobile) > mobileMaxLength || !acceptableMobile.MatchString(mobile) {
		return "invalidMobile"
	}
	return ""
}

// refusalOfNew is the refusal of a new user's details, all already trimmed but the password (User.RefusalOfNew).
func refusalOfNew(displayName, username, mobile, password string) string {
	if length := utf8.RuneCountInString(username); length == 0 || length > usernameMaxLength {
		return "invalidUsername"
	}
	if !KeepsPolicy(password, username) {
		return "weakPassword"
	}
	return RefusalOfDetails(displayName, mobile)
}

// RefusalOfEmail is the refusal of email for userID (User.RefusalOfEmailAsync): 400 emailInvalid when it is not an email - an empty one
// included, so every user keeps one - and 409 emailTaken when another account holds it; 0 and "" when it may be theirs.
func RefusalOfEmail(ctx context.Context, read *store.Queries, email string, userID uuid.UUID) (int, string, error) {
	parsed, err := mail.ParseAddress(email)
	if len(email) > emailMaxLength || err != nil || parsed.Address != email {
		return http.StatusBadRequest, "emailInvalid", nil
	}
	taken, err := read.EmailTaken(ctx, store.EmailTakenParams{Email: email, UserID: userID})
	if err != nil || !taken {
		return 0, "", err
	}
	return http.StatusConflict, "emailTaken", nil
}

// ResetPassword gives user password, signing out every device of theirs, lifting the lock and forgetting the wrong passwords, and returns the user
// as it leaves them (User.ResetPasswordAsync). The caller writes the History line.
func ResetPassword(ctx context.Context, q *store.Queries, user store.User, password string, mustChange bool) (store.User, error) {
	if err := q.RevokeEveryDevice(ctx, user.ID); err != nil {
		return user, err
	}
	user.PasswordHash = credentials.Hash(password)
	user.MustChangePassword = mustChange
	user.FailedSignIns = 0
	user.LockedUntil = moment.Never
	return user, q.SetPassword(ctx, store.SetPasswordParams{ID: user.ID, PasswordHash: user.PasswordHash, MustChangePassword: mustChange})
}

// ChangeEmail makes email the user's, ending every code they still hold when it is a new one (User.ChangeEmailAsync), and returns the user as it
// leaves them. The caller writes the History line.
func ChangeEmail(ctx context.Context, q *store.Queries, user store.User, email string) (store.User, error) {
	if email == user.Email {
		return user, nil
	}
	user.Email = email
	if err := q.SetEmail(ctx, store.SetEmailParams{ID: user.ID, Email: email}); err != nil {
		return user, err
	}
	return user, q.DeleteEveryCode(ctx, user.ID)
}

// Change is what changed on a user, as History keeps it: under the C# property names, never the password hash ([NotInHistory]).
func Change(before, after store.User) history.Change {
	was, is := history.Changed(fields(before), fields(after))
	return history.Change{Table: "Users", RecordID: after.ID, Before: was, After: is}
}

// Created is a new user, as History keeps one: every field but the hash.
func Created(user store.User) history.Change {
	return history.Change{Table: "Users", RecordID: user.ID, After: fields(user)}
}

func fields(user store.User) map[string]any {
	return map[string]any{
		"Username": user.Username, "DisplayName": user.DisplayName, "Mobile": user.Mobile, "Email": user.Email,
		"MustChangePassword": user.MustChangePassword, "FailedSignIns": user.FailedSignIns, "LockedUntil": user.LockedUntil, "CreatedAt": user.CreatedAt,
	}
}

// New is a new user as typed - name, username and mobile trimmed, the password hashed - or its refusal: invalidUsername, weakPassword,
// invalidName or invalidMobile (400), or usernameTaken (409) (User.AddNewAsync). Nothing is kept until Add.
func New(ctx context.Context, read *store.Queries, displayName, username, mobile, password string, mustChange bool, now moment.Moment) (store.User, int, string, error) {
	name, login, phone := strings.TrimSpace(displayName), strings.TrimSpace(username), strings.TrimSpace(mobile)
	if refusal := refusalOfNew(name, login, phone, password); refusal != "" {
		return store.User{}, http.StatusBadRequest, refusal, nil
	}
	taken, err := read.UsernameTaken(ctx, login)
	if err != nil {
		return store.User{}, 0, "", err
	}
	if taken {
		return store.User{}, http.StatusConflict, "usernameTaken", nil
	}
	return store.User{ID: uuid.Must(uuid.NewV7()), Username: login, DisplayName: name, Mobile: phone, PasswordHash: credentials.Hash(password),
		MustChangePassword: mustChange, LockedUntil: moment.Never, CreatedAt: now}, 0, "", nil
}

// Add keeps a user New made, recorded as created by actor.
func Add(ctx context.Context, q *store.Queries, user store.User, actor uuid.UUID, now moment.Moment) error {
	err := q.AddUser(ctx, store.AddUserParams{ID: user.ID, Username: user.Username, DisplayName: user.DisplayName, Mobile: user.Mobile,
		PasswordHash: user.PasswordHash, MustChangePassword: user.MustChangePassword, LockedUntil: user.LockedUntil, CreatedAt: user.CreatedAt, Email: user.Email})
	if err != nil {
		return err
	}
	return history.Write(ctx, q, now, actor, history.Create, Created(user))
}
