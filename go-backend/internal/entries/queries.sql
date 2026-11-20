-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: EntryOf :one
SELECT * FROM entries WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: EntryExists :one
SELECT EXISTS (SELECT 1 FROM entries WHERE id = ?);

-- name: EntriesBefore :many
-- The newest take of a project's entries but those removed, listed after the entry at (created_at, number), newest first: the entries list
-- reads a batch at a time through the index on their order (#87).
SELECT * FROM entries WHERE project_id = sqlc.arg(project_id) AND status <> 4 AND (created_at, number) < (sqlc.arg(created_at), CAST(sqlc.arg(number) AS INTEGER))
ORDER BY created_at DESC, number DESC LIMIT sqlc.arg(take);

-- name: EntriesOf :many
-- A project's entries but those removed (redix-api #119), newest first.
SELECT * FROM entries WHERE project_id = ? AND status <> 4 ORDER BY created_at DESC, number DESC;

-- name: PendingEntriesOf :many
SELECT * FROM entries WHERE project_id = ? AND status = 1 ORDER BY created_at, number;

-- name: NextEntryNumber :one
-- The project's next entry number (FR-54, D3): its counter moves and answers in one statement, so two entries never share a number and a number
-- taken is never given again.
UPDATE projects SET last_entry_number = last_entry_number + 1 WHERE id = ? RETURNING last_entry_number;

-- name: SaveEntry :exec
INSERT INTO entries (id, project_id, kind, status, entry_date, amount, account_id, description, created_by_user_id, created_at, approved_at, column_values, number)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET kind = excluded.kind, status = excluded.status, entry_date = excluded.entry_date, amount = excluded.amount,
    account_id = excluded.account_id, description = excluded.description, approved_at = excluded.approved_at, column_values = excluded.column_values;

-- name: RemoveEntry :exec
DELETE FROM entries WHERE id = ?;

-- name: ExpenseDetailsOf :many
-- The expense details of the entries named, a JSON list of the project's own ids (#87).
SELECT expense_details.* FROM json_each(sqlc.arg(entries)) AS named CROSS JOIN expense_details ON expense_details.entry_id = named.value;

-- name: PurchaseDetailsOf :many
-- The purchase details of the entries named, a JSON list of the project's own ids (#87).
SELECT purchase_details.* FROM json_each(sqlc.arg(entries)) AS named CROSS JOIN purchase_details ON purchase_details.entry_id = named.value;

-- name: PaymentDetailsOf :many
-- The contractor payment details of the entries named, a JSON list of the project's own ids (#87).
SELECT contractor_payment_details.* FROM json_each(sqlc.arg(entries)) AS named
CROSS JOIN contractor_payment_details ON contractor_payment_details.entry_id = named.value;

-- name: RevenueDetailsOf :many
-- The revenue details of the entries named, a JSON list of the project's own ids (#87).
SELECT revenue_details.* FROM json_each(sqlc.arg(entries)) AS named CROSS JOIN revenue_details ON revenue_details.entry_id = named.value;

-- name: CapitalDetailsOf :many
-- The capital movement details of the entries named, a JSON list of the project's own ids (#87).
SELECT capital_movement_details.* FROM json_each(sqlc.arg(entries)) AS named
CROSS JOIN capital_movement_details ON capital_movement_details.entry_id = named.value;

-- name: InvoicesOf :many
-- The invoices of the entries named, a JSON list of the project's own ids (#87).
SELECT invoices.* FROM json_each(sqlc.arg(entries)) AS named CROSS JOIN invoices ON invoices.entry_id = named.value;

-- name: EntryFilesOf :many
-- The files of the entries named, a JSON list of the project's own ids, the oldest first, each with its name and type (#87).
SELECT entry_attachments.entry_id, entry_attachments.attachment_id, entry_attachments.role, attachments.file_name, attachments.content_type,
    attachments.uploaded_at, attachments.sha256
FROM json_each(sqlc.arg(entries)) AS named CROSS JOIN entry_attachments ON entry_attachments.entry_id = named.value
JOIN attachments ON attachments.id = entry_attachments.attachment_id ORDER BY attachments.uploaded_at, attachments.id;

-- name: FileOnAnotherEntry :one
-- Whether bytes of this sha256 are on an entry of the project other than editing (BR-22, #87).
SELECT EXISTS (SELECT 1 FROM entry_attachments JOIN attachments ON attachments.id = entry_attachments.attachment_id
    WHERE attachments.project_id = sqlc.arg(project_id) AND attachments.sha256 = sqlc.arg(sha256) AND entry_attachments.entry_id <> sqlc.arg(editing));

-- name: AttachmentOf :one
SELECT * FROM attachments WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: AddExpenseDetails :exec
INSERT INTO expense_details (entry_id, item_id, subgroup_id, payment_method_id) VALUES (?, ?, ?, ?);

-- name: AddPurchaseDetails :exec
INSERT INTO purchase_details (entry_id, supplier_id) VALUES (?, ?);

-- name: AddPaymentDetails :exec
INSERT INTO contractor_payment_details (entry_id, contract_id, is_outside_contract) VALUES (?, ?, ?);

-- name: AddRevenueDetails :exec
INSERT INTO revenue_details (entry_id, item_id, subgroup_id) VALUES (?, ?, ?);

-- name: AddCapitalDetails :exec
INSERT INTO capital_movement_details (entry_id, partner_id) VALUES (?, ?);

-- name: AddEntryFile :exec
INSERT INTO entry_attachments (entry_id, attachment_id, role) VALUES (?, ?, ?);

-- name: RemoveExpenseDetails :exec
DELETE FROM expense_details WHERE entry_id = ?;

-- name: RemovePurchaseDetails :exec
DELETE FROM purchase_details WHERE entry_id = ?;

-- name: RemovePaymentDetails :exec
DELETE FROM contractor_payment_details WHERE entry_id = ?;

-- name: RemoveRevenueDetails :exec
DELETE FROM revenue_details WHERE entry_id = ?;

-- name: RemoveCapitalDetails :exec
DELETE FROM capital_movement_details WHERE entry_id = ?;

-- name: RemoveInvoice :exec
DELETE FROM invoices WHERE entry_id = ?;

-- name: RemoveEntryFile :exec
DELETE FROM entry_attachments WHERE entry_id = sqlc.arg(entry_id) AND attachment_id = sqlc.arg(attachment_id);

-- name: AddDecision :exec
INSERT INTO entry_decisions (entry_id, action, user_id, at, reason) VALUES (?, ?, ?, ?, ?);

-- name: RejectionsOf :many
-- The reasons the entries named, a JSON list of the project's own ids, were rejected, in the order given (#87).
SELECT entry_decisions.entry_id, entry_decisions.reason FROM json_each(sqlc.arg(entries)) AS named
CROSS JOIN entry_decisions ON entry_decisions.entry_id = named.value WHERE entry_decisions.action = 2 ORDER BY entry_decisions.id;

-- name: RemoveDecisions :exec
DELETE FROM entry_decisions WHERE entry_id = ?;

-- name: NotificationsAbout :many
SELECT notification_id FROM notification_entries WHERE entry_id = ?;

-- name: RemoveNotification :exec
DELETE FROM notifications WHERE id = ?;

-- name: RemoveNotificationEntry :exec
DELETE FROM notification_entries WHERE notification_id = ?;

-- name: AddNotificationEntry :exec
INSERT INTO notification_entries (notification_id, entry_id) VALUES (?, ?);

-- name: HoldersOf :many
-- Who holds a permission in a project.
SELECT user_id FROM project_permissions WHERE project_id = sqlc.arg(project_id) AND permission = sqlc.arg(permission) ORDER BY id;

-- name: PartnersOf :many
SELECT * FROM partners WHERE project_id = ? ORDER BY sort_order, id;

-- name: OfferedContracts :many
-- The project's active contracts, each with its contractor's name.
SELECT contracts.*, suppliers.name AS contractor FROM contracts JOIN suppliers ON suppliers.id = contracts.supplier_id
WHERE contracts.project_id = ? AND contracts.is_active = 1 ORDER BY contracts.sort_order, contracts.id;

-- name: UserNames :many
SELECT id, display_name FROM users;

-- name: HistoryOfLogin :many
-- Every History line of one login, newest first: what my history reads (UC-069 R9), so no other login's line can reach it.
SELECT * FROM history WHERE table_name = 'Users' AND record_id = ? ORDER BY id DESC;

-- name: HistoryBefore :many
-- The newest take of a project's History lines older than before, newest first: a history screen reads its page a batch at a time (#87), and
-- only the project's lines, so no line of another project can reach it (D11).
SELECT * FROM history WHERE project_id = sqlc.arg(project_id) AND id < sqlc.arg(before) ORDER BY id DESC LIMIT sqlc.arg(take);

-- name: HistoryAfter :many
-- The oldest take of a project's History lines newer than after, oldest first: the rest of a request a batch ends inside (#87).
SELECT * FROM history WHERE project_id = sqlc.arg(project_id) AND id > sqlc.arg(after) ORDER BY id LIMIT sqlc.arg(take);

-- name: HistoryOfRecords :many
-- Every line of a project's History about the records named, a JSON list of [table, id] pairs, oldest first: what History knows of them,
-- each record looked up through the index on table and record; CROSS JOIN keeps that order whatever SQLite's statistics say (#87).
SELECT history.* FROM json_each(sqlc.arg(records)) AS named CROSS JOIN history
    ON history.table_name = json_extract(named.value, '$[0]') AND history.record_id = json_extract(named.value, '$[1]')
WHERE history.project_id = sqlc.arg(project_id)
ORDER BY history.id;

-- name: DeletedAccountsOf :many
-- The History lines of a project's accounts deleted: the names the account history shows for them (#87).
SELECT * FROM history WHERE project_id = ? AND table_name = 'Accounts' AND action = 'Delete';

-- name: TransfersOf :many
SELECT * FROM transfers WHERE project_id = ?;

-- name: ReconciliationBalancesOf :many
SELECT reconciliation_balances.* FROM reconciliation_balances JOIN reconciliations ON reconciliations.id = reconciliation_balances.reconciliation_id
WHERE reconciliations.project_id = ?;

-- name: DecisionsOf :many
SELECT entry_decisions.* FROM entry_decisions JOIN entries ON entries.id = entry_decisions.entry_id WHERE entries.project_id = ? ORDER BY entry_decisions.id;

-- name: ArabicWord :one
SELECT translations.value FROM translations JOIN translation_keys ON translation_keys.id = translations.translation_key_id
WHERE translation_keys.key = ? AND translations.language = 'ar';

-- name: RegisterNames :many
-- The names a report's list columns choose from: the project's suppliers and accounts by name, its payment methods in their order.
SELECT 'supplier' AS register, suppliers.name, 0 AS place FROM suppliers WHERE suppliers.project_id = sqlc.arg(project_id)
UNION ALL SELECT 'account', accounts.name, 0 FROM accounts WHERE accounts.project_id = sqlc.arg(project_id)
UNION ALL SELECT 'paymentMethod', payment_methods.name, payment_methods.sort_order FROM payment_methods WHERE payment_methods.project_id = sqlc.arg(project_id)
ORDER BY 1, 3, 2 COLLATE names;

-- name: EntryTablesOf :many
-- The table of each entry named, a JSON list of the project's own ids: its contract for a contractor's payment, its group otherwise (#106 G1, #87).
SELECT entry_tables.entry_id, entry_tables.table_id FROM json_each(sqlc.arg(entries)) AS named
CROSS JOIN entry_tables ON entry_tables.entry_id = named.value;

-- name: TableOfEntry :many
-- An entry's table, none for a capital movement (#106).
SELECT table_id FROM entry_tables WHERE entry_id = sqlc.arg(entry_id);

-- name: EntryHiddenFrom :one
-- Whether the entry's table is unticked for the person, so they are not told of it (#106 R11).
SELECT EXISTS (SELECT 1 FROM entry_tables WHERE entry_tables.entry_id = sqlc.arg(entry_id) AND entry_tables.table_id IN (
    SELECT unseen_groups.subgroup_id FROM unseen_groups WHERE unseen_groups.user_id = sqlc.arg(user_id) AND unseen_groups.project_id = sqlc.arg(project_id)
    UNION ALL
    SELECT unseen_contracts.contract_id FROM unseen_contracts
    WHERE unseen_contracts.user_id = sqlc.arg(user_id) AND unseen_contracts.project_id = sqlc.arg(project_id))) AS hidden;
