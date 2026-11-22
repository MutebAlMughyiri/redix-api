// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"fmt"
	"maps"
	"net/http"
	"net/url"
	"slices"
	"strconv"

	"github.com/a-h/templ"
	"github.com/google/uuid"
)

// view is how a person left the shell: its bars, its side menu and that menu's width, the details panel's width, the home's summary, its open tabs, and the project they
// work in, kept in cookies and drawn by the server, so nothing flashes on the next open (#161 R18, redix-api #165).
type view struct {
	Top, Status, Side, Summary bool
	Width, DetailsWidth        int
	Tabs                       []string
	Active                     string
	Project                    uuid.UUID
	// Records are the record each tab opened for one keeps, as its page's query: whose Permissions, which item's Columns or Tables.
	Records map[string]string
}

// recordTabs are the tabs a page opens for one record, which show it again when their title is pressed or the page opens anew, as the app's
// screen stays on what it opened (redix-api #174).
var recordTabs = []string{"userPermissions", "itemColumns", "itemTables"}

const (
	// lookCookie keeps the bars, the side menu and its width, which the View menu changes; tabsCookie keeps the open tabs. Each is written only by
	// what changes it, so neither save carries a stale copy of the other over it: one cookie for both lost a tab opened while the side menu's
	// line was dragged, the drag's save leaving before the tab's answer came back.
	lookCookie = "redix_view"
	tabsCookie = "redix_tabs"
	// projectCookie keeps the project a person chose to work in, as the app keeps the project it opened (redix-api #165).
	projectCookie = "redix_project"
	// narrowest and widest are the side menu's width limits, detailsNarrowest and detailsWidest the details panel's; the content keeps the rest
	// (#161 R10, findings row 304).
	narrowest, widest               = 180, 480
	detailsNarrowest, detailsWidest = 240, 560
)

// viewOf is the view r's cookies keep; every bar, the side menu and the home's summary shown until a person hides them, and Home open first, as
// the app opens on a project's home (redix-api #165). A tab no feature opens is dropped.
func viewOf(r *http.Request) view {
	look := preference(r, lookCookie, url.Values{"top": {"1"}, "status": {"1"}, "side": {"1"}, "summary": {"1"}})
	tabs := preference(r, tabsCookie, url.Values{"tab": {"home"}, "active": {"home"}})
	v := view{Top: look.Get("top") == "1", Status: look.Get("status") == "1", Side: look.Get("side") == "1", Summary: look.Get("summary") != "0",
		Width: sized(look.Get("width"), 248, narrowest, widest), DetailsWidth: sized(look.Get("details"), 320, detailsNarrowest, detailsWidest)}
	if chosen, err := r.Cookie(projectCookie); err == nil {
		v.Project, _ = uuid.Parse(chosen.Value)
	}
	for _, name := range tabs["tab"] {
		if _, built := contents[name]; built && !slices.Contains(v.Tabs, name) {
			v.Tabs = append(v.Tabs, name)
		}
	}
	if slices.Contains(v.Tabs, tabs.Get("active")) {
		v.Active = tabs.Get("active")
	}
	for _, name := range v.Tabs {
		v = v.recording(name, tabs.Get("record."+name))
	}
	return v
}

// sized is a width as the cookie or the View menu keeps it, between least and most; absent when it keeps none, as a cookie kept before the
// details panel's width was.
func sized(kept string, absent, least, most int) int {
	width, err := strconv.Atoi(kept)
	if err != nil {
		return absent
	}
	return min(max(width, least), most)
}

// preference is the values r's cookie called name keeps, or absent when it keeps none.
func preference(r *http.Request, name string, absent url.Values) url.Values {
	if cookie, err := r.Cookie(name); err == nil {
		if values, err := url.ParseQuery(cookie.Value); err == nil {
			return values
		}
	}
	return absent
}

// keepLook sets v's bars, side menu and the two widths in their cookie.
func (v view) keepLook(w http.ResponseWriter) {
	setPreference(w, lookCookie, url.Values{"top": {flag(v.Top)}, "status": {flag(v.Status)}, "side": {flag(v.Side)}, "summary": {flag(v.Summary)},
		"width": {strconv.Itoa(v.Width)}, "details": {strconv.Itoa(v.DetailsWidth)}}.Encode())
}

// keepProject sets the project v works in in its cookie.
func (v view) keepProject(w http.ResponseWriter) { setPreference(w, projectCookie, v.Project.String()) }

// keepTabs sets v's open tabs in their cookie.
func (v view) keepTabs(w http.ResponseWriter) {
	kept := url.Values{"tab": v.Tabs, "active": {v.Active}}
	for name, record := range v.Records {
		kept.Set("record."+name, record)
	}
	setPreference(w, tabsCookie, kept.Encode())
}

// setPreference keeps a choice of the person's in the cookie called name for a year: the look, the tabs, the language. The page's script never
// reads it, and only this site is sent it.
func setPreference(w http.ResponseWriter, name, value string) {
	http.SetCookie(w, &http.Cookie{Name: name, Value: value, Path: "/", MaxAge: 365 * 24 * 60 * 60, HttpOnly: true, Secure: true, SameSite: http.SameSiteLaxMode})
}

// flag is a bar or the side menu as the cookie keeps it: "1" shown, "0" hidden.
func flag(shown bool) string {
	if shown {
		return "1"
	}
	return "0"
}

// opening is v with the tab named name open and in front.
func (v view) opening(name string) view {
	if !slices.Contains(v.Tabs, name) {
		v.Tabs = append(slices.Clone(v.Tabs), name)
	}
	v.Active = name
	return v
}

// recording is v with query kept as the record of the tab named name, when it is a tab opened for one and query names one.
func (v view) recording(name, query string) view {
	if !slices.Contains(recordTabs, name) || query == "" {
		return v
	}
	v.Records = maps.Clone(v.Records)
	if v.Records == nil {
		v.Records = map[string]string{}
	}
	v.Records[name] = query
	return v
}

// recordOf is r as the front tab's page reads it: with the record that tab keeps, when r names none.
func (v view) recordOf(r *http.Request) *http.Request {
	record, kept := v.Records[v.Active]
	if !kept || r.URL.RawQuery != "" {
		return r
	}
	again := r.Clone(r.Context())
	again.URL.RawQuery = record
	return again
}

// closing is v without the tab named name, and the record it kept; the last tab left comes to the front when it was.
func (v view) closing(name string) view {
	v.Records = maps.Clone(v.Records)
	delete(v.Records, name)
	v.Tabs = slices.DeleteFunc(slices.Clone(v.Tabs), func(open string) bool { return open == name })
	if v.Active == name {
		v.Active = ""
		if len(v.Tabs) > 0 {
			v.Active = v.Tabs[len(v.Tabs)-1]
		}
	}
	return v
}

// widths are the side menu's and the details panel's widths as the shell's style holds them; numbers of our own, so they are safe in a style.
func (v view) widths() templ.SafeCSS {
	return templ.SafeCSS(fmt.Sprintf("--side-w: %dpx; --details-w: %dpx", v.Width, v.DetailsWidth))
}
