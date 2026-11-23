[Documentation](../README.md) › UC-001

# UC-001: Set up the environment and base code

**What it is for:** the base every later use case builds on: a running API on SQLite, a health check, Swagger outside production, and the words the app shows.
**Who uses it:** everyone, before sign-in. Nothing here needs a permission.
**Issue:** redix-api #20

## Endpoints

| Endpoint | Purpose | Sign-in |
| --- | --- | --- |
| `GET /health` | Answers `Healthy` when the API runs and reaches the database | No |
| `GET /translations/{language}` | Every word of `ar` or `en` in one bundle, with an ETag; `304` while unchanged, `404` for another language | No: the sign-in screen needs its words |
| `GET /swagger`, `GET /openapi/v1.json` | Swagger UI on the OpenAPI document of every route, generated from the Go code: its path, what it reads, what it answers with each status, the codes it refuses with, the permission it needs (#142) | No; served in Dev and QA only, never Prod |
| `GET /privacy`, `GET /terms` | The privacy policy and the terms of use, one copy each, in Arabic and English (redix-api #143) | No: the stores link to them, and a person reads them before registering |

## Tables

| Table | Holds |
| --- | --- |
| `translation_keys` | One row per word, named by its key, such as `app.title` |
| `translations` | The word's value in each language, unique per key and language |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Money is an INTEGER of ten-thousandths, rounding only in totals (BR-13) | `fixed.Number` | `TestBR13_AnAmountIsWrittenWithItsFourDecimalsAsDotNetWritesIt`, `TestBR13_AnAmountIsReadExactlyAndRoundedPastFourDecimalsAsSqlServerStoresIt`, `TestBR13_AddingAmountsNeverLosesAHalala` |
| Every table has a required rowversion. **Retired with SQL Server (redix-api #140 N5):** SQLite writes one transaction at a time, so no row needs a version | - | - |
| `/health` checks the database | `server.Routes`: `/health` pings the database | `TestTheServerStartsAnswersAndStopsWhenAsked` (cmd/server) |
| A bundle holds every word of its language, each language its own (AC-86.7 among them) | `translations.bundle` | `TestUC001_EveryWordOfALanguageComesAsOneBundleWithAnETag`, `TestUC001_AC_86_7_EachLanguagesBundleHoldsItsOwnWords` |
| An unchanged bundle answers `304` | `translations.bundle` | `TestUC001_AnUnchangedBundleIsNotSentAgain` |
| Another language answers `404` | `translations.bundle` | `TestUC001_ALanguageWithNoWordsIsNotFound` |
| Swagger UI and the OpenAPI document are served in Dev and QA only, never Prod or an environment no settings name (#142 R4) | `environment.Read` (`Documented`), `server.Routes` | `TestR4_OnlyDevAndQaServeTheAPIDocument`, `TestR2_SwaggerUIShowsTheOpenAPIDocumentOfEveryRoute` |
| The document describes every route from its code: the doc comment over its `Handle` call or on its handler, the permission of its gate, what `web.Read` reads, what `web.JSON` answers with each status, the codes `problem.Refuse` and `changes.Refuse` refuse with (#142 R1, Q1, Q4) | `cmd/openapi` (`go generate ./internal/apidoc`) | `TestR1_ARouteSaysWhatItReadsAnswersRefusesAndNeeds` |
| A route with no description, or a document the code has moved past, fails the build (#142 R3) | `cmd/openapi` | `TestR1_R3_TheKeptDocumentIsWhatTheCodeSaysAndEveryRouteIsDescribed` |
| Fields are named as the API answers them, camelCase, and an enum is the list of its names (#142 R5) | `cmd/openapi` | `TestR5_FieldsAreCamelCaseAndAnEnumIsItsNames` |
| The privacy policy and the terms are served to anyone, in every environment, in Arabic and English (#143 R3, R4, Q6) | `legal.Map` | `TestQ6_ThePrivacyPolicyAndTheTermsAreServedToAnyoneInArabicAndEnglish` |
| Both pages name REDIX's contact, info@ on the domain, under its heading in Arabic and in English (#147 R11) | `internal/legal/privacy.html`, `terms.html` | `TestR11_ThePrivacyPolicyAndTheTermsNameREDIXsContactInArabicAndEnglish` |
| Every Go and SQL file opens with Tech Details' copyright notice, word for word; a file a tool writes and marks so is the tool's (#143 R1, R6) | `.idea/copyright/` for new files | `TestR6_EveryGoAndSqlFileOpensWithTheNoticeWordForWord` |
| Every error answer carries a code, a bare status's too: `notFound`, `unexpected`, ... (#150 R1, R7) | `problem.Status` | `TestN16_R7_ABareNotFoundIsProblemDetailsAsUseStatusCodePagesWroteItWithItsCode`, `TestR1_R3_D1_ARefusalAnswersItsCodeAndIsKeptWithItsTraceRouteAndWhoAsked` |
| Every answer carries its trace id as `X-Trace-Id`, a problem's in its body too; the app's own `traceparent` keeps its trace id (#150 R2, R10) | `problem.Traced` | `TestR10_AnAnswerKeepsTheTraceIdOfTheAppsTraceparent` |
| Every request writes one log line: method, route with its ids named `{id}`, status, code, time taken, user, project, trace id (#150 R5) | `incidents.Recorded` | `TestR5_EveryRequestWritesOneLogLineAndASuccessIsNoIncident` |
| Every answer of 400 or more is kept in `incidents.db`, refusals included, with its trace id, route, code and who asked; never a password, code or file (#150 R3, R11, D1) | `incidents.Recorded` | `TestR1_R3_D1_ARefusalAnswersItsCodeAndIsKeptWithItsTraceRouteAndWhoAsked` |
| A panic is answered 500 with its code and trace id, and kept with its stack (#150 R6) | `incidents.Recorded` | `TestR6_APanicAnswers500WithItsCodeAndTraceIdAndIsKeptWithItsStack` |
| An incident the file cannot take is still in the log (#150 R8) | `incidents.Recorded` | `TestR8_AnIncidentTheFileCannotTakeIsStillInTheLog` |
| An incident is kept 90 days (#150 R12) | `incidents.Store` | `TestR12_AnIncidentPastNinetyDaysGoesWhenTheNextIsKept` |
