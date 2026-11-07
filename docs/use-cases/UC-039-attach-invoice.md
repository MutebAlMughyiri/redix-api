[Documentation](../README.md) › [Use cases](../README.md) › UC-039

# UC-039: Attach an invoice to a recorded expense

**Purpose:** put a file that arrived later on an expense entered without one.

**Who uses it:** whoever holds `RecordEntries`. Field sees only their own expenses; whoever holds `ManageAnyEntry` sees everyone's.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/entries/attachable?itemId=&vatNumber=&total=&date=` | What in the item can take a file, matching ones first |
| `POST /projects/{id}/entries/{entryId}/invoice` with `{ attachmentId }` | Puts the file on it |

An expense can take a file when it has none, is positive, and is not reversed or a reversal. Attaching puts only the file on it and writes it in the audit trail (redix-api #96, D15): its status stays as it was, an approved one included, and nobody is asked again.

The capital movement option of AC-49.4 comes with UC-042 "Record a capital movement".

| Rule | Code | Test |
| --- | --- | --- |
| Only an expense with no file, in that item, is offered | `InvoiceAttachEndpoints.AttachableIn` | `InvoiceAttachTests.OnlyAnExpenseWithNoFileInThatItemIsOffered` |
| AC-49.2: the one matching the QR code is marked and comes first | `InvoiceAttachEndpoints.Attachable` | `InvoiceAttachTests.AC_49_2_TheOneMatchingTheQrCodeIsMarkedAndComesFirst` |
| AC-49.4: Field sees only their own | `InvoiceAttachEndpoints.Attachable` | `InvoiceAttachTests.AC_49_4_FieldSeesOnlyTheirOwn` |
| D15: attaching puts only the file on the entry, in the trail | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.D15_AttachingPutsOnlyTheFileOnTheEntryAndWritesItInTheTrail` |
| D15: every entry keeps its status, an approved one included, and nobody is asked again | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AWaitingEntryStaysWaitingAndADraftStaysADraft`, `D15_AnApprovedEntryStaysApprovedAndNobodyIsAskedAgain` |
| BR-22: a file already on an entry is refused | `EntryRecording.IsFileOnAnEntryAsync` | `InvoiceAttachTests.AFileAlreadyOnAnEntryIsRefused` |
| Someone else's entry is refused to Field | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.SomeoneElsesEntryIsRefusedToField` |
| What cannot take a file is refused or not offered | `InvoiceAttachEndpoints.AttachableIn` | `InvoiceAttachTests.WhatCannotTakeAFileIsRefused`, `AReversedEntryIsNotOffered` |
| A file of no such attachment is refused | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AFileOfNoSuchAttachmentIsRefused` |
