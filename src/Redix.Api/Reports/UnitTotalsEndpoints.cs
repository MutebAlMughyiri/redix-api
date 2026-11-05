using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Reports;

/// <summary>What each unit has cost (UC-060, FR-15): what was recorded against it, and its part of what was shared.</summary>
public static class UnitTotalsEndpoints
{
    /// <summary>Maps the route, for anyone who reads the project.</summary>
    /// <param name="app">The app.</param>
    public static void MapUnitTotals(this WebApplication app) =>
        app.MapGet("/projects/{projectId:guid}/reports/units", Totals).RequireAuthorization(nameof(Permission.ReadProject));

    /// <summary>Each of the project's units with what was recorded against it and its part of the shared expenses that were split, from what is approved; then the shared unit, holding what was shared and never split.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<UnitTotalView>> Totals(Guid projectId, RedixDbContext db)
    {
        var approved = db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved);
        var direct = await (
            from expense in db.ExpenseDetails
            join entry in approved on expense.EntryId equals entry.Id
            where !db.Set<UnitShare>().Any(share => share.EntryId == entry.Id)
            group entry.Amount by expense.UnitId into amounts
            select new { UnitId = amounts.Key, Total = amounts.Sum() }).ToDictionaryAsync(row => row.UnitId, row => row.Total);
        var shared = (await (
            from share in db.Set<UnitShare>()
            join entry in approved on share.EntryId equals entry.Id
            select new { share.UnitId, entry.Amount, share.Percent }).ToListAsync())
            .GroupBy(row => row.UnitId).ToDictionary(group => group.Key, group => group.Sum(row => row.Amount * row.Percent / 100));
        var units = await db.Set<Unit>().Where(unit => unit.ProjectId == projectId).OrderBy(unit => unit.IsShared).ThenBy(unit => unit.SortOrder).ToListAsync();
        return units.Select(unit => new UnitTotalView(unit.Id, unit.Name, unit.IsShared, direct.GetValueOrDefault(unit.Id), shared.GetValueOrDefault(unit.Id))).ToList();
    }
}

/// <summary>What one unit has cost.</summary>
/// <param name="UnitId">The unit.</param>
/// <param name="Name">Its name.</param>
/// <param name="IsShared">True for the shared unit, whose total is what was shared and never split.</param>
/// <param name="Direct">What was recorded against it.</param>
/// <param name="FromShared">Its part of the shared expenses that were split.</param>
public record UnitTotalView(Guid UnitId, string Name, bool IsShared, decimal Direct, decimal FromShared)
{
    /// <summary>Both together.</summary>
    public decimal Total => Direct + FromShared;
}
