-- name: AddSupplier :exec
INSERT INTO suppliers (id, project_id, name, normalized_name, vat_number, mobile, status, is_active, is_general_cash, created_by_user_id)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
