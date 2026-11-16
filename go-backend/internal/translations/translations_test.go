package translations

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

func api(t *testing.T) http.Handler {
	t.Helper()
	database, err := db.Open(context.Background(), filepath.Join(t.TempDir(), "redix.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { database.Close() })
	router := web.NewRouter("")
	Map(router, database)
	return problem.Traced(router)
}

func get(handler http.Handler, path string, header ...string) *httptest.ResponseRecorder {
	request := httptest.NewRequest(http.MethodGet, path, nil)
	for i := 0; i+1 < len(header); i += 2 {
		request.Header.Set(header[i], header[i+1])
	}
	recorder := httptest.NewRecorder()
	handler.ServeHTTP(recorder, request)
	return recorder
}

func TestUC001_EveryWordOfALanguageComesAsOneBundleWithAnETag(t *testing.T) {
	answer := get(api(t), "/translations/ar")
	var words map[string]string
	if answer.Code != 200 || answer.Header().Get("Content-Type") != "application/json" || json.Unmarshal(answer.Body.Bytes(), &words) != nil {
		t.Fatalf("%d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
	if len(words) != 817 || words["permissions.reportsGroup"] != "التقارير" {
		t.Errorf("%d words, permissions.reportsGroup = %q", len(words), words["permissions.reportsGroup"])
	}
	if etag := answer.Header().Get("ETag"); len(etag) != 66 || etag[0] != '"' {
		t.Errorf("ETag %s", etag)
	}
}

func TestUC001_AnUnchangedBundleIsNotSentAgain(t *testing.T) {
	handler := api(t)
	etag := get(handler, "/translations/en").Header().Get("ETag")
	if answer := get(handler, "/translations/en", "If-None-Match", etag); answer.Code != http.StatusNotModified || answer.Body.Len() != 0 {
		t.Errorf("%d %q", answer.Code, answer.Body)
	}
	if answer := get(handler, "/translations/en", "If-None-Match", `"OTHER"`); answer.Code != 200 {
		t.Errorf("another ETag: %d", answer.Code)
	}
}

func TestUC001_ALanguageWithNoWordsIsNotFound(t *testing.T) {
	if answer := get(api(t), "/api/v1/translations/fr"); answer.Code != 404 || answer.Header().Get("Content-Type") != "application/problem+json" {
		t.Errorf("%d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
}
