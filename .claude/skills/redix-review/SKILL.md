---
name: redix-review
description: What "review" means in the REDIX API - Muteb's own review of a change before it is committed - simple, clean, well structured, no customization, named and cited from the business, then the checks in order. Read it whenever he says "review", "our review", "review then commit" or "redix review", and before committing any change to redix-api.
---

# REDIX review (API)

> Brought from the Testin project, 12 November 2026, where Muteb defined it: *"our review ... simple code, clean, well-structured, no customization and every thing based on business. no redundant as possible unless it required to make code simpler. rules and use cases added to each method and class."*

A review is not a bug hunt. It takes a change that already works and makes it **simpler without changing what it does**, then commits it. What the app sees stays exactly as tested; only the shape of the code moves.

## The seven questions

Ask each of every file in the diff, in this order.

| # | Question | What a "no" looks like | Read |
| --- | --- | --- | --- |
| 1 | **Is it simple?** The direct form, fewest parts, fewest lines for the same behavior, the framework's feature before one of ours | A helper, flag or parameter that carries what the caller already knows; a hand-written copy of a framework feature | `deliver-smallest-change`, `use-what-the-platform-has` |
| 2 | **Is it clean?** No NULL and no nullable column or property, simple names with `using`, no dead code, zero build warnings | A `?` meaning "not set", a qualified name in a line, an unused parameter | `redix-code-standard`, `using-never-qualify` |
| 3 | **Is it well structured?** Each class one job its name says, in its feature folder; a change one migration step, merged into one a release | Routes mapped and rules decided in one class; a query a report needs inside a change class | `a-class-does-one-job`, `logic-lives-with-its-owner` |
| 4 | **Does every shared rule have one owner?** Who sees what, money precision, a word | The same condition written in two endpoints | `centralize-shared-design` |
| 5 | **No customization?** One route or surface for every case, each refusing with its reason | A per-screen variant of an endpoint | `one-surface-many-states` |
| 6 | **Is everything named for the business?** The glossary's words, in full | `Dto`, `Data`, a shortened word, a near-twin name | `names-say-the-business` |
| 7 | **Does everything that carries business cite its rule, and do the documents say what the code does?** | A behavior with no `BR-`, `FR-`, `AC-` or `UC-` in its doc comment; a Rules or Endpoints table left behind | `code-cites-its-rule` |

**No redundancy, unless it makes the code simpler.** Keep a repetition only when removing it needs more code or a less readable shape, and say so.

## What a review may and may not do

- **May:** rename, move a method to its owner, inline a one-use helper, delete a parameter, a branch or a class, merge two copies into one owner, add a missing citation, reword a document the code no longer matches.
- **May not:** change what the API answers, add a feature, or fix an unrelated defect inside the review. Those are findings: rows in #76 (`report-findings`).
- **If a feature is what makes the code complicated,** say so with its cost and ask whether to keep, simplify or remove it. Removing it is a fair answer.
- **Fewer lines or the same** for the same behavior. A review that grows the diff explains each added line.

## Checks before the commit

In this order.

1. `gofmt -l` lists nothing and `go vet ./...` is clean, from `go-backend/`. A finding is fixed, never silenced.
2. `make test`: the whole suite, the replay of the .NET API's recorded answers among them; `make cover` before a push, which fails below the coverage floor (redix-api #141).
3. A migration: a new numbered file in `internal/db/migrations`, named for what it adds; `make sqlc` when a query changes. The qa database is backed up before it is migrated.
4. When the app sees the change: deploy to qa (on Muteb's PC, `C:\Users\mtb\REDIX\.night\qa97.ps1`, which backs up first) and run the app's Patrol classes that reach it (redix-app's `redix-review`).

## Reporting it

One short table of what the review changed and why, then the checks and their results, then any finding left as a finding. Commit on `main`, one commit per use case, ending with the `Co-Authored-By` line and no session link. Push only when Muteb says push.
