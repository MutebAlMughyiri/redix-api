-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: AccountsOf :many
-- A project's accounts, banks first, then in the order they were added (AC-22.1), each with who holds it.
SELECT accounts.*, COALESCE(account_custodians.user_id, '') AS custodian_id, COALESCE(users.display_name, '') AS custodian_name
FROM accounts LEFT JOIN account_custodians ON account_custodians.account_id = accounts.id LEFT JOIN users ON users.id = account_custodians.user_id
WHERE accounts.project_id = ? ORDER BY accounts.type, accounts.id;

-- name: AccountOf :one
SELECT * FROM accounts WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: CustodianOf :many
SELECT * FROM account_custodians WHERE account_id = ?;

-- name: SaveAccount :exec
INSERT INTO accounts (id, project_id, type, name, is_active) VALUES (?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, is_active = excluded.is_active;

-- name: RemoveAccount :exec
DELETE FROM accounts WHERE id = ?;

-- name: SaveCustodian :exec
INSERT INTO account_custodians (account_id, user_id) VALUES (?, ?) ON CONFLICT (account_id) DO UPDATE SET user_id = excluded.user_id;

-- name: RemoveCustodian :exec
DELETE FROM account_custodians WHERE account_id = ?;

-- name: IsMember :one
-- Whether the user holds anything in the project (ProjectUsersEndpoints.IsMemberAsync).
SELECT EXISTS (SELECT 1 FROM project_permissions WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id));

-- name: AccountMovements :many
-- What each account of the project that anything moved holds (AccountMoney): its approved entries by their kind's sign (BR-10) - capital in and
-- revenue bring money in, the rest take it out - plus the transfers it received, less those it sent.
SELECT moved.account_id, CAST(SUM(moved.amount) AS INTEGER) AS total FROM (
    SELECT entries.account_id, CASE WHEN entries.kind IN (2, 4) THEN entries.amount ELSE -entries.amount END AS amount
    FROM entries WHERE entries.project_id = sqlc.arg(project_id) AND entries.status = 2
    UNION ALL SELECT transfers.to_account_id, transfers.amount FROM transfers WHERE transfers.project_id = sqlc.arg(project_id)
    UNION ALL SELECT transfers.from_account_id, -transfers.amount FROM transfers WHERE transfers.project_id = sqlc.arg(project_id)
) AS moved GROUP BY moved.account_id;

-- name: AccountsUsed :many
-- The project's accounts that something names, which then stay: an entry in any status, an actual-balances check, or a transfer either way (R15).
SELECT entries.account_id FROM entries WHERE entries.project_id = sqlc.arg(project_id)
UNION SELECT reconciliation_balances.account_id FROM reconciliation_balances
    JOIN reconciliations ON reconciliations.id = reconciliation_balances.reconciliation_id WHERE reconciliations.project_id = sqlc.arg(project_id)
UNION SELECT transfers.from_account_id FROM transfers WHERE transfers.project_id = sqlc.arg(project_id)
UNION SELECT transfers.to_account_id FROM transfers WHERE transfers.project_id = sqlc.arg(project_id);

-- name: ApprovedTotalsByKind :many
SELECT entries.kind, CAST(SUM(entries.amount) AS INTEGER) AS total FROM entries
WHERE entries.project_id = ? AND entries.status = 2 GROUP BY entries.kind;

-- name: ApprovedRevenueByItemType :many
SELECT items.type, CAST(SUM(entries.amount) AS INTEGER) AS total
FROM revenue_details JOIN entries ON entries.id = revenue_details.entry_id JOIN items ON items.id = revenue_details.item_id
WHERE entries.project_id = ? AND entries.status = 2 GROUP BY items.type;

-- name: AddTransfer :exec
INSERT INTO transfers (id, project_id, from_account_id, to_account_id, amount, date, notes, created_by_user_id, created_at)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: LatestReconciliation :many
SELECT * FROM reconciliations WHERE project_id = ? ORDER BY at DESC, id DESC LIMIT 1;

-- name: BalancesOf :many
SELECT * FROM reconciliation_balances WHERE reconciliation_id = ?;

-- name: AddReconciliation :exec
INSERT INTO reconciliations (id, project_id, at, user_id) VALUES (?, ?, ?, ?);

-- name: AddReconciliationBalance :exec
INSERT INTO reconciliation_balances (reconciliation_id, account_id, actual_balance) VALUES (?, ?, ?);
