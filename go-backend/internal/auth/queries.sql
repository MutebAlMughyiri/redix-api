-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

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

-- name: UserWithEmail :one
-- The user whose email this is; an empty email names nobody.
SELECT * FROM users WHERE email = sqlc.arg(email) AND email <> '';

-- name: EmailTaken :one
SELECT EXISTS (SELECT 1 FROM users WHERE email = sqlc.arg(email) AND id <> sqlc.arg(user_id)) AS taken;

-- name: SetEmail :exec
UPDATE users SET email = sqlc.arg(email) WHERE id = sqlc.arg(id);

-- name: RevokeEveryDevice :exec
UPDATE devices SET is_revoked = 1 WHERE user_id = sqlc.arg(user_id);

-- name: DeleteCodes :exec
DELETE FROM email_codes WHERE user_id = sqlc.arg(user_id) AND purpose = sqlc.arg(purpose);

-- name: DeleteEveryCode :exec
DELETE FROM email_codes WHERE user_id = sqlc.arg(user_id);

-- name: AddCode :exec
INSERT INTO email_codes (id, user_id, purpose, email, code_hash, expires_at, tries, used_at) VALUES (?, ?, ?, ?, ?, ?, 0, '1900-01-01T00:00:00.0000000Z');

-- name: LastCode :many
SELECT * FROM email_codes WHERE user_id = sqlc.arg(user_id) AND purpose = sqlc.arg(purpose) ORDER BY id DESC LIMIT 1;

-- name: CountTry :execrows
-- Counts one try of a code, only while it has tries left, so two tries at once never both count as the last.
UPDATE email_codes SET tries = tries + 1 WHERE id = sqlc.arg(id) AND tries < sqlc.arg(tries_allowed);

-- name: SpendCode :exec
UPDATE email_codes SET used_at = sqlc.arg(used_at) WHERE id = sqlc.arg(id);
