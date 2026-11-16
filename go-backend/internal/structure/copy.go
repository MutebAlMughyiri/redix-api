// Package structure keeps a project's structure: its categories, their items, each item's subgroups and columns, and the default columns a new
// item takes (ProjectStructure, redix-api #140).
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
	for _, column := range defaults {
		column.ID, column.ProjectID = uuid.Must(uuid.NewV7()), projectID
		if err := q.AddDefaultColumn(ctx, store.AddDefaultColumnParams(column)); err != nil {
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

// copyCategories copies source's active categories into projectID, each followed by its items, as Category.CopyFor and Item.CopyFor did.
func copyCategories(ctx context.Context, q *store.Queries, source, projectID uuid.UUID, defaults []store.DefaultColumn) error {
	categories, err := q.ActiveCategoriesOf(ctx, source)
	if err != nil {
		return err
	}
	items, err := q.ActiveItemsOf(ctx, source)
	if err != nil {
		return err
	}
	subgroups, err := q.SubgroupsOf(ctx, source)
	if err != nil {
		return err
	}
	columns, err := q.ShownColumnsOf(ctx, source)
	if err != nil {
		return err
	}
	for _, category := range categories {
		copied := store.Category{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, Name: category.Name, SortOrder: category.SortOrder, IsActive: true}
		if err := q.AddCategory(ctx, store.AddCategoryParams(copied)); err != nil {
			return err
		}
		for _, item := range items {
			if item.CategoryID != category.ID {
				continue
			}
			if err := copyItem(ctx, q, item, copied.ID, subgroups, columns, defaults); err != nil {
				return err
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
	if err := q.AddItem(ctx, store.AddItemParams(copied)); err != nil {
		return err
	}
	for _, subgroup := range subgroups {
		if subgroup.ItemID == item.ID {
			if err := q.AddSubgroup(ctx, store.AddSubgroupParams{ID: uuid.Must(uuid.NewV7()), ItemID: copied.ID, Name: subgroup.Name, SortOrder: subgroup.SortOrder}); err != nil {
				return err
			}
		}
	}
	var shown []store.ItemColumn
	for _, column := range columns {
		if column.ItemID == item.ID {
			shown = append(shown, column)
		}
	}
	if len(shown) == 0 {
		for _, column := range defaults {
			if column.ItemType == item.Type {
				shown = append(shown, store.ItemColumn{Key: column.Key, Name: column.Name, DataType: column.DataType, IsCore: column.IsCore,
					IsRequired: column.IsRequired, ShowInReports: column.ShowInReports, SortOrder: column.SortOrder, Choices: column.Choices})
			}
		}
	}
	for _, column := range shown {
		column.ID, column.ItemID, column.IsHidden = uuid.Must(uuid.NewV7()), copied.ID, false
		if err := q.AddItemColumn(ctx, store.AddItemColumnParams(column)); err != nil {
			return err
		}
	}
	return nil
}
