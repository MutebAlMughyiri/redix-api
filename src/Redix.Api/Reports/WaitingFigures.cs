using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Reports;

/// <summary>
/// What waits for approval, as two figures (findings row 147, Muteb 8 Nov 2026): expenses, which lower the surplus once approved, and revenue with partners' money, which raises it. The one
/// owner of "what waits" for the dashboard and the periodic report.
/// </summary>
/// <param name="ExpensesCount">How many purchases and contractor payments wait.</param>
/// <param name="ExpensesAmount">What they come to.</param>
/// <param name="RevenueCount">How many revenue entries and partners' movements wait.</param>
/// <param name="RevenueAmount">What they would add to revenue; a withdrawal takes away, as it does from the capital (BR-05).</param>
public record WaitingFigures(int ExpensesCount, decimal ExpensesAmount, int RevenueCount, decimal RevenueAmount)
{
    /// <summary>What waits in the project <paramref name="projectId"/> now.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<WaitingFigures> OfAsync(Guid projectId, RedixDbContext db)
    {
        var kinds = await db.Entries
            .Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval)
            .GroupBy(entry => entry.Kind)
            .Select(group => new { Kind = group.Key, Count = group.Count(), Amount = group.Sum(entry => entry.Amount) })
            .ToListAsync();
        var expenses = kinds.Where(row => IsExpense(row.Kind)).ToList();
        var revenue = kinds.Where(row => !IsExpense(row.Kind)).ToList();
        return new WaitingFigures(expenses.Sum(row => row.Count), expenses.Sum(row => row.Amount), revenue.Sum(row => row.Count), revenue.Sum(row => row.Kind == EntryKind.CapitalOut ? -row.Amount : row.Amount));
    }

    private static bool IsExpense(EntryKind kind) => kind is EntryKind.Purchase or EntryKind.ContractorPayment;
}
