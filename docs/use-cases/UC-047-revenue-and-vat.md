[Documentation](../README.md) › [Use cases](../README.md) › UC-047, UC-048, UC-056

# UC-047, UC-048, UC-056: Revenue, sales invoices and the VAT statement

**Purpose:** record money that comes in other than capital, record sales invoices for the VAT statement, and build that statement for any period.

**Who uses it:** whoever holds `MoveMoney` records; everyone who reads the project reads.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/revenue` | Revenue, newest first; a VAT refund with what its quarter's statement expected |
| `POST /projects/{id}/revenue` with `{ itemId, date, amount, accountId, description }` | Records revenue on a revenue item, whose type says what it is: a VAT refund (with `periodStart`), a sale or other revenue; anything but an active revenue item of the project answers `unknownItem` |
| `POST /projects/{id}/sales-invoices` | Records a sales invoice, on a sale already recorded or as a new one on the sale item named, or else the project's first |
| `GET /projects/{id}/vat-statement?from=&to=` | The statement, a section per quarter, and its exceptions |
| `POST /projects/{id}/entries/{entryId}/vat-period` | Declares an invoice in a later quarter (BR-17) |

`VatStatement` is the one owner of which invoices the statement holds and of the net VAT due. `ProjectFigures` splits revenue into capital, VAT refunds, sales and other by the type of the revenue item each entry came in on, and gives the surplus with and without sales (BR-07).

**Revenue items (UC-068, FR-28, FR-38).** Revenue is recorded on items, as expenses are: an item of type `Sale`, `VatRefund` or `OtherRevenue` (`RevenueDetails.ItemId`, `SubgroupId`). The type lives only on the item. The template and every project hold three revenue categories of one item each: استرجاع ضرائب, إيرادات أخرى, and فلوس البيع المودعة بالحساب holding البيع. The migration `RevenueItems` gave every existing project those three and moved each revenue entry to the item of its old kind.

**Open question 6** (the villa sales tax) is answered for now with both: a sale carries the real estate transaction tax by default, at 5%, kept out of the net VAT due and shown on its own line, or VAT at the project's rate. The accountant still has to confirm.

| Rule | Code | Test |
| --- | --- | --- |
| AC-28.1: a refund raises revenue and surplus, and shows its difference from the statement | `RevenueEndpoints`, `VatStatement.NetDueOfAsync` | `RevenueTests.AC_28_1_ARefundRaisesRevenueAndSurplusAndShowsItsDifferenceFromTheStatement` |
| A refund is for a quarter | `RevenueEndpoints.Record` | `RevenueTests.ARefundIsForAQuarter` |
| Other revenue is for its own day unless told, and reaches its account | `RevenueEndpoints.Record`, `AccountMoney` | `RevenueTests.OtherRevenueIsForItsOwnDayUnlessToldAndReachesItsAccount` |
| Only whoever moves money records revenue | `MapProjectChanges(MoveMoney)` | `RevenueTests.OnlyWhoeverMovesMoneyRecordsRevenue` |
| AC-42.1: a sale with VAT reaches the sales section and changes the net due | `RevenueEndpoints.RecordSale`, `VatQuarterView` | `RevenueTests.AC_42_1_ASaleWithVatReachesTheSalesSectionAndChangesTheNetDue` |
| A sale with the real estate tax stays out of the net VAT | `VatQuarterView.RealEstateTax` | `RevenueTests.ASaleWithTheRealEstateTaxShowsItsTaxAndStaysOutOfTheNetVat` |
| AC-42.1: a sale already recorded as revenue counts once | `RevenueEndpoints.RecordSale` | `RevenueTests.AC_42_1_ASaleAlreadyRecordedAsRevenueCountsOnce` |
| What a sales invoice cannot be is refused | `RevenueEndpoints.RefusalOfTyped` | `RevenueTests.WhatASalesInvoiceCannotBeIsRefused` |
| FR-28: revenue goes on a revenue item and never on an expense one | `RevenueEndpoints.Record` | `RevenueTests.FR_28_RevenueGoesOnARevenueItemAndNeverOnAnExpenseOne` |
| AC-46.7: the dashboard keeps revenue categories apart from expense ones | `DashboardEndpoints.Dashboard` | `RevenueTests.AC_46_7_TheDashboardKeepsRevenueCategoriesApartFromExpenseOnes` |
| Reversing a sale takes it out of the statement and the revenue | `EntryReversal` | `RevenueTests.ReversingASaleTakesItOutOfTheStatementAndTheRevenue` |
| AC-32.1: the statement adds the kept decimals (BR-13) and generates its description | `VatStatement.OfAsync` | `RevenueTests.AC_32_1_TheStatementAddsTheKeptDecimalsAndGeneratesItsDescription` |
| AC-32.2: a custom period across quarters has a section per quarter | `VatStatement.OfAsync` | `RevenueTests.AC_32_2_ACustomPeriodAcrossQuartersHasASectionPerQuarter`, `APeriodEndingBeforeItStartsIsRefused` |
| AC-32.3: what waits, has no VAT number or no invoice is left out and listed | `VatStatement.RefusalOf` | `RevenueTests.AC_32_3_WhatWaitsHasNoVatNumberOrNoInvoiceIsLeftOutAndListed` |
| UC-058: a row with no file is listed | `VatStatement.OfAsync` | `RevenueTests.UC_058_ARowWithNoFileIsListed` |
| BR-17: an invoice declared later leaves its quarter and is listed in the later one | `RevenueEndpoints.DeclareLater` | `RevenueTests.BR_17_AnInvoiceDeclaredLaterLeavesItsQuarterAndIsListedInTheLaterOne`, `AnInvoiceCannotBeDeclaredInAnEarlierQuarter` |
