using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>Putting an invoice that arrived later on an expense entered without one (UC-039, FR-49, BR-22).</summary>
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
    /// What in <paramref name="itemId"/> can take an invoice, matching ones first and then the newest: everything for whoever holds <see cref="Permission.ManageAnyEntry"/>, and the caller's own for anyone else (AC-49.4). One is matching when its supplier
    /// holds <paramref name="vatNumber"/>, its amount is within a riyal of <paramref name="total"/> and its date within three days of <paramref name="date"/> (AC-49.2).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="vatNumber">The seller's VAT number on the invoice's QR code; empty when there is none.</param>
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
    /// Puts the invoice on the entry, with its figures from <see cref="AttachInvoiceRequest.Total"/>, or the entry's amount when left out. What is approved goes back to approval, since what Finance approved has changed (AC-49.3); anything else stays where it stands, and Finance sees the difference when it decides.
    /// Someone else's entry answers <c>notYourEntry</c>; one that cannot take an invoice, <c>entryNotAttachable</c>. A file of no such attachment answers <c>unknownAttachment</c>, and one already on an entry <c>invoiceAlreadyAttached</c>. The same supplier's number on another entry answers <c>duplicateInvoice</c>, which BR-22 does not let anyone confirm.
    /// What was typed answers <c>invalidInvoiceNumber</c>, <c>invalidInvoiceDate</c>, <c>invalidVatNumber</c> or <c>invalidAmount</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">The file and the invoice's facts.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Attach(Guid projectId, Guid entryId, AttachInvoiceRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var access = await UserAccess.OfAsync(userId, db);
        if (entry.CreatedByUserId != userId && !access.In(projectId).Contains(Permission.ManageAnyEntry))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        if (!await AttachableIn(projectId, db).AnyAsync(row => row.Id == entryId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryNotAttachable");
        }

        var number = request.InvoiceNumber.Trim();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        var refusal = RefusalOfTyped(request, number, project.StartDate, Entry.Today(time));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if (!await db.Attachments.AnyAsync(attachment => attachment.Id == request.AttachmentId && attachment.ProjectId == projectId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAttachment");
        }
        if (await EntryRecording.IsFileOnAnEntryAsync(projectId, request.AttachmentId, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }
        var facts = await EntryFacts.OfAsync(projectId, [entry], db);
        var supplier = facts.SupplierOf(entry);
        var earlier = await EntryRecording.EarlierEntryAsync(supplier, number, access, db, entryId);
        if (earlier is not null)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "DuplicateInvoice", "earlier", earlier);
        }

        var at = time.GetUtcNow().UtcDateTime;
        var rate = supplier.VatNumber.Length == 0 ? 0 : project.VatRate;
        var figures = InvoiceFigures.Of(request.Total == 0 ? entry.Amount : request.Total, true, 0, rate);
        var invoice = EntryRecording.InvoiceOf(entryId, number, request.InvoiceDate, figures, false, request.SellerVatNumber.Trim());
        var file = new EntryAttachment { EntryId = entryId, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.Invoice };
        var expense = await db.ExpenseDetails.SingleAsync(row => row.EntryId == entryId);
        expense.WithoutInvoiceReason = "";
        db.Add(invoice);
        db.Add(file);
        db.Audit(invoice, AuditAction.Create, userId, at);
        db.Audit(file, AuditAction.Create, userId, at);
        db.Audit(expense, AuditAction.Update, userId, at);
        if (entry.Status == EntryStatus.Approved)
        {
            entry.Status = EntryStatus.PendingApproval;
            entry.ApprovedAt = Entry.NotApproved;
            db.Audit(entry, AuditAction.Submit, userId, at);
            db.EntryDecisions.Add(new EntryDecision { EntryId = entryId, Action = EntryDecisionAction.Submit, UserId = userId, At = at });
            await NotificationsEndpoints.TellDecidersAsync(entry, userId, at, db);
        }
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entryId, entry.Status, ""));
    }

    /// <summary>The one answer to what can take an invoice: an expense entered without one, not a deduction, which has none by nature (FR-14), and not reversed, or itself a reversal.</summary>
    private static IQueryable<Entry> AttachableIn(Guid projectId, RedixDbContext db) =>
        db.Entries.Where(entry => entry.ProjectId == projectId && entry.Amount > 0
            && db.ExpenseDetails.Any(row => row.EntryId == entry.Id && row.WithoutInvoiceReason != "")
            && !db.EntryLinks.Any(link => link.Type == EntryLinkType.Reversal && (link.EntryId == entry.Id || link.LinkedEntryId == entry.Id)));

    private static string RefusalOfTyped(AttachInvoiceRequest request, string number, DateOnly projectStart, DateOnly today)
    {
        if (number.Length > Invoice.NumberMaxLength)
        {
            return "InvalidInvoiceNumber";
        }
        if (!Entry.IsAcceptableDate(request.InvoiceDate, projectStart, today))
        {
            return "InvalidInvoiceDate";
        }
        if (request.SellerVatNumber.Trim().Length > Supplier.VatNumberLength)
        {
            return "InvalidVatNumber";
        }
        return request.Total < 0 || !Money.IsAcceptable(request.Total) ? "InvalidAmount" : "";
    }
}

/// <summary>An expense that can take an invoice.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Kind">What it is.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="EntryDate">Its date.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="Product">What was bought; empty when not said.</param>
/// <param name="SupplierName">Its supplier, or the contract's contractor.</param>
/// <param name="SupplierVatNumber">That supplier's VAT number; empty when none.</param>
/// <param name="IsMatching">True when the invoice's QR code matches it (AC-49.2).</param>
public record AttachableEntryView(Guid Id, EntryKind Kind, EntryStatus Status, DateOnly EntryDate, decimal Amount, string Product, string SupplierName, string SupplierVatNumber, bool IsMatching);

/// <summary>An invoice to put on an expense.</summary>
/// <param name="AttachmentId">Its file, already kept.</param>
/// <param name="InvoiceNumber">Its number.</param>
/// <param name="InvoiceDate">Its date.</param>
/// <param name="Total">Its total with VAT, from its QR code; left out, it is the entry's amount.</param>
/// <param name="SellerVatNumber">The seller's VAT number on its QR code; left out when there is none.</param>
public record AttachInvoiceRequest(Guid AttachmentId, string InvoiceNumber, DateOnly InvoiceDate, decimal Total = 0, string SellerVatNumber = "");
