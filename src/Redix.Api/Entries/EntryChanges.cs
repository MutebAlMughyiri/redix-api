using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Revenue;

namespace Redix.Api.Entries;

/// <summary>Who may change an entry, and how a change reaches the ledger (UC-033, UC-034, BR-11).</summary>
public static class EntryChanges
{
    /// <summary>
    /// Why <paramref name="entry"/> cannot be changed by <paramref name="userId"/>, holding <paramref name="held"/>; empty when it can. What is approved never changes (BR-11), and an edit made offline that arrives after
    /// the approval learns so (AC-16.2). Whoever holds <see cref="Permission.ManageAnyEntry"/> changes anything sent before approval; anyone else only their own, and only while it is not waiting for a decision.
    /// </summary>
    public static (int Status, string Reason) RefusalOf(Entry entry, Guid userId, List<Permission> held)
    {
        if (entry.Status == EntryStatus.Approved)
        {
            return (StatusCodes.Status409Conflict, "EntryApproved");
        }
        if (!MayHandle(entry, userId, held))
        {
            return (StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        return entry.Status == EntryStatus.PendingApproval && !held.Contains(Permission.ManageAnyEntry) ? (StatusCodes.Status409Conflict, "EntryWaitingForDecision") : (0, "");
    }

    /// <summary>Whether <paramref name="userId"/>, holding <paramref name="held"/>, may act on <paramref name="entry"/> at all: they entered it, or it was sent and they hold <see cref="Permission.ManageAnyEntry"/>. A draft is its author's alone until it is sent (BR-42).</summary>
    public static bool MayHandle(Entry entry, Guid userId, List<Permission> held) =>
        entry.CreatedByUserId == userId || (entry.Status != EntryStatus.Draft && held.Contains(Permission.ManageAnyEntry));

    /// <summary>The same question, reading what <paramref name="userId"/> holds in the entry's project.</summary>
    public static async Task<bool> MayHandleAsync(Entry entry, Guid userId, RedixDbContext db) => MayHandle(entry, userId, (await UserAccess.OfAsync(userId, db)).In(entry.ProjectId));

    /// <summary>Replaces what <paramref name="existing"/> holds with <paramref name="edited"/> and <paramref name="details"/>, keeping who entered it and when, and its status unless it is sent now. The History table holds the values before and after.</summary>
    public static async Task ReplaceAsync(Entry existing, Entry edited, List<object> details, Guid userId, DateTime at, RedixDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        await RemoveDetailsAsync(existing.Id, userId, at, db);
        await db.SaveChangesAsync();

        var sending = edited.Status == EntryStatus.PendingApproval && existing.Status != EntryStatus.PendingApproval;
        existing.Kind = edited.Kind;
        existing.EntryDate = edited.EntryDate;
        existing.Amount = edited.Amount;
        existing.AccountId = edited.AccountId;
        existing.Description = edited.Description;
        existing.ColumnValues = edited.ColumnValues;
        existing.Status = edited.Status == EntryStatus.PendingApproval ? EntryStatus.PendingApproval : existing.Status;
        db.AddHistory(existing, sending ? HistoryAction.Submit : HistoryAction.Update, userId, at);
        if (sending)
        {
            db.EntryDecisions.Add(new EntryDecision { EntryId = existing.Id, Action = EntryDecisionAction.Submit, UserId = userId, At = at });
            await NotificationsEndpoints.TellDecidersAsync(existing, userId, at, db);
        }
        foreach (var detail in details)
        {
            db.Add(detail);
            db.AddHistory(detail, HistoryAction.Create, userId, at);
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    /// <summary>Removes an entry that was never approved, with all it holds. The History table keeps what it held.</summary>
    public static async Task DeleteAsync(Entry entry, Guid userId, DateTime at, RedixDbContext db)
    {
        await RemoveDetailsAsync(entry.Id, userId, at, db);
        db.EntryDecisions.RemoveRange(await db.EntryDecisions.Where(row => row.EntryId == entry.Id).ToListAsync());
        db.Notifications.RemoveRange(await db.Notifications.Include(row => row.Entries).Where(row => row.Entries.Any(link => link.EntryId == entry.Id)).ToListAsync());
        db.AddHistory(entry, HistoryAction.Delete, userId, at);
        db.Entries.Remove(entry);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Removes an approved entry softly (redix-api #119, D1): it is marked <see cref="EntryStatus.Removed"/>, which no list and no figure reads, so it leaves the summary, its account, its
    /// item, its contract's paid amount and a partner's capital as if never approved; it stays in the database with all it holds, and the History table keeps who removed it and when.
    /// A periodic report issued before keeps it, as issued (BR-26).
    /// </summary>
    public static async Task RemoveAsync(Entry entry, Guid userId, DateTime at, RedixDbContext db)
    {
        entry.Status = EntryStatus.Removed;
        db.AddHistory(entry, HistoryAction.Delete, userId, at);
        await db.SaveChangesAsync();
    }

    /// <summary>Brings back an entry <see cref="RemoveAsync"/> removed: approved again, back in every list and figure, with a History row saying who and when.</summary>
    public static async Task RestoreAsync(Entry entry, Guid userId, DateTime at, RedixDbContext db)
    {
        entry.Status = EntryStatus.Approved;
        db.AddHistory(entry, HistoryAction.Restore, userId, at);
        await db.SaveChangesAsync();
    }

    private static async Task RemoveDetailsAsync(Guid entryId, Guid userId, DateTime at, RedixDbContext db)
    {
        List<object> details =
        [
            .. await db.ExpenseDetails.Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.PurchaseDetails.Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.ContractorPaymentDetails.Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.Invoices.Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.EntryAttachments.Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.Set<RevenueDetails>().Where(row => row.EntryId == entryId).ToListAsync(),
            .. await db.CapitalMovementDetails.Where(row => row.EntryId == entryId).ToListAsync(),
        ];
        foreach (var detail in details)
        {
            db.AddHistory(detail, HistoryAction.Delete, userId, at);
            db.Remove(detail);
        }
    }
}
