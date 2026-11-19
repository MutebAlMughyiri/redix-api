---
name: use-what-the-platform-has
description: Before building any part of a REDIX API feature - an error answer, a check, a schema change, seed data, a secret, a sign-in, a document, a workbook, a PDF, a cache, a scheduled job - find what Go's standard library, SQLite or a package the API already uses does, and use it the way its documentation shows. Build REDIX's own only when none can do it, and say so with the evidence before writing code. Use when designing a feature, when choosing between "use X" and "build X", and when reviewing code that hand-writes what a library has.
---

# Use what the platform has

> Brought from the Testin project, 12 November 2026, where Muteb wrote: *"use exist idea tools is better that customize new thing."* REDIX's own form of it: *"No customizations or workarounds, as we follow the business."* (Muteb, 1 November 2026, `redix-code-standard`).

A library's feature is documented, tested and maintained by others. A hand-written copy is a second thing to learn and a part REDIX maintains forever, and it is where the bugs hide.

> ## The library's feature first, the way its documentation shows it. Our own only with evidence that there is none.

## The order of choice

1. **Go's standard library or SQLite**, as their documentation shows it.
2. **A module the API already depends on** (`go-backend/go.mod`).
3. **A new module**: named in the feature's design, with why 1 and 2 cannot, so it is approved with the design; kept current like every module.
4. **REDIX's own**: only when 1 to 3 cannot do it. Say what was checked and why it cannot, in the design or the issue, before the first line.

Never offer 4 as a peer of 1, and never work around a library's feature.

## What the API already uses

Reach for these; never build beside them.

| Need | Platform | Where |
| --- | --- | --- |
| A route | `net/http`'s `ServeMux` patterns, through one router | `internal/platform/web` |
| An error answer | RFC 7807 problem details with a code | `internal/platform/problem` |
| Is the API and its database up | `/health`, a ping of the read pool | `internal/server` |
| Sign-in | JWT (`golang-jwt/jwt`) and PBKDF2 passwords | `internal/platform/credentials` |
| Who may call a route | The gate, each route naming its permission | `access.Gate.Needs` |
| A query | sqlc, generating Go from each domain's `queries.sql` | `internal/db/store` |
| A change to the tables | goose migrations, embedded in the binary, run as the server starts | `internal/db/migrations` |
| Rows every database starts with, the words | A migration's `INSERT`s | `internal/db/migrations/00002_seed.sql` |
| A secret | Environment variables prefixed `REDIX_` | Windows user variables, `/etc/redix/{prod,qa,dev}.env`, never the repo |
| A rule the data must never break | A SQLite constraint or trigger | History is append-only (triggers `history_is_append_only_*`, AC-21.1) |
| A workbook | excelize | `internal/statement` |
| A PDF | Typst, from a template and the page as JSON | `internal/statement` |
| A backup | Litestream for the database, rclone for the files | `deploy/` |

## What this does not mean

- **Not "never write our own".** The business is REDIX's own: an entry's rules, who sees a partner's money, the periodic report. When no library has the domain, the code is REDIX's, and that is correct.
- **Not a reason to bypass an owner.** Where REDIX has one owner of a rule (`entries.Record`, `access.Access.SeesCapitalOf`), a feature calls it, never the tables directly.

## Checks before building a part

1. Which standard library feature, or which module already in `go.mod`, does this? Name it.
2. Does REDIX already own it? Call the owner.
3. If none works, write down what was checked and why it cannot, before the first line of code.
