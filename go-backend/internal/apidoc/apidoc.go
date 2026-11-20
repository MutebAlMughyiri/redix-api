// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package apidoc serves the API's OpenAPI document, which cmd/openapi generates from the code, and Swagger UI to read every route and try it, in
// Dev and QA only, as .NET's MapOpenApi and UseSwaggerUI served them outside production (redix-api #142 R2, R4).
package apidoc

//go:generate go run ../../cmd/openapi

import (
	_ "embed"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

//go:embed openapi.json
var document []byte

//go:embed swagger.html
var page []byte

// Map routes GET /openapi/v1.json and GET /swagger, at the addresses .NET served them.
func Map(router *web.Router) {
	// The OpenAPI document of every route, generated from the code (redix-api #142 R1).
	router.Handle("GET", "/openapi/v1.json", served("application/json", document))
	// Swagger UI on the document, to read every route and try it with a session's access token (redix-api #142 R2).
	router.Handle("GET", "/swagger", served("text/html; charset=utf-8", page))
}

func served(contentType string, content []byte) http.HandlerFunc {
	return func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Content-Type", contentType)
		_, _ = w.Write(content)
	}
}
