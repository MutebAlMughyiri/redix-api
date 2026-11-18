[Documentation](../README.md) › [Use cases](../README.md) › UC-036

# UC-036: Tell people what happened

**Purpose:** whoever decides entries learns that one waits, and whoever entered it learns the decision. A manager learns what the admin decided about their project.

**Who uses it:** everyone signed in, each reading only their own.

| Route | What it does |
| --- | --- |
| `GET /notifications` | What the caller was told, the latest 100, newest first, about projects they still have |
| `POST /notifications/{id}/read` | Marks one of theirs as seen |

A notification holds a kind, not text, so its words come from the app in the reader's language. `entryId` is empty when it is about the project itself: only a notification about an entry has a row of `notification_entries`, so the database refuses one naming an entry that is not (findings row 214).

**Not built:** sending to the phone through Firebase or Huawei Push Kit, which needs those accounts. Until then the phone learns when it reads this list, so AC-20.1's minute holds only once push exists. Findings row 114 "UC-036, push to a closed phone" says what is missing.

Each test named `Class.Name` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/Class.Name`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| AC-20.1: sending an entry tells whoever decides, and not who sent it | `entries.tell` | `NotificationsTests.AC_20_1_SendingAnEntryTellsWhoeverDecidesAndNotWhoSentIt` |
| A draft tells nobody; sending it later does | `EntryRecording`, `entries.Routes.submit` | `NotificationsTests.SavingADraftTellsNobody`, `SendingADraftLaterTellsWhoeverDecides` |
| AC-20.1: a decision tells the entry's author | `entries.tell` | `NotificationsTests.AC_20_1_ApprovingTellsItsAuthor`, `AC_20_1_RejectingTellsItsAuthor` |
| Nobody is told of their own decision | `entries.tell` | `NotificationsTests.NobodyIsToldOfTheirOwnDecision` |
| The admin's decision tells the project's manager | `notifications.TellManagers` | `NotificationsTests.TheAdminsDecisionTellsTheManagerOfTheProject` |
| Only your own can be marked seen | `notifications.Map` | `NotificationsTests.MarkingOneReadChangesOnlyYourOwn` |
| Deleting an entry takes what was told about it | `entries.Delete` | `NotificationsTests.DeletingADraftTakesWhatWasToldAboutIt` |
| Someone who left a project no longer sees it (BR-33) | `notifications.Map` | `NotificationsTests.SomeoneWhoLeftAProjectIsNoLongerToldOfIt` |
| Nobody reads them without signing in | The fallback policy | `NotificationsTests.NobodyReadsThemWithoutSigningIn` |
| A notification about an entry names it in a row of its own, and one about the project has none; the database refuses an entry that is not (findings row 214) | `entries.tell`, `notification_entries` | `NotificationsTests.R214_ANotificationAboutAnEntryNamesItInARowOfItsOwnAndOneAboutTheProjectHasNone`, `R214_TheDatabaseRefusesANotificationNamingAnEntryThatIsNot` |
| A notification names a project the database knows (findings row 213) | The foreign key `notifications.project_id` → `projects.id`, `db/migrations/00001_schema.sql` | `NotificationsTests.R213_TheDatabaseKeepsANotificationNamingItsProject`, `R213_TheDatabaseRefusesANotificationNamingNoProject`, `MigrationStepsTests.R213_NotificationProjectKey_EveryNotificationKeepsItsProjectWhichTheDatabaseNowNames` |
