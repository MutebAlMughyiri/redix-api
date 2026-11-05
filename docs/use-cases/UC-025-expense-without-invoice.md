[Documentation](../README.md) › UC-025

# UC-025: Record an expense without an invoice

**What it is for:** allowing the rare expense that has no invoice, without letting it go unnoticed.
**Who uses it:** whoever holds `RecordEntries` in the project, which is Finance and Field. Anyone who reads the project reads the exceptions list.
**Issue:** redix-api #42; the app side is redix-app #43.

## How it fits together

An expense says it has no invoice by giving a reason, `ExpenseRequest.WithoutInvoiceReason`. There is no second flag that could disagree with it: `ExpenseRequest.IsWithoutInvoice` asks the reason, after trimming it. The reason is kept on `ExpenseDetails.WithoutInvoiceReason`, the column the design has for it.

Such an expense has no row in `Invoices`, so it has no number and no VAT, and the VAT statement can never count it. Its figures still come from `InvoiceFigures.Of`, at a rate of zero. Its date is the payment date, which is then required. It still names its supplier from the register (BR-19).

**Open question 2 is answered no cap**, so nothing here limits the amount beyond `Money.Largest`.

## Endpoints

| Route | Who | Does |
| --- | --- | --- |
| `POST /projects/{id}/expenses` | `RecordEntries` | Now takes `withoutInvoiceReason`. A reason too long answers `invalidWithoutInvoiceReason`, a reason beside a file `invoiceAndReason`, and sending with neither file nor reason `invoiceRequired` |
| `GET /projects/{id}/expenses/exceptions` | `ReadProject` | The exceptions list: the expenses sent or approved with a reason, in the order they were entered |

## Tables

No new table. `ExpenseDetails.WithoutInvoiceReason` was already there.

## Rules

Tests are in `WithoutInvoiceTests`.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-09.1: no file and no reason cannot be sent | `ExpensesEndpoints.Record` | `AC_09_1_ExpenseWithNoFileAndNoReasonCannotBeSent` |
| AC-09.1: with a reason it is pending and in the exceptions list | `ExpensesEndpoints.Exceptions` | `AC_09_1_ExpenseWithAReasonIsSentPendingAndShowsInTheExceptionsList` |
| Open question 2: no cap | Nothing limits it | `OpenQuestion2_ThereIsNoCapOnAnExpenseWithoutAnInvoice` |
| It has no invoice row and no VAT | `ExpensesEndpoints.Build`, `Record` | `AnExpenseWithoutAnInvoiceHasNoInvoiceRowAndNoVatSoTheVatStatementNeverSeesIt` |
| Its date is the payment date | `RefusalOfDates` | `ItsDateIsThePaymentDateWhichIsRequiredAndInThePeriod` |
| A reason and a file contradict each other | `ExpenseRequest.RefusalOfTyped` | `AReasonBesideAFileIsRefusedAndAReasonTooLongIsRefused` |
| Drafts and rejected entries are not exceptions | `Exceptions` | `ADraftIsNotInTheExceptionsListAndARejectedOneLeavesIt`, `AnExpenseWithAFileIsNotInTheExceptionsList` |
| BR-33: only who reads the project | `Permission.ReadProject` | `BR_33_TheExceptionsListIsOnlyForWhoReadsTheProject` |
| FR-21: the reason is in the audit trail | `RedixDbContext.Audit` | `FR_21_TheReasonIsInTheAuditTrail` |
