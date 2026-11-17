[Documentation](../README.md) › UC-011

# UC-011: Keep a phone with fingerprint or face signed in

**What it is for:** a phone that asks for a fingerprint or face on opening never needs the password again, until the session is ended on purpose (FR-35).
**Who uses it:** anyone signed in, for this phone.
**Issue:** redix-api #29; the app side is redix-app #30.

The check runs on the phone alone. The server never sees biometric data; it only remembers that this phone has it on.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /auth/biometrics` with `{ on }` | A session with no temporary password | `200` the session with new tokens and `usesBiometrics` |

Sign-in, renewal and a password change answer `usesBiometrics` too. A new sign-in starts with it off.

## Tables

| Column | Holds |
| --- | --- |
| `Devices.UsesBiometrics` | Whether this phone asks for a fingerprint or face on opening |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A phone with it on never times out | `auth.canRefresh` | `AuthTests.AnUnusedPhoneWithBiometricsNeverTimesOut` |
| Turning it off brings back the 12 hours, from that moment | `auth.Sessions.setBiometrics` | `AuthTests.TurningBiometricsOffBringsBackThe12Hours` |
| Sign-out and a changed password still end the session | `auth.canRefresh`, `auth.Sessions.refresh` | `AuthTests.BiometricsDoNotOutliveARevokedPhone` |
| It belongs to one phone | `auth.Sessions.setBiometrics` | `AuthTests.BiometricsBelongToOnePhone` |
| A new sign-in starts with it off | `Device` | `AuthTests.ANewSignInStartsWithBiometricsOff` |
| Every session answer says whether it is on | `auth.Sessions.answer` | `AuthTests.TurningBiometricsOnIsAnswered` |
| Changing the password keeps it on this phone | `auth.Sessions.changePassword` | `AuthTests.ChangingThePasswordKeepsBiometricsOnThisPhone` |
| A temporary password cannot turn it on | `access.Gate.Working` | `AuthTests.ATemporaryPasswordCannotTurnBiometricsOn` |
| Turning it on and off is in the History table | `auth.Sessions.setBiometrics` | `AuthTests.TurningBiometricsOnAndOffIsInHistory` |
