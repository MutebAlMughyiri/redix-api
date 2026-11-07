[Documentation](../README.md) › UC-023

# UC-023: Detect a duplicate invoice (retired)

**Retired:** 7 November 2026, by redix-api #96 "Simple expense" (D2, D11).

An expense no longer has a bill number, so there is nothing to compare: the `409 duplicateInvoice` answer, `confirmDuplicate` and `DuplicateInvoiceTests` are gone. One file is still on one expense only, which is BR-22 and is refused as `invoiceAlreadyAttached`.

Read [UC-022](UC-022-purchase-expense.md) instead.
