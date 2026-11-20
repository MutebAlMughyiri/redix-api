// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"fmt"
	"net/http"
	"net/url"
	"slices"
	"strconv"

	"github.com/a-h/templ"
)

// view is how a person left the shell: its bars, its side menu and that menu's width, and its open tabs, kept in cookies and drawn by the server,
// so nothing flashes on the next open (#161 R18).
type view struct {
	Top, Status, Side bool
	Width             int
	Tabs              []string
	Active            string
}

const (
	// lookCookie keeps the bars, the side menu and its width, which the View menu changes; tabsCookie keeps the open tabs. Each is written only by
	// what changes it, so neither save carries a stale copy of the other over it: one cookie for both lost a tab opened while the side menu's
	// line was dragged, the drag's save leaving before the tab's answer came back.
	lookCookie = "redix_view"
	tabsCookie = "redix_tabs"
	// narrowest and widest are the side menu's width limits; the content keeps the rest (#161 R10).
	narrowest, widest = 180, 480
)

// viewOf is the view r's cookies keep; every bar and the side menu shown until a person hides them. A tab no feature opens is dropped.
func viewOf(r *http.Request) view {
	look := preference(r, lookCookie, url.Values{"top": {"1"}, "status": {"1"}, "side": {"1"}, "width": {"248"}})
	tabs := preference(r, tabsCookie, url.Values{})
	width, _ := strconv.Atoi(look.Get("width"))
	v := view{Top: look.Get("top") == "1", Status: look.Get("status") == "1", Side: look.Get("side") == "1", Width: min(max(width, narrowest), widest)}
	for _, name := range tabs["tab"] {
		if _, built := contents[name]; built && !slices.Contains(v.Tabs, name) {
			v.Tabs = append(v.Tabs, name)
		}
	}
	if slices.Contains(v.Tabs, tabs.Get("active")) {
		v.Active = tabs.Get("active")
	}
	return v
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

// keepLook sets v's bars, side menu and width in their cookie.
func (v view) keepLook(w http.ResponseWriter) {
	setPreference(w, lookCookie, url.Values{"top": {flag(v.Top)}, "status": {flag(v.Status)}, "side": {flag(v.Side)}, "width": {strconv.Itoa(v.Width)}}.Encode())
}

// keepTabs sets v's open tabs in their cookie.
func (v view) keepTabs(w http.ResponseWriter) {
	setPreference(w, tabsCookie, url.Values{"tab": v.Tabs, "active": {v.Active}}.Encode())
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

// closing is v without the tab named name; the last tab left comes to the front when it was.
func (v view) closing(name string) view {
	v.Tabs = slices.DeleteFunc(slices.Clone(v.Tabs), func(open string) bool { return open == name })
	if v.Active == name {
		v.Active = ""
		if len(v.Tabs) > 0 {
			v.Active = v.Tabs[len(v.Tabs)-1]
		}
	}
	return v
}

// width is the side menu's width as the shell's style holds it; a number of our own, so it is safe in a style.
func (v view) width() templ.SafeCSS { return templ.SafeCSS(fmt.Sprintf("--side-w: %dpx", v.Width)) }
