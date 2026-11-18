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
| `GET /swagger`, `GET /openapi/v1.json` | The endpoint reference. **Waits for redix-api #142:** the Go API serves neither yet | No; not served in Production |
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
| Money is `decimal(19,4)` (BR-13) | `fixed.Number` | Enforced by the model; checked in the migration |
| Every table has a required rowversion. **Retired with SQL Server (redix-api #140 N5):** SQLite writes one transaction at a time, so no row needs a version | - | - |
| `/health` checks the database | `server.Routes`: `/health` pings the database | `HealthTests.HealthAnswersAgainstMigratedDatabase` |
| A bundle holds every word of its language | `translations.bundle` | `TranslationsTests.BundleHoldsEveryWordOfTheLanguage` |
| An unchanged bundle answers `304` | `translations.bundle` | `TranslationsTests.UnchangedBundleAnswersNotModified` |
| Another language answers `404` | `translations.bundle` | `TranslationsTests.UnknownLanguageIsNotFound` |
| Swagger only outside Production. **Waits for redix-api #142:** the Go API serves no Swagger yet, so nothing enforces or proves it | - | - |
| The privacy policy and the terms are served to anyone, in every environment, in Arabic and English (#143 R3, R4, Q6) | `legal.Map` | `TestQ6_ThePrivacyPolicyAndTheTermsAreServedToAnyoneInArabicAndEnglish` |
| Every Go and SQL file opens with Tech Details' copyright notice, word for word; a file a tool writes and marks so is the tool's (#143 R1, R6) | `.idea/copyright/` for new files | `TestR6_EveryGoAndSqlFileOpensWithTheNoticeWordForWord` |
