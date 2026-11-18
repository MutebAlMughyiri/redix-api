-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- UC-071's load says its own refusals that are no row's (redix-api #76 row 275): without them the bar showed the key itself.
INSERT INTO translation_keys (id, key, description) VALUES
    ('636d0cef-184b-47ca-87a9-38ee1563b844', 'projects.loadFile.noConnection', ''),
    ('0e62f99e-ab75-432f-82ff-509647f559b2', 'projects.loadFile.refused', ''),
    ('015004bd-644b-465b-a2eb-546af71b5974', 'projects.loadFile.projectFinished', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('568aae46-75b6-431e-8d46-84956277fbbd', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.noConnection'), 'ar', 'لا اتصال بالخادم؛ لم يُحمّل شيء'),
    ('9ef19010-d5a4-4b98-b5a0-f4f8ef6fcfd2', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.noConnection'), 'en', 'No connection to the server; nothing was loaded'),
    ('30a9d16d-6118-4696-b6c2-9fc66f83cf11', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.refused'), 'ar', 'رُفض التحميل؛ لم يُحمّل شيء'),
    ('4f940f68-1d94-4e61-8ec5-49f2c07ed2a7', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.refused'), 'en', 'The load was refused; nothing was loaded'),
    ('cc345d37-2956-400a-8113-7f78d5abbc7e', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.projectFinished'), 'ar', 'المشروع منتهٍ فلا يُحمّل إليه شيء'),
    ('785ff3eb-26b3-4983-ae21-039ac7479c73', (SELECT id FROM translation_keys WHERE key = 'projects.loadFile.projectFinished'), 'en', 'The project is finished, so nothing is loaded into it');
