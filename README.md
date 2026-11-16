# REDIX API

The server of REDIX (Real Estate Development Intelligence): ASP.NET Core on .NET 10 with SQL Server.
What it does, use case by use case, is in this repo's issues; the database follows the pinned **Database design** issue.
How it works is in the [documentation](docs/README.md).

## Run it locally

Needs the .NET 10 SDK, SQL Server 2025 (Developer edition, Windows authentication) and the EF Core tools (`dotnet tool install --global dotnet-ef`).

```
dotnet ef database update --project src/Redix.Api -- --environment Dev
dotnet run --project src/Redix.Api --launch-profile dev
```

The launch profile sets `ASPNETCORE_ENVIRONMENT`. `dotnet run --environment Dev` does not choose the environment on .NET 10: the API starts as Production, on qa's port. From Git Bash, first run `export MSYS2_ENV_CONV_EXCL=REDIX_`: the sign-in key begins with `/`, which Git Bash otherwise rewrites as a Windows path before the API reads it, and every request answers 500. PowerShell, which the `.night` scripts use, needs nothing.

Each environment is a name, and the name alone picks the port, the database and the admin, from `appsettings.{name}.json` (redix-api #118):

| Environment | Start it | Address | Database |
| --- | --- | --- | --- |
| dev, automated runs | `--launch-profile dev` | http://127.0.0.1:5010 | `RedixDev` |
| qa, Muteb's own testing | `--launch-profile qa`, or `dotnet Redix.Api.dll --environment QA` from its own copy of the build | http://127.0.0.1:5012 | `RedixQA` |
| Production | Added when the server is published | | |

A migration reaches an environment's database with `dotnet ef database update --project src/Redix.Api -- --environment QA`. Its secrets are Windows user variables prefixed `REDIX_{name}_`, such as `REDIX_QA_Admin__Password`. `GET /health` answers `Healthy` when the database is reachable.

Swagger UI is at http://127.0.0.1:5010/swagger and the OpenAPI document at `/openapi/v1.json`, in every environment except Production, where both answer 404.

## Words

Every word the app shows is a row in `TranslationKeys` (the key, such as `app.title`) and `Translations` (its value in `ar` and `en`). `GET /translations/{language}` returns all the words of a language in one bundle with an `ETag`; a request carrying that tag in `If-None-Match` gets `304` while nothing changed. It answers without sign-in, because the sign-in screen needs its words.

New words are seeded in `Translations/TranslationSeed.cs` and reach the database with a migration; the app generates its typed getters from the keys with its `tool/sync_translations.dart`. The app's code never holds a word's value.

## Test

```
dotnet test
```

The tests migrate the database named in the `Redix` connection string, so SQL Server must be running. CI runs the same tests on every push against SQL Server in a container.
