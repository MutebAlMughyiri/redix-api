// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net/http"
	"slices"
	"strconv"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// mayManageProject says whether p's caller may change the project's details: ManageProject, in a project not finished, as the app's
// mayManageProject (AC-04.1).
func (p page) mayManageProject() bool { return !p.finished() && p.may(access.ManageProject) }

// utcOffset is a time zone's distance from UTC as the app's utcOffsetText writes it: UTC+3, UTC−3:30.
func utcOffset(minutes int) string {
	sign := "+"
	if minutes < 0 {
		sign, minutes = "−", -minutes
	}
	text := "UTC" + sign + strconv.Itoa(minutes/60)
	if rest := minutes % 60; rest != 0 {
		text += ":" + strconv.Itoa(rest/10) + strconv.Itoa(rest%10)
	}
	return text
}

// zoneWord is the word of the time zone named name, as the app's city names it; the name itself for one REDIX does not offer.
func zoneWord(p page, name string) string {
	at := slices.IndexFunc(zones.All, func(zone zones.Zone) bool { return zone.Name == name })
	if at < 0 {
		return name
	}
	return p.words.Of(zones.All[at].WordKey)
}

// unitOf is the project's unit with unitID.
func unitOf(project projects.View, unitID uuid.UUID) (projects.Unit, bool) {
	at := slices.IndexFunc(project.Units, func(unit projects.Unit) bool { return unit.ID == unitID })
	if at < 0 {
		return projects.Unit{}, false
	}
	return project.Units[at], true
}

// projectInfoTab is More info in its tab (UC-006, redix-api #175), for everyone in the project: what the app's screen shows, changed by whoever
// manages it.
func (s site) projectInfoTab(_ *http.Request, p page) (templ.Component, error) {
	return projectInfoPage(p, said{}), nil
}

// projectInfoDetails is one of More info's forms in the details panel, as the app's dialogs ask them: Rename (rename), the time zones
// (timeZone), Add unit (unit), a unit's Rename (unit and its id) and Delete asked first (deleteUnit), and Close the project asked first (close).
// 404 without ManageProject, or for a form or a unit there is none of.
func (s site) projectInfoDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	asking := r.PathValue("asking")
	unitID, _ := uuid.Parse(r.URL.Query().Get("unit"))
	unit, unitFound := unitOf(p.project, unitID)
	known := slices.Contains([]string{"rename", "timeZone", "unit", "close"}, asking) || (asking == "deleteUnit" && unitFound)
	if !p.inProject() || !p.may(access.ManageProject) || !known || (r.URL.Query().Get("unit") != "" && !unitFound) {
		http.NotFound(w, r)
		return
	}
	draw(w, r, http.StatusOK, projectInfoForm(p, asking, unit, "", "", s.Clock.Now()))
}

// keepProjectInfo keeps what one of More info's forms asks, through the rule the app's route runs: the name (rename), the time zone (timeZone),
// a unit added (unit), renamed (unit with its id) or deleted (deleteUnit), the description (describe), and the project closed (close). Kept, the
// details panel closes and More info is drawn again beside it with the project's name wherever the shell shows it; the description, kept from
// More info itself, answers it in place saying it was saved, as the app does, or why not. 403 without ManageProject; 404 for a form or a unit
// there is none of.
func (s site) keepProjectInfo(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	asking := r.PathValue("asking")
	unitID, _ := uuid.Parse(r.PostFormValue("unit"))
	unit, unitFound := unitOf(p.project, unitID)
	known := slices.Contains([]string{"rename", "timeZone", "unit", "describe", "close"}, asking) || (asking == "deleteUnit" && unitFound)
	if !p.inProject() || r.ParseForm() != nil || !known || (r.PostFormValue("unit") != "" && !unitFound) {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageProject) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	typed := r.PostFormValue("value")
	err := changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
		switch {
		case asking == "rename":
			return projects.RenameProject(c, typed)
		case asking == "timeZone":
			return projects.SetTimeZone(c, typed)
		case asking == "unit" && unitFound:
			return projects.RenameUnit(c, unit.ID, typed)
		case asking == "unit":
			return projects.AddUnit(c, typed)
		case asking == "deleteUnit":
			return projects.DeleteUnit(c, unit.ID)
		case asking == "describe":
			return projects.DescribeProject(c, typed)
		}
		return projects.CloseProject(c)
	})
	refusal, err := refusalOf("projects", err)
	if err != nil {
		failed(w)
		return
	}
	if refusal != "" && asking != "describe" {
		draw(w, r, http.StatusOK, projectInfoForm(p, asking, unit, typed, refusal, s.Clock.Now()))
		return
	}
	if p, err = s.open(r, p, p.caller); err != nil {
		failed(w)
		return
	}
	if asking == "describe" {
		saying := said{refusal: refusal}
		if refusal == "" {
			saying.notice = "projectInfo.saved"
		}
		draw(w, r, http.StatusOK, projectInfoPage(p, saying))
		return
	}
	draw(w, r, http.StatusOK, projectInfoBeside(p), detailsPanel(true))
}

// startLine is the line under the project's name, as the app's says it: its start date, and that it reads only once finished.
func startLine(p page) string {
	line := p.words.Of("projects.startDate") + " " + p.project.StartDate.Format("2006-01-02")
	if p.finished() {
		line += " · " + p.words.Of("projects.readOnly")
	}
	return line
}

// projectInfoTitle, projectInfoLabel and projectInfoSave are the words of More info's forms, as the app's dialogs say them.
func projectInfoTitle(asking string, unit projects.Unit) string {
	switch {
	case asking == "unit" && unit.Name != "":
		return "projects.renameUnit"
	case asking == "unit":
		return "projects.addUnit"
	case asking == "timeZone":
		return "projects.timeZone"
	case asking == "deleteUnit":
		return "swipe.remove"
	case asking == "close":
		return "projects.close"
	}
	return "projects.rename"
}

func projectInfoLabel(asking string) string {
	if asking == "unit" {
		return "projects.unitName"
	}
	return "projects.name"
}

func projectInfoSave(asking string, unit projects.Unit) string {
	switch {
	case asking == "unit" && unit.Name == "":
		return "projects.addUnit"
	case asking == "deleteUnit":
		return "swipe.remove"
	case asking == "close":
		return "projects.close"
	}
	return "projects.save"
}

// typedOr is what a name field of More info's forms holds: what was typed, else the name it renames.
func typedOr(typed, asking string, unit projects.Unit, p page) string {
	switch {
	case typed != "":
		return typed
	case asking == "rename":
		return p.project.Name
	}
	return unit.Name
}
