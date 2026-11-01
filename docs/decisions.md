[Documentation](README.md) › Decisions

# Decisions

Newest first. Each says what was decided and why; the code and the issues hold the details.

| Date | Decision | Why |
| --- | --- | --- |
| 2026-11-01 | The audit trail is append-only by a database trigger that refuses every update and delete | One guard for every path: the API, a bulk query, or a tool connected to the database |
| 2026-11-01 | A trail record is built from EF Core's change tracker and saved by the same `SaveChanges` as its change | The values come from the change itself, and a change is never kept without its record |
| 2026-11-01 | The table is named `AuditTrail`, the glossary's code name, not `AuditLog` | One name for one thing; the database design issue follows the glossary |
| 2026-11-01 | Only a successful sign-in is recorded; wrong passwords are not | UC-002 records the sign-in; whether failed attempts and lockouts belong in the trail waits in the findings issue (row 22) |
| 2026-11-01 | A new password has at least 8 characters and differs from the current one | The BRS sets no rule; awaiting confirmation in the findings issue |
| 2026-11-01 | Every endpoint is closed by default (a fallback policy) and opened explicitly | A new endpoint cannot be left open by mistake; an unknown address answers 401 to an anonymous caller, revealing nothing |
| 2026-11-01 | A wrong current password on the change screen answers 400, not 401 | 401 means the session ended; the app renews and retries on 401, which must not happen for a typing mistake |
| 2026-11-01 | Sign-in uses ASP.NET Core's `PasswordHasher`, a 15-minute JWT access token, and a refresh token stored as its hash, replaced at each renewal and expired after 12 hours unused | The framework's own pieces; the idle period answers NFR-09, which gives no number |
| 2026-11-01 | The first owner is created by EF Core's seeding from the `REDIX_Owner__*` environment variables, with a password that must be changed | No password or hash ever sits in the repo or a migration |
| 2026-11-01 | Tests run on their own database, `RedixTests`, rebuilt each run | Tests never touch the developer's data and start from a known state |
| 2026-11-01 | Every gap, bug and finding goes into the pinned issue "Findings: open gaps, bugs and decisions" | One list, so nothing is lost between sessions |
| 2026-11-01 | Secrets live in Windows user environment variables prefixed `REDIX_` | Never in the repo, readable only by the developer's account, and no other .NET project picks them up |
| 2026-11-01 | Every public type and member has a doc comment; the build warns on a missing one | A contributor understands each method, and endpoint comments become the Swagger reference |
| 2026-11-01 | Words live in the database (`TranslationKeys`, `Translations`), served one language at a time with an ETag | The app's code holds only keys; a word changes without an app release |
| 2026-11-01 | Swagger and the OpenAPI document are served everywhere except Production | Test builds can be explored; production exposes nothing extra |
| 2026-11-01 | One shared migrated test server for every database test (xUnit collection fixture) | Separate fixtures raced to create the database on an empty server |
| 2026-11-01 | ASP.NET Core on .NET 10 with SQL Server, instead of Supabase and PostgreSQL | Full control of the server and its rules |
| 2026-11-01 | One project until a second one is needed | The smallest structure that works |
