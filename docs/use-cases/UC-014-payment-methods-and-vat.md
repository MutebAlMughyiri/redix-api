[Documentation](../README.md) › UC-014

# UC-014: Manage payment methods

**What it is for:** keeping the list of payment methods entries choose from (FR-40).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Everyone with access reads them.
**Issue:** redix-api #32; the app side is redix-app #33.

## How it fits together

The seeded template project holds three payment methods: شبكة, تحويل and كاش. FR-40 named a fourth, سداد, which went on 7 November 2026 (redix-api #96, D25). When a project is approved it takes its own copy of them, always from the template: what registration chooses is where the categories and items come from, and it says so on the form. So one project's change touches no other (BR-33).

Each method pays from the accounts its dialog ticks (redix-app #109): a method kept before that change, or added naming none, pays from every account of its project, and an account added later is paid from by every method of its project, until someone unticks it (D1b, the owner 17 Nov 2026). Add entry asks the method first and offers only the accounts it pays from.

A project has no VAT rate of its own any more. REDIX stores no VAT: the VAT statement's workbook works it out of each total at the rate in the config row `vatRatePercent` (UC-070, redix-api #96, D4, D18).

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/payment-methods` | `ReadProject` | `200` every method in order, disabled ones too, as `{ id, name, isActive, accountIds }` |
| `GET /projects/{projectId}/payment-methods/offered` | `ReadProject` | `200` what an entry may choose: the active ones |
| `POST /projects/{projectId}/payment-methods` with `{ name, accountIds }` | `ManageSettings` | `200`; `400` `invalidName`, `paysFromNothing` for an empty list, `unknownAccount` for an account the project does not hold. With no `accountIds` it pays from every account |
| `PUT /projects/{projectId}/payment-methods/{id}` with `{ name, accountIds }` | `ManageSettings` | `200`; `400` `invalidName`, `paysFromNothing`, `unknownAccount`; `404`. With no `accountIds` what it pays from stays |
| `POST /projects/{projectId}/payment-methods/{id}/disable` | `ManageSettings` | `200`; `404` |
| `POST /projects/{projectId}/payment-methods/{id}/enable` | `ManageSettings` | `200`; `404` |
| `DELETE /projects/{projectId}/payment-methods/{id}` | `ManageSettings` | `200`; `409` `paymentMethodUsed` when an expense was paid with it; `404` |

Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `PaymentMethods` | Each project's methods, and the template's seed |
| `payment_method_accounts` | The accounts each method pays from, one row a pair (redix-app #109) |

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/<Class>.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| An approved project starts with the three methods | `structure.Copy`, from the template's payment methods | `PaymentMethodsTests.AnApprovedProjectStartsWithTheThreeMethods` |
| They are rows of its own | `settings.AddPaymentMethod` | `PaymentMethodsTests.ProjectsTakeTheirOwnRowsOfTheTemplatesMethods` |
| A project copied from an earlier one still starts from the seed | `structure.Copy` | `PaymentMethodsTests.AProjectCopiedFromAnEarlierOneStillStartsFromTheSeed` |
| A method is added at the end, and renamed | `settings.Routes.add`, `Rename` | `PaymentMethodsTests.FinanceAddsAMethodAtTheEndOfTheList`, `FinanceRenamesAMethod` |
| A method needs a name of 1 to 100 characters | `settings.CreatePaymentMethod`, `settings.RenamePaymentMethod`, `projects.newProject` | `PaymentMethodsTests.AMethodNeedsANameOfAtMost100Characters` |
| A disabled method leaves entry and stays on the list (AC-40.1) | `settings.Routes.list`, `List` | `PaymentMethodsTests.ADisabledMethodLeavesEntryAndStaysOnTheList` |
| Enabling brings it back | `settings.Routes.activate` | `PaymentMethodsTests.EnablingBringsAMethodBackIntoEntry` |
| A method no expense was paid with is deleted; one an expense was paid with is refused and stays, to be disabled instead | `settings.Routes.delete` | `PaymentMethodsTests.FinanceDeletesAMethodNoExpenseWasPaidWith`, `AMethodAnExpenseWasPaidWithIsNotDeleted` |
| One project's methods change no other, nor the template (BR-33) | `access.Gate.Needs` | `PaymentMethodsTests.AChangeInOneProjectLeavesTheTemplateAndOtherProjectsAlone` |
| One project's route never reaches another's methods (BR-33) | `settings.Routes.change`, `PermissionHandler` | `PaymentMethodsTests.ARouteOfOneProjectNeverReachesAnothersMethods` |
| Everyone with access reads; only Finance changes | `access.ManageSettings` | `PaymentMethodsTests.EveryoneWithAccessReadsMethodsAndOnlyFinanceChangesThem` |
| A finished project refuses every change and stays readable (AC-04.1) | `access.Gate.Changes` | `PaymentMethodsTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| A method added after a delete goes last, and no two methods share a place | `settings.Routes.add` | `PaymentMethodsTests.AMethodAddedAfterADeleteGoesLastAndNoTwoMethodsShareAPlace` |
| A method pays from the accounts its dialog ticks, and a change to them is the method's History line (redix-app #109 R3) | `settings.setPaysFrom` | `settings_test.TestR3_AMethodPaysFromTheAccountsItsDialogTicks` |
| A method naming no accounts pays from every one, and an account added later is paid from by every method (R4, D1b); a deleted account or method takes its links with it | `settings.AddPaymentMethod`, `settings.TickOnEveryMethod`, `accounts.Routes.delete` | `settings_test.TestR4_D1b_EveryMethodPaysFromEveryAccountUntilOneIsUnticked` |
| A method pays from at least one of the project's accounts (R10) | `settings.setPaysFrom` | `settings_test.TestR10_AMethodPaysFromAtLeastOneOfTheProjectsAccounts` |

## On the web: Payment methods (redix-api #174)

The web's Payment methods is the app's screen in the shell, its item in the side menu for whoever manages the project's settings: New payment method, gray in a finished project; then each method, quiet while disabled, the accounts it pays from, and that it is disabled, from `settings.MethodsOf`, the function `GET …/payment-methods` answers, with the accounts from `accounts.ListOf`. A row opens the method in the details panel with Rename, Disable or Enable, and Delete asked first as the app's swipe asks. New payment method and Rename ask the name and tick the active accounts it pays from, every one for a new method, as the app's dialog does. Each keeps through the rule the app's route runs: `settings.CreatePaymentMethod`, `RenamePaymentMethod`, `ActivatePaymentMethod`, `DeletePaymentMethod`, run through the shared changes.Writer. Its pages are in `go-backend/web` (`methods.go`, `methods.templ`).

| Page | Purpose | Answers |
| --- | --- | --- |
| `GET /tab/paymentMethods` | Payment methods | `200` the tab's content; the item stays out of the menu without ManageSettings |
| `GET /details/payment-method/{id}` | A method in the details panel; `?asking=rename` or `delete`; `id` `new` for New payment method | `200` the panel or its form; `404` without ManageSettings, or for a method the project does not hold |
| `POST /payment-method/new/add`, `…/{id}/rename`, `…/disable`, `…/enable`, `…/delete` | Keeps New payment method or Rename (`name`, each `account` ticked), disables, enables or deletes a method | `200` the method in the panel, Payment methods beside it, or Payment methods alone once deleted; or the form or the panel saying why not; `403` without ManageSettings; `404` for an action there is none of |

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Payment methods lists every method in order with the accounts it pays from, said disabled when it is | `settings.MethodsOf`, `web.methodsShown` | `TestWeb_R174_UC014_PaymentMethodsListAsTheAppsScreen` |
| New payment method, Rename, Disable, Enable and Delete as the app's dialog and menu: a name of 1 to 100 letters, at least one account it pays from, every active account ticked for a new one, Delete asked first, a method an expense used never deleted (AC-40.1) | `settings.CreatePaymentMethod`, `RenamePaymentMethod`, `ActivatePaymentMethod`, `DeletePaymentMethod` | `TestWeb_R174_UC014_PaymentMethodsChangeAsTheAppsDialog` |
| Payment methods are for whoever manages the settings; a finished project changes nothing (AC-04.1); a failing database answers 500 | `web.page.mayKeepMethods`, `web.site.keepMethod` | `TestWeb_R174_UC014_PaymentMethodsAreForWhoeverManagesTheSettings` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Entry offering the active methods, and refusing a disabled one | The purchase and contractor payment entry use cases, reading `GET .../payment-methods/offered` |
