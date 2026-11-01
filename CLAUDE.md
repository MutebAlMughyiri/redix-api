# REDIX API

The server of REDIX (Real Estate Development Intelligence): ASP.NET Core on .NET 10 with SQL Server. The Flutter app is the `redix-app` repo.

- **The business:** the BRS (https://claude.ai/artifact/JJPc1nSe6ekignSgXUpYB4, private). What to build is in this repo's issues: features, and use cases titled `UC-NNN` in build order. The same number in `redix-app` is the same slice of the app.
- **The database:** the pinned **Database design** issue.
- **Findings:** every gap, bug and finding, from either repo, goes into the pinned issue "Findings: open gaps, bugs and decisions" as a row, never only into a chat.
- **Secrets:** Windows user environment variables prefixed `REDIX_` (for example `REDIX_Jwt__SigningKey`), read by `AddEnvironmentVariables("REDIX_")`. Never in the repo.
- **Before writing any code:** read `docs/ARCHITECTURE.md` and the `redix-code-standard` skill in `.claude/skills/`.
