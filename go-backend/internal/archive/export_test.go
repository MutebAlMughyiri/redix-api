// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package archive

// Placed says whether the table called name is listed: the project's own, kept, or shared with its reason (AC-73.7).
func Placed(name string) bool {
	_, listed := tableNamed(name)
	_, isShared := shared[name]
	return listed || isShared
}

// Archived is how many tables an archive holds.
func Archived() int { return len(tables) }

// Retrieved is the name and the scope of each table a retrieve replaces, for the tests to read them back (AC-73.9).
func Retrieved() [][2]string {
	var replaced [][2]string
	for _, t := range retrieved {
		replaced = append(replaced, [2]string{t.name, t.scope})
	}
	return replaced
}
