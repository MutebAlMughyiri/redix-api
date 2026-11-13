[Documentation](../README.md) › UC-009

# UC-009: Permissions

**What it is for:** making the server the real guard of who may do what (FR-03, FR-36, NFR-08), keeping every project separate (BR-33), with what each person holds kept in the database (BR-32).

**Who uses it:** every request passes through it. Whoever manages a project's users ticks each person's permissions in the app (UC-008).
**Issue:** redix-api #27, redesigned by redix-api #89 "No roles: each person's permissions ticked per project"; the app side is redix-app #75.

## How it works

There are no roles. A route names a permission, and a database row says who holds it: across the system in `SystemPermissions`, in one project in `ProjectPermissions`, once that project is approved (UC-066). Both are read on each request, so a change counts at the next request, with no new sign-in and no release.

| Table | Holds |
| --- | --- |
| `SystemPermissions` | What a user holds everywhere: the admin's `ApproveProjects`, made with the admin at the first start |
| `ProjectPermissions` | What a user holds in one project, one row per permission ticked |

Holding `ReadProject` is being in the project. Whoever registers a project holds every permission of a project in it, of the two capital ticks `SeeAllCapital`. `ApproveProjects` is never held in a project: the API refuses it and the database's check refuses it too.

## The permissions

| Permission | Where | What it allows |
| --- | --- | --- |
| `ApproveProjects` | The system | Seeing the registered projects, and approving or rejecting them |
| `ReadProject` | A project | Opening the project and reading everything in it |
| `ManageUsers` | A project | Adding people to the project and ticking their permissions |
| `SeeAllCapital` | A project | Seeing every partner's capital, and the report files |
| `SeeChosenCapital` | A project | Seeing only the partners chosen for one, in `ChosenPartners` (redix-api #106); it and `SeeAllCapital` exclude each other |
| `ManageProject` | A project | Its name, units and documents, and closing it |
| `ManageStructure` | A project | Categories, items, subgroups and columns |
| `ManageSettings` | A project | Payment methods, accounts, partners, suppliers and contracts |
| `AddSupplier` | A project | Adding a supplier while entering; without `ManageSettings` it waits for approval |
| `ChargeAnyAccount` | A project | Paying from any account; without it, only from those in one's custody |
| `RecordEntries` | A project | Entering purchases and contractor payments |
| `ManageAnyEntry` | A project | Changing, sending and deleting an entry someone else entered, or one waiting |
| `DeleteEntries` | A project | Deleting an entry not yet approved (UC-033) |
| `AttachFiles` | A project | Keeping a picture or a file for an entry, a receipt or a contract |
| `ApproveEntries` | A project | Approving or rejecting what waits, and reversing an approved entry |
| `MoveMoney` | A project | Capital movements, revenue, reconciling and the periodic report |

A new permission is code: a route has to name it, and a migration adds it. The database refuses a permission the code does not know.

## Routes

| Route | Needs | Answers |
| --- | --- | --- |
| `GET /projects` | Any session | The projects they hold `ReadProject` in, and those they registered that wait, each with `permissions`; those they are inactive in with `isInactive` true and no permissions (redix-api #126) |
| `GET /projects/{projectId}/users` | `ManageUsers` | `200` this project's people, each with what they hold in it |
| `POST /projects/{projectId}/users` | `ManageUsers` | `201` a new login holding `ReadProject`, the `permissions` ticked and, with `SeeChosenCapital`, the `partnerIds` chosen; `400` `invalidPermission` (also both capital ticks) or `unknownPartner` |
| `POST /projects/{projectId}/users/existing` | `ManageUsers` | `200` adds `{ username }` holding `ReadProject`; `404` `unknownUser` |
| `POST /projects/{projectId}/users/{userId}/permissions/{permission}` | `ManageUsers` | `200`; giving one capital tick takes the other, and `SeeAllCapital` forgets the chosen partners; `400` `invalidPermission`; `404` someone not in the project |
| `DELETE /projects/{projectId}/users/{userId}/permissions/{permission}` | `ManageUsers` | `200`; `ReadProject` takes every one of theirs; taking `SeeChosenCapital` forgets the chosen partners; `409` `lastManager` |
| `POST /projects/{projectId}/users/{userId}/partners/{partnerId}`, and `DELETE` on the same path | `ManageUsers` | `200` chooses a partner whose capital they see, giving `SeeChosenCapital`, or takes it (redix-api #106); `404` a person or a partner not of the project |
| `PUT /projects/{projectId}/users/{userId}/role-name` | `ManageUsers` | `200` names `{ roleName }`, a label only that decides nothing; `400` `invalidRoleName` |
| `POST /projects/{projectId}/users/{userId}/disable` and `/enable` | `ManageUsers` | `200` makes the person inactive in this project or active again (redix-api #126, UC-007); `409` `lastManager` on disable; `404` someone not in the project |
| `PUT /users/{userId}`; `POST` to its `/reset-password` | See below | `200`; `403` `otherProject` |

A permission is named in the route as the app names it, `approveEntries`, whatever its case.

A login is shared across projects, so changing one needs `ManageUsers` in every project that login is in, and a login holding a system permission can never be changed there. Otherwise one project's manager could reset a password and sign in to another project, or as the admin.

A session answers `permissions`, what the user holds everywhere.

## Moving from roles

The migration `PermissionsPerPerson` gave each person every permission their roles held in each project, and `DeleteEntries` to whoever held `RecordEntries`; the admin's role became his permission. Then it dropped `Roles`, `RolePermissions`, `UserRoles` and `ProjectMembers`. Going back cannot rebuild the roles from ticked permissions: only a backup taken before it does.

## Rules and where they are proven

| Rule | Code | Test |
| --- | --- | --- |
| The admin holds approving projects from the first start | `AdminSeed` | `PermissionsTests.TheAdminHoldsApprovingProjectsFromTheFirstStart` |
| A change in the database counts at the next request | `UserAccess` | `PermissionsTests.ANewRowGivesAPermissionAtTheNextRequest` |
| A project someone is inactive in gives him nothing, so every route of it refuses him (redix-api #126) | `UserAccess.OfAsync` | `InactiveMembersTests.R5_EveryRouteOfAProjectHeIsInactiveInRefusesHimAsForAProjectHeIsNotIn` |
| A permission held everywhere reaches every project | `UserAccess.In` | `PermissionsTests.APermissionHeldEverywhereReachesEveryProjectAndAnUnknownOneIsNotFound` |
| The database refuses an unknown permission, and the admin's in a project (BR-32) | `RedixDbContext` | `PermissionsTests.TheDatabaseRefusesAPermissionTheCodeDoesNotKnowAndTheAdminsInAProject` |
| redix-api #106: the two capital ticks exclude each other, choosing a partner gives `SeeChosenCapital`, taking it forgets the partners, and whoever registers a project sees all | `ProjectUsersEndpoints.Give`, `Choose`, `CapitalSight` | `PartnersTests.UC_106_SeeAllAndChosenCapitalExcludeEachOtherAndARegistrantSeesAll`, `UC_106_ChoosingAPartnerGivesChosenCapitalAndTakesSeeAllAndTakingChosenCapitalForgetsThePartners`, `UC_106_APersonIsCreatedWithChosenPartnersButNotWithBothCapitalTicksNorAnotherProjectsPartner`, `UC_106_ChoosingAPartnerOfAnotherProjectOrForSomeoneOutsideIsNotFound`, `ProjectUsersTests.WhoeverRegistersAProjectHoldsEveryPermissionOfAProjectInItSeeingAllCapitalRatherThanChosenPartners` |
| Every route that changes something names a policy | The routes | `PermissionsTests.EveryChangingRouteNamesAPolicy` |
| Every route of a project names a permission | The routes | `PermissionsTests.EveryRouteOfAProjectNamesAPermission` |
| Every route refuses whoever lacks its permission and opens once it is held | `PermissionHandler` | `PermissionsTests.EveryRouteRefusesWhoeverLacksItsPermissionAndOpensOnceItIsTicked` |
| Another project answers 403 (AC-36.1) | `PermissionHandler` | `ProjectsTests.AnotherProjectIsForbidden` |
| What a user may do follows what is ticked in each project (AC-36.2) | `UserAccess.In` | `ProjectsTests.PermissionsFollowWhatIsTickedInEachProject` |
| Field and Reader cannot change a project (AC-03.1) | `PermissionHandler` | `ProjectsTests.FieldAndReaderCannotChangeAProject` |
| No session answers 401 (AC-N08) | `PermissionHandler` | `ProjectsTests.ASessionlessRequestIsUnauthorized` |
| Whoever registers a project holds all 14 in it (BR-29) | `ProjectsEndpoints.AddWithManager` | `ProjectsTests.WhoeverStartsAProjectManagesIt`, `ProjectUsersTests.WhoeverRegistersAProjectHoldsEveryPermissionOfAProjectInIt` |
| Deleting is its own permission | `EntriesEndpoints` | `EntryChangesTests.DeletingIsItsOwnPermission` |

## Left to later use cases

The OWASP MASVS review in AC-N08 comes before release.
