// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
)

// tablesShown is an item's Tables as the app's screen shows them (UC-012): the item, its tables in the order its sheet prints them, and the
// search typed.
type tablesShown struct {
	item   structure.ItemView
	all    []entries.ItemTableView
	text   string
	asking string
}

// shown is the tables whose name holds the search, as the app's ListSearch keeps them.
func (s tablesShown) shown() []entries.ItemTableView {
	return slices.DeleteFunc(slices.Clone(s.all), func(table entries.ItemTableView) bool { return !searchHolds(s.text, table.Name) })
}

// tableOf is the table with tableID.
func (s tablesShown) tableOf(tableID uuid.UUID) (entries.ItemTableView, bool) {
	at := slices.IndexFunc(s.all, func(table entries.ItemTableView) bool { return table.ID == tableID })
	if at < 0 {
		return entries.ItemTableView{}, false
	}
	return s.all[at], true
}

// mayMove says whether Tables' rows move: for whoever may shape the structure, while the whole list shows, since an order names every table.
func (s tablesShown) mayMove(p page) bool { return p.mayShape() && s.text == "" }

// mayAddSubgroup says whether New subgroup works: for whoever may shape the structure, on an item that takes purchases, as the app's button.
func (s tablesShown) mayAddSubgroup(p page) bool {
	return p.mayShape() && (s.item.Type == structure.Purchases || s.item.Type == structure.Both)
}

// tableLine is what a table's row says under its name, as the app's row does: its kind, then how many approved entries it holds.
func (s tablesShown) tableLine(p page, table entries.ItemTableView) string {
	return p.words.Of(tableKindWord(table.Kind.Name(), s.item.Type.IsRevenue())) + " · " + strconv.Itoa(table.Count)
}

// tableKindWord is the word of a table's kind on an item of its side, as the app's ItemTableKind.wordKeyOf says it.
func tableKindWord(kind string, revenue bool) string {
	switch {
	case kind == "contract":
		return "categories.tableContract"
	case kind == "partner":
		return "categories.tableCapital"
	case revenue:
		return "categories.tableRevenue"
	}
	return "categories.tablePurchases"
}

// isNamedSubgroup says whether a table is a subgroup a person named, the one kind renamed and deleted, as the app's ItemTable.isNamedSubgroup.
func isNamedSubgroup(table entries.ItemTableView) bool { return table.Kind.Name() == "subgroup" }

// tablesOf is itemID's Tables with the search text, read through entries.ItemTablesOf, the rule the app's route answers; found false for an item
// the project does not hold. The error is the database's.
func (s site) tablesOf(ctx context.Context, p page, itemID uuid.UUID, text string) (tablesShown, bool, error) {
	all, err := s.structureOf(ctx, p, "")
	if err != nil {
		return tablesShown{}, false, err
	}
	item, _, found := all.itemOf(itemID)
	if !found {
		return tablesShown{}, false, nil
	}
	c := changes.Change{Ctx: ctx, Q: s.Database.Queries(), ProjectID: p.project.ID}
	tables, err := entries.ItemTablesOf(c, itemID, p.caller.Access)
	return tablesShown{item: item, all: tables, text: strings.TrimSpace(text)}, true, err
}

// itemTablesTab is an item's Tables in its tab (UC-012, redix-api #174), opened from the item's panel; an item the project does not hold leaves
// the tab empty.
func (s site) itemTablesTab(r *http.Request, p page) (templ.Component, error) {
	itemID, _ := uuid.Parse(r.URL.Query().Get("item"))
	shown, found, err := s.tablesOf(r.Context(), p, itemID, r.URL.Query().Get("text"))
	if !found {
		return tablesGone(p), err
	}
	return tablesList(p, shown, ""), err
}

// tableDetails is a table in the details panel, or a form of its as the app's dialogs ask it: New subgroup (id new), Rename (asking=rename) and
// Delete asked first (asking=delete). 404 without ManageStructure, or for an item or a table there is none of.
func (s site) tableDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	if !p.inProject() || !p.may(access.ManageStructure) || err != nil {
		http.NotFound(w, r)
		return
	}
	shown, found, err := s.tablesOf(r.Context(), p, itemID, "")
	switch {
	case err != nil:
		failed(w)
		return
	case !found:
		http.NotFound(w, r)
		return
	}
	if r.PathValue("id") == "new" {
		draw(w, r, http.StatusOK, subgroupForm(p, shown, entries.ItemTableView{}, "", ""))
		return
	}
	tableID, _ := uuid.Parse(r.PathValue("id"))
	table, found := shown.tableOf(tableID)
	switch {
	case !found:
		http.NotFound(w, r)
	case r.URL.Query().Get("asking") == "rename":
		draw(w, r, http.StatusOK, subgroupForm(p, shown, table, table.Name, ""))
	default:
		shown.asking = r.URL.Query().Get("asking")
		draw(w, r, http.StatusOK, tablePanel(p, shown, table, ""))
	}
}

// keepTable keeps New subgroup (POST /tables/{item}/new/add), Rename (…/{id}/rename) or Delete (…/{id}/delete) through the rule the app's route
// runs, and answers the subgroup's panel, Tables beside it, or Tables alone once deleted; refused, the form or the panel says why in the app's
// words. 403 without ManageStructure; 404 for an action there is none of, or for an item or a table there is none of; a finished project
// changes nothing (AC-04.1).
func (s site) keepTable(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	action, adding := r.PathValue("action"), r.PathValue("id") == "new"
	known := (adding && action == "add") || (!adding && (action == "rename" || action == "delete"))
	if !p.inProject() || err != nil || r.ParseForm() != nil || !known {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageStructure) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, found, err := s.tablesOf(r.Context(), p, itemID, "")
	if err != nil {
		failed(w)
		return
	}
	tableID, _ := uuid.Parse(r.PathValue("id"))
	table, tableFound := shown.tableOf(tableID)
	if !found || (!adding && !tableFound) {
		http.NotFound(w, r)
		return
	}
	refusal := "categories.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
			switch action {
			case "add":
				tableID, err = structure.CreateSubgroup(c, itemID, r.PostFormValue("name"))
			case "rename":
				err = structure.RenameSubgroup(c, itemID, tableID, r.PostFormValue("name"))
			default:
				err = structure.DeleteSubgroup(c, itemID, tableID)
			}
			return err
		})
		if refusal, err = refusalOf("categories", err); err != nil {
			failed(w)
			return
		}
	}
	if refusal != "" {
		if action == "delete" {
			draw(w, r, http.StatusOK, tablePanel(p, shown, table, refusal))
			return
		}
		draw(w, r, http.StatusOK, subgroupForm(p, shown, table, r.PostFormValue("name"), refusal))
		return
	}
	if shown, _, err = s.tablesOf(r.Context(), p, itemID, ""); err != nil {
		failed(w)
		return
	}
	if action == "delete" {
		draw(w, r, http.StatusOK, tablesBeside(p, shown))
		return
	}
	table, _ = shown.tableOf(tableID)
	draw(w, r, http.StatusOK, tablePanel(p, shown, table, ""), tablesBeside(p, shown))
}

// reorderTables keeps the order a drag or Alt+Up and Alt+Down left an item's tables in, through structure.ReorderTables (redix-api #174 D1), and
// draws Tables again; refused, it says why above the list. 403 without ManageStructure; a finished project changes nothing.
func (s site) reorderTables(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	if !p.inProject() || err != nil || r.ParseForm() != nil {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageStructure) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	var ids []uuid.UUID
	for _, id := range strings.Split(r.PostFormValue("ids"), ",") {
		if parsed, err := uuid.Parse(id); err == nil {
			ids = append(ids, parsed)
		}
	}
	refusal := "categories.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			return structure.ReorderTables(c, itemID, ids)
		})
		if refusal, err = refusalOf("categories", err); err != nil {
			failed(w)
			return
		}
	}
	shown, found, err := s.tablesOf(r.Context(), p, itemID, "")
	switch {
	case err != nil:
		failed(w)
	case !found:
		http.NotFound(w, r)
	default:
		draw(w, r, http.StatusOK, tablesList(p, shown, refusal))
	}
}

// subgroupFormPath is where a subgroup's form keeps it: New subgroup adds one to the item, Rename names the one it has.
func subgroupFormPath(shown tablesShown, table entries.ItemTableView) string {
	if table.ID == uuid.Nil {
		return "/tables/" + shown.item.ID.String() + "/new/add"
	}
	return "/tables/" + shown.item.ID.String() + "/" + table.ID.String() + "/rename"
}
