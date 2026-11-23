// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

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
	if len(words) != 1004 || words["permissions.reportsGroup"] != "التقارير" {
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
	if answer := testapi.New(t).Send("GET", "/translations/fr", ""); answer.Code != 404 || answer.Header().Get("Content-Type") != "application/problem+json" {
		t.Errorf("%d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
}

// Each language's bundle holds its own words: the app's title, the project chooser's, and AC-86.7's line saying when the widget's rows were read,
// with the place of the time.
func TestUC001_AC_86_7_EachLanguagesBundleHoldsItsOwnWords(t *testing.T) {
	api := testapi.New(t)
	for language, want := range map[string]map[string]string{
		"ar": {"app.title": "REDIX", "projects.choose": "اختر المشروع", "home.widgetReadAt": "آخر تحديث {0}"},
		"en": {"app.title": "REDIX", "projects.choose": "Choose a project", "home.widgetReadAt": "Updated {0}"},
	} {
		var words map[string]string
		if err := json.Unmarshal(api.Send("GET", "/translations/"+language, "").Body.Bytes(), &words); err != nil {
			t.Fatal(language, err)
		}
		for key, value := range want {
			if words[key] != value {
				t.Errorf("%s %s = %q, want %q", language, key, words[key], value)
			}
		}
	}
}
