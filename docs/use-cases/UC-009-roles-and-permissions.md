[Documentation](../README.md) › UC-009

# UC-009: Roles and permissions

**What it is for:** making the server the real guard of who may do what (FR-03, FR-36, NFR-08), keeping every project separate (BR-33), with every role kept in the database (BR-32).

**Who uses it:** every request passes through it. A project manager gives roles in the app; what a role may do is changed in the database.
**Issue:** redix-api #27; the app side is redix-app #75.

## How it works

A route names a permission. A database row says which role holds it. A role counts only in the project it was given for. Nothing in the code tells one role from another.

A user holds a permission everywhere through `UserRoles`, and in one project through `ProjectMembers`. Both are read on each request, so a change counts at the next request, with no new sign-in and no release.

| Table | Holds |
| --- | --- |
| `Roles` | Each role: the key of its word, `System` or `Project`, and its place in the list |
| `RolePermissions` | Which permission each role holds |
| `UserRoles` | A user's system-wide roles |
| `ProjectMembers` | A user's roles in one project, one row per role |

## The roles as they start

| Permission | System admin | Project manager | Finance | Field | Reader |
| --- | --- | --- | --- | --- | --- |
| `StartProject` | Yes | | | | |
| `ManageLogins` | Yes | | | | |
| `ManageUsers` | | Yes | | | |
| `ReadProject` | | Yes | Yes | Yes | Yes |
| `SeeAllCapital` | | Yes | Yes | | |
| `ManageProject` | | | Yes | | |

## Changing a role

```sql
-- Let readers see every partner's capital. Counts at each reader's next request.
INSERT INTO RolePermissions (Id, RoleId, Permission)
SELECT NEWID(), Id, 'SeeAllCapital' FROM Roles WHERE WordKey = 'users.roleReader';
```

To add a role, add its word to `TranslationKeys` and `Translations`, then its row in `Roles`, then its rows in `RolePermissions`. The app offers it at its next read. A new person starts at the last project role in the list.

A new permission is code: a route has to name it, and a migration adds it. The database refuses a permission the code does not know, a role with no word, and deleting a role in use.

## Routes

| Route | Needs | Answers |
| --- | --- | --- |
| `GET /projects` | Any session | The projects they hold `ReadProject` in, with `roles` and `permissions` |
| `POST /projects` | `StartProject` | `201` the project; the caller becomes its first manager |
| `GET /projects/{projectId}/roles` | `ManageUsers` | `200` the project roles, in order |
| `GET /projects/{projectId}/users` | `ManageUsers` | `200` this project's people and their roles |
| `POST /projects/{projectId}/users` | `ManageUsers` | `201` a new login, as the last role |
| `POST /projects/{projectId}/users/existing` | `ManageUsers` | `200` adds `{ username }`, as the last role; `404` `unknownUser` |
| `POST /projects/{projectId}/users/{userId}/roles/{roleId}` | `ManageUsers` | `200`; `400` `invalidRole` |
| `POST /projects/{projectId}/users/{userId}/roles/{roleId}/remove` | `ManageUsers` | `200`; `409` `lastManager` |
| `GET /users` | `ManageLogins` | `200` every login |
| `POST /users/{userId}`, `/reset-password`, `/disable`, `/enable` | See below | `200`; `403` `otherProject`; `409` `lastManager` on disable |

A login is shared across projects, so changing one needs `ManageLogins`, or `ManageUsers` in every project that login is in. Otherwise one project's manager could reset a password and sign in to another project.

A session answers `permissions`, what the user holds everywhere.

## Rules and where they are proven

| Rule | Code | Test |
| --- | --- | --- |
| The roles hold what BRS section 2 says | `RoleSeed` | `RolesTests.TheRolesHoldWhatBrsSectionTwoSays` |
| A change in the database counts at the next request | `UserAccess` | `RolesTests.ANewRowGivesARoleAPermissionAtTheNextRequest` |
| A new role in the database is offered and works | `ProjectUsersEndpoints` | `RolesTests.ANewRoleInTheDatabaseIsOfferedAndWorks` |
| The database refuses a broken configuration (BR-32) | `RedixDbContext` | `RolesTests.TheDatabaseRefuses*` |
| Every route that changes something names a policy | The routes | `RolesTests.EveryChangingRouteNamesAPolicy` |
| Another project answers 403 (AC-36.1) | `PermissionHandler` | `ProjectsTests.AnotherProjectIsForbidden` |
| What a user may do follows the project (AC-36.2) | `UserAccess.In` | `ProjectsTests.PermissionsFollowTheRoleInEachProject` |
| Field and Reader cannot change a project (AC-03.1) | `PermissionHandler` | `ProjectsTests.FieldAndReaderCannotChangeAProject` |
| No session answers 401 (AC-N08) | `PermissionHandler` | `ProjectsTests.ASessionlessRequestIsUnauthorized` |
| Whoever starts a project manages it (BR-29) | `ProjectsEndpoints.Create` | `ProjectsTests.WhoeverStartsAProjectManagesIt` |
| A manager changes project data only with that role too (BR-29) | `RoleSeed` | `ProjectsTests.AManagerChangesAProjectOnlyWithFinanceToo` |
| A manager reaches only their project (BR-33) | `PermissionHandler` | `ProjectUsersTests.AManagerCannotReachAnotherProject` |
| A manager cannot change a login in another project (BR-33) | `UsersEndpoints` | `ProjectUsersTests.AManagerCannotResetSomeoneInAnotherProject` |
| At least one active manager stays (BR-29) | `ProjectUsersEndpoints` | `ProjectUsersTests.TheLastManagerCannotLoseTheRole`, `TheLastManagerCannotBeDisabled` |
| Each role given or taken is in the audit trail | `db.Audit` | `ProjectUsersTests.EveryRoleChangeIsInTheAuditTrail` |

## Left to later use cases

Each adds its own permission and rows: entering expenses, approving, accounts, capital, revenue, exports and suppliers. Opening the project list right after sign-in is UC-010. Showing capital by `SeeAllCapital` is UC-044. The OWASP MASVS review in AC-N08 comes before release.
