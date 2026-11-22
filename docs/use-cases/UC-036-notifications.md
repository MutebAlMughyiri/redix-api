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

## On the web: Notifications (redix-api #175)

The web's Notifications is the app's bell and screen: Notifications in the top bar, counting what is unread beside it once the shell is drawn (D2), opens a tab, with or without a project, listing what the person was told from `notifications.ListOf`, the function `GET /notifications` answers: newest first, each in the app's words, bold while unread, its project and its day in the project's time, searched by words and project as the app's list is. A row opens what it is about, as the app's row does, and marks it read through `notifications.MarkRead`, the rule `POST /notifications/{id}/read` runs: its project chosen and, in the whole shell, Approvals for an entry sent, Permission requests for a request, My permissions for one decided, Home for the project itself, and Entries with the entry in the details panel for any other; one about a project the person may no longer open stays on Notifications, read. Its pages are in `go-backend/web` (`notifications.go`, `notifications.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/notifications?text=` | Notifications, searched | `200` the tab's content |
| `GET /notifications/count` | The top bar's count of what is unread | `200` the count, hidden at none |
| `GET /notification/{id}/open` | Opens a notification, marked read | `200` the whole shell (`HX-Retarget`) with what it is about in front, or Notifications again; `404` for one not the person's |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Notifications lists what the person was told as the app's screen, counts the unread in the top bar, and opening one marks it read and brings what it is about in front | `notifications.ListOf`, `notifications.MarkRead`, `web.noticeTabs` | `TestWeb_R175_UC036_NotificationsListWhatThePersonWasToldAsTheAppsScreen` |
| A notification about an entry opens Entries with the entry in the details panel | `entries.RowOf`, `web.page.details` | `TestWeb_R175_UC036_AnEntrysNotificationOpensItBesideEntries` |
| A failing database answers 500 | `web.site.openNotification` | `TestWeb_R175_NotificationsAnswerAFailingDatabase` |
