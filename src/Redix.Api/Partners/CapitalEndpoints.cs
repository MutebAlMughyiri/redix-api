using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>Partners' capital (UC-042, UC-043, UC-044, FR-25, FR-26, FR-27): what each put in and took out, and each one's share of it.</summary>
public static class CapitalEndpoints
{
    private const int SharePercentDecimals = 10;

    /// <summary>Maps the routes that read capital, for anyone who reads the project, and those that enter a movement, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapCapital(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/capital", Capital).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/partners/{partnerId:guid}/movements", Movements).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.MoveMoney);
        changes.MapPost("/capital-movements", Record);
    }

    /// <summary>The project's total capital, and each partner's balance and share, computed unrounded and given with ten decimals (BR-05, BR-28). Everyone who reads the project sees the total and their own line; whoever holds <see cref="Permission.SeeAllCapital"/> sees every line (BR-31).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<CapitalView> Capital(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesAll = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.SeeAllCapital);
        return await ViewOfAsync(projectId, userId, seesAll, db);
    }

    /// <summary>The project's capital with every line when <paramref name="seesAll"/>, otherwise only <paramref name="userId"/>'s: the one owner of balances and shares (BR-05, BR-28).</summary>
    public static async Task<CapitalView> ViewOfAsync(Guid projectId, Guid userId, bool seesAll, RedixDbContext db)
    {
        var balances = await BalancesAsync(projectId, db);
        var total = balances.Values.Sum();
        var partners = await db.Partners.Where(partner => partner.ProjectId == projectId && (seesAll || partner.UserId == userId)).ToListAsync();
        var lines = partners.OrderByDescending(partner => balances.GetValueOrDefault(partner.Id)).ThenBy(partner => partner.Id).Select(partner =>
        {
            var balance = balances.GetValueOrDefault(partner.Id);
            return new PartnerCapitalView(partner.Id, partner.Name, partner.UserId == userId, balance, total == 0 ? 0 : decimal.Round(balance / total * 100, SharePercentDecimals, MidpointRounding.AwayFromZero));
        }).ToList();
        return new CapitalView(total, lines);
    }

    /// <summary>One partner's movements, newest first, for whoever may see their capital (BR-31); anyone else gets 404, so a partner's existence does not show.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Movements(Guid projectId, Guid partnerId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesAll = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.SeeAllCapital);
        if (!await db.Partners.AnyAsync(partner => partner.Id == partnerId && partner.ProjectId == projectId && (seesAll || partner.UserId == userId)))
        {
            return Results.NotFound();
        }
        return Results.Ok(await MovementsOfAsync(projectId, partnerId, db));
    }

    /// <summary>One partner's movements of capital, newest first, as the movements screen shows them.</summary>
    public static async Task<List<CapitalMovementView>> MovementsOfAsync(Guid projectId, Guid partnerId, RedixDbContext db)
    {
        var rows = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            where details.PartnerId == partnerId && entry.Status == EntryStatus.Approved
            orderby entry.EntryDate descending, entry.Id descending
            select new { entry, details }).ToListAsync();
        var entries = rows.Select(row => row.entry).ToList();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var ids = entries.Select(entry => entry.Id).ToList();
        var reversed = (await db.EntryLinks.Where(link => link.Type == EntryLinkType.Reversal && (ids.Contains(link.EntryId) || ids.Contains(link.LinkedEntryId))).ToListAsync()).SelectMany(link => new[] { link.EntryId, link.LinkedEntryId }).ToHashSet();
        return rows.Select(row => new CapitalMovementView(row.entry.Id, row.details.Type, row.entry.EntryDate, row.entry.Kind == EntryKind.CapitalIn ? row.entry.Amount : -row.entry.Amount, row.entry.Notes, facts.AccountNameOf(row.entry), facts.AttachmentIdOf(row.entry), facts.ContentTypeOf(row.entry), reversed.Contains(row.entry.Id)) { FileName = facts.FileNameOf(row.entry) }).ToList();
    }

    /// <summary>
    /// Enters a partner's contribution or withdrawal, which waits for approval like every entry (BR-30, redix-api #101), with its receipt when there is one: a file is optional, as
    /// for every entry (Muteb, 8 Nov 2026). A partner not in the project answers <c>unknownPartner</c>. Otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The movement.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, CapitalMovementRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var partnerRefusal = await RefusalOfPartnerAsync(projectId, request.PartnerId, db);
        if (partnerRefusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, partnerRefusal);
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, request.AccountId, request.AttachmentId, time, db);
        if (refusal.Length > 0)
        {
            return DirectEntries.AnswerOf(refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var kind = CapitalMovementDetails.IsInflow(request.Type) ? EntryKind.CapitalIn : EntryKind.CapitalOut;
        var entry = DirectEntries.Pending(projectId, kind, request.Date, request.Amount, request.AccountId, request.Description.Trim(), userId, at);
        List<object> details = [new CapitalMovementDetails { EntryId = entry.Id, PartnerId = request.PartnerId, Type = request.Type }];
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entry.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.BankReceipt });
        }
        await DirectEntries.AddAsync(entry, details, userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entry.Id, entry.Status, ""));
    }

    /// <summary>
    /// Why <paramref name="partnerId"/> cannot move money in <paramref name="projectId"/>: <c>unknownPartner</c> when not a partner of it, or when their table's capital item is not offered
    /// (disabled, BR-15, or none yet), as an expense's item must be (redix-api #109); empty when they can.
    /// </summary>
    public static async Task<string> RefusalOfPartnerAsync(Guid projectId, Guid partnerId, RedixDbContext db)
    {
        var itemId = await db.Partners.Where(row => row.Id == partnerId && row.ProjectId == projectId).Select(row => row.ItemId).FirstOrDefaultAsync();
        return await ProjectStructure.OfferedItemAsync(projectId, itemId, ItemType.Capital, db) == Item.None ? "UnknownPartner" : "";
    }

    private static async Task<Dictionary<Guid, decimal>> BalancesAsync(Guid projectId, RedixDbContext db)
    {
        var rows = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved
            group entry by new { details.PartnerId, entry.Kind } into moved
            select new { moved.Key.PartnerId, moved.Key.Kind, Total = moved.Sum(entry => entry.Amount) }).ToListAsync();
        return rows.GroupBy(row => row.PartnerId).ToDictionary(group => group.Key, group => group.Sum(row => row.Kind == EntryKind.CapitalIn ? row.Total : -row.Total));
    }
}

/// <summary>A capital movement to enter.</summary>
/// <param name="PartnerId">The partner.</param>
/// <param name="Type">What it is.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">How much, above zero.</param>
/// <param name="AccountId">The account it moved through.</param>
/// <param name="Description">What was said about it; may be empty.</param>
public record CapitalMovementRequest(Guid PartnerId, CapitalMovementType Type, DateOnly Date, decimal Amount, Guid AccountId, string Description)
{
    /// <summary>The receipt; left out when there is none.</summary>
    public Guid AttachmentId { get; init; }
}

/// <summary>A project's capital.</summary>
/// <param name="Total">All partners' capital (BR-05).</param>
/// <param name="Partners">The lines the caller may see.</param>
public record CapitalView(decimal Total, List<PartnerCapitalView> Partners);

/// <summary>One partner's capital.</summary>
/// <param name="Id">The partner.</param>
/// <param name="Name">Their name.</param>
/// <param name="IsMine">True when this is the caller's own.</param>
/// <param name="Balance">Their capital.</param>
/// <param name="SharePercent">Their share of the total, as a percentage with ten decimals (BR-28).</param>
public record PartnerCapitalView(Guid Id, string Name, bool IsMine, decimal Balance, decimal SharePercent);

/// <summary>One movement of a partner's capital.</summary>
/// <param name="Id">The entry.</param>
/// <param name="Type">What it is.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">What it did to the capital: negative when it took some away.</param>
/// <param name="Description">What was said about it.</param>
/// <param name="AccountName">The account it moved through.</param>
/// <param name="AttachmentId">Its receipt; empty when none.</param>
/// <param name="ContentType">What the receipt is; empty when none.</param>
/// <param name="IsReversed">True when it was reversed, or is itself a reversal.</param>
public record CapitalMovementView(Guid Id, CapitalMovementType Type, DateOnly Date, decimal Amount, string Description, string AccountName, string AttachmentId, string ContentType, bool IsReversed)
{
    /// <summary>The receipt's name, so the phone hands it over by that name (findings row 158); empty when none, and in a report issued before 8 Nov 2026.</summary>
    public string FileName { get; init; } = "";
}
