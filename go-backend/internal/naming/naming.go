// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package naming is the rule of a name given to anything in a project - the project, a unit, a partner, a category, an item, a subgroup, a column,
// a payment method (Project.IsAcceptableName, ProjectChanges.WithNameAsync): one owner, which every domain asks.
package naming

import (
	"strings"
	"unicode/utf8"
)

// maxLength is the longest name (Project.NameMaxLength).
const maxLength = 100

// Invalid is the code a name Named refuses answers with, 400.
const Invalid = "invalidName"

// Named is typed trimmed, and whether it may name something of a project: one to a hundred characters.
func Named(typed string) (string, bool) {
	name := strings.TrimSpace(typed)
	length := utf8.RuneCountInString(name)
	return name, length > 0 && length <= maxLength
}
