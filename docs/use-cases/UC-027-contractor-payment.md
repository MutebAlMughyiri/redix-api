[Documentation](../README.md) › UC-027

# UC-027: Record a contractor payment

**What it is for:** paying a contractor against a contract and keeping its remaining right.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field.
**Issue:** redix-api #44; the app side is redix-app #45. Simplified by redix-api #96 "Simple expense".

## How it fits together

A payment is a row in `Entries` of kind `ContractorPayment`, one in `ExpenseDetails` and one in `ContractorPaymentDetails`. The contract says the item and the contractor. It goes under the item's general subgroup: its contract is its table on the item's sheet, so it names no subgroup (redix-app #84, R11).

Both kinds of expense share `EntryRequest` and `EntryRecording`: the payment method, the account (`ChargeAnyAccount` and `AccountCustodians`, BRS open question 4), the file, the custom columns, and how an entry reaches the ledger with its audit records. `EntryAnswers` owns which column a kind of entry answers, so what only a purchase holds is never asked of a payment.

**`ContractMoney` is the one owner of what a contract holds.** `Paid` is the approved payments inside it, and `Pending` the ones waiting, each by `Entries.Amount`, so a deduction takes away (AC-14.1). A payment outside the contract is neither (AC-13.1), and a draft or a rejected one is neither. BR-10 says only the approved counts, so that is what `Paid`, `Remaining` and the contract's overrun read.

**The alert while entering looks further.** `ContractorPaymentView` answers the remaining with everything paid or waiting, and this payment if inside. Otherwise two pending payments could overrun a contract with nobody told. Going beyond the value is saved, with the alert (AC-07.1). An open-value contract has no remaining and no alert (AC-07.2).

**The receipt is optional** (D16), for a draft and a sent payment alike, and is linked as the payment's receipt when there is one. A payment has no tax invoice and writes no `Invoices` row.

**A deduction** is a negative amount (FR-14). No money moved, so it has no receipt, and it says why in its notes (D14).

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/contractor-payments` | `RecordEntries` | Records a payment as a draft, or sent, from `{ id, contractId, isOutsideContract, amount, paymentDate, accountId, paymentMethodId, notes, customValues, attachmentId, submit }`. Answers what the contract stands at with it |
| `GET /projects/{id}/contracts` | `ReadProject` | Answers real `paid`, `pending`, `remaining` and `isOverrun`, and `itemId` for the entry form |

## Tables

`ContractorPaymentDetails`, as the database design has it.

## Rules

Tests are in `ContractorPaymentsTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| R11: a payment goes under its item's general subgroup, even when the item has named ones | `ContractorPaymentsEndpoints` | `R11_APaymentGoesUnderItsItemsGeneralSubgroupEvenWhenTheItemHasNamedOnes`, `ThePaymentTakesItsItemAndUnitFromTheContractInTheItemsGeneralSubgroup` |
| AC-07.1: a real 30,000 leaves 4,000, and 5,000 more is saved with an alert and (1,000) | `ContractMoney`, `Contract.RemainingAfter`, `IsOverrunBy` | `AC_07_1_ThePaymentsOfA34000ContractLeaveItsRemainingAndOneMoreOverrunsItWithAnAlertAndIsSaved` |
| AC-07.2: an open-value contract only adds up | `Contract` | `AC_07_2_AnOpenValueContractAddsUpWithNoRemainingAndNoAlert` |
| BR-10: only an approved payment is paid; a pending one waits beside it | `ContractMoney` | `BR_10_OnlyAnApprovedPaymentIsPaidAndAPendingOneWaitsBesideIt`, `ADraftAndARejectedPaymentCountForNothing` |
| The alert counts what waits | `ContractorPaymentsEndpoints.Record` | `TheAlertWhileEnteringCountsWhatWaitsSoTwoPendingPaymentsCannotOverrunUnseen` |
| AC-13.1: outside the contract it enters the item total and leaves the remaining | `ContractMoney` | `AC_13_1_APaymentOutsideTheContractEntersTheItemTotalAndLeavesTheRemainingAlone` |
| AC-14.1: a deduction of 136 reduces the item total and the paid | `Entries.Amount` | `AC_14_1_ADeductionOf136ReducesTheItemTotalAndThePaidOnTheContract` |
| D14: a deduction says why in its notes | `EntryRequest.RefusalOfCommon` | `D14_ADeductionSaysWhyInItsNotes` |
| D16: a payment is sent with no receipt, and writes no invoice row | `ContractorPaymentsEndpoints.Record` | `D16_APaymentIsSentWithNoReceipt`, `D16_APaymentWritesNoInvoiceRow` |
| A receipt is linked as a receipt; a file already on an entry is refused | `EntryRecording` | `TheReceiptIsLinkedAsAReceiptAndAFileAlreadyOnAnEntryIsRefused` |
| AC-38.1: an item that takes no contractor payments | `ProjectStructure.OfferedItemAsync` | `AC_38_1_AContractWhoseItemIsDisabledTakesNoPayment` |
| Open question 4: only what the user may charge | `EntryRecording.RefusalOfPaymentAsync` | `OpenQuestion4_FieldChargesAnAccountTheyHoldAndNoOther` |
| FR-41: its columns, and never a purchase's | `EntryAnswers` | `FR_41_ARequiredCustomColumnIsAskedOfAPaymentWhenSending`, `FR_41_WhatOnlyAPurchaseHoldsIsNeverAskedOfAPayment` |
| What was typed; a disabled contract; a finished project; the audit trail; an id sent twice | `ContractorPaymentRequest`, `ProjectChanges` | `WhatWasTypedIsRefusedWhenItCannotBeKept`, `ADisabledContractOrOneOfAnotherProjectIsRefused`, `AnEmptyIdIsRefused`, `AC_04_1_AFinishedProjectRecordsNoPayment`, `FR_21_APaymentIsInTheAuditTrail`, `AC_16_3_APaymentSentAgainLandsOnce` |
