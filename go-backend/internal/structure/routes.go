// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package structure

import (
	"context"
	"database/sql"
	"errors"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Routes keeps a project's structure (UC-012, UC-013).
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// write runs work as one change to the project's structure (changes.Writer.Write).
func (s Routes) write(w http.ResponseWriter, r *http.Request, caller access.Caller, work func(change) error) bool {
	return s.writer.Write(w, r, caller, func(c changes.Change) error { return work(change{c}) })
}

// done runs work as write does and answers 200 with no body.
func (s Routes) done(w http.ResponseWriter, r *http.Request, caller access.Caller, work func(change) error) {
	s.writer.Done(w, r, caller, func(c changes.Change) error { return work(change{c}) })
}

// Map routes the structure: reading it for whoever reads the project, changing it for whoever holds ManageStructure in a project not finished
// (AC-04.1). What is no longer wanted is disabled (BR-15); only a subgroup no entry uses is removed.
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	reads := func(handler access.Handler) http.HandlerFunc { return gate.Needs(access.ReadProject, handler) }
	manage := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageStructure, handler) }
	const project = "/projects/{projectId:guid}"
	router.Handle("GET", project+"/categories", reads(s.list))
	router.Handle("GET", project+"/categories/offered", reads(s.offered))
	router.Handle("POST", project+"/categories", manage(s.addCategory))
	router.Handle("POST", project+"/categories/reorder", manage(s.reorderCategories))
	router.Handle("PUT", project+"/categories/{categoryId:guid}", manage(s.renameCategory))
	router.Handle("POST", project+"/categories/{categoryId:guid}/disable", manage(s.activateCategory(false)))
	router.Handle("POST", project+"/categories/{categoryId:guid}/enable", manage(s.activateCategory(true)))
	router.Handle("POST", project+"/categories/{categoryId:guid}/items", manage(s.addItem))
	router.Handle("POST", project+"/categories/{categoryId:guid}/items/reorder", manage(s.reorderItems))
	router.Handle("PUT", project+"/items/{itemId:guid}", manage(s.renameItem))
	router.Handle("POST", project+"/items/{itemId:guid}/disable", manage(s.activateItem(false)))
	router.Handle("POST", project+"/items/{itemId:guid}/enable", manage(s.activateItem(true)))
	router.Handle("POST", project+"/items/{itemId:guid}/subgroups", manage(s.addSubgroup))
	router.Handle("PUT", project+"/items/{itemId:guid}/type", manage(s.changeItemType))
	router.Handle("PUT", project+"/items/{itemId:guid}/table-percentages", manage(s.setTablePercentages))
	router.Handle("POST", project+"/items/{itemId:guid}/tables/reorder", manage(s.reorderTables))
	router.Handle("PUT", project+"/items/{itemId:guid}/subgroups/{subgroupId:guid}", manage(s.renameSubgroup))
	router.Handle("DELETE", project+"/items/{itemId:guid}/subgroups/{subgroupId:guid}", manage(s.deleteSubgroup))
	router.Handle("GET", project+"/items/{itemId:guid}/columns", reads(s.columnsOfItem))
	router.Handle("GET", project+"/default-columns/{itemType}", reads(s.columnsOfTemplate))
	router.Handle("POST", project+"/items/{itemId:guid}/columns", manage(s.addToItem))
	router.Handle("POST", project+"/items/{itemId:guid}/columns/reorder", manage(s.reorderOfItem))
	router.Handle("PUT", project+"/items/{itemId:guid}/columns/{columnId:guid}", manage(s.editOfItem))
	router.Handle("DELETE", project+"/items/{itemId:guid}/columns/{columnId:guid}", manage(s.deleteOfItem))
	router.Handle("POST", project+"/default-columns/{itemType}", manage(s.addToTemplate))
	router.Handle("POST", project+"/default-columns/{itemType}/reorder", manage(s.reorderOfTemplate))
	router.Handle("PUT", project+"/default-columns/{itemType}/{columnId:guid}", manage(s.editOfTemplate))
	router.Handle("DELETE", project+"/default-columns/{itemType}/{columnId:guid}", manage(s.deleteOfTemplate))
}

type reorderRequest struct {
	IDs []uuid.UUID `json:"ids"`
}

// nameRequest is the body that names a category, an item or a subgroup.
type nameRequest struct {
	Name string `json:"name"`
}

// CategoryView is a category as the app shows it (CategoryView), and the web's item choices (redix-api #170).
type CategoryView struct {
	ID       uuid.UUID  `json:"id"`
	Name     string     `json:"name"`
	IsActive bool       `json:"isActive"`
	Items    []ItemView `json:"items"`
}

// ItemView is an item as the app shows it (ItemView): its subgroups in order, the general one first, whose name is empty.
type ItemView struct {
	ID                    uuid.UUID      `json:"id"`
	Name                  string         `json:"name"`
	Type                  ItemType       `json:"type"`
	IsActive              bool           `json:"isActive"`
	Subgroups             []SubgroupView `json:"subgroups"`
	ShowsTablePercentages bool           `json:"showsTablePercentages"`
}

// SubgroupView is one of an item's subgroups, as the app shows it.
type SubgroupView struct {
	ID   uuid.UUID `json:"id"`
	Name string    `json:"name"`
}

func itemViewOf(item store.Item, subgroups []store.Subgroup) ItemView {
	var views []SubgroupView
	for _, subgroup := range subgroups {
		if subgroup.ItemID == item.ID {
			views = append(views, SubgroupView{ID: subgroup.ID, Name: subgroup.Name})
		}
	}
	return ItemView{ID: item.ID, Name: item.Name, Type: ItemType(item.Type), IsActive: item.IsActive, Subgroups: views, ShowsTablePercentages: item.ShowsTablePercentages}
}

// categories is projectID's categories, each with those of its items offers keeps, in the order of the periodic report (AC-39.1).
func categories(ctx context.Context, read *store.Queries, projectID uuid.UUID, offers func(category store.Category, item store.Item) bool) ([]CategoryView, error) {
	categories, err := read.CategoriesOf(ctx, projectID)
	items, itemsErr := read.ItemsOf(ctx, projectID)
	subgroups, subgroupsErr := read.SubgroupsOf(ctx, projectID)
	var views []CategoryView
	for _, category := range categories {
		view := CategoryView{ID: category.ID, Name: category.Name, IsActive: category.IsActive}
		for _, item := range items {
			if item.CategoryID == category.ID && offers(category, item) {
				view.Items = append(view.Items, itemViewOf(item, subgroups))
			}
		}
		views = append(views, view)
	}
	return views, errors.Join(err, itemsErr, subgroupsErr)
}

// list is every category and item of the project, disabled ones included.
func (s Routes) list(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	views, err := ListOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// ListOf is projectID's categories with every item, in the order of the periodic report, as list answers them: the rule both doors call, the
// app's route and the web's Categories and items (redix-api #174). The error is the database's.
func ListOf(ctx context.Context, read *store.Queries, projectID uuid.UUID) ([]CategoryView, error) {
	return categories(ctx, read, projectID, func(store.Category, store.Item) bool { return true })
}

// offered is what an entry of itemType may choose from: active items of that type in active categories, a category with nothing to offer left out
// (AC-05.1, AC-05.3, AC-38.1).
func (s Routes) offered(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	kind, err := itemTypes.Parse(r.URL.Query().Get("itemType"))
	if err != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return
	}
	offering, err := OfferedOf(r.Context(), s.database.Queries(), web.ID(r, "projectId"), ItemType(kind))
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, offering)
}

// OfferedOf is what an entry of kind may choose from in projectID, as offered answers it: the rule both doors call, the app's route and the web's
// item choices (redix-api #170). The error is the database's.
func OfferedOf(ctx context.Context, read *store.Queries, projectID uuid.UUID, kind ItemType) ([]CategoryView, error) {
	views, err := categories(ctx, read, projectID, func(category store.Category, item store.Item) bool {
		return category.IsActive && item.IsActive && ItemType(item.Type).OfferedFor(kind)
	})
	var offering []CategoryView
	for _, view := range views {
		if len(view.Items) > 0 {
			offering = append(offering, view)
		}
	}
	return offering, err
}

// AddCategory adds the category named name at the end of the change's project's list (FR-39, CategoriesEndpoints.AddCategoryAsync): the one way
// a category is added, by hand or by a project file's load.
func AddCategory(c changes.Change, name string) (store.Category, error) {
	categories, err := c.Q.CategoriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return store.Category{}, err
	}
	category := store.Category{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Name: name, SortOrder: int64(len(categories)), IsActive: true}
	return category, change{c}.saveCategory(store.Category{}, category)
}

// CreateCategory adds the category typed as name at the end of the change's project's list, as addCategory does: invalidName for a name not 1 to
// 100 letters. The rule both doors call, the app's route and the web's New category (redix-api #174).
func CreateCategory(c changes.Change, name string) (store.Category, error) {
	named, ok := naming.Named(name)
	if !ok {
		return store.Category{}, changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return AddCategory(c, named)
}

// addCategory adds a category as CreateCategory does: 201 with it.
func (s Routes) addCategory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request nameRequest
	if !web.Read(w, r, &request) {
		return
	}
	var category store.Category
	added := s.write(w, r, caller, func(c change) (err error) {
		category, err = CreateCategory(c.Change, request.Name)
		return err
	})
	if added {
		w.Header().Set("Location", "/projects/"+category.ProjectID.String()+"/categories/"+category.ID.String())
		web.JSON(w, r, http.StatusCreated, CategoryView{ID: category.ID, Name: category.Name, IsActive: true})
	}
}

// reorderCategories puts the project's categories in the order given, their order in the periodic report (AC-39.1): invalidOrder (400) for a
// list that is not exactly the project's categories.
func (s Routes) reorderCategories(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error { return ReorderCategories(c.Change, request.IDs) })
}

// ReorderCategories puts the change's project's categories in the order ids gives, as reorderCategories does: the rule both doors call, the app's
// route and the web's Categories and items (redix-api #174). invalidOrder (400) for ids that are not exactly the project's categories.
func ReorderCategories(c changes.Change, ids []uuid.UUID) error {
	categories, err := c.Q.CategoriesOf(c.Ctx, c.ProjectID)
	if err != nil {
		return err
	}
	return reorder(categories, ids, func(row store.Category) uuid.UUID { return row.ID },
		func(row store.Category, place int64) store.Category { row.SortOrder = place; return row }, change{c}.saveCategory)
}

// reorder saves each of rows at its place in ids, through save (AC-39.1): invalidOrder (400) for ids that are not exactly rows', by id.
func reorder[T any](rows []T, ids []uuid.UUID, id func(T) uuid.UUID, placed func(T, int64) T, save func(before, after T) error) error {
	places, ok := inOrder(idsOf(rows, id), ids)
	if !ok {
		return refused(invalidOrder)
	}
	for _, before := range rows {
		if err := save(before, placed(before, places[id(before)])); err != nil {
			return err
		}
	}
	return nil
}

func idsOf[T any](rows []T, id func(T) uuid.UUID) []uuid.UUID {
	var ids []uuid.UUID
	for _, row := range rows {
		ids = append(ids, id(row))
	}
	return ids
}

// changeCategory applies apply to categoryID in the change's project: NotFound for one the project does not hold.
func changeCategory(c changes.Change, categoryID uuid.UUID, apply func(*store.Category)) error {
	before, err := changes.Found(c.Q.CategoryOf(c.Ctx, store.CategoryOfParams{ID: categoryID, ProjectID: c.ProjectID}))
	if err != nil {
		return err
	}
	after := before
	apply(&after)
	return change{c}.saveCategory(before, after)
}

// renameCategory renames a category, refusing a name the naming rule refuses as invalidName (UC-012), as RenameCategory does.
func (s Routes) renameCategory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request nameRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return RenameCategory(c.Change, web.ID(r, "categoryId"), request.Name) })
	}
}

// RenameCategory names categoryID name, refusing a name the naming rule refuses as invalidName (UC-012): the rule both doors call, the app's
// route and the web's Categories and items (redix-api #174). NotFound for a category the project does not hold.
func RenameCategory(c changes.Change, categoryID uuid.UUID, name string) error {
	named, ok := naming.Named(name)
	if !ok {
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return changeCategory(c, categoryID, func(category *store.Category) { category.Name = named })
}

// activateCategory disables a category, so it and its items leave entry and stay in the reports (BR-15), or brings it back, as ActivateCategory does.
func (s Routes) activateCategory(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.done(w, r, caller, func(c change) error { return ActivateCategory(c.Change, web.ID(r, "categoryId"), active) })
	}
}

// ActivateCategory disables categoryID, so it and its items leave entry and stay in the reports (BR-15), or brings it back when active: the rule
// both doors call, the app's route and the web's Categories and items (redix-api #174). NotFound for a category the project does not hold.
func ActivateCategory(c changes.Change, categoryID uuid.UUID, active bool) error {
	return changeCategory(c, categoryID, func(category *store.Category) { category.IsActive = active })
}

// addItemRequest is the body POST /projects/{projectId}/categories/{categoryId}/items (UC-012) reads.
type addItemRequest struct {
	Name string   `json:"name"`
	Type ItemType `json:"type"`
}

// addItem adds an item of its type at the end of a category, with its general group and the default columns of its type (FR-05, FR-38): 404 for
// a category the project does not hold, capitalItemExists (409) for a second capital item (BR-31); 201 with it.
func (s Routes) addItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request addItemRequest
	if !web.Read(w, r, &request) {
		return
	}
	var item store.Item
	var general store.Subgroup
	added := s.write(w, r, caller, func(c change) (err error) {
		item, general, err = CreateItem(c.Change, web.ID(r, "categoryId"), request.Name, request.Type)
		return err
	})
	if added {
		w.Header().Set("Location", "/projects/"+web.ID(r, "projectId").String()+"/items/"+item.ID.String())
		web.JSON(w, r, http.StatusCreated, itemViewOf(item, []store.Subgroup{general}))
	}
}

// CreateItem adds the item typed as name of itemType at the end of categoryID, as addItem does: invalidName for a name not 1 to 100 letters, then
// the refusals of AddItem. The rule both doors call, the app's route and the web's Add item (redix-api #174).
func CreateItem(c changes.Change, categoryID uuid.UUID, name string, itemType ItemType) (store.Item, store.Subgroup, error) {
	named, ok := naming.Named(name)
	if !ok {
		return store.Item{}, store.Subgroup{}, changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return AddItem(c, categoryID, named, itemType)
}

// AddItem adds the item named name of its type at the end of the category, with its general group and the default columns of its type (FR-05,
// FR-38, CategoriesEndpoints.AddItemAsync): 404 for a category the project does not hold, capitalItemExists (409) for a second capital item
// (BR-31). The one way an item is added, by hand or by a project file's load.
func AddItem(c changes.Change, categoryID uuid.UUID, name string, itemType ItemType) (store.Item, store.Subgroup, error) {
	one := change{c}
	if _, err := changes.Found(c.Q.CategoryOf(c.Ctx, store.CategoryOfParams{ID: categoryID, ProjectID: c.ProjectID})); err != nil {
		return store.Item{}, store.Subgroup{}, err
	}
	if err := one.refuseASecondCapital(itemType); err != nil {
		return store.Item{}, store.Subgroup{}, err
	}
	items, err := c.Q.ItemsOfCategory(c.Ctx, categoryID)
	if err != nil {
		return store.Item{}, store.Subgroup{}, err
	}
	item := store.Item{ID: uuid.Must(uuid.NewV7()), CategoryID: categoryID, Name: name, Type: int64(itemType), SortOrder: int64(len(items)), IsActive: true}
	general := store.Subgroup{ID: uuid.Must(uuid.NewV7()), ItemID: item.ID, Name: generalName}
	if err := one.saveItem(store.Item{}, item); err != nil {
		return store.Item{}, store.Subgroup{}, err
	}
	if err := one.saveSubgroup(store.Subgroup{}, general); err != nil {
		return store.Item{}, store.Subgroup{}, err
	}
	return item, general, one.addTemplateColumns(item, false)
}

// refuseASecondCapital refuses capitalItemExists when itemType is the capital and the project has its capital item already: one project, one
// capital, so its total is the whole capital every reader sees (BR-31, redix-api #109).
func (c change) refuseASecondCapital(itemType ItemType) error {
	if itemType != Capital {
		return nil
	}
	exists, err := c.Q.CapitalItemExists(c.Ctx, c.ProjectID)
	if err == nil && exists {
		return refused(capitalItemExists)
	}
	return err
}

// addTemplateColumns gives item the project's default columns of its type: each at its place in the template for a new item (Item.New), or
// renumbered from the first place when the item's type changes (FR-38).
func (c change) addTemplateColumns(item store.Item, renumbered bool) error {
	defaults, err := c.Q.DefaultColumnsOfType(c.Ctx, store.DefaultColumnsOfTypeParams{ProjectID: c.ProjectID, ItemType: item.Type})
	if err != nil {
		return err
	}
	for i, row := range defaults {
		one := ofDefaultColumn(row)
		if renumbered {
			one.SortOrder = int64(i)
		}
		if err := c.saveItemColumn(store.ItemColumn{}, one.onItem(uuid.Must(uuid.NewV7()), item.ID, false)); err != nil {
			return err
		}
	}
	return nil
}

// reorderItems puts a category's items in the order given, their order in the periodic report: 404 for a category the project does not hold,
// invalidOrder (400) for a list that is not exactly its items.
func (s Routes) reorderItems(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error { return ReorderItems(c.Change, web.ID(r, "categoryId"), request.IDs) })
}

// ReorderItems puts categoryID's items in the order ids gives, as reorderItems does: the rule both doors call, the app's route and the web's
// Categories and items (redix-api #174). NotFound for a category the project does not hold; invalidOrder (400) for ids that are not exactly its
// items.
func ReorderItems(c changes.Change, categoryID uuid.UUID, ids []uuid.UUID) error {
	if _, err := changes.Found(c.Q.CategoryOf(c.Ctx, store.CategoryOfParams{ID: categoryID, ProjectID: c.ProjectID})); err != nil {
		return err
	}
	items, err := c.Q.ItemsOfCategory(c.Ctx, categoryID)
	if err != nil {
		return err
	}
	return reorder(items, ids, func(row store.Item) uuid.UUID { return row.ID },
		func(row store.Item, place int64) store.Item { row.SortOrder = place; return row }, change{c}.saveItem)
}

// itemIn is itemID in the change's project, or changes.NotFound for one it does not hold (ProjectStructure.ItemOfAsync, BR-33).
func itemIn(c changes.Change, itemID uuid.UUID) (store.Item, error) {
	return changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: itemID, ProjectID: c.ProjectID}))
}

// changeItem applies apply to itemID in the change's project: NotFound for one the project does not hold.
func changeItem(c changes.Change, itemID uuid.UUID, apply func(*store.Item)) error {
	before, err := itemIn(c, itemID)
	if err != nil {
		return err
	}
	after := before
	apply(&after)
	return change{c}.saveItem(before, after)
}

// renameItem renames an item, refusing a name the naming rule refuses as invalidName (UC-012), as RenameItem does.
func (s Routes) renameItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request nameRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return RenameItem(c.Change, web.ID(r, "itemId"), request.Name) })
	}
}

// RenameItem names itemID name, refusing a name the naming rule refuses as invalidName (UC-012): the rule both doors call, the app's route and
// the web's Categories and items (redix-api #174). NotFound for an item the project does not hold.
func RenameItem(c changes.Change, itemID uuid.UUID, name string) error {
	named, ok := naming.Named(name)
	if !ok {
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	return changeItem(c, itemID, func(item *store.Item) { item.Name = named })
}

// activateItem disables an item, so it leaves entry and stays in the reports (BR-15), or brings it back, as ActivateItem does.
func (s Routes) activateItem(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.done(w, r, caller, func(c change) error { return ActivateItem(c.Change, web.ID(r, "itemId"), active) })
	}
}

// ActivateItem disables itemID, so it leaves entry and stays in the reports (BR-15), or brings it back when active: the rule both doors call,
// the app's route and the web's Categories and items (redix-api #174). NotFound for an item the project does not hold.
func ActivateItem(c changes.Change, itemID uuid.UUID, active bool) error {
	return changeItem(c, itemID, func(item *store.Item) { item.IsActive = active })
}

// tablePercentagesRequest is the body PUT /projects/{projectId}/items/{itemId}/table-percentages (UC-012) reads.
type tablePercentagesRequest struct {
	IsOn bool `json:"isOn"`
}

// setTablePercentages sets whether the item's sheet calculates each table's percentage of the whole (AC-46.9).
func (s Routes) setTablePercentages(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request tablePercentagesRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return SetTablePercentages(c.Change, web.ID(r, "itemId"), request.IsOn) })
	}
}

// SetTablePercentages sets whether itemID's sheet calculates each table's percentage of the whole (AC-46.9): the rule both doors call, the app's
// route and the web's Categories and items (redix-api #174). NotFound for an item the project does not hold.
func SetTablePercentages(c changes.Change, itemID uuid.UUID, isOn bool) error {
	return changeItem(c, itemID, func(item *store.Item) { item.ShowsTablePercentages = isOn })
}

// addSubgroup adds a named subgroup last among the item's tables (UC-012): 404 for an item the project does not hold, itemTakesNoPurchases (400)
// for an item that takes no purchases, since a sub-table holds purchases (the owner, 7 Nov 2026).
func (s Routes) addSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request nameRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error {
			_, err := CreateSubgroup(c.Change, web.ID(r, "itemId"), request.Name)
			return err
		})
	}
}

// CreateSubgroup adds the subgroup typed as name last among itemID's tables, as addSubgroup does: invalidName for a name not 1 to 100 letters,
// NotFound for an item the project does not hold, then the refusals of AddSubgroup. The rule both doors call, the app's route and the web's New
// subgroup (redix-api #174).
func CreateSubgroup(c changes.Change, itemID uuid.UUID, name string) (uuid.UUID, error) {
	named, ok := naming.Named(name)
	if !ok {
		return uuid.Nil, changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	item, err := itemIn(c, itemID)
	if err != nil {
		return uuid.Nil, err
	}
	return AddSubgroup(c, item, named)
}

// AddSubgroup adds the subgroup named name last among the item's tables (UC-012, CategoriesEndpoints.AddSubgroupAsync): itemTakesNoPurchases
// (400) for an item that takes no purchases. The one way a subgroup is added, by hand or by a project file's load.
func AddSubgroup(c changes.Change, item store.Item, name string) (uuid.UUID, error) {
	if !ItemType(item.Type).takesPurchases() {
		return uuid.Nil, refused(itemTakesNoPurchases)
	}
	place, err := NextTablePlace(c.Ctx, c.Q, item.ID)
	if err != nil {
		return uuid.Nil, err
	}
	subgroup := store.Subgroup{ID: uuid.Must(uuid.NewV7()), ItemID: item.ID, Name: name, SortOrder: place}
	return subgroup.ID, change{c}.saveSubgroup(store.Subgroup{}, subgroup)
}

// NextTablePlace is the place after every table of the item, its subgroups, contracts and partners alike (ProjectStructure.NextTablePlaceAsync,
// AC-46.8): where a subgroup, a contract or a partner added to it goes.
func NextTablePlace(ctx context.Context, q *store.Queries, itemID uuid.UUID) (int64, error) {
	subgroups, err := q.SubgroupsOfItem(ctx, itemID)
	contracts, contractsErr := q.ContractsOfItem(ctx, itemID)
	partners, partnersErr := q.PartnersInItem(ctx, itemID)
	last := int64(-1)
	for _, subgroup := range subgroups {
		last = max(last, subgroup.SortOrder)
	}
	for _, contract := range contracts {
		last = max(last, contract.SortOrder)
	}
	for _, partner := range partners {
		last = max(last, partner.SortOrder)
	}
	return last + 1, errors.Join(err, contractsErr, partnersErr)
}

// Offers says whether entering kind in projectID offers the item itemID (ProjectStructure.OfferedItemAsync, AC-05.3, AC-38.1): an item of the
// project, active in an active category, of a type kind is offered.
func Offers(ctx context.Context, q *store.Queries, projectID, itemID uuid.UUID, kind ItemType) (bool, error) {
	item, err := q.ItemOf(ctx, store.ItemOfParams{ID: itemID, ProjectID: projectID})
	if errors.Is(err, sql.ErrNoRows) {
		return false, nil
	}
	if err != nil {
		return false, err
	}
	category, err := q.CategoryOf(ctx, store.CategoryOfParams{ID: item.CategoryID, ProjectID: projectID})
	return err == nil && category.IsActive && item.IsActive && ItemType(item.Type).OfferedFor(kind), err
}

// itemTypeRequest is the body PUT /projects/{projectId}/items/{itemId}/type (UC-012) reads.
type itemTypeRequest struct {
	Type ItemType `json:"type"`
}

// changeItemType changes what an item is for (FR-38): while no entry names it, or itemHasEntries, and no contract, or itemHasContract - though an
// item of purchases or of contractor payments may always become both (the owner, 7 Nov 2026); never an item whose tables are partners,
// itemHasPartners; never a second capital item, capitalItemExists. Its built-in columns become the project's default columns of the new type, and
// the columns a person added stay, after them.
func (s Routes) changeItemType(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request itemTypeRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error { return ChangeItemType(c.Change, web.ID(r, "itemId"), request.Type) })
}

// ChangeItemType makes itemID for to, as changeItemType does: the rule both doors call, the app's route and the web's Change type (redix-api
// #174). NotFound for an item the project does not hold, then refuseTypeChange's refusals.
func ChangeItemType(c changes.Change, itemID uuid.UUID, to ItemType) error {
	one := change{c}
	before, err := itemIn(c, itemID)
	if err != nil {
		return err
	}
	if err := one.refuseTypeChange(before, to); err != nil {
		return err
	}
	after := before
	after.Type = int64(to)
	if err := one.saveItem(before, after); err != nil {
		return err
	}
	return one.rebuildColumns(after)
}

func (c change) refuseTypeChange(item store.Item, to ItemType) error {
	from := ItemType(item.Type)
	widens := to == Both && (from == Purchases || from == ContractorPayments)
	if !widens {
		holds, err := c.Q.ItemHasEntries(c.Ctx, item.ID)
		if err != nil || holds {
			return errors.Join(err, refusalIf(holds, itemHasEntries))
		}
		contracts, err := c.Q.ContractsOfItem(c.Ctx, item.ID)
		if err != nil || len(contracts) > 0 {
			return errors.Join(err, refusalIf(len(contracts) > 0, itemHasContract))
		}
	}
	partners, err := c.Q.PartnersInItem(c.Ctx, item.ID)
	if err != nil || len(partners) > 0 {
		return errors.Join(err, refusalIf(len(partners) > 0, itemHasPartners))
	}
	if from == to {
		return nil
	}
	return c.refuseASecondCapital(to)
}

// refusalIf is code when refused, and no error otherwise.
func refusalIf(holds bool, code string) error {
	if holds {
		return refused(code)
	}
	return nil
}

// rebuildColumns gives item the default columns of its type in place of its built-in ones, keeping after them the columns a person added that the
// template does not hold.
func (c change) rebuildColumns(item store.Item) error {
	defaults, err := c.Q.DefaultColumnsOfType(c.Ctx, store.DefaultColumnsOfTypeParams{ProjectID: c.ProjectID, ItemType: item.Type})
	if err != nil {
		return err
	}
	columns, err := c.Q.ColumnsOfItem(c.Ctx, item.ID)
	if err != nil {
		return err
	}
	var kept []store.ItemColumn
	for _, row := range columns {
		inTemplate := false
		for _, template := range defaults {
			inTemplate = inTemplate || template.Key == row.Key
		}
		if ofItemColumn(row).isCustom() && !inTemplate {
			kept = append(kept, row)
			continue
		}
		if err := c.Remove("ItemColumns", row.ID, itemColumnFields(row), func() error { return c.Q.RemoveItemColumn(c.Ctx, row.ID) }); err != nil {
			return err
		}
	}
	if err := c.addTemplateColumns(item, true); err != nil {
		return err
	}
	for place, before := range kept {
		after := before
		after.SortOrder = int64(len(defaults) + place)
		if err := c.saveItemColumn(before, after); err != nil {
			return err
		}
	}
	return nil
}

// reorderTables puts all the item's tables - its subgroups, contracts and partners alike - in the order given, the order of its sheet (AC-46.8):
// invalidOrder (400) for a list that is not exactly them.
func (s Routes) reorderTables(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error { return ReorderTables(c.Change, web.ID(r, "itemId"), request.IDs) })
	}
}

// ReorderTables puts all of itemID's tables - its subgroups, contracts and partners alike - in the order ids gives, the order of its sheet, as
// reorderTables does: the rule both doors call, the app's route and the web's Tables (redix-api #174). NotFound for an item the project does not
// hold; invalidOrder (400) for ids that are not exactly them.
func ReorderTables(c changes.Change, itemID uuid.UUID, order []uuid.UUID) error {
	one := change{c}
	item, err := itemIn(c, itemID)
	if err != nil {
		return err
	}
	var subgroups []store.Subgroup
	if ItemType(item.Type).hasSubgroupTables() {
		if subgroups, err = c.Q.SubgroupsOfItem(c.Ctx, item.ID); err != nil {
			return err
		}
	}
	contracts, err := c.Q.ContractsOfItem(c.Ctx, item.ID)
	if err != nil {
		return err
	}
	partners, err := one.keptPartnersIn(item.ID)
	if err != nil {
		return err
	}
	ids := append(append(idsOf(subgroups, func(row store.Subgroup) uuid.UUID { return row.ID }),
		idsOf(contracts, func(row store.Contract) uuid.UUID { return row.ID })...), idsOf(partners, func(row store.Partner) uuid.UUID { return row.ID })...)
	places, ok := inOrder(ids, order)
	if !ok {
		return refused(invalidOrder)
	}
	for _, before := range subgroups {
		after := before
		after.SortOrder = places[before.ID]
		if err := one.saveSubgroup(before, after); err != nil {
			return err
		}
	}
	for _, contract := range contracts {
		place := places[contract.ID]
		if err := one.Save("Contracts", contract.ID, false, map[string]any{"SortOrder": contract.SortOrder}, map[string]any{"SortOrder": place},
			func() error {
				return c.Q.SetContractPlace(c.Ctx, store.SetContractPlaceParams{ID: contract.ID, SortOrder: place})
			}); err != nil {
			return err
		}
	}
	for _, partner := range partners {
		place := places[partner.ID]
		if err := one.Save("Partners", partner.ID, false, map[string]any{"SortOrder": partner.SortOrder}, map[string]any{"SortOrder": place},
			func() error {
				return c.Q.SetPartnerPlace(c.Ctx, store.SetPartnerPlaceParams{ID: partner.ID, SortOrder: place})
			}); err != nil {
			return err
		}
	}
	return nil
}

// keptPartnersIn is the partners whose tables are in the item, those deleted aside (Partner.Kept).
func (c change) keptPartnersIn(itemID uuid.UUID) ([]store.Partner, error) {
	partners, err := c.Q.PartnersInItem(c.Ctx, itemID)
	var kept []store.Partner
	for _, partner := range partners {
		if !partner.IsDeleted {
			kept = append(kept, partner)
		}
	}
	return kept, err
}

// subgroupOf is subgroupID of itemID: changes.NotFound for either not the project's; generalSubgroup (409) for the item's general group, which
// is never renamed or removed, so an entry always has a subgroup (UC-012).
func subgroupOf(c changes.Change, itemID, subgroupID uuid.UUID) (store.Subgroup, error) {
	subgroups, err := c.Q.SubgroupsOfItem(c.Ctx, itemID)
	if err != nil {
		return store.Subgroup{}, err
	}
	for _, subgroup := range subgroups {
		if subgroup.ID != subgroupID {
			continue
		}
		if _, err := itemIn(c, itemID); err != nil {
			return subgroup, err
		}
		if subgroup.Name == generalName {
			return subgroup, refused(generalSubgroup)
		}
		return subgroup, nil
	}
	return store.Subgroup{}, changes.NotFound
}

// renameSubgroup renames a subgroup other than General, refusing a name the naming rule refuses as invalidName (UC-012).
func (s Routes) renameSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request nameRequest
	if web.Read(w, r, &request) {
		s.done(w, r, caller, func(c change) error {
			return RenameSubgroup(c.Change, web.ID(r, "itemId"), web.ID(r, "subgroupId"), request.Name)
		})
	}
}

// RenameSubgroup names subgroupID of itemID name, as renameSubgroup does: the rule both doors call, the app's route and the web's Tables
// (redix-api #174). invalidName for a name not 1 to 100 letters; NotFound for either not the project's; generalSubgroup (409) for the general
// group.
func RenameSubgroup(c changes.Change, itemID, subgroupID uuid.UUID, name string) error {
	named, ok := naming.Named(name)
	if !ok {
		return changes.Refuse(http.StatusBadRequest, naming.Invalid)
	}
	before, err := subgroupOf(c, itemID, subgroupID)
	if err != nil {
		return err
	}
	after := before
	after.Name = named
	return change{c}.saveSubgroup(before, after)
}

// deleteSubgroup removes a subgroup no entry uses: subgroupInUse (409) for one an entry uses (FR-05).
func (s Routes) deleteSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.done(w, r, caller, func(c change) error { return DeleteSubgroup(c.Change, web.ID(r, "itemId"), web.ID(r, "subgroupId")) })
}

// DeleteSubgroup removes subgroupID of itemID while no entry uses it, as deleteSubgroup does: the rule both doors call, the app's route and the
// web's Tables (redix-api #174). NotFound for either not the project's; generalSubgroup or subgroupInUse (409).
func DeleteSubgroup(c changes.Change, itemID, subgroupID uuid.UUID) error {
	subgroup, err := subgroupOf(c, itemID, subgroupID)
	if err != nil {
		return err
	}
	if inUse, err := c.Q.SubgroupHasEntries(c.Ctx, subgroup.ID); err != nil || inUse {
		return errors.Join(err, refusalIf(inUse, subgroupInUse))
	}
	return change{c}.Remove("Subgroups", subgroup.ID, subgroupFields(subgroup), func() error { return c.Q.RemoveSubgroup(c.Ctx, subgroup.ID) })
}
