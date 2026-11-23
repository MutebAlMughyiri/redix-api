// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"strconv"
	"strings"
)

// filtersTrigger is when a filter bar draws its list again (redix-api #183 R6): a one-choice filter, Mine and the entry number at once; a
// filter of several choices and the period once their list closes, so a tick never draws the page again under the open list; the search as
// it is typed.
const filtersTrigger = "change from:[data-applies], toggle[newState=='closed'] from:.filter-list, input changed delay:400ms from:[name='text'], submit"

// filter is one filter of a list's filter bar as the owner approved it (redix-api #183): a button saying what it filters and what is chosen,
// standing out while it narrows the list, that opens its choices as one list. A one-choice filter's first choice is everything, which narrows
// nothing. Every choice keeps the element id it always had (R7).
type filter struct {
	id, label string
	choices   []filterChoice
	several   bool
}

// filterChoice is one choice of a filter: its element id, the field and value it sends, its word, and whether it is chosen.
type filterChoice struct {
	id, name, value, word string
	chosen                bool
}

// summary is what the button says is chosen beside its word, as the approved design says it: nothing while the filter narrows nothing; one or
// two choices by their words; more by how many.
func (f filter) summary(p page) string {
	var chosen []string
	for i, choice := range f.choices {
		if choice.chosen && (f.several || i > 0) {
			chosen = append(chosen, choice.word)
		}
	}
	if len(chosen) > 2 {
		return p.words.Of("filters.chosenCount", strconv.Itoa(len(chosen)))
	}
	return parts(chosen...)
}

// inputType is a choice's input: a check box among several, a radio button for one.
func (f filter) inputType() string {
	if f.several {
		return "checkbox"
	}
	return "radio"
}

// anchored is the CSS joining a filter's list to its button, under the button's id: a dot is no part of a CSS name.
func anchored(property, id string) string {
	return property + ": --" + strings.ReplaceAll(id, ".", "-")
}

// periodSummary is what the period's button says is chosen: from and to, either alone, or nothing.
func periodSummary(p page, from, to string) string {
	switch {
	case from != "" && to != "":
		return p.words.Of("history.fromTo", from, to)
	case from != "":
		return p.words.Of("filters.since", from)
	case to != "":
		return p.words.Of("filters.until", to)
	}
	return ""
}
