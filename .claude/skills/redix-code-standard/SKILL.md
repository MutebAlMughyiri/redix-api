---
name: redix-code-standard
description: How REDIX API code is written - best practice of ASP.NET Core and EF Core, simple, clean and well structured, no customizations or workarounds, and only what the business (the BRS and the use case) asks. Read before writing or changing any code in this repo.
---

# REDIX code standard (API)

> "Our code must be best practice, simple, clean and structured well. No customizations or workarounds, as we follow the business." — Muteb, 1 November 2026

## Follow the business

- The BRS and the use case issue (`UC-NNN`) say what the code does. Build exactly that: no extra feature, and no rule bent to make the code easier.
- Every requirement and rule the use case lists has an xUnit test that runs it. The test name states the rule.
- When the business is unclear, or two rules disagree, stop. It becomes an open question in the BRS, never a guess in code.

## Best practice of the platform, no customizations

- Use what ASP.NET Core and EF Core already provide, the way their documentation shows it: minimal API endpoints, problem details, health checks, EF Core migrations, `HasData` for seed rows, the built-in OpenAPI document.
- Never hand-write a replacement for a framework feature, and never work around one. If the framework cannot do what the use case needs, say so before writing code.
- Keep packages current. A warning is fixed, never suppressed: a vulnerable transitive package is pinned to its fixed version.

## Simple and clean

- The smallest code that does the job: no dead code, no speculative helper, no comment that repeats the code.
- Every shared rule has one owner, and every caller uses it. Example: money precision is one convention in `RedixDbContext`, not an attribute on each property.
- Names come from the translations glossary (redix-app issue "Translations: English terms and their Arabic"). Its "In code" column is the C# name.
- No NULL. No nullable column or property: an optional value is an empty value of its own type (`""`, `0`, `1900-01-01`, an `Empty` enum constant) or a row in a link table.

## Documented

- Every public type and member has an XML doc comment saying what it is for, in one or two sentences, never how it does it. A missing one is a build warning (CS1591), and a warning is fixed.
- An endpoint is a named static method, so its doc comment becomes its Swagger summary.
- A use case is done only when `docs/use-cases/UC-NNN-<name>.md` exists: its purpose, who may use it, its endpoints and tables, and each rule with the class that enforces it and the test that proves it. It is listed in `docs/README.md`.
- A decision goes into `docs/decisions.md`; a change to the structure goes into `docs/ARCHITECTURE.md`.

## Structured well

- One folder per feature under `src/Redix.Api`, holding its entities, seed and endpoint (for example `Translations/`). Tests mirror it.
- Every table follows the pinned **Database design** issue. A change to the model is made there first, then in a migration.
- The database starts from one baseline migration (Muteb, 13 Nov 2026, redix-api #123). A change adds a step named after what it adds; before a release is published, its steps merge into one, proven by comparing a database built fresh with RedixQA, so the folder holds the baseline and one step a release.

## The skills that hold each part in detail

Brought from the Testin project on 12 November 2026; read the one a change touches.

| Skill | Holds |
| --- | --- |
| `a-class-does-one-job` | A class does one job, and its name says which |
| `names-say-the-business` | Every name is the glossary's word, in full |
| `code-cites-its-rule` | The doc comment names its rule; the use case page's tables change in the same commit |
| `use-what-the-platform-has` | The framework's feature first; what the API already uses |
| `using-never-qualify` | A type by its simple name, with a `using` |
| `redix-review` | The seven questions and the checks before a commit |
| `redix-story` | The issue format, labels and milestone, for both repos |
