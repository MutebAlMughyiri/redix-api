[Documentation](../README.md) › UC-023

# UC-023: Detect a duplicate invoice

**What it is for:** stopping the same invoice being entered twice.
**Who uses it:** the API, on its own, whenever an expense is recorded. The app's side is in the app's UC-022, whose screen asks.
**Issue:** redix-api #41. It has no app issue.

## How it fits together

`ExpensesEndpoints.Record` looks for the invoice's number from the same supplier before it saves. An expense with a match answers `409 duplicateInvoice` with the earlier entry, and saves only when the request says `confirmDuplicate`.

"The same supplier in every project" needs a reading, because each project now has its own register (BR-33), so one company is a different row in each. The same supplier is the same row, or in another project whoever holds the same VAT number. A supplier with no VAT number has nothing to compare, so only its own row matches.

The earlier entry may be in a project the caller cannot read, and projects are completely separate (BR-33). So `EarlierEntryView` then says only that it exists, with `isVisible` false, and the warning still stands. When several match, it prefers one the caller can read.

The alternative flow, the same invoice attached to a second expense, is refused and cannot be confirmed. BR-22 defines the same invoice by supplier and number, and AC-17.1 lets a person save that after a warning; those two cannot both stop it. So the refusal is for the same file: the attachment already on an entry, or the same bytes sent again (`Attachments.Sha256`). UC-039 "Attach an invoice to a recorded entry" adds its own check on a QR code's supplier and number.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/expenses` | `RecordEntries` | Now also answers `duplicateInvoice` with `earlier`, takes `confirmDuplicate`, and answers `invoiceAlreadyAttached` |

## Tables

No new table. `Invoices.Number` gains an index, because this looks across every project.

## Rules

Tests are in `DuplicateInvoiceTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-17.1: the same supplier and number answer a warning with the earlier entry, and save nothing | `ExpensesEndpoints.EarlierEntryAsync` | `AC_17_1_TheSameSupplierAndNumberAnswerAWarningWithTheEarlierEntryAndSaveNothing`, `TheNumberIsComparedTrimmed` |
| AC-17.1: confirming saves it | `ExpenseRequest.ConfirmDuplicate` | `AC_17_1_ConfirmingTheWarningSavesIt` |
| Another number, or another supplier, is no duplicate | `EarlierEntryAsync` | `AnotherNumberFromTheSameSupplierNeedsNoConfirmation`, `TheSameNumberFromAnotherSupplierNeedsNoConfirmation` |
| FR-17: in every project, by VAT number | `EarlierEntryAsync` | `FR_17_TheSameVatNumberInAnotherProjectOfTheCallerIsTheSameSupplierAndIsNamed`, `ASupplierWithNoVatNumberIsComparedInItsOwnProjectOnly` |
| BR-33: a project the caller cannot read says only that it exists | `EarlierEntryView` | `BR_33_AnEarlierEntryInAProjectTheCallerCannotReadSaysOnlyThatItExists` |
| BR-22: the same file on a second expense is refused, even confirmed | `ExpensesEndpoints.IsFileOnAnEntryAsync` | `BR_22_TheSameFileOnASecondExpenseIsRefusedEvenWhenConfirmed`, `BR_22_TheSameFileSentAgainIsTheSameInvoiceAndIsRefused`, `AFileKeptAndOnNoEntryIsFreeToUse` |
