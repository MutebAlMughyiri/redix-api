using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Accounts;

/// <summary>A project's money as the BRS reckons it, only ever from what is approved (BR-10): the one owner of capital, revenue and surplus, so the dashboard, the reconciliation and the report cannot disagree.</summary>
/// <param name="Capital">What partners put in, less what they took out (BR-05).</param>
/// <param name="OtherRevenue">VAT refunds and sales.</param>
/// <param name="Expenses">Purchases and contractor payments, by what was paid (BR-24).</param>
public readonly record struct ProjectFigures(decimal Capital, decimal OtherRevenue, decimal Expenses)
{
    /// <summary>Capital, VAT refunds and sales (BR-06).</summary>
    public decimal Revenue => Capital + OtherRevenue;

    /// <summary>Revenue less expenses (BR-07): what the accounts should hold.</summary>
    public decimal Surplus => Revenue - Expenses;

    /// <summary>The figures of the project <paramref name="projectId"/> now.</summary>
    public static async Task<ProjectFigures> OfAsync(Guid projectId, RedixDbContext db)
    {
        var totals = await db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved)
            .GroupBy(entry => entry.Kind)
            .Select(group => new { Kind = group.Key, Total = group.Sum(entry => entry.Amount) })
            .ToDictionaryAsync(row => row.Kind, row => row.Total);
        return new ProjectFigures(
            totals.GetValueOrDefault(EntryKind.CapitalIn) - totals.GetValueOrDefault(EntryKind.CapitalOut),
            totals.GetValueOrDefault(EntryKind.Revenue),
            totals.GetValueOrDefault(EntryKind.Purchase) + totals.GetValueOrDefault(EntryKind.ContractorPayment));
    }
}
