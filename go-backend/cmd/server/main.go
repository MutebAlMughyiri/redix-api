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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/environment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/server"
	"golang.org/x/crypto/acme/autocert"
)

func main() {
	logger := slog.New(slog.NewJSONHandler(os.Stdout, nil))
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
	defer database.Close()
	if len(args) > 0 && args[0] == "migrate" {
		logger.Info("the database's schema is up to date", "path", settings.DatabasePath)
		return nil
	}
	now := clock.System{}
	if err := auth.SeedAdmin(ctx, database, settings.AdminUsername, settings.AdminPassword, moment.Of(now.Now())); err != nil {
		return err
	}
	router := server.Routes(server.Parts{
		Database: database, Clock: now, SigningKey: settings.SigningKey, Sender: email.SenderOf(settings.EmailSmtp, settings.EmailFrom, settings.EmailFolder),
		EmailCodesPerHour: settings.EmailCodesPerHour, RegistrationsPerHour: settings.RegistrationsPerHour, WebFolder: settings.WebFolder,
		AttachmentsFolder: settings.AttachmentsFolder, TypstPath: settings.TypstPath, Log: logger,
	})
	return serve(ctx, logger, settings, problem.Traced(router))
}

// maxBodyBytes is the largest request body the API reads: the largest file a person may send is 25 MB (fileTooLarge), with room for its form
// (redix-api #145, R17).
const maxBodyBytes = 30 << 20

// serve answers until ctx ends. With a domain, as Prod runs, it answers HTTPS on the first of Urls, 8443, with the certificate Let's Encrypt
// gives the domain, which autocert keeps in the certificates folder and renews, and on the second, 80, proves the domain and sends every other
// request to HTTPS (redix-api #145, D1, R21: 443 is the web front end's). Let's Encrypt checks a domain over HTTPS on 443 alone, so the server
// does not start without its HTTP port. Without a domain, as Dev and QA run, plain HTTP on the first. No request's body passes 30 MB (R17).
func serve(ctx context.Context, logger *slog.Logger, settings environment.Settings, handler http.Handler) error {
	addresses, err := listenAddresses(settings.Urls)
	if err != nil {
		return err
	}
	api := &http.Server{Addr: addresses[0], Handler: http.MaxBytesHandler(handler, maxBodyBytes), ReadHeaderTimeout: 10 * time.Second}
	logger.Info("the API is listening", "address", api.Addr, "domain", settings.Domain, "environment", settings.Environment, "database", settings.DatabasePath)
	if settings.Domain == "" {
		return until(ctx, api, api.ListenAndServe)
	}
	if len(addresses) < 2 {
		return errors.New("with a domain, REDIX_Urls names the HTTPS address and then the HTTP one, as Prod's https://:8443;http://:80")
	}
	certificates := &autocert.Manager{
		Prompt:     autocert.AcceptTOS,
		HostPolicy: autocert.HostWhitelist(settings.Domain),
		Cache:      autocert.DirCache(settings.CertificatesFolder),
	}
	httpPort, err := net.Listen("tcp", addresses[1])
	if err != nil {
		return fmt.Errorf("the HTTP port proves the domain to Let's Encrypt: %w", err)
	}
	challenges := &http.Server{Handler: certificates.HTTPHandler(nil), ReadHeaderTimeout: 10 * time.Second}
	go until(ctx, challenges, func() error { return challenges.Serve(httpPort) })
	api.TLSConfig = certificates.TLSConfig()
	return until(ctx, api, func() error { return api.ListenAndServeTLS("", "") })
}

// until runs listen until ctx ends, then shuts server down within ten seconds; a server closed that way is no error.
func until(ctx context.Context, server *http.Server, listen func() error) error {
	go func() {
		<-ctx.Done()
		shutdown, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()
		server.Shutdown(shutdown)
	}()
	if err := listen(); !errors.Is(err, http.ErrServerClosed) {
		return err
	}
	return nil
}

// listenAddresses is the host and port of each of urls, which lists them as .NET's Urls setting did: "http://0.0.0.0:5012;http://...".
func listenAddresses(urls string) ([]string, error) {
	var addresses []string
	for _, each := range strings.Split(urls, ";") {
		parsed, err := url.Parse(each)
		if err != nil {
			return nil, err
		}
		addresses = append(addresses, parsed.Host)
	}
	return addresses, nil
}
