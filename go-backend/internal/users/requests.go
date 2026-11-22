// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users

import (
	"cmp"
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
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

// MarshalJSON writes the request's status by its name.
func (s requestStatus) MarshalJSON() ([]byte, error) { return requestStatuses.Write(int64(s)) }

// reasonMaxLength is the longest reason a request or a rejection gives.
const reasonMaxLength = 500

// RequestView is a request as the app and the web show it: who asked, what they ask given and taken, why, when, and where it stands.
type RequestView struct {
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

// StatusName is where the request stands by its name, as the app's words key it: waiting, approved or rejected.
func (v RequestView) StatusName() string { return requestStatuses[v.Status] }

// IsWaiting says whether the request still waits for a decision, so it may be withdrawn.
func (v RequestView) IsWaiting() bool { return v.Status == waiting }

func viewOfRequest(row store.PermissionRequest, displayName string) (RequestView, error) {
	gives, givesErr := permissionsIn(row.Gives)
	takes, takesErr := permissionsIn(row.Takes)
	if err := errors.Join(givesErr, takesErr); err != nil {
		return RequestView{}, err
	}
	return RequestView{ID: row.ID, UserID: row.UserID, DisplayName: displayName, Gives: gives, Takes: takes,
		Reason: row.Reason, AskedAt: row.AskedAt, Status: requestStatus(row.Status), RejectionReason: row.RejectionReason}, nil
}

// permissionsIn is the permissions a request keeps as JSON, each by its name as the API writes it.
func permissionsIn(kept string) ([]access.Permission, error) {
	var names []string
	if err := json.Unmarshal([]byte(kept), &names); err != nil {
		return nil, err
	}
	var permissions []access.Permission
	for _, name := range names {
		permission, _ := access.Named(name)
		permissions = append(permissions, permission)
	}
	return permissions, nil
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
	views, err := MyRequestOf(r.Context(), u.database.Queries(), web.ID(r, "projectId"), caller.UserID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// MyRequestOf is userID's latest request in projectID, waiting or decided, as a list of one, as myRequest answers it: the rule both doors call,
// the app's route and the web's My permissions (redix-api #173); empty when they never asked. The error is the database's.
func MyRequestOf(ctx context.Context, q *store.Queries, projectID, userID uuid.UUID) ([]RequestView, error) {
	rows, err := q.LatestPermissionRequestOf(ctx, store.LatestPermissionRequestOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return nil, err
	}
	var views []RequestView
	for _, row := range rows {
		view, err := viewOfRequest(row, "")
		if err != nil {
			return nil, err
		}
		views = append(views, view)
	}
	return views, nil
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
	var view RequestView
	if u.writer.Write(w, r, caller, func(c changes.Change) (err error) {
		view, err = Ask(c, asked.Gives, asked.Takes, asked.Reason)
		return err
	}) {
		web.JSON(w, r, http.StatusCreated, view)
	}
}

// Ask keeps the change's actor's request in its project for the permissions named gives and takes, with reason, as ask does: the rule both doors
// call, the app's route and the web's Ask for permissions (redix-api #173). Refused with nothingAsked, invalidPermission, alreadyHeld, notHeld,
// reasonRequired or invalidReason (400); it is the request kept.
func Ask(c changes.Change, gives, takes []string, reason string) (RequestView, error) {
	held := c.Access.In(c.ProjectID)
	given, givesCode := permissionsNamed(gives, func(p access.Permission) bool { return !slices.Contains(held, p) }, "alreadyHeld")
	taken, takesCode := permissionsNamed(takes, func(p access.Permission) bool { return slices.Contains(held, p) }, "notHeld")
	why, reasonCode := reasonOf(reason)
	code := cmp.Or(givesCode, takesCode, reasonCode)
	if code == "" && len(given)+len(taken) == 0 {
		code = "nothingAsked"
	}
	if code != "" {
		return RequestView{}, changes.Refuse(http.StatusBadRequest, code)
	}
	givesText, _ := encode.JSON(given)
	takesText, _ := encode.JSON(taken)
	row := store.PermissionRequest{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, UserID: c.Actor, Gives: string(givesText),
		Takes: string(takesText), Reason: why, Status: int64(waiting), AskedAt: c.Now}
	made := changeOf(c)
	if err := made.withdrawWaiting(c.ProjectID, c.Actor); err != nil {
		return RequestView{}, err
	}
	if err := c.Q.AddPermissionRequest(c.Ctx, store.AddPermissionRequestParams(row)); err != nil {
		return RequestView{}, err
	}
	if err := made.record(history.Create, history.Change{Table: "PermissionRequests", RecordID: row.ID, ProjectID: c.ProjectID, After: requestFields(row)}); err != nil {
		return RequestView{}, err
	}
	if err := notifications.TellManagers(c.Ctx, c.Q, c.ProjectID, notifications.PermissionRequested, c.Now); err != nil {
		return RequestView{}, err
	}
	return viewOfRequest(row, "")
}

// withdraw takes back the person's waiting request: 404 for none.
func (u Routes) withdraw(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if u.writer.Write(w, r, caller, func(c changes.Change) error { return Withdraw(c) }) {
		w.WriteHeader(http.StatusOK)
	}
}

// Withdraw takes back the change's actor's waiting request in its project, as withdraw does: the rule both doors call, the app's route and the
// web's My permissions (redix-api #173). NotFound for none waiting.
func Withdraw(c changes.Change) error {
	rows, err := c.Q.WaitingPermissionRequestOf(c.Ctx, store.WaitingPermissionRequestOfParams{ProjectID: c.ProjectID, UserID: c.Actor})
	if err != nil {
		return err
	}
	if len(rows) == 0 {
		return changes.NotFound
	}
	return changeOf(c).withdrawWaiting(c.ProjectID, c.Actor)
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
	views, err := WaitingRequestsOf(r.Context(), u.database.Queries(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// WaitingRequestsOf is every request waiting in projectID, oldest first, each with who asked, as waitingRequests answers them: the rule both
// doors call, the app's route and the web's Permission requests (redix-api #173). The error is the database's.
func WaitingRequestsOf(ctx context.Context, q *store.Queries, projectID uuid.UUID) ([]RequestView, error) {
	rows, err := q.WaitingPermissionRequests(ctx, projectID)
	if err != nil {
		return nil, err
	}
	var views []RequestView
	for _, row := range rows {
		view, err := viewOfRequest(row.PermissionRequest, row.DisplayName)
		if err != nil {
			return nil, err
		}
		views = append(views, view)
	}
	return views, nil
}

// approve applies a waiting request through the path ticking takes, so the last manager keeps managing (R4, BR-29: lastManager, 409); what no
// longer changes anything is left (R11). The person is told (R6). 404 for no request waiting in the project.
func (u Routes) approve(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if u.writer.Write(w, r, caller, func(c changes.Change) error { return Approve(c, web.ID(r, "requestId")) }) {
		w.WriteHeader(http.StatusOK)
	}
}

// Approve applies the request requestID waiting in the change's project and closes it, as approve does: the rule both doors call, the app's
// route and the web's Permission requests (redix-api #173). NotFound for no such request waiting; lastManager (409).
func Approve(c changes.Change, requestID uuid.UUID) error {
	return decide(c, requestID, approved, "", func(c change, row store.PermissionRequest) error {
		gives, givesErr := permissionsIn(row.Gives)
		takes, takesErr := permissionsIn(row.Takes)
		if err := errors.Join(givesErr, takesErr); err != nil {
			return err
		}
		for _, given := range gives {
			if err := c.give(row.ProjectID, row.UserID, given); err != nil {
				return err
			}
		}
		for _, taken := range takes {
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
	if u.writer.Write(w, r, caller, func(c changes.Change) error { return Reject(c, web.ID(r, "requestId"), rejection.Reason) }) {
		w.WriteHeader(http.StatusOK)
	}
}

// Reject refuses the request requestID waiting in the change's project with reason, changing no permission, as reject does: the rule both doors
// call, the app's route and the web's Permission requests (redix-api #173). Refused with reasonRequired or invalidReason (400); NotFound for no
// such request waiting.
func Reject(c changes.Change, requestID uuid.UUID, reason string) error {
	why, code := reasonOf(reason)
	if code != "" {
		return changes.Refuse(http.StatusBadRequest, code)
	}
	return decide(c, requestID, rejected, why, func(change, store.PermissionRequest) error { return nil })
}

// decide closes the request requestID waiting in the change's project as status, with reason when rejected, after apply, and tells the person
// who asked. NotFound for no such request waiting.
func decide(c changes.Change, requestID uuid.UUID, status requestStatus, reason string, apply func(change, store.PermissionRequest) error) error {
	row, err := changes.Found(c.Q.PermissionRequestWithID(c.Ctx, requestID))
	if err == nil && (row.ProjectID != c.ProjectID || requestStatus(row.Status) != waiting) {
		err = changes.NotFound
	}
	made := changeOf(c)
	if err == nil {
		err = apply(made, row)
	}
	if err != nil {
		return err
	}
	decided := row
	decided.Status, decided.RejectionReason = int64(status), reason
	if err := c.Q.DecidePermissionRequest(c.Ctx, store.DecidePermissionRequestParams{ID: row.ID, Status: decided.Status, RejectionReason: reason}); err != nil {
		return err
	}
	action := history.Approve
	if status == rejected {
		action = history.Reject
	}
	if err := made.record(action, history.Change{Table: "PermissionRequests", RecordID: row.ID, ProjectID: c.ProjectID, Before: requestFields(row), After: requestFields(decided)}); err != nil {
		return err
	}
	kind := notifications.PermissionRequestApproved
	if status == rejected {
		kind = notifications.PermissionRequestRejected
	}
	return c.Q.AddNotification(c.Ctx, store.AddNotificationParams{ID: uuid.Must(uuid.NewV7()), UserID: row.UserID, ProjectID: c.ProjectID, Kind: int64(kind), At: c.Now})
}

// permissionsNamed is the project permissions names names, each once, when every one is one of them, but ReadProject, and fits; otherwise the
// code saying why not: invalidPermission, or unfit when fits refuses one.
func permissionsNamed(names []string, fits func(access.Permission) bool, unfit string) ([]access.Permission, string) {
	var permissions []access.Permission
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
