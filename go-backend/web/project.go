// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
)

// mayOpen says whether a person opens project: approved, and they are not inactive in it, as the app's project list lets them (Project.mayOpen).
func mayOpen(project projects.View) bool {
	return (project.Status == "active" || project.Status == "finished") && !project.IsInactive
}

// projectOf is the project p's caller works in: the one they chose here, else their default project, else none while they choose one, as the
// app lands on its project list (AC-37.1, redix-api #165).
func (p page) projectOf() projects.View {
	if chosen, found := p.openable(p.view.Project); found {
		return chosen
	}
	project, _ := p.openable(p.defaultProject)
	return project
}

// openable is the project of p's list with id, when p's caller may open it.
func (p page) openable(id uuid.UUID) (projects.View, bool) {
	for _, project := range p.projects {
		if project.ID == id && mayOpen(project) {
			return project, true
		}
	}
	return projects.View{}, false
}

// inProject says whether p's caller works in a project, or still chooses one.
func (p page) inProject() bool { return p.project.ID != uuid.Nil }

// may says whether p's caller holds permission in the project they work in, as the app's buttons ask it.
func (p page) may(permission access.Permission) bool {
	return p.caller.Access.Holds(p.project.ID, permission)
}

// statusWord is the word of a project's status, as the app's project list chips it.
func statusWord(status string) string {
	if status == "pendingApproval" {
		return "projects.pending"
	}
	return "projects." + status
}

// starProject makes the project the path names the one the person's sign-in opens straight into, or clears it when it already is, as the app's
// star does (AC-37.1, redix-api #165); the list answers in place, still open, saying so: 404 for a project they may not open.
func (s site) starProject(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	caller, signedIn, err := s.session(w, r)
	if err != nil {
		failed(w)
		return
	}
	if !signedIn {
		p.content = signInForm(p, ended(r), "")
		reshell(w, r, p)
		return
	}
	if p, err = s.open(r, p, caller); err != nil {
		failed(w)
		return
	}
	starred, _ := uuid.Parse(r.PathValue("id"))
	project, found := p.openable(starred)
	if !found {
		http.NotFound(w, r)
		return
	}
	notice := ""
	if p.defaultProject == starred {
		err = projects.RemoveDefault(r.Context(), s.Database, caller.UserID)
		p.defaultProject = uuid.Nil
	} else {
		err = projects.SetDefault(r.Context(), s.Database, caller.UserID, starred)
		p.defaultProject, notice = starred, p.words.Of("projects.defaultSet", project.Name)
	}
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, projectMenu(p, notice))
}

// chooseProject makes the project the path names the one the person works in, and opens its home in the whole shell, as choosing a project on the
// phone opens its home (redix-api #165): 404 for a project they may not open.
func (s site) chooseProject(w http.ResponseWriter, r *http.Request) {
	p, err := s.page(r)
	if err != nil {
		failed(w)
		return
	}
	caller, signedIn, err := s.session(w, r)
	if err != nil {
		failed(w)
		return
	}
	if !signedIn {
		p.content = signInForm(p, ended(r), "")
		reshell(w, r, p)
		return
	}
	chosen, _ := uuid.Parse(r.PathValue("id"))
	p.view.Project, p.view.Tabs, p.view.Active = chosen, []string{"home"}, "home"
	if p, err = s.open(r, p, caller); err != nil {
		failed(w)
		return
	}
	if p.project.ID != chosen || chosen == uuid.Nil {
		http.NotFound(w, r)
		return
	}
	p.view.keepProject(w)
	p.view.keepTabs(w)
	reshell(w, r, p)
}
