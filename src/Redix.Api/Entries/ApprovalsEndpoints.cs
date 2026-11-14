using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Notifications;
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

    /// <summary>What waits for a decision in the project, oldest first, each with its file when it has one (AC-18.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<PendingEntryView>> Pending(Guid projectId, RedixDbContext db)
    {
        var entries = await db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval).OrderBy(entry => entry.CreatedAt).ThenBy(entry => entry.Id).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        return entries.Select(entry => new PendingEntryView(entry.Id, entry.Kind, entry.EntryDate, entry.Amount, facts.ItemNameOf(entry), facts.SupplierNameOf(entry), facts.InvoiceOf(entry).Number, facts.AccountNameOf(entry), entry.Description, facts.AttachmentIdOf(entry), facts.ContentTypeOf(entry), facts.FileNameOf(entry), facts.AuthorNameOf(entry), facts.PartnerNameOf(entry), facts.AccountBalanceOf(entry), facts.AccountWaitingOf(entry), facts.AccountShortByOf(entry))).ToList();
    }

    /// <summary>
    /// Approves the entries named, each on its own: what cannot be approved comes back by id with its reason, and nothing is approved in silence (AC-19.2). An entry not found answers <c>unknownEntry</c>, one nobody
    /// is waiting on <c>entryNotPending</c>. The first expense on a supplier waiting for approval approves it too.
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
            var refusal = !entries.TryGetValue(id, out var entry) ? "unknownEntry" : entry.Status != EntryStatus.PendingApproval ? "entryNotPending" : "";
            if (refusal.Length > 0)
            {
                failed.Add(new EntryFailureView(id, refusal));
                continue;
            }
            await ApproveAsync(entry!, userId, at, db);
            approved.Add(id);
        }
        await db.SaveChangesAsync();
        return Results.Ok(new ApprovalView(approved, failed));
    }

    /// <summary>
    /// Approves <paramref name="entry"/>, one waiting, in the name of <paramref name="userId"/> at <paramref name="at"/> (FR-19): its decision, its History line, and who entered it told
    /// unless they decided it themselves. The first expense on a supplier waiting for approval approves it too. The one way an entry is approved, for the inbox and a project file's load
    /// alike (BR-43); the caller saves.
    /// </summary>
    public static async Task ApproveAsync(Entry entry, Guid userId, DateTime at, RedixDbContext db)
    {
        entry.Status = EntryStatus.Approved;
        entry.ApprovedAt = at;
        db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Approve, UserId = userId, At = at });
        db.AddHistory(entry, HistoryAction.Approve, userId, at);
        NotificationsEndpoints.TellAuthor(entry, NotificationKind.EntryApproved, userId, at, db);
        await ApproveItsSupplierAsync(entry.Id, userId, at, db);
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
        var entry = await Entry.OfProjectAsync(projectId, entryId, db);
        if (entry == Entry.None)
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
        db.AddHistory(entry, HistoryAction.Reject, userId, at);
        NotificationsEndpoints.TellAuthor(entry, NotificationKind.EntryRejected, userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task ApproveItsSupplierAsync(Guid entryId, Guid userId, DateTime at, RedixDbContext db)
    {
        var bought = db.PurchaseDetails.Where(row => row.EntryId == entryId).Select(row => row.SupplierId);
        var paid = db.ContractorPaymentDetails.Where(row => row.EntryId == entryId).Join(db.Contracts, row => row.ContractId, contract => contract.Id, (_, contract) => contract.SupplierId);
        var supplier = await db.Suppliers.SingleOrDefaultAsync(row => row.Status == SupplierStatus.PendingApproval && (bought.Contains(row.Id) || paid.Contains(row.Id)));
        if (supplier is not null)
        {
            supplier.Status = SupplierStatus.Approved;
            db.AddHistory(supplier, HistoryAction.Update, userId, at);
        }
    }
}

/// <summary>An entry in Finance's inbox.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Kind">What it is.</param>
/// <param name="EntryDate">Its date.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="ItemName">Its item: an expense's or a revenue entry's.</param>
/// <param name="SupplierName">Its supplier, or the contract's contractor.</param>
/// <param name="InvoiceNumber">The number of an older bill that had one; empty for the rest.</param>
/// <param name="AccountName">What paid it.</param>
/// <param name="Description">«الوصف», what it says.</param>
/// <param name="AttachmentId">Its file; empty when it has none.</param>
/// <param name="ContentType">What the file is; empty when it has none.</param>
/// <param name="FileName">The file's name; empty when it has none.</param>
/// <param name="AuthorName">Who entered it.</param>
/// <param name="PartnerName">A capital movement's partner; empty for any other entry (redix-api #101).</param>
/// <param name="AccountBalance">What its account holds now (redix-app #89).</param>
/// <param name="AccountWaiting">What waits to leave its account, this entry among it.</param>
/// <param name="AccountShortBy">How much more what waits on its account takes than the account holds; zero when it covers all of it, so the inbox notes only a short one (R5, D4).</param>
public record PendingEntryView(Guid Id, EntryKind Kind, DateOnly EntryDate, decimal Amount, string ItemName, string SupplierName, string InvoiceNumber, string AccountName, string Description, string AttachmentId, string ContentType, string FileName, string AuthorName, string PartnerName, decimal AccountBalance, decimal AccountWaiting, decimal AccountShortBy);

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
