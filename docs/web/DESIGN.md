[Documentation](../README.md) › The web's design

# The web's design: the shell

Every REDIX web page lives inside one shell, the base design the owner approved on 21 Nov 2026: "this is our base design and this is our physology" (redix-api #80, #161). The approved mock is https://claude.ai/artifact/N8qnPde3wLkXTnaeTWanDS (private to the owner's Claude account).

## Where it lives

One program, two doors: the app's API (JSON, 8443) and the web (HTML, 443), on the same rules and the same database. Every web file is in `go-backend/web/`; the backend's packages hold no web code, only the rules both doors call (#161 S14).

| File | Holds |
| --- | --- |
| `web/shell.templ` | The page, the frame, the side menu, the tabs and the View menu, the sign-in and change-password forms, Settings |
| `web/assets/redix.css` | The one stylesheet: the colour tokens, light and dark, and every part's look |
| `web/assets/htmx.min.js` | htmx 2.0.11, the one script |
| `web/page.go` | One drawing of the shell; the side menu's items and which of them a built feature opens |
| `web/session.go` | The session's cookies, and the sign-in, change-password and sign-out pages |
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

A feature never draws a frame of its own. It adds its tab's content to `contents` in `web/page.go`, keyed by its side-menu item's name, and the item stops standing gray. Its rules stay in the backend's owner; where one sits inside a JSON handler, the feature moves it into a function both doors call (#80 D7).

## The look

The device's own font (`system-ui`), no web font. 13 px in the shell; a scale of 12 / 13 / 14 / 16 / 18. CSS logical properties only, so one stylesheet serves Arabic and English. Light and dark follow the device. Words first: no icon packs; a control's mark is a keyboard character. Motion only answers a person's action, and stops when the device asks for less.

| Token | Light | Dark | For |
| --- | --- | --- | --- |
| `--accent` | `#006A67` | `#80D5D0` | The app's primary: the selected item, the focus, Sign in |
| `--select` | `#CDEDEA` | `#1F3B39` | A selected or hovered item |
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
