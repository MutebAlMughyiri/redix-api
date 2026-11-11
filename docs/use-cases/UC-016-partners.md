[Documentation](../README.md) › UC-016

# UC-016: Manage partners

**What it is for:** keeping the list of a project's partners, who can then receive capital movements, and linking each to their own login, which says whose account the partner is and gives no sight of capital (redix-api #106).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Whoever holds `SeeAllCapital` reads them all; whoever holds `SeeChosenCapital` reads the partners chosen for them on their permissions (UC-009); anyone else none.
**Issue:** redix-api #34; the app side is redix-app #35.

## How it fits together

A partner belongs to one project (BR-33), so someone who is a partner in two projects has two rows. `Partners.UserId` is the login whose capital this is, and `Guid.Empty` means a partner with no login, as هيثم who withdrew. An empty value cannot point at a row, so this column has no foreign key: the route checks that the login is in the project, and logins are never deleted. A filtered unique index holds one login to one partner in a project.

Who sees a partner is not this link: it is `PartnerSight`, from the two capital ticks and the partners chosen under the second (redix-api #106), so an accountant who sees the figures is never taken for a partner. Partners are not copied from an earlier project, because only the structure is (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/partners` | `ReadProject` | `200` in their tables' order, as `{ id, name, userId, userName, mayDelete }`, deleted partners left out: everyone with `SeeAllCapital`, the chosen ones with `SeeChosenCapital`, otherwise none. `userId` and `userName` are empty for a partner with no login |
| `POST /projects/{projectId}/partners` with `{ name, userId }` | `ManageSettings` | `200`, a table of the project's one capital item; `400` `invalidName`, `itemNotOffered` (that item is disabled or no longer of capital), `unknownUser` or `userTaken` |
| `POST /projects/{projectId}/partners/{id}` with `{ name, userId }` | `ManageSettings` | The same, but for the item; `404`, also for a deleted partner |
| `GET /projects/{projectId}/partners/history?partner=&mine=&before=&text=` | `ReadProject` | `200` a page of the partners' history (redix-app #101): partners added, changed, given or losing an account, deleted; capital movements approved or reversed; newest first, 50 a page, only partners the caller sees (BR-31); `text` keeps the lines whose partner, account, who did it, the name it had, the login or the amount hold it, Arabic spelling read alike (redix-app #104); `400` `unknownPartner` for one not seen |
| `POST /projects/{projectId}/partners/{id}/delete` | `ManageSettings` | `200` deletes softly (redix-api #100); `404`, also once deleted; `409` `partnerNotDeletable` unless their capital is 0 and nothing unapproved names them |

**A partner is a table (redix-api #109).** Each partner is a table of a capital item of the structure, رأس المال, as a contract is a table of its item: Add entry lists رأس المال with its partners under it, its report and its sheet are a table per partner, and its percentages give each one's share (BR-05). The list answers each partner's `itemId` and `itemName` too, in their tables' order.

`userId` is left out for a partner with no login, which also takes a login away. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Partners` | Each project's partners, the login of each, and `IsDeleted` once deleted softly (redix-api #100) |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A partner is added by name, with no login | `PartnersEndpoints.Add` | `PartnersTests.FinanceAddsAPartnerByNameWithNoLogin` |
| A partner is linked to their login (BR-31) | `Partner.UserId` | `PartnersTests.APartnerIsLinkedToTheirLogin` |
| Partners show in the order added | `PartnersEndpoints.List` | `PartnersTests.PartnersShowInTheOrderTheyWereAdded` |
| An edit renames a partner and sets or takes away their login | `PartnersEndpoints.Edit` | `PartnersTests.AnEditRenamesAPartnerAndSetsTheirLogin`, `AnEditCanTakeALoginAway` |
| A partner needs a name of 1 to 100 characters | `ProjectChanges.WithNameAsync` | `PartnersTests.APartnerNeedsANameOfAtMost100Characters` |
| A login must be in the project (BR-33) | `PartnersEndpoints.RefusalOfAsync` | `PartnersTests.ALoginMustBeInTheProject` |
| A login is one partner in a project, also in the database, and any number have none | `PartnersEndpoints.RefusalOfAsync`, the index in `RedixDbContext` | `PartnersTests.ALoginIsOnePartnerInAProject`, `TheDatabaseHoldsALoginToOnePartnerInAProjectAndManyWithNoLogin` |
| One login may be a partner in two projects (BR-33) | The index in `RedixDbContext` | `PartnersTests.OneLoginMayBeAPartnerInTwoProjects` |
| redix-api #106: someone without `SeeAllCapital` sees only the partners chosen for them, or none; a partner's own login gives no sight (BR-31) | `PartnerSight` | `PartnersTests.UC_106_WithoutSeeAllCapitalSomeoneSeesOnlyThePartnersChosenForThemAndTheirOwnLoginGivesNoSight`, `SomeoneWhoIsNoPartnerSeesNoPartners` |
| redix-api #106: a partner is added by name to the project's one capital item, which nobody chooses; none is added while it is disabled | `PartnersEndpoints.Add` | `PartnersTests.UC_106_APartnerIsAddedByNameToTheProjectsOneCapitalItemAndNoneIsAddedWhileItIsDisabled` |
| Whoever holds `SeeAllCapital` sees every partner (BR-31) | `PartnersEndpoints.List` | `PartnersTests.WhoeverHoldsSeeAllCapitalSeesEveryPartner` |
| redix-api #109: partners may be declared by name when a project is created or registered (`partners` in `POST /projects` and `POST /projects/register`), an unacceptable name answering `invalidName`; once the project is approved they become tables of its first capital item, رأس المال, in the order typed | `PartnersEndpoints.DeclaredWith`, `ProjectStructure.ApproveAsync` | `PartnersTests.UC_109_PartnersDeclaredWithANewProjectBecomeTablesOfItsRasAlMalInTheirOrderOnceApproved` |
| redix-api #109: a partner is a table of the capital item, last among its tables, and the tables are put in order like any item's | `PartnersEndpoints.Add`, `ProjectStructure.NextTablePlaceAsync`, `CategoriesEndpoints.ReorderTables` | `CategoriesTests.UC_109_APartnerIsATableOfTheCapitalItemLastAmongItsTables` |
| redix-api #100: a partner at zero capital that nothing unapproved names is deleted softly: they leave Partners, Add entry, the item's tables and the capital lines, their movements stay in the entries, reports and History, and their account is free; one with capital or something waiting is not, and the list says which | `PartnersEndpoints.Delete`, `DeletableAsync`, `Partner.Kept` | `PartnerDeleteTests.UC_100_APartnerAtZeroCapitalIsDeletedSoftlyLeavingTheListsWhileTheirMovementsAndHistoryStayAndTheirAccountIsFree`, `UC_100_APartnerWithCapitalOrAMovementWaitingIsNotDeletedAndOneNeverUsedIs`, `UC_100_ADeletedPartnerCannotBeChosenForSomeone` |
| redix-app #101: the partners' history lists partner actions and approved or reversed capital movements, one partner's or what I did, only partners the caller sees; adding and changing a partner write History rows; paging is `HistoryPage`, shared with the accounts' history | `PartnerHistory`, `HistoryPage` | `PartnerHistoryTests.UC_101_EachPartnerActionAndApprovedOrReversedCapitalMovementIsALineNewestFirst`, `UC_101_OnePartnersLinesAndWhatICanBeKeptAlone`, `UC_101_BR_31_SomeoneWithChosenPartnersSeesOnlyTheirLinesAndSomeoneWithNeitherSeesNone` |
| Only Finance changes partners | `Permission.ManageSettings` | `PartnersTests.OnlyFinanceChangesPartners` |
| One project's partners are never another's (BR-33) | `Partner.ProjectId`, `PartnersEndpoints` | `PartnersTests.OneProjectsPartnersAreNeverAnothers` |
| A project copied from an earlier one starts with no partners (AC-05.2) | `ProjectStructure.ApproveAsync` copies only structure | `PartnersTests.ACopiedProjectStartsWithNoPartners` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges` | `PartnersTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Capital movements, and a withdrawn partner's zero balance (FR-25); `IsActive` was removed by findings row 148 | The capital movement use cases |
| Each partner's capital and share, and someone seeing only their own (BR-05, BR-31) | UC-044 "Compute partner shares" |
| A partner's pocket account | Retired by redix-api #99: adding a partner makes no account |
