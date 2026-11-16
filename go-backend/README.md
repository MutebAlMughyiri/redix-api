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
| `internal/platform` | What the domains share: the router, problem answers, tokens and passwords, money and moments as .NET wrote them |
| `internal/replay` | .NET's recorded tests replayed against Go, every answer compared (`testdata/golden`) |
| `deploy/` | Caddy, the systemd units, Litestream |

## Settings

Environment variables, named as the .NET API read them, `REDIX_` first; an environment's own, `REDIX_{Environment}_`, wins.

| Variable | What | Default |
| --- | --- | --- |
| `REDIX_Urls` | Where the API listens; behind Caddy, `http://127.0.0.1:5012` | `http://0.0.0.0:5012` |
| `REDIX_Database__Path` | The SQLite file | `Redix/redix.db` in local application data |
| `REDIX_Attachments__Folder` | The uploaded files | `Redix/Attachments` |
| `REDIX_Web__Folder` | The web front end's files; empty for none | |
| `REDIX_Jwt__SigningKey` | The tokens' key, base64 | required |
| `REDIX_Admin__Username`, `REDIX_Admin__Password` | The first user, made on an empty database | |
| `REDIX_Email__ConnectionString`, `REDIX_Email__From` | The email sender; empty writes emails to `REDIX_Email__Folder` | |
| `REDIX_Registration__PermitsPerHour`, `REDIX_EmailCodes__PermitsPerHour` | The two limits | 5 |

## Working on it

| Command | Does |
| --- | --- |
| `make test` | Every test, the replay of .NET's answers among them (about six minutes) |
| `make sqlc` | Generates the store, with two guards: query files stay ASCII, and no `sqlc.arg` is left unreplaced (G15) |
| `make build` | The Linux binary, `bin/redix-server` |
| `make migrate` | Brings the database up to date and stops |
| `make golden` | Records .NET's answers again |

Every row a change adds, changes or removes has its History line; a test that writes through the API runs under the History net
(`internal/history/historytest`), which fails on any row changed with no line.

## Deploying (P5)

On a 1 GB Linux server:

1. Copy `bin/redix-server` to `/usr/local/bin/`, and make a `redix` user.
2. Write `/etc/redix/redix.env` with the settings above, `REDIX_Urls=http://127.0.0.1:5012`, the database and attachments in `/var/lib/redix`.
3. Install `deploy/redix.service` into `/etc/systemd/system/`, then `systemctl enable --now redix`. It migrates before it starts.
4. Install Caddy with `deploy/Caddyfile` and `REDIX_DOMAIN` set; Caddy keeps the certificate.
5. Install Litestream with `deploy/litestream.yml` as `/etc/litestream.yml`, the bucket and keys in its environment, and enable it.
6. Install rclone with a remote named `backup` in `/etc/redix/rclone.conf`, then `deploy/redix-attachments.service` and its timer.

To restore: `litestream restore -o /var/lib/redix/redix.db /var/lib/redix/redix.db`, then `rclone copy backup:<bucket>/attachments
/var/lib/redix/attachments`, then start the service.

## Where it stands (16 Nov 2026)

136 of the API's 147 routes are on Go, and 727 of .NET's recorded tests replay against them, every answer matching. Still on .NET: the PDF
and workbook files of the periodic, item and contract reports, the VAT statement, the project file and file links. Then P3, the data
moved from SQL Server; P4, the phone's Patrol tests against Go; and P6, the switch.
