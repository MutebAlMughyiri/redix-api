[Documentation](../README.md) › [Use cases](../README.md) › UC-049, UC-050, UC-051, UC-063

# UC-049, UC-050, UC-051, UC-063: The dashboard, the item report, the contracts, completion and cost

**Purpose:** give a project's figures at a glance, everything spent on one item, each contract's value, paid and remaining, and how far the work has come.

**Who uses it:** everyone who reads the project.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/dashboard` | Revenue, expenses, surplus with and without sales, the reconciliation difference, what waits, each category and item's total, the completion |
| `GET /projects/{id}/reports/items/{itemId}` | An item's purchases by subgroup, then each contract with its payments |
| `GET /projects/{id}/contracts` | Each contract's value, paid, remaining and overrun (UC-020), which is the contracts report |

`ItemTotals` is the one owner of what each item cost (BR-02). The dashboard reads it on every request, so an approval shows at once.

**Completion is computed, not typed** (Muteb, 7 November 2026): `ProjectFigures.CompletionPercent` is the approved expenses as a percentage of revenue, to two decimals. It is 0 while nothing came in, and passes 100 once expenses pass revenue. UC-063 first had it typed by whoever holds `ManageProject`; cost is the approved expenses; everyone sees both.

| Rule | Code | Test |
| --- | --- | --- |
| AC-34.1: an approval shows at once in the surplus, the pending and the categories | `DashboardEndpoints.Dashboard`, `ItemTotals` | `DashboardTests.AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories` |
| The dashboard carries the reconciliation difference once there is one | `DashboardEndpoints.Dashboard` | `DashboardTests.TheDashboardCarriesTheReconciliationDifferenceOnceThereIsOne` |
| UC-063: completion is expenses as a percentage of revenue; everyone sees it and the cost, and nobody types it | `ProjectFigures.CompletionPercent`, `DashboardEndpoints.Dashboard` | `DashboardTests.UC_063_CompletionIsExpensesAsAPercentageOfRevenue`, `UC_063_EveryoneSeesTheCompletionAndTheCostAndNobodyTypesIt` |
| AC-30.1: an item's subgroups and contract payments add up to its total | `ItemReportEndpoints.OfAsync` | `DashboardTests.AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal` |
| An item of another project is not found (BR-33) | `ItemReportEndpoints.Report` | `DashboardTests.AnItemOfAnotherProjectIsNotFound` |
| An item's own columns print when they show in reports (FR-41, row 60) | `ItemReportEndpoints.OfAsync` | `DashboardTests.AnItemsOwnColumnsPrintWhenTheyShowInReports` |
