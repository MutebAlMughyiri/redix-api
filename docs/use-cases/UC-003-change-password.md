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
| The token's device must still be open, so a revoked device is refused at the next request; someone inactive in a project keeps his session, and that project's routes refuse him (redix-api #126) | `SessionPolicies.CheckSessionIsOpen` |
| Every endpoint not marked open falls under the `Working` policy: signed in, with no temporary password left to change | `SessionPolicies.Working`, the fallback policy |
| Changing the password is the one endpoint a temporary-password session may reach | `SessionPolicies.MayChangePassword` |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Changing a temporary password ends the forced change, and the new password works (AC-02.1) | `auth.Sessions.changePassword` | `ChangePasswordTests.ChangingATemporaryPasswordEndsTheForcedChangeAndTheNewPasswordWorks` |
| A wrong current password is refused and counts toward the lockout | `auth.attempt` | `ChangePasswordTests.AWrongCurrentPasswordIsRefusedAndCountsTowardTheLockout` |
| A new password holds the password policy (redix-api #113: at least 8 characters, with a letter and a digit, not the user name) and differs from the current one | `auth.mayReplace` | `ChangePasswordTests.R113_ANewPasswordShortWithoutALetterOrADigitOrUnchangedIsRefused`, `R113_ANewPasswordThatIsTheUserNameIsRefused`; `PasswordPolicyTests` |
| The other devices are signed out; this one stays signed in | `auth.Sessions.changePassword` | `ChangePasswordTests.ChangingThePasswordSignsOutTheOtherDevicesAndKeepsThisOne` |
| A password change is in the History table, never the password's hash (UC-005) | `auth.Sessions.changePassword` | `ChangePasswordTests.APasswordChangeIsInTheHistoryWithTheValuesBeforeAndAfterButNeverThePasswordHash` |
| Changing the password needs a session | `access.Gate.Working` | `ChangePasswordTests.ChangingThePasswordNeedsASession` |
| A temporary-password session reaches nothing but the password change | `access.Gate.Working` | `ChangePasswordTests.EveryEndpointNotOpenToAllRefusesASessionWithATemporaryPassword` |
