// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"net/http"
	"slices"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
)

// columnsShown is an item's Columns as the app's screen shows them (UC-013): the item and its category, whether the screen shows the default
// template of its type or this item's own columns, and those columns in order.
type columnsShown struct {
	item     structure.ItemView
	category structure.CategoryView
	template bool
	columns  []structure.ColumnView
}

// scope is the screen's scope as its pages name it: template or item.
func (s columnsShown) scope() string {
	if s.template {
		return "template"
	}
	return "item"
}

// path is the page that keeps the shown columns: the item's own, or the default template of its type.
func (s columnsShown) path() string {
	return "/columns/" + s.item.ID.String() + "/" + s.scope()
}

// columnOf is the shown column with columnID.
func (s columnsShown) columnOf(columnID uuid.UUID) (structure.ColumnView, bool) {
	at := slices.IndexFunc(s.columns, func(column structure.ColumnView) bool { return column.ID == columnID })
	if at < 0 {
		return structure.ColumnView{}, false
	}
	return s.columns[at], true
}

// itemDetails is the line under Columns' title, as the app's says it: the item's category, its type, and for a revenue item that it takes no
// columns of its own.
func (s columnsShown) itemDetails(p page) string {
	revenue := ""
	if s.item.Type.IsRevenue() {
		revenue = p.words.Of("columns.revenueTakesNoColumns")
	}
	return parts(s.category.Name, p.words.Of(itemTypeWord(s.item.Type)), revenue)
}

// columnLine is what a column's row says under its name, as the app's row does: custom, its type, required or optional, in reports.
func columnLine(p page, column structure.ColumnView) string {
	custom, required, reports := "", p.words.Of("columns.optional"), ""
	if column.IsCustom {
		custom = p.words.Of("columns.custom")
	}
	if column.IsRequired {
		required = p.words.Of("columns.required")
	}
	if column.ShowInReports {
		reports = p.words.Of("columns.inReports")
	}
	return parts(custom, p.words.Of(dataTypeWord(column.DataType)), required, reports)
}

// dataTypeWord is the word of a column's data type, as the app's ColumnDataType.wordKey names it: text is columns.typeText.
func dataTypeWord(dataType structure.DataType) string {
	name := dataType.Name()
	return "columns.type" + strings.ToUpper(name[:1]) + name[1:]
}

// choicesLine is a list's choices as the app's Edit column says them, each with its sign when it gives one.
func choicesLine(p page, column structure.ColumnView) string {
	names := make([]string, 0, len(column.Choices))
	for _, choice := range column.Choices {
		name := choice.Name
		if choice.Sign != structure.Unsigned {
			name += " (" + p.words.Of("columns.sign"+strings.ToUpper(choice.Sign.Name()[:1])+choice.Sign.Name()[1:]) + ")"
		}
		names = append(names, name)
	}
	return parts(names...)
}

// gainsChoices says whether Edit column adds choices to column: a list a person added, as the app's ItemColumn.gainsChoices.
func gainsChoices(column structure.ColumnView) bool {
	return column.DataType == structure.List && column.IsCustom
}

// linesOf is a text area's lines, each a choice the app sends as one.
func linesOf(typed string) []structure.ChoiceRequest {
	var choices []structure.ChoiceRequest
	for _, line := range strings.Split(strings.ReplaceAll(typed, "\r\n", "\n"), "\n") {
		choices = append(choices, structure.ChoiceRequest{Name: line})
	}
	return choices
}

// columnFormTitle, columnFormSave and columnFormPath are the words and the page of a column's form, as the app's dialogs say them: New column
// for one not kept yet, Edit column and Save for one that is.
func columnFormTitle(column structure.ColumnView) string {
	if column.ID == uuid.Nil {
		return "columns.new"
	}
	return "columns.edit"
}

func columnFormSave(column structure.ColumnView) string {
	if column.ID == uuid.Nil {
		return "columns.new"
	}
	return "columns.save"
}

func columnFormPath(shown columnsShown, column structure.ColumnView) string {
	if column.ID == uuid.Nil {
		return shown.path() + "/new/add"
	}
	return shown.path() + "/" + column.ID.String() + "/edit"
}

// columnsOf is itemID's Columns, the template of its type when template, read through the rules the app's routes answer
// (structure.ColumnsOfItem, structure.ColumnsOfTemplate); found false for an item the project does not hold. The error is the database's.
func (s site) columnsOf(ctx context.Context, p page, itemID uuid.UUID, template bool) (columnsShown, bool, error) {
	all, err := s.structureOf(ctx, p, "")
	if err != nil {
		return columnsShown{}, false, err
	}
	item, category, found := all.itemOf(itemID)
	if !found {
		return columnsShown{}, false, nil
	}
	shown := columnsShown{item: item, category: category, template: template}
	read := s.Database.Queries()
	if template {
		shown.columns, err = structure.ColumnsOfTemplate(ctx, read, p.project.ID, item.Type)
	} else {
		shown.columns, err = structure.ColumnsOfItem(ctx, read, p.project.ID, item.ID)
	}
	return shown, true, err
}

// columnsTab is an item's Columns in its tab (UC-013, redix-api #174), opened from the item's panel: this item's own columns, or with
// scope=template the default template of its type, as the app's screen switches them. An item the project does not hold leaves the tab empty.
func (s site) columnsTab(r *http.Request, p page) (templ.Component, error) {
	itemID, _ := uuid.Parse(r.URL.Query().Get("item"))
	shown, found, err := s.columnsOf(r.Context(), p, itemID, r.URL.Query().Get("scope") == "template")
	if !found {
		return columnsGone(p), err
	}
	return columnsList(p, shown, ""), err
}

// columnDetails is a column in the details panel, or a form of its as the app's dialogs ask it: New column (id new), Edit (asking=edit), and
// the question a template's change asks (asking=delete). 404 without ManageStructure, or for an item or a column there is none of.
func (s site) columnDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	scope := r.PathValue("scope")
	if !p.inProject() || !p.may(access.ManageStructure) || err != nil || (scope != "item" && scope != "template") {
		http.NotFound(w, r)
		return
	}
	shown, found, err := s.columnsOf(r.Context(), p, itemID, scope == "template")
	switch {
	case err != nil:
		failed(w)
		return
	case !found:
		http.NotFound(w, r)
		return
	}
	if r.PathValue("id") == "new" {
		draw(w, r, http.StatusOK, columnForm(p, shown, structure.ColumnView{ShowInReports: true}, "", ""))
		return
	}
	columnID, _ := uuid.Parse(r.PathValue("id"))
	column, found := shown.columnOf(columnID)
	switch {
	case !found:
		http.NotFound(w, r)
	case r.URL.Query().Get("asking") == "edit":
		draw(w, r, http.StatusOK, columnForm(p, shown, column, "", ""))
	case r.URL.Query().Get("asking") == "delete":
		draw(w, r, http.StatusOK, deleteQuestion(p, shown, column))
	default:
		draw(w, r, http.StatusOK, columnPanel(p, shown, column, ""))
	}
}

// typedColumn is the column New column's form typed.
func typedColumn(r *http.Request) structure.ColumnRequest {
	dataType, _ := structure.DataTypeNamed(r.PostFormValue("dataType"))
	typed := structure.ColumnRequest{Name: r.PostFormValue("name"), DataType: dataType, IsRequired: r.PostFormValue("required") == "on",
		ShowInReports: r.PostFormValue("reports") == "on"}
	if dataType == structure.List {
		typed.Choices = linesOf(r.PostFormValue("choices"))
	}
	return typed
}

// typedEdit is what Edit column's form typed: the name, required and in reports, and new choices when it typed any.
func typedEdit(r *http.Request) structure.EditRequest {
	typed := structure.EditRequest{Name: r.PostFormValue("name"), IsRequired: r.PostFormValue("required") == "on", ShowInReports: r.PostFormValue("reports") == "on"}
	if strings.TrimSpace(r.PostFormValue("choices")) != "" {
		typed.NewChoices = linesOf(r.PostFormValue("choices"))
	}
	return typed
}

// keepColumn keeps New column (POST /columns/{item}/{scope}/new/add), Edit column (…/{id}/edit) or Delete (…/{id}/delete) through the rule the
// app's route runs, the template's with applyToAll as the question answered it, and answers the column's panel, Columns beside it; refused, the
// form or the panel says why in the app's words. 403 without ManageStructure; 404 for an action there is none of, or for an item or a column
// there is none of; a finished project changes nothing (AC-04.1).
func (s site) keepColumn(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	scope, action, adding := r.PathValue("scope"), r.PathValue("action"), r.PathValue("id") == "new"
	known := (adding && action == "add") || (!adding && (action == "edit" || action == "delete"))
	if !p.inProject() || err != nil || r.ParseForm() != nil || !known || (scope != "item" && scope != "template") {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageStructure) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	template := scope == "template"
	shown, found, err := s.columnsOf(r.Context(), p, itemID, template)
	if err != nil {
		failed(w)
		return
	}
	columnID, _ := uuid.Parse(r.PathValue("id"))
	column, columnFound := shown.columnOf(columnID)
	if !found || (!adding && !columnFound) {
		http.NotFound(w, r)
		return
	}
	applyToAll, refusal := r.PostFormValue("applyToAll") == "true", "columns.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			switch {
			case adding && template:
				return structure.AddToTemplate(c, shown.item.Type, typedColumn(r), applyToAll)
			case adding:
				return structure.AddToItem(c, itemID, typedColumn(r))
			case action == "edit" && template:
				return structure.EditTemplateColumn(c, shown.item.Type, columnID, typedEdit(r), applyToAll)
			case action == "edit":
				return structure.EditItemColumn(c, itemID, columnID, typedEdit(r))
			case template:
				return structure.DeleteTemplateColumn(c, shown.item.Type, columnID, applyToAll)
			}
			return structure.DeleteItemColumn(c, itemID, columnID)
		})
		if refusal, err = refusalOf("columns", err); err != nil {
			failed(w)
			return
		}
	}
	if refusal != "" {
		if action == "delete" {
			draw(w, r, http.StatusOK, columnPanel(p, shown, column, refusal))
			return
		}
		typed := structure.ColumnView{ID: column.ID, Name: r.PostFormValue("name"), DataType: typedColumn(r).DataType, IsCore: column.IsCore,
			IsCustom: column.IsCustom, IsRequired: r.PostFormValue("required") == "on", ShowInReports: r.PostFormValue("reports") == "on", Choices: column.Choices}
		if !adding {
			typed.DataType = column.DataType
		}
		draw(w, r, http.StatusOK, columnForm(p, shown, typed, r.PostFormValue("choices"), refusal))
		return
	}
	if shown, _, err = s.columnsOf(r.Context(), p, itemID, template); err != nil {
		failed(w)
		return
	}
	if action == "delete" {
		draw(w, r, http.StatusOK, columnsBeside(p, shown))
		return
	}
	kept := shown.columns[len(shown.columns)-1]
	if !adding {
		kept, _ = shown.columnOf(columnID)
	}
	draw(w, r, http.StatusOK, columnPanel(p, shown, kept, ""), columnsBeside(p, shown))
}

// reorderColumns keeps the order a drag or Alt+Up and Alt+Down left the columns in, through structure.ReorderItemColumns or, the template's,
// structure.ReorderTemplate (redix-api #174 D1), and draws Columns again; a template's order first asks, in the details panel, whether it applies
// to every item of the type, as the app's question does. Refused, it says why above the list. 403 without ManageStructure; a finished project
// changes nothing.
func (s site) reorderColumns(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	itemID, err := uuid.Parse(r.PathValue("item"))
	scope := r.PathValue("scope")
	if !p.inProject() || err != nil || r.ParseForm() != nil || (scope != "item" && scope != "template") {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageStructure) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	template := scope == "template"
	shown, found, err := s.columnsOf(r.Context(), p, itemID, template)
	switch {
	case err != nil:
		failed(w)
		return
	case !found:
		http.NotFound(w, r)
		return
	}
	if template && r.PostFormValue("applyToAll") == "" && !p.finished() {
		w.Header().Set("HX-Retarget", "[id='web.details']")
		draw(w, r, http.StatusOK, orderQuestion(p, shown, r.PostFormValue("ids")))
		return
	}
	var ids []uuid.UUID
	for _, id := range strings.Split(r.PostFormValue("ids"), ",") {
		if parsed, err := uuid.Parse(id); err == nil {
			ids = append(ids, parsed)
		}
	}
	refusal := "columns.projectFinished"
	if !p.finished() {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) error {
			if template {
				return structure.ReorderTemplate(c, shown.item.Type, ids, r.PostFormValue("applyToAll") == "true")
			}
			return structure.ReorderItemColumns(c, itemID, ids)
		})
		if refusal, err = refusalOf("columns", err); err != nil {
			failed(w)
			return
		}
	}
	if shown, _, err = s.columnsOf(r.Context(), p, itemID, template); err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, columnsList(p, shown, refusal), detailsPanel(true))
}
