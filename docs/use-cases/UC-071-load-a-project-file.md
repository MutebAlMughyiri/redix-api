[Documentation](../README.md) › UC-071

# UC-071: Load a project file

**What it is for:** bringing a project that lived in Excel into REDIX in one step (FR-53, BR-43): its manager picks the project's JSON file, and REDIX adds the tables it names, records and approves every row through the same rules as Add entry, enters the actual balances and issues the file's report. One refused row, and nothing is saved.
**Who uses it:** the manager who created the project, who holds `ManageProject` (D1), into an approved project that holds no entry yet (D4).
**Issue:** redix-api #132 "Load a project's JSON file into REDIX: its structure, entries, actual balances and last report"; the file's shape is redix-api #130's.

## Endpoint

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/{projectId}/project-file` with the file as a form's `file`, as every file the app sends, so its upload waits as long as a file may (UC-070) | `ManageProject` (D1) | `200` `{ entries, tables, report }`: how many entries were recorded and approved, in how many tables, and the report issued, 0 for none; `400` `notAProjectFile` or `newerVersion`, nothing read (AC-53.9); `409` `projectHasEntries` (AC-53.3); `400` `rowsRefused` with every refusal under `rows`, nothing saved (AC-53.5); `403` without `ManageProject` (AC-53.2); `409` `projectFinished` |

A refusal under `rows` says where it is, by the file's names: `category`, `item`, `table`, and `row`, the row's place in its table counted from 1, as the sheet's «#» counts it, 0 for the part itself; then `owner`, whose rule refused it, which says whose words the app shows (D14): `entry` for Add entry's codes (`expenses.*`), `structure` for a category, an item or a subgroup (`categories.*`), `contract`, `partner`, `supplier`, `paymentMethod`, `reconciliation`, or `file` for the load's own, `accountNotInProject`, `attachmentNotInFile`, `unknownChoice` and `reportAlreadyIssued` (`projects.loadFile.*`); then `code`, and the `columnKey`, `value` and `column` a code names. `GET /projects` says of each project `holdsEntries`, from the same query the route refuses with, so the app grays its button (D15).

## How it loads

One write, `changes.Writer.Write`, holds everything in one transaction; a refusal anywhere returns before it commits, so nothing is saved (D3). The load holds no rule of its own: each part goes to the owner that takes it by hand.

| Part | Owner | When the project lacks it |
| --- | --- | --- |
| A supplier, a payment method of the file's lists | `SuppliersEndpoints.AddAsync` (approved, since the manager keeps the settings), `PaymentMethodsEndpoints.AddAsync` | Added (D6) |
| A category, an item | `CategoriesEndpoints.AddCategoryAsync`, `AddItemAsync`; found by name, an item within its category | Added at the end (D5, D13) |
| A purchase or revenue table | `CategoriesEndpoints.AddSubgroupAsync`; the item's own name is its general table | Added (D5) |
| A contract's payments | `ContractsEndpoints.AddAsync`; its contractor is the table's name after «دفعات », a supplier added when missing, its value `contractAmount`, an open value when empty | Added (D6) |
| A partner | `PartnersEndpoints.AddAsync`, with no login | Added (D6) |
| An account | Found by name; the file carries no type or custodian | Refused, `accountNotInProject` (D6) |
| A row | `EntryRecording.RecordAsync`, telling no decider, then `ApprovalsEndpoints.ApproveAsync` in the caller's name; it counts as approved from the file's report time, while its History lines keep the moment of the load (D7, D12). The rows are recorded by their date, then in the file's order, so each takes its «رقم القيد» in the calendar's order (redix-api #128 D6) | — |
| The actual balances | `ReconciliationsEndpoints.EnterAsync`: one reconciliation, a difference staying visible (D8) | — |
| The report | `PeriodicReportsEndpoints.IssueAsync` with the file's number and time; none for a file with none (D9) | A number the project issued already is `reportAlreadyIssued`, found before anything is saved |

Structure is added, never renamed or removed (D5). A table the caller is not offered is listed once, as `unknownTable`; its rows are not read.

## The file

REDIX's own model, as the generated workbook lays it out (redix-api #130), with what D11 adds: `accounts[].actualBalance`, and «الحساب» on the VAT refund table.

```
{ format: "redix.project", version: 1, project: "<name>",
  report: { number, date: "yyyy-MM-dd", time: "HH:mm" },
  accounts: [ { name, actualBalance } ],
  lists: [ { key, name, options: [ { id: "<key>-<n>", label, sign: "+" | "-" | "" } ] } ],
  categories: [ { name, items: [ { name, type, tables: [
      { name, kind, columns: [ { key, name, type, list? } ], rows: [ { <column key>: value } ], contractAmount?, details? } ] } ] } ] }
```

| Part | How it is read |
| --- | --- |
| `report` | Its number, and its date and time in Saudi Arabia's time; a file with none, or numbered 0, issues no report (D9) |
| `accounts` | Each account by its name, which the project must already hold (D6), with what it really held at the report (D8, D11) |
| `lists` | Every list once; a row keeps an option's id, and the option's label is the name REDIX knows it by |
| `categories` → `items` → `tables` | An item's `type` and a table's `kind` are named as `ItemType` is; a column's `type` as `ColumnDataType` is |
| `details`, `project` | Not read: the land details belong to redix-api #122 (D10), and the project is the one the manager chose (G1) |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| redix-api #128 D6: a loaded file's rows are numbered by their date, then the file's order, not the order of its tables | `projectfile.record` | `ProjectFileTests.R128_D6_ALoadedFilesRowsAreNumberedByTheirDateThenTheFilesOrderNotTheOrderOfItsTables` |
| AC-53.9: a file that is not a REDIX project file, or is not laid out as one, is refused with `notAProjectFile`, and nothing is read | `projectfile.read` | `ProjectFileTests.AC_53_9_AFileThatIsNotARedixProjectFileIsRefusedAndNothingIsRead` |
| AC-53.9: a file of a newer version is refused with `newerVersion` before its shape is read | `projectfile.read` | `ProjectFileTests.AC_53_9_AFileOfANewerVersionIsRefusedBeforeItsShapeIsRead` |
| A project file is read with its report, its actual balances, its lists and its tables | `projectfile.read` | `ProjectFileTests.AC_53_9_AProjectFileIsReadWithItsReportItsActualBalancesItsListsAndItsTables` |
| D2, D9: a file with no report, no accounts and no rows is its structure alone | `projectfile.read` | `ProjectFileTests.D9_D2_AFileWithNoReportNoAccountsAndNoRowsIsReadAsItsStructureAlone` |
| D7, BR-43: an entry recorded for a project file passes every check of Add entry and keeps its decision, and tells no decider, while Add entry tells them | `entries.Routes.record`, `AddAsync` | `ProjectFileTests.D7_AnEntryRecordedForAProjectFilePassesTheChecksOfAddEntry`, `D7_AnEntryRecordedForAProjectFileTellsNoDeciderWhileAddEntryTellsThem` |
| D9, AC-53.7: a report is issued with the number and the time it is given, and the next one follows it | `reports.Issue` | `ProjectFileTests.D9_AReportIsIssuedWithTheNumberAndTheTimeItIsGivenAndTheNextOneFollowsIt` |
| AC-53.1, D5, D6, D7, D13: a file loads its missing categories, items, tables, contracts, partners, suppliers and payment methods, and every row is recorded and approved in the manager's name, telling no decider | `projectfile.Routes.load` | `ProjectFileTests.AC_53_1_D5_D6_D7_D13_AFileLoadsItsMissingStructureAndEveryRowApprovedInTheManagersNameTellingNoDecider` |
| AC-53.6: after the load the summary, its revenue, its expenses and its surplus equal the file's | `projectfile.Routes.load` | `ProjectFileTests.AC_53_6_AfterTheLoadTheSummaryItsRevenueItsExpensesAndItsSurplusEqualTheFiles` |
| AC-53.7, AC-53.8, D9: the file's report is issued with its number and time, the next follows it, and its actual balances are the latest reconciliation | `projectfile.Routes.load`, `reports.Issue`, `accounts.Enter` | `ProjectFileTests.AC_53_7_AC_53_8_D9_TheFilesReportIsIssuedWithItsNumberAndTimeAndItsActualBalancesAreTheLatestReconciliation` |
| D2, D9: a file with no report, no accounts and no rows loads its structure alone and issues nothing | `projectfile.Routes.load` | `ProjectFileTests.D9_D2_AFileWithNoReportNoAccountsAndNoRowsLoadsItsStructureAloneAndIssuesNothing` |
| D12, G7: no loaded entry shows as new, while one approved after the load does | `entries.Routes.approve` | `ProjectFileTests.D12_G7_NoLoadedEntryShowsAsNewWhileOneApprovedAfterTheLoadDoes` |
| AC-53.2: without `ManageProject` the route answers 403 and nothing is saved | `projectfile.Map` | `ProjectFileTests.AC_53_2_WithoutManageProjectTheRouteAnswers403AndNothingIsSaved` |
| AC-53.3, D15: a project holding an entry says so, and refuses the file with `projectHasEntries` | `projectfile.Routes.load`, `projects.sight.view` | `ProjectFileTests.AC_53_3_D15_AProjectHoldingAnEntrySaysSoAndRefusesTheFileWithProjectHasEntries` |
| AC-53.4, AC-53.5, G3: a row Add entry refuses keeps its code, every refused row is listed with its table and place, a file named in a row is `attachmentNotInFile`, and nothing is saved | `projectfile.Routes.load`, `entries.Routes.record` | `ProjectFileTests.AC_53_4_AC_53_5_G3_ARowAddEntryRefusesKeepsItsCodeEveryRefusedRowIsListedAndNothingIsSaved` |
| G4: an option the file's lists do not hold, or a choice the table's list does not have, is `unknownChoice`, naming the value and the column | `projectfile.Routes.load` | `ProjectFileTests.G4_AnOptionTheFilesListsDoNotHoldOrAChoiceTheTablesListDoesNotHaveIsAnUnknownChoiceNamingItsColumn` |
| D14: a part its owner refuses is listed with that owner's code, and nothing is saved | `projectfile.Routes.load`, `structure.Routes.addItem` | `ProjectFileTests.D14_APartItsOwnerRefusesIsListedWithItsOwnersCodeAndNothingIsSaved` |
| AC-53.8, D6: an account of the actual balances the project lacks is `accountNotInProject`, and nothing is saved | `projectfile.Routes.load` | `ProjectFileTests.AC_53_8_D6_AnAccountOfTheActualBalancesTheProjectLacksIsRefusedAndNothingIsSaved` |
| AC-53.9: the route refuses a file that is not a project file, or is newer, and nothing changes | `projectfile.read`, `projectfile.Routes.load` | `ProjectFileTests.AC_53_9_TheRouteRefusesAFileThatIsNotAProjectFileOrIsNewerAndNothingChanges` |
| AC-29.3: a file whose report the project already issued is `reportAlreadyIssued`, listed before anything is saved; one whose number it has not issued loads beside the reports before it | `projectfile.Routes.load` | `ProjectFileTests.AC_29_3_AFileWhoseReportTheProjectAlreadyIssuedIsRefusedBeforeAnythingIsSaved`, `AC_29_3_AFileWhoseReportNumberTheProjectHasNotIssuedLoadsBesideTheReportsBeforeIt` |
