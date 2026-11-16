// Command migrate-data copies every row of REDIX's SQL Server database into a new SQLite file, once, the day of the switch (redix-api #140 R21,
// P3, P6), and proves nothing moved: every table holds as many rows on both sides, and the figures fingerprint - each project's entries counted
// and summed by kind, status and account - is the same on both (R34). It copies into a file whose schema holds no project yet, in one
// transaction: a count or a figure that differs keeps nothing.
//
//	migrate-data -from "sqlserver://localhost?database=RedixQA" -to /var/lib/redix/redix.db
package main

import (
	"context"
	"database/sql"
	"errors"
	"flag"
	"fmt"
	"log/slog"
	"os"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	_ "github.com/microsoft/go-mssqldb"
)

func main() {
	from := flag.String("from", "", `the SQL Server database, as "sqlserver://host?database=Name"; Windows sign-in when it names no user`)
	to := flag.String("to", "", "the SQLite file to create")
	flag.Parse()
	if *from == "" || *to == "" {
		flag.Usage()
		os.Exit(2)
	}
	if err := run(context.Background(), *from, *to); err != nil {
		slog.Error("nothing was copied", "error", err)
		os.Exit(1)
	}
}

func run(ctx context.Context, from, to string) error {
	source, err := sql.Open("sqlserver", from)
	if err != nil {
		return err
	}
	defer source.Close()
	target, err := db.Open(ctx, to)
	if err != nil {
		return err
	}
	defer target.Close()
	tables, err := db.Sources()
	if err != nil {
		return err
	}
	return target.Load(ctx, func(tx *sql.Tx) error {
		var projects int
		if err := tx.QueryRowContext(ctx, "SELECT count(*) FROM projects WHERE is_template = 0").Scan(&projects); err != nil || projects > 0 {
			return errors.Join(err, errors.New("the SQLite file holds projects already; migrate-data copies into a new one"))
		}
		for _, table := range tables {
			copied, err := copyTable(ctx, source, tx, table)
			if err != nil {
				return fmt.Errorf("%s.%s: %w", table.Schema, table.Name, err)
			}
			slog.Info("copied", "table", table.Table, "rows", copied)
		}
		return sameFigures(ctx, source, tx)
	})
}

// column is a SQLite column with the SQL Server column it is copied from, and that column's type.
type column struct {
	name, source, dataType string
	scale                  int
}

// copyTable replaces the rows the migrations seeded in table's SQLite table with every row of its SQL Server source, each value as the schema
// keeps it (G9, G10, N6), and answers how many it copied, after counting both sides.
func copyTable(ctx context.Context, source *sql.DB, tx *sql.Tx, table db.Source) (int, error) {
	columns, err := columnsOf(ctx, source, tx, table)
	if err != nil {
		return 0, err
	}
	var selected, names, marks []string
	for _, one := range columns {
		selected = append(selected, readAs(one))
		names = append(names, one.name)
		marks = append(marks, "?")
	}
	if _, err := tx.ExecContext(ctx, "DELETE FROM "+table.Table); err != nil {
		return 0, err
	}
	insert, err := tx.PrepareContext(ctx, fmt.Sprintf("INSERT INTO %s (%s) VALUES (%s)", table.Table, strings.Join(names, ", "), strings.Join(marks, ", ")))
	if err != nil {
		return 0, err
	}
	defer insert.Close()
	rows, err := source.QueryContext(ctx, fmt.Sprintf("SELECT %s FROM [%s].[%s]", strings.Join(selected, ", "), table.Schema, table.Name))
	if err != nil {
		return 0, err
	}
	defer rows.Close()
	copied := 0
	values := make([]any, len(columns))
	pointers := make([]any, len(columns))
	for index := range values {
		pointers[index] = &values[index]
	}
	for rows.Next() {
		if err := rows.Scan(pointers...); err != nil {
			return copied, err
		}
		if _, err := insert.ExecContext(ctx, values...); err != nil {
			return copied, err
		}
		copied++
	}
	if err := rows.Err(); err != nil {
		return copied, err
	}
	var held, kept int
	if err := source.QueryRowContext(ctx, fmt.Sprintf("SELECT count(*) FROM [%s].[%s]", table.Schema, table.Name)).Scan(&held); err != nil {
		return copied, err
	}
	if err := tx.QueryRowContext(ctx, "SELECT count(*) FROM "+table.Table).Scan(&kept); err != nil {
		return copied, err
	}
	if held != kept {
		return copied, fmt.Errorf("SQL Server holds %d rows, SQLite %d", held, kept)
	}
	return copied, nil
}

// columnsOf is every column of table's SQLite table with its SQL Server column, matched by name with no underscores and no case - Group became
// group_name (G11). A SQLite column with no source, or a SQL Server column with no place but a rowversion (N5), stops the copy.
func columnsOf(ctx context.Context, source *sql.DB, tx *sql.Tx, table db.Source) ([]column, error) {
	type sourceColumn struct {
		name, dataType string
		scale          int
	}
	rows, err := source.QueryContext(ctx, `SELECT COLUMN_NAME, DATA_TYPE, COALESCE(NUMERIC_SCALE, 0) FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_SCHEMA = @p1 AND TABLE_NAME = @p2 ORDER BY ORDINAL_POSITION`, table.Schema, table.Name)
	if err != nil {
		return nil, err
	}
	var sources []sourceColumn
	for rows.Next() {
		var one sourceColumn
		if err := rows.Scan(&one.name, &one.dataType, &one.scale); err != nil {
			rows.Close()
			return nil, err
		}
		sources = append(sources, one)
	}
	if err := errors.Join(rows.Err(), rows.Close()); err != nil {
		return nil, err
	}
	targets, err := tx.QueryContext(ctx, "SELECT name FROM pragma_table_info(?)", table.Table)
	if err != nil {
		return nil, err
	}
	var columns []column
	used := map[string]bool{}
	for targets.Next() {
		var name string
		if err := targets.Scan(&name); err != nil {
			targets.Close()
			return nil, err
		}
		at := slices.IndexFunc(sources, func(one sourceColumn) bool {
			return plain(one.name) == plain(name) || (name == "group_name" && one.name == "Group")
		})
		if at < 0 {
			targets.Close()
			return nil, fmt.Errorf("the column %s has no source", name)
		}
		used[sources[at].name] = true
		if one := sources[at]; (one.dataType == "decimal" || one.dataType == "numeric") && one.scale > 4 {
			targets.Close()
			return nil, fmt.Errorf("the column %s keeps %d decimals, more than ten-thousandths hold", one.name, one.scale)
		}
		columns = append(columns, column{name: name, source: sources[at].name, dataType: sources[at].dataType, scale: sources[at].scale})
	}
	if err := errors.Join(targets.Err(), targets.Close()); err != nil {
		return nil, err
	}
	for _, one := range sources {
		if !used[one.name] && one.dataType != "timestamp" && one.dataType != "rowversion" {
			return nil, fmt.Errorf("the column %s has no place", one.name)
		}
	}
	return columns, nil
}

// plain is a column's name with no underscore and no case.
func plain(name string) string { return strings.ToLower(strings.ReplaceAll(name, "_", "")) }

// readAs is how SQL Server reads a column out as the schema keeps it: a GUID as lower-case text, money as INTEGER ten-thousandths (N6), a moment
// as UTC text with seven decimals (G10), a day as yyyy-MM-dd, a bit as 0 or 1; anything else as it is.
func readAs(one column) string {
	name := "[" + one.source + "]"
	switch one.dataType {
	case "uniqueidentifier":
		return "LOWER(CONVERT(char(36), " + name + "))"
	case "decimal", "numeric", "money":
		return "CONVERT(bigint, " + name + " * 10000)"
	case "datetime2", "datetime", "datetimeoffset":
		return "FORMAT(CONVERT(datetime2(7), " + name + "), 'yyyy-MM-ddTHH:mm:ss.fffffff') + 'Z'"
	case "date":
		return "CONVERT(char(10), " + name + ", 23)"
	case "bit":
		return "CONVERT(int, " + name + ")"
	}
	return name
}

// sameFigures compares the figures fingerprint on both sides (R34): each project's entries counted and summed by kind, status and account.
func sameFigures(ctx context.Context, source *sql.DB, tx *sql.Tx) error {
	held, err := figuresOf(source.QueryContext(ctx, `SELECT LOWER(CONVERT(char(36), ProjectId)), Kind, Status, LOWER(CONVERT(char(36), AccountId)),
		count(*), CONVERT(bigint, SUM(Amount) * 10000) FROM [project].[Entries] GROUP BY ProjectId, Kind, Status, AccountId`))
	if err != nil {
		return err
	}
	kept, err := figuresOf(tx.QueryContext(ctx, `SELECT project_id, kind, status, account_id, count(*), SUM(amount) FROM entries
		GROUP BY project_id, kind, status, account_id`))
	if err != nil {
		return err
	}
	for key, figure := range held {
		if kept[key] != figure {
			return fmt.Errorf("the entries of %s count and sum %s on SQL Server, %s on SQLite", key, figure, kept[key])
		}
	}
	if len(kept) != len(held) {
		return fmt.Errorf("SQL Server's entries fall in %d groups, SQLite's in %d", len(held), len(kept))
	}
	slog.Info("the figures are the same", "groups", len(held))
	return nil
}

// figuresOf is each group's count and sum, by project, kind, status and account.
func figuresOf(rows *sql.Rows, err error) (map[string]string, error) {
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	figures := map[string]string{}
	for rows.Next() {
		var project, account string
		var kind, status, count, sum int64
		if err := rows.Scan(&project, &kind, &status, &account, &count, &sum); err != nil {
			return nil, err
		}
		figures[fmt.Sprintf("%s kind %d status %d account %s", project, kind, status, account)] = fmt.Sprintf("%d rows, %d", count, sum)
	}
	return figures, rows.Err()
}
