[Documentation](../README.md) › [Use cases](../README.md) › UC-064

# UC-064: Store engineering blueprints for a project or unit

**Purpose:** keep a project's engineering blueprints, for the whole project or for one of its units, and hand them to everyone who works on it.

**Who uses it:** whoever holds `ManageProject` keeps and removes them; everyone who reads the project opens them.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/blueprints` | The blueprints: the whole project's first, then each unit's in the units' order, newest first within each |
| `POST /projects/{id}/blueprints` | Keeps a file of the attachment store as a blueprint of the project, or of one of its own units |
| `POST /projects/{id}/blueprints/{blueprintId}/delete` | Takes a blueprint off the list; its file stays |

A blueprint is a title over a file of the attachment store, so it is a PDF or a picture under 1 MB, kept outside the database, and opened through `GET /projects/{id}/attachments/{attachmentId}` like any other file. A unit a blueprint names cannot be deleted (`unitInUse`).

These answers were decided without Muteb, so he can reverse any: PDF and pictures only, since drawing files such as DWG have no viewer on the phone; many blueprints per place, each titled, with no version history; a removed one leaves only the list, since no file is ever deleted (NFR-11); the manager keeps them, everyone reads them.

| Rule | Code | Test |
| --- | --- | --- |
| The manager keeps blueprints; everyone reads them, the project's first | `BlueprintsEndpoints.Add`, `List` | `BlueprintsTests.UC_064_TheManagerKeepsBlueprintsAndEveryoneReadsThemTheProjectsFirst` |
| A blueprint needs a title, a file of the project, and one of its own units | `BlueprintsEndpoints.Add` | `BlueprintsTests.ABlueprintNeedsATitleAFileOfTheProjectAndOneOfItsOwnUnits` |
| A removed blueprint leaves the list; its file stays, and its unit cannot be deleted | `BlueprintsEndpoints.Remove`, `ProjectsEndpoints.DeleteUnit` | `BlueprintsTests.ARemovedBlueprintLeavesTheListButItsFileStaysAndItsUnitCannotBeDeleted` |
