[Documentation](../README.md) › [Use cases](../README.md) › UC-036

# UC-036: Tell people what happened

**Purpose:** whoever decides entries learns that one waits, and whoever entered it learns the decision. A manager learns what the admin decided about their project.

**Who uses it:** everyone signed in, each reading only their own.

| Route | What it does |
| --- | --- |
| `GET /notifications` | What the caller was told, the latest 100, newest first, about projects they still have |
| `POST /notifications/{id}/read` | Marks one of theirs as seen |

A notification holds a kind, not text, so its words come from the app in the reader's language. `EntryId` is empty when it is about the project itself.

**Not built:** sending to the phone through Firebase or Huawei Push Kit, which needs those accounts. Until then the phone learns when it reads this list, so AC-20.1's minute holds only once push exists. Findings row 114 "UC-036, push to a closed phone" says what is missing.

| Rule | Code | Test |
| --- | --- | --- |
| AC-20.1: sending an entry tells whoever decides, and not who sent it | `NotificationsEndpoints.TellDecidersAsync` | `NotificationsTests.AC_20_1_SendingAnEntryTellsWhoeverDecidesAndNotWhoSentIt` |
| A draft tells nobody; sending it later does | `EntryRecording`, `EntriesEndpoints.Submit` | `NotificationsTests.SavingADraftTellsNobody`, `SendingADraftLaterTellsWhoeverDecides` |
| AC-20.1: a decision tells the entry's author | `NotificationsEndpoints.TellAuthor` | `NotificationsTests.AC_20_1_ApprovingTellsItsAuthor`, `AC_20_1_RejectingTellsItsAuthor` |
| Nobody is told of their own decision | `NotificationsEndpoints.TellAuthor` | `NotificationsTests.NobodyIsToldOfTheirOwnDecision` |
| The admin's decision tells the project's manager | `NotificationsEndpoints.TellManagersAsync` | `NotificationsTests.TheAdminsDecisionTellsTheManagerOfTheProject` |
| Only your own can be marked seen | `NotificationsEndpoints.MarkRead` | `NotificationsTests.MarkingOneReadChangesOnlyYourOwn` |
| Deleting an entry takes what was told about it | `EntryChanges.DeleteAsync` | `NotificationsTests.DeletingADraftTakesWhatWasToldAboutIt` |
| Someone who left a project no longer sees it (BR-33) | `NotificationsEndpoints.List` | `NotificationsTests.SomeoneWhoLeftAProjectIsNoLongerToldOfIt` |
| Nobody reads them without signing in | The fallback policy | `NotificationsTests.NobodyReadsThemWithoutSigningIn` |
| A notification names a project the database knows (findings row 213) | `RedixDbContext`, `FK_Notifications_Projects_ProjectId` | `NotificationsTests.R213_TheDatabaseKeepsANotificationNamingItsProject`, `R213_TheDatabaseRefusesANotificationNamingNoProject`, `MigrationStepsTests.R213_NotificationProjectKey_EveryNotificationKeepsItsProjectWhichTheDatabaseNowNames` |
