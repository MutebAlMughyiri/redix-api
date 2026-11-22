[Documentation](../README.md) › UC-010

# UC-010: Set the default project

**What it is for:** opening the usual project straight after sign-in (FR-37): a default project always opens directly (the owner, 8 Nov 2026).
**Who uses it:** anyone signed in, for themselves.
**Issue:** redix-api #28; the app side is redix-app #29.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/{projectId}/default` | `ReadProject` in it | `200` `{ defaultProjectId }`; `403` without access |
| `DELETE /projects/default` | Any session | `200` `{ defaultProjectId: "" }` |

A session answers `defaultProjectId`, the project sign-in opens; empty when there is none or when access to it is gone, being made inactive in it included: the default is skipped, not cleared, and comes back when he is made active again (redix-api #126).

## Tables

| Table | Holds |
| --- | --- |
| `user_default_projects` | A user's default project, which sign-in opens, keyed by the user (findings row 216); no row means none |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A default project is kept for the next session, which opens it (AC-37.1) | `projects.Routes.setDefault`, `projects.DefaultOf` | `ProjectsTests.AC_37_1_ADefaultProjectIsKeptForTheNextSessionWhichOpensIt` |
| A default without access is not offered (AC-37.2) | `projects.DefaultOf` | `ProjectsTests.ADefaultWithoutAccessIsNotOffered` |
| A default he is inactive in is skipped, not cleared, and comes back when he is active again (AC-37.2, redix-api #126) | `projects.DefaultOf`, `access.Book.Of` | `InactiveMembersTests.AC_37_2_ADefaultProjectHeIsInactiveInIsSkippedNotClearedAndComesBackWhenHeIsActiveAgain` |
| A project without access cannot be the default (BR-33) | `access.ReadProject` | `ProjectsTests.AProjectWithoutAccessCannotBeTheDefault` |
| A project waiting for approval cannot be the default | `access.ReadProject` | `ProjectsTests.AWaitingProjectCannotBeTheDefault` |
| A finished project can be the default | `access.ReadProject` | `ProjectsTests.AFinishedProjectCanBeTheDefault` |
| A new default replaces the old, one row per user, the user being its key (findings row 216) | The key, `projects.Routes.setDefault` | `ProjectsTests.ANewDefaultReplacesTheOld`, `R216_ADefaultProjectIsARowKeyedByItsUserSoANewDefaultChangesThatRow`, `R216_TheDatabaseRefusesASecondDefaultProjectForOneUserByItsKey`, `MigrationStepsTests.R216_UserDefaultProjectsKeyedByUser_EveryDefaultStaysKeyedByItsUserAndItsOwnIdGoes` |
| Removing the default leaves none, so sign-in shows the list | `projects.Routes.removeDefault` | `ProjectsTests.RemovingTheDefaultLeavesNoneSoSignInShowsTheList` |
| One user's default changes nothing for another | `projects.Routes.setDefault` | `ProjectsTests.ADefaultIsOneUsersAlone` |

## My email in Settings

Settings › «بريدي» changes the signed-in person's own email (redix-api #127, AC-10.4, R3); the admin and whoever registered a project set theirs here (D6). Both routes need a working session.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /me/email` with `{ email }` | Sends a six-digit code to the new address, in the language of `Accept-Language`; the email stays as it was | `202`; `400` `emailInvalid`; `409` `emailTaken`; `429` asked too often (`EmailCodes:PermitsPerHour`) |
| `POST /me/email/confirm` with `{ code }` | Makes the address the last code went to the person's email, ending every code sent before it | `204`; `400` `codeWrong`; `409` `emailTaken` |

A session answers `email`, which Settings shows; empty while none is set. The codes are kept in `EmailCodes` (UC-002).

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A new address holds only once its code is entered, and the old one until then (AC-10.4) | `auth.Codes.askToConfirm`, `auth.Codes.confirm`, the `email_codes` table | `MyEmailTests.AC_10_4_ANewAddressHoldsOnlyOnceItsCodeIsEnteredAndTheOldOneUntilThen`, `AC_10_4_AWrongCodeLeavesTheOldAddress` |
| A new address holding ends the reset code sent to the old one; asking a code for a new address ends none while the old one holds (redix-api #127) | `auth.Codes.confirm`, `users.ChangeEmail` | `MyEmailTests.R127_ANewAddressHoldingEndsTheResetCodeSentToTheOldOne`, `R127_ACodeAskedForANewAddressEndsNoResetCodeWhileTheOldAddressHolds` |
| A code of another purpose, or none asked for, confirms nothing | `auth.Codes.check` | `MyEmailTests.AC_10_4_AResetCodeConfirmsNoAddressAndNoCodeConfirmsNothing` |
| The new address is well formed and no other account holds it, when asked and when confirmed (R9) | `users.RefusalOfEmail` | `MyEmailTests.R9_AnAddressAnotherAccountHoldsOrOneNotWellFormedIsRefusedAndNothingIsSent`, `R9_AnAddressAnotherAccountTookBeforeItsCodeWasEnteredIsRefused` |
| The admin sets his email here and then resets his password with it (D6, R10) | `auth.Codes.askToConfirm`, `auth.Codes.confirm`, `auth.Codes.forgotPassword` | `MyEmailTests.D6_R10_TheAdminSetsHisEmailInSettingsAndThenResetsWithIt` |
| The confirmation is in the language the phone asked in (AC-02.10) | `email.Send` | `MyEmailTests.AC_02_10_TheConfirmationIsInTheLanguageThePhoneAskedIn` |
| A confirmed address is in the History table, by the person, with the old and the new (UC-005) | `auth.Codes.confirm` | `MyEmailTests.AConfirmedAddressIsInTheHistoryByThePersonWithTheOldAndTheNew` |

## On the web: My email (redix-api #175)

Settings shows the email the person's account holds, and My email asks for a new one in the details panel, as the app's two dialogs do: the new email and Send code, through `auth.Codes.AskToConfirmEmail`, the rule `POST /me/email` runs, one address asking only so often, the app's asks and the web's counted together; then the code sent to it and Save, through `auth.Codes.ConfirmEmail`, the rule `POST /me/email/confirm` runs. Kept, the panel closes and Settings shows the new email; refused, the form says why in the app's words: an email the rules refuse, one another account holds, or a code that is wrong or expired. Its pages are in `go-backend/web` (`myemail.go`, `myemail.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /details/my-email` | My email's first step | `200` the form |
| `POST /my-email` | Sends a code to the `email` typed | `200` the code's step, or the form saying why not |
| `POST /my-email/confirm` | Keeps the email the `code` was sent to | `200` the panel closed, Settings beside it; or the code's step saying why not |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| My email sends a code to the new email and keeps it once the code comes back; an email another account holds, or a wrong code, is refused (AC-10.4) | `auth.Codes.AskToConfirmEmail`, `auth.Codes.ConfirmEmail` | `TestWeb_R175_UC010_MyEmailIsConfirmedWithTheEmailedCodeAsTheAppsDialogs` |
