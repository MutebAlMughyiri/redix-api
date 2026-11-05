# REDIX API documentation

The server of REDIX (Real Estate Development Intelligence): it keeps every project's expenses, approvals, partners' capital and reports for the mobile app.

| Page | Read it to |
| --- | --- |
| [Architecture](ARCHITECTURE.md) | See how the API is built, where each thing lives, and how a request flows |
| [Decisions](decisions.md) | Know why things are as they are |
| [Use cases](#use-cases) | Know what each part does, who may use it, and where each rule lives in the code |
| Swagger, at `/swagger` outside production | Call an endpoint: its purpose, request, response and errors come from the code's doc comments |

The business behind every page is the [BRS](https://claude.ai/artifact/JJPc1nSe6ekignSgXUpYB4) (private link), and what to build next is in this repo's issues.

## Use cases

Numbered in build order; the same number in the app's documentation is the same slice of the app.

| Use case | What it does | Who |
| --- | --- | --- |
| [UC-001](use-cases/UC-001-setup.md) | The base: the database, health check, Swagger, and the words the app shows | Everyone |
| [UC-002](use-cases/UC-002-sign-in.md) | Sign in with a username and password, and keep the session alive | Every user |
| [UC-003](use-cases/UC-003-change-password.md) | Change the password, and secure every request | Every signed-in user |
| UC-004 | Switching the app language is the app's use case; the API's part is the words of My account, added with the `LanguageSwitch` migration | Every signed-in user |
| [UC-005](use-cases/UC-005-audit-trail.md) | The audit trail: who did what to which record, when, with the values before and after; never changed or deleted | The API, on its own |
| [UC-006](use-cases/UC-006-projects.md) | A project and its units, and close it when finished | A financial supervisor adds units and closes |
| [UC-007](use-cases/UC-007-users.md) | Correct, disable and reset logins | A project manager, for someone only in projects they manage |
| [UC-008](use-cases/UC-008-project-roles.md) | Add people to a project and give their roles, and show each person only their projects | The project manager adds and gives; everyone reads |
| [UC-009](use-cases/UC-009-roles-and-permissions.md) | Keep every role in the database, keep projects separate, and enforce both on the server | Every request |
| [UC-010](use-cases/UC-010-default-project.md) | Keep each person's default project, and open it at sign-in | Everyone, for themselves |
| [UC-066](use-cases/UC-066-register-a-project.md) | Register a project with its manager, and approve it | Anyone registers; only the admin approves |
| [UC-062](use-cases/UC-062-sign-out.md) | Sign out of one device | Every signed-in user |
