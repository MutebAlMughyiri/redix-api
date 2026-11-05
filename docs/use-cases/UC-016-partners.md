[Documentation](../README.md) › UC-016

# UC-016: Manage partners

**What it is for:** keeping the list of a project's partners, who can then receive capital movements, and linking each to their login, so the API can tell whose capital is someone's own (BR-31).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Whoever holds `SeeAllCapital` reads them all; anyone else reads only their own.
**Issue:** redix-api #34; the app side is redix-app #35.

## How it fits together

A partner belongs to one project (BR-33), so someone who is a partner in two projects has two rows. `Partners.UserId` is the login whose capital this is, and `Guid.Empty` means a partner with no login, as هيثم who withdrew. An empty value cannot point at a row, so this column has no foreign key: the route checks that the login is in the project, and logins are never deleted. A filtered unique index holds one login to one partner in a project.

The capital use cases read their figures through the same link: a partner's row is what `GET .../partners` answers its login. Partners are not copied from an earlier project, because only the structure is (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/partners` | `ReadProject` | `200` in the order added, as `{ id, name, userId, userName }`: everyone with `SeeAllCapital`, otherwise only the caller's own. `userId` and `userName` are empty for a partner with no login |
| `POST /projects/{projectId}/partners` with `{ name, userId }` | `ManageSettings` | `200`; `400` `invalidName`, `unknownUser` or `userTaken` |
| `POST /projects/{projectId}/partners/{id}` with `{ name, userId }` | `ManageSettings` | The same; `404` |

`userId` is left out for a partner with no login, which also takes a login away. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Partners` | Each project's partners, and the login of each |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A partner is added by name, with no login | `PartnersEndpoints.Add` | `PartnersTests.FinanceAddsAPartnerByNameWithNoLogin` |
| A partner is linked to their login (BR-31) | `Partner.UserId` | `PartnersTests.APartnerIsLinkedToTheirLogin` |
| Partners show in the order added | `PartnersEndpoints.List` | `PartnersTests.PartnersShowInTheOrderTheyWereAdded` |
| An edit renames a partner and sets or takes away their login | `PartnersEndpoints.Edit` | `PartnersTests.AnEditRenamesAPartnerAndSetsTheirLogin`, `AnEditCanTakeALoginAway` |
| A partner needs a name of 1 to 100 characters | `ProjectChanges.WithNameAsync` | `PartnersTests.APartnerNeedsANameOfAtMost100Characters` |
| A login must be in the project (BR-33) | `PartnersEndpoints.RefusalOfLoginAsync` | `PartnersTests.ALoginMustBeInTheProject` |
| A login is one partner in a project, also in the database, and any number have none | `PartnersEndpoints.RefusalOfLoginAsync`, the index in `RedixDbContext` | `PartnersTests.ALoginIsOnePartnerInAProject`, `TheDatabaseHoldsALoginToOnePartnerInAProjectAndManyWithNoLogin` |
| One login may be a partner in two projects (BR-33) | The index in `RedixDbContext` | `PartnersTests.OneLoginMayBeAPartnerInTwoProjects` |
| Someone without `SeeAllCapital` sees only their own partner, or none (BR-31) | `PartnersEndpoints.List` | `PartnersTests.SomeoneWithoutSeeAllCapitalSeesOnlyTheirOwnPartner`, `SomeoneWhoIsNoPartnerSeesNoPartners` |
| Whoever holds `SeeAllCapital` sees every partner (BR-31) | `PartnersEndpoints.List` | `PartnersTests.WhoeverHoldsSeeAllCapitalSeesEveryPartner` |
| Only Finance changes partners | `Permission.ManageSettings` | `PartnersTests.OnlyFinanceChangesPartners` |
| One project's partners are never another's (BR-33) | `Partner.ProjectId`, `PartnersEndpoints` | `PartnersTests.OneProjectsPartnersAreNeverAnothers` |
| A project copied from an earlier one starts with no partners (AC-05.2) | `ProjectStructure.ApproveAsync` copies only structure | `PartnersTests.ACopiedProjectStartsWithNoPartners` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges` | `PartnersTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Capital movements, and a withdrawn partner's zero balance and `IsActive = false` (FR-25) | The capital movement use cases |
| Each partner's capital and share, and someone seeing only their own (BR-05, BR-31) | UC-044 "Compute partner shares" |
| A partner's pocket account | UC-045 "Expense paid from a partner's money" |
