[Documentation](../README.md) › UC-012

# UC-012: Manage categories and items

**What it is for:** shaping a project's expense structure: its categories, their items and the items' subgroups (FR-05, FR-38, FR-39), which is what expenses are recorded against and what the periodic report prints, in this order. A new item inherits its columns through [UC-013](UC-013-item-columns.md).
**Who uses it:** whoever holds `ManageStructure` in the project, which is Finance. Everyone with access reads it.
**Issue:** redix-api #30; the app side is redix-app #31.

## Where a structure comes from

The default template is a project (`Projects.IsTemplate`) that nobody is a member of, seeded from BRS section 3. Copying from it and copying from an earlier project are therefore one copy.

A project's structure is copied **when the admin approves it** (`ProjectStructure.ApproveAsync`), never before. Registering from the sign-in page always takes the template; someone signed in may name an earlier project they can read (`CreateProjectRequest.StructureSourceId`, kept in `Projects.StructureSourceId`). Only active categories and active items are copied, with their subgroups; never entries, contracts or balances (AC-05.2).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/categories` | `ReadProject` | `200` every category and item, disabled ones included, in the order of the report |
| `GET /projects/{projectId}/categories/offered?itemType=` | `ReadProject` | `200` what an entry of `purchases` or `contractorPayments` may choose from; `400` for another word. The entry use cases call it |
| `POST /projects/{projectId}/categories` with `{ name }` | `ManageStructure` | `201` the category, at the end; `400` `invalidName` |
| `POST /projects/{projectId}/categories/reorder` with `{ ids }` | `ManageStructure` | `200`; `400` `invalidOrder` |
| `POST /projects/{projectId}/categories/{categoryId}` with `{ name }` | `ManageStructure` | `200`; `400` `invalidName`; `404` |
| `POST /projects/{projectId}/categories/{categoryId}/disable`, `/enable` | `ManageStructure` | `200`; `404` |
| `POST /projects/{projectId}/categories/{categoryId}/items` with `{ name, type }` | `ManageStructure` | `201` the item, at the end, with its general group; `400` `invalidName`; `404` |
| `POST /projects/{projectId}/categories/{categoryId}/items/reorder` with `{ ids }` | `ManageStructure` | `200`; `400` `invalidOrder`; `404` |
| `POST /projects/{projectId}/items/{itemId}` with `{ name }` | `ManageStructure` | `200`; `400` `invalidName`; `404` |
| `POST /projects/{projectId}/items/{itemId}/disable`, `/enable` | `ManageStructure` | `200`; `404` |
| `POST /projects/{projectId}/items/{itemId}/subgroups` with `{ name }` | `ManageStructure` | `200`; `400` `invalidName` or `itemTakesNoPurchases`; `404` |
| `POST /projects/{projectId}/items/{itemId}/subgroups/{subgroupId}` with `{ name }` | `ManageStructure` | `200`; `400` `invalidName`; `404`; `409` `generalSubgroup` |
| `GET /projects/{projectId}/items/{itemId}/tables` | `ReadProject` | `200` the item's tables in the order its sheet prints them: its general table (named after the item), each named subgroup, each contract's payments (named after its contractor), with how many approved entries each holds; `404` |
| `POST /projects/{projectId}/items/{itemId}/tables/reorder` with `{ ids }`, every subgroup and contract of the item | `ManageStructure` | `200`; `400` `invalidOrder`; `404` |
| `GET /projects/{projectId}/entry-tables` | `ReadProject` | `200` every table an expense may go into (redix-app #84): each active category's items taking purchases or contractor payments, `{ id, name, items: [{ id, name, tables: [{ id, kind, name }] }] }`, each item's tables in its sheet's order. An item's general and named tables show when it takes purchases, its active contracts when it takes contractor payments; an item or a category with nothing to offer, and every revenue item, is left out |
| `POST /projects/{projectId}/items/{itemId}/subgroups/{subgroupId}/delete` | `ManageStructure` | `200`; `404`; `409` `generalSubgroup` or `subgroupInUse` |
| `POST /projects/{projectId}/items/{itemId}/type` with `{ type }` | `ManageStructure` | `200`; `404`; `409` `itemHasEntries` or `itemHasContract`, except an item of purchases or of contractor payments becoming both, which every entry and contract stays valid for |
| `POST /projects/{projectId}/items/{itemId}/table-percentages` with `{ isOn }` | `ManageStructure` | `200`; `404`. Whether the item's sheet in the periodic report calculates each table's percentage (AC-46.9); off until chosen, in the History table |

Every change also answers `409` `projectFinished` in a finished project. `type` is an expense type, `purchases`, `contractorPayments` or `both`, or a revenue type, `sale`, `vatRefund` or `otherRevenue` (UC-068, FR-38): the type says the item's side. Entering an expense offers the items of its type or of both; entering revenue of any kind offers every revenue item. A revenue item asks no columns (`revenueTakesNoColumns`), and its type changes, even across sides, only while no entry names it.

## Tables

| Table | Holds |
| --- | --- |
| `Categories` | A project's categories: name, place in the report, whether active |
| `Items` | A category's items: name, type, place in the report, whether active |
| `Subgroups` | An item's groups, a general one named `''` in each, so an entry always has a subgroup and reports never special-case it; its place among the item's tables |
| `Contracts.SortOrder` | A contract's place among its item's tables, in the same order as the item's subgroups (migration `ContractTableOrder`: after the subgroups, in the order the contracts were added) |
| `Projects.IsTemplate` | The default template |
| `Projects.StructureSourceId` | The earlier project to copy from on approval; `Guid.Empty` for the template |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| An item a contract names keeps its type, or that contract's payments would be refused | `CategoriesEndpoints.ChangeItemType` | `CategoriesTests.AnItemAContractNamesKeepsItsType` |
| redix-api #109: رأس المال (`ItemType.Capital`) is a revenue type whose tables are its partners: an item with partners keeps its type (`409` `itemHasPartners`), and it takes no sub-table (`itemTakesNoPurchases`) | `CategoriesEndpoints.ChangeItemType`, `AddSubgroup`, `Item.HasSubgroupTables` | `CategoriesTests.UC_109_ACapitalItemStaysOneWhilePartnersAreItsTablesAndTakesNoSubTable` |
| redix-api #109: every project starts with رأس المال, a category of its own first among the revenue, its item calculating each partner's share | `StructureSeed`, migration `CapitalIsATable` | `CategoriesTests.UC_109_EveryProjectStartsWithRasAlMalACategoryOfItsOwnFirstAmongTheRevenueWhoseSheetGivesEachPartnersShare`, `TheTemplateHoldsTheCategoriesAndItemsOfTheBrs` |
| BR-31, redix-api #109: a project has one رأس المال; a second capital item, added or by a type change, answers `409` `capitalItemExists`, since its total would show one partner's balance to every reader | `ProjectStructure.RefusalOfTypeAsync` | `CategoriesTests.BR_31_UC_109_AProjectHasOneCapitalItemSoNoSecondOneShowsOnePartnersBalance` |
| BR-31, redix-api #109: رأس المال's tables show every partner to whoever sees all capital, their own to a partner, none to anyone else | `CategoriesEndpoints.Tables`, `PartnerSight` | `CategoriesTests.BR_31_UC_109_RasAlMalsTablesShowEveryPartnerToWhoeverSeesAllCapitalTheirOwnToAPartnerAndNoneToAnyoneElse` |
| redix-api #109: revenue goes into every revenue item but رأس المال, and a capital item is offered only for a partner | `Item.IsOfferedFor` | `CategoriesTests.UC_109_RevenueGoesIntoEveryRevenueItemButRasAlMalAndAPartnerIntoNothingElse` |
| The template holds the categories and items of BRS section 3 | `StructureSeed` | `CategoriesTests.TheTemplateHoldsTheCategoriesAndItemsOfTheBrs` |
| The category تكلفة الأراضي holds one item, تكلفة الأرض, of both types, because an entry needs an item (BRS section 3 lists it empty) | `StructureSeed` | `CategoriesTests.LandCostCanBeRecordedBecauseItsCategoryHoldsOneItemOfBothTypes` |
| A subgroup no entry uses is removed, and one added later still goes last; the general one is never removed | `CategoriesEndpoints.DeleteSubgroup`, `AddSubgroup` | `CategoriesTests.ASubgroupNoEntryUsesIsRemoved`, `ASubgroupAddedAfterARemovalStillGoesLast`, `ASubgroupAnEntryUsesIsNotRemoved`, `TheGeneralSubgroupIsNeverRemoved` |
| An item's tables, its general one, its subgroups and its contracts, reorder as one list, which is the order of its sheet; a new subgroup or contract goes last (AC-46.8) | `CategoriesEndpoints.ReorderTables`, `Tables`, `ProjectStructure.NextTablePlaceAsync` | `CategoriesTests.AC_46_8_AnItemsTablesAreReorderedAsOneListTheGeneralOneToo`, `AC_46_8_AnItemListsItsGeneralTableEachSubgroupAndEachContractWithTheirEntriesAndANewTableGoesLast`, `AnOrderOfTablesNamesEveryTableExactlyOnce`, `OneProjectsRoutesNeverReachAnothersSubgroups` |
| A sub-table holds purchases, so an item that takes none has no sub-table (Muteb, 7 Nov 2026) | `CategoriesEndpoints.AddSubgroup` | `CategoriesTests.SubTable_AnItemThatTakesNoPurchasesHasNoSubTable` |
| A sub-table added is offered to an expense and takes a purchase | `EntryTables`, `ExpensesEndpoints` | `CategoriesTests.SubTable_ASubTableAddedIsOfferedAndTakesAPurchase` |
| An item of purchases or of contractor payments may become both whatever it holds | `CategoriesEndpoints.ChangeItemType` | `CategoriesTests.SubTable_AnItemOfContractorPaymentsWithAContractMayBecomeBoth`, `SubTable_AnItemOfPurchasesWithAnEntryMayBecomeBoth` |
| R10: an expense is offered an item's general table, each named one and each active contract, in its sheet's order | `CategoriesEndpoints.EntryTables`, `TablesOf` | `CategoriesTests.R10_AnItemOffersAnExpenseItsGeneralTableEachNamedOneAndEachActiveContractInItsSheetsOrder` |
| R7: no disabled item or category, and no item without a table is offered; revenue items are offered only to whoever moves money (redix-api #101) | `CategoriesEndpoints.EntryTables` | `CategoriesTests.R7_NoDisabledItemOrCategoryAndNoItemWithoutATableIsOffered` |
| An item's type changes while no entry names it, in the History table, old and new | `CategoriesEndpoints.ChangeItemType` | `CategoriesTests.AnItemsTypeChangesWhileItHasNoEntriesAndIsOfferedAsItsNewTypeIs`, `AChangedTypeIsInTheHistoryOldAndNew`, `AnItemWithAnEntryKeepsItsType`, `OnlyFinanceChangesATypeAndNeverAnotherProjects` |
| A changed type asks the default columns of the new type, and keeps what a person added | `CategoriesEndpoints.ChangeItemType` | `CategoriesTests.AChangedTypeAsksTheColumnsOfTheNewTypeAndKeepsWhatAPersonAdded`, `ChangingATypeThereAndBackLeavesOneOfEachColumn` |
| FR-38: an item is added on its side, and revenue entry offers every revenue item and no expense one | `Item.IsOfferedFor` | `CategoriesTests.FR_38_AnItemIsAddedOnItsSideAndRevenueEntryOffersEveryRevenueItemAndNoExpenseOne` |
| FR-38: a revenue item asks no columns | `ColumnsEndpoints.AddToItem`, `AddToTemplate` | `CategoriesTests.FR_38_ARevenueItemAsksNoColumns` |
| Every item has a general subgroup | `Item.New`, `StructureSeed.Subgroups` | `CategoriesTests.EveryItemHasAGeneralSubgroup`, `AnItemIsAddedWithItsTypeAtTheEndOfItsCategoryWithItsGeneralGroup` |
| Every approved project starts with its own copy of the structure | `ProjectStructure.ApproveAsync` | `CategoriesTests.ApprovingCopiesTheTemplateIntoRowsOfTheProjectsOwn`, `ThePublicRegistrationStartsFromTheTemplateWhenApproved` |
| A waiting or rejected project has no structure | `ProjectStructure.ApproveAsync` | `CategoriesTests.AWaitingOrRejectedProjectHasNoStructure` |
| A project copies an earlier one's structure, and nothing else (AC-05.2) | `ProjectStructure.ApproveAsync`, `Category.CopyFor` | `CategoriesTests.AProjectCopiedFromAnEarlierOneTakesItsStructureAtApproval` |
| What was disabled is not copied | `Category.CopyFor` | `CategoriesTests.WhatWasDisabledInTheEarlierProjectIsNotCopied` |
| Only a project the caller reads can be copied (BR-33) | `ProjectsEndpoints.Create` | `CategoriesTests.AnEarlierProjectTheCallerCannotReadCannotBeCopied`, `TheTemplateItselfCannotBeNamedAsAnEarlierProject` |
| Nobody sees the template | Nobody holds a permission in it | `CategoriesTests.NobodySeesTheTemplate` |
| A change in one project touches nothing else (BR-33) | Each project's own rows | `CategoriesTests.AChangeInOneProjectLeavesTheTemplateAndOtherProjectsAlone` |
| One project's route never reaches another's rows (BR-33) | `ProjectStructure.ItemOfAsync` and the category lookups | `CategoriesTests.ARouteOfOneProjectNeverReachesAnothersRows` |
| The order set here is the order of the report (AC-39.1) | `IOrdered`, `CategoriesEndpoints.ReorderCategories`, `ReorderItems` | `CategoriesTests.TheOrderOfCategoriesIsTheOrderTheReportReads`, `TheOrderOfItemsIsTheOrderTheReportReads` |
| An order names every row exactly once | `ProjectStructure.PutInOrder` | `CategoriesTests.AnOrderMustNameEveryRowExactlyOnce` |
| A disabled item leaves entry and stays listed; no item or category can be deleted (AC-05.3, BR-15) | `Item.IsOfferedFor`; no delete route exists | `CategoriesTests.ADisabledItemDisappearsFromEntryAndStaysListed`, `NothingInTheStructureCanBeDeleted` |
| A disabled category takes its items out of entry | `Category.ItemsOfferedFor` | `CategoriesTests.ADisabledCategoryTakesItsItemsOutOfEntryAndStaysListed` |
| What was disabled can be enabled | `CategoriesEndpoints.EnableItem`, `EnableCategory` | `CategoriesTests.ADisabledItemComesBackWhenEnabled` |
| An item is offered only for what its type allows (AC-38.1) | `Item.IsOfferedFor` | `CategoriesTests.APurchasesItemIsNotOfferedForContractorPayments`, `AContractorPaymentsItemIsNotOfferedForPurchases` |
| A new item reaches Field at their next read (AC-05.1) | `CategoriesEndpoints.Offered` | `CategoriesTests.ANewItemIsOfferedToFieldAtOnce` |
| A name has 1 to 100 characters, spaces around it trimmed | `Project.IsAcceptableName` | `CategoriesTests.ACategoryAnItemOrASubgroupNeedsANameOfAtMost100Characters` |
| The general subgroup has no name to change | `CategoriesEndpoints.RenameSubgroup` | `CategoriesTests.TheGeneralSubgroupHasNoNameToChange` |
| Everyone with access reads; only Finance changes | `Permission.ManageStructure` | `CategoriesTests.EveryoneWithAccessReadsTheStructureAndOnlyFinanceChangesIt` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges`, an endpoint filter | `CategoriesTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| A subgroup an expense or revenue is kept on is not removed (`subgroupInUse`); "is anything kept on it" has one owner for items and subgroups | `ProjectStructure.SubgroupHasEntriesAsync`, `ItemHasEntriesAsync` | `CategoriesTests.ASubgroupAnEntryUsesIsNotRemoved`, `ASubgroupRevenueIsKeptOnIsNotRemoved` |

## Left to later use cases

| What | Use case |
| --- | --- |
| The item picker on the entry screens | The entry use cases, through `GET .../categories/offered` |
| Printing in the order set here (AC-39.1) | The periodic report, which reads `SortOrder` |
