[Documentation](../README.md) › UC-007

# UC-007: Manage users

**What it is for:** creating, correcting, disabling and resetting the users who work on the projects (FR-02).
**Who uses it:** owners, who manage the users (BR-29).
**Issue:** redix-api #25; the app side is redix-app #27.

## Endpoints

All need an owner's session, and answer `403` to anyone else.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /users` | Every user: owners first, then active users, by name | `200` the users |
| `POST /users` | Creates a user who must change the temporary password | `201` the user; `400` `invalidName`, `invalidUsername`, `invalidMobile` or `weakPassword`; `409` `usernameTaken` |
| `POST /users/{userId}` | Changes a user's name and mobile | `200` the user; `400` `invalidName` or `invalidMobile`; `404` |
| `POST /users/{userId}/reset-password` | Sets a new temporary password, ends the lock, and signs the user out everywhere | `200` the user; `400` `weakPassword`; `404` |
| `POST /users/{userId}/disable` | Disables a user, whose session ends at the next request | `200` the user; `409` `ownerCannotBeDisabled`; `404` |
| `POST /users/{userId}/enable` | Lets a disabled user sign in again | `200` the user; `404` |

A user answers as `{ id, username, displayName, mobile, isOwner, isActive, mustChangePassword }`, never with the password's hash.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A new user can sign in and must change the temporary password (AC-02.1) | `UsersEndpoints.Create` | `UsersTests.AnOwnerCreatesAUserWhoMustChangeThePassword` |
| A username is unique, whatever its case | The unique index, `UsersEndpoints.Create` | `UsersTests.ARepeatedUsernameIsRefusedWhateverItsCase` |
| Each field has its limit | `User.RefusalOfNew`, `User.RefusalOfDetails` | `UsersTests.AFieldOutsideItsLimitIsRefused` |
| Editing changes the name and mobile, and nothing else | `UsersEndpoints.Edit` | `UsersTests.EditingChangesTheNameAndMobile` |
| A reset forces a change, ends the lock and signs out every device | `User.ResetPassword`, `UsersEndpoints.ResetPassword` | `UsersTests.AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice` |
| A disabled user cannot sign in, and their open session ends at the next request (AC-01.3) | `SessionPolicies.CheckSessionIsOpen` | `UsersTests.DisablingAUserEndsTheOpenSessionAtTheNextRequest` |
| An enabled user can sign in again | `UsersEndpoints.Enable` | `UsersTests.AnEnabledUserCanSignInAgain` |
| An owner cannot be disabled, so at least one always stays (BR-29) | `User.Disable` | `UsersTests.AnOwnerCannotBeDisabled` |
| Only an owner manages users | `SessionPolicies.Owner` | `UsersTests.OnlyAnOwnerManagesUsers` |
| Every change is in the audit trail, never the password's hash | `db.Audit`, `[NotAudited]` | `UsersTests.EveryChangeIsInTheAuditTrailWithNoPasswordHash` |
| The answer never carries the password's hash | `UserView` | `UsersTests.AnAnswerNeverCarriesThePasswordHash` |

## Left to later use cases

A user's projects and role (UC-008, UC-009), and a cash custody created with the user (UC-015).
