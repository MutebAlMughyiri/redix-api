---
name: redix-story
description: Write or update a REDIX work item as a GitHub issue on MutebAlMughyiri/redix-api or redix-app in the house format - what the owner asked in his words, its state, the requirements table, today measured from the code, the questions for the plan - with the right labels and milestone. Use when the owner describes a feature, a use case, a bug or a chore for REDIX and wants it captured, when he says "add feature", "open an issue", "add to the backlog", or asks to rewrite an existing issue. A finding is not an issue: it is a row in #76.
---

# REDIX story

> Brought from the Testin project, 12 November 2026. GitHub is the source of truth for REDIX's work: the owner works across machines, and an issue travels where a local file does not.

## Which repo, which kind

| What it is | Where | Title |
| --- | --- | --- |
| A slice of the app in build order | Both repos, the same number: `UC-NNN` in redix-api for the server, in redix-app for the screens | `UC-069: History of every action, with its time, user and record` |
| A feature | The repo that holds most of it | What it does, in the business's words: `Emails: the daily report, a new user and every action, from templates and an actions enum` |
| A gap, a bug, a decision found on the way | **Not an issue**: a row in redix-api #76 "Findings: open gaps, bugs and decisions" | - |

Read the open issues first (`gh issue list`), and update an existing one with `gh issue edit <n> --body-file` rather than opening a duplicate. Before scoping from an issue, check its central claim against the code: fixes land under other numbers.

## The body

Write it to a file in the scratchpad, then `gh issue create --body-file`.

```markdown
## What the owner asked (12 Nov 2026)

> "his words, verbatim, typos and all"

**State:** filed for release 2 / planned, in release 1 / designed, waiting for approval / built. One line saying what exists.

## Requirements

| # | Requirement | From |
| --- | --- | --- |
| R1 | Each thing he asked, a testable sentence | The owner |
| R2 *added* | What the design needs, or what goes wrong without it | One line why |

## Today

| | Today |
| --- | --- |
| Each part the feature touches | What the code does now, with its file path, read from the code |

## Questions for the plan

| # | Question |
| --- | --- |
| Q1 | Each decision the plan must settle |
```

- **His words verbatim** in the quote: the issue is where he recognises what he asked.
- **Today is measured**, never assumed: a path, a count, a class name read from the code on the day.
- **Each decision he makes** goes back into the issue as a requirement or a dated decision (`D1 = check boxes`), so the issue stands on its own.
- **A design page** is linked near the top, marked as private to his Claude account, and its content is also in the issue.
- **A plan comes before a build** for anything new (`fix-or-file`): the issue stops at Questions until he asks for the plan.

## Labels and milestone

| Label or milestone | When |
| --- | --- |
| `use case` | A `UC-NNN` slice |
| `feature` | Anything else new |
| milestone **Release 1** | The owner put it in release 1 |
| `release 2` | The owner deferred it |
| `open question` | It waits for a business answer |
| `privacy` | It decides who sees what |
| `translations` | redix-app: words and the glossary |
| `out of scope` | redix-app: parked. Never propose or start such an issue |
| `findings` | #76 only |

## Words

Quote an issue's title the first time its number is named. Name code objects by their real names (`names-say-the-business`). Tables over prose. No session link in an issue, a comment or a commit.
