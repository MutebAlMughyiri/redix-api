---
name: code-cites-its-rule
description: In the REDIX API, every endpoint and every method that carries business says in its XML doc comment which rule or use case it carries (BR-, FR-, AC-, NFR- from the BRS, UC-NNN, or an issue's decision), and every change to what the API does changes the use case page's Rules table in the same commit. Use when writing or changing any C# under src/, when writing a test, and when reviewing a diff.
---

# Code cites its rule

> Brought from the Testin project, 12 November 2026.

REDIX's business is written down before it is built: the BRS numbers every rule (`BR-`, `FR-`, `AC-`, `NFR-`), the use case issues are `UC-NNN` in build order, and a decision made on an issue is `redix-api #119 D4`. Two things follow.

> ## The code says which rule it carries, and the documents change when the code does.

## 1. The doc comment names the rule

Every public type and member has an XML doc comment (`GenerateDocumentationFile` is on, CS1591 is a warning, and a warning is fixed), and the rule goes in it:

```csharp
/// <summary>
/// The project's entries <paramref name="userId"/> may see, never a removed one (redix-api #119): a draft only for who entered it (BR-42), and a partner's money only for whoever sees that partner (BR-31, <see cref="Partners.PartnerSight"/>).
/// </summary>
```

An endpoint is a named static method, so its comment is its Swagger summary too. Measured 12 November 2026: 116 of the 144 files in `src/` (outside `Migrations/`) cite a `BR-`, `FR-`, `AC-`, `NFR-` or `UC-` number.

## 2. The test name states the rule

An xUnit test is named for the rule it proves, starting with its number: `AC_14_2_D4_ACreditNoteIsANegativeTotalOfItsOwnThatTakesFromTheItem`. Every rule has a positive and a negative case.

## 3. The documents change in the same commit

| Document | What changes |
| --- | --- |
| `docs/use-cases/UC-NNN-*.md` | The Endpoints table, and the rule's row in the Rules table: **Rule**, **Enforced by** (the class and method), **Proved by** (the test) |
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

Plumbing carries only its one-line summary: a private helper, a mapping in `Program.cs`, a migration. The test: **could a user of the app describe what this does without being shown the code?** If yes, it has a rule, and the comment names it.

## Never weaken the documents to match the code

When code and document disagree, one is wrong and the disagreement is a finding (#76). Rewriting a rule so today's code satisfies it is not a fix.
