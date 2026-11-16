// Package projects keeps the projects themselves: listing them, starting and registering one, its name, description and units, closing it, the
// one a person opens straight into, and the admin's approval (ProjectsEndpoints, RegistrationEndpoints, redix-api #140).
package projects

import (
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"slices"
	"strings"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
)

// Where a project stands (ProjectStatus), as the database keeps it.
const (
	active          int64 = 0
	finished        int64 = 1
	pendingApproval int64 = 2
	rejected        int64 = 3
)

// statusNames are the statuses as the API writes them.
var statusNames = map[int64]string{active: "active", finished: "finished", pendingApproval: "pendingApproval", rejected: "rejected"}

const (
	descriptionMaxLength = 2000
	reasonMaxLength      = 500
)

// The refusals a change to a project answers with (ProjectChangeOutcome).
const (
	projectFinished    = "projectFinished"
	invalidName        = naming.Invalid
	invalidDescription = "invalidDescription"
	unitNameTaken      = "unitNameTaken"
	unknownUnit        = "unknownUnit"
	pendingEntries     = "pendingEntries"
	notPending         = "notPending"
	invalidReason      = "invalidReason"
)

// newProject is a project as Project.New makes it: its name, its description and its units, each trimmed; "" when it may be kept, or the last
// refusal found - invalidName, invalidDescription, or unitNameTaken.
func newProject(name, description string, units []string) (string, string, []string, string) {
	kept, ok := naming.Named(name)
	refusal := ""
	trimmed := strings.TrimSpace(description)
	switch {
	case !ok:
		refusal = invalidName
	case utf8.RuneCountInString(trimmed) > descriptionMaxLength:
		refusal = invalidDescription
	}
	var names []string
	for _, unit := range units {
		unitName, ok := naming.Named(unit)
		switch {
		case !ok:
			refusal = invalidName
		case slices.Contains(names, unitName):
			refusal = unitNameTaken
		default:
			names = append(names, unitName)
		}
	}
	return kept, trimmed, names, refusal
}

// declaredPartners are the partners named when a project is started, trimmed, each a name Named keeps (PartnersEndpoints.DeclaredWith).
func declaredPartners(names []string) ([]string, bool) {
	var kept []string
	for _, typed := range names {
		name, ok := naming.Named(typed)
		if !ok {
			return nil, false
		}
		kept = append(kept, name)
	}
	return kept, true
}

// projectFields is a project as History keeps it, under the C# property names.
func projectFields(project store.Project) map[string]any {
	return map[string]any{
		"Name": project.Name, "StartDate": project.StartDate, "Status": project.Status, "RejectionReason": project.RejectionReason,
		"Description": project.Description, "IsTemplate": project.IsTemplate, "LastEntryNumber": project.LastEntryNumber,
	}
}

// projectChange is what changed on a project, as History keeps it.
func projectChange(before, after store.Project) history.Change {
	was, is := history.Changed(projectFields(before), projectFields(after))
	return history.Change{Table: "Projects", RecordID: after.ID, ProjectID: after.ID, Before: was, After: is}
}

func unitFields(unit store.Unit) map[string]any {
	return map[string]any{"ProjectId": unit.ProjectID, "Name": unit.Name, "SortOrder": unit.SortOrder}
}
