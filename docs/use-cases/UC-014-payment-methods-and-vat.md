[Documentation](../README.md) › UC-014

# UC-014: Manage payment methods and the VAT rate

**What it is for:** keeping the list of payment methods entries choose from (FR-40), and the rate their VAT is computed at (BR-14).
**Who uses it:** whoever holds `ManageSettings` in the project, which is Finance. Everyone with access reads them.
**Issue:** redix-api #32; the app side is redix-app #33.

## How it fits together

The seeded template project holds three payment methods: شبكة, تحويل and كاش. FR-40 named a fourth, سداد, which went on 7 November 2026 (redix-api #96, D25). When a project is approved it takes its own copy of them, always from the template: what registration chooses is where the categories and items come from, and it says so on the form. So one project's change touches no other (BR-33).

The VAT rate is a column of the project, `Projects.VatRate`, a fraction that starts at 0.15, and every `ProjectView` carries it as a percentage in `vatPercent`, so the entry use cases need no second call. It is not copied from an earlier project, so a new project never inherits a rate from one that finished years ago.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/payment-methods` | `ReadProject` | `200` every method in order, disabled ones too, as `{ id, name, isActive }` |
| `GET /projects/{projectId}/payment-methods/offered` | `ReadProject` | `200` what an entry may choose: the active ones |
| `POST /projects/{projectId}/payment-methods` with `{ name }` | `ManageSettings` | `200`; `400` `invalidName` |
| `POST /projects/{projectId}/payment-methods/{id}` with `{ name }` | `ManageSettings` | `200`; `400` `invalidName`; `404` |
| `POST /projects/{projectId}/payment-methods/{id}/disable` | `ManageSettings` | `200`; `404` |
| `POST /projects/{projectId}/payment-methods/{id}/enable` | `ManageSettings` | `200`; `404` |
| `POST /projects/{projectId}/payment-methods/{id}/delete` | `ManageSettings` | `200`; `409` `paymentMethodUsed` when an expense was paid with it; `404` |
| `POST /projects/{projectId}/vat-rate` with `{ vatPercent }` | `ManageSettings` | `200` the project, with the rate old and new in the audit trail (`AChangedVatRateIsInTheAuditTrailOldAndNew`); `400` `invalidVatRate`; `404` |

Every change also answers `409` `projectFinished` in a finished project.

## Tables

| Table | Holds |
| --- | --- |
| `PaymentMethods` | Each project's methods, and the template's seed |
| `Projects.VatRate` | Each project's rate, a fraction |

## Rules and where they live

| Rule | Code | Test |
| --- | --- | --- |
| An approved project starts with the three methods | `PaymentMethodSeed`, `ProjectStructure.ApproveAsync` | `PaymentMethodsTests.AnApprovedProjectStartsWithTheThreeMethods` |
| They are rows of its own | `PaymentMethod.CopyFor` | `PaymentMethodsTests.ProjectsTakeTheirOwnRowsOfTheTemplatesMethods` |
| A project copied from an earlier one still starts from the seed, with the starting rate | `ProjectStructure.ApproveAsync`, `Project.StartingVatRate` | `PaymentMethodsTests.AProjectCopiedFromAnEarlierOneStillStartsFromTheSeed` |
| A method is added at the end, and renamed | `PaymentMethodsEndpoints.Add`, `Rename` | `PaymentMethodsTests.FinanceAddsAMethodAtTheEndOfTheList`, `FinanceRenamesAMethod` |
| A method needs a name of 1 to 100 characters | `ProjectChanges.WithNameAsync`, `Project.IsAcceptableName` | `PaymentMethodsTests.AMethodNeedsANameOfAtMost100Characters` |
| A disabled method leaves entry and stays on the list (AC-40.1) | `PaymentMethodsEndpoints.Offered`, `List` | `PaymentMethodsTests.ADisabledMethodLeavesEntryAndStaysOnTheList` |
| Enabling brings it back | `PaymentMethodsEndpoints.Enable` | `PaymentMethodsTests.EnablingBringsAMethodBackIntoEntry` |
| A method no expense was paid with is deleted; one an expense was paid with is refused and stays, to be disabled instead | `PaymentMethodsEndpoints.Delete` | `PaymentMethodsTests.FinanceDeletesAMethodNoExpenseWasPaidWith`, `AMethodAnExpenseWasPaidWithIsNotDeleted` |
| One project's methods and rate change no other, nor the template (BR-33) | `PaymentMethod.ProjectId`, `Project.VatRate` | `PaymentMethodsTests.AChangeInOneProjectLeavesTheTemplateAndOtherProjectsAlone` |
| One project's route never reaches another's methods (BR-33) | `PaymentMethodsEndpoints.ChangeAsync`, `PermissionHandler` | `PaymentMethodsTests.ARouteOfOneProjectNeverReachesAnothersMethods` |
| Everyone with access reads; only Finance changes | `Permission.ManageSettings` | `PaymentMethodsTests.EveryoneWithAccessReadsMethodsAndOnlyFinanceChangesThem`, `FinanceHoldsTheSettingsPermissionAndNoOtherRoleDoes` |
| A finished project refuses every change and stays readable (AC-04.1) | `ProjectChanges.MapProjectChanges`, `Project.SetVatPercent` | `PaymentMethodsTests.AFinishedProjectRefusesEveryChangeAndStaysReadable` |
| A new project's rate is 15% (BR-14) | `Project.StartingVatRate` | `PaymentMethodsTests.ANewProjectsVatRateIsFifteenPercent` |
| Finance sets the rate | `Project.SetVatPercent` | `PaymentMethodsTests.FinanceSetsTheVatRate` |
| The rate is from 0 to 100 with at most two decimals, which the column keeps exactly | `Project.SetVatPercent` | `PaymentMethodsTests.TheVatRateIsFrom0To100WithAtMostTwoDecimals` |

## Left to later use cases

| What | Use case |
| --- | --- |
| Entry offering the active methods, and refusing a disabled one | The purchase and contractor payment entry use cases, reading `GET .../payment-methods/offered` |
| Computing an invoice's VAT from the rate, and keeping it on the entry when the rate changes (BR-14) | The same, reading `vatPercent` from the project |
