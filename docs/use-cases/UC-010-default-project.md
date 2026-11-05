[Documentation](../README.md) › UC-010

# UC-010: Set the default project

**What it is for:** opening the usual project straight after sign-in (FR-37).
**Who uses it:** anyone signed in, for themselves.
**Issue:** redix-api #28; the app side is redix-app #29.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/{projectId}/default` with `{ openDirectly }` | `ReadProject` in it | `200` `{ defaultProjectId, openDefaultProject }`; `403` without access |
| `POST /projects/default/remove` | Any session | `200` `{ defaultProjectId: "", openDefaultProject: false }` |

A session answers `defaultProjectId`, empty when there is none or when access to it is gone, and `openDefaultProject`.

## Tables

| Table | Holds |
| --- | --- |
| `UserDefaultProjects` | A user's default project; no row means none |
| `Users.OpenDefaultProject` | Whether sign-in skips the list |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A default project is kept for the next session (AC-37.1) | `ProjectsEndpoints.SetDefault`, `Session.OfAsync` | `ProjectsTests.ADefaultProjectIsKeptForTheNextSession` |
| A default without access is not offered (AC-37.2) | `Session.OfAsync` | `ProjectsTests.ADefaultWithoutAccessIsNotOffered` |
| A project without access cannot be the default (BR-33) | `Permission.ReadProject` | `ProjectsTests.AProjectWithoutAccessCannotBeTheDefault` |
| A project waiting for approval cannot be the default | `Permission.ReadProject` | `ProjectsTests.AWaitingProjectCannotBeTheDefault` |
| A finished project can be the default | `Permission.ReadProject` | `ProjectsTests.AFinishedProjectCanBeTheDefault` |
| A new default replaces the old, one row per user | The unique index, `ProjectsEndpoints.SetDefault` | `ProjectsTests.ANewDefaultReplacesTheOld` |
| Removing the default turns "open directly" off | `ProjectsEndpoints.RemoveDefault` | `ProjectsTests.RemovingTheDefaultTurnsOpenDirectlyOff` |
| One user's default changes nothing for another | `UserDefaultProject.UserId` | `ProjectsTests.ADefaultIsOneUsersAlone` |
