package main

import (
	"context"
	"database/sql"
	"encoding/base64"
	"errors"
	"flag"
	"io"
	"log/slog"
	"net"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
)

var quiet = slog.New(slog.NewTextHandler(io.Discard, nil))

// settings is the server's settings for one test, in an environment no settings file names, so no variable of Dev's or QA's reaches it; its
// database file is returned.
func settings(t *testing.T, urls string) string {
	t.Helper()
	folder := t.TempDir()
	database := filepath.Join(folder, "redix.db")
	for key, value := range map[string]string{
		"REDIX_Environment": "Test", "REDIX_Urls": urls, "REDIX_Database__Path": database,
		"REDIX_Jwt__SigningKey": base64.StdEncoding.EncodeToString([]byte("test-key-for-the-server-main-32b")),
		"REDIX_Admin__Username": "test-admin", "REDIX_Admin__Password": "Test-admin-passw0rd",
		"REDIX_Attachments__Folder": filepath.Join(folder, "attachments"), "REDIX_Email__Folder": filepath.Join(folder, "emails"),
	} {
		t.Setenv(key, value)
	}
	return database
}

func freeAddress(t *testing.T) string {
	t.Helper()
	listener, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		t.Fatal(err)
	}
	defer listener.Close()
	return listener.Addr().String()
}

// The server listens where its settings say, answers /health once its database is up to date, and stops cleanly when its context ends, as
// systemd's stop ends it (redix-api #141 D2).
func TestTheServerStartsAnswersAndStopsWhenAsked(t *testing.T) {
	address := freeAddress(t)
	settings(t, "http://"+address+";http://127.0.0.1:1")
	ctx, stop := context.WithCancel(context.Background())
	stopped := make(chan error, 1)
	go func() { stopped <- run(ctx, quiet, nil) }()
	var answer string
	for range 100 {
		if response, err := http.Get("http://" + address + "/health"); err == nil {
			body, _ := io.ReadAll(response.Body)
			response.Body.Close()
			answer = string(body)
			break
		}
		time.Sleep(50 * time.Millisecond)
	}
	if answer != "Healthy" {
		t.Fatalf("/health answered %q", answer)
	}
	stop()
	if err := <-stopped; err != nil {
		t.Fatalf("the server stopped with %v", err)
	}
}

// migrate brings the database's schema up to date and stops, serving nothing; main runs it as systemd's unit does before it starts.
func TestMigrateBringsTheSchemaUpToDateAndStops(t *testing.T) {
	database := settings(t, "http://"+freeAddress(t))
	saved := os.Args
	t.Cleanup(func() { os.Args = saved })
	os.Args = []string{"redix-server", "migrate"}
	main()
	opened, err := sql.Open("sqlite", "file:"+database+"?mode=ro")
	if err != nil {
		t.Fatal(err)
	}
	defer opened.Close()
	var tables int
	if err := opened.QueryRow("SELECT count(*) FROM sqlite_schema WHERE type = 'table'").Scan(&tables); err != nil || tables < 59 {
		t.Fatalf("after migrate the database holds %d tables (%v)", tables, err)
	}
}

// The server does not start, and says why, on settings it cannot work with, a database it cannot open or seed, or an address it cannot use.
func TestTheServerRefusesToStartOnWhatItCannotWorkWith(t *testing.T) {
	busy, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		t.Fatal(err)
	}
	defer busy.Close()
	for name, arrange := range map[string]func(t *testing.T){
		"no signing key": func(t *testing.T) { settings(t, "http://"+freeAddress(t)); t.Setenv("REDIX_Jwt__SigningKey", "") },
		"a database in a folder that does not exist": func(t *testing.T) {
			settings(t, "http://"+freeAddress(t))
			t.Setenv("REDIX_Database__Path", filepath.Join(t.TempDir(), "missing", "redix.db"))
		},
		"a database that refuses the admin": func(t *testing.T) {
			path := settings(t, "http://"+freeAddress(t))
			database, err := db.Open(context.Background(), path)
			if err != nil {
				t.Fatal(err)
			}
			defer database.Close()
			if err := database.Exec(context.Background(), "CREATE TRIGGER no_users BEFORE INSERT ON users BEGIN SELECT RAISE(ABORT, 'no users'); END"); err != nil {
				t.Fatal(err)
			}
		},
		"an address that is not one":  func(t *testing.T) { settings(t, "http://[::1") },
		"an address already listened": func(t *testing.T) { settings(t, "http://"+busy.Addr().String()) },
	} {
		t.Run(name, func(t *testing.T) {
			arrange(t)
			if err := run(context.Background(), quiet, nil); err == nil || strings.TrimSpace(err.Error()) == "" {
				t.Fatalf("the server started, or stopped saying nothing (%v)", err)
			}
		})
	}
}

// The server exits with 1, as systemd counts a failed start, when it cannot start: here, with no signing key. It runs as a process of its own,
// as main ends the process; its coverage joins the test's through the folder go test gathers it in.
func TestTheServerExitsWithOneWhenItCannotStart(t *testing.T) {
	if os.Getenv("REDIX_TEST_MAIN_EXITS") == "1" {
		os.Args = []string{"redix-server"}
		main()
		return
	}
	settings(t, "http://"+freeAddress(t))
	arguments := []string{"-test.run=^TestTheServerExitsWithOneWhenItCannotStart$"}
	if folder := flag.Lookup("test.gocoverdir"); folder != nil && folder.Value.String() != "" {
		arguments = append(arguments, "-test.gocoverdir="+folder.Value.String())
	}
	command := exec.Command(os.Args[0], arguments...)
	command.Env = append(os.Environ(), "REDIX_TEST_MAIN_EXITS=1", "REDIX_Jwt__SigningKey=")
	var exit *exec.ExitError
	if err := command.Run(); !errors.As(err, &exit) || exit.ExitCode() != 1 {
		t.Fatalf("the server without a signing key ended with %v, not exit status 1", err)
	}
}
