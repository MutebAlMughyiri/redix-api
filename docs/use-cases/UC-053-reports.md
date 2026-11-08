[Documentation](../README.md) › [Use cases](../README.md) › UC-053 to UC-058

# UC-053 to UC-058: The periodic report and the report files

**Purpose:** issue the numbered periodic report that replaces the manual statement, and give it and the other reports as files: the periodic report as a PDF laid out as report 23 and as a workbook with live totals; the VAT statement as the accountant's workbook; an item's report and the contracts report as a PDF and a workbook.

**Who uses it:** whoever holds `MoveMoney` issues. Everyone who reads the project reads a report, seeing only their own partner line (BR-31). Only whoever holds `SeeAllCapital` gets the files: "exporting stays theirs alone".

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/periodic-reports` | The reports issued, newest first |
| `GET /projects/{id}/periodic-reports/next` | The next number, and what waits for approval |
| `POST /projects/{id}/periodic-reports` | Issues the next report as a snapshot that never changes |
| `GET /projects/{id}/periodic-reports/{number}` | A report as issued |
| `GET /projects/{id}/periodic-reports/{number}/pdf`, `/xlsx` | Its files |
| `GET /projects/{id}/vat-statement/xlsx?from=&to=` | The VAT statement's workbook, its VAT a formula on each total at `vatRatePercent` (redix-api #96, D19, D20) |
| `GET /projects/{id}/reports/items/{itemId}/pdf`, `/xlsx` | An item's report as files |
| `GET /projects/{id}/reports/contracts/pdf`, `/xlsx` | The contracts report as files |

**One table design (UC-067, FR-46).** Every sheet but الخلاصة is a list of tables written by `StatementWriter`, the one layout; a builder (`ItemReportFiles.SheetOf`, `ContractsExcel`, `RevenueOf`) only says what each table holds; رأس المال is an item like any other, its sheet a table per partner (redix-api #109). A table is a title row holding only its title, a header, its numbered rows, then its lines, each on a row of its own: الاجمالي; المتبقي and مبلغ الاتفاق when an agreed amount was filled in; its percentage when the sheet calculates one. A sheet of more than one table ends with اجمالي الكل, then نسبة الكل with percentages. Every amount sits in the sheet's last column; a narrower table stretches its first column. Every sheet, الخلاصة included, fits each column to its content up to 45 characters and wraps longer text (`StatementStyle.Fit`), so nothing is cut on screen or on paper. The capital sheet always calculates shares (نسبة المشاركة); an item does when it chose to (`Items.ShowsTablePercentages`). Every line of the summary has its own sheets, and no sheet serves two lines: a line is a category, a sheet is an item.

A report keeps every figure as issued, in `PeriodicReports.Snapshot`. What was approved after the previous report is new and shaded green, whatever its date (BR-26). `ReportFormats` is the one owner of how a file writes a number, a date and a sheet name, and of the embedded Arabic font (NFR-13). The PDFs use QuestPDF under its Community licence; the workbooks use ClosedXML.

**Not proven:** AC-29.1, AC-45.2 and AC-47.1's totals are golden tests against report 23 and the accountant's file, which need the history of غصون 6 imported (UC-052). AC-33.1's "opens in Excel, Google Sheets and Numbers" has not been tried in those programs; the tests open each file with ClosedXML. Findings row 119 says what is left.

| Rule | Code | Test |
| --- | --- | --- |
| AC-29.3: numbered with no gaps; the next says what waits | `PeriodicReportsEndpoints.Issue`, `Next` | `ReportFilesTests.AC_29_3_ReportsAreNumberedWithNoGapsAndTheNextSaysWhatWaits` |
| Findings row 147: what waits is two figures, expenses and revenue with partners' money (a withdrawal takes away); the dashboard, the next report and the issued report read one owner | `WaitingFigures.OfAsync` | `ReportFilesTests.Row_147_WhatWaitsIsExpensesAndRevenueWithPartnersMoneyApartAndTheDashboardTheNextAndTheIssuedReportAgree` |
| Findings row 130: the PDF draws the workbook's own tables, so every figure of the item sheets, رأس المال's among them (each row, الاجمالي, المتبقي, مبلغ الاتفاق, each share, اجمالي الكل, نسبة الكل) is a word on the PDF as often as the workbook shows it; read with PdfPig, a test-only package | `StatementPdf.Draw` | `ReportFilesTests.Row_130_EveryFigureOfTheWorkbooksItemSheetsRasAlMalAmongThemIsOnThePeriodicReportsPdf`, `Row_130_EveryFigureOfAnItemsWorkbookIsOnItsPdf` |
| The workbook shows waiting expenses beside their difference and waiting revenue under the surplus, leaving the statement's layout as it was | `PeriodicReportWorkbook.FillSummary` | `ReportFilesTests.Row_147_TheWorkbookShowsWaitingExpensesBesideTheirDifferenceAndWaitingRevenueUnderTheSurplus` |
| AC-29.2: an entry approved later leaves the issued report, and is new in the next | `PeriodicReportsEndpoints.SnapshotAsync`, `ItemReportEndpoints.OfAsync` | `ReportFilesTests.AC_29_2_AnEntryApprovedLaterLeavesTheIssuedReportAndIsNewInTheNext` |
| The summary adds up as the statement does | `PeriodicReportsEndpoints.SnapshotAsync` | `ReportFilesTests.TheSummaryAddsUpAsTheStatementDoes` |
| BR-31: everyone reads; only their own partner line; no one else exports | `PeriodicReportSnapshot.ForPartnerOf` | `ReportFilesTests.BR_31_EveryoneReadsTheReportAndSeesOnlyTheirOwnPartnerLineButNoOneElseExports` |
| BR-31, redix-api #109: a report read by someone who sees only their own capital holds no item report, since رأس المال's holds every partner's money; only the files, which they cannot take, use them | `PeriodicReportSnapshot.ForPartnerOf` | `ReportFilesTests.BR_31_UC_109_AReportForAPartnerHoldsTheirLineAndNoItemReportWhichOnlyTheFilesTake` |
| AC-45.1: a PDF, the summary then a page per item, رأس المال among them; the summary's revenue is its categories, as its expenses are | `PeriodicReportFiles.Pdf` | `ReportFilesTests.AC_45_1_ThePdfIsAPdfWithAPagePerItemAfterTheSummary` |
| AC-46.1: the workbook is laid out as the statement, every total live | `PeriodicReportWorkbook.Of` | `ReportFilesTests.AC_46_1_TheWorkbookIsLaidOutAsTheStatementAndEveryTotalIsLive` |
| AC-46.2: a contract's table has, under it, الاجمالي, then المتبقي (the agreed amount less the payments inside it), then مبلغ الاتفاق; with no agreed amount filled in, only الاجمالي | `StatementWriter.Add`, `ItemReportFiles.SheetOf` | `ReportFilesTests.AC_46_2_AContractsTotalWhatRemainsAndItsAgreedAmountAreLinesUnderItsTable`, `AC_46_2_AContractWithNoAgreedAmountShowsOnlyItsTotal` |
| AC-46.3: items of one name in two categories each name their category on their sheet | `PeriodicReportWorkbook.Of` | `ReportFilesTests.AC_46_3_ItemsOfOneNameInTwoCategoriesEachNameTheirCategoryOnTheirSheet` |
| AC-46.7: the revenue side is each revenue category in the structure's order, رأس المال first, each on its own sheet; a line of only sales is muted and left out of بدون البيع | `PeriodicReportWorkbook.FillSummary`, `ItemReportFiles.SheetOf` | `ReportFilesTests.AC_46_7_UC_109_TheRevenueSideIsEachRevenueCategoryInTheStructuresOrderRasAlMalFirstEachOnItsOwnSheetAndSalesAreMuted` |
| AC-46.5: a sheet of one table has no اجمالي الكل and no percentage; the summary points at its الاجمالي | `StatementWriter.Add` | `ReportFilesTests.AC_46_5_ASheetOfOneTableHasNoGrandTotalNorPercentageAndTheSummaryPointsAtItsTotal` |
| AC-46.6, redix-api #109: رأس المال is an item whose sheet is a table per partner, each one's share (النسبة) under their total and اجمالي الكل under them, through the item's own table percentages (AC-46.9) | `ItemReportFiles.SheetOf`, `StatementWriter.Add` | `ReportFilesTests.AC_46_6_UC_109_RasAlMalIsAnItemWhoseTablesAreItsPartnersEachOnesShareUnderTheirTotalAndTheWholeCapitalUnderThem` |
| AC-46.8: each subgroup with purchases is its own table, the general one titled with the item's name; an item's tables, contracts too, print in the order they were put in | `ItemReportFiles.SheetOf` | `ReportFilesTests.AC_46_8_EachSubgroupWithPurchasesIsItsOwnTableTitledWithItsNameTheGeneralOneWithTheItems`, `AC_46_8_TheSheetPrintsTheItemsTablesInTheOrderTheyWerePutIn` |
| AC-46.9: an item that chooses it shows each table's percentage under its total and نسبة الكل under اجمالي الكل | `StatementWriter.Add`, `CategoriesEndpoints.SetTablePercentages` | `ReportFilesTests.AC_46_9_AnItemThatChoosesPercentagesShowsEachTablesShareUnderItsTotalAndTheWholeUnderTheGrandTotal`, `CategoriesTests.AC_46_9_AnItemsChoiceToCalculateEachTablesPercentageIsKeptAndInTheAuditTrail` |
| AC-46.11: every column fits its content up to 45 characters, past which its text wraps; no row keeps a height of its own, so Excel grows it to its wrapped text | `StatementStyle.Fit`, `StatementStyle.AsBody` | `ReportFilesTests.AC_46_11_EveryColumnFitsItsContentAndTextPastFortyFiveCharactersWraps` |
| AC-46.10: every line of the summary has its own sheets; a category of one item is one line, only one of more items has a detail table | `PeriodicReportWorkbook.FillSummary` | `ReportFilesTests.AC_46_10_ACategoryOfOneItemIsOneLineAndOnlyACategoryOfMoreItemsHasADetailTable` |
| AC-47.1, D19: the VAT workbook works the VAT out of every total at the configured rate, whatever the party's VAT number | `VatStatementFiles.Excel` | `ReportFilesTests.AC_47_1_D19_TheVatWorkbookWorksVatOutOfEachTotalAtTheConfiguredRate` |
| D18: the rate is the config value `vatRatePercent` | `ReportFilesEndpoints.VatExcel` | `ReportFilesTests.D18_TheRateIsTheConfigValueOfVatRatePercent` |
| AC-33.1: the item and contracts reports export | `ItemReportFiles` | `ReportFilesTests.AC_33_1_TheItemAndContractsReportsExport` |
| Findings row 131: the contracts workbook is one statement table of each contract's contractor, item, value and what was paid, its amount what is left | `ItemReportFiles.ContractsExcel` | `ReportFilesTests.Findings131_TheContractsWorkbookIsOneStatementTableOfWhatEachContractHasLeft` |
| AC-31.1: the item report asks the contract what remains: nothing for an open-value contract, and no overrun | `Contract.RemainingAfter`, `IsOverrunBy` | `ContractorPaymentsTests.TheItemReportLeavesAnOpenValueContractNothingRemainingAndNoOverrun`, `TheItemReportLeavesAValuedContractItsValueLessWhatWasPaid` |
| A partner's movement with no description is named in the statement's Arabic in both report files, never by its code | `ReportFormats.Movement` | `ReportFilesTests.APartnersMovementWithNoDescriptionIsNamedInArabicNeverByItsCode` |
