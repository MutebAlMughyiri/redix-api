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
	movements, err := partners.MovementsOf(r.Context(), s.Database.Queries(), p.project.ID, partnerID, p.caller)
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, partnerPanel(p, shown, line, movements))
}

// shareOf is a partner's share as the app's row writes it (formatShare): ten decimals, and the percent sign.
func shareOf(line partners.CapitalLine) string {
	share, _ := strconv.ParseFloat(string(line.SharePercent), 64)
	return strconv.FormatFloat(share, 'f', 10, 64) + "%"
}
