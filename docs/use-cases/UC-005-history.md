[Documentation](../README.md) › UC-005

# UC-005: Keep the history

**What it is for:** knowing who did what to every record, and when, with the values before and after (FR-21, AC-21.1).
**Who uses it:** nobody directly: the API writes it on its own. «سجل المشروع» and «سجلي» read it (UC-069), and so do the accounts', partners' and approvals' histories.
**Issue:** redix-api #23; no app side.

## The table

`History`, one row per action, never changed or deleted:

| Column | Holds |
| --- | --- |
| `Id` | The record's number, in the order records were written (`bigint` identity: an append-only log) |
| `At` | When it was done |
| `UserId` | Who did it |
| `Action` | What was done, by name (`history.Action`) |
| `table_name`, `record_id` | The record it was done to |
| `Before`, `After` | The values it changed, as JSON objects of column names and values; every value of a new record in `After`, with `{}` in `Before` |
| `trace_id` | The request that wrote it: its trace id (redix-api #150), the same on every line one request wrote, so they are read back as one action (UC-069); empty on the lines written before 18 Nov 2026 |

## Recorded today

Every change records its line: the History net (`historytest.Check`, redix-api #140) runs under every request the Go tests send, and fails a test whose write leaves a row without a line naming it. The table lists the actions on logins and on a project's start; what each table's lines are in a project's history is UC-069's table.

| Action | When | Record | Values |
| --- | --- | --- | --- |
| `SignIn` | A user signs in (UC-002) | The user | What the sign-in changed, such as the count of wrong passwords cleared |
| `SignInFailed` | A wrong password is typed for a user, at sign-in or at a password change, or any password while they are locked (AC-01.2) | The user whose name was typed; an unknown username records nothing | The count of wrong passwords; never what was typed |
| `AccountLocked` | A wrong password locks the account | The same | The count, and `LockedUntil`, old and new |
| `ChangePassword` | A user changes their password (UC-003) | The user | `MustChangePassword`, when a temporary password ends; never the password's hash |
| `SignOut` | A user signs out of a device (UC-062) | The user | None: the record itself says it happened |
| `Create`, `Update`, `ResetPassword` | A user is managed (UC-007, UC-008); `Disable` and `Enable` on a user are the login-wide switch before redix-api #126 | The user |
| `Disable`, `Enable` | A person is made inactive in a project, or active again (redix-api #126); `RemoveProject` when taking him out of it ends it | His row of `InactiveMembers`, with the project | What the action changed; never the password's hash |
| `AssignProject`, `RemoveProject` | A permission in a project is ticked or unticked (UC-008, UC-009) | The `ProjectPermissions` row | The permission; every value of a removed row in `Before` |
| `RegisterProject` | A project is registered (UC-006, UC-066) | The project | Every value of the new record |
| `Create` | A unit is added (UC-006) | The unit | Every value of the new record |
| `CloseProject` | A project is closed (UC-006) | The project | Its status, old and new |
| `Update` | A project is renamed (UC-006), a unit is renamed (UC-006), an item's type changes (UC-012), or an account is edited (UC-015) | The project, the unit, the item, or the account | The values changed, old and new |
| `Delete` | A unit nothing used is deleted (BR-38) | The unit | Every value of the removed record in `Before` |

## How a use case records an action

Inside the write that makes its change, the endpoint records it with `changes.Change.Record`, which calls `history.Write` on the same transaction: the values are the ones the change set, and the change and its record are kept or lost together. Only the values the endpoint passes enter the History table, so a password hash never does.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A sign-in is recorded with the user and the time | `auth.Sessions.SignIn` | `AuthTests.ASignInIsInTheHistoryWithTheUserAndTheTime` |
| A refused sign-in is recorded against the user whose name was typed, and never what was typed | `auth.saveAttempt` | `AuthTests.AWrongPasswordIsInTheHistoryAgainstTheUserAndNeverSaysWhatWasTyped`, `AuthTests.TryingWhileLockedIsAFailedSignIn`, `AuthTests.R126_R10_SomeoneInactiveInEveryProjectIsToldAWrongPasswordAsAnyoneIs` |
| The wrong password that locks the account is recorded as a lockout, at sign-in or at a password change | `auth.saveAttempt` | `AuthTests.TheWrongPasswordThatLocksTheAccountIsRecordedAsALockout`, `ChangePasswordTests.AWrongCurrentPasswordIsRefusedAndCountsTowardTheLockout` |
| An unknown username records nothing, because there is nobody to record it against | `auth.Sessions.SignIn` | `AuthTests.AnUnknownUsernameRecordsNothing` |
| A password change is recorded with the values before and after, never the password's hash | `auth.Sessions.ReplacePassword` | `ChangePasswordTests.APasswordChangeIsInTheHistoryWithTheValuesBeforeAndAfterButNeverThePasswordHash` |
| A record is built from the values the save changes | `accounts.custodianFields` | Both tests above |
| No record can be changed or deleted, through the API or any query (AC-21.1) | The triggers `history_is_append_only_update` and `history_is_append_only_delete`, in migration 00001 | `HistoryTests.NoRecordOfTheTrailCanBeChangedOrDeletedByAnyone` |

## Read by

«سجل المشروع» reads a project's lines, one per request, each for whoever may see its record; «سجلي» reads the lines of one's own login (UC-069).
