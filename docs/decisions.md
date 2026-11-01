[Documentation](README.md) › Decisions

# Decisions

Newest first. Each says what was decided and why; the code and the issues hold the details.

| Date | Decision | Why |
| --- | --- | --- |
| 2026-11-01 | Every gap, bug and finding goes into the pinned issue "Findings: open gaps, bugs and decisions" | One list, so nothing is lost between sessions |
| 2026-11-01 | Secrets live in Windows user environment variables prefixed `REDIX_` | Never in the repo, readable only by the developer's account, and no other .NET project picks them up |
| 2026-11-01 | Every public type and member has a doc comment; the build warns on a missing one | A contributor understands each method, and endpoint comments become the Swagger reference |
| 2026-11-01 | Words live in the database (`TranslationKeys`, `Translations`), served one language at a time with an ETag | The app's code holds only keys; a word changes without an app release |
| 2026-11-01 | Swagger and the OpenAPI document are served everywhere except Production | Test builds can be explored; production exposes nothing extra |
| 2026-11-01 | One shared migrated test server for every database test (xUnit collection fixture) | Separate fixtures raced to create the database on an empty server |
| 2026-11-01 | ASP.NET Core on .NET 10 with SQL Server, instead of Supabase and PostgreSQL | Full control of the server and its rules |
| 2026-11-01 | One project until a second one is needed | The smallest structure that works |
