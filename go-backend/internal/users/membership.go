// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package users

import (
	"context"
	"slices"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/google/uuid"
)

// capitalTicks are the two ways of seeing a project's capital; a person holds one or the other (BR-31, #106).
var capitalTicks = []access.Permission{access.SeeAllCapital, access.SeeChosenCapital}

// roleNameMaxLength is the longest free-text role name (ProjectRoleName.MaxLength).
const roleNameMaxLength = 50

// change is one write to who is in a project: the queries of its transaction, and who makes it and when, which every line it adds to History names.
type change struct {
	ctx   context.Context
	q     *store.Queries
	actor uuid.UUID
	now   moment.Moment
}

func (c change) record(action history.Action, line history.Change) error {
	return history.Write(c.ctx, c.q, c.now, c.actor, action, line)
}

// addPermission ticks permission for userID in projectID.
func (c change) addPermission(projectID, userID uuid.UUID, permission access.Permission) error {
	row := store.ProjectPermission{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, UserID: userID, Permission: string(permission)}
	if err := c.q.AddPermission(c.ctx, store.AddPermissionParams{ID: row.ID, ProjectID: projectID, UserID: userID, Permission: row.Permission}); err != nil {
		return err
	}
	return c.record(history.AssignProject, history.Change{Table: "ProjectPermissions", RecordID: row.ID, ProjectID: projectID, After: permissionFields(row)})
}

func (c change) removePermission(row store.ProjectPermission) error {
	if err := c.q.RemovePermission(c.ctx, row.ID); err != nil {
		return err
	}
	return c.record(history.RemoveProject, history.Change{Table: "ProjectPermissions", RecordID: row.ID, ProjectID: row.ProjectID, Before: permissionFields(row)})
}

func permissionFields(row store.ProjectPermission) map[string]any {
	permission, _ := access.Named(row.Permission)
	return map[string]any{"ProjectId": row.ProjectID, "UserId": row.UserID, "Permission": permission.Number()}
}

// give ticks given unless held already; ticking one way of seeing capital takes the other away, and seeing all of it forgets the partners chosen
// before (ProjectUsersEndpoints.GiveAsync).
func (c change) give(projectID, userID uuid.UUID, given access.Permission) error {
	held, err := c.q.PermissionsOf(c.ctx, store.PermissionsOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return err
	}
	if !slices.ContainsFunc(held, func(row store.ProjectPermission) bool { return row.Permission == string(given) }) {
		if err := c.addPermission(projectID, userID, given); err != nil {
			return err
		}
	}
	if !slices.Contains(capitalTicks, given) {
		return nil
	}
	for _, other := range held {
		if other.Permission != string(given) && slices.Contains(capitalTicks, access.Permission(other.Permission)) {
			if err := c.removePermission(other); err != nil {
				return err
			}
		}
	}
	if given == access.SeeAllCapital {
		return c.clearChosen(projectID, userID, uuid.Nil)
	}
	return nil
}

// choose shows partnerID's capital to userID (CapitalSight.Choose).
func (c change) choose(projectID, userID, partnerID uuid.UUID) error {
	row := store.ChosenPartner{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, UserID: userID, PartnerID: partnerID}
	if err := c.q.AddChosen(c.ctx, store.AddChosenParams{ID: row.ID, ProjectID: projectID, UserID: userID, PartnerID: partnerID}); err != nil {
		return err
	}
	return c.record(history.AssignProject, history.Change{Table: "ChosenPartners", RecordID: row.ID, ProjectID: projectID, After: chosenFields(row)})
}

// clearChosen forgets the partners chosen for userID in projectID, or only partnerID when it is not uuid.Nil (CapitalSight.ClearAsync).
func (c change) clearChosen(projectID, userID, partnerID uuid.UUID) error {
	chosen, err := c.q.ChosenOf(c.ctx, store.ChosenOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return err
	}
	for _, row := range chosen {
		if partnerID != uuid.Nil && row.PartnerID != partnerID {
			continue
		}
		if err := c.q.RemoveChosen(c.ctx, row.ID); err != nil {
			return err
		}
		if err := c.record(history.RemoveProject, history.Change{Table: "ChosenPartners", RecordID: row.ID, ProjectID: projectID, Before: chosenFields(row)}); err != nil {
			return err
		}
	}
	return nil
}

func chosenFields(row store.ChosenPartner) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "UserId": row.UserID, "PartnerId": row.PartnerID}
}

// nameRole gives userID the free-text role name roleName in projectID, "" taking it away (ProjectRoleName.NameAsync).
func (c change) nameRole(projectID, userID uuid.UUID, roleName string) error {
	named, err := c.q.RoleNameOf(c.ctx, store.RoleNameOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return err
	}
	if len(named) == 0 {
		if roleName == "" {
			return nil
		}
		row := store.ProjectRoleName{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, UserID: userID, RoleName: roleName}
		if err := c.q.AddRoleName(c.ctx, store.AddRoleNameParams{ID: row.ID, ProjectID: projectID, UserID: userID, RoleName: roleName}); err != nil {
			return err
		}
		return c.record(history.Create, history.Change{Table: "ProjectRoleNames", RecordID: row.ID, ProjectID: projectID, After: roleFields(row)})
	}
	row := named[0]
	switch {
	case row.RoleName == roleName:
		return nil
	case roleName == "":
		if err := c.q.RemoveRoleName(c.ctx, row.ID); err != nil {
			return err
		}
		return c.record(history.Delete, history.Change{Table: "ProjectRoleNames", RecordID: row.ID, ProjectID: projectID, Before: roleFields(row)})
	default:
		if err := c.q.SetRoleName(c.ctx, store.SetRoleNameParams{ID: row.ID, RoleName: roleName}); err != nil {
			return err
		}
		return c.record(history.Update, history.Change{Table: "ProjectRoleNames", RecordID: row.ID, ProjectID: projectID,
			Before: map[string]any{"RoleName": row.RoleName}, After: map[string]any{"RoleName": roleName}})
	}
}

func roleFields(row store.ProjectRoleName) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "UserId": row.UserID, "RoleName": row.RoleName}
}

// disable makes userID inactive in projectID, unless they are already (#126).
func (c change) disable(projectID, userID uuid.UUID) error {
	inactive, err := c.q.InactiveOf(c.ctx, store.InactiveOfParams{ProjectID: projectID, UserID: userID})
	if err != nil || len(inactive) > 0 {
		return err
	}
	row := store.InactiveMember{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, UserID: userID}
	if err := c.q.AddInactive(c.ctx, store.AddInactiveParams{ID: row.ID, ProjectID: projectID, UserID: userID}); err != nil {
		return err
	}
	return c.record(history.Disable, history.Change{Table: "InactiveMembers", RecordID: row.ID, ProjectID: projectID, After: inactiveFields(row)})
}

// endInactive makes userID active in projectID again, recording action (InactiveMember.EndAsync).
func (c change) endInactive(projectID, userID uuid.UUID, action history.Action) error {
	inactive, err := c.q.InactiveOf(c.ctx, store.InactiveOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return err
	}
	for _, row := range inactive {
		if err := c.q.RemoveInactive(c.ctx, row.ID); err != nil {
			return err
		}
		if err := c.record(action, history.Change{Table: "InactiveMembers", RecordID: row.ID, ProjectID: projectID, Before: inactiveFields(row)}); err != nil {
			return err
		}
	}
	return nil
}

func inactiveFields(row store.InactiveMember) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "UserId": row.UserID}
}

// isTheLastManager says whether userID is the only one active in projectID who manages its users, so they may not lose that or be made inactive
// (ProjectManagers.IsTheLastAsync).
func isTheLastManager(ctx context.Context, q *store.Queries, projectID, userID uuid.UUID) (bool, error) {
	held, err := q.PermissionsOf(ctx, store.PermissionsOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return false, err
	}
	inactive, err := q.InactiveOf(ctx, store.InactiveOfParams{ProjectID: projectID, UserID: userID})
	if err != nil {
		return false, err
	}
	manages := slices.ContainsFunc(held, func(row store.ProjectPermission) bool { return row.Permission == string(access.ManageUsers) })
	if !manages || len(inactive) > 0 {
		return false, nil
	}
	others, err := q.OtherActiveManagers(ctx, store.OtherActiveManagersParams{ProjectID: projectID, UserID: userID})
	return others == 0, err
}

// TickStarter gives whoever starts or registers a project every permission in it but SeeChosenCapital and RemoveApprovedEntries, each recorded as
// theirs (ProjectsEndpoints.AddWithManager, ProjectPermission.OfWhoeverStarts). They count once the admin approves the project (BR-34).
func TickStarter(ctx context.Context, q *store.Queries, projectID, managerID uuid.UUID, now moment.Moment) error {
	c := change{ctx: ctx, q: q, actor: managerID, now: now}
	for _, permission := range access.All {
		if permission == access.ApproveProjects || permission == access.SeeChosenCapital || permission == access.RemoveApprovedEntries {
			continue
		}
		if err := c.addPermission(projectID, managerID, permission); err != nil {
			return err
		}
	}
	return nil
}
