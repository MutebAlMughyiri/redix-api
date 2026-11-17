[Documentation](../README.md) › [Use cases](../README.md) › UC-030

# UC-030: Send changes since the last sync

**Purpose:** a phone keeps what it needs to enter an entry with no network, and learns what changed.

**Who uses it:** the app, for anyone who reads the project.

| Route | What it answers |
| --- | --- |
| `GET /projects/{id}/sync?since={stamp}` | A stamp, and every list by the address the app reads it at. The current stamp answers no lists |
| `GET /projects/{id}/entries/mine` | Where the caller's own entries stand, newest first |

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A first sync answers every list | `sync.read` | `SyncTests.AFirstSyncAnswersEveryListAtTheAddressTheAppReadsItAt` |
| A kept list is what its own read answers | `sync.read` | `SyncTests.EachListHoldsWhatItsOwnReadAnswers` |
| Nothing changed answers nothing | `sync.Map` | `SyncTests.NothingChangedAnswersNoLists` |
| AC-05.1: an item Finance added reaches the field at the next sync, in its offered lists and in Add entry's table list (`entry-tables`), with no online visit to Add entry first | `sync.read` | `SyncTests.AC_05_1_AnItemFinanceAddedReachesTheFieldAtTheNextSync` |
| A row leaving a list changes the stamp too | `sync.Map` | `SyncTests.ARowThatLeftAListChangesTheStampToo` |
| The field keeps only the accounts they may charge | `accounts.Routes.list` | `SyncTests.TheFieldKeepsOnlyTheAccountsTheyMayCharge` |
| Each person keeps only their own entries' status | `entries.Routes.mine` | `SyncTests.EachPersonKeepsOnlyTheStatusOfTheirOwnEntries` |
| Someone outside the project gets nothing | The `ReadProject` policy | `SyncTests.SomeoneOutsideTheProjectGetsNothing` |
