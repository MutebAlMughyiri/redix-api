// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package entries

import (
	"encoding/json"
	"math"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/spelling"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// pageSize is how many lines a page of a history holds (HistoryPage.Size).
const pageSize = 50

// The history screens beside one another (redix-app #101): the accounts', the partners' and the approvals', each read from the History table by
// its project alone (D11), newest first and a page at a time.

// historyLine is a line of a history, which knows what a search finds it by (IHistoryLine).
type historyLine interface {
	holds(text string) bool
}

// historyBatch is how many History lines a screen reads at a time, newest first, until its page is full (#87): a page reads the batches it needs
// where it read every line of the project (the project history on a project ten times qa's size: 1.6 s to 0.5 s).
const historyBatch = 500

// historyLines reads a history screen's lines older than before, newest first, a batch at a time; none once they have run out.
type historyLines func(before int64) ([]store.History, error)

// projectLines are the project's History lines, a batch at a time, only the project's (D11).
func projectLines(c changes.Change) historyLines {
	return func(before int64) ([]store.History, error) {
		return c.Q.HistoryBefore(c.Ctx, store.HistoryBeforeParams{ProjectID: c.ProjectID, Before: before, Take: historyBatch})
	}
}

// page is a page of a history screen (HistoryPage.LinesAsync): its lines read newest first a batch at a time, after the line before (0 for the
// newest), linesOf keeping the ones the screen shows and making them; only those the person by wrote when one is named (UC-069 R5) and, once
// something was typed, only those that hold it, until the page is full or the lines run out (#87).
func page[T historyLine](r *http.Request, read historyLines, linesOf func(batch []store.History) ([]T, error)) ([]T, error) {
	query := r.URL.Query()
	by, _ := uuid.Parse(query.Get("by"))
	before, _ := strconv.ParseInt(query.Get("before"), 10, 64)
	if before <= 0 {
		before = math.MaxInt64
	}
	text := query.Get("text")
	lines := []T{}
	for {
		batch, err := read(before)
		if err != nil || len(batch) == 0 {
			return lines, err
		}
		before = batch[len(batch)-1].ID
		if by != uuid.Nil {
			batch = slices.DeleteFunc(batch, func(row store.History) bool { return row.UserID != by })
		}
		made, err := linesOf(batch)
		if err != nil {
			return nil, err
		}
		for _, line := range made {
			if strings.TrimSpace(text) != "" && !line.holds(text) {
				continue
			}
			lines = append(lines, line)
			if len(lines) == pageSize {
				return lines, nil
			}
		}
	}
}

// each makes a line of every row of a batch keep keeps: a screen whose lines need nothing beyond their own row and what it read before.
func each[T historyLine](keep func(store.History) bool, lineOf func(store.History) T) func([]store.History) ([]T, error) {
	return func(batch []store.History) ([]T, error) {
		var lines []T
		for _, row := range batch {
			if keep(row) {
				lines = append(lines, lineOf(row))
			}
		}
		return lines, nil
	}
}

// textIn is the text a line kept under name; "" when it kept none (HistoryPage.TextIn).
func textIn(values, name string) string {
	var kept map[string]any
	json.Unmarshal([]byte(values), &kept)
	text, _ := kept[name].(string)
	return text
}

// idIn is the id a line kept under name; uuid.Nil when it kept none (HistoryPage.IdIn).
func idIn(values, name string) uuid.UUID {
	id, _ := uuid.Parse(textIn(values, name))
	return id
}

// What a line of the account history says was done (AccountHistoryKind).
var accountKinds = web.Enum{"accountCreated", "accountChanged", "accountDisabled", "accountEnabled", "accountDeleted", "custodianChanged", "transfer",
	"entryIn", "entryOut", "check"}

type accountLine struct {
	ID           int64         `json:"id"`
	At           moment.Moment `json:"at"`
	By           string        `json:"by"`
	Kind         named         `json:"kind"`
	Account      string        `json:"account"`
	OtherAccount string        `json:"otherAccount"`
	Amount       fixed.Total   `json:"amount"`
	EntryKind    named         `json:"entryKind"`
	NameWas      string        `json:"nameWas"`
	Custodian    string        `json:"custodian"`
}

func (l accountLine) holds(text string) bool {
	return spelling.Holds(text, l.Amount.Number(), l.By, l.Account, l.OtherAccount, l.NameWas, l.Custodian)
}

// accountHistory is a page of the project's account history (AccountHistory, redix-app #89): accounts added, changed, disabled, enabled or
// deleted, a custodian changed, transfers, entries once approved or reversed, and checks; account keeps one account's lines, unknownAccount (400)
// for one not in the project.
func (s Routes) accountHistory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	accountID, _ := uuid.Parse(r.URL.Query().Get("account"))
	accounts, err := c.Q.AccountsOf(c.Ctx, c.ProjectID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	if accountID != uuid.Nil && !slices.ContainsFunc(accounts, func(account store.AccountsOfRow) bool { return account.ID == accountID }) {
		problem.Refuse(w, r, http.StatusBadRequest, "unknownAccount")
		return
	}
	seen, known, err := s.seenBy(c, caller)
	deleted, deletedErr := c.Q.DeletedAccountsOf(c.Ctx, c.ProjectID)
	transfers, transfersErr := c.Q.TransfersOf(c.Ctx, c.ProjectID)
	balances, balancesErr := c.Q.ReconciliationBalancesOf(c.Ctx, c.ProjectID)
	if err != nil || deletedErr != nil || transfersErr != nil || balancesErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	names := known.accounts
	for _, row := range deleted {
		names[row.RecordID] = textIn(row.Before, "Name")
	}
	entryOf := map[uuid.UUID]store.Entry{}
	for _, entry := range seen {
		entryOf[entry.ID] = entry
	}
	transferOf := map[uuid.UUID]store.Transfer{}
	for _, transfer := range transfers {
		transferOf[transfer.ID] = transfer
	}
	checked := map[uuid.UUID]fixed.Total{}
	checks := map[uuid.UUID][]uuid.UUID{}
	for _, balance := range balances {
		checked[balance.ReconciliationID] = checked[balance.ReconciliationID].Plus(fixed.Counted(balance.ActualBalance))
		checks[balance.ReconciliationID] = append(checks[balance.ReconciliationID], balance.AccountID)
	}
	keep := func(row store.History) bool {
		_, isSeen := entryOf[row.RecordID]
		isAccounts := slices.Contains([]string{"Accounts", "AccountCustodians", "Transfers", "Reconciliations"}, row.TableName)
		if !isAccounts && !(row.TableName == "Entries" && (row.Action == string(history.Approve) || row.Action == string(history.Reverse)) && isSeen) {
			return false
		}
		if accountID == uuid.Nil {
			return true
		}
		transfer, isTransfer := transferOf[row.RecordID]
		entry, isEntry := entryOf[row.RecordID]
		return row.RecordID == accountID || (isTransfer && (transfer.FromAccountID == accountID || transfer.ToAccountID == accountID)) ||
			(isEntry && entry.AccountID == accountID) || slices.Contains(checks[row.RecordID], accountID)
	}
	lines, err := page(r, projectLines(c), each(keep, func(row store.History) accountLine {
		line := accountLine{ID: row.ID, At: row.At, By: known.authors[row.UserID], Kind: named{1, accountKinds}, EntryKind: kindOf(purchase)}
		switch row.TableName {
		case "Accounts":
			kinds := map[string]int64{"Create": 0, "Disable": 2, "Enable": 3, "Delete": 4}
			line.Kind.value = 1
			if kind, found := kinds[row.Action]; found {
				line.Kind.value = kind
			}
			line.Account = names[row.RecordID]
			if row.Action == "Update" {
				line.NameWas = textIn(row.Before, "Name")
			}
		case "AccountCustodians":
			line.Kind.value, line.Account, line.Custodian = 5, names[row.RecordID], known.authors[idIn(row.After, "UserId")]
		case "Transfers":
			transfer := transferOf[row.RecordID]
			line.Kind.value, line.Account, line.OtherAccount, line.Amount = 6, names[transfer.FromAccountID], names[transfer.ToAccountID], fixed.Counted(transfer.Amount)
		case "Reconciliations":
			line.Kind.value, line.Amount = 9, checked[row.RecordID]
		default:
			entry := entryOf[row.RecordID]
			line.Kind.value = 7
			if signed(entry.Kind, fixed.Counted(entry.Amount)).Number() < 0 {
				line.Kind.value = 8
			}
			amount := entry.Amount
			if amount < 0 {
				amount = -amount
			}
			line.Account, line.Amount, line.EntryKind = names[entry.AccountID], fixed.Counted(amount), kindOf(entry.Kind)
		}
		return line
	}))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, lines)
}

// What a line of the partner history says was done (PartnerHistoryKind).
var partnerKinds = web.Enum{"partnerCreated", "partnerChanged", "partnerAccount", "partnerDeleted", "capitalIn", "capitalOut"}

type partnerLine struct {
	ID      int64         `json:"id"`
	At      moment.Moment `json:"at"`
	By      string        `json:"by"`
	Kind    named         `json:"kind"`
	Partner string        `json:"partner"`
	Account string        `json:"account"`
	Amount  fixed.Total   `json:"amount"`
	NameWas string        `json:"nameWas"`
	Login   string        `json:"login"`
}

func (l partnerLine) holds(text string) bool {
	return spelling.Holds(text, l.Amount.Number(), l.By, l.Partner, l.Account, l.NameWas, l.Login)
}

// partnerHistory is a page of the project's partner history (PartnerHistory, redix-app #101): partners added, changed and deleted, and capital
// movements once approved or reversed, for the partners the caller sees (BR-31); partner keeps one partner's lines, unknownPartner (400) for one
// not in the project or not seen.
func (s Routes) partnerHistory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId")}
	partners, err := c.Q.PartnersOf(c.Ctx, c.ProjectID)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	seesAll, chosen := caller.Access.Holds(c.ProjectID, access.SeeAllCapital), caller.Access.ChosenIn(c.ProjectID)
	if !caller.Access.Holds(c.ProjectID, access.SeeChosenCapital) {
		chosen = nil
	}
	var shown []uuid.UUID
	for _, partner := range partners {
		if seesAll || slices.Contains(chosen, partner.ID) {
			shown = append(shown, partner.ID)
		}
	}
	if partnerID, _ := uuid.Parse(r.URL.Query().Get("partner")); partnerID != uuid.Nil {
		if !slices.Contains(shown, partnerID) {
			problem.Refuse(w, r, http.StatusBadRequest, "unknownPartner")
			return
		}
		shown = []uuid.UUID{partnerID}
	}
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	known, factsErr := factsOf(c)
	if err != nil || factsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	entryOf := map[uuid.UUID]store.Entry{}
	for _, entry := range all {
		if partner, isCapital := known.capital[entry.ID]; isCapital && slices.Contains(shown, partner) {
			entryOf[entry.ID] = entry
		}
	}
	keep := func(row store.History) bool {
		_, isMovement := entryOf[row.RecordID]
		partnerRow := (row.TableName == "Partners" || (row.TableName == "PartnerUsers" && row.Action == "Update")) && slices.Contains(shown, row.RecordID)
		return partnerRow || (row.TableName == "Entries" && (row.Action == string(history.Approve) || row.Action == string(history.Reverse)) && isMovement)
	}
	lines, err := page(r, projectLines(c), each(keep, func(row store.History) partnerLine {
		line := partnerLine{ID: row.ID, At: row.At, By: known.authors[row.UserID], Kind: named{0, partnerKinds}}
		if row.TableName != "Entries" {
			switch {
			case row.Action == "Create":
			case row.Action == "Delete":
				line.Kind.value = 3
			case textIn(row.Before, "Name") != "":
				line.Kind.value = 1
			default:
				line.Kind.value = 2
			}
			line.Partner, line.NameWas, line.Login = known.partners[row.RecordID], textIn(row.Before, "Name"), known.authors[idIn(row.After, "UserId")]
			return line
		}
		entry := entryOf[row.RecordID]
		line.Kind.value = 4
		if entry.Kind == capitalOut {
			line.Kind.value = 5
		}
		line.Partner, line.Account, line.Amount = known.partners[known.capital[entry.ID]], known.accounts[entry.AccountID], fixed.Counted(entry.Amount)
		return line
	}))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, lines)
}

// What a line of the approval history says was decided (ApprovalHistoryKind).
var approvalKinds = web.Enum{"approved", "rejected"}

type approvalLine struct {
	ID     int64         `json:"id"`
	At     moment.Moment `json:"at"`
	By     string        `json:"by"`
	Kind   named         `json:"kind"`
	Item   string        `json:"item"`
	Party  string        `json:"party"`
	Amount fixed.Number  `json:"amount"`
	Reason string        `json:"reason"`
}

func (l approvalLine) holds(text string) bool {
	return spelling.Holds(text, l.Amount, l.By, l.Item, l.Party, l.Reason)
}

// approvalHistory is a page of the project's approval history (ApprovalHistory, the owner 12 Nov 2026): every entry approved or rejected, by whom and
// when, a rejection with its reason; kind keeps the decisions on entries of one kind, any other text every kind.
func (s Routes) approvalHistory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	c := changes.Change{Ctx: r.Context(), Q: s.database.Queries(), ProjectID: web.ID(r, "projectId"), Access: caller.Access}
	chosen, chosenErr := kinds.Parse(r.URL.Query().Get("kind"))
	all, err := c.Q.EntriesOf(c.Ctx, c.ProjectID)
	known, factsErr := factsOf(c)
	decisions, decisionsErr := c.Q.DecisionsOf(c.Ctx, c.ProjectID)
	if err != nil || factsErr != nil || decisionsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	entryOf := map[uuid.UUID]store.Entry{}
	for _, entry := range all {
		if (chosenErr != nil || entry.Kind == chosen) && known.tableSeen(caller.Access, c.ProjectID, entry.ID) {
			entryOf[entry.ID] = entry
		}
	}
	keep := func(row store.History) bool {
		_, decided := entryOf[row.RecordID]
		return decided && row.TableName == "Entries" && (row.Action == "Approve" || row.Action == "Reject")
	}
	lines, err := page(r, projectLines(c), each(keep, func(row store.History) approvalLine {
		entry := entryOf[row.RecordID]
		line := approvalLine{ID: row.ID, At: row.At, By: known.authors[row.UserID], Kind: named{0, approvalKinds}, Item: known.itemNameOf(entry),
			Party: known.supplierOf(entry).Name, Amount: entry.Amount}
		if line.Item == "" {
			line.Item = known.partners[known.capital[entry.ID]]
		}
		if row.Action == "Reject" {
			line.Kind.value, line.Reason = 1, reasonOf(decisions, entry.ID, row.At)
		}
		return line
	}))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, lines)
}
