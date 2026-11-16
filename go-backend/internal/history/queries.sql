-- name: Add :exec
INSERT INTO history (at, project_id, user_id, action, table_name, record_id, before, after) VALUES (?, ?, ?, ?, ?, ?, ?, ?);
