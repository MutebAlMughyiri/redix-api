[Documentation](../README.md) › UC-028

# UC-028: Submit an entry for approval

**What it is for:** moving a draft into Finance's inbox.
**Who uses it:** whoever holds `RecordEntries`, on an entry they entered; whoever holds `ManageAnyEntry`, which is Finance, on anyone's. Anyone who reads the project can read an entry.
**Issue:** redix-api #45. It has no app issue.

## How it fits together

An entry sent as it is recorded goes through its own route's checks. One saved as a draft is sent here, by reading what the ledger holds. The rule of its columns is the same one: `EntryAnswers` answers for a request and for the rows of a draft alike.

`POST .../submit` answers every key the entry lacks under `missing`: `attachment` for a file with no reason in its place, and each required column left empty, so a person fixes them all at once. The app does not call it yet, and a draft cannot be changed until UC-033 "Edit or delete an unapproved entry", so one that lacks something stays as it is until then.

Telling Finance belongs to UC-036 "Send push notifications": nothing is faked here, and the History table records who sent what. `EntryDecisions` belongs to UC-032 "Approve or reject entries", the first to need it.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `GET /projects/{id}/entries/{entryId}` | `ReadProject` | The entry, with what its kind has. What it does not have comes empty, with an empty `entryId` |
| `POST /projects/{id}/entries/{entryId}/submit` | `RecordEntries` | Sends a draft or a rejected entry. Answers `notYourEntry`, `entryNotSubmittable` or `entryIncomplete` with `missing` |

## Tables

No new table. `Permission.ManageAnyEntry` is ticked per person (UC-009).

## Rules

Tests are in `SubmitEntryTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A complete draft becomes pending | `EntriesEndpoints.Submit` | `UC_028_ADraftWithItsFileIsSentAndIsPending` |
| D3: a draft with no file is sent, and takes its file later (D15); AC-09.1 and the exceptions list retired with simple expense (redix-api #96) | `MissingOfAsync` | `D3_ADraftWithNoFileIsSent` |
| FR-41: each required column is named | `EntryAnswers.MissingOf` | `FR_41_EveryRequiredColumnLeftEmptyIsNamed`, `FR_41_WhatADraftHoldsIsWhatAnswersItsColumns` |
| D16: a contractor payment is sent with or without its receipt, and once sent waits on its contract | `MissingOfAsync`, `ContractMoney` | `D16_ADraftContractorPaymentIsSentWithOrWithoutItsReceiptAndWaitsOnItsContract` |
| Only a draft or a rejected entry | `Entry.CanBeSubmitted` | `UC_033_ARejectedEntryCanBeSentAgain`, `AnEntryAlreadySentCannotBeSentAgain` |
| Field sends their own; Finance anyone's | `Permission.ManageAnyEntry` | `FieldSendsTheirOwnEntryAndNotAnothersAndFinanceSendsAnyones`, `WhoHoldsNoRecordEntriesCannotSendAnEntry` |
| BR-33 and a finished project | `ProjectChanges`, the query by project | `BR_33_AnEntryOfAnotherProjectIsNotFoundAndSomeoneOutsideIsRefused`, `AC_04_1_AFinishedProjectSendsNothing` |
| FR-21: sending is in the History table | `RedixDbContext.AddHistory` | `FR_21_SendingIsInTheHistoryAndARefusalIsNot` |
| An entry reads back whole | `ViewOfAsync` | `ReadingADraftAnswersWhatItsKindHasAndEmptyForTheRest`, `ReadingACreditNoteAnswersWhatItIsLinkedTo` |
| A rejected revenue or partner entry is sent again and waits once more; it has no columns to check | `EntriesEndpoints.MissingOfAsync` | `RevenueTests.ARejectedRevenueEntryIsSentAgainAndWaitsOnceMore` |
| BR-31: one partner's money read by its id answers 404 to anyone but that partner and whoever holds `seeAllCapital`; the list and the single read share one filter | `EntriesEndpoints.SeenByAsync`, `Read` | `RevenueTests.BR_31_OnePartnersMoneyReadByItsIdIsLeftToThatPartnerAndWhoeverSeesAllCapital` |
