using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;
using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>Moving money between a project's accounts (FR-23, redix-app #89): whoever moves money transfers, and it counts at once in both balances (D1, D2).</summary>
public static class TransfersEndpoints
{
    /// <summary>Maps the route that transfers, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    public static void MapTransfers(this WebApplication app) => app.MapProjectChanges(Permission.MoveMoney).MapPost("/transfers", Transfer);

    /// <summary>
    /// Moves <see cref="TransferRequest.Amount"/> from one account of the project to another, in the History table. An account not in the project answers <c>unknownAccount</c>;
    /// then, as <see cref="Accounts.Transfer.RefusalOf"/> says, the same account twice <c>sameAccount</c>, a disabled one <c>accountDisabled</c>, no amount or one beyond the limit
    /// <c>invalidAmount</c>, notes too long <c>invalidNotes</c>, and more than the first account holds <c>insufficientBalance</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The two accounts, the amount, its day and notes.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Transfer(Guid projectId, TransferRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId && (account.Id == request.FromAccountId || account.Id == request.ToAccountId)).ToListAsync();
        var from = accounts.SingleOrDefault(account => account.Id == request.FromAccountId);
        var to = accounts.SingleOrDefault(account => account.Id == request.ToAccountId);
        if (from is null || to is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAccount");
        }

        var notes = request.Notes.Trim();
        var refusal = Accounts.Transfer.RefusalOf(from, to, request.Amount, notes, await AccountMoney.BalanceOfAsync(projectId, from.Id, db));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var transfer = new Transfer
        {
            ProjectId = projectId,
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            Amount = request.Amount,
            Date = request.Date,
            Notes = notes,
            CreatedByUserId = userId,
            CreatedAt = at,
        };
        db.Transfers.Add(transfer);
        db.AddHistory(transfer, HistoryAction.Create, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to move money between two accounts.</summary>
/// <param name="FromAccountId">The account the money leaves.</param>
/// <param name="ToAccountId">The account the money reaches.</param>
/// <param name="Amount">How much moves.</param>
/// <param name="Date">The day it moves.</param>
/// <param name="Notes">Notes on it; empty when none.</param>
public record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount, DateOnly Date, string Notes);
