-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: StructureSourceOf :many
-- The earlier project a registration chose to copy the structure from; none is copying the template (ProjectStructureSource.OfAsync).
SELECT structure_source_id FROM project_structure_sources WHERE project_id = ?;

-- name: CategoriesOf :many
-- A project's categories, disabled ones included, in the order of the periodic report (AC-39.1).
SELECT * FROM categories WHERE project_id = ? ORDER BY sort_order, id;

-- name: CategoryOf :one
SELECT * FROM categories WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: ItemsOf :many
-- Every item of a project's categories, disabled ones included.
SELECT * FROM items WHERE category_id IN (SELECT categories.id FROM categories WHERE categories.project_id = ?) ORDER BY sort_order, id;

-- name: ItemOf :one
-- The item if it belongs to the project: the one answer to whether an item is a project's, so one project's route never reaches another's rows (BR-33).
SELECT * FROM items WHERE items.id = sqlc.arg(id) AND items.category_id IN (SELECT categories.id FROM categories WHERE categories.project_id = sqlc.arg(project_id));

-- name: ItemsOfCategory :many
SELECT * FROM items WHERE category_id = ? ORDER BY sort_order, id;

-- name: CapitalItemExists :one
-- Whether the project has its one capital item, the capital (BR-31, redix-api #109).
SELECT EXISTS (SELECT 1 FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ? AND items.type = 6);

-- name: SubgroupsOf :many
-- The subgroups of every item of a project.
SELECT * FROM subgroups
WHERE item_id IN (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ?)
ORDER BY sort_order, id;

-- name: SubgroupsOfItem :many
SELECT * FROM subgroups WHERE item_id = ? ORDER BY sort_order, id;

-- name: ColumnsOf :many
-- The columns of every item of a project, the deleted ones - hidden (BR-21) - included.
SELECT * FROM item_columns
WHERE item_id IN (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ?)
ORDER BY sort_order, id;

-- name: ColumnsOfItem :many
-- An item's columns, the deleted ones included.
SELECT * FROM item_columns WHERE item_id = ? ORDER BY sort_order, id;

-- name: DefaultColumnsOf :many
-- A project's default columns, the ones a new item of each type takes.
SELECT * FROM default_columns WHERE project_id = ? ORDER BY sort_order, id;

-- name: DefaultColumnsOfType :many
SELECT * FROM default_columns WHERE project_id = sqlc.arg(project_id) AND item_type = sqlc.arg(item_type) ORDER BY sort_order, id;

-- name: ItemHasEntries :one
-- Whether an entry, an expense or revenue, is kept on the item (ProjectStructure.ItemHasEntriesAsync).
SELECT EXISTS (SELECT 1 FROM expense_details WHERE expense_details.item_id = sqlc.arg(item_id)
    UNION ALL SELECT 1 FROM revenue_details WHERE revenue_details.item_id = sqlc.arg(item_id));

-- name: SubgroupHasEntries :one
SELECT EXISTS (SELECT 1 FROM expense_details WHERE expense_details.subgroup_id = sqlc.arg(subgroup_id)
    UNION ALL SELECT 1 FROM revenue_details WHERE revenue_details.subgroup_id = sqlc.arg(subgroup_id));

-- name: ContractsOfItem :many
SELECT * FROM contracts WHERE item_id = ? ORDER BY sort_order, id;

-- name: PartnersInItem :many
-- The partners whose tables are in the item: every partner of its project when it is the project's capital item, none for any other item
-- (ProjectStructure.PartnersIn); a deleted partner included.
SELECT partners.* FROM partners JOIN categories ON categories.project_id = partners.project_id JOIN items ON items.category_id = categories.id
WHERE items.id = ? AND items.type = 6 ORDER BY partners.sort_order, partners.id;

-- name: SaveCategory :exec
INSERT INTO categories (id, project_id, name, sort_order, is_active) VALUES (?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, sort_order = excluded.sort_order, is_active = excluded.is_active;

-- name: SaveItem :exec
INSERT INTO items (id, category_id, name, type, sort_order, is_active, shows_table_percentages) VALUES (?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, type = excluded.type, sort_order = excluded.sort_order, is_active = excluded.is_active,
    shows_table_percentages = excluded.shows_table_percentages;

-- name: SaveSubgroup :exec
INSERT INTO subgroups (id, item_id, name, sort_order) VALUES (?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, sort_order = excluded.sort_order;

-- name: RemoveSubgroup :exec
DELETE FROM subgroups WHERE id = ?;

-- name: SaveItemColumn :exec
INSERT INTO item_columns (id, item_id, is_hidden, key, name, data_type, is_core, is_required, show_in_reports, sort_order, choices)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET is_hidden = excluded.is_hidden, name = excluded.name, is_required = excluded.is_required,
    show_in_reports = excluded.show_in_reports, sort_order = excluded.sort_order, choices = excluded.choices;

-- name: RemoveItemColumn :exec
DELETE FROM item_columns WHERE id = ?;

-- name: SaveDefaultColumn :exec
INSERT INTO default_columns (id, project_id, item_type, key, name, data_type, is_core, is_required, show_in_reports, sort_order, choices)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, is_required = excluded.is_required, show_in_reports = excluded.show_in_reports,
    sort_order = excluded.sort_order, choices = excluded.choices;

-- name: RemoveDefaultColumn :exec
DELETE FROM default_columns WHERE id = ?;

-- name: SetContractPlace :exec
UPDATE contracts SET sort_order = sqlc.arg(sort_order) WHERE id = sqlc.arg(id);

-- name: SetPartnerPlace :exec
UPDATE partners SET sort_order = sqlc.arg(sort_order) WHERE id = sqlc.arg(id);
