[Documentation](../README.md) › UC-008

# UC-008: Assign users to projects with a role

**What it is for:** deciding who works on which project, and as what (FR-36).
**Who uses it:** owners. What each role may then do is UC-009.
**Issue:** redix-api #26; the app side is redix-app #28.

## Endpoints

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `POST /users/{userId}/projects/{projectId}` with `{ role }` | Gives the project, or changes the role | `200` the user; `404`; `409` `ownerIsInEveryProject` |
| `POST /users/{userId}/projects/{projectId}/remove` | Takes the project away | `200` the user; `404` |
| `GET /projects` | Now open to every session: an owner gets every project as `financeSupervisor`; others only theirs, each with its role | `200` the projects |

A user's answer gains `projects: [{ projectId, name, role }]`, and a project's answer gains `role`. `role` is `financeSupervisor`, `fieldSupervisor` or `reader`.

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A user sees only their projects (AC-36.1) | `ProjectsEndpoints.List` | `ProjectsTests.AUserSeesOnlyTheirProjects` |
| Each project carries the role in it (AC-36.2) | `ProjectsEndpoints.List` | `ProjectsTests.EachProjectCarriesTheRoleInIt` |
| A project can be given with each role | `UsersEndpoints.AssignProject`, `ProjectRole` | `UsersTests.AnOwnerGivesAProjectWithEachRole` |
| Assigning twice changes the role, never adds a second row | The unique index, `UsersEndpoints.AssignProject` | `UsersTests.AssigningAgainChangesTheRole` |
| Removing takes the project out of the user's list | `UsersEndpoints.RemoveProject` | `UsersTests.RemovingTakesTheProjectAway` |
| An owner is already in every project (BR-29) | `UsersEndpoints.AssignProject` | `UsersTests.AnOwnerIsAlreadyInEveryProject` |
| Only an owner assigns | `SessionPolicies.Owner` | `UsersTests.OnlyAnOwnerAssigns` |
| Every assignment, change and removal is in the audit trail | `db.Audit` | `UsersTests.EveryAssignmentIsInTheAuditTrail` |

## Left to later use cases

Refusing what a role may not do (UC-009), and the default project (UC-010).
