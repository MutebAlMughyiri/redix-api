[Documentation](../README.md) › UC-020

# UC-020: Manage contracts

**What it is for:** recording what each contractor was promised, to follow what was paid and what remains (BR-04).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance, changes them. Everyone with access reads them.
**Issue:** redix-api #38; the app side is redix-app #39.

## How it fits together

A contractor is a supplier of the project (the database design's decision 2), so a contractor payment's tax invoice has the contractor's VAT number. A contract is for one item that takes contractor payments, and one unit, which may be the shared one.

The remaining lives on the contract: `Contract.RemainingAfter(paid)` and `IsOverrunBy(paid)`. What was paid comes from the ledger through `ContractMoney`, since UC-027 "Record a contractor payment".

An amendment is an edit of the terms: the value, open value, unit price, quantity and notes. The contractor and item change too while no payment names the contract, whatever became of that payment; once one does they stay, because a payment's VAT follows the contractor and its row holds the item and the unit.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/contracts` | `ReadProject` | `200` in the order added, disabled ones included, as `{ id, supplierName, itemName, unitName, value, isOpenValue, unitPrice, quantity, notes, paid, remaining, isOverrun, isActive }` |
| `POST /projects/{projectId}/contracts` with `{ supplierId, itemId, unitId, value, isOpenValue, unitPrice, quantity, notes }` | `ManageSettings` | `200`; `400` `unknownSupplier`, `itemNotOffered`, `unknownUnit`, `invalidValue`, `invalidUnitPrice`, `invalidQuantity` or `invalidNotes` |
| `PUT /projects/{projectId}/contracts/{id}` with `{ value, isOpenValue, unitPrice, quantity, notes }`, and `{ supplierId, itemId, unitId }` for any that is to change | `ManageSettings` | `200`; `400` for the terms and the parties, as above; `404`; `409` `contractHasPayments`, changing nothing |
| `POST /projects/{projectId}/contracts/{id}/disable`, `.../enable` | `ManageSettings` | `200`; `404` |

`unitPrice`, `quantity` and `notes` may be left out. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Contracts` | Each project's contracts, with their contractor, item and terms |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/ContractsTests.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Finance adds a contract with its contractor, item and value (FR-07) | `contracts.Routes.add` | `FinanceAddsAContractWithItsContractorItemAndValue` |
| A new contract has its whole value remaining | `ContractMoney` | `ANewContractHasItsWholeValueRemaining` |
| The remaining is the value less what was paid inside (AC-07.1, BR-04) | `contracts.Remaining` | `TheRemainingIsTheValueLessWhatWasPaidInsideTheContract` |
| A payment beyond the value leaves a negative remaining, with an alert (AC-07.1) | `contracts.Overrun` | `APaymentBeyondTheValueLeavesANegativeRemainingWithAnAlert` |
| An open-value contract adds up, with no remaining and no alert (AC-07.2) | `contracts.Remaining`, `IsOverrunBy` | `AnOpenValueContractAddsUpWithNoRemainingAndNoAlert` |
| An open-value contract keeps no value | `contracts.Terms.into` | `AnOpenValueContractKeepsNoValue` |
| A value is above zero, with at most two decimals (NFR-04) | `contracts.Terms.refusal`, `Money` | `AValueIsAboveZeroWithAtMostTwoDecimals` |
| A unit price and a quantity are not below zero, with at most two decimals | `contracts.Terms.refusal`, `Money` | `AUnitPriceAndAQuantityAreNotBelowZeroWithAtMostTwoDecimals` |
| Notes have at most 1000 characters | `contracts.Terms.refusal` | `NotesHaveAtMost1000Characters` |
| The contractor is an active supplier of the project (BR-15, BR-33) | `contracts.refusalOfParties` | `TheContractorIsAnActiveSupplierOfTheProject` |
| The item takes contractor payments (the issue's precondition) | `structure.Offers` | `TheItemTakesContractorPayments`, `ADisabledItemTakesNoNewContract` |
| An amendment changes the terms and keeps the contractor | `contracts.Routes.amend` | `AnAmendmentChangesTheTermsAndKeepsTheContractor`, `AnAmendmentCanMakeAContractOpenValue`, `AnAmendmentAnswersAsAddingDoes` |
| A contract answers its parties, and whether any payment names it | `ContractView`, `contracts.Routes.list` | `AContractAnswersItsPartiesAndThatItHasNoPayments` |
| A contract's contractor and item change while it has no payments, in the History table, old and new; new ones answer as adding does | `contracts.Routes.amend`, `amendContractRequest` | `AContractsPartiesChangeWhileItHasNoPaymentsAndTheHistoryShowsThemOldAndNew`, `NewPartiesAnswerAsAddingDoes` |
| Any payment, even a draft or a rejected one, holds them, and a refusal changes nothing; the terms can still change | `contracts.Routes.amend` | `AContractWithAnyPaymentKeepsItsPartiesAndChangesNothing`, `AContractWithAPaymentStillChangesItsTerms` |
| Every change of a contract is in the History table | `contracts.keep` | `EveryChangeOfAContractIsInTheHistory` |
| Disabled, it stays on the list, never deleted | No delete route | `ADisabledContractStaysOnTheListAndCanBeEnabled` |
| Contracts show in the order added | `contracts.Routes.list` | `ContractsShowInTheOrderTheyWereAdded` |
| Everyone with access reads them; only Finance changes them (BR-31) | `access.ReadProject`, `ManageSettings` | `EveryoneWithAccessReadsContractsAndOnlyFinanceChangesThem` |
| One project's contracts are never another's (BR-33) | `access.Gate.Needs`, the `ContractOf` query, by id and project | `OneProjectsContractsAreNeverAnothers` |
| A copied project starts with no contracts (AC-05.2) | `structure.Copy` copies only structure | `ACopiedProjectStartsWithNoContracts` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| The contract's document, through `ContractAttachments` | UC-021 "Store an invoice or receipt attachment" |
| The contracts report | UC-051 "Contracts report" |

A contract is for an item of the project, not a unit, since 7 November 2026 (redix-api #96): the unit, the shared unit and their tests went with it.
