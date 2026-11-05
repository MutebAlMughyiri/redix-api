[Documentation](README.md) › Decisions

# Decisions

Newest first. Each says what was decided and why; the code and the issues hold the details.

| Date | Decision | Why |
| --- | --- | --- |
| 2026-11-05 | A default project is one row in `UserDefaultProjects`, and `Users.OpenDefaultProject` says whether sign-in skips the list | Having no default is no row, never an empty column |
| 2026-11-05 | Access to the default is checked when the session is built, and it is not audited | AC-37.2: a default without access is not offered; it is one person's preference, and changes no record of the business |
| 2026-11-05 | Anyone registers a project and becomes its manager; an admin approves it, and that is the admin's only privilege | Muteb: "we will have admin that her approve the registered projects". Replaces the system admin of the row below, which could reset every login |
| 2026-11-05 | No role counts in a project until it is approved, decided once in `UserAccess.OfAsync` | No route can forget it |
| 2026-11-05 | A project manager can never reset, change or add the admin's login | Otherwise a manager could sign in as the admin |
| 2026-11-05 | The public registration route is limited per address by ASP.NET Core's rate limiter | It is the one route that writes with no session |
| 2026-11-05 | Owner is gone. Each project has its own Project manager, and one System admin role starts projects and manages logins | Muteb: "each real estate project should has separate admin. each project is separate completely from other projects". Rows below that name an owner are history |
| 2026-11-05 | One login across projects, with a role per project and a project list after sign-in | Muteb, and FR-37. A login is the only thing projects share |
| 2026-11-05 | A login is changed only with `ManageUsers` in every project it is in | A login is shared, so otherwise one project's manager could reset a password and sign in to another project |
| 2026-11-05 | Roles and their permissions are rows; which permissions exist is code | Muteb: "make all of roles to be configurable from data base". A permission means something only when a route names it |
| 2026-11-05 | One ASP.NET Core policy per permission, answered by one `AuthorizationHandler`, read from the database on each request | Muteb: "no work arounds or customizations". A change counts at the next request, and nothing about roles is in the token |
| 2026-11-05 | One table holds user, project and role | With a user-role table and a user-project table, nothing says which role goes with which project |
| 2026-11-05 | Only the permissions a route uses exist, plus `SeeAllCapital`; each later use case adds its own | A permission with no route is dead code; the BRS matrix already says who gets each |
| 2026-11-05 | An owner has no `ProjectMembers` rows: being a financial supervisor in every project is what being an owner means | The wrong state cannot be stored: an owner's role cannot be lost, and a new project needs nothing added for owners |
| 2026-11-05 | Three roles: `FinanceSupervisor`, `FieldSupervisor`, and `Reader`, which Muteb added | Reader is the least a user can be given; what it may do arrives with UC-009 |
| 2026-11-05 | `ProjectMembers` has its own `Id`, with project and user unique together | The audit trail names one record by one id |
| 2026-11-05 | Users are managed by owners only, and an owner cannot be disabled | BR-29 names users as an owner's and keeps at least one owner |
| 2026-11-05 | The owner types a temporary password and passes it on; a reset also ends the lock and signs out every device | As the canvas draws it; a locked user could not use the new password, and a forgotten password may mean a lost phone |
| 2026-11-05 | Each change to a user has its own audit action | The trail then says what was done, as it does with `SignIn` |
| 2026-11-05 | A new project and its units are one request, saved together | A lost network cannot leave half a project |
| 2026-11-05 | The status is `Finished` (منتهٍ), and the refusal `projectFinished`; closing stays the action | The glossary owns code names; the status is stored as a number, so no data changed |
| 2026-11-05 | A request with a missing or null field answers 400, through two serializer settings | One rule for every request record, instead of a check on each field |
| 2026-11-05 | The session says whether its user is an owner | The app shows what only an owner may do; UC-009 replaces it when project roles decide |
| 2026-11-01 | Projects are managed by owners only until UC-008 brings project roles; the access token says whether its user is an owner | BR-29 makes an owner a financial supervisor in every project, and no other user has a project role yet |
| 2026-11-01 | GUID ids are made in code for every table, declared once in `RedixDbContext` | A record added through a navigation, such as a project's unit, is inserted; without it EF Core takes a set GUID for an existing row |
| 2026-11-01 | Project and unit names need not be unique | The BRS sets no rule; whether to refuse a repeated name waits in the findings issue (row 24) |
| 2026-11-01 | A refusal is built by one owner, `Refusal.Of`, for every feature | Every refused request answers the same shape, with a `code` the app translates |
| 2026-11-01 | The audit trail is append-only by a database trigger that refuses every update and delete | One guard for every path: the API, a bulk query, or a tool connected to the database |
| 2026-11-01 | A trail record is built from EF Core's change tracker and saved by the same `SaveChanges` as its change | The values come from the change itself, and a change is never kept without its record |
| 2026-11-01 | The table is named `AuditTrail`, the glossary's code name, not `AuditLog` | One name for one thing; the database design issue follows the glossary |
| 2026-11-01 | Only a successful sign-in is recorded; wrong passwords are not | UC-002 records the sign-in; whether failed attempts and lockouts belong in the trail waits in the findings issue (row 22) |
| 2026-11-01 | A new password has at least 8 characters and differs from the current one | The BRS sets no rule; awaiting confirmation in the findings issue |
| 2026-11-01 | Every endpoint is closed by default (a fallback policy) and opened explicitly | A new endpoint cannot be left open by mistake; an unknown address answers 401 to an anonymous caller, revealing nothing |
| 2026-11-01 | A wrong current password on the change screen answers 400, not 401 | 401 means the session ended; the app renews and retries on 401, which must not happen for a typing mistake |
| 2026-11-01 | Sign-in uses ASP.NET Core's `PasswordHasher`, a 15-minute JWT access token, and a refresh token stored as its hash, replaced at each renewal and expired after 12 hours unused | The framework's own pieces; the idle period answers NFR-09, which gives no number |
| 2026-11-01 | The first owner, now the admin (`REDIX_Admin__*`), is created by EF Core's seeding from the `REDIX_Owner__*` environment variables, with a password that must be changed | No password or hash ever sits in the repo or a migration |
| 2026-11-01 | Tests run on their own database, `RedixTests`, rebuilt each run | Tests never touch the developer's data and start from a known state |
| 2026-11-01 | Every gap, bug and finding goes into the pinned issue "Findings: open gaps, bugs and decisions" | One list, so nothing is lost between sessions |
| 2026-11-01 | Secrets live in Windows user environment variables prefixed `REDIX_` | Never in the repo, readable only by the developer's account, and no other .NET project picks them up |
| 2026-11-01 | Every public type and member has a doc comment; the build warns on a missing one | A contributor understands each method, and endpoint comments become the Swagger reference |
| 2026-11-01 | Words live in the database (`TranslationKeys`, `Translations`), served one language at a time with an ETag | The app's code holds only keys; a word changes without an app release |
| 2026-11-01 | Swagger and the OpenAPI document are served everywhere except Production | Test builds can be explored; production exposes nothing extra |
| 2026-11-01 | One shared migrated test server for every database test (xUnit collection fixture) | Separate fixtures raced to create the database on an empty server |
| 2026-11-01 | ASP.NET Core on .NET 10 with SQL Server, instead of Supabase and PostgreSQL | Full control of the server and its rules |
| 2026-11-01 | One project until a second one is needed | The smallest structure that works |
