[Documentation](../README.md) › UC-016

# UC-016: Manage partners

**What it is for:** keeping the list of a project's partners, who can then receive capital movements, and linking each to their own login, which says whose account the partner is and gives no sight of capital (redix-api #106).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Whoever holds `SeeAllCapital` reads them all; whoever holds `SeeChosenCapital` reads the partners chosen for them on their permissions (UC-009); anyone else none.
**Issue:** redix-api #34; the app side is redix-app #35.

## How it fits together

A partner belongs to one project (BR-33), so someone who is a partner in two projects has two rows. A row of `partner_users` is the login whose capital this is, and no row means a partner with no login, as partner C who withdrew; the answer's `userId` is empty then. The row points at the login, so the database refuses one that names nobody (findings row 214), and a unique index holds one login to one partner in a project. A deleted partner's row goes with the delete, so the login may be another partner's.

Who sees a partner is not this link: it is `PartnerSight`, from the two capital ticks and the partners chosen under the second (redix-api #106), so an accountant who sees the figures is never taken for a partner. Partners are not copied from an earlier project, because only the structure is (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/partners` | `ReadProject` | `200` in their tables' order, as `{ id, name, userId, userName, mayDelete, othersDrafts, myDrafts }`: `othersDrafts` names whoever else holds a draft naming the partner, told only to whoever holds `ManageSettings` (BR-42, findings row 203), and `myDrafts` lists the caller's own drafts naming them, with their date and amount, deleted partners left out: everyone with `SeeAllCapital`, the chosen ones with `SeeChosenCapital`, otherwise none. `userId` and `userName` are empty for a partner with no login |
| `POST /projects/{projectId}/partners` with `{ name, userId }` | `ManageSettings` | `200`, a table of the project's one capital item; `400` `invalidName`, `itemNotOffered` (that item is disabled or no longer of capital), `unknownUser` or `userTaken` |
| `PUT /projects/{projectId}/partners/{id}` with `{ name, userId }` | `ManageSettings` | The same, but for the item; `404`, also for a deleted partner |
| `GET /projects/{projectId}/partners/history?partner=&mine=&before=&text=` | `ReadProject` | `200` a page of the partners' history (redix-app #101): partners added, changed, given or losing an account, deleted; capital movements approved (one removed by redix-api #119 leaves it); newest first, 50 a page, only partners the caller sees (BR-31); `text` keeps the lines whose partner, account, who did it, the name it had, the login or the amount hold it, Arabic spelling read alike (redix-app #104); `400` `unknownPartner` for one not seen |
| `DELETE /projects/{projectId}/partners/{id}` | `ManageSettings` | `200` deletes softly (redix-api #100); `404`, also once deleted; `409` `partnerNotDeletable` unless their capital is 0, nothing waiting or rejected names them and no one else's draft does (findings row 199). The caller's own drafts naming them are deleted with them, so only when the caller also holds `DeleteEntries` (findings row 202) |

**A partner is a table (redix-api #109).** Each partner is a table of a capital item of the structure, رأس المال, as a contract is a table of its item: Add entry lists رأس المال with its partners under it, its report and its sheet are a table per partner, and its percentages give each one's share (BR-05). The list answers each partner's `itemId` and `itemName` too, in their tables' order.

`userId` is left out for a partner with no login, which also takes a login away. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Partners` | Each project's partners, and `IsDeleted` once deleted softly (redix-api #100) |
| `partner_users` | Each kept partner's own login, for those who have one (findings row 214) |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A partner is added by name, with no login | `partners.Routes.add` | `PartnersTests.FinanceAddsAPartnerByNameWithNoLogin` |
| A partner is linked to their login (BR-31) | `partners.linkTo`, `partners.refusalOfLogin` | `PartnersTests.APartnerIsLinkedToTheirLogin` |
| A partner's login is a row of its own, which taking it away removes, and the database refuses one naming nobody (findings row 214) | `partners.linkTo`, `partner_users` | `PartnersTests.R214_APartnersLoginIsARowOfItsOwnThatTakingTheLoginAwayRemoves`, `R214_TheDatabaseRefusesAPartnersLoginNamingNoUser`, `FinanceAddsAPartnerByNameWithNoLogin` |
| A deleted partner's login may be another partner's in the project | `partners.Routes.delete` | `PartnersTests.R214_ADeletedPartnersLoginMayBeAnotherPartnersInTheProject` |
| Partners show in the order added | `partners.Routes.list` | `PartnersTests.PartnersShowInTheOrderTheyWereAdded` |
| An edit renames a partner and sets or takes away their login | `partners.Routes.edit` | `PartnersTests.AnEditRenamesAPartnerAndSetsTheirLogin`, `AnEditCanTakeALoginAway` |
| A partner needs a name of 1 to 100 characters | `changes.Named` | `PartnersTests.APartnerNeedsANameOfAtMost100Characters` |
| A login must be in the project (BR-33) | `partners.refusalOfLogin` | `PartnersTests.ALoginMustBeInTheProject` |
| A login is one partner in a project, also in the database, and any number have none | `partners.refusalOfLogin`, the unique index `ix_partner_users_project_id_user_id` | `PartnersTests.ALoginIsOnePartnerInAProject`, `TheDatabaseHoldsALoginToOnePartnerInAProjectAndManyWithNoLogin` |
| One login may be a partner in two projects (BR-33) | The unique index `ix_partner_users_project_id_user_id`, per project | `PartnersTests.OneLoginMayBeAPartnerInTwoProjects` |
| redix-api #106: someone without `SeeAllCapital` sees only the partners chosen for them, or none; a partner's own login gives no sight (BR-31) | `PartnerSight` | `PartnersTests.UC_106_WithoutSeeAllCapitalSomeoneSeesOnlyThePartnersChosenForThemAndTheirOwnLoginGivesNoSight`, `SomeoneWhoIsNoPartnerSeesNoPartners` |
| redix-api #106: a partner is added by name to the project's one capital item, which nobody chooses; none is added while it is disabled | `partners.Routes.add` | `PartnersTests.UC_106_APartnerIsAddedByNameToTheProjectsOneCapitalItemAndNoneIsAddedWhileItIsDisabled` |
| Whoever holds `SeeAllCapital` sees every partner (BR-31) | `partners.Routes.list` | `PartnersTests.WhoeverHoldsSeeAllCapitalSeesEveryPartner` |
| redix-api #109: partners may be declared by name when a project is created or registered (`partners` in `POST /projects` and `POST /projects/register`), an unacceptable name answering `invalidName`; once the project is approved they become tables of its first capital item, رأس المال, in the order typed | `projects.declaredPartners`, `structure.Copy` | `PartnersTests.UC_109_PartnersDeclaredWithANewProjectBecomeTablesOfItsRasAlMalInTheirOrderOnceApproved` |
| redix-api #109: a partner is a table of the capital item, last among its tables, and the tables are put in order like any item's | `partners.Routes.add`, `structure.NextTablePlace`, `structure.Routes.reorderTables` | `CategoriesTests.UC_109_APartnerIsATableOfTheCapitalItemLastAmongItsTables` |
| redix-api #100: a partner at zero capital that nothing unapproved names is deleted softly: they leave Partners, Add entry, the item's tables and the capital lines, their movements stay in the entries, reports and History, and their account is free; one with capital or something waiting is not, and the list says which | `partners.Routes.delete`, `DeletableAsync`, `structure.change.keptPartnersIn` | `PartnerDeleteTests.UC_100_APartnerAtZeroCapitalIsDeletedSoftlyLeavingTheListsWhileTheirMovementsAndHistoryStayAndTheirAccountIsFree`, `UC_100_APartnerWithCapitalOrAMovementWaitingIsNotDeletedAndOneNeverUsedIs`, `UC_100_ADeletedPartnerCannotBeChosenForSomeone` |
| redix-app #101: the partners' history lists partner actions and approved capital movements, one partner's or what I did, only partners the caller sees; adding and changing a partner write History rows; paging is `HistoryPage`, shared with the accounts' history | `PartnerHistory`, `HistoryPage` | `PartnerHistoryTests.UC_101_EachPartnerActionAndApprovedCapitalMovementIsALineNewestFirst`, `UC_101_OnePartnersLinesAndWhatICanBeKeptAlone`, `UC_101_BR_31_SomeoneWithChosenPartnersSeesOnlyTheirLinesAndSomeoneWithNeitherSeesNone` |
| Findings row 214: a login given, changed or taken in an edit is a line of its own, read from `partner_users`; the login given when a partner is added is part of the partner's added line, which names no login, as `entries.partnerLine.Login` says; rows written before keep reading as they did | `entries.Routes.partnerHistory`, `partners.Routes.add`, `Edit` | `PartnerHistoryTests.R214_APartnerAddedWithALoginIsOneLineAndTakingTheLoginAwayIsALineWithNone`, `UC_101_EachPartnerActionAndApprovedCapitalMovementIsALineNewestFirst` |
| Only Finance changes partners | `access.ManageSettings` | `PartnersTests.OnlyFinanceChangesPartners` |
| One project's partners are never another's (BR-33) | `access.Gate.Needs`, `partners.kept` | `PartnersTests.OneProjectsPartnersAreNeverAnothers` |
| A project copied from an earlier one starts with no partners (AC-05.2) | `structure.Copy` copies only structure | `PartnersTests.ACopiedProjectStartsWithNoPartners` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `PartnersTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| Findings row 199: someone else's draft keeps a partner from being deleted and says whose; the caller's own drafts are deleted with them | `partners.deletable`, `partners.Routes.delete` | `PartnerDeleteTests.Row_199_SomeoneElsesDraftHoldsThePartnerAndSaysWhoseWhileTheCallersOwnDraftsAreDeletedWithThem` |
| Findings row 202: a delete that would take the caller's own drafts needs `DeleteEntries` too | `partners.deletable` | `PartnerDeleteTests.Row_202_ADeleteThatTakesTheCallersOwnDraftsNeedsDeleteEntriesTooWhileOneWithNoDraftOfTheirsDoesNot` |
| Findings row 203, BR-42: whose draft names a partner is told only to whoever holds `ManageSettings` | `partners.Routes.list` | `PartnerDeleteTests.Row_203_WhoseDraftNamesAPartnerIsToldOnlyToWhoeverMayDeletePartners` |

## On the web: Partners (redix-api #169)

The web's Partners is the app's screen in the shell: the capital total from `partners.Capital`, the function `GET /projects/{id}/capital` calls too, everyone who reads the project seeing it; then each partner the person sees (BR-31), the largest balance first, with their login - or that they have none, from `partners.ListOf`, the function `GET /projects/{id}/partners` calls too - their share, written to ten decimals as the app writes it, and their balance. A row opens the partner in the details panel with their approved movements, newest first, from `partners.MovementsOf`, the function `GET /projects/{id}/partners/{partnerId}/movements` calls too; a movement's receipt stays off the web (#80 D2). Its pages are in `go-backend/web` (`partners.go`, `partners.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/partners` | Partners, with the capital total | `200` the tab's content |
| `GET /details/partner/{id}` | A partner and their movements in the details panel | `200` the panel; `404` for a partner the person does not see, or none |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Partners lists the capital and each partner's login, share and balance, the largest first; a partner's movements open in the panel (UC-040, BR-05, BR-28); a failing database answers 500 | `partners.Capital`, `partners.ListOf`, `partners.MovementsOf` | `TestWeb_R169_UC040_PartnersListsTheCapitalAndEachPartnersShare` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Capital movements, and a withdrawn partner's zero balance (FR-25); `IsActive` was removed by findings row 148 | The capital movement use cases |
| Each partner's capital and share, and someone seeing only their own (BR-05, BR-31) | UC-044 "Compute partner shares" |
| A partner's pocket account | Retired by redix-api #99: adding a partner makes no account |
