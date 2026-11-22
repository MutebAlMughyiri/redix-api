// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"slices"
	"strconv"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
)

// themeCookie keeps the appearance a person chose in Settings, light or dark; none follows the system, as the app's choice does (#175 D1).
// entriesCookie keeps how many entries the home lists and each Show more adds, as the app's LocalConfig keeps it (#175 D1).
const (
	themeCookie   = "redix_theme"
	entriesCookie = "redix_entries"
)

// themes are the appearances Settings offers, as the app's: the system's, light and dark.
var themes = []string{"system", "light", "dark"}

// entriesShownChoices are the counts Settings offers, as the app's LocalConfig.entriesShownChoices: 5 to 20, the first kept until one is chosen.
var entriesShownChoices = []int{5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}

// themeOf is the appearance r's person chose: light or dark, or "" for the system's.
func themeOf(r *http.Request) string {
	if chosen, err := r.Cookie(themeCookie); err == nil && (chosen.Value == "light" || chosen.Value == "dark") {
		return chosen.Value
	}
	return ""
}

// entriesShownOf is how many entries r's person chose the home to list, the app's first choice until they choose one.
func entriesShownOf(r *http.Request) int {
	if chosen, err := r.Cookie(entriesCookie); err == nil {
		if count, err := strconv.Atoi(chosen.Value); err == nil && slices.Contains(entriesShownChoices, count) {
			return count
		}
	}
	return entriesShownChoices[0]
}

// themeWord is the word of an appearance, as the app's segments name them.
func themeWord(theme string) string {
	return map[string]string{"system": "sharedSettings.themeSystem", "light": "sharedSettings.themeLight", "dark": "sharedSettings.themeDark"}[theme]
}

// settingsTab is Settings in its tab (redix-api #161, #175), with or without a project, as the app's screen: who is signed in, Switch project
// and the default project, Registered projects, the language, the appearance, how many entries the home lists, My email, Change password, My
// history, the privacy policy and the terms of use, Sign out, and the version.
func (s site) settingsTab(r *http.Request, p page) (templ.Component, error) {
	return s.settingsOf(r, p)
}

// settingsOf is Settings for p's caller as r's cookies keep their choices, with the email their account holds. The error is the database's.
func (s site) settingsOf(r *http.Request, p page) (templ.Component, error) {
	user, err := s.Database.Queries().UserWithID(r.Context(), p.caller.UserID)
	return settingsPage(p, themeOf(r), entriesShownOf(r), user.Email), err
}

// chooseTheme keeps the appearance chosen in Settings and opens the page again in it, as the language is chosen (#175 D1).
func (s site) chooseTheme(w http.ResponseWriter, r *http.Request) {
	theme := r.PathValue("theme")
	if !slices.Contains(themes, theme) {
		http.NotFound(w, r)
		return
	}
	setPreference(w, themeCookie, theme)
	http.Redirect(w, r, "/", http.StatusSeeOther)
}

// chooseEntriesShown keeps how many entries the home lists, chosen in Settings, and draws Settings again (#175 D1); 404 for a count Settings
// does not offer.
func (s site) chooseEntriesShown(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	count, err := strconv.Atoi(r.PostFormValue("count"))
	if err != nil || !slices.Contains(entriesShownChoices, count) {
		http.NotFound(w, r)
		return
	}
	setPreference(w, entriesCookie, strconv.Itoa(count))
	user, err := s.Database.Queries().UserWithID(r.Context(), p.caller.UserID)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, settingsPage(p, themeOf(r), count, user.Email))
}

// mayApproveProjects says whether p's caller approves the projects people register, as the app's session.may(approveProjects).
func (p page) mayApproveProjects() bool {
	return slices.Contains(p.caller.Access.Everywhere(), access.ApproveProjects)
}
