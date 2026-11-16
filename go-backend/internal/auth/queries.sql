-- name: HasUsers :one
-- Whether the database holds any user.
SELECT EXISTS (SELECT 1 FROM users) AS has_users;

-- name: AddUser :exec
INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: AddSystemPermission :exec
INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, ?);
