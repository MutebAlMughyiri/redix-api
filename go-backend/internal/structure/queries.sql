-- name: StructureSourceOf :many
-- The earlier project a registration chose to copy the structure from; none is copying the template (ProjectStructureSource.OfAsync).
SELECT structure_source_id FROM project_structure_sources WHERE project_id = ?;

-- name: DefaultColumnsOf :many
-- A project's default columns, the ones a new item of each type takes.
SELECT * FROM default_columns WHERE project_id = ? ORDER BY sort_order, id;

-- name: ActiveCategoriesOf :many
SELECT * FROM categories WHERE project_id = ? AND is_active = 1 ORDER BY sort_order, id;

-- name: ActiveItemsOf :many
SELECT * FROM items WHERE is_active = 1 AND category_id IN (SELECT categories.id FROM categories WHERE categories.project_id = ?) ORDER BY sort_order, id;

-- name: SubgroupsOf :many
-- The subgroups of every item of a project.
SELECT * FROM subgroups
WHERE item_id IN (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ?)
ORDER BY sort_order, id;

-- name: ShownColumnsOf :many
-- The columns of every item of a project but those deleted, which are hidden (BR-21).
SELECT * FROM item_columns
WHERE is_hidden = 0 AND item_id IN (SELECT items.id FROM items JOIN categories ON categories.id = items.category_id WHERE categories.project_id = ?)
ORDER BY sort_order, id;

-- name: PaymentMethodsOf :many
SELECT * FROM payment_methods WHERE project_id = ? ORDER BY sort_order, id;

-- name: AddDefaultColumn :exec
INSERT INTO default_columns (id, project_id, item_type, key, name, data_type, is_core, is_required, show_in_reports, sort_order, choices)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: AddCategory :exec
INSERT INTO categories (id, project_id, name, sort_order, is_active) VALUES (?, ?, ?, ?, ?);

-- name: AddItem :exec
INSERT INTO items (id, category_id, name, type, sort_order, is_active, shows_table_percentages) VALUES (?, ?, ?, ?, ?, ?, ?);

-- name: AddSubgroup :exec
INSERT INTO subgroups (id, item_id, name, sort_order) VALUES (?, ?, ?, ?);

-- name: AddItemColumn :exec
INSERT INTO item_columns (id, item_id, is_hidden, key, name, data_type, is_core, is_required, show_in_reports, sort_order, choices)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: AddPaymentMethod :exec
INSERT INTO payment_methods (id, project_id, name, sort_order, is_active) VALUES (?, ?, ?, ?, ?);
