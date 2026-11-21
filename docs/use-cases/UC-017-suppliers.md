[Documentation](../README.md) › UC-017

# UC-017: Manage suppliers

**What it is for:** keeping one clean supplier register in each project, so a purchase can only name a supplier from the list (BR-19) and one VAT number is one supplier (BR-16).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance, changes it. Everyone with access reads it.
**Issue:** redix-api #35; the app side is redix-app #36.

## How it fits together

A supplier belongs to one project (BR-33), so the same company in two projects is two rows, each with the same VAT number. `Suppliers.VatNumber` is optional, and empty means nothing more than that none was given (the owner, 7 Nov 2026: "if no vat number added when adding new supplier that is means nothing"); a filtered unique index holds a number to one supplier in a project, and lets any number of suppliers have none.

A VAT number is optional and, when given, any number of at most 20 digits (BR-16; the owner, 7 Nov 2026: "just a number has max 20 digit numbers and not required and can keep it empty with remove toogle not vat registered"). Two suppliers of a project still cannot share one.

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

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/SuppliersTests.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Finance adds a supplier with its name, VAT number, mobile and default item (FR-06) | `suppliers.Add` | `FinanceAddsASupplierWithAllItsDetails` |
| BR-16: a VAT number is empty or at most 20 digits; more, or anything but digits, is refused (the owner, 7 Nov 2026) | `suppliers.refusalOf` | `SuppliersTests.BR_16_AVatNumberIsNoneOrAnyNumberOfAtMost20Digits`, `BR_16_AVatNumberOfMoreThan20DigitsOrAnythingButDigitsIsRefused` |
| A number another supplier holds is refused, naming that supplier (AC-06.1) | `suppliers.refusalOf` | `AVatNumberAnotherSupplierHoldsIsRefusedNamingThatSupplier`, `AnEditToANumberAnotherSupplierHoldsIsRefused` |
| A disabled supplier still holds its number | `suppliers.refusalOf` | `ADisabledSupplierStillHoldsItsVatNumber` |
| The database holds a number to one supplier in a project | The unique index `ix_suppliers_project_id_vat_number` | `TheDatabaseHoldsAVatNumberToOneSupplierInAProject` |
| One number may be in two projects (BR-33) | The unique index `ix_suppliers_project_id_vat_number`, per project | `OneVatNumberMayBeInTwoProjects` |
| BR-16: a VAT number is none, or any number of at most 20 digits; anything else is refused | `suppliers.refusalOf` | `BR_16_AVatNumberIsNoneOrAnyNumberOfAtMost20Digits`, `BR_16_AVatNumberOfMoreThan20DigitsOrAnythingButDigitsIsRefused`, `BR_16_ASupplierWithNoVatNumberIsSavedWithNone` |
| Any number of suppliers may have no number | The unique index `ix_suppliers_project_id_vat_number`, which skips an empty number | `AnyNumberOfSuppliersMayHaveNoVatNumber` |
| An edit changes a supplier and leaves it its own number | `suppliers.Routes.edit` | `AnEditChangesASupplierAndLeavesItItsOwnNumber` |
| An edit can take the default item and the number away | `suppliers.defaultTo`, `suppliers.typedInto` | `AnEditCanTakeTheDefaultItemAndTheVatNumberAway` |
| A name has 1 to 100 characters, a mobile at most 20, in ASCII digits with a leading `+` at most | `suppliers.refusalOf`, `users.IsAcceptableMobile` | `ASupplierNeedsANameOfAtMost100Characters`, `AMobileMayHaveAtMost20Characters`, `ASuppliersMobileIsDigitsOnAddAndOnEdit` |
| A default item must be in the project (BR-33) | `structure.change.item` | `ADefaultItemMustBeInTheProject` |
| Disabled, it stays in the register, never deleted (BR-15) | No delete route; `suppliers.Routes.activate` | `ADisabledSupplierStaysInTheRegisterAndCanBeEnabled` |
| The register lists suppliers by name | `suppliers.Routes.list` | `TheRegisterListsSuppliersByName` |
| An approved project starts with the general cash supplier | `structure.Copy`, `suppliers.AddGeneralCash` | `AnApprovedProjectStartsWithTheGeneralCashSupplier` |
| The general cash supplier can be renamed and never disabled, by a flag set at approval, so another supplier with its name can | `suppliers.AddGeneralCash`, `suppliers.Routes.activate` | `TheGeneralCashSupplierCannotBeDisabled`, `TheGeneralCashSupplierCanBeRenamedAndStaysTheGeneralOne`, `ASupplierNamedLikeTheGeneralOneCanBeDisabled` |
| The migration marks the row the approval named, in a project approved before it | .NET's migration `GeneralCashSupplierFlag`, before the data moved to SQLite | `TheMigrationMarksTheRowTheApprovalNamedInAProjectApprovedBeforeIt` |
| A copied project starts with only that one (AC-05.2) | `structure.Copy` | `ACopiedProjectStartsWithOnlyTheGeneralCashSupplier` |
| Everyone with access reads the register (BR-31) | `access.ReadProject` | `EveryoneWithAccessReadsTheRegister` |
| Only Finance changes suppliers | `access.ManageSettings` | `OnlyFinanceChangesSuppliers` |
| One project's suppliers are never another's (BR-33) | `access.Gate.Needs`, the `SupplierOf` query, by id and project | `OneProjectsSuppliersAreNeverAnothers` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `AFinishedProjectRefusesEveryChangeAndStaysReadable` |

## On the web: Suppliers (redix-api #170)

The web's Suppliers is the app's screen in the shell, its item in the side menu between Partners and Contracts: every supplier of the register, disabled ones included, by name, from `suppliers.ListOf`, the function `GET /projects/{id}/suppliers` calls too, everyone who reads the project seeing it. Each row says what the app's row says - its name, quiet once disabled, then that it is disabled or waits for approval, its VAT number and its default item -, and the search finds a name, a VAT number, a mobile or a default item, as the app's does. A row opens the supplier in the details panel with its VAT number, mobile and default item. Whoever manages the project's settings, in a project not finished, adds a supplier from the head - New supplier stands gray with its reason for anyone else - and edits, disables or enables one in the panel, as the app's dialog and row menu do: the name, the VAT number - naming the supplier that already holds it -, the mobile, and the purchase item its expenses usually go to, from `structure.OfferedOf`, the function `GET /projects/{id}/categories/offered` calls too; kept through `suppliers.Add`, `suppliers.Edit` and `suppliers.Activate`, the rules `POST`, `PUT …/{supplierId}` and `…/disable|enable` call too. A default item since disabled stays offered on Edit, so saving keeps it. Its pages are in `go-backend/web` (`suppliers.go`, `suppliers.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/suppliers?text=` | Suppliers, searched | `200` the tab's content |
| `GET /details/supplier/{id}` | A supplier in the details panel; `?asking=edit` Edit's form | `200` the panel; `404` for a supplier the register does not hold |
| `GET /details/supplier/new` | New supplier's form in the details panel | `200` the form; `404` without ManageSettings |
| `POST /supplier`, `POST /supplier/{id}/edit`, `POST /supplier/{id}/disable`, `POST /supplier/{id}/enable` | Keeps the form (`name`, `vatNumber`, `mobile`, `defaultItem`), or disables or enables the supplier | `200` the panel showing the supplier, Suppliers beside it; or the form or the panel saying why not; `403` without ManageSettings; `404` for an action there is none of |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Suppliers lists the register, disabled suppliers included, each row as the app's says it; the search finds a name, VAT number, mobile or default item | `suppliers.ListOf`, `web.supplierDetails`, `web.searchHolds` | `TestWeb_R170_UC017_SuppliersListsTheRegisterAsTheAppsScreen` |
| New supplier, Edit, Disable and Enable as the app's: a name 1 to 100 letters, a VAT number of at most 20 digits another supplier does not hold (BR-16), a mobile; the general cash supplier is never disabled (BR-15) | `suppliers.Add`, `suppliers.Edit`, `suppliers.Activate`, `structure.OfferedOf` | `TestWeb_R170_UC017_NewSupplierEditDisableAndEnableAsTheAppsDialog` |
| A supplier's changes need ManageSettings; a finished project changes nothing (AC-04.1); a failing database answers 500 | `web.site.keepSupplier` | `TestWeb_R170_ASuppliersChangesAreGatedAsTheAppsRoutesAre` |

## Left to later use cases

| What | Use case |
| --- | --- |
| A purchase saved only with a supplier from the register (BR-19) | UC-022 "Record a purchase expense" |
| A supplier with no number having no VAT in the VAT statement's workbook (AC-06.2) | Retired 7 Nov 2026: an empty number means nothing, so the workbook works the VAT out of every total |
