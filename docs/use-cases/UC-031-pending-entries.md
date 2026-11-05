[Documentation](../README.md) › [Use cases](../README.md) › UC-031

# UC-031: List pending entries with their invoices

**Purpose:** Finance's inbox of entries waiting for a decision.

**Who uses it:** whoever holds `ApproveEntries`, which Finance does.

| Route | What it answers |
| --- | --- |
| `GET /projects/{id}/entries/pending` | What waits, oldest first: its item, unit, supplier, invoice number, VAT, account, notes and author, with its file or why it has none |

| Rule | Code | Test |
| --- | --- | --- |
| AC-18.1: oldest first, with the file or why there is none | `ApprovalsEndpoints.Pending` | `ApprovalsTests.AC_18_1_TheInboxIsOldestFirstWithTheFileOrWhyThereIsNone` |
| A draft and what is decided are not in it | `ApprovalsEndpoints.Pending` | `ApprovalsTests.ADraftAndWhatIsDecidedAreNotInTheInbox` |
| Only whoever decides entries sees it | The `ApproveEntries` policy | `ApprovalsTests.OnlyWhoeverDecidesEntriesSeesTheInboxOrDecides` |
