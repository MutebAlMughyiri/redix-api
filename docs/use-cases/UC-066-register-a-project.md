[Documentation](../README.md) › UC-066

# UC-066: Register a project, and approve it

**What it is for:** letting a new project in, with its own project manager, once an admin has approved it (FR-52, BR-29, BR-34).
**Who uses it:** anyone registers, from the sign-in page or from inside the app. Only the admin approves, and that is all the admin does.
**Issue:** redix-api #83; the app side is redix-app #79.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/register` | Nothing; limited per address | `201` the project; `400` `invalidName`, `invalidUsername`, `invalidMobile` or `weakPassword`; `409` `usernameTaken`; `429` |
| `POST /projects` | Any session | `201` the project, with the caller as manager; `400` `invalidName` |
| `GET /projects/registered` | `ApproveProjects` | `200` what is waiting, by start date, with the manager's details |
| `POST /projects/{projectId}/approve` | `ApproveProjects` | `200`; `404`; `409` `notPending` |
| `POST /projects/{projectId}/reject` with `{ reason }` | `ApproveProjects` | `200`; `400` `invalidReason`; `404`; `409` `notPending` |

A project answers `status`, now also `pendingApproval` or `rejected`, and `rejectionReason`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A registered project has its manager, all or nothing | `RegistrationEndpoints.Register` | `RegistrationTests.ARegisteredProjectHasItsManager`, `ABadFieldCreatesNothing` |
| Someone signed in registers with their own login | `ProjectsEndpoints.Create` | `RegistrationTests.SomeoneSignedInRegistersWithTheirOwnLogin` |
| A waiting project gives no permission (BR-34) | `UserAccess.OfAsync` | `RegistrationTests.AWaitingProjectGivesNoPermission` |
| Only the admin approves, and sees what is needed to decide | `Permission.ApproveProjects` | `RegistrationTests.OnlyTheAdminApproves`, `TheAdminSeesWhatIsNeededToDecide` |
| The admin sees nothing inside a project (BR-29) | `RoleSeed` | `RegistrationTests.TheAdminSeesNothingInsideAProject` |
| A rejection says why | `Project.Reject` | `RegistrationTests.ARejectionSaysWhy` |
| A project is decided once | `Project.Approve`, `Project.Reject` | `RegistrationTests.AProjectIsDecidedOnce` |
| A project manager cannot take over the admin | `UsersEndpoints.MayChangeAsync`, `ProjectUsersEndpoints.AddExisting` | `ProjectUsersTests.AManagerCannotTakeOverTheAdmin` |
| Too many tries from one address are refused | The rate limiter in `Program.cs` | `RegistrationTests.TooManyTriesAreRefused` |
| Every registration and decision is in the audit trail | `db.Audit` | `RegistrationTests.EveryDecisionIsInTheAuditTrail` |
