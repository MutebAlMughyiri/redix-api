package db

import (
	"context"
	"path/filepath"
	"testing"
)

func TestR21_EveryTableOfTheSchemaNamesTheSqlServerTableItIsCopiedFrom(t *testing.T) {
	sources, err := Sources()
	if err != nil {
		t.Fatal(err)
	}
	database, err := Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer database.Close()
	var tables int
	if err := database.write.QueryRow("SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name NOT IN ('goose_db_version', 'sqlite_sequence')").Scan(&tables); err != nil {
		t.Fatal(err)
	}
	if len(sources) != tables || tables != 59 {
		t.Errorf("%d tables name their source, of %d", len(sources), tables)
	}
	for _, source := range sources {
		if (source.Schema != "dbo" && source.Schema != "project") || source.Name == "" || source.Table == "" {
			t.Errorf("%+v names no SQL Server table", source)
		}
	}
}
