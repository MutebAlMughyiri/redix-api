using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Revenue;
using Redix.Api.Structure;

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
        app.MapPost("/projects/{projectId:guid}/liquidation/{entryId:guid}/receipt", AttachReceipt).RequireAuthorization(nameof(Permission.MoveMoney)).DisableAntiforgery();
        app.MapPost("/projects/{projectId:guid}/liquidation/late", RecordLate).RequireAuthorization(nameof(Permission.MoveMoney));
    }

    /// <summary>
    /// Attaches the bank receipt of a partner's part, after the transfer that follows the liquidation; the project may already be closed. One that is not a part of the project's liquidation is
    /// not found; one that already has its receipt answers <c>receiptAlreadyAttached</c>; a file that cannot be kept answers as the attachment store does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The partner's part.</param>
    /// <param name="file">The receipt.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="options">The folder files are kept under.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> AttachReceipt(Guid projectId, Guid entryId, IFormFile file, ClaimsPrincipal principal, RedixDbContext db, IOptions<AttachmentOptions> options, TimeProvider time)
    {
        var isPart = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            where entry.Id == entryId && entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && details.Type == CapitalMovementType.Liquidation
            select entry).AnyAsync();
        if (!isPart)
        {
            return Results.NotFound();
        }
        if (await db.EntryAttachments.AnyAsync(row => row.EntryId == entryId && row.Role == EntryAttachmentRole.BankReceipt))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "ReceiptAlreadyAttached");
        }
        var userId = AccessTokens.UserIdOf(principal);
        var (refusal, attachment, _) = await AttachmentsEndpoints.KeepAsync(projectId, file, FileKinds.PicturesAndPdfs, userId, db, options.Value, time);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var receipt = new EntryAttachment { EntryId = entryId, AttachmentId = attachment.Id, Role = EntryAttachmentRole.BankReceipt };
        db.Add(receipt);
        db.Audit(receipt, AuditAction.Attach, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(projectId, db));
    }

    /// <summary>
    /// Records an expense or revenue that came after the project closed (FR-51), approved at once, and shares it among the partners by the shares frozen at closing (BR-27): each pays in their part
    /// of a late expense and takes their part of late revenue, through the same account, so the surplus stays nothing. A project not closed answers <c>projectNotClosed</c>; an expense on an item
    /// that takes no purchases, <c>unknownItem</c>; no partner to share it among, <c>noPartners</c>; otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What came, and for an expense, its item.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> RecordLate(Guid projectId, LateEntryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        if (!project.IsReadOnly)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "ProjectNotClosed");
        }
        var item = await db.Items.Include(row => row.Subgroups)
            .SingleOrDefaultAsync(row => row.Id == request.ItemId && row.Type != ItemType.ContractorPayments && db.Categories.Any(category => category.Id == row.CategoryId && category.ProjectId == projectId)) ?? Item.None;
        if (item.Id == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownItem");
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.AccountId], Guid.Empty, time, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var partners = (await CapitalEndpoints.ViewOfAsync(projectId, Guid.Empty, true, db)).Partners.Where(partner => partner.SharePercent > 0).ToList();
        if (partners.Count == 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "NoPartners");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var description = request.Description.Trim();
        var isExpense = !item.Type.IsRevenue();
        var general = item.Subgroups.Single(subgroup => subgroup.Name == Subgroup.GeneralName).Id;
        if (isExpense)
        {
            var entry = DirectEntries.New(projectId, EntryKind.Purchase, request.Date, request.Amount, request.AccountId, description, userId, at);
            var method = await db.PaymentMethods.Where(row => row.ProjectId == projectId && row.IsActive).OrderBy(row => row.SortOrder).Select(row => row.Id).FirstAsync();
            var supplier = await db.Suppliers.Where(row => row.ProjectId == projectId && row.IsGeneralCash).Select(row => row.Id).FirstAsync();
            DirectEntries.Add(entry, [
                new ExpenseDetails { EntryId = entry.Id, ItemId = item.Id, SubgroupId = general, PaymentMethodId = method },
                new PurchaseDetails { EntryId = entry.Id, SupplierId = supplier, Product = description[..Math.Min(description.Length, PurchaseDetails.ProductMaxLength)] },
            ], userId, at, db);
        }
        else
        {
            var entry = DirectEntries.New(projectId, EntryKind.Revenue, request.Date, request.Amount, request.AccountId, description, userId, at);
            DirectEntries.Add(entry, [new RevenueDetails { EntryId = entry.Id, ItemId = item.Id, SubgroupId = general, PeriodStart = request.Date, PeriodEnd = request.Date }], userId, at, db);
        }
        var kind = isExpense ? EntryKind.CapitalIn : EntryKind.CapitalOut;
        foreach (var (partner, part) in PartsOf(request.Amount, partners).Where(share => share.Part != 0))
        {
            var entry = DirectEntries.New(projectId, kind, request.Date, part, request.AccountId, description, userId, at);
            DirectEntries.Add(entry, [new CapitalMovementDetails { EntryId = entry.Id, PartnerId = partner.Id, Type = CapitalMovementType.Liquidation, Description = description }], userId, at, db);
        }
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(projectId, db));
    }

    /// <summary>[amount] shared among [partners] by their shares to two decimals, the last taking what rounding left so the parts make it exactly (BR-27, BR-28).</summary>
    private static List<(PartnerCapitalView Partner, decimal Part)> PartsOf(decimal amount, List<PartnerCapitalView> partners)
    {
        var parts = new List<(PartnerCapitalView Partner, decimal Part)>();
        foreach (var partner in partners)
        {
            var part = parts.Count == partners.Count - 1 ? amount - parts.Sum(share => share.Part) : decimal.Round(amount * partner.SharePercent / 100, 2, MidpointRounding.AwayFromZero);
            parts.Add((partner, part));
        }
        return parts;
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
        var parts = PartsOf(surplus, partners).Select(share => new LiquidationPartView(share.Partner.Id, share.Partner.Name, share.Partner.SharePercent, share.Part)).ToList();
        var pending = await db.Entries.CountAsync(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval);
        var shared = await (
            from details in db.CapitalMovementDetails
            join entry in db.Entries on details.EntryId equals entry.Id
            join partner in db.Partners on details.PartnerId equals partner.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && details.Type == CapitalMovementType.Liquidation
            orderby entry.EntryDate, entry.Id
            select new { entry, partner.Name }).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, shared.Select(row => row.entry).ToList(), db);
        var distributed = shared.Select(row => new LiquidationShareView(row.entry.Id, row.Name, row.entry.EntryDate, row.entry.Kind == EntryKind.CapitalOut ? row.entry.Amount : -row.entry.Amount, facts.AccountNameOf(row.entry), row.entry.Notes, facts.AttachmentIdOf(row.entry), facts.ContentTypeOf(row.entry))).ToList();
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
/// <param name="Description">What it was for: the liquidation, or a late expense or revenue.</param>
/// <param name="ReceiptId">Its bank receipt; empty until attached.</param>
/// <param name="ReceiptType">What the receipt is; empty until attached.</param>
public record LiquidationShareView(Guid EntryId, string PartnerName, DateOnly Date, decimal Amount, string AccountName, string Description, string ReceiptId, string ReceiptType);

/// <summary>An expense or revenue that came after the project closed.</summary>
/// <param name="ItemId">The item: an expense item that takes purchases, or a revenue item, whose side says which this is.</param>
/// <param name="Date">The day.</param>
/// <param name="Amount">How much, above zero.</param>
/// <param name="AccountId">The account it went through.</param>
/// <param name="Description">What it was.</param>
public record LateEntryRequest(Guid ItemId, DateOnly Date, decimal Amount, Guid AccountId, string Description);
