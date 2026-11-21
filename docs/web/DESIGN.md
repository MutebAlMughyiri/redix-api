[Documentation](../README.md) › The web's design

# The web's design: the shell

Every REDIX web page lives inside one shell, the base design the owner approved on 21 Nov 2026: "this is our base design and this is our physology" (redix-api #80, #161). The approved mock is https://claude.ai/artifact/N8qnPde3wLkXTnaeTWanDS (private to the owner's Claude account).

## Where it lives

One program, two doors: the app's API (JSON, 8443) and the web (HTML, 443), on the same rules and the same database. Every web file is in `go-backend/web/`; the backend's packages hold no web code, only the rules both doors call (#161 S14).

| File | Holds |
| --- | --- |
| `web/shell.templ` | The page, the frame, the side menu, the tabs and the View menu, the sign-in, change-password and forgot-password forms, Settings |
| `web/assets/redix.css` | The one stylesheet: the colour tokens, light and dark, and every part's look |
| `web/assets/htmx.min.js` | htmx 2.0.11, the one script |
| `web/page.go` | One drawing of the shell; the side menu's items and which of them a built feature opens |
| `web/session.go` | The session's cookies, and the sign-in, change-password and sign-out pages |
| `web/reset.go` | Forgot password: the form, asking for a code, and the reset |
| `web/project.go` | The project the person works in: chosen from the top bar, else the default, kept in a cookie |
| `web/home.go`, `web/home.templ` | The home: the summary, my custody, the newest entries; the project list while choosing |
| `web/register.go`, `web/register.templ` | Register new project from the sign-in: the form, its lists, sending it |
| `web/entries.go`, `web/entries.templ` | The Entries tab: its filters, its pages, an entry in the details panel and its actions |
| `web/addentry.go`, `web/addentry.templ` | Add entry: the form the app's screen asks, drawn again as it changes, recorded through `entries.Record` |
| `web/shell.go` | The page, the tabs, the View menu's choices, the language |
| `web/view.go` | The shell's choices, kept in a cookie |
| `web/assets.go` | The embedded files, gzipped once, served under their hash and kept a year |

## The parts

| Part | The rule |
| --- | --- |
| Frame | IntelliJ-like: top bar, status bar, a side tree like the project explorer, tabbed content. The frame never reloads; a click changes the content area alone |
| Fit | The shell fits the window: the page never scrolls; the content, the tree and a panel scroll inside |
| Top bar | The logo and REDIX; once signed in, the person's name and Sign out. Never the language |
| Side menu | Folds with a keyboard character beside the project's name, never an icon: » in Arabic, « in English, tooltip "Collapse (Alt+1)"; the opposite arrow at the tab bar's start brings it back; Alt+1 is the arrows' `accesskey`. Its line drags from 180 to 480 px, the content keeping 320 |
| An item not built | Stands gray and disabled until its feature opens it |
| View menu | «عرض» / View at the tab bar's end, the browser's own popover: Top bar, Status bar, Side menu (Alt+1) |
| Status bar | The host, the version |
| Sign-in | In the content area; on success the other parts arrive with a little motion, once |
| Language | In Settings, and at the sign-in form's foot |
| Choices | The bars, the folded side menu, its width, the open tabs and the language, kept in cookies and drawn by the server, so nothing flashes |
| Details panel (with the first list) | Like Google Drive's: click or Enter shows it, Esc hides it from anywhere, Up and Down move the selection and the panel follows, Home and End; "Open in a tab" moves the record to its own tab; its line drags from 240 to 560 px |

## How a feature joins it

A feature never draws a frame of its own. It adds its tab's content to `contents` in `web/page.go`, keyed by its side-menu item's name, and the item stops standing gray. Every element it draws gets an id by the rule below. Its rules stay in the backend's owner; where one sits inside a JSON handler, the feature moves it into a function both doors call (#80 D7).

## Every element's id

Every element a test clicks or reads has an id, so automation finds it as the app's Patrol tests find a widget by its key (the owner, 21 Nov 2026). Where the app has the same element, the id is the app's key name; elsewhere it is the web's own, under `web.`. An id is on one element of a page only (`TestWeb_EveryElementsIdIsTheOnlyOneOnItsPage`). [redix-web-automation](https://github.com/MutebAlMughyiri/redix-web-automation) finds them with Playwright's `getByTestId`, set to read `id`.

| Part | Ids |
| --- | --- |
| Frame | `web.shell`, `web.topBar`, `web.brand`, `web.person`, `myAccount.signOut`, `web.content`, `web.statusBar`, `web.host`, `appVersion.line` |
| Side menu | `web.sideMenu`, `web.fold`, `web.tree`, `web.item.<name>` for home, entries, approvals, accounts, partners, contracts, reports, history and settings, `web.sideLine` |
| Tabs | `web.unfold`, `web.tabs`, `web.tab.<name>`, `web.tab.<name>.title`, `web.tab.<name>.close`, `web.pickFromMenu` |
| View menu | `web.view`, `web.viewMenu`, `web.view.topBar`, `web.view.statusBar`, `web.view.sideMenu`, `web.view.width` |
| Sign-in | `signIn.form`, `signIn.title`, `signIn.hint`, `signIn.notice`, `signIn.username`, `signIn.password`, `signIn.message`, `signIn.submit`, `signIn.registerProject`, `auth.forgotPassword` |
| Forgot password | `forgotPassword.title`, `forgotPassword.message`, `forgotPassword.sendForm`, `forgotPassword.login`, `forgotPassword.send`, `forgotPassword.sent`, `forgotPassword.form`, `forgotPassword.code`, `forgotPassword.new`, `forgotPassword.confirm`, `forgotPassword.reset`, `forgotPassword.cancel` |
| Change password | `changePassword.form`, `changePassword.title`, `changePassword.hint`, `changePassword.current`, `changePassword.new`, `changePassword.confirm`, `changePassword.message`, `changePassword.submit` |
| Settings | `sharedSettings.title`, `web.setting.language`, `myAccount.language`, `web.language`, `myAccount.arabic`, `myAccount.english` |
| Details panel | `web.details`; an entry: `entry.title`, `entry.close`, `entry.status`, `entry.reason`, `entry.date`, `entry.item`, `entry.supplier`, `entry.invoice`, `entry.amount`, `entry.paymentMethod`, `entry.account`, `entry.description`, `entry.enteredBy`, `entry.notice`, `entry.refusal`, `entry.approve`, `entry.send`, `entry.change`, `entry.remove`, `entry.ask`, `entry.removeConfirm`, `entry.removeCancel` |
| Add entry, Change (#166) | `addEntry.title`, `addEntry.number`, `addEntry.form`, `addEntry.table`, `addEntry.description`, `addEntry.date`, `addEntry.amount`, `addEntry.kind`, `addEntry.inside`, `addEntry.outside`, `addEntry.supplierSearch`, `addEntry.supplier`, `addEntry.columnsSection`, `addEntry.column.<key>`, `addEntry.columnOption.<key>.<choice>`, `payment.methodChoice`, `payment.method.<id>`, `addEntry.paymentSection`, `payment.account.<id>`, `payment.noAccount`, `addEntry.message`, `addEntry.draft`, `addEntry.send` |
| Entries | `entries.title`, `entries.new`, `entries.filters`, `entries.search`, `entries.mine`, `entries.kindChoice`, `entries.kind.<kind>`, `entries.statusChoice`, `entries.status.<status>`, `entries.period`, `entries.from`, `entries.to`, `entries.number`, `entries.clear`, `entries.empty`, `entries.row.<id>`, `entries.more` |
| Register new project | `newProject.form`, `newProject.title`, `newProject.hint`, `newProject.message`, `newProject.name`, `newProject.description`, `newProject.startDate`, `newProject.units`, `newProject.unitsHeading`, `newProject.unitName`, `newProject.addUnit`, `newProject.unit.<n>`, the same for `partners` and `partner`, `newProject.managerHeading`, `newProject.managerName`, `newProject.username`, `newProject.mobile`, `newProject.password`, `legal.agreement`, `newProject.submit`, `newProject.cancel` |
| Projects | `web.project`, `web.projects`, `web.projects.title`, `web.projects.notice`, `web.projects.<id>`, `projects.star.<id>`, `projects.new`, `web.sideProject`; on the list: `projects.title`, `projects.hint`, `projects.empty`, `projects.card.<id>` |
| Home | `home.title`, `home.roleName`, `home.collapse`, `home.addEntry.<id>`, `summary.card`, `summary.pending`, `summary.surplus.label`, `summary.surplus`, `summary.completion`, `summary.completionBar`, `summary.difference`, `summary.revenue.card`, `summary.revenue.body`, `summary.revenue`, the same for `expenses`, `summary.category.<id>`, `home.custody.<id>`, `home.custodyBalance.<id>`, `home.entriesHeading.<id>`, `home.entriesEmpty.<id>`, `entries.row.<id>`, `home.moreEntries.<id>`; `web.view.summary` keeps Hide details |

## The look

The device's own font (`system-ui`), no web font. 13 px in the shell; a scale of 12 / 13 / 14 / 16 / 18. CSS logical properties only, so one stylesheet serves Arabic and English. Light and dark follow the device. Words first: no icon packs; a control's mark is a keyboard character. Motion only answers a person's action, and stops when the device asks for less.

| Token | Light | Dark | For |
| --- | --- | --- | --- |
| `--accent` | `#006A67` | `#80D5D0` | The app's primary: the selected item, the focus, Sign in |
| `--select` | `#CDEDEA` | `#1F3B39` | A selected or hovered item, and a notice such as "a code was sent" |
| `--chrome` | `#EFF5F3` | `#1B2120` | The frame: bars, side menu, tabs |
| `--chrome-line` | `#D5DEDC` | `#2C3533` | The frame's lines |
| `--content` | `#FBFDFC` | `#141918` | The content area |
| `--text`, `--quiet`, `--faint` | `#161D1C`, `#56615F`, `#7F8A88` | `#DDE4E2`, `#A7B2B0`, `#7F8A88` | Text, its quieter forms, a parked item |
| `--refusal`, `--refusal-bg` | `#BA1A1A`, `#FFDAD6` | `#FFB4AB`, `#5C0F0A` | A refusal, and Sign out |

The stylesheet is their one owner; this table names them.

## Its weight

| Measure | Budget | Today |
| --- | --- | --- |
| The first open | under 100 KB compressed | the page, the stylesheet and htmx: about 25 KB, three requests (`TestWeb_R3_TheFirstOpenWeighsUnder100KBWithHtmxTheOneScript`) |
| Script | htmx alone | htmx 2.0.11, 16.5 KB gzipped; the shell's few lines in its `hx-on` |
| Pictures | the logo alone | the logo, an inline SVG |
| A page after the first | its HTML | a tab answers its content and the tabs and side menu beside it |
