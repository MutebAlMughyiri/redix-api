[Documentation](../README.md) › UC-027

# UC-027: Record a contractor payment

**What it is for:** paying a contractor against a contract and keeping its remaining right.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #44; the app side is redix-app #45.

## How it fits together

A payment is a row in `Entries` of kind `ContractorPayment`, one in `ExpenseDetails` and one in `ContractorPaymentDetails`. The contract says the item and the contractor. It goes to the item's general subgroup, because neither the issue nor the canvas asks for another.

Both kinds of expense share `EntryRequest` and `EntryRecording`: the payment method, the account (`ChargeAnyAccount` and `AccountCustodians`, BRS open question 4), the file, the custom columns, the invoice's number, and how an entry reaches the ledger with its audit records. `EntryAnswers` owns which column a kind of entry answers, so what only a purchase holds is never asked of a payment.

**`ContractMoney` is the one owner of what a contract holds.** `Paid` is the approved payments inside it, and `Pending` the ones waiting, each by `Entries.Amount`, so a deduction takes away (AC-14.1). A payment outside the contract is neither (AC-13.1), and a draft or a rejected one is neither. BR-10 says only the approved counts, and the database design says the same, so that is what `Paid`, `Remaining` and the contract's overrun read. UC-032 "Approve or reject entries" only has to change an entry's status for a payment to move across.

**The alert while entering looks further.** `ContractorPaymentView` answers the remaining with everything paid or waiting, and this payment if inside. Otherwise two pending payments could overrun a contract with nobody told, and until UC-032 nothing is approved at all. Going beyond the value is saved, with the alert (AC-07.1). An open-value contract has no remaining and no alert (AC-07.2).

**A deduction** is a negative amount (FR-14). No money moved, so there is no receipt: it says why in `withoutInvoiceReason`, carries no file and no tax invoice, and shows in the exceptions list.

**The tax invoice** is optional. It takes a number, a date and the amount before VAT; `InvoiceFigures.Of` computes the VAT, at 0 for a contractor with no VAT number. It is a row in `Invoices`, so the VAT statement finds it, with the contractor as its supplier (the design's rule). Its number is checked against purchases and payments alike (FR-17).

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/contractor-payments` | `RecordEntries` | Records a payment as a draft, or sent. Answers what the contract stands at with it |
| `GET /projects/{id}/contracts` | `ReadProject` | Now answers real `paid`, `pending`, `remaining` and `isOverrun`, and `itemId` and `supplierVatNumber` for the entry form |

## Tables

`ContractorPaymentDetails`, as the database design has it.

## Rules

Tests are in `ContractorPaymentsTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A payment goes under the subgroup it names, of its contract's item, or the general one | `ContractorPaymentsEndpoints` | `ContractorPaymentsTests.APaymentGoesUnderTheSubgroupItNames` |
| AC-07.1: a real 30,000 leaves 4,000, and 5,000 more is saved with an alert and (1,000) | `ContractMoney`, `Contract.RemainingAfter`, `IsOverrunBy` | `AC_07_1_ThePaymentsOfA34000ContractLeaveItsRemainingAndOneMoreOverrunsItWithAnAlertAndIsSaved` |
| AC-07.2: an open-value contract only adds up | `Contract` | `AC_07_2_AnOpenValueContractAddsUpWithNoRemainingAndNoAlert` |
| BR-10: only an approved payment is paid; a pending one waits beside it | `ContractMoney` | `BR_10_OnlyAnApprovedPaymentIsPaidAndAPendingOneWaitsBesideIt`, `ADraftAndARejectedPaymentCountForNothing` |
| The alert counts what waits | `ContractorPaymentsEndpoints.Record` | `TheAlertWhileEnteringCountsWhatWaitsSoTwoPendingPaymentsCannotOverrunUnseen` |
| AC-13.1: outside the contract it enters the item total and leaves the remaining | `ContractMoney` | `AC_13_1_APaymentOutsideTheContractEntersTheItemTotalAndLeavesTheRemainingAlone` |
| AC-14.1: a deduction of 136 reduces the item total and the paid | `Entries.Amount` | `AC_14_1_ADeductionOf136ReducesTheItemTotalAndThePaidOnTheContract`, `ADeductionSaysWhyInPlaceOfAReceiptAndOnlyADeductionMay` |
| AC-12.1: sending needs the receipt | `Record` | `AC_12_1_SendingNeedsTheReceiptAndADraftCanWaitForIt`, `TheReceiptIsLinkedAsAReceiptAndAFileAlreadyOnAnEntryIsRefused` |
| AC-38.1: an item that takes no contractor payments | `ProjectStructure.OfferedItemAsync` | `AC_38_1_AContractWhoseItemIsDisabledTakesNoPayment` |
| The tax invoice enters the statement, its VAT computed here | `InvoiceFigures.Of` | `ATaxInvoiceEntersTheStatementWithItsVatComputedHereAndTheContractorAsItsSupplier`, `APaymentWithNoTaxInvoiceHasNoInvoiceRow`, `AC_17_1_TheSameInvoiceNumberFromTheContractorWarnsOnAPaymentAndOnAPurchase` |
| Open question 4: only what the user may charge | `EntryRecording.RefusalOfPaymentAsync` | `OpenQuestion4_FieldChargesAnAccountTheyHoldAndNoOther` |
| FR-41: its columns, and never a purchase's | `EntryAnswers` | `FR_41_ARequiredCustomColumnIsAskedOfAPaymentWhenSending`, `FR_41_WhatOnlyAPurchaseHoldsIsNeverAskedOfAPayment` |
| What was typed; who may; a finished project; the audit trail | `ContractorPaymentRequest`, `Permission`, `ProjectChanges` | `WhatWasTypedIsRefusedWhenItCannotBeKept`, `ThePaymentTakesItsItemAndUnitFromTheContractInTheItemsGeneralSubgroup`, `ADisabledContractOrOneOfAnotherProjectIsRefused`, `WhoHoldsNoRecordEntriesCannotRecordAPayment`, `AC_04_1_AFinishedProjectRecordsNoPayment`, `FR_21_APaymentIsInTheAuditTrail`, `AC_16_3_AnIdAlreadyUsedSavesNothingNewAndAnEmptyOneIsRefused` |

Since nothing can be approved until UC-032, the tests record a real payment through this route and then change its status directly, which is all that use case has to do.
