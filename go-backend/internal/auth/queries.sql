-- name: HasUsers :one
-- Whether the database holds any user.
SELECT EXISTS (SELECT 1 FROM users) AS has_users;

-- name: AddUser :exec
INSERT INTO users (id, username, display_name, mobile, password_hash, must_change_password, failed_sign_ins, locked_until, created_at, email)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: AddSystemPermission :exec
INSERT INTO system_permissions (id, user_id, permission) VALUES (?, ?, ?);

-- name: UserNamed :one
-- The user signing in, by a username matched whatever its case (R29).
SELECT * FROM users WHERE username = sqlc.arg(username);

-- name: UserWithID :one
SELECT * FROM users WHERE id = sqlc.arg(id);

-- name: SaveUser :exec
-- What signing in or changing the password leaves on the user: the wrong passwords counted, the lock, the hash, and whether it must still change.
UPDATE users SET failed_sign_ins = sqlc.arg(failed_sign_ins), locked_until = sqlc.arg(locked_until), password_hash = sqlc.arg(password_hash),
    must_change_password = sqlc.arg(must_change_password) WHERE id = sqlc.arg(id);

-- name: AddDevice :exec
INSERT INTO devices (id, user_id, refresh_token_hash, refresh_expires_at, is_revoked, uses_biometrics, last_seen_at) VALUES (?, ?, ?, ?, 0, 0, ?);

-- name: DeviceWithRefreshToken :one
SELECT * FROM devices WHERE refresh_token_hash = sqlc.arg(refresh_token_hash);

-- name: DeviceWithID :one
SELECT * FROM devices WHERE id = sqlc.arg(id);

-- name: RenewDevice :exec
UPDATE devices SET refresh_token_hash = sqlc.arg(refresh_token_hash), refresh_expires_at = sqlc.arg(refresh_expires_at), last_seen_at = sqlc.arg(last_seen_at),
    uses_biometrics = sqlc.arg(uses_biometrics) WHERE id = sqlc.arg(id);

-- name: RevokeDevice :exec
UPDATE devices SET is_revoked = 1 WHERE id = sqlc.arg(id);

-- name: RevokeOtherDevices :exec
UPDATE devices SET is_revoked = 1 WHERE user_id = sqlc.arg(user_id) AND id <> sqlc.arg(kept_device_id);

-- name: DefaultProjectOf :many
SELECT project_id FROM user_default_projects WHERE user_id = sqlc.arg(user_id);
