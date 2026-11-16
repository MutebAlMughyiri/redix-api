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
	r.Handle("GET", "/projects/{projectId}", func(w http.ResponseWriter, request *http.Request) {
		if id, ok := ID(w, request, "projectId"); ok {
			JSON(w, request, http.StatusOK, map[string]string{"id": id.String()})
		}
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

func TestR6_AGuidRouteValueThatIsNotAGuidIsNotFound(t *testing.T) {
	got := ask(router(t), "GET", "/projects/12")
	if got.Code != 404 || !strings.Contains(got.Body.String(), `"title":"Not Found"`) {
		t.Errorf("%d %s", got.Code, got.Body)
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
