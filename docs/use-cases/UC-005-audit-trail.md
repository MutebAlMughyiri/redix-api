[Documentation](../README.md) › UC-005

# UC-005: Keep the audit trail

**What it is for:** knowing who did what to every record, and when, with the values before and after (FR-21, AC-21.1).
**Who uses it:** nobody directly: the API writes it on its own. Reading it comes with the screens that show a record's history.
**Issue:** redix-api #23; no app side.

## The table

`AuditTrail`, one row per action, never changed or deleted:

| Column | Holds |
| --- | --- |
| `Id` | The record's number, in the order records were written (`bigint` identity: an append-only log) |
| `At` | When it was done |
| `UserId` | Who did it |
| `Action` | What was done, by name (`AuditAction`) |
| `TableName`, `RecordId` | The record it was done to |
| `Before`, `After` | The values it changed, as JSON objects of column names and values; every value of a new record in `After`, with `{}` in `Before` |

## Recorded today

Each use case adds its own actions; entries, users and attachments join the trail as their use cases are built.

| Action | When | Record | Values |
| --- | --- | --- | --- |
| `SignIn` | A user signs in (UC-002) | The user | What the sign-in changed, such as the count of wrong passwords cleared |
| `ChangePassword` | A user changes their password (UC-003) | The user | `MustChangePassword`, when a temporary password ends; never the password's hash |
| `SignOut` | A user signs out of a device (UC-062) | The user | None: the record itself says it happened |
| `Create`, `Update`, `ResetPassword`, `Disable`, `Enable` | An owner manages a user (UC-007) | The user | What the action changed; never the password's hash |

## How a use case records an action

After making its change and before `SaveChanges`, the endpoint calls `db.Audit(record, action, userId, at)`. The record is built from EF Core's change tracker, so the values come from the change itself, and it is saved by the same `SaveChanges`, so the change and its record are kept or lost together. A property marked `[NotAudited]` never enters the trail.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A sign-in is recorded with the user and the time | `AuthEndpoints.SignIn` | `AuthTests.ASignInIsInTheAuditTrailWithTheUserAndTheTime` |
| A password change is recorded with the values before and after, never the password's hash | `AuthEndpoints.ChangePassword`, `User.PasswordHash` (`[NotAudited]`) | `ChangePasswordTests.APasswordChangeIsInTheAuditTrailWithTheValuesBeforeAndAfterButNeverThePasswordHash` |
| A record is built from the values the save changes | `AuditRecord.Of` | Both tests above |
| No record can be changed or deleted, through the API or any query (AC-21.1) | The database trigger `AuditTrailIsAppendOnly`, created by the `AuditTrail` migration | `AuditTrailTests.NoRecordOfTheTrailCanBeChangedOrDeletedByAnyone` |

## Left to later use cases

Creating, editing, submitting, approving, rejecting and reversing entries (UC-022 on) and attaching invoices (UC-039) each record their actions when built. Reading the trail comes with the first screen that shows a record's history.
