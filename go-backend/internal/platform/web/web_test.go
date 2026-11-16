package web

import (
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
)

func router(t *testing.T) *Router {
	t.Helper()
	folder := t.TempDir()
	os.WriteFile(filepath.Join(folder, "index.html"), []byte("<h1>REDIX</h1>"), 0o644)
	r := NewRouter(folder)
	r.Handle("GET", "/projects/{projectId:guid}", func(w http.ResponseWriter, request *http.Request) {
		JSON(w, request, http.StatusOK, map[string]string{"id": ID(request, "projectId").String()})
	})
	r.Handle("GET", "/reports/{number:int}", func(w http.ResponseWriter, request *http.Request) {
		JSON(w, request, http.StatusOK, map[string]string{"number": request.PathValue("number")})
	})
	return r
}

func ask(r *Router, method, path string) *httptest.ResponseRecorder {
	recorder := httptest.NewRecorder()
	problem.Traced(r).ServeHTTP(recorder, httptest.NewRequest(method, path, nil))
	return recorder
}

func TestR6_ARouteAnswersAtItsOldPathAndUnderApiV1(t *testing.T) {
	r := router(t)
	for _, path := range []string{"/projects/0193a1b2-0000-7000-8000-000000000001", "/api/v1/projects/0193a1b2-0000-7000-8000-000000000001"} {
		got := ask(r, "GET", path)
		if got.Code != 200 || got.Body.String() != `{"id":"0193a1b2-0000-7000-8000-000000000001"}` || got.Header().Get("Content-Type") != "application/json; charset=utf-8" {
			t.Errorf("%s: %d %s %s", path, got.Code, got.Header().Get("Content-Type"), got.Body)
		}
	}
}

func TestR4_ARouteValueThatDoesNotFitItsConstraintIsNotFound(t *testing.T) {
	r := router(t)
	for _, path := range []string{"/projects/12", "/projects/0193a1b200007000800000000000001", "/reports/x", "/reports/99999999999"} {
		if got := ask(r, "GET", path); got.Code != 404 || !strings.Contains(got.Body.String(), `"title":"Not Found"`) {
			t.Errorf("%s: %d %s", path, got.Code, got.Body)
		}
	}
	if got := ask(r, "GET", "/api/v1/reports/7"); got.Code != 200 || got.Body.String() != `{"number":"7"}` {
		t.Errorf("a fitting int: %d %s", got.Code, got.Body)
	}
}

func TestR4_AnUnknownPathIsNotFoundAndAKnownPathWithAnotherMethodIsNotAllowed(t *testing.T) {
	r := router(t)
	missing := ask(r, "GET", "/nothing-here")
	if missing.Code != 404 || missing.Header().Get("Content-Type") != "application/problem+json" || !strings.Contains(missing.Body.String(), `"status":404`) {
		t.Errorf("missing: %d %s %s", missing.Code, missing.Header().Get("Content-Type"), missing.Body)
	}
	wrong := ask(r, "DELETE", "/projects/0193a1b2-0000-7000-8000-000000000001")
	if wrong.Code != 405 || !strings.Contains(wrong.Body.String(), `"title":"Method Not Allowed"`) || !strings.Contains(wrong.Header().Get("Allow"), "GET") {
		t.Errorf("wrong method: %d %s allow %q", wrong.Code, wrong.Body, wrong.Header().Get("Allow"))
	}
}

func TestR18_AnyOtherPathIsTheFrontEndsFile(t *testing.T) {
	got := ask(router(t), "GET", "/")
	if got.Code != 200 || got.Body.String() != "<h1>REDIX</h1>" {
		t.Errorf("%d %s", got.Code, got.Body)
	}
}

type signIn struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

func readOf(contentType, body string) (int, signIn) {
	request := httptest.NewRequest(http.MethodPost, "/auth/sign-in", strings.NewReader(body))
	if contentType != "" {
		request.Header.Set("Content-Type", contentType)
	}
	recorder := httptest.NewRecorder()
	var read signIn
	if !Read(recorder, request, &read) {
		return recorder.Code, read
	}
	return 200, read
}

func TestR4_ARequestIsReadAsDotNetBoundItsRecord(t *testing.T) {
	for _, body := range []string{`{"username":"a","password":"b"}`, `{"Username":"a","PASSWORD":"b","extra":1}`} {
		if code, read := readOf("application/json; charset=utf-8", body); code != 200 || read != (signIn{"a", "b"}) {
			t.Errorf("%s: %d %+v", body, code, read)
		}
	}
	for body, want := range map[string]int{`{"username":"a"}`: 400, `{"username":"a","password":null}`: 400, `{"username":1,"password":"b"}`: 400, `not json`: 400, ``: 400} {
		if code, _ := readOf("application/json", body); code != want {
			t.Errorf("%q: %d, want %d", body, code, want)
		}
	}
	if code, _ := readOf("text/plain", `{"username":"a","password":"b"}`); code != 415 {
		t.Errorf("text/plain: %d", code)
	}
}

func TestR4_TheLanguageIsArabicUnlessEnglishIsAskedFirst(t *testing.T) {
	for header, want := range map[string]string{"": "ar", "en": "en", "en-US,en;q=0.9": "en", "ar-SA": "ar", "fr": "ar", "fr,en;q=0.8": "en", "de;q=0.9,ar;q=0.5,en;q=0.95": "en", "fr,de,it,en": "ar"} {
		request := httptest.NewRequest(http.MethodGet, "/", nil)
		request.Header.Set("Accept-Language", header)
		if got := Language(request); got != want {
			t.Errorf("%q: %s, want %s", header, got, want)
		}
	}
}
