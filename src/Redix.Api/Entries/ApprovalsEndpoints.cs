using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>Finance's inbox and its decisions (UC-031, UC-032, FR-18, FR-19).</summary>
public static class ApprovalsEndpoints
{
    /// <summary>The longest reason for a rejection.</summary>
    public const int ReasonMaxLength = 500;

    /// <summary>Maps the routes, for whoever decides entries in the project.</summary>
    /// <param name="app">The app.</param>
    public static void MapApprovals(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/entries/pending", Pending).RequireAuthorization(nameof(Permission.ApproveEntries));
        var changes = app.MapProjectChanges(Permission.ApproveEntries);
        changes.MapPost("/entries/approve", Approve);
        changes.MapPost("/entries/{entryId:guid}/reject", Reject);
    }

    /// <summary>What waits for a decision in the project, oldest first, each with its file or why it has none (AC-18.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<PendingEntryView>> Pending(Guid projectId, RedixDbContext db)
    {
        var entries = await db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval).OrderBy(entry => entry.CreatedAt).ThenBy(entry => entry.Id).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        return entries.Select(entry => new PendingEntryView(entry.Id, entry.Kind, entry.EntryDate, entry.Amount, facts.ItemNameOf(entry), facts.SupplierNameOf(entry), facts.InvoiceOf(entry).Number, facts.InvoiceOf(entry).Vat, facts.AccountNameOf(entry), entry.Notes, facts.ExpenseOf(entry).WithoutInvoiceReason, facts.AttachmentIdOf(entry), facts.ContentTypeOf(entry), facts.AuthorNameOf(entry), facts.InvoiceOf(entry).Total, facts.InvoiceOf(entry).SellerVatNumber, facts.SupplierVatNumberOf(entry))).ToList();
    }

    /// <summary>
    /// Approves the entries named, each on its own: what cannot be approved comes back by id with its reason, and nothing is approved in silence (AC-19.2). An entry not found answers <c>unknownEntry</c>, one nobody
    /// is waiting on <c>entryNotPending</c>, and a credit note whose invoice is not approved and is not among these, <c>originalNotApproved</c>. The first expense on a supplier waiting for approval approves it too.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The entries.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Approve(Guid projectId, ApproveEntriesRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var named = request.EntryIds.Distinct().ToList();
        var entries = await db.Entries.Where(entry => entry.ProjectId == projectId && named.Contains(entry.Id)).ToDictionaryAsync(entry => entry.Id);
        var approved = new List<Guid>();
        var failed = new List<EntryFailureView>();
        foreach (var id in named)
        {
            var refusal = !entries.TryGetValue(id, out var entry) ? "unknownEntry" : entry.Status != EntryStatus.PendingApproval ? "entryNotPending" : await RefusalOfOriginalAsync(id, named, db);
            if (refusal.Length > 0)
            {
                failed.Add(new EntryFailureView(id, refusal));
                continue;
            }
            entry!.Status = EntryStatus.Approved;
            entry.ApprovedAt = at;
            db.EntryDecisions.Add(new EntryDecision { EntryId = id, Action = EntryDecisionAction.Approve, UserId = userId, At = at });
            db.Audit(entry, AuditAction.Approve, userId, at);
            NotificationsEndpoints.TellAuthor(entry, NotificationKind.EntryApproved, userId, at, db);
            await ApproveItsSupplierAsync(id, userId, at, db);
            approved.Add(id);
        }
        await db.SaveChangesAsync();
        return Results.Ok(new ApprovalView(approved, failed));
    }

    /// <summary>Rejects an entry, saying why (AC-19.1): it returns to who entered it, to fix and send again. No reason answers <c>reasonRequired</c>, and an entry nobody is waiting on <c>entryNotPending</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">Why.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Reject(Guid projectId, Guid entryId, RejectEntryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var reason = request.Reason.Trim();
        if (reason.Length is 0 or > ReasonMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ReasonRequired");
        }
        if (entry.Status != EntryStatus.PendingApproval)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryNotPending");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        entry.Status = EntryStatus.Rejected;
        db.EntryDecisions.Add(new EntryDecision { EntryId = entryId, Action = EntryDecisionAction.Reject, UserId = userId, At = at, Reason = reason });
        db.Audit(entry, AuditAction.Reject, userId, at);
        NotificationsEndpoints.TellAuthor(entry, NotificationKind.EntryRejected, userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<string> RefusalOfOriginalAsync(Guid entryId, List<Guid> named, RedixDbContext db)
    {
        var originals = await db.EntryLinks.Where(link => link.EntryId == entryId && link.Type == EntryLinkType.CreditNoteOf).Select(link => link.LinkedEntryId).ToListAsync();
        var waiting = await db.Entries.AnyAsync(entry => originals.Contains(entry.Id) && entry.Status != EntryStatus.Approved && !named.Contains(entry.Id));
        return waiting ? "originalNotApproved" : "";
    }

    private static async Task ApproveItsSupplierAsync(Guid entryId, Guid userId, DateTime at, RedixDbContext db)
    {
        var bought = db.PurchaseDetails.Where(row => row.EntryId == entryId).Select(row => row.SupplierId);
        var paid = db.ContractorPaymentDetails.Where(row => row.EntryId == entryId).Join(db.Contracts, row => row.ContractId, contract => contract.Id, (_, contract) => contract.SupplierId);
        var supplier = await db.Suppliers.SingleOrDefaultAsync(row => row.Status == SupplierStatus.PendingApproval && (bought.Contains(row.Id) || paid.Contains(row.Id)));
        if (supplier is not null)
        {
            supplier.Status = SupplierStatus.Approved;
            db.Audit(supplier, AuditAction.Update, userId, at);
        }
    }
}

/// <summary>An entry in Finance's inbox.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Kind">What it is.</param>
/// <param name="EntryDate">Its date.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="ItemName">Its item.</param>
/// <param name="SupplierName">Its supplier, or the contract's contractor.</param>
/// <param name="InvoiceNumber">Its invoice's number; empty when it has none.</param>
/// <param name="Vat">Its invoice's VAT.</param>
/// <param name="AccountName">What paid it.</param>
/// <param name="Notes">What was noted.</param>
/// <param name="WithoutInvoiceReason">Why it has no invoice; empty when it has one.</param>
/// <param name="AttachmentId">Its file; empty when it has none.</param>
/// <param name="ContentType">What the file is; empty when it has none.</param>
/// <param name="AuthorName">Who entered it.</param>
/// <param name="InvoiceTotal">Its invoice's total, which may differ from what was paid (AC-49.3).</param>
/// <param name="SellerVatNumber">The seller's VAT number on an invoice attached later; empty otherwise.</param>
/// <param name="SupplierVatNumber">Its supplier's VAT number, to compare with that.</param>
public record PendingEntryView(Guid Id, EntryKind Kind, DateOnly EntryDate, decimal Amount, string ItemName, string SupplierName, string InvoiceNumber, decimal Vat, string AccountName, string Notes, string WithoutInvoiceReason, string AttachmentId, string ContentType, string AuthorName, decimal InvoiceTotal, string SellerVatNumber, string SupplierVatNumber);

/// <summary>The entries to approve.</summary>
/// <param name="EntryIds">Their ids.</param>
public record ApproveEntriesRequest(List<Guid> EntryIds);

/// <summary>Why an entry is rejected.</summary>
/// <param name="Reason">The reason.</param>
public record RejectEntryRequest(string Reason);

/// <summary>What became of a request to approve.</summary>
/// <param name="Approved">What was approved.</param>
/// <param name="Failed">What was not, and why.</param>
public record ApprovalView(List<Guid> Approved, List<EntryFailureView> Failed);

/// <summary>An entry that could not be approved.</summary>
/// <param name="EntryId">The entry.</param>
/// <param name="Code">Why.</param>
public record EntryFailureView(Guid EntryId, string Code);
