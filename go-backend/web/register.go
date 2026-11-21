// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
)

// listWords are the words and ids of the form's two lists, as the app's register form names them: its heading, its field, its Add, and the name
// its values are sent under.
var listWords = map[string]struct{ heading, name, add, field, title string }{
	"units":    {"projects.units", "projects.unitName", "projects.addUnit", "unit", "Unit"},
	"partners": {"partners.title", "partners.name", "partners.new", "partner", "Partner"},
}

// registering is the Register new project form as typed so far: what goes into projects.Registration, the units and partners added one at a time.
type registering struct {
	name, description, startDate, displayName, username, mobile string
	units, partners                                             []string
}

// registeringOf is the form r carries; the start date today's in the server's clock when none was chosen, as the app opens on today.
func (s site) registeringOf(r *http.Request) registering {
	_ = r.ParseForm()
	typed := registering{name: r.PostFormValue("name"), description: r.PostFormValue("description"), startDate: r.PostFormValue("startDate"),
		displayName: r.PostFormValue("displayName"), username: r.PostFormValue("username"), mobile: r.PostFormValue("mobile"),
		units: r.PostForm["unit"], partners: r.PostForm["partner"]}
	if typed.startDate == "" {
		typed.startDate = s.Clock.Now().Format(time.DateOnly)
	}
	return typed
}

// registerForm is Register new project, in the sign-in's place (UC-066, redix-api #164).
func (s site) registerForm(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, registerProject(p, "", registering{startDate: s.Clock.Now().Format(time.DateOnly)}))
}

// listed answers a list of the form - units or partners - with the one typed added, or the one named removed, its typing field emptied and
// waiting for the next, as the app's Add unit and swipe do; nothing is kept until Send for approval.
func (s site) listed(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	list := r.PathValue("list")
	named, known := listWords[list]
	if !known {
		http.NotFound(w, r)
		return
	}
	field := named.field
	typed, removing := strings.TrimSpace(r.PostFormValue(field+"Name")), r.PostFormValue("remove")
	items := r.PostForm[field]
	if removed, err := strconv.Atoi(removing); err == nil {
		if removed >= 0 && removed < len(items) {
			items = slices.Delete(slices.Clone(items), removed, removed+1)
		}
	} else if typed != "" {
		items = append(items, typed)
	}
	draw(w, r, http.StatusOK, registerList(p, list, items, true))
}

// register sends the project for approval through the rule the app's registration uses, one address only so often, the app's asks counted with
// the web's (UC-066, redix-api #164): the sign-in form saying it was sent, or the form again with the app's words for the refusal, as 422.
func (s site) register(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	typed := s.registeringOf(r)
	if !s.Registrations.AllowsCaller(r) {
		draw(w, r, http.StatusUnprocessableEntity, registerProject(p, "register.tooManyTries", typed))
		return
	}
	startDate, _ := time.Parse(time.DateOnly, typed.startDate)
	_, _, refusal, err := projects.Register(r.Context(), s.Database, s.Clock.Now(), projects.Registration{
		Start:       projects.Start{Name: typed.name, StartDate: moment.Date{Time: startDate}, Units: typed.units, Description: typed.description, Partners: typed.partners},
		DisplayName: typed.displayName, Username: typed.username, Mobile: typed.mobile, Password: r.PostFormValue("password")})
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" {
		draw(w, r, http.StatusUnprocessableEntity, registerProject(p, "register."+refusal, typed))
		return
	}
	p.notice = "register.sent"
	draw(w, r, http.StatusOK, signInForm(p, "", ""))
}
