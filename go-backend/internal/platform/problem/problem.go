// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package problem owns how the API answers what it refuses: RFC 9457 problem details carrying a code, which the app shows as a word, as ASP.NET Core's
// AddProblemDetails, UseStatusCodePages and Refusal.Of wrote them (redix-api #140, N16). The API sends codes, never words.
package problem

import (
	"bytes"
	"context"
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"net/http"
	"regexp"
)

// standard is the type link and title ASP.NET Core gives each status it knows (ProblemDetailsDefaults), checked against every problem the .NET API
// answered in its recorded tests: 429 has a title and no type.
var standard = map[int]struct{ link, title string }{
	400: {"https://tools.ietf.org/html/rfc9110#section-15.5.1", "Bad Request"},
	401: {"https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized"},
	403: {"https://tools.ietf.org/html/rfc9110#section-15.5.4", "Forbidden"},
	404: {"https://tools.ietf.org/html/rfc9110#section-15.5.5", "Not Found"},
	405: {"https://tools.ietf.org/html/rfc9110#section-15.5.6", "Method Not Allowed"},
	406: {"https://tools.ietf.org/html/rfc9110#section-15.5.7", "Not Acceptable"},
	408: {"https://tools.ietf.org/html/rfc9110#section-15.5.9", "Request Timeout"},
	409: {"https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict"},
	412: {"https://tools.ietf.org/html/rfc9110#section-15.5.13", "Precondition Failed"},
	415: {"https://tools.ietf.org/html/rfc9110#section-15.5.16", "Unsupported Media Type"},
	422: {"https://tools.ietf.org/html/rfc9110#section-15.5.21", "Unprocessable Entity"},
	426: {"https://tools.ietf.org/html/rfc9110#section-15.5.22", "Upgrade Required"},
	429: {"", "Too Many Requests"},
	500: {"https://tools.ietf.org/html/rfc9110#section-15.6.1", "An error occurred while processing your request."},
	502: {"https://tools.ietf.org/html/rfc9110#section-15.6.3", "Bad Gateway"},
	503: {"https://tools.ietf.org/html/rfc9110#section-15.6.4", "Service Unavailable"},
	504: {"https://tools.ietf.org/html/rfc9110#section-15.6.5", "Gateway Timeout"},
}

// Refuse answers with status and code, a camelCase reason such as "projectClosed" (Refusal.Of).
func Refuse(w http.ResponseWriter, r *http.Request, status int, code string) {
	write(w, r, status, field{"code", code})
}

// RefuseOver answers with status and code, and what the request was refused over under its own name, such as "rows" (Refusal.Of with a detail).
func RefuseOver(w http.ResponseWriter, r *http.Request, status int, code, name string, detail any) {
	write(w, r, status, field{"code", code}, field{name, detail})
}

// Status answers with a status and the code that names it (redix-api #150 R1, R7), as an empty 404 became problem details through
// UseStatusCodePages.
func Status(w http.ResponseWriter, r *http.Request, status int) {
	write(w, r, status, field{"code", codeOf(status)})
}

// statusCodes name the statuses answered with no reason of their own (#150 R7), so every error answer carries a code.
var statusCodes = map[int]string{
	http.StatusBadRequest:           "badRequest",
	http.StatusUnauthorized:         "signInRequired",
	http.StatusForbidden:            "forbidden",
	http.StatusNotFound:             "notFound",
	http.StatusUnsupportedMediaType: "unsupportedMediaType",
	http.StatusTooManyRequests:      "tooManyRequests",
	http.StatusInternalServerError:  "unexpected",
	http.StatusServiceUnavailable:   "unavailable",
}

// codeOf is the code a bare status is answered with; "failed" for one no route answers today.
func codeOf(status int) string {
	if code, named := statusCodes[status]; named {
		return code
	}
	return "failed"
}

type field struct {
	name  string
	value any
}

// write keeps ASP.NET Core's order: type and title when the status has them, status, the extensions, then traceId.
func write(w http.ResponseWriter, r *http.Request, status int, extensions ...field) {
	var fields []field
	if known := standard[status]; known.link != "" {
		fields = append(fields, field{"type", known.link})
	}
	if known := standard[status]; known.title != "" {
		fields = append(fields, field{"title", known.title})
	}
	fields = append(fields, field{"status", status})
	fields = append(fields, extensions...)
	for _, f := range extensions {
		if f.name == "code" {
			FactsOf(r.Context()).Code, _ = f.value.(string)
		}
	}
	fields = append(fields, field{"traceId", TraceOf(r.Context())})

	var body bytes.Buffer
	body.WriteByte('{')
	for i, f := range fields {
		if i > 0 {
			body.WriteByte(',')
		}
		name, _ := json.Marshal(f.name)
		value, err := json.Marshal(f.value)
		if err != nil {
			value = []byte("null")
		}
		body.Write(name)
		body.WriteByte(':')
		body.Write(value)
	}
	body.WriteByte('}')
	w.Header().Set("Content-Type", "application/problem+json")
	w.WriteHeader(status)
	_, _ = w.Write(body.Bytes())
}

type traceKey struct{}

type factsKey struct{}

// Facts is what a request turned out to be, which its log line and, when it fails, its incident keep (redix-api #150): the code it was answered
// with, which write records, and who asked, which the gate records.
type Facts struct {
	Code      string
	UserID    string
	ProjectID string
}

// FactsOf is the request's Facts, which Traced gave it; outside Traced, one nothing reads.
func FactsOf(ctx context.Context) *Facts {
	if facts, kept := ctx.Value(factsKey{}).(*Facts); kept {
		return facts
	}
	return &Facts{}
}

// sentTrace is a W3C traceparent header as the app sends it, whose trace id the answer keeps (#150 R10).
var sentTrace = regexp.MustCompile(`^00-([0-9a-f]{32})-[0-9a-f]{16}-[0-9a-f]{2}$`)

// Traced gives every request a W3C trace id, which every answer carries as its X-Trace-Id header and a problem answer as traceId, and the logs
// and incidents name (#150 R2, R10): the trace id of the request's own traceparent when it sends one, so the app's log and the API's join, or a
// new one. It also gives the request its empty Facts.
func Traced(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		trace, span := make([]byte, 16), make([]byte, 8)
		_, _ = rand.Read(trace)
		_, _ = rand.Read(span)
		traceID := hex.EncodeToString(trace)
		if sent := sentTrace.FindStringSubmatch(r.Header.Get("traceparent")); sent != nil && sent[1] != "00000000000000000000000000000000" {
			traceID = sent[1]
		}
		id := "00-" + traceID + "-" + hex.EncodeToString(span) + "-00"
		w.Header().Set("X-Trace-Id", id)
		ctx := context.WithValue(context.WithValue(r.Context(), traceKey{}, id), factsKey{}, &Facts{})
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// TraceOf is the trace id Traced gave the request, or "" outside one.
func TraceOf(ctx context.Context) string {
	id, _ := ctx.Value(traceKey{}).(string)
	return id
}
