-- name: EntryOf :one
SELECT * FROM entries WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: EntryExists :one
SELECT EXISTS (SELECT 1 FROM entries WHERE id = ?);

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
SELECT expense_details.* FROM expense_details JOIN entries ON entries.id = expense_details.entry_id WHERE entries.project_id = ?;

-- name: PurchaseDetailsOf :many
SELECT purchase_details.* FROM purchase_details JOIN entries ON entries.id = purchase_details.entry_id WHERE entries.project_id = ?;

-- name: PaymentDetailsOf :many
SELECT contractor_payment_details.* FROM contractor_payment_details JOIN entries ON entries.id = contractor_payment_details.entry_id
WHERE entries.project_id = ?;

-- name: RevenueDetailsOf :many
SELECT revenue_details.* FROM revenue_details JOIN entries ON entries.id = revenue_details.entry_id WHERE entries.project_id = ?;

-- name: CapitalDetailsOf :many
SELECT capital_movement_details.* FROM capital_movement_details JOIN entries ON entries.id = capital_movement_details.entry_id
WHERE entries.project_id = ?;

-- name: InvoicesOf :many
SELECT invoices.* FROM invoices JOIN entries ON entries.id = invoices.entry_id WHERE entries.project_id = ?;

-- name: EntryFilesOf :many
-- The files of the project's entries, the oldest first, each with its name and type.
SELECT entry_attachments.entry_id, entry_attachments.attachment_id, entry_attachments.role, attachments.file_name, attachments.content_type,
    attachments.uploaded_at, attachments.sha256
FROM entry_attachments JOIN attachments ON attachments.id = entry_attachments.attachment_id
WHERE attachments.project_id = ? ORDER BY attachments.uploaded_at, attachments.id;

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
-- The reasons the project's entries were rejected, in the order given.
SELECT entry_decisions.entry_id, entry_decisions.reason FROM entry_decisions JOIN entries ON entries.id = entry_decisions.entry_id
WHERE entries.project_id = ? AND entry_decisions.action = 2 ORDER BY entry_decisions.id;

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

-- name: HistoryOfProject :many
-- Every History line of a project, newest first: what the history screens read (D11), so no line of another project can reach them.
SELECT * FROM history WHERE project_id = ? ORDER BY id DESC;

-- name: TransfersOf :many
SELECT * FROM transfers WHERE project_id = ?;

-- name: ReconciliationBalancesOf :many
SELECT reconciliation_balances.* FROM reconciliation_balances JOIN reconciliations ON reconciliations.id = reconciliation_balances.reconciliation_id
WHERE reconciliations.project_id = ?;

-- name: DecisionsOf :many
SELECT entry_decisions.* FROM entry_decisions JOIN entries ON entries.id = entry_decisions.entry_id WHERE entries.project_id = ? ORDER BY entry_decisions.id;

-- name: LatestIssuedAt :many
-- When the project's latest periodic report was issued; what was approved after it is new (BR-26).
SELECT issued_at FROM periodic_reports WHERE project_id = ? ORDER BY number DESC LIMIT 1;

-- name: ArabicWord :one
SELECT translations.value FROM translations JOIN translation_keys ON translation_keys.id = translations.translation_key_id
WHERE translation_keys.key = ? AND translations.language = 'ar';

-- name: RegisterNames :many
-- The names a report's list columns choose from: the project's suppliers and accounts by name, its payment methods in their order.
SELECT 'supplier' AS register, suppliers.name, 0 AS place FROM suppliers WHERE suppliers.project_id = sqlc.arg(project_id)
UNION ALL SELECT 'account', accounts.name, 0 FROM accounts WHERE accounts.project_id = sqlc.arg(project_id)
UNION ALL SELECT 'paymentMethod', payment_methods.name, payment_methods.sort_order FROM payment_methods WHERE payment_methods.project_id = sqlc.arg(project_id)
ORDER BY 1, 3, 2 COLLATE names;
