[Documentation](../README.md) › [Use cases](../README.md) › UC-040 to UC-044

# UC-040 to UC-044: Money that moves without an expense

**Purpose:** check the accounts against the books, record what partners put in and take out, and reckon each partner's share.

**Who uses it:** whoever holds `MoveMoney`, which Finance does, enters these from Add entry, and they wait for approval like an expense (BR-30, reworded 8 Nov 2026, redix-api #101). Everyone who reads the project sees the accounts, the difference, the total capital and their own line (BR-31).

| Route | What it does |
| --- | --- |
| `GET /projects/{id}/reconciliations/latest` | The surplus, the latest balances, the difference |
| `POST /projects/{id}/reconciliations` | Keeps what each account really holds |
| `GET /projects/{id}/capital` | The total, and each line the caller may see |
| `GET /projects/{id}/partners/{partnerId}/movements` | One partner's movements |
| `POST /projects/{id}/entries` | Keeps a contribution, or with `isWithdrawal` a withdrawal, as a draft or sent for approval, the partner being its table (redix-api #110) |

`ProjectFigures` is the one owner of capital, revenue and surplus. `EntryRecording` is the one way every entry is recorded, capital and revenue as an expense, and a reversal, approved at once (redix-api #110). `EntryReversal` is the one way to reverse.

**One way in.** Since 8 November 2026 (redix-api #101) a partner's money is entered from Add entry, where `GET /entry-tables` offers whoever holds `MoveMoney` the revenue items with their tables and, right after the last category holding one, each active partner in a group with no id and no name, which the app heads الإيرادات · رأس المال: capital is revenue (BR-06). A movement is a contribution or a withdrawal only: settlement and exit, carry to another project, in kind and refundable advance went with حركة, with the carry route and the carry's link. RedixReal held none of them.

**No partner's pocket.** An expense is paid only from an account added in Accounts (redix-api #96, D24), and a partner paying with their own money is a capital movement (D27), so UC-045 "Expense paid from a partner's money" went on 7 November 2026. The pocket account each partner used to get went with it (redix-api #99): adding a partner makes no account, and the reconciliation has no "owed to partners" line.

**No transfers between accounts.** Removed on 8 November 2026 (Muteb: "remove this feature completely"): FR-23 and AC-23.1 are retired, and no account takes money from another. RedixReal held none.

| Rule | Code | Test |
| --- | --- | --- |
| AC-24.1: the difference is the actual balances less the surplus | `ReconciliationsEndpoints`, `ProjectFigures` | `MoneyMovementTests.AC_24_1_TheDifferenceIsTheActualBalancesLessTheSurplus` |
| A reconciliation needs every active account, and none of another project | `ReconciliationsEndpoints.Enter` | `MoneyMovementTests.AReconciliationNeedsEveryActiveAccountAndNoOtherProjects` |
| A movement takes its receipt when there is one and needs none, like every entry; "paid in cash" and its reason are gone, and a reason already typed joined the entry's notes (Muteb, 8 Nov 2026; AC-25.1 retired) | `EntryRecording`, migration `CapitalReceiptIsOptional` | `MoneyMovementTests.Simple_Entry_AMovementTakesItsReceiptWhenThereIsOneAndNeedsNone` |
| AC-25.2, AC-27.1: balances are movements, and the shares add up to 100 | `CapitalEndpoints.Capital` | `MoneyMovementTests.AC_25_2_AC_27_1_EachBalanceIsTheirMovementsAndTheSharesAddUpTo100` |
| BR-31: a partner sees the total and their own line, nobody else's | `CapitalEndpoints` | `MoneyMovementTests.BR_31_APartnerSeesTheTotalAndTheirOwnLineAndNoOneElses` |
| BR-31, redix-api #109: رأس المال's item report is a table per partner, a withdrawal taking away: every one for whoever holds `seeAllCapital`, their own for a partner, none for anyone else; everyone sees the whole capital, as the summary's رأس المال line | `ItemReportEndpoints.Report`, `ItemTotals` | `MoneyMovementTests.BR_31_UC_109_RasAlMalsReportIsATablePerPartnerEveryOneForWhoeverSeesAllCapitalTheirOwnForAPartnerNoneForAnyoneElseAndTheWholeCapitalForAll` |
| BR-15, redix-api #109: a partner of a disabled رأس المال takes no movement (`unknownPartner`), as an expense's disabled item takes none, yet is still renamed | `CapitalEndpoints.RefusalOfPartnerAsync`, `PartnersEndpoints.RefusalOfAsync` | `MoneyMovementTests.BR_15_UC_109_ADisabledRasAlMalTakesNoMovementYetItsPartnerIsStillRenamed` |
| A movement's text is its entry's notes, so changing the notes changes what the movements screen and رأس المال's report show; the copy `CapitalMovementDetails.Description` is gone (deep review, 8 Nov) | `CapitalEndpoints.MovementsOfAsync`, `ItemReportEndpoints.OfAsync` | `MoneyMovementTests.UC_109_AMovementsTextIsItsEntrysNotesSoChangingTheNotesChangesWhatTheCapitalAndItsReportShow` |
| A partner's entry or a revenue entry is saved as a draft or sent for approval, kept on the phone without a connection, and edited or fixed after a rejection, like every entry (redix-api #110) | `EntryRecording`, `PartnerTable`, `RevenueTable` | `OneEntryTests`: A_110_R8_R13 drafts, A_110_R15 a resend recorded once, A_110_R14 a rejected entry fixed, A_110_G3 a draft moved to another kind |
| A revenue item or a partner is entered only by whoever holds `moveMoney`, and an expense table only by whoever holds `recordEntries` (BR-30); anything else answers `unknownTable` | `EntryTable.OfferedAsync` | `OneEntryTests.A_110_R11_ThePickerAndTheRouteOfferTheSameTables` |
| A movement waits for approval and counts only once approved; a rejected one never counts (BR-30, redix-api #101) | `EntryRecording.AddAsync`, `PartnerTable` | `MoneyMovementTests.R10_AMovementWaitsForApprovalAndCountsOnlyOnceApproved`, `R10_ARejectedMovementNeverCounts` |
| What waits names a movement's partner and a revenue entry's item, and a movement sent tells whoever approves, not who sent it (redix-api #101) | `EntryFacts.PartnerNameOf`, `EntryRecording.AddAsync` | `MoneyMovementTests.R16_WhatWaitsNamesAMovementsPartnerAndARevenueEntrysItem`, `R16_AMovementSentTellsWhoeverApprovesAndNotWhoSentIt` |
| Only a contribution or a withdrawal is taken, and the carry route is gone (redix-api #101) | `CapitalMovementType`, `CapitalEndpoints.MapCapital` | `MoneyMovementTests.R4_R6_OnlyAContributionOrAWithdrawalIsTakenAndTheCarryIsGone` |
| Whoever moves money is offered the revenue items and رأس المال, whose tables are its partners in their order; anyone else neither (redix-api #101, #109). A partner has no active flag since findings row 148 | `CategoriesEndpoints.EntryTables`, `TablesOf` | `CategoriesTests.R3_R5_R13_UC_109_WhoeverMovesMoneyIsOfferedTheRevenueItemsAndRasAlMalWhoseTablesAreItsPartnersInTheirOrder`, `R9_SomeoneWhoMovesNoMoneyIsOfferedNoRevenueItemAndNoPartner` |
| redix-api #99: a partner added makes no account, so the accounts are only those added in Accounts | `PartnersEndpoints.Add` | `MoneyMovementTests.R99_APartnerAddedMakesNoAccountSoTheAccountsAreOnlyThoseAddedInAccounts` |
