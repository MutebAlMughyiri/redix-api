[Documentation](../README.md) › UC-017

# UC-017: Manage suppliers

**What it is for:** keeping one clean supplier register in each project, so a purchase can only name a supplier from the list (BR-19) and one VAT number is one supplier (BR-16).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance, changes it. Everyone with access reads it.
**Issue:** redix-api #35; the app side is redix-app #36.

## How it fits together

A supplier belongs to one project (BR-33), so the same company in two projects is two rows, each with the same VAT number. `Suppliers.VatNumber` is empty for a supplier not VAT-registered, whose invoices stay out of the VAT statement (AC-06.2); a filtered unique index holds a number to one supplier in a project, and lets any number of suppliers have none.

Whoever sends a supplier says either its number or `notVatRegistered`, so a number forgotten by mistake is refused rather than saved as "not registered".

`SupplierDefaultItems` holds the item a supplier's expenses usually go to, one row at most. Its row has a foreign key to the item, which an empty value in a column could not have.

The register starts with the general cash supplier (`Supplier.GeneralCash`, مورد نقدي عام), for expenses with no supplier: `ProjectStructure.ApproveAsync` adds it when the admin approves the project, in the admin's name. It is an ordinary row, and only it comes along: a project copied from an earlier one takes none of that project's suppliers (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/suppliers` | `ReadProject` | `200` by name, disabled ones included, as `{ id, name, vatNumber, mobile, defaultItemId, defaultItemName, status, isActive }`. The empty string stands for no number, mobile or default item; `status` is `approved` or `pendingApproval` |
| `POST /projects/{projectId}/suppliers` with `{ name, vatNumber, notVatRegistered, mobile, defaultItemId }` | `ManageSettings` | `201` with the supplier; `400` `invalidName`, `invalidVatNumber`, `invalidMobile` or `unknownItem`; `409` `vatNumberTaken`, naming the holder as `existing` |
| `POST /projects/{projectId}/suppliers/{id}` with the same | `ManageSettings` | `200`; the same refusals, leaving a supplier its own number; `404` |
| `POST /projects/{projectId}/suppliers/{id}/disable`, `.../enable` | `ManageSettings` | `200`; `404` |

Anything but `name` may be left out. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Suppliers` | Each project's register, with who added each supplier |
| `SupplierDefaultItems` | The item each supplier's expenses usually go to |

## Rules and where they live

| Rule | Code | Test in `SuppliersTests` |
| --- | --- | --- |
| Finance adds a supplier with its name, VAT number, mobile and default item (FR-06) | `SuppliersEndpoints.Add` | `FinanceAddsASupplierWithAllItsDetails` |
| A VAT number of 14 digits is refused (AC-06.1) | `Supplier.IsValidVatNumber` | `AVatNumberOf14DigitsIsRefused` |
| A VAT number is 15 digits, starting and ending with 3 (BR-16) | `Supplier.IsValidVatNumber` | `AVatNumberMustBe15DigitsStartingAndEndingWith3` |
| A number another supplier holds is refused, naming that supplier (AC-06.1) | `SuppliersEndpoints.IfAcceptableAsync` | `AVatNumberAnotherSupplierHoldsIsRefusedNamingThatSupplier`, `AnEditToANumberAnotherSupplierHoldsIsRefused` |
| A disabled supplier still holds its number | `SuppliersEndpoints.IfAcceptableAsync` | `ADisabledSupplierStillHoldsItsVatNumber` |
| The database holds a number to one supplier in a project | The index in `RedixDbContext` | `TheDatabaseHoldsAVatNumberToOneSupplierInAProject` |
| One number may be in two projects (BR-33) | The index in `RedixDbContext` | `OneVatNumberMayBeInTwoProjects` |
| A supplier not VAT-registered is saved with no number (AC-06.2) | `SupplierRequest.VatNumberKept` | `ASupplierNotVatRegisteredIsSavedWithNoNumber` |
| Any number of suppliers may have no number | The index in `RedixDbContext` | `AnyNumberOfSuppliersMayHaveNoVatNumber` |
| An edit changes a supplier and leaves it its own number | `SuppliersEndpoints.Edit` | `AnEditChangesASupplierAndLeavesItItsOwnNumber` |
| An edit can take the default item and the number away | `Supplier.DefaultTo`, `SupplierRequest.VatNumberKept` | `AnEditCanTakeTheDefaultItemAndTheVatNumberAway` |
| A name has 1 to 100 characters, a mobile at most 20 | `Supplier.RefusalOf` | `ASupplierNeedsANameOfAtMost100Characters`, `AMobileMayHaveAtMost20Characters` |
| A default item must be in the project (BR-33) | `ProjectStructure.ItemOfAsync` | `ADefaultItemMustBeInTheProject` |
| Disabled, it stays in the register, never deleted (BR-15) | No delete route; `SuppliersEndpoints.Disable` | `ADisabledSupplierStaysInTheRegisterAndCanBeEnabled` |
| The register lists suppliers by name | `SuppliersEndpoints.List` | `TheRegisterListsSuppliersByName` |
| An approved project starts with the general cash supplier | `ProjectStructure.ApproveAsync`, `Supplier.GeneralCash` | `AnApprovedProjectStartsWithTheGeneralCashSupplier` |
| A copied project starts with only that one (AC-05.2) | `ProjectStructure.ApproveAsync` | `ACopiedProjectStartsWithOnlyTheGeneralCashSupplier` |
| Everyone with access reads the register (BR-31) | `Permission.ReadProject` | `EveryoneWithAccessReadsTheRegister` |
| Only Finance changes suppliers | `Permission.ManageSettings` | `OnlyFinanceChangesSuppliers` |
| One project's suppliers are never another's (BR-33) | `Supplier.ProjectId`, `SuppliersEndpoints` | `OneProjectsSuppliersAreNeverAnothers` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges` | `AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Finding a supplier while entering an expense | UC-018 "Search suppliers" |
| A supplier added from the field, pending approval | UC-019 "Quick-add a supplier pending approval" |
| A purchase saved only with a supplier from the register (BR-19) | UC-022 "Record a purchase expense" |
| Invoices of a supplier with no number staying out of the VAT statement (AC-06.2) | UC-056 "Build the VAT statement for a period" |
