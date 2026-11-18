-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- name: ProjectsOfUser :many
-- Every project a user is in, as the users routes show it.
SELECT * FROM user_projects WHERE user_id = sqlc.arg(user_id) ORDER BY project_name COLLATE names;

-- name: UsersOfProject :many
-- Every user in a project, each with this project only.
SELECT * FROM user_projects WHERE project_id = sqlc.arg(project_id);

-- name: ProjectOfUser :many
SELECT * FROM user_projects WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id);

-- name: NonAdminNamed :many
-- A user who is not an admin, by a username matched whatever its case.
SELECT * FROM users WHERE username = sqlc.arg(username) AND NOT EXISTS (SELECT 1 FROM system_permissions WHERE system_permissions.user_id = users.id);

-- name: UsersInProject :many
-- Everyone in a project, the inactive last, by name (D7).
SELECT users.* FROM users JOIN user_projects ON user_projects.user_id = users.id
WHERE user_projects.project_id = sqlc.arg(project_id)
ORDER BY user_projects.is_inactive, users.display_name COLLATE names;

-- name: Members :many
-- Everyone active in a project, by name (D7).
SELECT users.id, users.display_name FROM users
WHERE EXISTS (SELECT 1 FROM project_permissions WHERE project_permissions.project_id = sqlc.arg(project_id) AND project_permissions.user_id = users.id)
  AND NOT EXISTS (SELECT 1 FROM inactive_members WHERE inactive_members.project_id = sqlc.arg(project_id) AND inactive_members.user_id = users.id)
ORDER BY users.display_name COLLATE names;

-- name: IsAdmin :one
SELECT EXISTS (SELECT 1 FROM system_permissions WHERE user_id = sqlc.arg(user_id)) AS is_admin;

-- name: ProjectIDsOfUser :many
SELECT DISTINCT project_id FROM project_permissions WHERE user_id = sqlc.arg(user_id);

-- name: UsernameTaken :one
SELECT EXISTS (SELECT 1 FROM users WHERE username = sqlc.arg(username)) AS taken;

-- name: SetDetails :exec
UPDATE users SET display_name = sqlc.arg(display_name), mobile = sqlc.arg(mobile) WHERE id = sqlc.arg(id);

-- name: SetPassword :exec
-- A password set by a reset: the lock lifted and the wrong passwords forgotten.
UPDATE users SET password_hash = sqlc.arg(password_hash), must_change_password = sqlc.arg(must_change_password), failed_sign_ins = 0,
    locked_until = '1900-01-01T00:00:00.0000000Z' WHERE id = sqlc.arg(id);

-- name: PermissionsOf :many
SELECT * FROM project_permissions WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id);

-- name: AddPermission :exec
INSERT INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?);

-- name: RemovePermission :exec
DELETE FROM project_permissions WHERE id = sqlc.arg(id);

-- name: OtherActiveManagers :one
-- How many others manage the project's users and are active in it (ProjectManagers).
SELECT count(*) FROM project_permissions
WHERE project_permissions.project_id = sqlc.arg(project_id) AND project_permissions.permission = 'ManageUsers' AND project_permissions.user_id <> sqlc.arg(user_id)
  AND NOT EXISTS (SELECT 1 FROM inactive_members WHERE inactive_members.project_id = project_permissions.project_id AND inactive_members.user_id = project_permissions.user_id);

-- name: RoleNameOf :many
SELECT * FROM project_role_names WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id);

-- name: AddRoleName :exec
INSERT INTO project_role_names (id, project_id, user_id, role_name) VALUES (?, ?, ?, ?);

-- name: SetRoleName :exec
UPDATE project_role_names SET role_name = sqlc.arg(role_name) WHERE id = sqlc.arg(id);

-- name: RemoveRoleName :exec
DELETE FROM project_role_names WHERE id = sqlc.arg(id);

-- name: ChosenOf :many
SELECT * FROM chosen_partners WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id) ORDER BY id;

-- name: AddChosen :exec
INSERT INTO chosen_partners (id, project_id, user_id, partner_id) VALUES (?, ?, ?, ?);

-- name: RemoveChosen :exec
DELETE FROM chosen_partners WHERE id = sqlc.arg(id);

-- name: KeptPartnerIDs :many
-- Which of a project's partners, not deleted, these are.
SELECT id FROM partners WHERE project_id = sqlc.arg(project_id) AND is_deleted = 0 AND id IN (sqlc.slice(ids));

-- name: InactiveOf :many
SELECT * FROM inactive_members WHERE project_id = sqlc.arg(project_id) AND user_id = sqlc.arg(user_id);

-- name: AddInactive :exec
INSERT INTO inactive_members (id, project_id, user_id) VALUES (?, ?, ?);

-- name: RemoveInactive :exec
DELETE FROM inactive_members WHERE id = sqlc.arg(id);

-- name: AddPermissionRequest :exec
INSERT INTO permission_requests (id, project_id, user_id, gives, takes, reason, asked_at, status, rejection_reason)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);

-- name: WaitingPermissionRequestOf :many
SELECT * FROM permission_requests WHERE project_id = ? AND user_id = ? AND status = 0;

-- name: LatestPermissionRequestOf :many
SELECT * FROM permission_requests WHERE project_id = ? AND user_id = ? ORDER BY asked_at DESC LIMIT 1;

-- name: RemovePermissionRequest :exec
DELETE FROM permission_requests WHERE id = ?;

-- name: WaitingPermissionRequests :many
SELECT sqlc.embed(permission_requests), users.display_name
FROM permission_requests JOIN users ON users.id = permission_requests.user_id
WHERE permission_requests.project_id = ? AND permission_requests.status = 0
ORDER BY permission_requests.asked_at;

-- name: PermissionRequestWithID :one
SELECT * FROM permission_requests WHERE id = ?;

-- name: DecidePermissionRequest :exec
UPDATE permission_requests SET status = ?, rejection_reason = ? WHERE id = ?;
