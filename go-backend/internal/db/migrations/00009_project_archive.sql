-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- UC-073 (redix-api #130): ManageProjectFiles, «ملفات المشروع», opens a project's archive, its retrieve and UC-071's load (D2).
-- project_permissions is rebuilt for its CHECK, as 00008 did, with user_projects around it.
DROP VIEW user_projects;
CREATE TABLE project_permissions_new (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    permission TEXT CHECK (length(permission) <= 30) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id),
    CHECK ((permission='GenerateReports' OR permission='SeeReports' OR permission='MoveMoney' OR permission='ApproveEntries' OR permission='AttachFiles' OR permission='RemoveApprovedEntries' OR permission='DeleteEntries' OR permission='ManageAnyEntry' OR permission='RecordEntries' OR permission='ChargeAnyAccount' OR permission='AddSupplier' OR permission='ManageSettings' OR permission='ManageStructure' OR permission='ManageProject' OR permission='SeeChosenCapital' OR permission='SeeAllCapital' OR permission='ReadProject' OR permission='ManageUsers' OR permission='ApproveProjects' OR permission='SeeProjectHistory' OR permission='ManageProjectFiles') AND permission<>'ApproveProjects')
) STRICT;
INSERT INTO project_permissions_new (id, project_id, user_id, permission) SELECT id, project_id, user_id, permission FROM project_permissions;
DROP TABLE project_permissions;
ALTER TABLE project_permissions_new RENAME TO project_permissions;
CREATE UNIQUE INDEX ix_project_permissions_project_id_user_id_permission ON project_permissions (project_id, user_id, permission);
CREATE INDEX ix_project_permissions_user_id ON project_permissions (user_id);

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

-- Nobody loses what they could do (R18, D2): the load needed ManageProject, so whoever holds it in a project holds ManageProjectFiles there.
INSERT INTO project_permissions (id, project_id, user_id, permission)
SELECT lower(substr(h, 1, 8) || '-' || substr(h, 9, 4) || '-' || substr(h, 13, 4) || '-' || substr(h, 17, 4) || '-' || substr(h, 21, 12)),
       project_id, user_id, 'ManageProjectFiles'
FROM (SELECT hex(randomblob(16)) AS h, project_id, user_id FROM project_permissions WHERE permission = 'ManageProject');

-- History says what was done with them (D9): a project archived, and retrieved from an archive.
INSERT INTO lk_history_action (id, name) VALUES (25, 'Archive'), (26, 'Retrieve');

-- The words of «أرشفة المشروع», «استرجاع المشروع», their History lines and the tick; «تصدير إعدادات المشروع», the grayed button's, goes (D10).
DELETE FROM translations WHERE translation_key_id = (SELECT id FROM translation_keys WHERE key = 'projects.exportSettings');
DELETE FROM translation_keys WHERE key = 'projects.exportSettings';

INSERT INTO translation_keys (id, key, description) VALUES
    ('737642cd-2e28-4086-8123-ee80c4c096dd', 'permissions.manageProjectFiles', ''),
    ('e47cc54c-5905-4022-a16c-f8ad930dcbe6', 'permissions.manageProjectFilesHint', ''),
    ('1fc95f06-800e-4e5f-aa39-24c3e71a23ac', 'projects.archive', ''),
    ('6b7ce598-b403-4182-b320-58425fd9606f', 'projects.retrieve', ''),
    ('3559d62f-16e0-4784-a335-d5f1725f5cc6', 'projects.archiveFileName', ''),
    ('b9546793-8271-4030-9e64-e5dad9ab8247', 'projects.retrieveConfirm', ''),
    ('ec7ad729-37d5-4cb5-acde-37d96fd6f7d6', 'projects.retrieveTakenBy', ''),
    ('609dd9f3-6390-4f43-9052-9a9a168db13e', 'projects.retrieveNow', ''),
    ('06f0b2c3-a904-4998-9b02-486c9a96cd33', 'projects.retrieveAfter', ''),
    ('931ad62e-a460-4f04-895e-d01b4bf31ccb', 'projects.retrieve.part.entries', ''),
    ('34cc1c45-fed7-432a-b70f-07ff9347eca3', 'projects.retrieve.part.contracts', ''),
    ('394dc00f-94b1-4f3e-8ef2-03fb353fcefe', 'projects.retrieve.part.accounts', ''),
    ('bd4250ed-feac-4610-892a-1b072a18531d', 'projects.retrieve.part.suppliers', ''),
    ('4088eb16-efc9-4798-aff0-b875c2eb74cc', 'projects.retrieve.part.partners', ''),
    ('eecd5d7f-11dc-4ff8-b322-91c286b54dc2', 'projects.retrieve.part.items', ''),
    ('49907853-bcf9-48b0-a6df-3355a94c746d', 'projects.retrieve.part.attachments', ''),
    ('09d97359-f7be-4d2c-8018-e5543e3f69d9', 'projects.retrieveWhat', ''),
    ('7fef4e5e-ca67-4af9-9ff0-385c779be5e1', 'projects.retrieveDone', ''),
    ('fdb20ced-47ac-4581-a92f-0f1031c18883', 'projects.retrieve.notAnArchive', ''),
    ('82979389-1895-49e4-a472-5ecd08561779', 'projects.retrieve.archiveOfAnotherProject', ''),
    ('a8632436-12de-4570-a611-4e7fe62655a4', 'projects.retrieve.newerArchive', ''),
    ('072df49b-d3b9-42c5-96b8-daae093ae6ce', 'projects.retrieve.archiveDoesNotFit', ''),
    ('51308445-15b0-4445-80ac-4d8728787736', 'projects.retrieve.noConnection', ''),
    ('eb408201-eb24-45cc-9d2a-392400edecc4', 'projects.retrieve.refused', ''),
    ('e7a32fbb-a521-4118-8d54-9105354a3bf2', 'projects.retrieve.projectFinished', ''),
    ('9e49e282-e2a1-414d-b9c3-b336e2d5423d', 'history.action.archived', ''),
    ('95278bae-a6a9-4441-94a8-81dd9b5d2162', 'history.action.retrieved', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('3ee8049a-67ec-4ec6-a9d8-707a6d50c60c', (SELECT id FROM translation_keys WHERE key = 'permissions.manageProjectFiles'), 'ar', 'ملفات المشروع'),
    ('443551c1-098d-4268-946c-a0001f87368d', (SELECT id FROM translation_keys WHERE key = 'permissions.manageProjectFiles'), 'en', 'Project files'),
    ('802d6e29-9254-4c30-9511-eec18c8bf410', (SELECT id FROM translation_keys WHERE key = 'permissions.manageProjectFilesHint'), 'ar', 'يأخذ أرشيف المشروع كاملًا ويسترجعه فوقه، ويحمّل ملف مشروع من Excel. في الأرشيف كل شيء، ومنه رأس مال كل شريك والسجل.'),
    ('ba8883ab-868a-49a3-bb6b-1847df43a1ed', (SELECT id FROM translation_keys WHERE key = 'permissions.manageProjectFilesHint'), 'en', 'Takes the whole project''s archive and retrieves it over the project, and loads a project file from Excel. The archive holds everything, every partner''s capital and the history included.'),
    ('4a5620bd-f2b9-49f9-9357-5c4dbf018bed', (SELECT id FROM translation_keys WHERE key = 'projects.archive'), 'ar', 'أرشفة المشروع'),
    ('62c5411f-1227-40b0-906a-8630f9a60574', (SELECT id FROM translation_keys WHERE key = 'projects.archive'), 'en', 'Archive the project'),
    ('9dde75d7-c6f7-4bf0-a770-4cb5cce075de', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve'), 'ar', 'استرجاع المشروع'),
    ('6cee16ac-94fd-4f5b-b4ab-43ce015b9284', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve'), 'en', 'Retrieve the project'),
    ('3362ce9f-4575-41f2-8c0b-d7a40fe17788', (SELECT id FROM translation_keys WHERE key = 'projects.archiveFileName'), 'ar', '{0} - أرشيف {1}'),
    ('61467892-d2dc-4cfb-8ccc-1061681e38ea', (SELECT id FROM translation_keys WHERE key = 'projects.archiveFileName'), 'en', '{0} - archive {1}'),
    ('6919c3bd-7e0f-4270-b052-3aec2e1ebd35', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveConfirm'), 'ar', 'استرجاع {0} من أرشيف {1}؟'),
    ('6373e4c7-14bb-44ac-8d92-ed8eb13d7bc6', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveConfirm'), 'en', 'Retrieve {0} from its archive of {1}?'),
    ('649357ff-eacb-4927-873d-67e02b34ca9d', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveTakenBy'), 'ar', 'أخذه {0}.'),
    ('7eed39df-bda7-421f-afdd-dd23a504359c', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveTakenBy'), 'en', 'Taken by {0}.'),
    ('af3701fa-5aa6-4714-97e7-7c98603bfeb1', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveNow'), 'ar', 'الآن'),
    ('00f5aa60-9a00-483e-8bd8-56e5b503efd7', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveNow'), 'en', 'Now'),
    ('358f1080-95ef-44fd-b1c1-2e2a4ea3f2bd', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveAfter'), 'ar', 'بعد الاسترجاع'),
    ('5c5b5e4d-1a6a-4da3-b0b5-8b8fe0729ab4', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveAfter'), 'en', 'After'),
    ('f933a584-d1f6-4ce9-bac0-00a681d0b20d', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.entries'), 'ar', 'القيود'),
    ('c0b9eb0e-15eb-430b-a427-bfc17c47017c', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.entries'), 'en', 'Entries'),
    ('c9d8fffb-3582-4a77-9648-1edb1c7e4706', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.contracts'), 'ar', 'العقود'),
    ('916920e9-8046-45e0-96a2-09b22255be1d', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.contracts'), 'en', 'Contracts'),
    ('aad9237c-b600-4cda-8b4a-af224961eba4', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.accounts'), 'ar', 'الحسابات'),
    ('98d5a07f-aae6-4609-bebc-0f48e3f463c3', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.accounts'), 'en', 'Accounts'),
    ('bc25ab4b-d307-41fb-92f4-ab0af697bbda', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.suppliers'), 'ar', 'الموردون'),
    ('4ba4bdd0-b554-4707-9b8b-160828f74ae8', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.suppliers'), 'en', 'Suppliers'),
    ('03b788cb-8a50-4d39-a207-bc323a575f7b', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.partners'), 'ar', 'الشركاء'),
    ('62023404-fa06-4546-8d0d-37cc910f26a2', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.partners'), 'en', 'Partners'),
    ('7e12ae81-abc4-42f4-a542-feca809722ff', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.items'), 'ar', 'البنود'),
    ('c5d8b96b-f66f-4df1-ad32-a6642cc47708', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.items'), 'en', 'Items'),
    ('e6cf8045-9dd0-45b6-8e3b-6aba78c95d8b', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.attachments'), 'ar', 'الملفات'),
    ('c595f3a7-4942-452c-873a-f16c6a68aa85', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.part.attachments'), 'en', 'Files'),
    ('0bdb171f-0986-4321-a2d8-45c833ab0faa', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveWhat'), 'ar', 'يُستبدل كل ما في المشروع بما في الأرشيف. يبقى السجل والتقارير الصادرة والأعضاء وصلاحياتهم.'),
    ('55328f7a-0bc6-4bef-96a6-f927a7914525', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveWhat'), 'en', 'Everything in the project is replaced by the archive''s. The history, the issued reports, the members and what they hold stay.'),
    ('a11777f8-3b97-4b17-b167-35ba8a609b2f', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveDone'), 'ar', 'استُرجع المشروع من أرشيف {0}: {1} قيد'),
    ('82eb716f-c458-4e91-bfc1-56ea18a948cd', (SELECT id FROM translation_keys WHERE key = 'projects.retrieveDone'), 'en', 'The project was retrieved from its archive of {0}: {1} entries'),
    ('8d709d53-343d-464a-b68a-bde6c7368137', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.notAnArchive'), 'ar', 'هذا الملف ليس أرشيف مشروع؛ لم يُسترجع شيء'),
    ('0884271c-9a95-42ab-a726-f0265e9d715f', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.notAnArchive'), 'en', 'This file is not a project archive; nothing was retrieved'),
    ('ca0e94cd-4406-4eaf-b6ef-73e5745f88fc', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.archiveOfAnotherProject'), 'ar', 'هذا أرشيف مشروع {0}، لا هذا المشروع؛ لم يُسترجع شيء'),
    ('201abf42-884d-4d7d-ad08-4ba962d428e2', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.archiveOfAnotherProject'), 'en', 'This is the archive of {0}, not of this project; nothing was retrieved'),
    ('0dd6aa9d-3aef-4824-8ee1-c887cbda58fa', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.newerArchive'), 'ar', 'أُخذ هذا الأرشيف على إصدار من REDIX أحدث من الخادم؛ لم يُسترجع شيء'),
    ('f45a5181-79ac-40e7-b1da-c940689633e9', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.newerArchive'), 'en', 'This archive was taken on a newer REDIX than the server''s; nothing was retrieved'),
    ('5bc456c0-299f-4a76-aa60-c8a696cbd77e', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.archiveDoesNotFit'), 'ar', 'في الأرشيف ما يسمّي شيئًا لا يجده الخادم؛ لم يُسترجع شيء'),
    ('05ec33e5-a6ab-44b3-99f3-a031cdde75da', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.archiveDoesNotFit'), 'en', 'The archive names something this server does not have; nothing was retrieved'),
    ('fb273bcd-9d8f-47ab-9c23-4ae3afabfca5', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.noConnection'), 'ar', 'لا اتصال بالخادم؛ لم يُسترجع شيء'),
    ('e1ce4507-6d14-4976-83d2-36b98894508d', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.noConnection'), 'en', 'No connection to the server; nothing was retrieved'),
    ('e62abe0a-dd7b-4b8c-8181-bf9ebe5e8386', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.refused'), 'ar', 'رُفض الاسترجاع؛ لم يُسترجع شيء'),
    ('39bec605-e52d-4dcb-bddb-331006adae99', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.refused'), 'en', 'The retrieve was refused; nothing was retrieved'),
    ('44df2796-d7b3-459d-b81c-fe33aadfe4c7', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.projectFinished'), 'ar', 'المشروع منتهٍ فلا يُسترجع'),
    ('efc3ee6c-f59a-4b9e-82ba-3b8e8169e8ea', (SELECT id FROM translation_keys WHERE key = 'projects.retrieve.projectFinished'), 'en', 'The project is finished, so it cannot be retrieved'),
    ('0b4ab42c-5a91-4d5b-a83f-3d954191f825', (SELECT id FROM translation_keys WHERE key = 'history.action.archived'), 'ar', 'أرشف'),
    ('1e4a708c-65b3-43ca-a142-7f0f5c326e11', (SELECT id FROM translation_keys WHERE key = 'history.action.archived'), 'en', 'archived'),
    ('cf0475d6-3fd7-4002-b86f-583f32bafe21', (SELECT id FROM translation_keys WHERE key = 'history.action.retrieved'), 'ar', 'استرجع'),
    ('1de54613-a467-4bf4-a325-826b173df837', (SELECT id FROM translation_keys WHERE key = 'history.action.retrieved'), 'en', 'retrieved');

