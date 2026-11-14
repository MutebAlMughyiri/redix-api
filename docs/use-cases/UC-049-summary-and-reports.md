[Documentation](../README.md) › [Use cases](../README.md) › UC-049, UC-050, UC-051, UC-063

# UC-049, UC-050, UC-051, UC-063: The summary, the item report, the contracts, completion and cost

**Purpose:** give a project's figures at a glance, everything spent on one item, each contract's value, paid and remaining, and how far the work has come.

**Who uses it:** everyone who reads the project reads the summary and the contracts; an item's report and a contract's are for whoever holds `SeeReports` (redix-api #131).

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/summary` | Revenue, expenses, the surplus, the reconciliation difference, what waits as two figures (`WaitingFigures`), each category and item's total, the capital with the partners' lines the caller may see (BR-31), the completion |
| `GET /projects/{id}/reports/items/{itemId}` | An item's purchases by subgroup, then each contract with its payments |
| `GET /projects/{id}/reports/contracts/{contractId}` | One contract's table, as the Contracts screen opens it: its item's report holding that contract alone (Muteb, 12 Nov 2026); another project's contract is not found |
| `GET /projects/{id}/contracts` | Each contract's value, paid, remaining and overrun (UC-020), which is the contracts report |

`ItemTotals` is the one owner of what each item cost (BR-02). The summary reads it on every request, so an approval shows at once.

**Completion is computed, not typed** (Muteb, 7 November 2026): `ProjectFigures.CompletionPercent` is the approved expenses as a percentage of revenue, to two decimals. It is 0 while nothing came in, and passes 100 once expenses pass revenue. UC-063 first had it typed by whoever holds `ManageProject`; cost is the approved expenses; everyone sees both.

| Rule | Code | Test |
| --- | --- | --- |
| AC-34.1: an approval shows at once in the surplus, the pending and the categories | `SummaryEndpoints.Summary`, `ItemTotals` | `SummaryTests.AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories` |
| The summary carries the reconciliation difference once there is one | `SummaryEndpoints.Summary` | `SummaryTests.TheSummaryCarriesTheReconciliationDifferenceOnceThereIsOne` |
| UC-063: completion is expenses as a percentage of revenue; everyone sees it and the cost, and nobody types it | `ProjectFigures.CompletionPercent`, `SummaryEndpoints.Summary` | `SummaryTests.UC_063_CompletionIsExpensesAsAPercentageOfRevenue`, `UC_063_EveryoneSeesTheCompletionAndTheCostAndNobodyTypesIt` |
| AC-30.1: an item's subgroups and contract payments add up to its total | `ItemReportEndpoints.OfAsync` | `SummaryTests.AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal` |
| An item of another project is not found (BR-33) | `ItemReportEndpoints.Report` | `SummaryTests.AnItemOfAnotherProjectIsNotFound` |
| An item's own columns print when they show in reports (FR-41, row 60) | `ItemReportEndpoints.OfAsync` | `SummaryTests.AnItemsOwnColumnsPrintWhenTheyShowInReports` |
