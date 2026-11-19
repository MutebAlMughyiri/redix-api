[Documentation](../README.md) › UC-008

# UC-008: Add people to a project and tick their permissions

**What it is for:** deciding who works on each project, and what each may do there (FR-36).
**Who uses it:** whoever manages a project's users, for that project alone (BR-33). What each permission allows is UC-009.
**Issue:** redix-api #26, redesigned by redix-api #89 "No roles: each person's permissions ticked per project"; the app side is redix-app #28.

## Endpoints

All need `ManageUsers` in the project, and answer `403` to anyone else.

| Endpoint | Purpose | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/users` | This project's people, those active there first (redix-api #126), by name, with only what they hold in this project | `200` the users |
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
| A user sees only their projects (AC-36.1) | `projects.Routes.list` | `ProjectsTests.AUserSeesOnlyTheirProjects` |
| Adding a person needs a well-formed email no other account holds, whatever its case, and a refusal creates nothing (AC-07.8, D1, R9) | `users.RefusalOfEmail`, `users.Routes.create`, the unique index on | `ProjectUsersTests.AC_07_8_AddingAPersonKeepsTheEmailTheManagerTyped`, `AC_07_8_D1_AddingAPersonNeedsAWellFormedEmailAndCreatesNothingWithout`, `AC_07_8_R9_AnEmailAnotherAccountHoldsIsRefusedWhateverItsCase` |
| The email is checked after the other fields, in the order the form shows them, so an empty form names the username first | `users.Routes.create` | `ProjectUsersTests.AC_07_8_AnEmptyFormNamesTheUsernameFirstAndTheEmailOnlyAfterTheOtherFields_AsTheFormOrdersThem` |
| Each project carries what the user holds in it (AC-36.2) | `projects.Routes.list` | `ProjectsTests.PermissionsFollowWhatIsTickedInEachProject` |
| A manager sees only their project's people, and none of those people's other projects (BR-33) | `users.Routes.list` | `ProjectUsersTests.AManagerSeesOnlyTheirProjectsPeopleAndNoneOfTheirOtherProjects` |
| A new login holds what was ticked and `ReadProject`, never the admin's permission | `users.Routes.create` | `ProjectUsersTests.AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword`, `ANewLoginCannotBeGivenTheAdminsPermission` |
| An existing user is added by their exact username, and an unknown one says so | `users.Routes.addExisting` | `ProjectUsersTests.AnExistingUserIsAddedByUsernameSeeingTheProject`, `AnUnknownUsernameSaysSo` |
| A tick gives one permission, once; unticking takes it | The unique index, `users.Routes.give`, `users.Routes.take` | `ProjectUsersTests.TickingGivesOnePermissionAndUntickingTakesOne` |
| Unticking `ReadProject` takes them out of the project | `users.Routes.take` | `ProjectUsersTests.UntickingSeeTheProjectTakesThemOutOfTheProject` |
| A tick, an untick, a chosen partner, its unchoosing or a role name sent twice at the same moment answers as done for both, keeping one row or none (findings row 190); any other refusal, a change to a row no longer there included, is still refused | `changes.Change.Save`, `users.Routes.give`, `users.Routes.take`, `Choose`, `Unchoose`, `SetRoleName` | `ProjectUsersTests.Row190_ManyTicksOfOnePermissionAtTheSameMomentAllAnswerAsSavedAndKeepOneRow`, `Row190_ManyUnticksOfOnePermissionAtTheSameMomentAllAnswerAsDoneAndLeaveNoRow`, `Row190_ManyFirstRoleNamesAtTheSameMomentAllAnswerAsSavedAndKeepOneRow`, `Row190_ASaveRefusedForAnythingButTheSameRowTwiceIsStillRefused`, `Row190_AChangeToARowNoLongerThereIsStillRefused`; `PartnersTests.Row190_ManyUnchoosesOfOnePartnerAtTheSameMomentAllAnswerAsDoneAndLeaveItUnchosen` |
| The admin's permission, or an unknown name, is refused | `access.Permission.UnmarshalJSON` | `ProjectUsersTests.TheAdminsPermissionOrAnUnknownNameIsRefused` |
| A project keeps at least one active person managing its users (BR-29); someone inactive there does not count (redix-api #126) | `ProjectManagers` | `ProjectUsersTests.TheLastWhoManagesUsersCannotLoseIt`, `WithASecondManagerTheFirstCanLeave`, `BR_29_AManagerInactiveInTheProjectDoesNotCountAsTheOneWhoStays` |
| The people of a project are listed those active there first, by name | `users.Routes.list` | `ProjectUsersTests.R126_ThePeopleOfAProjectAreListedThoseActiveThereFirstByName` |
| Every permission ticked or unticked is in the History table | `history.Write` | `ProjectUsersTests.EveryPermissionChangeIsInTheHistory` |

## Left to later use cases

Opening the project list right after sign-in, and the default project (UC-010).
