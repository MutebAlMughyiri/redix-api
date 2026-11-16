-- name: SuppliersOf :many
-- A project's suppliers by name, disabled ones included, each with the item its expenses usually go to and how many of user_id's purchases name
-- it.
SELECT suppliers.*, COALESCE(supplier_default_items.item_id, '') AS default_item_id, COALESCE(items.name, '') AS default_item_name,
    (SELECT count(*) FROM purchase_details JOIN entries ON entries.id = purchase_details.entry_id
     WHERE purchase_details.supplier_id = suppliers.id AND entries.created_by_user_id = sqlc.arg(user_id)) AS times_used
FROM suppliers LEFT JOIN supplier_default_items ON supplier_default_items.supplier_id = suppliers.id
    LEFT JOIN items ON items.id = supplier_default_items.item_id
WHERE suppliers.project_id = sqlc.arg(project_id)
ORDER BY suppliers.name COLLATE names, suppliers.id;

-- name: SupplierOf :one
SELECT * FROM suppliers WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: SaveSupplier :exec
INSERT INTO suppliers (id, project_id, name, normalized_name, vat_number, mobile, status, is_active, is_general_cash, created_by_user_id)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, normalized_name = excluded.normalized_name, vat_number = excluded.vat_number,
    mobile = excluded.mobile, status = excluded.status, is_active = excluded.is_active;

-- name: DefaultItemOf :many
SELECT * FROM supplier_default_items WHERE supplier_id = ?;

-- name: SaveDefaultItem :exec
INSERT INTO supplier_default_items (supplier_id, item_id) VALUES (?, ?) ON CONFLICT (supplier_id) DO UPDATE SET item_id = excluded.item_id;

-- name: RemoveDefaultItem :exec
DELETE FROM supplier_default_items WHERE supplier_id = ?;
