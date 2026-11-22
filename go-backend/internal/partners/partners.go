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
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/ledger"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
	"github.com/google/uuid"
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
		if row.Status != ledger.Approved {
			continue
		}
		held[row.PartnerID] = held[row.PartnerID].Plus(ledger.Signed(row.Kind, fixed.Counted(row.Amount)))
	}
	return held
}

// deletable is the kept partners the caller may delete (PartnersEndpoints.DeletableAsync): at zero capital, with nothing waiting or rejected naming
// them, and no draft of anyone else's - their own going with the partner, which only whoever deletes entries may do (findings rows 199, 202).
func deletable(partners []store.Partner, movements []store.CapitalMovementsRow, caller access.Caller, projectID uuid.UUID) map[uuid.UUID]bool {
	held := balances(movements)
	blocked := map[uuid.UUID]bool{}
	for _, row := range movements {
		othersDraft := row.Status == ledger.Draft && (row.CreatedByUserID != caller.UserID || !caller.Access.Holds(projectID, access.DeleteEntries))
		if row.Status == ledger.PendingApproval || row.Status == ledger.Rejected || othersDraft {
			blocked[row.PartnerID] = true
		}
	}
	may := map[uuid.UUID]bool{}
	for _, partner := range partners {
		may[partner.ID] = !partner.IsDeleted && held[partner.ID].Number() == 0 && !blocked[partner.ID]
	}
	return may
}

// Draft is part of what GET /projects/{projectId}/partners (UC-016) answers.
type Draft struct {
	EntryDate moment.Date  `json:"entryDate"`
	Amount    fixed.Number `json:"amount"`
}

// View is a partner as GET /projects/{projectId}/partners (UC-016) answers them, and as the web's Partners lists them (redix-api #169).
type View struct {
	ID           uuid.UUID `json:"id"`
	Name         string    `json:"name"`
	UserID       string    `json:"userId"`
	UserName     string    `json:"userName"`
	MayDelete    bool      `json:"mayDelete"`
	OthersDrafts []string  `json:"othersDrafts"`
	MyDrafts     []Draft   `json:"myDrafts"`
}

// list is the project's partners the caller sees, in their tables' order, each with the name of their own login, whether it may be deleted now,
// whose drafts hold it - for whoever keeps the settings - and the caller's own drafts naming it; a deleted partner is not among them.
func (s Routes) list(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	views, err := ListOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), caller)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// ListOf is projectID's partners caller sees, as list answers them: the rule both doors call, the app's route and the web's Partners (redix-api
// #169). The error is the database's.
func ListOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, caller access.Caller) ([]View, error) {
	partners, err := read.PartnersOf(ctx, projectID)
	movements, movementsErr := read.CapitalMovements(ctx, projectID)
	logins, loginsErr := read.PartnerLogins(ctx, projectID)
	names, namesErr := read.UserNames(ctx)
	if err := errors.Join(err, movementsErr, loginsErr, namesErr); err != nil {
		return nil, err
	}
	nameOf := map[uuid.UUID]string{}
	for _, row := range names {
		nameOf[row.ID] = row.DisplayName
	}
	may := deletable(partners, movements, caller, projectID)
	var views []View
	for _, partner := range partners {
		if partner.IsDeleted || !caller.Access.SeesCapitalOf(projectID, partner.ID) {
			continue
		}
		one := View{ID: partner.ID, Name: partner.Name, MayDelete: may[partner.ID]}
		for _, login := range logins {
			if login.PartnerID == partner.ID {
				one.UserID, one.UserName = login.UserID.String(), nameOf[login.UserID]
			}
		}
		for _, row := range movements {
			if row.PartnerID != partner.ID || row.Status != ledger.Draft {
				continue
			}
			if row.CreatedByUserID == caller.UserID {
				one.MyDrafts = append(one.MyDrafts, Draft{EntryDate: row.EntryDate, Amount: row.Amount})
			} else if author := nameOf[row.CreatedByUserID]; caller.Access.Holds(projectID, access.ManageSettings) && !slices.Contains(one.OthersDrafts, author) {
				one.OthersDrafts = append(one.OthersDrafts, author)
			}
		}
		views = append(views, one)
	}
	return views, nil
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
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		_, err := Create(c, typed.Name, typed.UserID)
		return err
	})
}

// Create adds the partner typed as name, with login as their own login unless it is uuid.Nil, as add answers it: invalidName for a name not 1 to
// 100 letters, then the refusals of Add. The rule both doors call, the app's route and the web's New partner (redix-api #169).
func Create(c changes.Change, name string, login uuid.UUID) (uuid.UUID, error) {
	named, ok := naming.Named(name)
	if !ok {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return Add(c, named, login)
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
	partnerID, err := Declare(c, name, place)
	if err != nil || login == uuid.Nil {
		return partnerID, err
	}
	if err := c.Q.SavePartnerLogin(c.Ctx, store.SavePartnerLoginParams{PartnerID: partnerID, ProjectID: c.ProjectID, UserID: login}); err != nil {
		return uuid.Nil, err
	}
	return partnerID, c.Record(history.Create, "PartnerUsers", partnerID, nil, map[string]any{"ProjectId": c.ProjectID, "UserId": login})
}

// Declare keeps the partner named name at place among the change's project's capital tables, with its Create line: the one way a partner is
// saved new, which Add calls and a registration calls for each partner it declares, so the partner history shows when every partner was added
// (findings row 294).
func Declare(c changes.Change, name string, place int64) (uuid.UUID, error) {
	partner := store.Partner{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Name: name, SortOrder: place}
	if err := c.Q.SavePartner(c.Ctx, store.SavePartnerParams(partner)); err != nil {
		return uuid.Nil, err
	}
	return partner.ID, c.Record(history.Create, "Partners", partner.ID, nil, fields(partner))
}

func orRefused(err error, code string) error {
	if err != nil {
		return err
	}
	return changes.Refuse(http.StatusBadRequest, code)
}

// keptOf is the partner partnerID of c's project while not deleted: changes.NotFound otherwise.
func keptOf(c changes.Change, partnerID uuid.UUID) (store.Partner, error) {
	partners, err := c.Q.PartnersOf(c.Ctx, c.ProjectID)
	if err != nil {
		return store.Partner{}, err
	}
	at := slices.IndexFunc(partners, func(partner store.Partner) bool { return partner.ID == partnerID && !partner.IsDeleted })
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
	partnerID := web.ID(r, "partnerId")
	s.writer.Done(w, r, caller, func(c changes.Change) error { return Edit(c, partnerID, typed.Name, typed.UserID) })
}

// Edit names the partner partnerID of c's project name and makes login their own login, uuid.Nil none, as edit answers it: invalidName,
// changes.NotFound for a partner not kept, then the refusals of a login. The rule both doors call, the app's route and the web's Edit (redix-api
// #169).
func Edit(c changes.Change, partnerID uuid.UUID, name string, login uuid.UUID) error {
	named, ok := naming.Named(name)
	if !ok {
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	before, err := keptOf(c, partnerID)
	if err != nil {
		return err
	}
	if code, err := refusalOfLogin(c, before.ID, login); err != nil || code != "" {
		return orRefused(err, code)
	}
	after := before
	after.Name = named
	if err := c.Save("Partners", after.ID, false, fields(before), fields(after), func() error { return c.Q.SavePartner(c.Ctx, store.SavePartnerParams(after)) }); err != nil {
		return err
	}
	return linkTo(c, after.ID, login)
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
	partnerID := web.ID(r, "partnerId")
	s.writer.Done(w, r, caller, func(c changes.Change) error { return Delete(c, caller, partnerID) })
}

// Delete deletes the partner partnerID of c's project softly as caller, as delete answers it (redix-api #100): changes.NotFound for a partner not
// kept, partnerNotDeletable (409) unless deletable says so. The rule both doors call, the app's route and the web's Delete (redix-api #169).
func Delete(c changes.Change, caller access.Caller, partnerID uuid.UUID) error {
	partner, err := keptOf(c, partnerID)
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
		if row.PartnerID == partner.ID && row.Status == ledger.Draft {
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
	var lines []CapitalLine
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

// Movement is one of a partner's approved movements, as GET /projects/{projectId}/partners/{partnerId}/movements answers them (UC-040) and the
// web's panel lists them.
type Movement struct {
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
	views, err := MovementsOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), web.ID(r, "partnerId"), caller)
	if !changes.Answered(w, r, err) {
		web.JSON(w, r, http.StatusOK, views)
	}
}

// MovementsOf is the partner partnerID's approved movements in projectID, newest first, as movements answers them (UC-040): the rule both doors
// call, the app's route and the web's details panel (redix-api #169). changes.NotFound for a partner caller may not see, or none; otherwise the
// error is the database's.
func MovementsOf(ctx context.Context, read *store.Queries, projectID, partnerID uuid.UUID, caller access.Caller) ([]Movement, error) {
	partners, err := read.PartnersOf(ctx, projectID)
	if err == nil && (!caller.Access.SeesCapitalOf(projectID, partnerID) || !slices.ContainsFunc(partners, func(partner store.Partner) bool { return partner.ID == partnerID })) {
		err = changes.NotFound
	}
	if err != nil {
		return nil, err
	}
	movements, err := read.CapitalMovements(ctx, projectID)
	moved := make([]uuid.UUID, 0, len(movements))
	for _, row := range movements {
		moved = append(moved, row.ID)
	}
	accounts, accountsErr := read.AccountsOf(ctx, projectID)
	files, filesErr := read.EntryFilesOf(ctx, db.IDList(moved))
	items, itemsErr := read.ItemsOf(ctx, projectID)
	columns, columnsErr := read.ColumnsOf(ctx, projectID)
	if err := errors.Join(err, accountsErr, filesErr, itemsErr, columnsErr); err != nil {
		return nil, err
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
	var views []Movement
	for _, row := range movements {
		if row.PartnerID != partnerID || row.Status != ledger.Approved {
			continue
		}
		movementName, err := structure.ChoiceNameIn(capitalColumns, row.ColumnValues)
		if err != nil {
			return nil, err
		}
		one := Movement{ID: row.ID, MovementName: movementName, Date: row.EntryDate, Amount: ledger.Signed(row.Kind, fixed.Counted(row.Amount)),
			Description: row.Description, AccountName: accountName[row.AccountID]}
		for _, file := range files {
			if file.EntryID == row.ID {
				one.AttachmentID, one.ContentType, one.FileName = file.AttachmentID.String(), file.ContentType, file.FileName
				break
			}
		}
		views = append(views, one)
	}
	return views, nil
}
