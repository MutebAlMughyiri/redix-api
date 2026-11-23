-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The words of the web's Expenses, Revenue and item reports as the owner approved their redesign on 23 Nov 2026: the grid's headings, the counts
-- beside a total, an item's tables and the way back above it.
INSERT INTO translation_keys (id, key, description) VALUES
    ('c5a05123-2bc5-4b79-8540-d4289659134a', 'reports.categoryAndItem', ''),
    ('a690ace5-aa5a-40e1-9918-d9e8b0fd9da7', 'reports.total', ''),
    ('f681d49a-1e36-4a2f-8837-fbcf3ed32bbb', 'reports.ofSide', ''),
    ('8c3dfa54-0b16-4fef-a9dd-26c49d6b6e60', 'reports.itemsCount', ''),
    ('381e29b3-362a-4fd3-ae72-ae6bc011bc29', 'reports.entriesCount', ''),
    ('b9db25ff-a436-4959-8acb-68eb35c88c41', 'reports.tablesCount', ''),
    ('8d1ec318-efbf-4a03-b9b0-546b49b22707', 'reports.tables', ''),
    ('b651a37a-2bdc-429f-814e-39b8be846caa', 'reports.table', ''),
    ('0b42d0aa-cd5e-4a20-b6e9-673a0aab8c20', 'reports.lastEntry', ''),
    ('5c222123-6fd7-4184-ae5a-a6be99499ff5', 'reports.ofItem', ''),
    ('7871b4da-326f-4a88-9a6c-431cf7ad7115', 'reports.payments', ''),
    ('015c38a8-11a5-4c2b-9ad7-0fcc85c93934', 'reports.path', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('01907ce2-08fd-401e-af58-1ad803e959f0', (SELECT id FROM translation_keys WHERE key = 'reports.categoryAndItem'), 'ar', 'الفئة والبند'),
    ('ccf74b2c-9671-4777-adeb-fc32e2cc066c', (SELECT id FROM translation_keys WHERE key = 'reports.categoryAndItem'), 'en', 'Category and item'),
    ('1f5bf3d0-b591-4237-ba22-b4d9748ab739', (SELECT id FROM translation_keys WHERE key = 'reports.total'), 'ar', 'الإجمالي'),
    ('3c8984f1-c25c-4f0d-a0ac-b82e115efa4b', (SELECT id FROM translation_keys WHERE key = 'reports.total'), 'en', 'Total'),
    ('c427f70c-e977-4e53-b4a9-ae30f3d68abe', (SELECT id FROM translation_keys WHERE key = 'reports.ofSide'), 'ar', 'من {0}'),
    ('ce6ef3e2-3a2d-4c5d-b284-5dbb71a3c0a4', (SELECT id FROM translation_keys WHERE key = 'reports.ofSide'), 'en', 'Of {0}'),
    ('10e31b96-bce3-4a46-940c-a713d9c67dbb', (SELECT id FROM translation_keys WHERE key = 'reports.itemsCount'), 'ar', 'البنود: {0}'),
    ('121d216b-6214-456e-b7c0-0cd2aad302af', (SELECT id FROM translation_keys WHERE key = 'reports.itemsCount'), 'en', 'Items: {0}'),
    ('2357ac56-0d6c-463f-a23c-8d610c2f7f18', (SELECT id FROM translation_keys WHERE key = 'reports.entriesCount'), 'ar', 'القيود: {0}'),
    ('5c2b03a3-5de4-4e98-b98a-8b97dd2138a6', (SELECT id FROM translation_keys WHERE key = 'reports.entriesCount'), 'en', 'Entries: {0}'),
    ('e0e580cd-74cf-4801-859e-6857f625c3fa', (SELECT id FROM translation_keys WHERE key = 'reports.tablesCount'), 'ar', 'الجداول: {0}'),
    ('2e5f26bc-6672-4464-9599-29f3c58ccfb6', (SELECT id FROM translation_keys WHERE key = 'reports.tablesCount'), 'en', 'Tables: {0}'),
    ('94bb8745-7a13-4aab-8090-6e5afedcd3ec', (SELECT id FROM translation_keys WHERE key = 'reports.tables'), 'ar', 'الجداول'),
    ('d1e2379f-a984-4421-94fb-c57ad55d57e1', (SELECT id FROM translation_keys WHERE key = 'reports.tables'), 'en', 'Tables'),
    ('876412aa-f775-4636-86ba-e9adccd09ce3', (SELECT id FROM translation_keys WHERE key = 'reports.table'), 'ar', 'الجدول'),
    ('4db26226-b0e4-4f79-a507-a0da0b3f78aa', (SELECT id FROM translation_keys WHERE key = 'reports.table'), 'en', 'Table'),
    ('1525e16b-38ce-4285-b69c-f42c030f0697', (SELECT id FROM translation_keys WHERE key = 'reports.lastEntry'), 'ar', 'آخر قيد'),
    ('c60dc26a-e256-4647-b105-b53307114aa3', (SELECT id FROM translation_keys WHERE key = 'reports.lastEntry'), 'en', 'Last entry'),
    ('1ed7a938-faa7-4b16-9def-5dae5dce85bb', (SELECT id FROM translation_keys WHERE key = 'reports.ofItem'), 'ar', 'من البند'),
    ('b3f4a684-142b-47d9-b3e4-3d61e4c60bc5', (SELECT id FROM translation_keys WHERE key = 'reports.ofItem'), 'en', 'Of the item'),
    ('97805f3f-96fc-488f-94a0-d017be8372e1', (SELECT id FROM translation_keys WHERE key = 'reports.payments'), 'ar', 'الدفعات'),
    ('9a4f8dce-15af-40ca-9693-7badf3d4bef9', (SELECT id FROM translation_keys WHERE key = 'reports.payments'), 'en', 'Payments'),
    ('6d693837-1a54-47fb-8f43-04e57b943b17', (SELECT id FROM translation_keys WHERE key = 'reports.path'), 'ar', 'المسار'),
    ('2a62e712-fb37-4052-bf1b-5945902d3385', (SELECT id FROM translation_keys WHERE key = 'reports.path'), 'en', 'Path');
