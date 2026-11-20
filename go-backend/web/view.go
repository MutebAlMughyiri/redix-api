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

// view is how a person left the shell: its bars, its side menu and that menu's width, and its open tabs, kept in a cookie and drawn by the server,
// so nothing flashes on the next open (#161 R18).
type view struct {
	Top, Status, Side bool
	Width             int
	Tabs              []string
	Active            string
}

const (
	viewCookie = "redix_view"
	// narrowest and widest are the side menu's width limits; the content keeps the rest (#161 R10).
	narrowest, widest = 180, 480
)

// viewOf is the view r's cookie keeps; every bar and the side menu shown until a person hides them. A tab no feature opens is dropped.
func viewOf(r *http.Request) view {
	kept := url.Values{"top": {"1"}, "status": {"1"}, "side": {"1"}, "width": {"248"}}
	if cookie, err := r.Cookie(viewCookie); err == nil {
		if values, err := url.ParseQuery(cookie.Value); err == nil {
			kept = values
		}
	}
	width, _ := strconv.Atoi(kept.Get("width"))
	v := view{Top: kept.Get("top") == "1", Status: kept.Get("status") == "1", Side: kept.Get("side") == "1", Width: min(max(width, narrowest), widest)}
	for _, name := range kept["tab"] {
		if _, built := contents[name]; built && !slices.Contains(v.Tabs, name) {
			v.Tabs = append(v.Tabs, name)
		}
	}
	if slices.Contains(v.Tabs, kept.Get("active")) {
		v.Active = kept.Get("active")
	}
	return v
}

// keep sets v in its cookie for a year.
func (v view) keep(w http.ResponseWriter) {
	values := url.Values{"top": {flag(v.Top)}, "status": {flag(v.Status)}, "side": {flag(v.Side)}, "width": {strconv.Itoa(v.Width)}, "tab": v.Tabs, "active": {v.Active}}
	http.SetCookie(w, &http.Cookie{Name: viewCookie, Value: values.Encode(), Path: "/", MaxAge: 365 * 24 * 60 * 60, HttpOnly: true, Secure: true,
		SameSite: http.SameSiteLaxMode})
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
