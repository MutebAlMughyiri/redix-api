using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What recording any expense shares, a purchase or a contractor payment: the rules of what it names, its columns and its file, and the way it reaches the ledger.</summary>
public static class EntryRecording
{
    /// <summary>Why <paramref name="id"/> cannot be a new entry's: empty, or used already, which saves nothing new (AC-16.3). An empty reason when it can.</summary>
    public static async Task<(int Status, string Reason)> RefusalOfIdAsync(Guid id, RedixDbContext db)
    {
        if (id == Guid.Empty)
        {
            return (StatusCodes.Status400BadRequest, "InvalidEntryId");
        }
        return await db.Entries.AnyAsync(entry => entry.Id == id) ? (StatusCodes.Status409Conflict, "EntryExists") : (0, "");
    }

    /// <summary>The entry <paramref name="id"/> if <paramref name="userId"/> already recorded it in the project: an upload that was cut and sent again lands once (AC-16.3, UC-029).</summary>
    public static Task<EntryStatusView?> RepeatOfAsync(Guid id, Guid projectId, Guid userId, RedixDbContext db) =>
        db.Entries.Where(entry => entry.Id == id && entry.ProjectId == projectId && entry.CreatedByUserId == userId).Select(entry => new EntryStatusView(entry.Id, entry.Status, "")).Cast<EntryStatusView?>().SingleOrDefaultAsync();

    /// <summary>Why the payment method, the account or the file <paramref name="request"/> names cannot be used by <paramref name="userId"/>, who holds <paramref name="held"/>. Empty when they can.</summary>
    public static async Task<string> RefusalOfPaymentAsync(Guid projectId, EntryRequest request, Guid userId, List<Permission> held, RedixDbContext db)
    {
        if (!await db.PaymentMethods.AnyAsync(method => method.Id == request.PaymentMethodId && method.ProjectId == projectId && method.IsActive))
        {
            return "UnknownPaymentMethod";
        }
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == request.AccountId && row.ProjectId == projectId);
        if (account is null || !account.IsOfferedTo(userId, held.Contains(Permission.ChargeAnyAccount)))
        {
            return "AccountNotOffered";
        }
        return request.AttachmentId == Guid.Empty || await db.Attachments.AnyAsync(attachment => attachment.Id == request.AttachmentId && attachment.ProjectId == projectId) ? "" : "UnknownAttachment";
    }

    /// <summary>Whether the file <paramref name="attachmentId"/> names, or the same bytes, are on an entry of the project already (BR-22).</summary>
    public static async Task<bool> IsFileOnAnEntryAsync(Guid projectId, Guid attachmentId, RedixDbContext db, Guid editing = default)
    {
        var hash = await db.Attachments.Where(attachment => attachment.Id == attachmentId).Select(attachment => attachment.Sha256).SingleAsync();
        return await db.EntryAttachments.AnyAsync(link => link.EntryId != editing && db.Attachments.Any(attachment => attachment.Id == link.AttachmentId && attachment.ProjectId == projectId && attachment.Sha256 == hash));
    }

    /// <summary>Why the custom values of <paramref name="request"/> do not fit <paramref name="columns"/> or, when sending, a required one is empty (FR-41), and the column's key. An empty reason when they fit.</summary>
    public static (string Reason, string ColumnKey) RefusalOfColumns(EntryRequest request, List<ItemColumn> columns)
    {
        foreach (var (key, value) in request.CustomValuesKept())
        {
            var column = columns.SingleOrDefault(row => row.Key == key && row.IsCustom);
            if (column is null)
            {
                return ("UnknownColumn", key);
            }
            if (!column.Accepts(value))
            {
                return ("InvalidColumnValue", key);
            }
        }
        var missing = request.Submit ? request.Answers().MissingOf(columns) : [];
        return missing.Count == 0 ? ("", "") : ("RequiredColumnMissing", missing[0]);
    }

    /// <summary>A new entry from <paramref name="request"/>: sent for approval, or else a draft.</summary>
    public static Entry EntryOf(EntryRequest request, Guid projectId, EntryKind kind, DateOnly entryDate, decimal amount, Guid userId, DateTime at) => new()
    {
        Id = request.Id,
        ProjectId = projectId,
        Kind = kind,
        Status = request.Submit ? EntryStatus.PendingApproval : EntryStatus.Draft,
        EntryDate = entryDate,
        Amount = amount,
        AccountId = request.AccountId,
        Notes = request.Notes.Trim(),
        CreatedByUserId = userId,
        CreatedAt = at,
    };

    /// <summary>What every expense has, from <paramref name="request"/>, for <paramref name="itemId"/> and <paramref name="subgroupId"/>.</summary>
    public static ExpenseDetails ExpenseDetailsOf(EntryRequest request, Guid itemId, Guid subgroupId) => new()
    {
        EntryId = request.Id,
        ItemId = itemId,
        SubgroupId = subgroupId,
        PaymentMethodId = request.PaymentMethodId,
        CustomValues = JsonSerializer.Serialize(request.CustomValuesKept()),
    };

    /// <summary>Saves <paramref name="entry"/> with its <paramref name="details"/> and the audit record of each (FR-21).</summary>
    public static async Task SaveAsync(Entry entry, List<object> details, Guid userId, RedixDbContext db)
    {
        db.Add(entry);
        details.ForEach(detail => db.Add(detail));
        db.Audit(entry, entry.Status == EntryStatus.PendingApproval ? AuditAction.Submit : AuditAction.Create, userId, entry.CreatedAt);
        if (entry.Status == EntryStatus.PendingApproval)
        {
            db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Submit, UserId = userId, At = entry.CreatedAt });
            await NotificationsEndpoints.TellDecidersAsync(entry, userId, entry.CreatedAt, db);
        }
        details.ForEach(detail => db.Audit(detail, AuditAction.Create, userId, entry.CreatedAt));
        await db.SaveChangesAsync();
    }
}
