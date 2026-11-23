// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package access_test

import (
	"encoding/json"
	"regexp"
	"strings"
	"testing"

	"github.com/google/uuid"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// documented is every route of the API document with what it says of itself.
func documented(t *testing.T, api *testapi.API) map[string]map[string]struct {
	Description string `json:"description"`
} {
	t.Helper()
	var document struct {
		Paths map[string]map[string]struct {
			Description string `json:"description"`
		} `json:"paths"`
	}
	if err := json.Unmarshal(api.Send("GET", "/openapi/v1.json", "").Body.Bytes(), &document); err != nil {
		t.Fatal(err)
	}
	return document.Paths
}

// gatedRoute is a route the API document says needs a permission, at an address with made-up ids: the permission is asked before any is read.
type gatedRoute struct {
	method, path, address string
	permission            access.Permission
	inProject             bool
}

// gatedRoutes are the routes the document says need a permission ("needs the ManageSettings permission"), a project's in project: whoever holds a
// project's permission holds it there, ApproveProjects everywhere, as it is never ticked in a project.
func gatedRoutes(t *testing.T, api *testapi.API, project string) []gatedRoute {
	t.Helper()
	needs, parameter := regexp.MustCompile(`needs the (\w+) permission`), regexp.MustCompile(`\{[^}]+\}`)
	var routes []gatedRoute
	for path, operations := range documented(t, api) {
		for method, operation := range operations {
			found := needs.FindStringSubmatch(operation.Description)
			if found == nil {
				continue
			}
			permission := access.Permission(found[1])
			inProject := strings.HasPrefix(path, "/projects/{projectId}") && permission != access.ApproveProjects
			address := path
			if inProject {
				address = strings.Replace(address, "{projectId}", project, 1)
			}
			address = parameter.ReplaceAllStringFunc(address, func(name string) string {
				if name == "{number}" {
					return "1"
				}
				return uuid.NewString()
			})
			routes = append(routes, gatedRoute{strings.ToUpper(method), path, address, permission, inProject})
		}
	}
	return routes
}

// FR-03, BR-29, BR-33: every route that needs a permission refuses whoever lacks it with 403, and lets through whoever holds it - in the project
// for a project's route, everywhere for the rest -, read from the API document so a new route is swept the day it is added. Every refusal is
// asked first, while nothing has changed; then every route with each permission held, each holder's permissions given again before each, so
// what one route changes never decides another's answer.
func TestFR03_EveryRouteRefusesWhoeverLacksItsPermissionAndOpensOnceItIsHeld(t *testing.T) {
	api := testapi.New(t)
	_, path := api.ApprovedProject()
	project := strings.TrimPrefix(path, "/projects/")
	routes := gatedRoutes(t, api, project)
	if len(routes) < 100 {
		t.Fatalf("%d routes need a permission", len(routes))
	}
	everywhere, everywhereToken := api.Person("Everywhere")
	member, memberToken := api.Person("Member")
	giveAll := func() {
		for _, permission := range access.All {
			if permission.Number() < 19 {
				api.Exec(`INSERT OR IGNORE INTO system_permissions (id, user_id, permission) VALUES (?, ?, ?)`, uuid.NewString(), everywhere, permission)
			}
			if permission != access.ApproveProjects {
				api.Exec(`INSERT OR IGNORE INTO project_permissions (id, project_id, user_id, permission) VALUES (?, ?, ?, ?)`, uuid.NewString(), project, member,
					permission)
			}
		}
	}
	send := func(route gatedRoute) int {
		token := everywhereToken
		if route.inProject {
			token = memberToken
		}
		return api.Send(route.method, route.address, "", "Authorization", "Bearer "+token).Code
	}
	giveAll()
	for _, route := range routes {
		if route.inProject {
			api.Exec(`DELETE FROM project_permissions WHERE user_id = ? AND permission = ?`, member, route.permission)
		} else {
			api.Exec(`DELETE FROM system_permissions WHERE user_id = ? AND permission = ?`, everywhere, route.permission)
		}
		if code := send(route); code != 403 {
			t.Errorf("%s %s without %s answered %d, want 403", route.method, route.path, route.permission, code)
		}
		giveAll()
	}
	for _, route := range routes {
		giveAll()
		if code := send(route); code == 403 {
			t.Errorf("%s %s holding %s answered 403", route.method, route.path, route.permission)
		}
	}
}

// FR-03: every route of a project needs a permission there, as its document says.
func TestFR03_EveryRouteOfAProjectNeedsAPermission(t *testing.T) {
	for path, operations := range documented(t, testapi.New(t)) {
		for method, operation := range operations {
			if strings.HasPrefix(path, "/projects/{projectId}") && !strings.Contains(operation.Description, "permission") {
				t.Errorf("%s %s names no permission", strings.ToUpper(method), path)
			}
		}
	}
}
