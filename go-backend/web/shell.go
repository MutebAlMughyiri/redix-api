// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"strconv"
)

// home is the whole page: the shell as its cookies leave it, signed in, or with the sign-in form in its content area (#161 R5, R6).
func (s site) home(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err == nil {
		p, err = s.shellFor(w, r, p)
	}
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, document(p))
}

// shellFor is p as r's session leaves it: open for a signed-in caller, the sign-in form otherwise. The error is the database's.
func (s site) shellFor(w http.ResponseWriter, r *http.Request, p page) (page, error) {
	caller, signedIn, err := s.session(w, r)
	if err != nil || !signedIn {
		p.content = signInForm(p, ended(r), "")
		return p, err
	}
	return s.open(r, p, caller)
}

// openTab opens the tab named name and brings it to the front: its content in the content area, the tabs and the side menu marked beside it;
// only the content area changes, the frame stays (#161 R5).
func (s site) openTab(w http.ResponseWriter, r *http.Request) {
	name := r.PathValue("name")
	if _, built := contents[name]; !built {
		http.NotFound(w, r)
		return
	}
	s.turn(w, r, func(v view) view { return v.opening(name) })
}

// closeTab closes the tab named name; the last tab left comes to the front.
func (s site) closeTab(w http.ResponseWriter, r *http.Request) {
	name := r.PathValue("name")
	s.turn(w, r, func(v view) view { return v.closing(name) })
}

// turn changes the view's tabs with change, keeps it, and answers the front tab's content with the tabs and the side menu; a request with no session
// answers the sign-in form in the whole shell.
func (s site) turn(w http.ResponseWriter, r *http.Request, change func(view) view) {
	s.turnSaying(w, r, change, said{})
}

// turnSaying is turn, the tab it brings to the front told what the change that brought it there said, as a screen the app pops back to shows its
// message.
func (s site) turnSaying(w http.ResponseWriter, r *http.Request, change func(view) view, saying said) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	caller, signedIn, err := s.session(w, r)
	if err != nil {
		failed(w)
		return
	}
	if !signedIn {
		p.content = signInForm(p, ended(r), "")
		reshell(w, r, p)
		return
	}
	p.view = change(p.view)
	p.view.keepTabs(w)
	p.said = saying
	if p, err = s.open(r, p, caller); err != nil {
		failed(w)
		return
	}
	if !p.in {
		reshell(w, r, p)
		return
	}
	draw(w, r, http.StatusOK, p.content, tabs(p, true), itemLinks(p), detailsPanel(true))
}

// keepView keeps what the View menu, the fold arrows and the side menu's line chose: the bars, the side menu, its width (#161 R9, R10, R18). The
// shell already shows the choice; nothing is drawn.
func (s site) keepView(w http.ResponseWriter, r *http.Request) {
	v := viewOf(r)
	v.Top, v.Status, v.Side = r.PostFormValue("top") == "on", r.PostFormValue("status") == "on", r.PostFormValue("side") == "on"
	v.Summary = r.PostFormValue("summary") == "on"
	if width, err := strconv.Atoi(r.PostFormValue("width")); err == nil {
		v.Width = min(max(width, narrowest), widest)
	}
	v.keepLook(w)
	w.WriteHeader(http.StatusNoContent)
}

// chooseLanguage keeps the language chosen in Settings or at the sign-in form's foot (#161 R11, R12) and opens the page again in it: every word
// changes, so the whole page is drawn again, once.
func (s site) chooseLanguage(w http.ResponseWriter, r *http.Request) {
	language := r.PathValue("language")
	if language != "ar" && language != "en" {
		http.NotFound(w, r)
		return
	}
	setPreference(w, languageCookie, language)
	http.Redirect(w, r, "/", http.StatusSeeOther)
}
