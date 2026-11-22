// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// permissionWords is permissions named as the app names them in a request, apart by its comma.
func permissionWords(p page, permissions []access.Permission) string {
	names := make([]string, 0, len(permissions))
	for _, permission := range permissions {
		names = append(names, p.words.Of("permissions."+permissionName(permission)))
	}
	return strings.Join(names, "، ")
}

// askedFor is what a request asks as the app says it: a line naming the permissions given, a line naming those taken.
func askedFor(p page, gives, takes []access.Permission) []string {
	var lines []string
	if len(gives) > 0 {
		lines = append(lines, p.words.Of("permissionRequests.gives", permissionWords(p, gives)))
	}
	if len(takes) > 0 {
		lines = append(lines, p.words.Of("permissionRequests.takes", permissionWords(p, takes)))
	}
	return lines
}

// askedIn is a request as the app's PermissionRequestsScreen.askedIn says it, a line each: what it asks, why, and when, in the project's time.
func askedIn(p page, request users.RequestView, location *time.Location) string {
	return strings.Join(append(askedFor(p, request.Gives, request.Takes), request.Reason, request.AskedAt.In(location).Format("2006-01-02 15:04")), "\n")
}

// myPermissionsShown is My permissions as the app's screen shows it (redix-api #112): the search typed, the caller's latest request, asked while
// there is one, the partners and tables they see, the project's time, and what the last change said.
type myPermissionsShown struct {
	text     string
	request  users.RequestView
	asked    bool
	partners []partners.View
	tables   []entries.OfferedCategory
	location *time.Location
	said     said
}

// shows is the permissions My permissions lists: those whose name or hint holds the search, as the app's ListSearch keeps them.
func (s myPermissionsShown) shows(p page) []access.Permission {
	return slices.DeleteFunc(slices.Clone(permissionTicks), func(permission access.Permission) bool {
		name := permissionName(permission)
		return !searchHolds(s.text, p.words.Of("permissions."+name), p.words.Of("permissions."+name+"Hint"))
	})
}

// requestText is the caller's latest request as My permissions says it: where it stands, a rejection's reason in it, and while it waits what
// it asks, why and when.
func (s myPermissionsShown) requestText(p page) string {
	stands := p.words.Of("permissionRequests."+s.request.StatusName(), s.request.RejectionReason)
	if !s.request.IsWaiting() {
		return stands
	}
	return stands + "\n" + askedIn(p, s.request, s.location)
}

// myPermissionsOf is My permissions for p's caller with text and said, read through the rules the app's routes answer (users.MyRequestOf,
// partners.ListOf, entries.SightTablesOf). The error is the database's.
func (s site) myPermissionsOf(ctx context.Context, p page, text string, saying said) (myPermissionsShown, error) {
	read := s.Database.Queries()
	requests, err := users.MyRequestOf(ctx, read, p.project.ID, p.caller.UserID)
	choices, partnersErr := partners.ListOf(ctx, read, p.project.ID, p.caller)
	tables, tablesErr := entries.SightTablesOf(ctx, read, p.project.ID, p.caller.Access)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	shown := myPermissionsShown{text: strings.TrimSpace(text), partners: choices, tables: tables, location: zones.Location(zone), said: saying}
	if len(requests) > 0 {
		shown.request, shown.asked = requests[0], true
	}
	return shown, errors.Join(err, partnersErr, tablesErr, zoneErr)
}

// myPermissionsTab is My permissions in its tab (redix-api #112, #173), for everyone in the project; what Ask for permissions said once it was
// sent shows on it, as the app's screen shows its message.
func (s site) myPermissionsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.myPermissionsOf(r.Context(), p, r.URL.Query().Get("text"), p.said)
	return myPermissionsPage(p, shown), err
}

// withdrawRequest takes back the caller's waiting request through users.Withdraw, the rule the app's route runs, and draws My permissions again
// saying so, or why not; a finished project changes nothing, as the API refuses it.
func (s site) withdrawRequest(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	saying, err := s.askingChange(p, r, func(c changes.Change) error { return users.Withdraw(c) }, "permissionRequests.withdrawn")
	if err != nil {
		failed(w)
		return
	}
	shown, err := s.myPermissionsOf(r.Context(), p, "", saying)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, myPermissionsPage(p, shown))
}

// askingChange runs act as one change to p's project, saying done once kept or why it was refused in the app's words; a finished project changes
// nothing, as the API refuses a request's changes in it. The error is the database's.
func (s site) askingChange(p page, r *http.Request, act func(c changes.Change) error, done string) (said, error) {
	if p.finished() {
		return said{refusal: "permissionRequests.projectFinished"}, nil
	}
	err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, act)
	refusal, err := refusalOf("permissionRequests", err)
	if err != nil || refusal != "" {
		return said{refusal: refusal}, err
	}
	return said{notice: done}, nil
}

// wanted is the permissions Ask for permissions has ticked, sent as its form sends them, and what they give and take from those p's caller holds:
// Read project is held and asked of no one.
func wanted(p page, r *http.Request) (gives, takes []string) {
	ticked := r.Form["permission"]
	for _, permission := range permissionTicks {
		holds, wants := p.may(permission), slices.Contains(ticked, string(permission))
		switch {
		case permission == access.ReadProject:
		case wants && !holds:
			gives = append(gives, string(permission))
		case holds && !wants:
			takes = append(takes, string(permission))
		}
	}
	return gives, takes
}

// namedPermissions is the permissions names name, each known.
func namedPermissions(names []string) []access.Permission {
	var permissions []access.Permission
	for _, name := range names {
		if permission, known := access.Named(name); known {
			permissions = append(permissions, permission)
		}
	}
	return permissions
}

// askTab is Ask for permissions in its tab (redix-api #112), from My permissions: every project permission ticked as the caller holds it.
func (s site) askTab(_ *http.Request, p page) (templ.Component, error) {
	return askPage(p), nil
}

// askDialog is Ask for permissions' Send in the details panel, as the app's dialog asks it: the reason, below it what the ticks ask.
func (s site) askDialog(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	gives, takes := wanted(p, r)
	draw(w, r, http.StatusOK, askForm(p, askedFor(p, namedPermissions(gives), namedPermissions(takes)), "", ""))
}

// sendRequest keeps what Ask for permissions ticked, with its reason, through users.Ask, the rule the app's route runs, and brings My
// permissions to the front saying it was sent, Ask closed, as the app's screen pops; refused, the dialog says why in the app's words.
func (s site) sendRequest(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	gives, takes := wanted(p, r)
	saying, err := s.askingChange(p, r, func(c changes.Change) error {
		_, err := users.Ask(c, gives, takes, r.PostFormValue("reason"))
		return err
	}, "permissionRequests.sent")
	if err != nil {
		failed(w)
		return
	}
	if saying.refusal != "" {
		draw(w, r, http.StatusOK, askForm(p, askedFor(p, namedPermissions(gives), namedPermissions(takes)), r.PostFormValue("reason"), saying.refusal))
		return
	}
	w.Header().Set("HX-Retarget", "[id='web.content']")
	s.turnSaying(w, r, func(v view) view { return v.closing("permissionRequest").opening("myPermissions") }, saying)
}

// requestsShown is Permission requests as the app's screen lists them (redix-api #112): every request waiting, the project's time, what the last
// decision said and whose request it decided.
type requestsShown struct {
	waiting  []users.RequestView
	location *time.Location
	said     said
	decided  string
}

// requestOf is the request waiting with requestID.
func (s requestsShown) requestOf(requestID uuid.UUID) (users.RequestView, bool) {
	at := slices.IndexFunc(s.waiting, func(request users.RequestView) bool { return request.ID == requestID })
	if at < 0 {
		return users.RequestView{}, false
	}
	return s.waiting[at], true
}

// requestsOf is Permission requests with said, read through users.WaitingRequestsOf, the rule the app's route answers. The error is the
// database's.
func (s site) requestsOf(ctx context.Context, p page, saying said) (requestsShown, error) {
	read := s.Database.Queries()
	waiting, err := users.WaitingRequestsOf(ctx, read, p.project.ID)
	zone, zoneErr := zones.Of(ctx, read, p.project.ID)
	return requestsShown{waiting: waiting, location: zones.Location(zone), said: saying}, errors.Join(err, zoneErr)
}

// requestsTab is Permission requests in its tab (redix-api #112, #173), for whoever manages the project's users.
func (s site) requestsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.requestsOf(r.Context(), p, said{})
	return requestsList(p, shown), err
}

// rejectDialog is a request's Reject in the details panel, as the app's sheet asks it: who asked and what, and the reason. 404 without
// ManageUsers, or for no request waiting with that id.
func (s site) rejectDialog(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.may(access.ManageUsers) {
		http.NotFound(w, r)
		return
	}
	shown, err := s.requestsOf(r.Context(), p, said{})
	if err != nil {
		failed(w)
		return
	}
	requestID, _ := uuid.Parse(r.PathValue("id"))
	request, found := shown.requestOf(requestID)
	if !found {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, requestRejectForm(p, request, shown.location, ""))
}

// decideRequest approves the request the path names or rejects it with its reason, through users.Approve and users.Reject, the rules the app's
// routes run, and draws Permission requests again saying so, the details panel closed; a rejection refused keeps its sheet, saying why. 403
// without ManageUsers; 404 for an action there is none of, or no request waiting with that id.
func (s site) decideRequest(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	if !p.inProject() || r.ParseForm() != nil || (action != "approve" && action != "reject") {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageUsers) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.requestsOf(r.Context(), p, said{})
	if err != nil {
		failed(w)
		return
	}
	requestID, _ := uuid.Parse(r.PathValue("id"))
	request, found := shown.requestOf(requestID)
	if !found {
		http.NotFound(w, r)
		return
	}
	act, done := func(c changes.Change) error { return users.Approve(c, requestID) }, "permissionRequests.approvedDone"
	if action == "reject" {
		act, done = func(c changes.Change) error { return users.Reject(c, requestID, r.PostFormValue("reason")) }, "permissionRequests.rejectedDone"
	}
	saying, err := s.askingChange(p, r, act, done)
	if err != nil {
		failed(w)
		return
	}
	if action == "reject" && saying.refusal != "" {
		draw(w, r, http.StatusOK, requestRejectForm(p, request, shown.location, saying.refusal))
		return
	}
	if shown, err = s.requestsOf(r.Context(), p, saying); err != nil {
		failed(w)
		return
	}
	shown.decided = request.DisplayName
	if action == "reject" {
		draw(w, r, http.StatusOK, requestsBeside(p, shown))
		return
	}
	draw(w, r, http.StatusOK, requestsList(p, shown), detailsPanel(true))
}

// sees says whether p's caller sees tableID's entries.
func (p page) sees(tableID uuid.UUID) bool { return p.caller.Access.Sees(p.project.ID, tableID) }

// mineTablesHint is what My permissions says under its tables: that some are not the caller's, as the app says it, or nothing.
func mineTablesHint(p page) string {
	if p.caller.Access.SeesEveryTable(p.project.ID) {
		return ""
	}
	return p.words.Of("reports.periodicNotForYou")
}

// readOnlyTick, readOnlyChoice and readOnlyTables are what a tick does on My permissions: nothing, it only shows.
func readOnlyTick(access.Permission) templ.Attributes { return templ.Attributes{"disabled": true} }

func readOnlyChoice(uuid.UUID) templ.Attributes { return templ.Attributes{"disabled": true} }

func readOnlyTables([]uuid.UUID, bool) templ.Attributes { return templ.Attributes{"disabled": true} }

// everyPartner chooses every partner listed: My permissions lists the partners whose capital the caller sees.
func everyPartner(uuid.UUID) bool { return true }

// askedChanged grays Ask for permissions' Send until a tick differs from what the caller holds, as the app's button waits for something asked.
// A constant of our own, holding no person's data.
const askedChanged = `document.getElementById('permissionRequest.send').disabled = [...this.querySelectorAll('input[name=permission]')].every(box => box.checked === box.defaultChecked)`
