// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/stored"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Routes keeps people and who is in each project: editing a person and resetting their password (UC-007), and a project's members, its users and
// what each may do there (FR-02, BR-29 to BR-33, #120, #126).
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes PUT /users/{userId} and POST /users/{userId}/reset-password for a working session; GET /projects/{projectId}/members for whoever reads
// the project; and the project's users routes for whoever manages its users.
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	u := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	u.mapRequests(router, gate)
	router.Handle("PUT", "/users/{userId:guid}", gate.Working(u.edit))
	router.Handle("POST", "/users/{userId:guid}/reset-password", gate.Working(u.resetPassword))
	router.Handle("GET", "/projects/{projectId:guid}/members", gate.Needs(access.ReadProject, u.members))
	manage := func(handler access.Handler) http.HandlerFunc { return gate.Needs(access.ManageUsers, handler) }
	router.Handle("GET", "/projects/{projectId:guid}/users", manage(u.list))
	router.Handle("POST", "/projects/{projectId:guid}/users", manage(u.create))
	router.Handle("POST", "/projects/{projectId:guid}/users/existing", manage(u.addExisting))
	router.Handle("POST", "/projects/{projectId:guid}/users/{userId:guid}/permissions/{permission}", manage(u.give))
	router.Handle("DELETE", "/projects/{projectId:guid}/users/{userId:guid}/permissions/{permission}", manage(u.take))
	router.Handle("POST", "/projects/{projectId:guid}/users/{userId:guid}/partners/{partnerId:guid}", manage(u.choose))
	router.Handle("POST", "/projects/{projectId:guid}/users/{userId:guid}/tables", manage(u.seeTables))
	router.Handle("DELETE", "/projects/{projectId:guid}/users/{userId:guid}/partners/{partnerId:guid}", manage(u.unchoose))
	router.Handle("PUT", "/projects/{projectId:guid}/users/{userId:guid}/role-name", manage(u.setRoleName))
	router.Handle("POST", "/projects/{projectId:guid}/users/{userId:guid}/disable", manage(u.disable))
	router.Handle("POST", "/projects/{projectId:guid}/users/{userId:guid}/enable", manage(u.enable))
}

// ProjectView is what a person holds in one project (UserProjectView), as the app's and the web's Users read it.
type ProjectView struct {
	ProjectID   uuid.UUID           `json:"projectId"`
	Permissions []access.Permission `json:"permissions"`
	RoleName    string              `json:"roleName"`
	PartnerIDs  []uuid.UUID         `json:"partnerIds"`
	IsInactive  bool                `json:"isInactive"`
	// UnseenTables are the tables unticked for the person: the groups and contracts they do not see (#106).
	UnseenTables []uuid.UUID `json:"unseenTables"`
}

// View is a person and what they hold in their projects (UserView), as the app's and the web's Users read them (redix-api #173).
type View struct {
	ID                 uuid.UUID     `json:"id"`
	Username           string        `json:"username"`
	DisplayName        string        `json:"displayName"`
	Mobile             string        `json:"mobile"`
	Email              string        `json:"email"`
	MustChangePassword bool          `json:"mustChangePassword"`
	Projects           []ProjectView `json:"projects"`
}

// memberView is someone active in a project, as a picker lists them (MemberView).
type memberView struct {
	ID          uuid.UUID `json:"id"`
	DisplayName string    `json:"displayName"`
}

func viewOf(user store.User, rows []store.UserProject) View {
	var projects []ProjectView
	for _, row := range rows {
		var names []string
		var partners []uuid.UUID
		var unseen []uuid.UUID
		stored.Read([]byte(row.Permissions), &names)
		stored.Read([]byte(row.PartnerIds), &partners)
		stored.Read([]byte(row.UnseenTables), &unseen)
		var permissions []access.Permission
		for _, name := range names {
			if permission, ok := access.Named(name); ok {
				permissions = append(permissions, permission)
			}
		}
		slices.SortFunc(permissions, func(a, b access.Permission) int { return a.Number() - b.Number() })
		projects = append(projects, ProjectView{ProjectID: row.ProjectID, Permissions: permissions, RoleName: row.RoleName, PartnerIDs: partners,
			IsInactive: row.IsInactive, UnseenTables: unseen})
	}
	return View{ID: user.ID, Username: user.Username, DisplayName: user.DisplayName, Mobile: user.Mobile, Email: user.Email, MustChangePassword: user.MustChangePassword, Projects: projects}
}

// editUserRequest is the body PUT /users/{userId} (UC-007) reads.
type editUserRequest struct {
	DisplayName string `json:"displayName"`
	Mobile      string `json:"mobile"`
	Email       string `json:"email"`
}

// edit changes a person's name, mobile and email (UC-007): invalidName, invalidMobile, emailInvalid (400) or emailTaken (409).
func (u Routes) edit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request editUserRequest
	if !web.Read(w, r, &request) {
		return
	}
	u.changeUser(w, r, caller, func(c changes.Change, userID uuid.UUID) error {
		return EditDetails(c, caller, userID, request.DisplayName, request.Mobile, request.Email)
	})
}

// EditDetails changes the person userID's name, mobile and email as caller, as edit does (UC-007): the rule both doors call, the app's route
// and the web's Users (redix-api #173). Refused as changeOfUser says, then invalidName, invalidMobile, emailInvalid (400) or emailTaken (409).
func EditDetails(c changes.Change, caller access.Caller, userID uuid.UUID, displayName, mobile, email string) error {
	name, mobile, email := strings.TrimSpace(displayName), strings.TrimSpace(mobile), strings.TrimSpace(email)
	return changeOfUser(c, caller, userID, history.Update, func(user store.User) (int, string, error) {
		if refusal := RefusalOfDetails(name, mobile); refusal != "" {
			return http.StatusBadRequest, refusal, nil
		}
		return RefusalOfEmail(c.Ctx, c.Q, email, user.ID)
	}, func(user store.User) (store.User, error) {
		if err := c.Q.SetDetails(c.Ctx, store.SetDetailsParams{ID: user.ID, DisplayName: name, Mobile: mobile}); err != nil {
			return user, err
		}
		user.DisplayName, user.Mobile = name, mobile
		return ChangeEmail(c.Ctx, c.Q, user, email)
	})
}

// resetPasswordRequest is the body POST /users/{userId}/reset-password (UC-007) reads.
type resetPasswordRequest struct {
	TemporaryPassword string `json:"temporaryPassword"`
}

// resetPassword gives a person a temporary password they must change at their next sign-in, signing out every device of theirs (UC-007):
// weakPassword (400) when it does not keep the policy.
func (u Routes) resetPassword(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request resetPasswordRequest
	if !web.Read(w, r, &request) {
		return
	}
	u.changeUser(w, r, caller, func(c changes.Change, userID uuid.UUID) error {
		return ResetTemporary(c, caller, userID, request.TemporaryPassword)
	})
}

// ResetTemporary gives the person userID the temporary password, which they must change at their next sign-in, signing out every device of
// theirs, as resetPassword does (UC-007): the rule both doors call, the app's route and the web's Users (redix-api #173). Refused as
// changeOfUser says, then weakPassword (400) for one that does not keep the policy.
func ResetTemporary(c changes.Change, caller access.Caller, userID uuid.UUID, password string) error {
	return changeOfUser(c, caller, userID, history.ResetPassword, func(user store.User) (int, string, error) {
		if !KeepsPolicy(password, user.Username) {
			return http.StatusBadRequest, "weakPassword", nil
		}
		return 0, "", nil
	}, func(user store.User) (store.User, error) {
		return ResetPassword(c.Ctx, c.Q, user, password, true)
	})
}

// changeUser runs a change to the person the route names and answers them with all their projects (UsersEndpoints.ChangeAsync).
func (u Routes) changeUser(w http.ResponseWriter, r *http.Request, caller access.Caller, act func(c changes.Change, userID uuid.UUID) error) {
	userID := web.ID(r, "userId")
	if !changes.Answered(w, r, u.writer.Run(r.Context(), uuid.Nil, caller, func(c changes.Change) error { return act(c, userID) })) {
		u.answer(w, r, http.StatusOK, userID, func(q *store.Queries) ([]store.UserProject, error) { return q.ProjectsOfUser(r.Context(), userID) })
	}
}

// changeOfUser changes the person userID through apply and records it as action: NotFound for no such person; otherProject (403) unless caller
// manages the users of every project they are in and they are not an admin; then refusalOf's refusal, a status with its code.
func changeOfUser(c changes.Change, caller access.Caller, userID uuid.UUID, action history.Action, refusalOf func(store.User) (int, string, error),
	apply func(store.User) (store.User, error)) error {
	user, err := changes.Found(c.Q.UserWithID(c.Ctx, userID))
	if err != nil {
		return err
	}
	mayChange, err := mayChange(c.Ctx, c.Q, caller, user.ID)
	if err != nil {
		return err
	}
	if !mayChange {
		return changes.Refuse(http.StatusForbidden, "otherProject")
	}
	status, refusal, err := refusalOf(user)
	if err != nil {
		return err
	}
	if refusal != "" {
		return changes.Refuse(status, refusal)
	}
	changed, err := apply(user)
	if err != nil {
		return err
	}
	return changeOf(c).record(action, Change(user, changed))
}

// mayChange says whether the caller may change userID: not an admin, in at least one project, and every one of them one whose users the caller
// manages (UsersEndpoints.MayChangeAsync).
func mayChange(ctx context.Context, read *store.Queries, caller access.Caller, userID uuid.UUID) (bool, error) {
	if admin, err := read.IsAdmin(ctx, userID); err != nil || admin {
		return false, err
	}
	projects, err := read.ProjectIDsOfUser(ctx, userID)
	if err != nil || len(projects) == 0 {
		return false, err
	}
	for _, projectID := range projects {
		if !caller.Access.Holds(projectID, access.ManageUsers) {
			return false, nil
		}
	}
	return true, nil
}

// members is everyone active in the project, by name (ProjectUsersEndpoints.Members).
func (u Routes) members(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	rows, err := u.database.Queries().Members(r.Context(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	var members []memberView
	for _, row := range rows {
		members = append(members, memberView{ID: row.ID, DisplayName: row.DisplayName})
	}
	web.JSON(w, r, http.StatusOK, members)
}

// list is everyone in the project, the inactive last, by name, each with what they hold here (ProjectUsersEndpoints.List).
func (u Routes) list(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	views, err := ListOf(r.Context(), u.database.Queries(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// ListOf is everyone in projectID, the inactive last, by name, each with what they hold there, as list answers them: the rule both doors call, the
// app's route and the web's Users (redix-api #173). The error is the database's.
func ListOf(ctx context.Context, read *store.Queries, projectID uuid.UUID) ([]View, error) {
	people, err := read.UsersInProject(ctx, projectID)
	held, heldErr := read.UsersOfProject(ctx, projectID)
	if err := errors.Join(err, heldErr); err != nil {
		return nil, err
	}
	var views []View
	for _, person := range people {
		mine := slices.DeleteFunc(slices.Clone(held), func(row store.UserProject) bool { return row.UserID != person.ID })
		views = append(views, viewOf(person, mine))
	}
	return views, nil
}

// NewPerson is a new person as New user types them, the body POST /projects/{projectId}/users (UC-008) reads and the web's New user sends.
type NewPerson struct {
	DisplayName       string              `json:"displayName"`
	Username          string              `json:"username"`
	Mobile            string              `json:"mobile"`
	Email             string              `json:"email"`
	TemporaryPassword string              `json:"temporaryPassword"`
	Permissions       []access.Permission `json:"permissions,omitzero"`
	RoleName          string              `json:"roleName,omitzero"`
	PartnerIDs        []uuid.UUID         `json:"partnerIds,omitzero"`
	UnseenTableIDs    []uuid.UUID         `json:"unseenTableIds,omitzero"`
}

// create adds a new person to the project with a temporary password, ReadProject and the permissions ticked, the partners chosen and a role name
// (FR-02): invalidRoleName, invalidPermission, unknownPartner, the details' and the email's refusals, or usernameTaken (409); 201 with them.
func (u Routes) create(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request NewPerson
	if !web.Read(w, r, &request) {
		return
	}
	var created uuid.UUID
	if !u.writer.Write(w, r, caller, func(c changes.Change) (err error) {
		created, err = Create(c, request)
		return err
	}) {
		return
	}
	w.Header().Set("Location", "/users/"+created.String())
	u.answerInProject(w, r, http.StatusCreated, web.ID(r, "projectId"), created)
}

// Create adds the new person typed to the change's project with a temporary password, ReadProject and the permissions ticked, the partners chosen,
// the tables unticked and a role name (FR-02), as create does: the rule both doors call, the app's route and the web's New user (redix-api
// #173). Refused with invalidRoleName, invalidPermission, unknownPartner, unknownTable, the details' and the email's refusals, or usernameTaken
// (409); it is the new person's id.
func Create(c changes.Change, typed NewPerson) (uuid.UUID, error) {
	roleName := strings.TrimSpace(typed.RoleName)
	if utf8.RuneCountInString(roleName) > roleNameMaxLength {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, "invalidRoleName")
	}
	if slices.Contains(typed.Permissions, access.ApproveProjects) ||
		(slices.Contains(typed.Permissions, access.SeeAllCapital) && slices.Contains(typed.Permissions, access.SeeChosenCapital)) {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, "invalidPermission")
	}
	var partners []uuid.UUID
	if slices.Contains(typed.Permissions, access.SeeChosenCapital) {
		for _, id := range typed.PartnerIDs {
			if !slices.Contains(partners, id) {
				partners = append(partners, id)
			}
		}
	}
	kept, err := c.Q.KeptPartnerIDs(c.Ctx, store.KeptPartnerIDsParams{ProjectID: c.ProjectID, Ids: partners})
	if err != nil {
		return uuid.Nil, err
	}
	if len(kept) != len(partners) {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, "unknownPartner")
	}
	for _, tableID := range typed.UnseenTableIDs {
		known, err := isTable(c.Ctx, c.Q, c.ProjectID, tableID)
		if err != nil {
			return uuid.Nil, err
		}
		if !known {
			return uuid.Nil, changes.Refuse(http.StatusBadRequest, "unknownTable")
		}
	}
	user, status, refusal, err := New(c.Ctx, c.Q, typed.DisplayName, typed.Username, typed.Mobile, typed.TemporaryPassword, true, c.Now)
	if err != nil {
		return uuid.Nil, err
	}
	if refusal != "" {
		return uuid.Nil, changes.Refuse(status, refusal)
	}
	user.Email = strings.TrimSpace(typed.Email)
	if status, refusal, err := RefusalOfEmail(c.Ctx, c.Q, user.Email, uuid.Nil); err != nil || refusal != "" {
		if err != nil {
			return uuid.Nil, err
		}
		return uuid.Nil, changes.Refuse(status, refusal)
	}
	made := changeOf(c)
	if err := Add(c.Ctx, c.Q, user, c.Actor, c.Now); err != nil {
		return uuid.Nil, err
	}
	ticked := append([]access.Permission{access.ReadProject}, typed.Permissions...)
	for i, permission := range ticked {
		if !slices.Contains(ticked[:i], permission) {
			if err := made.addPermission(c.ProjectID, user.ID, permission); err != nil {
				return uuid.Nil, err
			}
		}
	}
	for _, partnerID := range partners {
		if err := made.choose(c.ProjectID, user.ID, partnerID); err != nil {
			return uuid.Nil, err
		}
	}
	for _, tableID := range typed.UnseenTableIDs {
		if err := made.unsee(c.ProjectID, user.ID, tableID); err != nil {
			return uuid.Nil, err
		}
	}
	return user.ID, made.nameRole(c.ProjectID, user.ID, roleName)
}

// addExistingUserRequest is the body POST /projects/{projectId}/users/existing (UC-008) reads.
type addExistingUserRequest struct {
	Username string `json:"username"`
}

// addExisting adds a person already in REDIX, by username, to the project with ReadProject: unknownUser (404) for nobody of that name, or an admin.
func (u Routes) addExisting(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addExistingUserRequest
	if !web.Read(w, r, &request) {
		return
	}
	projectID := web.ID(r, "projectId")
	var userID uuid.UUID
	if !u.writer.Write(w, r, caller, func(c changes.Change) (err error) {
		userID, err = AddExisting(c, request.Username)
		return err
	}) {
		return
	}
	u.answerInProject(w, r, http.StatusOK, projectID, userID)
}

// AddExisting adds the person already in REDIX called username to the change's project with ReadProject, as addExisting does (UC-008): the
// rule both doors call, the app's route and the web's Users (redix-api #173). unknownUser (404) for nobody of that name, or an admin; someone
// already in the project stays as they are.
func AddExisting(c changes.Change, username string) (uuid.UUID, error) {
	found, err := c.Q.NonAdminNamed(c.Ctx, strings.TrimSpace(username))
	if err != nil {
		return uuid.Nil, err
	}
	if len(found) == 0 {
		return uuid.Nil, changes.Refuse(http.StatusNotFound, "unknownUser")
	}
	held, err := c.Q.PermissionsOf(c.Ctx, store.PermissionsOfParams{ProjectID: c.ProjectID, UserID: found[0].ID})
	if err != nil || len(held) > 0 {
		return found[0].ID, err
	}
	return found[0].ID, changeOf(c).addPermission(c.ProjectID, found[0].ID, access.ReadProject)
}

// give ticks a permission for someone in the project: 404 when they are not in it, invalidPermission (400) for one that is not a project's.
func (u Routes) give(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Give(c, userID, r.PathValue("permission")) })
}

// Give ticks the permission named permission for userID in the change's project, as give does: the rule both doors call, the app's route and
// the web's Users (redix-api #173). NotFound when they are not in it; invalidPermission (400) for one that is not a project's.
func Give(c changes.Change, userID uuid.UUID, permission string) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	given, ok := access.Named(permission)
	if !ok || given == access.ApproveProjects {
		return changes.Refuse(http.StatusBadRequest, "invalidPermission")
	}
	return changeOf(c).give(c.ProjectID, userID, given)
}

// take removes a permission from someone in the project; taking ReadProject takes them out of it, with every permission, their role name and
// their being inactive. lastManager (409) when it would leave the project with no active manager of its users.
func (u Routes) take(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Take(c, userID, r.PathValue("permission")) })
}

// Take removes the permission named permission from userID in the change's project, as take does: the rule both doors call, the app's route and
// the web's Users (redix-api #173). NotFound when they are not in it; invalidPermission (400); lastManager (409).
func Take(c changes.Change, userID uuid.UUID, permission string) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	taking, ok := access.Named(permission)
	if !ok || taking == access.ApproveProjects {
		return changes.Refuse(http.StatusBadRequest, "invalidPermission")
	}
	return changeOf(c).take(c.ProjectID, userID, taking)
}

// seeTablesRequest is the tables one tick changes for someone: all of an item's, or one (findings row 280).
type seeTablesRequest struct {
	Seen     bool        `json:"seen"`
	TableIDs []uuid.UUID `json:"tableIds"`
}

// seeTables ticks the tables tableIds for someone in the project when seen, or unticks them, so they no longer see their entries nor count them
// (#106), all in one request and one transaction, a History line each (findings row 280: an item's tick is one request): 404 for a table that
// is neither a group nor a contract of the project, or for someone not in it, and then nothing changes.
func (u Routes) seeTables(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request seeTablesRequest
	if !web.Read(w, r, &request) {
		return
	}
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error {
		return SeeTables(c, userID, request.Seen, request.TableIDs)
	})
}

// SeeTables ticks tableIDs for userID in the change's project when seen, or unticks them, as seeTables does: the rule both doors call, the app's
// route and the web's Users (redix-api #173). NotFound when they are not in it, or for a table that is neither a group nor a contract of the
// project, and then nothing changes.
func SeeTables(c changes.Change, userID uuid.UUID, seen bool, tableIDs []uuid.UUID) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	for _, tableID := range tableIDs {
		known, err := isTable(c.Ctx, c.Q, c.ProjectID, tableID)
		if err != nil {
			return err
		}
		if !known {
			return changes.NotFound
		}
	}
	apply := changeOf(c).unsee
	if seen {
		apply = changeOf(c).see
	}
	for _, tableID := range tableIDs {
		if err := apply(c.ProjectID, userID, tableID); err != nil {
			return err
		}
	}
	return nil
}

// choose shows a partner's capital to someone in the project, ticking SeeChosenCapital: 404 when they are not in it or the partner is not one of
// the project's (#106).
func (u Routes) choose(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Choose(c, userID, web.ID(r, "partnerId")) })
}

// Choose shows partnerID's capital to userID in the change's project, ticking SeeChosenCapital, as choose does: the rule both doors call, the
// app's route and the web's Users (redix-api #173). NotFound when they are not in it or the partner is not one of the project's (#106).
func Choose(c changes.Change, userID, partnerID uuid.UUID) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	kept, err := c.Q.KeptPartnerIDs(c.Ctx, store.KeptPartnerIDsParams{ProjectID: c.ProjectID, Ids: []uuid.UUID{partnerID}})
	if err != nil {
		return err
	}
	if len(kept) == 0 {
		return changes.NotFound
	}
	if err := changeOf(c).give(c.ProjectID, userID, access.SeeChosenCapital); err != nil {
		return err
	}
	chosen, err := c.Q.ChosenOf(c.Ctx, store.ChosenOfParams{ProjectID: c.ProjectID, UserID: userID})
	if err != nil || slices.ContainsFunc(chosen, func(row store.ChosenPartner) bool { return row.PartnerID == partnerID }) {
		return err
	}
	return changeOf(c).choose(c.ProjectID, userID, partnerID)
}

// unchoose stops showing a partner's capital to someone in the project.
func (u Routes) unchoose(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Unchoose(c, userID, web.ID(r, "partnerId")) })
}

// Unchoose stops showing partnerID's capital to userID in the change's project, as unchoose does: the rule both doors call (redix-api #173).
// NotFound when they are not in it.
func Unchoose(c changes.Change, userID, partnerID uuid.UUID) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	return changeOf(c).clearChosen(c.ProjectID, userID, partnerID)
}

// roleNameRequest is the body PUT /projects/{projectId}/users/{userId}/role-name (UC-008) reads.
type roleNameRequest struct {
	RoleName string `json:"roleName"`
}

// setRoleName gives someone in the project a free-text role name, "" taking it away (the owner, 9 Nov 2026): invalidRoleName (400) past 50 characters.
func (u Routes) setRoleName(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request roleNameRequest
	if !web.Read(w, r, &request) {
		return
	}
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return NameRole(c, userID, request.RoleName) })
}

// NameRole gives userID in the change's project the free-text role name, "" taking it away, as setRoleName does: the rule both doors call, the
// app's route and the web's Users (redix-api #173). NotFound when they are not in it; invalidRoleName (400) past 50 characters.
func NameRole(c changes.Change, userID uuid.UUID, roleName string) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	roleName = strings.TrimSpace(roleName)
	if utf8.RuneCountInString(roleName) > roleNameMaxLength {
		return changes.Refuse(http.StatusBadRequest, "invalidRoleName")
	}
	return changeOf(c).nameRole(c.ProjectID, userID, roleName)
}

// disable makes someone inactive in the project (#126): lastManager (409) when they are its only active manager of users.
func (u Routes) disable(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Disable(c, userID) })
}

// Disable makes userID inactive in the change's project (#126), as disable does: the rule both doors call, the app's route and the web's Users
// (redix-api #173). NotFound when they are not in it; lastManager (409) when they are its only active manager of users.
func Disable(c changes.Change, userID uuid.UUID) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	last, err := isTheLastManager(c.Ctx, c.Q, c.ProjectID, userID)
	if err != nil {
		return err
	}
	if last {
		return changes.Refuse(http.StatusConflict, "lastManager")
	}
	return changeOf(c).disable(c.ProjectID, userID)
}

// enable makes someone active in the project again (#126).
func (u Routes) enable(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(c changes.Change, userID uuid.UUID) error { return Enable(c, userID) })
}

// Enable makes userID active in the change's project again (#126), as enable does: the rule both doors call (redix-api #173). NotFound when they
// are not in it.
func Enable(c changes.Change, userID uuid.UUID) error {
	if err := inProject(c, userID); err != nil {
		return err
	}
	return changeOf(c).endInactive(c.ProjectID, userID, history.Enable)
}

// member runs act on the person the route names in the route's project, through the shared writer, and answers them with what they hold there.
func (u Routes) member(w http.ResponseWriter, r *http.Request, caller access.Caller, act func(c changes.Change, userID uuid.UUID) error) {
	projectID, userID := web.ID(r, "projectId"), web.ID(r, "userId")
	if u.writer.Write(w, r, caller, func(c changes.Change) error { return act(c, userID) }) {
		u.answerInProject(w, r, http.StatusOK, projectID, userID)
	}
}

// inProject is NotFound unless userID is in the change's project.
func inProject(c changes.Change, userID uuid.UUID) error {
	held, err := c.Q.PermissionsOf(c.Ctx, store.PermissionsOfParams{ProjectID: c.ProjectID, UserID: userID})
	if err == nil && len(held) == 0 {
		return changes.NotFound
	}
	return err
}

// changeOf is the users' own change from c, the change both doors run.
func changeOf(c changes.Change) change {
	return change{ctx: c.Ctx, q: c.Q, actor: c.Actor, now: c.Now}
}

// answerInProject answers userID with what they hold in projectID (ViewInProjectAsync).
func (u Routes) answerInProject(w http.ResponseWriter, r *http.Request, status int, projectID, userID uuid.UUID) {
	u.answer(w, r, status, userID, func(q *store.Queries) ([]store.UserProject, error) {
		return q.ProjectOfUser(r.Context(), store.ProjectOfUserParams{ProjectID: projectID, UserID: userID})
	})
}

func (u Routes) answer(w http.ResponseWriter, r *http.Request, status int, userID uuid.UUID, projects func(*store.Queries) ([]store.UserProject, error)) {
	read := u.database.Queries()
	user, err := read.UserWithID(r.Context(), userID)
	rows, rowsErr := projects(read)
	if err != nil || rowsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, status, viewOf(user, rows))
}
