[Documentation](../README.md) › UC-071

# UC-071: Load a project file

**What it is for:** bringing a project that lived in Excel into REDIX in one step (FR-53, BR-43): its manager picks the project's JSON file, and REDIX adds the tables it names, records and approves every row through the same rules as Add entry, enters the actual balances and issues the file's report. One refused row, and nothing is saved.
**Who uses it:** the manager who created the project, who holds `ManageProject` (D1), into an approved project that holds no entry yet (D4).
**Issue:** redix-api #132 "Load a project's JSON file into REDIX: its structure, entries, actual balances and last report"; the file's shape is redix-api #130's.

**Built so far:** the file's reader (`Import/ProjectFile`). The route that loads it, `POST /projects/{id}/project-file`, is #132's plan step 3.

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

| Rule | Code | Test |
| --- | --- | --- |
| AC-53.9: a file that is not a REDIX project file, or is not laid out as one, is refused with `notAProjectFile`, and nothing is read | `ProjectFile.Read` | `ProjectFileTests.AC_53_9_AFileThatIsNotARedixProjectFileIsRefusedAndNothingIsRead` |
| AC-53.9: a file of a newer version is refused with `newerVersion` before its shape is read | `ProjectFile.Read` | `ProjectFileTests.AC_53_9_AFileOfANewerVersionIsRefusedBeforeItsShapeIsRead` |
| A project file is read with its report, its actual balances, its lists and its tables | `ProjectFile.Read` | `ProjectFileTests.AC_53_9_AProjectFileIsReadWithItsReportItsActualBalancesItsListsAndItsTables` |
| D2, D9: a file with no report, no accounts and no rows is its structure alone | `ProjectFile.Read` | `ProjectFileTests.D9_D2_AFileWithNoReportNoAccountsAndNoRowsIsReadAsItsStructureAlone` |
