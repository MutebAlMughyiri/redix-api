// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package main

import (
	"bytes"
	"context"
	"crypto/tls"
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
	defer func() { _ = listener.Close() }()
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
			_ = response.Body.Close()
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
	defer func() { _ = opened.Close() }()
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
	defer func() { _ = busy.Close() }()
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
			defer func() { _ = database.Close() }()
			if err := database.Exec(context.Background(), "CREATE TRIGGER no_users BEFORE INSERT ON users BEGIN SELECT RAISE(ABORT, 'no users'); END"); err != nil {
				t.Fatal(err)
			}
		},
		"an address that is not one": func(t *testing.T) { settings(t, "http://[::1") },
		"a domain whose HTTP port is taken, so it could never be proven": func(t *testing.T) {
			withDomain(t, freeAddress(t), busy.Addr().String())
		},
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

// started runs the server with the settings the test set until the test ends, and waits until address answers.
func started(t *testing.T, logger *slog.Logger, address string) {
	t.Helper()
	ctx, stop := context.WithCancel(context.Background())
	stopped := make(chan error, 1)
	go func() { stopped <- run(ctx, logger, nil) }()
	t.Cleanup(func() {
		stop()
		if err := <-stopped; err != nil {
			t.Errorf("the server stopped with %v", err)
		}
	})
	for range 100 {
		if connection, err := net.Dial("tcp", address); err == nil {
			_ = connection.Close()
			return
		}
		time.Sleep(50 * time.Millisecond)
	}
	t.Fatalf("nothing answered on %s", address)
}

// withDomain is the settings of a server answering HTTPS on its domain, redix.test, at httpsAddress, and plain HTTP at httpAddress, as Prod
// answers on 8443 and 80 (redix-api #145 D1, R21).
func withDomain(t *testing.T, httpsAddress, httpAddress string) {
	t.Helper()
	settings(t, "https://"+httpsAddress+";http://"+httpAddress)
	t.Setenv("REDIX_Domain", "redix.test")
	t.Setenv("REDIX_Certificates__Folder", t.TempDir())
}

// handshakeWith answers whether a TLS hello naming name gets a certificate from the server at address. A name the domain is not is refused there,
// before Let's Encrypt is ever asked, so no test reaches the internet.
func handshakeWith(address, name string) error {
	connection, err := tls.Dial("tcp", address, &tls.Config{ServerName: name})
	if err == nil {
		_ = connection.Close()
	}
	return err
}

// With a domain, the HTTP port sends every request to the same path over HTTPS, and the HTTPS port gives a certificate for the domain alone,
// never for another name someone asks for (redix-api #145 D1, R16).
func TestR16_WithADomainHttpGoesToHttpsAndNoOtherNameGetsACertificate(t *testing.T) {
	httpsAddress, httpAddress := freeAddress(t), freeAddress(t)
	withDomain(t, httpsAddress, httpAddress)
	started(t, quiet, httpsAddress)

	request, _ := http.NewRequest("GET", "http://"+httpAddress+"/health", nil)
	request.Host = "redix.test"
	client := &http.Client{CheckRedirect: func(*http.Request, []*http.Request) error { return http.ErrUseLastResponse }}
	response, err := client.Do(request)
	if err != nil {
		t.Fatal(err)
	}
	_ = response.Body.Close()
	if response.StatusCode != http.StatusFound || response.Header.Get("Location") != "https://redix.test/health" {
		t.Errorf("HTTP answered %d, to %q", response.StatusCode, response.Header.Get("Location"))
	}
	if err := handshakeWith(httpsAddress, "elsewhere.test"); err == nil {
		t.Error("a name that is not the domain got a certificate")
	}
}

// qa and dev answer HTTPS on their own name and port with no HTTP port, which production holds for them, and still give no other name a
// certificate (redix-api #145 R25, D-R25).
func TestR25_AnInstanceWithoutAnHttpPortAnswersHttpsOnItsOwnName(t *testing.T) {
	httpsAddress := freeAddress(t)
	settings(t, "https://"+httpsAddress)
	t.Setenv("REDIX_Domain", "qa.redix.test")
	t.Setenv("REDIX_Certificates__Folder", t.TempDir())
	started(t, quiet, httpsAddress)

	if err := handshakeWith(httpsAddress, "elsewhere.test"); err == nil {
		t.Error("a name that is not the instance's got a certificate")
	}
}

// Production's HTTP port hands Let's Encrypt the token another instance kept in the certificates folder the three share, for a name it proves,
// and refuses a name it does not (redix-api #145 D-R25).
func TestR25_ProductionsHttpPortProvesTheNamesOfTheOtherInstances(t *testing.T) {
	httpsAddress, httpAddress := freeAddress(t), freeAddress(t)
	withDomain(t, httpsAddress, httpAddress)
	t.Setenv("REDIX_Certificates__Proves", "qa.redix.test;dev.redix.test")
	folder := t.TempDir()
	t.Setenv("REDIX_Certificates__Folder", folder)
	if err := os.WriteFile(filepath.Join(folder, "token-of-qa+http-01"), []byte("token-of-qa.thumbprint"), 0o600); err != nil {
		t.Fatal(err)
	}
	started(t, quiet, httpsAddress)

	for host, want := range map[string]int{"qa.redix.test": http.StatusOK, "elsewhere.test": http.StatusForbidden} {
		request, _ := http.NewRequest("GET", "http://"+httpAddress+"/.well-known/acme-challenge/token-of-qa", nil)
		request.Host = host
		response, err := http.DefaultClient.Do(request)
		if err != nil {
			t.Fatal(err)
		}
		body, _ := io.ReadAll(response.Body)
		_ = response.Body.Close()
		if response.StatusCode != want || want == http.StatusOK && string(body) != "token-of-qa.thumbprint" {
			t.Errorf("%s: answered %d %q, want %d", host, response.StatusCode, body, want)
		}
	}
}

// No request's body passes 30 MB: a larger one is refused before any route keeps it, as Caddy refused it (redix-api #145 R17).
func TestR17_ABodyOver30MBIsRefused(t *testing.T) {
	address := freeAddress(t)
	settings(t, "http://"+address)
	started(t, quiet, address)

	response, err := http.Post("http://"+address+"/auth/sign-in", "application/json", bytes.NewReader(make([]byte, maxBodyBytes+1)))
	if err != nil {
		t.Fatal(err)
	}
	_ = response.Body.Close()
	if response.StatusCode != http.StatusBadRequest {
		t.Errorf("a body of 30 MB and a byte answered %d", response.StatusCode)
	}
}
