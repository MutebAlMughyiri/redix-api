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

These answers were decided with Muteb on 7 November 2026 (redix-api 92 "Documents in place of المخططات: add one with a name and its file"), replacing the blueprints: no unit, any file type, an optional group; many documents per group, with no version history; a removed one leaves only the list, since no file is ever deleted (NFR-11).

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The manager keeps documents; everyone reads them, those in no group first, then by group | `documents.Routes.add`, `List` | `DocumentsTests.UC_064_TheManagerKeepsDocumentsAndEveryoneReadsThemUngroupedFirstThenByGroup` |
| A document is any file, named by its extension, while an invoice stays a picture or a PDF | `attachments.Keep`, `attachments.kindOf` | `DocumentsTests.ADocumentIsAnyFileNamedByItsExtensionWhileAnInvoiceStaysAPictureOrAPdf` |
| A document needs a name, a group of 100 letters at most, a file of 25 MB at most, and is kept once | `documents.Routes.add` | `DocumentsTests.ADocumentNeedsANameAGroupOf100LettersAtMostAFileOf25MbAtMostAndIsKeptOnce` |
| Findings row 137: a document's request may hold as much as a form may (128 MB), not the server's 28 MB, so a file over 25 MB is answered `fileTooLarge` rather than cut off | `documents.Map` | `DocumentsTests.Findings137_ADocumentsRequestMayHoldAsMuchAsAFormSoAFileOver25MbIsAnsweredRatherThanCutOff` |
| A removed document leaves the list; its file stays | `documents.Routes.remove` | `DocumentsTests.ARemovedDocumentLeavesTheListButItsFileStays` |
