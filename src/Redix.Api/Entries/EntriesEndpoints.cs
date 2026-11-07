using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What any entry shares once it is in the ledger: reading it, and sending it for approval (UC-028).</summary>
public static class EntriesEndpoints
{
    /// <summary>Maps the route that reads an entry, for anyone who reads the project, and the one that sends it, for whoever holds <see cref="Permission.RecordEntries"/>.</summary>
    public static void MapEntries(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/entries/{entryId:guid}", Read).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entries/mine", Mine).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entries", List).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.RecordEntries);
        changes.MapPost("/entries/{entryId:guid}/submit", Submit);
        changes.MapPost("/entries/{entryId:guid}/notes", ChangeNotes);
        app.MapProjectChanges(Permission.ApproveEntries).MapPost("/entries/{entryId:guid}/reverse", Reverse);
        app.MapProjectChanges(Permission.DeleteEntries).MapPost("/entries/{entryId:guid}/delete", Delete);
    }

    /// <summary>Where the caller's own entries stand in the project, newest first.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<EntryRowView>> Mine(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        return RowsAsync(projectId, userId, db, entry => entry.ProjectId == projectId && (entry.Kind == EntryKind.Purchase || entry.Kind == EntryKind.ContractorPayment) && entry.CreatedByUserId == userId);
    }

    /// <summary>Every entry of the project, newest first: everyone who reads a project sees all its expenses, and drafts only of their own.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<EntryRowView>> List(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        return RowsAsync(projectId, userId, db, entry => entry.ProjectId == projectId && (entry.Kind == EntryKind.Purchase || entry.Kind == EntryKind.ContractorPayment) && (entry.Status != EntryStatus.Draft || entry.CreatedByUserId == userId));
    }

    private static async Task<List<EntryRowView>> RowsAsync(Guid projectId, Guid userId, RedixDbContext db, System.Linq.Expressions.Expression<Func<Entry, bool>> shown)
    {
        var entries = await db.Entries.Where(shown).OrderByDescending(entry => entry.CreatedAt).ThenByDescending(entry => entry.Id).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var ids = entries.Select(entry => entry.Id).ToList();
        var rejections = (await db.EntryDecisions.Where(row => ids.Contains(row.EntryId) && row.Action == EntryDecisionAction.Reject).OrderBy(row => row.Id).ToListAsync()).GroupBy(row => row.EntryId).ToDictionary(group => group.Key, group => group.Last().Reason);
        var reversed = (await db.EntryLinks.Where(link => link.Type == EntryLinkType.Reversal && (ids.Contains(link.EntryId) || ids.Contains(link.LinkedEntryId))).ToListAsync()).SelectMany(link => new[] { link.EntryId, link.LinkedEntryId }).ToHashSet();
        return entries.Select(entry => new EntryRowView(
            entry.Id,
            entry.Kind,
            entry.Status,
            entry.Status == EntryStatus.Rejected ? rejections.GetValueOrDefault(entry.Id, "") : "",
            entry.EntryDate,
            entry.Amount,
            facts.ItemNameOf(entry),
            facts.SupplierNameOf(entry),
            facts.InvoiceOf(entry).Number,
            facts.AuthorNameOf(entry),
            entry.CreatedByUserId == userId,
            reversed.Contains(entry.Id))).ToList();
    }

    /// <summary>One entry of the project, with what its kind has. What it does not have comes empty, with an empty id. Anything else answers 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Read(Guid projectId, Guid entryId, RedixDbContext db)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        return entry is null ? Results.NotFound() : Results.Ok(await ViewOfAsync(entry, db));
    }

    /// <summary>
    /// Sends a draft or a rejected entry for approval, and answers it (UC-028). Only its author sends it, unless the caller holds <see cref="Permission.ManageAnyEntry"/>: otherwise <c>notYourEntry</c>. Anything already sent answers <c>entryNotSubmittable</c>.
    /// What it still lacks answers <c>entryIncomplete</c> with every key under <c>missing</c>: <c>attachment</c> for a file with no reason in its place (FR-09), and each required column left empty (FR-41). Telling Finance is UC-036 "Send push notifications".
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Submit(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        if (entry.CreatedByUserId != userId && !(await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.ManageAnyEntry))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        if (!entry.CanBeSubmitted())
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryNotSubmittable");
        }
        var missing = await MissingOfAsync(entry, db);
        if (missing.Count > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "EntryIncomplete", "missing", missing);
        }

        entry.Status = EntryStatus.PendingApproval;
        db.Audit(entry, AuditAction.Submit, userId, time.GetUtcNow().UtcDateTime);
        db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Submit, UserId = userId, At = time.GetUtcNow().UtcDateTime });
        await NotificationsEndpoints.TellDecidersAsync(entry, userId, time.GetUtcNow().UtcDateTime, db);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(entry, db));
    }

    /// <summary>Removes an entry never approved (UC-033), with the refusals of <see cref="EntryChanges.RefusalOf"/>. One a credit note names answers <c>entryHasCreditNote</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var (status, refusal) = EntryChanges.RefusalOf(entry, userId, (await UserAccess.OfAsync(userId, db)).In(projectId));
        if (refusal.Length > 0)
        {
            return Refusal.Of(status, refusal);
        }
        if (await db.EntryLinks.AnyAsync(link => link.LinkedEntryId == entryId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryHasCreditNote");
        }
        await EntryChanges.DeleteAsync(entry, userId, time.GetUtcNow().UtcDateTime, db);
        return Results.Ok();
    }

    /// <summary>Changes an entry's notes, which is allowed even once it is approved (AC-50.2, BR-11), for who entered it or whoever holds <see cref="Permission.ManageAnyEntry"/>; anyone else gets <c>notYourEntry</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">The notes.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> ChangeNotes(Guid projectId, Guid entryId, ChangeNotesRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        if (entry.CreatedByUserId != userId && !(await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.ManageAnyEntry))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "NotYourEntry");
        }
        entry.Notes = request.Notes.Trim();
        db.Audit(entry, AuditAction.Update, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>
    /// Reverses an approved entry (UC-035, FR-50): an opposite entry of the same amount, tied to the original and approved at once, so both show in the reports and the net effect is zero. The original does not change.
    /// What was entered as one reverses as one, a transfer's two sides, a carry's two sides, or an expense and the capital it gave. No reason answers <c>reasonRequired</c>; an entry not approved <c>entryNotApproved</c>; one already reversed, or itself a reversal, <c>alreadyReversed</c>; and one tied to a project the caller cannot change, <c>tiedProjectRefused</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">Why.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Reverse(Guid projectId, Guid entryId, RejectEntryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var reason = request.Reason.Trim();
        if (reason.Length is 0 or > ApprovalsEndpoints.ReasonMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ReasonRequired");
        }
        if (entry.Status != EntryStatus.Approved)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryNotApproved");
        }
        if (await db.EntryLinks.AnyAsync(link => link.Type == EntryLinkType.Reversal && (link.LinkedEntryId == entryId || link.EntryId == entryId)))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "AlreadyReversed");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var today = Entry.Today(time);
        var tied = await EntryReversal.TiedToAsync(entryId, db);
        var access = await UserAccess.OfAsync(userId, db);
        var otherProjects = tied.Select(row => row.ProjectId).Where(id => id != projectId).ToList();
        if (otherProjects.Any(id => !access.In(id).Contains(Permission.ApproveEntries)) || await db.Projects.AnyAsync(project => otherProjects.Contains(project.Id) && project.Status != ProjectStatus.Running))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "TiedProjectRefused");
        }
        var reversal = await EntryReversal.AddAsync(entry, reason, userId, at, today, db);
        foreach (var other in tied)
        {
            await EntryReversal.AddAsync(other, reason, userId, at, today, db);
        }
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 })
        {
            // A second request for the same entry passed the check above before the first was saved: the one-reversal index refuses it.
            return Refusal.Of(StatusCodes.Status409Conflict, "AlreadyReversed");
        }
        return Results.Created($"/projects/{projectId}/entries/{reversal.Id}", new EntryStatusView(reversal.Id, reversal.Status, ""));
    }

    private static async Task<List<string>> MissingOfAsync(Entry entry, RedixDbContext db)
    {
        var expense = await db.ExpenseDetails.SingleAsync(row => row.EntryId == entry.Id);
        var purchase = await db.PurchaseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new PurchaseDetails();
        var columns = await db.Set<ItemColumn>().Where(column => column.ItemId == expense.ItemId && !column.IsHidden).OrderBy(column => column.SortOrder).ToListAsync();
        var answers = new EntryAnswers(entry.Kind, purchase.Product, purchase.UnitPrice, purchase.Quantity, entry.Notes, expense.CustomValuesByKey());
        return answers.MissingOf(columns);
    }

    private static async Task<EntryView> ViewOfAsync(Entry entry, RedixDbContext db) => new(
        entry,
        await db.ExpenseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new ExpenseDetails(),
        await db.PurchaseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new PurchaseDetails(),
        await db.ContractorPaymentDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new ContractorPaymentDetails(),
        await db.Invoices.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new Invoice(),
        await db.EntryAttachments.Where(row => row.EntryId == entry.Id).ToListAsync(),
        await db.EntryLinks.Where(row => row.EntryId == entry.Id).ToListAsync());
}

/// <summary>An entry on a list.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Kind">What it is.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="RejectionReason">Why it was rejected, while it stands rejected; empty otherwise.</param>
/// <param name="EntryDate">Its date.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="ItemName">Its item.</param>
/// <param name="SupplierName">Its supplier, or its contract's contractor.</param>
/// <param name="InvoiceNumber">Its invoice's number; empty when it has none.</param>
/// <param name="AuthorName">Who entered it.</param>
/// <param name="IsMine">True when the caller entered it.</param>
/// <param name="IsReversed">True when it was reversed, or is itself a reversal.</param>
public record EntryRowView(Guid Id, EntryKind Kind, EntryStatus Status, string RejectionReason, DateOnly EntryDate, decimal Amount, string ItemName, string SupplierName, string InvoiceNumber, string AuthorName, bool IsMine, bool IsReversed);

/// <summary>An entry's notes.</summary>
/// <param name="Notes">The notes; may be empty.</param>
public record ChangeNotesRequest(string Notes);

/// <summary>An entry and everything its kind has. What it does not have is empty, with an empty <c>entryId</c>, so a reader asks nothing but that.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="Expense">What every expense has.</param>
/// <param name="Purchase">What only a purchase has.</param>
/// <param name="ContractorPayment">What only a contractor payment has.</param>
/// <param name="Invoice">The bill number it was entered with before REDIX stopped asking for one; empty on a newer entry (redix-api #96, D2).</param>
/// <param name="Attachments">The files it carries.</param>
/// <param name="Links">What ties it to other entries.</param>
public record EntryView(Entry Entry, ExpenseDetails Expense, PurchaseDetails Purchase, ContractorPaymentDetails ContractorPayment, Invoice Invoice, List<EntryAttachment> Attachments, List<EntryLink> Links);
