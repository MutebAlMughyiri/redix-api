// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package access owns what each person may do, everywhere and in each project, so the server guards what a role may do whatever the app shows
// (FR-03, NFR-08, BR-32 to BR-34, redix-api #120).
package access

import (
	"context"
	"encoding/json"
	"fmt"
	"slices"
	"strings"
	"sync"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/encode"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Permission is what a route can ask of its caller (BRS section 2). Who holds each one is data, ticked per person per project.
type Permission string

// The permissions, as the database keeps their names.
const (
	ApproveProjects       Permission = "ApproveProjects"
	ManageUsers           Permission = "ManageUsers"
	ReadProject           Permission = "ReadProject"
	SeeAllCapital         Permission = "SeeAllCapital"
	SeeChosenCapital      Permission = "SeeChosenCapital"
	ManageProject         Permission = "ManageProject"
	ManageStructure       Permission = "ManageStructure"
	ManageSettings        Permission = "ManageSettings"
	AddSupplier           Permission = "AddSupplier"
	ChargeAnyAccount      Permission = "ChargeAnyAccount"
	RecordEntries         Permission = "RecordEntries"
	ManageAnyEntry        Permission = "ManageAnyEntry"
	DeleteEntries         Permission = "DeleteEntries"
	RemoveApprovedEntries Permission = "RemoveApprovedEntries"
	AttachFiles           Permission = "AttachFiles"
	ApproveEntries        Permission = "ApproveEntries"
	MoveMoney             Permission = "MoveMoney"
	SeeReports            Permission = "SeeReports"
	GenerateReports       Permission = "GenerateReports"
	SeeProjectHistory     Permission = "SeeProjectHistory"
	ManageProjectFiles    Permission = "ManageProjectFiles"
)

// All is every permission in the order .NET's enum gave them, then those added since: History keeps one as its place here (its number), so a
// new one goes last (SeeProjectHistory, UC-069; ManageProjectFiles, redix-api #130). A list of them is ordered so.
var All = []Permission{ApproveProjects, ManageUsers, ReadProject, SeeAllCapital, SeeChosenCapital, ManageProject, ManageStructure, ManageSettings, AddSupplier,
	ChargeAnyAccount, RecordEntries, ManageAnyEntry, DeleteEntries, RemoveApprovedEntries, AttachFiles, ApproveEntries, MoveMoney, SeeReports, GenerateReports,
	SeeProjectHistory, ManageProjectFiles}

// Number is the permission's place in All, as History keeps it.
func (p Permission) Number() int { return slices.Index(All, p) }

// Named is the permission name names, whatever its case, as Enum.TryParse read it with ignoreCase.
func Named(name string) (Permission, bool) {
	for _, permission := range All {
		if strings.EqualFold(string(permission), name) {
			return permission, true
		}
	}
	return "", false
}

// UnmarshalJSON reads a permission by its name, whatever its case, or by its number, as JsonStringEnumConverter did.
func (p *Permission) UnmarshalJSON(data []byte) error {
	var name string
	if json.Unmarshal(data, &name) == nil {
		if named, ok := Named(name); ok {
			*p = named
			return nil
		}
		return fmt.Errorf("access: no permission %q", name)
	}
	var number int
	if err := json.Unmarshal(data, &number); err != nil || number < 0 || number >= len(All) {
		return fmt.Errorf("access: no permission %s", data)
	}
	*p = All[number]
	return nil
}

// MarshalJSON writes the permission as the API's enums are written, camelCase: "approveProjects".
func (p Permission) MarshalJSON() ([]byte, error) { return encode.JSON(web.Camel(string(p))) }

// Access is what one person holds: everywhere, in each project, the partners chosen for them (UserAccess), and the tables unticked for them
// (#106). Nothing in it changes once read.
type Access struct {
	everywhere []Permission
	byProject  map[uuid.UUID][]Permission
	chosen     map[uuid.UUID][]uuid.UUID
	unseen     map[uuid.UUID][]uuid.UUID
}

// Sees says whether the person sees tableID, an item's group or a contract, in projectID: every table but those unticked for them (#106 D1, R8).
func (a Access) Sees(projectID, tableID uuid.UUID) bool {
	return !slices.Contains(a.unseen[projectID], tableID)
}

// SeesEveryTable says whether nothing is unticked for the person in projectID, so the periodic report, its files and the VAT statement are theirs
// (#106 R7, R12).
func (a Access) SeesEveryTable(projectID uuid.UUID) bool { return len(a.unseen[projectID]) == 0 }

// UnseenIn is the tables unticked for the person in projectID, which صلاحياتي shows unticked (#106 D9).
func (a Access) UnseenIn(projectID uuid.UUID) []uuid.UUID {
	return append([]uuid.UUID{}, a.unseen[projectID]...)
}

// Unseen is the tables unticked for the person in projectID as the JSON list the sums leave out: [] when they see every table (#106 R6, D6).
func (a Access) Unseen(projectID uuid.UUID) string {
	listed, _ := encode.JSON(a.UnseenIn(projectID))
	return string(listed)
}

// Everything is the JSON list leaving nothing out, for a figure that is everyone's: the periodic report and its files (#106 R7).
const Everything = "[]"

// Holds says whether the person holds permission in projectID, or everywhere: a permission counts only in the project it was ticked in, so one project
// never opens another (BR-33).
func (a Access) Holds(projectID uuid.UUID, permission Permission) bool {
	return slices.Contains(a.everywhere, permission) || slices.Contains(a.byProject[projectID], permission)
}

// SeesCapitalOf says whether the person may see the partner's capital in projectID (PartnerSight, BR-31): every partner's with SeeAllCapital, only the
// partners chosen for them with SeeChosenCapital.
func (a Access) SeesCapitalOf(projectID, partnerID uuid.UUID) bool {
	return a.Holds(projectID, SeeAllCapital) || (a.Holds(projectID, SeeChosenCapital) && slices.Contains(a.chosen[projectID], partnerID))
}

// In is what the person holds in projectID: what they hold everywhere first, then the project's own, each once (UserAccess.In); empty, never
// nil, when nothing.
func (a Access) In(projectID uuid.UUID) []Permission {
	held := a.Everywhere()
	for _, permission := range a.byProject[projectID] {
		if !slices.Contains(held, permission) {
			held = append(held, permission)
		}
	}
	return held
}

// Everywhere is what the person holds across the system, which the session tells the app; empty, never nil, when nothing.
func (a Access) Everywhere() []Permission { return append([]Permission{}, a.everywhere...) }

// ChosenIn is the partners of projectID chosen for the person (#106); empty when none are.
func (a Access) ChosenIn(projectID uuid.UUID) []uuid.UUID { return a.chosen[projectID] }

// Book reads each person's access at most once and keeps it until the next committed write, so a change counts on the person's next request
// (AccessCache, AccessChanges, #120 D1). Kept, it answers in about 67 ns; read again, in about 77 µs (measured 16 Nov 2026).
type Book struct {
	read       *store.Queries
	mu         sync.Mutex
	kept       map[uuid.UUID]Access
	generation uint64
}

// NewBook keeps the access read from database, forgetting it after every write committed to it.
func NewBook(database db.DB) *Book {
	book := &Book{read: database.Queries(), kept: map[uuid.UUID]Access{}}
	database.AfterEveryWrite(book.forget)
	return book
}

// Of is what userID holds: the kept access, or one read now.
func (b *Book) Of(ctx context.Context, userID uuid.UUID) (Access, error) {
	b.mu.Lock()
	kept, ok := b.kept[userID]
	generation := b.generation
	b.mu.Unlock()
	if ok {
		return kept, nil
	}
	read, err := b.readOf(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	b.mu.Lock()
	if b.generation == generation {
		b.kept[userID] = read
	}
	b.mu.Unlock()
	return read, nil
}

// forget drops every kept access, and stops one being read across a write from being kept.
func (b *Book) forget() {
	b.mu.Lock()
	b.kept = map[uuid.UUID]Access{}
	b.generation++
	b.mu.Unlock()
}

func (b *Book) readOf(ctx context.Context, userID uuid.UUID) (Access, error) {
	everywhere, err := b.read.HeldEverywhere(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	inProjects, err := b.read.HeldInProjects(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	chosen, err := b.read.ChosenPartners(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	unseen, err := b.read.UnseenTables(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	access := Access{byProject: map[uuid.UUID][]Permission{}, chosen: map[uuid.UUID][]uuid.UUID{}, unseen: map[uuid.UUID][]uuid.UUID{}}
	for _, row := range unseen {
		access.unseen[row.ProjectID] = append(access.unseen[row.ProjectID], row.TableID)
	}
	for _, permission := range everywhere {
		access.everywhere = append(access.everywhere, Permission(permission))
	}
	for _, row := range inProjects {
		access.byProject[row.ProjectID] = append(access.byProject[row.ProjectID], Permission(row.Permission))
	}
	for _, row := range chosen {
		access.chosen[row.ProjectID] = append(access.chosen[row.ProjectID], row.PartnerID)
	}
	return access, nil
}
