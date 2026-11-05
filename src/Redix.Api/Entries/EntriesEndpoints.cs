using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What any entry shares once it is in the ledger: reading it, and sending it for approval (UC-028).</summary>
public static class EntriesEndpoints
{
    /// <summary>The key a refusal gives for a file an entry lacks, which is the file's own column.</summary>
    public const string FileKey = "attachment";

    /// <summary>Maps the route that reads an entry, for anyone who reads the project, and the one that sends it, for whoever holds <see cref="Permission.RecordEntries"/>.</summary>
    public static void MapEntries(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/entries/{entryId:guid}", Read).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapProjectChanges(Permission.RecordEntries).MapPost("/entries/{entryId:guid}/submit", Submit);
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
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(entry, db));
    }

    private static async Task<List<string>> MissingOfAsync(Entry entry, RedixDbContext db)
    {
        var expense = await db.ExpenseDetails.SingleAsync(row => row.EntryId == entry.Id);
        var purchase = await db.PurchaseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new PurchaseDetails();
        var hasFile = await db.EntryAttachments.AnyAsync(row => row.EntryId == entry.Id);
        var columns = await db.Set<ItemColumn>().Where(column => column.ItemId == expense.ItemId && !column.IsHidden).OrderBy(column => column.SortOrder).ToListAsync();
        var answers = new EntryAnswers(entry.Kind, purchase.Product, purchase.UnitPrice, purchase.Quantity, entry.Notes, expense.CustomValuesByKey());
        return [.. hasFile || expense.WithoutInvoiceReason.Length > 0 ? [] : new[] { FileKey }, .. answers.MissingOf(columns)];
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

/// <summary>An entry and everything its kind has. What it does not have is empty, with an empty <c>entryId</c>, so a reader asks nothing but that.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="Expense">What every expense has.</param>
/// <param name="Purchase">What only a purchase has.</param>
/// <param name="ContractorPayment">What only a contractor payment has.</param>
/// <param name="Invoice">Its invoice, which the VAT statement goes by.</param>
/// <param name="Attachments">The files it carries.</param>
/// <param name="Links">What ties it to other entries.</param>
public record EntryView(Entry Entry, ExpenseDetails Expense, PurchaseDetails Purchase, ContractorPaymentDetails ContractorPayment, Invoice Invoice, List<EntryAttachment> Attachments, List<EntryLink> Links);
