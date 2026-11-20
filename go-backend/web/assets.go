// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"bytes"
	"compress/gzip"
	"crypto/sha256"
	"embed"
	"encoding/hex"
	"net/http"
	"path"
	"strings"

	"github.com/a-h/templ"
)

// files are the web's own files, in the binary: the stylesheet and htmx 2.0.11, the one script (#80 D9, R19).
//
//go:embed assets
var files embed.FS

// asset is one embedded file as it is served: its body, gzipped once, and its type.
type asset struct {
	body, gzipped []byte
	kind          string
}

// assets are the embedded files by their address, which carries their hash: a changed file is a new address, so each is cached a year (#80 D9).
type assets struct {
	byAddress map[string]asset
	css, htmx templ.SafeURL
}

// kinds are the types of the files the web serves.
var kinds = map[string]string{".css": "text/css; charset=utf-8", ".js": "text/javascript; charset=utf-8"}

// loadAssets reads and gzips every embedded file once, as the server starts.
func loadAssets() assets {
	found := assets{byAddress: map[string]asset{}}
	address := func(name string) templ.SafeURL {
		body, err := files.ReadFile("assets/" + name)
		if err != nil {
			panic(err)
		}
		sum := sha256.Sum256(body)
		extension := path.Ext(name)
		served := strings.TrimSuffix(name, extension) + "." + hex.EncodeToString(sum[:4]) + extension
		var gzipped bytes.Buffer
		writer, _ := gzip.NewWriterLevel(&gzipped, gzip.BestCompression)
		_, _ = writer.Write(body)
		_ = writer.Close()
		found.byAddress[served] = asset{body: body, gzipped: gzipped.Bytes(), kind: kinds[extension]}
		return templ.SafeURL("/assets/" + served)
	}
	found.css, found.htmx = address("redix.css"), address("htmx.min.js")
	return found
}

// serve answers the file at its address, gzipped for a browser that takes it, to be kept a year.
func (a assets) serve(w http.ResponseWriter, r *http.Request) {
	file, found := a.byAddress[r.PathValue("file")]
	if !found {
		http.NotFound(w, r)
		return
	}
	w.Header().Set("Content-Type", file.kind)
	w.Header().Set("Cache-Control", "public, max-age=31536000, immutable")
	w.Header().Set("Vary", "Accept-Encoding")
	body := file.body
	if strings.Contains(r.Header.Get("Accept-Encoding"), "gzip") {
		w.Header().Set("Content-Encoding", "gzip")
		body = file.gzipped
	}
	_, _ = w.Write(body)
}
