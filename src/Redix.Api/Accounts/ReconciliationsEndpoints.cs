using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>Checking the money in the accounts against the books (UC-041, FR-24, BR-08).</summary>
public static class ReconciliationsEndpoints
{
    /// <summary>Maps the route that reads the latest, for anyone who reads the project, and the one that enters one, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapReconciliations(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/reconciliations/latest", Latest).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapProjectChanges(Permission.MoveMoney).MapPost("/reconciliations", Enter);
    }

    /// <summary>The surplus now, the latest check's balances, and the difference. With no check yet, the balances are empty and the difference is what the surplus says is missing.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<ReconciliationView> Latest(Guid projectId, RedixDbContext db)
    {
        var latest = await db.Reconciliations.Include(row => row.Balances).Where(row => row.ProjectId == projectId).OrderByDescending(row => row.At).ThenByDescending(row => row.Id).FirstOrDefaultAsync() ?? new Reconciliation();
        var surplus = (await ProjectFigures.OfAsync(projectId, db)).Surplus;
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId).ToListAsync();
        var moved = await AccountMoney.MovementsOfProjectAsync(projectId, db);
        var actual = latest.Balances.ToDictionary(row => row.AccountId, row => row.ActualBalance);
        var rows = accounts.Where(account => account.IsActive || actual.ContainsKey(account.Id)).OrderBy(account => account.Type).ThenBy(account => account.Id)
            .Select(account => new ReconciliationBalanceView(account.Id, account.Name, account.Type, moved.GetValueOrDefault(account.Id), actual.GetValueOrDefault(account.Id), actual.ContainsKey(account.Id))).ToList();
        var total = actual.Values.Sum();
        return new ReconciliationView(latest.At, surplus, total, total - surplus, rows);
    }

    /// <summary>Keeps what each bank account and custody really holds, and answers the difference. An account not in the project answers <c>unknownAccount</c>; an amount beyond the limit <c>invalidAmount</c>; an active account left out <c>accountMissing</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What each holds.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Enter(Guid projectId, ReconcileRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId).ToListAsync();
        if (request.Balances.Any(row => accounts.All(account => account.Id != row.AccountId)))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAccount");
        }
        if (request.Balances.Any(row => !Money.IsAcceptable(row.ActualBalance)))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidAmount");
        }
        if (accounts.Any(account => account.IsActive && request.Balances.All(row => row.AccountId != account.Id)))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "AccountMissing");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var reconciliation = new Reconciliation { ProjectId = projectId, At = at, UserId = userId };
        reconciliation.Balances.AddRange(request.Balances.Select(row => new ReconciliationBalance { ReconciliationId = reconciliation.Id, AccountId = row.AccountId, ActualBalance = row.ActualBalance }));
        db.Reconciliations.Add(reconciliation);
        db.AddHistory(reconciliation, HistoryAction.Create, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok(await Latest(projectId, db));
    }
}

/// <summary>What the accounts really hold.</summary>
/// <param name="Balances">Each account's balance.</param>
public record ReconcileRequest(List<ActualBalanceRequest> Balances);

/// <summary>What one account really holds.</summary>
/// <param name="AccountId">The account.</param>
/// <param name="ActualBalance">What it holds.</param>
public record ActualBalanceRequest(Guid AccountId, decimal ActualBalance);

/// <summary>The accounts against the books.</summary>
/// <param name="At">When the latest check was done; the first day of the calendar when there was none.</param>
/// <param name="Surplus">What the books say the accounts should hold (BR-07).</param>
/// <param name="ActualTotal">What they really held at the latest check.</param>
/// <param name="Difference">That total less the surplus (BR-08).</param>
/// <param name="Balances">Each bank account and custody.</param>
public record ReconciliationView(DateTime At, decimal Surplus, decimal ActualTotal, decimal Difference, List<ReconciliationBalanceView> Balances);

/// <summary>One account against the books.</summary>
/// <param name="AccountId">The account.</param>
/// <param name="AccountName">Its name.</param>
/// <param name="Type">What kind it is.</param>
/// <param name="BooksBalance">What the books say it holds now.</param>
/// <param name="ActualBalance">What it really held at the latest check; zero when that check did not hold it.</param>
/// <param name="WasChecked">True when the latest check held this account, so an account added since is not read as checked empty (findings row 187).</param>
public record ReconciliationBalanceView(Guid AccountId, string AccountName, AccountType Type, decimal BooksBalance, decimal ActualBalance, bool WasChecked);
