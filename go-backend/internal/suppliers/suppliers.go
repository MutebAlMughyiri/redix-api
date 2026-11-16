// Package suppliers keeps a project's register of suppliers (SuppliersEndpoints, redix-api #140).
package suppliers

import (
	"context"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/spelling"
	"github.com/google/uuid"
)

// approved is a supplier anyone in the project may pick (SupplierStatus.Approved).
const approved int64 = 0

// generalCashName is the supplier every register starts with, for paying cash to no one in particular (Supplier.GeneralCashSupplierName, UC-017).
const generalCashName = "مورد نقدي عام"

// AddGeneralCash starts projectID's register with the general cash supplier, added in the name of createdBy (Supplier.GeneralCashSupplier). Like
// the structure it comes with, it has no History line of its own: the approval's line is the record.
func AddGeneralCash(ctx context.Context, q *store.Queries, projectID, createdBy uuid.UUID) error {
	return q.AddSupplier(ctx, store.AddSupplierParams{ID: uuid.Must(uuid.NewV7()), ProjectID: projectID, Name: generalCashName,
		NormalizedName: spelling.Normalize(generalCashName), Status: approved, IsActive: true, IsGeneralCash: true, CreatedByUserID: createdBy})
}
