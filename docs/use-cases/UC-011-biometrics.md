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

| Rule | Code | Test |
| --- | --- | --- |
| A phone with it on never times out | `Device.CanRefresh` | `AuthTests.AnUnusedPhoneWithBiometricsNeverTimesOut` |
| Turning it off brings back the 12 hours, from that moment | `AuthEndpoints.SetBiometrics` | `AuthTests.TurningBiometricsOffBringsBackThe12Hours` |
| Sign-out, a changed password or a disabled user still end the session | `Device.CanRefresh`, `AuthEndpoints.Refresh` | `AuthTests.BiometricsDoNotOutliveARevokedPhoneOrADisabledUser` |
| It belongs to one phone | `Device.UsesBiometrics` | `AuthTests.BiometricsBelongToOnePhone` |
| A new sign-in starts with it off | `Device` | `AuthTests.ANewSignInStartsWithBiometricsOff` |
| Every session answer says whether it is on | `Session.OfAsync` | `AuthTests.TurningBiometricsOnIsAnswered` |
| Changing the password keeps it on this phone | `AuthEndpoints.ChangePassword` | `AuthTests.ChangingThePasswordKeepsBiometricsOnThisPhone` |
| A temporary password cannot turn it on | `SessionPolicies.Working` | `AuthTests.ATemporaryPasswordCannotTurnBiometricsOn` |
| Turning it on and off is in the History table | `AuthEndpoints.SetBiometrics` | `AuthTests.TurningBiometricsOnAndOffIsInHistory` |
