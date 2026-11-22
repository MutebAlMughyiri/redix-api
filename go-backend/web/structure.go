// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"cmp"
	"context"
	"net/http"
	"slices"
	"strconv"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/structure"
)

// structureShown is Categories and items as the app's screen shows them (UC-012): every category with its items, in the order of the periodic
// report, and the search typed.
type structureShown struct {
	all  []structure.CategoryView
	text string
}

// shown is the categories whose name or an item's name holds the search, each with all its items, as the app's ListSearch keeps them.
func (s structureShown) shown() []structure.CategoryView {
	return slices.DeleteFunc(slices.Clone(s.all), func(category structure.CategoryView) bool {
		fields := []string{category.Name}
		for _, item := range category.Items {
			fields = append(fields, item.Name)
		}
		return !searchHolds(s.text, fields...)
	})
}

// categoryOf is the category with categoryID.
func (s structureShown) categoryOf(categoryID uuid.UUID) (structure.CategoryView, bool) {
	at := slices.IndexFunc(s.all, func(category structure.CategoryView) bool { return category.ID == categoryID })
	if at < 0 {
		return structure.CategoryView{}, false
	}
	return s.all[at], true
}

// itemOf is the item with itemID and the category holding it.
func (s structureShown) itemOf(itemID uuid.UUID) (structure.ItemView, structure.CategoryView, bool) {
	for _, category := range s.all {
		if at := slices.IndexFunc(category.Items, func(item structure.ItemView) bool { return item.ID == itemID }); at >= 0 {
			return category.Items[at], category, true
		}
	}
	return structure.ItemView{}, structure.CategoryView{}, false
}

// mayShape says whether p's caller may change the project's structure: ManageStructure, in a project not finished, as the app's
// mayChangeStructure (AC-04.1).
func (p page) mayShape() bool { return !p.finished() && p.may(access.ManageStructure) }

// itemTypeWord is the word of an item type, as the app's ItemType.wordKey names it: purchases is categories.typePurchases.
func itemTypeWord(itemType structure.ItemType) string {
	name := itemType.Name()
	return "categories.type" + strings.ToUpper(name[:1]) + name[1:]
}

// categoryLine is what a category's row says under its name, as the app's card does: how many items it holds, or that it is disabled.
func categoryLine(p page, category structure.CategoryView) string {
	if !category.IsActive {
		return p.words.Of("categories.disabled")
	}
	return p.words.Of("categories.items") + ": " + strconv.Itoa(len(category.Items))
}

// itemLine is what an item's row says under its name, as the app's row does: its type, or that it is disabled.
func itemLine(p page, item structure.ItemView) string {
	if !item.IsActive {
		return p.words.Of("categories.disabled")
	}
	return p.words.Of(itemTypeWord(item.Type))
}

// categoryFormTitle and categoryFormSave are the words of a category's form, as the app's dialogs say them: New category for one not kept yet,
// Rename and Save for one that is.
func categoryFormTitle(category structure.CategoryView) string {
	if category.ID == uuid.Nil {
		return "categories.new"
	}
	return "categories.rename"
}

func categoryFormSave(category structure.CategoryView) string {
	if category.ID == uuid.Nil {
		return "categories.new"
	}
	return "categories.save"
}

// categoryFormPath is where a category's form keeps it: New category adds one, Rename names the one it has.
func categoryFormPath(category structure.CategoryView) string {
	if category.ID == uuid.Nil {
		return "/structure/category/new/add"
	}
	return "/structure/category/" + category.ID.String() + "/rename"
}

// itemFormTitle is the title of an item's form, as the app's dialogs say it: Add item, Rename or Change type.
func itemFormTitle(asking string) string {
	return map[string]string{"item": "categories.addItem", "rename": "categories.rename", "type": "categories.changeType"}[asking]
}

// itemFormPath is where an item's form keeps it: Add item adds one to category; Rename and Change type change item.
func itemFormPath(category structure.CategoryView, item structure.ItemView, asking string) string {
	if asking == "item" {
		return "/structure/category/" + category.ID.String() + "/item"
	}
	return "/structure/item/" + item.ID.String() + "/" + asking
}

// sideOf is the side an item type is on, as the type choice groups them.
func sideOf(itemType structure.ItemType) string {
	if itemType.IsRevenue() {
		return "revenue"
	}
	return "expenses"
}

// structureOf is Categories and items with the search text, read through structure.ListOf, the rule the app's route answers. The error is the
// database's.
func (s site) structureOf(ctx context.Context, p page, text string) (structureShown, error) {
	all, err := structure.ListOf(ctx, s.Database.Queries(), p.project.ID)
	return structureShown{all: all, text: strings.TrimSpace(text)}, err
}

// categoriesTab is Categories and items in its tab (UC-012, redix-api #174), for whoever shapes the project's structure.
func (s site) categoriesTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.structureOf(r.Context(), p, r.URL.Query().Get("text"))
	return categoriesList(p, shown, ""), err
}

// structureDetails is a category or an item in the details panel, or a form of theirs as the app's dialogs ask it: New category; a category's
// Rename and Add item; an item's Rename and Change type. 404 without ManageStructure, or for one the project does not hold.
func (s site) structureDetails(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	kind := r.PathValue("kind")
	if !p.inProject() || !p.may(access.ManageStructure) || (kind != "category" && kind != "item") {
		http.NotFound(w, r)
		return
	}
	shown, err := s.structureOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	id, _ := uuid.Parse(r.PathValue("id"))
	asking := r.URL.Query().Get("asking")
	if kind == "category" {
		if r.PathValue("id") == "new" {
			draw(w, r, http.StatusOK, categoryForm(p, structure.CategoryView{}, "", ""))
			return
		}
		category, found := shown.categoryOf(id)
		switch {
		case !found:
			http.NotFound(w, r)
		case asking == "rename":
			draw(w, r, http.StatusOK, categoryForm(p, category, category.Name, ""))
		case asking == "item":
			draw(w, r, http.StatusOK, itemForm(p, category, structure.ItemView{Type: structure.Both}, "item", ""))
		default:
			draw(w, r, http.StatusOK, categoryPanel(p, category, ""))
		}
		return
	}
	item, category, found := shown.itemOf(id)
	switch {
	case !found:
		http.NotFound(w, r)
	case asking == "rename" || asking == "type":
		draw(w, r, http.StatusOK, itemForm(p, category, item, asking, ""))
	default:
		draw(w, r, http.StatusOK, itemPanel(p, item, category, ""))
	}
}

// categoryActs are what a category's forms and actions keep, each through the rule the app's route runs: a name typed for New category and
// Rename, an item added, the category disabled or enabled.
var categoryActs = map[string]func(c changes.Change, categoryID uuid.UUID, r *http.Request) (uuid.UUID, error){
	"rename": func(c changes.Change, categoryID uuid.UUID, r *http.Request) (uuid.UUID, error) {
		return categoryID, structure.RenameCategory(c, categoryID, r.PostFormValue("name"))
	},
	"disable": func(c changes.Change, categoryID uuid.UUID, _ *http.Request) (uuid.UUID, error) {
		return categoryID, structure.ActivateCategory(c, categoryID, false)
	},
	"enable": func(c changes.Change, categoryID uuid.UUID, _ *http.Request) (uuid.UUID, error) {
		return categoryID, structure.ActivateCategory(c, categoryID, true)
	},
}

// itemActs are what an item's forms and actions keep, each through the rule the app's route runs.
var itemActs = map[string]func(c changes.Change, itemID uuid.UUID, r *http.Request) error{
	"rename": func(c changes.Change, itemID uuid.UUID, r *http.Request) error {
		return structure.RenameItem(c, itemID, r.PostFormValue("name"))
	},
	"type": func(c changes.Change, itemID uuid.UUID, r *http.Request) error {
		return structure.ChangeItemType(c, itemID, typedItemType(r))
	},
	"percentages": func(c changes.Change, itemID uuid.UUID, r *http.Request) error {
		return structure.SetTablePercentages(c, itemID, r.PostFormValue("held") == "on")
	},
	"disable": func(c changes.Change, itemID uuid.UUID, _ *http.Request) error {
		return structure.ActivateItem(c, itemID, false)
	},
	"enable": func(c changes.Change, itemID uuid.UUID, _ *http.Request) error {
		return structure.ActivateItem(c, itemID, true)
	},
}

// typedItemType is the item type a form chose.
func typedItemType(r *http.Request) structure.ItemType {
	itemType, _ := structure.ItemTypeNamed(r.PostFormValue("type"))
	return itemType
}

// keepStructure keeps what a category's or an item's form or action asks, through the rule the app's route runs, in one change, and answers the
// one it changed in the details panel, Categories and items beside it; refused, the form or the panel says why in the app's words. A new category
// is POST /structure/category/new/add; an item added, POST /structure/category/{id}/item. 403 without ManageStructure; 404 for an action there
// is none of, or for one the project does not hold; a finished project changes nothing (AC-04.1).
func (s site) keepStructure(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	kind, action := r.PathValue("kind"), r.PathValue("action")
	_, categoryAct := categoryActs[action]
	_, itemAct := itemActs[action]
	adding := kind == "category" && action == "add" && r.PathValue("id") == "new"
	known := adding || (kind == "category" && (categoryAct || action == "item")) || (kind == "item" && itemAct)
	if !p.inProject() || r.ParseForm() != nil || !known {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageStructure) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	shown, err := s.structureOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	id, _ := uuid.Parse(r.PathValue("id"))
	category, categoryFound := shown.categoryOf(id)
	item, holder, itemFound := shown.itemOf(id)
	if (kind == "category" && !adding && !categoryFound) || (kind == "item" && !itemFound) {
		http.NotFound(w, r)
		return
	}
	changed, refusal := id, ""
	if p.finished() {
		refusal = "categories.projectFinished"
	} else {
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
			switch {
			case adding:
				var added store.Category
				added, err = structure.CreateCategory(c, r.PostFormValue("name"))
				changed = added.ID
			case action == "item":
				var added store.Item
				added, _, err = structure.CreateItem(c, id, r.PostFormValue("name"), typedItemType(r))
				changed = added.ID
			case kind == "category":
				changed, err = categoryActs[action](c, id, r)
			default:
				err = itemActs[action](c, id, r)
			}
			return err
		})
		if refusal, err = refusalOf("categories", err); err != nil {
			failed(w)
			return
		}
	}
	if refusal != "" {
		typed := r.PostFormValue("name")
		switch {
		case adding || (kind == "category" && action == "rename"):
			draw(w, r, http.StatusOK, categoryForm(p, category, typed, refusal))
		case action == "item":
			draw(w, r, http.StatusOK, itemForm(p, category, structure.ItemView{Name: typed, Type: typedItemType(r)}, "item", refusal))
		case kind == "item" && (action == "rename" || action == "type"):
			item.Name, item.Type = cmp.Or(typed, item.Name), typedItemType(r)
			draw(w, r, http.StatusOK, itemForm(p, holder, item, action, refusal))
		case kind == "category":
			draw(w, r, http.StatusOK, categoryPanel(p, category, refusal))
		default:
			draw(w, r, http.StatusOK, itemPanel(p, item, holder, refusal))
		}
		return
	}
	if shown, err = s.structureOf(r.Context(), p, ""); err != nil {
		failed(w)
		return
	}
	if category, found := shown.categoryOf(changed); found {
		draw(w, r, http.StatusOK, categoryPanel(p, category, ""), categoriesBeside(p, shown))
		return
	}
	item, holder, _ = shown.itemOf(changed)
	draw(w, r, http.StatusOK, itemPanel(p, item, holder, ""), categoriesBeside(p, shown))
}

// reorderStructure keeps the order a drag or Alt+Up and Alt+Down left the categories in (POST /structure/reorder), or a category's items (POST
// /structure/category/{id}/reorder), through structure.ReorderCategories and structure.ReorderItems (redix-api #174 D1), and draws Categories and
// items again; refused, it says why above the list. 403 without ManageStructure; a finished project changes nothing.
func (s site) reorderStructure(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	categoryID, err := uuid.Parse(r.PathValue("id"))
	ofItems := r.PathValue("id") != ""
	if !p.inProject() || r.ParseForm() != nil || (ofItems && err != nil) {
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
			if ofItems {
				return structure.ReorderItems(c, categoryID, ids)
			}
			return structure.ReorderCategories(c, ids)
		})
		if refusal, err = refusalOf("categories", err); err != nil {
			failed(w)
			return
		}
	}
	shown, err := s.structureOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, categoriesList(p, shown, refusal))
}

// reorderRows moves a row of a list among its own, dragged or with Alt+Up and Alt+Down, as the app's ReorderableListView does (redix-api #174
// D1), and keeps the new order through the page its list names in data-reorder; a row that moves is draggable and has an id, data-id; a list's
// other rows stay. Other keys go on to the list's own. A constant of our own, holding no person's data.
const reorderRows = `const movable = event.target.closest ? event.target.closest('[draggable=true]') : null; ` +
	`const keep = (list, focus) => htmx.ajax('POST', list.dataset.reorder, {target: '[id=\'web.content\']', values: {ids: [...list.children].filter(row => row.dataset.id).map(row => row.dataset.id).join(',')}}).then(() => { const again = focus && document.getElementById(focus); if (again) again.focus() }); ` +
	`if (event.type === 'keydown' && event.altKey && movable && (event.key === 'ArrowUp' || event.key === 'ArrowDown')) { event.preventDefault(); ` +
	`const beside = event.key === 'ArrowUp' ? movable.previousElementSibling : movable.nextElementSibling; if (!beside || !beside.dataset.id) return; ` +
	`movable.parentElement.insertBefore(movable, event.key === 'ArrowUp' ? beside : beside.nextElementSibling); keep(movable.parentElement, event.target.id); return } ` +
	`if (event.type.startsWith('drag') || event.type === 'drop') { ` +
	`if (event.type === 'dragstart') { this.dragged = movable; if (movable) event.dataTransfer.effectAllowed = 'move'; return } ` +
	`const dragged = this.dragged; if (!dragged) return; ` +
	`if (event.type === 'dragend') { this.dragged = null; keep(dragged.parentElement, ''); return } ` +
	`event.preventDefault(); let over = event.target.closest('[data-id]'); while (over && over.parentElement !== dragged.parentElement) over = over.parentElement.closest('[data-id]'); ` +
	`if (over && over !== dragged) { const box = over.getBoundingClientRect(); dragged.parentElement.insertBefore(dragged, event.clientY < box.top + box.height / 2 ? over : over.nextElementSibling) } return } `

// sideChoice shows an item type form's types of the side chosen, as the app's choice does, and chooses the first of them when the side changes.
// A constant of our own, holding no person's data.
const sideChoice = `const box = event.target; if (box.name === 'side') this.querySelector('input[name=type][data-side=' + box.value + ']').checked = true`
