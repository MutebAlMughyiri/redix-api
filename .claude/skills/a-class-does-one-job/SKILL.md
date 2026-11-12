---
name: a-class-does-one-job
description: Before adding a method to an existing class in the REDIX API, check that it is the same job the class already does, and that its name still says which. Use when an endpoints class gains logic, when a table or rules class gains a method about another feature, or when a class's name and its method list have drifted apart.
---

# A class does one job, and its name says which

> Brought from the Testin project, 12 November 2026, at Muteb's word: *"let us also bring our skills that in testin project ... skills like every class do one job"*.

A class earns its name by doing one job. The failure this catches is not a bad class. It is a good one that quietly took on a second job, one convenient method at a time, until its name stopped describing it. Nothing breaks when that happens, which is why it survives review.

## How the API already splits a feature

The `Entries/` folder is the shape to copy; each class says its job in one sentence:

| Class | Its job |
| --- | --- |
| `EntriesEndpoints` | Maps the routes and answers them: who may call, what comes back |
| `EntryChanges` | Changes an entry and writes its History: remove, restore (redix-api #119) |
| `EntryTable` | Turns a request into an entry's row |
| `PurchaseTable` | A purchase's own rules: its amount, what it refuses (`AmountOf`, `RefusalOfTyped`) |
| `Partners.PartnerSight` | Who may see a partner's money (BR-31), asked by every list |

When `EntriesEndpoints` grows a calculation, or `EntryChanges` grows a query a report needs, the class has taken a second job.

## The rule

**Before adding a method, say the class's job in one sentence, then the new method's job. If the second sentence is not the first, the method goes somewhere else.**

"It's related" is not the test; "it is the same job" is.

## The signals

- A method that composes a value is added to a class that maps routes, or one that decides is added to one that stores.
- The new name repeats a domain noun the class name does not contain: an `AccountHistory` method that knows a supplier's bills has learned another feature's business.
- Its doc comment cites a rule from another feature folder.
- The class name would need an "and" to be accurate.
- Names ending in `Manager`, `Helper`, `Util`, `Service` or `Handler` describe no job. Measured 12 November 2026: none in `src/` (`ProjectManagers` is the business's word, the people who manage a project).

## Where the method goes instead

In order; stop at the first that fits.

1. **A type that already owns this vocabulary**: `Permission`, `HistoryAction`, `EntryStatus`.
2. **The entity the value is about**: an entry's question on `Entry`, as `Entry.OfProjectAsync` and `SeenByAsync` already are.
3. **A new class named for the job**, in the feature's folder.
4. **The single caller**, when exactly one place uses it.

Never "the class that already has the `DbContext`".

## This is not

- **Not an argument against shared owners.** `centralize-shared-design` says a rule needs *an* owner; this skill says the owner is not whatever class is convenient.
- **Not an argument for many tiny classes.** A class with one job may be large.

## When you find one

In code you were not asked to change: a finding, a row in #76 (`report-findings`). In the class you are editing: say it in one sentence, move the methods, keep every route, field and word byte-identical, and let the xUnit tests prove nothing else moved.
