// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package projects

import (
	"context"
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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
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

// Start is a project as the one starting it types it (CreateProjectRequest), on the phone or on the web.
type Start struct {
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
	var request Start
	if !web.Read(w, r, &request) {
		return
	}
	projectID, refusal, err := Create(r.Context(), p.database, p.clock.Now(), caller, request)
	if refusal != "" {
		problem.Refuse(w, r, http.StatusBadRequest, refusal)
		return
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Location", "/projects/"+projectID.String())
	p.answer(w, r, http.StatusCreated, projectID, caller.UserID)
}

// Create starts the project request describes at now, waiting for the admin, with caller as its manager, as create answers it (UC-004): the
// rule both doors call, the app's route and the web's signed-in Register new project (redix-api #164). It is the project's id, or the refusal
// of what was typed - invalidName, invalidDescription, unitNameTaken, unknownSource for a structure source caller may not read; the error is
// the database's.
func Create(ctx context.Context, database db.DB, now time.Time, caller access.Caller, request Start) (uuid.UUID, string, error) {
	project, units, partners, refusal := started(request)
	if refusal != "" {
		return uuid.Nil, refusal, nil
	}
	if request.StructureSourceID != uuid.Nil && !caller.Access.Holds(request.StructureSourceID, access.ReadProject) {
		return uuid.Nil, "unknownSource", nil
	}
	at := moment.Of(now)
	return project.ID, "", database.InTx(ctx, func(q *store.Queries) error {
		if err := addWithManager(ctx, q, project, units, partners, caller.UserID, at); err != nil {
			return err
		}
		if request.StructureSourceID == uuid.Nil {
			return nil
		}
		if err := q.AddStructureSource(ctx, store.AddStructureSourceParams{ProjectID: project.ID, StructureSourceID: request.StructureSourceID}); err != nil {
			return err
		}
		return history.Write(ctx, q, at, caller.UserID, history.RegisterProject, history.Change{Table: "ProjectStructureSources", RecordID: project.ID,
			ProjectID: project.ID, After: map[string]any{"StructureSourceId": request.StructureSourceID}})
	})
}

// started is the project request describes, with its units and partners, or the refusal of what was typed (Project.New, DeclaredWith).
func started(request Start) (store.Project, []string, []string, string) {
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
func addWithManager(ctx context.Context, q *store.Queries, project store.Project, units, partnerNames []string, managerID uuid.UUID, now moment.Moment) error {
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
	declaring := changes.Change{Ctx: ctx, Q: q, ProjectID: project.ID, Actor: managerID, Now: now}
	for place, name := range partnerNames {
		if _, err := partners.Declare(declaring, name, int64(place+1)); err != nil {
			return err
		}
	}
	return users.TickStarter(ctx, q, project.ID, managerID, now)
}

// Registration is a project registered with its manager's own new login (RegisterProjectRequest): the body POST /projects/register reads
// (UC-066), and what the web's form sends.
type Registration struct {
	Start
	DisplayName string `json:"displayName"`
	Username    string `json:"username"`
	Mobile      string `json:"mobile"`
	Password    string `json:"password"`
}

// register lets someone with no account register a project and become its manager (UC-066): the project's refusals as create's, then the new
// user's; 201 with the project as its manager sees it.
func (p Routes) register(w http.ResponseWriter, r *http.Request) {
	var request Registration
	if !web.Read(w, r, &request) {
		return
	}
	registered, status, refusal, err := Register(r.Context(), p.database, p.clock.Now(), request)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, status, refusal)
		return
	}
	w.Header().Set("Location", "/projects/"+registered.ProjectID.String())
	p.answer(w, r, http.StatusCreated, registered.ProjectID, registered.ManagerID)
}

// Registered is a registration kept: the project waiting for the admin, and its manager.
type Registered struct{ ProjectID, ManagerID uuid.UUID }

// Register keeps registration at now: the project waiting for the admin, with the one registering it as its manager on a login of their own
// (UC-066): the rule both doors call, the app's registration and the web's (redix-api #164, #80 D7). It refuses as register describes, with the
// status the refusal answers; the error is the database's.
func Register(ctx context.Context, database db.DB, now time.Time, registration Registration) (Registered, int, string, error) {
	project, units, partners, refused := started(registration.Start)
	if refused != "" {
		return Registered{}, http.StatusBadRequest, refused, nil
	}
	at := moment.Of(now)
	manager, status, refusal, err := users.New(ctx, database.Queries(), registration.DisplayName, registration.Username, registration.Mobile, registration.Password, false, at)
	if err != nil || refusal != "" {
		return Registered{}, status, refusal, err
	}
	return Registered{ProjectID: project.ID, ManagerID: manager.ID}, http.StatusCreated, "", database.InTx(ctx, func(q *store.Queries) error {
		if err := users.Add(ctx, q, manager, manager.ID, at); err != nil {
			return err
		}
		return addWithManager(ctx, q, project, units, partners, manager.ID, at)
	})
}

// RegisteredView is a project waiting for the admin, with who registered it (RegisteredProjectView).
type RegisteredView struct {
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
	views, err := WaitingOf(r.Context(), p.database.Queries())
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// WaitingOf is the projects waiting for the admin, the earliest start first, each with its manager, as waiting answers them: the rule both doors
// call, the app's route and the web's Registered projects (redix-api #175). The error is the database's.
func WaitingOf(ctx context.Context, read *store.Queries) ([]RegisteredView, error) {
	projects, err := read.PendingProjects(ctx)
	managers, managersErr := read.ManagersOfPending(ctx)
	units, unitsErr := read.AllUnits(ctx)
	if err := errors.Join(err, managersErr, unitsErr); err != nil {
		return nil, err
	}
	var views []RegisteredView
	for _, project := range projects {
		view := RegisteredView{ID: project.ID, Name: project.Name, StartDate: project.StartDate, Description: project.Description}
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
	return views, nil
}

// approve lets a registered project in, with its structure copied and its register started with the general cash supplier in the admin's name,
// and tells its managers; its permissions count from the next request (BR-34, FR-05, UC-017): 404 for no such project, notPending (409) for one
// decided already.
func (p Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	(changes.Writer{Database: p.database, Clock: p.clock}).Done(w, r, caller, func(c changes.Change) error { return ApproveProject(c) })
}

// ApproveProject approves the change's project while it waits for the admin, as approve does: the rule both doors call, the app's route and the
// web's Registered projects (redix-api #175). It starts with the template's structure and the general cash supplier, and its manager is told.
// NotFound for no such project; notPending (409) for one decided already.
func ApproveProject(c changes.Change) error {
	before, err := waitingIn(c)
	if err != nil {
		return err
	}
	after := before
	after.Status = active
	return decideProject(c, history.ApproveProject, notifications.ProjectApproved, before, after, func(ctx context.Context, q *store.Queries) error {
		if err := structure.Copy(ctx, q, after.ID); err != nil {
			return err
		}
		return suppliers.AddGeneralCash(ctx, q, after.ID, c.Actor)
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
	if web.Read(w, r, &request) {
		(changes.Writer{Database: p.database, Clock: p.clock}).Done(w, r, caller, func(c changes.Change) error { return RejectProject(c, request.Reason) })
	}
}

// RejectProject rejects the change's project while it waits for the admin, with reason, as reject does: the rule both doors call, the app's
// route and the web's Registered projects (redix-api #175). Its manager is told. NotFound for no such project; notPending (409) for one decided
// already; invalidReason (400) for none or one past 500 letters.
func RejectProject(c changes.Change, reason string) error {
	before, err := waitingIn(c)
	if err != nil {
		return err
	}
	kept := strings.TrimSpace(reason)
	if length := len([]rune(kept)); length == 0 || length > reasonMaxLength {
		return changes.Refuse(http.StatusBadRequest, invalidReason)
	}
	after := before
	after.Status, after.RejectionReason = rejected, kept
	return decideProject(c, history.RejectProject, notifications.ProjectRejected, before, after, nothingMore)
}

// waitingIn is the change's project while it waits for the admin: changes.NotFound for no such project, notPending (409) for one decided
// already.
func waitingIn(c changes.Change) (store.Project, error) {
	project, err := changes.Found(c.Q.ProjectWithID(c.Ctx, c.ProjectID))
	if err == nil && project.Status != pendingApproval {
		err = changes.Refuse(http.StatusConflict, notPending)
	}
	return project, err
}

// decideProject keeps the admin's decision on a waiting project, before to after, with its History line under action, then also, and tells its
// managers told.
func decideProject(c changes.Change, action history.Action, told notifications.Kind, before, after store.Project, also func(context.Context, *store.Queries) error) error {
	if err := keepChanges(c.Ctx, c.Q, c.Actor, action, c.Now, before, after, nil, nil); err != nil {
		return err
	}
	if err := also(c.Ctx, c.Q); err != nil {
		return err
	}
	return notifications.TellManagers(c.Ctx, c.Q, after.ID, told, c.Now)
}

// timeZoneRequest is the body PUT /projects/{projectId}/time-zone (UC-006) reads.
type timeZoneRequest struct {
	TimeZone string `json:"timeZone"`
}

// setTimeZone puts the project in a zone of the list, the one its entries' today is counted in (redix-app #107, D1): unknownTimeZone (400) for one
// not on it, projectFinished (409) in a finished project. A change is its own History line.
func (p Routes) setTimeZone(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request timeZoneRequest
	if web.Read(w, r, &request) {
		p.change(w, r, caller, func(c changes.Change) error { return SetTimeZone(c, request.TimeZone) })
	}
}

// SetTimeZone sets the change's project's time zone to timeZone, as setTimeZone does: the rule both doors call, the app's route and the web's
// More info (redix-api #175). unknownTimeZone (400) for one REDIX does not offer; NotFound for no such project; projectFinished (409).
func SetTimeZone(c changes.Change, timeZone string) error {
	if !zones.Offered(timeZone) {
		return changes.Refuse(http.StatusBadRequest, "unknownTimeZone")
	}
	project, err := changes.Found(c.Q.ProjectWithID(c.Ctx, c.ProjectID))
	if err != nil {
		return err
	}
	if project.Status == finished {
		return changes.Refuse(http.StatusConflict, projectFinished)
	}
	before, err := zones.Of(c.Ctx, c.Q, c.ProjectID)
	if err != nil || before == timeZone {
		return err
	}
	if err := c.Q.SetProjectTimeZone(c.Ctx, store.SetProjectTimeZoneParams{ProjectID: c.ProjectID, TimeZone: timeZone}); err != nil {
		return err
	}
	return c.Record(history.Update, "ProjectTimeZones", c.ProjectID, map[string]any{"TimeZone": before}, map[string]any{"TimeZone": timeZone})
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
	if web.Read(w, r, &request) {
		p.change(w, r, caller, func(c changes.Change) error { return RenameProject(c, request.Name) })
	}
}

// RenameProject names the change's project name, as rename does: the rule both doors call, the app's route and the web's More info (redix-api
// #175). Refused as changeProject refuses; invalidName for a name not 1 to 100 letters.
func RenameProject(c changes.Change, name string) error {
	return changeProject(c, history.Update, func(project projectChangeContext) error {
		named, ok := naming.Named(name)
		if !ok {
			return changes.Refuse(http.StatusBadRequest, invalidName)
		}
		project.project.Name = named
		return nil
	})
}

// describeProjectRequest is the body PUT /projects/{projectId}/description (UC-006) reads.
type describeProjectRequest struct {
	Description string `json:"description"`
}

// describe gives the project its description, up to 2,000 characters.
func (p Routes) describe(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request describeProjectRequest
	if web.Read(w, r, &request) {
		p.change(w, r, caller, func(c changes.Change) error { return DescribeProject(c, request.Description) })
	}
}

// DescribeProject gives the change's project description, as describe does: the rule both doors call, the app's route and the web's More info
// (redix-api #175). Refused as changeProject refuses; invalidDescription past 2,000 characters.
func DescribeProject(c changes.Change, description string) error {
	return changeProject(c, history.Update, func(project projectChangeContext) error {
		kept := strings.TrimSpace(description)
		if len([]rune(kept)) > descriptionMaxLength {
			return changes.Refuse(http.StatusBadRequest, invalidDescription)
		}
		project.project.Description = kept
		return nil
	})
}

// addUnitRequest is the body POST /projects/{projectId}/units (UC-006) reads.
type addUnitRequest struct {
	Name string `json:"name"`
}

// addUnit adds a unit after the last: invalidName (400) or unitNameTaken (409).
func (p Routes) addUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addUnitRequest
	if web.Read(w, r, &request) {
		p.change(w, r, caller, func(c changes.Change) error { return AddUnit(c, request.Name) })
	}
}

// AddUnit adds the unit named name after the change's project's last, as addUnit does: the rule both doors call, the app's route and the web's
// More info (redix-api #175). Refused as changeProject refuses; invalidName (400) or unitNameTaken (409).
func AddUnit(c changes.Change, name string) error {
	return changeProject(c, history.Create, func(project projectChangeContext) error {
		named, ok := naming.Named(name)
		switch {
		case !ok:
			return changes.Refuse(http.StatusBadRequest, invalidName)
		case project.unitNamed(named, uuid.Nil):
			return changes.Refuse(http.StatusConflict, unitNameTaken)
		}
		order := int64(0)
		for _, unit := range *project.units {
			order = max(order, unit.SortOrder+1)
		}
		*project.units = append(*project.units, store.Unit{ID: uuid.Must(uuid.NewV7()), ProjectID: project.project.ID, Name: named, SortOrder: order})
		return nil
	})
}

// renameUnitRequest is the body PUT /projects/{projectId}/units/{unitId} (UC-006) reads.
type renameUnitRequest struct {
	Name string `json:"name"`
}

// renameUnit renames one of the project's units: 404 for one it does not have, invalidName (400) or unitNameTaken (409).
func (p Routes) renameUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request renameUnitRequest
	if web.Read(w, r, &request) {
		p.change(w, r, caller, func(c changes.Change) error { return RenameUnit(c, web.ID(r, "unitId"), request.Name) })
	}
}

// RenameUnit names unitID of the change's project name, as renameUnit does: the rule both doors call, the app's route and the web's More info
// (redix-api #175). Refused as changeProject refuses; NotFound for a unit it does not have, invalidName (400) or unitNameTaken (409).
func RenameUnit(c changes.Change, unitID uuid.UUID, name string) error {
	return changeProject(c, history.Update, func(project projectChangeContext) error {
		unit := project.unit(unitID)
		if unit == nil {
			return changes.NotFound
		}
		named, ok := naming.Named(name)
		switch {
		case !ok:
			return changes.Refuse(http.StatusBadRequest, invalidName)
		case project.unitNamed(named, unitID):
			return changes.Refuse(http.StatusConflict, unitNameTaken)
		}
		unit.Name = named
		return nil
	})
}

// deleteUnit removes one of the project's units: 404 for one it does not have.
func (p Routes) deleteUnit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	p.change(w, r, caller, func(c changes.Change) error { return DeleteUnit(c, web.ID(r, "unitId")) })
}

// DeleteUnit removes unitID from the change's project, as deleteUnit does: the rule both doors call, the app's route and the web's More info
// (redix-api #175). Refused as changeProject refuses; NotFound for a unit it does not have.
func DeleteUnit(c changes.Change, unitID uuid.UUID) error {
	return changeProject(c, history.Delete, func(project projectChangeContext) error {
		if project.unit(unitID) == nil {
			return changes.NotFound
		}
		var kept []store.Unit
		for _, unit := range *project.units {
			if unit.ID != unitID {
				kept = append(kept, unit)
			}
		}
		*project.units = kept
		return nil
	})
}

// close finishes the project, which then only reads (AC-04.1): pendingEntries (409) while an entry waits for approval.
func (p Routes) close(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	p.change(w, r, caller, func(c changes.Change) error { return CloseProject(c) })
}

// CloseProject finishes the change's project, which then only reads (AC-04.1), as close does: the rule both doors call, the app's route and the
// web's More info (redix-api #175). Refused as changeProject refuses; pendingEntries (409) while an entry waits for approval.
func CloseProject(c changes.Change) error {
	waiting, err := c.Q.HasEntriesWaiting(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	return changeProject(c, history.CloseProject, func(project projectChangeContext) error {
		if waiting {
			return changes.Refuse(http.StatusConflict, pendingEntries)
		}
		project.project.Status = finished
		return nil
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

// change runs act as one change to the route's project and answers the project as the caller sees it, or the refusal act gave.
func (p Routes) change(w http.ResponseWriter, r *http.Request, caller access.Caller, act func(c changes.Change) error) {
	if (changes.Writer{Database: p.database, Clock: p.clock}).Write(w, r, caller, act) {
		p.answer(w, r, http.StatusOK, web.ID(r, "projectId"), caller.UserID)
	}
}

// changeProject applies decide to the change's project and its units, keeps what it changed and records each change under action, as
// ChangeAsync and AddHistoryOfChanges did: NotFound for no such project; projectFinished (409) in a finished project; then the refusal decide
// gives, each with its own status.
func changeProject(c changes.Change, action history.Action, decide func(projectChangeContext) error) error {
	before, err := changes.Found(c.Q.ProjectWithID(c.Ctx, c.ProjectID))
	if err != nil {
		return err
	}
	if before.Status == finished {
		return changes.Refuse(http.StatusConflict, projectFinished)
	}
	unitsBefore, err := c.Q.UnitsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	project, units := before, append([]store.Unit{}, unitsBefore...)
	if err := decide(projectChangeContext{project: &project, units: &units}); err != nil {
		return err
	}
	return keepChanges(c.Ctx, c.Q, c.Actor, action, c.Now, before, project, unitsBefore, units)
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
	err := SetDefault(r.Context(), p.database, caller.UserID, projectID)
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
	if err := RemoveDefault(r.Context(), p.database, caller.UserID); err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, defaultProjectView{})
}

// SetDefault makes projectID the project userID's sign-in opens straight into (AC-37.1): the rule both doors call, the app's star and the web's
// (redix-api #165). changes.NotFound for a project there is none of; any other error is the database's.
func SetDefault(ctx context.Context, database db.DB, userID, projectID uuid.UUID) error {
	return database.InTx(ctx, func(q *store.Queries) error {
		if _, err := changes.Found(q.ProjectWithID(ctx, projectID)); err != nil {
			return err
		}
		return q.SetDefaultProject(ctx, store.SetDefaultProjectParams{UserID: userID, ProjectID: projectID})
	})
}

// RemoveDefault lets userID's sign-in open the project list again (AC-37.1), on either door. The error is the database's.
func RemoveDefault(ctx context.Context, database db.DB, userID uuid.UUID) error {
	return database.InTx(ctx, func(q *store.Queries) error { return q.RemoveDefaultProject(ctx, userID) })
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
