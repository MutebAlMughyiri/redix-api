-- name: HeldEverywhere :many
-- What a person holds across the system.
SELECT permission FROM system_permissions WHERE user_id = sqlc.arg(user_id);

-- name: HeldInProjects :many
-- What a person holds in each project that is approved (Active or Finished, BR-34) and that they are not inactive in (#126 D1).
SELECT project_permissions.project_id, project_permissions.permission
FROM project_permissions JOIN projects ON projects.id = project_permissions.project_id
WHERE project_permissions.user_id = sqlc.arg(user_id) AND projects.status IN (0, 1)
  AND NOT EXISTS (SELECT 1 FROM inactive_members WHERE inactive_members.project_id = project_permissions.project_id AND inactive_members.user_id = project_permissions.user_id);

-- name: ChosenPartners :many
-- The partners chosen for a person, in each project (#106).
SELECT project_id, partner_id FROM chosen_partners WHERE user_id = sqlc.arg(user_id);
