[Documentation](../README.md) › UC-006

# UC-006: Manage projects and units

**What it is for:** starting a project with its units, and freezing it when it is finished (FR-04).
**Who uses it:** whoever registers a project becomes its first project manager (UC-066); its financial supervisors add units and close it (UC-009).
**Issue:** redix-api #24; the app side is redix-app #26.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects` | The caller's projects with their units, running ones first, then the newest (UC-008) | `200` the projects |
| `POST /projects` | Registers a project from a name, a start date and its units, all or nothing, with the caller as its first manager, waiting for approval. `structureSourceId`, left out for the default template, names an earlier project to copy the structure from (UC-012) | `201` the project; `400` `invalidName` or `unknownSource`; `400` for a missing field |
| `POST /projects/{projectId}/units` | Adds a unit at the end of the project's list | `200` the project; `400` `invalidName`; `409` `projectFinished` or `unitNameTaken`; `403` without `ManageProject` in it |
| `POST /projects/{projectId}/close` | Closes a finished project | `200` the project; `409` `projectFinished` when already closed, or `pendingEntries` while entries wait for approval; `403` without `ManageProject` in it |

A project answers as `{ id, name, startDate, status, rejectionReason, units: [{ id, name }], roles, permissions }`, with `status` `pendingApproval`, `rejected`, `running` or `finished`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A project keeps its units, in the order given | `Project.New`, `Project.AddUnit` | `ProjectsTests.AProjectKeepsItsUnitsInOrder` |
| A bad unit name refuses the whole project and saves nothing | `ProjectsEndpoints.Create` | `ProjectsTests.ABadUnitNameCreatesNothing` |
| A request missing a field answers 400, in every environment | `ConfigureHttpJsonOptions` and `RouteHandlerOptions` in `Program.cs` | `ProjectsTests.ARequestMissingAFieldIsABadRequest` |
| A unit is added to a running project at the end of its list | `Project.AddUnit` | `ProjectsTests.AUnitIsAddedAtTheEnd` |
| A finished project is read-only and stays listed, so its reports stay available (AC-04.1) | `Project.IsReadOnly`, `Project.Close` | `ProjectsTests.AFinishedProjectIsReadOnlyAndStaysListed` |
| A project with entries waiting for approval cannot be closed; drafts and decided entries hold nothing open, and another project's entries nothing here (BR-35) | `Project.Close`, `ProjectsEndpoints.Close` | `ProjectsTests.BR_35_AProjectWithEntriesWaitingForApprovalCannotBeClosed`, `BR_35_OnlyEntriesWaitingForApprovalHoldAProjectOpen`, `BR_35_AnEntryWaitingInAnotherProjectHoldsNothingOpen` |
| The status travels by its glossary name, `running` or `finished` | `ConfigureHttpJsonOptions`, `ProjectStatus` | `ProjectsTests.TheStatusTravelsAsItsGlossaryName` |
| A unit's name cannot repeat inside its project, compared trimmed, the shared unit's included; projects may repeat a name, because each is separate (BR-37, BR-33) | `Project.AddUnit` | `ProjectsTests.BR_37_AUnitsNameCannotRepeatInItsProjectComparedTrimmed`, `BR_37_NoUnitTakesTheSharedUnitsName`, `BR_37_ANewProjectCannotRepeatAUnitsNameAndSavesNothing`, `BR_37_ProjectsMayRepeatANameAndEachTheirUnitsNames`; `RegistrationTests.BR_37_ARegistrationCannotRepeatAUnitsName` |
| A project or a unit needs a name of 1 to 100 characters, spaces around it trimmed | `Project.IsAcceptableName` | `ProjectsTests.AProjectOrAUnitNeedsANameOfAtMost100Characters` |
| Whoever registers a project manages it | `ProjectsEndpoints.AddWithManagerAsync` | `ProjectsTests.WhoeverStartsAProjectManagesIt` |
| Creating a project, adding a unit and closing a project are in the audit trail | `ProjectsEndpoints.AddWithManagerAsync`, `ProjectsEndpoints.ChangeAsync`, `RedixDbContext.AuditChanges` | `ProjectsTests.CreatingAProjectIsInTheAuditTrailWithItsName`, `AddingAUnitIsInTheAuditTrailWithItsName`, `ClosingAProjectIsInTheAuditTrailWithItsStatusOldAndNew` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Refusing new entries and approvals in a finished project | Each entry use case, through `Project.IsReadOnly` |
| The shared unit (`Units.IsShared`) | Open question 3 of the BRS |
