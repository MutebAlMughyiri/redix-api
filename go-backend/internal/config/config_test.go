// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package config_test

import (
	"context"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/config"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

func values(t *testing.T, api *testapi.API) map[string]any {
	t.Helper()
	code, answer := api.Ask("GET", "/config", "", "")
	byKey, ok := answer.(map[string]any)
	if code != 200 || !ok {
		t.Fatalf("%d %v", code, answer)
	}
	return byKey
}

func TestUC070_TheAppReadsHowLongToWaitBeforeAnyoneSignsIn(t *testing.T) {
	got := values(t, testapi.New(t))
	if got["requestTimeoutSeconds"] != "10" || got["fileTimeoutSeconds"] != "120" || got["vatRatePercent"] != "15" {
		t.Errorf("%v", got)
	}
}

func TestUC070_AValueChangedInTheDatabaseIsReadAtTheNextRequest(t *testing.T) {
	api := testapi.New(t)
	api.Exec("UPDATE config_values SET value = '15' WHERE key = 'requestTimeoutSeconds'")
	if got := values(t, api)["requestTimeoutSeconds"]; got != "15" {
		t.Errorf("read %v", got)
	}
}

// The largest request the server reads is the value requestSizeMegabytes, 30 to start with, so a 25 MB file fits with its form; changed in
// the database, it is read anew (redix-api #145 R17; the owner, 23 Nov 2026).
func TestR17_TheLargestRequestIsTheConfigValueInMegabytes(t *testing.T) {
	api := testapi.New(t)
	if size, err := config.RequestSize(context.Background(), api.Database); size != 30<<20 || err != nil {
		t.Errorf("starts at %d bytes (%v)", size, err)
	}
	api.Exec("UPDATE config_values SET value = '40' WHERE key = 'requestSizeMegabytes'")
	if size, err := config.RequestSize(context.Background(), api.Database); size != 40<<20 || err != nil {
		t.Errorf("changed to 40, reads %d bytes (%v)", size, err)
	}
}

// A largest request that is no whole number of megabytes above 0, or none, is no size: the server stops at its start, naming the key (R17).
func TestR17_ALargestRequestThatIsNoNumberOfMegabytesIsRefused(t *testing.T) {
	for value, statement := range map[string]string{
		"thirty":  "UPDATE config_values SET value = 'thirty' WHERE key = 'requestSizeMegabytes'",
		"0":       "UPDATE config_values SET value = '0' WHERE key = 'requestSizeMegabytes'",
		"missing": "DELETE FROM config_values WHERE key = 'requestSizeMegabytes'",
	} {
		api := testapi.New(t)
		api.Exec(statement)
		if size, err := config.RequestSize(context.Background(), api.Database); err == nil || !strings.Contains(err.Error(), "requestSizeMegabytes") {
			t.Errorf("%s: read %d bytes (%v)", value, size, err)
		}
	}
}
