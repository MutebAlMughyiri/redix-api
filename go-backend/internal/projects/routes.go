// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package projects

import (
	"context"
	"database/sql"
	"errors"
	"net/http"
	"strings"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/suppliers"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/users"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
	"github.com/google/uuid"
)

// Routes keeps the projects themselves.
type Routes struct {
	database db.DB
	book     *access.Book
	clock    clock.Clock
}

// Map routes the projects: listing and starting them for a working session; registering one with no session, perAddress times an hour; their
// name, description, units and closing for whoever manages the project; the default project; and for the admin, the projects waiting and the
// decision on each.
func Map(router *web.Router, gate *access.Gate, database db.DB, book *access.Book, clock clock.Clock, perAddress *limits.Window) {
	p := Routes{database: database, book: book, clock: clock}
	router.Handle("GET", "/projects", gate.Working(p.list))
	router.Handle("POST", "/projects", gate.Working(p.create))
	router.Handle("POST", "/projects/register", perAddress.PerAddress(p.register))
	router.Handle("GET", "/projects/registered", gate.Needs(access.ApproveProjects, p.waiting))
	router.Handle("POST", "/projects/{projectId:guid}/approve", gate.Needs(access.ApproveProjects, p.approve))
	router.Handle("POST", "/projects/{projectId:guid}/reject", gate.Needs(access.ApproveProjects, p.reject))
	manage := func(handler access.Handler) http.HandlerFunc { return gate.Needs(access.ManageProject, handler) }
	router.Handle("PUT", "/projects/{projectId:guid}", manage(p.rename))
	router.Handle("PUT", "/projects/{projectId:guid}/description", manage(p.describe))
	router.Handle("PUT", "/projects/{projectId:guid}/time-zone", manage(p.setTimeZone))
	router.Handle("GET", "/time-zones", gate.Working(p.timeZones))
	router.Handle("POST", "/projects/{projectId:guid}/units", manage(p.addUnit))
	router.Handle("PUT", "/projects/{projectId:guid}/units/{unitId:guid}", manage(p.renameUnit))
	router.Handle("DELETE", "/projects/{projectId:guid}/units/{unitId:guid}", manage(p.deleteUnit))
	router.Handle("POST", "/projects/{projectId:guid}/close", manage(p.close))
	router.Handle("POST", "/projects/{projectId:guid}/default", gate.Needs(access.ReadProject, p.setDefault))
	router.Handle("DELETE", "/projects/default", gate.Working(p.removeDefault))
}

// Unit is one of a project's units (UnitView).
type Unit struct {
	ID   uuid.UUID `json:"id"`
	Name string    `json:"name"`
}

// View is a project as the person asking sees it (ProjectView), on the phone and on the web: what they hold in it - nothing while inactive there -
// and their role name.
type View struct {
	ID              uuid.UUID           `json:"id"`
	Name            string              `json:"name"`
	StartDate       moment.Date         `json:"startDate"`
	Status          string              `json:"status"`
	RejectionReason string              `json:"rejectionReason"`
	Description     string              `json:"description"`
	Units           []Unit              `json:"units"`
	Permissions     []access.Permission `json:"permissions"`
	RoleName        string              `json:"roleName"`
	IsInactive      bool                `json:"isInactive"`
	HoldsEntries    bool                `json:"holdsEntries"`
	TimeZone        string              `json:"timeZone"`
	UTCOffset       int                 `json:"utcOffsetMinutes"`
	// UnseenTables are the tables unticked for the person, the groups and contracts they do not see (#106 D9).
	UnseenTables []uuid.UUID `json:"unseenTables"`
}

// sight is what the person asking holds and is, which every view of a project reads.
type sight struct {
	access    access.Access
	roleNames map[uuid.UUID]string
	inactive  map[uuid.UUID]bool
	holding   map[uuid.UUID]bool
	zones     map[uuid.UUID]string
	now       time.Time
}

// zoneOf is the zone the project is in: the one its manager chose, Riyadh for one that chose none (redix-app #107).
func (s sight) zoneOf(projectID uuid.UUID) string {
	if zone, chosen := s.zones[projectID]; chosen {
		return zone
	}
	return zones.Riyadh
}

// sightOf is what userID holds and is, read through book at now.
func sightOf(ctx context.Context, read *store.Queries, book *access.Book, now time.Time, userID uuid.UUID) (sight, error) {
	held, err := book.Of(ctx, userID)
	if err != nil {
		return sight{}, err
	}
	named, err := read.RoleNamesOfUser(ctx, userID)
	if err != nil {
		return sight{}, err
	}
	inactive, err := read.InactiveProjectsOf(ctx, userID)
	if err != nil {
		return sight{}, err
	}
	holding, err := read.ProjectsHoldingEntries(ctx)
	if err != nil {
		return sight{}, err
	}
	chosen, err := read.ChosenTimeZones(ctx)
	if err != nil {
		return sight{}, err
	}
	s := sight{access: held, roleNames: map[uuid.UUID]string{}, inactive: map[uuid.UUID]bool{}, holding: map[uuid.UUID]bool{}, zones: map[uuid.UUID]string{},
		now: now}
	for _, row := range chosen {
		s.zones[row.ProjectID] = row.TimeZone
	}
	for _, row := range named {
		s.roleNames[row.ProjectID] = row.RoleName
	}
	for _, id := range inactive {
		s.inactive[id] = true
	}
	for _, id := range holding {
		s.holding[id] = true
	}
	return s, nil
}

func (s sight) view(project store.Project, units []store.Unit) View {
	var views []Unit
	for _, unit := range units {
		views = append(views, Unit{ID: unit.ID, Name: unit.Name})
	}
	permissions := s.access.In(project.ID)
	if s.inactive[project.ID] {
		permissions = nil
	}
	return View{ID: project.ID, Name: project.Name, StartDate: project.StartDate, Status: statusNames[project.Status], RejectionReason: project.RejectionReason,
		Description: project.Description, Units: views, Permissions: permissions, RoleName: s.roleNames[project.ID], IsInactive: s.inactive[project.ID],
		HoldsEntries: s.holding[project.ID], TimeZone: s.zoneOf(project.ID), UTCOffset: zones.OffsetMinutes(s.zoneOf(project.ID), s.now),
		UnseenTables: s.access.UnseenIn(project.ID)}
}

// list is the projects the person may read, and those they started or were made inactive in, the active first, the latest start first
// (ProjectsEndpoints.List).
func (p Routes) list(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	views, err := List(r.Context(), p.database.Queries(), p.book, p.clock.Now(), caller.UserID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// List is the projects userID may read, and those they started or were made inactive in, the active first, the latest start first: the rule both
// doors call, the app's project list and the web's project switcher (redix-api #165, #80 D7). The error is the database's.
func List(ctx context.Context, read *store.Queries, book *access.Book, now time.Time, userID uuid.UUID) ([]View, error) {
	seen, err := sightOf(ctx, read, book, now, userID)
	projects, projectsErr := read.AllProjects(ctx)
	units, unitsErr := read.AllUnits(ctx)
	theirs, theirsErr := read.ProjectsOfUserWithRows(ctx, userID)
	if err := errors.Join(err, projectsErr, unitsErr, theirsErr); err != nil {
		return nil, err
	}
	unitsOf := map[uuid.UUID][]store.Unit{}
	for _, unit := range units {
		unitsOf[unit.ProjectID] = append(unitsOf[unit.ProjectID], unit)
	}
	isTheirs := map[uuid.UUID]bool{}
	for _, id := range theirs {
		isTheirs[id] = true
	}
	var views []View
	for _, project := range projects {
		approved := project.Status == active || project.Status == finished
		if seen.access.Holds(project.ID, access.ReadProject) || (isTheirs[project.ID] && (!approved || seen.inactive[project.ID])) {
			views = append(views, seen.view(project, unitsOf[project.ID]))
		}
	}
	return views, nil
}

// startRequest is a project as the one starting it types it (CreateProjectRequest).
type startRequest struct {
	Name              string      `json:"name"`
	StartDate         moment.Date `json:"startDate"`
	Units             []string    `json:"units"`
	StructureSourceID uuid.UUID   `json:"structureSourceId,omitzero"`
	Description       string      `json:"description,omitzero"`
	Partners          []string    `json:"partners,omitzero"`
}

// create starts a project, which waits for the admin, with the one starting it as its manager (UC-004): invalidName, invalidDescription or
// unitNameTaken (400) for what was typed, unknownSource (400) for a structure source they may not read; 201 with it.
func (p Routes) create(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request startRequest
	if !web.Read(w, r, &request) {
		return
	}
	project, units, partners, refusal := p.started(request)
	if refusal != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
		return
	}
	if request.StructureSourceID != uuid.Nil && !caller.Access.Holds(request.StructureSourceID, access.ReadProject) {
		problem.Refuse(w, r, http.StatusBadRequest, "unknownSource")
		return
	}
	now := moment.Of(p.clock.Now())
	err := p.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := addWithManager(r.Context(), q, project, units, partners, caller.UserID, now); err != nil {
			return err
		}
		if request.StructureSourceID == uuid.Nil {
			return nil
		}
		if err := q.AddStructureSource(r.Context(), store.AddStructureSourceParams{ProjectID: project.ID, StructureSourceID: request.StructureSourceID}); err != nil {
			return err
		}
		return history.Write(r.Context(), q, now, caller.UserID, history.RegisterProject, history.Change{Table: "ProjectStructureSources", RecordID: project.ID,
			ProjectID: project.ID, After: map[string]any{"StructureSourceId": request.StructureSourceID}})
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Location", "/projects/"+project.ID.String())
	p.answer(w, r, http.StatusCreated, project.ID, caller.UserID)
}

// started is the project request describes, with its units and partners, or the refusal of what was typed (Project.New, DeclaredWith).
func (p Routes) started(request startRequest) (store.Project, []string, []string, string) {
	name, description, units, refusal := newProject(request.Name, request.Description, request.Units)
	if refusal != "" {
		return store.Project{}, nil, nil, refusal
	}
	partners, ok := declaredPartners(request.Partners)
	if !ok {
		return store.Project{}, nil, nil, invalidName
	}
	return store.Project{ID: uuid.Must(uuid.NewV7()), Name: name, StartDate: request.StartDate, Status: pendingApproval, Description: description}, units, partners, ""
}

// addWithManager keeps a new project waiting for the admin, its units and partners, and its manager's permissions, recording the project as
// registered by its manager (ProjectsEndpoints.AddWithManager).
func addWithManager(ctx context.Context, q *store.Queries, project store.Project, units, partners []string, managerID uuid.UUID, now moment.Moment) error {
	err := q.AddProject(ctx, store.AddProjectParams{ID: project.ID, Name: project.Name, StartDate: project.StartDate, Status: project.Status, Description: project.Description})
	if err != nil {
		return err
	}
	if err := history.Write(ctx, q, now, managerID, history.RegisterProject, history.Change{Table: "Projects", RecordID: project.ID, ProjectID: project.ID,
		After: projectFields(project)}); err != nil {
		return err
	}
	for place, name := range units {
		if err := q.AddUnit(ctx, store.AddUnitParams{ID: uuid.Must(uuid.NewV7()), ProjectID: project.ID, Name: name, SortOrder: int64(place)}); err != nil {
			return err
		}
	}
	for place, name := range partners {
		if err := q.AddPartner(ctx, store.AddPartnerParams{ID: uuid.Must(uuid.NewV7()), ProjectID: project.ID, Name: name, SortOrder: int64(place + 1)}); err != nil {
			return err
		}
	}
	return users.TickStarter(ctx, q, project.ID, managerID, now)
}

// registerProjectRequest is the body POST /projects/register (UC-066) reads.
type registerProjectRequest struct {
	startRequest
	DisplayName string `json:"displayName"`
	Username    string `json:"username"`
	Mobile      string `json:"mobile"`
	Password    string `json:"password"`
}

// register lets someone with no account register a project and become its manager (UC-066): the project's refusals as create's, then the new
// user's; 201 with the project as its manager sees it.
func (p Routes) register(w http.ResponseWriter, r *http.Request) {
	var request registerProjectRequest
	if !web.Read(w, r, &request) {
		return
	}
	project, units, partners, refusal := p.started(request.startRequest)
	if refusal != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
		return
	}
	now := moment.Of(p.clock.Now())
	manager, status, refusal, err := users.New(r.Context(), p.database.Queries(), request.DisplayName, request.Username, request.Mobile, request.Password, false, now)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, status, refusal)
		return
	}
	err = p.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := users.Add(r.Context(), q, manager, manager.ID, now); err != nil {
			return err
		}
		return addWithManager(r.Context(), q, project, units, partners, manager.ID, now)
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Location", "/projects/"+project.ID.String())
	p.answer(w, r, http.StatusCreated, project.ID, manager.ID)
}

// registeredView is a project waiting for the admin, with who registered it (RegisteredProjectView).
type registeredView struct {
	ID              uuid.UUID   `json:"id"`
	Name            string      `json:"name"`
	StartDate       moment.Date `json:"startDate"`
	Units           []string    `json:"units"`
	ManagerName     string      `json:"managerName"`
	ManagerUsername string      `json:"managerUsername"`
	ManagerMobile   string      `json:"managerMobile"`
	Description     string      `json:"description"`
}

// waiting is the projects waiting for the admin, the earliest start first, each with its manager (RegistrationEndpoints.Waiting).
func (p Routes) waiting(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	read := p.database.Queries()
	projects, err := read.PendingProjects(r.Context())
	managers, managersErr := read.ManagersOfPending(r.Context())
	units, unitsErr := read.AllUnits(r.Context())
	if err != nil || managersErr != nil || unitsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	var views []registeredView
	for _, project := range projects {
		view := registeredView{ID: project.ID, Name: project.Name, StartDate: project.StartDate, Description: project.Description}
		for _, unit := range units {
			if unit.ProjectID == project.ID {
				view.Units = append(view.Units, unit.Name)
			}
		}
		for _, manager := range managers {
			if manager.ProjectID == project.ID {
				view.ManagerName, view.ManagerUsername, view.ManagerMobile = manager.DisplayName, manager.Username, manager.Mobile
				break
			}
		}
		views = append(views, view)
	}
	web.JSON(w, r, http.StatusOK, views)
}

// approve lets a registered project in, with its structure copied and its register started with the general cash supplier in the admin's name,
// and tells its managers; its permissions count from the next request (BR-34, FR-05, UC-017): 404 for no such project, notPending (409) for one
// decided already.
func (p Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	before, waiting := p.waitingProject(w, r)
	if !waiting {
		return
	}
	after := before
	after.Status = active
	p.decide(w, r, caller, history.ApproveProject, notifications.ProjectApproved, before, after, func(ctx context.Context, q *store.Queries) error {
		if err := structure.Copy(ctx, q, after.ID); err != nil {
			return err
		}
		return suppliers.AddGeneralCash(ctx, q, after.ID, caller.UserID)
	})
}

// rejectProjectRequest is the body POST /projects/{projectId}/reject (UC-066) reads.
type rejectProjectRequest struct {
	Reason string `json:"reason"`
}

// reject refuses a registered project, saying why, which its manager sees (AC-52.2): 404 for no such project, notPending (409) for one decided
// already, invalidReason (400) for a reason empty or over 500 characters.
func (p Routes) reject(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request rejectProjectRequest
	if !web.Read(w, r, &request) {
		return
	}
	before, waiting := p.waitingProject(w, r)
	if !waiting {
		return
	}
	reason := strings.TrimSpace(request.Reason)
	if length := len([]rune(reason)); length == 0 || length > reasonMaxLength {
		problem.Refuse(w, r, http.StatusBadRequest, invalidReason)
		return
	}
	after := before
	after.Status, after.RejectionReason = rejected, reason
	p.decide(w, r, caller, history.RejectProject, notifications.ProjectRejected, before, after, nothingMore)
}

// waitingProject is the route's project while it waits for the admin: 404 for no such project, notPending (409) for one decided already.
func (p Routes) waitingProject(w http.ResponseWriter, r *http.Request) (store.Project, bool) {
	project, err := p.database.Queries().ProjectWithID(r.Context(), web.ID(r, "projectId"))
	switch {
	case errors.Is(err, sql.ErrNoRows):
		problem.Status(w, r, http.StatusNotFound)
	case err != nil:
		problem.Status(w, r, http.StatusInternalServerError)
	case project.Status != pendingApproval:
		problem.Refuse(w, r, http.StatusConflict, notPending)
	default:
		return project, true
	}
	return project, false
}

// decide keeps the admin's decision on a project - its new status, with also's rows beside it - recorded as action, and tells its managers
// (RegistrationEndpoints.DecideAsync): 200 with no body.
func (p Routes) decide(w http.ResponseWriter, r *http.Request, caller access.Caller, action history.Action, told notifications.Kind, before, after store.Project,
	also func(context.Context, *store.Queries) error) {
	now := moment.Of(p.clock.Now())
	err := p.database.InTx(r.Context(), func(q *store.Queries) error {
		if err := keepChanges(r.Context(), q, caller.UserID, action, now, before, after, nil, nil); err != nil {
			return err
		}
		if err := also(r.Context(), q); err != nil {
			return err
		}
		return notifications.TellManagers(r.Context(), q, after.ID, told, now)
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusOK)
}

// timeZoneRequest is the body PUT /projects/{projectId}/time-zone (UC-006) reads.
type timeZoneRequest struct {
	TimeZone string `json:"timeZone"`
}

// setTimeZone puts the project in a zone of the list, the one its entries' today is counted in (redix-app #107, D1): unknownTimeZone (400) for one
// not on it, projectFinished (409) in a finished project. A change is its own History line.
func (p Routes) setTimeZone(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request timeZoneRequest
	if !web.Read(w, r, &request) {
		return
	}
	if !zones.Offered(request.TimeZone) {
		problem.Refuse(w, r, http.StatusBadRequest, "unknownTimeZone")
		return
	}
	done := changes.Writer{Database: p.database, Clock: p.clock}.Write(w, r, caller, func(c changes.Change) error {
		project, err := changes.Found(c.Q.ProjectWithID(c.Ctx, c.ProjectID))
		if err != nil {
			return err
		}
		if project.Status == finished {
			return changes.Refuse(http.StatusConflict, projectFinished)
		}
		before, err := zones.Of(c.Ctx, c.Q, c.ProjectID)
		if err != nil || before == request.TimeZone {
			return err
		}
		if err := c.Q.SetProjectTimeZone(c.Ctx, store.SetProjectTimeZoneParams{ProjectID: c.ProjectID, TimeZone: request.TimeZone}); err != nil {
			return err
		}
		return c.Record(history.Update, "ProjectTimeZones", c.ProjectID, map[string]any{"TimeZone": before}, map[string]any{"TimeZone": request.TimeZone})
	})
	if done {
		p.answer(w, r, http.StatusOK, web.ID(r, "projectId"), caller.UserID)
	}
}

// zoneView is a zone a project may be in, as its picker shows it: the word of its city and how far it is ahead of UTC now.
type zoneView struct {
	Name      string `json:"name"`
	WordKey   string `json:"wordKey"`
	UTCOffset int    `json:"utcOffsetMinutes"`
}

// timeZones is the zones a project may be in (D3), Saudi Arabia first.
func (p Routes) timeZones(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	now := p.clock.Now()
	var views []zoneView
	for _, zone := range zones.All {
		views = append(views, zoneView{Name: zone.Name, WordKey: zone.WordKey, UTCOffset: zones.OffsetMinutes(zone.Name, now)})
	}
	web.JSON(w, r, http.StatusOK, views)
}

// nothingMore is a decision that keeps no rows beside the project's own.
func nothingMore(context.Context, *store.Queries) error { return nil }

// renameProjectRequest is the body PUT /projects/{projectId} (UC-006) reads.
type renameProjectRequest struct {
	Name string `json:"name"`
}

// rename renames the project.
func (p Routes) rename(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request renameProjectRequest
	if !web.Read(w, r, &request) {
		return
	}
	p.change(w, r, caller, history.Update, func(c projectChangeContext) string {
		name, ok := naming.Named(request.Name)
		if !ok {
			return invalidName
		}
		c.project.Name = name
		return ""
	})
}

// describeProjectRequest is the body PUT /projects/{projectId}/description (UC-006) reads.
type describeProjectRequest struct {
	Description string `json:"description"`
}

// describe gives the project its description, up to 2,000 characters.
func (p Routes) describe(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request describeProjectRequest
	if !web.Read(w, r, &request) {
		return
	}
	p.change(w, r, caller, history.Update, func(c projectChangeContext) string {
		description := strings.TrimSpace(request.Description)
		if len([]rune(description)) > descriptionMaxLength {
			return invalidDescription
		}
		c.project.Description = description
		return ""
	})
}

// addUnitRequest is the body POST /projects/{projectId}/units (UC-006) reads.
type addUnitRequest struct {
	Name string `json:"name"`
}

// addUnit adds a unit after the last: invalidName (400) or unitNameTaken (409).
func (p Routes) addUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addUnitRequest
	if !web.Read(w, r, &request) {
		return
	}
	p.change(w, r, caller, history.Create, func(c projectChangeContext) string {
		name, ok := naming.Named(request.Name)
		switch {
		case !ok:
			return invalidName
		case c.unitNamed(name, uuid.Nil):
			return unitNameTaken
		}
		order := int64(0)
		for _, unit := range *c.units {
			order = max(order, unit.SortOrder+1)
		}
		*c.units = append(*c.units, store.Unit{ID: uuid.Must(uuid.NewV7()), ProjectID: c.project.ID, Name: name, SortOrder: order})
		return ""
	})
}

// renameUnitRequest is the body PUT /projects/{projectId}/units/{unitId} (UC-006) reads.
type renameUnitRequest struct {
	Name string `json:"name"`
}

// renameUnit renames one of the project's units: 404 for one it does not have, invalidName (400) or unitNameTaken (409).
func (p Routes) renameUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request renameUnitRequest
	if !web.Read(w, r, &request) {
		return
	}
	unitID := web.ID(r, "unitId")
	p.change(w, r, caller, history.Update, func(c projectChangeContext) string {
		unit := c.unit(unitID)
		if unit == nil {
			return unknownUnit
		}
		name, ok := naming.Named(request.Name)
		switch {
		case !ok:
			return invalidName
		case c.unitNamed(name, unitID):
			return unitNameTaken
		}
		unit.Name = name
		return ""
	})
}

// deleteUnit removes one of the project's units: 404 for one it does not have.
func (p Routes) deleteUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	unitID := web.ID(r, "unitId")
	p.change(w, r, caller, history.Delete, func(c projectChangeContext) string {
		if c.unit(unitID) == nil {
			return unknownUnit
		}
		var kept []store.Unit
		for _, unit := range *c.units {
			if unit.ID != unitID {
				kept = append(kept, unit)
			}
		}
		*c.units = kept
		return ""
	})
}

// close finishes the project, which then only reads (AC-04.1): pendingEntries (409) while an entry waits for approval.
func (p Routes) close(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	waiting, err := p.database.Queries().HasEntriesWaiting(r.Context(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	p.change(w, r, caller, history.CloseProject, func(c projectChangeContext) string {
		if waiting {
			return pendingEntries
		}
		c.project.Status = finished
		return ""
	})
}

// projectChangeContext is the project a change works on, and its units, which the change edits in place.
type projectChangeContext struct {
	project *store.Project
	units   *[]store.Unit
}

func (c projectChangeContext) unit(id uuid.UUID) *store.Unit {
	for i := range *c.units {
		if (*c.units)[i].ID == id {
			return &(*c.units)[i]
		}
	}
	return nil
}

func (c projectChangeContext) unitNamed(name string, other uuid.UUID) bool {
	for _, unit := range *c.units {
		if unit.Name == name && unit.ID != other {
			return true
		}
	}
	return false
}

// change applies decide to the route's project and its units, keeps what it changed and records each change, as ChangeAsync and
// AddHistoryOfChanges did: 404 for no such project or unit; a finished project refuses with projectFinished (409); invalidName and
// invalidDescription answer 400, any other refusal 409; then the project as the caller sees it.
func (p Routes) change(w http.ResponseWriter, r *http.Request, caller access.Caller, action history.Action, decide func(projectChangeContext) string) {
	projectID := web.ID(r, "projectId")
	read := p.database.Queries()
	before, err := read.ProjectWithID(r.Context(), projectID)
	if errors.Is(err, sql.ErrNoRows) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	unitsBefore, unitsErr := read.UnitsOf(r.Context(), projectID)
	if err != nil || unitsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	project, units := before, append([]store.Unit{}, unitsBefore...)
	refusal := projectFinished
	if project.Status != finished {
		refusal = decide(projectChangeContext{project: &project, units: &units})
	}
	switch refusal {
	case "":
	case unknownUnit:
		problem.Status(w, r, http.StatusNotFound)
		return
	case invalidName, invalidDescription:
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
		return
	default:
		problem.Refuse(w, r, http.StatusConflict, refusal)
		return
	}
	now := moment.Of(p.clock.Now())
	err = p.database.InTx(r.Context(), func(q *store.Queries) error {
		return keepChanges(r.Context(), q, caller.UserID, action, now, before, project, unitsBefore, units)
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	p.answer(w, r, http.StatusOK, projectID, caller.UserID)
}

// keepChanges writes what changed between before and after - the project and each unit added, renamed or removed - each with its History line.
func keepChanges(ctx context.Context, q *store.Queries, actor uuid.UUID, action history.Action, now moment.Moment,
	before, after store.Project, unitsBefore, unitsAfter []store.Unit) error {
	if line := projectChange(before, after); len(line.After) > 0 {
		if err := q.SetProjectName(ctx, store.SetProjectNameParams{ID: after.ID, Name: after.Name}); err != nil {
			return err
		}
		if err := q.SetProjectDescription(ctx, store.SetProjectDescriptionParams{ID: after.ID, Description: after.Description}); err != nil {
			return err
		}
		if err := q.SetProjectStatus(ctx, store.SetProjectStatusParams{ID: after.ID, Status: after.Status, RejectionReason: after.RejectionReason}); err != nil {
			return err
		}
		if err := history.Write(ctx, q, now, actor, action, line); err != nil {
			return err
		}
	}
	was := map[uuid.UUID]store.Unit{}
	for _, unit := range unitsBefore {
		was[unit.ID] = unit
	}
	for _, unit := range unitsAfter {
		old, existed := was[unit.ID]
		delete(was, unit.ID)
		switch {
		case !existed:
			if err := q.AddUnit(ctx, store.AddUnitParams(unit)); err != nil {
				return err
			}
			if err := history.Write(ctx, q, now, actor, action, history.Change{Table: "Units", RecordID: unit.ID, ProjectID: unit.ProjectID, After: unitFields(unit)}); err != nil {
				return err
			}
		case old.Name != unit.Name:
			if err := q.SetUnitName(ctx, store.SetUnitNameParams{ID: unit.ID, Name: unit.Name}); err != nil {
				return err
			}
			if err := history.Write(ctx, q, now, actor, action, history.Change{Table: "Units", RecordID: unit.ID, ProjectID: unit.ProjectID,
				Before: map[string]any{"Name": old.Name}, After: map[string]any{"Name": unit.Name}}); err != nil {
				return err
			}
		}
	}
	for _, unit := range was {
		if err := q.RemoveUnit(ctx, unit.ID); err != nil {
			return err
		}
		if err := history.Write(ctx, q, now, actor, action, history.Change{Table: "Units", RecordID: unit.ID, ProjectID: unit.ProjectID, Before: unitFields(unit)}); err != nil {
			return err
		}
	}
	return nil
}

// defaultProjectView is what POST /projects/{projectId}/default and DELETE /projects/default answer (UC-010): the project sign-in opens straight
// into, "" for none.
type defaultProjectView struct {
	DefaultProjectID string `json:"defaultProjectId"`
}

// DefaultOf is the project userID chose to open straight into, named only while held may still read it, so one check covers every way of
// losing it, being made inactive included (AC-37.2, #126); uuid.Nil when there is none: what the app's sign-in names and the web lands in
// (redix-api #165). The error is the database's.
func DefaultOf(ctx context.Context, read *store.Queries, userID uuid.UUID, held access.Access) (uuid.UUID, error) {
	chosen, err := read.DefaultProjectOf(ctx, userID)
	if err != nil || len(chosen) == 0 || !held.Holds(chosen[0], access.ReadProject) {
		return uuid.Nil, err
	}
	return chosen[0], nil
}

// setDefault makes the project the one sign-in opens straight into (AC-37.1).
func (p Routes) setDefault(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	projectID := web.ID(r, "projectId")
	err := p.database.InTx(r.Context(), func(q *store.Queries) error {
		if _, err := changes.Found(q.ProjectWithID(r.Context(), projectID)); err != nil {
			return err
		}
		return q.SetDefaultProject(r.Context(), store.SetDefaultProjectParams{UserID: caller.UserID, ProjectID: projectID})
	})
	if errors.Is(err, changes.NotFound) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, defaultProjectView{projectID.String()})
}

// removeDefault lets sign-in open the project list again.
func (p Routes) removeDefault(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	err := p.database.InTx(r.Context(), func(q *store.Queries) error { return q.RemoveDefaultProject(r.Context(), caller.UserID) })
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, defaultProjectView{})
}

// answer is the project as userID sees it (ProjectsEndpoints.ViewOfAsync).
func (p Routes) answer(w http.ResponseWriter, r *http.Request, status int, projectID, userID uuid.UUID) {
	read := p.database.Queries()
	project, err := read.ProjectWithID(r.Context(), projectID)
	units, unitsErr := read.UnitsOf(r.Context(), projectID)
	seen, seenErr := sightOf(r.Context(), p.database.Queries(), p.book, p.clock.Now(), userID)
	if err != nil || unitsErr != nil || seenErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, status, seen.view(project, units))
}
