-- name: PartnerLogins :many
SELECT * FROM partner_users WHERE project_id = ?;

-- name: SavePartner :exec
INSERT INTO partners (id, project_id, name, sort_order, is_deleted) VALUES (?, ?, ?, ?, ?)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, sort_order = excluded.sort_order, is_deleted = excluded.is_deleted;

-- name: SavePartnerLogin :exec
INSERT INTO partner_users (partner_id, project_id, user_id) VALUES (?, ?, ?) ON CONFLICT (partner_id) DO UPDATE SET user_id = excluded.user_id;

-- name: RemovePartnerLogin :exec
DELETE FROM partner_users WHERE partner_id = ?;

-- name: CapitalMovements :many
-- Every capital movement of the project, by its entry's status, with its partner.
SELECT capital_movement_details.partner_id, entries.*
FROM capital_movement_details JOIN entries ON entries.id = capital_movement_details.entry_id
WHERE entries.project_id = ? ORDER BY entries.entry_date DESC, entries.number DESC;
