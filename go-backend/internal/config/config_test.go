package config

import (
	"context"
	"database/sql"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

func values(t *testing.T, handler http.Handler) map[string]string {
	t.Helper()
	recorder := httptest.NewRecorder()
	handler.ServeHTTP(recorder, httptest.NewRequest(http.MethodGet, "/config", nil))
	var byKey map[string]string
	if recorder.Code != 200 || json.Unmarshal(recorder.Body.Bytes(), &byKey) != nil {
		t.Fatalf("%d %s", recorder.Code, recorder.Body)
	}
	return byKey
}

func TestUC070_TheAppReadsHowLongToWaitBeforeAnyoneSignsIn(t *testing.T) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer database.Close()
	router := web.NewRouter("")
	Map(router, database)
	got := values(t, router)
	if got["requestTimeoutSeconds"] != "10" || got["fileTimeoutSeconds"] != "120" || got["vatRatePercent"] != "15" {
		t.Errorf("%v", got)
	}
}

func TestUC070_AValueChangedInTheDatabaseIsReadAtTheNextRequest(t *testing.T) {
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	defer database.Close()
	router := web.NewRouter("")
	Map(router, database)
	err = database.InTx(context.Background(), func(tx *sql.Tx) error {
		_, err := tx.Exec("UPDATE config_values SET value = '15' WHERE key = 'requestTimeoutSeconds'")
		return err
	})
	if err != nil {
		t.Fatal(err)
	}
	if got := values(t, router)["requestTimeoutSeconds"]; got != "15" {
		t.Errorf("read %s", got)
	}
}
