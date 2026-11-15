[Documentation](../README.md) › UC-002

# UC-002: Sign in with username and password

**What it is for:** opening a session, so the app can act for a user; renewing it while the user keeps working; and, with «نسيت كلمة المرور؟», resetting a forgotten password with a code sent by email (redix-api #127).
**Who uses it:** every user with an account. The first user, the admin (BR-29), is created from configuration.
**Issue:** redix-api #21; the app side is redix-app #23. The forgotten password: redix-api #127.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /auth/sign-in` | Checks a username and password and opens a session on a new device | `200` a session; `401` with `code` `invalidCredentials` or `accountLocked` |
| `POST /auth/refresh` | Renews a session with the device's refresh token, which it replaces | `200` a session; `401` with `code` `sessionExpired` |
| `POST /auth/forgot-password` | `{ login }`, a username or an email: sends a six-digit code to the account's email, in the language of `Accept-Language` (redix-api #127) | `202` whatever happened, an unknown account and one with no email included; `429` asked too often |
| `POST /auth/reset-with-code` | `{ login, code, newPassword }`: replaces the password once the code is right, and signs the person out everywhere | `204`; `400` `codeWrong` or `weakPassword`; `429` asked too often |

The two routes of a forgotten password are open without a session too, and share one limit per caller, `EmailCodes:PermitsPerHour` (5 by default, as registration's); `/auth/forgot-password` is limited per login typed too, `EmailCode.AsksPerLoginAnHour` (5 an hour), whatever its case and whether or not it names an account (D4). A session is a JWT access token valid 15 minutes, the moment it expires (`accessTokenExpiresAt`, UTC, which the app renews before, redix-api #126), a refresh token, and whether the password must be changed first. Both endpoints are open without a session. Full requests and responses are in Swagger.

## Tables

| Table | Holds |
| --- | --- |
| `Users` | Username (unique), password hash, must-change-password, wrong passwords in a row, locked until |
| `Devices` | One row per sign-in: the user, the SHA-256 of its refresh token (unique), when it expires, revoked, last seen |
| `EmailCodes` | The code sent for a person and a purpose, the last one: the address, the code's hash, when it stops working, its wrong tries, when it was spent (redix-api #127) |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A password is stored only as its hash (NFR-09) | `User.SetPassword`, ASP.NET Core's `PasswordHasher` | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| The right password opens a session with a 15-minute access token (FR-01, AC-01.1) | `AuthEndpoints.SignIn`, `AccessTokens` | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| A wrong password and an unknown username get the same answer (AC-01.2) | `AuthEndpoints.SignIn`, `User.SignIn` | `AuthTests.AWrongPasswordAndAnUnknownUsernameGetTheSameAnswer` |
| Five wrong passwords in a row lock the account for 15 minutes (AC-01.2) | `User.SignIn` | `AuthTests.FiveWrongPasswordsInARowLockTheAccountFor15Minutes` |
| The right password clears the count of wrong ones | `User.SignIn` | `AuthTests.TheRightPasswordClearsTheCountOfWrongOnes` |
| Someone inactive in every project signs in and renews as anyone does, to his project list, and a wrong password says what it says to anyone (redix-api #126, D3, R10) | `User.SignIn`, `AuthEndpoints.Refresh` | `AuthTests.R126_D3_SomeoneInactiveInEveryProjectSignsInAndRenewsAsAnyoneDoes`, `R126_R10_SomeoneInactiveInEveryProjectIsToldAWrongPasswordAsAnyoneIs` |
| An access token works for 15 minutes, by the server's clock | `AccessTokens.ValidationParameters` | `AuthTests.AnAccessTokenWorksFor15MinutesByTheServersClock` |
| A session says when its access token expires, at sign-in and at each renewal, and the token is refused from then on (redix-api #126, R6) | `AccessTokens.Create`, `Session.OfAsync` | `AuthTests.R126_R6_ASessionSaysWhenItsAccessTokenExpiresAtSignInAndAtEachRenewal`, `R126_R6_AnAccessTokenIsRefusedOnceTheMomentItsSessionNamedHasPassed` |
| A refresh replaces the refresh token; the old one stops working | `Device.RenewRefreshToken` | `AuthTests.ARefreshReplacesTheRefreshToken` |
| A refresh token unused for 12 hours expires (NFR-09) | `Device.CanRefresh` | `AuthTests.ASessionUsedWithin12HoursStaysOpenAndOneUnusedFor12HoursEnds` |
| Only the refresh token's hash is stored | `Device.HashOf` | Enforced by the model |
| The first user is the admin, from configuration, and must change the password (BR-29) | `AdminSeed`, run by EF Core's seeding | `AuthTests.TheFirstUserIsTheAdminFromConfigurationAndMustChangeThePassword` |
| The seed waits while a migration is still to apply, so a rollback to before the `Users` table succeeds quietly | `AdminSeed.Seed`, `AdminSeed.SeedAsync` | `AdminSeedTests.AMigrationThatStopsBeforeTheUsersTableSeedsNothingAndSucceeds_AsARollbackDoes` |
| A sign-in is in the History table with the user and the time (UC-005) | `AuthEndpoints.SignIn` | `AuthTests.ASignInIsInTheHistoryWithTheUserAndTheTime` |
| Every session says what its user holds everywhere (UC-009) | `Session.OfAsync` | `AuthTests.ASessionSaysWhatItsUserHoldsEverywhere` |
| «نسيت كلمة المرور؟» with a username or an email sends a six-digit code to the account's email; the code and a new password reset it (AC-02.6, D5) | `PasswordResetEndpoints.ForgotPassword`, `ResetWithCode`, `EmailCode.MakeAsync` | `PasswordResetTests.AC_02_6_ACodeToTheAccountsEmailAndANewPasswordResetItAndTheOldOneNoLongerSignsIn`, `AC_02_6_D5_TheEmailNamesTheAccountAsTheUsernameDoesWhateverItsCase`, `AC_02_6_AWrongCodeResetsNothing` |
| The admin resets the same way, with the email set in Settings (R10, D6) | `PasswordResetEndpoints` | `PasswordResetTests.R10_D6_TheAdminResetsTheSameWay` |
| Asking answers the same whether or not the account or its email exists, and sends nothing then (AC-02.7, R5) | `PasswordResetEndpoints.ForgotPassword`, `ResetWithCode` | `PasswordResetTests.AC_02_7_R5_AnUnknownAccountOrOneWithNoEmailGetsTheSameAnswerAsAKnownOneAndNothingIsSent`, `EmailSenderTests.R5_D2_AnAzureSenderNotSetUpThrowsNothingSoTheAnswerStaysAsItWas` |
| A code works once, within 15 minutes, for 5 tries, and a new code ends the earlier one (AC-02.8, D3) | `EmailCode.CheckAsync`, `MakeAsync`, `Spend` | `PasswordResetTests.AC_02_8_ACodeWorksOnce`, `AC_02_8_ACodeWorksWithin15MinutesAndNotAfter`, `AC_02_8_FiveWrongCodesEndItSoTheRightOneIsRefusedAfterThemButNotWithATryLeft`, `AC_02_8_D3_ANewCodeEndsTheEarlierOne` |
| Asking too often is refused for a while, per caller (AC-02.8, D4) | `EmailCode.AskingLimit`, ASP.NET Core's rate limiter in `Program.cs` | `PasswordResetTests.AC_02_8_D4_AskingTooOftenIsRefusedForAWhileAndAskingWithinTheLimitIsNot` |
| One login typed too often is refused for a while, whatever its case, an unknown one as an account's, and the refused ask sends nothing; another login is not (AC-02.8, D4, R5) | `PasswordResetEndpoints.ForgotPassword`, `EmailCode.AsksPerLoginAnHour`, the keyed `PartitionedRateLimiter` in `Program.cs` | `PasswordResetTests.AC_02_8_D4_OneLoginTypedTooOftenIsRefusedWhateverItsCaseAndAnotherLoginIsNot`, `AC_02_8_D4_R5_AnAccountsLoginIsLimitedAsAnUnknownOneIsAndTheRefusedAskSendsNothing` |
| Only the code's hash is kept (the rule) | `EmailCode.MakeAsync`, ASP.NET Core's `PasswordHasher` | `PasswordResetTests.R127_OnlyTheCodesHashIsKept` |
| A reset ends every session, unlocks the account and asks for no other change; a wrong code ends nothing (AC-02.9, R7) | `User.ResetPasswordAsync`, the one reset a manager's uses too | `PasswordResetTests.AC_02_9_R7_AResetEndsEverySessionUnlocksTheAccountAndAsksForNoOtherChange`, `AC_02_9_AWrongCodeEndsNoSession` |
| A new password holds `PasswordPolicy`, and a refused one leaves the code unspent | `PasswordResetEndpoints.ResetWithCode` | `PasswordResetTests.AWeakNewPasswordIsRefusedAndTheCodeStaysUnspent` |
| The email is in the language the phone asked in, Arabic for any other (AC-02.10, R8) | `UseRequestLocalization`, `EmailSender.SendAsync` | `PasswordResetTests.AC_02_10_R8_TheEmailIsInTheLanguageThePhoneAskedInAndArabicForAnyOther`, `EmailSenderTests.R8_AC_02_10_AnEmailsSubjectAndBodyAreTheTranslationsWordsInTheLanguageAskedWithItsCode` |
| Development keeps each email in a folder and sends nothing (D7) | `FolderEmailSender` | `EmailSenderTests.R127_D7_DevelopmentKeepsEachEmailAsAFileInItsFolderAndSendsNothing` |
| A reset by code is in the History table, by the person, never the password's hash (UC-005) | `PasswordResetEndpoints.ResetWithCode` | `PasswordResetTests.AResetIsInTheHistoryByThePersonWithNoPasswordHash` |

## Left to later use cases

The user's projects in the session (UC-010). The history row of each sign-in came with UC-005, and the forced password change with token checks on every request with UC-003.
