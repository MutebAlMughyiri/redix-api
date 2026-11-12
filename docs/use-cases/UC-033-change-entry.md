[Documentation](../README.md) › [Use cases](../README.md) › UC-033, UC-034, UC-035

# UC-033, UC-034, UC-035: Change an unapproved entry, keep the approved version, remove an approved one

**Purpose:** correct an entry before it is approved, and only then. What is approved never changes (BR-11); a mistaken approved entry is removed softly, by its own permission (redix-api #119). There is no reversing an entry (Muteb, 12 Nov 2026: «لايوجد شيء اسمه عكس القيد. اما حذف و مرتجع بالسالب»).

| Route | Who | What it does |
| --- | --- | --- |
| `PUT /projects/{id}/entries/{entryId}` | Whoever may enter its table (redix-api #110) | Replaces what it holds, through every check a new one passes, whatever its table; moved to a table of another kind, it becomes that kind |
| `DELETE /projects/{id}/entries/{entryId}` | `DeleteEntries` before approval, `RemoveApprovedEntries` after it | Removes one never approved: one's own draft or rejected entry, or with `ManageAnyEntry` anyone's or one waiting. Removes an approved one softly: marked `Removed`, it leaves every list and figure, and stays in the database with its history. Without the permission the entry asks for, `403` |
| `PUT /projects/{id}/entries/{entryId}/notes` | `RecordEntries` | Changes its notes, even once approved |

**Removing an approved entry** (redix-api #119, D1): `RemoveApprovedEntries` is ticked per person per project, and nobody holds it until it is ticked, not even whoever starts the project. A removed entry is `EntryStatus.Removed`: no list shows it, reading it by its id answers `404`, and since every figure counts only approved entries (BR-10), it leaves the summary, its account, its item, its contract's paid amount and a partner's capital as if never approved. A periodic report issued before keeps it as issued (BR-26), so the next report shows the difference (D2). Entries reversed before redix-api #119 stay as two ordinary approved entries, the entry and its opposite, so no figure moved (D5); the History line their opposite wrote stays, since History never changes (AC-21.1), and the histories read it as an approval.

| Rule | Code | Test |
| --- | --- | --- |
| An author changes their draft, and it holds the new values | `EntryRecording.Change`, `EntryChanges.ReplaceAsync` | `EntryChangesTests.AnAuthorChangesTheirDraftAndItHoldsTheNewValues` |
| A change keeps who entered it and when | `EntryChanges.ReplaceAsync` | `EntryChangesTests.AChangeKeepsWhoEnteredItAndWhen` |
| Every change is in the History table, before and after | `db.AddHistory` | `EntryChangesTests.EveryChangeIsInTheHistoryWithTheValuesBeforeAndAfter` |
| An edit goes through the checks of a new entry | `EntryRecording.SaveAsync` | `EntryChangesTests.AnEditGoesThroughTheChecksOfANewEntry` |
| A rejected entry is fixed and sent in one edit | `EntryChanges.ReplaceAsync` | `EntryChangesTests.ARejectedEntryIsFixedAndSentAgainInOneEdit` |
| An edit does not call its own file or number a repeat | The `editing` id | `EntryChangesTests.AnEditKeepsItsOwnFileAndInvoiceNumberWithoutCallingThemRepeats` |
| The field cannot change what waits, or someone else's | `EntryChanges.RefusalOf` | `EntryChangesTests.TheFieldCannotChangeWhatWaitsForADecisionOrSomeoneElsesEntry` |
| Finance changes anything before approval | `EntryChanges.RefusalOf` | `EntryChangesTests.FinanceChangesAnyEntryBeforeApproval` |
| AC-16.2: an edit arriving after approval is refused, and the server's version stays | `EntryChanges.RefusalOf` | `EntryChangesTests.AC_16_2_AnOfflineEditOfAnApprovedEntryIsRefusedAndTheServersVersionStays` |
| AC-50.2: an approved entry's notes may change, and deleting is not enough to remove it | `EntriesEndpoints.ChangeNotes`, `Delete` | `EntryChangesTests.AC_50_2_AnApprovedEntrysNotesMayChangeAndDeletingIsNotEnoughToRemoveIt` |
| A deleted entry leaves only the History table | `EntryChanges.DeleteAsync` | `EntryChangesTests.ADeletedEntryLeavesNothingButTheHistory` |
| A bill not approved is removed, whether a draft, waiting or rejected | `EntriesEndpoints.Delete` | `EntryChangesTests.ABillNotApprovedIsRemoved` |
| Deleting is its own permission | `EntriesEndpoints.Delete` | `EntryChangesTests.DeletingIsItsOwnPermission` |
| AC-50.1: whoever holds the removal removes an approved entry softly, and the History table says who and when | `EntriesEndpoints.Delete`, `EntryChanges.RemoveAsync` | `RemoveApprovedEntryTests.D1_WhoeverHoldsTheRemovalRemovesAnApprovedEntrySoftlyAndTheHistorySaysWhoAndWhen` |
| AC-50.1: without the removal an approved entry stays, whatever else they hold | `EntriesEndpoints.Delete` | `RemoveApprovedEntryTests.D1_WithoutTheRemovalAnApprovedEntryStaysWhateverElseTheyHold` |
| D1: nobody holds the removal until it is ticked | `ProjectPermission.OfWhoeverStarts` | `RemoveApprovedEntryTests.D1_NobodyHoldsTheRemovalUntilItIsTicked` |
| D1: the removal alone deletes nothing that is not approved | `EntriesEndpoints.Delete` | `RemoveApprovedEntryTests.D1_TheRemovalAloneDeletesNothingThatIsNotApproved` |
| D1: a removed entry leaves every list and is not found by its id | `EntriesEndpoints.SeenByAsync`, `Entry.OfProjectAsync` | `RemoveApprovedEntryTests.D1_ARemovedEntryLeavesEveryListAndIsNotFoundByItsId` |
| D1: a removed entry is not removed, changed or noted again | `Entry.OfProjectAsync` | `RemoveApprovedEntryTests.D1_ARemovedEntryIsNotRemovedChangedOrNotedAgain` |
| D1: a removed entry leaves the summary, its account and its item | BR-10, approved entries only | `RemoveApprovedEntryTests.D1_ARemovedEntryLeavesTheSummaryItsAccountAndItsItem` |
| D1: a removed payment leaves what its contract paid | `ContractMoney` | `RemoveApprovedEntryTests.D1_ARemovedPaymentLeavesWhatItsContractPaid` |
| D1: a removed movement leaves its partner's capital and no longer holds the partner | `CapitalEndpoints.BalancesAsync`, `PartnersEndpoints` | `RemoveApprovedEntryTests.D1_ARemovedMovementLeavesItsPartnersCapitalAndNoLongerHoldsThePartner` |
| D1: the histories leave the removed entry's approval | `ApprovalHistory`, `AccountHistory` | `RemoveApprovedEntryTests.D1_TheHistoriesLeaveTheRemovedEntrysApproval` |
| D1: a removed entry is not offered to take an invoice | `InvoiceAttachEndpoints` | `InvoiceAttachTests.App_119_ARemovedEntryIsNotOffered` |
| D2: a report issued before keeps the removed entry, and the next shows the difference | BR-26 | `RemoveApprovedEntryTests.D2_AReportIssuedBeforeKeepsTheRemovedEntryAndTheNextShowsTheDifference` |
| D3: nothing is reversed any more | No route | `RemoveApprovedEntryTests.D3_NothingIsReversedAnyMore` |
| D5: an entry reversed before reads as an ordinary approved entry | `AccountHistory`, `PartnerHistory` | `RemoveApprovedEntryTests.D5_AnEntryReversedBeforeReadsAsAnOrdinaryApprovedEntry` |
| Notes longer than 1,000 characters answer `invalidNotes`; notes of 1,000 are kept | `EntriesEndpoints.ChangeNotes`, `Entry.NotesMaxLength` | `EntryChangesTests.NotesLongerThanTheLimitAreRefusedAndNotesOfTheLimitAreKept` |
