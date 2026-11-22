-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The largest request the API and the web read, in megabytes: the largest file a person may send is 25 MB (fileTooLarge), with room for its
-- form (redix-api #145 R17), kept here so it changes without a release (the owner, 23 Nov 2026).
INSERT INTO config_values (id, key, value) VALUES
    ('9887ca87-8378-4eb6-9b04-2780d9d0e50f', 'requestSizeMegabytes', '30');
