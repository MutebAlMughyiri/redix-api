[Documentation](../README.md) › [Use cases](../README.md) › UC-040 to UC-045

# UC-040 to UC-045: Money that moves without an expense

**Purpose:** move money between accounts, check the accounts against the books, record what partners put in and take out, and reckon each partner's share.

**Who uses it:** whoever holds `MoveMoney`, which Finance does, enters these, and they count at once (BR-30). Everyone who reads the project sees the accounts, the difference, the total capital and their own line (BR-31).

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/transfers` | The latest transfers |
| `POST /projects/{id}/transfers` | Moves money between two accounts |
| `GET /projects/{id}/reconciliations/latest` | The surplus, the latest balances, the difference |
| `POST /projects/{id}/reconciliations` | Keeps what each account really holds |
| `GET /projects/{id}/capital` | The total, and each line the caller may see |
| `GET /projects/{id}/partners/{partnerId}/movements` | One partner's movements |
| `POST /projects/{id}/capital-movements` | Enters a movement |
| `POST /projects/{id}/capital-movements/carry` | Carries a balance to another project |
| `POST /projects/{id}/entries/{entryId}/partner-treatment` | Says whether what a partner paid is capital or owed back |

`ProjectFigures` is the one owner of capital, revenue and surplus. `DirectEntries` is the one way an entry counts with no approval. `EntryReversal` is the one way to reverse: what was entered as one reverses as one, a transfer's two sides, a carry's two sides, or an expense and the capital it gave.

**A partner's pocket.** Every partner has an account of type `PartnerPocket`, made with them and named after them. So it is offered wherever an account is, and "paid from a partner" needed no field of its own. It stays out of the reconciliation (BR-08). A negative pocket is what the project owes that partner.

| Rule | Code | Test |
| --- | --- | --- |
| AC-23.1: a transfer moves both balances and leaves the surplus | `TransfersEndpoints.Transfer` | `MoneyMovementTests.AC_23_1_ATransferMovesBothBalancesAndLeavesTheSurplus` |
| A transfer is approved at once and listed once | `TransfersEndpoints` | `MoneyMovementTests.ATransferIsApprovedAtOnceAndListedOnce` |
| What a transfer cannot be is refused | `TransfersEndpoints.Transfer`, `DirectEntries.RefusalOfAsync` | `MoneyMovementTests.WhatATransferCannotBeIsRefused` |
| Only whoever moves money enters one | `MapProjectChanges(MoveMoney)` | `MoneyMovementTests.OnlyWhoeverMovesMoneyEntersOne` |
| Reversing a transfer reverses both sides | `EntryReversal` | `MoneyMovementTests.ReversingATransferReversesBothSides` |
| AC-24.1: the difference is the actual balances less the surplus | `ReconciliationsEndpoints`, `ProjectFigures` | `MoneyMovementTests.AC_24_1_TheDifferenceIsTheActualBalancesLessTheSurplus` |
| A reconciliation needs every active account, and no pocket | `ReconciliationsEndpoints.Enter` | `MoneyMovementTests.AReconciliationNeedsEveryActiveAccountAndNoPocket` |
| AC-25.1: no receipt is refused; cash with a reason is not | `CapitalEndpoints.Record` | `MoneyMovementTests.AC_25_1_AMovementWithNoReceiptIsRefusedAndCashWithAReasonIsNot` |
| Something in kind needs its reason | `CapitalEndpoints.Record` | `MoneyMovementTests.SomethingInKindNeedsItsReason` |
| A settlement takes all they hold, and they leave; reversing brings them back | `CapitalEndpoints.Record`, `EntryReversal` | `MoneyMovementTests.ASettlementTakesAllTheyHoldAndTheyLeave`, `ReversingASettlementBringsThePartnerBack` |
| AC-26.1: a carry lowers one and raises the other, with no receipt | `CapitalEndpoints.Carry` | `MoneyMovementTests.AC_26_1_ACarryLowersOneAndRaisesTheOtherWithNoReceipt` |
| A carry needs money rights in both projects and a running target | `CapitalEndpoints.Carry` | `MoneyMovementTests.ACarryNeedsMoneyRightsInBothProjectsAndARunningTarget` |
| Reversing a carry reverses both projects | `EntryReversal` | `MoneyMovementTests.ReversingACarryReversesBothProjects` |
| AC-25.2, AC-27.1: balances are movements, and the shares add up to 100 | `CapitalEndpoints.Capital` | `MoneyMovementTests.AC_25_2_AC_27_1_EachBalanceIsTheirMovementsAndTheSharesAddUpTo100` |
| BR-31: a partner sees the total and their own line, nobody else's | `CapitalEndpoints` | `MoneyMovementTests.BR_31_APartnerSeesTheTotalAndTheirOwnLineAndNoOneElses` |
| AC-25.3: approving what a partner paid adds it to their capital | `PartnerExpenses.WhenApprovedAsync` | `MoneyMovementTests.AC_25_3_ApprovingAnExpenseAPartnerPaidAddsTheSameAmountAndDateToTheirCapital` |
| A rejected expense gives the partner nothing | `PartnerExpenses` | `MoneyMovementTests.ARejectedExpenseGivesThePartnerNothing` |
| AC-25.3: Finance may say it is owed back | `ApprovalsEndpoints.SetPartnerTreatment` | `MoneyMovementTests.AC_25_3_FinanceMaySayItIsOwedBack`, `OnlyAnExpenseAPartnerPaidTakesATreatment` |
| Approving again does not add the capital twice | `PartnerExpenses` | `MoneyMovementTests.ApprovingAgainAfterAnAttachedInvoiceDoesNotAddTheCapitalTwice` |
| Reversing the expense takes back the capital it gave | `EntryReversal` | `MoneyMovementTests.ReversingTheExpenseTakesBackTheCapitalItGave` |
| A pocket is offered to Field, hides its balance, and follows the partner | `Account.IsOfferedTo`, `AccountsEndpoints`, `PartnersEndpoints` | `MoneyMovementTests.APocketIsOfferedToFieldHidesItsBalanceAndFollowsThePartner` |
