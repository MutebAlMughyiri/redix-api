package auth

import (
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/credentials"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
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

// mayReplace says whether newPassword may replace currentPassword: it keeps the policy and is not the current password (PasswordPolicy.MayReplace).
func mayReplace(newPassword, currentPassword, username string) bool {
	return users.KeepsPolicy(newPassword, username) && newPassword != currentPassword
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
