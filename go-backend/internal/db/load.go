package db

import (
	"bufio"
	"context"
	"database/sql"
	"strings"
)

// Load runs load in one write transaction whose foreign keys are checked only as it commits (PRAGMA defer_foreign_keys): for cmd/migrate-data,
// which copies every table from SQL Server once, a row sometimes before the row it names. The server writes only through InTx.
func (db DB) Load(ctx context.Context, load func(*sql.Tx) error) error {
	return db.inTx(ctx, func(tx *sql.Tx) error {
		if _, err := tx.ExecContext(ctx, "PRAGMA defer_foreign_keys = ON"); err != nil {
			return err
		}
		return load(tx)
	})
}

// Source is a table of the schema and the SQL Server table it was generated from, schema and name (redix-api #140 R19).
type Source struct {
	Table, Schema, Name string
}

// Sources is every table of the schema with its SQL Server source, in the schema's order, read from the line naming it above each CREATE TABLE.
func Sources() ([]Source, error) {
	schema, err := migrations.ReadFile("migrations/00001_schema.sql")
	if err != nil {
		return nil, err
	}
	var sources []Source
	named := ""
	lines := bufio.NewScanner(strings.NewReader(string(schema)))
	for lines.Scan() {
		line := lines.Text()
		if source, isSource := strings.CutPrefix(line, "-- "); isSource && strings.Count(source, ".") == 1 && !strings.Contains(source, " ") {
			named = source
			continue
		}
		if table, isTable := strings.CutPrefix(line, "CREATE TABLE "); isTable && named != "" {
			schemaName, name, _ := strings.Cut(named, ".")
			sources = append(sources, Source{Table: strings.TrimSuffix(table, " ("), Schema: schemaName, Name: name})
		}
		named = ""
	}
	return sources, lines.Err()
}
