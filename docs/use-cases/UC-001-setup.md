[Documentation](../README.md) › UC-001

# UC-001: Set up the environment and base code

**What it is for:** the base every later use case builds on: a running API on SQL Server, a health check, Swagger outside production, and the words the app shows.
**Who uses it:** everyone, before sign-in. Nothing here needs a permission.
**Issue:** redix-api #20

## Endpoints

| Endpoint | Purpose | Sign-in |
| --- | --- | --- |
| `GET /health` | Answers `Healthy` when the API runs and reaches the database | No |
| `GET /translations/{language}` | Every word of `ar` or `en` in one bundle, with an ETag; `304` while unchanged, `404` for another language | No: the sign-in screen needs its words |
| `GET /swagger`, `GET /openapi/v1.json` | The endpoint reference | No; not served in Production |

## Tables

| Table | Holds |
| --- | --- |
| `TranslationKeys` | One row per word, named by its key, such as `app.title` |
| `Translations` | The word's value in each language, unique per key and language |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| Money is `decimal(19,4)` (BR-13) | `RedixDbContext.ConfigureConventions` | Enforced by the model; checked in the migration |
| Every table has a required rowversion | `RedixDbContext.OnModelCreating` | Checked in the migration |
| `/health` checks the database | `Program.cs`, `AddDbContextCheck` | `HealthTests.HealthAnswersAgainstMigratedDatabase` |
| A bundle holds every word of its language | `TranslationsEndpoint.GetBundle` | `TranslationsTests.BundleHoldsEveryWordOfTheLanguage` |
| An unchanged bundle answers `304` | `TranslationsEndpoint.GetBundle` | `TranslationsTests.UnchangedBundleAnswersNotModified` |
| Another language answers `404` | `TranslationsEndpoint.GetBundle` | `TranslationsTests.UnknownLanguageIsNotFound` |
| Swagger only outside Production | `Program.cs` | `SwaggerTests.SwaggerIsServedOutsideProductionOnly` |
