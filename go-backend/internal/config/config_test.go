package config_test

import (
	"testing"

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
