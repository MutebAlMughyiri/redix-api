[Documentation](../README.md) › UC-020

# UC-020: Manage contracts

**What it is for:** recording what each contractor was promised, to follow what was paid and what remains (BR-04).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance, changes them. Everyone with access reads them.
**Issue:** redix-api #38; the app side is redix-app #39.

## How it fits together

A contractor is a supplier of the project (the database design's decision 2), so a contractor payment's tax invoice has the contractor's VAT number. A contract is for one item that takes contractor payments, and one unit, which may be the shared one.

`Units.IsShared` marks the shared unit (مشترك), a row added when the admin approves the project (open question 3). `ProjectView.units` keeps the project's own units; `entryUnits` is what an entry or a contract may name, the shared one last.

The remaining lives on the contract: `Contract.RemainingAfter(paid)` and `IsOverrunBy(paid)`. What was paid is 0 in one property, `Contract.Paid`, until UC-027 "Record a contractor payment" adds the ledger's approved payments inside the contract. AC-07.1 and AC-07.2 are therefore proved on the class.

An amendment is an edit of the terms: the value, open value, unit price, quantity and notes. The contractor, item and unit never change, because a payment's VAT follows the contractor; a wrong one is disabled and added again. Every change of a contract goes in the audit trail, with what it was before.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/contracts` | `ReadProject` | `200` in the order added, disabled ones included, as `{ id, supplierName, itemName, unitName, value, isOpenValue, unitPrice, quantity, notes, paid, remaining, isOverrun, isActive }` |
| `POST /projects/{projectId}/contracts` with `{ supplierId, itemId, unitId, value, isOpenValue, unitPrice, quantity, notes }` | `ManageSettings` | `200`; `400` `unknownSupplier`, `itemNotOffered`, `unknownUnit`, `invalidValue`, `invalidUnitPrice`, `invalidQuantity` or `invalidNotes` |
| `POST /projects/{projectId}/contracts/{id}` with `{ value, isOpenValue, unitPrice, quantity, notes }` | `ManageSettings` | `200`; `400` for the terms, as above; `404` |
| `POST /projects/{projectId}/contracts/{id}/disable`, `.../enable` | `ManageSettings` | `200`; `404` |

`unitPrice`, `quantity` and `notes` may be left out. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Contracts` | Each project's contracts, with their contractor, item, unit and terms |
| `Units` | Gains `IsShared`, for the shared unit |

## Rules and where they live

| Rule | Code | Test in `ContractsTests` |
| --- | --- | --- |
| Finance adds a contract with its contractor, item, unit and value (FR-07) | `ContractsEndpoints.Add` | `FinanceAddsAContractWithItsContractorItemUnitAndValue` |
| A new contract has its whole value remaining | `Contract.Paid` | `ANewContractHasItsWholeValueRemaining` |
| The remaining is the value less what was paid inside (AC-07.1, BR-04) | `Contract.RemainingAfter` | `TheRemainingIsTheValueLessWhatWasPaidInsideTheContract` |
| A payment beyond the value leaves a negative remaining, with an alert (AC-07.1) | `Contract.IsOverrunBy` | `APaymentBeyondTheValueLeavesANegativeRemainingWithAnAlert` |
| An open-value contract adds up, with no remaining and no alert (AC-07.2) | `Contract.RemainingAfter`, `IsOverrunBy` | `AnOpenValueContractAddsUpWithNoRemainingAndNoAlert` |
| An open-value contract keeps no value | `Contract.SetTerms` | `AnOpenValueContractKeepsNoValue` |
| A value is above zero, with at most two decimals (NFR-04) | `Contract.RefusalOf`, `Money` | `AValueIsAboveZeroWithAtMostTwoDecimals` |
| A unit price and a quantity are not below zero, with at most two decimals | `Contract.RefusalOf`, `Money` | `AUnitPriceAndAQuantityAreNotBelowZeroWithAtMostTwoDecimals` |
| Notes have at most 1000 characters | `Contract.RefusalOf` | `NotesHaveAtMost1000Characters` |
| The contractor is an active supplier of the project (BR-15, BR-33) | `ContractsEndpoints.RefusalOfPartiesAsync` | `TheContractorIsAnActiveSupplierOfTheProject` |
| The item takes contractor payments (the issue's precondition) | `Category.ItemsOfferedFor` | `TheItemTakesContractorPayments`, `ADisabledItemTakesNoNewContract` |
| The unit is in the project (BR-33) | `ContractsEndpoints.RefusalOfPartiesAsync` | `TheUnitIsInTheProject` |
| A contract may be for the shared unit (open question 3) | `Project.AddSharedUnit` | `AContractMayBeForTheSharedUnit` |
| An approved project offers its units, then the shared one | `ProjectView.Of` | `AnApprovedProjectOffersItsUnitsAndThenTheSharedOne` |
| An amendment changes the terms and keeps the contractor | `ContractsEndpoints.Amend` | `AnAmendmentChangesTheTermsAndKeepsTheContractor`, `AnAmendmentCanMakeAContractOpenValue`, `AnAmendmentAnswersAsAddingDoes` |
| Every change of a contract is in the audit trail | `ContractsEndpoints` | `EveryChangeOfAContractIsInTheAuditTrail` |
| Disabled, it stays on the list, never deleted | No delete route | `ADisabledContractStaysOnTheListAndCanBeEnabled` |
| Contracts show in the order added | `ContractsEndpoints.List` | `ContractsShowInTheOrderTheyWereAdded` |
| Everyone with access reads them; only Finance changes them (BR-31) | `Permission.ReadProject`, `ManageSettings` | `EveryoneWithAccessReadsContractsAndOnlyFinanceChangesThem` |
| One project's contracts are never another's (BR-33) | `Contract.ProjectId`, `ContractsEndpoints` | `OneProjectsContractsAreNeverAnothers` |
| A copied project starts with no contracts (AC-05.2) | `ProjectStructure.ApproveAsync` copies only structure | `ACopiedProjectStartsWithNoContracts` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges` | `AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| The contract's document, through `ContractAttachments` | UC-021 "Store an invoice or receipt attachment" |
| What was paid inside the contract, from the ledger's approved payments | UC-027 "Record a contractor payment" |
| The contracts report | UC-051 "Contracts report" |
