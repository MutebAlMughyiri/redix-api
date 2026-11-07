[Documentation](../README.md) › UC-022

# UC-022: Record a purchase expense

**What it is for:** saving a purchase as what was paid: its item, supplier, date, total, account and method, with its file when there is one. This is the first time money enters the ledger.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #40; the app side is redix-app #41. Simplified by redix-api #96 "Simple expense".

## How it fits together

Every movement of money is one row in `Entries`, with a `Kind`; what only one kind has lives in a detail table keyed by the entry (the database design's decision 1). A purchase is a row in `Entries`, one each in `ExpenseDetails` and `PurchaseDetails`, and one in `EntryAttachments` for its file when it has one. The later entries build on these tables.

An expense is its total on its one date (redix-api #96, D10, D11). `Entries.Amount` is the total, `Entries.EntryDate` its date, and both are what every report goes by. The total is the amount typed, or price × quantity when no amount is typed (BR-01). Nothing else is computed or stored: no VAT, no discount, no amount before VAT, and no bill number (D2, D4, D12). The VAT statement works the VAT out of each total in its workbook (UC-056).

A new expense writes no `Invoices` row (D11). That table keeps only the number of a bill entered before 7 November 2026, which the credit note's list of bills still shows (UC-026).

The file is optional, for a draft and a sent expense alike (D3, D16). A file can be put on later, and only the file changes (UC-039, D15). A negative amount is a deduction and says why in its notes (D14).

The id is made on the phone (the database design's principle 4, AC-16.3), so an offline entry has one before it reaches the server. An id already used saves nothing new. UC-029 "Accept offline uploads exactly once" builds on that.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/expenses` | `RecordEntries` | Records a draft, or with `submit` one pending approval, from `{ id, itemId, subgroupId, supplierId, paymentDate, amount, product, unitPrice, quantity, accountId, paymentMethodId, notes, customValues, attachmentId, isCreditNote, originalEntryId, submit }` |
| `GET /projects/{id}/suppliers/search` | `ReadProject` | Puts what the caller used most first, and answers `timesUsed` |

Its refusals are in the endpoint's Swagger summary. Each names a word the app shows.

## Tables

`Entries`, `ExpenseDetails`, `PurchaseDetails` and `EntryAttachments`, as the database design has them. Submitting here sets the status and writes the audit trail's `Submit`; UC-028 "Submit an entry for approval" adds the decision's row and the notification.

## Rules

Tests are in `ExpensesTests` and `SimpleExpenseTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-1: an expense is sent with its item, supplier, date, total, account and method, and no file | `ExpensesEndpoints.SaveAsync` | `SimpleExpenseTests.AC_1_AnExpenseIsSentWithItsItemSupplierDateTotalAccountAndMethodAndNoFile` |
| D10: an expense is its total on its one date | `ExpensesEndpoints.SaveAsync` | `ExpensesTests.D10_AnExpenseIsItsTotalOnItsOneDate` |
| D11: a new expense writes no invoice row, and a draft with no file is sent later with none | `ExpensesEndpoints.SaveAsync` | `SimpleExpenseTests.D11_ANewExpenseWritesNoInvoiceRow`, `D11_ADraftWithNoFileIsSentLaterWithNoFile` |
| D14: a deduction says why in its notes | `EntryRequest.RefusalOfCommon` | `SimpleExpenseTests.D14_ADeductionSaysWhyInItsNotes` |
| BR-01: left without an amount, the total is price × quantity; a typed amount wins | `ExpenseRequest.Total` | `ExpensesTests.BR_01_LeftWithoutAnAmountTheTotalIsThePriceTimesTheQuantity`, `AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped` |
| BR-12: a date inside the project's period, not in the future, in Saudi time | `Entry.IsAcceptableDate`, `Entry.Today` | `ExpensesTests.BR_12_ADateOutsideTheProjectsPeriodOrInTheFutureIsRefused`, `BR_12_TheProjectsFirstDayAndTodayAreAccepted`, `BR_12_TodayIsTheDateInSaudiArabiaNotOnTheServer` |
| BR-19: a supplier from the register, never free text | `Supplier.OfferedTo` | `ExpensesTests.BR_19_ASupplierNotInTheRegisterIsRefused`, `ADisabledSupplierIsRefused`, `FR_44_FieldUsesTheirOwnPendingSupplierAndNotAnothers` |
| Open question 4: Field charges only what they hold; `ChargeAnyAccount` lets Finance charge any active account | `Account.IsOfferedTo` | `ExpensesTests.OpenQuestion4_FieldChargesAnAccountTheyAreACustodianOf`, `OpenQuestion4_FieldCannotChargeAnAccountTheyAreNotACustodianOf`, `ChargeAnyAccountLetsFinanceChargeAnyActiveAccountAndNotADisabledOne` |
| AC-38.1: only an item that takes purchases | `ProjectStructure.OfferedItemAsync` | `ExpensesTests.AC_38_1_AnItemThatTakesNoPurchasesOrIsDisabledOrOfAnotherProjectIsRefused` |
| AC-40.1: a disabled payment method is refused | `ExpensesEndpoints` | `ExpensesTests.AC_40_1_ADisabledPaymentMethodIsRefused` |
| FR-41, AC-41.1: a required column is asked when sending, of its own item only | `ExpenseRequest.Answers` | `ExpensesTests.AC_41_1_ARequiredCustomColumnIsAskedOnlyOfItsItemWhenSending`, `ADraftCanWaitForARequiredColumn`, `FR_41_ABuiltInColumnMadeRequiredIsAskedWhenSending` |
| FR-41: values are stored with the entry, and must fit their column | `ExpenseDetails.CustomValues`, `Column.Accepts` | `ExpensesTests.FR_41_CustomValuesAreStoredWithTheEntryAndAnEmptyOneIsNoValue`, `FR_41_AValueThatDoesNotFitItsColumnIsRefusedNamingIt`, `FR_41_AValueThatFitsItsColumnIsKept`, `AValueForAColumnTheItemDoesNotAskIsRefused` |
| BR-21: a deleted column is no longer asked | `ExpensesEndpoints` | `ExpensesTests.BR_21_ADeletedColumnIsNoLongerAsked` |
| A file, when there is one, is linked to its entry as its invoice | `ExpensesEndpoints.SaveAsync` | `ExpensesTests.TheFileOfAnInvoiceIsLinkedToItsEntryAsItsInvoice` |
| BR-22: one file is on one expense | `EntryAttachments` | `SimpleExpenseTests.BR_22_TheSameFileOnASecondExpenseIsRefused`, `BR_22_TheSameFileSentAgainIsTheSameFileAndIsRefused`, `AFileKeptAndOnNoEntryIsFreeToUse` |
| BR-33: nothing of another project | Each check | `ExpensesTests.BR_33_AFileOfAnotherProjectIsRefusedOnAnEntry`, `BR_33_AnAccountOfAnotherProjectIsRefused`, `BR_33_SomeoneOutsideTheProjectCannotRecordInIt` |
| AC-16.3: an id used twice saves once | `ExpensesEndpoints.Record` | `ExpensesTests.AC_16_3_AnUploadSentAgainLandsOnceAndAnswersTheEntry`, `AnIdSomeoneElseUsedIsRefusedAndTellsThemNothingOfIt` |
| FR-21: who recorded it and its total are in the audit trail | `db.Audit`, one record per table | `ExpensesTests.FR_21_AnExpenseIsInTheAuditTrailWithWhoAndItsTotal`, `ADraftIsCreatedInTheAuditTrailRatherThanSubmitted` |
| AC-43.1: the caller's most used suppliers first | `SuppliersEndpoints.Search` | `ExpensesTests.AC_43_1_TheSearchPutsWhatTheCallerUsedMostFirstAndThenByName`, `AC_43_1_MostUsedIsTheCallersOwnNotSomeoneElses` |

A QR code on the file fills the date, the total and the supplier in the app (UC-024, D21). The API takes no QR code value.
