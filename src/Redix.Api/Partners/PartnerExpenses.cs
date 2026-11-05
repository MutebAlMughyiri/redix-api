using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Partners;

/// <summary>An expense a partner paid from their own pocket (UC-045, BR-25). It is charged to their pocket; what comes back to them is decided when it is approved, so a rejected expense gives them nothing (BR-10).</summary>
public static class PartnerExpenses
{
    /// <summary>Adds to the partner's capital what <paramref name="entry"/> took from their pocket, unless Finance said it is owed back or it was added already; saved with the change.</summary>
    public static async Task WhenApprovedAsync(Entry entry, Guid userId, DateTime at, RedixDbContext db)
    {
        var pocket = await db.PartnerPockets.SingleOrDefaultAsync(row => row.AccountId == entry.AccountId);
        if (pocket is null)
        {
            return;
        }
        var expense = await db.ExpenseDetails.SingleAsync(row => row.EntryId == entry.Id);
        if (expense.PartnerTreatment == PartnerTreatment.AmountDue || await db.EntryLinks.AnyAsync(link => link.Type == EntryLinkType.ContributionOf && link.LinkedEntryId == entry.Id))
        {
            return;
        }
        expense.PartnerTreatment = PartnerTreatment.Capital;
        var contribution = DirectEntries.New(entry.ProjectId, EntryKind.CapitalIn, entry.EntryDate, entry.Amount, entry.AccountId, entry.Notes, userId, at);
        DirectEntries.Add(
            contribution,
            [
                new CapitalMovementDetails { EntryId = contribution.Id, PartnerId = pocket.PartnerId, Type = CapitalMovementType.Contribution, Description = entry.Notes },
                new EntryLink { EntryId = contribution.Id, Type = EntryLinkType.ContributionOf, LinkedEntryId = entry.Id },
            ],
            userId,
            at,
            db);
    }
}
