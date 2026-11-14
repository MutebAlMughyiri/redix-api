[Documentation](../README.md) › [Use cases](../README.md) › UC-032

# UC-032: Approve or reject entries

**Purpose:** decide on what waits, one at a time or many at once. Only what is approved counts (BR-10).

**Who uses it:** whoever holds `ApproveEntries`, which Finance does.

| Route | What it does |
| --- | --- |
| `POST /projects/{id}/entries/approve` | Approves the entries named, answering what was approved and what was not, with why |
| `POST /projects/{id}/entries/{entryId}/reject` | Rejects one, saying why, and returns it to who entered it |

`EntryDecisions` holds every step: sent, approved, rejected, with who, when and why.

| Rule | Code | Test |
| --- | --- | --- |
| Approving marks when, and it counts | `ApprovalsEndpoints.ApproveAsync` | `ApprovalsTests.AnApprovedEntryCountsAndMarksWhenItWasApproved` |
| An account's balance moves only with what is approved | `AccountMoney` | `ApprovalsTests.AnAccountsBalanceMovesOnlyWithWhatIsApproved` |
| AC-19.2: a bulk approval names every failure and approves the rest | `ApprovalsEndpoints.Approve` | `ApprovalsTests.AC_19_2_ABulkApprovalNamesEveryFailureAndApprovesTheRest` |
| An entry of another project is unknown here (BR-33) | `ApprovalsEndpoints.Approve` | `ApprovalsTests.AnEntryOfAnotherProjectIsUnknownHere` |
| AC-19.1: a rejection needs a reason, and returns to its author with it | `ApprovalsEndpoints.Reject`, `EntriesEndpoints.Mine` | `ApprovalsTests.AC_19_1_ARejectionNeedsAReasonAndReturnsTheEntryToItsAuthorWithIt` |
| A rejected entry is sent again, and counts only once approved | `EntriesEndpoints.Submit` | `ApprovalsTests.ARejectedEntryIsSentAgainAndCountsOnlyOnceApproved` |
| What is decided cannot be decided again | `ApprovalsEndpoints` | `ApprovalsTests.WhatIsAlreadyDecidedCannotBeDecidedAgain` |
| AC-44.1: approving the first expense on a pending supplier approves it too, and rejecting leaves it | `ApprovalsEndpoints.ApproveAsync` | `ApprovalsTests.AC_44_1_ApprovingTheFirstExpenseOnAPendingSupplierApprovesItToo`, `AC_44_1_RejectingItLeavesTheSupplierPending` |
| A credit note waits for its invoice, or goes with it | `ApprovalsEndpoints.RefusalOfOriginalAsync` | `ApprovalsTests.ACreditNoteWaitsForItsInvoice`, `ACreditNoteWhoseInvoiceWasRejectedStaysRefused`, `ACreditNoteGoesWithItsInvoiceInOneApproval` |
| Each step is recorded, with who and why | `EntryDecisions`, `db.AddHistory` | `ApprovalsTests.EachSendingAndDecisionIsRecordedWithWhoAndWhy` |
