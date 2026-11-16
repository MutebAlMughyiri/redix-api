// Package structure keeps a project's structure: its categories, their items, each item's subgroups and columns, and the default columns a new
// item takes (ProjectStructure, CategoriesEndpoints, ColumnsEndpoints; redix-api #140).
package structure

import (
	"context"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/google/uuid"
)

// template is the project a new project's structure is copied from when its registration chose none (StructureSeed.TemplateId), seeded by the
// migration 00004_template.
var template = uuid.MustParse("f11ea9ed-0623-ebf7-abff-3f4e8714248a")

// Copy gives projectID, as the admin approves it, the structure its registration chose or the template's (ProjectStructure.ApproveAsync, FR-05):
// the source's default columns; its active categories with their active items, each with its subgroups and its shown columns, or the default
// columns of its type when it shows none; and the template's payment methods. Never entries, contracts or balances (AC-05.2). A copy has no History
// line of its own: the approval's line is the record.
func Copy(ctx context.Context, q *store.Queries, projectID uuid.UUID) error {
	source, err := sourceOf(ctx, q, projectID)
	if err != nil {
		return err
	}
	defaults, err := q.DefaultColumnsOf(ctx, source)
	if err != nil {
		return err
	}
	for _, row := range defaults {
		if err := q.SaveDefaultColumn(ctx, store.SaveDefaultColumnParams(ofDefaultColumn(row).inTemplate(uuid.Must(uuid.NewV7()), projectID, row.ItemType))); err != nil {
			return err
		}
	}
	if err := copyCategories(ctx, q, source, projectID, defaults); err != nil {
		return err
	}
	methods, err := q.PaymentMethodsOf(ctx, template)
	if err != nil {
		return err
	}
	for _, method := range methods {
		method.ID, method.ProjectID, method.IsActive = uuid.Must(uuid.NewV7()), projectID, true
		if err := q.AddPaymentMethod(ctx, store.AddPaymentMethodParams(method)); err != nil {
			return err
		}
	}
	return nil
}

func sourceOf(ctx context.Context, q *store.Queries, projectID uuid.UUID) (uuid.UUID, error) {
	chosen, err := q.StructureSourceOf(ctx, projectID)
	if err != nil || len(chosen) == 0 {
		return template, err
	}
	return chosen[0], nil
}

// copyCategories copies source's active categories into projectID, each followed by its active items, as Category.CopyFor and Item.CopyFor did.
func copyCategories(ctx context.Context, q *store.Queries, source, projectID uuid.UUID, defaults []store.DefaultColumn) error {
	categories, err := q.CategoriesOf(ctx, source)
	if err != nil {
		return err
	}
	items, err := q.ItemsOf(ctx, source)
	if err != nil {
		return err
	}
	subgroups, err := q.SubgroupsOf(ctx, source)
	if err != nil {
		return err
	}
	columns, err := q.ColumnsOf(ctx, source)
	if err != nil {
		return err
	}
	for _, category := range categories {
		if !category.IsActive {
			continue
		}
		copied := store.Category{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, Name: category.Name, SortOrder: category.SortOrder, IsActive: true}
		if err := q.SaveCategory(ctx, store.SaveCategoryParams(copied)); err != nil {
			return err
		}
		for _, item := range items {
			if item.CategoryID == category.ID && item.IsActive {
				if err := copyItem(ctx, q, item, copied.ID, subgroups, columns, defaults); err != nil {
					return err
				}
			}
		}
	}
	return nil
}

// copyItem copies item into categoryID with its subgroups and shown columns; an item showing none takes the default columns of its type.
func copyItem(ctx context.Context, q *store.Queries, item store.Item, categoryID uuid.UUID, subgroups []store.Subgroup, columns []store.ItemColumn,
	defaults []store.DefaultColumn) error {
	copied := store.Item{ID: uuid.Must(uuid.NewV7()), CategoryID: categoryID, Name: item.Name, Type: item.Type, SortOrder: item.SortOrder, IsActive: true,
		ShowsTablePercentages: item.ShowsTablePercentages}
	if err := q.SaveItem(ctx, store.SaveItemParams(copied)); err != nil {
		return err
	}
	for _, subgroup := range subgroups {
		if subgroup.ItemID == item.ID {
			if err := q.SaveSubgroup(ctx, store.SaveSubgroupParams{ID: uuid.Must(uuid.NewV7()), ItemID: copied.ID, Name: subgroup.Name, SortOrder: subgroup.SortOrder}); err != nil {
				return err
			}
		}
	}
	var shown []column
	for _, row := range columns {
		if row.ItemID == item.ID && !row.IsHidden {
			shown = append(shown, ofItemColumn(row))
		}
	}
	if len(shown) == 0 {
		shown = defaultsOfType(defaults, item.Type)
	}
	for _, one := range shown {
		if err := q.SaveItemColumn(ctx, store.SaveItemColumnParams(one.onItem(uuid.Must(uuid.NewV7()), copied.ID, false))); err != nil {
			return err
		}
	}
	return nil
}

// defaultsOfType is the default columns of itemType among defaults, each at its own place: what a new item of that type asks (Item.Inherit).
func defaultsOfType(defaults []store.DefaultColumn, itemType int64) []column {
	var columns []column
	for _, row := range defaults {
		if row.ItemType == itemType {
			columns = append(columns, ofDefaultColumn(row))
		}
	}
	return columns
}
