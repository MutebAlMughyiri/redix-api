[Documentation](../README.md) › UC-028

# UC-028: Submit an entry for approval

**What it is for:** moving a draft into Finance's inbox.
**Who uses it:** whoever holds `RecordEntries`, on an entry they entered; whoever holds `ManageAnyEntry`, which is Finance, on anyone's. Anyone who reads the project can read an entry.
**Issue:** redix-api #45. It has no app issue.

## How it fits together

An entry sent as it is recorded goes through its own route's checks. One saved as a draft is sent here, by reading what the ledger holds. The rule of its columns is the same one: `EntryAnswers` answers for a request and for the rows of a draft alike.

`POST .../submit` answers every key the entry lacks under `missing`: `attachment` for a file with no reason in its place, and each required column left empty, so a person fixes them all at once. The app does not call it yet, and a draft cannot be changed until UC-033 "Edit or delete an unapproved entry", so one that lacks something stays as it is until then.

Telling Finance belongs to UC-036 "Send push notifications": nothing is faked here, and the audit trail records who sent what. `EntryDecisions` belongs to UC-032 "Approve or reject entries", the first to need it.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `GET /projects/{id}/entries/{entryId}` | `ReadProject` | The entry, with what its kind has. What it does not have comes empty, with an empty `entryId` |
| `POST /projects/{id}/entries/{entryId}/submit` | `RecordEntries` | Sends a draft or a rejected entry. Answers `notYourEntry`, `entryNotSubmittable` or `entryIncomplete` with `missing` |

## Tables

No new table. `Permission.ManageAnyEntry` joins Finance's role, and the migration carries the row.

## Rules

Tests are in `SubmitEntryTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A complete draft becomes pending | `EntriesEndpoints.Submit` | `UC_028_ADraftWithItsFileIsSentAndIsPending` |
| AC-09.1: no file and no reason is refused; a reason is enough | `MissingOfAsync` | `AC_09_1_ADraftWithNoFileAndNoReasonIsRefusedNamingTheFile`, `AC_09_1_ADraftThatSaysWhyThereIsNoInvoiceIsSentAndJoinsTheExceptionsList` |
| FR-41: each required column is named | `EntryAnswers.MissingOf` | `FR_41_EveryRequiredColumnLeftEmptyIsNamedWithTheFile`, `FR_41_WhatADraftHoldsIsWhatAnswersItsColumns` |
| A contractor payment needs its receipt, and waits once sent | `MissingOfAsync`, `ContractMoney` | `ADraftContractorPaymentNeedsItsReceiptAndOnceSentWaitsOnItsContract` |
| Only a draft or a rejected entry | `Entry.CanBeSubmitted` | `UC_033_ARejectedEntryCanBeSentAgain`, `AnEntryAlreadySentCannotBeSentAgain` |
| Field sends their own; Finance anyone's | `Permission.ManageAnyEntry` | `FieldSendsTheirOwnEntryAndNotAnothersAndFinanceSendsAnyones`, `WhoHoldsNoRecordEntriesCannotSendAnEntry` |
| BR-33 and a finished project | `ProjectChanges`, the query by project | `BR_33_AnEntryOfAnotherProjectIsNotFoundAndSomeoneOutsideIsRefused`, `AC_04_1_AFinishedProjectSendsNothing` |
| FR-21: sending is in the audit trail | `RedixDbContext.Audit` | `FR_21_SendingIsInTheAuditTrailAndARefusalIsNot` |
| An entry reads back whole | `ViewOfAsync` | `ReadingADraftAnswersWhatItsKindHasAndEmptyForTheRest`, `ReadingAnExpenseWithoutAnInvoiceAnswersItsReasonAndAnEmptyInvoice`, `ReadingACreditNoteAnswersWhatItIsLinkedTo` |
