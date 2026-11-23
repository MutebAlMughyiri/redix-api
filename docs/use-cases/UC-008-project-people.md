[Documentation](../README.md) › UC-008

# UC-008: Add people to a project and tick their permissions

**What it is for:** deciding who works on each project, and what each may do there (FR-36).
**Who uses it:** whoever manages a project's users, for that project alone (BR-33). What each permission allows is UC-009.
**Issue:** redix-api #26, redesigned by redix-api #89 "No roles: each person's permissions ticked per project"; the app side is redix-app #28.

## Endpoints

All need `ManageUsers` in the project, and answer `403` to anyone else.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/users` | This project's people, those active there first (redix-api #126), by name, with only what they hold in this project and when each was last seen, `lastSeenAt` (redix-api #182) | `200` the users |
| `POST /projects/{projectId}/users` | Creates a login in this project holding `ReadProject` and the `permissions` ticked, with the `email` the manager typed, required (redix-api #127) | `201` the user; `400` `invalidName`, `invalidUsername`, `invalidMobile`, `emailInvalid`, `weakPassword` or `invalidPermission`; `409` `usernameTaken` or `emailTaken` |
| `POST /projects/{projectId}/users/existing` with `{ username }` | Adds someone who already has a login, holding `ReadProject` | `200` the user; `404` `unknownUser` |
| `POST /projects/{projectId}/users/{userId}/permissions/{permission}` | Ticks one permission | `200` the user; `400` `invalidPermission`; `404` |
| `DELETE /projects/{projectId}/users/{userId}/permissions/{permission}` | Unticks one; `readProject` takes them out of the project | `200` the user; `409` `lastManager`; `404` |
| `PUT /projects/{projectId}/users/{userId}/role-name` with `{ roleName }` | Names what the person is called in this project, free text shown under the project's name on their home; empty clears it, and taking them out of the project clears it too (the owner, 9 Nov 2026) | `200` the user; `400` `invalidRoleName` past 50 characters; `404` someone not in the project |
| `GET /projects` | Open to every session: the projects they hold `ReadProject` in, each with `permissions`, and those they are inactive in with `isInactive` true and no permissions (redix-api #126) | `200` the projects |

A user's answer has `projects: [{ projectId, permissions, roleName, partnerIds, isInactive }]`, one line per project. Making a person inactive in the project and active again is UC-007.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A user sees only their projects (AC-36.1) | `projects.List` | `ProjectsTests.AUserSeesOnlyTheirProjects` |
| Adding a person needs a well-formed email no other account holds, whatever its case, and a refusal creates nothing (AC-07.8, D1, R9) | `users.RefusalOfEmail`, `users.Routes.create`, the unique index on | `ProjectUsersTests.AC_07_8_AddingAPersonKeepsTheEmailTheManagerTyped`, `AC_07_8_D1_AddingAPersonNeedsAWellFormedEmailAndCreatesNothingWithout`, `AC_07_8_R9_AnEmailAnotherAccountHoldsIsRefusedWhateverItsCase` |
| The email is checked after the other fields, in the order the form shows them, so an empty form names the username first | `users.Routes.create` | `ProjectUsersTests.AC_07_8_AnEmptyFormNamesTheUsernameFirstAndTheEmailOnlyAfterTheOtherFields_AsTheFormOrdersThem` |
| Each project carries what the user holds in it (AC-36.2) | `projects.List` | `ProjectsTests.PermissionsFollowWhatIsTickedInEachProject` |
| A manager sees only their project's people, and none of those people's other projects (BR-33) | `users.Routes.list` | `ProjectUsersTests.AManagerSeesOnlyTheirProjectsPeopleAndNoneOfTheirOtherProjects` |
| A new login holds what was ticked and `ReadProject`, never the admin's permission | `users.Routes.create` | `ProjectUsersTests.AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword`, `ANewLoginCannotBeGivenTheAdminsPermission` |
| An existing user is added by their exact username, and an unknown one says so | `users.Routes.addExisting` | `ProjectUsersTests.AnExistingUserIsAddedByUsernameSeeingTheProject`, `AnUnknownUsernameSaysSo` |
| A tick gives one permission, once; unticking takes it | The unique index, `users.Routes.give`, `users.Routes.take` | `ProjectUsersTests.TickingGivesOnePermissionAndUntickingTakesOne` |
| Unticking `ReadProject` takes them out of the project | `users.Routes.take` | `ProjectUsersTests.UntickingSeeTheProjectTakesThemOutOfTheProject` |
| A tick, an untick, a chosen partner, its unchoosing or a role name sent twice at the same moment answers as done for both, keeping one row or none (findings row 190); any other refusal, a change to a row no longer there included, is still refused | `changes.Change.Save`, `users.Routes.give`, `users.Routes.take`, `Choose`, `Unchoose`, `SetRoleName` | `ProjectUsersTests.Row190_ManyTicksOfOnePermissionAtTheSameMomentAllAnswerAsSavedAndKeepOneRow`, `Row190_ManyUnticksOfOnePermissionAtTheSameMomentAllAnswerAsDoneAndLeaveNoRow`, `Row190_ManyFirstRoleNamesAtTheSameMomentAllAnswerAsSavedAndKeepOneRow`, `Row190_ASaveRefusedForAnythingButTheSameRowTwiceIsStillRefused`, `Row190_AChangeToARowNoLongerThereIsStillRefused`; `PartnersTests.Row190_ManyUnchoosesOfOnePartnerAtTheSameMomentAllAnswerAsDoneAndLeaveItUnchosen` |
| The admin's permission, or an unknown name, is refused | `access.Permission.UnmarshalJSON` | `ProjectUsersTests.TheAdminsPermissionOrAnUnknownNameIsRefused` |
| A project keeps at least one active person managing its users (BR-29); someone inactive there does not count (redix-api #126) | `ProjectManagers` | `ProjectUsersTests.TheLastWhoManagesUsersCannotLoseIt`, `WithASecondManagerTheFirstCanLeave`, `BR_29_AManagerInactiveInTheProjectDoesNotCountAsTheOneWhoStays` |
| The people of a project are listed those active there first, by name | `users.Routes.list` | `ProjectUsersTests.R126_ThePeopleOfAProjectAreListedThoseActiveThereFirstByName` |
| Each person says when they were last seen, on any device - their latest sign-in or renewal -, or `1900-01-01T00:00:00Z` before their first; the app's Users rows and the web's say «آخر دخول …» or «لم يدخل بعد» (redix-api #182) | `users.lastSeenOf` | `TestIssue182_EachPersonSaysWhenTheyWereLastSeenOrNeverBeforeTheirFirstSignIn`, `TestWeb_R182_UsersSaysWhenEachPersonLastSignedInOrThatTheyHaveNotYet` |
| Every permission ticked or unticked is in the History table | `history.Write` | `ProjectUsersTests.EveryPermissionChangeIsInTheHistory` |

## On the web: Users (redix-api #173)

The web's Users is the app's screen in the shell, its item in the side menu for whoever manages the project's users: everyone in the project, the inactive last, by name, from `users.ListOf`, the function `GET /projects/{id}/users` calls too, each with their username, role name and mobile, how many permissions they hold here, and said disabled when they are; the search finds a name, username, mobile or role name, as the app's does. A row opens the person in the details panel. Edit asks what the app's dialog asks and sends it as the app does - the name, mobile and email through `users.EditDetails` only when one changed, then the role name through `users.NameRole` only when it changed; Reset password gives a temporary password through `users.ResetTemporary`; Disable, asked first, and Enable go through `users.Disable` and `users.Enable`; Add an existing user adds someone already in REDIX by username through `users.AddExisting`. These are the rules `PUT /users/{userId}`, `POST /users/{userId}/reset-password`, `…/role-name`, `…/disable|enable` and `…/users/existing` call too, run through the shared changes.Writer; as the app's routes, a finished project's users stay manageable. New user, from Users' button, asks what the app's screen asks and adds the person through `users.Create`, the rule `POST /projects/{id}/users` runs: the name with its hint, username, role name, mobile, email and a temporary password with its rules; the permissions to tick in the app's order, Read project always, one of the capital pair at most (ticking one takes the other away), Specific partners' capital opening the partners to choose; and the tables to tick from `entries.SightTablesOf`, the function `GET /projects/{id}/tables` answers, each item's tick over its tables', every one seen until unticked. Kept, the form starts again and the person shows in the details panel, as Add entry does; refused, the form keeps what was typed but the password and says why. A person's Permissions, from their panel, shows what they hold here as the app's screen does, with the same ticks New user draws (one owner, `permissionTickList` and `tableTicks`), and keeps each tick at once, as the app does: a permission through `users.Give` or `users.Take` (taking Read project takes them out of the project, and the page empties as the app's does), a partner through `users.Choose` or `users.Unchoose`, an item's tables or one table through `users.SeeTables`, all an item's in one change; the page is drawn again as they then hold it, an item's tick between while only some of its tables are, and their panel beside it. My permissions, Ask for permissions and Permission requests are on UC-009's web section. Its pages are in `go-backend/web` (`users.go`, `users.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/users?text=` | Users, searched | `200` the tab's content; the item stays out of the menu without ManageUsers |
| `GET /details/user/{id}` | A person in the details panel; `?asking=edit`, `reset` or `disable` | `200` the panel or its form; `404` without ManageUsers, or for someone not in the project |
| `GET /details/user/existing` | Add an existing user's form | `200` the form; `404` without ManageUsers |
| `GET /tab/newUser` | New user | `200` the form; the tab stays out of reach without ManageUsers |
| `POST /users/new` | Adds the person New user typed (`name`, `username`, `roleName`, `mobile`, `email`, `password`, each `permission`, `partner` and `seen` table ticked) | `200` the form again with the person in the details panel, or the form saying why not; `403` without ManageUsers |
| `GET /tab/userPermissions?user={id}` | A person's Permissions | `200` the page; its title only for someone not in the project |
| `POST /user/{id}/permission/{permission}`, `POST /user/{id}/partner/{partnerId}`, `POST /user/{id}/tables?table=` | Keeps one tick at once: given, chosen or seen while `held` is sent, taken otherwise | `200` the page again with the person beside it, or saying why not; `403` without ManageUsers; `404` for someone or something not in the project |
| `POST /user`, `POST /user/{id}/edit`, `…/reset`, `…/disable`, `…/enable` | Adds an existing user (`username`), keeps Edit (`name`, `mobile`, `email`, `roleName`) or Reset password (`password`), or disables or enables the person | `200` the panel showing them, Users beside it; or the form or the panel saying why not; `403` without ManageUsers; `404` for an action there is none of |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Users lists everyone in the project, the inactive last, with their role name and permission count; the search finds name, username, mobile or role name | `users.ListOf`, `web.searchHolds` | `TestWeb_R173_UC008_UsersListsEveryoneAsTheAppsScreen` |
| Edit, Reset password, Disable, Enable and Add an existing user as the app's: a mobile and an email the rules accept, a role name of at most 50 letters, a password that keeps the policy, never the last manager of the users disabled, an unknown username refused (UC-007, UC-008) | `users.EditDetails`, `users.NameRole`, `users.ResetTemporary`, `users.Disable`, `users.Enable`, `users.AddExisting` | `TestWeb_R173_UC007_EditResetDisableEnableAndAddExistingAsTheAppsDialogs` |
| New user asks what the app's screen asks: the permissions in the app's order, Read project ticked and fixed, never Approve projects, the capital and reports groups, the partners under Specific partners' capital, the tables ticked | `web.permissionTicks`, `entries.SightTablesOf` | `TestWeb_R173_UC008_NewUserAsksWhatTheAppsScreenAsks` |
| New user adds the person with Read project and what is ticked, the partners chosen and the tables unticked unseen; both capital ticks, or a username taken, refused (FR-02) | `users.Create` | `TestWeb_R173_UC008_NewUserAddsThePersonWithWhatIsTicked` |
| New user is for whoever manages the project's users; a failing database answers 500 | `web.site.createUser`, `web.menuNeeds` | `TestWeb_R173_NewUserIsForWhoeverManagesTheUsers` |
| A person's Permissions shows what they hold here: the permissions, the partners chosen, the tables they see | `users.ListOf`, `entries.SightTablesOf`, `web.permissionTickList`, `web.tableTicks` | `TestWeb_R173_UC008_PermissionsShowWhatThePersonHoldsAsTheAppsScreen` |
| Each tick is kept at once as the app's: one of the capital pair at most, a partner chosen and unchosen, an item's tables in one change and its tick between while some are seen, the last manager of the users kept, Approve projects never given, Read project taken taking them out | `users.Give`, `users.Take`, `users.Choose`, `users.Unchoose`, `users.SeeTables` | `TestWeb_R173_UC008_EachTickIsKeptAtOnceAsTheAppsScreenKeepsIt` |
| Permissions is for whoever manages the project's users; a failing database answers 500 | `web.site.keepTick`, `web.menuNeeds` | `TestWeb_R173_PermissionsAreForWhoeverManagesTheUsers` |
| Users is for whoever manages the project's users; a failing database answers 500 | `web.site.keepUser`, `web.menuNeeds` | `TestWeb_R173_UsersAreForWhoeverManagesThem` |

## Left to later use cases

Opening the project list right after sign-in, and the default project (UC-010).
