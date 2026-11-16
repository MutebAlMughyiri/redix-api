// Package clock owns what time it is, so every rule that waits - a token's 15 minutes, a lock, an hourly limit - is tested by moving a clock,
// as FakeTimeProvider moved .NET's TimeProvider (redix-api #140, N17).
package clock

import (
	"sync"
	"time"
)

// Clock says what time it is.
type Clock interface {
	Now() time.Time
}

// System is the computer's clock, in UTC.
type System struct{}

// Now is the computer's time in UTC.
func (System) Now() time.Time { return time.Now().UTC() }

// Fake is a clock that stands still until a test moves it.
type Fake struct {
	mu  sync.Mutex
	now time.Time
}

// NewFake is a clock standing at now.
func NewFake(now time.Time) *Fake { return &Fake{now: now.UTC()} }

// Now is where the clock stands.
func (f *Fake) Now() time.Time {
	f.mu.Lock()
	defer f.mu.Unlock()
	return f.now
}

// Advance moves the clock on by d.
func (f *Fake) Advance(d time.Duration) {
	f.mu.Lock()
	defer f.mu.Unlock()
	f.now = f.now.Add(d)
}
