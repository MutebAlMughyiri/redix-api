# REDIX API

The server of REDIX (Real Estate Development Intelligence): ASP.NET Core on .NET 10 with SQL Server.
What it does, use case by use case, is in this repo's issues; the database follows the pinned **Database design** issue.
How it works is in the [documentation](docs/README.md).

## Run it locally

Needs the .NET 10 SDK, SQL Server 2025 (Developer edition, Windows authentication) and the EF Core tools (`dotnet tool install --global dotnet-ef`).

```
dotnet ef database update --project src/Redix.Api
dotnet run --project src/Redix.Api
```

The API listens on http://localhost:5010. `GET /health` answers `Healthy` when the database is reachable.

Swagger UI is at http://localhost:5010/swagger and the OpenAPI document at `/openapi/v1.json`, in every environment except Production, where both answer 404.

## Words

Every word the app shows is a row in `TranslationKeys` (the key, such as `app.title`) and `Translations` (its value in `ar` and `en`). `GET /translations/{language}` returns all the words of a language in one bundle with an `ETag`; a request carrying that tag in `If-None-Match` gets `304` while nothing changed. It answers without sign-in, because the sign-in screen needs its words.

New words are seeded in `Translations/TranslationSeed.cs` and reach the database with a migration; the app generates its typed getters from the keys with its `tool/sync_translations.dart`. The app's code never holds a word's value.

## Test

```
dotnet test
```

The tests migrate the database named in the `Redix` connection string, so SQL Server must be running. CI runs the same tests on every push against SQL Server in a container.
