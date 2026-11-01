[Documentation](../README.md) › UC-006

# UC-006: Manage projects and units

**What it is for:** starting a project with its units, and freezing it when it is finished (FR-04).
**Who uses it:** the financial supervisor. Until project roles arrive with UC-008, that means an owner, who is a financial supervisor in every project (BR-29).
**Issue:** redix-api #24; the app side is redix-app #26.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects` | Every project with its units, running ones first, then the newest | `200` the projects; `403` for a user who is not an owner |
| `POST /projects` | Creates a running project from a name and a start date | `201` the project; `400` `invalidName` |
| `POST /projects/{projectId}/units` | Adds a unit at the end of the project's list | `200` the project; `400` `invalidName`; `409` `projectClosed`; `404` for an unknown project |
| `POST /projects/{projectId}/close` | Closes a finished project | `200` the project; `409` `projectClosed` when already closed; `404` for an unknown project |

A project answers as `{ id, name, startDate, status, units: [{ id, name }] }`, with `status` `running` or `closed`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| An owner creates a project and adds its units, kept in the order added | `Project.AddUnit` | `ProjectsTests.AnOwnerCreatesAProjectAndAddsItsUnitsInOrder` |
| A closed project is read-only and stays listed, so its reports stay available (AC-04.1) | `Project.IsReadOnly`, `Project.Close` | `ProjectsTests.AClosedProjectIsReadOnlyAndStaysListed` |
| A project or a unit needs a name of 1 to 100 characters, spaces around it trimmed | `Project.IsAcceptableName` | `ProjectsTests.AProjectOrAUnitNeedsANameOfAtMost100Characters` |
| Only an owner manages projects until UC-008 | `SessionPolicies.Owner`, the access token's `isOwner` claim | `ProjectsTests.OnlyAnOwnerManagesProjects` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Copying the structure from the default template or an earlier project (AC-05.2), and the template itself (`Projects.IsTemplate`) | UC-012, the first with categories and items to copy |
| The financial supervisors of a project, besides owners | UC-008 and UC-009 |
| Refusing new entries and approvals in a closed project | Each entry use case, through `Project.IsReadOnly` |
| The shared unit (`Units.IsShared`) | Open question 3 of the BRS |
