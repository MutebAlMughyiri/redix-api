using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Revenue;

/// <summary>Money that comes in other than capital (UC-047, FR-28) and sales invoices (UC-048, FR-42). Finance enters these, and they count at once (BR-30).</summary>
public static class RevenueEndpoints
{
    /// <summary>Maps the routes that read, for anyone who reads the project, and those that enter, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapRevenue(this WebApplication app)
    {
        app.MapProjectChanges(Permission.MoveMoney).MapPost("/revenue", Record);
    }

    /// <summary>
    /// Records revenue on one of the project's revenue items, approved at once; the item's type says what it is (FR-28, FR-38). Anything but an active revenue item of the project, or a
    /// subgroup not its own, answers <c>unknownItem</c>. It is for its own day unless a period is given, a VAT refund included (D23), and a period ending before it starts answers <c>invalidPeriod</c>.
    /// Otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The revenue.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, RevenueRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var item = await ProjectStructure.OfferedItemAsync(projectId, request.ItemId, ItemType.OtherRevenue, db) ?? Item.None;
        var subgroupId = SubgroupOf(item, request.SubgroupId);
        if (subgroupId == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownItem");
        }
        var start = request.PeriodStart == default ? request.Date : request.PeriodStart;
        var end = request.PeriodEnd == default ? start : request.PeriodEnd;
        if (end < start)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPeriod");
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.AccountId], request.AttachmentId, time, db);
        if (refusal.Length > 0)
        {
            return DirectEntries.AnswerOf(refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var entry = DirectEntries.Pending(projectId, EntryKind.Revenue, request.Date, request.Amount, request.AccountId, request.Description.Trim(), userId, at);
        await DirectEntries.AddAsync(entry, DetailsOf(entry.Id, item.Id, subgroupId, start, end, request.AttachmentId), userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entry.Id, entry.Status, ""));
    }

    /// <summary>The subgroup <paramref name="subgroupId"/> of <paramref name="item"/>, or its general one when none is named; empty when the item has no such subgroup.</summary>
    private static Guid SubgroupOf(Item item, Guid subgroupId) =>
        item.Subgroups.Where(subgroup => subgroupId == Guid.Empty ? subgroup.Name == Subgroup.GeneralName : subgroup.Id == subgroupId).Select(subgroup => subgroup.Id).FirstOrDefault();

    private static List<object> DetailsOf(Guid entryId, Guid itemId, Guid subgroupId, DateOnly start, DateOnly end, Guid attachmentId)
    {
        List<object> details = [new RevenueDetails { EntryId = entryId, ItemId = itemId, SubgroupId = subgroupId, PeriodStart = start, PeriodEnd = end }];
        if (attachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entryId, AttachmentId = attachmentId, Role = EntryAttachmentRole.Other });
        }
        return details;
    }

}

/// <summary>Revenue to record.</summary>
/// <param name="ItemId">The revenue item it came in on, whose type says what it is.</param>
/// <param name="Date">The day it came in.</param>
/// <param name="Amount">How much, above zero.</param>
/// <param name="AccountId">The account it reached.</param>
/// <param name="Description">What was said about it; may be empty.</param>
public record RevenueRequest(Guid ItemId, DateOnly Date, decimal Amount, Guid AccountId, string Description)
{
    /// <summary>The item's subgroup; left out, its general one.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The first day of its period; left out, its own day.</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>The last day of its period; left out, its first.</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>A file showing it; left out when none.</summary>
    public Guid AttachmentId { get; init; }
}
