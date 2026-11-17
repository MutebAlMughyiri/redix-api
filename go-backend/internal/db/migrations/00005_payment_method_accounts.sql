-- +goose Up
-- The accounts each payment method pays from (redix-app #109, D1), one row a pair, a project's table (redix-api #123). Every method kept before
-- pays from every account its project has (R4), as an account added later is paid from by every method of its project (D1b), until someone
-- unticks it.
CREATE TABLE payment_method_accounts (
    payment_method_id TEXT NOT NULL,
    account_id TEXT NOT NULL,
    PRIMARY KEY (payment_method_id, account_id),
    FOREIGN KEY (payment_method_id) REFERENCES payment_methods (id),
    FOREIGN KEY (account_id) REFERENCES accounts (id)
) STRICT;
CREATE INDEX ix_payment_method_accounts_account_id ON payment_method_accounts (account_id);

INSERT INTO payment_method_accounts (payment_method_id, account_id)
SELECT payment_methods.id, accounts.id FROM payment_methods JOIN accounts ON accounts.project_id = payment_methods.project_id;

INSERT INTO translation_keys (id, key, description) VALUES
    ('03551221-75c4-4b9c-ab59-85650e4f0e60', 'paymentMethods.unknownAccount', ''),
    ('22ff3421-cfa8-48e5-a85a-a667e9fb4b98', 'paymentMethods.paysFrom', ''),
    ('3cea2035-9979-4349-8f9e-2117e73d0cda', 'paymentMethods.paysFromNothing', ''),
    ('2a807d5b-22de-4be3-b753-37b57977fdeb', 'expenses.accountNotForMethod', ''),
    ('39435154-985d-49e9-bd2d-60608d74c16f', 'expenses.noAccountForMethod', ''),
    ('7ac48792-f853-4fea-aa0e-820f0a402315', 'expenses.paidFrom', '');

INSERT INTO translations (id, translation_key_id, language, value) VALUES
    ('16cc3fce-eed7-4500-9862-c8d054e8f5dd', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.unknownAccount'), 'ar', 'هذا الحساب ليس من حسابات المشروع'),
    ('1c103121-9ebc-4c75-a00e-6520d1991b95', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.unknownAccount'), 'en', 'This account is not one of the project''s'),
    ('39d58a69-3d84-497a-9590-fb8c9d4df837', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.paysFrom'), 'ar', 'يدفع من'),
    ('e1ebf726-55a1-4065-92b1-08b1c416a3b8', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.paysFrom'), 'en', 'Pays from'),
    ('f8ae3eaf-d5a1-4f90-b27f-ffb41bdc5a56', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.paysFromNothing'), 'ar', 'اختر حسابًا واحدًا على الأقل تدفع منه هذه الطريقة'),
    ('4ee56b9b-ee0e-4144-8a01-e94d024f7337', (SELECT id FROM translation_keys WHERE key = 'paymentMethods.paysFromNothing'), 'en', 'Choose at least one account this method pays from'),
    ('8f56a0c1-0014-496a-b9e7-7479ccf8fbcf', (SELECT id FROM translation_keys WHERE key = 'expenses.accountNotForMethod'), 'ar', 'طريقة الدفع هذه لا تدفع من هذا الحساب'),
    ('f7703898-110e-4705-9111-4d4e65a878d8', (SELECT id FROM translation_keys WHERE key = 'expenses.accountNotForMethod'), 'en', 'This payment method does not pay from this account'),
    ('30affdb8-bd61-490d-8a72-c252dc0a5b56', (SELECT id FROM translation_keys WHERE key = 'expenses.noAccountForMethod'), 'ar', 'لا حساب لهذه الطريقة تدفع منه'),
    ('742ae9c0-6656-41dc-ac02-f2b52e846ffe', (SELECT id FROM translation_keys WHERE key = 'expenses.noAccountForMethod'), 'en', 'No account this method pays from'),
    ('08716fe0-d212-43b0-8e2d-5123c7c2c20d', (SELECT id FROM translation_keys WHERE key = 'expenses.paidFrom'), 'ar', 'يُدفع من'),
    ('85bbdd2b-8f04-4cc3-9cc6-e5938be52b56', (SELECT id FROM translation_keys WHERE key = 'expenses.paidFrom'), 'en', 'Paid from');
