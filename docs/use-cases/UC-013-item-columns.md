[Documentation](../README.md) › UC-013

# UC-013: Manage item columns

**What it is for:** changing the fields an item asks for without a code change (FR-41): on one item, or on the project's default template for one type of item.
**Who uses it:** whoever holds `ManageStructure` in the project, which is Finance. Everyone with access reads them.
**Issue:** redix-api #31; the app side is redix-app #32.

## How it fits together

The seeded template project holds the default columns of each item type. When a project is approved it takes its own copy, so each project has its own default template and changing it touches no other (BR-33).

A new item starts from its project's default columns of its type (FR-38), and from then on holds its own rows in `item_columns`. A column keeps its `Key` from the template to every item, which is how "apply to all items" finds it. A column a person adds has a key starting `custom.`. Every column an entry answers in no field of its own, a person's own, the unit price, the quantity and the product alike, has its value kept in the entry's `ColumnValues` under its key (redix-api #129, R5); the date, the supplier, the amount, the account, the payment method, the file, the contract, the place in it and the notes are the entry's own fields (`Column.IsEntryField`). An item of both types asks a purchase nothing only a contractor payment's template has, and a payment nothing only a purchase's has (`entries.AskedColumns`).

A list's choices live in its definition, `Choices` (redix-api #129): each has an id, a name and a sign, `none`, `plus` or `minus`. An entry keeps the id of the choice chosen, never its name, so a choice keeps its entries whatever it is called. A sign decides the direction of the amount of the entry that chooses it, so an item holds at most one list whose choices carry a sign.

An item of the template holds no column rows of its own: its columns are its type's defaults, and its copy takes them. That keeps about 550 rows out of the migrations.

Each type's template holds its columns in the order and under the words of the report workbook the owner approved on 13 Nov 2026 (redix-api #129): a purchase الوصف, المورد, التاريخ, سعر الحبة, العدد, نوع القيد, الحساب, طريقة الدفع, المرفق and التكلفة; a contractor payment الوصف, التاريخ, الاتفاق, الحساب, طريقة الدفع, المورد, المرفق, العقد and الدفعة; an item of both types the purchase's, each column only a payment has after the one it follows in a payment's; رأس المال نوع الحركة, الوصف, التاريخ, الحساب, المرفق and المبلغ; revenue الوصف, التاريخ, الحساب, المرفق and المبلغ. «الوصف» is every entry's one description, first in every table but رأس المال's, where «نوع الحركة» comes before it; «الصنف» and «ملاحظات» went into it. The migration gave every item and project template the new words, unless a project had named the column itself. «نوع القيد» and «نوع الحركة» are the lists whose signs decide the amount's direction. A person adds no column to a revenue item, رأس المال among them: it asks what its template gives it. A contractor payment is not asked the supplier, which its contract names.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/items/{itemId}/columns` | `ReadProject` | `200` what the item asks for, in order, without deleted ones; `404` |
| `GET /projects/{projectId}/default-columns/{itemType}` | `ReadProject` | `200` the template for `purchases`, `contractorPayments`, `both`, `capital`, `sale`, `vatRefund` or `otherRevenue`; `400` for another word |
| `POST /projects/{projectId}/items/{itemId}/columns` with `{ name, dataType, isRequired, showInReports, choices: [{ name, sign }] }` | `ManageStructure` | `200`; `400` `invalidName`, `invalidDataType`, `invalidOptions` or `signedListExists`; `404` |
| `POST /projects/{projectId}/items/{itemId}/columns/reorder` with `{ ids }` | `ManageStructure` | `200`; `400` `invalidOrder`; `404` |
| `PUT /projects/{projectId}/items/{itemId}/columns/{columnId}` with `{ name, isRequired, showInReports, newChoices: [{ name, sign }] }` | `ManageStructure` | `200`; `400` `invalidName`, `invalidOptions` or `signedListExists`; `404`; `409` `coreColumn` |
| `DELETE /projects/{projectId}/items/{itemId}/columns/{columnId}` | `ManageStructure` | `200`; `404`; `409` `coreColumn` |
| `POST /projects/{projectId}/default-columns/{itemType}` with the same, and `applyToAllItems` | `ManageStructure` | As on an item |
| `POST /projects/{projectId}/default-columns/{itemType}/reorder` with `{ ids, applyToAllItems }` | `ManageStructure` | As on an item |
| `PUT /projects/{projectId}/default-columns/{itemType}/{columnId}` with the same, and `applyToAllItems` | `ManageStructure` | As on an item |
| `DELETE /projects/{projectId}/default-columns/{itemType}/{columnId}?applyToAllItems=true` or `=false`, in the query since a delete has no body | `ManageStructure` | As on an item |

Every change also answers `409` `projectFinished` in a finished project. A column answers as `{ id, key, name, dataType, isCore, isCustom, isRequired, showInReports, choices: [{ id, name, sign }], isEntryField }`.

## Tables

| Table | Holds |
| --- | --- |
| `DefaultColumns` | A project's default template: what a new item of each type starts with |
| `item_columns` | What each item asks for, with `IsHidden` once deleted |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A new item inherits the default columns of its type (FR-38) | `structure.change.addTemplateColumns` | `ColumnsTests.ANewItemInheritsTheDefaultColumnsOfItsType` |
| A template item is copied with the columns of its type | `structure.copyCategories` | `ColumnsTests.ATemplateItemIsCopiedWithTheColumnsOfItsType` |
| The core columns of a purchase are the five left of BR-20: date, supplier, total, account and file, the bill number left with D2 (findings row 150); «نوع القيد», which decides the total's sign; and «الوصف» (redix-api #129) | `StructureSeed` | `ColumnsTests.TheCoreColumnsOfAPurchaseAreTheFiveLeftOfBr20AndNawAlQaydWhichDecidesItsSign` |
| Every template holds «الوصف» and none «ملاحظات» or «الصنف»; each column is named with the approved workbook's word (redix-api #129 D1) | `db/migrations/00004_template.sql` | `ColumnsTests.R129_D1_NoTemplateHoldsTheNotesOrTheProductAndEveryOneHoldsTheDescription`, `R129_EachColumnIsNamedWithTheApprovedWorkbooksWord` |
| An item of both types holds the purchase columns, with what only a contractor payment has after the column it follows | `db/migrations/00004_template.sql` | `ColumnsTests.AnItemOfBothTypesHoldsThePurchaseColumnsWithWhatOnlyAContractorPaymentHasAfterWhatItFollows` |
| Each template holds its columns in the order of the approved workbook, and a purchase's prints the supplier, the account, the method and the file (redix-api #129) | `StructureSeed` | `ColumnsTests.R129_EachTemplateHoldsItsColumnsInTheOrderOfTheApprovedWorkbook`, `R129_ThePurchaseTemplatePrintsTheSupplierTheAccountTheMethodAndTheFileButNotTheNotes` |
| A person adds no column to a revenue item; رأس المال asks «نوع الحركة» from its template (FR-38, redix-api #129) | `structure.Routes.addToItem`, `structure.change.template` | `ColumnsTests.R129_APersonStillAddsNoColumnToARevenueItem`, `R129_RasAlMalAsksNawAlHarakaWhoseChoicesPutInAndTakeOut` |
| A payment is asked nothing only a purchase has, the supplier among it, and a purchase nothing only a payment has; the tables Add entry offers name the columns each asks (FR-38, redix-api #129, R3) | `entries.asksOf`, `entries.AskedColumns` | `ContractorPaymentsTests.R129_R3_APaymentIsAskedNothingOnlyAPurchaseHasAndAPurchaseNothingOnlyAPaymentHas`, `R129_AUnitPriceRequiredOfThePurchasesIsNotAskedOfAPaymentOnTheSameItem` |
| Every item and every project's template took the new columns, and the old ones follow the approved order and print as it does, a person's own after them (redix-api #129, R6) | Migration `EntryTypeAndMovementTypeLists` | `MigrationStepsTests.R129_EntryTypeAndMovementTypeLists_EveryEntryKeepsItsFigureAndTakesTheChoiceItsSignSays` |
| A column added to one item is asked in that item only (AC-41.1) | `structure.Routes.addToItem` | `ColumnsTests.AColumnAddedToOneItemIsAskedInThatItemOnly` |
| A column needs a name of 1 to 100 characters | `structure.columnRequest.refusalOfNew`, `structure.column.edited` | `ColumnsTests.AColumnNeedsANameOfAtMost100Characters` |
| A list a person added gains choices; those it has stay, in their order, because entries hold them; a new choice has the limit of a name; the template's reach items only when applied (BR-21) | `structure.column.edited`, `structure.Routes.editOfItem`, `structure.Routes.editOfTemplate` | `ColumnsTests.APickListGainsChoicesAndKeepsItsOwnInTheirOrder`, `AnEditThatGivesNoNewChoiceChangesNoChoice`, `ANewChoiceIsWithinTheLimitOfAName`, `OnlyAPickListAPersonAddedGainsChoices`, `AnEntryCanHoldTheChoiceAListGained`, `AChoiceTheTemplateGainsReachesItemsOnlyWhenApplied` |
| A pick list needs a choice; another type keeps none | `structure.columnRequest.refusalOfNew`, `structure.columnRequest.newColumn` | `ColumnsTests.APickListNeedsAChoice`, `APickListKeepsItsChoicesAndAnotherTypeNone` |
| A list's choices live in its definition, each with its own id and an optional sign, + or − (redix-api #129) | `ColumnChoice`, `structure.choicesOf` | `ColumnsTests.R129_AListsChoicesEachHaveAnIdAndASignOfTheirOwn` |
| An item holds at most one list whose choices give a sign, so the amount's direction has one answer (redix-api #129) | `structure.hasTwoSignedLists` | `ColumnsTests.R129_AnItemTakesOneListWhoseChoicesGiveASignSoTheAmountsDirectionHasOneAnswer` |
| An entry keeps the id of the choice chosen, and a choice's name is refused (redix-api #129, R5) | `structure.Accepts`, `structure.column.choiceOf` | `ColumnsTests.R129_R5_AnEntryKeepsTheIdOfTheChoiceChosenAndIsRefusedItsName`, `ExpensesTests.FR_41_AValueThatFitsItsColumnIsKept` |
| Every choice kept before got an id and no sign, and every value naming one holds its id (redix-api #129) | Migration `ListChoicesWithSigns` | `MigrationStepsTests.R129_ListChoicesWithSigns_EveryChoiceGetsAnIdAndNoSignAndEveryValueNamingOneHoldsItsId` |
| A person chooses among five types | `structure.columnRequest.refusalOfNew` | `ColumnsTests.APersonCannotAddAFileColumn` |
| A core column is renamed and reordered (BR-20) | `structure.column.edited`, `structure.inOrder` | `ColumnsTests.ACoreColumnIsRenamedAndReordered` |
| A core column changes nothing but its name (BR-20) | `structure.column.edited` | `ColumnsTests.ACoreColumnChangesNothingButItsName` |
| Any other column changes what entry asks and what prints | `structure.column.edited` | `ColumnsTests.AColumnThatIsNotCoreChangesWhatEntryAsksAndWhatPrints` |
| A core column is never deleted, on an item or in the template (AC-41.2) | `structure.change.hide`, `structure.Routes.deleteOfTemplate` | `ColumnsTests.ACoreColumnIsNeverDeleted` |
| Deleting hides the column and keeps its row, so its values stay (AC-41.2, BR-21) | `structure.change.hide` | `ColumnsTests.ADeletedColumnLeavesEntryAndItsRowStays`, `ANonCoreColumnOfTheTemplateIsDeletedOnAnItem`, `ADeletedColumnCanBeDeletedOrChangedNoMore` |
| A type never changes (AC-41.3, BR-21) | `structure.column.edited`; no request carries it | `ColumnsTests.ATypeNeverChanges` |
| An order names every visible column exactly once | `structure.inOrder` | `ColumnsTests.AnOrderMustNameEveryVisibleColumnExactlyOnce` |
| A template change leaves existing items alone unless applied (AC-41.3) | `structure.Routes.editOfTemplate` | `ColumnsTests.ATemplateChangeLeavesExistingItemsAloneUnlessApplied` |
| A new item starts from the template as it is now | `structure.Routes.addItem` | `ColumnsTests.ANewItemStartsFromTheTemplateAsItIsNow` |
| "Apply to all items" reaches every item of that type, disabled ones too, and no other | `structure.Routes.addToTemplate`, `structure.Routes.editOfTemplate`, `structure.Routes.deleteOfTemplate` | `ColumnsTests.AColumnAppliedToAllReachesEveryItemOfThatTypeAndNoOther`, `ARenameAppliedToAllReachesEveryItemOfThatType`, `ADeleteAppliedToAllHidesItOnEveryItemOfThatType` |
| An order applied to all keeps what only that item has after the rest | `structure.Routes.reorderOfTemplate` | `ColumnsTests.AnOrderAppliedToAllKeepsWhatOnlyThatItemHasAfterTheRest` |
| One project's template changes no other (BR-33) | `access.Gate.Needs` | `ColumnsTests.OneProjectsTemplateChangesNoOtherProject` |
| A project copied from an earlier one takes its columns, without what it deleted (AC-05.2) | `structure.Copy`, `structure.copyCategories` | `ColumnsTests.AnEarlierProjectsColumnsAreCopiedWithoutWhatItDeleted` |
| Everyone with access reads; only Finance changes | `access.ManageStructure` | `ColumnsTests.EveryoneWithAccessReadsColumnsAndOnlyFinanceChangesThem` |
| One project's route never reaches another's columns (BR-33) | `structure.change.item`, `structure.change.template` | `ColumnsTests.ARouteOfOneProjectNeverReachesAnothersColumns` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `ColumnsTests.AFinishedProjectRefusesColumnChangesAndStaysReadable` |

## On the web: Columns (redix-api #174)

The web's Columns is the app's screen in a tab, opened from an item's panel in Categories and items: New column, gray in a finished project or for a revenue item; the item's category and type, a revenue item saying it takes no columns of its own; this item's own columns or the default template of its type, chosen as the app's two segments; the hint, for whoever may shape the structure; then each column, custom or not, its type, required or optional, in reports, a core one marked. Its columns come from `structure.ColumnsOfItem` and `structure.ColumnsOfTemplate`, the functions `GET …/items/{itemId}/columns` and `GET …/default-columns/{itemType}` answer. A row opens the column in the details panel with Edit, and Delete for one not core, asked first as the app's swipe asks; New column asks the name, the type of the five a person chooses, a list's choices a line each, required and in reports; Edit asks the name, required and in reports, fixed on a core column, and a list a person added gains new choices. A template's change, its new column, edit, delete or order, ends with the app's question, the template only or every item of its type. Each keeps through the rule the app's route runs: `structure.AddToItem`, `EditItemColumn`, `DeleteItemColumn`, `ReorderItemColumns`, `AddToTemplate`, `EditTemplateColumn`, `DeleteTemplateColumn`, `ReorderTemplate`, run through the shared changes.Writer. A row moves by dragging it or with Alt+Up and Alt+Down (#174 D1). Its pages are in `go-backend/web` (`columns.go`, `columns.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/itemColumns?item={id}&scope=template` | An item's Columns: its own, or the template of its type | `200` the tab's content; its title only for an item the project does not hold |
| `GET /details/columns/{item}/{scope}/{id}` | A column in the details panel, `scope` `item` or `template`; `?asking=edit` or `delete`; `id` `new` for New column | `200` the panel or its form; `404` without ManageStructure, or for an item or a column there is none of |
| `POST /columns/{item}/{scope}/new/add`, `…/{id}/edit`, `…/{id}/delete` | Keeps New column (`name`, `dataType`, `choices`, `required`, `reports`), Edit (`name`, `required`, `reports`, new `choices`) or Delete; a template's with `applyToAll` | `200` the column in the panel, Columns beside it, or Columns alone once deleted; or the form or the panel saying why not; `403` without ManageStructure; `404` for an action there is none of |
| `POST /columns/{item}/{scope}/reorder` | Keeps the columns' order (`ids`, comma-separated); a template's without `applyToAll` first asks the app's question in the details panel (`HX-Retarget`) | `200` Columns, saying why above it when refused |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Columns shows an item's own columns or its type's template, each with what the app's row says, a core one marked; a revenue item takes no new column (FR-41) | `structure.ColumnsOfItem`, `structure.ColumnsOfTemplate`, `web.columnLine` | `TestWeb_R174_UC013_ColumnsListAsTheAppsScreen` |
| New column, Edit and Delete as the app's dialogs: a name of 1 to 100 letters, a list with a choice, a list's choices only added to, Delete asked first, a core column never deleted; the order kept, refused when it is not exactly the item's columns (AC-41.1, AC-41.2, BR-20, BR-21) | `structure.AddToItem`, `EditItemColumn`, `DeleteItemColumn`, `ReorderItemColumns` | `TestWeb_R174_UC013_NewEditAndDeleteAColumnAsTheAppsDialogs` |
| A template's new column, edit, delete and order ask whether they reach every item of the type, and reach them only when told (AC-41.3, BR-21) | `structure.AddToTemplate`, `EditTemplateColumn`, `DeleteTemplateColumn`, `ReorderTemplate` | `TestWeb_R174_UC013_ATemplatesChangeAsksWhetherItReachesEveryItem` |
| Columns are for whoever shapes the structure; a finished project shows them without handles and changes nothing (AC-04.1); a failing database answers 500 | `web.site.keepColumn`, `web.site.reorderColumns` | `TestWeb_R174_UC013_ColumnsAreForWhoeverShapesTheStructure` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Entry asking for the columns, and refusing a required one left empty (AC-41.1) | The purchase and contractor payment entry use cases, reading `GET .../items/{itemId}/columns` |
| Column values stored with each entry, in `Entries.ColumnValues` under the column's `Key` (redix-api #129) | UC-022 "Record a purchase" |
| The item's table in the report following `ShowInReports` and `SortOrder` | The periodic report |
