-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: AddHistory :exec
INSERT INTO history (at, project_id, user_id, action, table_name, record_id, before, after, trace_id) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);
