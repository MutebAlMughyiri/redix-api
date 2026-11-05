using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Accounts;

/// <summary>What moved through a project's accounts: only what is approved counts (BR-10).</summary>
public static class AccountMoney
{
    private static readonly EntryKind[] Inflows = [EntryKind.CapitalIn, EntryKind.Revenue, EntryKind.TransferIn];

    /// <summary>The approved movement of each account of the project: what came in, less what went out.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<Dictionary<Guid, decimal>> MovementsOfProjectAsync(Guid projectId, RedixDbContext db)
    {
        var moved = await db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved)
            .GroupBy(entry => new { entry.AccountId, entry.Kind })
            .Select(group => new { group.Key.AccountId, group.Key.Kind, Total = group.Sum(entry => entry.Amount) })
            .ToListAsync();
        return moved.GroupBy(row => row.AccountId).ToDictionary(group => group.Key, group => group.Sum(row => Inflows.Contains(row.Kind) ? row.Total : -row.Total));
    }
}
