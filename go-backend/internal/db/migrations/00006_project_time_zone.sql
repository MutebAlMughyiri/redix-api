-- +goose Up
-- The time zone each project is in (redix-app #107, D1, D3): its entries' today is counted there, every moment staying UTC. A project with no row
-- is in Asia/Riyadh, as every project was before. Its own table, since SQLite adds no column holding a foreign key and a default; the zones a
-- project may take are their lookup table, seeded from internal/zones (redix-api #87). A project's table (redix-api #123).
CREATE TABLE lk_time_zone (
    name TEXT NOT NULL,
    PRIMARY KEY (name)
) STRICT;
INSERT INTO lk_time_zone (name) VALUES ('Asia/Riyadh'), ('Asia/Dubai'), ('Asia/Kuwait'), ('Asia/Bahrain'), ('Asia/Qatar'), ('Asia/Muscat'), ('Africa/Cairo'), ('Asia/Amman'), ('Europe/Istanbul'), ('Europe/London'), ('Europe/Paris'), ('Asia/Kolkata'), ('Asia/Karachi'), ('Asia/Dhaka'), ('Asia/Manila'), ('Asia/Shanghai'), ('America/New_York'), ('America/Los_Angeles');

CREATE TABLE project_time_zones (
    project_id TEXT NOT NULL,
    time_zone TEXT NOT NULL,
    PRIMARY KEY (project_id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (time_zone) REFERENCES lk_time_zone (name)
) STRICT;
CREATE INDEX ix_project_time_zones_time_zone ON project_time_zones (time_zone);

INSERT INTO translation_keys (id, key, description) VALUES
    ('658b79c4-c9fb-4ed3-8551-ee9bc413de28', 'timeZone.riyadh', ''),
    ('cfa00e64-2905-4ad6-a9d6-61d18fc10c6c', 'timeZone.dubai', ''),
    ('6782cafd-7d3d-4dad-94f1-3ea47d727001', 'timeZone.kuwait', ''),
    ('64380283-352b-4c81-9e71-b17061d9692d', 'timeZone.bahrain', ''),
    ('fa01895c-2ea7-453f-99f6-0d8f3de073d1', 'timeZone.doha', ''),
    ('6380aefe-6432-4e9b-a812-5be759606b86', 'timeZone.muscat', ''),
    ('e893e91d-26d9-4539-849a-7e04432fb540', 'timeZone.cairo', ''),
    ('d585d43e-e6e5-4c8d-80f3-119ca4378d2c', 'timeZone.amman', ''),
    ('4e5e8d01-728e-45ab-bbfb-a2deee675a86', 'timeZone.istanbul', ''),
    ('167766c2-0ec5-42f5-9467-809b39010111', 'timeZone.london', ''),
    ('1a89e3a8-22ec-4fdc-824d-41480b176f26', 'timeZone.paris', ''),
    ('1c3fc41f-2426-4f40-982c-5706cfaa1229', 'timeZone.newDelhi', ''),
    ('463ae943-e47d-49da-921d-3c61f3ea0139', 'timeZone.karachi', ''),
    ('881be405-32c0-4ca6-8938-39e5b4c689a4', 'timeZone.dhaka', ''),
    ('fd2e9d0e-b555-4849-b1e9-f5d3803b9cc1', 'timeZone.manila', ''),
    ('677d6322-878a-4cef-951d-bf148fdebcfe', 'timeZone.beijing', ''),
    ('3e011f48-bae6-409a-9b9a-6c8a7b1dc291', 'timeZone.newYork', ''),
    ('f38080e7-b612-473a-9940-46ebe4d584a4', 'timeZone.losAngeles', ''),
    ('a4b0e7a7-4d80-4804-87ac-f90489a9271d', 'projects.timeZone', ''),
    ('ea57c402-8c0b-418d-aa17-9d1e84e55730', 'projects.unknownTimeZone', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('e047da1d-6e36-45f2-9591-9128cfe91ae5', (SELECT id FROM translation_keys WHERE key = 'timeZone.riyadh'), 'ar', 'الرياض'),
    ('2ed73a1d-a8c6-40bb-9145-ecc6fc951847', (SELECT id FROM translation_keys WHERE key = 'timeZone.riyadh'), 'en', 'Riyadh'),
    ('d52876c8-352a-40bf-b5e6-05f85e07b00e', (SELECT id FROM translation_keys WHERE key = 'timeZone.dubai'), 'ar', 'دبي'),
    ('cb543bb8-16ed-4153-a89c-4cc16d1d63af', (SELECT id FROM translation_keys WHERE key = 'timeZone.dubai'), 'en', 'Dubai'),
    ('d390f45a-964b-475b-95a6-3c3be5f9a6bb', (SELECT id FROM translation_keys WHERE key = 'timeZone.kuwait'), 'ar', 'الكويت'),
    ('ce6017f0-6768-4630-9fef-4585bbb59496', (SELECT id FROM translation_keys WHERE key = 'timeZone.kuwait'), 'en', 'Kuwait'),
    ('2083ea57-edb8-4543-98b4-3d21ab84d4f8', (SELECT id FROM translation_keys WHERE key = 'timeZone.bahrain'), 'ar', 'البحرين'),
    ('a3569d7f-4a72-4aea-8be0-cb3532888f8c', (SELECT id FROM translation_keys WHERE key = 'timeZone.bahrain'), 'en', 'Bahrain'),
    ('6b0afe21-f664-4383-8b0e-a974938924ea', (SELECT id FROM translation_keys WHERE key = 'timeZone.doha'), 'ar', 'الدوحة'),
    ('6c2105e6-7835-49ec-991b-f5bb1a0c53a6', (SELECT id FROM translation_keys WHERE key = 'timeZone.doha'), 'en', 'Doha'),
    ('942067ae-ba86-40cd-b371-8e81715697a4', (SELECT id FROM translation_keys WHERE key = 'timeZone.muscat'), 'ar', 'مسقط'),
    ('a87c6059-6014-43b8-a141-2e186bebd6b3', (SELECT id FROM translation_keys WHERE key = 'timeZone.muscat'), 'en', 'Muscat'),
    ('a0092b27-6e44-48ed-a4ee-dcdc044100d6', (SELECT id FROM translation_keys WHERE key = 'timeZone.cairo'), 'ar', 'القاهرة'),
    ('21a3096e-5e17-48cb-8002-0f957b9efdfc', (SELECT id FROM translation_keys WHERE key = 'timeZone.cairo'), 'en', 'Cairo'),
    ('4f7edafd-5a6b-484b-b4ed-16b345e612af', (SELECT id FROM translation_keys WHERE key = 'timeZone.amman'), 'ar', 'عمّان'),
    ('7ea0ad47-0a7b-46e9-80ea-e7b1cb915912', (SELECT id FROM translation_keys WHERE key = 'timeZone.amman'), 'en', 'Amman'),
    ('6d0cd06c-c22f-4938-9253-127ca08617f4', (SELECT id FROM translation_keys WHERE key = 'timeZone.istanbul'), 'ar', 'إسطنبول'),
    ('1a3a45cc-2b1d-4dcc-83b3-f58c4d68d5c0', (SELECT id FROM translation_keys WHERE key = 'timeZone.istanbul'), 'en', 'Istanbul'),
    ('3f957ff5-583d-4c39-80fa-37d1879fb5a5', (SELECT id FROM translation_keys WHERE key = 'timeZone.london'), 'ar', 'لندن'),
    ('92f7fb71-dab5-4752-b510-2df28737c2ea', (SELECT id FROM translation_keys WHERE key = 'timeZone.london'), 'en', 'London'),
    ('c8c708f9-e27d-4383-ae5e-a3931b8d8481', (SELECT id FROM translation_keys WHERE key = 'timeZone.paris'), 'ar', 'باريس'),
    ('bb818dda-2540-4fdf-bc97-b96a981a56c9', (SELECT id FROM translation_keys WHERE key = 'timeZone.paris'), 'en', 'Paris'),
    ('5cb2b414-054c-4ad4-b655-14d15de749ea', (SELECT id FROM translation_keys WHERE key = 'timeZone.newDelhi'), 'ar', 'نيودلهي'),
    ('e9b472d0-fcdc-4942-b117-61ab0e4beaa0', (SELECT id FROM translation_keys WHERE key = 'timeZone.newDelhi'), 'en', 'New Delhi'),
    ('c253c1f3-3bac-4f49-85c0-55941f0d4b59', (SELECT id FROM translation_keys WHERE key = 'timeZone.karachi'), 'ar', 'كراتشي'),
    ('81fd32e1-66db-4a62-88a9-9294fb2cfef1', (SELECT id FROM translation_keys WHERE key = 'timeZone.karachi'), 'en', 'Karachi'),
    ('ea9bb93d-0028-4240-a74d-476539c6ae3c', (SELECT id FROM translation_keys WHERE key = 'timeZone.dhaka'), 'ar', 'دكا'),
    ('46a795e9-629f-435c-a312-5a802b4b763a', (SELECT id FROM translation_keys WHERE key = 'timeZone.dhaka'), 'en', 'Dhaka'),
    ('6f379251-ddd6-418f-ae32-8146fd74d935', (SELECT id FROM translation_keys WHERE key = 'timeZone.manila'), 'ar', 'مانيلا'),
    ('a7599cab-1109-4ea5-bb0f-88deb531f177', (SELECT id FROM translation_keys WHERE key = 'timeZone.manila'), 'en', 'Manila'),
    ('6869cdce-287a-4704-9404-4d761c945bb6', (SELECT id FROM translation_keys WHERE key = 'timeZone.beijing'), 'ar', 'بكين'),
    ('33174bc7-6086-4f5f-a548-3b76e259bcde', (SELECT id FROM translation_keys WHERE key = 'timeZone.beijing'), 'en', 'Beijing'),
    ('d37caf3e-6c94-4f9c-b169-23bf6f0ab30b', (SELECT id FROM translation_keys WHERE key = 'timeZone.newYork'), 'ar', 'نيويورك'),
    ('3c8acc5b-9349-4969-ae23-1f83712fc9fb', (SELECT id FROM translation_keys WHERE key = 'timeZone.newYork'), 'en', 'New York'),
    ('ae594239-3de6-4eb9-80f2-707db2486d15', (SELECT id FROM translation_keys WHERE key = 'timeZone.losAngeles'), 'ar', 'لوس أنجلوس'),
    ('899fa231-1caa-46a3-bffa-2dce3d5f056e', (SELECT id FROM translation_keys WHERE key = 'timeZone.losAngeles'), 'en', 'Los Angeles'),
    ('fe8b07ff-95f8-499d-b70b-458463eb9d68', (SELECT id FROM translation_keys WHERE key = 'projects.timeZone'), 'ar', 'المنطقة الزمنية'),
    ('dc00f2a8-832b-4ba3-abcd-2c9eccac884c', (SELECT id FROM translation_keys WHERE key = 'projects.timeZone'), 'en', 'Time zone'),
    ('38db0e94-a2f1-40c2-be50-3faa82f6d75f', (SELECT id FROM translation_keys WHERE key = 'projects.unknownTimeZone'), 'ar', 'هذه المنطقة الزمنية غير متاحة'),
    ('2c5c7f10-2905-4950-9e98-ffadc55a4e9e', (SELECT id FROM translation_keys WHERE key = 'projects.unknownTimeZone'), 'en', 'This time zone is not offered');
