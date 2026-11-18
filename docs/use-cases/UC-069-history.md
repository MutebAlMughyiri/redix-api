[Documentation](../README.md) › UC-069

# UC-069: Read the history

**What it is for:** knowing who did what in a project, and when (FR-21), and each person knowing what was done to their own login.
**Who uses it:** «سجل المشروع» whoever holds `SeeProjectHistory` («يرى سجل المشروع»), which whoever registers or starts a project holds; «سجلي» everyone signed in.
**Issue:** redix-api #86; the app's screens are «سجل المشروع» in Project settings and «سجلي» in Shared settings. Writing the history is UC-005.

## Endpoints

| Route | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{id}/history?area=&by=&before=&text=` | `SeeProjectHistory` | A page of the project's actions, newest first, 50 at a time |
| `GET /me/history?area=&by=&before=&text=` | A session whose password is changed | A page of the caller's own login's lines |

`area` keeps one kind (any other text, every kind), `by` one person's lines, `before` the lines older than the one named, `text` those holding what was typed. The accounts', partners' and approvals' histories take the same `by`, `before` and `text`.

A line of the project history is what was done (`action`), to what (`thing`), the record's `name`, what else it names (`about`), its old name after a rename (`nameWas`), the accounts money went `from` and `to`, and its `amount` (0 for none). A line of «سجلي» is what happened to the login (`kind`), and the name, username, mobile and email a change of it gave (`about`).

## One request is one line

A request often writes several History lines: an expense its entry, its details and its file; an item its subgroup and columns. Each line keeps the request's trace id (UC-005), and the project history shows a request once, through the line of the record it acted on:

| Record (History's table) | Thing | Kind (`area`) | Shown through, when the same request wrote it | Seen by |
| --- | --- | --- | --- | --- |
| Entries | `entry` القيد | `entries` | - | Whoever the entries screens show the entry to: never someone else's draft, capital only for a partner one sees (BR-31); one deleted since, whoever recorded it |
| ExpenseDetails, PurchaseDetails, ContractorPaymentDetails, RevenueDetails, CapitalMovementDetails | `entry` | `entries` | Entries | As its entry |
| Invoices | `invoice` الفاتورة | `entries` | Entries | As its entry |
| EntryAttachments | `file` الملف | `entries` | Entries | As its entry |
| Attachments | `file` | `entries` | Always: a file shows through what it is attached to | - |
| Accounts, Transfers, Reconciliations, PaymentMethods | `account`, `transfer`, `reconciliation`, `paymentMethod` | `accounts` | - | Everyone holding the permission |
| AccountCustodians | `custody` العهدة | `accounts` | Accounts | Everyone holding the permission |
| Partners | `partner` الشريك | `partners` | - | Whoever sees that partner's capital (BR-31) |
| PartnerUsers | `partnerLogin` | `partners` | Partners | As its partner |
| Suppliers, Contracts | `supplier`, `contract` | `suppliers` | - | Everyone holding the permission |
| SupplierDefaultItems | `supplier`, always `changed` | `suppliers` | Suppliers | Everyone holding the permission |
| ContractAttachments | `contractDocument` | `suppliers` | Contracts | Everyone holding the permission |
| Categories, Items, DefaultColumns | `category`, `item`, `defaultColumn` | `structure` | - | Everyone holding the permission |
| Subgroups | `subgroup` | `structure` | Items | Everyone holding the permission |
| ItemColumns | `column` | `structure` | Items or DefaultColumns | Everyone holding the permission |
| Projects, Units, ProjectTimeZones, Documents | `project`, `unit`, `timeZone`, `document` | `project` | - | Everyone holding the permission |
| ProjectStructureSources | `project` | `project` | Projects | Everyone holding the permission |
| PeriodicReports | `periodicReport`, created as `issued` | `project` | - | `SeeReports` |
| ProjectPermissions | `permission` الصلاحية | `people` | Projects | `ManageUsers` |
| InactiveMembers, ProjectRoleNames, ChosenPartners | `person`, `roleName`, `partnerSight` | `people` | ProjectPermissions | `ManageUsers` |
| Users | - | - | Never in a project's history: «سجلي» reads them | The person whose login it is |

An entry's Create is said as `drafted`; every other action as `actionOf` says it: `created`, `changed`, `deleted`, `disabled`, `enabled`, `sent`, `approved` (an entry's, a project's, and an old reversal's), `rejected`, `restored`, `registered`, `closed`, `attached`, `given`, `taken`, and a project's `archived` and `retrieved`, the archive's moment under `about` (UC-073).

## Rules and where they live

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| R1 «سجل المشروع» needs `SeeProjectHistory`; whoever registers or starts a project holds it | `gate.Needs`, `users.TickStarter` | `TestR1_TheProjectHistoryOpensBySeeProjectHistoryWhichAProjectsStarterHolds` |
| R2 One request is one line: a part whose owner wrote in the same request shows through it; a file never alone; every table is a thing or says why it shows nowhere | `entries.shown`, `entries.things` | `TestR2_OneRequestIsOneLineOfTheRecordItActedOn`, `TestR2_EveryTableIsAThingOfTheProjectHistoryOrSaysWhyNot` |
| R3 A line shows only to whoever may see its record | `things[...].seen` | `TestR3_ALineShowsOnlyToWhoeverMaySeeItsRecord` |
| R4 A line names its record from History's own values, so a deleted record keeps its name | `entries.knowledgeOf` | `TestR4_ALineNamesItsRecordEvenAfterItIsDeleted` |
| R5 Newest first, 50 at a time, filtered by kind and person, searched by its words | `entries.page` | `TestR5_AHistoryIsFilteredByKindAndPersonAndSearched` |
| R6 «سجلي» reads the lines of the caller's own login alone, never a hash, a code or a token | `entries.myHistory`, the query `HistoryOfLogin` | `TestR6_MyHistoryIsMyOwnLoginsLinesAlone` |
| R7 Every line keeps the trace id of the request that wrote it; a new permission takes no older one's number | `history.Write`, `access.All` | `TestR7_EveryLineKeepsTheTraceIdOfTheRequestThatWroteIt`, `TestR7_ANewPermissionTakesNoOlderPermissionsNumber` |
