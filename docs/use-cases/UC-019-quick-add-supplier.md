[Documentation](../README.md) › UC-019

# UC-019: Quick-add a supplier pending approval

**What it is for:** adding a missing supplier without leaving the expense being entered, while what the field adds still waits for Finance (FR-44, BR-30).
**Who uses it:** whoever holds `AddSupplier` in the project, which is Finance and Field.
**Issue:** redix-api #37; the app side is redix-app #38.

## How it fits together

There is one route that adds a supplier, UC-017's, because the rules are the same (BR-16, BR-19) and only the status differs: whoever holds `ManageSettings` adds an approved supplier, and anyone else one pending approval.

A pending supplier already holds its VAT number, and is in the register for everyone to see, marked. It is offered to pick (`Supplier.OfferedTo`) only to who added it and to whoever holds `ManageSettings`.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/{projectId}/suppliers` with `{ name, vatNumber }` | `AddSupplier` | `201` with the supplier and its `status`; the refusals of UC-017, `vatNumberTaken` naming the holder as `existing`; `409` `projectFinished` |
| `GET /projects/{projectId}/suppliers/search?text=...` | `ReadProject` | UC-018's, with what is on offer to the caller |

## Tables

No new table: it uses `Suppliers.Status` and `Suppliers.CreatedByUserId`. Field and Finance hold the new permission through `RolePermissions`.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/SuppliersTests.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| What Field adds waits for approval, and shows so (AC-44.1) | `suppliers.Add` | `WhatFieldAddsWaitsForApproval` |
| What Finance adds is approved at once | `suppliers.Add` | `FinanceAddsASupplierWithAllItsDetails` |
| What Field adds is validated as any supplier (BR-16) | `suppliers.refusalOf` | `WhatFieldAddsIsValidatedAsAnySupplier` |
| A number already in the register is refused, naming its supplier (BR-19) | `suppliers.refusalOf` | `AVatNumberAlreadyInTheRegisterIsRefusedToFieldNamingItsSupplier` |
| A pending supplier already holds its number | The same | `APendingSupplierAlreadyHoldsItsVatNumber` |
| A pending supplier is offered to who added it and to Finance only (step 3) | `suppliers.Routes.search` | `APendingSupplierIsOfferedToWhoAddedItAndToFinanceOnly` |
| Everyone with access sees it in the register, marked (BR-31) | `suppliers.Routes.list` | `EveryoneWithAccessSeesAPendingSupplierInTheRegister` |
| Only Finance and Field add suppliers | `access.AddSupplier` | `OnlyFinanceAndFieldAddSuppliers` |
| Field changes nothing else | `access.ManageSettings` | `OnlyFinanceChangesSuppliers` |
| A finished project refuses it (AC-04.1) | `access.Gate.Changes` | `AFinishedProjectRefusesWhatFieldAdds` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Approving the first expense on a supplier approves it; rejecting it leaves it pending (AC-44.1, BR-30) | UC-032 "Approve or reject entries" |
| An entry refusing a supplier not on offer to its creator | UC-022 "Record a purchase expense" |
