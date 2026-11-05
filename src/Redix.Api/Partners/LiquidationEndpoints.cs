using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Partners;

/// <summary>
/// Liquidating a project (UC-061, FR-51, BR-27): the final surplus shared among the partners by their shares, each part paid out or carried to a later project, or a deficit paid in, and then the
/// project closed. What was distributed is the report's liquidation; the partners' capital and shares stay as they were.
/// </summary>
public static class LiquidationEndpoints
{
    private const string Description = "تصفية المشروع";

    /// <summary>Maps the routes, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapLiquidation(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/liquidation", Liquidation).RequireAuthorization(nameof(Permission.MoveMoney));
        app.MapProjectChanges(Permission.MoveMoney).MapPost("/liquidation", Liquidate);
    }

    /// <summary>What closing now would share out, and what was shared out when it closed.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<LiquidationView> Liquidation(Guid projectId, RedixDbContext db) => ViewOfAsync(projectId, db);

    /// <summary>
    /// Shares out the surplus as <see cref="ViewOfAsync"/> parts it, each partner's part as their line says, on <see cref="LiquidationRequest.Date"/>, and closes the project (AC-04.1). Entries waiting for
    /// approval answer <c>pendingEntries</c> (BR-35); a surplus with no partner to take it <c>noPartners</c>; lines that do not name each partner with a part exactly once, or name another,
    /// <c>incompleteLiquidation</c>. A part paid out or in answers as <see cref="DirectEntries.RefusalOfAsync"/> does; a carried one as <see cref="CapitalEndpoints.Carry"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The day, and how each partner's part goes.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Liquidate(Guid projectId, LiquidationRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var view = await ViewOfAsync(projectId, db);
        if (view.PendingCount > 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, nameof(ProjectChangeOutcome.PendingEntries));
        }
        var parts = view.Parts.Where(part => part.Amount != 0).ToList();
        if (view.Surplus != 0 && parts.Count == 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "NoPartners");
        }
        var named = request.Lines.Select(line => line.PartnerId).ToHashSet();
        if (named.Count != request.Lines.Count || !named.SetEquals(parts.Select(part => part.PartnerId)))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "IncompleteLiquidation");
        }
        var lines = request.Lines.ToDictionary(line => line.PartnerId);

        var userId = AccessTokens.UserIdOf(principal);
        var kind = view.Surplus > 0 ? EntryKind.CapitalOut : EntryKind.CapitalIn;
        var carries = parts.Select(part => lines[part.PartnerId].AsCarry(part.PartnerId, request.Date, Math.Abs(part.Amount), Description)).ToList();
        foreach (var carry in carries)
        {
            var (status, refusal) = carry.TargetProjectId == Guid.Empty
                ? (StatusCodes.Status400BadRequest, await PaidRefusalAsync(projectId, carry, time, db))
                : await CapitalEndpoints.RefusalOfCarryAsync(projectId, carry, userId, time, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(status, refusal);
            }
        }

        var at = time.GetUtcNow().UtcDateTime;
        foreach (var carry in carries)
        {
            if (carry.TargetProjectId == Guid.Empty)
            {
                var entry = DirectEntries.New(projectId, kind, carry.Date, carry.Amount, carry.AccountId, Description, userId, at);
                DirectEntries.Add(entry, [new CapitalMovementDetails { EntryId = entry.Id, PartnerId = carry.PartnerId, Type = CapitalMovementType.Liquidation, Description = Description }], userId, at, db);
            }
            else
            {
                CapitalEndpoints.AddCarry(projectId, carry, kind, CapitalMovementType.Liquidation, userId, at, db);
            }
        }
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        project.Close(hasEntriesWaiting: false);
        db.Audit(project, AuditAction.CloseProject, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(projectId, db));
    }

    /// <summary>
    /// The project's liquidation: the surplus left to share, each partner's share and part of it to two decimals with the last taking what rounding left so the parts make the surplus exactly
    /// (BR-27, BR-28), how many entries wait for approval, and each part already shared out.
    /// </summary>
    public static async Task<LiquidationView> ViewOfAsync(Guid projectId, RedixDbContext db)
    {
        var surplus = (await ProjectFigures.OfAsync(projectId, db)).Surplus;
        var partners = (await CapitalEndpoints.ViewOfAsync(projectId, Guid.Empty, true, db)).Partners.Where(partner => partner.SharePercent > 0).ToList();
        var parts = new List<LiquidationPartView>();
        foreach (var partner in partners)
        {
            var amount = parts.Count == partners.Count - 1 ? surplus - parts.Sum(part => part.Amount) : decimal.Round(surplus * partner.SharePercent / 100, 2, MidpointRounding.AwayFromZero);
            parts.Add(new LiquidationPartView(partner.Id, partner.Name, partner.SharePercent, amount));
        }
        var pending = await db.Entries.CountAsync(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval);
        var shared = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            join partner in db.Partners on details.PartnerId equals partner.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && details.Type == CapitalMovementType.Liquidation
            orderby entry.EntryDate, entry.Id
            select new { entry, partner.Name }).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, shared.Select(row => row.entry).ToList(), db);
        var distributed = shared.Select(row => new LiquidationShareView(row.entry.Id, row.Name, row.entry.EntryDate, row.entry.Kind == EntryKind.CapitalOut ? row.entry.Amount : -row.entry.Amount, facts.AccountNameOf(row.entry))).ToList();
        return new LiquidationView(surplus, pending, parts, distributed);
    }

    private static async Task<string> PaidRefusalAsync(Guid projectId, CarryRequest paid, TimeProvider time, RedixDbContext db)
    {
        var refusal = await CapitalEndpoints.RefusalOfPartnerAsync(projectId, paid.PartnerId, db);
        return refusal.Length > 0 ? refusal : await DirectEntries.RefusalOfAsync(projectId, paid.Date, paid.Amount, paid.Description, [paid.AccountId], Guid.Empty, time, db);
    }
}

/// <summary>How the surplus is shared out on a day, closing the project.</summary>
/// <param name="Date">The day.</param>
/// <param name="Lines">How each partner's part goes, one line each.</param>
public record LiquidationRequest(DateOnly Date, List<LiquidationLineRequest> Lines);

/// <summary>How one partner's part goes: paid out of, or a deficit paid into, an account here; or carried to their line in a later project when that project is named.</summary>
/// <param name="PartnerId">The partner.</param>
/// <param name="AccountId">The account here it leaves or reaches.</param>
public record LiquidationLineRequest(Guid PartnerId, Guid AccountId)
{
    /// <summary>The later project it is carried to; left out when it is paid.</summary>
    public Guid TargetProjectId { get; init; }

    /// <summary>The partner there.</summary>
    public Guid TargetPartnerId { get; init; }

    /// <summary>The account there.</summary>
    public Guid TargetAccountId { get; init; }

    /// <summary>This line as a carry of <paramref name="amount"/> on <paramref name="date"/>; a paid line names no project.</summary>
    public CarryRequest AsCarry(Guid partnerId, DateOnly date, decimal amount, string description) =>
        new(partnerId, AccountId, TargetProjectId, TargetPartnerId, TargetAccountId, date, amount, description);
}

/// <summary>A project's liquidation.</summary>
/// <param name="Surplus">What is left to share out: the surplus, or a deficit when negative; nothing once shared.</param>
/// <param name="PendingCount">Entries waiting for approval, which hold the close (BR-35).</param>
/// <param name="Parts">Each partner's part of it.</param>
/// <param name="Distributed">Each part already shared out.</param>
public record LiquidationView(decimal Surplus, int PendingCount, List<LiquidationPartView> Parts, List<LiquidationShareView> Distributed);

/// <summary>A partner's part of the surplus.</summary>
/// <param name="PartnerId">The partner.</param>
/// <param name="Name">Their name.</param>
/// <param name="SharePercent">Their share, with ten decimals (BR-28).</param>
/// <param name="Amount">Their part; negative for their part of a deficit.</param>
public record LiquidationPartView(Guid PartnerId, string Name, decimal SharePercent, decimal Amount);

/// <summary>A part shared out.</summary>
/// <param name="EntryId">The entry.</param>
/// <param name="PartnerName">The partner.</param>
/// <param name="Date">The day.</param>
/// <param name="Amount">What they took; negative for what they paid in.</param>
/// <param name="AccountName">The account it went through.</param>
public record LiquidationShareView(Guid EntryId, string PartnerName, DateOnly Date, decimal Amount, string AccountName);
