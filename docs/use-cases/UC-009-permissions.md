[Documentation](../README.md) › UC-009

# UC-009: Permissions

**What it is for:** making the server the real guard of who may do what (FR-03, FR-36, NFR-08), keeping every project separate (BR-33), with what each person holds kept in the database (BR-32).

**Who uses it:** every request passes through it. Whoever manages a project's users ticks each person's permissions in the app (UC-008).
**Issue:** redix-api #27, redesigned by redix-api #89 "No roles: each person's permissions ticked per project"; the app side is redix-app #75.

## How it works

There are no roles. A route names a permission, and a database row says who holds it: across the system in `system_permissions`, in one project in `ProjectPermissions`, once that project is approved (UC-066). Both are read on each request, so a change counts at the next request, with no new sign-in and no release.

| Table | Holds |
| --- | --- |
| `system_permissions` | What a user holds everywhere: the admin's `ApproveProjects`, made with the admin at the first start |
| `ProjectPermissions` | What a user holds in one project, one row per permission ticked |

Holding `ReadProject` is being in the project. Whoever registers a project holds every permission of a project in it, of the two capital ticks `SeeAllCapital`. `ApproveProjects` is never held in a project: the API refuses it and the database's check refuses it too.

## The permissions

| Permission | Where | What it allows |
| --- | --- | --- |
| `ApproveProjects` | The system | Seeing the registered projects, and approving or rejecting them |
| `ReadProject` | A project | Opening the project and reading everything in it |
| `ManageUsers` | A project | Adding people to the project and ticking their permissions |
| `SeeAllCapital` | A project | Seeing every partner's capital |
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
| `MoveMoney` | A project | Capital movements, revenue and reconciling |
| `SeeReports` | A project | «يرى التقارير»: opening the reports, the periodic report, an item's report, a contract's and the VAT statement (redix-api #131) |
| `GenerateReports` | A project | «يُصدر التقارير»: issuing the periodic report and taking every report's Excel and PDF, whose capital holds only the partners one may see (redix-api #131, BR-31) |
| `SeeProjectHistory` | A project | «يرى سجل المشروع»: reading «سجل المشروع», every action in the project, each line only for whoever may see its record (UC-069) |
| `ManageProjectFiles` | A project | «ملفات المشروع»: taking the project's archive, which holds everything, every partner's capital and the history included; retrieving one over the project; loading a project file from Excel (UC-073, UC-071) |

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

.NET's migration `PermissionsPerPerson`, before the data moved to SQLite, gave each person every permission their roles held in each project, and `DeleteEntries` to whoever held `RecordEntries`; the admin's role became his permission. Then it dropped `Roles`, `RolePermissions`, `UserRoles` and `ProjectMembers`. Going back cannot rebuild the roles from ticked permissions: only a backup taken before it does.

.NET's migration `ReportPermissions` (redix-api #131 D3) gave `SeeReports` to everyone holding `ReadProject`, and `GenerateReports` to everyone holding `MoveMoney` or `SeeAllCapital`, so nobody lost a report or a file they had; the two are ticked like any other since, and someone added later holds them only once ticked. Going back takes both away.

## Rules and where they are proven

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The admin holds approving projects from the first start | `AdminSeed` | `PermissionsTests.TheAdminHoldsApprovingProjectsFromTheFirstStart` |
| A change in the database counts at the next request | `UserAccess` | `PermissionsTests.ANewRowGivesAPermissionAtTheNextRequest` |
| A project someone is inactive in gives him nothing, so every route of it refuses him (redix-api #126) | `access.Book.Of` | `InactiveMembersTests.R5_EveryRouteOfAProjectHeIsInactiveInRefusesHimAsForAProjectHeIsNotIn` |
| A permission held everywhere reaches every project | `access.Access.In` | `PermissionsTests.APermissionHeldEverywhereReachesEveryProjectAndAnUnknownOneIsNotFound` |
| The database refuses an unknown permission, and the admin's in a project (BR-32) | The `CHECK` on `project_permissions.permission`, `db/migrations/00001_schema.sql` | `PermissionsTests.TheDatabaseRefusesAPermissionTheCodeDoesNotKnowAndTheAdminsInAProject` |
| redix-api #106: the two capital ticks exclude each other, choosing a partner gives `SeeChosenCapital`, taking it forgets the partners, and whoever registers a project sees all | `users.Routes.give`, `Choose`, `CapitalSight` | `PartnersTests.UC_106_SeeAllAndChosenCapitalExcludeEachOtherAndARegistrantSeesAll`, `UC_106_ChoosingAPartnerGivesChosenCapitalAndTakesSeeAllAndTakingChosenCapitalForgetsThePartners`, `UC_106_APersonIsCreatedWithChosenPartnersButNotWithBothCapitalTicksNorAnotherProjectsPartner`, `UC_106_ChoosingAPartnerOfAnotherProjectOrForSomeoneOutsideIsNotFound`, `ProjectUsersTests.WhoeverRegistersAProjectHoldsEveryPermissionOfAProjectInItSeeingAllCapitalRatherThanChosenPartners` |
| Every route that changes something names a policy | The routes | `PermissionsTests.EveryChangingRouteNamesAPolicy` |
| Every route of a project names a permission | The routes | `PermissionsTests.EveryRouteOfAProjectNamesAPermission` |
| Every route refuses whoever lacks its permission and opens once it is held | `PermissionHandler` | `PermissionsTests.EveryRouteRefusesWhoeverLacksItsPermissionAndOpensOnceItIsTicked` |
| Another project answers 403 (AC-36.1) | `PermissionHandler` | `ProjectsTests.AnotherProjectIsForbidden` |
| What a user may do follows what is ticked in each project (AC-36.2) | `access.Access.In` | `ProjectsTests.PermissionsFollowWhatIsTickedInEachProject` |
| Field and Reader cannot change a project (AC-03.1) | `PermissionHandler` | `ProjectsTests.FieldAndReaderCannotChangeAProject` |
| No session answers 401 (AC-N08) | `PermissionHandler` | `ProjectsTests.ASessionlessRequestIsUnauthorized` |
| Whoever registers a project holds all 16 in it (BR-29) | `projects.addWithManager` | `ProjectsTests.WhoeverStartsAProjectManagesIt`, `ProjectUsersTests.WhoeverRegistersAProjectHoldsEveryPermissionOfAProjectInIt` |
| Deleting is its own permission | `entries.Routes.delete` | `EntryChangesTests.DeletingIsItsOwnPermission` |
| redix-api #131 D3: the upgrade gives `SeeReports` to everyone in a project, and `GenerateReports` to whoever moved money or saw all capital | .NET's migration `ReportPermissions` | `MigrationStepsTests.R131_D3_ReportPermissions_WhoeverReadsAProjectSeesItsReportsAndWhoeverMovedMoneyOrSawAllCapitalGeneratesThem` |

## Asking for a change (redix-api #112)

From صلاحياتي a person asks for permissions given and taken away in the project, with a reason; nothing changes until whoever holds `ManageUsers` there approves it, through the same path ticking takes, so the last manager keeps managing (BR-29). One request waits per person per project: a new one replaces it. The managers are told of a new request under the bell, the person of the answer; a rejection carries its reason. Every request, replacement, withdrawal and decision is a History line on `PermissionRequests`.

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/permission-requests/mine` | `ReadProject` | `200` the person's latest request, waiting or decided, as a list of one; empty when they never asked |
| `POST /projects/{projectId}/permission-requests` `{ gives, takes, reason }` | `ReadProject` | `201` the request, replacing the waiting one; `400` `nothingAsked`, `invalidPermission` (unknown, `ApproveProjects`, or `ReadProject`), `alreadyHeld`, `notHeld`, `reasonRequired`, `invalidReason` (past 500 letters) |
| `DELETE /projects/{projectId}/permission-requests/mine` | `ReadProject` | `200`, the waiting request withdrawn; `404` for none |
| `GET /projects/{projectId}/permission-requests` | `ManageUsers` | `200` every waiting request, oldest first, each with `displayName` |
| `POST /projects/{projectId}/permission-requests/{requestId}/approve` | `ManageUsers` | `200`: what still changes something is applied as ticking does; `409` `lastManager`; `404` for no waiting request in the project |
| `POST /projects/{projectId}/permission-requests/{requestId}/reject` `{ reason }` | `ManageUsers` | `200`; `400` `reasonRequired`, `invalidReason`; `404` |

| Rule | Proven by |
| --- | --- |
| #112 R1, R2, R5, R6: asking changes nothing, the managers are told, and the request waits on their list with the person's name | `users.TestR112_APersonAsksAndNothingChangesUntilAManagerDecides` |
| #112 R4, R6, R11: an approved request is applied as ticking is, what changes nothing is left, and the person is told | `users.TestR112_AnApprovedRequestIsAppliedAsTickingIsAndThePersonIsTold` |
| #112 R8: a rejection needs its reason, changes nothing, and the person reads it | `users.TestR112_ARejectionCarriesItsReasonAndChangesNothing` |
| #112 R7: a new request replaces the waiting one, and one is withdrawn | `users.TestR112_ANewRequestReplacesTheWaitingOneAndOneIsWithdrawn` |
| #112 R10: a request asks only for what changes something, with a reason | `users.TestR112_ARequestAsksOnlyForWhatChangesSomething` |
| #112 R4, BR-29: the last manager keeps managing users whatever they ask | `users.TestR112_TheLastManagerKeepsManagingUsersWhateverTheyAsk` |

## Left to later use cases

The OWASP MASVS review in AC-N08 comes before release.
