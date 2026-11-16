package auth

import (
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"strings"
	"time"
	"unicode"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
)

// The refusals a sign-in or a password change answers with.
const (
	invalidCredentials = "invalidCredentials"
	accountLocked      = "accountLocked"
	weakPassword       = "weakPassword"
	sessionExpired     = "sessionExpired"
)

const (
	// wrongPasswordsBeforeLock is how many wrong passwords in a row lock the account (AC-01.2).
	wrongPasswordsBeforeLock = 5
	// lockDuration is how long a locked account stays locked (AC-01.2).
	lockDuration = 15 * time.Minute
	// idleTimeout is how long a refresh token lasts unused on a phone without biometrics (NFR-09).
	idleTimeout = 12 * time.Hour
	// minimumLength is the shortest password the policy keeps (redix-app #113).
	minimumLength = 8
)

// attempt is what typing password does to user (User.SignIn): "" when it signs in, or the refusal, and the user as the attempt leaves it. A locked
// account refuses even the right password; the fifth wrong password in a row locks it for 15 minutes and still answers invalidCredentials; a right
// password kept with a weaker hash is hashed again.
func attempt(user store.User, password string, now moment.Moment) (string, store.User) {
	if user.LockedUntil.After(now.Time) {
		return accountLocked, user
	}
	switch credentials.Check(user.PasswordHash, password) {
	case credentials.Wrong:
		user.FailedSignIns++
		if user.FailedSignIns == wrongPasswordsBeforeLock {
			user.LockedUntil = moment.Of(now.Add(lockDuration))
			user.FailedSignIns = 0
		}
		return invalidCredentials, user
	case credentials.RightButWeak:
		user.PasswordHash = credentials.Hash(password)
	}
	user.FailedSignIns = 0
	return "", user
}

// refusedAction is the History action of a refused attempt: AccountLocked when it locked the account, SignInFailed otherwise.
func refusedAction(before, after store.User) history.Action {
	if !after.LockedUntil.Equal(before.LockedUntil.Time) {
		return history.AccountLocked
	}
	return history.SignInFailed
}

// userChange is what changed on a user, as History keeps it: under the C# property names, never the password hash ([NotInHistory]).
func userChange(before, after store.User) history.Change {
	was, is := history.Changed(visible(before), visible(after))
	return history.Change{Table: "Users", RecordID: before.ID, Before: was, After: is}
}

func visible(user store.User) map[string]any {
	return map[string]any{
		"Username": user.Username, "DisplayName": user.DisplayName, "Mobile": user.Mobile, "Email": user.Email,
		"MustChangePassword": user.MustChangePassword, "FailedSignIns": user.FailedSignIns, "LockedUntil": user.LockedUntil, "CreatedAt": user.CreatedAt,
	}
}

// holds says whether password keeps the policy (PasswordPolicy.Holds): at least eight characters, a letter and a digit, and not the username.
func holds(password, username string) bool {
	hasLetter := strings.IndexFunc(password, unicode.IsLetter) >= 0
	hasDigit := strings.IndexFunc(password, unicode.IsDigit) >= 0
	return utf8.RuneCountInString(password) >= minimumLength && hasLetter && hasDigit && !strings.EqualFold(password, strings.TrimSpace(username))
}

// mayReplace says whether newPassword may replace currentPassword: it keeps the policy and is not the current password (PasswordPolicy.MayReplace).
func mayReplace(newPassword, currentPassword, username string) bool {
	return holds(newPassword, username) && newPassword != currentPassword
}

// renew gives device a new refresh token, which it returns, keeping only its hash, valid for 12 hours from now (Device.RenewRefreshToken).
func renew(device *store.Device, now moment.Moment) string {
	random := make([]byte, 32)
	rand.Read(random)
	token := base64.RawURLEncoding.EncodeToString(random)
	device.RefreshTokenHash = hashOf(token)
	device.RefreshExpiresAt = moment.Of(now.Add(idleTimeout))
	device.LastSeenAt = now
	return token
}

// hashOf is how a refresh token is kept: its SHA-256 in upper-case hex (Device.HashOf).
func hashOf(refreshToken string) string {
	sum := sha256.Sum256([]byte(refreshToken))
	return strings.ToUpper(hex.EncodeToString(sum[:]))
}

// canRefresh says whether device's refresh token still renews a session: not revoked, and unused for under 12 hours unless the phone uses
// biometrics (Device.CanRefresh).
func canRefresh(device store.Device, now moment.Moment) bool {
	return !device.IsRevoked && (device.UsesBiometrics || device.RefreshExpiresAt.After(now.Time))
}
