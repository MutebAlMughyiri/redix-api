using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Entries;

/// <summary>Putting a file that arrived later on an expense that has none (UC-039, BR-22): only the file goes on, nothing else about the expense changes, and it is not sent back for approval (redix-api #96, D15).</summary>
public static class InvoiceAttachEndpoints
{
    private const decimal MatchingAmountDifference = 1;

    private const int MatchingDaysDifference = 3;

    /// <summary>Maps the routes, for whoever holds <see cref="Permission.RecordEntries"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapInvoiceAttach(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/entries/attachable", Attachable).RequireAuthorization(nameof(Permission.RecordEntries));
        app.MapProjectChanges(Permission.RecordEntries).MapPost("/entries/{entryId:guid}/invoice", Attach);
    }

    /// <summary>
    /// The expenses of <paramref name="itemId"/> the caller may put a file on, newest first: their own, and with <see cref="Permission.ManageAnyEntry"/> anyone's sent or approved. Those whose supplier, total and date match what a QR code on the file says come first (AC-49.2).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="vatNumber">The seller's VAT number on the file's QR code; empty when there is none.</param>
    /// <param name="total">The total on the QR code.</param>
    /// <param name="date">The date on the QR code.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<AttachableEntryView>> Attachable(Guid projectId, Guid itemId, string vatNumber, decimal total, DateOnly date, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesAll = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.ManageAnyEntry);
        var entries = await AttachableIn(projectId, db)
            .Where(entry => db.ExpenseDetails.Any(row => row.EntryId == entry.Id && row.ItemId == itemId))
            .Where(entry => entry.CreatedByUserId == userId || (seesAll && entry.Status != EntryStatus.Draft))
            .OrderByDescending(entry => entry.CreatedAt).ThenByDescending(entry => entry.Id).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        return entries.Select(entry =>
        {
            var supplierVatNumber = facts.SupplierVatNumberOf(entry);
            var isMatching = vatNumber.Length > 0 && supplierVatNumber == vatNumber && Math.Abs(entry.Amount - total) <= MatchingAmountDifference && Math.Abs(entry.EntryDate.DayNumber - date.DayNumber) <= MatchingDaysDifference;
            return new AttachableEntryView(entry.Id, entry.Kind, entry.Status, entry.EntryDate, entry.Amount, facts.ProductOf(entry), facts.SupplierNameOf(entry), supplierVatNumber, isMatching);
        }).OrderByDescending(row => row.IsMatching).ToList();
    }

    /// <summary>
    /// Puts the file on the expense, in the History table, and changes nothing else (D15). Someone else's draft, or someone else's sent entry to whoever may not manage any entry, answers <c>notYourEntry</c> (BR-42); one that has a file already, is a deduction or credit note, or was reversed answers <c>entryNotAttachable</c>; a file of no such attachment <c>unknownAttachment</c>,
    /// and one already on an entry <c>invoiceAlreadyAttached</c> (BR-22).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">The file.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Attach(Guid projectId, Guid entryId, AttachInvoiceRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await Entry.OfProjectAsync(projectId, entryId, db);
        if (entry == Entry.None)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        if (!await EntryChanges.MayHandleAsync(entry, userId, db))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        if (!await AttachableIn(projectId, db).AnyAsync(row => row.Id == entryId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryNotAttachable");
        }
        if (!await db.Attachments.AnyAsync(attachment => attachment.Id == request.AttachmentId && attachment.ProjectId == projectId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAttachment");
        }
        if (await EntryRecording.IsFileOnAnEntryAsync(projectId, request.AttachmentId, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }

        var role = entry.Kind == EntryKind.ContractorPayment ? EntryAttachmentRole.Receipt : EntryAttachmentRole.Invoice;
        var file = new EntryAttachment { EntryId = entryId, AttachmentId = request.AttachmentId, Role = role };
        db.Add(file);
        db.AddHistory(file, HistoryAction.Create, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entryId, entry.Status, ""));
    }

    /// <summary>The one answer to what can take a file later: an expense with none yet, not a deduction or credit note, which pay nothing, and not removed (redix-api #119).</summary>
    private static IQueryable<Entry> AttachableIn(Guid projectId, RedixDbContext db) =>
        db.Entries.Where(entry => entry.ProjectId == projectId && entry.Amount > 0
            && db.ExpenseDetails.Any(row => row.EntryId == entry.Id)
            && !db.EntryAttachments.Any(file => file.EntryId == entry.Id)
            && entry.Status != EntryStatus.Removed);
}

/// <summary>An expense that can take a file.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Kind">What it is.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="EntryDate">Its date.</param>
/// <param name="Amount">Its total.</param>
/// <param name="Product">What was bought; empty when not said.</param>
/// <param name="SupplierName">Its supplier, or the contract's contractor.</param>
/// <param name="SupplierVatNumber">That supplier's VAT number; empty when none.</param>
/// <param name="IsMatching">True when the file's QR code matches it (AC-49.2).</param>
public record AttachableEntryView(Guid Id, EntryKind Kind, EntryStatus Status, DateOnly EntryDate, decimal Amount, string Product, string SupplierName, string SupplierVatNumber, bool IsMatching);

/// <summary>A file to put on an expense.</summary>
/// <param name="AttachmentId">The file, already kept.</param>
public record AttachInvoiceRequest(Guid AttachmentId);
