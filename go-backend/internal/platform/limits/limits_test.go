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
