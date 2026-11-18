-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The words of the links to the privacy policy, the terms of use and the packages' licences, in Settings and on registering a project
-- (redix-api #143 R7, R8).
INSERT INTO translation_keys (id, key, description) VALUES
    ('5e75e53d-b090-4a3b-86c4-afb89e4efe96', 'legal.privacy', ''),
    ('17cefbda-7872-47c1-a67b-bc0523dde383', 'legal.terms', ''),
    ('2ee55453-d05d-4511-a960-919fec8993ad', 'legal.licenses', ''),
    ('c011b32e-4722-4d02-a4bb-a9eaff35559f', 'legal.agreement', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('ee68efe6-5210-42d5-80fc-4175dc3ceb12', (SELECT id FROM translation_keys WHERE key = 'legal.privacy'), 'ar', 'سياسة الخصوصية'),
    ('e50bdf31-2a4f-410e-bd5a-5cf7a39bc86d', (SELECT id FROM translation_keys WHERE key = 'legal.privacy'), 'en', 'Privacy policy'),
    ('c18776d6-02c0-4963-81ea-95fcfc63cd8c', (SELECT id FROM translation_keys WHERE key = 'legal.terms'), 'ar', 'شروط الاستخدام'),
    ('eff6d336-ab1c-4dbc-8464-e769a6f8bb16', (SELECT id FROM translation_keys WHERE key = 'legal.terms'), 'en', 'Terms of use'),
    ('44433e70-5a3a-4a23-ab50-e4f7a07785d4', (SELECT id FROM translation_keys WHERE key = 'legal.licenses'), 'ar', 'التراخيص'),
    ('197826be-1159-4dc8-9da9-310a000d3d3f', (SELECT id FROM translation_keys WHERE key = 'legal.licenses'), 'en', 'Licenses'),
    ('b1ab378b-ae66-4801-8491-4f240e2c4036', (SELECT id FROM translation_keys WHERE key = 'legal.agreement'), 'ar', 'بالتسجيل فإنك توافق على شروط الاستخدام وسياسة الخصوصية.'),
    ('bbf40663-d657-40fe-bde1-3b48e5793129', (SELECT id FROM translation_keys WHERE key = 'legal.agreement'), 'en', 'By registering, you agree to the terms of use and the privacy policy.');
