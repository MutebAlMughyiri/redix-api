[Documentation](../README.md) › UC-062

# UC-062: Sign out

**What it is for:** ending a session on one device, so that the next person signs in as themselves (added by Muteb on 5 Nov 2026; numbered last, built after UC-007).
**Who uses it:** every signed-in user.
**Issue:** redix-api #78; the app side is redix-app #74.

## Endpoint

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /auth/sign-out` | Ends this device's session | `204`; `401` without an open session |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A sign-out ends this device's tokens, and only this device's | `auth.Sessions.signOut`, `access.Gate.SignedIn` | `AuthTests.ASignOutEndsThisDeviceOnly` |
| A sign-out is in the History table | `auth.Sessions.signOut` | `AuthTests.ASignOutIsInTheHistory` |
| Signing out needs a session | `access.Gate.Working` | `AuthTests.SigningOutNeedsASession` |
