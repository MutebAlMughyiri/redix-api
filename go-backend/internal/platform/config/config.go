// Package config owns the API's settings, read from environment variables only (R11), under the names .NET read: REDIX_Jwt__SigningKey, and an
// environment's own REDIX_{Environment}_Jwt__SigningKey, which wins (redix-api #118).
package config

import (
	"encoding/base64"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strconv"
)

// Config is every setting the API reads.
type Config struct {
	// Environment names the settings' environment, such as Dev or QA (REDIX_Environment).
	Environment string
	// Urls is where the API listens (Urls).
	Urls string
	// DatabasePath is the SQLite file (Database__Path).
	DatabasePath string
	// SigningKey signs access tokens; at least 256 bits, kept base64 (Jwt__SigningKey).
	SigningKey []byte
	// AdminUsername and AdminPassword make the first admin when there is none (Admin__Username, Admin__Password).
	AdminUsername, AdminPassword string
	// RegistrationsPerHour and EmailCodesPerHour limit asking, per hour (Registration__PermitsPerHour, EmailCodes__PermitsPerHour).
	RegistrationsPerHour, EmailCodesPerHour int
	// AttachmentsFolder keeps uploaded files; the backup must include it (Attachments__Folder, NFR-10, R32).
	AttachmentsFolder string
	// EmailConnectionString and EmailFrom send email through Azure Communication Services; empty until set (Email__ConnectionString, Email__From).
	EmailConnectionString, EmailFrom string
	// EmailFolder keeps each email instead of sending it while no connection string is set (Email__Folder).
	EmailFolder string
}

// Read is the settings in the environment variables lookup gives, with .NET's defaults where one is not set.
func Read(lookup func(string) (string, bool)) (Config, error) {
	environment := valueOr(lookup, "REDIX_Environment", "Production")
	get := func(key, fallback string) string {
		if value, ok := lookup("REDIX_" + environment + "_" + key); ok {
			return value
		}
		return valueOr(lookup, "REDIX_"+key, fallback)
	}
	data := filepath.Join(localData(lookup), "Redix")
	key, err := base64.StdEncoding.DecodeString(get("Jwt__SigningKey", ""))
	if err != nil || len(key) == 0 {
		return Config{}, errors.New("set the REDIX_Jwt__SigningKey environment variable")
	}
	registrations, err := strconv.Atoi(get("Registration__PermitsPerHour", "5"))
	if err != nil {
		return Config{}, fmt.Errorf("REDIX_Registration__PermitsPerHour: %w", err)
	}
	emailCodes, err := strconv.Atoi(get("EmailCodes__PermitsPerHour", "5"))
	if err != nil {
		return Config{}, fmt.Errorf("REDIX_EmailCodes__PermitsPerHour: %w", err)
	}
	return Config{
		Environment:           environment,
		Urls:                  get("Urls", "http://0.0.0.0:5012"),
		DatabasePath:          get("Database__Path", filepath.Join(data, "redix.db")),
		SigningKey:            key,
		AdminUsername:         get("Admin__Username", ""),
		AdminPassword:         get("Admin__Password", ""),
		RegistrationsPerHour:  registrations,
		EmailCodesPerHour:     emailCodes,
		AttachmentsFolder:     get("Attachments__Folder", filepath.Join(data, "Attachments")),
		EmailConnectionString: get("Email__ConnectionString", ""),
		EmailFrom:             get("Email__From", ""),
		EmailFolder:           get("Email__Folder", filepath.Join(data, "Emails")),
	}, nil
}

// FromEnvironment is the settings in this process's environment variables.
func FromEnvironment() (Config, error) { return Read(os.LookupEnv) }

func valueOr(lookup func(string) (string, bool), key, fallback string) string {
	if value, ok := lookup(key); ok {
		return value
	}
	return fallback
}

// localData is the folder .NET called LocalApplicationData: %LOCALAPPDATA% on Windows, $XDG_DATA_HOME or ~/.local/share elsewhere.
func localData(lookup func(string) (string, bool)) string {
	if runtime.GOOS == "windows" {
		return valueOr(lookup, "LOCALAPPDATA", ".")
	}
	if folder, ok := lookup("XDG_DATA_HOME"); ok {
		return folder
	}
	return filepath.Join(valueOr(lookup, "HOME", "."), ".local", "share")
}
