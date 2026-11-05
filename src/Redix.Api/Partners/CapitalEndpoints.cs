using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;

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
        changes.MapPost("/capital-movements/carry", Carry);
    }

    /// <summary>The project's total capital, and each partner's balance and share, computed unrounded and given with ten decimals (BR-05, BR-28). Everyone who reads the project sees the total and their own line; whoever holds <see cref="Permission.SeeAllCapital"/> sees every line (BR-31).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<CapitalView> Capital(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesAll = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.SeeAllCapital);
        var balances = await BalancesAsync(projectId, db);
        var total = balances.Values.Sum();
        var partners = await db.Partners.Where(partner => partner.ProjectId == projectId && (seesAll || partner.UserId == userId)).ToListAsync();
        var lines = partners.OrderByDescending(partner => partner.IsActive).ThenByDescending(partner => balances.GetValueOrDefault(partner.Id)).ThenBy(partner => partner.Id).Select(partner =>
        {
            var balance = balances.GetValueOrDefault(partner.Id);
            return new PartnerCapitalView(partner.Id, partner.Name, partner.IsActive, partner.UserId == userId, balance, total == 0 ? 0 : decimal.Round(balance / total * 100, SharePercentDecimals, MidpointRounding.AwayFromZero));
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
        return Results.Ok(rows.Select(row => new CapitalMovementView(row.entry.Id, row.details.Type, row.entry.EntryDate, row.entry.Kind == EntryKind.CapitalIn ? row.entry.Amount : -row.entry.Amount, row.details.Description, row.details.CashReason, facts.AccountNameOf(row.entry), facts.AttachmentIdOf(row.entry), facts.ContentTypeOf(row.entry), reversed.Contains(row.entry.Id))).ToList());
    }

    /// <summary>
    /// Enters a capital movement, approved at once (BR-30). It needs its bank receipt, unless paid in cash with a reason (BR-23), otherwise <c>receiptRequired</c>; a reason too long answers <c>invalidReason</c>, and something in kind with no reason <c>receiptRequired</c>. A carry here answers <c>useCarry</c>, since that has its own route.
    /// A settlement must take all the partner holds, otherwise <c>settlementNotFull</c>, and then they are no longer active. A partner not in the project answers <c>unknownPartner</c>; one who left <c>partnerLeft</c>. Otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The movement.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, CapitalMovementRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        if (request.Type is CapitalMovementType.CarryIn or CapitalMovementType.CarryOut)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UseCarry");
        }
        var partnerRefusal = await RefusalOfPartnerAsync(projectId, request.PartnerId, db);
        if (partnerRefusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, partnerRefusal);
        }
        var reason = request.CashReason.Trim();
        if (reason.Length > CapitalMovementDetails.ReasonMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidReason");
        }
        var needsReason = request.Type == CapitalMovementType.InKind;
        if (needsReason ? reason.Length == 0 : request.AttachmentId == Guid.Empty && reason.Length == 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ReceiptRequired");
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.AccountId], request.AttachmentId, time, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(refusal == "InvoiceAlreadyAttached" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, refusal);
        }
        if (request.Type == CapitalMovementType.SettlementAndExit && (await BalancesAsync(projectId, db)).GetValueOrDefault(request.PartnerId) != request.Amount)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "SettlementNotFull");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var kind = CapitalMovementDetails.IsInflow(request.Type) ? EntryKind.CapitalIn : EntryKind.CapitalOut;
        var entry = DirectEntries.New(projectId, kind, request.Date, request.Amount, request.AccountId, request.Description.Trim(), userId, at);
        List<object> details = [new CapitalMovementDetails { EntryId = entry.Id, PartnerId = request.PartnerId, Type = request.Type, Description = request.Description.Trim(), CashReason = reason }];
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entry.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.BankReceipt });
        }
        DirectEntries.Add(entry, details, userId, at, db);
        if (request.Type == CapitalMovementType.SettlementAndExit)
        {
            await SetActiveAsync(request.PartnerId, false, db);
        }
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entry.Id, entry.Status, ""));
    }

    /// <summary>
    /// Moves a partner's balance from this project to another in one action (FR-26): a movement out here and a movement in there, tied together, with no receipt (BR-23). Projects are otherwise separate (BR-33), so the caller must hold <see cref="Permission.MoveMoney"/> in both, otherwise <c>targetProjectRefused</c>, and it must be running, otherwise <c>targetProjectFinished</c>. The same project answers <c>sameProject</c>.
    /// A partner not in their project answers <c>unknownPartner</c>; one who left <c>partnerLeft</c>; otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does, on each side.
    /// </summary>
    /// <param name="projectId">The project it leaves.</param>
    /// <param name="request">The partner in each project, the account in each, and the amount.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Carry(Guid projectId, CarryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        if (request.TargetProjectId == projectId)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "SameProject");
        }
        var userId = AccessTokens.UserIdOf(principal);
        if (!(await UserAccess.OfAsync(userId, db)).In(request.TargetProjectId).Contains(Permission.MoveMoney))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "TargetProjectRefused");
        }
        if (!await db.Projects.AnyAsync(project => project.Id == request.TargetProjectId && project.Status == ProjectStatus.Running))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "TargetProjectFinished");
        }
        var partnerRefusal = await RefusalOfPartnerAsync(projectId, request.PartnerId, db);
        partnerRefusal = partnerRefusal.Length > 0 ? partnerRefusal : await RefusalOfPartnerAsync(request.TargetProjectId, request.TargetPartnerId, db);
        var refusal = partnerRefusal.Length > 0 ? partnerRefusal : await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.AccountId], Guid.Empty, time, db);
        refusal = refusal.Length > 0 ? refusal : await DirectEntries.RefusalOfAsync(request.TargetProjectId, request.Date, request.Amount, request.Description, [request.TargetAccountId], Guid.Empty, time, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var at = time.GetUtcNow().UtcDateTime;
        var description = request.Description.Trim();
        var leaving = DirectEntries.New(projectId, EntryKind.CapitalOut, request.Date, request.Amount, request.AccountId, description, userId, at);
        var arriving = DirectEntries.New(request.TargetProjectId, EntryKind.CapitalIn, request.Date, request.Amount, request.TargetAccountId, description, userId, at);
        DirectEntries.Add(leaving, [new CapitalMovementDetails { EntryId = leaving.Id, PartnerId = request.PartnerId, Type = CapitalMovementType.CarryOut, Description = description }, new EntryLink { EntryId = leaving.Id, Type = EntryLinkType.CarryPair, LinkedEntryId = arriving.Id }], userId, at, db);
        DirectEntries.Add(arriving, [new CapitalMovementDetails { EntryId = arriving.Id, PartnerId = request.TargetPartnerId, Type = CapitalMovementType.CarryIn, Description = description }, new EntryLink { EntryId = arriving.Id, Type = EntryLinkType.CarryPair, LinkedEntryId = leaving.Id }], userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(leaving.Id, leaving.Status, ""));
    }

    /// <summary>Marks a partner as still in or as gone, with their pocket, so it stops being offered; saved with the change.</summary>
    public static async Task SetActiveAsync(Guid partnerId, bool isActive, RedixDbContext db)
    {
        var partner = await db.Partners.SingleAsync(row => row.Id == partnerId);
        partner.IsActive = isActive;
        var pockets = await (from pocket in db.PartnerPockets join account in db.Accounts on pocket.AccountId equals account.Id where pocket.PartnerId == partnerId select account).ToListAsync();
        pockets.ForEach(account => account.IsActive = isActive);
    }

    private static async Task<string> RefusalOfPartnerAsync(Guid projectId, Guid partnerId, RedixDbContext db)
    {
        var partner = await db.Partners.SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
        if (partner is null)
        {
            return "UnknownPartner";
        }
        return partner.IsActive ? "" : "PartnerLeft";
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
    /// <summary>The bank receipt; left out when paid in cash.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>Why there is no receipt; left out when there is one (BR-23).</summary>
    public string CashReason { get; init; } = "";
}

/// <summary>A partner's balance to carry to another project.</summary>
/// <param name="PartnerId">The partner in this project.</param>
/// <param name="AccountId">The account it leaves in this project.</param>
/// <param name="TargetProjectId">The project it goes to.</param>
/// <param name="TargetPartnerId">The partner in that project.</param>
/// <param name="TargetAccountId">The account it reaches in that project.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">How much, above zero.</param>
/// <param name="Description">What was said about it; may be empty.</param>
public record CarryRequest(Guid PartnerId, Guid AccountId, Guid TargetProjectId, Guid TargetPartnerId, Guid TargetAccountId, DateOnly Date, decimal Amount, string Description);

/// <summary>A project's capital.</summary>
/// <param name="Total">All partners' capital (BR-05).</param>
/// <param name="Partners">The lines the caller may see.</param>
public record CapitalView(decimal Total, List<PartnerCapitalView> Partners);

/// <summary>One partner's capital.</summary>
/// <param name="Id">The partner.</param>
/// <param name="Name">Their name.</param>
/// <param name="IsActive">False once they settled and withdrew.</param>
/// <param name="IsMine">True when this is the caller's own.</param>
/// <param name="Balance">Their capital.</param>
/// <param name="SharePercent">Their share of the total, as a percentage with ten decimals (BR-28).</param>
public record PartnerCapitalView(Guid Id, string Name, bool IsActive, bool IsMine, decimal Balance, decimal SharePercent);

/// <summary>One movement of a partner's capital.</summary>
/// <param name="Id">The entry.</param>
/// <param name="Type">What it is.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">What it did to the capital: negative when it took some away.</param>
/// <param name="Description">What was said about it.</param>
/// <param name="CashReason">Why it has no receipt; empty when it has one.</param>
/// <param name="AccountName">The account it moved through.</param>
/// <param name="AttachmentId">Its receipt; empty when none.</param>
/// <param name="ContentType">What the receipt is; empty when none.</param>
/// <param name="IsReversed">True when it was reversed, or is itself a reversal.</param>
public record CapitalMovementView(Guid Id, CapitalMovementType Type, DateOnly Date, decimal Amount, string Description, string CashReason, string AccountName, string AttachmentId, string ContentType, bool IsReversed);
