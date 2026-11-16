package auth

import (
	"errors"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/golang-jwt/jwt/v5"
	"github.com/google/uuid"
)

// Lifetime is how long an access token is valid; the device's refresh token renews it (redix-api #126, R6).
const Lifetime = 15 * time.Minute

// issuer is the issuer and audience of every REDIX token.
const issuer = "redix"

// Session is who an access token speaks for: the user, the device the session is open on, and whether the password must be changed first.
// Nothing about permissions is in it: they are read on each request, so a change counts at once.
type Session struct {
	UserID             uuid.UUID
	Username           string
	DeviceID           uuid.UUID
	MustChangePassword bool
}

// Tokens issues the access tokens an app sends with each request, and checks them, with the key in REDIX_Jwt__SigningKey; tokens .NET issued
// with the same key are accepted, so a signed-in phone stays signed in (R5).
type Tokens struct {
	key   []byte
	clock clock.Clock
}

// NewTokens signs and checks with key, on clock's time.
func NewTokens(key []byte, clock clock.Clock) Tokens { return Tokens{key: key, clock: clock} }

// claims is what a token carries, under the names .NET gave them.
type claims struct {
	jwt.RegisteredClaims
	Name               string `json:"name"`
	DeviceID           string `json:"sid"`
	MustChangePassword bool   `json:"mustChangePassword"`
}

// Create is a signed access token for session, and the moment it expires.
func (t Tokens) Create(session Session) (string, time.Time) {
	now := t.clock.Now()
	expiresAt := now.Add(Lifetime)
	token, _ := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		"aud":                issuer,
		"iss":                issuer,
		"exp":                expiresAt.Unix(),
		"iat":                now.Unix(),
		"nbf":                now.Unix(),
		"sub":                session.UserID.String(),
		"name":               session.Username,
		"sid":                session.DeviceID.String(),
		"mustChangePassword": session.MustChangePassword,
	}).SignedString(t.key)
	return token, expiresAt
}

// ErrNotValid says a token is not one this API signed, or is not valid now.
var ErrNotValid = errors.New("auth: the access token is not valid")

// Check is the session a token speaks for, when this key signed it for this issuer and audience and it is valid now: not before its start and
// before its end, with no leeway, as AccessTokens.ValidationParameters checked it.
func (t Tokens) Check(token string) (Session, error) {
	var read claims
	_, err := jwt.ParseWithClaims(token, &read, func(*jwt.Token) (any, error) { return t.key, nil },
		jwt.WithValidMethods([]string{jwt.SigningMethodHS256.Alg()}), jwt.WithIssuer(issuer), jwt.WithAudience(issuer),
		jwt.WithExpirationRequired(), jwt.WithTimeFunc(t.clock.Now))
	if err != nil {
		return Session{}, ErrNotValid
	}
	userID, userErr := uuid.Parse(read.Subject)
	deviceID, deviceErr := uuid.Parse(read.DeviceID)
	if userErr != nil || deviceErr != nil {
		return Session{}, ErrNotValid
	}
	return Session{UserID: userID, Username: read.Name, DeviceID: deviceID, MustChangePassword: read.MustChangePassword}, nil
}
