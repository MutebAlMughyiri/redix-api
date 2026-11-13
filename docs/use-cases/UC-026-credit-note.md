[Documentation](../README.md) › UC-026

# UC-026: Record a credit note or a deduction

**What it is for:** recording money coming back as a negative entry.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #43; the app side is redix-app #44. Simplified by redix-api #96 "Simple expense", and by redix-api #119, whose D4 made a credit note a plain minus row.

## How it fits together

**A credit note**, a مرتجع (BR-18), is a purchase whose «نوع القيد» is «مرتجع» (redix-api #129): «نوع القيد» is a list column of the purchase template, `entryType`, whose choices are «شراء» (+) and «مرتجع» (−), and the sign of the choice taken decides the total's direction. It is typed as a plain amount and kept as a negative `Entries.Amount`: its own row, as in Muteb's statement, so anything that adds up totals, an item total or the VAT statement, takes it away by itself. It names no bill (Muteb, 12 Nov 2026, redix-api #119 D4), so nothing caps it by what a bill counted, and it is approved like any entry, alone, whatever its supplier's bills stand at. The workbook prints its row negative in `#,##0.00;(#,##0.00)`, as his original statement does.

**A deduction** (FR-14) is a negative amount typed as it is. It says why in its notes (D14), and `Entries.Amount` carries the negative, which reduces the item total.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/entries` | `RecordEntries` | Takes `columnValues.entryType`, the id of «مرتجع», and a plain `amount`, or a negative `amount` for a deduction. Answers `invalidAmount` for a credit note typed negative, `invalidColumnValue` for a choice not of the list, and `deductionNeedsNotes` |

## Rules

Tests are in `CreditNoteTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-14.2, D4: a credit note is a negative total of its own that takes from the item | `EntryTable.AmountOf` | `AC_14_2_D4_ACreditNoteIsANegativeTotalOfItsOwnThatTakesFromTheItem` |
| «نوع القيد» is a core list of the purchase template, «شراء» (+) and «مرتجع» (−) (redix-api #129) | `StructureSeed.PurchaseColumns` | `R129_APurchasesNawAlQaydIsAListOfShiraPlusAndMurtajaMinus` |
| The sign of the choice taken decides the total's direction; no choice counts it as typed; a choice not of the list is refused (redix-api #129) | `EntryTable.SignOf`, `Column.SignedChoiceOf`, `Column.Accepts` | `R129_TheSignOfTheChoiceDecidesTheTotalsDirectionAndNoChoiceCountsItAsTyped`, `R129_AChoiceNotOfTheListIsRefused` |
| Every purchase kept negative before the list became «مرتجع», every other «شراء», each figure as it was (redix-api #129, R6) | Migration `EntryTypeAndMovementTypeLists` | `MigrationStepsTests.R129_EntryTypeAndMovementTypeLists_EveryEntryKeepsItsFigureAndTakesTheChoiceItsSignSays` |
| D4: a credit note needs no bill and is held to no bill's total | `EntryTable.RefusalOfTyped` | `D4_ACreditNoteNeedsNoBillAndIsNotHeldToWhatAnyBillCounted` |
| D4: a credit note is approved alone, wherever its bill stands | `ApprovalsEndpoints.Approve` | `D4_ACreditNoteIsApprovedAloneWhereverItsBillStands` |
| D4: what a credit note could return is no longer asked | No route | `D4_WhatACreditNoteCouldReturnIsNoLongerAsked` |
| A credit note is typed as a plain amount | `EntryTable.RefusalOfTyped` | `ACreditNoteIsTypedAsAPlainAmount` |
| AC-14.1: a deduction of 136 reduces the item total and says why in its notes | `EntryRecording` | `AC_14_1_ADeductionOf136ReducesTheItemTotalAndSaysWhyInItsNotes` |
| AC-04.1: a finished project records no credit note | `ProjectChanges` | `AFinishedProjectRecordsNoCreditNote` |

D14, a deduction saying why in its notes, is proved in `SimpleExpenseTests.D14_ADeductionSaysWhyInItsNotes`. The checks that went with the bill, `creditNoteNeedsOriginal`, `unknownOriginalInvoice`, `creditNoteAboveOriginal` and `originalNotApproved`, are recorded in redix-api #107 "Review every validation: decide which checks stay".
