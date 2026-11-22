// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"context"
	"net/http"
	"slices"
	"strings"
	"time"

	"github.com/a-h/templ"
	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/entries"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/notifications"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// noticesShown is Notifications as the app's screen lists them (UC-036): what the person was told, newest first, the search typed, and each
// project's time.
type noticesShown struct {
	all       []notifications.View
	text      string
	locations map[uuid.UUID]*time.Location
}

// shown is the notifications whose words or project hold the search, as the app's ListSearch keeps them.
func (s noticesShown) shown(p page) []notifications.View {
	return slices.DeleteFunc(slices.Clone(s.all), func(notice notifications.View) bool {
		return !searchHolds(s.text, p.words.Of("notifications."+notice.Kind.Name()), notice.ProjectName)
	})
}

// unread is how many the person has not opened yet, as the app's bell counts them.
func (s noticesShown) unread() int {
	count := 0
	for _, notice := range s.all {
		if !notice.IsRead {
			count++
		}
	}
	return count
}

// lineOf is what a notification's row says under its words, as the app's row does: its project and its day, in the project's time.
func (s noticesShown) lineOf(notice notifications.View) string {
	return notice.ProjectName + " · " + notice.At.In(s.locations[notice.ProjectID]).Format("2006-01-02")
}

// noticeTabs are the tabs a notification opens, as the app's _screens choose its screen: an entry sent for approval opens Approvals, a request
// for permissions Permission requests, a request decided My permissions; any other about an entry opens Entries with the entry in the details
// panel; one about a project itself opens its Home.
var noticeTabs = map[string]string{"entrySubmitted": "approvals", "permissionRequested": "requests", "permissionRequestApproved": "myPermissions",
	"permissionRequestRejected": "myPermissions", "projectApproved": "home", "projectRejected": "home"}

// noticesOf is Notifications with the search text, read through notifications.ListOf, the rule the app's route answers. The error is the
// database's.
func (s site) noticesOf(ctx context.Context, p page, text string) (noticesShown, error) {
	read := s.Database.Queries()
	all, err := notifications.ListOf(ctx, read, p.caller.UserID)
	if err != nil {
		return noticesShown{}, err
	}
	shown := noticesShown{all: all, text: strings.TrimSpace(text), locations: map[uuid.UUID]*time.Location{}}
	for _, notice := range all {
		if _, known := shown.locations[notice.ProjectID]; !known {
			zone, err := zones.Of(ctx, read, notice.ProjectID)
			if err != nil {
				return noticesShown{}, err
			}
			shown.locations[notice.ProjectID] = zones.Location(zone)
		}
	}
	return shown, nil
}

// notificationsTab is Notifications in its tab (UC-036, redix-api #175), from the top bar, with or without a project.
func (s site) notificationsTab(r *http.Request, p page) (templ.Component, error) {
	shown, err := s.noticesOf(r.Context(), p, r.URL.Query().Get("text"))
	return noticesList(p, shown), err
}

// notificationsButton is the top bar's Notifications with how many are unread, which the shell asks once it is drawn (D2).
func (s site) notificationsButton(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	shown, err := s.noticesOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	draw(w, r, http.StatusOK, noticesCount(p, shown.unread()))
}

// openNotification marks the notification the path names as opened, through notifications.MarkRead, and opens what it is about, as the app's
// row does: its project chosen, the tab noticeTabs names in front, an entry's panel beside Entries, in the whole shell. A notification about a
// project the person may no longer open stays on Notifications, opened. 404 for one that is not theirs.
func (s site) openNotification(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	shown, err := s.noticesOf(r.Context(), p, "")
	if err != nil {
		failed(w)
		return
	}
	noticeID, _ := uuid.Parse(r.PathValue("id"))
	at := slices.IndexFunc(shown.all, func(notice notifications.View) bool { return notice.ID == noticeID })
	if at < 0 {
		http.NotFound(w, r)
		return
	}
	notice := shown.all[at]
	if _, err := notifications.MarkRead(r.Context(), s.Database, notice.ID, p.caller.UserID); err != nil {
		failed(w)
		return
	}
	if _, openable := p.openable(notice.ProjectID); !openable {
		if shown, err = s.noticesOf(r.Context(), p, ""); err != nil {
			failed(w)
			return
		}
		draw(w, r, http.StatusOK, noticesList(p, shown), noticesCountBeside(p, shown.unread()))
		return
	}
	tab, named := noticeTabs[notice.Kind.Name()]
	if !named {
		tab = "entries"
	}
	if p.view.Project != notice.ProjectID {
		p.view.Tabs = []string{"home"}
	}
	p.view.Project = notice.ProjectID
	p.view = p.view.opening(tab)
	if p, err = s.open(r, p, p.caller); err != nil {
		failed(w)
		return
	}
	if !named {
		row, found, err := entries.RowOf(r.Context(), s.Database.Queries(), p.project.ID, p.caller, notice.EntryID)
		if err != nil {
			failed(w)
			return
		}
		if found {
			p.details = entryPanel(p, row, said{})
		}
	}
	p.view.keepProject(w)
	p.view.keepTabs(w)
	reshell(w, r, p)
}
