using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Reports;

/// <summary>What each item of a project has cost: its approved purchases and contractor payments, by what was paid (BR-02). The one owner of item totals, so the dashboard, the item report and the periodic report agree.</summary>
public static class ItemTotals
{
    /// <summary>Each item's total in the project <paramref name="projectId"/>; an item with nothing is left out, and its default is nothing.</summary>
    public static Task<Dictionary<Guid, decimal>> OfProjectAsync(Guid projectId, RedixDbContext db) => (
        from expense in db.ExpenseDetails
        join entry in db.Entries on expense.EntryId equals entry.Id
        where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved
        group entry.Amount by expense.ItemId into amounts
        select new { ItemId = amounts.Key, Total = amounts.Sum() }).ToDictionaryAsync(row => row.ItemId, row => row.Total);
}
