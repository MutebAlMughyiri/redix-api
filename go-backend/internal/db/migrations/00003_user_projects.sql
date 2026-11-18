-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- What a person holds in each project they are in, as every users route shows it (UsersEndpoints.ProjectsByUserAsync, redix-api #140): the
-- project's name, the permissions, the free-text role name, the partners chosen for them in order, and whether they are inactive there.
CREATE VIEW user_projects AS
SELECT project_permissions.user_id,
       project_permissions.project_id,
       projects.name AS project_name,
       json_group_array(project_permissions.permission) AS permissions,
       COALESCE((SELECT role_name FROM project_role_names
                 WHERE project_role_names.project_id = project_permissions.project_id AND project_role_names.user_id = project_permissions.user_id), '') AS role_name,
       (SELECT json_group_array(partner_id) FROM (SELECT partner_id FROM chosen_partners
                 WHERE chosen_partners.project_id = project_permissions.project_id AND chosen_partners.user_id = project_permissions.user_id ORDER BY id)) AS partner_ids,
       EXISTS (SELECT 1 FROM inactive_members
               WHERE inactive_members.project_id = project_permissions.project_id AND inactive_members.user_id = project_permissions.user_id) AS is_inactive
FROM project_permissions JOIN projects ON projects.id = project_permissions.project_id
GROUP BY project_permissions.user_id, project_permissions.project_id;

-- +goose Down
DROP VIEW user_projects;
