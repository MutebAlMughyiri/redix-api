using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Accounts;

/// <summary>A project's money as the BRS reckons it, only ever from what is approved (BR-10): the one owner of capital, revenue and surplus, so the dashboard, the reconciliation and the report cannot disagree.</summary>
/// <param name="Capital">What partners put in, less what they took out (BR-05); their parts of the liquidation are not capital.</param>
/// <param name="VatRefunds">VAT that came back.</param>
/// <param name="Sales">What units sold for.</param>
/// <param name="OtherRevenue">Anything else that came in.</param>
/// <param name="Expenses">Purchases and contractor payments, by what was paid (BR-24).</param>
/// <param name="Liquidation">What was distributed to the partners when the project closed, less what they paid in for a deficit (BR-27); zero until then.</param>
public readonly record struct ProjectFigures(decimal Capital, decimal VatRefunds, decimal Sales, decimal OtherRevenue, decimal Expenses, decimal Liquidation)
{
    /// <summary>Capital, VAT refunds and sales (BR-06).</summary>
    public decimal Revenue => RevenueWithoutSales + Sales;

    /// <summary>All revenue but sales.</summary>
    public decimal RevenueWithoutSales => Capital + VatRefunds + OtherRevenue;

    /// <summary>Revenue less expenses and what was distributed (BR-07, BR-27): what the accounts should hold, nothing once the project is liquidated.</summary>
    public decimal Surplus => Revenue - (Expenses + Liquidation);

    /// <summary>The surplus without sales (BR-07), as the statement reckons it: before the liquidation.</summary>
    public decimal SurplusWithoutSales => RevenueWithoutSales - Expenses;

    /// <summary>The figures of the project <paramref name="projectId"/> now.</summary>
    public static async Task<ProjectFigures> OfAsync(Guid projectId, RedixDbContext db)
    {
        var approved = db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved);
        var totals = await approved
            .GroupBy(entry => entry.Kind)
            .Select(group => new { Kind = group.Key, Total = group.Sum(entry => entry.Amount) })
            .ToDictionaryAsync(row => row.Kind, row => row.Total);
        var revenue = await (
            from details in db.Set<RevenueDetails>()
            join entry in approved on details.EntryId equals entry.Id
            join item in db.Items on details.ItemId equals item.Id
            group entry.Amount by item.Type into amounts
            select new { Type = amounts.Key, Total = amounts.Sum() }).ToDictionaryAsync(row => row.Type, row => row.Total);
        var liquidation = await (
            from details in db.CapitalMovementDetails
            join entry in approved on details.EntryId equals entry.Id
            where details.Type == CapitalMovementType.Liquidation
            select entry.Kind == EntryKind.CapitalOut ? entry.Amount : -entry.Amount).SumAsync();
        return new ProjectFigures(
            totals.GetValueOrDefault(EntryKind.CapitalIn) - totals.GetValueOrDefault(EntryKind.CapitalOut) + liquidation,
            revenue.GetValueOrDefault(ItemType.VatRefund),
            revenue.GetValueOrDefault(ItemType.Sale),
            revenue.GetValueOrDefault(ItemType.OtherRevenue),
            totals.GetValueOrDefault(EntryKind.Purchase) + totals.GetValueOrDefault(EntryKind.ContractorPayment),
            liquidation);
    }
}
