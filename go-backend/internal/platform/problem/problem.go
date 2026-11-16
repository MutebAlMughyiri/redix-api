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

// Status answers with a bare status, as an empty 404 became problem details through UseStatusCodePages.
func Status(w http.ResponseWriter, r *http.Request, status int) {
	write(w, r, status)
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
	w.Write(body.Bytes())
}

type traceKey struct{}

// Traced gives every request a W3C trace id, which a problem answer carries as traceId and the logs name.
func Traced(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		trace, span := make([]byte, 16), make([]byte, 8)
		rand.Read(trace)
		rand.Read(span)
		id := "00-" + hex.EncodeToString(trace) + "-" + hex.EncodeToString(span) + "-00"
		next.ServeHTTP(w, r.WithContext(context.WithValue(r.Context(), traceKey{}, id)))
	})
}

// TraceOf is the trace id Traced gave the request, or "" outside one.
func TraceOf(ctx context.Context) string {
	id, _ := ctx.Value(traceKey{}).(string)
	return id
}
