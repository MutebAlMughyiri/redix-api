---
name: use-what-the-platform-has
description: Before building any part of a REDIX API feature - an error answer, a check, a schema change, seed data, a secret, a sign-in, a document, a workbook, a PDF, a cache, a scheduled job - find what ASP.NET Core, EF Core, SQL Server or an already-used package does, and use it the way its documentation shows. Build REDIX's own only when none can do it, and say so with the evidence before writing code. Use when designing a feature, when choosing between "use X" and "build X", and when reviewing code that hand-writes what the framework has.
---

# Use what the platform has

> Brought from the Testin project, 12 November 2026, where Muteb wrote: *"use exist idea tools is better that customize new thing."* REDIX's own form of it: *"No customizations or workarounds, as we follow the business."* (Muteb, 1 November 2026, `redix-code-standard`).

A framework feature is documented, tested and maintained by Microsoft. A hand-written copy is a second thing to learn and a part REDIX maintains forever, and it is where the bugs hide.

> ## The framework's feature first, the way its documentation shows it. Our own only with evidence that there is none.

## The order of choice

1. **ASP.NET Core, EF Core or SQL Server**, as their documentation shows it.
2. **A package the API already depends on.**
3. **A new package**: named in the feature's design, with why 1 and 2 cannot, so it is approved with the design; kept current like every package.
4. **REDIX's own**: only when 1 to 3 cannot do it. Say what was checked and why it cannot, in the design or the issue, before the first line.

Never offer 4 as a peer of 1, and never work around a framework feature.

## What the API already uses

Reach for these; never build beside them.

| Need | Platform | Where |
| --- | --- | --- |
| An error answer | Problem details (`AddProblemDetails`, `UseExceptionHandler`) | `Program.cs` |
| Is the API and its database up | Health checks (`AddHealthChecks().AddDbContextCheck`) | `/health` |
| The API's document | `AddOpenApi`, Swagger UI | `/openapi/v1.json`, `/swagger` |
| Sign-in | JWT bearer (`AddJwtBearer`) | `Program.cs` |
| Who may call a route | Authorization policies | `MapProjectChanges(Permission...)` |
| A change to the tables | EF Core migrations, one per use case, named for what it adds | `src/Redix.Api/Migrations` |
| Rows every database starts with, the words | `HasData` | `Translations/TranslationSeed.cs` |
| A secret | Configuration from environment variables (`AddEnvironmentVariables("REDIX_")`) | Windows user variables, never the repo |
| A rule the data must never break | A SQL Server constraint or trigger | History is append-only (trigger `HistoryIsAppendOnly`, AC-21.1) |
| A workbook | ClosedXML | `Reports/` |
| A PDF | QuestPDF | `Reports/StatementPdf.cs` |

## What this does not mean

- **Not "never write our own".** The business is REDIX's own: an entry's rules, who sees a partner's money, the periodic report. When the framework has no feature for the domain, the code is REDIX's, and that is correct.
- **Not a reason to bypass an owner.** Where REDIX has one owner of a rule (`PartnerSight`, `EntryChanges`), a feature calls it, never the tables directly.

## Checks before building a part

1. Which framework feature, or which package already in `Redix.Api.csproj`, does this? Name it.
2. Does REDIX already own it? Call the owner.
3. If none works, write down what was checked and why it cannot, before the first line of code.
