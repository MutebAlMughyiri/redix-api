[Documentation](../README.md) › [Use cases](../README.md) › UC-029

# UC-029: Accept offline uploads exactly once

**Purpose:** an entry saved on a phone with no network lands once, however many times the upload is cut and sent again.

**Who uses it:** the app, on its own.

| Route | What it answers |
| --- | --- |
| `POST /projects/{id}/entries`, for every table (redix-api #110) | An id its author already recorded in that project answers `200` with where the entry stands, and changes nothing; from anyone else `409 entryExists` |
| `POST /projects/{id}/attachments` | The same file from the same person in that project answers `200` with the one already kept |

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-16.3: an upload sent again does not repeat the entry | `entries.Routes.record` | `ExpensesTests.AC_16_3_AnUploadSentAgainLandsOnceAndAnswersTheEntry`, `ContractorPaymentsTests.AC_16_3_APaymentSentAgainLandsOnce` |
| Someone else's id is refused, and tells them nothing of it | `entries.Routes.record` | `ExpensesTests.AnIdSomeoneElseUsedIsRefusedAndTellsThemNothingOfIt` |
| A file sent again is kept once | `attachments.Routes.upload` | `AttachmentsTests.TheSameFileSentAgainIsKeptOnce` |
