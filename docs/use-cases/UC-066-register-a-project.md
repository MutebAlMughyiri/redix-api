[Documentation](../README.md) › UC-066

# UC-066: Register a project, and approve it

**What it is for:** letting a new project in, with its own project manager, once an admin has approved it (FR-52, BR-29, BR-34).
**Who uses it:** anyone registers, from the sign-in page or from inside the app. Only the admin approves, and that is all the admin does.
**Issue:** redix-api #83; the app side is redix-app #79.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `POST /projects/register` | Nothing; limited per address | `201` the project; `400` `invalidName`, `unitNameTaken`, `invalidUsername`, `invalidMobile` or `weakPassword`; `409` `usernameTaken`; `429` |
| `POST /projects` | Any session | `201` the project, with the caller as manager; `400` `invalidName` |
| `GET /projects/registered` | `ApproveProjects` | `200` what is waiting, by start date, with the manager's details |
| `POST /projects/{projectId}/approve` | `ApproveProjects` | `200`, and the project's structure is copied (UC-012); `404`; `409` `notPending` |
| `POST /projects/{projectId}/reject` with `{ reason }` | `ApproveProjects` | `200`; `400` `invalidReason`; `404`; `409` `notPending` |

Both `POST /projects/register` and `POST /projects` take `partners`, the names of its partners in order, left out for none: each becomes a table of its رأس المال once it is approved (redix-api #109). A blank or overlong name answers `invalidName`, and nothing is kept.

A project answers `status`, now also `pendingApproval` or `rejected`, and `rejectionReason`.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| A registered project has its manager, all or nothing | `projects.Routes.register` | `RegistrationTests.ARegisteredProjectHasItsManager`, `ABadFieldCreatesNothing` |
| Its declared partners are kept in their order with it, all or nothing | `projects.declaredPartners` | `RegistrationTests.R109_TheDeclaredPartnersAreKeptInTheirOrderAndABlankOneCreatesNothing` |
| Someone signed in registers with their own login | `projects.Routes.create` | `RegistrationTests.SomeoneSignedInRegistersWithTheirOwnLogin` |
| A waiting project gives no permission (BR-34) | `access.Book.Of` | `RegistrationTests.AWaitingProjectGivesNoPermission` |
| Only the admin approves, and sees what is needed to decide | `access.ApproveProjects` | `RegistrationTests.OnlyTheAdminApproves`, `TheAdminSeesWhatIsNeededToDecide` |
| The admin sees nothing inside a project (BR-29) | `ProjectPermission` rows | `RegistrationTests.TheAdminSeesNothingInsideAProject` |
| A rejection says why | `projects.Routes.reject` | `RegistrationTests.ARejectionSaysWhy` |
| A project is decided once | `projects.Routes.approve`, `projects.Routes.reject` | `RegistrationTests.AProjectIsDecidedOnce` |
| A project manager cannot take over the admin | `users.Routes.mayChange`, `users.Routes.addExisting` | `ProjectUsersTests.AManagerCannotTakeOverTheAdmin` |
| Too many tries from one address are refused | `limits.PerHour`, `Window.PerAddress` | `RegistrationTests.TooManyTriesAreRefused` |
| Every registration and decision is in the History table | `history.Write` | `RegistrationTests.EveryDecisionIsInTheHistory` |

## On the web (redix-api #164)

The sign-in's «تسجيل مشروع جديد» opens the same registration in the sign-in's place: the project, its units and partners added one at a time, and the manager's own login. Sending calls `projects.Register`, the rule `POST /projects/register` calls, and counts in the same window per address, so asking at the web does not reset the app's count. Its pages are in `go-backend/web` (`register.go`, `register.templ`).

Signed in, the project menu's «تسجيل مشروع جديد» opens the same form in a tab of its own, without the manager's login - the person registering is its manager - and with «نسخ التصنيفات والبنود من» one of the projects they open (UC-006, UC-012). The same `POST /register` answers by the session: signed in, it calls `projects.Create`, the rule `POST /projects` calls, with no window per address, as that route has none.

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /register` | The form, in the sign-in's place | `200` the form |
| `POST /register/{list}` | Adds the unit or partner typed (`unitName`, `partnerName`), or removes the one at `remove`, among those sent (`unit`, `partner`) | `200` the list alone; `404` for a list the form has not |
| `POST /register` | Sends the project for approval | Signed out: `200` the sign-in form saying «أُرسل للاعتماد. سجّل الدخول لمتابعته.»; `422` the form with the refusal's words. Signed in: `200` the form emptied, saying the project waits for the admin, the project menu beside it holding it; or as typed with the refusal's words |
| `GET /tab/registerProject` | The form signed in, in its tab | `200` the tab's content |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| The sign-in's link opens the form; Cancel goes back | `web.signInForm`, `web.site.registerForm` | `TestWeb_R164_TheSignInsRegisterLinkOpensItsFormAndCancelGoesBack` |
| A registration waits for the admin with its units, and its manager signs in with the password they chose (UC-066) | `projects.Register` | `TestWeb_R164_UC066_ARegistrationWaitsForTheAdminAndTheSignInSaysSo` |
| One address registers only so often an hour, the app's registrations counted with the web's; the one past it keeps nothing | `limits.Window.AllowsCaller` | `TestWeb_R290_ForgotPasswordAndRegisterShareTheAppsPerAddressWindows` |
| A refusal answers the app's words and keeps what was typed but the password | `web.site.register` | `TestWeb_R164_ARefusalAnswersTheFormWithTheAppsWordsAndKeepsWhatWasTyped` |
| Add and Remove answer the list alone; a blank is not added | `web.site.listed` | `TestWeb_R164_AddAndRemoveAnswerTheListAlone` |
| Signed in, a person registers a project they manage, its structure copied from a project they open; a name or a source they may not read is refused in the app's words (UC-006) | `projects.Create` | `TestWeb_R164_UC006_ASignedInPersonRegistersAProjectTheyManage` |
