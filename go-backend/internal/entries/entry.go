// Package entries keeps the ledger: every movement of a project's money, of any kind - a purchase, a contractor payment, a partner's capital in
// or out, revenue - recorded once per id, sent for approval, decided, and read back (Entries, EntryRecording, EntryChanges, ApprovalsEndpoints;
// redix-api #140).
package entries

import (
	"strings"
	"time"
	"unicode/utf8"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/moment"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// What an entry is (EntryKind).
const (
	purchase int64 = iota
	contractorPayment
	capitalIn
	capitalOut
	revenue
)

var kinds = web.Enum{"purchase", "contractorPayment", "capitalIn", "capitalOut", "revenue"}

// Where an entry stands (EntryStatus).
const (
	draft int64 = iota
	pendingApproval
	approved
	rejected
	removed
)

var statuses = web.Enum{"draft", "pendingApproval", "approved", "rejected", "removed"}

// What a file is to its entry (EntryAttachmentRole).
var roles = web.Enum{"invoice", "receipt", "bankReceipt", "other"}

// What a step of approval was (EntryDecisionAction).
const (
	submitted int64 = iota
	approvedBy
	rejectedBy
)

// named writes an enum kept as a number by its name.
type named struct {
	value int64
	enum  web.Enum
}

func (n named) MarshalJSON() ([]byte, error) { return n.enum.Write(n.value) }

func kindOf(value int64) named   { return named{value, kinds} }
func statusOf(value int64) named { return named{value, statuses} }

// signed is an amount of kind as it moves its account: capital in and revenue bring money in, the rest take it out (EntryKinds.Signed, BR-10).
func signed(kind int64, amount fixed.Total) fixed.Total {
	if kind == capitalIn || kind == revenue {
		return amount
	}
	return fixed.Total{}.Minus(amount)
}

// fileRole is what an entry of kind's own file is (EntryKinds.FileRole).
func fileRole(kind int64) int64 {
	switch kind {
	case purchase:
		return 0
	case contractorPayment:
		return 1
	case capitalIn, capitalOut:
		return 2
	}
	return 3
}

// descriptionMaxLength is the longest description an entry keeps (Entry.DescriptionMaxLength).
const descriptionMaxLength = 2000

// saudiOffset is Saudi Arabia's time, three hours ahead of UTC all year: the one owner of a project's day (Entry.SaudiOffset).
const saudiOffset = 3 * time.Hour

// notApproved is the approved moment of an entry not approved yet (Entry.NotApproved).
var notApproved = moment.Never

// request is what the app sends for any entry, whatever its table (EntryRequest, redix-api #110).
type request struct {
	ID                uuid.UUID   `json:"id"`
	ItemID            uuid.UUID   `json:"itemId,omitzero"`
	TableID           uuid.UUID   `json:"tableId,omitzero"`
	Date              moment.Date `json:"date,omitzero"`
	Amount            fixed.Typed `json:"amount,omitzero"`
	AccountID         uuid.UUID   `json:"accountId,omitzero"`
	PaymentMethodID   uuid.UUID   `json:"paymentMethodId,omitzero"`
	Description       string      `json:"description,omitzero"`
	ColumnValues      web.Ordered `json:"columnValues,omitzero"`
	AttachmentID      uuid.UUID   `json:"attachmentId,omitzero"`
	Submit            bool        `json:"submit,omitzero"`
	SupplierID        uuid.UUID   `json:"supplierId,omitzero"`
	IsOutsideContract bool        `json:"isOutsideContract,omitzero"`
}

// kept is the column values that hold something, trimmed, in the order typed (EntryRequest.ColumnValuesKept).
func (r request) kept() web.Ordered {
	values := web.Ordered{Values: map[string]string{}}
	for _, key := range r.ColumnValues.Keys {
		if value := strings.TrimSpace(r.ColumnValues.Values[key]); value != "" {
			values.Keys = append(values.Keys, key)
			values.Values[key] = value
		}
	}
	return values
}

func (r request) description() string { return strings.TrimSpace(r.Description) }

// fields is an entry as History keeps it, under the C# property names.
func fields(entry store.Entry) map[string]any {
	return map[string]any{"ProjectId": entry.ProjectID, "Number": entry.Number, "Kind": entry.Kind, "Status": entry.Status, "EntryDate": entry.EntryDate,
		"Amount": entry.Amount, "AccountId": entry.AccountID, "Description": entry.Description, "ColumnValues": entry.ColumnValues,
		"CreatedByUserId": entry.CreatedByUserID, "CreatedAt": entry.CreatedAt, "ApprovedAt": entry.ApprovedAt}
}

// isAcceptableDate says whether date is inside the project's period and not in the future, in Saudi Arabia (BR-12).
func isAcceptableDate(date, projectStart moment.Date, now time.Time) bool {
	today := now.UTC().Add(saudiOffset)
	return !date.Before(projectStart.Time) && !date.After(time.Date(today.Year(), today.Month(), today.Day(), 0, 0, 0, 0, time.UTC))
}

func tooLong(text string, limit int) bool { return utf8.RuneCountInString(text) > limit }
