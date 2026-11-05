using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Audit;

namespace Redix.Api.Entries;

/// <summary>What Finance enters and what counts at once, with no approval: transfers, capital movements, revenue and sales invoices (BR-30). What is approved never changes after that (BR-11).</summary>
public static class DirectEntries
{
    /// <summary>A new approved entry.</summary>
    public static Entry New(Guid projectId, EntryKind kind, DateOnly date, decimal amount, Guid accountId, string notes, Guid userId, DateTime at) => new()
    {
        Id = Guid.CreateVersion7(),
        ProjectId = projectId,
        Kind = kind,
        Status = EntryStatus.Approved,
        EntryDate = date,
        Amount = amount,
        AccountId = accountId,
        Notes = notes,
        CreatedByUserId = userId,
        CreatedAt = at,
        ApprovedAt = at,
    };

    /// <summary>Adds <paramref name="entry"/> with its <paramref name="details"/>, the audit record of each and its approval; saved with the change.</summary>
    public static void Add(Entry entry, List<object> details, Guid userId, DateTime at, RedixDbContext db)
    {
        db.Add(entry);
        db.Audit(entry, AuditAction.Create, userId, at);
        db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Approve, UserId = userId, At = at });
        foreach (var detail in details)
        {
            db.Add(detail);
            db.Audit(detail, AuditAction.Create, userId, at);
        }
    }

    /// <summary>Why a money movement cannot be kept: <c>invalidDate</c> outside the project's period (BR-12), <c>invalidAmount</c> not above zero or beyond the limit, <c>invalidNotes</c> too long, <c>unknownAccount</c> for an account not active in the project, <c>unknownAttachment</c>, or <c>invoiceAlreadyAttached</c> for a file already on an entry (BR-22). Empty when it can.</summary>
    public static async Task<string> RefusalOfAsync(Guid projectId, DateOnly date, decimal amount, string notes, List<Guid> accountIds, Guid attachmentId, TimeProvider time, RedixDbContext db)
    {
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        if (!Entry.IsAcceptableDate(date, project.StartDate, Entry.Today(time)))
        {
            return "InvalidDate";
        }
        if (amount <= 0 || !Money.IsAcceptable(amount))
        {
            return "InvalidAmount";
        }
        if (notes.Trim().Length > Entry.NotesMaxLength)
        {
            return "InvalidNotes";
        }
        foreach (var accountId in accountIds)
        {
            if (!await db.Accounts.AnyAsync(account => account.Id == accountId && account.ProjectId == projectId && account.IsActive))
            {
                return "UnknownAccount";
            }
        }
        if (attachmentId == Guid.Empty)
        {
            return "";
        }
        if (!await db.Attachments.AnyAsync(attachment => attachment.Id == attachmentId && attachment.ProjectId == projectId))
        {
            return "UnknownAttachment";
        }
        return await EntryRecording.IsFileOnAnEntryAsync(projectId, attachmentId, db) ? "InvoiceAlreadyAttached" : "";
    }
}
