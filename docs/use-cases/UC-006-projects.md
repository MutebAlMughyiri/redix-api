[Documentation](../README.md) › UC-006

# UC-006: Manage projects and units

**What it is for:** starting a project with its units, correcting their names, removing a unit added by mistake, and freezing the project when it is finished (FR-04, BR-38).
**Who uses it:** whoever registers a project becomes its first project manager (UC-066); whoever holds `ManageProject` in it, which Finance does, renames it, adds, renames and deletes units, and closes it (UC-009).
**Issue:** redix-api #24; the app side is redix-app #26.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects` | The caller's projects with their units, running ones first, then the newest (UC-008); each says `isInactive`, true for one he is inactive in, which holds no permissions (redix-api #126), and `holdsEntries`, true once it holds an entry, so no project file may be loaded into it (UC-071, redix-api #132 D15) | `200` the projects |
| `POST /projects` | Registers a project from a name, a start date, its units and its `partners` (names, redix-api #109, each a table of its رأس المال once approved), all or nothing, with the caller as its first manager, waiting for approval. `structureSourceId`, left out for the default template, names an earlier project to copy the structure from (UC-012) | `201` the project; `400` `invalidName` or `unknownSource`; `400` for a missing field |
| `POST /projects/{projectId}/units` | Adds a unit at the end of the project's list | `200` the project; `400` `invalidName`; `409` `projectFinished` or `unitNameTaken`; `403` without `ManageProject` in it |
| `PUT /projects/{projectId}` with `{ name }` | Renames the project while it is open (BR-38) | `200` the project; `400` `invalidName`; `409` `projectFinished`; `403` without `ManageProject` in it |
| `PUT /projects/{projectId}/description` with `{ description }` | Keeps the project's description while it is open, trimmed; empty clears it (redix-api 93) | `200` the project; `400` `invalidDescription` over 2,000 characters; `409` `projectFinished`; `403` without `ManageProject` in it |
| `PUT /projects/{projectId}/time-zone` with `{ timeZone }` | Puts the project in a zone of the list, the one its entries' today is counted in (redix-app #107) | `200` the project; `400` `unknownTimeZone` for one not on the list; `409` `projectFinished`; `403` without `ManageProject` in it |
| `GET /time-zones` | The zones a project may be in, Asia/Riyadh first, each with the word of its city and how far it is ahead of UTC now | `200` |
| `PUT /projects/{projectId}/units/{unitId}` with `{ name }` | Renames a unit while the project is open (BR-38) | `200` the project; `400` `invalidName`; `409` `projectFinished`, `unitNameTaken` or `sharedUnit`; `404` outside the project; `403` as above |
| `DELETE /projects/{projectId}/units/{unitId}` | Deletes a unit while the project is open: nothing spends against a unit, since expenses and contracts belong to the project (BR-38, redix-api #96) | `200` the project; `409` `projectFinished`; `404` outside the project |
| `POST /projects/{projectId}/close` | Closes a finished project | `200` the project; `409` `projectFinished` when already closed, or `pendingEntries` while entries wait for approval; `403` without `ManageProject` in it |

A project answers as `{ id, name, startDate, status, rejectionReason, description, units: [{ id, name }], permissions, timeZone, utcOffsetMinutes }`, with `status` `pendingApproval`, `rejected`, `active` or `finished`.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A project is in Asia/Riyadh until its manager puts it in another zone of the list; its entries' today is counted there, every moment staying UTC; a change is a History line, written once (redix-app #107, D1, D3) | `projects.Routes.setTimeZone`, `zones.Of` | `projects_test.TestR4_107_AManagerPutsTheProjectInAZoneOfTheList`, `TestR4_107_OnlyAZoneOfTheListOnlyByItsManagerOnlyWhileOpen`, `TestR4_107_TheZonesOfferedComeWithTheirCityAndOffset` |
| A project's description is kept for everyone in it, trimmed, and in the History table; an empty one clears it (redix-api 93) | `projects.Routes.describe` | `ProjectsTests.More_info_ADescriptionIsKeptForEveryoneInTheProjectAndInTheHistory`, `More_info_AnEmptyDescriptionClearsIt` |
| A description has at most 2,000 characters, when written and when the project is made or registered | `projects.Routes.describe` | `ProjectsTests.More_info_ADescriptionOf2000CharactersIsKeptAndOneLongerIsRefused`, `More_info_AProjectMadeWithADescriptionTooLongIsNotMade`; `RegistrationTests.More_info_ARegistrationWithADescriptionTooLongCreatesNothing` |
| Only `ManageProject` describes a project, and only while it is open (AC-04.1) | `access.ManageProject`, `projects.Routes.describe` | `ProjectsTests.More_info_OnlyWhoeverManagesTheProjectDescribesIt`, `More_info_AFinishedProjectKeepsItsDescription` |
| A description can be written when a project is made, and the admin reads it before deciding | `CreateProjectRequest`, `RegisterProjectRequest`, `RegisteredProjectView` | `ProjectsTests.More_info_ADescriptionWrittenWhenTheProjectIsMadeIsKept`; `RegistrationTests.More_info_TheAdminReadsTheDescriptionBeforeDeciding` |
| A project keeps its units, in the order given | `projects.newProject`, `projects.Routes.addUnit` | `ProjectsTests.AProjectKeepsItsUnitsInOrder` |
| A bad unit name refuses the whole project and saves nothing | `projects.Routes.create` | `ProjectsTests.ABadUnitNameCreatesNothing` |
| A request missing a field answers 400, in every environment | `web.Read` | `ProjectsTests.ARequestMissingAFieldIsABadRequest` |
| A unit is added to a running project at the end of its list | `projects.Routes.addUnit` | `ProjectsTests.AUnitIsAddedAtTheEnd` |
| A finished project is read-only and stays listed, so its reports stay available (AC-04.1) | `access.Gate.Changes`, `projects.Routes.close` | `ProjectsTests.AFinishedProjectIsReadOnlyAndStaysListed` |
| A project and a unit are renamed while the project is open; a rename takes no other unit's name and may keep its own; the History table shows old and new (BR-38, BR-37) | `projects.Routes.rename`, `projects.Routes.renameUnit`, `history.Changed` | `ProjectsTests.BR_38_ARunningProjectIsRenamedAndTheHistoryShowsBothNames`, `BR_38_AUnitIsRenamedInPlaceAndTheHistoryShowsBothNames`, `BR_38_AProjectOrAUnitIsRenamedOnlyToAnAcceptableName`, `BR_37_ARenameCannotTakeAnotherUnitsNameAndMayKeepItsOwn` |
| A unit is deleted whatever expenses and contracts hold, since they belong to the project; the History table keeps what it was (BR-38) | `projects.Routes.deleteUnit` | `BR_38_AUnitNothingUsesIsDeletedAndTheHistoryKeepsWhatItWas`, `BR_38_ExpensesAndContractsBelongToTheProjectSoAUnitIsDeletedWhateverTheyHold` |
| A unit added after a deletion still goes last | `projects.Routes.addUnit` | `AUnitAddedAfterADeletionStillGoesLast` |
| A finished project refuses them all, a unit of another project is 404, and only whoever holds `ManageProject` does them | `access.Gate.Changes`, `projects.Map` | `BR_38_AFinishedProjectRefusesRenamesAndDeletions`, `BR_33_AUnitOfAnotherProjectIsNotFound`, `BR_38_OnlyWhoeverManagesTheProjectRenamesAndDeletes` |
| A project with entries waiting for approval cannot be closed; drafts and decided entries hold nothing open, and another project's entries nothing here (BR-35) | `projects.Routes.close` | `ProjectsTests.BR_35_AProjectWithEntriesWaitingForApprovalCannotBeClosed`, `BR_35_OnlyEntriesWaitingForApprovalHoldAProjectOpen`, `BR_35_AnEntryWaitingInAnotherProjectHoldsNothingOpen` |
| The status travels by its glossary name, `active` or `finished` | `ConfigureHttpJsonOptions`, `ProjectStatus` | `ProjectsTests.TheStatusTravelsAsItsGlossaryName` |
| A unit's name cannot repeat inside its project, compared trimmed; projects may repeat a name, because each is separate (BR-37, BR-33) | `projects.Routes.addUnit` | `ProjectsTests.BR_37_AUnitsNameCannotRepeatInItsProjectComparedTrimmed`, `BR_37_ANewProjectCannotRepeatAUnitsNameAndSavesNothing`, `BR_37_ProjectsMayRepeatANameAndEachTheirUnitsNames`; `RegistrationTests.BR_37_ARegistrationCannotRepeatAUnitsName` |
| A project or a unit needs a name of 1 to 100 characters, spaces around it trimmed | `projects.newProject` | `ProjectsTests.AProjectOrAUnitNeedsANameOfAtMost100Characters` |
| Whoever registers a project manages it | `projects.addWithManager` | `ProjectsTests.WhoeverStartsAProjectManagesIt` |
| Creating a project, adding a unit and closing a project are in the History table | `projects.addWithManager`, `projects.Routes.change`, `history.Changed` | `ProjectsTests.CreatingAProjectIsInTheHistoryWithItsName`, `AddingAUnitIsInTheHistoryWithItsName`, `ClosingAProjectIsInTheHistoryWithItsStatusOldAndNew` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Refusing new entries and approvals in a finished project | Each entry use case, through `Project.IsReadOnly` |
