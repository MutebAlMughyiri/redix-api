-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The word before the app's version, «الإصدار 1.0.0», at the foot of the sign-in screen and of Shared settings (redix-app #124).
INSERT INTO translation_keys (id, key, description) VALUES
    ('d00f7d94-fcaf-4b38-aa9e-b89b3fdd5078', 'app.version', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('8412a551-74ec-4813-912a-8fcc67db945b', (SELECT id FROM translation_keys WHERE key = 'app.version'), 'ar', 'الإصدار'),
    ('d7c56590-cd28-4457-97e2-556c31310692', (SELECT id FROM translation_keys WHERE key = 'app.version'), 'en', 'Version');
