using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Revenue;

namespace Redix.Api.Reports;

/// <summary>
/// What each item of a project came to: an expense item's approved purchases and contractor payments, by what was paid (BR-02), a revenue item's approved revenue. The one owner of item
/// totals, so the dashboard, the item report and the periodic report agree.
/// </summary>
public static class ItemTotals
{
    /// <summary>Each item's total in the project <paramref name="projectId"/>; an item with nothing is left out, and its default is nothing.</summary>
    public static async Task<Dictionary<Guid, decimal>> OfProjectAsync(Guid projectId, RedixDbContext db)
    {
        var approved = db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved);
        var expenses = from expense in db.ExpenseDetails join entry in approved on expense.EntryId equals entry.Id select new { expense.ItemId, entry.Amount };
        var revenue = from details in db.Set<RevenueDetails>() join entry in approved on details.EntryId equals entry.Id select new { details.ItemId, entry.Amount };
        return await expenses.Concat(revenue).GroupBy(row => row.ItemId).Select(group => new { ItemId = group.Key, Total = group.Sum(row => row.Amount) }).ToDictionaryAsync(row => row.ItemId, row => row.Total);
    }
}
