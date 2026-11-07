using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Accounts;

/// <summary>A project's money as the BRS reckons it, only ever from what is approved (BR-10): the one owner of capital, revenue and surplus, so the dashboard, the reconciliation and the report cannot disagree.</summary>
/// <param name="Capital">What partners put in, less what they took out (BR-05).</param>
/// <param name="VatRefunds">VAT that came back.</param>
/// <param name="Sales">What units sold for.</param>
/// <param name="OtherRevenue">Anything else that came in.</param>
/// <param name="Expenses">Purchases and contractor payments, by what was paid (BR-24).</param>
public readonly record struct ProjectFigures(decimal Capital, decimal VatRefunds, decimal Sales, decimal OtherRevenue, decimal Expenses)
{
    /// <summary>Capital, VAT refunds and sales (BR-06).</summary>
    public decimal Revenue => RevenueWithoutSales + Sales;

    /// <summary>All revenue but sales.</summary>
    public decimal RevenueWithoutSales => Capital + VatRefunds + OtherRevenue;

    /// <summary>Revenue less expenses (BR-07): what the accounts should hold.</summary>
    public decimal Surplus => Revenue - Expenses;

    /// <summary>The surplus without sales (BR-07), as the statement reckons it.</summary>
    public decimal SurplusWithoutSales => RevenueWithoutSales - Expenses;

    /// <summary>How far the work has come (UC-063): expenses as a percentage of revenue, to two decimals; 0 while nothing came in, and over 100 once expenses pass revenue.</summary>
    public decimal CompletionPercent => Revenue == 0 ? 0 : decimal.Round(Expenses / Revenue * 100, 2);

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
        return new ProjectFigures(
            totals.GetValueOrDefault(EntryKind.CapitalIn) - totals.GetValueOrDefault(EntryKind.CapitalOut),
            revenue.GetValueOrDefault(ItemType.VatRefund),
            revenue.GetValueOrDefault(ItemType.Sale),
            revenue.GetValueOrDefault(ItemType.OtherRevenue),
            totals.GetValueOrDefault(EntryKind.Purchase) + totals.GetValueOrDefault(EntryKind.ContractorPayment));
    }
}
