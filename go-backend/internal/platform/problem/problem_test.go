// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package problem

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"regexp"
	"strings"
	"testing"
)

func answer(handler http.HandlerFunc) *httptest.ResponseRecorder {
	recorder := httptest.NewRecorder()
	Traced(handler).ServeHTTP(recorder, httptest.NewRequest(http.MethodGet, "/projects", nil))
	return recorder
}

func TestN16_ARefusalIsProblemDetailsWithItsCodeInDotNetsOrder(t *testing.T) {
	got := answer(func(w http.ResponseWriter, r *http.Request) { Refuse(w, r, 409, "projectClosed") })
	if got.Code != 409 || got.Header().Get("Content-Type") != "application/problem+json" {
		t.Fatalf("%d %s", got.Code, got.Header().Get("Content-Type"))
	}
	pattern := `^\{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.10","title":"Conflict","status":409,"code":"projectClosed","traceId":"00-[0-9a-f]{32}-[0-9a-f]{16}-00"\}$`
	if !regexp.MustCompile(pattern).MatchString(got.Body.String()) {
		t.Errorf("body %s", got.Body)
	}
}

func TestN16_ARefusalOverSomethingCarriesItUnderItsName(t *testing.T) {
	got := answer(func(w http.ResponseWriter, r *http.Request) {
		RefuseOver(w, r, 400, "rowsRefused", "rows", []map[string]string{{"code": "unknownChoice"}})
	})
	var body map[string]any
	json.Unmarshal(got.Body.Bytes(), &body)
	if body["code"] != "rowsRefused" || len(body["rows"].([]any)) != 1 || body["title"] != "Bad Request" {
		t.Errorf("body %s", got.Body)
	}
}

func TestN16_R7_ABareNotFoundIsProblemDetailsAsUseStatusCodePagesWroteItWithItsCode(t *testing.T) {
	got := answer(func(w http.ResponseWriter, r *http.Request) { Status(w, r, 404) })
	if !strings.HasPrefix(got.Body.String(), `{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"code":"notFound","traceId":"00-`) {
		t.Errorf("body %s", got.Body)
	}
}

func TestN16_TooManyRequestsHasATitleAndNoTypeAsDotNetAnswered(t *testing.T) {
	got := answer(func(w http.ResponseWriter, r *http.Request) { Status(w, r, 429) })
	if !strings.HasPrefix(got.Body.String(), `{"title":"Too Many Requests","status":429,"code":"tooManyRequests","traceId":`) {
		t.Errorf("body %s", got.Body)
	}
}

func TestN16_R7_AStatusASPNETHasNoDefaultsForHasNeitherTypeNorTitleAndIsFailed(t *testing.T) {
	got := answer(func(w http.ResponseWriter, r *http.Request) { Status(w, r, 418) })
	if !strings.HasPrefix(got.Body.String(), `{"status":418,"code":"failed","traceId":`) {
		t.Errorf("body %s", got.Body)
	}
}
