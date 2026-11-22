// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"net"
	"net/http"
	"net/url"
	"slices"
	"strings"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projects"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/translations"
)

// page is one drawing of the shell: who asks, in which language, and what its parts show.
type page struct {
	words    words
	language string
	// theme is the appearance the person chose, light or dark, or "" for the system's (#175 D1).
	theme  string
	host   string
	view   view
	assets assets
	// person is the signed-in person's name; "" before anyone signs in.
	person string
	// caller is who is signed in, and what they hold; the zero caller before anyone is.
	caller access.Caller
	// projects are the projects the caller may read or started, as the app's project list shows them (redix-api #165).
	projects []projects.View
	// project is the project the caller works in, the one the top bar names; the zero view while they choose one.
	project projects.View
	// defaultProject is the project the caller's sign-in opens straight into, starred in their list; uuid.Nil when none is (AC-37.1).
	defaultProject uuid.UUID
	// in shows the side menu and the tabs: someone is signed in with no temporary password left to change (AC-02.1).
	in bool
	// arriving plays the parts' motion once, in the answer to signing in (#161 R6).
	arriving bool
	// notice is the word key the sign-in form says above its fields, once, in the answer that brought it: after a reset, auth.passwordReset.
	notice string
	// details is what the shell's details panel shows when the whole shell is drawn: nothing, or the entry a notification opened.
	details templ.Component
	// said is what the change that brought the front tab there said, for the tab to show: Ask for permissions' sent, on My permissions.
	said    said
	content templ.Component
}

// words are one language's words by key (translations); a key with no word shows itself, so a missing word is seen, never hidden.
type words map[string]string

// Of is key's word, each {n} in it filled with fill[n] (translations.Fill).
func (w words) Of(key string, fill ...string) string {
	word, found := w[key]
	if !found {
		return key
	}
	return translations.Fill(word, fill...)
}

// languageCookie keeps the language a person chose (#80 D11).
const languageCookie = "redix_language"

// languageOf is the language r asks in: the one its person chose, else the browser's, Arabic for any but English (web.Language).
func languageOf(r *http.Request) string {
	if chosen, err := r.Cookie(languageCookie); err == nil && (chosen.Value == "ar" || chosen.Value == "en") {
		return chosen.Value
	}
	return web.Language(r)
}

// page is the shell for r before anyone is known: its language and words, its view, and the host it answers on.
func (s site) page(r *http.Request) (page, error) {
	language := languageOf(r)
	rows, err := s.Database.Queries().WordsIn(r.Context(), language)
	if err != nil {
		return page{}, err
	}
	found := make(words, len(rows))
	for _, row := range rows {
		found[row.Key] = row.Value
	}
	host, _, err := net.SplitHostPort(r.Host)
	if err != nil {
		host = r.Host
	}
	p := page{words: found, language: language, theme: themeOf(r), host: host, view: viewOf(r), assets: s.assets, details: templ.NopComponent}
	p.content = emptyContent(p)
	return p, nil
}

// open is p for caller: their name, their projects and the one they work in, and the shell's parts with the active tab; or the change-password
// form while a temporary password is left to change (AC-02.1, #161 R13). The error is the database's.
func (s site) open(r *http.Request, p page, caller access.Caller) (page, error) {
	user, err := s.Database.Queries().UserWithID(r.Context(), caller.UserID)
	if err != nil {
		return p, err
	}
	p.person = user.DisplayName
	if caller.MustChangePassword {
		p.content = changePasswordForm(p, "", caller.Username)
		return p, nil
	}
	p.in, p.caller = true, caller
	if p.projects, err = projects.List(r.Context(), s.Database.Queries(), s.Book, s.Clock.Now(), caller.UserID); err != nil {
		return p, err
	}
	if p.defaultProject, err = projects.DefaultOf(r.Context(), s.Database.Queries(), caller.UserID, caller.Access); err != nil {
		return p, err
	}
	p.project = p.projectOf()
	if tab, opened := contents[p.view.Active]; opened && p.offers(p.view.Active) {
		p.content, err = tab(s, p.view.recordOf(r), p)
	}
	return p, err
}

// direction is the way p's language reads.
func (p page) direction() string {
	if p.language == "ar" {
		return "rtl"
	}
	return "ltr"
}

// foldArrow folds the side menu toward its edge: the menu stands on the right in Arabic, on the left in English (#161 R7).
func (p page) foldArrow() string {
	if p.language == "ar" {
		return "»"
	}
	return "«"
}

// unfoldArrow brings the side menu back.
func (p page) unfoldArrow() string {
	if p.language == "ar" {
		return "«"
	}
	return "»"
}

// shortcut is key's word with the shortcut that does the same, kept whole inside Arabic text.
func (p page) shortcut(key string) string { return p.words.Of(key) + " \u2068(Alt+1)\u2069" }

// menuItem is an item of the side menu: its tab's name and its title's word.
type menuItem struct{ name, title string }

// tab draws a built feature's tab for p's caller in their project. The error is the database's.
type tab func(s site, r *http.Request, p page) (templ.Component, error)

// built says whether a feature opens item's tab; an item none opens stands gray (#161 R15).
func (item menuItem) built() bool {
	_, built := contents[item.name]
	return built
}

// sideMenu is the side menu's items, in the order the app's drawer lists them (#161, the shell).
var sideMenu = []menuItem{
	{"home", "web.home"}, {"entries", "entries.title"}, {"approvals", "approvals.title"}, {"accounts", "accounts.title"}, {"partners", "partners.title"},
	{"suppliers", "suppliers.title"}, {"contracts", "contracts.title"}, {"reports", "reports.title"}, {"projectHistory", "history.projectTitle"},
	{"users", "users.title"}, {"requests", "permissionRequests.listTitle"}, {"categories", "categories.title"}, {"paymentMethods", "paymentMethods.title"},
	{"loadFile", "projects.loadFile"}, {"archive", "projects.archive"}, {"retrieve", "projects.retrieve"}, {"projectInfo", "projectInfo.title"}, {"myPermissions", "permissions.mine"}, {"documents", "documents.title"}, {"settings", "sharedSettings.title"},
}

// menuNeeds is the permission a side-menu item needs, as the app shows its screen only to whoever holds it; an item missing here needs none.
var menuNeeds = map[string]access.Permission{"approvals": access.ApproveEntries, "approvalHistory": access.ApproveEntries, "reports": access.SeeReports,
	"expenses": access.SeeReports, "revenue": access.SeeReports, "itemReport": access.SeeReports,
	"periodicReport": access.SeeReports,
	"projectHistory": access.SeeProjectHistory, "users": access.ManageUsers, "newUser": access.ManageUsers, "userPermissions": access.ManageUsers, "requests": access.ManageUsers, "categories": access.ManageStructure, "itemColumns": access.ManageStructure, "itemTables": access.ManageStructure,
	"paymentMethods": access.ManageSettings, "loadFile": access.ManageProjectFiles, "archive": access.ManageProjectFiles,
	"retrieve": access.ManageProjectFiles}

// handsOver are the side menu's items that hand a file over rather than open a tab, by the path that hands it: Archive, as the app's button
// hands the project's archive to the phone (AC-73.1).
var handsOver = map[string]string{"archive": "/project-file/archive"}

// offers says whether p's caller may use the tab named name: the side menu and the open tabs leave out one they may not, and its content is not
// drawn, even asked for by its address, as the app hides its screen (REDIX rule of 7 Nov).
func (p page) offers(name string) bool {
	needs, guarded := menuNeeds[name]
	return !guarded || p.may(needs)
}

// pageTabs are the tabs a page opens that the side menu does not list: Add entry, from Entries' button, and an entry's Change, from the
// details panel (redix-api #166); the approval history, from Approvals (redix-api #167); Register new project, from the project menu (redix-api #164); the count of the actual balances and the account history, from Accounts (redix-api #168); the partner history, from Partners (redix-api #169); each side's item reports, from Reports, an item's or a contract's report, and a periodic report (redix-api #171); My history, from the top bar (redix-api #172); New user and a person's Permissions, from Users, and Ask for permissions, from My permissions (redix-api #173); an item's Columns and Tables, from its panel (redix-api #174); Notifications, from the top bar, and Change password and Registered projects, from Settings (redix-api #175).
var pageTabs = []menuItem{{"addEntry", "entries.new"}, {"changeEntry", "entries.edit"}, {"approvalHistory", "approvals.history"}, {"registerProject", "projects.new"}, {"reconcile", "reconciliations.enterTitle"}, {"accountHistory", "accounts.history"}, {"partnerHistory", "partners.history"}, {"expenses", "summary.expenses"}, {"revenue", "summary.revenue"}, {"itemReport", "contracts.item"}, {"periodicReport", "reports.periodic"}, {"myHistory", "history.myTitle"}, {"newUser", "users.new"}, {"userPermissions", "permissions.title"}, {"permissionRequest", "permissionRequests.title"}, {"itemColumns", "columns.title"}, {"itemTables", "categories.tables"}, {"notifications", "notifications.title"}, {"changePassword", "myAccount.changePassword"}, {"registered", "registered.title"}}

// contents are what each built feature's tab shows, by its item's name; an item with none stands gray until its feature is built (#161 R15).
var contents = map[string]tab{
	"home":              site.homeTab,
	"entries":           site.entriesTab,
	"addEntry":          site.addEntryTab,
	"approvals":         site.approvalsTab,
	"accounts":          site.accountsTab,
	"partners":          site.partnersTab,
	"suppliers":         site.suppliersTab,
	"contracts":         site.contractsTab,
	"reports":           site.reportsTab,
	"expenses":          site.expensesTab,
	"revenue":           site.revenueTab,
	"itemReport":        site.itemReportTab,
	"periodicReport":    site.periodicReportTab,
	"projectHistory":    site.projectHistoryTab,
	"myHistory":         site.myHistoryTab,
	"users":             site.usersTab,
	"newUser":           site.newUserTab,
	"userPermissions":   site.userPermissionsTab,
	"myPermissions":     site.myPermissionsTab,
	"categories":        site.categoriesTab,
	"itemColumns":       site.columnsTab,
	"itemTables":        site.itemTablesTab,
	"paymentMethods":    site.paymentMethodsTab,
	"projectInfo":       site.projectInfoTab,
	"notifications":     site.notificationsTab,
	"registered":        site.registeredTab,
	"permissionRequest": site.askTab,
	"requests":          site.requestsTab,
	"partnerHistory":    site.partnerHistoryTab,
	"reconcile":         site.countTab,
	"accountHistory":    site.accountHistoryTab,
	"approvalHistory":   site.approvalHistoryTab,
	"registerProject":   site.registerTab,
	"changeEntry":       site.changeEntryTab,
	"settings":          site.settingsTab,
	"documents":         site.documentsTab,
	"loadFile":          site.loadFileTab,
	"retrieve":          site.retrieveTab,
	"changePassword":    func(_ site, _ *http.Request, p page) (templ.Component, error) { return changePasswordTab(p), nil },
}

// openTabs are the open tabs' items, in the order they were opened.
func (p page) openTabs() []menuItem {
	var open []menuItem
	for _, name := range p.view.Tabs {
		for _, item := range slices.Concat(sideMenu, pageTabs) {
			if item.name == name && p.offers(name) {
				open = append(open, item)
			}
		}
	}
	return open
}

// lineDragged moves a line the person drags (dragLine): the part beside it takes the pointer's distance from the start or the end, as data-edge
// says in the page's direction, of the box data-within names, between data-least and data-most pixels and leaving the content 320; it is set on
// the shell as the CSS variable data-width and in the View menu's field data-keeps (#161 R10).
const lineDragged = `if (!this.hasPointerCapture(event.pointerId)) return; ` +
	`const box = this.closest(this.dataset.within).getBoundingClientRect(), fromLeft = (this.dataset.edge === 'start') !== (getComputedStyle(this).direction === 'rtl'); ` +
	`const along = fromLeft ? event.clientX - box.left : box.right - event.clientX; ` +
	`const width = Math.round(Math.max(+this.dataset.least, Math.min(+this.dataset.most, box.width - 320, along))); ` +
	`this.closest('.shell').style.setProperty(this.dataset.width, width + 'px'); document.getElementById(this.dataset.keeps).value = width`

// shellKeys is the shell's one keydown handler (see document): Alt+1 and Esc. It and the lists' handlers below are constants of our own, holding no
// person's data, so the templates write them as trusted script.
const shellKeys = `if (event.altKey && event.code === 'Digit1' && document.getElementById('web.view.sideMenu')) { event.preventDefault(); ` +
	`document.getElementById('web.view.sideMenu').click() } ` +
	`const details = document.getElementById('web.details'); ` +
	`if (event.key === 'Escape' && details && details.hasChildNodes() && !document.querySelector(':popover-open')) { event.preventDefault(); ` +
	`details.replaceChildren(); const shown = document.querySelector('.grid [aria-selected=true]'); if (shown) shown.focus() }`

// selectRow defines select, which marks one row of a list as the selected one and keeps it as the selection of the tab in front, so the details
// panel comes back to it when that tab does (findings row 293); both of a list's handlers start with it.
const selectRow = `window.selected = window.selected || {}; ` +
	`window.select = window.select || (row => { row.closest('tbody').querySelectorAll('[aria-selected=true]').forEach(other => ` +
	`other.setAttribute('aria-selected', 'false')); row.setAttribute('aria-selected', 'true'); ` +
	`const front = document.querySelector('.tab[aria-selected=true]'); if (front) window.selected[front.id] = row.getAttribute('hx-get') }); `

// followSelection is the shell's one settle handler (see document): when a tab's content comes from /tab/ - the tab brought to the front, or its
// filters changed -, the row that tab last selected is selected again and shown, so the one details panel always shows what the tab in front
// has selected (the owner, 23 Nov 2026, findings row 293: "one view panel and changed based on selected"). A list redrawn beside the panel
// after a change is not a tab arriving, and the panel keeps what it says.
const followSelection = selectRow + `const path = event.detail.pathInfo && event.detail.pathInfo.requestPath; ` +
	`if (event.target.id !== 'web.content' || !path || !path.startsWith('/tab/')) return; ` +
	`const front = document.querySelector('.tab[aria-selected=true]'), address = front && window.selected[front.id]; ` +
	`const row = address && [...event.target.querySelectorAll('tr[hx-get]')].find(each => each.getAttribute('hx-get') === address); ` +
	`if (row) { select(row); row.click() }`

// listKeys are a list's own keys, which every list of the shell carries (the base design): a click or Enter shows a row in the details panel;
// Up, Down, Home and End move the selection, and the panel follows it while it is open.
const listKeys = selectRow + `const row = event.target.closest('tr[hx-get]'); if (!row) return; ` +
	`const rows = [...this.querySelectorAll('tr[hx-get]')], at = rows.indexOf(row); ` +
	`if (event.key === 'Enter') { event.preventDefault(); row.click(); return } ` +
	`const next = rows[{ArrowDown: at + 1, ArrowUp: at - 1, Home: 0, End: rows.length - 1}[event.key]]; if (!next) return; ` +
	`event.preventDefault(); next.focus(); if (document.getElementById('web.details').hasChildNodes()) { next.click() } else { select(next) }`

// listSelects marks the row a click shows as the selected one.
// searchHolds says whether a list's search keeps a row: nothing typed keeps every row, and otherwise one of its fields holds text, whatever the
// letters' case - as the app's ListSearch keeps it. Every list the web searches in the page asks it.
func searchHolds(text string, fields ...string) bool {
	text = strings.ToLower(text)
	return text == "" || slices.ContainsFunc(fields, func(field string) bool { return strings.Contains(strings.ToLower(field), text) })
}

// searchGroup is what a list's search keeps of a group's members, and whether the group shows, as the app's ListSearch.groupOf: every member
// when the group's own name holds the text, otherwise the members that hold it, a group keeping none hidden; nothing typed keeps everything.
func searchGroup[T any](text, name string, members []T, textOf func(T) string) ([]T, bool) {
	if searchHolds(text, name) {
		return members, true
	}
	kept := slices.DeleteFunc(slices.Clone(members), func(member T) bool { return !searchHolds(text, textOf(member)) })
	return kept, len(kept) > 0
}

const listSelects = selectRow + `const row = event.target.closest('tr[hx-get]'); if (row) select(row)`

// logoSVG is REDIX's mark, written once: drawn in the top bar, and the browser tab's icon.
const logoSVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" aria-hidden="true"><rect width="1024" height="1024" fill="#0E6B68"/>` +
	`<g stroke="#FFFFFF" fill="none"><path d="M276 430 L512 250 L748 430" stroke-width="76" stroke-linecap="round" stroke-linejoin="round"/>` +
	`<path d="M410 444 V780" stroke-width="96"/><path d="M410 492 H560 A104 104 0 0 1 560 700 H410" stroke-width="96"/>` +
	`<path d="M536 700 L640 780" stroke-width="96"/></g></svg>`

// logoIcon is the browser tab's icon: the logo inside the page, so no request asks for /favicon.ico.
var logoIcon = templ.SafeURL("data:image/svg+xml," + url.PathEscape(logoSVG))

// swapOutOfBand marks a part an answer carries beside its content, so htmx puts it in its place by its id.
func swapOutOfBand(outOfBand bool) templ.Attributes {
	if outOfBand {
		return templ.Attributes{"hx-swap-oob": "true"}
	}
	return templ.Attributes{}
}
