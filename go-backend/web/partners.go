// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"errors"
	"net/http"
	"strconv"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/partners"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

// partnersShown is what Partners shows: the project's capital, each partner the caller sees with their balance and share, and the partners as
// the list knows them - their logins.
type partnersShown struct {
	total    fixed.Total
	lines    []partners.CapitalLine
	partners []partners.View
}

// login is the name of partner's own login, as the app's row says it; "" for one with none.
func (s partnersShown) login(partnerID uuid.UUID) string {
	for _, partner := range s.partners {
		if partner.ID == partnerID {
			return partner.UserName
		}
	}
	return ""
}

// lineOf is the capital line of the partner partnerID, and whether the caller sees one.
func (s partnersShown) lineOf(partnerID uuid.UUID) (partners.CapitalLine, bool) {
	for _, line := range s.lines {
		if line.ID == partnerID {
			return line, true
		}
	}
	return partners.CapitalLine{}, false
}

// partnersOf is Partners for p's caller, through the rules the app's routes answer (partners.Capital, partners.ListOf): the capital everyone who
// reads the project sees, each line whoever sees that partner (BR-31). The error is the database's.
func (s site) partnersOf(ctx context.Context, p page) (partnersShown, error) {
	read := s.Database.Queries()
	total, lines, err := partners.Capital(ctx, read, p.project.ID, func(partnerID uuid.UUID) bool { return p.caller.Access.SeesCapitalOf(p.project.ID, partnerID) })
	listed, listErr := partners.ListOf(ctx, read, p.project.ID, p.caller)
	return partnersShown{total: total, lines: lines, partners: listed}, errors.Join(err, listErr)
}

// partnersTab is Partners in its tab (UC-016, UC-040, redix-api #169): the capital total, then each partner the caller sees, the largest
// balance first, with their login, share and balance, each opening in the details panel.
func (s site) partnersTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.partnersOf(r.Context(), p)
	return partnersList(p, shown), err
}

// partnerDetails is a partner in the details panel: their balance and share, and their approved movements, newest first, through the rule the
// app's route answers (partners.MovementsOf). 404 for a partner the caller may not see, or none.
func (s site) partnerDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	partnerID, _ := uuid.Parse(r.PathValue("id"))
	shown, err := s.partnersOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	line, seen := shown.lineOf(partnerID)
	if !seen {
		http.NotFound(w, r)
		return
	}
	if r.URL.Query().Get("asking") == "edit" && p.mayKeepPartners() {
		partner, _ := shown.partnerOf(partnerID)
		login, _ := uuid.Parse(partner.UserID)
		s.partnerFormOf(w, r, p, partner, partner.Name, login, "")
		return
	}
	movements, err := partners.MovementsOf(r.Context(), s.Database.Queries(), p.project.ID, partnerID, p.caller)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, partnerPanel(p, shown, line, movements, said{asking: r.URL.Query().Get("asking")}))
}

// shareOf is a partner's share as the app's row writes it (formatShare): ten decimals, and the percent sign.
func shareOf(line partners.CapitalLine) string {
	share, _ := strconv.ParseFloat(string(line.SharePercent), 64)
	return strconv.FormatFloat(share, 'f', 10, 64) + "%"
}

// partnerOf is the partner partnerID as the list knows them - their login, whether they may be deleted, whose drafts hold them -, and whether
// the caller sees one.
func (s partnersShown) partnerOf(partnerID uuid.UUID) (partners.View, bool) {
	for _, partner := range s.partners {
		if partner.ID == partnerID {
			return partner, true
		}
	}
	return partners.View{}, false
}

// mayKeepPartners says whether p's caller may change the project's partners: ManageSettings, in a project not finished.
func (p page) mayKeepPartners() bool { return !p.finished() && p.may(access.ManageSettings) }

// partnerFormOf answers the details panel with New partner's form - or Edit's, for editing - as typed, the project's members to give them as their
// own login, and why it could not be kept.
func (s site) partnerFormOf(w http.ResponseWriter, r *http.Request, p page, editing partners.View, name string, login uuid.UUID, refusal string) {
	members, err := s.Database.Queries().Members(r.Context(), p.project.ID)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, partnerForm(p, editing, name, login, members, refusal))
}

// newPartnerForm is New partner in the details panel, for whoever manages the project's settings (UC-016, redix-api #169).
func (s site) newPartnerForm(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.mayKeepPartners() {
		http.NotFound(w, r)
		return
	}
	s.partnerFormOf(w, r, p, partners.View{}, "", uuid.Nil, "")
}

// keepPartner keeps the form posted, a new partner through partners.Create or the partner the path names through partners.Edit - the rules the
// app's routes run - or deletes that partner through partners.Delete, in one change: the panel shows the partner, or that they were deleted, and
// Partners beside it follows; refused, the panel says why in the app's words. 403 without ManageSettings; a finished project changes nothing
// (AC-04.1).
func (s site) keepPartner(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageSettings) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.partnersOf(r.Context(), p)
	if err != nil {
		failed(w)
		return
	}
	name, editing := r.PostFormValue("name"), partners.View{}
	login, _ := uuid.Parse(r.PostFormValue("login"))
	if action := r.PathValue("action"); action != "" && action != "edit" && action != "delete" {
		http.NotFound(w, r)
		return
	}
	if r.PathValue("id") != "" {
		partnerID, _ := uuid.Parse(r.PathValue("id"))
		var found bool
		if editing, found = shown.partnerOf(partnerID); !found {
			http.NotFound(w, r)
			return
		}
	}
	deleting := r.PathValue("action") == "delete"
	refusal := ""
	if p.finished() {
		refusal = "projects.projectFinished"
	} else {
		partnerID := editing.ID
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			switch {
			case deleting:
				return partners.Delete(c, p.caller, editing.ID)
			case editing.ID != uuid.Nil:
				return partners.Edit(c, editing.ID, name, login)
			}
			var err error
			partnerID, err = partners.Create(c, name, login)
			return err
		})
		var refused changes.Refusal
		switch {
		case errors.As(err, &refused):
			refusal = "partners." + refused.Code
		case err != nil:
			failed(w)
			return
		}
		editing.ID = partnerID
	}
	if refusal != "" && !deleting {
		s.partnerFormOf(w, r, p, editing, name, login, refusal)
		return
	}
	if shown, err = s.partnersOf(r.Context(), p); err != nil {
		failed(w)
		return
	}
	line, seen := shown.lineOf(editing.ID)
	if !seen {
		draw(w, r, http.StatusOK, partnerGone(p), partnersBeside(p, shown))
		return
	}
	movements, err := partners.MovementsOf(r.Context(), s.Database.Queries(), p.project.ID, line.ID, p.caller)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, partnerPanel(p, shown, line, movements, said{refusal: refusal}), partnersBeside(p, shown))
}
