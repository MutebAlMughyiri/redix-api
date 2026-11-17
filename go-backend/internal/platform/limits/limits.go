// Package limits owns the fixed windows that cap how often one caller may ask - to register, or for an email code - per hour, as ASP.NET Core's
// fixed window rate limiter did (redix-api #140, N15). One process owns them (R3), so they live in memory.
package limits

import (
	"net"
	"net/http"
	"sync"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
)

// Window lets each key ask permits times per length of time, counted from its first ask.
type Window struct {
	mu      sync.Mutex
	permits int
	length  time.Duration
	clock   clock.Clock
	asked   map[string]window
}

type window struct {
	start time.Time
	count int
}

// PerHour is a window of permits an hour.
func PerHour(permits int, clock clock.Clock) *Window {
	return &Window{permits: permits, length: time.Hour, clock: clock, asked: map[string]window{}}
}

// Allow counts one ask by key, and says whether it is within the window's permits.
func (w *Window) Allow(key string) bool {
	w.mu.Lock()
	defer w.mu.Unlock()
	now := w.clock.Now()
	for other, open := range w.asked {
		if !now.Before(open.start.Add(w.length)) {
			delete(w.asked, other)
		}
	}
	open, ok := w.asked[key]
	if !ok {
		open = window{start: now}
	}
	if open.count >= w.permits {
		return false
	}
	open.count++
	w.asked[key] = open
	return true
}

// PerAddress lets a request through within the window for its caller's address, and answers 429 past it, as RequireRateLimiting did.
func (w *Window) PerAddress(next http.HandlerFunc) http.HandlerFunc {
	return func(writer http.ResponseWriter, r *http.Request) {
		if !w.Allow(clientOf(r)) {
			problem.Status(writer, r, http.StatusTooManyRequests)
			return
		}
		next(writer, r)
	}
}

// clientOf is the address a request came from (G14): the connection's own. The API answers the internet itself, with no proxy in front
// (#145 D1), so a forwarded header is anyone's to write and is never read.
func clientOf(r *http.Request) string {
	address, _, err := net.SplitHostPort(r.RemoteAddr)
	if err != nil {
		return r.RemoteAddr
	}
	return address
}
