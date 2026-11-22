// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
)

// usersShown is Users as the app's screen shows it (UC-008): everyone in the project, the inactive last, by name, each with what they hold here,
// and the search typed.
type usersShown struct {
	all  []users.View
	text string
}

// heldHere is what person holds in the project Users lists: the one line of theirs users.ListOf answers.
func heldHere(person users.View) users.ProjectView {
	if len(person.Projects) == 0 {
		return users.ProjectView{}
	}
	return person.Projects[0]
}

// shown is what the search keeps: as the app's, it finds a person's name, username, mobile and role name.
func (s usersShown) shown() []users.View {
	return slices.DeleteFunc(slices.Clone(s.all), func(person users.View) bool {
		return !searchHolds(s.text, person.DisplayName, person.Username, person.Mobile, heldHere(person).RoleName)
	})
}

// personOf is the person userID, and whether the project holds them.
func (s usersShown) personOf(userID uuid.UUID) (users.View, bool) {
	at := slices.IndexFunc(s.all, func(person users.View) bool { return person.ID == userID })
	if at < 0 {
		return users.View{}, false
	}
	return s.all[at], true
}

// permissionCount is how many permissions person holds here, as the app's row counts them.
func permissionCount(p page, person users.View) string {
	return p.words.Of("permissions.count", strconv.Itoa(len(heldHere(person).Permissions)))
}

// usersOf is Users for p's caller with the search text, through the rule the app's route answers (users.ListOf). The error is the database's.
func (s site) usersOf(ctx context.Context, p page, text string) (usersShown, error) {
	all, err := users.ListOf(ctx, s.Database.Queries(), p.project.ID)
	return usersShown{all: all, text: strings.TrimSpace(text)}, err
}

// usersTab is Users in its tab (UC-008, redix-api #173), for whoever manages the project's users.
func (s site) usersTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.usersOf(r.Context(), p, r.URL.Query().Get("text"))
	return usersList(p, shown), err
}

// userTyped is a person as Edit's form types them.
type userTyped struct {
	name, mobile, email, roleName string
}

// userTypedOf is person as Edit's form starts, to change them.
func userTypedOf(person users.View) userTyped {
	return userTyped{name: person.DisplayName, mobile: person.Mobile, email: person.Email, roleName: heldHere(person).RoleName}
}

// userDetails is a person in the details panel, or one of its forms for ?asking=edit or reset; 404 for someone not in the project.
func (s site) userDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.may(access.ManageUsers) {
		http.NotFound(w, r)
		return
	}
	shown, err := s.usersOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	userID, _ := uuid.Parse(r.PathValue("id"))
	person, found := shown.personOf(userID)
	if !found {
		http.NotFound(w, r)
		return
	}
	switch asking := r.URL.Query().Get("asking"); asking {
	case "edit":
		draw(w, r, http.StatusOK, userForm(p, person, userTypedOf(person), ""))
	case "reset":
		draw(w, r, http.StatusOK, resetForm(p, person, ""))
	default:
		draw(w, r, http.StatusOK, userPanel(p, person, said{asking: asking}))
	}
}

// existingUserForm is Add an existing user in the details panel, for whoever manages the project's users.
func (s site) existingUserForm(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.may(access.ManageUsers) {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, existingForm(p, "", ""))
}

// keepUser runs what the panel asks of a person through the rules the app's routes run: Add an existing user (users.AddExisting), Edit - the
// name, mobile and email when one changed (users.EditDetails), then the role name when it changed (users.NameRole), as the app's dialog sends
// them -, Reset password (users.ResetTemporary), Disable (users.Disable) and Enable (users.Enable), in one change. The panel shows the person
// and Users beside it follows; refused, the panel says why in the app's words. 403 without ManageUsers; as the app's routes, a finished
// project's users stay manageable.
func (s site) keepUser(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	if !p.inProject() || r.ParseForm() != nil || !slices.Contains([]string{"", "edit", "reset", "disable", "enable"}, action) {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageUsers) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.usersOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	person := users.View{}
	if action != "" {
		userID, _ := uuid.Parse(r.PathValue("id"))
		var found bool
		if person, found = shown.personOf(userID); !found {
			http.NotFound(w, r)
			return
		}
	}
	typed := userTyped{name: r.PostFormValue("name"), mobile: r.PostFormValue("mobile"), email: r.PostFormValue("email"), roleName: r.PostFormValue("roleName")}
	userID := person.ID
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
		switch action {
		case "":
			userID, err = users.AddExisting(c, r.PostFormValue("username"))
		case "edit":
			was := userTypedOf(person)
			if typed.name != was.name || typed.mobile != was.mobile || typed.email != was.email {
				err = users.EditDetails(c, p.caller, userID, typed.name, typed.mobile, typed.email)
			}
			if err == nil && strings.TrimSpace(typed.roleName) != was.roleName {
				err = users.NameRole(c, userID, typed.roleName)
			}
		case "reset":
			err = users.ResetTemporary(c, p.caller, userID, r.PostFormValue("password"))
		case "disable":
			err = users.Disable(c, userID)
		case "enable":
			err = users.Enable(c, userID)
		}
		return err
	})
	refusal := ""
	var refused changes.Refusal
	switch {
	case errors.As(err, &refused) && refused.Code == "":
		refusal = "users.refused"
	case errors.As(err, &refused):
		refusal = "users." + refused.Code
	case err != nil:
		failed(w)
		return
	}
	if refusal != "" {
		switch action {
		case "":
			draw(w, r, http.StatusOK, existingForm(p, r.PostFormValue("username"), refusal))
		case "edit":
			draw(w, r, http.StatusOK, userForm(p, person, typed, refusal))
		case "reset":
			draw(w, r, http.StatusOK, resetForm(p, person, refusal))
		default:
			draw(w, r, http.StatusOK, userPanel(p, person, said{refusal: refusal}))
		}
		return
	}
	if shown, err = s.usersOf(r.Context(), p, ""); err != nil {
		failed(w)
		return
	}
	person, _ = shown.personOf(userID)
	draw(w, r, http.StatusOK, userPanel(p, person, said{}), usersBeside(p, shown))
}
