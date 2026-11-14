[Documentation](../README.md) › UC-010

# UC-010: Set the default project

**What it is for:** opening the usual project straight after sign-in (FR-37): a default project always opens directly (Muteb, 8 Nov 2026).
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
| `UserDefaultProjects` | A user's default project, which sign-in opens, keyed by the user (findings row 216); no row means none |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A default project is kept for the next session, which opens it (AC-37.1) | `ProjectsEndpoints.SetDefault`, `Session.OfAsync` | `ProjectsTests.AC_37_1_ADefaultProjectIsKeptForTheNextSessionWhichOpensIt` |
| A default without access is not offered (AC-37.2) | `Session.OfAsync` | `ProjectsTests.ADefaultWithoutAccessIsNotOffered` |
| A default he is inactive in is skipped, not cleared, and comes back when he is active again (AC-37.2, redix-api #126) | `Session.OfAsync`, `UserAccess.OfAsync` | `InactiveMembersTests.AC_37_2_ADefaultProjectHeIsInactiveInIsSkippedNotClearedAndComesBackWhenHeIsActiveAgain` |
| A project without access cannot be the default (BR-33) | `Permission.ReadProject` | `ProjectsTests.AProjectWithoutAccessCannotBeTheDefault` |
| A project waiting for approval cannot be the default | `Permission.ReadProject` | `ProjectsTests.AWaitingProjectCannotBeTheDefault` |
| A finished project can be the default | `Permission.ReadProject` | `ProjectsTests.AFinishedProjectCanBeTheDefault` |
| A new default replaces the old, one row per user, the user being its key (findings row 216) | The key, `ProjectsEndpoints.SetDefault` | `ProjectsTests.ANewDefaultReplacesTheOld`, `R216_ADefaultProjectIsARowKeyedByItsUserSoANewDefaultChangesThatRow`, `R216_TheDatabaseRefusesASecondDefaultProjectForOneUserByItsKey`, `MigrationStepsTests.R216_UserDefaultProjectsKeyedByUser_EveryDefaultStaysKeyedByItsUserAndItsOwnIdGoes` |
| Removing the default leaves none, so sign-in shows the list | `ProjectsEndpoints.RemoveDefault` | `ProjectsTests.RemovingTheDefaultLeavesNoneSoSignInShowsTheList` |
| One user's default changes nothing for another | `UserDefaultProject.UserId` | `ProjectsTests.ADefaultIsOneUsersAlone` |

## My email in Settings

Settings › «بريدي» changes the signed-in person's own email (redix-api #127, AC-10.4, R3); the admin and whoever registered a project set theirs here (D6). Both routes need a working session.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /me/email` with `{ email }` | Sends a six-digit code to the new address, in the language of `Accept-Language`; the email stays as it was | `202`; `400` `emailInvalid`; `409` `emailTaken`; `429` asked too often (`EmailCodes:PermitsPerHour`) |
| `POST /me/email/confirm` with `{ code }` | Makes the address the last code went to the person's email | `204`; `400` `codeWrong`; `409` `emailTaken` |

A session answers `email`, which Settings shows; empty while none is set. The codes are kept in `EmailCodes` (UC-002).

| Rule | Code | Test |
| --- | --- | --- |
| A new address holds only once its code is entered, and the old one until then (AC-10.4) | `MyEmailEndpoints.Ask`, `Confirm`, `EmailCode` | `MyEmailTests.AC_10_4_ANewAddressHoldsOnlyOnceItsCodeIsEnteredAndTheOldOneUntilThen`, `AC_10_4_AWrongCodeLeavesTheOldAddress` |
| A code of another purpose, or none asked for, confirms nothing | `EmailCode.CheckAsync` | `MyEmailTests.AC_10_4_AResetCodeConfirmsNoAddressAndNoCodeConfirmsNothing` |
| The new address is well formed and no other account holds it, when asked and when confirmed (R9) | `User.RefusalOfEmailAsync` | `MyEmailTests.R9_AnAddressAnotherAccountHoldsOrOneNotWellFormedIsRefusedAndNothingIsSent`, `R9_AnAddressAnotherAccountTookBeforeItsCodeWasEnteredIsRefused` |
| The admin sets his email here and then resets his password with it (D6, R10) | `MyEmailEndpoints`, `PasswordResetEndpoints` | `MyEmailTests.D6_R10_TheAdminSetsHisEmailInSettingsAndThenResetsWithIt` |
| The confirmation is in the language the phone asked in (AC-02.10) | `EmailSender.SendAsync` | `MyEmailTests.AC_02_10_TheConfirmationIsInTheLanguageThePhoneAskedIn` |
| A confirmed address is in the History table, by the person, with the old and the new (UC-005) | `MyEmailEndpoints.Confirm` | `MyEmailTests.AConfirmedAddressIsInTheHistoryByThePersonWithTheOldAndTheNew` |
