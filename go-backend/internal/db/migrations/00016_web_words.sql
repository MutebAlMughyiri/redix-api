-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The words only the web's shell shows (redix-api #161 S13): its side menu's Home, its View menu and what it hides, folding the side menu, closing a
-- tab, and the content area before a tab is open. The shortcut, (Alt+1), is no word: the page adds it.
INSERT INTO translation_keys (id, key, description) VALUES
    ('b4d69f90-0568-4422-bc19-5fb26e9ad262', 'web.home', ''),
    ('0c233905-4752-4fe5-bb51-0ec00f45ffb2', 'web.view', ''),
    ('421b291b-f79b-4963-8974-2091192cd60d', 'web.topBar', ''),
    ('37b0dcf5-e378-48c7-840e-7bee51b55d02', 'web.statusBar', ''),
    ('eb69fbc1-5e32-4f18-a59f-c01c71eaf2ee', 'web.sideMenu', ''),
    ('99c323cb-e36a-4095-bf4c-67f5ea55eac0', 'web.collapse', ''),
    ('360dd317-f294-40ce-8383-6fde91f676f1', 'web.showMenu', ''),
    ('90025a0b-a38a-4e63-9a3f-2e5d5477a14d', 'web.pickFromMenu', ''),
    ('ed5c3017-e00b-4ebd-851d-cbaa3e090890', 'web.close', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('fd8e6143-77d9-4469-ac1e-07ca8f4b50d0', (SELECT id FROM translation_keys WHERE key = 'web.home'), 'ar', 'الرئيسية'),
    ('ebc98d07-0617-4af9-83f6-7dd8a873abd6', (SELECT id FROM translation_keys WHERE key = 'web.home'), 'en', 'Home'),
    ('d93618fc-5240-492c-b2bd-c3ef92c5e00b', (SELECT id FROM translation_keys WHERE key = 'web.view'), 'ar', 'عرض'),
    ('81842fff-4093-43d6-ba8d-5cdd41c9e54f', (SELECT id FROM translation_keys WHERE key = 'web.view'), 'en', 'View'),
    ('7d35ce34-ef7c-4a38-b9e3-3212c0f0938c', (SELECT id FROM translation_keys WHERE key = 'web.topBar'), 'ar', 'الشريط العلوي'),
    ('f2e50fc5-cdbc-4063-a643-c2a3ae9ad032', (SELECT id FROM translation_keys WHERE key = 'web.topBar'), 'en', 'Top bar'),
    ('47ca6b0a-32cf-4868-8922-4641a1accff3', (SELECT id FROM translation_keys WHERE key = 'web.statusBar'), 'ar', 'شريط الحالة'),
    ('9fd689bc-29a3-42f4-9642-02a09dda899b', (SELECT id FROM translation_keys WHERE key = 'web.statusBar'), 'en', 'Status bar'),
    ('ac72e7f2-a968-476d-8768-773ea87b5901', (SELECT id FROM translation_keys WHERE key = 'web.sideMenu'), 'ar', 'القائمة الجانبية'),
    ('08b1ae5b-9a73-4ca9-9d04-b04cd1ae5ae2', (SELECT id FROM translation_keys WHERE key = 'web.sideMenu'), 'en', 'Side menu'),
    ('eedd6f9e-cbd0-4b24-9c8f-6dd5e28a23aa', (SELECT id FROM translation_keys WHERE key = 'web.collapse'), 'ar', 'طيّ القائمة'),
    ('46fc8bbd-acef-4235-8998-c928552dc598', (SELECT id FROM translation_keys WHERE key = 'web.collapse'), 'en', 'Collapse'),
    ('8233e02a-cbbf-4bd1-ae2e-a4be1ad1e1e4', (SELECT id FROM translation_keys WHERE key = 'web.showMenu'), 'ar', 'إظهار القائمة'),
    ('2164d4fa-a3bd-4992-80fb-b17eeb8f4c45', (SELECT id FROM translation_keys WHERE key = 'web.showMenu'), 'en', 'Show the menu'),
    ('95afb72e-4a98-4d62-aa43-0fa61b726a0c', (SELECT id FROM translation_keys WHERE key = 'web.pickFromMenu'), 'ar', 'اختر من القائمة الجانبية لتبدأ.'),
    ('7525164d-3a75-4d45-ad63-e4263d75be4a', (SELECT id FROM translation_keys WHERE key = 'web.pickFromMenu'), 'en', 'Pick an item in the side menu to start.'),
    ('caae0737-94f4-4c7b-b83f-2ba1d3496fac', (SELECT id FROM translation_keys WHERE key = 'web.close'), 'ar', 'إغلاق'),
    ('5f9c3ad4-3255-4fd8-996a-de561918dd77', (SELECT id FROM translation_keys WHERE key = 'web.close'), 'en', 'Close');
