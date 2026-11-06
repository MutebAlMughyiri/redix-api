[Documentation](../README.md) › [Use cases](../README.md) › UC-061

# UC-061: Liquidate and close a project

**Purpose:** end a project by sharing its final surplus among the partners by their shares, each part paid out or carried to a later project, or a deficit paid in, and then close it (FR-51, BR-27).

**Who uses it:** whoever holds `MoveMoney`, the financial supervisor.

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/liquidation` | The surplus left to share, each partner's share and part, the entries waiting for approval, and what was shared out |
| `POST /projects/{id}/liquidation` | Shares it out on a day, each partner's part as their line says, and closes the project |

A part is a capital movement of type `Liquidation`: out of the account here, or carried to the partner's line in a later project with `CapitalEndpoints.AddCarry`, the same pair a carry makes. For a deficit the movement runs the other way. It is not capital: `ProjectFigures.Capital`, the partners' balances and shares, and their movements leave it out, and `ProjectFigures.Liquidation` adds it up. So after closing, the capital and shares stand as they were, and the surplus is revenue less expenses and liquidation, nothing.

Each part is the share times the surplus to two decimals, and the last partner takes what rounding left, so the parts make the surplus exactly. The periodic report's liquidation is `ProjectFigures.Liquidation`.

**After closing.** A plain close is refused while there is a surplus or deficit to share (`liquidateFirst`): liquidating is the only way to close it. A late expense or revenue (`POST /projects/{id}/liquidation/late`) names one item: an expense item that takes purchases, or a revenue item, whose side says which it is (UC-068). It is approved at once, and each partner pays in their share of an expense, or takes their share of revenue, by their shares at closing, so the surplus stays nothing. Each part shared out takes its bank receipt afterwards, once (`POST /projects/{id}/liquidation/{entryId}/receipt`, BR-23).

| Rule | Code | Test |
| --- | --- | --- |
| BR-27: each partner's share of the surplus, the parts making it exactly | `LiquidationEndpoints.ViewOfAsync` | `LiquidationTests.BR_27_EachPartnerTakesTheirShareOfTheSurplusAndThePartsMakeItExactly` |
| FR-51: paid out, the surplus is the liquidation, the capital stays, the project closes | `LiquidationEndpoints.Liquidate`, `ProjectFigures` | `LiquidationTests.FR_51_PaidOutTheSurplusIsTheLiquidationTheCapitalStaysAndTheProjectCloses` |
| FR-51: a part carried reaches the partner in the later project | `CapitalEndpoints.AddCarry` | `LiquidationTests.FR_51_APartCarriedReachesThePartnerInTheLaterProject` |
| BR-27: a deficit is paid in by share and leaves nothing | `LiquidationEndpoints.Liquidate` | `LiquidationTests.BR_27_ADeficitIsPaidInByShareAndLeavesNothing` |
| BR-35: entries waiting, or a line missing or extra, are refused and nothing is entered | `LiquidationEndpoints.Liquidate` | `LiquidationTests.BR_35_EntriesWaitingAndALineMissingOrExtraAreRefusedAndNothingIsEntered` |
| A part of the liquidation is not a plain movement | `CapitalEndpoints.Record` | `LiquidationTests.APartOfTheLiquidationCannotBeEnteredAsAPlainMovement` |
| BR-27: a late expense is paid in and late revenue taken by the shares at closing; the surplus stays nothing | `LiquidationEndpoints.RecordLate` | `LiquidationTests.BR_27_ALateExpenseIsPaidInAndLateRevenueTakenByTheSharesAtClosingAndTheSurplusStaysNothing` |
| FR-51: a late entry needs a closed project and an item that takes purchases, or a revenue item | `LiquidationEndpoints.RecordLate` | `LiquidationTests.FR_51_ALateEntryNeedsAClosedProjectAndAnItemThatTakesPurchases` |
| BR-23: a payout's receipt is attached after closing, and only once | `LiquidationEndpoints.AttachReceipt` | `LiquidationTests.BR_23_APayoutsReceiptIsAttachedAfterTheProjectClosesAndOnlyOnce` |
| BR-27: a plain close is refused while there is something to share | `ProjectsEndpoints.Close` | `LiquidationTests.BR_27_APlainCloseIsRefusedWhileThereIsASurplusToShare` |
