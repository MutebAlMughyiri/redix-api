[Documentation](../README.md) › UC-070

# UC-070: Configurable values, and how long the app waits

**What it is for:** keeping values that change without a release in one table, starting with how long the app waits for an answer before it says there is no connection.
**Who uses it:** the app, at start, before anyone signs in. The values are changed in the database.
**Issue:** redix-api #94 "UC-070: Request timeouts from a config table".

## How it works

`ConfigValues` holds one row per value: a `Key` and its `Value` as text. `GET /config` answers every row by its key, to anyone, so a value here is never a secret: secrets stay in environment variables.

| Key | Starts at | Means |
| --- | --- | --- |
| `requestTimeoutSeconds` | `10` | How long the app waits for any answer before it says there is no connection and offers to try again |
| `fileTimeoutSeconds` | `120` | How long it waits for a file sent or read: an invoice, a document, a report's PDF, workbook or ZIP |

Muteb, 7 November 2026: "add time out 10 second for all request. add config table in db to set the time out. later we will use this config table to add any configurable value"; files wait longer, since a 25 MB document or a report built on the server cannot answer in 10 seconds on a phone's network.

```sql
-- Wait 15 seconds from each app's next start.
UPDATE ConfigValues SET Value = '15' WHERE [Key] = 'requestTimeoutSeconds';
```

## Rules and where they are proven

| Rule | Code | Test |
| --- | --- | --- |
| The app reads the values before anyone signs in | `ConfigEndpoints.Read` | `ConfigTests.UC_070_TheAppReadsHowLongToWaitBeforeAnyoneSignsIn` |
| A value changed in the database is read at the next request | `ConfigEndpoints.Read` | `ConfigTests.UC_070_AValueChangedInTheDatabaseIsReadAtTheNextRequest` |
