-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- redix-api #87: a project's entries by status in their order, holding what a balance sums, in place of the index on project and status alone.
-- The entries waiting for approval are read through it, where SQLite walked every entry in order since migration 13 (a project ten times qa's
-- size: 3.6 ms to none), and an account's balance is summed from it alone, not from the table (8.7 ms to 3 ms, the sum taken per account first).
DROP INDEX ix_entries_project_id_status;
CREATE INDEX ix_entries_project_id_status_created_at_number ON entries (project_id, status, created_at, number, account_id, kind, amount, id);
