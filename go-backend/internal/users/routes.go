// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users

import (
	"context"
	"database/sql"
	"encoding/json"
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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Routes keeps people and who is in each project: editing a person and resetting their password (UC-007), and a project's members, its users and
// what each may do there (FR-02, BR-29 to BR-33, #120, #126).
type Routes struct {
	database db.DB
	clock    clock.Clock
}

// Map routes PUT /users/{userId} and POST /users/{userId}/reset-password for a working session; GET /projects/{projectId}/members for whoever reads
// the project; and the project's users routes for whoever manages its users.
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	u := Routes{database: database, clock: clock}
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

// projectView is what a person holds in one project (UserProjectView).
type projectView struct {
	ProjectID   uuid.UUID           `json:"projectId"`
	Permissions []access.Permission `json:"permissions"`
	RoleName    string              `json:"roleName"`
	PartnerIDs  []uuid.UUID         `json:"partnerIds"`
	IsInactive  bool                `json:"isInactive"`
	// UnseenTables are the tables unticked for the person: the groups and contracts they do not see (#106).
	UnseenTables []uuid.UUID `json:"unseenTables"`
}

// userView is a person and what they hold in their projects (UserView).
type userView struct {
	ID                 uuid.UUID     `json:"id"`
	Username           string        `json:"username"`
	DisplayName        string        `json:"displayName"`
	Mobile             string        `json:"mobile"`
	Email              string        `json:"email"`
	MustChangePassword bool          `json:"mustChangePassword"`
	Projects           []projectView `json:"projects"`
}

// memberView is someone active in a project, as a picker lists them (MemberView).
type memberView struct {
	ID          uuid.UUID `json:"id"`
	DisplayName string    `json:"displayName"`
}

func viewOf(user store.User, rows []store.UserProject) userView {
	var projects []projectView
	for _, row := range rows {
		// The user_projects view builds these three with json_group_array, so each is an array and reads without fail.
		var names []string
		var partners []uuid.UUID
		var unseen []uuid.UUID
		_ = json.Unmarshal([]byte(row.Permissions), &names)
		_ = json.Unmarshal([]byte(row.PartnerIds), &partners)
		_ = json.Unmarshal([]byte(row.UnseenTables), &unseen)
		var permissions []access.Permission
		for _, name := range names {
			if permission, ok := access.Named(name); ok {
				permissions = append(permissions, permission)
			}
		}
		slices.SortFunc(permissions, func(a, b access.Permission) int { return a.Number() - b.Number() })
		projects = append(projects, projectView{ProjectID: row.ProjectID, Permissions: permissions, RoleName: row.RoleName, PartnerIDs: partners,
			IsInactive: row.IsInactive, UnseenTables: unseen})
	}
	return userView{ID: user.ID, Username: user.Username, DisplayName: user.DisplayName, Mobile: user.Mobile, Email: user.Email, MustChangePassword: user.MustChangePassword, Projects: projects}
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
	name, mobile, email := strings.TrimSpace(request.DisplayName), strings.TrimSpace(request.Mobile), strings.TrimSpace(request.Email)
	u.changeUser(w, r, caller, history.Update, func(user store.User) (int, string, error) {
		if refusal := RefusalOfDetails(name, mobile); refusal != "" {
			return http.StatusBadRequest, refusal, nil
		}
		return RefusalOfEmail(r.Context(), u.database.Queries(), email, user.ID)
	}, func(ctx context.Context, q *store.Queries, user store.User) (store.User, error) {
		if err := q.SetDetails(ctx, store.SetDetailsParams{ID: user.ID, DisplayName: name, Mobile: mobile}); err != nil {
			return user, err
		}
		user.DisplayName, user.Mobile = name, mobile
		return ChangeEmail(ctx, q, user, email)
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
	u.changeUser(w, r, caller, history.ResetPassword, func(user store.User) (int, string, error) {
		if !KeepsPolicy(request.TemporaryPassword, user.Username) {
			return http.StatusBadRequest, "weakPassword", nil
		}
		return 0, "", nil
	}, func(ctx context.Context, q *store.Queries, user store.User) (store.User, error) {
		return ResetPassword(ctx, q, user, request.TemporaryPassword, true)
	})
}

// changeUser changes the person the route names, records it, and answers them with all their projects (UsersEndpoints.ChangeAsync): 404 for no
// such person; otherProject (403) unless the caller manages the users of every project they are in and they are not an admin.
func (u Routes) changeUser(w http.ResponseWriter, r *http.Request, caller access.Caller, action history.Action,
	refusalOf func(store.User) (int, string, error), apply func(context.Context, *store.Queries, store.User) (store.User, error)) {
	read := u.database.Queries()
	user, err := read.UserWithID(r.Context(), web.ID(r, "userId"))
	if errors.Is(err, sql.ErrNoRows) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	mayChange, mayErr := u.mayChange(r.Context(), read, caller, user.ID)
	if err != nil || mayErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if !mayChange {
		problem.Refuse(w, r, http.StatusForbidden, "otherProject")
		return
	}
	status, refusal, err := refusalOf(user)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, status, refusal)
		return
	}
	now := moment.Of(u.clock.Now())
	err = u.database.InTx(r.Context(), func(q *store.Queries) error {
		changed, err := apply(r.Context(), q, user)
		if err != nil {
			return err
		}
		return history.Write(r.Context(), q, now, caller.UserID, action, Change(user, changed))
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	u.answer(w, r, http.StatusOK, user.ID, func(q *store.Queries) ([]store.UserProject, error) { return q.ProjectsOfUser(r.Context(), user.ID) })
}

// mayChange says whether the caller may change userID: not an admin, in at least one project, and every one of them one whose users the caller
// manages (UsersEndpoints.MayChangeAsync).
func (u Routes) mayChange(ctx context.Context, read *store.Queries, caller access.Caller, userID uuid.UUID) (bool, error) {
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
	projectID := web.ID(r, "projectId")
	read := u.database.Queries()
	people, err := read.UsersInProject(r.Context(), projectID)
	held, heldErr := read.UsersOfProject(r.Context(), projectID)
	if err != nil || heldErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	var views []userView
	for _, person := range people {
		mine := slices.DeleteFunc(slices.Clone(held), func(row store.UserProject) bool { return row.UserID != person.ID })
		views = append(views, viewOf(person, mine))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// createUserRequest is the body POST /projects/{projectId}/users (UC-008) reads.
type createUserRequest struct {
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
	var request createUserRequest
	if !web.Read(w, r, &request) {
		return
	}
	projectID := web.ID(r, "projectId")
	roleName := strings.TrimSpace(request.RoleName)
	if utf8.RuneCountInString(roleName) > roleNameMaxLength {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidRoleName")
		return
	}
	if slices.Contains(request.Permissions, access.ApproveProjects) ||
		(slices.Contains(request.Permissions, access.SeeAllCapital) && slices.Contains(request.Permissions, access.SeeChosenCapital)) {
		problem.Refuse(w, r, http.StatusBadRequest, "invalidPermission")
		return
	}
	var partners []uuid.UUID
	if slices.Contains(request.Permissions, access.SeeChosenCapital) {
		for _, id := range request.PartnerIDs {
			if !slices.Contains(partners, id) {
				partners = append(partners, id)
			}
		}
	}
	read := u.database.Queries()
	kept, err := read.KeptPartnerIDs(r.Context(), store.KeptPartnerIDsParams{ProjectID: projectID, Ids: partners})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if len(kept) != len(partners) {
		problem.Refuse(w, r, http.StatusBadRequest, "unknownPartner")
		return
	}
	for _, tableID := range request.UnseenTableIDs {
		known, err := isTable(r.Context(), read, projectID, tableID)
		switch {
		case err != nil:
			problem.Status(w, r, http.StatusInternalServerError)
			return
		case !known:
			problem.Refuse(w, r, http.StatusBadRequest, "unknownTable")
			return
		}
	}
	now := moment.Of(u.clock.Now())
	user, status, refusal, err := New(r.Context(), read, request.DisplayName, request.Username, request.Mobile, request.TemporaryPassword, true, now)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if refusal != "" {
		problem.Refuse(w, r, status, refusal)
		return
	}
	user.Email = strings.TrimSpace(request.Email)
	if status, refusal, err := RefusalOfEmail(r.Context(), read, user.Email, uuid.Nil); err != nil || refusal != "" {
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
		} else {
			problem.Refuse(w, r, status, refusal)
		}
		return
	}
	ticked := append([]access.Permission{access.ReadProject}, request.Permissions...)
	err = u.write(r, caller, func(c change) error {
		if err := Add(c.ctx, c.q, user, c.actor, now); err != nil {
			return err
		}
		for i, permission := range ticked {
			if !slices.Contains(ticked[:i], permission) {
				if err := c.addPermission(projectID, user.ID, permission); err != nil {
					return err
				}
			}
		}
		for _, partnerID := range partners {
			if err := c.choose(projectID, user.ID, partnerID); err != nil {
				return err
			}
		}
		for _, tableID := range request.UnseenTableIDs {
			if err := c.unsee(projectID, user.ID, tableID); err != nil {
				return err
			}
		}
		return c.nameRole(projectID, user.ID, roleName)
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Location", "/users/"+user.ID.String())
	u.answerInProject(w, r, http.StatusCreated, projectID, user.ID)
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
	found, err := u.database.Queries().NonAdminNamed(r.Context(), strings.TrimSpace(request.Username))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if len(found) == 0 {
		problem.Refuse(w, r, http.StatusNotFound, "unknownUser")
		return
	}
	user := found[0]
	err = u.write(r, caller, func(c change) error {
		held, err := c.q.PermissionsOf(c.ctx, store.PermissionsOfParams{ProjectID: projectID, UserID: user.ID})
		if err != nil || len(held) > 0 {
			return err
		}
		return c.addPermission(projectID, user.ID, access.ReadProject)
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	u.answerInProject(w, r, http.StatusOK, projectID, user.ID)
}

// give ticks a permission for someone in the project: 404 when they are not in it, invalidPermission (400) for one that is not a project's.
func (u Routes) give(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		given, ok := access.Named(r.PathValue("permission"))
		if !ok || given == access.ApproveProjects {
			return http.StatusBadRequest, "invalidPermission", nil
		}
		return 0, "", func(c change) error { return c.give(projectID, userID, given) }
	})
}

// take removes a permission from someone in the project; taking ReadProject takes them out of it, with every permission, their role name and
// their being inactive. lastManager (409) when it would leave the project with no active manager of its users.
func (u Routes) take(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		taking, ok := access.Named(r.PathValue("permission"))
		if !ok || taking == access.ApproveProjects {
			return http.StatusBadRequest, "invalidPermission", nil
		}
		return 0, "", func(c change) error { return c.take(projectID, userID, taking) }
	})
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
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		for _, tableID := range request.TableIDs {
			known, err := isTable(r.Context(), u.database.Queries(), projectID, tableID)
			switch {
			case err != nil:
				return http.StatusInternalServerError, "", nil
			case !known:
				return http.StatusNotFound, "", nil
			}
		}
		return 0, "", func(c change) error {
			apply := c.unsee
			if request.Seen {
				apply = c.see
			}
			for _, tableID := range request.TableIDs {
				if err := apply(projectID, userID, tableID); err != nil {
					return err
				}
			}
			return nil
		}
	})
}

// choose shows a partner's capital to someone in the project, ticking SeeChosenCapital: 404 when they are not in it or the partner is not one of
// the project's (#106).
func (u Routes) choose(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		partnerID := web.ID(r, "partnerId")
		kept, err := u.database.Queries().KeptPartnerIDs(r.Context(), store.KeptPartnerIDsParams{ProjectID: projectID, Ids: []uuid.UUID{partnerID}})
		if err != nil {
			return http.StatusInternalServerError, "", nil
		}
		if len(kept) == 0 {
			return http.StatusNotFound, "", nil
		}
		return 0, "", func(c change) error {
			if err := c.give(projectID, userID, access.SeeChosenCapital); err != nil {
				return err
			}
			chosen, err := c.q.ChosenOf(c.ctx, store.ChosenOfParams{ProjectID: projectID, UserID: userID})
			if err != nil || slices.ContainsFunc(chosen, func(row store.ChosenPartner) bool { return row.PartnerID == partnerID }) {
				return err
			}
			return c.choose(projectID, userID, partnerID)
		}
	})
}

// unchoose stops showing a partner's capital to someone in the project.
func (u Routes) unchoose(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		partnerID := web.ID(r, "partnerId")
		return 0, "", func(c change) error { return c.clearChosen(projectID, userID, partnerID) }
	})
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
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		roleName := strings.TrimSpace(request.RoleName)
		if utf8.RuneCountInString(roleName) > roleNameMaxLength {
			return http.StatusBadRequest, "invalidRoleName", nil
		}
		return 0, "", func(c change) error { return c.nameRole(projectID, userID, roleName) }
	})
}

// disable makes someone inactive in the project (#126): lastManager (409) when they are its only active manager of users.
func (u Routes) disable(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		last, err := isTheLastManager(r.Context(), u.database.Queries(), projectID, userID)
		if err != nil {
			return http.StatusInternalServerError, "", nil
		}
		if last {
			return http.StatusConflict, "lastManager", nil
		}
		return 0, "", func(c change) error { return c.disable(projectID, userID) }
	})
}

// enable makes someone active in the project again (#126).
func (u Routes) enable(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.member(w, r, caller, func(projectID, userID uuid.UUID) (int, string, func(change) error) {
		return 0, "", func(c change) error { return c.endInactive(projectID, userID, history.Enable) }
	})
}

// member runs a change to someone in the project: 404 when they are not in it; then decide's refusal - a status with a code, or a bare status with
// none - or its change, recorded and answered with what they hold here.
func (u Routes) member(w http.ResponseWriter, r *http.Request, caller access.Caller, decide func(projectID, userID uuid.UUID) (int, string, func(change) error)) {
	projectID, userID := web.ID(r, "projectId"), web.ID(r, "userId")
	held, err := u.database.Queries().PermissionsOf(r.Context(), store.PermissionsOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if len(held) == 0 {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	status, refusal, apply := decide(projectID, userID)
	switch {
	case refusal != "":
		problem.Refuse(w, r, status, refusal)
		return
	case apply == nil:
		problem.Status(w, r, status)
		return
	}
	if !changes.Answered(w, r, u.write(r, caller, apply)) {
		u.answerInProject(w, r, http.StatusOK, projectID, userID)
	}
}

// write runs apply in one transaction, as caller, now.
func (u Routes) write(r *http.Request, caller access.Caller, apply func(change) error) error {
	now := moment.Of(u.clock.Now())
	return u.database.InTx(r.Context(), func(q *store.Queries) error {
		return apply(change{ctx: r.Context(), q: q, actor: caller.UserID, now: now})
	})
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
