---
name: code-cites-its-rule
description: In the REDIX API, every endpoint and every method that carries business says in its doc comment which rule or use case it carries (BR-, FR-, AC-, NFR- from the BRS, UC-NNN, or an issue's decision), and every change to what the API does changes the use case page's Rules table in the same commit. Use when writing or changing any Go under go-backend/, when writing a test, and when reviewing a diff.
---

# Code cites its rule

> Brought from the Testin project, 12 November 2026.

REDIX's business is written down before it is built: the BRS numbers every rule (`BR-`, `FR-`, `AC-`, `NFR-`), the use case issues are `UC-NNN` in build order, and a decision made on an issue is `redix-api #119 D4`. Two things follow.

> ## The code says which rule it carries, and the documents change when the code does.

## 1. The doc comment names the rule

Every exported name has a doc comment, and the rule goes in it:

```go
// Record records typed as a new entry of the change's project through every check Add entry passes, sent for approval when typed.Submit
// (EntryRecording.RecordAsync): whoever decides entries is told unless tellDeciders is false, as a project file's load records them (redix-api
// #132 D7). Each refusal names its column under columnKey. The one way an entry is recorded, by hand or by a project file's load.
func Record(c changes.Change, typed Request, tellDeciders bool) (store.Entry, error) {
```

A route's handler is a named method of its domain's `Routes`, so its comment says what the route answers and which rule it carries.

## 2. The test name states the rule

A Go test is named for the rule it proves, starting with its number: `TestBR33_APermissionCountsOnlyInTheProjectItWasTickedIn`. Every rule has a positive and a negative case.

## 3. The documents change in the same commit

| Document | What changes |
| --- | --- |
| `docs/use-cases/UC-NNN-*.md` | The Endpoints table, and the rule's row in the Rules table: **Rule**, **Enforced by** (the package and function), **Proved by** (the test) |
| `docs/README.md` | The use case's line, when a page is new |
| `docs/decisions.md` | A dated row when a decision was made |
| `docs/ARCHITECTURE.md` | A change to the structure |
| The pinned **Database design** issue | A change to a table, made there first, then in a migration |

A commit that changes what a route answers and leaves its row in the Endpoints table is incomplete.

## The identifier has to exist

A comment naming a rule the BRS or the use case page does not hold is worse than none. Grep `docs/use-cases` in both repos before citing.

## When there is no rule yet

Write the rule first. When the business is unclear or two rules disagree, stop: it is an open question for Muteb, never a guess in code (`redix-code-standard`).

## When a method carries no rule

Plumbing carries only its one-line summary: an unexported helper, the wiring in `internal/server`, a migration. The test: **could a user of the app describe what this does without being shown the code?** If yes, it has a rule, and the comment names it.

## Never weaken the documents to match the code

When code and document disagree, one is wrong and the disagreement is a finding (#76). Rewriting a rule so today's code satisfies it is not a fix.
