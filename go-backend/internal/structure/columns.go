package structure

import (
	"errors"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"math"
	"net/http"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
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
	columns := []column{}
	for _, row := range rows {
		columns = append(columns, of(row))
	}
	return columns
}

// columnsOfItem is the columns an item asks for, in order; a deleted one is not among them. 404 for an item the project does not hold.
func (s Routes) columnsOfItem(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	read := s.database.Queries()
	item, err := changes.Found(read.ItemOf(r.Context(), store.ItemOfParams{ID: web.ID(r, "itemId"), ProjectID: web.ID(r, "projectId")}))
	if errors.Is(err, changes.NotFound) {
		problem.Status(w, r, http.StatusNotFound)
		return
	}
	columns, columnsErr := read.ColumnsOfItem(r.Context(), item.ID)
	if err != nil || columnsErr != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []columnView{}
	for _, row := range visible(columns) {
		views = append(views, ofItemColumn(row).view(row.ID))
	}
	web.JSON(w, r, http.StatusOK, views)
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
	template, err := s.database.Queries().DefaultColumnsOfType(r.Context(), store.DefaultColumnsOfTypeParams{ProjectID: web.ID(r, "projectId"), ItemType: int64(itemType)})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	views := []columnView{}
	for _, row := range template {
		views = append(views, ofDefaultColumn(row).view(row.ID))
	}
	web.JSON(w, r, http.StatusOK, views)
}

// addToItem adds a column to one item, at the end (AC-41.1): invalidName, invalidDataType or invalidOptions (400) for what was typed; 404 for an
// item the project does not hold; revenueTakesNoColumns (400) on a revenue item, which asks only what its template gives it; signedListExists
// (400) for a second list whose choices give a sign (redix-api #129).
func (s Routes) addToItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request columnRequest
	if !web.Read(w, r, &request) {
		return
	}
	if reason := request.refusalOfNew(); reason != "" {
		refuse(w, r, reason)
		return
	}
	s.done(w, r, caller, func(c change) error {
		item, err := c.item(r)
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
		added := request.newColumn(int64(len(columns)))
		if hasTwoSignedLists(append(asColumns(visible(columns), ofItemColumn), added)) {
			return refused(signedListExists)
		}
		return c.saveItemColumn(store.ItemColumn{}, added.onItem(uuid.Must(uuid.NewV7()), item.ID, false))
	})
}

// reorderOfItem puts an item's columns in the order given, a core one too (BR-20): invalidOrder (400) for a list that is not exactly its columns.
func (s Routes) reorderOfItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		item, err := c.item(r)
		if err != nil {
			return err
		}
		columns, err := c.Q.ColumnsOfItem(c.Ctx, item.ID)
		if err != nil {
			return err
		}
		shown := visible(columns)
		places, ok := inOrder(idsOf(shown, func(row store.ItemColumn) uuid.UUID { return row.ID }), request.IDs)
		if !ok {
			return refused(invalidOrder)
		}
		for _, before := range shown {
			after := before
			after.SortOrder = places[before.ID]
			if err := c.saveItemColumn(before, after); err != nil {
				return err
			}
		}
		return nil
	})
}

// itemColumn is the route's column of the route's item, and the item's shown columns: changes.NotFound for a column deleted or not the project's.
func (c change) itemColumn(r *http.Request) (store.ItemColumn, []store.ItemColumn, error) {
	columns, err := c.Q.ColumnsOfItem(c.Ctx, web.ID(r, "itemId"))
	if err != nil {
		return store.ItemColumn{}, nil, err
	}
	shown := visible(columns)
	at := slices.IndexFunc(shown, func(row store.ItemColumn) bool { return row.ID == web.ID(r, "columnId") })
	if at < 0 {
		return store.ItemColumn{}, nil, changes.NotFound
	}
	if _, err := c.item(r); err != nil {
		return store.ItemColumn{}, nil, err
	}
	return shown[at], shown, nil
}

// editOfItem changes a column of one item (BR-20, BR-21): its type never changes, and a list's choices are only added to; signedListExists (400)
// for a sign on a second list of the item (redix-api #129).
func (s Routes) editOfItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request editRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		before, shown, err := c.itemColumn(r)
		if err != nil {
			return err
		}
		edited, reason := ofItemColumn(before).edited(request)
		if reason != "" {
			return refused(reason)
		}
		after := edited.onItem(before.ID, before.ItemID, false)
		if hasTwoSignedLists(asColumns(replaced(shown, after), ofItemColumn)) {
			return refused(signedListExists)
		}
		return c.saveItemColumn(before, after)
	})
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
	s.done(w, r, caller, func(c change) error {
		before, _, err := c.itemColumn(r)
		if err != nil {
			return err
		}
		return c.hide(before)
	})
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
	columnRequest
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
	if !web.Read(w, r, &request) {
		return
	}
	reason := request.refusalOfNew()
	if itemType.IsRevenue() {
		reason = revenueTakesNoColumns
	}
	if reason != "" {
		refuse(w, r, reason)
		return
	}
	s.done(w, r, caller, func(c change) error {
		template, err := c.template(itemType)
		if err != nil {
			return err
		}
		added := request.newColumn(int64(len(template)))
		if hasTwoSignedLists(append(asColumns(template, ofDefaultColumn), added)) {
			return refused(signedListExists)
		}
		if err := c.saveDefaultColumn(store.DefaultColumn{}, added.inTemplate(uuid.Must(uuid.NewV7()), c.ProjectID, int64(itemType))); err != nil {
			return err
		}
		if !request.ApplyToAllItems {
			return nil
		}
		byItem, err := c.columnsOfType(itemType)
		if err != nil {
			return err
		}
		for itemID, columns := range byItem {
			onItem := added
			onItem.SortOrder = int64(len(columns))
			if hasTwoSignedLists(append(asColumns(visible(columns), ofItemColumn), onItem)) {
				return refused(signedListExists)
			}
			if err := c.saveItemColumn(store.ItemColumn{}, onItem.onItem(uuid.Must(uuid.NewV7()), itemID, false)); err != nil {
				return err
			}
		}
		return nil
	})
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
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		template, err := c.template(itemType)
		if err != nil {
			return err
		}
		places, ok := inOrder(idsOf(template, func(row store.DefaultColumn) uuid.UUID { return row.ID }), request.IDs)
		if !ok {
			return refused(invalidOrder)
		}
		placeOfKey := map[string]int64{}
		for _, before := range template {
			after := before
			after.SortOrder = places[before.ID]
			placeOfKey[before.Key] = after.SortOrder
			if err := c.saveDefaultColumn(before, after); err != nil {
				return err
			}
		}
		if !request.ApplyToAllItems {
			return nil
		}
		byItem, err := c.columnsOfType(itemType)
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
				if err := c.saveItemColumn(before, after); err != nil {
					return err
				}
			}
		}
		return nil
	})
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

// templateColumn is the route's column of the template of itemType, and the whole template: changes.NotFound for none.
func (c change) templateColumn(r *http.Request, itemType ItemType) (store.DefaultColumn, []store.DefaultColumn, error) {
	template, err := c.template(itemType)
	if err != nil {
		return store.DefaultColumn{}, nil, err
	}
	at := slices.IndexFunc(template, func(row store.DefaultColumn) bool { return row.ID == web.ID(r, "columnId") })
	if at < 0 {
		return store.DefaultColumn{}, nil, changes.NotFound
	}
	return template[at], template, nil
}

// editTemplateColumnRequest is the body PUT /projects/{projectId}/default-columns/{itemType}/{columnId} (UC-013) reads.
type editTemplateColumnRequest struct {
	editRequest
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
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		before, template, err := c.templateColumn(r, itemType)
		if err != nil {
			return err
		}
		edited, reason := ofDefaultColumn(before).edited(request.editRequest)
		if reason != "" {
			return refused(reason)
		}
		after := edited.inTemplate(before.ID, before.ProjectID, before.ItemType)
		edits := asColumns(template, ofDefaultColumn)
		edits[slices.IndexFunc(template, func(row store.DefaultColumn) bool { return row.ID == before.ID })] = edited
		if hasTwoSignedLists(edits) {
			return refused(signedListExists)
		}
		if err := c.saveDefaultColumn(before, after); err != nil {
			return err
		}
		if !request.ApplyToAllItems {
			return nil
		}
		return c.editOnItems(itemType, before.Key, request.editRequest)
	})
}

// editOnItems makes typed of the column key on every shown column of the project's items of itemType; a column its own rules refuse it to stays
// as it is, as Column.Edit left it.
func (c change) editOnItems(itemType ItemType, key string, typed editRequest) error {
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
		column, _, err := c.templateColumn(r, itemType)
		if err != nil {
			return err
		}
		if column.IsCore {
			return refused(coreColumn)
		}
		if err := c.Remove("DefaultColumns", column.ID, defaultColumnFields(column), func() error { return c.Q.RemoveDefaultColumn(c.Ctx, column.ID) }); err != nil {
			return err
		}
		if !applyToAllItems {
			return nil
		}
		byItem, err := c.columnsOfType(itemType)
		if err != nil {
			return err
		}
		for _, columns := range byItem {
			for _, row := range visible(columns) {
				if row.Key == column.Key && !row.IsCore {
					if err := c.hide(row); err != nil {
						return err
					}
				}
			}
		}
		return nil
	})
}

// boolean is a query value as bool.TryParse read it: true or false in any case; false and false for anything else or nothing.
func boolean(value string) (bool, bool) {
	value = strings.TrimSpace(value)
	return strings.EqualFold(value, "true"), strings.EqualFold(value, "true") || strings.EqualFold(value, "false")
}
