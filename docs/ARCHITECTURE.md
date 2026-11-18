[Documentation](README.md) › Architecture

# Architecture

One Go program and one SQLite file, on one small Linux server (redix-api #140). It keeps every path, answer, status code and error the .NET API gave, at its old paths and again under `/api/v1`; the .NET API was removed on 17 Nov 2026. Everything lives in `go-backend/`, whose [README](../go-backend/README.md) holds its settings, commands and deployment.

```
go-backend/
  cmd/server/             the API: reads its settings, brings the database up to date, serves the routes and the web front end's files
  internal/db/            the SQLite file: one writer and four readers, the migrations (goose), and the one way to write, InTx
  internal/db/store/      the code sqlc generates from every domain's queries.sql, each query prepared once as the server starts
  internal/changes/       what every change inside a project shares: one transaction, the refusal it rolls back with, a History line for every row
  internal/access/        what each person may do, everywhere and in each project, and the gate every route passes
  internal/auth/          sign-in, sessions and devices, the codes sent by email, the forgotten password, and the admin's seed
  internal/users/         people: their logins, their role names, who is inactive in which project
  internal/projects/      projects and their units, registration and approval, each person's default project
  internal/structure/     what entries are recorded against: categories, items, subgroups and columns, the default template, Add entry's tables
  internal/settings/      what a project keeps besides its structure: its payment methods
  internal/config/        the configurable values (UC-070): request and file timeouts, the VAT rate
  internal/accounts/      where a project's money sits: its accounts and who holds them, transfers, the reconciliation
  internal/partners/      a project's partners, their capital movements and shares
  internal/suppliers/     the supplier register, its search, and the rule of a VAT number
  internal/contracts/     what a project's contractors were promised, what the ledger holds inside each, and what remains
  internal/entries/       the ledger: recording, approving, rejecting and removing entries, an item's report, the VAT statement
  internal/attachments/   a project's files: the record of each, the folder they live in, and the signed links that read them
  internal/documents/     a project's documents (UC-064): any file under a name, in optional groups
  internal/notifications/ who was told what happened to an entry or a project (UC-036)
  internal/reports/       the summary, the contracts and the numbered periodic report, their workbooks and PDFs
  internal/statement/     the workbook every report file shares (excelize), and the report PDFs' pages, laid out by Typst
  internal/sync/          what a phone keeps so it can work with no network (UC-030)
  internal/projectfile/   a project's file (UC-071): a project that lived in Excel, loaded through the owner of each part
  internal/history/       the History table, and historytest, the net every test request runs under
  internal/translations/  the words the app shows, by language, with an ETag
  internal/legal/         the privacy policy and the terms, served at /privacy and /terms, and the copyright notice every source file opens with
  internal/incidents/     one log line per request, every failure kept 90 days in incidents.db with its trace id, a panic answered 500
  internal/email/         the one sender of every email: into a folder today (redix-api #127)
  internal/naming/        what a name may be
  internal/platform/      what every domain shares: the router (web), problem answers (problem), tokens and passwords (credentials),
                          money (fixed), moments (moment), the clock, the limits, the environment's settings, Arabic spelling
  internal/server/        every domain's routes on one router, and /health
  internal/testapi/       the one way a domain's tests ask the API
  internal/replay/        the .NET API's recorded tests, replayed against Go with every answer compared (testdata/golden)
  deploy/                 the server setup script (setup.sh), the systemd units, Litestream and rclone
docs/                     these pages
```

## A request

1. `net/http` receives it. `problem.Traced` turns any failure into RFC 7807 problem details with a `code` the app translates.
2. The route's gate, `access.Gate`, checks the access token (`credentials`, JWT) and that its device is still open, then the permission the route needs. What a person holds is kept in the access book, which every committed write refreshes (`db.AfterEveryWrite`).
3. A read goes through `db.Queries()`, the read pool. A change goes through `db.InTx`, inside `changes.Change`: one IMMEDIATE transaction on the one writer, with a History line for every row it adds, changes or removes. Nothing inside it waits on the network or a file.
4. The rule a request depends on lives with the owner of the data (`entries.Record`, `accounts.Enter`, `contracts.Add`, `partners.Add`), so the route only coordinates.

## Recording an entry

Every entry, whatever its table, is recorded by `entries.Record` through one route, `POST /projects/{id}/entries`, and changed through `PUT /projects/{id}/entries/{entryId}` (redix-api #110). The phone names the entry's id, so a second post of the same entry answers the first. The recorder finds the entry's table among the tables the caller is offered, the same list the picker shows, checks what every entry shares by its item's side, then the checks and rows of the table's own kind: a purchase, a contract payment, revenue or a partner's capital.

## Rules every part follows

| Rule | Where it lives |
| --- | --- |
| Money is an INTEGER of ten-thousandths, rounding only in totals (BR-13); no VAT is stored, the VAT workbook works it out (redix-api #96, D4) | `internal/platform/fixed`, `internal/entries/vat.go` |
| Every moment is kept as UTC text with seven decimals, so its JSON names the zone and the phone shows it in its own time | `internal/platform/moment` |
| Every table is STRICT and its columns are snake_case; REDIX's core tables and each project's tables share the one file, and each project table's migration names its old schema (`project.Accounts`), so release 2 can still move a project's tables to its own database (redix-api #123) | `internal/db/migrations/00001_schema.sql` |
| Ids are time-ordered UUIDs (version 7) made in code, never by the database | Each owner's add |
| No NULL column: an optional value is an empty value of its own type or a row in a link table | The schema |
| Every column holding an enum has a foreign key to the enum's lookup table, `lk_{enum}` (redix-api #87) | The schema |
| History is append-only (AC-21.1) | Triggers `history_is_append_only_update` and `_delete` |
| The API sends codes, never words; the app shows the word for the code | `changes.Refuse`, `problem`, and the words in `translations` |
| Enums travel as camelCase names, such as `active` | `web.Enum` |
| Each environment is a name: `Dev` (automated runs, 127.0.0.1:5010, `RedixDev.db`), `QA` (Muteb's own testing, 127.0.0.1:5012, `RedixQA.db`) and `Prod` (the published server, answering HTTPS on its domain itself, its data under `/var/lib/redix`) | `internal/platform/environment` |
| Secrets come from environment variables prefixed `REDIX_`, never a file in the repo: on Muteb's PC Windows user variables, on the server `/etc/redix/redix.env`. An environment's own, prefixed `REDIX_{name}_`, win over the shared ones: `REDIX_QA_Admin__Password` | `internal/platform/environment` |
| Every email leaves through one sender, its words the translations' in the language the request asked in | `internal/email` |
| A business value someone may change while the API runs is a row of `config_values` (UC-070), never a setting in a file | `internal/config` |

The full data model is the pinned **Database design** issue. The database as it stands is mapped in [Database](database.md).

## Tests

`testapi.New` is the whole router on a fresh database of the test's own, with a clock the test moves and every email kept in memory; every request runs under the History net (`historytest`), which fails on any row changed without its History line. `internal/replay` replays the 852 tests the .NET API's suite recorded, comparing every answer, workbook cells included; since .NET's removal those recordings are kept as they are. `make test` runs everything, and `make cover` fails when coverage falls below its floor (redix-api #141).
