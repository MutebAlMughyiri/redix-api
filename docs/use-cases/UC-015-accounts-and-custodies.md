[Documentation](../README.md) › UC-015

# UC-015: Manage accounts and cash custodies

**What it is for:** defining where a project's money sits: its bank accounts and cash custodies, each holding what its approved entries and transfers moved and, for a custody, who holds it (FR-22).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Everyone with access reads them (BR-31, AC-22.1).
**Issue:** redix-api #33; the app side is redix-app #34.

## How it fits together

An account belongs to one project (BR-33). Its type is set when it is added and never changes. A cash custody must have a custodian, who must be in the project; a bank account may have one, as when a field supervisor carries the card.

`account_custodians` says who holds what, one row per account at most, and no row means nobody. It answers BRS open question 4: Field spends only from accounts they hold. `accounts.ListOf`, asked for the offered ones, is that rule, and `GET .../accounts/offered` serves it. Finance holds `ChargeAnyAccount`, so it is offered every active account.

`balance` is what the account's approved entries and transfers moved, by their sign (`AccountMoney`). An account has no opening balance since redix-app #89 (10 Nov 2026), the owner's formula: revenue less expenses equals the total of actual balances.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/accounts` | `ReadProject` | `200` every account, banks first and then in the order added, disabled ones too |
| `GET /projects/{projectId}/accounts/offered` | `ReadProject` | `200` what the caller may spend from |
| `POST /projects/{projectId}/accounts` with `{ type, name, custodianId }` | `ManageSettings` | `200`; `400` `invalidName`, `custodianRequired` or `unknownCustodian` |
| `PUT /projects/{projectId}/accounts/{id}` with `{ name, custodianId }` | `ManageSettings` | The same; `404` |
| `POST /projects/{projectId}/accounts/{id}/disable` | `ManageSettings` | `200`; `404` |
| `POST /projects/{projectId}/accounts/{id}/enable` | `ManageSettings` | `200`; `404` |
| `DELETE /projects/{projectId}/accounts/{id}` | `ManageSettings` | `200` deletes an account nothing names, its history staying; `404`; `409` `accountInUse` when an entry, an actual-balances check or a transfer names it (BR-15) |
| `GET /projects/{projectId}/accounts/history?account=&mine=&before=&text=` | `ReadProject` | `200` a page of the accounts' history (redix-app #89): accounts added, changed, disabled, enabled or deleted, a custodian changed, transfers, entries once approved (one removed by redix-api #119 leaves it), actual-balances checks; newest first, 50 a page, the next after `before`; `account` keeps one account's lines, `mine` the caller's own, `text` the lines whose accounts, people, the name it had or the amount hold it, Arabic spelling read alike (redix-app #104); a partner's money only for whoever sees it (BR-31); `400` `unknownAccount` for one not in the project |
| `GET /projects/{projectId}/members` | `ReadProject` | `200` the project's people active there (redix-api #126) by name, once each, as `{ id, displayName }`, for choosing a custodian |

`custodianId` is left out when nobody holds the account. Every change also answers `409` `projectFinished` in a finished project. An account answers as `{ id, type, name, balance, custodianId, custodianName, isMine, isActive, isUsed }`, where `custodianId` and `custodianName` are empty when nobody holds it, and `isMine` says the caller does.

## Tables

| Table | Holds |
| --- | --- |
| `Accounts` | Each project's accounts |
| `account_custodians` | Who holds which account |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A bank account is added, holding nothing until an entry or a transfer moves money into it | `accounts.Routes.add` | `AccountsTests.FinanceAddsABankAccountWithItsOpeningBalance` |
| A cash custody is held by someone in the project | `accounts.Routes.add`, `accounts.handTo` | `AccountsTests.FinanceAddsACashCustodyHeldBySomeoneInTheProject` |
| A bank account may have a custodian | `accounts.Routes.refused` | `AccountsTests.ABankAccountMayHaveACustodian` |
| An overdrawn account may open below zero. **Retired with the opening balance (redix-app #89, 10 Nov 2026):** money enters an account only through entries and transfers | - | - |
| A cash custody needs a custodian, on adding and on editing | `accounts.Routes.refused` | `AccountsTests.ACashCustodyNeedsACustodian`, `AnEditCannotLeaveACashCustodyWithNobody` |
| A custodian must be in the project (BR-33) | `accounts.Routes.refused`, `users.Routes.member` | `AccountsTests.ACustodianMustBeInTheProject` |
| An account needs a name of 1 to 100 characters | `accounts.Routes.refused` | `AccountsTests.AnAccountNeedsANameOfAtMost100Characters` |
| An opening balance has two decimals (NFR-04) and a limit. **Retired with the opening balance (redix-app #89, 10 Nov 2026):** money enters an account only through entries and transfers | - | - |
| An edit changes the name and the custodian; the type never changes | `accounts.Routes.edit`, `accounts.handTo` | `AccountsTests.AnEditChangesTheNameTheOpeningBalanceAndTheCustodian`, `AnEditCanLeaveABankAccountWithNobody` |
| A changed opening balance is in the History table, old and new. **Retired with the opening balance (redix-app #89, 10 Nov 2026):** money enters an account only through entries and transfers | - | - |
| An account an entry names still changes its name | `accounts.Routes.edit` | `AccountsTests.AnAccountWithAnEntryStillChangesItsName` |
| A disabled account leaves entry and stays on the list (BR-15) | `accounts.ListOf` | `AccountsTests.ADisabledAccountLeavesEntryAndStaysOnTheList`, `EnablingBringsAnAccountBackIntoEntry` |
| Banks come first, then in the order added | `accounts.ListOf` | `AccountsTests.BanksComeFirstThenInTheOrderTheyWereAdded` |
| A custodian sees their custody and every other account, and changes none (AC-22.1, BR-31) | `accounts.ListOf`, `access.ManageSettings` | `AccountsTests.ACustodianSeesTheirCustodyAndEveryOtherAccountWithoutChangingThem` |
| Field is offered only what they hold, and only while it is active (open question 4) | `accounts.ListOf` | `AccountsTests.FieldIsOfferedOnlyTheAccountsTheyHold`, `FieldHoldingNothingIsOfferedNothing`, `FieldIsNotOfferedADisabledAccountTheyHold` |
| Finance is offered every active account | `access.ChargeAnyAccount` | `AccountsTests.FinanceIsOfferedEveryActiveAccount`, `FieldIsOfferedOnlyTheAccountsTheyHold` |
| One project's accounts are never another's (BR-33) | `access.Gate.Needs`, `accounts.Routes.account` | `AccountsTests.OneProjectsAccountsAreNeverAnothers` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `AccountsTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| Everyone with access reads; only Finance changes | `access.ManageSettings` | `AccountsTests.EveryoneWithAccessReadsAccountsAndOnlyFinanceChangesThem` |
| The people to choose among are the active ones, once each, with nothing but a name | `users.Routes.members` | `AccountsTests.TheMembersListNamesEachActivePersonOnceWithNothingElse`, `EveryoneWithAccessReadsTheMembersAndNobodyElse` |

## On the web: Accounts (redix-api #168)

The web's Accounts is the app's screen in the shell. Its card is the last count of the actual balances from `accounts.LatestOf`, the function `GET /projects/{id}/reconciliations/latest` calls too: the surplus the books make, the actual total and their difference, explained while there is one, or that nothing was counted yet. Under it the accounts from `accounts.ListOf`, banks then cash custodies, each with its type, custodian, the last count and its balance. A row opens the account in the details panel, where whoever manages the project's settings disables or enables it (`accounts.Activate`) and deletes it, asked first (`accounts.Delete`); Delete stands gray with its reason while the account is in use. Its pages are in `go-backend/web` (`accounts.go`, `accounts.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/accounts` | Accounts, with the last count's card | `200` the tab's content |
| `GET /details/account/{id}` | An account in the details panel; `?asking=delete` asks before Delete | `200` the panel; `404` for an account not in the list |
| `POST /account/{id}/disable`, `/enable`, `/delete` | An account's action, through the rule the app's route runs, behind its gate | `200` the panel saying what it did or why not, the row beside it; a deleted account leaves the list; `403` without ManageSettings; `404` for an action there is none of |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Accounts lists the accounts by type, with the last count's card (UC-015) | `accounts.ListOf`, `accounts.LatestOf` | `TestWeb_R168_UC015_AccountsListsTheAccountsByTypeWithTheLastCount` |
| Disable and Enable as the app's; an account in use is only disabled, its Delete gray with the reason and refused; an unused one is deleted, asked first (BR-15) | `accounts.Activate`, `accounts.Delete` | `TestWeb_R168_BR15_DisableEnableAndDeleteAsTheAppsRoutes` |
| An account's actions need ManageSettings, a finished project changes nothing (AC-04.1), a failing database answers 500 | `web.site.accountAction` | `TestWeb_R168_AnAccountsActionsAreGatedAsTheAppsRoutesAre` |

## Left to later use cases

| What | Use case |
| --- | --- |
| An entry refusing an account its creator may not spend from, through `entries.refusalOfPayment` | The purchase and contractor payment entry use cases |
| A balance that moves with approved entries, in `Account.Balance` | The same |
| Reconciliation, which the canvas draws on the same screen; custody top-ups were removed on 8 Nov 2026, and transfers came back on 10 Nov (redix-app #89 R3) | UC-040 |
| A partner's pocket, a third type of account | Retired by redix-api #99: a partner paying with their own money is a capital movement (redix-api #96, D27), so no account holds it |
