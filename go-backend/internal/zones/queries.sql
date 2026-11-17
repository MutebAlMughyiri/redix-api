-- name: ProjectTimeZone :one
-- The zone a project's manager chose (redix-app #107); none for one still in Riyadh.
SELECT time_zone FROM project_time_zones WHERE project_id = sqlc.arg(project_id);

-- name: ChosenTimeZones :many
-- Every zone a project's manager chose, for the lists of projects.
SELECT project_id, time_zone FROM project_time_zones;

-- name: SetProjectTimeZone :exec
INSERT INTO project_time_zones (project_id, time_zone) VALUES (sqlc.arg(project_id), sqlc.arg(time_zone))
ON CONFLICT (project_id) DO UPDATE SET time_zone = excluded.time_zone;
