[Documentation](../README.md) › [Use cases](../README.md) › UC-064

# UC-064: Keep a project's documents

**Purpose:** keep a project's documents, each under a name and, if wanted, in a group, and hand them to everyone who works on it.

**Who uses it:** whoever holds `ManageProject` keeps and removes them; everyone who reads the project opens them.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/documents` | The documents: those in no group first, then each group in order, newest first in each |
| `POST /projects/{id}/documents` | Keeps a file, sent with its name and group, as a document of the project |
| `DELETE /projects/{id}/documents/{documentId}` | Takes a document off the list; its file stays |

A document is any file of at most 25 MB, kept in the attachment store outside the database and opened through `GET /projects/{id}/attachments/{attachmentId}` like any other file. A picture or a PDF is known by its first bytes; any other file is named by its extension, so the phone knows which app opens it. Only a document may be any file: the attachment route that entries, contracts and receipts use still keeps only pictures and PDFs.

Its group is a free name of at most 100 letters, empty for none. It only groups the list: no unit, report or other part of REDIX reads it.

These answers were decided with the owner on 7 November 2026 (redix-api 92 "Documents in place of المخططات: add one with a name and its file"), replacing the blueprints: no unit, any file type, an optional group; many documents per group, with no version history; a removed one leaves only the list, since no file is ever deleted (NFR-11).

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The manager keeps documents; everyone reads them, those in no group first, then by group | `documents.Routes.add`, `List` | `DocumentsTests.UC_064_TheManagerKeepsDocumentsAndEveryoneReadsThemUngroupedFirstThenByGroup` |
| A document is any file, named by its extension, while an invoice stays a picture or a PDF | `attachments.Keep`, `attachments.kindOf` | `DocumentsTests.ADocumentIsAnyFileNamedByItsExtensionWhileAnInvoiceStaysAPictureOrAPdf` |
| A document needs a name, a group of 100 letters at most, a file of 25 MB at most, and is kept once | `documents.Routes.add` | `DocumentsTests.ADocumentNeedsANameAGroupOf100LettersAtMostAFileOf25MbAtMostAndIsKeptOnce` |
| Findings row 137: a document's request may hold as much as a form may (128 MB), not the server's 28 MB, so a file over 25 MB is answered `fileTooLarge` rather than cut off | `documents.Map` | `DocumentsTests.Findings137_ADocumentsRequestMayHoldAsMuchAsAFormSoAFileOver25MbIsAnsweredRatherThanCutOff` |
| A removed document leaves the list; its file stays | `documents.Routes.remove` | `DocumentsTests.ARemovedDocumentLeavesTheListButItsFileStays` |

## On the web: Documents (redix-api #175 D3)

Documents is the app's screen in a tab of the side menu, for everyone who reads the project: the documents from `documents.ListOf`, the function `GET /projects/{id}/documents` answers, those in no group first, then each group under its heading, each with its name, its file's name and the day it was kept in the project's time zone; a search finding a name, a group or a file's name, as the app's. A document opens in the details panel, its picture shown when it is one, as the app's viewer shows it, with Download handing the file over under its own name, through `attachments.FileOf`, the rule `GET /projects/{id}/attachments/{attachmentId}` reads it with, and Remove, asked first as the app's swipe asks, through `documents.Remove`. Add document, gray for whoever may not manage the project or in a finished one, asks the name, the group with the groups already kept to choose from, and the file, as the app's dialog, and keeps it through `documents.Add`, the rule `POST /projects/{id}/documents` runs; a form with no file says to choose one, as the app's dialog does, and a refusal says why in the app's words. Its pages are in `go-backend/web` (`documents.go`, `documents.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/documents` | Documents, `text` its search | `200` the tab's content |
| `GET /details/document/{id}` | A document's panel, `asking=remove` asking first; `new` is Add document's form | `200` the panel; `404` for a document the project does not hold, or `new` without ManageProject |
| `POST /document/new/add` | Keeps the form's `file` under its `name` and `group` | `200` the new document's panel saying it was saved, Documents beside it; or the form saying why not; `403` without ManageProject |
| `POST /document/{id}/remove` | Takes the document off the list | `200` the panel closed, Documents beside it; or the panel saying why not; `403` without ManageProject; `404` for a document the project does not hold |
| `GET /attachment/{id}` | A file the project keeps, under its own name | `200` the file; `404` for one of another project, or whose file is not kept |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Documents lists the project's documents, those in no group first, then by group, and finds a name, a group or a file's name; a document's panel shows its picture and hands its file over; Add document keeps a file under its name and group, refused as the app's; Remove asks first; whoever does not manage the project reads and downloads only, and a finished project keeps nothing (UC-064, AC-04.1) | `documents.ListOf`, `documents.Add`, `documents.Remove`, `attachments.FileOf` | `TestWeb_R175_UC064_DocumentsAreListedKeptDownloadedAndRemovedAsTheAppsScreen` |
