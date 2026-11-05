[Documentation](../README.md) › [Use cases](../README.md) › UC-049, UC-050, UC-051, UC-063

# UC-049, UC-050, UC-051, UC-063: The dashboard, the item report, the contracts, completion and cost

**Purpose:** give a project's figures at a glance, everything spent on one item, each contract's value, paid and remaining, and how far the work has come.

**Who uses it:** everyone who reads the project. Whoever holds `ManageProject` sets the completion.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/dashboard` | Revenue, expenses, surplus with and without sales, the reconciliation difference, what waits, each category and item's total, the completion |
| `POST /projects/{id}/completion` | Sets how far the work has come |
| `GET /projects/{id}/reports/items/{itemId}` | An item's purchases by subgroup, then each contract with its payments |
| `GET /projects/{id}/contracts` | Each contract's value, paid, remaining and overrun (UC-020), which is the contracts report |

`ItemTotals` is the one owner of what each item cost (BR-02). The dashboard reads it on every request, so an approval shows at once.

**UC-063 had five open questions**, decided without Muteb and listed in the decisions: completion is one percentage for the project, typed by whoever holds `ManageProject`, kept in the audit trail; cost is the approved expenses; everyone sees both.

| Rule | Code | Test |
| --- | --- | --- |
| AC-34.1: an approval shows at once in the surplus, the pending and the categories | `DashboardEndpoints.Dashboard`, `ItemTotals` | `DashboardTests.AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories` |
| The dashboard carries the reconciliation difference once there is one | `DashboardEndpoints.Dashboard` | `DashboardTests.TheDashboardCarriesTheReconciliationDifferenceOnceThereIsOne` |
| UC-063: the manager sets the completion; everyone sees it and the cost | `DashboardEndpoints.SetCompletion`, `Project.SetCompletion` | `DashboardTests.UC_063_TheManagerSetsTheCompletionAndEveryoneSeesItAndTheCost` |
| AC-30.1: an item's subgroups and contract payments add up to its total | `ItemReportEndpoints.OfAsync` | `DashboardTests.AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal` |
| An item of another project is not found (BR-33) | `ItemReportEndpoints.Report` | `DashboardTests.AnItemOfAnotherProjectIsNotFound` |
| An item's own columns print when they show in reports (FR-41, row 60) | `ItemReportEndpoints.OfAsync` | `DashboardTests.AnItemsOwnColumnsPrintWhenTheyShowInReports` |
