// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import "slices"

// MenuNeeds is menuNeeds, the one owner of the permission each guarded tab needs, for the permission sweep (permissions_test.go).
var MenuNeeds = menuNeeds

// InSideMenu says whether the side menu lists the tab named name, or a page opens it.
func InSideMenu(name string) bool {
	return slices.ContainsFunc(sideMenu, func(item menuItem) bool { return item.name == name })
}
