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

The web lands where the app lands: the default project's home, or the list of the person's projects when none is the default (AC-37.1). The project button in the top bar lists them on every page; choosing one opens its home and keeps it in a cookie. The home is the project's home in the shell's content area: the summary from `reports.SummaryOf`, always open, and the custody the person holds from `accounts.ListOf`, the functions the app's routes call. The owner made the web's home its own on 23 Nov 2026 (findings rows 306, 310): one column in the middle of the page, no Hide details, Revenue and Expenses side by side with every category and its share, never folded, and no newest entries, which Entries lists; the app's home keeps its own. Its pages are in `go-backend/web` (`project.go`, `home.go`, `home.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/home` | The home of the project the person works in, or their projects while they choose one | `200` the tab's content |
| `POST /project/{id}` | Chooses the project to work in | `200` the whole shell, its home in front; `404` for a project the person may not open |
| `POST /project/{id}/default` | Stars the project as the default, or clears the star | `200` the top bar's list, still open, saying so; `404` for a project the person may not open |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| With no default project the home lists the person's projects; choosing one opens its home, named in the top bar and the side menu, and keeps it | `web.site.projectOf`, `web.site.chooseProject`, `projects.List` | `TestWeb_R165_WithNoDefaultProjectTheHomeListsTheProjectsAndChoosingOneOpensItsHome` |
| The default project opens straight into its home (AC-37.1) | `projects.DefaultOf` | `TestWeb_R165_AC37_1_TheDefaultProjectOpensStraightIntoItsHome` |
| The summary is the app's: the surplus, the totals, what waits, as BR-13 writes money | `reports.SummaryOf`, `statement.Money` | `TestWeb_R165_UC049_TheSummaryIsTheApps`, `TestWeb_R165_AnEmptyProjectShowsNoWaiting` |
| What waits shows only to whoever may approve it | `web.page.waitsForMe` | `TestWeb_R165_WhatWaitsShowsOnlyToWhoeverMayApprove` |
| My custody shows each active account I hold | `accounts.ListOf`, `web.site.homeTab` | `TestWeb_R165_MyCustodyShowsWhatIHold` |
| A project one may not open is not chosen | `web.site.chooseProject` | `TestWeb_R165_AProjectOneMayNotOpenIsNotChosen` |
| A star makes a project the default, saying so, and a second clears it, as the app's star (AC-37.1) | `projects.SetDefault`, `projects.RemoveDefault`, `web.site.starProject` | `TestWeb_R165_AC37_1_TheStarMakesAProjectTheDefaultAndAgainClearsIt` |
| The web's home shows the summary always and lists no entries; Add entry opens Add entry for whoever may enter expenses (findings row 306) | `web.home`, `web.page.mayEnterExpenses` | `TestWeb_R306_TheHomeShowsTheSummaryAlwaysAndListsNoEntries` |
| My account, at the status bar's end, opens My history, Shared settings and Sign out; the status bar is always shown (findings row 306) | `web.statusBar` | `TestWeb_R306_MyAccountInTheStatusBarHoldsMyHistorySharedSettingsAndSignOut` |
| Settings is the app's: who is signed in, Switch project and the default project from the top bar's list, the language, the appearance kept in a cookie (#175 D1) - not the count of the home's entries, the web's home listing none (findings row 306) - Change password, My history, the privacy policy and the terms of use the web serves too, Sign out and the version | `web.settingsPage`, `web.site.chooseTheme`, `legal.Privacy`, `legal.Terms` | `TestWeb_R175_SettingsAsTheAppsScreen` |

## On the web: the item reports (redix-api #171)

Reports' Expenses and Revenue open each side's item reports in a tab, as the app's screen shows them: each category's total and its share of the side, from `reports.SummaryOf` as the person sees it (BR-31), opening onto its items with their totals; the search keeps a category whose name it finds whole, otherwise the items it finds. An item opens its report in a tab, from `entries.ItemReportIn`, the function `GET /projects/{id}/reports/items/{itemId}` calls too: the item's total, then each table - a subgroup's purchases or revenue, the general one called «عام», a partner's money in and out for whoever sees that partner's capital - with its total and its rows, each row its description, its other values with their columns' names and its amount; then each contract with its value, paid and remaining, overrun said, its payments and their total. Contracts' panel opens a contract's report the same way, from `entries.ContractReportOf` (`GET …/reports/contracts/{contractId}`): its item's report holding that contract alone. The report's PDF and Excel - an item's or a contract's - are `reports.ReportPdfOf` and `reports.ReportWorkbookOf`, the functions `GET …/reports/items/{itemId}/pdf|xlsx` and `…/reports/contracts/{contractId}/pdf|xlsx` call too, for whoever holds GenerateReports, gray with its reason for anyone else.

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/expenses?text=`, `GET /tab/revenue?text=` | A side's item reports, searched | `200` the tab's content |
| `GET /report-file/item/{itemId}/pdf|xlsx`, `GET /report-file/contract/{contractId}/pdf|xlsx` | An item's or a contract's report as a file | `200` the file; `403` without GenerateReports; `404` for one the person may not see, or another format |
| `GET /tab/itemReport?item=&text=`, `GET /tab/itemReport?contract=&text=` | An item's or a contract's report, searched | `200` the tab's content; for an item or contract the person may not see, that it could not be shown |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Each side's categories with their totals and shares, their items opening their reports; an item's report by table and contract, overrun said; a contract's report holds that contract alone (UC-050, BR-04); the search keeps a group by its name or its members; a failing database answers 500 | `reports.SummaryOf`, `entries.ItemReportIn`, `entries.ContractReportOf`, `web.searchGroup` | `TestWeb_R171_UC050_EachSidesItemReportsAndAnItemsOrAContractsReport` |
| A report row says its values by column, an amount column's as money, and a named table by its name | `web.reportRowDetails`, `web.subgroupName` | `TestWeb_R171_UC050_AReportRowSaysItsValuesByColumnAndANamedTableByName` |
