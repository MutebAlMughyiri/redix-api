using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Audit;
using Redix.Api.Notifications;

namespace Redix.Api.Entries;

/// <summary>
/// What Finance enters outside the expense form: capital movements and revenue, which wait for approval like an expense (BR-30, Muteb 8 Nov 2026, redix-api #101), and a
/// reversal, which whoever approves enters approved (FR-50). What is approved never changes after that (BR-11).
/// </summary>
public static class DirectEntries
{
    /// <summary>A new entry, sent for approval.</summary>
    public static Entry Pending(Guid projectId, EntryKind kind, DateOnly date, decimal amount, Guid accountId, string notes, Guid userId, DateTime at) =>
        New(projectId, kind, EntryStatus.PendingApproval, date, amount, accountId, notes, userId, at, Entry.NotApproved);

    /// <summary>A new entry, approved by whoever enters it: a reversal (FR-50).</summary>
    public static Entry Approved(Guid projectId, EntryKind kind, DateOnly date, decimal amount, Guid accountId, string notes, Guid userId, DateTime at) =>
        New(projectId, kind, EntryStatus.Approved, date, amount, accountId, notes, userId, at, at);

    private static Entry New(Guid projectId, EntryKind kind, EntryStatus status, DateOnly date, decimal amount, Guid accountId, string notes, Guid userId, DateTime at, DateTime approvedAt) => new()
    {
        Id = Guid.CreateVersion7(),
        ProjectId = projectId,
        Kind = kind,
        Status = status,
        EntryDate = date,
        Amount = amount,
        AccountId = accountId,
        Notes = notes,
        CreatedByUserId = userId,
        CreatedAt = at,
        ApprovedAt = approvedAt,
    };

    /// <summary>
    /// Adds <paramref name="entry"/> with its <paramref name="details"/>, the audit record of each and its decision: sent, telling whoever approves, as an expense is (FR-21), or approved.
    /// Saved with the change.
    /// </summary>
    public static async Task AddAsync(Entry entry, List<object> details, Guid userId, DateTime at, RedixDbContext db)
    {
        var sent = entry.Status == EntryStatus.PendingApproval;
        db.Add(entry);
        db.Audit(entry, sent ? AuditAction.Submit : AuditAction.Create, userId, at);
        db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = sent ? EntryDecisionAction.Submit : EntryDecisionAction.Approve, UserId = userId, At = at });
        if (sent)
        {
            await NotificationsEndpoints.TellDecidersAsync(entry, userId, at, db);
        }
        foreach (var detail in details)
        {
            db.Add(detail);
            db.Audit(detail, AuditAction.Create, userId, at);
        }
    }

    /// <summary>The answer to <paramref name="refusal"/>, as <see cref="RefusalOfAsync"/> gives one: a conflict for a file already on an entry, a bad request for anything else.</summary>
    public static IResult AnswerOf(string refusal) => Refusal.Of(refusal == "InvoiceAlreadyAttached" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, refusal);

    /// <summary>Why a money movement cannot be kept: <c>invalidDate</c> outside the project's period (BR-12), <c>invalidAmount</c> not above zero or beyond the limit, <c>invalidNotes</c> too long, <c>unknownAccount</c> for an account not active in the project, <c>unknownAttachment</c>, or <c>invoiceAlreadyAttached</c> for a file already on an entry (BR-22). Empty when it can.</summary>
    public static async Task<string> RefusalOfAsync(Guid projectId, DateOnly date, decimal amount, string notes, Guid accountId, Guid attachmentId, TimeProvider time, RedixDbContext db)
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
        if (!await db.Accounts.AnyAsync(account => account.Id == accountId && account.ProjectId == projectId && account.IsActive))
        {
            return "UnknownAccount";
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
