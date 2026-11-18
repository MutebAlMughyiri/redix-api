// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package apidoc_test

import (
	"encoding/json"
	"strings"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

func TestR2_SwaggerUIShowsTheOpenAPIDocumentOfEveryRoute(t *testing.T) {
	api := testapi.New(t)
	swagger := api.Send("GET", "/swagger", "")
	if swagger.Code != 200 || !strings.HasPrefix(swagger.Header().Get("Content-Type"), "text/html") || !strings.Contains(swagger.Body.String(), `url: "/openapi/v1.json"`) {
		t.Fatalf("/swagger answered %d %q", swagger.Code, swagger.Body.String())
	}
	answer := api.Send("GET", "/openapi/v1.json", "")
	var document struct {
		OpenAPI string                    `json:"openapi"`
		Paths   map[string]map[string]any `json:"paths"`
	}
	if answer.Code != 200 || answer.Header().Get("Content-Type") != "application/json" || json.Unmarshal(answer.Body.Bytes(), &document) != nil {
		t.Fatalf("/openapi/v1.json answered %d %s", answer.Code, answer.Header().Get("Content-Type"))
	}
	if document.OpenAPI != "3.1.0" || document.Paths["/projects/{projectId}/accounts"]["post"] == nil || document.Paths["/swagger"]["get"] == nil {
		t.Errorf("the document is OpenAPI %q with %d paths", document.OpenAPI, len(document.Paths))
	}
}
