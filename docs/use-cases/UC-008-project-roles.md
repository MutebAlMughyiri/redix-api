[Documentation](../README.md) › UC-008

# UC-008: Add people to a project and give their roles

**What it is for:** deciding who works on each project, and as what (FR-36).
**Who uses it:** each project's manager, for that project alone (BR-33). What each role may then do is UC-009.
**Issue:** redix-api #26; the app side is redix-app #28.

## Endpoints

All need `ManageUsers` in the project, and answer `403` to anyone else.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/roles` | The roles the project can give, in order | `200` `[{ id, wordKey }]` |
| `GET /projects/{projectId}/users` | This project's people, active users first, by name, with only this project's roles | `200` the users |
| `POST /projects/{projectId}/users` | Creates a login in this project, as the last role | `201` the user; `400` `invalidName`, `invalidUsername`, `invalidMobile` or `weakPassword`; `409` `usernameTaken` |
| `POST /projects/{projectId}/users/existing` with `{ username }` | Adds someone who already has a login, as the last role | `200` the user; `404` `unknownUser` |
| `POST /projects/{projectId}/users/{userId}/roles/{roleId}` | Gives a role | `200` the user; `400` `invalidRole`; `404` |
| `POST /projects/{projectId}/users/{userId}/roles/{roleId}/remove` | Takes a role; the last takes them out of the project | `200` the user; `409` `lastManager`; `404` |
| `GET /projects` | Open to every session: the projects they hold `ReadProject` in, each with `roles` and `permissions` | `200` the projects |

A user's answer has `projects: [{ projectId, name, role: { id, wordKey } }]`, one line per role.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A user sees only their projects (AC-36.1) | `ProjectsEndpoints.List` | `ProjectsTests.AUserSeesOnlyTheirProjects` |
| Each project carries the roles and permissions in it (AC-36.2) | `ProjectsEndpoints.List` | `ProjectsTests.PermissionsFollowTheRoleInEachProject` |
| A manager sees only their project's people, and none of those people's other projects (BR-33) | `ProjectUsersEndpoints.List` | `ProjectUsersTests.AManagerSeesOnlyTheirProjectsPeopleAndNoneOfTheirOtherProjects` |
| An existing user is added by their exact username, and an unknown one says so | `ProjectUsersEndpoints.AddExisting` | `ProjectUsersTests.AnExistingUserIsAddedByUsernameAsTheLastRole`, `AnUnknownUsernameSaysSo` |
| Someone can hold several roles in a project | The unique index, `ProjectUsersEndpoints.GiveRole` | `ProjectUsersTests.ThePeopleOfAProjectCanHoldSeveralRolesInIt` |
| Taking the last role takes them out of the project | `ProjectUsersEndpoints.TakeRole` | `ProjectUsersTests.TakingTheLastRoleTakesThemOutOfTheProject` |
| A role of the wrong scope, or an unknown one, is refused | `ProjectUsersEndpoints.GiveRole` | `ProjectUsersTests.ARoleOfTheWrongScopeOrAnUnknownOneIsRefused` |
| A project keeps at least one active manager (BR-29) | `ProjectManagers` | `ProjectUsersTests.TheLastManagerCannotLoseTheRole`, `WithASecondManagerTheFirstCanLeave`, `ADisabledManagerDoesNotCountAsTheOneWhoStays` |
| Every role given or taken is in the audit trail | `db.Audit` | `ProjectUsersTests.EveryRoleChangeIsInTheAuditTrail` |

## Left to later use cases

Opening the project list right after sign-in, and the default project (UC-010).
