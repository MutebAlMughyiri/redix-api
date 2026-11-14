[Documentation](../README.md) › UC-017

# UC-017: Manage suppliers

**What it is for:** keeping one clean supplier register in each project, so a purchase can only name a supplier from the list (BR-19) and one VAT number is one supplier (BR-16).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance, changes it. Everyone with access reads it.
**Issue:** redix-api #35; the app side is redix-app #36.

## How it fits together

A supplier belongs to one project (BR-33), so the same company in two projects is two rows, each with the same VAT number. `Suppliers.VatNumber` is optional, and empty means nothing more than that none was given (Muteb, 7 Nov 2026: "if no vat number added when adding new supplier that is means nothing"); a filtered unique index holds a number to one supplier in a project, and lets any number of suppliers have none.

A VAT number is optional and, when given, any number of at most 20 digits (BR-16; Muteb, 7 Nov 2026: "just a number has max 20 digit numbers and not required and can keep it empty with remove toogle not vat registered"). Two suppliers of a project still cannot share one.

`SupplierDefaultItems` holds the item a supplier's expenses usually go to, one row at most. Its row has a foreign key to the item, which an empty value in a column could not have.

The register starts with the general cash supplier (`Supplier.GeneralCashSupplier`, مورد نقدي عام), for expenses with no supplier: `ProjectStructure.ApproveAsync` adds it when the admin approves the project, in the admin's name. It is an ordinary row, and only it comes along: a project copied from an earlier one takes none of that project's suppliers (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/suppliers` | `ReadProject` | `200` by name, disabled ones included, as `{ id, name, vatNumber, mobile, defaultItemId, defaultItemName, status, isActive }`. The empty string stands for no number, mobile or default item; `status` is `approved` or `pendingApproval` |
| `POST /projects/{projectId}/suppliers` with `{ name, vatNumber, mobile, defaultItemId }` | `AddSupplier` (UC-019) | `201` with the supplier; `400` `invalidName`, `invalidVatNumber`, `invalidMobile` or `unknownItem`; `409` `vatNumberTaken`, naming the holder as `existing` |
| `PUT /projects/{projectId}/suppliers/{id}` with the same | `ManageSettings` | `200`; the same refusals, leaving a supplier its own number; `404` |
| `POST /projects/{projectId}/suppliers/{id}/disable`, `.../enable` | `ManageSettings` | `200`; `404`; disabling the general cash supplier `409` `generalCashSupplier` |

Anything but `name` may be left out. Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `Suppliers` | Each project's register, with who added each supplier |
| `SupplierDefaultItems` | The item each supplier's expenses usually go to |

## Rules and where they live

| Rule | Code | Test in `SuppliersTests` |
| --- | --- | --- |
| Finance adds a supplier with its name, VAT number, mobile and default item (FR-06) | `SuppliersEndpoints.AddAsync` | `FinanceAddsASupplierWithAllItsDetails` |
| BR-16: a VAT number is empty or at most 20 digits; more, or anything but digits, is refused (Muteb, 7 Nov 2026) | `Supplier.IsValidVatNumber` | `SuppliersTests.BR_16_AVatNumberIsNoneOrAnyNumberOfAtMost20Digits`, `BR_16_AVatNumberOfMoreThan20DigitsOrAnythingButDigitsIsRefused` |
| A number another supplier holds is refused, naming that supplier (AC-06.1) | `SuppliersEndpoints.RefusalOfAsync` | `AVatNumberAnotherSupplierHoldsIsRefusedNamingThatSupplier`, `AnEditToANumberAnotherSupplierHoldsIsRefused` |
| A disabled supplier still holds its number | `SuppliersEndpoints.RefusalOfAsync` | `ADisabledSupplierStillHoldsItsVatNumber` |
| The database holds a number to one supplier in a project | The index in `RedixDbContext` | `TheDatabaseHoldsAVatNumberToOneSupplierInAProject` |
| One number may be in two projects (BR-33) | The index in `RedixDbContext` | `OneVatNumberMayBeInTwoProjects` |
| BR-16: a VAT number is none, or any number of at most 20 digits; anything else is refused | `Supplier.IsValidVatNumber` | `BR_16_AVatNumberIsNoneOrAnyNumberOfAtMost20Digits`, `BR_16_AVatNumberOfMoreThan20DigitsOrAnythingButDigitsIsRefused`, `BR_16_ASupplierWithNoVatNumberIsSavedWithNone` |
| Any number of suppliers may have no number | The index in `RedixDbContext` | `AnyNumberOfSuppliersMayHaveNoVatNumber` |
| An edit changes a supplier and leaves it its own number | `SuppliersEndpoints.Edit` | `AnEditChangesASupplierAndLeavesItItsOwnNumber` |
| An edit can take the default item and the number away | `Supplier.DefaultTo`, `SupplierRequest.VatNumberKept` | `AnEditCanTakeTheDefaultItemAndTheVatNumberAway` |
| A name has 1 to 100 characters, a mobile at most 20, in ASCII digits with a leading `+` at most | `Supplier.RefusalOf`, `User.IsAcceptableMobile` | `ASupplierNeedsANameOfAtMost100Characters`, `AMobileMayHaveAtMost20Characters`, `ASuppliersMobileIsDigitsOnAddAndOnEdit` |
| A default item must be in the project (BR-33) | `ProjectStructure.ItemOfAsync` | `ADefaultItemMustBeInTheProject` |
| Disabled, it stays in the register, never deleted (BR-15) | No delete route; `SuppliersEndpoints.Disable` | `ADisabledSupplierStaysInTheRegisterAndCanBeEnabled` |
| The register lists suppliers by name | `SuppliersEndpoints.List` | `TheRegisterListsSuppliersByName` |
| An approved project starts with the general cash supplier | `ProjectStructure.ApproveAsync`, `Supplier.GeneralCashSupplier` | `AnApprovedProjectStartsWithTheGeneralCashSupplier` |
| The general cash supplier can be renamed and never disabled, by a flag set at approval, so another supplier with its name can | `Supplier.IsGeneralCash`, `SuppliersEndpoints.SetActiveAsync` | `TheGeneralCashSupplierCannotBeDisabled`, `TheGeneralCashSupplierCanBeRenamedAndStaysTheGeneralOne`, `ASupplierNamedLikeTheGeneralOneCanBeDisabled` |
| The migration marks the row the approval named, in a project approved before it | The `GeneralCashSupplierFlag` migration | `TheMigrationMarksTheRowTheApprovalNamedInAProjectApprovedBeforeIt` |
| A copied project starts with only that one (AC-05.2) | `ProjectStructure.ApproveAsync` | `ACopiedProjectStartsWithOnlyTheGeneralCashSupplier` |
| Everyone with access reads the register (BR-31) | `Permission.ReadProject` | `EveryoneWithAccessReadsTheRegister` |
| Only Finance changes suppliers | `Permission.ManageSettings` | `OnlyFinanceChangesSuppliers` |
| One project's suppliers are never another's (BR-33) | `Supplier.ProjectId`, `SuppliersEndpoints` | `OneProjectsSuppliersAreNeverAnothers` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges` | `AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| A purchase saved only with a supplier from the register (BR-19) | UC-022 "Record a purchase expense" |
| A supplier with no number having no VAT in the VAT statement's workbook (AC-06.2) | Retired 7 Nov 2026: an empty number means nothing, so the workbook works the VAT out of every total |
