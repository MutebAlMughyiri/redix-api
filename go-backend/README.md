# REDIX backend on Go and SQLite

The REDIX API on one small Linux server: Go, SQLite, and the same contract the app already speaks - every path, answer, status code and error
of the .NET API, at its old paths and again under `/api/v1` (redix-api #140).

## What it is

| Part | What |
| --- | --- |
| `cmd/server` | The API: reads its settings, opens the database, brings its schema up to date, serves the routes and the web front end's files |
| `internal/<domain>` | One folder per domain - `projects`, `structure`, `entries`, `accounts`... - each with its routes, its rules and its `queries.sql` |
| `internal/db` | The SQLite file, its two pools (one writer, four readers), the migrations, and the one way to write (`InTx`) |
| `internal/db/store` | The code sqlc generates from every domain's queries, each prepared once as the server starts |
| `internal/changes` | What every change inside a project shares: one transaction, the refusal it rolls back with, a History line for every row |
| `internal/statement` | The statement workbook every report file shares, written with excelize, and the report PDFs' pages, laid out by a Typst template with the Arabic font embedded |
| `internal/platform` | What the domains share: the router, problem answers, tokens and passwords, money and moments as .NET wrote them |
| `internal/replay` | .NET's recorded tests replayed against Go, every answer compared (`testdata/golden`) |
| `cmd/migrate-data` | The one-time copy of the data from SQL Server, the day of the switch (P3, P6) |
| `deploy/` | Caddy, the systemd units, Litestream |

## Settings

Environment variables, named as the .NET API read them, `REDIX_` first; an environment's own, `REDIX_{Environment}_`, wins.

| Variable | What | Default |
| --- | --- | --- |
| `REDIX_Environment` | Which environment's settings: `Dev` takes port 5010, the admin `dev-admin`, no asking limits and `RedixDev.db`; `QA` port 5012, the admin `real-admin` and `RedixQA.db`, as their `appsettings` files named; `Prod`, the published server, port 5012 behind Caddy and its data under `/var/lib/redix`, its secrets in `deploy/prod.env.example`; a variable set wins | `Production` |
| `REDIX_Urls` | Where the API listens; behind Caddy, `http://127.0.0.1:5012` | `http://0.0.0.0:5012` |
| `REDIX_Database__Path` | The SQLite file | `Redix/redix.db` in local application data |
| `REDIX_Attachments__Folder` | The uploaded files | `Redix/Attachments` |
| `REDIX_Web__Folder` | The web front end's files; empty for none | |
| `REDIX_Jwt__SigningKey` | The tokens' key, base64 | required |
| `REDIX_Admin__Username`, `REDIX_Admin__Password` | The first user, made on an empty database | |
| `REDIX_Email__ConnectionString`, `REDIX_Email__From` | The email sender; empty writes emails to `REDIX_Email__Folder` | |
| `REDIX_Registration__PermitsPerHour`, `REDIX_EmailCodes__PermitsPerHour` | The two limits | 5 |
| `REDIX_Typst__Path` | The typst binary that makes the report PDFs; the tests read it too | `typst` on the path |

## Working on it

| Command | Does |
| --- | --- |
| `make test` | Every test, the replay of .NET's answers among them (about six minutes) |
| `make sqlc` | Generates the store, with two guards: query files stay ASCII, and no `sqlc.arg` is left unreplaced (G15) |
| `make build` | The Linux binary, `bin/redix-server` |
| `make migrate` | Brings the database up to date and stops |
| `make golden` | Records .NET's answers again |
| `bin/migrate-data -from "sqlserver://localhost?database=RedixQA&TrustServerCertificate=true" -to redix.db` | Copies every row from SQL Server into a new SQLite file, once, in one transaction; it keeps nothing unless every table's count and the figures fingerprint (each project's entries by kind, status and account) are equal on both sides (R21, R34). Windows sign-in when the address names no user |

Every row a change adds, changes or removes has its History line; a test that writes through the API runs under the History net
(`internal/history/historytest`), which fails on any row changed with no line.

## Deploying (P5)

On a 1 GB Linux server:

1. Copy `bin/redix-server` to `/usr/local/bin/`, and make a `redix` user. Put Typst 0.15's binary at `/usr/local/bin/typst` for the PDFs.
2. Copy `deploy/prod.env.example` to `/etc/redix/redix.env` (mode 600) and fill in its secrets; `REDIX_Environment=Prod` names the rest - port 5012 behind
   Caddy, the database and files under `/var/lib/redix`.
3. Install `deploy/redix.service` into `/etc/systemd/system/`, then `systemctl enable --now redix`. It migrates before it starts.
4. Install Caddy with `deploy/Caddyfile` and `REDIX_DOMAIN` set; Caddy keeps the certificate.
5. Install Litestream with `deploy/litestream.yml` as `/etc/litestream.yml`, the bucket and keys in its environment, and enable it.
6. Install rclone with a remote named `backup` in `/etc/redix/rclone.conf`, then `deploy/redix-attachments.service` and its timer.

To restore: `litestream restore -o /var/lib/redix/redix.db /var/lib/redix/redix.db`, then `rclone copy backup:<bucket>/attachments
/var/lib/redix/attachments`, then start the service.

## Where it stands (16 Nov 2026)

Every one of the API's 147 routes is on Go, and 758 of .NET's recorded tests replay against them, every answer matching - each workbook cell by
cell, its values and formulas. The one test that waits checks .NET's Swagger page, which Go does not serve. Then P3, the data moved from SQL
Server; P4, the phone's Patrol tests against Go; P5, the rehearsal on a 1 GB server; and P6, the switch.
