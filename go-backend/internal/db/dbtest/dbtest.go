// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package dbtest is the REDIX database failing on purpose, for tests only (redix-api #141 D1): SQLite's own driver, wrapped so that once a
// test arms it, the nth call its database makes - a begin, a prepare, an exec, a query, a ping, a commit - fails, and every branch that answers a
// failed database runs. The server's binary never imports it.
package dbtest

import (
	"context"
	"database/sql"
	"database/sql/driver"
	"errors"
	"strings"
	"sync"
	"sync/atomic"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
)

// Driver is the name the failing driver is registered under.
const Driver = "sqlite-failing"

// ErrFailed is what the armed call answers.
var ErrFailed = errors.New("dbtest: the database failed on purpose")

// Failure counts the calls one database file's connections make, and fails the one it is armed for.
type Failure struct {
	calls, failAt atomic.Int64
	untilCommit   atomic.Bool
	failed        atomic.Bool
	// fresh commits an empty write before each armed request, so whatever keeps a copy of the database - the access book - is forgotten and
	// read again: every round of a request then makes the same calls in the same order, and each of them fails once (redix-api #76 row 277).
	fresh func()
}

// At fails the nth call from now, and only that one; 0 fails none.
func (f *Failure) At(n int64) { f.arm(n, false) }

// BeforeCommit fails the nth call from now unless a write commits first: a change already committed is never answered as a failure, so a
// request that failed changed nothing, and the next one finds the database as it was.
func (f *Failure) BeforeCommit(n int64) { f.arm(n, true) }

// Failed is whether the armed call came, and failed.
func (f *Failure) Failed() bool { return f.failed.Load() }

func (f *Failure) arm(n int64, untilCommit bool) {
	f.failAt.Store(0)
	if n > 0 {
		f.fresh()
	}
	f.calls.Store(0)
	f.failed.Store(false)
	f.untilCommit.Store(untilCommit)
	f.failAt.Store(n)
}

func (f *Failure) next() error {
	if at := f.failAt.Load(); at > 0 && f.calls.Add(1) == at {
		f.failed.Store(true)
		return ErrFailed
	}
	return nil
}

func (f *Failure) committed() {
	if f.untilCommit.Load() {
		f.failAt.Store(0)
	}
}

var (
	watchedMu sync.Mutex
	watched   = map[string]*Failure{}
)

func init() {
	// The driver SQLite's package registers as "sqlite", so the collations REDIX registers on it (db.Names) hold on these connections too. A
	// handle on a registered driver always opens and holds no connection; it is closed once its driver is taken.
	sqlite, _ := sql.Open("sqlite", "")
	sql.Register(Driver, failing{sqlite.Driver()})
	_ = sqlite.Close()
}

// Watch is the Failure of the database file at path, for as long as t runs: every connection the failing driver opens on it counts its calls
// there.
func Watch(t testing.TB, path string) *Failure {
	failure := &Failure{fresh: func() {}}
	watchedMu.Lock()
	watched[path] = failure
	watchedMu.Unlock()
	t.Cleanup(func() {
		watchedMu.Lock()
		delete(watched, path)
		watchedMu.Unlock()
	})
	return failure
}

// Open is the database at path, opened through the failing driver, and its Failure, which fails nothing until it is armed.
func Open(t testing.TB, path string) (db.DB, *Failure) {
	t.Helper()
	failure := Watch(t, path)
	database, err := db.OpenWith(context.Background(), Driver, path)
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { _ = database.Close() })
	failure.fresh = func() { _ = database.InSQL(context.Background(), func(*sql.Tx, *store.Queries) error { return nil }) }
	return database, failure
}

// failing is SQLite's driver, its connections counted for the file Watch names.
type failing struct{ sqlite driver.Driver }

func (d failing) Open(name string) (driver.Conn, error) {
	inner, err := d.sqlite.Open(name)
	if err != nil {
		return nil, err
	}
	path, _, _ := strings.Cut(strings.TrimPrefix(name, "file:"), "?")
	watchedMu.Lock()
	failure, isWatched := watched[path]
	watchedMu.Unlock()
	if !isWatched {
		return inner, nil
	}
	return conn{inner: inner, failure: failure}, nil
}

// conn keeps every optional interface SQLite's connection has, as database/sql otherwise falls back to the context-free calls.
type conn struct {
	inner   driver.Conn
	failure *Failure
}

func (c conn) Prepare(query string) (driver.Stmt, error) {
	return c.PrepareContext(context.Background(), query)
}

func (c conn) PrepareContext(ctx context.Context, query string) (driver.Stmt, error) {
	if err := c.failure.next(); err != nil {
		return nil, err
	}
	inner, err := c.inner.(driver.ConnPrepareContext).PrepareContext(ctx, query)
	if err != nil {
		return nil, err
	}
	return stmt{inner: inner, failure: c.failure}, nil
}

func (c conn) Close() error { return c.inner.Close() }

func (c conn) Begin() (driver.Tx, error) { return c.BeginTx(context.Background(), driver.TxOptions{}) }

func (c conn) BeginTx(ctx context.Context, opts driver.TxOptions) (driver.Tx, error) {
	if err := c.failure.next(); err != nil {
		return nil, err
	}
	inner, err := c.inner.(driver.ConnBeginTx).BeginTx(ctx, opts)
	if err != nil {
		return nil, err
	}
	return tx{inner: inner, failure: c.failure}, nil
}

func (c conn) ExecContext(ctx context.Context, query string, args []driver.NamedValue) (driver.Result, error) {
	if err := c.failure.next(); err != nil {
		return nil, err
	}
	return c.inner.(driver.ExecerContext).ExecContext(ctx, query, args)
}

func (c conn) QueryContext(ctx context.Context, query string, args []driver.NamedValue) (driver.Rows, error) {
	if err := c.failure.next(); err != nil {
		return nil, err
	}
	return c.inner.(driver.QueryerContext).QueryContext(ctx, query, args)
}

func (c conn) Ping(ctx context.Context) error {
	if err := c.failure.next(); err != nil {
		return err
	}
	return c.inner.(driver.Pinger).Ping(ctx)
}

func (c conn) ResetSession(ctx context.Context) error {
	return c.inner.(driver.SessionResetter).ResetSession(ctx)
}

func (c conn) IsValid() bool { return c.inner.(driver.Validator).IsValid() }

type stmt struct {
	inner   driver.Stmt
	failure *Failure
}

func (s stmt) Close() error  { return s.inner.Close() }
func (s stmt) NumInput() int { return s.inner.NumInput() }

// Exec and Query are what driver.Stmt requires; database/sql calls ExecContext and QueryContext instead on a statement that has them, so these
// only pass to those, never to the inner statement's deprecated forms.
func (s stmt) Exec(args []driver.Value) (driver.Result, error) {
	return s.ExecContext(context.Background(), named(args))
}

func (s stmt) Query(args []driver.Value) (driver.Rows, error) {
	return s.QueryContext(context.Background(), named(args))
}

// named is args as the Context forms take them, numbered from 1.
func named(args []driver.Value) []driver.NamedValue {
	values := make([]driver.NamedValue, len(args))
	for index, value := range args {
		values[index] = driver.NamedValue{Ordinal: index + 1, Value: value}
	}
	return values
}

func (s stmt) ExecContext(ctx context.Context, args []driver.NamedValue) (driver.Result, error) {
	if err := s.failure.next(); err != nil {
		return nil, err
	}
	return s.inner.(driver.StmtExecContext).ExecContext(ctx, args)
}

func (s stmt) QueryContext(ctx context.Context, args []driver.NamedValue) (driver.Rows, error) {
	if err := s.failure.next(); err != nil {
		return nil, err
	}
	return s.inner.(driver.StmtQueryContext).QueryContext(ctx, args)
}

type tx struct {
	inner   driver.Tx
	failure *Failure
}

// Commit fails as any call does, rolling the transaction back first, so its connection is left with none open.
func (t tx) Commit() error {
	if err := t.failure.next(); err != nil {
		_ = t.inner.Rollback()
		return err
	}
	if err := t.inner.Commit(); err != nil {
		return err
	}
	t.failure.committed()
	return nil
}

func (t tx) Rollback() error { return t.inner.Rollback() }
