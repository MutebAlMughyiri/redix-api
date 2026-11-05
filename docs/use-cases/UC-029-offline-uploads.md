[Documentation](../README.md) › [Use cases](../README.md) › UC-029

# UC-029: Accept offline uploads exactly once

**Purpose:** an entry saved on a phone with no network lands once, however many times the upload is cut and sent again.

**Who uses it:** the app, on its own.

| Route | What it answers |
| --- | --- |
| `POST /projects/{id}/expenses`, `/contractor-payments` | An id its author already recorded in that project answers `200` with where the entry stands, and changes nothing; from anyone else `409 entryExists` |
| `POST /projects/{id}/attachments` | The same file from the same person in that project answers `200` with the one already kept |

| Rule | Code | Test |
| --- | --- | --- |
| AC-16.3: an upload sent again does not repeat the entry | `EntryRecording.RepeatOfAsync` | `ExpensesTests.AC_16_3_AnUploadSentAgainLandsOnceAndAnswersTheEntry`, `ContractorPaymentsTests.AC_16_3_APaymentSentAgainLandsOnce` |
| Someone else's id is refused, and tells them nothing of it | `EntryRecording.RefusalOfIdAsync` | `ExpensesTests.AnIdSomeoneElseUsedIsRefusedAndTellsThemNothingOfIt` |
| A file sent again is kept once | `AttachmentsEndpoints.Upload` | `AttachmentsTests.TheSameFileSentAgainIsKeptOnce` |
