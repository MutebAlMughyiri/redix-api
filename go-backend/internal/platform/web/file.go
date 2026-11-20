// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"net/url"
	"strings"
)

// SendFile answers content of contentType as a file to keep, named name: the plain filename with what is not ASCII as underscores, and the
// name itself as UTF-8 (RFC 6266), the one way the API hands a file it makes.
func SendFile(w http.ResponseWriter, contentType, name string, content []byte) {
	w.Header().Set("Content-Type", contentType)
	w.Header().Set("Content-Disposition", "attachment; filename="+asciiName(name)+"; filename*=UTF-8''"+url.PathEscape(name))
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write(content)
}

// asciiName is a file's name as the plain filename of a Content-Disposition carries it: what is not ASCII as underscores.
func asciiName(name string) string {
	return strings.Map(func(letter rune) rune {
		if letter > 0x7E || letter < 0x20 {
			return '_'
		}
		return letter
	}, name)
}
