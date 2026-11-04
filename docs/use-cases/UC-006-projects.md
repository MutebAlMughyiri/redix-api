[Documentation](../README.md) › UC-006

# UC-006: Manage projects and units

**What it is for:** starting a project with its units, and freezing it when it is finished (FR-04).
**Who uses it:** the system admin starts a project and becomes its first project manager; its financial supervisors add units and close it (UC-009).
**Issue:** redix-api #24; the app side is redix-app #26.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects` | The caller's projects with their units, running ones first, then the newest (UC-008) | `200` the projects |
| `POST /projects` | Starts a running project from a name, a start date and its units, all or nothing, with the caller as its first manager | `201` the project; `400` `invalidName`; `400` for a missing field |
| `POST /projects/{projectId}/units` | Adds a unit at the end of the project's list | `200` the project; `400` `invalidName`; `409` `projectFinished`; `403` without `ManageProject` in it |
| `POST /projects/{projectId}/close` | Closes a finished project | `200` the project; `409` `projectFinished` when already closed; `403` without `ManageProject` in it |

A project answers as `{ id, name, startDate, status, units: [{ id, name }], roles, permissions }`, with `status` `running` or `finished`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| The system admin starts a project with its units, at once, in the order given | `ProjectsEndpoints.Create`, `Project.AddUnit` | `ProjectsTests.TheSystemAdminStartsAProjectWithItsUnitsInOrder` |
| A bad unit name refuses the whole project and saves nothing | `ProjectsEndpoints.Create` | `ProjectsTests.ABadUnitNameCreatesNothing` |
| A request missing a field answers 400, in every environment | `ConfigureHttpJsonOptions` and `RouteHandlerOptions` in `Program.cs` | `ProjectsTests.ARequestMissingAFieldIsABadRequest` |
| A unit is added to a running project at the end of its list | `Project.AddUnit` | `ProjectsTests.AUnitIsAddedAtTheEnd` |
| A finished project is read-only and stays listed, so its reports stay available (AC-04.1) | `Project.IsReadOnly`, `Project.Close` | `ProjectsTests.AFinishedProjectIsReadOnlyAndStaysListed` |
| The status travels by its glossary name, `running` or `finished` | `ConfigureHttpJsonOptions`, `ProjectStatus` | `ProjectsTests.TheStatusTravelsAsItsGlossaryName` |
| A project or a unit needs a name of 1 to 100 characters, spaces around it trimmed | `Project.IsAcceptableName` | `ProjectsTests.AProjectOrAUnitNeedsANameOfAtMost100Characters` |
| Only the system admin starts a project, and whoever starts it manages it | `Permission.StartProject`, `ProjectsEndpoints.Create` | `ProjectsTests.OnlyTheSystemAdminStartsAProject`, `WhoeverStartsAProjectManagesIt` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Copying the structure from the default template or an earlier project (AC-05.2), and the template itself (`Projects.IsTemplate`) | UC-012, the first with categories and items to copy |
| Refusing new entries and approvals in a finished project | Each entry use case, through `Project.IsReadOnly` |
| The shared unit (`Units.IsShared`) | Open question 3 of the BRS |
