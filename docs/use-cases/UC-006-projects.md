[Documentation](../README.md) › UC-006

# UC-006: Manage projects and units

**What it is for:** starting a project with its units, and freezing it when it is finished (FR-04).
**Who uses it:** the financial supervisor. Until project roles arrive with UC-008, that means an owner, who is a financial supervisor in every project (BR-29).
**Issue:** redix-api #24; the app side is redix-app #26.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects` | The caller's projects with their units, running ones first, then the newest (UC-008) | `200` the projects |
| `POST /projects` | Creates a running project from a name, a start date and its units, all or nothing | `201` the project; `400` `invalidName`; `400` for a missing field |
| `POST /projects/{projectId}/units` | Adds a unit at the end of the project's list | `200` the project; `400` `invalidName`; `409` `projectFinished`; `404` for an unknown project |
| `POST /projects/{projectId}/close` | Closes a finished project | `200` the project; `409` `projectFinished` when already closed; `404` for an unknown project |

A project answers as `{ id, name, startDate, status, units: [{ id, name }] }`, with `status` `running` or `finished`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| An owner creates a project with its units, at once, in the order given | `ProjectsEndpoints.Create`, `Project.AddUnit` | `ProjectsTests.AnOwnerCreatesAProjectWithItsUnitsInOrder` |
| A bad unit name refuses the whole project and saves nothing | `ProjectsEndpoints.Create` | `ProjectsTests.ABadUnitNameCreatesNothing` |
| A request missing a field answers 400, in every environment | `ConfigureHttpJsonOptions` and `RouteHandlerOptions` in `Program.cs` | `ProjectsTests.ARequestMissingAFieldIsABadRequest` |
| A unit is added to a running project at the end of its list | `Project.AddUnit` | `ProjectsTests.AUnitIsAddedAtTheEnd` |
| A finished project is read-only and stays listed, so its reports stay available (AC-04.1) | `Project.IsReadOnly`, `Project.Close` | `ProjectsTests.AFinishedProjectIsReadOnlyAndStaysListed` |
| The status travels by its glossary name, `running` or `finished` | `ConfigureHttpJsonOptions`, `ProjectStatus` | `ProjectsTests.TheStatusTravelsAsItsGlossaryName` |
| A project or a unit needs a name of 1 to 100 characters, spaces around it trimmed | `Project.IsAcceptableName` | `ProjectsTests.AProjectOrAUnitNeedsANameOfAtMost100Characters` |
| Only an owner manages projects until UC-008 | `SessionPolicies.Owner`, the access token's `isOwner` claim | `ProjectsTests.OnlyAnOwnerManagesProjects` |
| Every session says whether its user is an owner | `AuthEndpoints`, `Session.IsOwner` | `AuthTests.ASessionSaysWhetherItsUserIsAnOwner` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Copying the structure from the default template or an earlier project (AC-05.2), and the template itself (`Projects.IsTemplate`) | UC-012, the first with categories and items to copy |
| The financial supervisors of a project, besides owners | UC-008 and UC-009 |
| Refusing new entries and approvals in a finished project | Each entry use case, through `Project.IsReadOnly` |
| The shared unit (`Units.IsShared`) | Open question 3 of the BRS |
