// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
)

// myEmailDialog is My email's first step in the details panel, as the app's dialog asks it: the new email, and Send code.
func (s site) myEmailDialog(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	draw(w, r, http.StatusOK, myEmailForm(p, "", ""))
}

// askToConfirmEmail emails a code to the email typed through auth.Codes.AskToConfirmEmail, the rule the app's route runs, and asks for it; one
// address asks only so often, the app's asks and the web's counted together (AC-02.8). Refused, the form says why in the app's words.
func (s site) askToConfirmEmail(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	typed := strings.TrimSpace(r.PostFormValue("email"))
	if !s.EmailCodes.AllowsCaller(r) {
		draw(w, r, http.StatusOK, myEmailForm(p, typed, "auth.tooManyRequests"))
		return
	}
	refusal, err := refusalOf("users", s.Codes.AskToConfirmEmail(r.Context(), p.caller.UserID, typed, p.language))
	switch {
	case err != nil:
		failed(w)
	case refusal != "":
		draw(w, r, http.StatusOK, myEmailForm(p, typed, refusal))
	default:
		draw(w, r, http.StatusOK, myEmailCodeForm(p, typed, ""))
	}
}

// confirmEmail makes the email the code was sent to the person's through auth.Codes.ConfirmEmail, the rule the app's route runs, and closes the
// details panel, Settings' list showing the new email (findings row 321); refused, the code's form says why in the app's words: another account took the email,
// or the code is not the one sent.
func (s site) confirmEmail(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	err := s.Codes.ConfirmEmail(r.Context(), p.caller.UserID, r.PostFormValue("code"))
	var refused changes.Refusal
	if errors.As(err, &refused) && refused.Status == http.StatusConflict {
		draw(w, r, http.StatusOK, myEmailCodeForm(p, r.PostFormValue("email"), "users.emailTaken"))
		return
	}
	refusal, err := refusalOf("auth", err)
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusOK, myEmailCodeForm(p, r.PostFormValue("email"), refusal))
		return
	}
	user, err := s.Database.Queries().UserWithID(r.Context(), p.caller.UserID)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, myEmailKept(user.Email))
}
