-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The words of the web's filter buttons as the owner approved their design on 23 Nov 2026 (redix-api #183): how many choices are chosen,
-- the list's None, and the period's from and to.
INSERT INTO translation_keys (id, key, description) VALUES
    ('43ee3683-dd47-4814-92a0-e4bb8e6970a2', 'filters.chosenCount', ''),
    ('e10ad37a-b649-4f84-bdc5-02daf5391d1c', 'filters.none', ''),
    ('a39a52c1-d986-4445-bbe5-77e0521cd632', 'filters.from', ''),
    ('bb6e8961-bdd7-45ea-96aa-e1db69214a30', 'filters.to', ''),
    ('2d3dcae0-2271-4fc6-af56-6a1599836b89', 'filters.since', ''),
    ('bca0f2c9-8898-418a-bc98-a28e9e37a5ff', 'filters.until', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('c6d80d67-ecda-446a-bfc4-97dcbc062080', (SELECT id FROM translation_keys WHERE key = 'filters.chosenCount'), 'ar', '{0} مختارة'),
    ('fc1b2185-2ad6-44eb-817a-746d9ebd14b0', (SELECT id FROM translation_keys WHERE key = 'filters.chosenCount'), 'en', '{0} chosen'),
    ('882aecad-0380-404f-bc33-2cd33c9a8c34', (SELECT id FROM translation_keys WHERE key = 'filters.none'), 'ar', 'لا شيء'),
    ('c2519cc7-814f-48e3-ac31-1328a1b46144', (SELECT id FROM translation_keys WHERE key = 'filters.none'), 'en', 'None'),
    ('4e49dcb6-8f3e-4547-acd8-32ef16eee50c', (SELECT id FROM translation_keys WHERE key = 'filters.from'), 'ar', 'من'),
    ('3ee8c975-55fe-42b3-a29f-ad8db066c71c', (SELECT id FROM translation_keys WHERE key = 'filters.from'), 'en', 'From'),
    ('ac81fe69-7b44-4187-87ea-f85e3688764d', (SELECT id FROM translation_keys WHERE key = 'filters.to'), 'ar', 'إلى'),
    ('23730d9c-aaa5-4f42-bd7a-bea49499bed2', (SELECT id FROM translation_keys WHERE key = 'filters.to'), 'en', 'To'),
    ('71a60474-4352-472e-964d-c70a8a150425', (SELECT id FROM translation_keys WHERE key = 'filters.since'), 'ar', 'من {0}'),
    ('b4bdc035-f4fb-4c0b-a792-4b7b54c62feb', (SELECT id FROM translation_keys WHERE key = 'filters.since'), 'en', 'From {0}'),
    ('f10cb1b1-007d-4f6b-bf17-e2c694f383bd', (SELECT id FROM translation_keys WHERE key = 'filters.until'), 'ar', 'إلى {0}'),
    ('8b0e81a7-f7b6-4583-8848-248e389acd79', (SELECT id FROM translation_keys WHERE key = 'filters.until'), 'en', 'Until {0}');
