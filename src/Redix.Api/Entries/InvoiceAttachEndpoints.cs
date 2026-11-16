using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Entries;

/// <summary>
/// Putting a file shared to REDIX on an entry (UC-039, BR-22, redix-app #115): any entry its person may handle, of any kind, an approved one too, beside the files it has; only the
/// file goes on, nothing else about the entry changes, and it is not sent back for approval (redix-api #96, D15).
/// </summary>
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
    /// The entries a QR code on a shared file matches (AC-49.2), as the lists show them, which the share screen pins over the list (redix-app #115 D4, D6): those the caller may put
    /// a file on whose supplier's VAT number is the code's, whose total is within one riyal of its total and whose date within three days of its date; none for a file with no code.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="vatNumber">The seller's VAT number on the file's QR code; empty when there is none.</param>
    /// <param name="total">The total on the QR code.</param>
    /// <param name="date">The date on the QR code.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<EntryRowView>> Attachable(Guid projectId, string vatNumber, decimal total, DateOnly date, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var near = await AttachableBy(db.Entries.Where(entry => entry.ProjectId == projectId), userId, held)
            .Where(entry => entry.Amount >= total - MatchingAmountDifference && entry.Amount <= total + MatchingAmountDifference)
            .Where(entry => entry.EntryDate >= date.AddDays(-MatchingDaysDifference) && entry.EntryDate <= date.AddDays(MatchingDaysDifference))
            .ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, near, db);
        var matching = near.Where(entry => vatNumber.Length > 0 && facts.SupplierVatNumberOf(entry) == vatNumber).Select(entry => entry.Id).ToList();
        return await EntriesEndpoints.RowsAsync(projectId, userId, db, entries => entries.Where(entry => matching.Contains(entry.Id)), 0);
    }

    /// <summary>
    /// Puts the file on the entry beside any it has, as its kind's file (D5), in the History table, and changes nothing else (D15). Someone else's draft, or someone else's sent entry
    /// to whoever may not manage any entry, answers <c>notYourEntry</c> (BR-42); a deduction, a credit note or a removed entry answers <c>entryNotAttachable</c>; a file of no such
    /// attachment <c>unknownAttachment</c>, and one already on an entry <c>invoiceAlreadyAttached</c> (BR-22).
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
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        if (!EntryChanges.MayHandle(entry, userId, held))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        if (!await AttachableBy(db.Entries, userId, held).AnyAsync(row => row.Id == entryId))
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

        var file = new EntryAttachment { EntryId = entryId, AttachmentId = request.AttachmentId, Role = entry.Kind.FileRole() };
        db.Add(file);
        db.AddHistory(file, HistoryAction.Create, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entryId, entry.Number, entry.Status, ""));
    }

    /// <summary>
    /// The one answer to which of <paramref name="entries"/> <paramref name="userId"/>, holding <paramref name="held"/>, may put a file on (redix-app #115 D2, D5): any kind, with or
    /// without files, paying something, so no deduction or credit note, not removed (redix-api #119), and theirs to handle (<see cref="EntryChanges.MayHandle(Guid, List{Permission})"/>).
    /// The share screen's list and the attach ask it alike.
    /// </summary>
    /// <param name="entries">The entries asked about.</param>
    /// <param name="userId">Who would put the file on.</param>
    /// <param name="held">What they hold in the project.</param>
    public static IQueryable<Entry> AttachableBy(IQueryable<Entry> entries, Guid userId, List<Permission> held) =>
        entries.Where(entry => entry.Amount > 0 && entry.Status != EntryStatus.Removed).Where(EntryChanges.MayHandle(userId, held));
}

/// <summary>A file to put on an entry.</summary>
/// <param name="AttachmentId">The file, already kept.</param>
public record AttachInvoiceRequest(Guid AttachmentId);
