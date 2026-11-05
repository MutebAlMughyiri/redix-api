[Documentation](../README.md) › [Use cases](../README.md) › UC-039

# UC-039: Attach an invoice to a recorded expense

**Purpose:** put an invoice that arrived later on the expense entered without it.

**Who uses it:** whoever holds `RecordEntries`. Field sees only their own expenses; whoever holds `ManageAnyEntry` sees everyone's.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/entries/attachable?itemId=&vatNumber=&total=&date=` | What in the item can take an invoice, matching ones first |
| `POST /projects/{id}/entries/{entryId}/invoice` | Puts the invoice on it |

An expense can take an invoice when it was entered without one, is not a deduction, and is not reversed or a reversal. Only what is approved changes status: a draft stays a draft and what waits stays waiting, and Finance sees any difference when it decides.

The capital movement option of AC-49.4 comes with UC-042 "Record a capital movement".

| Rule | Code | Test |
| --- | --- | --- |
| Only an expense without an invoice, in that item, is offered | `InvoiceAttachEndpoints.AttachableIn` | `InvoiceAttachTests.OnlyAnExpenseWithoutAnInvoiceInThatItemIsOffered` |
| AC-49.2: the one matching the QR code is marked and comes first | `InvoiceAttachEndpoints.Attachable` | `InvoiceAttachTests.AC_49_2_TheOneMatchingTheQrCodeIsMarkedAndComesFirst` |
| AC-49.4: Field sees only their own | `InvoiceAttachEndpoints.Attachable` | `InvoiceAttachTests.AC_49_4_FieldSeesOnlyTheirOwn` |
| Attaching puts the invoice on the entry, in the trail (BR-22) | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AttachingPutsTheInvoiceOnTheEntryAndWritesItInTheTrail` |
| A draft and what waits keep their status | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AWaitingEntryStaysWaitingAndADraftStaysADraft` |
| AC-49.3: an approved entry goes back to approval, and Finance is told | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AC_49_3_AnApprovedEntryGoesBackToApprovalAndFinanceIsTold` |
| AC-49.3: Finance sees the difference | `ApprovalsEndpoints.Pending` | `InvoiceAttachTests.AC_49_3_FinanceSeesTheDifferenceWhenItDecides` |
| AC-49.3: the same invoice cannot go on two expenses, confirmed or not | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AC_49_3_TheSameInvoiceCannotGoOnTwoExpensesEvenIfConfirmed` |
| A file already on an entry is refused | `EntryRecording.IsFileOnAnEntryAsync` | `InvoiceAttachTests.AFileAlreadyOnAnEntryIsRefused` |
| Someone else's entry is refused to Field | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.SomeoneElsesEntryIsRefusedToField` |
| What cannot take an invoice is refused or not offered | `InvoiceAttachEndpoints.AttachableIn` | `InvoiceAttachTests.WhatCannotTakeAnInvoiceIsRefused`, `AReversedEntryIsNotOffered` |
| What was typed is checked | `InvoiceAttachEndpoints.RefusalOfTyped` | `InvoiceAttachTests.WhatWasTypedIsChecked` |
