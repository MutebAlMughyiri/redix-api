[Documentation](../README.md) › UC-026

# UC-026: Record a credit note or a deduction

**What it is for:** recording money coming back as a negative entry.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field. Anyone who reads the project can read what can be returned.
**Issue:** redix-api #43; the app side is redix-app #44.

## How it fits together

**A credit note** (BR-18) is a purchase with `isCreditNote`, its own number and date, and an amount typed as a plain amount. `InvoiceFigures.Negated` is the one owner of its sign: all five figures in `Invoices` and `Entries.Amount` are stored negative, and each still adds up as an invoice's do. So anything that adds up figures, an item total or a VAT statement, takes away what the original counted with no check of what a row is. `Invoices.IsCreditNote` only says what it is.

It may name its original invoice, `originalEntryId`, kept as an `EntryLinks` row of type `CreditNoteOf`. The invoice has to be the same supplier's in the same project, sent or approved, and not itself a credit note. Its value is its `Total`, which is after its discount and with VAT. What the credit notes naming it hold together cannot go above that: `ExpensesEndpoints.Creditable` reckons what is left, counting every credit note but a rejected one. A linked credit note is computed at the rate of its invoice, `InvoiceFigures.RateOf`, not today's, so it takes back the VAT that was counted. Its date may be before the invoice's. An unlinked one follows the same rate as any new expense.

**A deduction** (FR-14) is a negative amount. It has no tax document, because that is what a credit note is for, so it goes the way of an expense without an invoice (UC-025): it says why in `withoutInvoiceReason`, has no `Invoices` row and no VAT, and shows in the exceptions list. `Entries.Amount` carries the negative, which reduces the item total.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/expenses` | `RecordEntries` | Now takes `isCreditNote`, `originalEntryId` and a negative `amount`. Answers `creditNoteAboveOriginal`, `unknownOriginalInvoice`, `creditNoteNeedsInvoice` and `deductionNeedsReason` |
| `GET /projects/{id}/expenses/creditable?supplierId=` | `ReadProject` | That supplier's invoices a credit note can return, each with what is left and its rate |

## Tables

`EntryLinks`, as the database design has it. Only `CreditNoteOf` is used so far.

## Rules

Tests are in `CreditNoteTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-14.2: a credit note with its own number and an earlier date is saved, and adds up negative | `InvoiceFigures.Negated`, `Build` | `AC_14_2_ACreditNoteOf320WithItsOwnNumberAndAnEarlierDateIsSavedAndShowsNegativeInTheVatStatement`, `BR_18_EveryFigureOfACreditNoteIsNegativeAndStillAddsUp` |
| BR-18: linking is optional | `CreditableInvoiceView.None` | `BR_18_ACreditNoteNeedNotBeLinked` |
| A credit note takes away what the original counted, on a partly discounted invoice too | `InvoiceFigures.Negated` | `BR_18_AWholeCreditNoteOnAPartlyDiscountedInvoiceTakesAwayExactlyWhatItCounted`, `BR_18_APartCreditNoteOnAPartlyDiscountedInvoiceLeavesTheRestCounted` |
| BR-18: a linked one cannot go above the value, alone or together | `ExpensesEndpoints.Record`, `Creditable` | `BR_18_ALinkedCreditNoteCannotGoAboveTheInvoicesValueAfterItsDiscount`, `BR_18_TheCreditNotesOfOneInvoiceTogetherCannotGoAboveItsValue`, `ARejectedCreditNoteNoLongerTakesFromItsInvoiceAndADraftOneDoes` |
| It follows its invoice's rate | `InvoiceFigures.RateOf` | `ALinkedCreditNoteFollowsTheRateItsInvoiceWasComputedAtNotTodays`, `ACreditNoteOfAnInvoiceWithNoVatHasNoVat`, `TheRateOfAnInvoiceComesBackFromItsKeptFigures` |
| Only the same supplier's sent invoice | `Creditable` | `OnlyAnInvoiceOfTheSameSupplierThatWasSentCanBeReturned` |
| BR-24: its own payment is negative too | `ExpenseRequest.PaidOf` | `BR_24_ACreditNotesOwnPaymentIsMoneyComingBackSoItIsNegativeToo` |
| FR-09, FR-17: its file and its number as an invoice's | `Record` | `FR_09_ACreditNoteIsSentWithItsFile`, `AC_17_1_ACreditNotesNumberEnteredAgainWarnsAsAnInvoicesDoes`, `ACreditNoteHasAnInvoiceNumberAndIsTypedAsAPlainAmount` |
| AC-14.1: a deduction of 136 reduces the item total, with no invoice | `ExpenseRequest.IsDeduction` | `AC_14_1_ADeductionOf136ReducesTheItemTotalAndHasNoInvoice` |
| A deduction says why, and is an exception | `RefusalOfTyped` | `ADeductionSaysWhyAndGoesOnTheExceptionsListHavingNoInvoice`, `ADeductionHasNoDiscountAndNoSeparatePayment` |
| FR-21: the link is in the audit trail | `RedixDbContext.Audit` | `FR_21_TheLinkIsInTheAuditTrail` |

The item total and the VAT statement are not built yet (UC-050 "Item report", UC-056 "Build the VAT statement for a period"). The tests add figures up as the database design defines them, which is what those use cases will do.
