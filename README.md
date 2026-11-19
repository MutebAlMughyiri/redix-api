# REDIX API

The server of REDIX (Real Estate Development Intelligence): Go with SQLite, in [`go-backend/`](go-backend/README.md) (redix-api #140).
What it does, use case by use case, is in this repo's issues; the database follows the pinned **Database design** issue.
How it works is in the [documentation](docs/README.md).

## Run it locally

Needs Go (the version in `go-backend/go.mod`) and, for the report PDFs, Typst 0.15 (`REDIX_Typst__Path`, or `typst` on the path).

```
cd go-backend
REDIX_Environment=Dev go run ./cmd/server
```

From Git Bash, first run `export MSYS2_ENV_CONV_EXCL=REDIX_`: the sign-in key begins with `/`, which Git Bash otherwise rewrites as a Windows path before the API reads it. PowerShell needs nothing.

Each environment is a name, and the name alone picks the port, the database and the admin:

| Environment | Start it | Address | Database |
| --- | --- | --- | --- |
| dev, automated runs | `REDIX_Environment=Dev` | http://127.0.0.1:5010 | `RedixDev.db` in `%LOCALAPPDATA%\Redix` |
| qa, the owner's own testing | `REDIX_Environment=QA` | http://127.0.0.1:5012 | `RedixQA.db` in `%LOCALAPPDATA%\Redix` |
| Production | `REDIX_Environment=Prod`, by `deploy/setup.sh` | https://&lt;domain&gt;:8443 | `/var/lib/redix/prod/redix.db` |
| qa and dev for testers, on the same server (#145 R25) | `QA` and `Dev`, by `deploy/setup.sh` | https://qa.&lt;domain&gt;:8444, https://dev.&lt;domain&gt;:8445 | `/var/lib/redix/qa/redix.db`, `/var/lib/redix/dev/redix.db` |

Secrets are environment variables prefixed `REDIX_`, never a file in the repo; an environment's own, prefixed `REDIX_{name}_` such as `REDIX_QA_Admin__Password`, win. The schema is brought up to date as the server starts. `GET /health` answers `Healthy` when the database is reachable. A `.db` file opens in GoLand's Database window as a SQLite data source.

## Words

Every word the app shows is a row in `translation_keys` (the key, such as `app.title`) and `translations` (its value in `ar` and `en`). `GET /translations/{language}` returns all the words of a language in one bundle with an `ETag`; a request carrying that tag in `If-None-Match` gets `304` while nothing changed. It answers without sign-in, because the sign-in screen needs its words.

New words reach the database with a migration in `go-backend/internal/db/migrations`; the app generates its typed getters from the keys with its `tool/sync_translations.dart`. The app's code never holds a word's value.

## Test

```
cd go-backend
make test     # every test, the replay of the .NET API's recorded answers among them
make cover    # the same, failing below the coverage floor (redix-api #141)
```
