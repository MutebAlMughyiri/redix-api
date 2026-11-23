-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The words of a person's last sign-in on the app's Users screen and the web's Users page (redix-api #182).
INSERT INTO translation_keys (id, key, description) VALUES
    ('d93a9e21-c255-48ee-90df-c9dead2093ee', 'users.lastSeen', ''),
    ('cd6de0ad-43cf-4dc9-bb24-a956497d0912', 'users.neverSignedIn', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('c0771b42-17ba-48ab-bbb0-37c783d0d52c', (SELECT id FROM translation_keys WHERE key = 'users.lastSeen'), 'ar', 'آخر دخول {0}'),
    ('9bd21b0c-ea54-489a-a6fa-c753f8018580', (SELECT id FROM translation_keys WHERE key = 'users.lastSeen'), 'en', 'Last signed in {0}'),
    ('e059ece4-c923-4704-bb17-49efef965779', (SELECT id FROM translation_keys WHERE key = 'users.neverSignedIn'), 'ar', 'لم يدخل بعد'),
    ('80cd48bc-4e5e-4f79-a41d-6109f9ca675a', (SELECT id FROM translation_keys WHERE key = 'users.neverSignedIn'), 'en', 'Not signed in yet');

