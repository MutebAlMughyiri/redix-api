# REDIX API documentation

The server of REDIX (Real Estate Development Intelligence): it keeps every project's expenses, approvals, partners' capital and reports for the mobile app.

| Page | Read it to |
| --- | --- |
| [Architecture](ARCHITECTURE.md) | See how the API is built, where each thing lives, and how a request flows |
| [Database](database.md) | See every table, its columns and keys, what each points at, the lookup tables and their values, and the diagrams of how the tables relate |
| [Decisions](decisions.md) | Know why things are as they are |
| [Use cases](#use-cases) | Know what each part does, who may use it, and where each rule lives in the code |
| The OpenAPI document | Not on Go yet: generated from the Go code, it waits in redix-api #142 |

The business behind every page is the [BRS](https://claude.ai/artifact/JJPc1nSe6ekignSgXUpYB4) (private link), and what to build next is in this repo's issues.

## Use cases

Numbered in build order; the same number in the app's documentation is the same slice of the app.

| Use case | What it does | Who |
| --- | --- | --- |
| [UC-001](use-cases/UC-001-setup.md) | The base: the database, health check, Swagger, and the words the app shows | Everyone |
| [UC-002](use-cases/UC-002-sign-in.md) | Sign in with a username and password, keep the session alive, and reset a forgotten password with a code by email | Every user |
| [UC-003](use-cases/UC-003-change-password.md) | Change the password, and secure every request | Every signed-in user |
| UC-004 | Switching the app language is the app's use case; the API's part is the words of My account, added with the `LanguageSwitch` migration | Every signed-in user |
| [UC-005](use-cases/UC-005-history.md) | The History table: who did what to which record, when, with the values before and after; never changed or deleted | The API, on its own |
| [UC-006](use-cases/UC-006-projects.md) | A project and its units: rename them, delete a unit, and close the project when finished | Whoever does the project's finance |
| [UC-007](use-cases/UC-007-users.md) | Correct and reset logins, and make a person inactive in one project | A project manager, for someone only in projects they manage |
| [UC-008](use-cases/UC-008-project-people.md) | Add people to a project and tick their permissions, and show each person only their projects | Whoever manages its users adds and ticks; everyone reads |
| [UC-009](use-cases/UC-009-permissions.md) | Keep what each person may do in the database, keep projects separate, and enforce both on the server | Every request |
| [UC-010](use-cases/UC-010-default-project.md) | Keep each person's default project, and open it at sign-in; and their own email, changed with a code | Everyone, for themselves |
| [UC-011](use-cases/UC-011-biometrics.md) | Keep a phone with fingerprint or face signed in | Everyone, for this phone |
| [UC-012](use-cases/UC-012-categories-and-items.md) | Shape a project's categories, items and subgroups, and start each approved project with a copy | Finance changes; everyone with access reads |
| [UC-013](use-cases/UC-013-item-columns.md) | Change the fields an item asks for, on one item or on the project's default template | Finance changes; everyone with access reads |
| [UC-014](use-cases/UC-014-payment-methods-and-vat.md) | Keep a project's payment methods | Finance changes; everyone with access reads |
| [UC-015](use-cases/UC-015-accounts-and-custodies.md) | Define a project's bank accounts and cash custodies, and who may spend from them | Finance changes; everyone with access reads |
| [UC-016](use-cases/UC-016-partners.md) | Keep a project's partners, each linked to their login, so their capital is theirs to see | Finance changes; `SeeAllCapital` reads all; anyone else only their own |
| [UC-017](use-cases/UC-017-suppliers.md) | Keep a project's supplier register, with one VAT number to one supplier | Finance changes; everyone with access reads |
| [UC-018](use-cases/UC-018-search-suppliers.md) | Find a supplier by name or VAT number while entering an expense | Everyone with access |
| [UC-019](use-cases/UC-019-quick-add-supplier.md) | Add a missing supplier from the field, pending approval | Finance and Field |
| [UC-020](use-cases/UC-020-contracts.md) | Record what each contractor was promised, to follow paid and remaining | Finance changes; everyone with access reads |
| [UC-021](use-cases/UC-021-attachments.md) | Keep an invoice's picture or PDF, or a contract's document, and serve it only to people with access to the project | Finance and Field keep; everyone with access reads |
| [UC-022](use-cases/UC-022-purchase-expense.md) | Record a purchase in the ledger, its figures computed as the accountant computes them | Finance and Field |
| [UC-023](use-cases/UC-023-duplicate-invoice.md) | Retired: an expense has no bill number to repeat (redix-api #96) | Nobody |
| [UC-025](use-cases/UC-025-expense-without-invoice.md) | Retired: every expense's file is optional, so none needs a reason (redix-api #96) | Nobody |
| [UC-026](use-cases/UC-026-credit-note.md) | Record money coming back: a credit note, or a deduction | Finance and Field |
| [UC-027](use-cases/UC-027-contractor-payment.md) | Pay a contractor against a contract, and keep its remaining right | Finance and Field |
| [UC-028](use-cases/UC-028-submit-entry.md) | Read an entry, and send a draft for approval | Finance and Field; everyone with access reads |
| [UC-029](use-cases/UC-029-offline-uploads.md) | Take an entry sent from a phone exactly once, however often it is retried | The app, on its own |
| [UC-030](use-cases/UC-030-sync.md) | Send a phone what it keeps to work without a network | The app, for anyone who reads the project |
| [UC-031](use-cases/UC-031-pending-entries.md) | List what waits for approval, oldest first, with its file | Whoever decides entries |
| [UC-032](use-cases/UC-032-approve-reject.md) | Approve one or many, or reject saying why | Whoever decides entries |
| [UC-033, UC-034, UC-035](use-cases/UC-033-change-entry.md) | Change an unapproved entry, keep the approved version, remove an approved one | Who entered it, and whoever decides entries |
| [UC-036](use-cases/UC-036-notifications.md) | Tell people what happened to an entry or a project | Everyone signed in |
| [UC-039](use-cases/UC-039-attach-invoice.md) | Attach an invoice to an expense entered without one | Whoever enters expenses |
| [UC-061](use-cases/UC-061-liquidation.md) | Retired 8 Nov 2026: liquidation and closing removed | — |
| [UC-064](use-cases/UC-064-documents.md) | Keep a project's documents, any file, in optional groups | The manager keeps them; everyone reads |
| [UC-053 to UC-058](use-cases/UC-053-reports.md) | The numbered periodic report, and every report as PDF and workbook; the VAT statement as a workbook | Finance issues; everyone reads; whoever sees all capital exports |
| [UC-049, UC-050, UC-051, UC-063](use-cases/UC-049-summary-and-reports.md) | The summary, an item's report, the contracts, completion and cost | Everyone in the project |
| [UC-047, UC-056](use-cases/UC-047-revenue-and-vat.md) | Revenue, and the VAT statement's workbook for any period, its VAT worked out and never stored | Whoever does the project's finance; everyone reads |
| [UC-040 to UC-044](use-cases/UC-040-money-movements.md) | Reconciliation, capital movements, and shares | Whoever does the project's finance; everyone reads |
| [UC-066](use-cases/UC-066-register-a-project.md) | Register a project with its manager, and approve it | Anyone registers; only the admin approves |
| [UC-070](use-cases/UC-070-config.md) | Keep configurable values in one table, starting with how long the app waits for an answer | The app, before sign-in |
| [UC-071](use-cases/UC-071-load-a-project-file.md) | Load a project's JSON file: its structure, entries, actual balances and last report | The manager who created the project |
| [UC-062](use-cases/UC-062-sign-out.md) | Sign out of one device | Every signed-in user |
