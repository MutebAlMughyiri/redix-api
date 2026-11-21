// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"net/http"
	"strings"
)

// signInAgain is the sign-in form, back from Forgot password.
func (s site) signInAgain(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, signInForm(p, "", ""))
}

// forgotPassword is the Forgot password form, in the sign-in's place (UC-002, AC-02.6).
func (s site) forgotPassword(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, forgotPasswordForm(p, "", "", false))
}

// sendResetCode emails a reset code through the rule the app's route uses (AC-02.6): one answer whoever the login names, so it says nothing
// about which accounts exist (AC-02.7). One address and one login ask only so often (AC-02.8), the app's asks and the web's counted together;
// past that it refuses auth.tooManyRequests, keeping the code's fields when a code was sent before. The code is emailed once the answer is out,
// as the app's route emails it after answering 202.
func (s site) sendResetCode(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	login := strings.TrimSpace(r.PostFormValue("login"))
	if !s.EmailCodes.AllowsCaller(r) || !s.Codes.MayAskReset(login) {
		draw(w, r, http.StatusUnprocessableEntity, forgotPasswordForm(p, "auth.tooManyRequests", login, r.PostFormValue("sent") != ""))
		return
	}
	draw(w, r, http.StatusOK, forgotPasswordForm(p, "", login, true))
	_ = http.NewResponseController(w).Flush()
	s.Codes.SendResetCode(context.WithoutCancel(r.Context()), login, p.language)
}

// resetWithCode sets the new password with the emailed code through the rule the app's route uses (AC-02.6), the two new passwords matching
// first, as the app checks them; then the sign-in form says the password was changed.
func (s site) resetWithCode(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	login := r.PostFormValue("login")
	if r.PostFormValue("new") != r.PostFormValue("confirm") {
		draw(w, r, http.StatusUnprocessableEntity, forgotPasswordForm(p, "changePassword.mismatch", login, true))
		return
	}
	refusal, err := s.Codes.ResetWithCode(r.Context(), login, r.PostFormValue("code"), r.PostFormValue("new"))
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusUnprocessableEntity, forgotPasswordForm(p, "auth."+refusal, login, true))
		return
	}
	p.notice = "auth.passwordReset"
	draw(w, r, http.StatusOK, signInForm(p, "", ""))
}
