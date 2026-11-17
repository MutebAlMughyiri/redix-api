[Documentation](../README.md) › UC-007

# UC-007: Manage users

**What it is for:** correcting and resetting logins (FR-02), and making a person inactive in one project and active again (redix-api #126). Anyone with an email, a project's only manager and the admin included, also resets a forgotten password himself from the sign-in screen with a code by email (UC-002, redix-api #127).
**Who uses it:** a project manager, for someone only in projects they manage (BR-29, BR-33); making someone inactive, whoever manages the users of that one project. A new login is created inside a project: see UC-008.
**Issue:** redix-api #25; the app side is redix-app #27.

## Endpoints

A login is shared across projects, so changing one needs `ManageUsers` in every project it is in, and a login holding a system permission can never be changed here. Otherwise it answers `403` `otherProject`, because a reset could let a manager sign in to another project or as the admin. Making someone inactive is per project, so it needs `ManageUsers` in that project alone, and is allowed for a shared login.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `PUT /users/{userId}` | Changes a user's name, mobile and email (redix-api #127); another email ends every code sent before it | `200` the user; `400` `invalidName`, `invalidMobile` or `emailInvalid`; `409` `emailTaken`; `404` |
| `POST /users/{userId}/reset-password` | Sets a new temporary password, ends the lock, and signs the user out everywhere | `200` the user; `400` `weakPassword`; `404` |
| `POST /projects/{projectId}/users/{userId}/disable` | Disable: he can no longer open this project; it shows gray on his project list with «حسابك غير مفعّل في هذا المشروع.», and if he is inside it, the list replaces it at his next action, when the app opens or comes back, or within 15 minutes. His permissions and entries stay. Inactive already changes nothing | `200` the user; `409` `lastManager`; `404` someone not in the project; `403` without `ManageUsers` in it |
| `POST /projects/{projectId}/users/{userId}/enable` | Enable: he opens it again, with everything as before. Active already changes nothing | `200` the user; `404` someone not in the project; `403` without `ManageUsers` in it |

A user answers as `{ id, username, displayName, mobile, email, mustChangePassword, projects }`, never with the password's hash; each of `projects` says `isInactive` for that project. The login-wide `/users/{userId}/disable` and `/enable` and `Users.IsActive` went with redix-api #126 (D3): someone inactive in every project still signs in, to his project list.

## Tables

| Table | Holds |
| --- | --- |
| `InactiveMembers` | One row per person per project he is inactive in (redix-api #126); no row is active |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A new user can sign in and must change the temporary password (AC-02.1) | `users.Routes.create` | `ProjectUsersTests.AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword` |
| A username is unique, whatever its case | The unique index, `users.Routes.create` | `ProjectUsersTests.ARepeatedUsernameIsRefusedWhateverItsCase` |
| Each field has its limit | `users.refusalOfNew`, `users.RefusalOfDetails` | `ProjectUsersTests.AFieldOutsideItsLimitIsRefused` |
| Editing changes the name and mobile, and nothing else | `users.Routes.edit` | `UsersTests.EditingChangesTheNameAndMobile` |
| The manager changes a person's email, or keeps it, to a well-formed one no other account holds (AC-07.8, R9) | `users.Routes.edit`, `users.RefusalOfEmail` | `UsersTests.AC_07_8_TheManagerChangesAPersonsEmailOrKeepsIt`, `AC_07_8_R9_TheManagerCannotGiveAnEmailAnotherAccountHoldsNorOneNotWellFormed` |
| Another email ends the person's codes, a reset code to the old address and a code to an address not yet confirmed; an edit keeping the email ends none (redix-api #127) | `users.ChangeEmail`, `auth.Codes.confirm` | `UsersTests.R127_TheManagerChangingAPersonsEmailEndsTheResetCodeSentToTheOldOne`, `R127_TheManagerChangingAPersonsEmailEndsTheirCodeToAnAddressNotYetConfirmed`, `R127_AnEditThatKeepsTheEmailLeavesItsResetCodeWorking` |
| A mobile is ASCII digits with a leading `+` at most, or empty, for a user or a supplier, so the browser front end cannot store `????`; one owner | `users.IsAcceptableMobile` | `ProjectUsersTests.AMobileIsAsciiDigitsWithALeadingPlusAtMost`, `ProjectUsersTests.AMobileMayStartWithAPlus`, `UsersTests.AnEditCannotGiveAMobileThatIsNotDigits`, `RegistrationTests.ABadFieldCreatesNothing`, `SuppliersTests.ASuppliersMobileIsDigitsOnAddAndOnEdit` |
| A reset forces a change, ends the lock and signs out every device | `users.ResetPassword`, the one reset a code by email uses too, `users.Routes.resetPassword` | `UsersTests.AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice` |
| A manager makes someone inactive in his project and active again, and he keeps what he holds there (redix-api #126, D1, R7) | `users.Routes.disable`, `Enable` | `InactiveMembersTests.R1_D1_R7_AManagerMakesSomeoneInactiveInHisProjectAndActiveAgainAndHeKeepsWhatHeHolds` |
| Every route of a project he is inactive in refuses him as for a project he is not in; his other projects work with the same session (R5) | `access.Book.Of` | `InactiveMembersTests.R5_EveryRouteOfAProjectHeIsInactiveInRefusesHimAsForAProjectHeIsNotIn`, `R5_InactiveInOneProjectHeWorksInAnotherWithTheSameSession` |
| His project list keeps a project he is inactive in, saying so and holding nothing (R2, D2) | `projects.Routes.list`, `projects.sight.view` | `InactiveMembersTests.R2_D2_HisProjectListShowsAProjectHeIsInactiveInSayingSoAndHoldingNothing`, `R2_SomeoneTakenOutOfAProjectHeWasInactiveInNoLongerSeesItOnHisList` |
| Someone inactive in every project signs in and renews as anyone does, and a wrong password says what it says to anyone (D3, R10) | `auth.attempt`, `auth.Sessions.refresh` | `AuthTests.R126_D3_SomeoneInactiveInEveryProjectSignsInAndRenewsAsAnyoneDoes`, `R126_R10_SomeoneInactiveInEveryProjectIsToldAWrongPasswordAsAnyoneIs` |
| A login is no longer disabled or enabled as a whole (D3) | `users.Map` | `UsersTests.R126_D3_ALoginIsNoLongerDisabledOrEnabledAsAWhole` |
| A project's last active manager cannot be made inactive there, by anyone (BR-29, R8) | `users.isTheLastManager` | `InactiveMembersTests.BR_29_TheLastActiveManagerOfAProjectCannotBeMadeInactiveThere`, `BR_29_WithASecondActiveManagerTheFirstCanBeMadeInactiveAndThenTheSecondCannot` |
| Making inactive and active again each write a History line (R9, AC-21.1) | `users.Routes.disable`, `Enable` | `InactiveMembersTests.R9_MakingSomeoneInactiveAndActiveAgainEachWriteAHistoryLineByTheManager` |
| Doing it twice changes nothing, at the same moment too (findings row 190): a second disable meets the unique index, a second enable finds the row already gone, and each answers as saved | `users.Routes.disable`, `Enable`, `changes.Change.Save` | `InactiveMembersTests.R126_DoingItTwiceChangesNothing`, `Row190_ManyDisablesAtTheSameMomentAllAnswerAsSavedAndKeepOneRow`, `Row190_ManyEnablesAtTheSameMomentAllAnswerAsSavedActiveAndWriteOneHistoryLine` |
| Only whoever manages the project's users makes someone inactive, and only someone in it | `users.Map`, `InProjectAsync` | `InactiveMembersTests.R126_OnlyWhoeverManagesItsUsersMakesSomeoneInactiveOrActive`, `R126_SomeoneNotInTheProjectIsNotFound` |
| Taking someone out of the project ends his being inactive there, so added again he arrives active | `users.Routes.take` | `InactiveMembersTests.R126_TakingSomeoneOutOfTheProjectEndsHisBeingInactiveThereSoAddedAgainHeArrivesActive` |
| A user's answer says inactive for each project, and nothing for the whole login | `UserProjectView`, `UserView` | `InactiveMembersTests.R126_AUserAnswerSaysInactiveForEachProjectAndNothingForTheWholeLogin` |
| A login is changed only by someone who manages every project it is in, and never the admin's (BR-33) | `users.Routes.mayChange` | `UsersTests.SomeoneWithNoRoleOverALoginCannotChangeIt`, `ProjectUsersTests.AManagerCannotResetSomeoneInAnotherProject`, `AManagerCannotTakeOverTheAdmin` |
| Every change is in the History table, never the password's hash | `history.Write` | `UsersTests.EveryChangeIsInTheHistoryWithNoPasswordHash` |
| The answer never carries the password's hash | `UserView` | `UsersTests.AnAnswerNeverCarriesThePasswordHash` |

## Left to later use cases

A cash custody created with the user (UC-015).
