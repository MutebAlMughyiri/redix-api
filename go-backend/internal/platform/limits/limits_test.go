package limits

import (
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
)

func TestN15_AKeyAsksItsPermitsAnHourAndThenWaitsForTheNextHour(t *testing.T) {
	fake := clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC))
	window := PerHour(5, fake)
	for i := 1; i <= 5; i++ {
		if !window.Allow("MANAGER") {
			t.Fatalf("ask %d refused", i)
		}
	}
	if window.Allow("MANAGER") {
		t.Error("a sixth ask was allowed")
	}
	if !window.Allow("OTHER") {
		t.Error("another key was refused")
	}
	fake.Advance(59 * time.Minute)
	if window.Allow("MANAGER") {
		t.Error("allowed before the hour ended")
	}
	fake.Advance(time.Minute)
	if !window.Allow("MANAGER") {
		t.Error("refused once the hour ended")
	}
}

func TestN15_PastItsPermitsAnAddressIsAnsweredTooManyRequests(t *testing.T) {
	window := PerHour(1, clock.NewFake(time.Date(2026, 11, 16, 10, 0, 0, 0, time.UTC)))
	handler := window.PerAddress(func(w http.ResponseWriter, r *http.Request) { w.WriteHeader(http.StatusNoContent) })
	codes := []int{}
	for range 2 {
		recorder := httptest.NewRecorder()
		request := httptest.NewRequest(http.MethodPost, "/auth/forgot-password", nil)
		request.RemoteAddr = "192.0.2.7:51000"
		handler(recorder, request)
		codes = append(codes, recorder.Code)
	}
	if codes[0] != 204 || codes[1] != 429 {
		t.Errorf("%v", codes)
	}
}

func TestG14_BehindCaddyEachPersonIsTheAddressCaddySaw(t *testing.T) {
	for _, c := range []struct{ remote, forwarded, want string }{
		{"127.0.0.1:41000", "198.51.100.4", "198.51.100.4"},
		{"127.0.0.1:41000", "203.0.113.9, 198.51.100.4", "198.51.100.4"},
		{"127.0.0.1:41000", "", "127.0.0.1"},
		{"192.0.2.7:51000", "198.51.100.4", "192.0.2.7"},
	} {
		request := httptest.NewRequest("POST", "/projects/register", nil)
		request.RemoteAddr = c.remote
		if c.forwarded != "" {
			request.Header.Set("X-Forwarded-For", c.forwarded)
		}
		if got := clientOf(request); got != c.want {
			t.Errorf("from %s forwarded %q: %s, want %s", c.remote, c.forwarded, got, c.want)
		}
	}
}
