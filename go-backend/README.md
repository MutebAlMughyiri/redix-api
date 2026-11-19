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
| `internal/replay` | The .NET API's recorded tests replayed against Go, every answer compared (`testdata/golden`); kept as they were recorded since .NET's removal |
| `deploy/` | The server setup script, the systemd units, Litestream |

## Settings

Environment variables, named as the .NET API read them, `REDIX_` first; an environment's own, `REDIX_{Environment}_`, wins.

| Variable | What | Default |
| --- | --- | --- |
| `REDIX_Environment` | Which environment's settings: `Dev` takes port 5010, the admin `dev-admin`, no asking limits and `RedixDev.db`; `QA` port 5012, the admin `real-admin` and `RedixQA.db`, as their `appsettings` files named; `Prod`, the published server, HTTPS on its domain on 8443 and 80 (443 is the web front end's) and its data under `/var/lib/redix`, its secrets in `deploy/prod.env.example`; a variable set wins | `Production` |
| `REDIX_Urls` | Where the API listens: with no domain, plain HTTP on the first; with one, HTTPS on the first and, for production, HTTP on the second, which only proves the names and sends everyone to HTTPS; qa and dev on the server name no second (#145 D-R25) | `http://0.0.0.0:5012`; Prod: `https://:8443;http://:80`, set in `/etc/redix/prod.env`, where `setup.sh` reads the firewall's ports too |
| `REDIX_Domain` | The name the API answers HTTPS on, with the certificate Let's Encrypt gives it; empty for plain HTTP (#145) | |
| `REDIX_Certificates__Folder` | Where the domain's certificate is kept | `Redix/Certificates`; Prod: `/var/lib/redix/certificates` |
| `REDIX_Certificates__Proves` | The names of the server's other instances, `;` between them, that production's port 80 proves to Let's Encrypt for them (#145 D-R25) | Prod: qa's and dev's names, in `/etc/redix/prod.env` |
| `REDIX_Database__Path` | The SQLite file | `Redix/redix.db` in local application data |
| `REDIX_Attachments__Folder` | The uploaded files | `Redix/Attachments` |
| `REDIX_Incidents__Path` | The failed requests, kept 90 days (redix-api #150) | `incidents.db` beside the database |
| `REDIX_Web__Folder` | The web front end's files; empty for none | |
| `REDIX_Jwt__SigningKey` | The tokens' key, base64 | required |
| `REDIX_Admin__Username`, `REDIX_Admin__Password` | The first user, made on an empty database | |
| `REDIX_Email__Smtp`, `REDIX_Email__From` | The mail server every email is handed to, as host:port, and the address it comes from; Prod hands it to Postfix on the same machine (#127, #145). With none, each email is written to `REDIX_Email__Folder` | Prod: `127.0.0.1:25` |
| `REDIX_Registration__PermitsPerHour`, `REDIX_EmailCodes__PermitsPerHour` | The two limits | 5 |
| `REDIX_Typst__Path` | The typst binary that makes the report PDFs; the tests read it too | `typst` on the path |

## Working on it

| Command | Does |
| --- | --- |
| `make test` | Every test, the replay of .NET's answers among them (about seven minutes) |
| `make cover` | The same, failing below the coverage floor (redix-api #141) |
| `go generate ./internal/apidoc` | Writes the OpenAPI document from the code (`cmd/openapi`), after any change to a route, what it reads, answers or refuses; `make test` fails until it is rewritten. Dev and QA serve it at `/openapi/v1.json`, with Swagger UI at `/swagger` (#142) |
| `make sqlc` | Generates the store, with two guards: query files stay ASCII, and no `sqlc.arg` is left unreplaced (G15) |
| `make build` | The Linux binary, `bin/redix-server` |
| `make migrate` | Brings the database up to date and stops |

Every row a change adds, changes or removes has its History line; a test that writes through the API runs under the History net
(`internal/history/historytest`), which fails on any row changed with no line.

## Deploying (redix-api #145)

`deploy/setup.sh` prepares a fresh Ubuntu 24.04 server, or brings one up to date, and is the record of every step: at a new company, run it
on the new server. It installs only what REDIX runs: the API, which holds its own HTTPS certificate, Typst, Litestream and rclone for the
backups, Postfix and OpenDKIM for the email. Running it twice changes nothing the second time.

The server runs three instances of the API, each a `redix@` service with its own settings file, name, port, folder and backup path (R25):

| Instance | Name | Port | Settings | Data |
| --- | --- | --- | --- | --- |
| prod | the domain | 8443, and 80, which proves the three names to Let's Encrypt | `/etc/redix/prod.env` | `/var/lib/redix/prod` |
| qa | qa.&lt;domain&gt; | 8444 | `/etc/redix/qa.env` | `/var/lib/redix/qa` |
| dev | dev.&lt;domain&gt; | 8445 | `/etc/redix/dev.env` | `/var/lib/redix/dev` |

Only one process can hold port 80, so qa and dev keep their certificates in `/var/lib/redix/certificates` with production's, where
production's port 80 reads the token Let's Encrypt asks for (D-R25).

Before the first run:

1. The three names' A records point at the server, and the server's reverse DNS names the domain (the company's panel).
   Mail to REDIX is received by Cloudflare's Email Routing, free, and forwarded to the owner's inbox (redix-api #147): in Cloudflare, Email ›
   Email Routing › Settings › Subdomains adds the domain; Destination addresses holds the owner's inbox, confirmed by its link; Routing
   rules send info@ and no-reply@ to it. The domain keeps one SPF record, the value the script prints, naming this server and Cloudflare.
2. A bucket, S3-compatible, with a key pair.
3. On the PC, outside the repo: filled copies of `deploy/prod.env.example` named `prod.env`, of `deploy/qa.env.example` named `qa.env`
   and again, for dev, `dev.env`, and of `deploy/backup.env.example` named `backup.env`.
4. The PC's key on the server, put there with the company's root password, once; then change that password in the panel:

       ssh-keygen -t ed25519 -f ~/.ssh/redix
       ssh root@<ip> "mkdir -p ~/.ssh /opt/redix-setup && cat >> ~/.ssh/authorized_keys" < ~/.ssh/redix.pub

Every run, the first or a new version, from go-backend in Git Bash (`make build` is the first line, where make is installed):

    CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -trimpath -ldflags='-s -w' -o bin/redix-server ./cmd/server
    scp -i ~/.ssh/redix -r deploy bin/redix-server <prod.env> <qa.env> <dev.env> <backup.env> root@<ip>:/opt/redix-setup/
    ssh -i ~/.ssh/redix root@<ip> bash /opt/redix-setup/deploy/setup.sh

On a new server it restores each instance's database and attachments from the bucket before it first starts. It ends by listing the
services, each instance's certificate and whether port 25 is open, and prints the DNS records the names and the mail need. It sends no
request to the API.

## Where it stands (17 Nov 2026)

| Phase | State |
| --- | --- |
| P2, the routes | All 147 on Go. 852 of .NET's recorded tests replay against them, every answer matching - each workbook cell by cell, each write a test made straight into the database repeated in SQLite. 39 are ported by hand: SQL Server's own migrations and lookup tables, tests reading its database or racing it, one opening a link .NET's own key signed. One waits: .NET's Swagger page, which Go will generate itself (#142) |
| P3, the data | `migrate-data` proven on a copy of RedixQA: 59 tables equal, the figures equal, and Go signed in qa's admin and read the project and both issued reports |
| P4, the phone | The Patrol tests run against Go as Dev (`REDIX_Environment=Dev`) |
| P5, the rehearsal | Waits for the cloud server |
| P6, the switch | The owner's |
