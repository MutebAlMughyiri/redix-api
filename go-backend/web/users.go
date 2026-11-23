// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"net/url"
	"slices"
	"strconv"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// usersShown is Users as the app's screen shows it (UC-008): everyone in the project, the inactive last, by name, each with what they hold here,
// and the search typed.
type usersShown struct {
	all      []users.View
	text     string
	location *time.Location
}

// lastSeen is when person last signed in, in the project's time as the web says a moment, or that they have not yet, as the app's row says
// it (redix-api #182).
func (s usersShown) lastSeen(p page, person users.View) string {
	if person.LastSeenAt.Equal(moment.Never.Time) {
		return p.words.Of("users.neverSignedIn")
	}
	return p.words.Of("users.lastSeen", person.LastSeenAt.In(s.location).Format("2006-01-02 15:04"))
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
	read := s.Database.Queries()
	all, err := users.ListOf(ctx, read, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	return usersShown{all: all, text: strings.TrimSpace(text), location: zones.Location(zone)}, errors.Join(err, zoneErr)
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
	refusal, err := refusalOf("users", err)
	if err != nil {
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

// permissionTicks are a project's permissions in the order the app's ticks list them (Permission.inAProject): ReadProject first, every person
// holding it; never ApproveProjects, which is no project's.
var permissionTicks = []access.Permission{access.ReadProject, access.ManageUsers, access.SeeAllCapital, access.SeeChosenCapital, access.ManageProject,
	access.ManageStructure, access.ManageSettings, access.AddSupplier, access.ChargeAnyAccount, access.RecordEntries, access.ManageAnyEntry,
	access.DeleteEntries, access.RemoveApprovedEntries, access.AttachFiles, access.ApproveEntries, access.MoveMoney, access.SeeReports,
	access.GenerateReports, access.SeeProjectHistory, access.ManageProjectFiles}

// permissionName is permission's name as the app's words and keys spell it: RecordEntries is recordEntries.
func permissionName(permission access.Permission) string {
	name := string(permission)
	return strings.ToLower(name[:1]) + name[1:]
}

// permissionGroup is the word of the group a permission's tick opens, as the app's Permission.groupWordKey: the capital pair, the reports pair.
func permissionGroup(permission access.Permission) string {
	switch permission {
	case access.SeeAllCapital, access.SeeChosenCapital:
		return "permissions.capitalGroup"
	case access.SeeReports, access.GenerateReports:
		return "permissions.reportsGroup"
	}
	return ""
}

// opensGroup says whether permission's tick is the first of its group among those shown, so the group's heading goes above it.
func opensGroup(shown []access.Permission, permission access.Permission) bool {
	at := slices.Index(shown, permission)
	return permissionGroup(permission) != "" && (at == 0 || permissionGroup(shown[at-1]) != permissionGroup(permission))
}

// excludes is the id of the tick a capital tick takes away when ticked, as the app keeps one of the pair at most; "" for any other.
func excludes(prefix string, permission access.Permission) string {
	switch permission {
	case access.SeeAllCapital:
		return prefix + ".seeChosenCapital"
	case access.SeeChosenCapital:
		return prefix + ".seeAllCapital"
	}
	return ""
}

// newUserShown is New user as the app's screen asks it (UC-008): the person typed, the permissions, partners and tables ticked, the project's
// partners and tables to tick, and why it could not be kept.
type newUserShown struct {
	typed    users.NewPerson
	partners []partners.View
	tables   []entries.OfferedCategory
	refusal  string
}

// holds says whether New user's form has permission ticked: ReadProject always.
func (s newUserShown) holds(permission access.Permission) bool {
	return permission == access.ReadProject || slices.Contains(s.typed.Permissions, permission)
}

// chose says whether New user's form has partnerID chosen.
func (s newUserShown) chose(partnerID uuid.UUID) bool {
	return slices.Contains(s.typed.PartnerIDs, partnerID)
}

// sees says whether New user's form leaves tableID ticked, seen.
func (s newUserShown) sees(tableID uuid.UUID) bool {
	return !slices.Contains(s.typed.UnseenTableIDs, tableID)
}

// newUserOf is New user with typed and refusal, the partners and tables to tick read through the rules the app's routes answer (partners.ListOf,
// entries.SightTablesOf). The error is the database's.
func (s site) newUserOf(ctx context.Context, p page, typed users.NewPerson, refusal string) (newUserShown, error) {
	read := s.Database.Queries()
	listed, err := partners.ListOf(ctx, read, p.project.ID, p.caller)
	tables, tablesErr := entries.SightTablesOf(ctx, read, p.project.ID, p.caller.Access)
	return newUserShown{typed: typed, partners: listed, tables: tables, refusal: refusal}, errors.Join(err, tablesErr)
}

// newUserTab is New user in its tab (UC-008, redix-api #173), opened from Users.
func (s site) newUserTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.newUserOf(r.Context(), p, users.NewPerson{}, "")
	return newUserPage(p, shown), err
}

// createUser adds the person New user typed through users.Create, the rule the app's route runs: ReadProject and the permissions ticked, the
// partners chosen, the tables left unticked unseen, with a temporary password they change at their first sign-in. Kept, the form starts again
// and the person shows in the details panel; refused, the form says why in the app's words. 403 without ManageUsers.
func (s site) createUser(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageUsers) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.newUserOf(r.Context(), p, users.NewPerson{}, "")
	if err != nil {
		failed(w)
		return
	}
	form := r.PostForm
	typed := users.NewPerson{DisplayName: form.Get("name"), Username: form.Get("username"), Mobile: form.Get("mobile"), Email: form.Get("email"),
		TemporaryPassword: form.Get("password"), RoleName: form.Get("roleName")}
	for _, name := range form["permission"] {
		if permission, known := access.Named(name); known {
			typed.Permissions = append(typed.Permissions, permission)
		}
	}
	for _, id := range form["partner"] {
		if partnerID, err := uuid.Parse(id); err == nil {
			typed.PartnerIDs = append(typed.PartnerIDs, partnerID)
		}
	}
	for _, category := range shown.tables {
		for _, item := range category.Items {
			for _, table := range item.Tables {
				if !slices.Contains(form["seen"], table.ID.String()) {
					typed.UnseenTableIDs = append(typed.UnseenTableIDs, table.ID)
				}
			}
		}
	}
	var created uuid.UUID
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
		created, err = users.Create(c, typed)
		return err
	})
	refusal, err := refusalOf("users", err)
	switch {
	case err != nil:
		failed(w)
		return
	case refusal != "":
		typed.TemporaryPassword = ""
		shown.typed, shown.refusal = typed, refusal
		draw(w, r, http.StatusOK, newUserPage(p, shown))
		return
	}
	listed, err := s.usersOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	person, _ := listed.personOf(created)
	draw(w, r, http.StatusOK, newUserPage(p, shown), userBeside(p, person))
}

// tickGroups ties an item's tick to its tables' as the app's TableTicks does: the item's tick sets every table of its, and a table's tick leaves
// the item's ticked when every table is, unticked when none is, and between otherwise. A constant of our own, holding no person's data.
const tickGroups = `const box = event.target, group = box.closest('.tick-item'); if (!group) return; ` +
	`const tables = [...group.querySelectorAll('input[data-table]')], item = group.querySelector('input[data-item]'); ` +
	`if (box === item) tables.forEach(table => { table.checked = item.checked }); ` +
	`const seen = tables.filter(table => table.checked).length; item.checked = seen === tables.length; item.indeterminate = seen > 0 && seen < tables.length`

// capitalPair keeps one of the capital ticks at most, as the app's New user does: ticking one takes the other away.
const capitalPair = `const box = event.target; if (box.checked && box.dataset.excludes) document.getElementById(box.dataset.excludes).checked = false`

// refusalOf is why a change was refused, in the words of the screen whose words start with screen: its code, refused for one without; "" when
// it was kept. The error is the database's.
func refusalOf(screen string, err error) (string, error) {
	var refused changes.Refusal
	switch {
	case errors.As(err, &refused) && refused.Code == "":
		return screen + ".refused", nil
	case errors.As(err, &refused):
		return screen + "." + refused.Code, nil
	}
	return "", err
}

// formTick is what a permission's tick does on a form whose ticks' ids start with prefix, New user's and Ask for permissions': it is sent with
// the form, Read project always and fixed, ticking one of the capital pair taking the other away.
func formTick(prefix string) func(access.Permission) templ.Attributes {
	return func(permission access.Permission) templ.Attributes {
		attributes := templ.Attributes{"name": "permission", "value": string(permission), "disabled": permission == access.ReadProject}
		if excluded := excludes(prefix, permission); excluded != "" {
			attributes["data-excludes"] = excluded + ".input"
		}
		return attributes
	}
}

// newUserChoice is what a partner's tick does on New user: it is sent with the form.
func newUserChoice(partnerID uuid.UUID) templ.Attributes {
	return templ.Attributes{"name": "partner", "value": partnerID.String()}
}

// newUserTableTick is what a table's tick does on New user: it is sent with the form while ticked; an item's ticks its tables (tickGroups).
func newUserTableTick(tableIDs []uuid.UUID, table bool) templ.Attributes {
	if table {
		return templ.Attributes{"name": "seen", "value": tableIDs[0].String(), "data-table": true}
	}
	return templ.Attributes{"data-item": true}
}

// seenOf is how many of item's tables are ticked.
func seenOf(item entries.OfferedItem, sees func(uuid.UUID) bool) int {
	seen := 0
	for _, table := range item.Tables {
		if sees(table.ID) {
			seen++
		}
	}
	return seen
}

// tableIDsOf is the ids of item's tables, those its tick ticks.
func tableIDsOf(item entries.OfferedItem) []uuid.UUID {
	ids := make([]uuid.UUID, 0, len(item.Tables))
	for _, table := range item.Tables {
		ids = append(ids, table.ID)
	}
	return ids
}

// userPermissionsShown is a person's Permissions as the app's screen shows them (UC-008): the person, found while in the project, the project's
// partners and tables to tick, and why a tick could not be kept.
type userPermissionsShown struct {
	person   users.View
	found    bool
	partners []partners.View
	tables   []entries.OfferedCategory
	refusal  string
}

// holds says whether the person holds permission here.
func (s userPermissionsShown) holds(permission access.Permission) bool {
	return slices.Contains(heldHere(s.person).Permissions, permission)
}

// chose says whether the person sees partnerID's capital.
func (s userPermissionsShown) chose(partnerID uuid.UUID) bool {
	return slices.Contains(heldHere(s.person).PartnerIDs, partnerID)
}

// sees says whether the person sees tableID's entries.
func (s userPermissionsShown) sees(tableID uuid.UUID) bool {
	return !slices.Contains(heldHere(s.person).UnseenTables, tableID)
}

// tick is what a permission's tick does on Permissions: it is kept at once, given while ticked and taken while not.
func (s userPermissionsShown) tick(permission access.Permission) templ.Attributes {
	return s.keptAt("/permission/" + string(permission))
}

// choose is what a partner's tick does on Permissions: kept at once.
func (s userPermissionsShown) choose(partnerID uuid.UUID) templ.Attributes {
	return s.keptAt("/partner/" + partnerID.String())
}

// tickTables is what an item's or a table's tick does on Permissions: its tables kept at once, all an item's in one request, as the app sends them.
func (s userPermissionsShown) tickTables(tableIDs []uuid.UUID, _ bool) templ.Attributes {
	query := url.Values{}
	for _, id := range tableIDs {
		query.Add("table", id.String())
	}
	return s.keptAt("/tables?" + query.Encode())
}

// keptAt is a tick of the person's Permissions posted to path at once, held while ticked, the page drawn again as they then hold it.
func (s userPermissionsShown) keptAt(path string) templ.Attributes {
	return templ.Attributes{"name": "held", "value": "on", "hx-post": "/user/" + s.person.ID.String() + path, "hx-target": "[id='web.content']"}
}

// userPermissionsOf is userID's Permissions with refusal, the person, partners and tables read through the rules the app's routes answer
// (users.ListOf, partners.ListOf, entries.SightTablesOf). The error is the database's.
func (s site) userPermissionsOf(ctx context.Context, p page, userID uuid.UUID, refusal string) (userPermissionsShown, error) {
	read := s.Database.Queries()
	listed, err := s.usersOf(ctx, p, "")
	choices, partnersErr := partners.ListOf(ctx, read, p.project.ID, p.caller)
	tables, tablesErr := entries.SightTablesOf(ctx, read, p.project.ID, p.caller.Access)
	person, found := listed.personOf(userID)
	return userPermissionsShown{person: person, found: found, partners: choices, tables: tables, refusal: refusal}, errors.Join(err, partnersErr, tablesErr)
}

// userPermissionsTab is a person's Permissions in its tab (UC-008, redix-api #173), opened from their panel in Users.
func (s site) userPermissionsTab(r *http.Request, p page) (templ.Component, error) {
	userID, _ := uuid.Parse(r.URL.Query().Get("user"))
	shown, err := s.userPermissionsOf(r.Context(), p, userID, "")
	return userPermissionsPage(p, shown), err
}

// keepPermission gives the permission named in the path while its tick is held, or takes it, through users.Give and users.Take, the rules the
// app's routes run: taking Read project takes the person out of the project.
func (s site) keepPermission(w http.ResponseWriter, r *http.Request) {
	s.keepTick(w, r, func(c changes.Change, userID uuid.UUID, held bool) error {
		if held {
			return users.Give(c, userID, r.PathValue("permission"))
		}
		return users.Take(c, userID, r.PathValue("permission"))
	})
}

// keepChosenPartner shows the partner in the path to the person while its tick is held, or stops showing them, through users.Choose and
// users.Unchoose.
func (s site) keepChosenPartner(w http.ResponseWriter, r *http.Request) {
	s.keepTick(w, r, func(c changes.Change, userID uuid.UUID, held bool) error {
		partnerID, err := uuid.Parse(r.PathValue("partner"))
		if err != nil {
			return changes.NotFound
		}
		if held {
			return users.Choose(c, userID, partnerID)
		}
		return users.Unchoose(c, userID, partnerID)
	})
}

// keepTables ticks the tables in the query for the person while held, or unticks them, through users.SeeTables, all in one change.
func (s site) keepTables(w http.ResponseWriter, r *http.Request) {
	s.keepTick(w, r, func(c changes.Change, userID uuid.UUID, held bool) error {
		var tableIDs []uuid.UUID
		for _, id := range r.URL.Query()["table"] {
			tableID, err := uuid.Parse(id)
			if err != nil {
				return changes.NotFound
			}
			tableIDs = append(tableIDs, tableID)
		}
		return users.SeeTables(c, userID, held, tableIDs)
	})
}

// keepTick keeps one tick of a person's Permissions at once through act, held while ticked, and draws the page again as they now hold it, the
// person beside it in the details panel, as the app's screen keeps each tick; refused, the page says why in the app's words. 403 without
// ManageUsers; 404 for someone not in the project.
func (s site) keepTick(w http.ResponseWriter, r *http.Request, act func(c changes.Change, userID uuid.UUID, held bool) error) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	userID, err := uuid.Parse(r.PathValue("id"))
	if !p.inProject() || err != nil || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageUsers) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		return act(c, userID, r.PostForm.Get("held") == "on")
	})
	if errors.Is(err, changes.NotFound) {
		http.NotFound(w, r)
		return
	}
	refusal, err := refusalOf("users", err)
	if err != nil {
		failed(w)
		return
	}
	shown, err := s.userPermissionsOf(r.Context(), p, userID, refusal)
	if err != nil {
		failed(w)
		return
	}
	if !shown.found {
		draw(w, r, http.StatusOK, userPermissionsPage(p, shown), detailsPanel(true))
		return
	}
	draw(w, r, http.StatusOK, userPermissionsPage(p, shown), userBeside(p, shown.person))
}

// partialTicks shows an item's tick between ticked and not while only some of its tables are, as the app's TableTicks does: HTML holds that
// state only as a property, so it is set on every page htmx loads. A constant of our own, holding no person's data.
const partialTicks = `const loaded = event.target; [...loaded.querySelectorAll('input[data-partial]')].forEach(box => { box.indeterminate = true })`
