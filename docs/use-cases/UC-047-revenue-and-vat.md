[Documentation](../README.md) › [Use cases](../README.md) › UC-047, UC-048, UC-056

# UC-047, UC-056: Revenue and the VAT statement

**Purpose:** record money that comes in other than capital, and give the VAT statement of any period as the accountant's workbook.

**No Revenue screen and no sales invoice** (Muteb, 8 Nov 2026: "remove those completely"). Revenue is entered from Add entry, on a revenue item's table, waits for approval, and shows in القيود with every other entry, where it is reversed. The sales invoice, its route and its table went; RedixReal held none. The VAT statement keeps its purchases.

**Who uses it:** whoever holds `MoveMoney` records; everyone who reads the project reads; whoever holds `SeeAllCapital` takes the statement's workbook.

| Route | What it does |
| --- | --- |
| `POST /projects/{id}/revenue` with `{ itemId, date, amount, accountId, description, periodStart, periodEnd }` | Records revenue on a revenue item, whose type says what it is: a VAT refund, a sale or other revenue; anything but an active revenue item of the project answers `unknownItem`. It is for its own day unless a period is given |
| `GET /projects/{id}/vat-statement/xlsx?from=&to=` | The statement's workbook (UC-053) |

**REDIX stores no VAT** (redix-api #96, D4; Muteb, 7 November 2026: "store only the invoice amount; the VAT is printed in the report without storing it"). `VatStatement` is the one owner of what the statement holds: the period's approved purchases and approved sales invoices, quarter by quarter, each a total with its date, its party, its party's VAT number and what it was. The workbook works the VAT out of each total with a formula at the configured rate, `vatRatePercent` (D18, D19): `VAT = total × rate ÷ (100 + rate)`, on every row, whether its party has a VAT number or not: an empty number means nothing (Muteb, 7 Nov 2026: "if no vat number added when adding new supplier that is means nothing").

`ProjectFigures` splits revenue into capital, VAT refunds, sales and other by the type of the revenue item each entry came in on (BR-07).

**A VAT refund is plain revenue** (D23): it has no quarter and no statement to compare with.


**Revenue items (UC-068, FR-28, FR-38).** Revenue is recorded on items, as expenses are: an item of type `Sale`, `VatRefund` or `OtherRevenue` (`RevenueDetails.ItemId`, `SubgroupId`). The type lives only on the item. The template and every project hold three revenue categories of one item each: استرجاع ضرائب, إيرادات أخرى, and فلوس البيع المودعة بالحساب holding البيع.

| Rule | Code | Test |
| --- | --- | --- |
| D23: a refund is plain revenue that raises revenue and surplus | `RevenueEndpoints.Record` | `RevenueTests.D23_ARefundIsPlainRevenueThatRaisesRevenueAndSurplus` |
| UC-026: a credit note and a reversal take away from the statement what their bills counted | `VatStatement.OfAsync` | `RevenueTests.UC_026_ACreditNoteAndAReversalTakeAwayFromTheStatementWhatTheirBillsCounted` |
| Other revenue reaches its account; a revenue entry has no period since findings row 149 | `RevenueEndpoints.Record`, `AccountMoney` | `RevenueTests.OtherRevenueReachesItsAccount` |
| Only whoever moves money records revenue | `MapProjectChanges(MoveMoney)` | `RevenueTests.OnlyWhoeverMovesMoneyRecordsRevenue` |
| FR-28: revenue goes on a revenue item and never on an expense one | `RevenueEndpoints.Record` | `RevenueTests.FR_28_RevenueGoesOnARevenueItemAndNeverOnAnExpenseOne` |
| AC-46.7: the dashboard keeps revenue categories apart from expense ones | `DashboardEndpoints.Dashboard` | `RevenueTests.AC_46_7_TheDashboardKeepsRevenueCategoriesApartFromExpenseOnes` |
| BR-06, AC-46.7: the dashboard's revenue side is its revenue categories, رأس المال first as the structure orders it (redix-api #109), adding up to the revenue for every reader, as the periodic report's does | `DashboardEndpoints.OfAsync`, `ItemTotals` | `RevenueTests.BR_06_TheDashboardGivesEveryReaderTheCapitalSoTheRevenueSideAddsUpToTheRevenue` |
| Reversing a sale takes it out of the revenue | `EntryReversal` | `RevenueTests.ReversingASaleTakesItOutOfTheRevenue` |
| The entries list holds revenue and partners' money with their status; a partner's money shows only to whoever sees all capital, or to that partner (BR-31) | `EntriesEndpoints.RowsAsync` | `RevenueTests.OneList_TheEntriesListHoldsRevenueAndPartnersMoneyWithTheirStatus`, `BR_31_InTheEntriesListAPartnerSeesTheirOwnMoneyAndNoOneElses` |
| redix-app #83: an entry on the list carries its notes, the account it was paid from or reached and, for an expense, its payment method, which the app's shared text holds | `EntryRowView`, `EntryFacts.AccountNameOf`, `PaymentMethodNameOf` | `EntryChangesTests.UC_083_AnEntryOnTheListCarriesItsNotesAccountAndPaymentMethodForItsSharedText` |
| A waiting revenue or partner entry is deleted with everything it holds | `EntryChanges.RemoveDetailsAsync` | `RevenueTests.OneList_AWaitingRevenueOrPartnerEntryIsDeletedWithEverythingItHolds` |
| The revenue list and the sales invoice are gone | `RevenueEndpoints.MapRevenue` | `RevenueTests.NoRevenueList_TheRevenueListAndTheSalesInvoiceAreGone` |
| AC-32.1, D19: the statement lists approved purchases with their supplier and what was bought | `VatStatement.OfAsync` | `RevenueTests.AC_32_1_D19_TheStatementListsApprovedPurchasesWithTheirSupplierAndWhatWasBought` |
| AC-32.2: a custom period across quarters has a section per quarter | `VatStatement.OfAsync` | `RevenueTests.AC_32_2_ACustomPeriodAcrossQuartersHasASectionPerQuarter`, `APeriodEndingBeforeItStartsIsRefused` |
| Revenue takes its item as the table list offers it: an item of a disabled category answers `unknownItem` | `ProjectStructure.OfferedItemAsync` | `RevenueTests.RevenueOnAnItemOfADisabledCategoryIsRefusedAsTheTableListHidesIt` |

The workbook's formula and its rate are proved in `ReportFilesTests` (UC-053).
