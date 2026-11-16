package auth

import (
	"encoding/base64"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/google/uuid"
)

// A token made by .NET 10's JsonWebTokenHandler, as AccessTokens.Create makes one, at 10:00 UTC on 16 Nov 2026, with a made-up test key.
const (
	testKey     = "dGVzdC1rZXktZm9yLWdvLWJhY2tlbmQtb25seS0zMmI="
	dotNetToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJyZWRpeCIsImlzcyI6InJlZGl4IiwiZXhwIjoxNzk0ODI0MTAwLCJpYXQiOjE3OTQ4MjMyMDAsIm5iZiI6MTc5NDgyMzIwMCwic3ViIjoiMDE5M2ExYjItMDAwMC03MDAwLTgwMDAtMDAwMDAwMDAwMDAxIiwibmFtZSI6ImRldi1hZG1pbiIsInNpZCI6IjAxOTNhMWIyLTAwMDAtNzAwMC04MDAwLTAwMDAwMDAwMDBkMSIsIm11c3RDaGFuZ2VQYXNzd29yZCI6dHJ1ZX0.iQFkao4koRQTsnStJhzUBn_ESl8CFIMGBAcBgFiz1-M"
)

var issuedAt = time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC)

func tokensAt(t *testing.T, now time.Time) Tokens {
	t.Helper()
	key, err := base64.StdEncoding.DecodeString(testKey)
	if err != nil {
		t.Fatal(err)
	}
	return NewTokens(key, clock.NewFake(now))
}

func TestR5_ATokenDotNetIssuedStillSpeaksForItsSession(t *testing.T) {
	session, err := tokensAt(t, issuedAt.Add(14*time.Minute)).Check(dotNetToken)
	want := Session{UserID: uuid.MustParse("0193a1b2-0000-7000-8000-000000000001"), Username: "dev-admin", DeviceID: uuid.MustParse("0193a1b2-0000-7000-8000-0000000000d1"), MustChangePassword: true}
	if err != nil || session != want {
		t.Errorf("%+v %v", session, err)
	}
}

func TestR5_ATokenIsValidFromItsStartUntilJustBeforeItsEnd(t *testing.T) {
	for offset, valid := range map[time.Duration]bool{-time.Second: false, 0: true, Lifetime - time.Second: true, Lifetime: false} {
		_, err := tokensAt(t, issuedAt.Add(offset)).Check(dotNetToken)
		if (err == nil) != valid {
			t.Errorf("at %v: %v", offset, err)
		}
	}
}

func TestR5_ATokenSignedWithAnotherKeyOrChangedIsRefused(t *testing.T) {
	other := NewTokens([]byte("another-key-another-key-another!!"), clock.NewFake(issuedAt))
	if _, err := other.Check(dotNetToken); err == nil {
		t.Error("another key accepted the token")
	}
	changed := dotNetToken[:len(dotNetToken)-2] + "xx"
	if _, err := tokensAt(t, issuedAt).Check(changed); err == nil {
		t.Error("a changed signature was accepted")
	}
}

func TestR5_ATokenMadeHereLastsFifteenMinutesAndChecks(t *testing.T) {
	tokens := tokensAt(t, issuedAt)
	session := Session{UserID: uuid.New(), Username: "manager", DeviceID: uuid.New()}
	token, expiresAt := tokens.Create(session)
	if !expiresAt.Equal(issuedAt.Add(15 * time.Minute)) {
		t.Errorf("expires %v", expiresAt)
	}
	if got, err := tokens.Check(token); err != nil || got != session {
		t.Errorf("%+v %v", got, err)
	}
}
