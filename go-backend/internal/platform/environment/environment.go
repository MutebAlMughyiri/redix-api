// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package environment owns the API's settings, read from environment variables only (R11), under the names .NET read: REDIX_Jwt__SigningKey, and an
// environment's own REDIX_{Environment}_Jwt__SigningKey, which wins (redix-api #118).
package environment

import (
	"encoding/base64"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strconv"
)

// Settings is every setting the API reads.
type Settings struct {
	// Environment names the settings' environment, such as Dev or QA (REDIX_Environment).
	Environment string
	// Urls is where the API listens in plain HTTP when no domain is set, as Dev and QA run (Urls).
	Urls string
	// Domain is the name the API answers HTTPS on, with the certificate Let's Encrypt gives it; "" where it answers plain HTTP on Urls
	// (Domain, redix-api #145 D1).
	Domain string
	// CertificatesFolder keeps the domain's certificate across restarts (Certificates__Folder, #145).
	CertificatesFolder string
	// DatabasePath is the SQLite file (Database__Path).
	DatabasePath string
	// IncidentsPath is the SQLite file of the requests that failed, beside the database unless set (Incidents__Path, redix-api #150).
	IncidentsPath string
	// SigningKey signs access tokens; at least 256 bits, kept base64 (Jwt__SigningKey).
	SigningKey []byte
	// AdminUsername and AdminPassword make the first admin when there is none (Admin__Username, Admin__Password).
	AdminUsername, AdminPassword string
	// RegistrationsPerHour and EmailCodesPerHour limit asking, per hour (Registration__PermitsPerHour, EmailCodes__PermitsPerHour).
	RegistrationsPerHour, EmailCodesPerHour int
	// AttachmentsFolder keeps uploaded files; the backup must include it (Attachments__Folder, NFR-10, R32).
	AttachmentsFolder string
	// TypstPath is the typst binary that makes the report PDFs (Typst__Path, #140 D2); typst on the path by default.
	TypstPath string
	// EmailSmtp is the mail server every email is handed to, as host:port, and EmailFrom the address it comes from (Email__Smtp, Email__From).
	EmailSmtp, EmailFrom string
	// EmailFolder keeps each email instead of sending it where no mail server is named (Email__Folder).
	EmailFolder string
	// Documented says whether the API serves its OpenAPI document and Swagger UI: in Dev and QA, never in Prod or an environment no settings
	// name, since the document lists every route to anyone who asks (redix-api #142 R4).
	Documented bool
	// WebFolder holds the web front end's files, served for every path that is not a route; "" while there is no front end (Web__Folder, R18).
	WebFolder string
}

// named is what each environment's settings name that is not a secret, as appsettings.{Environment}.json did: where it listens, its admin's
// username and, for Dev, which Patrol registers projects and asks codes from without end, the limits lifted. Dev and QA each keep their own
// database file, Redix{Environment}.db, as each had its own database; Prod is the published server, answering HTTPS on its domain itself
// (#145 D1) and keeping its database, files and certificate under /var/lib/redix, as deploy/setup.sh lays it out; its ports are its server's own,
// REDIX_Urls in /etc/redix/redix.env, where the firewall reads them too. Its secrets are the server's own, in /etc/redix/redix.env (deploy/prod.env.example).
// A variable set in the environment wins over all of it.
var named = map[string]map[string]string{
	"Dev": {"Urls": "http://127.0.0.1:5010", "Admin__Username": "dev-admin", "Registration__PermitsPerHour": "100000", "EmailCodes__PermitsPerHour": "100000"},
	"QA":  {"Urls": "http://127.0.0.1:5012", "Admin__Username": "real-admin"},
	"Prod": {"Database__Path": "/var/lib/redix/redix.db", "Attachments__Folder": "/var/lib/redix/attachments",
		"Email__Folder": "/var/lib/redix/emails", "Email__Smtp": "127.0.0.1:25", "Certificates__Folder": "/var/lib/redix/certificates"},
}

// Read is the settings in the environment variables lookup gives: an environment's own variable, then the shared one, then what its settings
// file named, then .NET's default.
func Read(lookup func(string) (string, bool)) (Settings, error) {
	environment := valueOr(lookup, "REDIX_Environment", "Production")
	get := func(key, fallback string) string {
		if value, ok := lookup("REDIX_" + environment + "_" + key); ok {
			return value
		}
		if value, ok := named[environment][key]; ok {
			fallback = value
		}
		return valueOr(lookup, "REDIX_"+key, fallback)
	}
	data := filepath.Join(localData(runtime.GOOS, lookup), "Redix")
	databaseFile := "redix.db"
	if _, isNamed := named[environment]; isNamed {
		databaseFile = "Redix" + environment + ".db"
	}
	key, err := base64.StdEncoding.DecodeString(get("Jwt__SigningKey", ""))
	if err != nil || len(key) == 0 {
		return Settings{}, errors.New("set the REDIX_Jwt__SigningKey environment variable")
	}
	registrations, err := strconv.Atoi(get("Registration__PermitsPerHour", "5"))
	if err != nil {
		return Settings{}, fmt.Errorf("REDIX_Registration__PermitsPerHour: %w", err)
	}
	emailCodes, err := strconv.Atoi(get("EmailCodes__PermitsPerHour", "5"))
	if err != nil {
		return Settings{}, fmt.Errorf("REDIX_EmailCodes__PermitsPerHour: %w", err)
	}
	databasePath := get("Database__Path", filepath.Join(data, databaseFile))
	return Settings{
		Environment:          environment,
		Urls:                 get("Urls", "http://0.0.0.0:5012"),
		Domain:               get("Domain", ""),
		CertificatesFolder:   get("Certificates__Folder", filepath.Join(data, "Certificates")),
		DatabasePath:         databasePath,
		Documented:           environment == "Dev" || environment == "QA",
		IncidentsPath:        get("Incidents__Path", filepath.Join(filepath.Dir(databasePath), "incidents.db")),
		SigningKey:           key,
		AdminUsername:        get("Admin__Username", ""),
		AdminPassword:        get("Admin__Password", ""),
		RegistrationsPerHour: registrations,
		EmailCodesPerHour:    emailCodes,
		AttachmentsFolder:    get("Attachments__Folder", filepath.Join(data, "Attachments")),
		TypstPath:            get("Typst__Path", "typst"),
		EmailSmtp:            get("Email__Smtp", ""),
		EmailFrom:            get("Email__From", ""),
		EmailFolder:          get("Email__Folder", filepath.Join(data, "Emails")),
		WebFolder:            get("Web__Folder", ""),
	}, nil
}

// FromProcess is the settings in this process's environment variables.
func FromProcess() (Settings, error) { return Read(os.LookupEnv) }

func valueOr(lookup func(string) (string, bool), key, fallback string) string {
	if value, ok := lookup(key); ok {
		return value
	}
	return fallback
}

// localData is the folder .NET called LocalApplicationData on the operating system goos: %LOCALAPPDATA% on Windows, $XDG_DATA_HOME or
// ~/.local/share elsewhere.
func localData(goos string, lookup func(string) (string, bool)) string {
	if goos == "windows" {
		return valueOr(lookup, "LOCALAPPDATA", ".")
	}
	if folder, ok := lookup("XDG_DATA_HOME"); ok {
		return folder
	}
	return filepath.Join(valueOr(lookup, "HOME", "."), ".local", "share")
}
