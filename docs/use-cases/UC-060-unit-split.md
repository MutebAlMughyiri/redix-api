[Documentation](../README.md) › [Use cases](../README.md) › UC-060

# UC-060: Split a shared expense across units

**Purpose:** an expense recorded against the shared unit (مشترك) can say how much of it each of the project's own units carries, so each unit's cost is known (FR-15).

**Who uses it:** whoever enters expenses gives the split; everyone who reads the project sees each unit's total.

| Route | What it does |
| --- | --- |
| `POST /projects/{id}/expenses`, `/expenses/{entryId}` | Takes `unitShares`: each own unit's percentage of an expense on the shared unit |
| `GET /projects/{id}/reports/units` | Each unit's total: what was recorded against it and its part of the split shared expenses, then the shared unit with what was never split |

The parts are `UnitShare` rows, details of the entry like its purchase details: replaced when the entry changes, copied by a reversal, and shown in the entry's view. Only what is approved counts.

| Rule | Code | Test |
| --- | --- | --- |
| FR-15: a split shared expense reaches each unit's total by its percentage | `UnitTotalsEndpoints.Totals` | `UnitSplitTests.ASharedExpenseSplitByPercentageReachesEachUnitsTotal` |
| Only the shared unit is split; the parts are own units, named once, above zero with two decimals at most, together 100 | `EntryRecording.RefusalOfSplitAsync` | `UnitSplitTests.OnlyAnExpenseOnTheSharedUnitIsSplitAndThePartsMakeAHundred` |
| A changed split replaces the old one, and the entry shows it | `ExpensesEndpoints.Build`, `EntryChanges.RemoveDetailsAsync` | `UnitSplitTests.AChangedSplitReplacesTheOldOneAndTheEntryShowsIt` |
| Reversing a split expense takes each unit's part back | `EntryReversal` | `UnitSplitTests.ReversingASplitExpenseTakesEachUnitsPartBack` |
