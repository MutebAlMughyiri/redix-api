using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>Partners' capital (UC-042, UC-043, UC-044, FR-25, FR-26, FR-27): what each put in and took out, and each one's share of it.</summary>
public static class CapitalEndpoints
{
    private const int SharePercentDecimals = 10;

    /// <summary>Maps the routes that read capital, for anyone who reads the project; a movement is entered as every entry is (redix-api #110).</summary>
    /// <param name="app">The app.</param>
    public static void MapCapital(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/capital", Capital).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/partners/{partnerId:guid}/movements", Movements).RequireAuthorization(nameof(Permission.ReadProject));
    }

    /// <summary>The project's total capital, and each partner's balance and share, computed unrounded and given with ten decimals (BR-05, BR-28). Everyone who reads the project sees the total; each line shows to whoever sees that partner (BR-31, <see cref="PartnerSight"/>).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<CapitalView> Capital(Guid projectId, ClaimsPrincipal principal, RedixDbContext db) =>
        await ViewOfAsync(projectId, await PartnerSight.OfAsync(principal, projectId, db), db);

    /// <summary>The project's capital with the lines <paramref name="sight"/> sees: the one owner of balances and shares (BR-05, BR-28).</summary>
    public static async Task<CapitalView> ViewOfAsync(Guid projectId, PartnerSight sight, RedixDbContext db)
    {
        var balances = await BalancesAsync(projectId, db);
        var total = balances.Values.Sum();
        var partners = (await db.Partners.Where(Partner.Kept).Where(partner => partner.ProjectId == projectId).ToListAsync()).Where(sight.Sees);
        var lines = partners.OrderByDescending(partner => balances.GetValueOrDefault(partner.Id)).ThenBy(partner => partner.Id).Select(partner =>
        {
            var balance = balances.GetValueOrDefault(partner.Id);
            return new PartnerCapitalView(partner.Id, partner.Name, balance, total == 0 ? 0 : decimal.Round(balance / total * 100, SharePercentDecimals, MidpointRounding.AwayFromZero));
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
        var sight = await PartnerSight.OfAsync(principal, projectId, db);
        if (!sight.Sees(partnerId) || !await db.Partners.AnyAsync(partner => partner.Id == partnerId && partner.ProjectId == projectId))
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
        var columns = await db.ItemColumns.Where(column => !column.IsHidden && db.Partners.Any(partner => partner.Id == partnerId && partner.ItemId == column.ItemId)).ToListAsync();
        return rows.Select(row => new CapitalMovementView(row.entry.Id, Column.SignedChoiceOf(columns, row.entry.ColumnValuesByKey()).Name, row.entry.EntryDate, row.entry.Kind.Signed(row.entry.Amount), row.entry.Notes, facts.AccountNameOf(row.entry), facts.AttachmentIdOf(row.entry), facts.ContentTypeOf(row.entry)) { FileName = facts.FileNameOf(row.entry) }).ToList();
    }

    /// <summary>Each partner's capital in the project, from its approved movements: the one owner of a partner's balance (BR-05), which deleting a partner asks too (redix-api #100).</summary>
    public static async Task<Dictionary<Guid, decimal>> BalancesAsync(Guid projectId, RedixDbContext db)
    {
        var rows = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved
            group entry by new { details.PartnerId, entry.Kind } into moved
            select new { moved.Key.PartnerId, moved.Key.Kind, Total = moved.Sum(entry => entry.Amount) }).ToListAsync();
        return rows.GroupBy(row => row.PartnerId).ToDictionary(group => group.Key, group => group.Sum(row => row.Kind.Signed(row.Total)));
    }
}

/// <summary>A project's capital.</summary>
/// <param name="Total">All partners' capital (BR-05).</param>
/// <param name="Partners">The lines the caller may see.</param>
public record CapitalView(decimal Total, List<PartnerCapitalView> Partners);

/// <summary>One partner's capital.</summary>
/// <param name="Id">The partner.</param>
/// <param name="Name">Their name.</param>
/// <param name="Balance">Their capital.</param>
/// <param name="SharePercent">Their share of the total, as a percentage with ten decimals (BR-28).</param>
public record PartnerCapitalView(Guid Id, string Name, decimal Balance, decimal SharePercent);

/// <summary>One movement of a partner's capital.</summary>
/// <param name="Id">The entry.</param>
/// <param name="MovementName">What it is: the name of the choice of «نوع الحركة» it took (redix-api #129); empty when it took none.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">What it did to the capital: negative when it took some away.</param>
/// <param name="Description">What was said about it.</param>
/// <param name="AccountName">The account it moved through.</param>
/// <param name="AttachmentId">Its receipt; empty when none.</param>
/// <param name="ContentType">What the receipt is; empty when none.</param>
public record CapitalMovementView(Guid Id, string MovementName, DateOnly Date, decimal Amount, string Description, string AccountName, string AttachmentId, string ContentType)
{
    /// <summary>The receipt's name, so the phone hands it over by that name (findings row 158); empty when none, and in a report issued before 8 Nov 2026.</summary>
    public string FileName { get; init; } = "";
}
