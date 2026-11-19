// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package web owns how requests reach the API: every route at its old path and again under /api/v1 (R6), the front end's files for any other path
// (R18, G1), and the answers ASP.NET Core gave a path or a method it did not know (redix-api #140).
package web

import (
	"cmp"
	"encoding/json"
	"io"
	"mime"
	"net/http"
	"net/url"
	"os"
	"path"
	"path/filepath"
	"reflect"
	"regexp"
	"slices"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/google/uuid"
)

// version is the prefix every route is also served under.
const version = "/api/v1"

// Router holds the API's routes and the front end's folder.
type Router struct {
	mux      *http.ServeMux
	frontEnd string
	files    http.Handler
	checks   map[string]map[string]func(string) bool
}

// NewRouter is a router whose every other path is a file of frontEnd, the web front end's folder; "" when there is none yet.
func NewRouter(frontEnd string) *Router {
	return &Router{mux: http.NewServeMux(), frontEnd: frontEnd, files: http.FileServer(http.Dir(frontEnd)), checks: map[string]map[string]func(string) bool{}}
}

// Handle routes method and path at the path and under /api/v1. A path names its values as .NET did, with their constraints: "/projects/{projectId:guid}",
// "/periodic-reports/{number:int}". A value that does not fit answers 404 before anything else, as a route that did not match did.
func (r *Router) Handle(method, path string, handler http.HandlerFunc) {
	pattern, constraints := constrained(path)
	checked := func(w http.ResponseWriter, request *http.Request) {
		for name, fits := range constraints {
			if !fits(request.PathValue(name)) {
				problem.Status(w, request, http.StatusNotFound)
				return
			}
		}
		handler(w, request)
	}
	r.mux.HandleFunc(method+" "+pattern, checked)
	r.mux.HandleFunc(method+" "+version+pattern, checked)
	r.checks[method+" "+pattern] = constraints
}

// constraints are the route constraints the API's paths use.
var constraints = map[string]func(string) bool{
	"guid": func(value string) bool { _, err := uuid.Parse(value); return err == nil && len(value) == 36 },
	"int":  func(value string) bool { _, err := strconv.ParseInt(value, 10, 32); return err == nil },
}

// constrained is path as the standard mux reads it, "{projectId}", and the constraint of each value that has one.
func constrained(path string) (string, map[string]func(string) bool) {
	found := map[string]func(string) bool{}
	pattern := routeValue.ReplaceAllStringFunc(path, func(value string) string {
		parts := routeValue.FindStringSubmatch(value)
		if parts[2] != "" {
			found[parts[1]] = constraints[parts[2]]
		}
		return "{" + parts[1] + "}"
	})
	return pattern, found
}

var routeValue = regexp.MustCompile(`\{(\w+)(?::(guid|int))?}`)

// Handles says whether a route answers method on path, so a replay can tell the routes already on Go from the rest.
func (r *Router) Handles(method, path string) bool {
	_, pattern := r.mux.Handler(&http.Request{Method: method, URL: &url.URL{Path: path}})
	if pattern == "" {
		return false
	}
	routeSegments, pathSegments := strings.Split(strings.SplitN(pattern, " ", 2)[1], "/"), strings.Split(path, "/")
	for i, segment := range routeSegments {
		name := strings.TrimSuffix(strings.TrimPrefix(segment, "{"), "}")
		if fits, constrained := r.checks[pattern][name]; constrained && i < len(pathSegments) && !fits(pathSegments[i]) {
			return false
		}
	}
	return true
}

// ServeHTTP answers a route, then a front end file, then what ASP.NET Core answered: 405 for a known path with another method, 404 for the rest.
func (r *Router) ServeHTTP(w http.ResponseWriter, request *http.Request) {
	if _, pattern := r.mux.Handler(request); pattern != "" {
		r.mux.ServeHTTP(w, request)
		return
	}
	if r.serveFile(w, request) {
		return
	}
	r.mux.ServeHTTP(&problemOnly{ResponseWriter: w, request: request}, request)
}

// serveFile serves the front end's file at the request's path, or a folder's index.html, when there is one.
func (r *Router) serveFile(w http.ResponseWriter, request *http.Request) bool {
	if r.frontEnd == "" || (request.Method != http.MethodGet && request.Method != http.MethodHead) {
		return false
	}
	file := filepath.Join(r.frontEnd, filepath.FromSlash(path.Clean("/"+request.URL.Path)))
	info, err := os.Stat(file)
	if err == nil && info.IsDir() {
		_, err = os.Stat(filepath.Join(file, "index.html"))
	}
	if err != nil {
		return false
	}
	r.files.ServeHTTP(w, request)
	return true
}

// problemOnly turns the mux's own plain-text 404 and 405 into problem details, keeping its Allow header.
type problemOnly struct {
	http.ResponseWriter
	request *http.Request
	written bool
}

func (p *problemOnly) WriteHeader(status int) {
	p.written = true
	p.ResponseWriter.Header().Del("Content-Type")
	p.ResponseWriter.Header().Del("X-Content-Type-Options")
	problem.Status(p.ResponseWriter, p.request, status)
}

func (p *problemOnly) Write(body []byte) (int, error) {
	if !p.written {
		p.WriteHeader(http.StatusOK)
	}
	return len(body), nil
}

// ID is the GUID route value name, which the route's {name:guid} constraint has already checked.
func ID(r *http.Request, name string) uuid.UUID { return uuid.MustParse(r.PathValue(name)) }

// Int is the path's whole number named name, which its route's int constraint let through.
func Int(r *http.Request, name string) int64 {
	number, _ := strconv.ParseInt(r.PathValue(name), 10, 32)
	return number
}

// JSON answers status with value as JSON, as Results.Ok and Results.Json wrote it.
func JSON(w http.ResponseWriter, r *http.Request, status int, value any) {
	body, err := json.Marshal(value)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	w.Write(body)
}

// Read decodes the request's JSON body into request, a struct whose every field is required unless tagged omitzero, as .NET bound a request record
// with RespectRequiredConstructorParameters and RespectNullableAnnotations, its init properties with defaults being the optional ones: no body at all
// answers 400 before its type is looked at, as ASP.NET Core answered a missing body; a body that is not JSON answers 415; a missing or null required
// field and a value of the wrong type answer 400; names match whatever their case, and fields the struct does not have are ignored.
func Read(w http.ResponseWriter, r *http.Request, request any) bool {
	body, err := io.ReadAll(r.Body)
	if err != nil || len(body) == 0 {
		problem.Status(w, r, http.StatusBadRequest)
		return false
	}
	if mediaType, _, _ := mime.ParseMediaType(r.Header.Get("Content-Type")); mediaType != "application/json" {
		problem.Status(w, r, http.StatusUnsupportedMediaType)
		return false
	}
	var sent map[string]json.RawMessage
	if json.Unmarshal(body, &sent) != nil || !hasEvery(sent, request) || json.Unmarshal(body, request) != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return false
	}
	return true
}

// hasEvery says whether sent names every required field of request, none of them null; an embedded struct's fields count as request's own, as
// encoding/json reads them.
func hasEvery(sent map[string]json.RawMessage, request any) bool {
	return hasFields(sent, reflect.TypeOf(request).Elem())
}

func hasFields(sent map[string]json.RawMessage, fields reflect.Type) bool {
	for field := range fields.Fields() {
		if field.Anonymous && field.Type.Kind() == reflect.Struct {
			if !hasFields(sent, field.Type) {
				return false
			}
			continue
		}
		name, options, _ := strings.Cut(field.Tag.Get("json"), ",")
		if options == "omitzero" {
			continue
		}
		found := false
		for key, value := range sent {
			if strings.EqualFold(key, name) && string(value) != "null" {
				found = true
			}
		}
		if !found {
			return false
		}
	}
	return true
}

// Camel is a name as .NET's JsonNamingPolicy.CamelCase wrote an enum value or a refusal's code: its first letter lower case, "approveProjects".
func Camel(name string) string {
	if name == "" {
		return name
	}
	return strings.ToLower(name[:1]) + name[1:]
}

// Language is the language the caller asked for, as RequestLocalization chose it: "en" when English comes first among Accept-Language's first three
// tags by preference, "ar" otherwise - Arabic is the default.
func Language(r *http.Request) string {
	type tag struct {
		name    string
		quality float64
	}
	var tags []tag
	for part := range strings.SplitSeq(r.Header.Get("Accept-Language"), ",") {
		name, parameters, _ := strings.Cut(strings.TrimSpace(part), ";")
		quality := 1.0
		if value, ok := strings.CutPrefix(strings.TrimSpace(parameters), "q="); ok {
			quality, _ = strconv.ParseFloat(value, 64)
		}
		if name != "" {
			tags = append(tags, tag{strings.ToLower(name), quality})
		}
	}
	slices.SortStableFunc(tags, func(a, b tag) int { return cmp.Compare(b.quality, a.quality) })
	for _, asked := range tags[:min(3, len(tags))] {
		for _, language := range []string{"ar", "en"} {
			if asked.name == language || strings.HasPrefix(asked.name, language+"-") {
				return language
			}
		}
	}
	return "ar"
}
