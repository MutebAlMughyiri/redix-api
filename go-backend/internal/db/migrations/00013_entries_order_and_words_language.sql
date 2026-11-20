-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- redix-api #87: a project's entries in the order they are listed, so a page of them is read through an index rather than every entry sorted
-- (a page of 50 on a project ten times qa's size: 108 ms to 0.3 ms); and the words of one language, the one full scan the measurement found.
CREATE INDEX ix_entries_project_id_created_at_number ON entries (project_id, created_at, number);
CREATE INDEX ix_translations_language ON translations (language);
