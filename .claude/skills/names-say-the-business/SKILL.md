---
name: names-say-the-business
description: Every class, method, property, variable, column, route, enum value, word key and test in the REDIX API is named with the business word from the glossary, in full - an entry is an Entry, a capital movement a CapitalMovement, never a shortened or generic name. Use when adding or renaming anything in redix-api, when writing a test name, a doc comment, an issue or a message to Muteb, and when a name says Data, Item, Info or Dto where a business word belongs.
---

# Names say the business word, in full

> Brought from the Testin project, 12 November 2026. In Testin Muteb wrote: *"correct name will allow me and contributors to understand easily and any name should match our correct names in business."*

## The one owner of the words

The glossary is the redix-app issue **"Translations: English terms and their Arabic"** (redix-app #21). Its **In code** column is the name a term has in Go, in Dart, in the API's fields, in the tables and in enum values. A term is agreed there before it becomes a word in `TranslationSeed` or a name in code. This skill does not repeat the list; read the issue.

## The rule

**Name it after the thing it is about, with the glossary's word spelled out.**

| Not this | This | Why |
| --- | --- | --- |
| `EntryDto`, `EntryData`, `EntryInfo` | `EntryRowView`, `Entry` | Say what it is: a row as the lists show it, or the entry itself |
| `item` alone | `entry`, `account`, `capitalMovement` | The reader is reading the name, not the type |
| `amt`, `qty`, `proj`, `acc` | `amount`, `quantity`, `project`, `account` | No short names for the business |
| a test named `Works` | `AC_14_2_D4_ACreditNoteIsANegativeTotalOfItsOwnThatTakesFromTheItem` | A test's name is the rule it proves, starting with the rule's number |
| an enum value `Action2` | `HistoryAction.Restore` | An enum value names what it is, as the glossary says it |

Measured 12 November 2026: no `var` or typed local or field in `src/` is named `acc`, `proj`, `amt`, `qty`, `tx`, `msg`, `btn`, `req`, `res`, `ctx`, `cfg`, `idx`, `val`, `tmp` or `str`.

**What may stay short:** Go's own idioms (`err`, `i` in a loop, `w` and `r` in a handler, `ctx`, `q` for the queries, `t` in a test).

## Every sentence to Muteb too

Chats, designs, `AskUserQuestion` briefs and issues use the same words as the code and the screens: an entry (قيد) not "a record", a custody (عهدة) not "a balance". A word the glossary does not hold is added to the glossary first.

## Stored and sent names

A table, a column, a route, a JSON field or a word key is renamed only on purpose, never by a sweep: it is a contract with the app and the database. A rename of a stored name is a migration named after it, and the qa database is backed up before it runs. Diff every string literal before and after.

## Checks before you name something

1. Which glossary term is this? Write its **In code** name in full.
2. Would a contributor who has never read this file know what the name holds?
3. Does the name repeat the type (`Dto`, `Data`, `Model`, `List`) instead of saying the business word?
4. Is it shortened? Then it is a Go idiom, or it is spelled out.
