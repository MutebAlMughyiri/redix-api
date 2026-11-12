[Documentation](../README.md) › UC-008

# UC-008: Add people to a project and tick their permissions

**What it is for:** deciding who works on each project, and what each may do there (FR-36).
**Who uses it:** whoever manages a project's users, for that project alone (BR-33). What each permission allows is UC-009.
**Issue:** redix-api #26, redesigned by redix-api #89 "No roles: each person's permissions ticked per project"; the app side is redix-app #28.

## Endpoints

All need `ManageUsers` in the project, and answer `403` to anyone else.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/users` | This project's people, active users first, by name, with only what they hold in this project | `200` the users |
| `POST /projects/{projectId}/users` | Creates a login in this project holding `ReadProject` and the `permissions` ticked | `201` the user; `400` `invalidName`, `invalidUsername`, `invalidMobile`, `weakPassword` or `invalidPermission`; `409` `usernameTaken` |
| `POST /projects/{projectId}/users/existing` with `{ username }` | Adds someone who already has a login, holding `ReadProject` | `200` the user; `404` `unknownUser` |
| `POST /projects/{projectId}/users/{userId}/permissions/{permission}` | Ticks one permission | `200` the user; `400` `invalidPermission`; `404` |
| `DELETE /projects/{projectId}/users/{userId}/permissions/{permission}` | Unticks one; `readProject` takes them out of the project | `200` the user; `409` `lastManager`; `404` |
| `PUT /projects/{projectId}/users/{userId}/role-name` with `{ roleName }` | Names what the person is called in this project, free text shown under the project's name on their home; empty clears it, and taking them out of the project clears it too (Muteb, 9 Nov 2026) | `200` the user; `400` `invalidRoleName` past 50 characters; `404` someone not in the project |
| `GET /projects` | Open to every session: the projects they hold `ReadProject` in, each with `permissions` | `200` the projects |

A user's answer has `projects: [{ projectId, name, permissions }]`, one line per project.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A user sees only their projects (AC-36.1) | `ProjectsEndpoints.List` | `ProjectsTests.AUserSeesOnlyTheirProjects` |
| Each project carries what the user holds in it (AC-36.2) | `ProjectsEndpoints.List` | `ProjectsTests.PermissionsFollowWhatIsTickedInEachProject` |
| A manager sees only their project's people, and none of those people's other projects (BR-33) | `ProjectUsersEndpoints.List` | `ProjectUsersTests.AManagerSeesOnlyTheirProjectsPeopleAndNoneOfTheirOtherProjects` |
| A new login holds what was ticked and `ReadProject`, never the admin's permission | `ProjectUsersEndpoints.Create` | `ProjectUsersTests.AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword`, `ANewLoginCannotBeGivenTheAdminsPermission` |
| An existing user is added by their exact username, and an unknown one says so | `ProjectUsersEndpoints.AddExisting` | `ProjectUsersTests.AnExistingUserIsAddedByUsernameSeeingTheProject`, `AnUnknownUsernameSaysSo` |
| A tick gives one permission, once; unticking takes it | The unique index, `ProjectUsersEndpoints.Give`, `Take` | `ProjectUsersTests.TickingGivesOnePermissionAndUntickingTakesOne` |
| Unticking `ReadProject` takes them out of the project | `ProjectUsersEndpoints.Take` | `ProjectUsersTests.UntickingSeeTheProjectTakesThemOutOfTheProject` |
| The admin's permission, or an unknown name, is refused | `ProjectPermission.TryOf` | `ProjectUsersTests.TheAdminsPermissionOrAnUnknownNameIsRefused` |
| A project keeps at least one active person managing its users (BR-29) | `ProjectManagers` | `ProjectUsersTests.TheLastWhoManagesUsersCannotLoseIt`, `WithASecondManagerTheFirstCanLeave`, `ADisabledManagerDoesNotCountAsTheOneWhoStays` |
| Every permission ticked or unticked is in the History table | `db.AddHistory` | `ProjectUsersTests.EveryPermissionChangeIsInTheHistory` |

## Left to later use cases

Opening the project list right after sign-in, and the default project (UC-010).
