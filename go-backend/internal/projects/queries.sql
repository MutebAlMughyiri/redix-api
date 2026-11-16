-- name: ProjectWithID :one
SELECT * FROM projects WHERE id = sqlc.arg(id);

-- name: AllProjects :many
-- Every project, the active first, then the finished, the pending and the rejected, the latest start first (ProjectsEndpoints.List).
SELECT * FROM projects ORDER BY status, start_date DESC, id;

-- name: UnitsOf :many
SELECT * FROM units WHERE project_id = sqlc.arg(project_id) ORDER BY sort_order;

-- name: AllUnits :many
SELECT * FROM units ORDER BY project_id, sort_order;

-- name: ProjectsHoldingEntries :many
-- The projects that hold an entry, whatever it is and wherever it stands.
SELECT DISTINCT project_id FROM entries;

-- name: ProjectsOfUserWithRows :many
-- The projects a person holds a permission row in, approved or not.
SELECT DISTINCT project_id FROM project_permissions WHERE user_id = sqlc.arg(user_id);

-- name: RoleNamesOfUser :many
SELECT project_id, role_name FROM project_role_names WHERE user_id = sqlc.arg(user_id);

-- name: InactiveProjectsOf :many
SELECT project_id FROM inactive_members WHERE user_id = sqlc.arg(user_id);

-- name: AddProject :exec
INSERT INTO projects (id, name, start_date, status, rejection_reason, description, is_template, last_entry_number) VALUES (?, ?, ?, ?, '', ?, 0, 0);

-- name: SetProjectName :exec
UPDATE projects SET name = sqlc.arg(name) WHERE id = sqlc.arg(id);

-- name: SetProjectDescription :exec
UPDATE projects SET description = sqlc.arg(description) WHERE id = sqlc.arg(id);

-- name: SetProjectStatus :exec
UPDATE projects SET status = sqlc.arg(status), rejection_reason = sqlc.arg(rejection_reason) WHERE id = sqlc.arg(id);

-- name: AddUnit :exec
INSERT INTO units (id, project_id, name, sort_order) VALUES (?, ?, ?, ?);

-- name: SetUnitName :exec
UPDATE units SET name = sqlc.arg(name) WHERE id = sqlc.arg(id);

-- name: RemoveUnit :exec
DELETE FROM units WHERE id = sqlc.arg(id);

-- name: AddStructureSource :exec
INSERT INTO project_structure_sources (project_id, structure_source_id) VALUES (?, ?);

-- name: AddPartner :exec
INSERT INTO partners (id, project_id, name, sort_order, is_deleted) VALUES (?, ?, ?, ?, 0);

-- name: HasEntriesWaiting :one
SELECT EXISTS (SELECT 1 FROM entries WHERE project_id = sqlc.arg(project_id) AND status = 1) AS waiting;

-- name: SetDefaultProject :exec
INSERT INTO user_default_projects (user_id, project_id) VALUES (?, ?) ON CONFLICT (user_id) DO UPDATE SET project_id = excluded.project_id;

-- name: RemoveDefaultProject :exec
DELETE FROM user_default_projects WHERE user_id = sqlc.arg(user_id);

-- name: PendingProjects :many
-- The projects waiting for the admin, the earliest start first, by name (D7).
SELECT * FROM projects WHERE status = 2 ORDER BY start_date, name COLLATE names;

-- name: ManagersOfPending :many
-- Who manages the users of each project waiting for the admin.
SELECT project_permissions.project_id, users.display_name, users.username, users.mobile
FROM project_permissions JOIN users ON users.id = project_permissions.user_id JOIN projects ON projects.id = project_permissions.project_id
WHERE project_permissions.permission = 'ManageUsers' AND projects.status = 2 ORDER BY project_permissions.id;
