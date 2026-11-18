[Documentation](../README.md) › UC-073

# UC-073: Archive a project and retrieve it

**What it is for:** keeping the whole project in one file and putting it back. «أرشفة المشروع» hands over every row of the project with its id and times, its History among them; «استرجاع المشروع» makes the project what such a file holds, after showing what will change.
**Who uses it:** whoever holds `ManageProjectFiles` («ملفات المشروع») in the project, which every ManageProject holder was given by migration 00009 and whoever registers or starts a project holds (D2).
**Issue:** redix-api #130 "UC-073: Archive a project to one file, and retrieve it over the project"; it replaced #115 and #114, the export and import of settings. UC-073, since the app's UC-072 is the launcher's. The design: https://claude.ai/artifact/PriQGhLBP9V2437o8Fcu9n (private to Muteb's Claude account).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/archive/json` | `ManageProjectFiles` | `200` the archive, `application/json`; a History line, Archive, in the taker's name; `403` |
| `POST /projects/{projectId}/retrieve?preview=true`, the archive as a form's `file` | `ManageProjectFiles` | `200` `{ archivedAt, archivedBy, now, after }`, each of `now` and `after` counting `entries`, `contracts`, `accounts`, `suppliers`, `partners`, `items` and `attachments`; the refusals below; nothing saved |
| `POST /projects/{projectId}/retrieve` | `ManageProjectFiles` | `200` `{ archivedAt, after }`; `400` `notAnArchive`, `newerArchive`; `409` `archiveOfAnotherProject` with the archive's project under `project`, `archiveDoesNotFit`, `projectFinished`; `403` |

## The file

One JSON file. The app names it «{project} - أرشيف {yyyy-MM-dd HH.mm}.json» (`projects.archiveFileName`).

```
{ "format": "redix.archive", "version": 1, "release": 9,
  "project": { "id": "…", "name": "غصون 6" },
  "archivedAt": "2026-11-18T18:15:04.1234567Z", "archivedBy": { "id": "…", "name": "متعب" },
  "tables": { "entries": [ { "id": "…", "number": 1056, "amount": 115000, "created_at": "…", … } ],
              "attachments": [ { "id": "…", "file_name": "فاتورة.jpg", "storage_path": "<project id>/<attachment id>.jpg", "sha256": "…", … } ],
              …, "history": [ … ] } }
```

`release` is the migration the database stood at. Each table is listed by its own name, each row by its own column names, each value as stored: text as text, whole numbers as whole numbers, money in halalas, a column holding JSON as its text (D4). A bill or a PDF is its attachment's row, its file's name and storage path; the file itself stays in the storage whose base the server's configuration names (`Attachments__Folder`, a bucket's address once files move there), and is never inside the archive (Muteb, 18 Nov 2026).

## What a retrieve replaces

`archive.tables` lists every table holding a project's rows, each after the tables its rows name, with the condition finding the project's rows in it. A retrieve empties the retrieved ones in the reverse order, then fills them in this one, in one transaction (D5).

| Group | Tables | Retrieve |
| --- | --- | --- |
| The project's own | `units`, `categories`, `items`, `subgroups`, `item_columns`, `default_columns`, `accounts`, `account_custodians`, `payment_methods`, `payment_method_accounts`, `suppliers`, `supplier_default_items`, `partners`, `partner_users`, `chosen_partners`, `contracts`, `unseen_groups`, `unseen_contracts`, `attachments`, `contract_attachments`, `documents`, `entries`, `expense_details`, `purchase_details`, `revenue_details`, `contractor_payment_details`, `capital_movement_details`, `invoices`, `entry_attachments`, `entry_decisions`, `transfers`, `reconciliations`, `reconciliation_balances`, `notifications`, `notification_entries`: 35 | Replaced, each row as it was |
| What happened | `history`, `periodic_reports` | Stay: History is never deleted (AC-21.1), an issued report is on paper |
| The project and who belongs | `projects` with its entry counter, `project_time_zones`, `project_structure_sources`, `project_permissions`, `project_role_names`, `inactive_members`, `permission_requests` | Stay: a removed member is never let back in, the person retrieving keeps their access, no entry number is given twice |
| Shared | `users`, `devices`, `email_codes`, `system_permissions`, `user_default_projects`, `config_values`, `translation_keys`, `translations`, `goose_db_version`, `sqlite_sequence`, the lookups | Never in an archive |

## How it retrieves

1. The file is read: one that is not JSON, or names another format or version, is `notAnArchive`; an archive of another project is `archiveOfAnotherProject`; one of a newer release than the server's is `newerArchive`.
2. A scratch database in memory is made at the archive's release by the server's own migrations, checking no foreign key, and filled with the archive's rows; a table, a column or a value its release does not hold is `notAnArchive`. The migrations still to run then bring it, rows and all, to the server's release; rows they cannot bring up are `archiveDoesNotFit` (D7).
3. A preview answers here.
4. One transaction empties the 35 tables of the project's rows and fills them with the archive's, only names the schema gives reaching a statement; a row naming what this server lacks, such as a person, is `archiveDoesNotFit`, and nothing is saved. A History line, Retrieve, names when the archive was taken and by whom (`ArchivedAt`, `ArchivedByUserId`). The History net lets the rows of a retrieve pass under it (D9).
5. No file is written or removed: the storage keeps every file an archive may name.

«سجل المشروع» shows the two lines under «المشروع»: `archived`, and `retrieved` with the archive's moment under `about`.

## Rules

| Rule | Where | Proven by |
| --- | --- | --- |
| AC-73.1: an archive is one JSON file of every row of the project's 44 tables as stored, a bill or a PDF as its attachment's row; taking it writes an Archive line in the taker's name | `archive.Routes.archive` | `TestAC73_1_AnArchiveHoldsEveryRowOfTheProjectAndNamesItsFilesAndWhoTookIt` |
| AC-73.2: a preview answers when and by whom the archive was taken and seven counts now and after, and changes nothing | `archive.Routes.retrieve` | `TestAC73_2_APreviewCountsNowAndAfterAndChangesNothing` |
| AC-73.3, AC-73.4, AC-73.9: a retrieve makes the 35 tables the archive's again, and keeps History, the issued reports, the entry counter and the members; «سجل المشروع» says it was retrieved | `archive.override`, `archive.tables` | `TestAC73_9_RetrievedTheProjectIsItsArchiveAgainWhileHistoryReportsAndMembersStay` |
| AC-73.5: a file that is no archive, of a newer release, or naming a person the server lacks is refused, and nothing is saved | `archive.opened`, `archive.broughtUp`, `archive.override` | `TestAC73_5_AnArchiveThatCannotRetrieveIsRefusedAndNothingIsSaved` |
| AC-73.5, R12: another project's archive is refused, naming that project | `archive.Routes.retrieve` | `TestAC73_5_AnotherProjectsArchiveIsRefusedNamingIt` |
| AC-73.5, R17: a finished project is archived, never retrieved | `archive.Map` | `TestAC73_5_AFinishedProjectIsArchivedNeverRetrieved` |
| AC-73.6: an archive of an earlier release is brought up by the server's migrations, and retrieves | `db.OpenScratch`, `archive.broughtUp` | `TestAC73_6_AnArchiveOfAnEarlierReleaseIsBroughtUp` |
| AC-73.7: every table of the database is the project's own, kept, or shared with its reason | `archive.tables`, `archive.shared` | `TestAC73_7_EveryTableOfTheDatabaseIsTheProjectsKeptOrShared` |
| AC-73.8: without `ManageProjectFiles` neither the archive, the retrieve nor UC-071's load answers | `archive.Map`, `projectfile.Map` | `TestAC73_8_WithoutManageProjectFilesNoRouteOfTheProjectsFilesAnswers` |
| redix-api #141 D1: the database failing at any call of the archive or the retrieve answers 500 and changes nothing | `archive` | `TestAFailingDatabaseAtAnyCallOfTheArchiveOrTheRetrieveAnswers500AndChangesNothing` |
