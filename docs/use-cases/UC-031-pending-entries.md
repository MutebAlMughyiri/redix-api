[Documentation](../README.md) › [Use cases](../README.md) › UC-031

# UC-031: List pending entries with their invoices

**Purpose:** Finance's inbox of entries waiting for a decision.

**Who uses it:** whoever holds `ApproveEntries`, which Finance does.

| Route | What it answers |
| --- | --- |
| `GET /projects/{id}/entries/pending` | What waits, oldest first: its `number`, «رقم القيد» (FR-54, redix-api #128), its date, total, item, supplier, an older bill's number, account, description and author, with its file when it has one |
| `GET /projects/{id}/approvals/history` | What was decided, newest first, 50 a page: each entry approved or rejected, by whom and when, a rejection with its reason; `kind`, `mine`, `text` and `before` as the other histories take them (Muteb, 12 Nov 2026) |

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-18.1: oldest first, with the file when there is one | `entries.Routes.pending` | `ApprovalsTests.AC_18_1_TheInboxIsOldestFirstWithTheFileWhenThereIsOne` |
| A draft and what is decided are not in it | `entries.Routes.pending` | `ApprovalsTests.ADraftAndWhatIsDecidedAreNotInTheInbox` |
| AC-54.4 (redix-api #128): each row carries its entry's own «رقم القيد», never its place in the inbox | `entries.Routes.pending`, `entries.facts.pending` | `EntryNumberTests.AC_54_4_FinancesInboxLeadsEachWaitingEntryWithItsNumber`, `AC_54_4_TheInboxShowsTheEntrysOwnNumberNotItsPlaceInTheInbox` |
| Only whoever decides entries sees it | The `ApproveEntries` policy | `ApprovalsTests.OnlyWhoeverDecidesEntriesSeesTheInboxOrDecides` |
