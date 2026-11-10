[Documentation](../README.md) › UC-026

# UC-026: Record a credit note or a deduction

**What it is for:** recording money coming back as a negative entry.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field. Anyone who reads the project can read what can be returned.
**Issue:** redix-api #43; the app side is redix-app #44. Simplified by redix-api #96 "Simple expense".

## How it fits together

**A credit note** (BR-18) is a purchase with `isCreditNote`, typed as a plain amount and kept as a negative `Entries.Amount`, so anything that adds up totals, an item total or the VAT statement, takes away what the bill counted. It always names the bill it returns, `originalEntryId`, kept as an `EntryLinks` row of type `CreditNoteOf` (D13). Only a credit note names a bill.

The bill has to be the same supplier's in the same project, sent or approved, and positive. What the credit notes naming it hold together cannot go above its total: `ExpensesEndpoints.Creditable` reckons what is left, counting every credit note but a rejected one. A credit note's date may be before its bill's.

**A deduction** (FR-14) is a negative amount with no bill. It says why in its notes (D14), and `Entries.Amount` carries the negative, which reduces the item total.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/entries` | `RecordEntries` | Takes `isCreditNote`, `originalEntryId` and a negative `amount`. Answers `creditNoteNeedsOriginal`, `unknownOriginalInvoice`, `creditNoteAboveOriginal` and `deductionNeedsNotes` |
| `GET /projects/{id}/expenses/creditable?supplierId=` | `ReadProject` | That supplier's bills a credit note can return, each as `{ entryId, number, date, returnable }`: the number of an older bill that had one, empty for the rest |

## Tables

`EntryLinks`, as the database design has it. Only `CreditNoteOf` is used so far.

## Rules

Tests are in `CreditNoteTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-14.2: a credit note is a negative total, linked to its bill, and takes from the item | `PurchaseTable.AmountOf`, `PurchaseTable.DetailsOf` | `AC_14_2_ACreditNoteIsANegativeTotalLinkedToItsBillAndTakesFromTheItem` |
| D13: a credit note needs its bill, and only a credit note names one | `PurchaseTable.RefusalOfTyped` | `D13_ACreditNoteNeedsItsBillAndOnlyACreditNoteNamesOne` |
| D13: a credit note cannot go above what is left of its bill, alone or together | `PurchaseTable.RefusalOfAsync`, `ExpensesEndpoints.CreditableExceptAsync` | `D13_ACreditNoteCannotGoAboveItsBill`, `D13_TheCreditNotesOfOneBillTogetherCannotGoAboveIt`, `ARejectedCreditNoteNoLongerTakesFromItsBillAndADraftOneDoes` |
| Only the same supplier's sent bill | `Creditable` | `OnlyABillOfTheSameSupplierThatWasSentCanBeReturned` |
| A credit note is typed as a plain amount | `PurchaseTable.RefusalOfTyped` | `ACreditNoteIsTypedAsAPlainAmount` |
| AC-14.1: a deduction of 136 reduces the item total and names no bill | `EntryRequest.IsDeduction` | `AC_14_1_ADeductionOf136ReducesTheItemTotalAndNamesNoBill` |
| BR-31: anyone who reads the project sees what can be returned | `Creditable` | `BR_31_AnyoneWhoReadsTheProjectSeesWhatCanBeReturned` |
| FR-21: the link is in the History table | `RedixDbContext.AddHistory` | `FR_21_TheLinkIsInTheHistory` |
| AC-04.1: a finished project records no credit note | `ProjectChanges` | `AFinishedProjectRecordsNoCreditNote` |

D14, a deduction saying why in its notes, is proved in `SimpleExpenseTests.D14_ADeductionSaysWhyInItsNotes`.
