# REDIX API

The server of REDIX (Real Estate Development Intelligence): Go with SQLite, in `go-backend/` (redix-api #140; the .NET API it replaced was removed on 17 Nov 2026). The Flutter app is the `redix-app` repo.

- **The business:** the BRS (https://claude.ai/artifact/JJPc1nSe6ekignSgXUpYB4, private). What to build is in this repo's issues: features, and use cases titled `UC-NNN` in build order. The same number in `redix-app` is the same slice of the app.
- **The database:** the pinned **Database design** issue.
- **Findings:** every gap, bug and finding, from either repo, goes into the pinned issue "Findings: open gaps, bugs and decisions" as a row, never only into a chat.
- **Secrets:** environment variables prefixed `REDIX_` (for example `REDIX_Jwt__SigningKey`), read by `environment.Read`: on Muteb's PC Windows user variables, on the server `/etc/redix/redix.env`. An environment's own ones are prefixed `REDIX_{name}_`, such as `REDIX_QA_Admin__Password` (redix-api #118). Never in the repo.
- **Issues:** written in the `redix-story` skill's format (`.claude/skills/`), for both repos.
- **Before writing any code:** read `docs/ARCHITECTURE.md`, `go-backend/README.md` and the `redix-code-standard` skill in `.claude/skills/`.
