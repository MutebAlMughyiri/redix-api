package structure

import (
	"context"
	"database/sql"
	"errors"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
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
	changes := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageStructure, handler) }
	const project = "/projects/{projectId:guid}"
	router.Handle("GET", project+"/categories", reads(s.list))
	router.Handle("GET", project+"/categories/offered", reads(s.offered))
	router.Handle("POST", project+"/categories", changes(s.addCategory))
	router.Handle("POST", project+"/categories/reorder", changes(s.reorderCategories))
	router.Handle("PUT", project+"/categories/{categoryId:guid}", changes(s.renameCategory))
	router.Handle("POST", project+"/categories/{categoryId:guid}/disable", changes(s.activateCategory(false)))
	router.Handle("POST", project+"/categories/{categoryId:guid}/enable", changes(s.activateCategory(true)))
	router.Handle("POST", project+"/categories/{categoryId:guid}/items", changes(s.addItem))
	router.Handle("POST", project+"/categories/{categoryId:guid}/items/reorder", changes(s.reorderItems))
	router.Handle("PUT", project+"/items/{itemId:guid}", changes(s.renameItem))
	router.Handle("POST", project+"/items/{itemId:guid}/disable", changes(s.activateItem(false)))
	router.Handle("POST", project+"/items/{itemId:guid}/enable", changes(s.activateItem(true)))
	router.Handle("POST", project+"/items/{itemId:guid}/subgroups", changes(s.addSubgroup))
	router.Handle("PUT", project+"/items/{itemId:guid}/type", changes(s.changeItemType))
	router.Handle("PUT", project+"/items/{itemId:guid}/table-percentages", changes(s.setTablePercentages))
	router.Handle("POST", project+"/items/{itemId:guid}/tables/reorder", changes(s.reorderTables))
	router.Handle("PUT", project+"/items/{itemId:guid}/subgroups/{subgroupId:guid}", changes(s.renameSubgroup))
	router.Handle("DELETE", project+"/items/{itemId:guid}/subgroups/{subgroupId:guid}", changes(s.deleteSubgroup))
	router.Handle("GET", project+"/items/{itemId:guid}/columns", reads(s.columnsOfItem))
	router.Handle("GET", project+"/default-columns/{itemType}", reads(s.columnsOfTemplate))
	router.Handle("POST", project+"/items/{itemId:guid}/columns", changes(s.addToItem))
	router.Handle("POST", project+"/items/{itemId:guid}/columns/reorder", changes(s.reorderOfItem))
	router.Handle("PUT", project+"/items/{itemId:guid}/columns/{columnId:guid}", changes(s.editOfItem))
	router.Handle("DELETE", project+"/items/{itemId:guid}/columns/{columnId:guid}", changes(s.deleteOfItem))
	router.Handle("POST", project+"/default-columns/{itemType}", changes(s.addToTemplate))
	router.Handle("POST", project+"/default-columns/{itemType}/reorder", changes(s.reorderOfTemplate))
	router.Handle("PUT", project+"/default-columns/{itemType}/{columnId:guid}", changes(s.editOfTemplate))
	router.Handle("DELETE", project+"/default-columns/{itemType}/{columnId:guid}", changes(s.deleteOfTemplate))
}

type reorderRequest struct {
	IDs []uuid.UUID `json:"ids"`
}

// categoryView is a category as the app shows it (CategoryView).
type categoryView struct {
	ID       uuid.UUID  `json:"id"`
	Name     string     `json:"name"`
	IsActive bool       `json:"isActive"`
	Items    []itemView `json:"items"`
}

// itemView is an item as the app shows it (ItemView): its subgroups in order, the general one first, whose name is empty.
type itemView struct {
	ID                    uuid.UUID      `json:"id"`
	Name                  string         `json:"name"`
	Type                  ItemType       `json:"type"`
	IsActive              bool           `json:"isActive"`
	Subgroups             []subgroupView `json:"subgroups"`
	ShowsTablePercentages bool           `json:"showsTablePercentages"`
}

type subgroupView struct {
	ID   uuid.UUID `json:"id"`
	Name string    `json:"name"`
}

func itemViewOf(item store.Item, subgroups []store.Subgroup) itemView {
	views := []subgroupView{}
	for _, subgroup := range subgroups {
		if subgroup.ItemID == item.ID {
			views = append(views, subgroupView{ID: subgroup.ID, Name: subgroup.Name})
		}
	}
	return itemView{ID: item.ID, Name: item.Name, Type: ItemType(item.Type), IsActive: item.IsActive, Subgroups: views, ShowsTablePercentages: item.ShowsTablePercentages}
}

// categories is the route project's categories, each with those of its items offers keeps, in the order of the periodic report (AC-39.1).
func (s Routes) categories(r *http.Request, offers func(category store.Category, item store.Item) bool) ([]categoryView, error) {
	read, projectID := s.database.Queries(), web.ID(r, "projectId")
	categories, err := read.CategoriesOf(r.Context(), projectID)
	items, itemsErr := read.ItemsOf(r.Context(), projectID)
	subgroups, subgroupsErr := read.SubgroupsOf(r.Context(), projectID)
	views := []categoryView{}
	for _, category := range categories {
		view := categoryView{ID: category.ID, Name: category.Name, IsActive: category.IsActive, Items: []itemView{}}
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
	views, err := s.categories(r, func(store.Category, store.Item) bool { return true })
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	web.JSON(w, r, http.StatusOK, views)
}

// offered is what an entry of itemType may choose from: active items of that type in active categories, a category with nothing to offer left out
// (AC-05.1, AC-05.3, AC-38.1).
func (s Routes) offered(w http.ResponseWriter, r *http.Request, _ access.Caller) {
	kind, err := itemTypes.Parse(r.URL.Query().Get("itemType"))
	if err != nil {
		problem.Status(w, r, http.StatusBadRequest)
		return
	}
	views, err := s.categories(r, func(category store.Category, item store.Item) bool {
		return category.IsActive && item.IsActive && ItemType(item.Type).OfferedFor(ItemType(kind))
	})
	if err != nil {
		problem.Status(w, r, http.StatusInternalServerError)
		return
	}
	offering := []categoryView{}
	for _, view := range views {
		if len(view.Items) > 0 {
			offering = append(offering, view)
		}
	}
	web.JSON(w, r, http.StatusOK, offering)
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

// addCategory adds a category as AddCategory does: 201 with it.
func (s Routes) addCategory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, ok := changes.Named(w, r)
	if !ok {
		return
	}
	var category store.Category
	added := s.write(w, r, caller, func(c change) (err error) {
		category, err = AddCategory(c.Change, name)
		return err
	})
	if added {
		w.Header().Set("Location", "/projects/"+category.ProjectID.String()+"/categories/"+category.ID.String())
		web.JSON(w, r, http.StatusCreated, categoryView{ID: category.ID, Name: category.Name, IsActive: true, Items: []itemView{}})
	}
}

// reorderCategories puts the project's categories in the order given, their order in the periodic report (AC-39.1): invalidOrder (400) for a
// list that is not exactly the project's categories.
func (s Routes) reorderCategories(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request reorderRequest
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		categories, err := c.Q.CategoriesOf(c.Ctx, c.ProjectID)
		if err != nil {
			return err
		}
		places, ok := inOrder(idsOf(categories, func(row store.Category) uuid.UUID { return row.ID }), request.IDs)
		if !ok {
			return refused(invalidOrder)
		}
		for _, before := range categories {
			after := before
			after.SortOrder = places[before.ID]
			if err := c.saveCategory(before, after); err != nil {
				return err
			}
		}
		return nil
	})
}

func idsOf[T any](rows []T, id func(T) uuid.UUID) []uuid.UUID {
	ids := []uuid.UUID{}
	for _, row := range rows {
		ids = append(ids, id(row))
	}
	return ids
}

// changeCategory applies change to the route's category: 404 for one the project does not hold.
func (s Routes) changeCategory(w http.ResponseWriter, r *http.Request, caller access.Caller, apply func(*store.Category)) {
	s.done(w, r, caller, func(c change) error {
		before, err := changes.Found(c.Q.CategoryOf(c.Ctx, store.CategoryOfParams{ID: web.ID(r, "categoryId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		after := before
		apply(&after)
		return c.saveCategory(before, after)
	})
}

func (s Routes) renameCategory(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if name, ok := changes.Named(w, r); ok {
		s.changeCategory(w, r, caller, func(category *store.Category) { category.Name = name })
	}
}

// activateCategory disables a category, so it and its items leave entry and stay in the reports (BR-15), or brings it back.
func (s Routes) activateCategory(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.changeCategory(w, r, caller, func(category *store.Category) { category.IsActive = active })
	}
}

// addItem adds an item of its type at the end of a category, with its general group and the default columns of its type (FR-05, FR-38): 404 for
// a category the project does not hold, capitalItemExists (409) for a second capital item (BR-31); 201 with it.
func (s Routes) addItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		Name string   `json:"name"`
		Type ItemType `json:"type"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	name, ok := naming.Named(request.Name)
	if !ok {
		refuse(w, r, naming.Invalid)
		return
	}
	var item store.Item
	var general store.Subgroup
	added := s.write(w, r, caller, func(c change) (err error) {
		item, general, err = AddItem(c.Change, web.ID(r, "categoryId"), name, request.Type)
		return err
	})
	if added {
		w.Header().Set("Location", "/projects/"+web.ID(r, "projectId").String()+"/items/"+item.ID.String())
		web.JSON(w, r, http.StatusCreated, itemViewOf(item, []store.Subgroup{general}))
	}
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
	s.done(w, r, caller, func(c change) error {
		categoryID := web.ID(r, "categoryId")
		if _, err := changes.Found(c.Q.CategoryOf(c.Ctx, store.CategoryOfParams{ID: categoryID, ProjectID: c.ProjectID})); err != nil {
			return err
		}
		items, err := c.Q.ItemsOfCategory(c.Ctx, categoryID)
		if err != nil {
			return err
		}
		places, ok := inOrder(idsOf(items, func(row store.Item) uuid.UUID { return row.ID }), request.IDs)
		if !ok {
			return refused(invalidOrder)
		}
		for _, before := range items {
			after := before
			after.SortOrder = places[before.ID]
			if err := c.saveItem(before, after); err != nil {
				return err
			}
		}
		return nil
	})
}

// item is the route's item, or changes.NotFound for one the project does not hold (ProjectStructure.ItemOfAsync, BR-33).
func (c change) item(r *http.Request) (store.Item, error) {
	return changes.Found(c.Q.ItemOf(c.Ctx, store.ItemOfParams{ID: web.ID(r, "itemId"), ProjectID: c.ProjectID}))
}

// changeItem applies change to the route's item: 404 for one the project does not hold.
func (s Routes) changeItem(w http.ResponseWriter, r *http.Request, caller access.Caller, apply func(*store.Item)) {
	s.done(w, r, caller, func(c change) error {
		before, err := c.item(r)
		if err != nil {
			return err
		}
		after := before
		apply(&after)
		return c.saveItem(before, after)
	})
}

func (s Routes) renameItem(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if name, ok := changes.Named(w, r); ok {
		s.changeItem(w, r, caller, func(item *store.Item) { item.Name = name })
	}
}

// activateItem disables an item, so it leaves entry and stays in the reports (BR-15), or brings it back.
func (s Routes) activateItem(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.changeItem(w, r, caller, func(item *store.Item) { item.IsActive = active })
	}
}

// setTablePercentages sets whether the item's sheet calculates each table's percentage of the whole (AC-46.9).
func (s Routes) setTablePercentages(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		IsOn bool `json:"isOn"`
	}
	if web.Read(w, r, &request) {
		s.changeItem(w, r, caller, func(item *store.Item) { item.ShowsTablePercentages = request.IsOn })
	}
}

// addSubgroup adds a named subgroup last among the item's tables (UC-012): 404 for an item the project does not hold, itemTakesNoPurchases (400)
// for an item that takes no purchases, since a sub-table holds purchases (Muteb, 7 Nov 2026).
func (s Routes) addSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, ok := changes.Named(w, r)
	if !ok {
		return
	}
	s.done(w, r, caller, func(c change) error {
		item, err := c.item(r)
		if err != nil {
			return err
		}
		_, err = AddSubgroup(c.Change, item, name)
		return err
	})
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

// changeItemType changes what an item is for (FR-38): while no entry names it, or itemHasEntries, and no contract, or itemHasContract - though an
// item of purchases or of contractor payments may always become both (Muteb, 7 Nov 2026); never an item whose tables are partners,
// itemHasPartners; never a second capital item, capitalItemExists. Its built-in columns become the project's default columns of the new type, and
// the columns a person added stay, after them.
func (s Routes) changeItemType(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	var request struct {
		Type ItemType `json:"type"`
	}
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		before, err := c.item(r)
		if err != nil {
			return err
		}
		if err := c.refuseTypeChange(before, request.Type); err != nil {
			return err
		}
		after := before
		after.Type = int64(request.Type)
		if err := c.saveItem(before, after); err != nil {
			return err
		}
		return c.rebuildColumns(after)
	})
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
	if !web.Read(w, r, &request) {
		return
	}
	s.done(w, r, caller, func(c change) error {
		item, err := c.item(r)
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
		partners, err := c.keptPartnersIn(item.ID)
		if err != nil {
			return err
		}
		ids := append(append(idsOf(subgroups, func(row store.Subgroup) uuid.UUID { return row.ID }),
			idsOf(contracts, func(row store.Contract) uuid.UUID { return row.ID })...), idsOf(partners, func(row store.Partner) uuid.UUID { return row.ID })...)
		places, ok := inOrder(ids, request.IDs)
		if !ok {
			return refused(invalidOrder)
		}
		for _, before := range subgroups {
			after := before
			after.SortOrder = places[before.ID]
			if err := c.saveSubgroup(before, after); err != nil {
				return err
			}
		}
		for _, contract := range contracts {
			place := places[contract.ID]
			if err := c.Save("Contracts", contract.ID, false, map[string]any{"SortOrder": contract.SortOrder}, map[string]any{"SortOrder": place},
				func() error {
					return c.Q.SetContractPlace(c.Ctx, store.SetContractPlaceParams{ID: contract.ID, SortOrder: place})
				}); err != nil {
				return err
			}
		}
		for _, partner := range partners {
			place := places[partner.ID]
			if err := c.Save("Partners", partner.ID, false, map[string]any{"SortOrder": partner.SortOrder}, map[string]any{"SortOrder": place},
				func() error {
					return c.Q.SetPartnerPlace(c.Ctx, store.SetPartnerPlaceParams{ID: partner.ID, SortOrder: place})
				}); err != nil {
				return err
			}
		}
		return nil
	})
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

// subgroup is the route's subgroup of the route's item: changes.NotFound for either not the project's; generalSubgroup (409) for the item's general
// group, which is never renamed or removed, so an entry always has a subgroup (UC-012).
func (c change) subgroup(r *http.Request) (store.Subgroup, error) {
	subgroups, err := c.Q.SubgroupsOfItem(c.Ctx, web.ID(r, "itemId"))
	if err != nil {
		return store.Subgroup{}, err
	}
	subgroupID := web.ID(r, "subgroupId")
	for _, subgroup := range subgroups {
		if subgroup.ID != subgroupID {
			continue
		}
		if _, err := c.item(r); err != nil {
			return subgroup, err
		}
		if subgroup.Name == generalName {
			return subgroup, refused(generalSubgroup)
		}
		return subgroup, nil
	}
	return store.Subgroup{}, changes.NotFound
}

func (s Routes) renameSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, ok := changes.Named(w, r)
	if !ok {
		return
	}
	s.done(w, r, caller, func(c change) error {
		before, err := c.subgroup(r)
		if err != nil {
			return err
		}
		after := before
		after.Name = name
		return c.saveSubgroup(before, after)
	})
}

// deleteSubgroup removes a subgroup no entry uses: subgroupInUse (409) for one an entry uses (FR-05).
func (s Routes) deleteSubgroup(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.done(w, r, caller, func(c change) error {
		subgroup, err := c.subgroup(r)
		if err != nil {
			return err
		}
		if inUse, err := c.Q.SubgroupHasEntries(c.Ctx, subgroup.ID); err != nil || inUse {
			return errors.Join(err, refusalIf(inUse, subgroupInUse))
		}
		return c.Remove("Subgroups", subgroup.ID, subgroupFields(subgroup), func() error { return c.Q.RemoveSubgroup(c.Ctx, subgroup.ID) })
	})
}
