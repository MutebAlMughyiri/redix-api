[Documentation](../README.md) › UC-018

# UC-018: Search suppliers

**What it is for:** finding the right supplier in under a second while entering an expense, by part of its name or VAT number, or by the exact number a QR code holds.
**Who uses it:** everyone with access to the project: the app offers it to whoever enters an expense or a contract.
**Issue:** redix-api #36; the app side is redix-app #37.

## How it fits together

One route answers both what was typed and what a QR code read. A VAT number has one holder in a project, so an exact number can only find that supplier, and an empty list is how the app learns that nobody holds it, to offer a quick add.

The list is by name for now. "Most used first" (AC-43.1) needs to know which supplier each expense used, and nothing records that until UC-022 "Record a purchase expense". The order lives in one place, `SuppliersEndpoints.Search`.

## Endpoints

| Endpoint | Needs | Answers |
| --- | --- | --- |
| `GET /projects/{projectId}/suppliers/search?text=...` | `ReadProject` | `200` with at most 20 suppliers on offer to the caller (UC-019), by name, whose name or VAT number holds `text`, each as in the register. `text` may be left out; fewer than 3 letters narrow nothing down |

## Tables

No new table. An index on `suppliers (project_id, name)` serves the order. `suppliers.normalized_name`, kept beside the name and filled when it is saved, is what a search compares, because no collation treats Arabic spelling variants alike.

## Rules and where they live

Each test named without `Test` is .NET's, replayed against Go as `TestReplayDotNetsAnswers/SuppliersTests.<name>`; one named `Test…` is Go's own.

| Rule | Enforced by | Proved by |
| --- | --- | --- |
| Three letters of the name find the supplier (AC-43.1) | `suppliers.Routes.search` | `ThreeLettersOfTheNameFindTheSupplier` |
| A search treats أ إ آ with ا, ة with ه, ى with ي, ؤ with و, ئ with ي alike and ignores tashkeel, both ways; an answer shows the name as it was typed | `spelling.Normalize`, `suppliers.Routes.search` | `ASearchTreatsArabicSpellingVariantsAlikeAndIgnoresTashkeel`, `ArabicSpellingKeepsOneFormOfEachLetterAndNoTashkeel`, `AnAnswerShowsTheNameAsItWasTyped` |
| A rename moves what a search finds; tashkeel counts for nothing toward the three letters | `suppliers.save`, `suppliers.Routes.search` | `ARenamedSupplierIsFoundByItsNewNameAndNoLongerByItsOldOne`, `TashkeelCountsForNothingTowardTheThreeLettersASearchNeeds` |
| The migration fills the normalized name of rows saved before it, by the same rule | `spelling.Normalize`, the `SupplierNormalizedName` migration | `TheMigrationFillsTheNormalizedNameOfSuppliersSavedBeforeIt` |
| Part of the VAT number finds the supplier (AC-43.1) | `suppliers.Routes.search` | `PartOfTheVatNumberFindsTheSupplier` |
| Fewer than three letters narrow nothing down | `suppliers.Routes.search` | `FewerThanThreeLettersNarrowNothingDown` |
| With no text, the suppliers on offer, by name | `suppliers.Routes.search` | `NoTextOffersTheSuppliersByName` |
| An exact VAT number finds its supplier alone (FR-43) | The index that holds a number to one supplier | `AnExactVatNumberFindsItsSupplierAlone` |
| A number nobody holds finds nothing, so the app can offer a quick add (FR-43) | `suppliers.Routes.search` | `AVatNumberNoSupplierHoldsFindsNothing` |
| A disabled supplier is not offered (BR-15) | `suppliers.Routes.search` | `ADisabledSupplierIsNotOffered` |
| An answer holds at most 20 suppliers, within a second (AC-43.1) | `suppliers.Routes.search`, the index `ix_suppliers_project_id_name` | `ASearchAnswersAtMost20SuppliersWithinASecond` |
| Everyone with access searches (BR-31) | `access.ReadProject` | `EveryoneWithAccessSearches` |
| A search never reaches another project (BR-33) | `suppliers.Routes.search` | `ASearchNeverReachesAnotherProject` |

## Left to later use cases

| What | Use case |
| --- | --- |
| The caller's most used suppliers first (AC-43.1) | UC-022 "Record a purchase expense", once entries name suppliers |
| Reading the VAT number from an invoice's QR code | UC-024 "Fill the expense from the e-invoice QR" |
