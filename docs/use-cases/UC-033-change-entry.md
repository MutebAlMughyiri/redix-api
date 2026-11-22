[Documentation](../README.md) › [Use cases](../README.md) › UC-033, UC-034, UC-035

# UC-033, UC-034, UC-035: Change an unapproved entry, keep the approved version, remove an approved one

**Purpose:** correct an entry before it is approved, and only then. What is approved never changes (BR-11); a mistaken approved entry is removed softly, by its own permission (redix-api #119). There is no reversing an entry (the owner, 12 Nov 2026: «لايوجد شيء اسمه عكس القيد. اما حذف و مرتجع بالسالب»).

| Route | Who | What it does |
| --- | --- | --- |
| `PUT /projects/{id}/entries/{entryId}` | Whoever may enter its table (redix-api #110) | Replaces what it holds, through every check a new one passes, whatever its table; moved to a table of another kind, it becomes that kind |
| `DELETE /projects/{id}/entries/{entryId}` | `DeleteEntries` before approval, `RemoveApprovedEntries` after it | Removes one never approved: one's own draft or rejected entry, or with `ManageAnyEntry` anyone's or one waiting. Removes an approved one softly: marked `Removed`, it leaves every list and figure, and stays in the database with its history. Without the permission the entry asks for, `403` |
| `PUT /projects/{id}/entries/{entryId}/description` | `RecordEntries` | Changes its description, «الوصف», even once approved (redix-api #129) |

**Removing an approved entry** (redix-api #119, D1): `RemoveApprovedEntries` is ticked per person per project, and nobody holds it until it is ticked, not even whoever starts the project. A removed entry is `entries.removed`: no list shows it, reading it by its id answers `404`, and since every figure counts only approved entries (BR-10), it leaves the summary, its account, its item, its contract's paid amount and a partner's capital as if never approved. A periodic report issued before keeps it as issued (BR-26), so the next report shows the difference (D2). Entries reversed before redix-api #119 stay as two ordinary approved entries, the entry and its opposite, so no figure moved (D5); the History line their opposite wrote stays, since History never changes (AC-21.1), and the histories read it as an approval.

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| An author changes their draft, and it holds the new values | `entries.Routes.change`, `entries.replace` | `EntryChangesTests.AnAuthorChangesTheirDraftAndItHoldsTheNewValues` |
| A change keeps who entered it and when | `entries.replace` | `EntryChangesTests.AChangeKeepsWhoEnteredItAndWhen` |
| Every change is in the History table, before and after | `history.Write` | `EntryChangesTests.EveryChangeIsInTheHistoryWithTheValuesBeforeAndAfter` |
| An edit goes through the checks of a new entry | `entries.Routes.record` | `EntryChangesTests.AnEditGoesThroughTheChecksOfANewEntry` |
| A rejected entry is fixed and sent in one edit | `entries.replace` | `EntryChangesTests.ARejectedEntryIsFixedAndSentAgainInOneEdit` |
| An edit does not call its own file or number a repeat | The `editing` id | `EntryChangesTests.AnEditKeepsItsOwnFileAndInvoiceNumberWithoutCallingThemRepeats` |
| The field cannot change what waits, or someone else's | `entries.refusalOfChange` | `EntryChangesTests.TheFieldCannotChangeWhatWaitsForADecisionOrSomeoneElsesEntry` |
| Finance changes anything before approval | `entries.refusalOfChange` | `EntryChangesTests.FinanceChangesAnyEntryBeforeApproval` |
| AC-16.2: an edit arriving after approval is refused, and the server's version stays | `entries.refusalOfChange` | `EntryChangesTests.AC_16_2_AnOfflineEditOfAnApprovedEntryIsRefusedAndTheServersVersionStays` |
| AC-50.2: an approved entry's description may change, and deleting is not enough to remove it | `entries.Routes.describe`, `Delete` | `EntryChangesTests.AC_50_2_AnApprovedEntrysNotesMayChangeAndDeletingIsNotEnoughToRemoveIt` |
| A deleted entry leaves only the History table | `entries.Delete` | `EntryChangesTests.ADeletedEntryLeavesNothingButTheHistory` |
| A bill not approved is removed, whether a draft, waiting or rejected | `entries.Routes.delete` | `EntryChangesTests.ABillNotApprovedIsRemoved` |
| Deleting is its own permission | `entries.Routes.delete` | `EntryChangesTests.DeletingIsItsOwnPermission` |
| AC-50.1: whoever holds the removal removes an approved entry softly, and the History table says who and when | `entries.Routes.delete` | `RemoveApprovedEntryTests.D1_WhoeverHoldsTheRemovalRemovesAnApprovedEntrySoftlyAndTheHistorySaysWhoAndWhen` |
| AC-50.1: without the removal an approved entry stays, whatever else they hold | `entries.Routes.delete` | `RemoveApprovedEntryTests.D1_WithoutTheRemovalAnApprovedEntryStaysWhateverElseTheyHold` |
| D1: nobody holds the removal until it is ticked | `users.TickStarter` | `RemoveApprovedEntryTests.D1_NobodyHoldsTheRemovalUntilItIsTicked` |
| D1: the removal alone deletes nothing that is not approved | `entries.Routes.delete` | `RemoveApprovedEntryTests.D1_TheRemovalAloneDeletesNothingThatIsNotApproved` |
| D1: a removed entry leaves every list and is not found by its id | `entries.Routes.seenBy`, `entries.Routes.entry` | `RemoveApprovedEntryTests.D1_ARemovedEntryLeavesEveryListAndIsNotFoundByItsId` |
| D1: a removed entry is not removed, changed or noted again | `entries.Routes.entry` | `RemoveApprovedEntryTests.D1_ARemovedEntryIsNotRemovedChangedOrNotedAgain` |
| D1: a removed entry leaves the summary, its account and its item | BR-10, approved entries only | `RemoveApprovedEntryTests.D1_ARemovedEntryLeavesTheSummaryItsAccountAndItsItem` |
| D1: a removed payment leaves what its contract paid | `ContractMoney` | `RemoveApprovedEntryTests.D1_ARemovedPaymentLeavesWhatItsContractPaid` |
| D1: a removed movement leaves its partner's capital and no longer holds the partner | `partners.balances`, `partners.deletable` | `RemoveApprovedEntryTests.D1_ARemovedMovementLeavesItsPartnersCapitalAndNoLongerHoldsThePartner` |
| D1: the histories leave the removed entry's approval | `ApprovalHistory`, `AccountHistory` | `RemoveApprovedEntryTests.D1_TheHistoriesLeaveTheRemovedEntrysApproval` |
| D1: a removed entry is not offered to take an invoice | `entries.attachableBy` | `EntryFileTests.App_119_ARemovedEntryIsNotOffered` |
| D2: a report issued before keeps the removed entry, and the next shows the difference | BR-26 | `RemoveApprovedEntryTests.D2_AReportIssuedBeforeKeepsTheRemovedEntryAndTheNextShowsTheDifference` |
| D3: nothing is reversed any more | No route | `RemoveApprovedEntryTests.D3_NothingIsReversedAnyMore` |
| D5: an entry reversed before reads as an ordinary approved entry | `AccountHistory`, `PartnerHistory` | `RemoveApprovedEntryTests.D5_AnEntryReversedBeforeReadsAsAnOrdinaryApprovedEntry` |
| A description longer than 2,000 characters answers `invalidDescription`; one of 2,000 is kept (redix-api #129) | `entries.Routes.describe`, `entries.descriptionMaxLength` | `EntryChangesTests.NotesLongerThanTheLimitAreRefusedAndNotesOfTheLimitAreKept` |

## On the web: Change and Fix (redix-api #166)

An entry the person may change shows Change in the details panel, Fix for a rejected one (`entries.Row.Changeable`, from `refusalOfChange`). It opens Add entry's form in a tab of its own, filled with the entry as it stands (`entries.Typed`: its table, fields, columns, payment and supplier; a credit note's amount without its minus), drawn again as it changes as Add entry is. Saving goes through `entries.Change`: the checks the app's route makes, then every check a new entry passes. The app's route `PUT /projects/{id}/entries/{entryId}` calls `entries.Change` too, the one rule both doors run (findings row 292). Its pages are in `go-backend/web` (`addentry.go`, `addentry.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/changeEntry?entry={id}` | Change or Fix, filled with the entry; then drawn again with what was typed | `200` the tab's content; for an entry gone or one the person may not change, a line saying so |
| `POST /entry/{id}` | Saves the change, `submit=draft` or `submit=send` | `200` the form saying it was kept, the entry in the details panel; or as typed, saying why not; `404` for an entry the person does not see |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Change fills the form with the entry and saves through the app's rule (UC-033) | `entries.Typed`, `entries.Change` | `TestWeb_R166_UC033_ChangeFillsTheFormAndSavesThroughTheAppsRule` |
| An approved entry is not changed (BR-11) | `entries.refusalOfChange` | `TestWeb_R166_UC033_AnApprovedEntryIsNotChanged` |
| A rejected entry opens as Fix, its columns filled | `web.entryForm.title`, `entries.Typed` | `TestWeb_R166_UC033_FixFillsARejectedEntryWithItsColumns` |
| A failing database answers 500 and keeps nothing | `changes.Writer.Run` | `TestWeb_R166_ChangeAnswersAFailingDatabaseWith500` |
