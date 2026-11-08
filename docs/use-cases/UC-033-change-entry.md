[Documentation](../README.md) › [Use cases](../README.md) › UC-033, UC-034, UC-035

# UC-033, UC-034, UC-035: Change an unapproved entry, keep the approved version, reverse an approved one

**Purpose:** correct an entry before it is approved, and only then. What is approved never changes (BR-11); the fix is a reversal.

| Route | Who | What it does |
| --- | --- | --- |
| `POST /projects/{id}/entries/{entryId}` | Whoever may enter its table (redix-api #110) | Replaces what it holds, through every check a new one passes, whatever its table; moved to a table of another kind, it becomes that kind |
| `POST /projects/{id}/entries/{entryId}/delete` | `DeleteEntries` | Removes one never approved: one's own draft or rejected entry, or with `ManageAnyEntry` anyone's or one waiting |
| `POST /projects/{id}/entries/{entryId}/notes` | `RecordEntries` | Changes its notes, even once approved |
| `POST /projects/{id}/entries/{entryId}/reverse` | `ApproveEntries` | Adds an opposite entry tied to the original, approved at once |

| Rule | Code | Test |
| --- | --- | --- |
| An author changes their draft, and it holds the new values | `EntryRecording.Change`, `EntryChanges.ReplaceAsync` | `EntryChangesTests.AnAuthorChangesTheirDraftAndItHoldsTheNewValues` |
| A change keeps who entered it and when | `EntryChanges.ReplaceAsync` | `EntryChangesTests.AChangeKeepsWhoEnteredItAndWhen` |
| Every change is in the audit trail, before and after | `db.Audit` | `EntryChangesTests.EveryChangeIsInTheAuditTrailWithTheValuesBeforeAndAfter` |
| An edit goes through the checks of a new entry | `ExpensesEndpoints.SaveAsync` | `EntryChangesTests.AnEditGoesThroughTheChecksOfANewEntry` |
| A rejected entry is fixed and sent in one edit | `EntryChanges.ReplaceAsync` | `EntryChangesTests.ARejectedEntryIsFixedAndSentAgainInOneEdit` |
| An edit does not call its own file or number a repeat | The `editing` id | `EntryChangesTests.AnEditKeepsItsOwnFileAndInvoiceNumberWithoutCallingThemRepeats` |
| The field cannot change what waits, or someone else's | `EntryChanges.RefusalOf` | `EntryChangesTests.TheFieldCannotChangeWhatWaitsForADecisionOrSomeoneElsesEntry` |
| Finance changes anything before approval | `EntryChanges.RefusalOf` | `EntryChangesTests.FinanceChangesAnyEntryBeforeApproval` |
| AC-16.2: an edit arriving after approval is refused, and the server's version stays | `EntryChanges.RefusalOf` | `EntryChangesTests.AC_16_2_AnOfflineEditOfAnApprovedEntryIsRefusedAndTheServersVersionStays` |
| AC-50.2: an approved entry's notes may change, and it is not deleted | `EntriesEndpoints.ChangeNotes`, `Delete` | `EntryChangesTests.AC_50_2_AnApprovedEntrysNotesMayChangeAndItIsNotDeleted` |
| A deleted entry leaves only the audit trail | `EntryChanges.DeleteAsync` | `EntryChangesTests.ADeletedEntryLeavesNothingButTheAuditTrail` |
| A bill not approved is removed, whether a draft, waiting or rejected | `EntriesEndpoints.Delete` | `EntryChangesTests.ABillNotApprovedIsRemoved` |
| An approved bill is not removed but reversed, both staying with a net of zero | `EntriesEndpoints.Delete`, `Reverse` | `EntryChangesTests.AnApprovedBillIsNotRemovedButReversedAndBothStayWithANetOfZero` |
| Deleting is its own permission | The route | `EntryChangesTests.DeletingIsItsOwnPermission` |
| An invoice a credit note returns is not deleted | `EntriesEndpoints.Delete` | `EntryChangesTests.AnInvoiceACreditNoteReturnsIsNotDeleted` |
| AC-50.1: a reversal is opposite and tied, the net is zero, and the original stays | `EntriesEndpoints.Reverse` | `EntryChangesTests.AC_50_1_AReversalIsAnOppositeEntryTiedToTheOriginalAndTheNetEffectIsZero` |
| Only an approved entry is reversed, once, with a reason | `EntriesEndpoints.Reverse` | `EntryChangesTests.OnlyAnApprovedEntryIsReversedOnceAndWithAReason` |
| Only whoever decides entries reverses them | The `ApproveEntries` policy | `EntryChangesTests.OnlyWhoeverDecidesEntriesReversesThem` |
| Notes longer than 1,000 characters answer `invalidNotes`; notes of 1,000 are kept | `EntriesEndpoints.ChangeNotes`, `Entry.NotesMaxLength` | `EntryChangesTests.NotesLongerThanTheLimitAreRefusedAndNotesOfTheLimitAreKept` |
