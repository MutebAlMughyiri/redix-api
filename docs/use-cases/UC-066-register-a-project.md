[Documentation](../README.md) › UC-066

# UC-066: Register a project, and approve it

**What it is for:** letting a new project in, with its own project manager, once an admin has approved it (FR-52, BR-29, BR-34).
**Who uses it:** anyone registers, from the sign-in page or from inside the app. Only the admin approves, and that is all the admin does.
**Issue:** redix-api #83; the app side is redix-app #79.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/register` | Nothing; limited per address | `201` the project; `400` `invalidName`, `unitNameTaken`, `invalidUsername`, `invalidMobile` or `weakPassword`; `409` `usernameTaken`; `429` |
| `POST /projects` | Any session | `201` the project, with the caller as manager; `400` `invalidName` |
| `GET /projects/registered` | `ApproveProjects` | `200` what is waiting, by start date, with the manager's details |
| `POST /projects/{projectId}/approve` | `ApproveProjects` | `200`, and the project's structure is copied (UC-012); `404`; `409` `notPending` |
| `POST /projects/{projectId}/reject` with `{ reason }` | `ApproveProjects` | `200`; `400` `invalidReason`; `404`; `409` `notPending` |

Both `POST /projects/register` and `POST /projects` take `partners`, the names of its partners in order, left out for none: each becomes a table of its رأس المال once it is approved (redix-api #109). A blank or overlong name answers `invalidName`, and nothing is kept.

A project answers `status`, now also `pendingApproval` or `rejected`, and `rejectionReason`.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A registered project has its manager, all or nothing | `projects.Routes.register` | `RegistrationTests.ARegisteredProjectHasItsManager`, `ABadFieldCreatesNothing` |
| Its declared partners are kept in their order with it, all or nothing | `projects.declaredPartners` | `RegistrationTests.R109_TheDeclaredPartnersAreKeptInTheirOrderAndABlankOneCreatesNothing` |
| Someone signed in registers with their own login | `projects.Routes.create` | `RegistrationTests.SomeoneSignedInRegistersWithTheirOwnLogin` |
| A waiting project gives no permission (BR-34) | `access.Book.Of` | `RegistrationTests.AWaitingProjectGivesNoPermission` |
| Only the admin approves, and sees what is needed to decide | `access.ApproveProjects` | `RegistrationTests.OnlyTheAdminApproves`, `TheAdminSeesWhatIsNeededToDecide` |
| The admin sees nothing inside a project (BR-29) | `ProjectPermission` rows | `RegistrationTests.TheAdminSeesNothingInsideAProject` |
| A rejection says why | `projects.Routes.reject` | `RegistrationTests.ARejectionSaysWhy` |
| A project is decided once | `projects.Routes.approve`, `projects.Routes.reject` | `RegistrationTests.AProjectIsDecidedOnce` |
| A project manager cannot take over the admin | `users.Routes.mayChange`, `users.Routes.addExisting` | `ProjectUsersTests.AManagerCannotTakeOverTheAdmin` |
| Too many tries from one address are refused | `limits.PerHour`, `Window.PerAddress` | `RegistrationTests.TooManyTriesAreRefused` |
| Every registration and decision is in the History table | `history.Write` | `RegistrationTests.EveryDecisionIsInTheHistory` |
