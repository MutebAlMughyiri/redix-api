// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/url"
	"slices"
	"strings"
)

// notFilters are the query keys that name what a tab shows, or how much of it, never what narrows its list (#180).
var notFilters = []string{"search", "opened", "pages", "before", "asking", "item", "contract", "user", "entry"}

// searchOpen says whether a tab's search shows: asked for, its redraws carrying search=on so typing never folds it, or carrying a filter, so no
// filter narrows a list out of sight (the owner, 23 Nov 2026, #180 R1, R4, R6).
func searchOpen(query url.Values) bool {
	if query.Get("search") == "on" {
		return true
	}
	for key, values := range query {
		if !slices.Contains(notFilters, key) && slices.ContainsFunc(values, func(value string) bool { return value != "" }) {
			return true
		}
	}
	return false
}

// searchAddress is where a page's search toggle at path goes: while closed, to the page with its search asked for, its field taking the cursor;
// while open, to the page alone, the search closed and its filters cleared (#180 R2, R5).
func searchAddress(path string, open bool) string {
	if open {
		return path
	}
	if strings.Contains(path, "?") {
		return path + "&search=on&opened=1"
	}
	return path + "?search=on&opened=1"
}
