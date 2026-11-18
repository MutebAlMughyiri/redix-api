-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- redix-api #106: a manager unticks, for one person in one project, the tables they may not see. What is kept is what is not seen (D5), so
-- everyone sees every table until unticked, a new table included (D1, R8).
CREATE TABLE unseen_groups (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    subgroup_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id),
    FOREIGN KEY (subgroup_id) REFERENCES subgroups (id) ON DELETE CASCADE
) STRICT;
CREATE UNIQUE INDEX ix_unseen_groups_project_id_user_id_subgroup_id ON unseen_groups (project_id, user_id, subgroup_id);
CREATE INDEX ix_unseen_groups_user_id ON unseen_groups (user_id);
CREATE INDEX ix_unseen_groups_subgroup_id ON unseen_groups (subgroup_id);

CREATE TABLE unseen_contracts (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    contract_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id),
    FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE CASCADE
) STRICT;
CREATE UNIQUE INDEX ix_unseen_contracts_project_id_user_id_contract_id ON unseen_contracts (project_id, user_id, contract_id);
CREATE INDEX ix_unseen_contracts_user_id ON unseen_contracts (user_id);
CREATE INDEX ix_unseen_contracts_contract_id ON unseen_contracts (contract_id);

-- An entry's table, the one owner of it every sum reads (#106 G1): a contractor's payment is its contract's, though it goes under its item's general
-- group; any other expense or revenue its group's. A capital movement has none: its partner is the capital ticks' (R14).
CREATE VIEW entry_tables AS
SELECT entry_id, contract_id AS table_id FROM contractor_payment_details
UNION ALL
SELECT entry_id, subgroup_id FROM expense_details WHERE entry_id NOT IN (SELECT entry_id FROM contractor_payment_details)
UNION ALL
SELECT entry_id, subgroup_id FROM revenue_details;

-- user_projects tells each person, beside what they hold, the tables they do not see.
DROP VIEW user_projects;
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
               WHERE inactive_members.project_id = project_permissions.project_id AND inactive_members.user_id = project_permissions.user_id) AS is_inactive,
       (SELECT json_group_array(table_id) FROM (
                 SELECT subgroup_id AS table_id FROM unseen_groups
                 WHERE unseen_groups.project_id = project_permissions.project_id AND unseen_groups.user_id = project_permissions.user_id
                 UNION ALL
                 SELECT contract_id FROM unseen_contracts
                 WHERE unseen_contracts.project_id = project_permissions.project_id AND unseen_contracts.user_id = project_permissions.user_id
                 ORDER BY 1)) AS unseen_tables
FROM project_permissions JOIN projects ON projects.id = project_permissions.project_id
GROUP BY project_permissions.user_id, project_permissions.project_id;

INSERT INTO translation_keys (id, key, description) VALUES
    ('31288785-bae9-4623-aa30-874dab684b97', 'users.tables', ''),
    ('281a223c-b898-407a-906e-2334dd075739', 'users.tablesHint', ''),
    ('257c49c1-f8d5-4749-9b5f-5db9ea477e5e', 'users.unknownTable', ''),
    ('9e59dbaa-f770-470c-8ebd-32ff1cdf1da4', 'permissions.mineTables', ''),
    ('ac753fb1-6709-4d77-8f0b-16a0207d0347', 'reports.periodicNotForYou', ''),
    ('9bfa65a7-fb6d-4002-8b54-333684a936db', 'history.thing.tableSight', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('12ebd38f-3430-4ff3-bdbc-5f984e72634a', (SELECT id FROM translation_keys WHERE key = 'users.tables'), 'ar', 'الجداول'),
    ('7ffb00c0-1619-4adf-9405-867374a33107', (SELECT id FROM translation_keys WHERE key = 'users.tables'), 'en', 'Tables'),
    ('6f8db9fd-47c5-4bdc-96ae-b1cf05efab7a', (SELECT id FROM translation_keys WHERE key = 'users.tablesHint'), 'ar', 'ما لا تُعلَّم عليه لا يراه الشخص: لا قيوده ولا مجموعه ولا ما حرّكه من الحسابات.'),
    ('e059ea10-a108-49cd-8342-6284e2f65508', (SELECT id FROM translation_keys WHERE key = 'users.tablesHint'), 'en', 'What is not ticked, the person does not see: not its entries, not its total, not what it moved in the accounts.'),
    ('89b427a1-4c56-4b85-a899-83650a565fe6', (SELECT id FROM translation_keys WHERE key = 'users.unknownTable'), 'ar', 'جدول ليس من جداول المشروع'),
    ('1bcc4d05-e677-42e9-86e6-836a257759da', (SELECT id FROM translation_keys WHERE key = 'users.unknownTable'), 'en', 'A table that is not one of the project''s'),
    ('e3a867d5-7aaf-4f8d-80da-b9f3ce5dab61', (SELECT id FROM translation_keys WHERE key = 'permissions.mineTables'), 'ar', 'الجداول التي تراها'),
    ('6538dccf-4267-48c2-aff7-0fd74133f7ef', (SELECT id FROM translation_keys WHERE key = 'permissions.mineTables'), 'en', 'The tables you see'),
    ('60327096-cac3-4121-8f9a-9aaa6645d3fd', (SELECT id FROM translation_keys WHERE key = 'reports.periodicNotForYou'), 'ar', 'التقرير الدوري لمن يرى كل الجداول'),
    ('74b2155d-4d6b-4f77-b35c-6e916752e0dc', (SELECT id FROM translation_keys WHERE key = 'reports.periodicNotForYou'), 'en', 'The periodic report is for those who see every table'),
    ('c4a2e263-2f5e-4304-9ce8-3d19c06787a9', (SELECT id FROM translation_keys WHERE key = 'history.thing.tableSight'), 'ar', 'رؤية الجدول'),
    ('328019cd-c4bf-4c6a-8dfa-12afda0f325d', (SELECT id FROM translation_keys WHERE key = 'history.thing.tableSight'), 'en', 'sight of a table');
