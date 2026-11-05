using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>Money moved between a project's accounts, or a custody topped up (UC-040, FR-23). The surplus does not change.</summary>
public static class TransfersEndpoints
{
    /// <summary>How many a person sees: the latest.</summary>
    public const int Latest = 50;

    /// <summary>Maps the route that reads them, for anyone who reads the project, and the one that enters one, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapTransfers(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/transfers", List).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapProjectChanges(Permission.MoveMoney).MapPost("/transfers", Transfer);
    }

    /// <summary>The latest transfers of the project, newest first, each once.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<TransferView>> List(Guid projectId, RedixDbContext db)
    {
        var rows = await (
            from leaving in db.Entries
            join link in db.EntryLinks on leaving.Id equals link.EntryId
            join arriving in db.Entries on link.LinkedEntryId equals arriving.Id
            where leaving.ProjectId == projectId && leaving.Kind == EntryKind.TransferOut && link.Type == EntryLinkType.TransferPair
            orderby leaving.CreatedAt descending, leaving.Id descending
            select new { leaving.Id, leaving.EntryDate, leaving.Amount, leaving.Notes, FromId = leaving.AccountId, ToId = arriving.AccountId }).Take(Latest).ToListAsync();
        var names = await db.Accounts.Where(account => account.ProjectId == projectId).ToDictionaryAsync(account => account.Id, account => account.Name);
        return rows.Select(row => new TransferView(row.Id, row.EntryDate, row.Amount, names.GetValueOrDefault(row.FromId, ""), names.GetValueOrDefault(row.ToId, ""), row.Notes)).ToList();
    }

    /// <summary>Moves money between two accounts, approved at once (BR-30): two entries tied together. The same account twice answers <c>sameAccount</c>; otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The accounts, the amount, and what it is for.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Transfer(Guid projectId, TransferRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        if (request.FromAccountId == request.ToAccountId)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "SameAccount");
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.FromAccountId, request.ToAccountId], request.AttachmentId, time, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(refusal == "InvoiceAlreadyAttached" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var description = request.Description.Trim();
        var leaving = DirectEntries.New(projectId, EntryKind.TransferOut, request.Date, request.Amount, request.FromAccountId, description, userId, at);
        var arriving = DirectEntries.New(projectId, EntryKind.TransferIn, request.Date, request.Amount, request.ToAccountId, description, userId, at);
        List<object> details = [new EntryLink { EntryId = leaving.Id, Type = EntryLinkType.TransferPair, LinkedEntryId = arriving.Id }];
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = leaving.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.BankReceipt });
        }
        DirectEntries.Add(leaving, details, userId, at, db);
        DirectEntries.Add(arriving, [new EntryLink { EntryId = arriving.Id, Type = EntryLinkType.TransferPair, LinkedEntryId = leaving.Id }], userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(leaving.Id, leaving.Status, ""));
    }
}

/// <summary>Money to move between two accounts.</summary>
/// <param name="Date">The day it moved.</param>
/// <param name="FromAccountId">Where it left.</param>
/// <param name="ToAccountId">Where it arrived.</param>
/// <param name="Amount">How much.</param>
/// <param name="Description">What it is for; may be empty.</param>
public record TransferRequest(DateOnly Date, Guid FromAccountId, Guid ToAccountId, decimal Amount, string Description)
{
    /// <summary>A file showing it; left out when none.</summary>
    public Guid AttachmentId { get; init; }
}

/// <summary>A transfer as the app shows it.</summary>
/// <param name="Id">The entry that left.</param>
/// <param name="Date">The day it moved.</param>
/// <param name="Amount">How much.</param>
/// <param name="FromAccountName">Where it left.</param>
/// <param name="ToAccountName">Where it arrived.</param>
/// <param name="Description">What it is for.</param>
public record TransferView(Guid Id, DateOnly Date, decimal Amount, string FromAccountName, string ToAccountName, string Description);
