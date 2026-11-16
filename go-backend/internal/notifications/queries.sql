-- name: ManagersOf :many
-- Who manages the users of a project.
SELECT user_id FROM project_permissions WHERE project_id = ? AND permission = 'ManageUsers' ORDER BY id;

-- name: AddNotification :exec
INSERT INTO notifications (id, user_id, project_id, kind, at, is_read) VALUES (?, ?, ?, ?, ?, ?);
