// Package access owns what each person may do, everywhere and in each project, so the server guards what a role may do whatever the app shows
// (FR-03, NFR-08, BR-32 to BR-34, redix-api #120).
package access

import (
	"context"
	"encoding/json"
	"slices"
	"sync"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
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
)

// MarshalJSON writes the permission as the API's enums are written, camelCase: "approveProjects".
func (p Permission) MarshalJSON() ([]byte, error) { return json.Marshal(web.Camel(string(p))) }

// Access is what one person holds: everywhere, in each project, and the partners chosen for them (UserAccess). Nothing in it changes once read.
type Access struct {
	everywhere []Permission
	byProject  map[uuid.UUID][]Permission
	chosen     map[uuid.UUID][]uuid.UUID
}

// Holds says whether the person holds permission in projectID, or everywhere: a permission counts only in the project it was ticked in, so one project
// never opens another (BR-33).
func (a Access) Holds(projectID uuid.UUID, permission Permission) bool {
	return slices.Contains(a.everywhere, permission) || slices.Contains(a.byProject[projectID], permission)
}

// Everywhere is what the person holds across the system, which the session tells the app; empty, never nil, when nothing.
func (a Access) Everywhere() []Permission { return append([]Permission{}, a.everywhere...) }

// ChosenIn is the partners of projectID chosen for the person (#106); empty when none are.
func (a Access) ChosenIn(projectID uuid.UUID) []uuid.UUID { return a.chosen[projectID] }

// Book reads each person's access at most once and keeps it until a change to who holds what, so a change counts on the person's next request
// (AccessCache, #120 D1).
type Book struct {
	read *store.Queries
	mu   sync.Mutex
	kept map[uuid.UUID]Access
}

// NewBook keeps the access read from database.
func NewBook(database db.DB) *Book {
	return &Book{read: store.New(database.Read), kept: map[uuid.UUID]Access{}}
}

// Of is what userID holds: the kept access, or one read now.
func (b *Book) Of(ctx context.Context, userID uuid.UUID) (Access, error) {
	b.mu.Lock()
	kept, ok := b.kept[userID]
	b.mu.Unlock()
	if ok {
		return kept, nil
	}
	read, err := b.readOf(ctx, userID)
	if err != nil {
		return Access{}, err
	}
	b.mu.Lock()
	b.kept[userID] = read
	b.mu.Unlock()
	return read, nil
}

// Forget drops every kept access; each domain that changes who holds what calls it after its commit (AccessChanges).
func (b *Book) Forget() {
	b.mu.Lock()
	b.kept = map[uuid.UUID]Access{}
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
	access := Access{byProject: map[uuid.UUID][]Permission{}, chosen: map[uuid.UUID][]uuid.UUID{}}
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
