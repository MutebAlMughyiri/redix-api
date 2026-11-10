[Documentation](../README.md) › UC-021

# UC-021: Store an invoice or receipt attachment

**What it is for:** keeping every invoice picture or PDF safe, and serving it only to people with access to its project.
**Who uses it:** whoever holds `AttachFiles` keeps a file, which is Finance and Field. Everyone with access to the project reads it.
**Issue:** redix-api #39; the app side is redix-app #40.

## How it fits together

A file is sent first and answered with its id, and whatever needs it names that id: a contract here, and an entry from UC-022 "Record a purchase expense". So an attachment belongs to a project from the moment it is sent, in `Attachments.ProjectId`, and that one column answers who may read it (the database design's decision 22).

The file itself is kept outside the database, under the folder `AttachmentOptions.Folder`, set with the environment variable `REDIX_Attachments__Folder`, and by default in the local application data of the user running the API. It is never in the repo. Its path is made from the project's and the attachment's ids, so nothing a person typed reaches the disk. What a file is comes from its first bytes, never from what the sender says.

No route deletes or replaces a file, so it stays for the legal retention period (NFR-11). `Attachments.Sha256` shows a kept file is unchanged.

A contract's documents join here (UC-020 step 3): adding or amending a contract names `attachmentIds`, and each one stays once attached.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/attachments` | `AttachFiles` | Keeps the multipart `file`, and answers its `AttachmentView` |
| `GET /projects/{id}/attachments/{attachmentId}` | `ReadProject` | Serves the file; one of another project is not found |
| `POST /projects/{id}/contracts`, `.../contracts/{contractId}` | `ManageSettings` | Now also take `attachmentIds`; `GET .../contracts` answers each contract's `documents` |

## Tables

`Attachments` and `ContractAttachments`, as the database design has them, with `Attachments.ProjectId` added. `EntryAttachments` needs `Entries`, so it comes with UC-022.

## Rules

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| An image or a PDF is kept with its name, type, size and hash (FR-09) | `Attachment.Of` | `AttachmentsTests.APictureOrAPdfIsKeptWithItsNameTypeSizeAndHash` |
| A file that is not an image or a PDF is refused | `Attachment.RefusalOf` | `AFileThatIsNotAPictureOrAPdfIsRefused`, `WhatAFileIsComesFromItsBytesNotFromWhatTheSenderSays` |
| A picture is under 1 MB (NFR-06) | `Attachment.RefusalOf` | `APictureOf1MbOrMoreIsRefusedAndOneJustBelowIsKept`; `APdfOf1MbOrMoreIsKept` |
| Files live outside the database, and come back unchanged | `AttachmentOptions.PathOf`, `AttachmentsEndpoints.Read` | `TheFileIsKeptOutsideTheDatabaseAndServedBackUnchanged` |
| A typed name never reaches the disk | `Attachment.Of` | `ANameTheSenderChoseNeverReachesThePathOfTheFile` |
| Only people with access to the project read a file (BR-33) | The `ReadProject` policy, `AttachmentsEndpoints.Read` | `SomeoneWithNoAccessToTheProjectCannotReadOrKeepAFile`, `AFileOfAnotherProjectIsNotFoundEvenForSomeoneInBoth`, `WithoutASessionNoFileIsServed` |
| Field and Finance keep files; Reader and Project manager only read them | `ProjectPermission` rows | `FieldAndFinanceKeepFiles`, `WhoHoldsNoAttachFilesCannotKeepAFileButReadsIt` |
| A finished project keeps no new file and still serves its own (AC-04.1) | `MapProjectChanges` | `AFinishedProjectKeepsNoNewFileAndStillServesItsOwn` |
| Keeping a file, and linking it to a contract, is in the History table with which file | `db.AddHistory` | `KeepingAFileIsInTheHistory`, `LinkingADocumentToAContractIsInTheHistoryWithWhichOne` |
| A contract carries its documents, each staying once attached, and only files of its project | `ContractsEndpoints` | `AContractCarriesItsDocumentFromWhenItIsAdded`, `AnAmendmentAddsADocumentAndKeepsThoseAlreadyCarried`, `ADocumentOfAnotherProjectIsRefusedOnAContract` |

## Not proved here

AC-N10, restoring a backup and finding entries, attachments and figures unchanged, needs a backup, and none exists yet: see the findings. The folder has to join the daily backup when one is set up (NFR-10).
