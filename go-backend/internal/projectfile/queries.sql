-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: ProjectHoldsEntries :one
-- Whether the project holds any entry, a removed one too (Project.HoldingEntriesAsync): a project file loads only into a project holding none.
SELECT EXISTS (SELECT 1 FROM entries WHERE project_id = ?);
