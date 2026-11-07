[Documentation](../README.md) › UC-022

# UC-022: Record a purchase expense

**What it is for:** saving a purchase with its invoice data, computed exactly as the accountant computes it. This is the first time money enters the ledger.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #40; the app side is redix-app #41.

## How it fits together

Every movement of money is one row in `Entries`, with a `Kind`; what only one kind has lives in a detail table keyed by the entry (the database design's decision 1). A purchase is a row in `Entries`, one each in `ExpenseDetails`, `PurchaseDetails` and `Invoices`, and one in `EntryAttachments` for its file. The later entries build on these tables.

The app sends what was typed and never a figure. `InvoiceFigures.Of` is the one owner of the arithmetic: the amount, including VAT or not; `before = including ÷ (1 + rate)`; `after = before − discount`; `VAT = after × rate`; `total = after + VAT`. It runs on the unrounded amounts, and each figure is kept with the four decimals of its column, so nothing is rounded to two decimals before it is stored (BR-13). 6,000.00 including VAT stores 5,217.3913, 782.6087 and 6,000.0000. Rounding to two decimals belongs to display and totals.

The rate is the project's at the moment of saving, 0 for a supplier not VAT-registered or an invoice not taxable. It is stored as the VAT and never recomputed, so a later change of the rate leaves the entry alone (findings row 6).

`Entries.EntryDate` and `Entries.Amount` are what the periodic report goes by, and `Invoices` is what the VAT statement goes by. An expense is paid its total on its own date: the invoice's, or the payment date of one without an invoice (redix-api #96, D10). BR-24's payment that differs from its invoice, with its amount paid and bank fees, went on 7 November 2026; an older bill whose payment differed keeps the amount it stored.

The id is made on the phone (the database design's principle 4, AC-16.3), so an offline entry has one before it reaches the server. An id already used saves nothing new. UC-029 "Accept offline uploads exactly once" builds on that.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/expenses` | `RecordEntries` | Records a draft, or with `submit` one pending approval, and answers its figures |
| `GET /projects/{id}/suppliers/search` | `ReadProject` | Now puts what the caller used most first, and answers `timesUsed` |

Its refusals are in the endpoint's Swagger summary. Each names a word the app shows.

## Tables

`Entries`, `ExpenseDetails`, `PurchaseDetails`, `Invoices` and `EntryAttachments`, as the database design has them. `EntryLinks` and `EntryDecisions` come with the use cases that need them. Submitting here sets the status and writes the audit trail's `Submit`; UC-028 "Submit an entry for approval" adds the decision's row and the notification.

## Rules

Tests are in `ExpensesTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-08.1: 6,000.00 including VAT stores 5,217.3913 / 782.6087 / 6,000.0000 | `InvoiceFigures.Of` | `AC_08_1_AnAmountIncludingVatKeepsFourDecimalsAndAddsUpToWhatWasTyped`, `AC_08_1_TheFiguresReachTheDatabaseWithTheirFourDecimals` |
| AC-08.2: VAT on 2,569.90 is 385.485, total 2,955.385 | `InvoiceFigures.Of` | `AC_08_2_VatOnAnAmountBeforeVatIsNotRounded` |
| AC-08.3: the discount comes off before VAT | `InvoiceFigures.Of` | `AC_08_3_TheDiscountComesOffBeforeVat`, `ADiscountOnAnAmountIncludingVatComesOffTheAmountBeforeVat` |
| BR-14: the rate comes from the project, at several rates | `InvoiceFigures.Of`, `ExpensesEndpoints.Record` | `TheVatFollowsTheRateOfTheProject`, `AtEveryRateTheStoredFiguresAddUpAndAnAmountIncludingVatStaysExactly`, `BR_14_TheRateIsTheProjectsAndAnEarlierEntryKeepsTheVatItWasSavedWith` |
| A whole invoice with no credit note adds up | `InvoiceFigures.Of` | `AWholeInvoiceWithNoCreditNoteAddsUpFromItsTypedAmountsToItsTotals` |
| BR-13: no figure is rounded to two decimals before it is stored; a total rounds once | `InvoiceFigures.Kept`, `RedixDbContext.ConfigureConventions` | `AFigureIsNeverRoundedToTwoDecimalsBeforeItIsStored`, `BR_13_VatStaysUnroundedOnEachRowSoATotalRoundsOnlyOnce` |
| BR-14: a supplier not VAT-registered, or an invoice not taxable, has no VAT | `ExpensesEndpoints.Record` | `BR_14_ASupplierNotVatRegisteredHasNoVatAndTheAmountStaysWhole`, `BR_14_AnInvoiceNotTaxableHasNoVat` |
| BR-01: price × quantity includes VAT; a typed amount wins | `ExpenseRequest.EnteredAmount` | `BR_01_LeftWithoutAnAmountThePriceTimesTheQuantityIncludesVat`, `AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped` |
| D10: an expense is paid its total on its own date | `ExpensesEndpoints.Build` | `D10_AnExpenseIsPaidItsInvoicesTotalOnTheInvoicesDate` |
| BR-17: the VAT period is the quarter of the invoice | `Invoice.QuarterStartOf` | `BR_17_TheVatPeriodIsTheQuarterOfTheInvoicesDate` |
| BR-12: a date inside the project's period, not in the future, in Saudi time | `Entry.IsAcceptableDate`, `Entry.Today` | `BR_12_ADateOutsideTheProjectsPeriodOrInTheFutureIsRefused`, `BR_12_TheProjectsFirstDayAndTodayAreAccepted`, `BR_12_TodayIsTheDateInSaudiArabiaNotOnTheServer` |
| BR-19: a supplier from the register, never free text | `Supplier.OfferedTo` | `BR_19_ASupplierNotInTheRegisterIsRefused`, `ADisabledSupplierIsRefused`, `FR_44_FieldUsesTheirOwnPendingSupplierAndNotAnothers` |
| Open question 4: Field charges only what they hold; `ChargeAnyAccount` lets Finance charge any active account | `Account.IsOfferedTo` | `OpenQuestion4_FieldChargesAnAccountTheyAreACustodianOf`, `OpenQuestion4_FieldCannotChargeAnAccountTheyAreNotACustodianOf`, `ChargeAnyAccountLetsFinanceChargeAnyActiveAccountAndNotADisabledOne` |
| AC-38.1: only an item that takes purchases | `ProjectStructure.OfferedItemAsync` | `AC_38_1_AnItemThatTakesNoPurchasesOrIsDisabledOrOfAnotherProjectIsRefused` |
| AC-40.1: a disabled payment method is refused | `ExpensesEndpoints` | `AC_40_1_ADisabledPaymentMethodIsRefused` |
| FR-41, AC-41.1: a required column is asked when sending, of its own item only | `ExpenseRequest.Answers` | `AC_41_1_ARequiredCustomColumnIsAskedOnlyOfItsItemWhenSending`, `ADraftCanWaitForARequiredColumn`, `FR_41_ABuiltInColumnMadeRequiredIsAskedWhenSending` |
| FR-41: values are stored with the entry, and must fit their column | `ExpenseDetails.CustomValues`, `Column.Accepts` | `FR_41_CustomValuesAreStoredWithTheEntryAndAnEmptyOneIsNoValue`, `FR_41_AValueThatDoesNotFitItsColumnIsRefusedNamingIt`, `FR_41_AValueThatFitsItsColumnIsKept`, `AValueForAColumnTheItemDoesNotAskIsRefused` |
| BR-21: a deleted column is no longer asked | `ExpensesEndpoints` | `BR_21_ADeletedColumnIsNoLongerAsked` |
| FR-09: sending needs the invoice's file; a draft can wait | `ExpensesEndpoints.Record` | `FR_09_SendingNeedsTheInvoicesFileAndADraftCanWaitForIt`, `TheFileOfAnInvoiceIsLinkedToItsEntryAsItsInvoice` |
| BR-33: nothing of another project | Each check | `BR_33_AFileOfAnotherProjectIsRefusedOnAnEntry`, `BR_33_AnAccountOfAnotherProjectIsRefused`, `BR_33_SomeoneOutsideTheProjectCannotRecordInIt` |
| AC-16.3: an id used twice saves once | `ExpensesEndpoints.Record` | `AC_16_3_AnIdAlreadyUsedSavesNothingNewAndAnEmptyOneIsRefused` |
| FR-21: who recorded it and its figures are in the audit trail | `db.Audit`, one record per table | `FR_21_AnExpenseIsInTheAuditTrailWithWhoAndItsFigures`, `ADraftIsCreatedInTheAuditTrailRatherThanSubmitted` |
| AC-43.1: the caller's most used suppliers first | `SuppliersEndpoints.Search` | `AC_43_1_TheSearchPutsWhatTheCallerUsedMostFirstAndThenByName`, `AC_43_1_MostUsedIsTheCallersOwnNotSomeoneElses` |

AC-11.3, a warning when a QR code's VAT differs from the computed VAT, is the app's: UC-024 has no API work, and the warning stops nothing, so the API never needs the QR code's figure. See the app's UC-024.
