// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package incidents

import (
	"bytes"
	"encoding/json"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"strings"
	"testing"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
)

// api is a router behind Traced and Recorded, as the server runs it, with its incidents file and its log.
type api struct {
	t       *testing.T
	store   *Store
	log     *bytes.Buffer
	handler http.Handler
	clock   *clock.Fake
}

func newAPI(t *testing.T) *api {
	store, err := Open(filepath.Join(t.TempDir(), "incidents.db"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { store.Close() })
	mux := http.NewServeMux()
	mux.HandleFunc("GET /projects/{projectId}/suppliers/{supplierId}", func(w http.ResponseWriter, r *http.Request) {
		facts := problem.FactsOf(r.Context())
		facts.UserID, facts.ProjectID = "01a1e687-e54f-772e-961b-955cc95e4ed7", r.PathValue("projectId")
		problem.Status(w, r, http.StatusNotFound)
	})
	mux.HandleFunc("GET /periodic-reports/{number}", func(w http.ResponseWriter, r *http.Request) { panic("a nil map") })
	mux.HandleFunc("GET /health", func(w http.ResponseWriter, r *http.Request) { w.Write([]byte("Healthy")) })
	log := &bytes.Buffer{}
	fake := clock.NewFake(time.Date(2026, 11, 18, 10, 0, 0, 0, time.UTC))
	return &api{t: t, store: store, log: log, clock: fake, handler: problem.Traced(Recorded(mux, store, slog.New(slog.NewJSONHandler(log, nil)), fake))}
}

func (a *api) send(path string, headers ...string) *httptest.ResponseRecorder {
	request := httptest.NewRequest("GET", path, nil)
	for i := 0; i+1 < len(headers); i += 2 {
		request.Header.Set(headers[i], headers[i+1])
	}
	answer := httptest.NewRecorder()
	a.handler.ServeHTTP(answer, request)
	return answer
}

// kept is the incident with the trace id, as the file holds it.
func (a *api) kept(traceID string) (incident Incident, found bool) {
	err := a.store.db.QueryRow(`SELECT trace_id, method, route, status, code, user_id, project_id, stack FROM incidents WHERE trace_id = ?`, traceID).
		Scan(&incident.TraceID, &incident.Method, &incident.Route, &incident.Status, &incident.Code, &incident.UserID, &incident.ProjectID, &incident.Stack)
	return incident, err == nil
}

func traceOf(t *testing.T, answer *httptest.ResponseRecorder) string {
	var body struct {
		Code    string `json:"code"`
		TraceID string `json:"traceId"`
	}
	if err := json.Unmarshal(answer.Body.Bytes(), &body); err != nil || body.TraceID == "" || body.TraceID != answer.Header().Get("X-Trace-Id") {
		t.Fatalf("the answer %s does not carry its X-Trace-Id %q", answer.Body, answer.Header().Get("X-Trace-Id"))
	}
	return body.TraceID
}

func TestR1_R3_D1_ARefusalAnswersItsCodeAndIsKeptWithItsTraceRouteAndWhoAsked(t *testing.T) {
	a := newAPI(t)
	answer := a.send("/projects/01a1e688-02bd-77b3-84f5-e539ed83a963/suppliers/01a1e688-0bd4-7ea0-b9df-0f4dd6000000")
	if answer.Code != 404 || !strings.Contains(answer.Body.String(), `"code":"notFound"`) {
		t.Fatalf("%d %s", answer.Code, answer.Body)
	}
	incident, found := a.kept(traceOf(t, answer))
	want := Incident{TraceID: incident.TraceID, Method: "GET", Route: "/projects/{id}/suppliers/{id}", Status: 404, Code: "notFound",
		UserID: "01a1e687-e54f-772e-961b-955cc95e4ed7", ProjectID: "01a1e688-02bd-77b3-84f5-e539ed83a963"}
	if !found || incident != want {
		t.Errorf("kept %+v, want %+v", incident, want)
	}
}

func TestR6_APanicAnswers500WithItsCodeAndTraceIdAndIsKeptWithItsStack(t *testing.T) {
	a := newAPI(t)
	answer := a.send("/periodic-reports/28")
	if answer.Code != 500 || !strings.Contains(answer.Body.String(), `"code":"unexpected"`) {
		t.Fatalf("%d %s", answer.Code, answer.Body)
	}
	incident, found := a.kept(traceOf(t, answer))
	if !found || incident.Route != "/periodic-reports/{id}" || incident.Code != "unexpected" || !strings.Contains(incident.Stack, "a nil map") ||
		!strings.Contains(incident.Stack, "incidents_test.go") {
		t.Errorf("kept %+v", incident)
	}
	if !strings.Contains(a.log.String(), `"level":"ERROR","msg":"request"`) || !strings.Contains(a.log.String(), "a nil map") {
		t.Errorf("log %s", a.log)
	}
}

func TestR5_EveryRequestWritesOneLogLineAndASuccessIsNoIncident(t *testing.T) {
	a := newAPI(t)
	answer := a.send("/health")
	trace := answer.Header().Get("X-Trace-Id")
	if _, found := a.kept(trace); found || answer.Code != 200 {
		t.Fatalf("%d, kept %v", answer.Code, found)
	}
	var line map[string]any
	if err := json.Unmarshal(a.log.Bytes(), &line); err != nil {
		t.Fatalf("one line: %v %s", err, a.log)
	}
	if line["msg"] != "request" || line["level"] != "INFO" || line["route"] != "/health" || line["status"] != 200.0 || line["traceId"] != trace || line["stack"] != nil {
		t.Errorf("line %v", line)
	}
}

func TestR10_AnAnswerKeepsTheTraceIdOfTheAppsTraceparent(t *testing.T) {
	a := newAPI(t)
	answer := a.send("/health", "traceparent", "00-11111111111111111111111111111111-2222222222222222-01")
	if trace := answer.Header().Get("X-Trace-Id"); !strings.HasPrefix(trace, "00-11111111111111111111111111111111-") || trace == "00-11111111111111111111111111111111-2222222222222222-01" {
		t.Errorf("trace %q", trace)
	}
	if trace := a.send("/health", "traceparent", "not a trace").Header().Get("X-Trace-Id"); len(trace) != 55 {
		t.Errorf("a broken traceparent gives a new trace, not %q", trace)
	}
}

func TestR12_AnIncidentPastNinetyDaysGoesWhenTheNextIsKept(t *testing.T) {
	a := newAPI(t)
	old := Incident{At: moment.Of(a.clock.Now().Add(-91 * 24 * time.Hour)), TraceID: "00-old", Method: "GET", Route: "/old", Status: 500, Code: "unexpected"}
	recent := old
	recent.At, recent.TraceID = moment.Of(a.clock.Now().Add(-89*24*time.Hour)), "00-recent"
	for _, incident := range []Incident{old, recent} {
		if err := a.store.add(t.Context(), incident); err != nil {
			t.Fatal(err)
		}
	}
	a.send("/projects/01a1e688-02bd-77b3-84f5-e539ed83a963/suppliers/01a1e688-0bd4-7ea0-b9df-0f4dd6000000")
	if _, found := a.kept("00-old"); found {
		t.Error("an incident past 90 days stays")
	}
	if _, found := a.kept("00-recent"); !found {
		t.Error("an incident within 90 days went")
	}
}

func TestR8_AnIncidentTheFileCannotTakeIsStillInTheLog(t *testing.T) {
	a := newAPI(t)
	a.store.Close()
	answer := a.send("/periodic-reports/28")
	trace := answer.Header().Get("X-Trace-Id")
	if answer.Code != 500 || !strings.Contains(a.log.String(), `"msg":"request"`) || !strings.Contains(a.log.String(), "an incident was not kept") ||
		strings.Count(a.log.String(), trace) != 2 {
		t.Errorf("%d, log %s", answer.Code, a.log)
	}
}
