// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"slices"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
)

// registeredLine is what a registered project's card says under its name, as the app's: its start date and its units.
func registeredLine(project projects.RegisteredView) string {
	return parts(append([]string{project.StartDate.Format("2006-01-02")}, project.Units...)...)
}

// managerLine is who registered it, as the app's card says: their name, username and mobile.
func managerLine(project projects.RegisteredView) string {
	return parts(project.ManagerName, project.ManagerUsername, project.ManagerMobile)
}

// registeredTab is Registered projects in its tab (UC-006, redix-api #175), from Settings, for whoever approves the projects people register:
// each waiting, the earliest start first, read through projects.WaitingOf, the rule the app's route answers. Anyone else sees its title only.
func (s site) registeredTab(r *http.Request, p page) (templ.Component, error) {
	if !p.mayApproveProjects() {
		return registeredList(p, nil, said{}), nil
	}
	waiting, err := projects.WaitingOf(r.Context(), s.Database.Queries())
	return registeredList(p, waiting, said{}), err
}

// rejectProjectDialog is a registered project's Reject in the details panel, as the app's sheet asks it: the project's name and the reason. 404
// without ApproveProjects, or for no project waiting with that id.
func (s site) rejectProjectDialog(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.mayApproveProjects() {
		http.NotFound(w, r)
		return
	}
	waiting, err := projects.WaitingOf(r.Context(), s.Database.Queries())
	if err != nil {
		failed(w)
		return
	}
	projectID, _ := uuid.Parse(r.PathValue("id"))
	at := slices.IndexFunc(waiting, func(project projects.RegisteredView) bool { return project.ID == projectID })
	if at < 0 {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, rejectProjectForm(p, waiting[at], "", ""))
}

// decideRegistered approves the registered project the path names, or rejects it with its reason, through projects.ApproveProject and
// projects.RejectProject, the rules the app's routes run, and draws Registered projects again; a rejection keeps its sheet, saying why, when
// refused. 403 without ApproveProjects; 404 for an action there is none of, or no project waiting with that id.
func (s site) decideRegistered(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	action := r.PathValue("action")
	if r.ParseForm() != nil || (action != "approve" && action != "reject") {
		http.NotFound(w, r)
		return
	}
	if !p.mayApproveProjects() {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	waiting, err := projects.WaitingOf(r.Context(), s.Database.Queries())
	if err != nil {
		failed(w)
		return
	}
	projectID, _ := uuid.Parse(r.PathValue("id"))
	at := slices.IndexFunc(waiting, func(project projects.RegisteredView) bool { return project.ID == projectID })
	if at < 0 {
		http.NotFound(w, r)
		return
	}
	err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), projectID, p.caller, func(c changes.Change) error {
		if action == "approve" {
			return projects.ApproveProject(c)
		}
		return projects.RejectProject(c, r.PostFormValue("reason"))
	})
	refusal, err := refusalOf("registered", err)
	if err != nil {
		failed(w)
		return
	}
	if action == "reject" && refusal != "" {
		draw(w, r, http.StatusOK, rejectProjectForm(p, waiting[at], strings.TrimSpace(r.PostFormValue("reason")), refusal))
		return
	}
	if waiting, err = projects.WaitingOf(r.Context(), s.Database.Queries()); err != nil {
		failed(w)
		return
	}
	if action == "reject" {
		draw(w, r, http.StatusOK, registeredBeside(p, waiting))
		return
	}
	draw(w, r, http.StatusOK, registeredList(p, waiting, said{refusal: refusal}), detailsPanel(true))
}
