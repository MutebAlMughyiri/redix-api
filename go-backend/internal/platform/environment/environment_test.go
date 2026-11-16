package environment

import (
	"path/filepath"
	"strings"
	"testing"
)

func lookupIn(values map[string]string) func(string) (string, bool) {
	return func(key string) (string, bool) {
		value, ok := values[key]
		return value, ok
	}
}

const key = "dGVzdC1rZXktZm9yLWdvLWJhY2tlbmQtb25seS0zMmI="

func TestR11_AnEnvironmentsOwnSettingWinsOverTheShared(t *testing.T) {
	got, err := Read(lookupIn(map[string]string{
		"REDIX_Environment": "QA", "REDIX_Jwt__SigningKey": key,
		"REDIX_Admin__Username": "shared-admin", "REDIX_QA_Admin__Username": "real-admin", "REDIX_Urls": "http://127.0.0.1:5012",
	}))
	if err != nil || got.AdminUsername != "real-admin" || got.Urls != "http://127.0.0.1:5012" || got.Environment != "QA" {
		t.Errorf("%+v %v", got, err)
	}
}

func TestR11_WhatIsNotSetTakesDotNetsDefault(t *testing.T) {
	got, err := Read(lookupIn(map[string]string{"REDIX_Jwt__SigningKey": key, "LOCALAPPDATA": `C:\Users\someone\AppData\Local`, "HOME": "/home/someone"}))
	if err != nil {
		t.Fatal(err)
	}
	if got.Environment != "Production" || got.Urls != "http://0.0.0.0:5012" || got.RegistrationsPerHour != 5 || got.EmailCodesPerHour != 5 {
		t.Errorf("%+v", got)
	}
	if !strings.Contains(got.AttachmentsFolder, "Redix") || !strings.HasSuffix(got.AttachmentsFolder, "Attachments") {
		t.Errorf("attachments folder %s", got.AttachmentsFolder)
	}
}

func TestR11_TheAPIDoesNotStartWithoutItsSigningKey(t *testing.T) {
	for _, value := range []string{"", "not base64!"} {
		if _, err := Read(lookupIn(map[string]string{"REDIX_Jwt__SigningKey": value})); err == nil || !strings.Contains(err.Error(), "REDIX_Jwt__SigningKey") {
			t.Errorf("%q: %v", value, err)
		}
	}
	if _, err := Read(lookupIn(map[string]string{})); err == nil {
		t.Error("started with no key")
	}
}

func TestR11_DevAndQaTakeWhatTheirSettingsFilesNamedEachWithItsOwnDatabaseAndASetVariableStillWins(t *testing.T) {
	dev, err := Read(lookupIn(map[string]string{"REDIX_Environment": "Dev", "REDIX_Jwt__SigningKey": key, "LOCALAPPDATA": "data", "HOME": "data"}))
	if err != nil {
		t.Fatal(err)
	}
	if dev.Urls != "http://127.0.0.1:5010" || dev.AdminUsername != "dev-admin" || dev.RegistrationsPerHour != 100000 || dev.EmailCodesPerHour != 100000 ||
		filepath.Base(dev.DatabasePath) != "RedixDev.db" {
		t.Errorf("Dev read %+v", dev)
	}
	qa, err := Read(lookupIn(map[string]string{"REDIX_Environment": "QA", "REDIX_Jwt__SigningKey": key, "REDIX_Urls": "http://0.0.0.0:6000", "LOCALAPPDATA": "data", "HOME": "data"}))
	if err != nil {
		t.Fatal(err)
	}
	if qa.Urls != "http://0.0.0.0:6000" || qa.AdminUsername != "real-admin" || qa.RegistrationsPerHour != 5 || filepath.Base(qa.DatabasePath) != "RedixQA.db" {
		t.Errorf("QA read %+v", qa)
	}
}

func TestR11_ProdListensBehindCaddyAndKeepsItsDataWhereTheDeployLaysItOut(t *testing.T) {
	prod, err := Read(lookupIn(map[string]string{"REDIX_Environment": "Prod", "REDIX_Jwt__SigningKey": key, "REDIX_Prod_Admin__Username": "owner"}))
	if err != nil {
		t.Fatal(err)
	}
	if prod.Urls != "http://127.0.0.1:5012" || prod.DatabasePath != "/var/lib/redix/redix.db" || prod.AttachmentsFolder != "/var/lib/redix/attachments" ||
		prod.EmailFolder != "/var/lib/redix/emails" || prod.AdminUsername != "owner" || prod.RegistrationsPerHour != 5 {
		t.Errorf("Prod read %+v", prod)
	}
}
