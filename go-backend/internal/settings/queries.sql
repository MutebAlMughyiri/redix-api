-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

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

-- name: PaymentMethodAccountsOf :many
-- The accounts each payment method of the project pays from (redix-app #109).
SELECT payment_method_accounts.payment_method_id, payment_method_accounts.account_id FROM payment_method_accounts
JOIN payment_methods ON payment_methods.id = payment_method_accounts.payment_method_id
WHERE payment_methods.project_id = sqlc.arg(project_id) ORDER BY payment_method_accounts.payment_method_id, payment_method_accounts.account_id;

-- name: AddPaymentMethodAccount :exec
INSERT INTO payment_method_accounts (payment_method_id, account_id) VALUES (sqlc.arg(payment_method_id), sqlc.arg(account_id));

-- name: RemovePaymentMethodAccount :exec
DELETE FROM payment_method_accounts WHERE payment_method_id = sqlc.arg(payment_method_id) AND account_id = sqlc.arg(account_id);

-- name: RemovePaymentMethodAccounts :exec
-- Every account a payment method pays from, as the method is deleted.
DELETE FROM payment_method_accounts WHERE payment_method_id = sqlc.arg(payment_method_id);

-- name: RemoveAccountFromPaymentMethods :exec
-- An account from every payment method paying from it, as the account is deleted.
DELETE FROM payment_method_accounts WHERE account_id = sqlc.arg(account_id);

-- name: MethodPaysFrom :one
-- Whether the payment method pays from the account (redix-app #109, R5).
SELECT EXISTS (
    SELECT 1 FROM payment_method_accounts WHERE payment_method_id = sqlc.arg(payment_method_id) AND account_id = sqlc.arg(account_id)
) AS is_paid_from;
