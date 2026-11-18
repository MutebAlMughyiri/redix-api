-- +goose Up
-- Copyright 2026 Tech Details. All rights reserved.
-- Use of this file is governed by the LICENSE file at the root of this repository.

-- The REDIX schema on SQLite, generated once from the SQL Server database at main 8012391 by the type map in redix-api #140, then owned here.
-- Every table is STRICT; columns are snake_case (G9); times are ISO 8601 UTC text with seven decimals (G10); money is INTEGER ten-thousandths (N6);
-- rowversion is gone (N5); the dbo and project schemas share one file (N9).

-- project.AccountCustodians
CREATE TABLE account_custodians (
    account_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    PRIMARY KEY (account_id),
    FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_account_custodians_user_id ON account_custodians (user_id);

-- project.Accounts
CREATE TABLE accounts (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    type INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (type) REFERENCES lk_account_type (id),
    FOREIGN KEY (project_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_accounts_project_id ON accounts (project_id);
CREATE INDEX ix_accounts_type ON accounts (type);

-- project.Attachments
CREATE TABLE attachments (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    storage_path TEXT CHECK (length(storage_path) <= 200) NOT NULL,
    file_name TEXT CHECK (length(file_name) <= 255) NOT NULL,
    content_type TEXT CHECK (length(content_type) <= 255) NOT NULL,
    size_bytes INTEGER NOT NULL,
    sha256 TEXT CHECK (length(sha256) <= 64) NOT NULL,
    uploaded_by_user_id TEXT NOT NULL,
    uploaded_at TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (uploaded_by_user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_attachments_project_id_sha256 ON attachments (project_id, sha256);
CREATE INDEX ix_attachments_uploaded_by_user_id ON attachments (uploaded_by_user_id);

-- project.CapitalMovementDetails
CREATE TABLE capital_movement_details (
    entry_id TEXT NOT NULL,
    partner_id TEXT NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (partner_id) REFERENCES partners (id)
) STRICT;
CREATE INDEX ix_capital_movement_details_partner_id ON capital_movement_details (partner_id);

-- project.Categories
CREATE TABLE categories (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    sort_order INTEGER NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_categories_project_id ON categories (project_id);

-- project.ChosenPartners
CREATE TABLE chosen_partners (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    partner_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (partner_id) REFERENCES partners (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_chosen_partners_partner_id ON chosen_partners (partner_id);
CREATE UNIQUE INDEX ix_chosen_partners_project_id_user_id_partner_id ON chosen_partners (project_id, user_id, partner_id);
CREATE INDEX ix_chosen_partners_user_id ON chosen_partners (user_id);

-- dbo.ConfigValues
CREATE TABLE config_values (
    id TEXT NOT NULL,
    key TEXT CHECK (length(key) <= 100) NOT NULL,
    value TEXT CHECK (length(value) <= 500) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ix_config_values_key ON config_values (key);

-- project.ContractAttachments
CREATE TABLE contract_attachments (
    contract_id TEXT NOT NULL,
    attachment_id TEXT NOT NULL,
    PRIMARY KEY (contract_id, attachment_id),
    FOREIGN KEY (attachment_id) REFERENCES attachments (id),
    FOREIGN KEY (contract_id) REFERENCES contracts (id)
) STRICT;
CREATE INDEX ix_contract_attachments_attachment_id ON contract_attachments (attachment_id);

-- project.ContractorPaymentDetails
CREATE TABLE contractor_payment_details (
    entry_id TEXT NOT NULL,
    contract_id TEXT NOT NULL,
    is_outside_contract INTEGER CHECK (is_outside_contract IN (0, 1)) NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (contract_id) REFERENCES contracts (id),
    FOREIGN KEY (entry_id) REFERENCES entries (id)
) STRICT;
CREATE INDEX ix_contractor_payment_details_contract_id ON contractor_payment_details (contract_id);

-- project.Contracts
CREATE TABLE contracts (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    sort_order INTEGER NOT NULL,
    supplier_id TEXT NOT NULL,
    value INTEGER NOT NULL,
    is_open_value INTEGER CHECK (is_open_value IN (0, 1)) NOT NULL,
    unit_price INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    notes TEXT CHECK (length(notes) <= 1000) NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (item_id) REFERENCES items (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (supplier_id) REFERENCES suppliers (id)
) STRICT;
CREATE INDEX ix_contracts_item_id ON contracts (item_id);
CREATE INDEX ix_contracts_project_id ON contracts (project_id);
CREATE INDEX ix_contracts_supplier_id ON contracts (supplier_id);

-- project.DefaultColumns
CREATE TABLE default_columns (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    item_type INTEGER NOT NULL,
    key TEXT CHECK (length(key) <= 50) NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    data_type INTEGER NOT NULL,
    is_core INTEGER CHECK (is_core IN (0, 1)) NOT NULL,
    is_required INTEGER CHECK (is_required IN (0, 1)) NOT NULL,
    show_in_reports INTEGER CHECK (show_in_reports IN (0, 1)) NOT NULL,
    sort_order INTEGER NOT NULL,
    choices TEXT CHECK (json_valid(choices)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (data_type) REFERENCES lk_column_data_type (id),
    FOREIGN KEY (item_type) REFERENCES lk_item_type (id),
    FOREIGN KEY (project_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_default_columns_data_type ON default_columns (data_type);
CREATE INDEX ix_default_columns_item_type ON default_columns (item_type);
CREATE UNIQUE INDEX ix_default_columns_project_id_item_type_key ON default_columns (project_id, item_type, key);

-- dbo.Devices
CREATE TABLE devices (
    id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    refresh_token_hash TEXT CHECK (length(refresh_token_hash) <= 64) NOT NULL,
    refresh_expires_at TEXT NOT NULL,
    is_revoked INTEGER CHECK (is_revoked IN (0, 1)) NOT NULL,
    uses_biometrics INTEGER CHECK (uses_biometrics IN (0, 1)) NOT NULL,
    last_seen_at TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
) STRICT;
CREATE UNIQUE INDEX ix_devices_refresh_token_hash ON devices (refresh_token_hash);
CREATE INDEX ix_devices_user_id ON devices (user_id);

-- project.Documents
CREATE TABLE documents (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    attachment_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 200) NOT NULL,
    group_name TEXT CHECK (length(group_name) <= 100) NOT NULL,
    uploaded_by_user_id TEXT NOT NULL,
    uploaded_at TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (attachment_id) REFERENCES attachments (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (uploaded_by_user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_documents_attachment_id ON documents (attachment_id);
CREATE UNIQUE INDEX ix_documents_project_id_attachment_id ON documents (project_id, attachment_id);
CREATE INDEX ix_documents_uploaded_by_user_id ON documents (uploaded_by_user_id);

-- dbo.EmailCodes
CREATE TABLE email_codes (
    id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    purpose INTEGER NOT NULL,
    email TEXT CHECK (length(email) <= 254) NOT NULL,
    code_hash TEXT CHECK (length(code_hash) <= 200) NOT NULL,
    expires_at TEXT NOT NULL,
    tries INTEGER NOT NULL,
    used_at TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (purpose) REFERENCES lk_email_code_purpose (id),
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
) STRICT;
CREATE INDEX ix_email_codes_purpose ON email_codes (purpose);
CREATE INDEX ix_email_codes_user_id_purpose ON email_codes (user_id, purpose);

-- project.Entries
CREATE TABLE entries (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    kind INTEGER NOT NULL,
    status INTEGER NOT NULL,
    entry_date TEXT NOT NULL,
    amount INTEGER NOT NULL,
    account_id TEXT NOT NULL,
    description TEXT CHECK (length(description) <= 2000) NOT NULL,
    created_by_user_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    approved_at TEXT NOT NULL,
    column_values TEXT CHECK (json_valid(column_values)) NOT NULL DEFAULT '{}',
    number INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    FOREIGN KEY (account_id) REFERENCES accounts (id),
    FOREIGN KEY (kind) REFERENCES lk_entry_kind (id),
    FOREIGN KEY (status) REFERENCES lk_entry_status (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (created_by_user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_entries_account_id ON entries (account_id);
CREATE INDEX ix_entries_created_by_user_id ON entries (created_by_user_id);
CREATE INDEX ix_entries_kind ON entries (kind);
CREATE UNIQUE INDEX ix_entries_project_id_number ON entries (project_id, number);
CREATE INDEX ix_entries_project_id_status ON entries (project_id, status);
CREATE INDEX ix_entries_status ON entries (status);

-- project.EntryAttachments
CREATE TABLE entry_attachments (
    entry_id TEXT NOT NULL,
    attachment_id TEXT NOT NULL,
    role INTEGER NOT NULL,
    PRIMARY KEY (entry_id, attachment_id),
    FOREIGN KEY (attachment_id) REFERENCES attachments (id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (role) REFERENCES lk_entry_attachment_role (id)
) STRICT;
CREATE INDEX ix_entry_attachments_attachment_id ON entry_attachments (attachment_id);
CREATE INDEX ix_entry_attachments_role ON entry_attachments (role);

-- project.EntryDecisions
CREATE TABLE entry_decisions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entry_id TEXT NOT NULL,
    action INTEGER NOT NULL,
    user_id TEXT NOT NULL,
    at TEXT NOT NULL,
    reason TEXT CHECK (length(reason) <= 500) NOT NULL,
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (action) REFERENCES lk_entry_decision_action (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_entry_decisions_action ON entry_decisions (action);
CREATE INDEX ix_entry_decisions_entry_id ON entry_decisions (entry_id);
CREATE INDEX ix_entry_decisions_user_id ON entry_decisions (user_id);

-- project.ExpenseDetails
CREATE TABLE expense_details (
    entry_id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    subgroup_id TEXT NOT NULL,
    payment_method_id TEXT NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (item_id) REFERENCES items (id),
    FOREIGN KEY (payment_method_id) REFERENCES payment_methods (id),
    FOREIGN KEY (subgroup_id) REFERENCES subgroups (id)
) STRICT;
CREATE INDEX ix_expense_details_item_id ON expense_details (item_id);
CREATE INDEX ix_expense_details_payment_method_id ON expense_details (payment_method_id);
CREATE INDEX ix_expense_details_subgroup_id ON expense_details (subgroup_id);

-- dbo.History
CREATE TABLE history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    at TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    action TEXT CHECK (length(action) <= 30) NOT NULL,
    table_name TEXT CHECK (length(table_name) <= 128) NOT NULL,
    record_id TEXT NOT NULL,
    before TEXT CHECK (json_valid(before)) NOT NULL,
    after TEXT CHECK (json_valid(after)) NOT NULL,
    FOREIGN KEY (action) REFERENCES lk_history_action (name),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_history_action ON history (action);
CREATE INDEX ix_history_project_id_id ON history (project_id, id);
CREATE INDEX ix_history_user_id ON history (user_id);

-- dbo.InactiveMembers
CREATE TABLE inactive_members (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE UNIQUE INDEX ix_inactive_members_project_id_user_id ON inactive_members (project_id, user_id);
CREATE INDEX ix_inactive_members_user_id ON inactive_members (user_id);

-- project.Invoices
CREATE TABLE invoices (
    entry_id TEXT NOT NULL,
    number TEXT CHECK (length(number) <= 50) NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id)
) STRICT;
CREATE INDEX ix_invoices_number ON invoices (number);

-- project.ItemColumns
CREATE TABLE item_columns (
    id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    is_hidden INTEGER CHECK (is_hidden IN (0, 1)) NOT NULL,
    key TEXT CHECK (length(key) <= 50) NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    data_type INTEGER NOT NULL,
    is_core INTEGER CHECK (is_core IN (0, 1)) NOT NULL,
    is_required INTEGER CHECK (is_required IN (0, 1)) NOT NULL,
    show_in_reports INTEGER CHECK (show_in_reports IN (0, 1)) NOT NULL,
    sort_order INTEGER NOT NULL,
    choices TEXT CHECK (json_valid(choices)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (item_id) REFERENCES items (id),
    FOREIGN KEY (data_type) REFERENCES lk_column_data_type (id)
) STRICT;
CREATE INDEX ix_item_columns_data_type ON item_columns (data_type);
CREATE UNIQUE INDEX ix_item_columns_item_id_key ON item_columns (item_id, key);

-- project.Items
CREATE TABLE items (
    id TEXT NOT NULL,
    category_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    type INTEGER NOT NULL,
    sort_order INTEGER NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    shows_table_percentages INTEGER CHECK (shows_table_percentages IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (category_id) REFERENCES categories (id),
    FOREIGN KEY (type) REFERENCES lk_item_type (id)
) STRICT;
CREATE INDEX ix_items_category_id ON items (category_id);
CREATE INDEX ix_items_type ON items (type);

-- dbo.LK_AccountType
CREATE TABLE lk_account_type (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_account_type_name ON lk_account_type (name);

-- dbo.LK_ColumnDataType
CREATE TABLE lk_column_data_type (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_column_data_type_name ON lk_column_data_type (name);

-- dbo.LK_EmailCodePurpose
CREATE TABLE lk_email_code_purpose (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_email_code_purpose_name ON lk_email_code_purpose (name);

-- dbo.LK_EntryAttachmentRole
CREATE TABLE lk_entry_attachment_role (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_entry_attachment_role_name ON lk_entry_attachment_role (name);

-- dbo.LK_EntryDecisionAction
CREATE TABLE lk_entry_decision_action (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_entry_decision_action_name ON lk_entry_decision_action (name);

-- dbo.LK_EntryKind
CREATE TABLE lk_entry_kind (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_entry_kind_name ON lk_entry_kind (name);

-- dbo.LK_EntryStatus
CREATE TABLE lk_entry_status (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_entry_status_name ON lk_entry_status (name);

-- dbo.LK_HistoryAction
CREATE TABLE lk_history_action (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_history_action_name ON lk_history_action (name);

-- dbo.LK_ItemType
CREATE TABLE lk_item_type (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_item_type_name ON lk_item_type (name);

-- dbo.LK_NotificationKind
CREATE TABLE lk_notification_kind (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_notification_kind_name ON lk_notification_kind (name);

-- dbo.LK_ProjectStatus
CREATE TABLE lk_project_status (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_project_status_name ON lk_project_status (name);

-- dbo.LK_SupplierStatus
CREATE TABLE lk_supplier_status (
    id INTEGER NOT NULL,
    name TEXT CHECK (length(name) <= 30) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ak_lk_supplier_status_name ON lk_supplier_status (name);

-- dbo.NotificationEntries
CREATE TABLE notification_entries (
    notification_id TEXT NOT NULL,
    entry_id TEXT NOT NULL,
    PRIMARY KEY (notification_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (notification_id) REFERENCES notifications (id) ON DELETE CASCADE
) STRICT;
CREATE INDEX ix_notification_entries_entry_id ON notification_entries (entry_id);

-- dbo.Notifications
CREATE TABLE notifications (
    id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    kind INTEGER NOT NULL,
    at TEXT NOT NULL,
    is_read INTEGER CHECK (is_read IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (kind) REFERENCES lk_notification_kind (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_notifications_kind ON notifications (kind);
CREATE INDEX ix_notifications_project_id ON notifications (project_id);
CREATE INDEX ix_notifications_user_id_at ON notifications (user_id, at);

-- project.Partners
CREATE TABLE partners (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    sort_order INTEGER NOT NULL,
    is_deleted INTEGER CHECK (is_deleted IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_partners_project_id ON partners (project_id);

-- project.PartnerUsers
CREATE TABLE partner_users (
    partner_id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    PRIMARY KEY (partner_id),
    FOREIGN KEY (partner_id) REFERENCES partners (id) ON DELETE CASCADE,
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE UNIQUE INDEX ix_partner_users_project_id_user_id ON partner_users (project_id, user_id);
CREATE INDEX ix_partner_users_user_id ON partner_users (user_id);

-- project.PaymentMethods
CREATE TABLE payment_methods (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    sort_order INTEGER NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_payment_methods_project_id ON payment_methods (project_id);

-- project.PeriodicReports
CREATE TABLE periodic_reports (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    number INTEGER NOT NULL,
    issued_at TEXT NOT NULL,
    issued_by_user_id TEXT NOT NULL,
    expenses_total INTEGER NOT NULL,
    snapshot TEXT CHECK (json_valid(snapshot)) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (issued_by_user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_periodic_reports_issued_by_user_id ON periodic_reports (issued_by_user_id);
CREATE UNIQUE INDEX ix_periodic_reports_project_id_number ON periodic_reports (project_id, number);

-- dbo.ProjectPermissions
CREATE TABLE project_permissions (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    permission TEXT CHECK (length(permission) <= 30) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id),
    CHECK ((permission='GenerateReports' OR permission='SeeReports' OR permission='MoveMoney' OR permission='ApproveEntries' OR permission='AttachFiles' OR permission='RemoveApprovedEntries' OR permission='DeleteEntries' OR permission='ManageAnyEntry' OR permission='RecordEntries' OR permission='ChargeAnyAccount' OR permission='AddSupplier' OR permission='ManageSettings' OR permission='ManageStructure' OR permission='ManageProject' OR permission='SeeChosenCapital' OR permission='SeeAllCapital' OR permission='ReadProject' OR permission='ManageUsers' OR permission='ApproveProjects') AND permission<>'ApproveProjects')
) STRICT;
CREATE UNIQUE INDEX ix_project_permissions_project_id_user_id_permission ON project_permissions (project_id, user_id, permission);
CREATE INDEX ix_project_permissions_user_id ON project_permissions (user_id);

-- dbo.ProjectRoleNames
CREATE TABLE project_role_names (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    role_name TEXT CHECK (length(role_name) <= 50) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE UNIQUE INDEX ix_project_role_names_project_id_user_id ON project_role_names (project_id, user_id);
CREATE INDEX ix_project_role_names_user_id ON project_role_names (user_id);

-- dbo.Projects
CREATE TABLE projects (
    id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    start_date TEXT NOT NULL,
    status INTEGER NOT NULL,
    rejection_reason TEXT CHECK (length(rejection_reason) <= 500) NOT NULL,
    description TEXT CHECK (length(description) <= 2000) NOT NULL,
    is_template INTEGER CHECK (is_template IN (0, 1)) NOT NULL,
    last_entry_number INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    FOREIGN KEY (status) REFERENCES lk_project_status (id)
) STRICT;
CREATE INDEX ix_projects_status ON projects (status);

-- dbo.ProjectStructureSources
CREATE TABLE project_structure_sources (
    project_id TEXT NOT NULL,
    structure_source_id TEXT NOT NULL,
    PRIMARY KEY (project_id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (structure_source_id) REFERENCES projects (id)
) STRICT;
CREATE INDEX ix_project_structure_sources_structure_source_id ON project_structure_sources (structure_source_id);

-- project.PurchaseDetails
CREATE TABLE purchase_details (
    entry_id TEXT NOT NULL,
    supplier_id TEXT NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (supplier_id) REFERENCES suppliers (id)
) STRICT;
CREATE INDEX ix_purchase_details_supplier_id ON purchase_details (supplier_id);

-- project.ReconciliationBalances
CREATE TABLE reconciliation_balances (
    reconciliation_id TEXT NOT NULL,
    account_id TEXT NOT NULL,
    actual_balance INTEGER NOT NULL,
    PRIMARY KEY (reconciliation_id, account_id),
    FOREIGN KEY (account_id) REFERENCES accounts (id),
    FOREIGN KEY (reconciliation_id) REFERENCES reconciliations (id) ON DELETE CASCADE
) STRICT;
CREATE INDEX ix_reconciliation_balances_account_id ON reconciliation_balances (account_id);

-- project.Reconciliations
CREATE TABLE reconciliations (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    at TEXT NOT NULL,
    user_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_reconciliations_project_id_at ON reconciliations (project_id, at);
CREATE INDEX ix_reconciliations_user_id ON reconciliations (user_id);

-- project.RevenueDetails
CREATE TABLE revenue_details (
    entry_id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    subgroup_id TEXT NOT NULL,
    PRIMARY KEY (entry_id),
    FOREIGN KEY (entry_id) REFERENCES entries (id),
    FOREIGN KEY (item_id) REFERENCES items (id),
    FOREIGN KEY (subgroup_id) REFERENCES subgroups (id)
) STRICT;
CREATE INDEX ix_revenue_details_item_id ON revenue_details (item_id);
CREATE INDEX ix_revenue_details_subgroup_id ON revenue_details (subgroup_id);

-- project.Subgroups
CREATE TABLE subgroups (
    id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    sort_order INTEGER NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (item_id) REFERENCES items (id)
) STRICT;
CREATE INDEX ix_subgroups_item_id ON subgroups (item_id);

-- project.SupplierDefaultItems
CREATE TABLE supplier_default_items (
    supplier_id TEXT NOT NULL,
    item_id TEXT NOT NULL,
    PRIMARY KEY (supplier_id),
    FOREIGN KEY (item_id) REFERENCES items (id),
    FOREIGN KEY (supplier_id) REFERENCES suppliers (id) ON DELETE CASCADE
) STRICT;
CREATE INDEX ix_supplier_default_items_item_id ON supplier_default_items (item_id);

-- project.Suppliers
CREATE TABLE suppliers (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    normalized_name TEXT CHECK (length(normalized_name) <= 100) NOT NULL,
    vat_number TEXT CHECK (length(vat_number) <= 20) NOT NULL,
    mobile TEXT CHECK (length(mobile) <= 20) NOT NULL,
    status INTEGER NOT NULL,
    is_active INTEGER CHECK (is_active IN (0, 1)) NOT NULL,
    is_general_cash INTEGER CHECK (is_general_cash IN (0, 1)) NOT NULL,
    created_by_user_id TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (status) REFERENCES lk_supplier_status (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (created_by_user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_suppliers_created_by_user_id ON suppliers (created_by_user_id);
CREATE INDEX ix_suppliers_project_id_name ON suppliers (project_id, name);
CREATE UNIQUE INDEX ix_suppliers_project_id_vat_number ON suppliers (project_id, vat_number) WHERE vat_number<>'';
CREATE INDEX ix_suppliers_status ON suppliers (status);

-- dbo.SystemPermissions
CREATE TABLE system_permissions (
    id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    permission TEXT CHECK (length(permission) <= 30) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (user_id) REFERENCES users (id),
    CHECK (permission='GenerateReports' OR permission='SeeReports' OR permission='MoveMoney' OR permission='ApproveEntries' OR permission='AttachFiles' OR permission='RemoveApprovedEntries' OR permission='DeleteEntries' OR permission='ManageAnyEntry' OR permission='RecordEntries' OR permission='ChargeAnyAccount' OR permission='AddSupplier' OR permission='ManageSettings' OR permission='ManageStructure' OR permission='ManageProject' OR permission='SeeChosenCapital' OR permission='SeeAllCapital' OR permission='ReadProject' OR permission='ManageUsers' OR permission='ApproveProjects')
) STRICT;
CREATE UNIQUE INDEX ix_system_permissions_user_id_permission ON system_permissions (user_id, permission);

-- project.Transfers
CREATE TABLE transfers (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    from_account_id TEXT NOT NULL,
    to_account_id TEXT NOT NULL,
    amount INTEGER NOT NULL,
    date TEXT NOT NULL,
    notes TEXT CHECK (length(notes) <= 1000) NOT NULL,
    created_by_user_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (from_account_id) REFERENCES accounts (id),
    FOREIGN KEY (to_account_id) REFERENCES accounts (id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (created_by_user_id) REFERENCES users (id),
    CHECK (amount>(0)),
    CHECK (from_account_id<>to_account_id)
) STRICT;
CREATE INDEX ix_transfers_created_by_user_id ON transfers (created_by_user_id);
CREATE INDEX ix_transfers_from_account_id ON transfers (from_account_id);
CREATE INDEX ix_transfers_project_id ON transfers (project_id);
CREATE INDEX ix_transfers_to_account_id ON transfers (to_account_id);

-- dbo.TranslationKeys
CREATE TABLE translation_keys (
    id TEXT NOT NULL,
    key TEXT CHECK (length(key) <= 100) NOT NULL,
    description TEXT CHECK (length(description) <= 500) NOT NULL,
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ix_translation_keys_key ON translation_keys (key);

-- dbo.Translations
CREATE TABLE translations (
    id TEXT NOT NULL,
    translation_key_id TEXT NOT NULL,
    language TEXT CHECK (length(language) <= 2) NOT NULL,
    value TEXT CHECK (length(value) <= 1000) NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (translation_key_id) REFERENCES translation_keys (id) ON DELETE CASCADE
) STRICT;
CREATE UNIQUE INDEX ix_translations_translation_key_id_language ON translations (translation_key_id, language);

-- project.Units
CREATE TABLE units (
    id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    name TEXT CHECK (length(name) <= 100) NOT NULL,
    sort_order INTEGER NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (project_id) REFERENCES projects (id) ON DELETE CASCADE
) STRICT;
CREATE INDEX ix_units_project_id ON units (project_id);

-- dbo.UserDefaultProjects
CREATE TABLE user_default_projects (
    user_id TEXT NOT NULL,
    project_id TEXT NOT NULL,
    PRIMARY KEY (user_id),
    FOREIGN KEY (project_id) REFERENCES projects (id),
    FOREIGN KEY (user_id) REFERENCES users (id)
) STRICT;
CREATE INDEX ix_user_default_projects_project_id ON user_default_projects (project_id);

-- dbo.Users
CREATE TABLE users (
    id TEXT NOT NULL,
    username TEXT COLLATE NOCASE CHECK (length(username) <= 50) NOT NULL,
    display_name TEXT CHECK (length(display_name) <= 100) NOT NULL,
    mobile TEXT CHECK (length(mobile) <= 20) NOT NULL,
    password_hash TEXT CHECK (length(password_hash) <= 200) NOT NULL,
    must_change_password INTEGER CHECK (must_change_password IN (0, 1)) NOT NULL,
    failed_sign_ins INTEGER NOT NULL,
    locked_until TEXT NOT NULL,
    created_at TEXT NOT NULL,
    email TEXT COLLATE NOCASE CHECK (length(email) <= 254) NOT NULL DEFAULT '',
    PRIMARY KEY (id)
) STRICT;
CREATE UNIQUE INDEX ix_users_email ON users (email) WHERE email<>'';
CREATE UNIQUE INDEX ix_users_username ON users (username);

-- History is append-only (AC-21.1), as the SQL Server trigger HistoryIsAppendOnly made it.
-- +goose StatementBegin
CREATE TRIGGER history_is_append_only_update BEFORE UPDATE ON history
BEGIN
    SELECT RAISE(ABORT, 'The History table is append-only: its rows are never changed or deleted (AC-21.1).');
END;
-- +goose StatementEnd
-- +goose StatementBegin
CREATE TRIGGER history_is_append_only_delete BEFORE DELETE ON history
BEGIN
    SELECT RAISE(ABORT, 'The History table is append-only: its rows are never changed or deleted (AC-21.1).');
END;
-- +goose StatementEnd

-- +goose Down
SELECT 1;
