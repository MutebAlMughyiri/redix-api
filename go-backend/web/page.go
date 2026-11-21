// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net"
	"net/http"
	"net/url"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
)

// page is one drawing of the shell: who asks, in which language, and what its parts show.
type page struct {
	words    words
	language string
	host     string
	view     view
	assets   assets
	// person is the signed-in person's name; "" before anyone signs in.
	person string
	// in shows the side menu and the tabs: someone is signed in with no temporary password left to change (AC-02.1).
	in bool
	// arriving plays the parts' motion once, in the answer to signing in (#161 R6).
	arriving bool
	// notice is the word key the sign-in form says above its fields, once, in the answer that brought it: after a reset, auth.passwordReset.
	notice  string
	content templ.Component
}

// words are one language's words by key (translations); a key with no word shows itself, so a missing word is seen, never hidden.
type words map[string]string

// Of is key's word.
func (w words) Of(key string) string {
	if word, found := w[key]; found {
		return word
	}
	return key
}

// languageCookie keeps the language a person chose (#80 D11).
const languageCookie = "redix_language"

// languageOf is the language r asks in: the one its person chose, else the browser's, Arabic for any but English (web.Language).
func languageOf(r *http.Request) string {
	if chosen, err := r.Cookie(languageCookie); err == nil && (chosen.Value == "ar" || chosen.Value == "en") {
		return chosen.Value
	}
	return web.Language(r)
}

// page is the shell for r before anyone is known: its language and words, its view, and the host it answers on.
func (s site) page(r *http.Request) (page, error) {
	language := languageOf(r)
	rows, err := s.Database.Queries().WordsIn(r.Context(), language)
	if err != nil {
		return page{}, err
	}
	found := make(words, len(rows))
	for _, row := range rows {
		found[row.Key] = row.Value
	}
	host, _, err := net.SplitHostPort(r.Host)
	if err != nil {
		host = r.Host
	}
	p := page{words: found, language: language, host: host, view: viewOf(r), assets: s.assets}
	p.content = emptyContent(p)
	return p, nil
}

// open is p for caller: their name, and the shell's parts with the active tab, or the change-password form while a temporary password is left to
// change (AC-02.1, #161 R13). The error is the database's.
func (s site) open(r *http.Request, p page, caller access.Caller) (page, error) {
	user, err := s.Database.Queries().UserWithID(r.Context(), caller.UserID)
	if err != nil {
		return p, err
	}
	p.person = user.DisplayName
	if caller.MustChangePassword {
		p.content = changePasswordForm(p, "", caller.Username)
		return p, nil
	}
	p.in = true
	if tab, opened := contents[p.view.Active]; opened {
		p.content = tab(p)
	}
	return p, nil
}

// direction is the way p's language reads.
func (p page) direction() string {
	if p.language == "ar" {
		return "rtl"
	}
	return "ltr"
}

// foldArrow folds the side menu toward its edge: the menu stands on the right in Arabic, on the left in English (#161 R7).
func (p page) foldArrow() string {
	if p.language == "ar" {
		return "»"
	}
	return "«"
}

// unfoldArrow brings the side menu back.
func (p page) unfoldArrow() string {
	if p.language == "ar" {
		return "«"
	}
	return "»"
}

// shortcut is key's word with the shortcut that does the same, kept whole inside Arabic text.
func (p page) shortcut(key string) string { return p.words.Of(key) + " \u2068(Alt+1)\u2069" }

// menuItem is an item of the side menu: its tab's name and its title's word.
type menuItem struct{ name, title string }

// built says whether a feature opens item's tab; an item none opens stands gray (#161 R15).
func (item menuItem) built() bool {
	_, built := contents[item.name]
	return built
}

// sideMenu is the side menu's items, in the order the app's drawer lists them (#161, the shell).
var sideMenu = []menuItem{
	{"home", "web.home"}, {"entries", "entries.title"}, {"approvals", "approvals.title"}, {"accounts", "accounts.title"}, {"partners", "partners.title"},
	{"contracts", "contracts.title"}, {"reports", "reports.title"}, {"history", "history.projectTitle"}, {"settings", "sharedSettings.title"},
}

// contents are what each built feature's tab shows, by its item's name; an item with none stands gray until its feature is built (#161 R15).
var contents = map[string]func(page) templ.Component{"settings": settingsTab}

// openTabs are the open tabs' items, in the order they were opened.
func (p page) openTabs() []menuItem {
	var open []menuItem
	for _, name := range p.view.Tabs {
		for _, item := range sideMenu {
			if item.name == name {
				open = append(open, item)
			}
		}
	}
	return open
}

// logoSVG is REDIX's mark, written once: drawn in the top bar, and the browser tab's icon.
const logoSVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" aria-hidden="true"><rect width="1024" height="1024" fill="#0E6B68"/>` +
	`<g stroke="#FFFFFF" fill="none"><path d="M276 430 L512 250 L748 430" stroke-width="76" stroke-linecap="round" stroke-linejoin="round"/>` +
	`<path d="M410 444 V780" stroke-width="96"/><path d="M410 492 H560 A104 104 0 0 1 560 700 H410" stroke-width="96"/>` +
	`<path d="M536 700 L640 780" stroke-width="96"/></g></svg>`

// logoIcon is the browser tab's icon: the logo inside the page, so no request asks for /favicon.ico.
var logoIcon = templ.SafeURL("data:image/svg+xml," + url.PathEscape(logoSVG))

// swapOutOfBand marks a part an answer carries beside its content, so htmx puts it in its place by its id.
func swapOutOfBand(outOfBand bool) templ.Attributes {
	if outOfBand {
		return templ.Attributes{"hx-swap-oob": "true"}
	}
	return templ.Attributes{}
}
