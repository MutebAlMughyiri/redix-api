-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The invoice QR's words go with it (the owner, 23 Nov 2026, findings row 303: "we dont need it completely for app and web and backend"): what
-- Add entry said of a QR code read or not and of its supplier, and what the share screen said of the entries a QR code matched.
DELETE FROM translations WHERE translation_key_id IN (SELECT id FROM translation_keys WHERE key IN ('expenses.qrAddSupplier', 'expenses.qrRead', 'expenses.qrSupplierMissing', 'expenses.qrUnreadable', 'share.matches', 'share.amountDiffers', 'share.supplierDiffers'));
DELETE FROM translation_keys WHERE key IN ('expenses.qrAddSupplier', 'expenses.qrRead', 'expenses.qrSupplierMissing', 'expenses.qrUnreadable', 'share.matches', 'share.amountDiffers', 'share.supplierDiffers');
