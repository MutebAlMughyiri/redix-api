[Documentation](../README.md) › [Use cases](../README.md) › UC-053 to UC-058

# UC-053 to UC-058: The periodic report and the report files

**Purpose:** issue the numbered periodic report that replaces the manual statement, and give it and the other reports as files: the periodic report as a PDF laid out as report 23 and as a workbook with live totals; the VAT statement as the accountant's workbook, a PDF and a ZIP of its invoices; an item's report and the contracts report as a PDF and a workbook.

**Who uses it:** whoever holds `MoveMoney` issues. Everyone who reads the project reads a report, seeing only their own partner line (BR-31). Only whoever holds `SeeAllCapital` gets the files: "exporting stays theirs alone".

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/periodic-reports` | The reports issued, newest first |
| `GET /projects/{id}/periodic-reports/next` | The next number, and what waits for approval |
| `POST /projects/{id}/periodic-reports` | Issues the next report as a snapshot that never changes |
| `GET /projects/{id}/periodic-reports/{number}` | A report as issued |
| `GET /projects/{id}/periodic-reports/{number}/pdf`, `/xlsx` | Its files |
| `GET /projects/{id}/vat-statement/xlsx`, `/pdf`, `/zip?from=&to=` | The VAT statement's files |
| `GET /projects/{id}/reports/items/{itemId}/pdf`, `/xlsx` | An item's report as files |
| `GET /projects/{id}/reports/contracts/pdf`, `/xlsx` | The contracts report as files |

A report keeps every figure as issued, in `PeriodicReports.Snapshot`. What was approved after the previous report is new and shaded green, whatever its date (BR-26). `ReportFormats` is the one owner of how a file writes a number, a date and a sheet name, and of the embedded Arabic font (NFR-13). The PDFs use QuestPDF under its Community licence; the workbooks use ClosedXML.

**Not proven:** AC-29.1, AC-45.2 and AC-47.1's totals are golden tests against report 23 and the accountant's file, which need the history of غصون 6 imported (UC-052). AC-33.1's "opens in Excel, Google Sheets and Numbers" has not been tried in those programs; the tests open each file with ClosedXML. Findings row 119 says what is left.

| Rule | Code | Test |
| --- | --- | --- |
| AC-29.3: numbered with no gaps; the next says what waits | `PeriodicReportsEndpoints.Issue`, `Next` | `ReportFilesTests.AC_29_3_ReportsAreNumberedWithNoGapsAndTheNextSaysWhatWaits` |
| AC-29.2: an entry approved later leaves the issued report, and is new in the next | `PeriodicReportsEndpoints.SnapshotAsync`, `ItemReportEndpoints.OfAsync` | `ReportFilesTests.AC_29_2_AnEntryApprovedLaterLeavesTheIssuedReportAndIsNewInTheNext` |
| The summary adds up as the statement does | `PeriodicReportsEndpoints.SnapshotAsync` | `ReportFilesTests.TheSummaryAddsUpAsTheStatementDoes` |
| BR-31: everyone reads; only their own partner line; no one else exports | `PeriodicReportSnapshot.ForPartnerOf` | `ReportFilesTests.BR_31_EveryoneReadsTheReportAndSeesOnlyTheirOwnPartnerLineButNoOneElseExports` |
| AC-45.1: a PDF, the summary and partners then a page per item | `PeriodicReportFiles.Pdf` | `ReportFilesTests.AC_45_1_ThePdfIsAPdfWithAPagePerItemAfterTheSummaryAndPartners` |
| AC-46.1: the workbook's summary adds the item sheets live | `PeriodicReportWorkbook.Of` | `ReportFilesTests.AC_46_1_TheWorkbookHasItsSheetsAndTheSummaryAddsTheItemSheetsLive` |
| AC-47.1: the VAT workbook is the accountant's file | `VatStatementFiles.Excel` | `ReportFilesTests.AC_47_1_TheVatWorkbookIsTheAccountantsFile` |
| UC-058: the ZIP holds each invoice named by its row | `VatStatementFiles.Zip` | `ReportFilesTests.UC_058_TheZipHoldsEachInvoiceNamedByItsRow` |
| AC-33.1: the item and contracts reports export | `ItemReportFiles` | `ReportFilesTests.AC_33_1_TheItemAndContractsReportsExport` |
