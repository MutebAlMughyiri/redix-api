// Command server is the REDIX API: one process that owns the SQLite database and answers the app and the web front end (redix-api #140, R3).
package main

import (
	"context"
	"errors"
	"log/slog"
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
)

func main() {
	logger := slog.New(slog.NewJSONHandler(os.Stdout, nil))
	if err := run(logger); err != nil {
		logger.Error("the API stopped", "error", err)
		os.Exit(1)
	}
}

func run(logger *slog.Logger) error {
	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()

	settings, err := environment.FromProcess()
	if err != nil {
		return err
	}
	database, err := db.Open(ctx, settings.DatabasePath)
	if err != nil {
		return err
	}
	defer database.Close()
	now := clock.System{}
	if err := auth.SeedAdmin(ctx, database, settings.AdminUsername, settings.AdminPassword, moment.Of(now.Now())); err != nil {
		return err
	}
	router := server.Routes(server.Parts{
		Database: database, Clock: now, SigningKey: settings.SigningKey, Sender: email.Folder{Path: settings.EmailFolder},
		EmailCodesPerHour: settings.EmailCodesPerHour, RegistrationsPerHour: settings.RegistrationsPerHour, WebFolder: settings.WebFolder,
		AttachmentsFolder: settings.AttachmentsFolder, Log: logger,
	})

	address, err := listenAddress(settings.Urls)
	if err != nil {
		return err
	}
	api := &http.Server{Addr: address, Handler: problem.Traced(router), ReadHeaderTimeout: 10 * time.Second}
	go func() {
		<-ctx.Done()
		shutdown, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()
		api.Shutdown(shutdown)
	}()
	logger.Info("the API is listening", "address", address, "environment", settings.Environment, "database", settings.DatabasePath)
	if err := api.ListenAndServe(); !errors.Is(err, http.ErrServerClosed) {
		return err
	}
	return nil
}

// listenAddress is the host and port of the first of urls, which lists them as .NET's Urls setting did: "http://0.0.0.0:5012;http://...".
func listenAddress(urls string) (string, error) {
	first, err := url.Parse(strings.Split(urls, ";")[0])
	if err != nil {
		return "", err
	}
	return first.Host, nil
}
