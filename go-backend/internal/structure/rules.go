package structure

import (
	"context"
	"encoding/json"
	"net/http"
	"slices"
	"strings"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/history"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/naming"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// ItemType is what an item is for (ItemType, FR-38), which also says its side: an expense or revenue.
type ItemType int64

// The types of item, as the database keeps them.
const (
	Purchases ItemType = iota
	ContractorPayments
	Both
	Sale
	VatRefund
	OtherRevenue
	Capital
)

var itemTypes = web.Enum{"purchases", "contractorPayments", "both", "sale", "vatRefund", "otherRevenue", "capital"}

// MarshalJSON writes the type by its name.
func (t ItemType) MarshalJSON() ([]byte, error) { return itemTypes.Write(int64(t)) }

// UnmarshalJSON reads the type by its name or its number.
func (t *ItemType) UnmarshalJSON(data []byte) error {
	value, err := itemTypes.Read(data)
	*t = ItemType(value)
	return err
}

// IsRevenue says whether an item of this type is on the revenue side: its entries are money coming in (ItemTypes.IsRevenue).
func (t ItemType) IsRevenue() bool { return t >= Sale }

// OfferedFor says whether entering kind offers an item of this type, when the item is active (Item.IsOfferedFor, AC-05.3, AC-38.1): an expense,
// an item of its type or of both; revenue of any kind, every revenue item but a capital one; capital, every capital item (redix-api #109).
func (t ItemType) OfferedFor(kind ItemType) bool {
	if kind.IsRevenue() {
		return t.IsRevenue() && (t == Capital) == (kind == Capital)
	}
	return t == Both || t == kind
}

// hasSubgroupTables says whether an item of this type prints its subgroups as tables: every item but a capital one, whose tables are its partners
// alone (Item.HasSubgroupTables).
func (t ItemType) hasSubgroupTables() bool { return t != Capital }

// takesPurchases says whether an item of this type holds purchases, so a sub-table (Muteb, 7 Nov 2026).
func (t ItemType) takesPurchases() bool { return t == Purchases || t == Both }

// DataType is what a column holds (ColumnDataType, FR-41); a person chooses among the first five, and a file is the attachment's alone.
type DataType int64

const (
	text DataType = iota
	number
	amount
	date
	list
	file
)

var dataTypes = web.Enum{"text", "number", "amount", "date", "list", "file"}

// MarshalJSON writes the data type by its name.
func (d DataType) MarshalJSON() ([]byte, error) { return dataTypes.Write(int64(d)) }

// UnmarshalJSON reads the data type by its name or its number.
func (d *DataType) UnmarshalJSON(data []byte) error {
	value, err := dataTypes.Read(data)
	*d = DataType(value)
	return err
}

// Sign is the sign a list's choice gives the amount of the entry that chooses it (ChoiceSign, redix-api #129).
type Sign int64

const (
	unsigned Sign = iota
	plus
	minus
)

var signs = web.Enum{"none", "plus", "minus"}

// MarshalJSON writes the sign by its name.
func (s Sign) MarshalJSON() ([]byte, error) { return signs.Write(int64(s)) }

// UnmarshalJSON reads the sign by its name or its number.
func (s *Sign) UnmarshalJSON(data []byte) error {
	value, err := signs.Read(data)
	*s = Sign(value)
	return err
}

// choice is one choice of a list column, as the column's definition keeps it (ColumnChoice): its id - what an entry keeps once it chooses it -
// its name and its sign, under the C# names, its sign a number.
type choice struct {
	ID   uuid.UUID `json:"Id"`
	Name string    `json:"Name"`
	Sign int64     `json:"Sign"`
}

// choiceView is a choice as the API writes it.
type choiceView struct {
	ID   uuid.UUID `json:"id"`
	Name string    `json:"name"`
	Sign Sign      `json:"sign"`
}

// choiceRequest is a choice as the app sends it, for a new list or one gaining choices (ChoiceRequest).
type choiceRequest struct {
	Name string `json:"name"`
	Sign Sign   `json:"sign,omitzero"`
}

// The refusals of a change to a project's structure (StructureChangeOutcome); those of a row's state are conflicts, the rest bad requests
// (ProjectStructure.Refused).
const (
	invalidOrder          = "invalidOrder"
	generalSubgroup       = "generalSubgroup"
	subgroupInUse         = "subgroupInUse"
	itemHasEntries        = "itemHasEntries"
	itemHasContract       = "itemHasContract"
	itemHasPartners       = "itemHasPartners"
	capitalItemExists     = "capitalItemExists"
	invalidDataType       = "invalidDataType"
	invalidOptions        = "invalidOptions"
	coreColumn            = "coreColumn"
	signedListExists      = "signedListExists"
	revenueTakesNoColumns = "revenueTakesNoColumns"
	itemTakesNoPurchases  = "itemTakesNoPurchases"
)

var conflicts = []string{generalSubgroup, subgroupInUse, itemHasEntries, itemHasContract, itemHasPartners, capitalItemExists, coreColumn}

// refuse answers refusal: 409 for one about what a row holds, 400 for any other.
func refuse(w http.ResponseWriter, r *http.Request, refusal string) {
	status := http.StatusBadRequest
	if slices.Contains(conflicts, refusal) {
		status = http.StatusConflict
	}
	problem.Refuse(w, r, status, refusal)
}

// generalName is the name of an item's general group, the one an entry falls in when it names no other (Subgroup.GeneralName).
const generalName = ""

// column is what a default column and an item column hold alike (Column, FR-41): the rules of BR-20 and BR-21 are about it.
type column struct {
	Key           string
	Name          string
	DataType      DataType
	IsCore        bool
	IsRequired    bool
	ShowInReports bool
	SortOrder     int64
	Choices       []choice
}

const customKeyPrefix = "custom."

// entryFieldKeys are the columns an entry answers in a field of its own (Column.EntryFieldKeys, redix-api #129, R5).
var entryFieldKeys = []string{"description", "date", "supplier", "total", "amount", "account", "paymentMethod", "attachment", "contract", "outsideContract"}

func choicesIn(kept string) []choice {
	choices := []choice{}
	json.Unmarshal([]byte(kept), &choices)
	return choices
}

func (c column) choicesText() string {
	written, _ := json.Marshal(append([]choice{}, c.Choices...))
	return string(written)
}

func ofItemColumn(row store.ItemColumn) column {
	return column{Key: row.Key, Name: row.Name, DataType: DataType(row.DataType), IsCore: row.IsCore, IsRequired: row.IsRequired,
		ShowInReports: row.ShowInReports, SortOrder: row.SortOrder, Choices: choicesIn(row.Choices)}
}

func ofDefaultColumn(row store.DefaultColumn) column {
	return column{Key: row.Key, Name: row.Name, DataType: DataType(row.DataType), IsCore: row.IsCore, IsRequired: row.IsRequired,
		ShowInReports: row.ShowInReports, SortOrder: row.SortOrder, Choices: choicesIn(row.Choices)}
}

// onItem is the column as item itemID's, at its own place (ItemColumn.Of).
func (c column) onItem(id, itemID uuid.UUID, isHidden bool) store.ItemColumn {
	return store.ItemColumn{ID: id, ItemID: itemID, IsHidden: isHidden, Key: c.Key, Name: c.Name, DataType: int64(c.DataType), IsCore: c.IsCore,
		IsRequired: c.IsRequired, ShowInReports: c.ShowInReports, SortOrder: c.SortOrder, Choices: c.choicesText()}
}

// inTemplate is the column as projectID's default column for itemType.
func (c column) inTemplate(id, projectID uuid.UUID, itemType int64) store.DefaultColumn {
	return store.DefaultColumn{ID: id, ProjectID: projectID, ItemType: itemType, Key: c.Key, Name: c.Name, DataType: int64(c.DataType), IsCore: c.IsCore,
		IsRequired: c.IsRequired, ShowInReports: c.ShowInReports, SortOrder: c.SortOrder, Choices: c.choicesText()}
}

// isCustom says whether a person added the column, rather than the template starting with it.
func (c column) isCustom() bool { return strings.HasPrefix(c.Key, customKeyPrefix) }

// signed says whether the column is a list whose choices give the amount a sign: a table has at most one (redix-api #129).
func (c column) signed() bool {
	return slices.ContainsFunc(c.Choices, func(one choice) bool { return Sign(one.Sign) != unsigned })
}

func hasTwoSignedLists(columns []column) bool {
	count := 0
	for _, one := range columns {
		if one.signed() {
			count++
		}
	}
	return count > 1
}

// columnView is a column as the app shows it (ColumnView).
type columnView struct {
	ID            uuid.UUID    `json:"id"`
	Key           string       `json:"key"`
	Name          string       `json:"name"`
	DataType      DataType     `json:"dataType"`
	IsCore        bool         `json:"isCore"`
	IsCustom      bool         `json:"isCustom"`
	IsRequired    bool         `json:"isRequired"`
	ShowInReports bool         `json:"showInReports"`
	Choices       []choiceView `json:"choices"`
	IsEntryField  bool         `json:"isEntryField"`
}

func (c column) view(id uuid.UUID) columnView {
	choices := []choiceView{}
	for _, one := range c.Choices {
		choices = append(choices, choiceView{ID: one.ID, Name: one.Name, Sign: Sign(one.Sign)})
	}
	return columnView{ID: id, Key: c.Key, Name: c.Name, DataType: c.DataType, IsCore: c.IsCore, IsCustom: c.isCustom(), IsRequired: c.IsRequired,
		ShowInReports: c.ShowInReports, Choices: choices, IsEntryField: slices.Contains(entryFieldKeys, c.Key)}
}

// choicesOf is what was typed as a list's choices, each a new choice with its sign: each name trimmed, the empty and the repeated ones gone
// (Column.ChoicesOf).
func choicesOf(typed []choiceRequest) []choice {
	choices := []choice{}
	for _, one := range typed {
		name := strings.TrimSpace(one.Name)
		if name != "" && !slices.ContainsFunc(choices, func(kept choice) bool { return kept.Name == name }) {
			choices = append(choices, choice{ID: uuid.Must(uuid.NewV7()), Name: name, Sign: int64(one.Sign)})
		}
	}
	return choices
}

func acceptable(choices []choice) bool {
	return !slices.ContainsFunc(choices, func(one choice) bool { _, ok := naming.Named(one.Name); return !ok })
}

// columnRequest is a new column as the app sends it (AddColumnRequest).
type columnRequest struct {
	Name          string          `json:"name"`
	DataType      DataType        `json:"dataType"`
	IsRequired    bool            `json:"isRequired"`
	ShowInReports bool            `json:"showInReports"`
	Choices       []choiceRequest `json:"choices"`
}

// refusalOfNew is why a column typed so cannot be added (Column.CheckNew): invalidName, invalidDataType for a file, or invalidOptions for a list
// with no acceptable choice; "" when it can.
func (typed columnRequest) refusalOfNew() string {
	choices := choicesOf(typed.Choices)
	_, named := naming.Named(typed.Name)
	switch {
	case !named:
		return naming.Invalid
	case typed.DataType == file:
		return invalidDataType
	case typed.DataType == list && (len(choices) == 0 || !acceptable(choices)):
		return invalidOptions
	}
	return ""
}

// newColumn is the column a person typed, at sortOrder, with a key of its own; only a list keeps choices (DefaultColumn.New).
func (typed columnRequest) newColumn(sortOrder int64) column {
	name, _ := naming.Named(typed.Name)
	choices := []choice{}
	if typed.DataType == list {
		choices = choicesOf(typed.Choices)
	}
	return column{Key: customKeyPrefix + strings.ReplaceAll(uuid.Must(uuid.NewV7()).String(), "-", ""), Name: name, DataType: typed.DataType,
		IsRequired: typed.IsRequired, ShowInReports: typed.ShowInReports, SortOrder: sortOrder, Choices: choices}
}

// editRequest is what a column becomes (EditColumnRequest); its type is not here, because it never changes (BR-21).
type editRequest struct {
	Name          string          `json:"name"`
	IsRequired    bool            `json:"isRequired"`
	ShowInReports bool            `json:"showInReports"`
	NewChoices    []choiceRequest `json:"newChoices,omitzero"`
}

// edited is the column as typed makes it (Column.Edit), or the refusal: invalidName; coreColumn for more than a new name on a core column
// (BR-20); invalidOptions for new choices on anything but a list a person added, or one too long. A list's own choices stay, since entries hold
// their ids; new ones join them.
func (c column) edited(typed editRequest) (column, string) {
	name, named := naming.Named(typed.Name)
	if !named {
		return c, naming.Invalid
	}
	if c.IsCore && (typed.IsRequired != c.IsRequired || typed.ShowInReports != c.ShowInReports) {
		return c, coreColumn
	}
	added := choicesOf(typed.NewChoices)
	if len(added) > 0 && (c.DataType != list || !c.isCustom() || !acceptable(added)) {
		return c, invalidOptions
	}
	c.Name, c.IsRequired, c.ShowInReports = name, typed.IsRequired, typed.ShowInReports
	c.Choices = slices.Clone(c.Choices)
	for _, one := range added {
		if !slices.ContainsFunc(c.Choices, func(kept choice) bool { return kept.Name == one.Name }) {
			c.Choices = append(c.Choices, one)
		}
	}
	return c, ""
}

// inOrder is the place of each of rows in ids, when ids name every one of them exactly once (ProjectStructure.PutInOrder).
func inOrder(rows []uuid.UUID, ids []uuid.UUID) (map[uuid.UUID]int64, bool) {
	sorted := func(list []uuid.UUID) []uuid.UUID {
		kept := slices.Clone(list)
		slices.SortFunc(kept, func(a, b uuid.UUID) int { return strings.Compare(a.String(), b.String()) })
		return kept
	}
	if !slices.Equal(sorted(rows), sorted(ids)) {
		return nil, false
	}
	places := map[uuid.UUID]int64{}
	for place, id := range ids {
		places[id] = int64(place)
	}
	return places, true
}

// change is one write to a project's structure: its queries, and who makes it in which project and when, which each History line names. Every
// row it adds, changes or removes has its line (UC-069, #86: every action is recorded), where .NET recorded only an item's type and its
// percentages.
type change struct {
	ctx       context.Context
	q         *store.Queries
	projectID uuid.UUID
	actor     uuid.UUID
	now       moment.Moment
}

// save writes a row by write and records it: created when isNew, otherwise only what changed between before and after, and a row that did not
// change is neither written nor recorded.
func (c change) save(table string, id uuid.UUID, isNew bool, before, after map[string]any, write func() error) error {
	action := history.Create
	if isNew {
		before = nil
	} else {
		action = history.Update
		if before, after = history.Changed(before, after); len(after) == 0 {
			return nil
		}
	}
	if err := write(); err != nil {
		return err
	}
	return history.Write(c.ctx, c.q, c.now, c.actor, action, history.Change{Table: table, RecordID: id, ProjectID: c.projectID, Before: before, After: after})
}

// remove deletes a row by write and records what it held.
func (c change) remove(table string, id uuid.UUID, before map[string]any, write func() error) error {
	if err := write(); err != nil {
		return err
	}
	return history.Write(c.ctx, c.q, c.now, c.actor, history.Delete, history.Change{Table: table, RecordID: id, ProjectID: c.projectID, Before: before})
}

func (c change) saveCategory(before, after store.Category) error {
	return c.save("Categories", after.ID, before.ID == uuid.Nil, categoryFields(before), categoryFields(after),
		func() error { return c.q.SaveCategory(c.ctx, store.SaveCategoryParams(after)) })
}

func (c change) saveItem(before, after store.Item) error {
	return c.save("Items", after.ID, before.ID == uuid.Nil, itemFields(before), itemFields(after),
		func() error { return c.q.SaveItem(c.ctx, store.SaveItemParams(after)) })
}

func (c change) saveSubgroup(before, after store.Subgroup) error {
	return c.save("Subgroups", after.ID, before.ID == uuid.Nil, subgroupFields(before), subgroupFields(after),
		func() error { return c.q.SaveSubgroup(c.ctx, store.SaveSubgroupParams(after)) })
}

func (c change) saveItemColumn(before, after store.ItemColumn) error {
	return c.save("ItemColumns", after.ID, before.ID == uuid.Nil, itemColumnFields(before), itemColumnFields(after),
		func() error { return c.q.SaveItemColumn(c.ctx, store.SaveItemColumnParams(after)) })
}

func (c change) saveDefaultColumn(before, after store.DefaultColumn) error {
	return c.save("DefaultColumns", after.ID, before.ID == uuid.Nil, defaultColumnFields(before), defaultColumnFields(after),
		func() error { return c.q.SaveDefaultColumn(c.ctx, store.SaveDefaultColumnParams(after)) })
}

// The rows as History keeps them, under the C# property names, an enum as its number.

func categoryFields(row store.Category) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "Name": row.Name, "SortOrder": row.SortOrder, "IsActive": row.IsActive}
}

func itemFields(row store.Item) map[string]any {
	return map[string]any{"CategoryId": row.CategoryID, "Name": row.Name, "Type": row.Type, "SortOrder": row.SortOrder, "IsActive": row.IsActive,
		"ShowsTablePercentages": row.ShowsTablePercentages}
}

func subgroupFields(row store.Subgroup) map[string]any {
	return map[string]any{"ItemId": row.ItemID, "Name": row.Name, "SortOrder": row.SortOrder}
}

func itemColumnFields(row store.ItemColumn) map[string]any {
	return map[string]any{"ItemId": row.ItemID, "IsHidden": row.IsHidden, "Key": row.Key, "Name": row.Name, "DataType": row.DataType, "IsCore": row.IsCore,
		"IsRequired": row.IsRequired, "ShowInReports": row.ShowInReports, "SortOrder": row.SortOrder, "Choices": json.RawMessage(orEmptyList(row.Choices))}
}

func defaultColumnFields(row store.DefaultColumn) map[string]any {
	return map[string]any{"ProjectId": row.ProjectID, "ItemType": row.ItemType, "Key": row.Key, "Name": row.Name, "DataType": row.DataType,
		"IsCore": row.IsCore, "IsRequired": row.IsRequired, "ShowInReports": row.ShowInReports, "SortOrder": row.SortOrder,
		"Choices": json.RawMessage(orEmptyList(row.Choices))}
}

// orEmptyList is a kept list of choices, or an empty one for a row not yet written.
func orEmptyList(kept string) string {
	if kept == "" {
		return "[]"
	}
	return kept
}
