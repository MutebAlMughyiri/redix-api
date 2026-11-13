using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Revenue;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What any entry shares once it is in the ledger: reading it, and sending it for approval (UC-028).</summary>
public static class EntriesEndpoints
{
    /// <summary>
    /// Maps the routes that read an entry, the caller's own and the project's list, for anyone who reads the project; those that send it and change its description, for whoever holds
    /// <see cref="Permission.RecordEntries"/>; and deleting, for anyone who reads the project, since what it asks for follows the entry:
    /// <see cref="Permission.DeleteEntries"/> before approval, <see cref="Permission.RemoveApprovedEntries"/> after it (redix-api #119).
    /// </summary>
    public static void MapEntries(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/entries/{entryId:guid}", Read).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entries/{entryId:guid}/row", Row).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entries/mine", Mine).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entries", List).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.RecordEntries);
        changes.MapPost("/entries/{entryId:guid}/submit", Submit);
        changes.MapPut("/entries/{entryId:guid}/description", ChangeDescription);
        app.MapProjectChanges(Permission.ReadProject).MapDelete("/entries/{entryId:guid}", Delete);
        app.MapProjectChanges(Permission.RemoveApprovedEntries).MapPost("/entries/{entryId:guid}/restore", Restore);
    }

    /// <summary>Where the caller's own entries stand in the project, newest first.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<EntryRowView>> Mine(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        return RowsAsync(projectId, userId, db, entry => entry.CreatedByUserId == userId, 0);
    }

    /// <summary>
    /// The entries of the project <paramref name="principal"/> may see, newest first: everyone who reads a project sees all its expenses and revenue (Muteb, 8 Nov 2026), as <see cref="SeenByAsync"/> decides.
    /// Only those an <see cref="EntrySearch"/> keeps when anything narrows them (redix-app #104), and only the newest <paramref name="take"/> of them when it is above 0, as the
    /// app's home and Entries screen read them a page at a time (redix-app #105); every one otherwise.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="kinds">The kinds kept, as their names travel; none for every kind.</param>
    /// <param name="statuses">Where the kept entries stand; none for any.</param>
    /// <param name="take">How many of the newest to give; 0 for every one.</param>
    /// <param name="text">What was typed; empty for any.</param>
    /// <param name="mine">True to keep only the caller's own.</param>
    /// <param name="from">The first day kept, as an ISO date; empty for no first day.</param>
    /// <param name="to">The last day kept, as an ISO date; empty for no last day.</param>
    public static async Task<List<EntryRowView>> List(
        Guid projectId,
        ClaimsPrincipal principal,
        RedixDbContext db,
        [FromQuery] string[] kinds,
        [FromQuery] string[] statuses,
        int take = 0,
        string text = "",
        bool mine = false,
        string from = "",
        string to = "")
    {
        var userId = AccessTokens.UserIdOf(principal);
        var search = EntrySearch.Of(text, mine, kinds, statuses, from, to);
        if (!search.Narrows)
        {
            return await RowsAsync(projectId, userId, db, entry => true, take);
        }
        var kept = (await RowsAsync(projectId, userId, db, entry => true, 0)).Where(search.Keeps);
        return [.. take > 0 ? kept.Take(take) : kept];
    }

    /// <summary>
    /// The project's entries <paramref name="userId"/> may see, never a removed one (redix-api #119): a draft only for who entered it (BR-42), and a partner's money only for whoever sees that partner (BR-31, <see cref="Partners.PartnerSight"/>).
    /// The one owner of both rules: the list, one entry read by its id and the account history read it.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">Who is looking.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IQueryable<Entry>> SeenByAsync(Guid projectId, Guid userId, RedixDbContext db)
    {
        var sight = await Partners.PartnerSight.OfUserAsync(userId, projectId, db);
        var seesAll = sight.SeesAll;
        var chosen = sight.Chosen;
        return db.Entries.Where(entry => entry.ProjectId == projectId
            && entry.Status != EntryStatus.Removed
            && (entry.Status != EntryStatus.Draft || entry.CreatedByUserId == userId)
            && (seesAll || !db.CapitalMovementDetails.Any(details => details.EntryId == entry.Id && !chosen.Contains(details.PartnerId))));
    }

    /// <summary>The entries <paramref name="shown"/> keeps of those the caller may see, newest first: only the newest <paramref name="take"/> when it is above 0.</summary>
    private static async Task<List<EntryRowView>> RowsAsync(Guid projectId, Guid userId, RedixDbContext db, Expression<Func<Entry, bool>> shown, int take)
    {
        var newestFirst = (await SeenByAsync(projectId, userId, db)).Where(shown).OrderByDescending(entry => entry.CreatedAt).ThenByDescending(entry => entry.Id);
        var entries = await (take > 0 ? newestFirst.Take(take) : newestFirst).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var ids = entries.Select(entry => entry.Id).ToList();
        var rejections = (await db.EntryDecisions.Where(row => ids.Contains(row.EntryId) && row.Action == EntryDecisionAction.Reject).OrderBy(row => row.Id).ToListAsync()).GroupBy(row => row.EntryId).ToDictionary(group => group.Key, group => group.Last().Reason);
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
            facts.PartnerNameOf(entry),
            entry.Description,
            facts.AccountNameOf(entry),
            facts.PaymentMethodNameOf(entry),
            facts.AccountShortByOf(entry))).ToList();
    }

    /// <summary>
    /// One entry as the lists show it (findings row 201): where it stands and what the caller may do with it, for a screen that opened it, whichever list it came from or whether
    /// that list still holds it. One the caller does not see (BR-31, BR-42), or none, answers 404.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Row(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var rows = await RowsAsync(projectId, AccessTokens.UserIdOf(principal), db, entry => entry.Id == entryId, 0);
        return rows.Count == 1 ? Results.Ok(rows[0]) : Results.NotFound();
    }

    /// <summary>One entry of the project, with what its kind has. What it does not have comes empty, with an empty id. A partner's money the caller may not see (BR-31), or anything else, answers 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Read(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var entry = await (await SeenByAsync(projectId, AccessTokens.UserIdOf(principal), db)).SingleOrDefaultAsync(row => row.Id == entryId) ?? Entry.None;
        return entry == Entry.None ? Results.NotFound() : Results.Ok(await ViewOfAsync(entry, db));
    }

    /// <summary>
    /// Sends a draft or a rejected entry for approval, and answers it (UC-028). Only its author sends it, unless it is a rejected entry and the caller holds <see cref="Permission.ManageAnyEntry"/>: otherwise <c>notYourEntry</c> (BR-42). Anything already sent answers <c>entryNotSubmittable</c>.
    /// What it still lacks answers <c>entryIncomplete</c> with every key under <c>missing</c>: each required column left empty (FR-41). Telling Finance is UC-036 "Send push notifications".
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Submit(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
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
        db.AddHistory(entry, HistoryAction.Submit, userId, time.GetUtcNow().UtcDateTime);
        db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Submit, UserId = userId, At = time.GetUtcNow().UtcDateTime });
        await NotificationsEndpoints.TellDecidersAsync(entry, userId, time.GetUtcNow().UtcDateTime, db);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(entry, db));
    }

    /// <summary>
    /// Deletes an entry. One never approved goes, with all it holds (UC-033), for whoever holds <see cref="Permission.DeleteEntries"/>, with the refusals of <see cref="EntryChanges.RefusalOf"/>.
    /// An approved one is removed softly for whoever holds <see cref="Permission.RemoveApprovedEntries"/> (redix-api #119), as <see cref="EntryChanges.RemoveAsync"/> does. Without the
    /// permission the entry asks for, the answer is 403, as a route's own permission answers.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await Entry.OfProjectAsync(projectId, entryId, db);
        if (entry == Entry.None)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var at = time.GetUtcNow().UtcDateTime;
        if (entry.Status == EntryStatus.Approved)
        {
            if (!held.Contains(Permission.RemoveApprovedEntries))
            {
                return Results.Forbid();
            }
            await EntryChanges.RemoveAsync(entry, userId, at, db);
            return Results.Ok();
        }
        if (!held.Contains(Permission.DeleteEntries))
        {
            return Results.Forbid();
        }
        var (status, refusal) = EntryChanges.RefusalOf(entry, userId, held);
        if (refusal.Length > 0)
        {
            return Refusal.Of(status, refusal);
        }
        await EntryChanges.DeleteAsync(entry, userId, at, db);
        return Results.Ok();
    }

    /// <summary>
    /// Brings back an approved entry removed softly, the undo of its removal (redix-api #119, Muteb 12 Nov 2026): it is approved again with all it held, so it is back in every
    /// list and figure, and the History table keeps who brought it back and when. One that is not removed, or not this project's, answers 404.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Restore(Guid projectId, Guid entryId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId && row.Status == EntryStatus.Removed);
        if (entry is null)
        {
            return Results.NotFound();
        }
        await EntryChanges.RestoreAsync(entry, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db);
        return Results.Ok();
    }

    /// <summary>Changes an entry's description, «الوصف», which is allowed even once it is approved (AC-50.2, BR-11), for who entered it or whoever holds <see cref="Permission.ManageAnyEntry"/>; anyone else gets <c>notYourEntry</c>, and a description longer than <see cref="Entry.DescriptionMaxLength"/> <c>invalidDescription</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">The description.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> ChangeDescription(Guid projectId, Guid entryId, ChangeDescriptionRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
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
        var description = request.Description.Trim();
        if (description.Length > Entry.DescriptionMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidDescription");
        }
        entry.Description = description;
        db.AddHistory(entry, HistoryAction.Update, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<List<string>> MissingOfAsync(Entry entry, RedixDbContext db)
    {
        var itemId = await db.ExpenseDetails.Where(row => row.EntryId == entry.Id).Select(row => row.ItemId)
            .Concat(db.Set<RevenueDetails>().Where(row => row.EntryId == entry.Id).Select(row => row.ItemId))
            .Concat(from movement in db.CapitalMovementDetails join partner in db.Partners on movement.PartnerId equals partner.Id where movement.EntryId == entry.Id select partner.ItemId)
            .FirstOrDefaultAsync();
        var columns = (await db.Set<ItemColumn>().Where(column => column.ItemId == itemId && !column.IsHidden).OrderBy(column => column.SortOrder).ToListAsync()).Where(column => column.IsAskedOf(entry.Kind));
        return new EntryAnswers(entry.Description, entry.ColumnValuesByKey()).MissingOf(columns);
    }

    private static async Task<EntryView> ViewOfAsync(Entry entry, RedixDbContext db)
    {
        var expense = await db.ExpenseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new ExpenseDetails();
        var payment = await db.ContractorPaymentDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new ContractorPaymentDetails();
        var partnerId = await db.CapitalMovementDetails.Where(row => row.EntryId == entry.Id).Select(row => row.PartnerId).SingleOrDefaultAsync();
        var revenueTableId = await db.Set<RevenueDetails>().Where(row => row.EntryId == entry.Id).Select(row => row.SubgroupId).SingleOrDefaultAsync();
        return new(
            entry,
            new[] { payment.ContractId, partnerId, revenueTableId, expense.SubgroupId }.FirstOrDefault(id => id != Guid.Empty),
            expense,
            await db.PurchaseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new PurchaseDetails(),
            payment,
            await db.Invoices.SingleOrDefaultAsync(row => row.EntryId == entry.Id) ?? new Invoice(),
            await db.EntryAttachments.Where(row => row.EntryId == entry.Id).ToListAsync());
    }
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
/// <param name="PartnerName">A capital movement's partner; empty for any other entry.</param>
/// <param name="Description">«الوصف», what it says; empty for nothing.</param>
/// <param name="AccountName">The account it was paid from or reached.</param>
/// <param name="PaymentMethodName">How an expense was paid; empty when nobody said, and for any other entry.</param>
/// <param name="AccountShortBy">How much more what waits on its account takes than the account holds, which the shared entry notes without amounts (R6, R16); zero when it covers all of it.</param>
public record EntryRowView(Guid Id, EntryKind Kind, EntryStatus Status, string RejectionReason, DateOnly EntryDate, decimal Amount, string ItemName, string SupplierName, string InvoiceNumber, string AuthorName, bool IsMine, string PartnerName, string Description, string AccountName, string PaymentMethodName, decimal AccountShortBy);

/// <summary>An entry's description.</summary>
/// <param name="Description">The description; may be empty.</param>
public record ChangeDescriptionRequest(string Description);

/// <summary>An entry and everything its kind has. What it does not have is empty, with an empty <c>entryId</c>, so a reader asks nothing but that.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="TableId">The table it is in: its contract, its partner, or its item's group (redix-api #110).</param>
/// <param name="Expense">What every expense has.</param>
/// <param name="Purchase">A purchase's supplier; empty, with an empty id, when it names none.</param>
/// <param name="ContractorPayment">What only a contractor payment has.</param>
/// <param name="Invoice">The bill number it was entered with before REDIX stopped asking for one; empty on a newer entry (redix-api #96, D2).</param>
/// <param name="Attachments">The files it carries.</param>
public record EntryView(Entry Entry, Guid TableId, ExpenseDetails Expense, PurchaseDetails Purchase, ContractorPaymentDetails ContractorPayment, Invoice Invoice, List<EntryAttachment> Attachments);
