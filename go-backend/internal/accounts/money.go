// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package accounts keeps where a project's money sits - its bank accounts and cash custodies (UC-015, FR-22) - what each holds, the money moved
// between them (FR-23), the checks of what they really hold (UC-041, FR-24), and the project's figures every summary reads (AccountsEndpoints,
// TransfersEndpoints, ReconciliationsEndpoints, ProjectFigures; redix-api #140).
package accounts

import (
	"context"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/ledger"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
	"github.com/google/uuid"
)

// The revenue types of item whose totals the figures name (ItemType).
const (
	sale         int64 = 3
	vatRefund    int64 = 4
	otherRevenue int64 = 5
)

// Balances is what each account of the project that anything moved holds (AccountMoney, redix-app #89): what its approved entries moved by their
// kind's sign (BR-10), plus what transfers brought in, less what they took out. An account nothing moved is not among them, and holds nothing.
// The entries of the tables in unseen, a JSON list, are left out: what someone narrowed sees (#106 D6); access.Everything leaves nothing out.
func Balances(ctx context.Context, q *store.Queries, projectID uuid.UUID, unseen string) (map[uuid.UUID]fixed.Total, error) {
	rows, err := q.AccountMovements(ctx, store.AccountMovementsParams{ProjectID: projectID, Unseen: unseen})
	balances := map[uuid.UUID]fixed.Total{}
	for _, row := range rows {
		balances[row.AccountID] = fixed.Counted(fixed.Number(row.Total))
	}
	return balances, err
}

// Figures is a project's money as the BRS reckons it, only ever from what is approved (ProjectFigures, BR-10): the one owner of capital, revenue
// and surplus, so the summary, the reconciliation and the report cannot disagree.
type Figures struct {
	Capital, VatRefunds, Sales, OtherRevenue, Expenses fixed.Total
}

// RevenueWithoutSales is capital, VAT refunds and other revenue.
func (f Figures) RevenueWithoutSales() fixed.Total {
	return f.Capital.Plus(f.VatRefunds).Plus(f.OtherRevenue)
}

// Revenue is every revenue, sales too (BR-06).
func (f Figures) Revenue() fixed.Total { return f.RevenueWithoutSales().Plus(f.Sales) }

// Surplus is revenue less expenses (BR-07): what the accounts should hold.
func (f Figures) Surplus() fixed.Total { return f.Revenue().Minus(f.Expenses) }

// SurplusWithoutSales is the surplus as the statement reckons it, without sales (BR-07).
func (f Figures) SurplusWithoutSales() fixed.Total { return f.RevenueWithoutSales().Minus(f.Expenses) }

// FiguresOf is the project's figures now, but the entries of the tables in unseen (#106 R6); access.Everything leaves nothing out.
func FiguresOf(ctx context.Context, q *store.Queries, projectID uuid.UUID, unseen string) (Figures, error) {
	kinds, err := q.ApprovedTotalsByKind(ctx, store.ApprovedTotalsByKindParams{ProjectID: projectID, Unseen: unseen})
	if err != nil {
		return Figures{}, err
	}
	types, err := q.ApprovedRevenueByItemType(ctx, store.ApprovedRevenueByItemTypeParams{ProjectID: projectID, Unseen: unseen})
	byKind, byType := map[int64]fixed.Total{}, map[int64]fixed.Total{}
	for _, row := range kinds {
		byKind[row.Kind] = fixed.Counted(fixed.Number(row.Total))
	}
	for _, row := range types {
		byType[row.Type] = fixed.Counted(fixed.Number(row.Total))
	}
	return Figures{
		Capital:      byKind[ledger.CapitalIn].Minus(byKind[ledger.CapitalOut]),
		VatRefunds:   byType[vatRefund],
		Sales:        byType[sale],
		OtherRevenue: byType[otherRevenue],
		Expenses:     byKind[ledger.Purchase].Plus(byKind[ledger.ContractorPayment]),
	}, err
}
