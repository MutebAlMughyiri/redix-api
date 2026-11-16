-- name: ManagersOf :many
-- Who manages the users of a project.
SELECT user_id FROM project_permissions WHERE project_id = ? AND permission = 'ManageUsers' ORDER BY id;

-- name: AddNotification :exec
INSERT INTO notifications (id, user_id, project_id, kind, at, is_read) VALUES (?, ?, ?, ?, ?, ?);

-- name: NotificationsOf :many
-- What a person was told, newest first, only about the projects they still hold anything in, at most 100 (NotificationsEndpoints.List).
SELECT notifications.id, notifications.kind, notifications.project_id, projects.name AS project_name,
    COALESCE(notification_entries.entry_id, '') AS entry_id, notifications.at, notifications.is_read
FROM notifications JOIN projects ON projects.id = notifications.project_id
    LEFT JOIN notification_entries ON notification_entries.notification_id = notifications.id
WHERE notifications.user_id = sqlc.arg(user_id)
    AND EXISTS (SELECT 1 FROM project_permissions WHERE project_permissions.user_id = sqlc.arg(user_id) AND project_permissions.project_id = notifications.project_id)
ORDER BY notifications.at DESC, notifications.id DESC LIMIT 100;

-- name: MarkRead :execrows
UPDATE notifications SET is_read = 1 WHERE id = sqlc.arg(id) AND user_id = sqlc.arg(user_id);
