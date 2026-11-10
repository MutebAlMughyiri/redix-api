using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Accounts;

/// <summary>
/// What a project's accounts hold, the one owner of a balance (redix-app #89): what its approved entries moved by their kind's sign (BR-10), plus what transfers brought
/// in, less what they took out. Nothing else changes it, and no account has an opening balance.
/// </summary>
public static class AccountMoney
{
    /// <summary>The balance of each account of the project that anything has moved; an account nothing moved holds nothing.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<Dictionary<Guid, decimal>> MovementsOfProjectAsync(Guid projectId, RedixDbContext db)
    {
        var entries = await db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved)
            .GroupBy(entry => new { entry.AccountId, entry.Kind })
            .Select(group => new { group.Key.AccountId, group.Key.Kind, Total = group.Sum(entry => entry.Amount) })
            .ToListAsync();
        var transfersIn = await db.Transfers.Where(transfer => transfer.ProjectId == projectId).GroupBy(transfer => transfer.ToAccountId)
            .Select(group => new { AccountId = group.Key, Total = group.Sum(transfer => transfer.Amount) }).ToListAsync();
        var transfersOut = await db.Transfers.Where(transfer => transfer.ProjectId == projectId).GroupBy(transfer => transfer.FromAccountId)
            .Select(group => new { AccountId = group.Key, Total = -group.Sum(transfer => transfer.Amount) }).ToListAsync();
        return entries.Select(row => new { row.AccountId, Total = row.Kind.Signed(row.Total) })
            .Concat(transfersIn)
            .Concat(transfersOut)
            .GroupBy(row => row.AccountId)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Total));
    }

    /// <summary>What <paramref name="accountId"/> holds now.</summary>
    /// <param name="projectId">The project it belongs to.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<decimal> BalanceOfAsync(Guid projectId, Guid accountId, RedixDbContext db) =>
        (await MovementsOfProjectAsync(projectId, db)).GetValueOrDefault(accountId);
}
