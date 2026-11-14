[Documentation](../README.md) › [Use cases](../README.md) › UC-039

# UC-039: Attach a shared file to a recorded entry

**Purpose:** put a file that arrived later, shared to REDIX, on an entry already recorded: any entry its person may handle, of any kind, an approved one too, beside the files it has (redix-app #115).

**Who uses it:** whoever holds `RecordEntries`, on the entries they may handle (`EntryChanges.MayHandle`): their own, and with `ManageAnyEntry` anyone's but a draft (BR-42).

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/entries?attachable=true&text=&...` | The entries list (UC-028) kept to those the caller may put a file on, with its search, a page at a time: what the share screen lists (D6) |
| `GET /projects/{id}/entries/attachable?vatNumber=&total=&date=` | The entries a shared file's QR code matches, as the lists show them, which the share screen pins over the list (AC-49.2, D4); none for a file with no code |
| `POST /projects/{id}/entries/{entryId}/invoice` with `{ attachmentId }` | Puts the file on it, beside any it has |

An entry can take a file when it pays something, so no deduction or credit note, and was not removed (redix-api #119), whatever its kind and whether it has files already (D1, D5); `InvoiceAttachEndpoints.AttachableBy` answers it, with `EntryChanges.MayHandle`, for the list and for the attach alike (D2, findings row 198). The file is what the entry's table names its file (`EntryTable.FileRole`, D5): an invoice for a purchase, a receipt for a contractor payment, a bank receipt for a partner's money, anything else for revenue, and anything else for a table the project no longer offers. Attaching puts only the file on it and writes it in the History table (redix-api #96, D15): its figures and its status stay as they were, an approved one included, and nobody is asked again.

| Rule | Code | Test |
| --- | --- | --- |
| AC-46.10c, D5: every kind that pays something is listed, with or without a file | `InvoiceAttachEndpoints.AttachableBy`, `EntriesEndpoints.List` | `InvoiceAttachTests.AC_46_10c_D5_EveryKindThatPaysSomethingIsListedWithOrWithoutAFile` |
| A deduction, a credit note and a removed entry are not listed and cannot take one | `InvoiceAttachEndpoints.AttachableBy` | `InvoiceAttachTests.AC_46_10c_ADeductionACreditNoteAndARemovedEntryAreNotListedAndCannotTakeOne` |
| AC-46.10f, R7, R8: a person lists and attaches to their own; with `ManageAnyEntry` others' too, never their drafts; the list and the attach agree | `EntryChanges.MayHandle`, `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AC_46_10f_FieldListsAndAttachesToTheirOwnAlone`, `AC_46_10f_WhoeverManagesAnyEntryListsAndAttachesToOthersButNeverToTheirDrafts` |
| AC-49.2, D6: the QR code's match alone is answered, for the pin over the list | `InvoiceAttachEndpoints.Attachable` | `InvoiceAttachTests.AC_49_2_D6_TheQrCodesMatchAloneIsAnsweredForThePinOverTheList` |
| AC-46.10a, R10, D15: a file goes on an approved entry, its figures stay, and a History line is written | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AC_46_10a_R10_AFileGoesOnAnApprovedEntryItsFiguresStayAndAHistoryLineIsWritten` |
| AC-46.10e, R5, D1: an entry with a file is listed, and keeps both after the attach | `InvoiceAttachEndpoints.AttachableBy`, `Attach` | `InvoiceAttachTests.AC_46_10e_R5_AnEntryWithAFileIsListedAndKeepsBothFilesAfterTheAttach` |
| D5: each file takes its table's role | `InvoiceAttachEndpoints.FileRoleOfAsync`, `EntryTable.FileRole` | `InvoiceAttachTests.D5_EachFileTakesItsTablesRole` |
| D15: every entry keeps its status, and nobody is asked again | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AWaitingEntryStaysWaitingAndADraftStaysADraft`, `D15_AnApprovedEntryStaysApprovedAndNobodyIsAskedAgain` |
| G1, BR-22: a file already on an entry is refused, the same entry too | `EntryRecording.IsFileOnAnEntryAsync` | `InvoiceAttachTests.G1_BR_22_AFileAlreadyOnAnEntryIsRefused` |
| A file of no such attachment is refused | `InvoiceAttachEndpoints.Attach` | `InvoiceAttachTests.AFileOfNoSuchAttachmentIsRefused` |
