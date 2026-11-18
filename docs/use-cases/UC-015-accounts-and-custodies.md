[Documentation](../README.md) › UC-015

# UC-015: Manage accounts and cash custodies

**What it is for:** defining where a project's money sits: its bank accounts and cash custodies, each with an opening balance and, for a custody, who holds it (FR-22).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Everyone with access reads them (BR-31, AC-22.1).
**Issue:** redix-api #33; the app side is redix-app #34.

## How it fits together

An account belongs to one project (BR-33). Its type is set when it is added and never changes. A cash custody must have a custodian, who must be in the project; a bank account may have one, as when a field supervisor carries the card.

`AccountCustodians` says who holds what, one row per account at most, and no row means nobody. It answers BRS open question 4: Field spends only from accounts they hold. `Account.IsOfferedTo` is that rule, and `GET .../accounts/offered` serves it. Finance holds `ChargeAnyAccount`, so it is offered every active account.

`Balance` is the opening balance until the entry use cases bring the ledger; only `Account.Balance` changes then.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/accounts` | `ReadProject` | `200` every account, banks first and then in the order added, disabled ones too |
| `GET /projects/{projectId}/accounts/offered` | `ReadProject` | `200` what the caller may spend from |
| `POST /projects/{projectId}/accounts` with `{ type, name, openingBalance, custodianId }` | `ManageSettings` | `200`; `400` `invalidName`, `invalidOpeningBalance`, `custodianRequired` or `unknownCustodian` |
| `PUT /projects/{projectId}/accounts/{id}` with `{ name, openingBalance, custodianId }` | `ManageSettings` | The same; `404`; `409` `accountHasEntries` for a new opening balance once any entry names the account |
| `POST /projects/{projectId}/accounts/{id}/disable` | `ManageSettings` | `200`; `404` |
| `POST /projects/{projectId}/accounts/{id}/enable` | `ManageSettings` | `200`; `404` |
| `DELETE /projects/{projectId}/accounts/{id}` | `ManageSettings` | `200` deletes an account nothing names, its history staying; `404`; `409` `accountInUse` when an entry, an actual-balances check or a transfer names it (BR-15) |
| `GET /projects/{projectId}/accounts/history?account=&mine=&before=&text=` | `ReadProject` | `200` a page of the accounts' history (redix-app #89): accounts added, changed, disabled, enabled or deleted, a custodian changed, transfers, entries once approved (one removed by redix-api #119 leaves it), actual-balances checks; newest first, 50 a page, the next after `before`; `account` keeps one account's lines, `mine` the caller's own, `text` the lines whose accounts, people, the name it had or the amount hold it, Arabic spelling read alike (redix-app #104); a partner's money only for whoever sees it (BR-31); `400` `unknownAccount` for one not in the project |
| `GET /projects/{projectId}/members` | `ReadProject` | `200` the project's people active there (redix-api #126) by name, once each, as `{ id, displayName }`, for choosing a custodian |

`custodianId` is left out when nobody holds the account. Every change also answers `409` `projectFinished` in a finished project. An account answers as `{ id, type, name, openingBalance, balance, custodianId, custodianName, isMine, isActive }`, where `custodianId` and `custodianName` are empty when nobody holds it, and `isMine` says the caller does.

## Tables

| Table | Holds |
| --- | --- |
| `Accounts` | Each project's accounts |
| `AccountCustodians` | Who holds which account |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A bank account is added with its opening balance | `accounts.Routes.add` | `AccountsTests.FinanceAddsABankAccountWithItsOpeningBalance` |
| A cash custody is held by someone in the project | `accounts.Routes.add`, `accounts.handTo` | `AccountsTests.FinanceAddsACashCustodyHeldBySomeoneInTheProject` |
| A bank account may have a custodian | `accounts.Routes.refused` | `AccountsTests.ABankAccountMayHaveACustodian` |
| An overdrawn account may open below zero | `accounts.Routes.refused` | `AccountsTests.AnOverdrawnAccountMayOpenBelowZero` |
| A cash custody needs a custodian, on adding and on editing | `accounts.Routes.refused` | `AccountsTests.ACashCustodyNeedsACustodian`, `AnEditCannotLeaveACashCustodyWithNobody` |
| A custodian must be in the project (BR-33) | `accounts.Routes.refused`, `users.Routes.member` | `AccountsTests.ACustodianMustBeInTheProject` |
| An account needs a name of 1 to 100 characters | `accounts.Routes.refused` | `AccountsTests.AnAccountNeedsANameOfAtMost100Characters` |
| An opening balance has two decimals (NFR-04) and a limit | `accounts.Routes.refused` | `AccountsTests.AnOpeningBalanceHasTwoDecimalsAndALimit` |
| An edit changes the name, the opening balance and the custodian; the type never changes | `accounts.Routes.edit`, `accounts.handTo` | `AccountsTests.AnEditChangesTheNameTheOpeningBalanceAndTheCustodian`, `AnEditCanLeaveABankAccountWithNobody` |
| A changed opening balance is in the History table, old and new | `accounts.Routes.edit`, `history.Write` | `AccountsTests.AChangedOpeningBalanceIsInTheHistoryOldAndNew` |
| An opening balance cannot change once any entry names the account, drafts included; its name still can | `accounts.Routes.edit` | `AccountsTests.AnOpeningBalanceCannotChangeOnceTheAccountHasAnEntry`, `AnAccountWithAnEntryStillChangesItsName` |
| A disabled account leaves entry and stays on the list (BR-15) | `accounts.Routes.list` | `AccountsTests.ADisabledAccountLeavesEntryAndStaysOnTheList`, `EnablingBringsAnAccountBackIntoEntry` |
| Banks come first, then in the order added | `accounts.Routes.list` | `AccountsTests.BanksComeFirstThenInTheOrderTheyWereAdded` |
| A custodian sees their custody and every other account, and changes none (AC-22.1, BR-31) | `accounts.Routes.list`, `access.ManageSettings` | `AccountsTests.ACustodianSeesTheirCustodyAndEveryOtherAccountWithoutChangingThem` |
| Field is offered only what they hold, and only while it is active (open question 4) | `accounts.Routes.list` | `AccountsTests.FieldIsOfferedOnlyTheAccountsTheyHold`, `FieldHoldingNothingIsOfferedNothing`, `FieldIsNotOfferedADisabledAccountTheyHold` |
| Finance is offered every active account | `access.ChargeAnyAccount` | `AccountsTests.FinanceIsOfferedEveryActiveAccount`, `FieldIsOfferedOnlyTheAccountsTheyHold` |
| One project's accounts are never another's (BR-33) | `access.Gate.Needs`, `accounts.Routes.account` | `AccountsTests.OneProjectsAccountsAreNeverAnothers` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `AccountsTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| Everyone with access reads; only Finance changes | `access.ManageSettings` | `AccountsTests.EveryoneWithAccessReadsAccountsAndOnlyFinanceChangesThem` |
| The people to choose among are the active ones, once each, with nothing but a name | `users.Routes.members` | `AccountsTests.TheMembersListNamesEachActivePersonOnceWithNothingElse`, `EveryoneWithAccessReadsTheMembersAndNobodyElse` |

## Left to later use cases

| What | Use case |
| --- | --- |
| An entry refusing an account its creator may not spend from, through `Account.IsOfferedTo` | The purchase and contractor payment entry use cases |
| A balance that moves with approved entries, in `Account.Balance` | The same |
| Reconciliation, which the canvas draws on the same screen; transfers and custody top-ups were removed on 8 Nov 2026 | UC-040 |
| A partner's pocket, a third type of account | Retired by redix-api #99: a partner paying with their own money is a capital movement (redix-api #96, D27), so no account holds it |
