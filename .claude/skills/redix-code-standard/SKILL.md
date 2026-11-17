---
name: redix-code-standard
description: How REDIX API code is written - best practice of Go and its standard library, SQLite through sqlc, simple, clean and well structured, no customizations or workarounds, and only what the business (the BRS and the use case) asks. Read before writing or changing any code in this repo.
---

# REDIX code standard (API)

> "Our code must be best practice, simple, clean and structured well. No customizations or workarounds, as we follow the business." — Muteb, 1 November 2026

The API is Go with SQLite, in `go-backend/`, since 17 November 2026 (redix-api #140).

## Follow the business

- The BRS and the use case issue (`UC-NNN`) say what the code does. Build exactly that: no extra feature, and no rule bent to make the code easier.
- Every requirement and rule the use case lists has a Go test that runs it. The test name states the rule.
- When the business is unclear, or two rules disagree, stop. It becomes an open question in the BRS, never a guess in code.

## Best practice of the platform, no customizations

- Use what Go's standard library and the packages the API already depends on provide, the way their documentation shows it: `net/http`'s router, `database/sql` with sqlc's generated queries, goose's migrations, `log/slog`.
- Never hand-write a replacement for a library feature, and never work around one. If none can do what the use case needs, say so before writing code.
- Keep modules current. `go vet` stays clean: a finding is fixed, never silenced.

## Simple and clean

- The smallest code that does the job: no dead code, no speculative helper, no comment that repeats the code.
- Every shared rule has one owner, and every caller uses it. Example: money is one type, `fixed.Number`, never a float or a column's own rounding.
- Names come from the translations glossary (redix-app issue "Translations: English terms and their Arabic"). Its "In code" column is the Go name.
- No NULL. No nullable column or field: an optional value is an empty value of its own type (`""`, `0`, `1900-01-01`, an empty constant) or a row in a link table.

## Documented

- Every exported name has a doc comment saying what it is for, in one or two sentences, never how it does it.
- A use case is done only when `docs/use-cases/UC-NNN-<name>.md` exists: its purpose, who may use it, its routes and tables, and each rule with the function that enforces it and the test that proves it. It is listed in `docs/README.md`.
- A decision goes into `docs/decisions.md`; a change to the structure goes into `docs/ARCHITECTURE.md`.

## Structured well

- One package per domain under `go-backend/internal`, holding its routes, its rules and its `queries.sql` (for example `internal/entries`). Its tests sit beside it.
- Every table follows the pinned **Database design** issue. A change to the model is made there first, then in a migration in `internal/db/migrations`.
- A migration adds a step named after what it adds; a database is brought up to date as the server starts.

## The skills that hold each part in detail

Brought from the Testin project on 12 November 2026; read the one a change touches.

| Skill | Holds |
| --- | --- |
| `a-class-does-one-job` | A type, file or package does one job, and its name says which |
| `names-say-the-business` | Every name is the glossary's word, in full |
| `code-cites-its-rule` | The doc comment names its rule; the use case page's tables change in the same commit |
| `use-what-the-platform-has` | The library's feature first; what the API already uses |
| `redix-review` | The seven questions and the checks before a commit |
| `redix-story` | The issue format, labels and milestone, for both repos |
