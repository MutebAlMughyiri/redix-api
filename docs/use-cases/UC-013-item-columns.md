[Documentation](../README.md) › UC-013

# UC-013: Manage item columns

**What it is for:** changing the fields an item asks for without a code change (FR-41): on one item, or on the project's default template for one type of item.
**Who uses it:** whoever holds `ManageStructure` in the project, which is Finance. Everyone with access reads them.
**Issue:** redix-api #31; the app side is redix-app #32.

## How it fits together

The seeded template project holds the default columns of each item type. When a project is approved it takes its own copy, so each project has its own default template and changing it touches no other (BR-33).

A new item starts from its project's default columns of its type (FR-38), and from then on holds its own rows in `ItemColumns`. A column keeps its `Key` from the template to every item, which is how "apply to all items" finds it. A column a person adds has a key starting `custom.`, under which its values will sit in an entry's `CustomValues`.

An item of the template holds no column rows of its own: its columns are its type's defaults, and its copy takes them. That keeps about 550 rows out of the migrations.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/items/{itemId}/columns` | `ReadProject` | `200` what the item asks for, in order, without deleted ones; `404` |
| `GET /projects/{projectId}/default-columns/{itemType}` | `ReadProject` | `200` the template for `purchases`, `contractorPayments` or `both`; `400` for another word |
| `POST /projects/{projectId}/items/{itemId}/columns` with `{ name, dataType, isRequired, showInReports, options }` | `ManageStructure` | `200`; `400` `invalidName`, `invalidDataType` or `invalidOptions`; `404` |
| `POST /projects/{projectId}/items/{itemId}/columns/reorder` with `{ ids }` | `ManageStructure` | `200`; `400` `invalidOrder`; `404` |
| `POST /projects/{projectId}/items/{itemId}/columns/{columnId}` with `{ name, isRequired, showInReports }` | `ManageStructure` | `200`; `400` `invalidName`; `404`; `409` `coreColumn` |
| `POST /projects/{projectId}/items/{itemId}/columns/{columnId}/delete` | `ManageStructure` | `200`; `404`; `409` `coreColumn` |
| `POST /projects/{projectId}/default-columns/{itemType}` with the same, and `applyToAllItems` | `ManageStructure` | As on an item |
| `POST /projects/{projectId}/default-columns/{itemType}/reorder` with `{ ids, applyToAllItems }` | `ManageStructure` | As on an item |
| `POST /projects/{projectId}/default-columns/{itemType}/{columnId}` with the same, and `applyToAllItems` | `ManageStructure` | As on an item |
| `POST /projects/{projectId}/default-columns/{itemType}/{columnId}/delete` with `{ applyToAllItems }` | `ManageStructure` | As on an item |

Every change also answers `409` `projectFinished` in a finished project. A column answers as `{ id, key, name, dataType, isCore, isCustom, isRequired, showInReports, options }`.

## Tables

| Table | Holds |
| --- | --- |
| `DefaultColumns` | A project's default template: what a new item of each type starts with |
| `ItemColumns` | What each item asks for, with `IsHidden` once deleted |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| A new item inherits the default columns of its type (FR-38) | `Item.New` | `ColumnsTests.ANewItemInheritsTheDefaultColumnsOfItsType` |
| A template item is copied with the columns of its type | `Item.CopyFor` | `ColumnsTests.ATemplateItemIsCopiedWithTheColumnsOfItsType` |
| The core columns are the nine of BR-20, and the rest of BR-20 are not core | `StructureSeed` | `ColumnsTests.TheCoreColumnsOfAPurchaseAreTheNineOfBr20` |
| An item of both types holds the purchase columns, then what only a contractor payment has | `StructureSeed.DefaultColumns` | `ColumnsTests.AnItemOfBothTypesHoldsThePurchaseColumnsThenWhatOnlyAContractorPaymentHas` |
| A column added to one item is asked in that item only (AC-41.1) | `ColumnsEndpoints.AddToItem` | `ColumnsTests.AColumnAddedToOneItemIsAskedInThatItemOnly` |
| A column needs a name of 1 to 100 characters | `Column.CheckNew`, `Column.Edit` | `ColumnsTests.AColumnNeedsANameOfAtMost100Characters` |
| A list a person added gains choices; those it has stay, in their order, because entries hold them; a new choice has the limit of a name; the template's reach items only when applied (BR-21) | `Column.Edit`, `ColumnsEndpoints.EditOfItem`, `EditOfTemplate` | `ColumnsTests.APickListGainsChoicesAndKeepsItsOwnInTheirOrder`, `AnEditThatGivesNoNewChoiceChangesNoChoice`, `ANewChoiceIsWithinTheLimitOfAName`, `OnlyAPickListAPersonAddedGainsChoices`, `AnEntryCanHoldTheChoiceAListGained`, `AChoiceTheTemplateGainsReachesItemsOnlyWhenApplied` |
| A pick list needs a choice; another type keeps none | `Column.CheckNew`, `DefaultColumn.New` | `ColumnsTests.APickListNeedsAChoice`, `APickListKeepsItsChoicesAndAnotherTypeNone` |
| A person chooses among five types | `Column.CheckNew` | `ColumnsTests.APersonCannotAddAFileColumn` |
| A core column is renamed and reordered (BR-20) | `Column.Edit`, `ProjectStructure.PutInOrder` | `ColumnsTests.ACoreColumnIsRenamedAndReordered` |
| A core column changes nothing but its name (BR-20) | `Column.Edit` | `ColumnsTests.ACoreColumnChangesNothingButItsName` |
| Any other column changes what entry asks and what prints | `Column.Edit` | `ColumnsTests.AColumnThatIsNotCoreChangesWhatEntryAsksAndWhatPrints` |
| A core column is never deleted, on an item or in the template (AC-41.2) | `ItemColumn.Hide`, `ColumnsEndpoints.DeleteOfTemplate` | `ColumnsTests.ACoreColumnIsNeverDeleted` |
| Deleting hides the column and keeps its row, so its values stay (AC-41.2, BR-21) | `ItemColumn.Hide` | `ColumnsTests.ADeletedColumnLeavesEntryAndItsRowStays`, `ANonCoreColumnOfTheTemplateIsDeletedOnAnItem`, `ADeletedColumnCanBeDeletedOrChangedNoMore` |
| A type never changes (AC-41.3, BR-21) | `Column.DataType`; no request carries it | `ColumnsTests.ATypeNeverChanges` |
| An order names every visible column exactly once | `ProjectStructure.PutInOrder` | `ColumnsTests.AnOrderMustNameEveryVisibleColumnExactlyOnce` |
| A template change leaves existing items alone unless applied (AC-41.3) | `ColumnsEndpoints` | `ColumnsTests.ATemplateChangeLeavesExistingItemsAloneUnlessApplied` |
| A new item starts from the template as it is now | `CategoriesEndpoints.AddItem` | `ColumnsTests.ANewItemStartsFromTheTemplateAsItIsNow` |
| "Apply to all items" reaches every item of that type, disabled ones too, and no other | `ColumnsEndpoints.AddToTemplate`, `EditOfTemplate`, `DeleteOfTemplate` | `ColumnsTests.AColumnAppliedToAllReachesEveryItemOfThatTypeAndNoOther`, `ARenameAppliedToAllReachesEveryItemOfThatType`, `ADeleteAppliedToAllHidesItOnEveryItemOfThatType` |
| An order applied to all keeps what only that item has after the rest | `ColumnsEndpoints.ReorderOfTemplate` | `ColumnsTests.AnOrderAppliedToAllKeepsWhatOnlyThatItemHasAfterTheRest` |
| One project's template changes no other (BR-33) | `DefaultColumn.ProjectId` | `ColumnsTests.OneProjectsTemplateChangesNoOtherProject` |
| A project copied from an earlier one takes its columns, without what it deleted (AC-05.2) | `ProjectStructure.ApproveAsync`, `Item.CopyFor` | `ColumnsTests.AnEarlierProjectsColumnsAreCopiedWithoutWhatItDeleted` |
| Everyone with access reads; only Finance changes | `Permission.ManageStructure` | `ColumnsTests.EveryoneWithAccessReadsColumnsAndOnlyFinanceChangesThem` |
| One project's route never reaches another's columns (BR-33) | `ProjectStructure.ItemOfAsync`, `ColumnsEndpoints.TemplateOf` | `ColumnsTests.ARouteOfOneProjectNeverReachesAnothersColumns` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectStructure.MapStructureChanges` | `ColumnsTests.AFinishedProjectRefusesColumnChangesAndStaysReadable` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Entry asking for the columns, and refusing a required one left empty (AC-41.1) | The purchase and contractor payment entry use cases, reading `GET .../items/{itemId}/columns` |
| Custom values stored with each entry (step 4), in `ExpenseDetails.CustomValues` under the column's `Key` | The same |
| The item's table in the report following `ShowInReports` and `SortOrder` | The periodic report |
