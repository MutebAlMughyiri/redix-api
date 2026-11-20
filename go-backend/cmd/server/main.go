// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Command server is the REDIX API: one process that owns the SQLite database and answers the app and the web front end (redix-api #140, R3).
package main

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"net"
	"net/http"
	"net/url"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/email"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/incidents"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/environment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/server"
	"golang.org/x/crypto/acme/autocert"
)

func main() {
	logger := slog.New(slog.NewJSONHandler(os.Stdout, nil))
	slog.SetDefault(logger)
	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()
	if err := run(ctx, logger, os.Args[1:]); err != nil {
		logger.Error("the API stopped", "error", err)
		os.Exit(1)
	}
}

// run serves the API until ctx ends, as systemd's stop ends it; with the argument migrate, it brings the database up to date and stops.
func run(ctx context.Context, logger *slog.Logger, args []string) error {
	settings, err := environment.FromProcess()
	if err != nil {
		return err
	}
	database, err := db.Open(ctx, settings.DatabasePath)
	if err != nil {
		return err
	}
	defer func() { _ = database.Close() }()
	if len(args) > 0 && args[0] == "migrate" {
		logger.Info("the database's schema is up to date", "path", settings.DatabasePath)
		return nil
	}
	now := clock.System{}
	if err := auth.SeedAdmin(ctx, database, settings.AdminUsername, settings.AdminPassword, moment.Of(now.Now())); err != nil {
		return err
	}
	doors := server.Routes(server.Parts{
		Database: database, Clock: now, SigningKey: settings.SigningKey, Sender: email.SenderOf(settings.EmailSmtp, settings.EmailFrom, settings.EmailFolder),
		EmailCodesPerHour: settings.EmailCodesPerHour, RegistrationsPerHour: settings.RegistrationsPerHour, WebFolder: settings.WebFolder,
		AttachmentsFolder: settings.AttachmentsFolder, TypstPath: settings.TypstPath, Documented: settings.Documented, Log: logger,
	})
	failures, err := incidents.Open(settings.IncidentsPath)
	if err != nil {
		return err
	}
	defer func() { _ = failures.Close() }()
	recorded := func(door http.Handler) http.Handler {
		return problem.Traced(incidents.Recorded(door, failures, logger, now))
	}
	return serve(ctx, logger, settings, recorded(doors.API), recorded(doors.Pages))
}

// maxBodyBytes is the largest request body the API reads: the largest file a person may send is 25 MB (fileTooLarge), with room for its form
// (redix-api #145, R17).
const maxBodyBytes = 30 << 20

// serve answers until ctx ends: the API on the first of Urls and, when Web__Urls names one, the web's pages on its first (redix-api #161 S14).
// With a domain, as the server's instances run, both answer HTTPS, the API on 8443 and the web on 443 for production, with the certificate Let's
// Encrypt gives the domain, which autocert keeps in the certificates folder and renews (redix-api #145, D1, R21). Let's Encrypt checks a name over
// HTTP on port 80, which one process alone can hold: the instance whose Urls name a second, HTTP address, production, proves its own name there
// and those of Certificates__Proves, and sends every other request to HTTPS; qa and dev name none, and their tokens reach port 80 through the
// certificates folder the three share (D-R25). Without a domain, as Dev and QA run on a PC, plain HTTP. No request's body passes 30 MB (R17).
func serve(ctx context.Context, logger *slog.Logger, settings environment.Settings, api, pages http.Handler) error {
	addresses, err := listenAddresses(settings.Urls)
	if err != nil {
		return err
	}
	servers := []*http.Server{{Addr: addresses[0], Handler: http.MaxBytesHandler(api, maxBodyBytes), ReadHeaderTimeout: 10 * time.Second}}
	if settings.WebUrls != "" {
		web, err := listenAddresses(settings.WebUrls)
		if err != nil {
			return err
		}
		servers = append(servers, &http.Server{Addr: web[0], Handler: http.MaxBytesHandler(pages, maxBodyBytes), ReadHeaderTimeout: 10 * time.Second})
	}
	logger.Info("the API is listening", "address", servers[0].Addr, "web", settings.WebUrls, "domain", settings.Domain, "environment", settings.Environment,
		"database", settings.DatabasePath)
	if settings.Domain == "" {
		return serveAll(ctx, servers, (*http.Server).ListenAndServe)
	}
	certificates := &autocert.Manager{
		Prompt:     autocert.AcceptTOS,
		HostPolicy: autocert.HostWhitelist(append([]string{settings.Domain}, settings.CertificatesProves...)...),
		Cache:      autocert.DirCache(settings.CertificatesFolder),
	}
	// Asking for the HTTP handler is what makes autocert prove the name over HTTP, the one check port 443's absence leaves, here too where
	// another instance serves it.
	proofs := certificates.HTTPHandler(nil)
	if len(addresses) > 1 {
		httpPort, err := net.Listen("tcp", addresses[1])
		if err != nil {
			return fmt.Errorf("the HTTP port proves the domain to Let's Encrypt: %w", err)
		}
		challenges := &http.Server{Handler: proofs, ReadHeaderTimeout: 10 * time.Second}
		go func() {
			if err := until(ctx, challenges, func() error { return challenges.Serve(httpPort) }); err != nil {
				logger.Error("the HTTP port stopped proving the domain to Let's Encrypt", "error", err)
			}
		}()
	}
	for _, door := range servers {
		door.TLSConfig = certificates.TLSConfig()
	}
	return serveAll(ctx, servers, func(door *http.Server) error { return door.ListenAndServeTLS("", "") })
}

// serveAll runs every one of servers with listen until ctx ends, or until one stops with an error, which stops the others and is answered.
func serveAll(ctx context.Context, servers []*http.Server, listen func(*http.Server) error) error {
	ctx, stop := context.WithCancel(ctx)
	defer stop()
	stopped := make(chan error, len(servers))
	for _, door := range servers {
		go func() { stopped <- until(ctx, door, func() error { return listen(door) }) }()
	}
	var first error
	for range servers {
		if err := <-stopped; err != nil && first == nil {
			first = err
			stop()
		}
	}
	return first
}

// until runs listen until ctx ends, then shuts server down within ten seconds and waits for it: a server closed that way is no error, a
// shutdown that failed is.
func until(ctx context.Context, server *http.Server, listen func() error) error {
	stopped := make(chan error, 1)
	go func() {
		<-ctx.Done()
		shutdown, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()
		stopped <- server.Shutdown(shutdown)
	}()
	if err := listen(); !errors.Is(err, http.ErrServerClosed) {
		return err
	}
	return <-stopped
}

// listenAddresses is the host and port of each of urls, which lists them as .NET's Urls setting did: "http://0.0.0.0:5012;http://...".
func listenAddresses(urls string) ([]string, error) {
	var addresses []string
	for each := range strings.SplitSeq(urls, ";") {
		parsed, err := url.Parse(each)
		if err != nil {
			return nil, err
		}
		addresses = append(addresses, parsed.Host)
	}
	return addresses, nil
}
