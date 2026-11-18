-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: ContractsOf :many
-- A project's contracts, disabled ones included, each with its contractor's and its item's names.
SELECT contracts.*, suppliers.name AS supplier_name, items.name AS item_name
FROM contracts JOIN suppliers ON suppliers.id = contracts.supplier_id JOIN items ON items.id = contracts.item_id
WHERE contracts.project_id = ? ORDER BY contracts.id;

-- name: ContractOf :one
SELECT * FROM contracts WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id);

-- name: ContractDocuments :many
-- The documents each contract of the project carries, in the order they were kept.
SELECT contract_attachments.contract_id, attachments.id, attachments.file_name, attachments.content_type, attachments.size_bytes
FROM contract_attachments JOIN attachments ON attachments.id = contract_attachments.attachment_id
WHERE attachments.project_id = ? ORDER BY attachments.id;

-- name: ContractMoney :many
-- What the ledger holds inside each contract of the project (ContractMoney, BR-04): its approved payments and those waiting, inside the contract,
-- leaving out the entry left_out, the one being changed.
SELECT contractor_payment_details.contract_id, entries.status, CAST(SUM(entries.amount) AS INTEGER) AS total
FROM contractor_payment_details JOIN entries ON entries.id = contractor_payment_details.entry_id
WHERE entries.project_id = sqlc.arg(project_id) AND entries.id <> sqlc.arg(left_out) AND contractor_payment_details.is_outside_contract = 0
    AND entries.status IN (1, 2)
GROUP BY contractor_payment_details.contract_id, entries.status;

-- name: ContractsWithPayments :many
-- The contracts any payment names, whatever became of it: their contractor and item then stay.
SELECT DISTINCT contractor_payment_details.contract_id FROM contractor_payment_details
JOIN contracts ON contracts.id = contractor_payment_details.contract_id WHERE contracts.project_id = ?;

-- name: ActiveSupplierExists :one
SELECT EXISTS (SELECT 1 FROM suppliers WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id) AND is_active = 1);

-- name: AttachmentExists :one
SELECT EXISTS (SELECT 1 FROM attachments WHERE id = sqlc.arg(id) AND project_id = sqlc.arg(project_id));

-- name: DocumentsOfContract :many
SELECT attachment_id FROM contract_attachments WHERE contract_id = ?;

-- name: AddContractDocument :exec
INSERT INTO contract_attachments (contract_id, attachment_id) VALUES (?, ?);

-- name: SaveContract :exec
INSERT INTO contracts (id, project_id, item_id, sort_order, supplier_id, value, is_open_value, unit_price, quantity, notes, is_active)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET item_id = excluded.item_id, sort_order = excluded.sort_order, supplier_id = excluded.supplier_id,
    value = excluded.value, is_open_value = excluded.is_open_value, unit_price = excluded.unit_price, quantity = excluded.quantity,
    notes = excluded.notes, is_active = excluded.is_active;
