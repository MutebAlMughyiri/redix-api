[Documentation](../README.md) › UC-002

# UC-002: Sign in with username and password

**What it is for:** opening a session, so the app can act for a user; renewing it while the user keeps working.
**Who uses it:** every user with an account. The first user, the system admin (BR-29), is created from configuration.
**Issue:** redix-api #21; the app side is redix-app #23.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /auth/sign-in` | Checks a username and password and opens a session on a new device | `200` a session; `401` with `code` `invalidCredentials` or `accountLocked` |
| `POST /auth/refresh` | Renews a session with the device's refresh token, which it replaces | `200` a session; `401` with `code` `sessionExpired` |

A session is a JWT access token valid 15 minutes, a refresh token, and whether the password must be changed first. Both endpoints are open without a session. Full requests and responses are in Swagger.

## Tables

| Table | Holds |
| --- | --- |
| `Users` | Username (unique), password hash, must-change-password, active, wrong passwords in a row, locked until |
| `Devices` | One row per sign-in: the user, the SHA-256 of its refresh token (unique), when it expires, revoked, last seen |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A password is stored only as its hash (NFR-09) | `User.SetPassword`, ASP.NET Core's `PasswordHasher` | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| The right password opens a session with a 15-minute access token (FR-01, AC-01.1) | `AuthEndpoints.SignIn`, `AccessTokens` | `AuthTests.TheRightPasswordOpensASessionWithA15MinuteAccessToken` |
| A wrong password and an unknown username get the same answer (AC-01.2) | `AuthEndpoints.SignIn`, `User.SignIn` | `AuthTests.AWrongPasswordAndAnUnknownUsernameGetTheSameAnswer` |
| Five wrong passwords in a row lock the account for 15 minutes (AC-01.2) | `User.SignIn` | `AuthTests.FiveWrongPasswordsInARowLockTheAccountFor15Minutes` |
| The right password clears the count of wrong ones | `User.SignIn` | `AuthTests.TheRightPasswordClearsTheCountOfWrongOnes` |
| A disabled user cannot sign in (AC-01.3) | `User.SignIn` | `AuthTests.ADisabledUserCannotSignIn` |
| A disabled user's open session ends at its next request (AC-01.3) | `AuthEndpoints.Refresh` | `AuthTests.ADisabledUsersOpenSessionEndsAtItsNextRequest` |
| An access token works for 15 minutes, by the server's clock | `AccessTokens.ValidationParameters` | `AuthTests.AnAccessTokenWorksFor15MinutesByTheServersClock` |
| A refresh replaces the refresh token; the old one stops working | `Device.RenewRefreshToken` | `AuthTests.ARefreshReplacesTheRefreshToken` |
| A refresh token unused for 12 hours expires (NFR-09) | `Device.CanRefresh` | `AuthTests.ASessionUsedWithin12HoursStaysOpenAndOneUnusedFor12HoursEnds` |
| Only the refresh token's hash is stored | `Device.HashOf` | Enforced by the model |
| The first user is the system admin, from configuration, and must change the password (BR-29) | `SystemAdminSeed`, run by EF Core's seeding | `AuthTests.TheFirstUserIsTheSystemAdminFromConfigurationAndMustChangeThePassword` |
| A sign-in is in the audit trail with the user and the time (UC-005) | `AuthEndpoints.SignIn` | `AuthTests.ASignInIsInTheAuditTrailWithTheUserAndTheTime` |
| Every session says what its user holds everywhere (UC-009) | `Session.OfAsync` | `AuthTests.ASessionSaysWhatItsUserHoldsEverywhere` |

## Left to later use cases

The user's projects in the session (UC-010). The audit record of each sign-in came with UC-005, and the forced password change with token checks on every request with UC-003.
