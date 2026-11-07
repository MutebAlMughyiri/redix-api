[Documentation](../README.md) › [Use cases](../README.md) › UC-047, UC-048, UC-056

# UC-047, UC-048, UC-056: Revenue, sales invoices and the VAT statement

**Purpose:** record money that comes in other than capital, record sales invoices, and give the VAT statement of any period as the accountant's workbook.

**Who uses it:** whoever holds `MoveMoney` records; everyone who reads the project reads; whoever holds `SeeAllCapital` takes the statement's workbook.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/revenue` | Revenue, newest first |
| `POST /projects/{id}/revenue` with `{ itemId, date, amount, accountId, description, periodStart, periodEnd }` | Records revenue on a revenue item, whose type says what it is: a VAT refund, a sale or other revenue; anything but an active revenue item of the project answers `unknownItem`. It is for its own day unless a period is given |
| `POST /projects/{id}/sales-invoices` with `{ buyerName, buyerVatNumber, number, invoiceDate, total, accountId, saleEntryId }` | Records a sales invoice, on a sale already recorded or as a new one on the sale item named, or else the project's first |
| `GET /projects/{id}/vat-statement/xlsx?from=&to=` | The statement's workbook (UC-053) |

**REDIX stores no VAT** (redix-api #96, D4; Muteb, 7 November 2026: "store only the invoice amount; the VAT is printed in the report without storing it"). `VatStatement` is the one owner of what the statement holds: the period's approved purchases and approved sales invoices, quarter by quarter, each a total with its date, its party, its party's VAT number and what it was. The workbook works the VAT out of each total with a formula at the configured rate, `vatRatePercent` (D18, D19): `VAT = total × rate ÷ (100 + rate)`, on every row, whether its party has a VAT number or not: an empty number means nothing (Muteb, 7 Nov 2026: "if no vat number added when adding new supplier that is means nothing").

`ProjectFigures` splits revenue into capital, VAT refunds, sales and other by the type of the revenue item each entry came in on (BR-07).

**A VAT refund is plain revenue** (D23): it has no quarter and no statement to compare with.

**A sale is a total** (D22): its buyer, the buyer's VAT number, its number, its date and its total. It has no tax type and no discount.

**Revenue items (UC-068, FR-28, FR-38).** Revenue is recorded on items, as expenses are: an item of type `Sale`, `VatRefund` or `OtherRevenue` (`RevenueDetails.ItemId`, `SubgroupId`). The type lives only on the item. The template and every project hold three revenue categories of one item each: استرجاع ضرائب, إيرادات أخرى, and فلوس البيع المودعة بالحساب holding البيع.

| Rule | Code | Test |
| --- | --- | --- |
| D23: a refund is plain revenue that raises revenue and surplus | `RevenueEndpoints.Record` | `RevenueTests.D23_ARefundIsPlainRevenueThatRaisesRevenueAndSurplus` |
| Other revenue is for its own day unless told, and reaches its account | `RevenueEndpoints.Record`, `AccountMoney` | `RevenueTests.OtherRevenueIsForItsOwnDayUnlessToldAndReachesItsAccount`, `APeriodEndingBeforeItStartsIsRefusedForRevenue` |
| Only whoever moves money records revenue | `MapProjectChanges(MoveMoney)` | `RevenueTests.OnlyWhoeverMovesMoneyRecordsRevenue` |
| AC-42.1, D22: a sale is a total that reaches the sales section | `RevenueEndpoints.RecordSale`, `VatStatement.OfAsync` | `RevenueTests.AC_42_1_D22_ASaleIsATotalThatReachesTheSalesSection` |
| AC-42.1: a sale already recorded as revenue counts once | `RevenueEndpoints.RecordSale` | `RevenueTests.AC_42_1_ASaleAlreadyRecordedAsRevenueCountsOnce` |
| What a sales invoice cannot be is refused | `RevenueEndpoints.RefusalOfTyped` | `RevenueTests.WhatASalesInvoiceCannotBeIsRefused` |
| FR-28: revenue goes on a revenue item and never on an expense one | `RevenueEndpoints.Record` | `RevenueTests.FR_28_RevenueGoesOnARevenueItemAndNeverOnAnExpenseOne` |
| AC-46.7: the dashboard keeps revenue categories apart from expense ones | `DashboardEndpoints.Dashboard` | `RevenueTests.AC_46_7_TheDashboardKeepsRevenueCategoriesApartFromExpenseOnes` |
| Reversing a sale takes it out of the statement and the revenue | `EntryReversal` | `RevenueTests.ReversingASaleTakesItOutOfTheStatementAndTheRevenue` |
| AC-32.1, D19: the statement lists approved purchases with their supplier and what was bought | `VatStatement.OfAsync` | `RevenueTests.AC_32_1_D19_TheStatementListsApprovedPurchasesWithTheirSupplierAndWhatWasBought` |
| AC-32.2: a custom period across quarters has a section per quarter | `VatStatement.OfAsync` | `RevenueTests.AC_32_2_ACustomPeriodAcrossQuartersHasASectionPerQuarter`, `APeriodEndingBeforeItStartsIsRefused` |

The workbook's formula and its rate are proved in `ReportFilesTests` (UC-053).
