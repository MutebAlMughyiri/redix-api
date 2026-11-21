// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
)

// The session's cookies: the access token, 15 minutes, and the device's refresh token, 12 hours unused (#161 S2).
const (
	accessCookie  = "redix_access"
	refreshCookie = "redix_refresh"
)

// session is the caller r's cookies speak for. An access token found expired is renewed from the refresh cookie inside this request, and both
// cookies kept again (#161 S2); a session that cannot be renewed has its cookies cleared. false when no session is open; the error is the
// database's.
func (s site) session(w http.ResponseWriter, r *http.Request) (access.Caller, bool, error) {
	if token, err := r.Cookie(accessCookie); err == nil {
		caller, err := s.Gate.Check(r.Context(), token.Value)
		if err == nil {
			return caller, true, nil
		}
		if errors.Is(err, access.ErrDatabase) {
			return access.Caller{}, false, err
		}
	}
	refresh, err := r.Cookie(refreshCookie)
	if err != nil {
		return access.Caller{}, false, nil
	}
	opened, refusal, err := s.Sessions.Renew(r.Context(), refresh.Value)
	if err != nil {
		return access.Caller{}, false, err
	}
	if refusal != "" {
		forget(w)
		return access.Caller{}, false, nil
	}
	caller, err := s.Gate.Check(r.Context(), s.keep(w, opened))
	return caller, err == nil, err
}

// keep sets opened's session in the cookies a page's script cannot read and only this site is sent (#161 R17), and is its new access token.
func (s site) keep(w http.ResponseWriter, opened auth.Opened) string {
	token, expires := s.Sessions.AccessToken(opened)
	now := s.Clock.Now()
	setSession(w, accessCookie, token, expires.Sub(now))
	setSession(w, refreshCookie, opened.RefreshToken, opened.Device.RefreshExpiresAt.Sub(now))
	return token
}

// forget clears the session's cookies.
func forget(w http.ResponseWriter) {
	setSession(w, accessCookie, "", -time.Second)
	setSession(w, refreshCookie, "", -time.Second)
}

func setSession(w http.ResponseWriter, name, value string, lasts time.Duration) {
	http.SetCookie(w, &http.Cookie{Name: name, Value: value, Path: "/", MaxAge: int(lasts.Seconds()), HttpOnly: true, Secure: true,
		SameSite: http.SameSiteStrictMode})
}

// ended is the sign-in form's message for r: the session ended, when r still carried one, as the app says it (signIn.sessionExpired).
func ended(r *http.Request) string {
	if _, err := r.Cookie(refreshCookie); err == nil {
		return "signIn.sessionExpired"
	}
	return ""
}

// signIn signs in through the rule the app's route uses (UC-002, #161 R1, R4): the shell's parts arrive, the name and Sign out in the top bar
// (R6); a refusal answers the form again with the app's words for it, as 422, which htmx is told to swap (S4).
func (s site) signIn(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	username := r.PostFormValue("username")
	opened, refusal, err := s.Sessions.SignIn(r.Context(), username, r.PostFormValue("password"))
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusUnprocessableEntity, signInForm(p, "signIn."+refusal, username))
		return
	}
	s.arrive(w, r, p, opened)
}

// changePassword replaces a temporary password through the rule the app's route uses (UC-003, AC-02.1, #161 R13); the two new passwords must
// match first, as the app checks them.
func (s site) changePassword(w http.ResponseWriter, r *http.Request) {
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
	if r.PostFormValue("new") != r.PostFormValue("confirm") {
		draw(w, r, http.StatusUnprocessableEntity, changePasswordForm(p, "changePassword.mismatch", caller.Username))
		return
	}
	opened, refusal, err := s.Sessions.ReplacePassword(r.Context(), caller.Session, r.PostFormValue("current"), r.PostFormValue("new"))
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusUnprocessableEntity, changePasswordForm(p, "changePassword."+refusal, caller.Username))
		return
	}
	s.arrive(w, r, p, opened)
}

// arrive keeps opened's session in its cookies and answers the whole shell, its parts arriving with their motion.
func (s site) arrive(w http.ResponseWriter, r *http.Request, p page, opened auth.Opened) {
	caller, err := s.Gate.Check(r.Context(), s.keep(w, opened))
	if err == nil {
		p, err = s.open(r, p, caller)
	}
	if err != nil {
		failed(w)
		return
	}
	p.arriving = true
	reshell(w, r, p)
}

// signOut ends this browser's session through the rule the app's route uses (UC-062, #161 R16), clears its cookies, and answers the sign-in form.
func (s site) signOut(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	caller, signedIn, err := s.session(w, r)
	if err == nil && signedIn {
		err = s.Sessions.SignOut(r.Context(), caller.Session)
	}
	if err != nil {
		failed(w)
		return
	}
	forget(w)
	p.content = signInForm(p, "", "")
	reshell(w, r, p)
}

// working is r's page opened for its signed-in caller, their projects and the one they work in; false once it has answered - 500 for a failing
// database, the sign-in form in the whole shell for a session that ended.
func (s site) working(w http.ResponseWriter, r *http.Request) (page, bool) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return p, false
	}
	caller, signedIn, err := s.session(w, r)
	if err != nil {
		failed(w)
		return p, false
	}
	if !signedIn {
		p.content = signInForm(p, ended(r), "")
		reshell(w, r, p)
		return p, false
	}
	if p, err = s.open(r, p, caller); err != nil {
		failed(w)
		return p, false
	}
	return p, true
}

// reshell answers the whole shell in place of whatever part asked, without loading the page again (#161 R5, R6).
func reshell(w http.ResponseWriter, r *http.Request, p page) {
	w.Header().Set("HX-Retarget", "[id='web.shell']")
	w.Header().Set("HX-Reswap", "outerHTML")
	draw(w, r, http.StatusOK, shell(p))
}
