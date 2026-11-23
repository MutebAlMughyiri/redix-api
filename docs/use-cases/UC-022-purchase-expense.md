[Documentation](../README.md) › UC-022

# UC-022: Record a purchase expense

**What it is for:** saving a purchase as what was paid: its item, date, total, account and method, its supplier when it names one, with its file when there is one. This is the first time money enters the ledger.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #40; the app side is redix-app #41. Simplified by redix-api #96 "Simple expense".

## How it fits together

Every movement of money is one row in `Entries`, with a `Kind`; what only one kind has lives in a detail table keyed by the entry (the database design's decision 1). A purchase is a row in `Entries`, one each in `ExpenseDetails` and `PurchaseDetails`, and one in `EntryAttachments` for its file when it has one. The later entries build on these tables.

An expense is its total on its one date (redix-api #96, D10, D11). `Entries.Amount` is the total, `Entries.EntryDate` its date, and both are what every report goes by. The total is the amount typed, or price × quantity when no amount is typed and the entry gives both (BR-01). The unit price and the quantity are the item's columns like any other: a table may have none of them, and their values are kept in `Entries.ColumnValues` under their keys with every other column's (redix-api #129, R2, R5). Every entry has one description, «الوصف» (`Entries.Description`, key `description`), first among its columns: «الصنف» and «ملاحظات» went into it (the owner, 13 Nov 2026), joined as «product · notes» where both were typed. The supplier is optional (the owner, 13 Nov 2026): a purchase that names one has a `PurchaseDetails` row, and one that names none has no row. Nothing else is computed or stored: no VAT, no discount, no amount before VAT, and no bill number (D2, D4, D12). The VAT statement works the VAT out of each total in its workbook (UC-056).

A new expense writes no `Invoices` row (D11). That table keeps only the number of a bill entered before 7 November 2026, which the credit note's list of bills still shows (UC-026).

The file is optional, for a draft and a sent expense alike (D3, D16). A file can be put on later, and only the file changes (UC-039, D15). A negative amount is a deduction and says why in its description (D14).

The id is made on the phone (the database design's principle 4, AC-16.3), so an offline entry has one before it reaches the server. An id already used saves nothing new. UC-029 "Accept offline uploads exactly once" builds on that.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/entries` | `RecordEntries`, through the table on offer (redix-api #110) | Records a draft, or with `submit` one pending approval, from `{ id, itemId, tableId, supplierId, date, amount, accountId, paymentMethodId, description, columnValues, attachmentId, submit }`; `supplierId` left out names none, and `columnValues` holds the unit price and the quantity under `unitPrice` and `quantity`, and «نوع القيد» under `entryType` |
| `GET /projects/{id}/suppliers/search` | `ReadProject` | Puts what the caller used most first, and answers `timesUsed` |

Its refusals are in the endpoint's Swagger summary. Each names a word the app shows.

## Tables

`Entries`, `ExpenseDetails`, `PurchaseDetails` and `EntryAttachments`, as the database design has them. Submitting here sets the status and writes the History table's `Submit`; UC-028 "Submit an entry for approval" adds the decision's row and the notification.

## Rules

Tests are in `ExpensesTests` and `SimpleExpenseTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| redix-app #109 R5: an expense goes only through an account its payment method pays from, otherwise `accountNotForMethod` (400), checked after `accountNotOffered` | `entries.refusalOfPayment` | `settings_test.TestR5_AnExpenseGoesOnlyThroughAnAccountItsMethodPaysFrom` |
| AC-1: an expense is sent with its item, supplier, date, total, account and method, and no file | `entries.Routes.save` | `SimpleExpenseTests.AC_1_AnExpenseIsSentWithItsItemSupplierDateTotalAccountAndMethodAndNoFile` |
| D10: an expense is its total on its one date | `entries.Routes.save` | `ExpensesTests.D10_AnExpenseIsItsTotalOnItsOneDate` |
| D11: a new expense writes no invoice row, and a draft with no file is sent later with none | `entries.Routes.save` | `SimpleExpenseTests.D11_ANewExpenseWritesNoInvoiceRow`, `D11_ADraftWithNoFileIsSentLaterWithNoFile` |
| D14: a deduction says why in its description | `entries.refusal` | `SimpleExpenseTests.D14_ADeductionSaysWhyInItsNotes` |
| BR-01: left without an amount, the total is price × quantity; a typed amount wins | `reports.totalOf` | `ExpensesTests.BR_01_LeftWithoutAnAmountTheTotalIsThePriceTimesTheQuantity`, `AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped` |
| The unit price and the quantity are kept with the entry as every other column is, and a table without one is refused its value; there is no product column (redix-api #129, R2, R5) | `entries.Record`, `entries.refusalOfColumns` | `ExpensesTests.R129_R5_TheUnitPriceAndTheQuantityAreKeptWithTheEntryAsEveryOtherColumnIs`, `R129_R2_ATableWithoutTheUnitPriceIsRefusedOneAndTakesItsAmountTyped`, `WhatWasTypedIsRefusedWhenItCannotBeKept` |
| D1: every entry has one description, «الوصف», and a purchase's product and notes became it, «product · notes» where both were typed (the owner, 13 Nov 2026, redix-api #129) | `entries.Request.description`, migration `DescriptionForEveryEntry` | `ExpensesTests.AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped`, `MigrationStepsTests.R129_D1_DescriptionForEveryEntry_AProductAndItsNotesBecomeOneDescriptionAndTheColumnsTakeTheWorkbooksWords` |
| Every value, the product, the price and the quantity moved to the entry unchanged, every figure as it was, and the supplier column is optional on every item (redix-api #129, R6) | Migration `ColumnValuesOnTheEntry` | `MigrationStepsTests.R129_R6_ColumnValuesOnTheEntry_EveryValueTheProductThePriceAndTheQuantityMoveToTheEntryUnchangedAndTheSupplierIsOptional` |
| The supplier is optional: a purchase with none is sent and holds no supplier row; one that names a supplier keeps it (the owner, 13 Nov 2026, redix-api #129) | `entries.purchaseForm.keep`, `reports.detailsOf` | `ExpensesTests.R129_ThePurchasesSupplierIsOptionalSoOneWithNoneIsSentAndHoldsNoSupplierRow`, `R129_APurchaseThatNamesASupplierKeepsItsRow`, `ReportFilesTests.R129_APurchaseWithNoSupplierIsOnTheVatStatementWithNoPartyAndNoVatNumber` |
| BR-12: a date inside the project's period and not after its today, counted in the project's time zone, Asia/Riyadh until its manager chooses another (redix-app #107) | `entries.isAcceptableDate`, `zones.Today` | `entries_test.TestBR12_AnEntrysTodayIsItsProjectsTodayInItsZone`, `zones.TestBR12_AProjectsTodayIsTheServersClockInItsZone` |
| BR-19: a supplier named is from the register, never free text | `suppliers.Routes.search` | `ExpensesTests.BR_19_ASupplierNotInTheRegisterIsRefused`, `ADisabledSupplierIsRefused`, `FR_44_FieldUsesTheirOwnPendingSupplierAndNotAnothers` |
| Open question 4: Field charges only what they hold; `ChargeAnyAccount` lets Finance charge any active account | `accounts.ListOf` | `ExpensesTests.OpenQuestion4_FieldChargesAnAccountTheyAreACustodianOf`, `OpenQuestion4_FieldCannotChargeAnAccountTheyAreNotACustodianOf`, `ChargeAnyAccountLetsFinanceChargeAnyActiveAccountAndNotADisabledOne` |
| AC-38.1: only an item that takes purchases | `structure.Offers` | `ExpensesTests.AC_38_1_AnItemThatTakesNoPurchasesOrIsDisabledOrOfAnotherProjectIsRefused` |
| AC-40.1: a disabled payment method is refused | `entries.refusalOfPayment` | `ExpensesTests.AC_40_1_ADisabledPaymentMethodIsRefused` |
| FR-41, AC-41.1: a required column is asked when sending, of its own item only, and only what its kind of entry is asked | `entries.AskedColumns` | `ExpensesTests.AC_41_1_ARequiredCustomColumnIsAskedOnlyOfItsItemWhenSending`, `ADraftCanWaitForARequiredColumn`, `FR_41_ABuiltInColumnMadeRequiredIsAskedWhenSending` |
| FR-41: values are stored with the entry, and must fit their column | `entries.Record`, `structure.Accepts` | `ExpensesTests.FR_41_CustomValuesAreStoredWithTheEntryAndAnEmptyOneIsNoValue`, `FR_41_AValueThatDoesNotFitItsColumnIsRefusedNamingIt`, `FR_41_AValueThatFitsItsColumnIsKept`, `AValueForAColumnTheItemDoesNotAskIsRefused` |
| BR-21: a deleted column is no longer asked | `entries.AskedColumns` | `ExpensesTests.BR_21_ADeletedColumnIsNoLongerAsked` |
| A file, when there is one, is linked to its entry as its invoice | `entries.Routes.save` | `ExpensesTests.TheFileOfAnInvoiceIsLinkedToItsEntryAsItsInvoice` |
| BR-22: one file is on one expense | `EntryAttachments` | `SimpleExpenseTests.BR_22_TheSameFileOnASecondExpenseIsRefused`, `BR_22_TheSameFileSentAgainIsTheSameFileAndIsRefused`, `AFileKeptAndOnNoEntryIsFreeToUse` |
| BR-33: nothing of another project | Each check | `ExpensesTests.BR_33_AFileOfAnotherProjectIsRefusedOnAnEntry`, `BR_33_AnAccountOfAnotherProjectIsRefused`, `BR_33_SomeoneOutsideTheProjectCannotRecordInIt` |
| AC-16.3: an id used twice saves once | `entries.Routes.record` | `ExpensesTests.AC_16_3_AnUploadSentAgainLandsOnceAndAnswersTheEntry`, `AnIdSomeoneElseUsedIsRefusedAndTellsThemNothingOfIt` |
| FR-21: who recorded it and its total are in the History table | `history.Write`, one record per table | `ExpensesTests.FR_21_AnExpenseIsInTheHistoryWithWhoAndItsTotal`, `ADraftIsCreatedInTheHistoryRatherThanSubmitted` |
| AC-43.1: the caller's most used suppliers first | `suppliers.Routes.search` | `ExpensesTests.AC_43_1_TheSearchPutsWhatTheCallerUsedMostFirstAndThenByName`, `AC_43_1_MostUsedIsTheCallersOwnNotSomeoneElses` |

No QR code is read: the invoice QR was removed from the app, the web and the API (the owner, 23 Nov 2026, findings row 303).

## On the web: Add entry (redix-api #166)

The web's Add entry is the app's screen in a tab of its own, opened by Entries' Add entry button (gray, with its reason, without RecordEntries or in a finished project, as the app's is). It offers what the app's routes answer: the tables from `entries.Offered`, each named as the app's picker names it; the table's own columns from `entries.AskedColumns`, drawn from `structure.ColumnOf`; the payment methods from `settings.MethodsOf` and the accounts from `accounts.ListOf`, only those the method pays from; a purchase's suppliers from `suppliers.Search`. Changing the table, the method or the supplier search draws the form again with what was typed. Saving records through `entries.Record`, the rule the app's route and a project file's load run, as a draft or sent for approval. For whoever may attach files, the form takes the entry's file as the app's section does - an expense's invoice, a revenue's file, a contractor's or a partner's receipt, a picture or a PDF - kept before the entry through `attachments.Keep`, the rule `POST /projects/{id}/attachments` runs, and named by the entry; a refused form carries the file it kept, so it is not chosen again, and the file field stands outside the form, joined to it, so changing the table keeps it chosen (#175 D3). Its pages are in `go-backend/web` (`addentry.go`, `addentry.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/addEntry?table=&method=&supplierText=&…` | Add entry, drawn with what was typed | `200` the tab's content |
| `GET /entry-figures?table=&amount=&outside=&entry=` | A contract payment's figures as the form stands, drawn again as the amount or inside and outside change | `200` the figures |
| `POST /entry` | Records the form, `submit=draft` or `submit=send` | `200` the form again: emptied and saying it was kept, the new entry in the details panel; or as typed, saying why not and marking the column refused |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Add entry offers the caller's tables, methods and accounts, records through the app's rule, and shows the new entry (UC-022, FR-41) | `entries.Record`, `web.site.recordEntry` | `TestWeb_R166_UC022_AddEntryRecordsThroughTheAppsRuleAndShowsTheEntry` |
| What cannot be saved says why and keeps what was typed: an amount that is no number, a refusal of the API's (BR-12); a finished project records nothing (AC-04.1) | `web.entryForm.whatIsWrong`, `entries.Record` | `TestWeb_R166_UC022_AddEntrySaysWhyNotAndKeepsWhatWasTyped` |
| A failing database answers 500 and keeps nothing | `changes.Writer.Run` | `TestWeb_R166_AddEntryAnswersAFailingDatabaseWith500` |
| A contract payment shows the contract's value, what is paid and what remains after it; past the value it warns and still saves, saying the contract is overrun; outside the contract it takes nothing from what remains (BR-07, FR-13) | `contracts.StandingOf`, the one owner a payment's answer asks too | `TestWeb_R166_BR07_AContractPaymentShowsTheContractsFiguresAsTheAmountChanges` |
| Add entry takes the entry's file for whoever may attach files, kept before the entry and carried by a refused form; one that is no picture or PDF is refused in the app's words (#175 D3) | `web.keepEntryFile`, `attachments.Keep`, `entries.Record` | `TestWeb_R175_D3_AddEntryKeepsTheFileItTakesAsTheAppsInvoiceSection` |
