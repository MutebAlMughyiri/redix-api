[Documentation](../README.md) › UC-003

# UC-003: Change password

**What it is for:** replacing a password, above all the temporary one a new user receives, which must be changed before anything else (AC-02.1).
**Who uses it:** any signed-in user, including one whose password is temporary.
**Issue:** redix-api #22; the app side is redix-app #24.

## Endpoint

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /auth/change-password` | Replaces the signed-in user's password and signs out the user's other devices | `200` a renewed session for this device; `400` with `code` `invalidCredentials`, `accountLocked` or `weakPassword`; `401` without an open session |

## How every request is secured, from UC-003 on

| Step | Code |
| --- | --- |
| The access token is checked by ASP.NET Core's JWT bearer authentication, with the same key, issuer and audience it was signed with | `AccessTokens.ValidationParameters` |
| The token's device must still be open and its user still active, so a revoked device or a disabled user is refused at the next request (AC-01.3) | `SessionPolicies.CheckSessionIsOpen` |
| Every endpoint not marked open falls under the `Working` policy: signed in, with no temporary password left to change | `SessionPolicies.Working`, the fallback policy |
| Changing the password is the one endpoint a temporary-password session may reach | `SessionPolicies.MayChangePassword` |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| Changing a temporary password ends the forced change, and the new password works (AC-02.1) | `AuthEndpoints.ChangePassword` | `ChangePasswordTests.ChangingATemporaryPasswordEndsTheForcedChangeAndTheNewPasswordWorks` |
| A wrong current password is refused and counts toward the lockout | `User.SignIn` | `ChangePasswordTests.AWrongCurrentPasswordIsRefusedAndCountsTowardTheLockout` |
| A new password has at least 8 characters and differs from the current one | `User.IsAcceptableNewPassword` | `ChangePasswordTests.ANewPasswordShorterThan8CharactersOrUnchangedIsRefused` |
| The other devices are signed out; this one stays signed in | `AuthEndpoints.ChangePassword` | `ChangePasswordTests.ChangingThePasswordSignsOutTheOtherDevicesAndKeepsThisOne` |
| A password change is in the audit trail, never the password's hash (UC-005) | `AuthEndpoints.ChangePassword` | `ChangePasswordTests.APasswordChangeIsInTheAuditTrailWithTheValuesBeforeAndAfterButNeverThePasswordHash` |
| Changing the password needs a session | `SessionPolicies.MayChangePassword` | `ChangePasswordTests.ChangingThePasswordNeedsASession` |
| A disabled user's access token is refused at the next request (AC-01.3) | `SessionPolicies.CheckSessionIsOpen` | `ChangePasswordTests.ADisabledUsersAccessTokenIsRefusedAtTheNextRequest` |
| A temporary-password session reaches nothing but the password change | `SessionPolicies.Working` | `ChangePasswordTests.EveryEndpointNotOpenToAllRefusesASessionWithATemporaryPassword` |
