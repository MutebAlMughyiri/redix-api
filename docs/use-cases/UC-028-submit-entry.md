[Documentation](../README.md) › UC-028

# UC-028: Submit an entry for approval

**What it is for:** moving a draft into Finance's inbox.
**Who uses it:** whoever holds `RecordEntries`, on an entry they entered; whoever holds `ManageAnyEntry`, which is Finance, on anyone's rejected entry, never on another person's draft (BR-42). Anyone who reads the project can read an entry, but a draft only its author.
**Issue:** redix-api #45. It has no app issue.

## How it fits together

An entry sent as it is recorded goes through its own route's checks. One saved as a draft is sent here, by reading what the ledger holds. The rule of its columns is the same one: `EntryAnswers` answers for a request and for the rows of a draft alike.

`POST .../submit` answers each required column the entry left empty under `missing` (FR-41), so a person fixes them all at once; a missing file stops nothing (D3). The app sends a draft from its row on Entries (redix-app's `OpenedEntry.send`), and a draft that lacks something is changed first in Add entry (UC-033).

Telling Finance belongs to UC-036 "Send push notifications": nothing is faked here, and the History table records who sent what. `entry_decisions` belongs to UC-032 "Approve or reject entries", the first to need it.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `GET /projects/{id}/entries?take=&text=&mine=&kinds=&statuses=&from=&to=&number=` | `ReadProject` | The entries the caller may see (BR-31, BR-42), newest first, each with its `number`, «رقم القيد» (FR-54): the newest `take` of them, every one for 0 (redix-app #105); `text` matched against the item, partner, supplier, who entered it, a bill's number and the amount, Arabic spelling read alike; `mine` the caller's own; `kinds` and `statuses` repeated for several; `from` and `to` ISO days, both included (redix-app #104, `EntrySearch`); `number` the one entry with that «رقم القيد», 0 for any (redix-api #128, D4); `attachable=true` those the caller may put a file on (UC-039, redix-app #115 D6). Each row carries its supplier's `supplierVatNumber`, which the share screen compares with a bill's QR code (AC-49.3) |
| `GET /projects/{id}/entries/{entryId}` | `ReadProject` | The entry, with what its kind has. What it does not have comes empty, with an empty `entryId`. `attachments` holds every file it carries, the oldest first, each with its `attachmentId`, `role`, `fileName`, `contentType` and `uploadedAt`, which its page lists (UC-039, redix-app #115 AC-46.10g) |
| `GET /projects/{id}/entries/{entryId}/row` | `ReadProject` | The entry as the lists show it, so an opened entry says where it stands and offers what can be done to it whichever list opened it (findings row 201); `404` for one the caller does not see (BR-31, BR-42) |
| `POST /projects/{id}/entries/{entryId}/submit` | `RecordEntries` | Sends a draft or a rejected entry. Answers `notYourEntry`, `entryNotSubmittable` or `entryIncomplete` with `missing` |

## Tables

No new table. `Permission.ManageAnyEntry` is ticked per person (UC-009).

## Rules

Tests are in `SubmitEntryTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A complete draft becomes pending | `entries.Routes.submit` | `UC_028_ADraftWithItsFileIsSentAndIsPending` |
| D3: a draft with no file is sent, and takes its file later (D15); AC-09.1 and the exceptions list retired with simple expense (redix-api #96) | `MissingOfAsync` | `D3_ADraftWithNoFileIsSent` |
| FR-41: each required column is named | `entries.missingOf` | `FR_41_EveryRequiredColumnLeftEmptyIsNamed`, `FR_41_WhatADraftHoldsIsWhatAnswersItsColumns` |
| D16: a contractor payment is sent with or without its receipt, and once sent waits on its contract | `MissingOfAsync`, `ContractMoney` | `D16_ADraftContractorPaymentIsSentWithOrWithoutItsReceiptAndWaitsOnItsContract` |
| Only a draft or a rejected entry | `entries.Routes.submit` | `UC_033_ARejectedEntryCanBeSentAgain`, `AnEntryAlreadySentCannotBeSentAgain` |
| BR-42: a draft is its author's alone until it is sent; Finance sends anyone's rejected entry again | `entries.Routes.seenBy`, `entries.mayHandle` | `FieldSendsTheirOwnEntryAndNotAnothersAndFinanceSendsAnyonesRejectedEntryButNotTheirDraft`, `EntryChangesTests.BR_42_ADraftIsItsAuthorsAloneUntilItIsSent`, `WhoHoldsNoRecordEntriesCannotSendAnEntry` |
| Findings row 201: an opened entry reads itself by its id; someone else's draft is not found | `entries.Routes.row` | `EntryChangesTests.Row_201_AnOpenedEntryReadsItselfByItsId`, `EntryChangesTests.Row_201_SomeoneElsesDraftAndAnEntryThatDoesNotExistAreNotFound` |
| BR-33 and a finished project | `ProjectChanges`, the query by project | `BR_33_AnEntryOfAnotherProjectIsNotFoundAndSomeoneOutsideIsRefused`, `AC_04_1_AFinishedProjectSendsNothing` |
| FR-21: sending is in the History table | `history.Write` | `FR_21_SendingIsInTheHistoryAndARefusalIsNot` |
| AC-54.1, R5: every entry has «رقم القيد», one more than the last its project gave, counted within the project (redix-api #128) | `entries.add` | `EntryNumberTests.AC_54_1_AKeptEntryHasANumberAndTheNextOneKeptInTheProjectHasTheNext`, `AC_54_1_R5_EachProjectCountsItsOwnAndTakesNoneOfAnotherProjectsNumbers` |
| AC-54.2, R3: given when the server first keeps the entry, so a draft and a pending entry have theirs; a deleted draft leaves a gap, and an upload sent again takes no other (D1, D3) | `entries.add`, `entries.Routes.record` | `EntryNumberTests.AC_54_2_ADraftAndAPendingEntryHaveTheirNumberAndADeletedDraftLeavesItsNumberUnused`, `FR_54_AnUploadSentAgainAnswersItsNumberAndTakesNoOther` |
| AC-54.3: changing, moving to another table, approving, removing and restoring keep the number | `EntryChanges` (the entry's row is kept) | `EntryNumberTests.AC_54_3_ChangingMovingApprovingRemovingAndRestoringAnEntryKeepsItsNumber` |
| D3, G1: entries arriving at once each take their own number, by one atomic `UPDATE … RETURNING` on the project's counter; the unique index `ix_entries_project_id_number` refuses a second | `entries.add`, `db/migrations/00001_schema.sql` | `EntryNumberTests.G1_EntriesArrivingAtTheSameMomentEachTakeANumberOfTheirOwn`, `D3_TheDatabaseRefusesASecondEntryOfTheProjectWithTheSameNumber` |
| AC-54.5, R9: `number` lists that entry alone, with every other filter; one the caller may not see lists nothing, as a number no entry has | `entries.search.keeps`, `entries.Routes.seenBy` | `EntryNumberTests.AC_54_5_TheNumberFilterListsThatEntryAloneAndANumberNoEntryHasListsNothing`, `AC_54_5_R9_ANumberOfAnEntryThePersonMayNotSeeListsNothingAsANumberNoEntryHas` |
| AC-54.8: the entries that existed before are numbered once by their date, then by when they were made; each project's counter is its last | migration `EntryNumbers` | `MigrationStepsTests.R128_D6_EntryNumbers_EveryEntryIsNumberedInItsProjectByItsDateThenWhenItWasMadeAndEveryFigureStays` |
| An entry reads back whole | `ViewOfAsync` | `ReadingADraftAnswersWhatItsKindHasAndEmptyForTheRest`, `ReadingACreditNoteAnswersWhatItIsLinkedTo` |
| A rejected revenue or partner entry is sent again and waits once more; it has no columns to check | `entries.Routes.missingOf` | `RevenueTests.ARejectedRevenueEntryIsSentAgainAndWaitsOnceMore` |
| BR-31: one partner's money read by its id answers 404 to anyone but that partner and whoever holds `seeAllCapital`; the list and the single read share one filter | `entries.Routes.seenBy`, `Read` | `RevenueTests.BR_31_OnePartnersMoneyReadByItsIdIsLeftToThatPartnerAndWhoeverSeesAllCapital` |

## On the web: the entries and the details panel (redix-api #166)

The web's Entries tab is the app's list in the shell: the same search and filters, sent by the same names, read by `entries.Listed`, the function `GET /projects/{id}/entries` calls too; a page at a time with Show more. A row's click or Enter shows the entry in the shell's details panel from `entries.RowOf`, the row `GET /projects/{id}/entries/{entryId}/row` answers; Up, Down, Home and End move along the list with the panel following, and Esc hides it. Its pages are in `go-backend/web` (`entries.go`, `entries.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/entries?text=&mine=&kinds=&statuses=&from=&to=&number=&take=` | The Entries tab, filtered and paged | `200` the tab's content |
| `GET /details/entry/{id}` | An entry in the details panel | `200` the panel's content; `404` for one the person does not see |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The list shows what the person sees, and the filters narrow it as the app's do | `entries.Listed` | `TestWeb_R166_UC028_TheListShowsWhatTheCallerSeesAndTheFiltersNarrowIt` |
| Show more adds a page with the same filters | `web.site.entriesTab` | `TestWeb_R166_ShowMoreAddsAPageWithTheSameFilters` |
| A row opens its entry in the details panel; an entry the person does not see is not shown (BR-31) | `entries.RowOf` | `TestWeb_R166_ARowOpensItsEntryInTheDetailsPanel`, `TestWeb_R166_AnEntryOfAnotherProjectIsNotShown` |
| A rejected entry says why | `web.entryPanel` | `TestWeb_R166_ARejectedEntrySaysWhy` |
