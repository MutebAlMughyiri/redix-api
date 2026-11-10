[Documentation](../README.md) › UC-007

# UC-007: Manage users

**What it is for:** correcting, disabling and resetting logins (FR-02). A forgotten password with nobody to reset it, such as a project's only manager or the admin, takes a database fix for now.
**Who uses it:** a project manager, for someone only in projects they manage (BR-29, BR-33). A new login is created inside a project: see UC-008.
**Issue:** redix-api #25; the app side is redix-app #27.

## Endpoints

A login is shared across projects, so changing one needs `ManageUsers` in every project it is in, and a login holding a system permission can never be changed here. Otherwise it answers `403` `otherProject`, because a reset could let a manager sign in to another project or as the admin.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /users/{userId}` | Changes a user's name and mobile | `200` the user; `400` `invalidName` or `invalidMobile`; `404` |
| `POST /users/{userId}/reset-password` | Sets a new temporary password, ends the lock, and signs the user out everywhere | `200` the user; `400` `weakPassword`; `404` |
| `POST /users/{userId}/disable` | Disables a user, whose session ends at the next request | `200` the user; `409` `lastManager`; `404` |
| `POST /users/{userId}/enable` | Lets a disabled user sign in again | `200` the user; `404` |

A user answers as `{ id, username, displayName, mobile, isActive, mustChangePassword, projects }`, never with the password's hash.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A new user can sign in and must change the temporary password (AC-02.1) | `ProjectUsersEndpoints.Create` | `ProjectUsersTests.AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword` |
| A username is unique, whatever its case | The unique index, `ProjectUsersEndpoints.Create` | `ProjectUsersTests.ARepeatedUsernameIsRefusedWhateverItsCase` |
| Each field has its limit | `User.RefusalOfNew`, `User.RefusalOfDetails` | `ProjectUsersTests.AFieldOutsideItsLimitIsRefused` |
| Editing changes the name and mobile, and nothing else | `UsersEndpoints.Edit` | `UsersTests.EditingChangesTheNameAndMobile` |
| A mobile is ASCII digits with a leading `+` at most, or empty, for a user or a supplier, so the browser front end cannot store `????`; one owner | `User.IsAcceptableMobile` | `ProjectUsersTests.AMobileIsAsciiDigitsWithALeadingPlusAtMost`, `ProjectUsersTests.AMobileMayStartWithAPlus`, `UsersTests.AnEditCannotGiveAMobileThatIsNotDigits`, `RegistrationTests.ABadFieldCreatesNothing`, `SuppliersTests.ASuppliersMobileIsDigitsOnAddAndOnEdit` |
| A reset forces a change, ends the lock and signs out every device | `User.ResetPassword`, `UsersEndpoints.ResetPassword` | `UsersTests.AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice` |
| A disabled user cannot sign in, and their open session ends at the next request (AC-01.3) | `SessionPolicies.CheckSessionIsOpen` | `UsersTests.DisablingAUserEndsTheOpenSessionAtTheNextRequest` |
| An enabled user can sign in again | `UsersEndpoints.Enable` | `UsersTests.AnEnabledUserCanSignInAgain` |
| A project's last active manager cannot be disabled, by anyone (BR-29) | `ProjectManagers.IsTheLastOfAProjectAsync` | `ProjectUsersTests.TheLastManagerCannotBeDisabled` |
| A login is changed only by someone who manages every project it is in, and never the admin's (BR-33) | `UsersEndpoints.MayChangeAsync` | `UsersTests.SomeoneWithNoRoleOverALoginCannotChangeIt`, `ProjectUsersTests.AManagerCannotResetSomeoneInAnotherProject`, `AManagerCannotTakeOverTheAdmin` |
| Every change is in the History table, never the password's hash | `db.AddHistory`, `[NotInHistory]` | `UsersTests.EveryChangeIsInTheHistoryWithNoPasswordHash` |
| The answer never carries the password's hash | `UserView` | `UsersTests.AnAnswerNeverCarriesThePasswordHash` |

## Left to later use cases

A cash custody created with the user (UC-015).
