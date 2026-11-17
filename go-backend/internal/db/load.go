package db

import (
	"context"
	"database/sql"
)

// Load runs load in one write transaction whose foreign keys are checked only as it commits (PRAGMA defer_foreign_keys): for the replay of the
// rows .NET's tests wrote straight into their database, a row sometimes before the row it names. The server writes only through InTx.
func (db DB) Load(ctx context.Context, load func(*sql.Tx) error) error {
	return db.inTx(ctx, func(tx *sql.Tx) error {
		if _, err := tx.ExecContext(ctx, "PRAGMA defer_foreign_keys = ON"); err != nil {
			return err
		}
		return load(tx)
	})
}
