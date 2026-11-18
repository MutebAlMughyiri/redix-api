// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package db owns the REDIX database: its schema, its two connection pools, the named queries prepared once on each, and the one way to write
// (redix-api #140).
package db

import (
	"context"
	"database/sql"
	"embed"
	"errors"
	"fmt"
	"io/fs"
	"sync"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/pressly/goose/v3"
	_ "modernc.org/sqlite"
)

//go:embed migrations/*.sql
var migrations embed.FS

// pragmas hold on every connection (R12).
const pragmas = "_pragma=journal_mode(WAL)&_pragma=busy_timeout(5000)&_pragma=foreign_keys(1)&_pragma=synchronous(NORMAL)"

// readers is the read pool's size on one vCPU (G3).
const readers = 4

// DB is the database's two pools - the one connection that writes, and several that only read (R13) - each with every named query prepared on
// it once, so SQLite compiles a query as the server starts rather than on every call: 7.5 µs a query instead of 14.7 (measured 16 Nov 2026).
// Read is the read pool itself, for the tests' own checks.
type DB struct {
	write     *sql.DB
	Read      *sql.DB
	reads     *store.Queries
	writes    *store.Queries
	committed *afterCommit
}

// afterCommit holds what runs after every committed write.
type afterCommit struct {
	mu    sync.Mutex
	calls []func()
}

// Open opens the database file at path, brings its schema up to date and returns its pools.
func Open(ctx context.Context, path string) (DB, error) { return OpenWith(ctx, "sqlite", path) }

// OpenWith is Open through the database/sql driver registered as driverName: SQLite's own for the server, and for tests the failing one that
// wraps it (dbtest). Both pools go through the one driver, so once the write pool's opens, the read pool's does too.
func OpenWith(ctx context.Context, driverName, path string) (DB, error) {
	write, err := sql.Open(driverName, "file:"+path+"?"+pragmas+"&_txlock=immediate")
	if err != nil {
		return DB{}, err
	}
	write.SetMaxOpenConns(1)
	if err := migrate(ctx, write); err != nil {
		write.Close()
		return DB{}, fmt.Errorf("migrating %s: %w", path, err)
	}
	read, _ := sql.Open(driverName, "file:"+path+"?mode=ro&"+pragmas)
	read.SetMaxOpenConns(readers)
	reads, err := store.Prepare(ctx, read)
	if err != nil {
		return DB{}, errors.Join(err, read.Close(), write.Close())
	}
	writes, err := store.Prepare(ctx, write)
	if err != nil {
		return DB{}, errors.Join(err, reads.Close(), read.Close(), write.Close())
	}
	return DB{write: write, Read: read, reads: reads, writes: writes, committed: &afterCommit{}}, nil
}

func migrate(ctx context.Context, write *sql.DB) error {
	scripts, err := fs.Sub(migrations, "migrations")
	if err != nil {
		return err
	}
	provider, err := goose.NewProvider(goose.DialectSQLite3, write, scripts)
	if err != nil {
		return err
	}
	_, err = provider.Up(ctx)
	return err
}

// Queries are the named queries prepared on the read pool, for whatever only reads.
func (db DB) Queries() *store.Queries { return db.reads }

// InTx runs change in one IMMEDIATE transaction on the write connection, with the named queries prepared there, and commits it. Nothing inside
// change may wait on the network or a file (R14).
func (db DB) InTx(ctx context.Context, change func(*store.Queries) error) error {
	return db.inTx(ctx, func(tx *sql.Tx) error { return change(db.writes.WithTx(tx)) })
}

// Exec runs one statement no named query holds, in a transaction of its own: for the tests and the replay's seeds, which put rows straight into
// the database as the .NET factory's helpers did. The server writes only through InTx.
func (db DB) Exec(ctx context.Context, statement string, args ...any) error {
	return db.inTx(ctx, func(tx *sql.Tx) error {
		_, err := tx.ExecContext(ctx, statement, args...)
		return err
	})
}

func (db DB) inTx(ctx context.Context, change func(*sql.Tx) error) error {
	tx, err := db.write.BeginTx(ctx, nil)
	if err != nil {
		return err
	}
	if err := change(tx); err != nil {
		tx.Rollback()
		return err
	}
	if err := tx.Commit(); err != nil {
		return err
	}
	db.committed.mu.Lock()
	calls := db.committed.calls
	db.committed.mu.Unlock()
	for _, call := range calls {
		call()
	}
	return nil
}

// AfterEveryWrite runs call after each committed write, so whatever keeps a copy of the database - the access book - stays true without any
// endpoint having to remember it (AccessChanges).
func (db DB) AfterEveryWrite(call func()) {
	db.committed.mu.Lock()
	db.committed.calls = append(db.committed.calls, call)
	db.committed.mu.Unlock()
}

// Close closes the prepared queries and both pools.
func (db DB) Close() error {
	return errors.Join(db.reads.Close(), db.writes.Close(), db.Read.Close(), db.write.Close())
}
