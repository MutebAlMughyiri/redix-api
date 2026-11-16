// Package web owns how requests reach the API: every route at its old path and again under /api/v1 (R6), the front end's files for any other path
// (R18, G1), and the answers ASP.NET Core gave a path or a method it did not know (redix-api #140).
package web

import (
	"encoding/json"
	"net/http"
	"os"
	"path"
	"path/filepath"

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
}

// NewRouter is a router whose every other path is a file of frontEnd, the web front end's folder; "" when there is none yet.
func NewRouter(frontEnd string) *Router {
	return &Router{mux: http.NewServeMux(), frontEnd: frontEnd, files: http.FileServer(http.Dir(frontEnd))}
}

// Handle routes method and path, such as "GET" and "/projects/{projectId}", at the path and under /api/v1.
func (r *Router) Handle(method, path string, handler http.HandlerFunc) {
	r.mux.HandleFunc(method+" "+path, handler)
	r.mux.HandleFunc(method+" "+version+path, handler)
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

// ID is the GUID path value name; a value that is not a GUID answers 404, as the route constraint {name:guid} did.
func ID(w http.ResponseWriter, r *http.Request, name string) (uuid.UUID, bool) {
	id, err := uuid.Parse(r.PathValue(name))
	if err != nil || len(r.PathValue(name)) != 36 {
		problem.Status(w, r, http.StatusNotFound)
		return uuid.Nil, false
	}
	return id, true
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
