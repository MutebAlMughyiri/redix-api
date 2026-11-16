// Package db owns the REDIX database: its schema, its two connection pools and the one way to write (redix-api #140).
package db

import (
	"context"
	"database/sql"
	"embed"
	"errors"
	"fmt"
	"io/fs"

	"github.com/pressly/goose/v3"
	_ "modernc.org/sqlite"
)

//go:embed migrations/*.sql
var migrations embed.FS

// pragmas hold on every connection (R12).
const pragmas = "_pragma=journal_mode(WAL)&_pragma=busy_timeout(5000)&_pragma=foreign_keys(1)&_pragma=synchronous(NORMAL)"

// readers is the read pool's size on one vCPU (G3).
const readers = 4

// DB is the database's two pools: the one connection that writes, and several that only read (R13).
type DB struct {
	write *sql.DB
	Read  *sql.DB
}

// Open opens the database file at path, brings its schema up to date and returns its pools.
func Open(ctx context.Context, path string) (DB, error) {
	write, err := sql.Open("sqlite", "file:"+path+"?"+pragmas+"&_txlock=immediate")
	if err != nil {
		return DB{}, err
	}
	write.SetMaxOpenConns(1)
	if err := migrate(ctx, write); err != nil {
		write.Close()
		return DB{}, fmt.Errorf("migrating %s: %w", path, err)
	}
	read, err := sql.Open("sqlite", "file:"+path+"?mode=ro&"+pragmas)
	if err != nil {
		write.Close()
		return DB{}, err
	}
	read.SetMaxOpenConns(readers)
	return DB{write: write, Read: read}, nil
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

// InTx runs change in one IMMEDIATE transaction on the write connection and commits it. Nothing inside change may wait on the network or a file (R14).
func (db DB) InTx(ctx context.Context, change func(*sql.Tx) error) error {
	tx, err := db.write.BeginTx(ctx, nil)
	if err != nil {
		return err
	}
	if err := change(tx); err != nil {
		tx.Rollback()
		return err
	}
	return tx.Commit()
}

// Close closes both pools.
func (db DB) Close() error {
	return errors.Join(db.Read.Close(), db.write.Close())
}
