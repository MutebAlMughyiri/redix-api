[Documentation](../README.md) › [Use cases](../README.md) › UC-049, UC-050, UC-051, UC-063

# UC-049, UC-050, UC-051, UC-063: The summary, the item report, the contracts, completion and cost

**Purpose:** give a project's figures at a glance, everything spent on one item, each contract's value, paid and remaining, and how far the work has come.

**Who uses it:** everyone who reads the project reads the summary and the contracts; an item's report and a contract's are for whoever holds `SeeReports` (redix-api #131).

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/summary` | Revenue, expenses, the surplus, the reconciliation difference, what waits as two figures (`WaitingFigures`), each category and item's total, the capital with the partners' lines the caller may see (BR-31), the completion |
| `GET /projects/{id}/reports/items/{itemId}` | An item's purchases by subgroup, then each contract with its payments |
| `GET /projects/{id}/reports/contracts/{contractId}` | One contract's table, as the Contracts screen opens it: its item's report holding that contract alone (the owner, 12 Nov 2026); another project's contract is not found |
| `GET /projects/{id}/contracts` | Each contract's value, paid, remaining and overrun (UC-020), which is the contracts report |

`ItemTotals` is the one owner of what each item cost (BR-02). The summary reads it on every request, so an approval shows at once.

**Completion is computed, not typed** (the owner, 7 November 2026): `ProjectFigures.CompletionPercent` is the approved expenses as a percentage of revenue, to two decimals. It is 0 while nothing came in, and passes 100 once expenses pass revenue. UC-063 first had it typed by whoever holds `ManageProject`; cost is the approved expenses; everyone sees both.

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-34.1: an approval shows at once in the surplus, the pending and the categories | `reports.SummaryOf`, `ItemTotals` | `SummaryTests.AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories` |
| The summary carries the reconciliation difference once there is one | `reports.SummaryOf` | `SummaryTests.TheSummaryCarriesTheReconciliationDifferenceOnceThereIsOne` |
| UC-063: completion is expenses as a percentage of revenue; everyone sees it and the cost, and nobody types it | `reports.SummaryOf` | `SummaryTests.UC_063_CompletionIsExpensesAsAPercentageOfRevenue`, `UC_063_EveryoneSeesTheCompletionAndTheCostAndNobodyTypesIt` |
| AC-30.1: an item's subgroups and contract payments add up to its total | `entries.ItemReportOf` | `SummaryTests.AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal` |
| An item of another project is not found (BR-33) | `entries.Routes.itemReportRoute` | `SummaryTests.AnItemOfAnotherProjectIsNotFound` |
| An item's own columns print when they show in reports (FR-41, row 60) | `entries.ItemReportOf` | `SummaryTests.AnItemsOwnColumnsPrintWhenTheyShowInReports` |

## On the web: projects and the home (redix-api #165)

The web lands where the app lands: the default project's home, or the list of the person's projects when none is the default (AC-37.1). The project button in the top bar lists them on every page; choosing one opens its home and keeps it in a cookie. The home is the app's home in the shell's content area: the summary from `reports.SummaryOf`, the custody the person holds from `accounts.ListOf`, and the newest entries from `entries.Newest`, the functions the app's routes call. Its pages are in `go-backend/web` (`project.go`, `home.go`, `home.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/home` | The home of the project the person works in, or their projects while they choose one | `200` the tab's content |
| `POST /project/{id}` | Chooses the project to work in | `200` the whole shell, its home in front; `404` for a project the person may not open |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| With no default project the home lists the person's projects; choosing one opens its home, named in the top bar and the side menu, and keeps it | `web.site.projectOf`, `web.site.chooseProject`, `projects.List` | `TestWeb_R165_WithNoDefaultProjectTheHomeListsTheProjectsAndChoosingOneOpensItsHome` |
| The default project opens straight into its home (AC-37.1) | `projects.DefaultOf` | `TestWeb_R165_AC37_1_TheDefaultProjectOpensStraightIntoItsHome` |
| The summary and the newest entries are the app's: the surplus, the totals, what waits, each entry's status and amount as BR-13 writes money | `reports.SummaryOf`, `entries.Newest`, `statement.Money` | `TestWeb_R165_UC049_TheSummaryAndTheNewestEntriesAreTheApps`, `TestWeb_R165_AnEmptyProjectSaysSoAndShowsNoWaiting` |
| What waits shows only to whoever may approve it | `web.page.waitsForMe` | `TestWeb_R165_WhatWaitsShowsOnlyToWhoeverMayApprove` |
| My custody shows each active account I hold | `accounts.ListOf`, `web.site.homeTab` | `TestWeb_R165_MyCustodyShowsWhatIHold` |
| A project one may not open is not chosen | `web.site.chooseProject` | `TestWeb_R165_AProjectOneMayNotOpenIsNotChosen` |
| Hide details folds the summary and is kept with the shell's look | `web.view.keepLook` | `TestWeb_R165_HideDetailsIsKeptWithTheLook` |
