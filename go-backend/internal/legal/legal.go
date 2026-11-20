// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package legal owns what REDIX says about its use and a person's data: the privacy policy and the terms of use, one copy each in Arabic and
// English, served open to anyone in every environment, since the stores link to them and a person reads them before registering
// (redix-api #143 R3, R4, Q6); and the copyright notice every source file opens with (R1, R6).
package legal

import (
	_ "embed"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

//go:embed privacy.html
var privacy []byte

//go:embed terms.html
var terms []byte

// Map routes GET /privacy and GET /terms, open with no session.
func Map(router *web.Router) {
	router.Handle("GET", "/privacy", page(privacy))
	router.Handle("GET", "/terms", page(terms))
}

// page answers one of the two pages as it is kept.
func page(html []byte) http.HandlerFunc {
	return func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Content-Type", "text/html; charset=utf-8")
		_, _ = w.Write(html)
	}
}
