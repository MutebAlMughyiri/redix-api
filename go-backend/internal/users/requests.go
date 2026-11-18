// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users

import (
	"cmp"
	"encoding/json"
	"net/http"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// A permission request (redix-api #112): a person asks, from صلاحياتي, for permissions given and taken away in a project, with a reason; whoever
// manages its users approves it, applying it as ticking does, or rejects it with a reason. One waits per person per project: a new one replaces it
// (D3).

// requestStatus is where a request stands, as lk_permission_request_status numbers it.
type requestStatus int64

const (
	waiting requestStatus = iota
	approved
	rejected
)

var requestStatuses = web.Enum{"waiting", "approved", "rejected"}

func (s requestStatus) MarshalJSON() ([]byte, error) { return requestStatuses.Write(int64(s)) }

// reasonMaxLength is the longest reason a request or a rejection gives.
const reasonMaxLength = 500

// requestView is a request as the app shows it: who asked, what they ask given and taken, why, when, and where it stands.
type requestView struct {
	ID              uuid.UUID           `json:"id"`
	UserID          uuid.UUID           `json:"userId"`
	DisplayName     string              `json:"displayName"`
	Gives           []access.Permission `json:"gives"`
	Takes           []access.Permission `json:"takes"`
	Reason          string              `json:"reason"`
	AskedAt         moment.Moment       `json:"askedAt"`
	Status          requestStatus       `json:"status"`
	RejectionReason string              `json:"rejectionReason"`
}

func viewOfRequest(row store.PermissionRequest, displayName string) requestView {
	return requestView{ID: row.ID, UserID: row.UserID, DisplayName: displayName, Gives: permissionsIn(row.Gives), Takes: permissionsIn(row.Takes),
		Reason: row.Reason, AskedAt: row.AskedAt, Status: requestStatus(row.Status), RejectionReason: row.RejectionReason}
}

// permissionsIn is the permissions a request keeps as JSON, each by its name as the API writes it.
func permissionsIn(kept string) []access.Permission {
	var names []string
	json.Unmarshal([]byte(kept), &names)
	permissions := []access.Permission{}
	for _, name := range names {
		permission, _ := access.Named(name)
		permissions = append(permissions, permission)
	}
	return permissions
}

func requestFields(row store.PermissionRequest) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "UserId": row.UserID, "Gives": row.Gives, "Takes": row.Takes, "Reason": row.Reason,
		"Status": row.Status, "RejectionReason": row.RejectionReason}
}

// mapRequests routes the requests: the person's own, and the managers' list and decisions.
func (u Routes) mapRequests(router *web.Router, gate *access.Gate) {
	const requests = "/projects/{projectId:guid}/permission-requests"
	router.Handle("GET", requests+"/mine", gate.Needs(access.ReadProject, u.myRequest))
	router.Handle("POST", requests, gate.Changes(access.ReadProject, u.ask))
	router.Handle("DELETE", requests+"/mine", gate.Changes(access.ReadProject, u.withdraw))
	router.Handle("GET", requests, gate.Needs(access.ManageUsers, u.waitingRequests))
	router.Handle("POST", requests+"/{requestId:guid}/approve", gate.Changes(access.ManageUsers, u.approve))
	router.Handle("POST", requests+"/{requestId:guid}/reject", gate.Changes(access.ManageUsers, u.reject))
}

// myRequest is the person's latest request in the project, waiting or decided, as a list of one; empty when they never asked (#112 R1).
func (u Routes) myRequest(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	rows, err := u.database.Queries().LatestPermissionRequestOf(r.Context(), store.LatestPermissionRequestOfParams{ProjectID: web.ID(r, "projectId"), UserID: caller.UserID})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []requestView{}
	for _, row := range rows {
		views = append(views, viewOfRequest(row, ""))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// askRequest is what a person sends: the permissions to give and to take, by name, and why.
type askRequest struct {
	Gives  []string `json:"gives,omitzero"`
	Takes  []string `json:"takes,omitzero"`
	Reason string   `json:"reason"`
}

// ask keeps the person's request, replacing the one waiting (D3), and tells whoever manages the project's users (R6). It asks only for what
// changes something (R10): nothingAsked for nothing, invalidPermission for a name no project permission has or for ReadProject, being in the
// project; alreadyHeld for a permission to give they hold, notHeld for one to take they lack; reasonRequired without a reason (R8).
func (u Routes) ask(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var asked askRequest
	if !web.Read(w, r, &asked) {
		return
	}
	projectID := web.ID(r, "projectId")
	held := caller.Access.In(projectID)
	gives, givesCode := permissionsNamed(asked.Gives, func(p access.Permission) bool { return !slices.Contains(held, p) }, "alreadyHeld")
	takes, takesCode := permissionsNamed(asked.Takes, func(p access.Permission) bool { return slices.Contains(held, p) }, "notHeld")
	reason, reasonCode := reasonOf(asked.Reason)
	code := cmp.Or(givesCode, takesCode, reasonCode)
	if code == "" && len(gives)+len(takes) == 0 {
		code = "nothingAsked"
	}
	if code != "" {
		problem.Refuse(w, r, http.StatusBadRequest, code)
		return
	}
	givesText, _ := json.Marshal(gives)
	takesText, _ := json.Marshal(takes)
	row := store.PermissionRequest{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, UserID: caller.UserID, Gives: string(givesText),
		Takes: string(takesText), Reason: reason, Status: int64(waiting)}
	err := u.write(r, caller, func(c change) error {
		row.AskedAt = c.now
		if err := c.withdrawWaiting(projectID, caller.UserID); err != nil {
			return err
		}
		if err := c.q.AddPermissionRequest(c.ctx, store.AddPermissionRequestParams(row)); err != nil {
			return err
		}
		if err := c.record(history.Create, history.Change{Table: "PermissionRequests", RecordID: row.ID, ProjectID: projectID, After: requestFields(row)}); err != nil {
			return err
		}
		return notifications.TellManagers(c.ctx, c.q, projectID, notifications.PermissionRequested, c.now)
	})
	if !changes.Answered(w, r, err) {
		web.JSON(w, r, http.StatusCreated, viewOfRequest(row, ""))
	}
}

// withdraw takes back the person's waiting request: 404 for none.
func (u Routes) withdraw(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	projectID := web.ID(r, "projectId")
	err := u.write(r, caller, func(c change) error {
		rows, err := c.q.WaitingPermissionRequestOf(c.ctx, store.WaitingPermissionRequestOfParams{ProjectID: projectID, UserID: caller.UserID})
		if err != nil {
			return err
		}
		if len(rows) == 0 {
			return changes.Refuse(http.StatusNotFound, "")
		}
		return c.withdrawWaiting(projectID, caller.UserID)
	})
	if !changes.Answered(w, r, err) {
		w.WriteHeader(http.StatusOK)
	}
}

// withdrawWaiting removes userID's waiting request in projectID, if any.
func (c change) withdrawWaiting(projectID, userID uuid.UUID) error {
	rows, err := c.q.WaitingPermissionRequestOf(c.ctx, store.WaitingPermissionRequestOfParams{ProjectID: projectID, UserID: userID})
	for _, row := range rows {
		if err == nil {
			err = c.q.RemovePermissionRequest(c.ctx, row.ID)
		}
		if err == nil {
			err = c.record(history.Delete, history.Change{Table: "PermissionRequests", RecordID: row.ID, ProjectID: projectID, Before: requestFields(row)})
		}
	}
	return err
}

// waitingRequests is every request waiting in the project, oldest first, each with who asked (R3).
func (u Routes) waitingRequests(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	rows, err := u.database.Queries().WaitingPermissionRequests(r.Context(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []requestView{}
	for _, row := range rows {
		views = append(views, viewOfRequest(row.PermissionRequest, row.DisplayName))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// approve applies a waiting request through the path ticking takes, so the last manager keeps managing (R4, BR-29: lastManager, 409); what no
// longer changes anything is left (R11). The person is told (R6). 404 for no request waiting in the project.
func (u Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	u.decide(w, r, caller, approved, "", func(c change, row store.PermissionRequest) error {
		for _, given := range permissionsIn(row.Gives) {
			if err := c.give(row.ProjectID, row.UserID, given); err != nil {
				return err
			}
		}
		for _, taken := range permissionsIn(row.Takes) {
			if err := c.take(row.ProjectID, row.UserID, taken); err != nil {
				return err
			}
		}
		return nil
	})
}

// rejectRequest is a rejection's reason.
type rejectRequest struct {
	Reason string `json:"reason"`
}

// reject refuses a waiting request with its reason (R8), changing no permission; the person is told (R6).
func (u Routes) reject(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var rejection rejectRequest
	if !web.Read(w, r, &rejection) {
		return
	}
	reason, code := reasonOf(rejection.Reason)
	if code != "" {
		problem.Refuse(w, r, http.StatusBadRequest, code)
		return
	}
	u.decide(w, r, caller, rejected, reason, func(change, store.PermissionRequest) error { return nil })
}

// decide closes the waiting request named in the path as status, with reason when rejected, after apply, and tells the person who asked.
func (u Routes) decide(w http.ResponseWriter, r *http.Request, caller access.Caller, status requestStatus, reason string, apply func(change, store.PermissionRequest) error) {
	projectID, requestID := web.ID(r, "projectId"), web.ID(r, "requestId")
	err := u.write(r, caller, func(c change) error {
		row, err := changes.Found(c.q.PermissionRequestWithID(c.ctx, requestID))
		if err == nil && (row.ProjectID != projectID || requestStatus(row.Status) != waiting) {
			err = changes.Refuse(http.StatusNotFound, "")
		}
		if err == nil {
			err = apply(c, row)
		}
		if err != nil {
			return err
		}
		decided := row
		decided.Status, decided.RejectionReason = int64(status), reason
		if err := c.q.DecidePermissionRequest(c.ctx, store.DecidePermissionRequestParams{ID: row.ID, Status: decided.Status, RejectionReason: reason}); err != nil {
			return err
		}
		action := history.Approve
		if status == rejected {
			action = history.Reject
		}
		if err := c.record(action, history.Change{Table: "PermissionRequests", RecordID: row.ID, ProjectID: projectID, Before: requestFields(row), After: requestFields(decided)}); err != nil {
			return err
		}
		kind := notifications.PermissionRequestApproved
		if status == rejected {
			kind = notifications.PermissionRequestRejected
		}
		return c.q.AddNotification(c.ctx, store.AddNotificationParams{ID: uuid.Must(uuid.NewV7()), UserID: row.UserID, ProjectID: projectID, Kind: int64(kind), At: c.now})
	})
	if !changes.Answered(w, r, err) {
		w.WriteHeader(http.StatusOK)
	}
}

// permissionsNamed is the project permissions names names, each once, when every one is one of them, but ReadProject, and fits; otherwise the
// code saying why not: invalidPermission, or unfit when fits refuses one.
func permissionsNamed(names []string, fits func(access.Permission) bool, unfit string) ([]access.Permission, string) {
	permissions := []access.Permission{}
	for _, name := range names {
		permission, known := access.Named(name)
		switch {
		case !known || permission == access.ApproveProjects || permission == access.ReadProject:
			return nil, "invalidPermission"
		case !fits(permission):
			return nil, unfit
		case !slices.Contains(permissions, permission):
			permissions = append(permissions, permission)
		}
	}
	return permissions, ""
}

// reasonOf is a request's or a rejection's reason, trimmed: reasonRequired when empty, invalidReason past reasonMaxLength.
func reasonOf(sent string) (string, string) {
	reason := strings.TrimSpace(sent)
	switch {
	case reason == "":
		return "", "reasonRequired"
	case utf8.RuneCountInString(reason) > reasonMaxLength:
		return "", "invalidReason"
	}
	return reason, ""
}
