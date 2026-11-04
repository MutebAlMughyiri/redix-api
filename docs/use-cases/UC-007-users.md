[Documentation](../README.md) › UC-007

# UC-007: Manage users

**What it is for:** correcting, disabling and resetting logins (FR-02).
**Who uses it:** the system admin, for any login; a project manager, for someone only in projects they manage (BR-29, BR-33). A new login is created inside a project: see UC-009.
**Issue:** redix-api #25; the app side is redix-app #27.

## Endpoints

A login is shared across projects, so changing one needs `ManageLogins`, or `ManageUsers` in every project it is in. Otherwise it answers `403` `otherProject`, because a reset could let one project's manager into another.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /users` | Every login, active users first, by name, with their projects and roles; needs `ManageLogins` | `200` the users |
| `POST /users/{userId}` | Changes a user's name and mobile | `200` the user; `400` `invalidName` or `invalidMobile`; `404` |
| `POST /users/{userId}/reset-password` | Sets a new temporary password, ends the lock, and signs the user out everywhere | `200` the user; `400` `weakPassword`; `404` |
| `POST /users/{userId}/disable` | Disables a user, whose session ends at the next request | `200` the user; `409` `lastManager`; `404` |
| `POST /users/{userId}/enable` | Lets a disabled user sign in again | `200` the user; `404` |

A user answers as `{ id, username, displayName, mobile, isActive, mustChangePassword, projects }`, never with the password's hash.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A new user can sign in and must change the temporary password (AC-02.1) | `ProjectUsersEndpoints.Create` | `ProjectUsersTests.AManagerCreatesALoginInTheProjectWhoStartsAsTheLastRoleAndMustChangeThePassword` |
| A username is unique, whatever its case | The unique index, `ProjectUsersEndpoints.Create` | `ProjectUsersTests.ARepeatedUsernameIsRefusedWhateverItsCase` |
| Each field has its limit | `User.RefusalOfNew`, `User.RefusalOfDetails` | `ProjectUsersTests.AFieldOutsideItsLimitIsRefused` |
| Editing changes the name and mobile, and nothing else | `UsersEndpoints.Edit` | `UsersTests.EditingChangesTheNameAndMobile` |
| A reset forces a change, ends the lock and signs out every device | `User.ResetPassword`, `UsersEndpoints.ResetPassword` | `UsersTests.AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice` |
| A disabled user cannot sign in, and their open session ends at the next request (AC-01.3) | `SessionPolicies.CheckSessionIsOpen` | `UsersTests.DisablingAUserEndsTheOpenSessionAtTheNextRequest` |
| An enabled user can sign in again | `UsersEndpoints.Enable` | `UsersTests.AnEnabledUserCanSignInAgain` |
| A project's last active manager cannot be disabled, by anyone (BR-29) | `ProjectManagers.IsTheLastOfAProjectAsync` | `ProjectUsersTests.TheLastManagerCannotBeDisabled` |
| A login is changed only by the system admin, or by someone who manages every project it is in (BR-33) | `UsersEndpoints.MayChangeAsync` | `UsersTests.SomeoneWithNoRoleOverALoginCannotChangeIt`, `ProjectUsersTests.AManagerCannotResetSomeoneInAnotherProject`, `SomeoneWhoManagesBothProjectsIsNotHeldBack` |
| The system admin resets any login, the way back for a project's only manager | `Permission.ManageLogins` | `UsersTests.TheSystemAdminResetsAnyLogin` |
| Every change is in the audit trail, never the password's hash | `db.Audit`, `[NotAudited]` | `UsersTests.EveryChangeIsInTheAuditTrailWithNoPasswordHash` |
| The answer never carries the password's hash | `UserView` | `UsersTests.AnAnswerNeverCarriesThePasswordHash` |

## Left to later use cases

A cash custody created with the user (UC-015).
