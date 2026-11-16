# REDIX's Go backend (redix-api #140, R22). Run from go-backend/; sqlc and Go must be on the path.

.PHONY: build test sqlc migrate golden

# A Linux binary for the server, with no C toolchain: modernc's SQLite is pure Go; and the one-time data move, for the machine SQL Server runs on.
build:
	CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -trimpath -ldflags="-s -w" -o bin/redix-server ./cmd/server
	go build -trimpath -o bin/ ./cmd/migrate-data

# Every test, the replay of .NET's recorded answers among them.
test:
	go test ./... -timeout 30m

# The store from every domain's queries, then two guards: sqlc's SQLite engine loses its place after a non-ASCII character in a query file,
# and leaves sqlc.arg unreplaced in an ORDER BY (G15); either would compile and fail at run time.
sqlc:
	sqlc generate
	@! grep -nP '[^\x00-\x7F]' internal/*/queries.sql || (echo 'a query file holds a non-ASCII character' && false)
	@! grep -rn 'sqlc.arg' internal/db/store || (echo 'sqlc left sqlc.arg in a query (G15)' && false)

# Brings the database named by REDIX_Database__Path up to date, and stops.
migrate:
	go run ./cmd/server migrate

# Records .NET's answers again (the recorder lives in the .NET tests only, R23) into an empty folder, since the recorder adds to what it finds, then
# compresses each collection into testdata/golden.
golden:
	rm -rf .golden && mkdir .golden
	cd .. && REDIX_RECORD=$(CURDIR)/.golden dotnet test tests/Redix.Api.Tests
	for f in .golden/*.jsonl; do gzip -c "$$f" > "testdata/golden/$$(basename "$$f").gz"; done
