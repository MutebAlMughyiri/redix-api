package translations_test

import (
	"encoding/json"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

func TestUC001_EveryWordOfALanguageComesAsOneBundleWithAnETag(t *testing.T) {
	answer := testapi.New(t).Send("GET", "/translations/ar", "")
	var words map[string]string
	if answer.Code != 200 || answer.Header().Get("Content-Type") != "application/json" || json.Unmarshal(answer.Body.Bytes(), &words) != nil {
		t.Fatalf("%d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
	if len(words) != 823 || words["permissions.reportsGroup"] != "التقارير" {
		t.Errorf("%d words, permissions.reportsGroup = %q", len(words), words["permissions.reportsGroup"])
	}
	if etag := answer.Header().Get("ETag"); len(etag) != 66 || etag[0] != '"' {
		t.Errorf("ETag %s", etag)
	}
}

func TestUC001_AnUnchangedBundleIsNotSentAgain(t *testing.T) {
	api := testapi.New(t)
	etag := api.Send("GET", "/translations/en", "").Header().Get("ETag")
	if answer := api.Send("GET", "/translations/en", "", "If-None-Match", etag); answer.Code != 304 || answer.Body.Len() != 0 {
		t.Errorf("%d %q", answer.Code, answer.Body)
	}
	if answer := api.Send("GET", "/translations/en", "", "If-None-Match", `"OTHER"`); answer.Code != 200 {
		t.Errorf("another ETag: %d", answer.Code)
	}
}

func TestUC001_ALanguageWithNoWordsIsNotFound(t *testing.T) {
	if answer := testapi.New(t).Send("GET", "/api/v1/translations/fr", ""); answer.Code != 404 || answer.Header().Get("Content-Type") != "application/problem+json" {
		t.Errorf("%d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
}
