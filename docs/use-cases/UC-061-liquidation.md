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

**Not built:** revenue and late expenses after closing (findings row 124); a payout's bank receipt (row 125).

| Rule | Code | Test |
| --- | --- | --- |
| BR-27: each partner's share of the surplus, the parts making it exactly | `LiquidationEndpoints.ViewOfAsync` | `LiquidationTests.BR_27_EachPartnerTakesTheirShareOfTheSurplusAndThePartsMakeItExactly` |
| FR-51: paid out, the surplus is the liquidation, the capital stays, the project closes | `LiquidationEndpoints.Liquidate`, `ProjectFigures` | `LiquidationTests.FR_51_PaidOutTheSurplusIsTheLiquidationTheCapitalStaysAndTheProjectCloses` |
| FR-51: a part carried reaches the partner in the later project | `CapitalEndpoints.AddCarry` | `LiquidationTests.FR_51_APartCarriedReachesThePartnerInTheLaterProject` |
| BR-27: a deficit is paid in by share and leaves nothing | `LiquidationEndpoints.Liquidate` | `LiquidationTests.BR_27_ADeficitIsPaidInByShareAndLeavesNothing` |
| BR-35: entries waiting, or a line missing or extra, are refused and nothing is entered | `LiquidationEndpoints.Liquidate` | `LiquidationTests.BR_35_EntriesWaitingAndALineMissingOrExtraAreRefusedAndNothingIsEntered` |
| A part of the liquidation is not a plain movement | `CapitalEndpoints.Record` | `LiquidationTests.APartOfTheLiquidationCannotBeEnteredAsAPlainMovement` |
