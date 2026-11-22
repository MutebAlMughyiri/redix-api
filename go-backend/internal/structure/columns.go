// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package structure

import (
	"context"
	"errors"
	"math"
	"net/http"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// The fields an item asks for, changed without a code change (UC-013, FR-41): on one item, or on the project's default template for one type of
// item, which reaches the existing items only when the change says applyToAllItems (BR-21).

// visible is the item's columns but those deleted, which are hidden.
func visible(columns []store.ItemColumn) []store.ItemColumn {
	var shown []store.ItemColumn
	for _, row := range columns {
		if !row.IsHidden {
			shown = append(shown, row)
		}
	}
	return shown
}

func asColumns[T any](rows []T, of func(T) column) []column {
	var columns []column
	for _, row := range rows {
		columns = append(columns, of(row))
	}
	return columns
}

// columnsOfItem is the columns an item asks for, in order; a deleted one is not among them. 404 for an item the project does not hold.
func (s Routes) columnsOfItem(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	views, err := ColumnsOfItem(r.Context(), s.database.Queries(), web.ID(r, "projectId"), web.ID(r, "itemId"))
	if errors.Is(err, changes.NotFound) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// ColumnsOfItem is the columns itemID of projectID asks for, in order, a deleted one not among them, as columnsOfItem answers them: the rule
// both doors call, the app's route and the web's Columns (redix-api #174). changes.NotFound for an item the project does not hold; any other
// error is the database's.
func ColumnsOfItem(ctx context.Context, read *store.Queries, projectID, itemID uuid.UUID) ([]ColumnView, error) {
	item, err := changes.Found(read.ItemOf(ctx, store.ItemOfParams{ID: itemID, ProjectID: projectID}))
	if err != nil {
		return nil, err
	}
	columns, err := read.ColumnsOfItem(ctx, item.ID)
	if err != nil {
		return nil, err
	}
	var views []ColumnView
	for _, row := range visible(columns) {
		views = append(views, ColumnOf(row))
	}
	return views, nil
}

// itemTypeOf is the route's item type, or false once it has answered 400 for a name or number that is none (ItemTypeInUrl).
func itemTypeOf(w http.ResponseWriter, r *http.Request) (ItemType, bool) {
	value, err := itemTypes.Parse(r.PathValue("itemType"))
	if err != nil {
		problem.Status(w, r, http.StatusBadRequest)
	}
	return ItemType(value), err == nil
}

// columnsOfTemplate is the project's default template for one type of item, in order.
func (s Routes) columnsOfTemplate(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	itemType, ok := itemTypeOf(w, r)
	if !ok {
		return
	}
	views, err := ColumnsOfTemplate(r.Context(), s.database.Queries(), web.ID(r, "projectId"), itemType)
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// ColumnsOfTemplate is projectID's default template for itemType, in order, as columnsOfTemplate answers it: the rule both doors call, the app's
// route and the web's Columns (redix-api #174). The error is the database's.
func ColumnsOfTemplate(ctx context.Context, read *store.Queries, projectID uuid.UUID, itemType ItemType) ([]ColumnView, error) {
	template, err := read.DefaultColumnsOfType(ctx, store.DefaultColumnsOfTypeParams{ProjectID: projectID, ItemType: int64(itemType)})
	if err != nil {
		return nil, err
	}
	var views []ColumnView
	for _, row := range template {
		views = append(views, ofDefaultColumn(row).view(row.ID))
	}
	return views, nil
}

// addToItem adds a column to one item, at the end (AC-41.1): invalidName, invalidDataType or invalidOptions (400) for what was typed; 404 for an
// item the project does not hold; revenueTakesNoColumns (400) on a revenue item, which asks only what its template gives it; signedListExists
// (400) for a second list whose choices give a sign (redix-api #129).
func (s Routes) addToItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request ColumnRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return AddToItem(c.Change, web.ID(r, "itemId"), request) })
	}
}

// AddToItem adds the column typed to itemID, at the end, as addToItem does: the rule both doors call, the app's route and the web's New column
// (redix-api #174). Refused as addToItem answers.
func AddToItem(c changes.Change, itemID uuid.UUID, typed ColumnRequest) error {
	if reason := typed.refusalOfNew(); reason != "" {
		return refused(reason)
	}
	one := change{c}
	item, err := itemIn(c, itemID)
	if err != nil {
		return err
	}
	if ItemType(item.Type).IsRevenue() {
		return refused(revenueTakesNoColumns)
	}
	columns, err := c.Q.ColumnsOfItem(c.Ctx, item.ID)
	if err != nil {
		return err
	}
	added := typed.newColumn(int64(len(columns)))
	if hasTwoSignedLists(append(asColumns(visible(columns), ofItemColumn), added)) {
		return refused(signedListExists)
	}
	return one.saveItemColumn(store.ItemColumn{}, added.onItem(uuid.Must(uuid.NewV7()), item.ID, false))
}

// reorderOfItem puts an item's columns in the order given, a core one too (BR-20): invalidOrder (400) for a list that is not exactly its columns.
func (s Routes) reorderOfItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return ReorderItemColumns(c.Change, web.ID(r, "itemId"), request.IDs) })
	}
}

// ReorderItemColumns puts itemID's columns in the order ids gives, a core one too (BR-20), as reorderOfItem does: the rule both doors call, the
// app's route and the web's Columns (redix-api #174). NotFound for an item the project does not hold; invalidOrder (400) for ids that are not
// exactly its columns.
func ReorderItemColumns(c changes.Change, itemID uuid.UUID, ids []uuid.UUID) error {
	one := change{c}
	item, err := itemIn(c, itemID)
	if err != nil {
		return err
	}
	columns, err := c.Q.ColumnsOfItem(c.Ctx, item.ID)
	if err != nil {
		return err
	}
	shown := visible(columns)
	places, ok := inOrder(idsOf(shown, func(row store.ItemColumn) uuid.UUID { return row.ID }), ids)
	if !ok {
		return refused(invalidOrder)
	}
	for _, before := range shown {
		after := before
		after.SortOrder = places[before.ID]
		if err := one.saveItemColumn(before, after); err != nil {
			return err
		}
	}
	return nil
}

// itemColumn is columnID of itemID, and the item's shown columns: changes.NotFound for a column deleted or not the project's.
func itemColumn(c changes.Change, itemID, columnID uuid.UUID) (store.ItemColumn, []store.ItemColumn, error) {
	columns, err := c.Q.ColumnsOfItem(c.Ctx, itemID)
	if err != nil {
		return store.ItemColumn{}, nil, err
	}
	shown := visible(columns)
	at := slices.IndexFunc(shown, func(row store.ItemColumn) bool { return row.ID == columnID })
	if at < 0 {
		return store.ItemColumn{}, nil, changes.NotFound
	}
	if _, err := itemIn(c, itemID); err != nil {
		return store.ItemColumn{}, nil, err
	}
	return shown[at], shown, nil
}

// editOfItem changes a column of one item (BR-20, BR-21): its type never changes, and a list's choices are only added to; signedListExists (400)
// for a sign on a second list of the item (redix-api #129).
func (s Routes) editOfItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request EditRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error {
			return EditItemColumn(c.Change, web.ID(r, "itemId"), web.ID(r, "columnId"), request)
		})
	}
}

// EditItemColumn makes columnID of itemID as typed, as editOfItem does: the rule both doors call, the app's route and the web's Edit column
// (redix-api #174). NotFound for a column deleted or not the project's; refused as editOfItem answers.
func EditItemColumn(c changes.Change, itemID, columnID uuid.UUID, typed EditRequest) error {
	before, shown, err := itemColumn(c, itemID, columnID)
	if err != nil {
		return err
	}
	edited, reason := ofItemColumn(before).edited(typed)
	if reason != "" {
		return refused(reason)
	}
	after := edited.onItem(before.ID, before.ItemID, false)
	if hasTwoSignedLists(asColumns(replaced(shown, after), ofItemColumn)) {
		return refused(signedListExists)
	}
	return change{c}.saveItemColumn(before, after)
}

// replaced is columns with row in place of the one of its id.
func replaced(columns []store.ItemColumn, row store.ItemColumn) []store.ItemColumn {
	kept := slices.Clone(columns)
	for i := range kept {
		if kept[i].ID == row.ID {
			kept[i] = row
		}
	}
	return kept
}

// deleteOfItem deletes a column of one item by hiding it: it leaves entry, and its values stay on their entries and in History (AC-41.2, BR-21).
// coreColumn (409) for a core column, which is never deleted (BR-20).
func (s Routes) deleteOfItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.done(w, r, caller, func(c change) error { return DeleteItemColumn(c.Change, web.ID(r, "itemId"), web.ID(r, "columnId")) })
}

// DeleteItemColumn deletes columnID of itemID by hiding it, as deleteOfItem does: the rule both doors call, the app's route and the web's Delete
// column (redix-api #174). NotFound for a column deleted or not the project's; coreColumn (409) for a core one.
func DeleteItemColumn(c changes.Change, itemID, columnID uuid.UUID) error {
	before, _, err := itemColumn(c, itemID, columnID)
	if err != nil {
		return err
	}
	return change{c}.hide(before)
}

// hide deletes an item's column by hiding it (ItemColumn.Hide): coreColumn for a core one.
func (c change) hide(before store.ItemColumn) error {
	if before.IsCore {
		return refused(coreColumn)
	}
	after := before
	after.IsHidden = true
	return c.saveItemColumn(before, after)
}

// template is the route project's default columns of itemType.
func (c change) template(itemType ItemType) ([]store.DefaultColumn, error) {
	return c.Q.DefaultColumnsOfType(c.Ctx, store.DefaultColumnsOfTypeParams{ProjectID: c.ProjectID, ItemType: int64(itemType)})
}

// columnsOfType is the shown columns of the project's items of itemType, by item.
func (c change) columnsOfType(itemType ItemType) (map[uuid.UUID][]store.ItemColumn, error) {
	items, err := c.Q.ItemsOf(c.Ctx, c.ProjectID)
	if err != nil {
		return nil, err
	}
	columns, err := c.Q.ColumnsOf(c.Ctx, c.ProjectID)
	byItem := map[uuid.UUID][]store.ItemColumn{}
	for _, item := range items {
		if ItemType(item.Type) == itemType {
			byItem[item.ID] = []store.ItemColumn{}
		}
	}
	for _, row := range columns {
		if shown, ofType := byItem[row.ItemID]; ofType {
			byItem[row.ItemID] = append(shown, row)
		}
	}
	return byItem, err
}

// addTemplateColumnRequest is the body POST /projects/{projectId}/default-columns/{itemType} (UC-013) reads.
type addTemplateColumnRequest struct {
	ColumnRequest
	ApplyToAllItems bool `json:"applyToAllItems"`
}

// addToTemplate adds a column to the default template, so new items of this type start with it, and to every existing one with applyToAllItems
// (AC-41.3); refused as on an item.
func (s Routes) addToTemplate(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	itemType, ok := itemTypeOf(w, r)
	if !ok {
		return
	}
	var request addTemplateColumnRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error {
			return AddToTemplate(c.Change, itemType, request.ColumnRequest, request.ApplyToAllItems)
		})
	}
}

// AddToTemplate adds the column typed to the change's project's template of itemType, and to every existing item of that type when applyToAll,
// as addToTemplate does: the rule both doors call, the app's route and the web's New column (redix-api #174). Refused as addToTemplate answers.
func AddToTemplate(c changes.Change, itemType ItemType, typed ColumnRequest, applyToAll bool) error {
	reason := typed.refusalOfNew()
	if itemType.IsRevenue() {
		reason = revenueTakesNoColumns
	}
	if reason != "" {
		return refused(reason)
	}
	one := change{c}
	template, err := one.template(itemType)
	if err != nil {
		return err
	}
	added := typed.newColumn(int64(len(template)))
	if hasTwoSignedLists(append(asColumns(template, ofDefaultColumn), added)) {
		return refused(signedListExists)
	}
	if err := one.saveDefaultColumn(store.DefaultColumn{}, added.inTemplate(uuid.Must(uuid.NewV7()), c.ProjectID, int64(itemType))); err != nil {
		return err
	}
	if !applyToAll {
		return nil
	}
	byItem, err := one.columnsOfType(itemType)
	if err != nil {
		return err
	}
	for itemID, columns := range byItem {
		onItem := added
		onItem.SortOrder = int64(len(columns))
		if hasTwoSignedLists(append(asColumns(visible(columns), ofItemColumn), onItem)) {
			return refused(signedListExists)
		}
		if err := one.saveItemColumn(store.ItemColumn{}, onItem.onItem(uuid.Must(uuid.NewV7()), itemID, false)); err != nil {
			return err
		}
	}
	return nil
}

// reorderTemplateColumnsRequest is the body POST /projects/{projectId}/default-columns/{itemType}/reorder (UC-013) reads.
type reorderTemplateColumnsRequest struct {
	reorderRequest
	ApplyToAllItems bool `json:"applyToAllItems"`
}

// reorderOfTemplate puts the default template in the order given, and with applyToAllItems every existing item of this type follows it, what only
// that item has after the rest (AC-41.3): invalidOrder (400) for a list that is not exactly its columns.
func (s Routes) reorderOfTemplate(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	itemType, ok := itemTypeOf(w, r)
	if !ok {
		return
	}
	var request reorderTemplateColumnsRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return ReorderTemplate(c.Change, itemType, request.IDs, request.ApplyToAllItems) })
	}
}

// ReorderTemplate puts the change's project's template of itemType in the order ids gives, and every existing item of that type after it when
// applyToAll, what only that item has after the rest, as reorderOfTemplate does: the rule both doors call, the app's route and the web's Columns
// (redix-api #174). invalidOrder (400) for ids that are not exactly the template's columns.
func ReorderTemplate(c changes.Change, itemType ItemType, ids []uuid.UUID, applyToAll bool) error {
	one := change{c}
	template, err := one.template(itemType)
	if err != nil {
		return err
	}
	places, ok := inOrder(idsOf(template, func(row store.DefaultColumn) uuid.UUID { return row.ID }), ids)
	if !ok {
		return refused(invalidOrder)
	}
	placeOfKey := map[string]int64{}
	for _, before := range template {
		after := before
		after.SortOrder = places[before.ID]
		placeOfKey[before.Key] = after.SortOrder
		if err := one.saveDefaultColumn(before, after); err != nil {
			return err
		}
	}
	if !applyToAll {
		return nil
	}
	byItem, err := one.columnsOfType(itemType)
	if err != nil {
		return err
	}
	for _, columns := range byItem {
		ordered := visible(columns)
		slices.SortStableFunc(ordered, func(a, b store.ItemColumn) int {
			if placeA, placeB := placeOr(placeOfKey, a.Key), placeOr(placeOfKey, b.Key); placeA != placeB {
				return compare(placeA, placeB)
			}
			return compare(a.SortOrder, b.SortOrder)
		})
		for place, before := range ordered {
			after := before
			after.SortOrder = int64(place)
			if err := one.saveItemColumn(before, after); err != nil {
				return err
			}
		}
	}
	return nil
}

func placeOr(places map[string]int64, key string) int64 {
	if place, found := places[key]; found {
		return place
	}
	return math.MaxInt64
}

func compare(a, b int64) int {
	switch {
	case a < b:
		return -1
	case a > b:
		return 1
	}
	return 0
}

// templateColumn is columnID of the template of itemType, and the whole template: changes.NotFound for none.
func (c change) templateColumn(itemType ItemType, columnID uuid.UUID) (store.DefaultColumn, []store.DefaultColumn, error) {
	template, err := c.template(itemType)
	if err != nil {
		return store.DefaultColumn{}, nil, err
	}
	at := slices.IndexFunc(template, func(row store.DefaultColumn) bool { return row.ID == columnID })
	if at < 0 {
		return store.DefaultColumn{}, nil, changes.NotFound
	}
	return template[at], template, nil
}

// editTemplateColumnRequest is the body PUT /projects/{projectId}/default-columns/{itemType}/{columnId} (UC-013) reads.
type editTemplateColumnRequest struct {
	EditRequest
	ApplyToAllItems bool `json:"applyToAllItems"`
}

// editOfTemplate changes a column of the default template, and with applyToAllItems the same column on every existing item of this type
// (AC-41.3); refused as on an item.
func (s Routes) editOfTemplate(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	itemType, ok := itemTypeOf(w, r)
	if !ok {
		return
	}
	var request editTemplateColumnRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error {
			return EditTemplateColumn(c.Change, itemType, web.ID(r, "columnId"), request.EditRequest, request.ApplyToAllItems)
		})
	}
}

// EditTemplateColumn makes columnID of the template of itemType as typed, and the same column on every existing item of that type when
// applyToAll, as editOfTemplate does: the rule both doors call, the app's route and the web's Edit column (redix-api #174). NotFound for no such
// column; refused as editOfTemplate answers.
func EditTemplateColumn(c changes.Change, itemType ItemType, columnID uuid.UUID, typed EditRequest, applyToAll bool) error {
	one := change{c}
	before, template, err := one.templateColumn(itemType, columnID)
	if err != nil {
		return err
	}
	edited, reason := ofDefaultColumn(before).edited(typed)
	if reason != "" {
		return refused(reason)
	}
	after := edited.inTemplate(before.ID, before.ProjectID, before.ItemType)
	edits := asColumns(template, ofDefaultColumn)
	edits[slices.IndexFunc(template, func(row store.DefaultColumn) bool { return row.ID == before.ID })] = edited
	if hasTwoSignedLists(edits) {
		return refused(signedListExists)
	}
	if err := one.saveDefaultColumn(before, after); err != nil {
		return err
	}
	if !applyToAll {
		return nil
	}
	return one.editOnItems(itemType, before.Key, typed)
}

// editOnItems makes typed of the column key on every shown column of the project's items of itemType; a column its own rules refuse it to stays
// as it is, as Column.Edit left it.
func (c change) editOnItems(itemType ItemType, key string, typed EditRequest) error {
	byItem, err := c.columnsOfType(itemType)
	if err != nil {
		return err
	}
	for _, columns := range byItem {
		shown := visible(columns)
		var changed []store.ItemColumn
		for i, row := range shown {
			if row.Key != key {
				continue
			}
			if edited, reason := ofItemColumn(row).edited(typed); reason == "" {
				shown[i] = edited.onItem(row.ID, row.ItemID, false)
				changed = append(changed, row)
			}
		}
		if hasTwoSignedLists(asColumns(shown, ofItemColumn)) {
			return refused(signedListExists)
		}
		for _, before := range changed {
			if err := c.saveItemColumn(before, shown[slices.IndexFunc(shown, func(row store.ItemColumn) bool { return row.ID == before.ID })]); err != nil {
				return err
			}
		}
	}
	return nil
}

// deleteOfTemplate deletes a column of the default template, so new items no longer start with it, and with applyToAllItems - sent in the query,
// since a delete has no body (redix-api #98) - hides it on every existing item of this type (AC-41.3). coreColumn (409) for a core column.
func (s Routes) deleteOfTemplate(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	itemType, ok := itemTypeOf(w, r)
	if !ok {
		return
	}
	applyToAllItems, given := boolean(r.URL.Query().Get("applyToAllItems"))
	if !given {
		problem.Status(w, r, http.StatusBadRequest)
		return
	}
	s.done(w, r, caller, func(c change) error {
		return DeleteTemplateColumn(c.Change, itemType, web.ID(r, "columnId"), applyToAllItems)
	})
}

// DeleteTemplateColumn deletes columnID of the template of itemType, and hides it on every existing item of that type when applyToAll, as
// deleteOfTemplate does: the rule both doors call, the app's route and the web's Delete column (redix-api #174). NotFound for no such column;
// coreColumn (409) for a core one.
func DeleteTemplateColumn(c changes.Change, itemType ItemType, columnID uuid.UUID, applyToAll bool) error {
	one := change{c}
	column, _, err := one.templateColumn(itemType, columnID)
	if err != nil {
		return err
	}
	if column.IsCore {
		return refused(coreColumn)
	}
	if err := one.Remove("DefaultColumns", column.ID, defaultColumnFields(column), func() error { return c.Q.RemoveDefaultColumn(c.Ctx, column.ID) }); err != nil {
		return err
	}
	if !applyToAll {
		return nil
	}
	byItem, err := one.columnsOfType(itemType)
	if err != nil {
		return err
	}
	for _, columns := range byItem {
		for _, row := range visible(columns) {
			if row.Key == column.Key && !row.IsCore {
				if err := one.hide(row); err != nil {
					return err
				}
			}
		}
	}
	return nil
}

// boolean is a query value as bool.TryParse read it: true or false in any case; false and false for anything else or nothing.
func boolean(value string) (bool, bool) {
	value = strings.TrimSpace(value)
	return strings.EqualFold(value, "true"), strings.EqualFold(value, "true") || strings.EqualFold(value, "false")
}
