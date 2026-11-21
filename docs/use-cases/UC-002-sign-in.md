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
| `POST /auth/forgot-password` | `{ login }`, a username or an email: sends a six-digit code to the account's email, in the language of `Accept-Language`, once the answer has left (redix-api #127, R5) | `202` at once whatever was typed, an unknown account and one with no email included; `429` asked too often |
| `POST /auth/reset-with-code` | `{ login, code, newPassword }`: replaces the password once the code is right, and signs the person out everywhere | `204`; `400` `codeWrong` or `weakPassword` |

The two routes of a forgotten password are open without a session too, and share one limit per caller, `REDIX_EmailCodes__PermitsPerHour` (5 by default, as registration's); `/auth/forgot-password` is limited per login typed too, `auth.asksPerLoginAnHour` (5 an hour), whatever its case and whether or not it names an account (D4). A session is a JWT access token valid 15 minutes, the moment it expires (`accessTokenExpiresAt`, UTC, which the app renews before, redix-api #126), a refresh token, and whether the password must be changed first. Both endpoints are open without a session. Full requests and responses are in Swagger.

## Tables

| Table | Holds |
| --- | --- |
| `Users` | Username (unique), password hash, must-change-password, wrong passwords in a row, locked until |
| `devices` | One row per sign-in: the user, the SHA-256 of its refresh token (unique), when it expires, revoked, last seen |
| `EmailCodes` | The code sent for a person and a purpose, the last one: the address, the code's hash, when it stops working, its wrong tries, when it was spent (redix-api #127) |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A password is stored only as its hash (NFR-09) | `credentials.Hash`, in ASP.NET Core Identity's format (PBKDF2, HMAC-SHA512, 100,000 iterations), so .NET's hashes still check | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| The right password opens a session with a 15-minute access token (FR-01, AC-01.1) | `auth.Sessions.SignIn`, `AccessTokens` | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| A wrong password and an unknown username get the same answer (AC-01.2) | `auth.Sessions.SignIn`, `auth.attempt` | `AuthTests.AWrongPasswordAndAnUnknownUsernameGetTheSameAnswer` |
| Five wrong passwords in a row lock the account for 15 minutes (AC-01.2) | `auth.attempt` | `AuthTests.FiveWrongPasswordsInARowLockTheAccountFor15Minutes` |
| The right password clears the count of wrong ones | `auth.attempt` | `AuthTests.TheRightPasswordClearsTheCountOfWrongOnes` |
| Someone inactive in every project signs in and renews as anyone does, to his project list, and a wrong password says what it says to anyone (redix-api #126, D3, R10) | `auth.attempt`, `auth.Sessions.Renew` | `AuthTests.R126_D3_SomeoneInactiveInEveryProjectSignsInAndRenewsAsAnyoneDoes`, `R126_R10_SomeoneInactiveInEveryProjectIsToldAWrongPasswordAsAnyoneIs` |
| An access token works for 15 minutes, by the server's clock | `credentials.Tokens.Check` | `AuthTests.AnAccessTokenWorksFor15MinutesByTheServersClock` |
| A session says when its access token expires, at sign-in and at each renewal, and the token is refused from then on (redix-api #126, R6) | `credentials.Tokens.Create`, `auth.Sessions.answer` | `AuthTests.R126_R6_ASessionSaysWhenItsAccessTokenExpiresAtSignInAndAtEachRenewal`, `R126_R6_AnAccessTokenIsRefusedOnceTheMomentItsSessionNamedHasPassed` |
| A refresh replaces the refresh token; the old one stops working | `auth.renew` | `AuthTests.ARefreshReplacesTheRefreshToken` |
| A refresh token unused for 12 hours expires (NFR-09) | `auth.canRefresh` | `AuthTests.ASessionUsedWithin12HoursStaysOpenAndOneUnusedFor12HoursEnds` |
| Only the refresh token's hash is stored | `auth.hashOf` | Enforced by the model |
| The first user is the admin, from configuration, and must change the password (BR-29) | `auth.SeedAdmin`, as the server starts | `AuthTests.TheFirstUserIsTheAdminFromConfigurationAndMustChangeThePassword` |
| The seed waits while a migration is still to apply, so a rollback to before the `Users` table succeeds quietly | `auth.SeedAdmin` | `AdminSeedTests.AMigrationThatStopsBeforeTheUsersTableSeedsNothingAndSucceeds_AsARollbackDoes` |
| A sign-in is in the History table with the user and the time (UC-005) | `auth.Sessions.SignIn` | `AuthTests.ASignInIsInTheHistoryWithTheUserAndTheTime` |
| Every session says what its user holds everywhere (UC-009) | `auth.Sessions.answer` | `AuthTests.ASessionSaysWhatItsUserHoldsEverywhere` |
| «نسيت كلمة المرور؟» with a username or an email sends a six-digit code to the account's email; the code and a new password reset it (AC-02.6, D5) | `auth.Codes.SendResetCode`, `auth.Codes.ResetWithCode`, `auth.Codes.make` | `PasswordResetTests.AC_02_6_ACodeToTheAccountsEmailAndANewPasswordResetItAndTheOldOneNoLongerSignsIn`, `AC_02_6_D5_TheEmailNamesTheAccountAsTheUsernameDoesWhateverItsCase`, `AC_02_6_AWrongCodeResetsNothing` |
| The admin resets the same way, with the email set in Settings (R10, D6) | `auth.Codes.SendResetCode`, `auth.Codes.ResetWithCode` | `PasswordResetTests.R10_D6_TheAdminResetsTheSameWay` |
| Asking answers the same whether or not the account or its email exists, and sends nothing then (AC-02.7, R5) | `auth.Codes.SendResetCode`, `auth.Codes.ResetWithCode` | `PasswordResetTests.AC_02_7_R5_AnUnknownAccountOrOneWithNoEmailGetsTheSameAnswerAsAKnownOneAndNothingIsSent`, `EmailSenderTests.R5_D2_AnAzureSenderNotSetUpThrowsNothingSoTheAnswerStaysAsItWas` |
| The answer leaves before the code is made and sent, so how long it takes says nothing of the account (AC-02.7, R5) | `auth.Codes.forgotPassword`: it answers 202 and flushes it before it makes and sends the code | `PasswordResetTests.AC_02_7_R5_TheAnswerLeavesBeforeTheCodeIsSentSoHowLongItTakesSaysNothing` |
| A code works once, within 15 minutes, for 5 tries, and a new code ends the earlier one (AC-02.8, D3) | `auth.Codes.check`, `auth.Codes.make`, the `SpendCode` query | `PasswordResetTests.AC_02_8_ACodeWorksOnce`, `AC_02_8_ACodeWorksWithin15MinutesAndNotAfter`, `AC_02_8_FiveWrongCodesEndItSoTheRightOneIsRefusedAfterThemButNotWithATryLeft`, `AC_02_8_D3_ANewCodeEndsTheEarlierOne` |
| Asking too often is refused for a while, per caller (AC-02.8, D4) | `limits.PerHour`, `Window.PerAddress` | `PasswordResetTests.AC_02_8_D4_AskingTooOftenIsRefusedForAWhileAndAskingWithinTheLimitIsNot` |
| One login typed too often is refused for a while, whatever its case, an unknown one as an account's, and the refused ask sends nothing; another login is not (AC-02.8, D4, R5) | `auth.Codes.MayAskReset`, `limits.PerHour`, a window keyed by the login | `PasswordResetTests.AC_02_8_D4_OneLoginTypedTooOftenIsRefusedWhateverItsCaseAndAnotherLoginIsNot`, `AC_02_8_D4_R5_AnAccountsLoginIsLimitedAsAnUnknownOneIsAndTheRefusedAskSendsNothing` |
| Only the code's hash is kept (the rule) | `auth.Codes.make`, `credentials.Hash` | `PasswordResetTests.R127_OnlyTheCodesHashIsKept` |
| A reset ends every session, unlocks the account and asks for no other change; a wrong code ends nothing (AC-02.9, R7) | `users.ResetPassword`, the one reset a manager's uses too | `PasswordResetTests.AC_02_9_R7_AResetEndsEverySessionUnlocksTheAccountAndAsksForNoOtherChange`, `AC_02_9_AWrongCodeEndsNoSession` |
| A new password holds `PasswordPolicy`, and a refused one leaves the code unspent | `auth.Codes.ResetWithCode` | `PasswordResetTests.AWeakNewPasswordIsRefusedAndTheCodeStaysUnspent` |
| The email is in the language the phone asked in, Arabic for any other (AC-02.10, R8) | `web.Language`, `email.Send` | `PasswordResetTests.AC_02_10_R8_TheEmailIsInTheLanguageThePhoneAskedInAndArabicForAnyOther`, `EmailSenderTests.R8_AC_02_10_AnEmailsSubjectAndBodyAreTheTranslationsWordsInTheLanguageAskedWithItsCode` |
| Development keeps each email in a folder and sends nothing (D7) | `FolderEmailSender` | `EmailSenderTests.R127_D7_DevelopmentKeepsEachEmailAsAFileInItsFolderAndSendsNothing` |
| A reset by code is in the History table, by the person, never the password's hash (UC-005) | `auth.Codes.ResetWithCode` | `PasswordResetTests.AResetIsInTheHistoryByThePersonWithNoPasswordHash` |

## On the web (redix-api #161)

The web signs a person in through the same rules, `auth.Sessions.SignIn`, `Renew`, `ReplacePassword` and `SignOut`, and resets a forgotten password through `auth.Codes.MayAskReset`, `SendResetCode` and `ResetWithCode`, with the same limits, inside the shell every web page lives in ([the web's design](../web/DESIGN.md)). Its pages are in `go-backend/web`, on the web's own port.

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /` | The shell, signed in, or with the sign-in form in its content area | `200` the page |
| `GET /sign-in` | The sign-in form again, from Forgot password's Cancel | `200` the form |
| `POST /sign-in` | Signs in from the form: `username`, `password` | `200` the whole shell, its parts arriving; `422` the form with the refusal's words |
| `GET /forgot-password` | The Forgot password form, in the sign-in's place | `200` the form |
| `POST /forgot-password` | Asks for a reset code: `login`, a username or an email; `sent` once a code was sent | `200` the form with the code's fields, whatever the login names, the code emailed once the answer has left; `422` asked too often |
| `POST /reset-with-code` | Sets a new password: `login`, `code`, `new`, `confirm` | `200` the sign-in form, saying the password was changed; `422` the form with the refusal's words |
| `POST /change-password` | Replaces a temporary password: `current`, `new`, `confirm` | `200` the whole shell; `422` the form with the refusal's words |
| `POST /sign-out` | Ends this browser's session | `200` the shell with the sign-in form |
| `GET /tab/{name}`, `DELETE /tab/{name}` | Opens or closes a tab: its content, beside the tabs and the side menu | `200`; `404` for an item no feature has built |
| `POST /view` | Keeps the View menu's choices: `top`, `status`, `side`, `width` | `204` |
| `GET /language/{language}` | Keeps `ar` or `en` and opens the page again in it | `303` to `/`; `404` for any other |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The right password opens the shell, its parts arriving once; a wrong or locked one answers the app's words under the form | `web.site.signIn`, `auth.Sessions.SignIn` | `TestWeb_R1_TheRightPasswordOpensTheShellWithItsMotion`, `TestWeb_R1_AWrongPasswordAnswersTheFormWithTheAppsWords`, `TestWeb_R1_TheFifthWrongPasswordLocksAsOnThePhone` |
| The session lives in two cookies a page's script cannot read, sent to this site alone; a form another site posts is refused | `web.site.keep`, `http.CrossOriginProtection` | `TestWeb_R17_TheSessionsCookiesAreHiddenFromScriptAndOtherSites` |
| An expired access token is renewed inside the request; 12 hours unused end the session, which the form says | `web.site.session`, `auth.Sessions.Renew` | `TestWeb_R16_AnExpiredAccessTokenIsRenewedInsideTheRequest`, `TestWeb_R16_TwelveHoursUnusedEndTheSession` |
| A temporary password is changed before anything else | `web.site.open`, `web.site.changePassword` | `TestWeb_R13_ATemporaryPasswordIsChangedBeforeAnythingElse` |
| Sign out ends this browser's device, and only it, and clears its cookies | `web.site.signOut`, `auth.Sessions.SignOut` | `TestWeb_R16_SignOutEndsThisBrowserAndClearsItsCookies` |
| The language is chosen at the sign-in form's foot and in Settings, never in the top bar | `web.site.chooseLanguage`, `web.languageChoice` | `TestWeb_R11_TheLanguageIsInSettingsNotTheTopBar`, `TestWeb_R12_R20_TheSignInFormCarriesTheLanguageAndEnglishReadsLeftToRight` |
| Every side-menu item not built stands gray | `web.contents` | `TestWeb_R14_R15_WhatIsNotBuiltStandsGray` |
| "Forgot your password?" opens its form in the sign-in's place, and Cancel goes back | `web.signInForm`, `web.site.forgotPassword`, `web.site.signInAgain` | `TestWeb_AC02_6_ForgotPasswordOpensFromTheSignInAndCancelGoesBack` |
| A username or an email and Send email a code; the code and a new password, typed twice, reset it, and the sign-in says so; a refusal answers the app's words (AC-02.6) | `web.site.sendResetCode`, `web.site.resetWithCode`, `auth.Codes.SendResetCode`, `auth.Codes.ResetWithCode` | `TestWeb_AC02_6_AForgottenPasswordIsResetWithTheEmailedCodeThenTheSignInSaysSo`, `TestWeb_AC02_6_AResetIsRefusedWithTheAppsWords` |
| Asking answers the same whatever the login names, and sends nothing for an unknown one (AC-02.7) | `web.site.sendResetCode`, `auth.Codes.SendResetCode` | `TestWeb_AC02_7_AnUnknownLoginIsAnsweredAsAKnownOneAndNothingIsSent` |
| One login asks five times an hour and one address only so often, the app's asks counted with the web's; a refusal keeps the code's fields (AC-02.8, D4) | `auth.Codes.MayAskReset`, `limits.Window.AllowsCaller` | `TestWeb_AC02_8_OneLoginAsksFiveTimesAnHourTheAppsAsksCountedWithTheWebs` |

## Left to later use cases

The user's projects in the session (UC-010). The history row of each sign-in came with UC-005, and the forced password change with token checks on every request with UC-003.
