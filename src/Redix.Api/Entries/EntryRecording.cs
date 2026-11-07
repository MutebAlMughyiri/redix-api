using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What recording any expense shares, a purchase or a contractor payment: recording it once, changing it before approval, the checks every one passes, and the way it reaches the ledger.</summary>
public static class EntryRecording
{
    /// <summary>
    /// Records <paramref name="request"/> as a new entry through <paramref name="save"/>. The same id from whoever recorded it answers that entry, so a retried upload lands once (AC-16.3,
    /// UC-029); from anyone else <c>entryExists</c>, and an empty one <c>invalidEntryId</c>.
    /// </summary>
    public static async Task<IResult> RecordAsync(Guid projectId, EntryRequest request, ClaimsPrincipal principal, RedixDbContext db, Func<Task<IResult>> save)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var repeated = await db.Entries.Where(entry => entry.Id == request.Id && entry.ProjectId == projectId && entry.CreatedByUserId == userId)
            .Select(entry => new EntryStatusView(entry.Id, entry.Status, "")).ToListAsync();
        if (repeated.Count > 0)
        {
            return Results.Ok(repeated[0]);
        }
        if (request.Id == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidEntryId");
        }
        return await db.Entries.AnyAsync(entry => entry.Id == request.Id) ? Refusal.Of(StatusCodes.Status409Conflict, "EntryExists") : await save();
    }

    /// <summary>Changes the <paramref name="kind"/> entry <paramref name="entryId"/> through <paramref name="save"/> before it is approved (UC-033), with the refusals of <see cref="EntryChanges.RefusalOf"/>; one not in the project is not found.</summary>
    public static async Task<IResult> ChangeAsync(Guid projectId, Guid entryId, EntryKind kind, ClaimsPrincipal principal, RedixDbContext db, Func<Entry, Task<IResult>> save)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId && row.Kind == kind);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var (status, refusal) = EntryChanges.RefusalOf(entry, userId, (await UserAccess.OfAsync(userId, db)).In(projectId));
        return refusal.Length > 0 ? Refusal.Of(status, refusal) : await save(entry);
    }

    /// <summary>
    /// Why <paramref name="request"/>, on <paramref name="item"/>, cannot be kept as no expense can: a payment method, an account or a file <paramref name="userId"/>, holding
    /// <paramref name="held"/>, may not use; what was typed; a date outside the project's period or in the future (BR-12); its item's columns (FR-41), naming the column; its file on
    /// another entry than <paramref name="editing"/> (BR-22). An empty code when it can.
    /// </summary>
    public static async Task<(int Status, string Code, string ColumnKey)> RefusalOfAsync(Guid projectId, EntryRequest request, Item item, Guid userId, List<Permission> held, Guid editing, RedixDbContext db, TimeProvider time)
    {
        var refusal = await RefusalOfPaymentAsync(projectId, request, userId, held, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTyped();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 || Entry.IsAcceptableDate(request.PaymentDate, project.StartDate, Entry.Today(time)) ? refusal : "InvalidPaymentDate";
        if (refusal.Length > 0)
        {
            return (StatusCodes.Status400BadRequest, refusal, "");
        }
        var (reason, columnKey) = RefusalOfColumns(request, item.Columns.Where(column => !column.IsHidden).ToList());
        if (reason.Length > 0)
        {
            return (StatusCodes.Status400BadRequest, reason, columnKey);
        }
        return request.AttachmentId != Guid.Empty && await IsFileOnAnEntryAsync(projectId, request.AttachmentId, db, editing) ? (StatusCodes.Status409Conflict, "InvoiceAlreadyAttached", "") : (0, "", "");
    }

    /// <summary>Keeps <paramref name="entry"/> with its <paramref name="details"/>: as a new entry, or in place of <paramref name="existing"/> when one is being changed. Answers the entry kept.</summary>
    public static async Task<Entry> KeepAsync(Entry entry, List<object> details, Entry? existing, Guid userId, RedixDbContext db)
    {
        if (existing is null)
        {
            await SaveAsync(entry, details, userId, db);
            return entry;
        }
        await EntryChanges.ReplaceAsync(existing, entry, details, userId, entry.CreatedAt, db);
        return existing;
    }

    /// <summary>Why the payment method, the account or the file <paramref name="request"/> names cannot be used by <paramref name="userId"/>, who holds <paramref name="held"/>. Empty when they can.</summary>
    private static async Task<string> RefusalOfPaymentAsync(Guid projectId, EntryRequest request, Guid userId, List<Permission> held, RedixDbContext db)
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
    private static (string Reason, string ColumnKey) RefusalOfColumns(EntryRequest request, List<ItemColumn> columns)
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

    /// <summary>A new entry from <paramref name="request"/>, on its one date (D10): sent for approval, or else a draft.</summary>
    public static Entry EntryOf(EntryRequest request, Guid projectId, EntryKind kind, decimal amount, Guid userId, DateTime at) => new()
    {
        Id = request.Id,
        ProjectId = projectId,
        Kind = kind,
        Status = request.Submit ? EntryStatus.PendingApproval : EntryStatus.Draft,
        EntryDate = request.PaymentDate,
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
    private static async Task SaveAsync(Entry entry, List<object> details, Guid userId, RedixDbContext db)
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
