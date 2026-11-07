[Documentation](../README.md) › UC-025

# UC-025: Record an expense without an invoice (retired)

**Retired:** 7 November 2026, by redix-api #96 "Simple expense" (D3, D17).

Every expense's file is optional now, so no expense needs a reason for having none: `withoutInvoiceReason`, its column `ExpenseDetails.WithoutInvoiceReason`, the exceptions list and `WithoutInvoiceTests` are gone. The reasons already kept moved into each entry's notes. A deduction says why in its notes (D14).

Read [UC-022](UC-022-purchase-expense.md) instead.
