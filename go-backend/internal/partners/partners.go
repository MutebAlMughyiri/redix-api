// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package partners keeps a project's partners (UC-016) and their capital (UC-042 to UC-044, FR-25 to FR-27): added as tables of the project's
// capital item (redix-api #109), read by whoever may see them (BR-31), deleted softly at zero capital (redix-api #100) (PartnersEndpoints,
// CapitalEndpoints; redix-api #140).
package partners

import (
	"context"
	"errors"
	"net/http"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
)

// The entry statuses and kinds capital reads (EntryStatus, EntryKind).
const (
	draft, pendingApproval, approved, rejected int64 = 0, 1, 2, 3
	capitalOut                                 int64 = 3
)

// Routes keeps a project's partners.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the partners and their capital: reading them for whoever reads the project, changing them for whoever holds ManageSettings, in a
// project not finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const project = "/projects/{projectId:guid}"
	keeps := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", project+"/partners", gate.Needs(access.ReadProject, s.list))
	router.Handle("POST", project+"/partners", keeps(s.add))
	router.Handle("PUT", project+"/partners/{partnerId:guid}", keeps(s.edit))
	router.Handle("DELETE", project+"/partners/{partnerId:guid}", keeps(s.delete))
	router.Handle("GET", project+"/capital", gate.Needs(access.ReadProject, s.capital))
	router.Handle("GET", project+"/partners/{partnerId:guid}/movements", gate.Needs(access.ReadProject, s.movements))
}

// balances is each partner's capital from their approved movements (CapitalEndpoints.BalancesAsync, BR-05): in less out.
func balances(movements []store.CapitalMovementsRow) map[uuid.UUID]fixed.Total {
	held := map[uuid.UUID]fixed.Total{}
	for _, row := range movements {
		if row.Status != approved {
			continue
		}
		if row.Kind == capitalOut {
			held[row.PartnerID] = held[row.PartnerID].Minus(fixed.Counted(row.Amount))
		} else {
			held[row.PartnerID] = held[row.PartnerID].Plus(fixed.Counted(row.Amount))
		}
	}
	return held
}

// deletable is the kept partners the caller may delete (PartnersEndpoints.DeletableAsync): at zero capital, with nothing waiting or rejected naming
// them, and no draft of anyone else's - their own going with the partner, which only whoever deletes entries may do (findings rows 199, 202).
func deletable(partners []store.Partner, movements []store.CapitalMovementsRow, caller access.Caller, projectID uuid.UUID) map[uuid.UUID]bool {
	held := balances(movements)
	blocked := map[uuid.UUID]bool{}
	for _, row := range movements {
		othersDraft := row.Status == draft && (row.CreatedByUserID != caller.UserID || !caller.Access.Holds(projectID, access.DeleteEntries))
		if row.Status == pendingApproval || row.Status == rejected || othersDraft {
			blocked[row.PartnerID] = true
		}
	}
	may := map[uuid.UUID]bool{}
	for _, partner := range partners {
		may[partner.ID] = !partner.IsDeleted && held[partner.ID].Number() == 0 && !blocked[partner.ID]
	}
	return may
}

// partnerDraftView is part of what GET /projects/{projectId}/partners (UC-016) answers.
type partnerDraftView struct {
	EntryDate moment.Date  `json:"entryDate"`
	Amount    fixed.Number `json:"amount"`
}

// partnerView is part of what GET /projects/{projectId}/partners (UC-016) answers.
type partnerView struct {
	ID           uuid.UUID          `json:"id"`
	Name         string             `json:"name"`
	UserID       string             `json:"userId"`
	UserName     string             `json:"userName"`
	MayDelete    bool               `json:"mayDelete"`
	OthersDrafts []string           `json:"othersDrafts"`
	MyDrafts     []partnerDraftView `json:"myDrafts"`
}

// list is the project's partners the caller sees, in their tables' order, each with the name of their own login, whether it may be deleted now,
// whose drafts hold it - for whoever keeps the settings - and the caller's own drafts naming it; a deleted partner is not among them.
func (s Routes) list(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	partners, err := read.PartnersOf(r.Context(), projectID)
	movements, movementsErr := read.CapitalMovements(r.Context(), projectID)
	logins, loginsErr := read.PartnerLogins(r.Context(), projectID)
	names, namesErr := read.UserNames(r.Context())
	if err != nil || movementsErr != nil || loginsErr != nil || namesErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	nameOf := map[uuid.UUID]string{}
	for _, row := range names {
		nameOf[row.ID] = row.DisplayName
	}
	may := deletable(partners, movements, caller, projectID)
	views := []partnerView{}
	for _, partner := range partners {
		if partner.IsDeleted || !caller.Access.SeesCapitalOf(projectID, partner.ID) {
			continue
		}
		one := partnerView{ID: partner.ID, Name: partner.Name, MayDelete: may[partner.ID], OthersDrafts: []string{}, MyDrafts: []partnerDraftView{}}
		for _, login := range logins {
			if login.PartnerID == partner.ID {
				one.UserID, one.UserName = login.UserID.String(), nameOf[login.UserID]
			}
		}
		for _, row := range movements {
			if row.PartnerID != partner.ID || row.Status != draft {
				continue
			}
			if row.CreatedByUserID == caller.UserID {
				one.MyDrafts = append(one.MyDrafts, partnerDraftView{EntryDate: row.EntryDate, Amount: row.Amount})
			} else if author := nameOf[row.CreatedByUserID]; caller.Access.Holds(projectID, access.ManageSettings) && !slices.Contains(one.OthersDrafts, author) {
				one.OthersDrafts = append(one.OthersDrafts, author)
			}
		}
		views = append(views, one)
	}
	web.JSON(w, r, http.StatusOK, views)
}

type partnerRequest struct {
	Name   string    `json:"name"`
	UserID uuid.UUID `json:"userId,omitzero"`
}

func fields(partner store.Partner) map[string]any {
	return map[string]any{"ProjectId": partner.ProjectID, "Name": partner.Name, "SortOrder": partner.SortOrder, "IsDeleted": partner.IsDeleted}
}

// refusalOfLogin is why userID cannot be partnerID's own login (LoginRefusalAsync): unknownUser for someone not in the project, userTaken for
// another partner's login; "" for none.
func refusalOfLogin(c changes.Change, partnerID, userID uuid.UUID) (string, error) {
	if userID == uuid.Nil {
		return "", nil
	}
	member, err := c.Q.IsMember(c.Ctx, store.IsMemberParams{ProjectID: c.ProjectID, UserID: userID})
	if err != nil || !member {
		return "unknownUser", err
	}
	logins, err := c.Q.PartnerLogins(c.Ctx, c.ProjectID)
	if slices.ContainsFunc(logins, func(login store.PartnerUser) bool { return login.UserID == userID && login.PartnerID != partnerID }) {
		return "userTaken", err
	}
	return "", err
}

// add adds a partner, with their own login if they have one, as the last table of the project's capital item (redix-api #109): invalidName,
// itemNotOffered for a capital item disabled or gone, unknownUser, userTaken (400).
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed partnerRequest
	if !web.Read(w, r, &typed) {
		return
	}
	name, ok := naming.Named(typed.Name)
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, naming.Invalid)
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		_, err := Add(c, name, typed.UserID)
		return err
	})
}

// Add adds the partner named name, with login as their own login unless it is uuid.Nil, as the last table of the change's project's capital item
// (PartnersEndpoints.AddAsync): itemNotOffered, unknownUser, userTaken (400). The one way a partner is added, by hand or by a project file's load.
func Add(c changes.Change, name string, login uuid.UUID) (uuid.UUID, error) {
	items, err := c.Q.ItemsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return uuid.Nil, err
	}
	capitalItem := uuid.Nil
	for _, item := range items {
		if item.Type == int64(structure.Capital) {
			capitalItem = item.ID
		}
	}
	offered, err := structure.Offers(c.Ctx, c.Q, c.ProjectID, capitalItem, structure.Capital)
	if err != nil || !offered {
		return uuid.Nil, orRefused(err, "itemNotOffered")
	}
	if code, err := refusalOfLogin(c, uuid.Nil, login); err != nil || code != "" {
		return uuid.Nil, orRefused(err, code)
	}
	place, err := structure.NextTablePlace(c.Ctx, c.Q, capitalItem)
	if err != nil {
		return uuid.Nil, err
	}
	partner := store.Partner{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Name: name, SortOrder: place}
	if err := c.Q.SavePartner(c.Ctx, store.SavePartnerParams(partner)); err != nil {
		return uuid.Nil, err
	}
	if err := c.Record(history.Create, "Partners", partner.ID, nil, fields(partner)); err != nil {
		return uuid.Nil, err
	}
	if login == uuid.Nil {
		return partner.ID, nil
	}
	if err := c.Q.SavePartnerLogin(c.Ctx, store.SavePartnerLoginParams{PartnerID: partner.ID, ProjectID: c.ProjectID, UserID: login}); err != nil {
		return uuid.Nil, err
	}
	return partner.ID, c.Record(history.Create, "PartnerUsers", partner.ID, nil, map[string]any{"ProjectId": c.ProjectID, "UserId": login})
}

func orRefused(err error, code string) error {
	if err != nil {
		return err
	}
	return changes.Refuse(http.StatusBadRequest, code)
}

// kept is the route's partner while not deleted: notFound otherwise.
func kept(c changes.Change, r *http.Request) (store.Partner, error) {
	partners, err := c.Q.PartnersOf(c.Ctx, c.ProjectID)
	if err != nil {
		return store.Partner{}, err
	}
	at := slices.IndexFunc(partners, func(partner store.Partner) bool { return partner.ID == web.ID(r, "partnerId") && !partner.IsDeleted })
	if at < 0 {
		return store.Partner{}, changes.NotFound
	}
	return partners[at], nil
}

// edit renames a partner and sets their own login, answering as add does; each change in History as .NET's AddHistoryOfChanges wrote it.
func (s Routes) edit(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var typed partnerRequest
	if !web.Read(w, r, &typed) {
		return
	}
	name, ok := naming.Named(typed.Name)
	if !ok {
		problem.Refuse(w, r, http.StatusBadRequest, naming.Invalid)
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		before, err := kept(c, r)
		if err != nil {
			return err
		}
		if code, err := refusalOfLogin(c, before.ID, typed.UserID); err != nil || code != "" {
			return orRefused(err, code)
		}
		after := before
		after.Name = name
		if err := c.Save("Partners", after.ID, false, fields(before), fields(after), func() error { return c.Q.SavePartner(c.Ctx, store.SavePartnerParams(after)) }); err != nil {
			return err
		}
		return linkTo(c, after.ID, typed.UserID)
	})
}

// linkTo makes userID the partner's own login, uuid.Nil none (Partner.LinkTo), its line an update whatever became of its row.
func linkTo(c changes.Change, partnerID, userID uuid.UUID) error {
	logins, err := c.Q.PartnerLogins(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	at := slices.IndexFunc(logins, func(login store.PartnerUser) bool { return login.PartnerID == partnerID })
	switch {
	case at < 0 && userID == uuid.Nil, at >= 0 && logins[at].UserID == userID:
		return nil
	case userID == uuid.Nil:
		if err := c.Q.RemovePartnerLogin(c.Ctx, partnerID); err != nil {
			return err
		}
		return c.Record(history.Update, "PartnerUsers", partnerID, map[string]any{"ProjectId": c.ProjectID, "UserId": logins[at].UserID}, nil)
	}
	if err := c.Q.SavePartnerLogin(c.Ctx, store.SavePartnerLoginParams{PartnerID: partnerID, ProjectID: c.ProjectID, UserID: userID}); err != nil {
		return err
	}
	if at < 0 {
		return c.Record(history.Update, "PartnerUsers", partnerID, nil, map[string]any{"ProjectId": c.ProjectID, "UserId": userID})
	}
	return c.Record(history.Update, "PartnerUsers", partnerID, map[string]any{"UserId": logins[at].UserID}, map[string]any{"UserId": userID})
}

// delete deletes a partner softly (redix-api #100): they leave every place a new movement starts, their movements staying in the books and their
// login theirs no more; only as deletable says, otherwise partnerNotDeletable (409). The caller's own drafts naming them go with them.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		partner, err := kept(c, r)
		if err != nil {
			return err
		}
		partners, err := c.Q.PartnersOf(c.Ctx, c.ProjectID)
		if err != nil {
			return err
		}
		movements, err := c.Q.CapitalMovements(c.Ctx, c.ProjectID)
		if err != nil {
			return err
		}
		if !deletable(partners, movements, caller, c.ProjectID)[partner.ID] {
			return changes.Refuse(http.StatusConflict, "partnerNotDeletable")
		}
		for _, row := range movements {
			if row.PartnerID == partner.ID && row.Status == draft {
				entry := store.Entry{ID: row.ID, ProjectID: row.ProjectID, Kind: row.Kind, Status: row.Status, EntryDate: row.EntryDate, Amount: row.Amount,
					AccountID: row.AccountID, Description: row.Description, CreatedByUserID: row.CreatedByUserID, CreatedAt: row.CreatedAt, ApprovedAt: row.ApprovedAt,
					ColumnValues: row.ColumnValues, Number: row.Number}
				if err := entries.Delete(c, entry); err != nil {
					return err
				}
			}
		}
		after := partner
		after.IsDeleted = true
		if err := c.Q.SavePartner(c.Ctx, store.SavePartnerParams(after)); err != nil {
			return err
		}
		if err := c.Q.RemovePartnerLogin(c.Ctx, partner.ID); err != nil {
			return err
		}
		return c.Record(history.Delete, "Partners", partner.ID, map[string]any{"IsDeleted": false}, map[string]any{"IsDeleted": true})
	})
}

// CapitalLine is one partner's capital and share of the whole (CapitalLineView).
type CapitalLine struct {
	ID           uuid.UUID     `json:"id"`
	Name         string        `json:"name"`
	Balance      fixed.Total   `json:"balance"`
	SharePercent fixed.Written `json:"sharePercent"`
}

// Capital is the project's whole capital and each partner's balance and share (BR-05, BR-28), for the partners shows keeps, the largest balance
// first (CapitalEndpoints.ViewOfAsync): the one owner of the capital screen and the periodic report's partners.
func Capital(ctx context.Context, q *store.Queries, projectID uuid.UUID, shows func(uuid.UUID) bool) (fixed.Total, []CapitalLine, error) {
	partners, err := q.PartnersOf(ctx, projectID)
	movements, movementsErr := q.CapitalMovements(ctx, projectID)
	if err := errors.Join(err, movementsErr); err != nil {
		return fixed.Total{}, nil, err
	}
	held := balances(movements)
	total := fixed.Total{}
	for _, balance := range held {
		total = total.Plus(balance)
	}
	lines := []CapitalLine{}
	for _, partner := range partners {
		if !partner.IsDeleted && shows(partner.ID) {
			lines = append(lines, CapitalLine{ID: partner.ID, Name: partner.Name, Balance: held[partner.ID], SharePercent: fixed.Percent(held[partner.ID].Number(), total.Number(), 10, true)})
		}
	}
	slices.SortStableFunc(lines, func(a, b CapitalLine) int {
		if a.Balance.Number() != b.Balance.Number() {
			return int(b.Balance.Number() - a.Balance.Number())
		}
		return strings.Compare(a.ID.String(), b.ID.String())
	})
	return total, lines, nil
}

// capitalView is what GET /projects/{projectId}/capital answers (UC-040).
type capitalView struct {
	Total    fixed.Total   `json:"total"`
	Partners []CapitalLine `json:"partners"`
}

// capital is the capital screen: everyone who reads the project sees the total, each line whoever sees that partner (BR-31).
func (s Routes) capital(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	projectID := web.ID(r, "projectId")
	total, lines, err := Capital(r.Context(), s.database.Queries(), projectID, func(partnerID uuid.UUID) bool { return caller.Access.SeesCapitalOf(projectID, partnerID) })
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, capitalView{total, lines})
}

// partnerMovementView is part of what GET /projects/{projectId}/partners/{partnerId}/movements answers (UC-040).
type partnerMovementView struct {
	ID           uuid.UUID   `json:"id"`
	MovementName string      `json:"movementName"`
	Date         moment.Date `json:"date"`
	Amount       fixed.Total `json:"amount"`
	Description  string      `json:"description"`
	AccountName  string      `json:"accountName"`
	AttachmentID string      `json:"attachmentId"`
	ContentType  string      `json:"contentType"`
	FileName     string      `json:"fileName"`
}

// movements is one partner's approved movements, newest first, for whoever may see their capital (BR-31): 404 for anyone else, so a partner's
// existence does not show.
func (s Routes) movements(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	read, projectID, partnerID := s.database.Queries(), web.ID(r, "projectId"), web.ID(r, "partnerId")
	partners, err := read.PartnersOf(r.Context(), projectID)
	if err == nil && (!caller.Access.SeesCapitalOf(projectID, partnerID) || !slices.ContainsFunc(partners, func(partner store.Partner) bool { return partner.ID == partnerID })) {
		err = changes.NotFound
	}
	if changes.Answered(w, r, err) {
		return
	}
	movements, err := read.CapitalMovements(r.Context(), projectID)
	accounts, accountsErr := read.AccountsOf(r.Context(), projectID)
	files, filesErr := read.EntryFilesOf(r.Context(), projectID)
	items, itemsErr := read.ItemsOf(r.Context(), projectID)
	columns, columnsErr := read.ColumnsOf(r.Context(), projectID)
	if err != nil || accountsErr != nil || filesErr != nil || itemsErr != nil || columnsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	accountName := map[uuid.UUID]string{}
	for _, account := range accounts {
		accountName[account.ID] = account.Name
	}
	var capitalColumns []store.ItemColumn
	for _, item := range items {
		for _, column := range columns {
			if item.Type == int64(structure.Capital) && column.ItemID == item.ID && !column.IsHidden {
				capitalColumns = append(capitalColumns, column)
			}
		}
	}
	views := []partnerMovementView{}
	for _, row := range movements {
		if row.PartnerID != partnerID || row.Status != approved {
			continue
		}
		movementName, err := structure.ChoiceNameIn(capitalColumns, row.ColumnValues)
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		one := partnerMovementView{ID: row.ID, MovementName: movementName, Date: row.EntryDate, Amount: fixed.Counted(row.Amount),
			Description: row.Description, AccountName: accountName[row.AccountID]}
		if row.Kind == capitalOut {
			one.Amount = fixed.Total{}.Minus(fixed.Counted(row.Amount))
		}
		for _, file := range files {
			if file.EntryID == row.ID {
				one.AttachmentID, one.ContentType, one.FileName = file.AttachmentID.String(), file.ContentType, file.FileName
				break
			}
		}
		views = append(views, one)
	}
	web.JSON(w, r, http.StatusOK, views)
}
