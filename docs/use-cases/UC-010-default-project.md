[Documentation](../README.md) › UC-010

# UC-010: Set the default project

**What it is for:** opening the usual project straight after sign-in (FR-37): a default project always opens directly (Muteb, 8 Nov 2026).
**Who uses it:** anyone signed in, for themselves.
**Issue:** redix-api #28; the app side is redix-app #29.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/{projectId}/default` | `ReadProject` in it | `200` `{ defaultProjectId }`; `403` without access |
| `DELETE /projects/default` | Any session | `200` `{ defaultProjectId: "" }` |

A session answers `defaultProjectId`, the project sign-in opens; empty when there is none or when access to it is gone, being made inactive in it included: the default is skipped, not cleared, and comes back when he is made active again (redix-api #126).

## Tables

| Table | Holds |
| --- | --- |
| `UserDefaultProjects` | A user's default project, which sign-in opens; no row means none |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A default project is kept for the next session, which opens it (AC-37.1) | `ProjectsEndpoints.SetDefault`, `Session.OfAsync` | `ProjectsTests.AC_37_1_ADefaultProjectIsKeptForTheNextSessionWhichOpensIt` |
| A default without access is not offered (AC-37.2) | `Session.OfAsync` | `ProjectsTests.ADefaultWithoutAccessIsNotOffered` |
| A default he is inactive in is skipped, not cleared, and comes back when he is active again (AC-37.2, redix-api #126) | `Session.OfAsync`, `UserAccess.OfAsync` | `InactiveMembersTests.AC_37_2_ADefaultProjectHeIsInactiveInIsSkippedNotClearedAndComesBackWhenHeIsActiveAgain` |
| A project without access cannot be the default (BR-33) | `Permission.ReadProject` | `ProjectsTests.AProjectWithoutAccessCannotBeTheDefault` |
| A project waiting for approval cannot be the default | `Permission.ReadProject` | `ProjectsTests.AWaitingProjectCannotBeTheDefault` |
| A finished project can be the default | `Permission.ReadProject` | `ProjectsTests.AFinishedProjectCanBeTheDefault` |
| A new default replaces the old, one row per user | The unique index, `ProjectsEndpoints.SetDefault` | `ProjectsTests.ANewDefaultReplacesTheOld` |
| Removing the default leaves none, so sign-in shows the list | `ProjectsEndpoints.RemoveDefault` | `ProjectsTests.RemovingTheDefaultLeavesNoneSoSignInShowsTheList` |
| One user's default changes nothing for another | `UserDefaultProject.UserId` | `ProjectsTests.ADefaultIsOneUsersAlone` |
