-- name: PaymentMethodsOf :many
-- A project's payment methods, disabled ones included, in order.
SELECT * FROM payment_methods WHERE project_id = ? ORDER BY sort_order, id;

-- name: PaymentMethodOf :one
SELECT * FROM payment_methods WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: PaymentMethodUsed :one
-- Whether an expense was paid with the payment method, which then keeps it.
SELECT EXISTS (SELECT 1 FROM expense_details WHERE payment_method_id = ?);

-- name: SavePaymentMethod :exec
INSERT INTO payment_methods (id, project_id, name, sort_order, is_active) VALUES (?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, sort_order = excluded.sort_order, is_active = excluded.is_active;

-- name: RemovePaymentMethod :exec
DELETE FROM payment_methods WHERE id = ?;
