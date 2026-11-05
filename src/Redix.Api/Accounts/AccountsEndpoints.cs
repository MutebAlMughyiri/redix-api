using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>A project's bank accounts and cash custodies (UC-015, FR-22). There is no delete: what is no longer wanted is disabled (BR-15).</summary>
public static class AccountsEndpoints
{
    /// <summary>Maps the routes that read a project's accounts, for anyone who reads the project (BR-31), and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapAccounts(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/accounts").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/offered", Offered);

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/accounts", Add);
        changes.MapPost("/accounts/{accountId:guid}", Edit);
        changes.MapPost("/accounts/{accountId:guid}/disable", Disable);
        changes.MapPost("/accounts/{accountId:guid}/enable", Enable);
    }

    /// <summary>Every account of the project, disabled ones included, banks first and then in the order they were added (AC-22.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<AccountView>> List(Guid projectId, ClaimsPrincipal principal, RedixDbContext db) => ViewsAsync(projectId, AccessTokens.UserIdOf(principal), db, _ => true);

    /// <summary>What the caller's entry may be paid from: the active accounts, all of them with <see cref="Permission.ChargeAnyAccount"/>, otherwise only those they hold (BRS open question 4).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<AccountView>> Offered(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var chargesAny = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.ChargeAnyAccount);
        return await ViewsAsync(projectId, userId, db, account => account.IsOfferedTo(userId, chargesAny));
    }

    /// <summary>
    /// Adds an account. An unacceptable name answers <c>invalidName</c>; an opening balance beyond the limit or with more than two decimals, <c>invalidOpeningBalance</c>; a cash custody
    /// with nobody holding it, <c>custodianRequired</c>; a custodian who is not in the project, <c>unknownCustodian</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The account's type, name and opening balance, and who holds it.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<IResult> Add(Guid projectId, AddAccountRequest request, RedixDbContext db)
    {
        var name = request.Name.Trim();
        var refusal = await RefusalOfAsync(projectId, name, request.Type, request.OpeningBalance, request.CustodianId, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var account = new Account { ProjectId = projectId, Type = request.Type, Name = name, OpeningBalance = request.OpeningBalance };
        account.HandTo(request.CustodianId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>
    /// Changes an account's name, its opening balance and who holds it, in the audit trail; its type never changes. It answers as <see cref="Add"/> does. Once any entry names it, a new opening balance
    /// answers <c>accountHasEntries</c>, because it would move every balance after it and could hide a reconciliation difference (BR-08).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="request">Its name and opening balance, and who holds it.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Edit(Guid projectId, Guid accountId, EditAccountRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == accountId && row.ProjectId == projectId);
        if (account is null)
        {
            return Results.NotFound();
        }

        var name = request.Name.Trim();
        var refusal = await RefusalOfAsync(projectId, name, account.Type, request.OpeningBalance, request.CustodianId, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        if (request.OpeningBalance != account.OpeningBalance && await db.Entries.AnyAsync(entry => entry.AccountId == accountId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "AccountHasEntries");
        }

        account.Name = name;
        account.OpeningBalance = request.OpeningBalance;
        account.HandTo(request.CustodianId);
        db.Audit(account, AuditAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Disables an account: it disappears from entry and stays in the reports (BR-15).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Disable(Guid projectId, Guid accountId, RedixDbContext db) => SetActiveAsync(projectId, accountId, false, db);

    /// <summary>Brings a disabled account back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Enable(Guid projectId, Guid accountId, RedixDbContext db) => SetActiveAsync(projectId, accountId, true, db);

    private static async Task<List<AccountView>> ViewsAsync(Guid projectId, Guid callerId, RedixDbContext db, Func<Account, bool> shown)
    {
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId).Include(account => account.Custodians).ToListAsync();
        var custodianIds = accounts.Select(account => account.CustodianId).ToList();
        var names = await db.Users.Where(user => custodianIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        var moved = await AccountMoney.MovementsOfProjectAsync(projectId, db);
        return accounts.Where(shown).OrderBy(account => account.Type).ThenBy(account => account.Id)
            .Select(account => AccountView.Of(account, names.GetValueOrDefault(account.CustodianId, ""), callerId, moved.GetValueOrDefault(account.Id))).ToList();
    }

    private static async Task<string> RefusalOfAsync(Guid projectId, string name, AccountType type, decimal openingBalance, Guid custodianId, RedixDbContext db)
    {
        var refusal = Account.RefusalOf(name, type, openingBalance, custodianId);
        return refusal.Length == 0 && custodianId != Guid.Empty && !await ProjectUsersEndpoints.IsMemberAsync(projectId, custodianId, db) ? "UnknownCustodian" : refusal;
    }

    private static async Task<IResult> SetActiveAsync(Guid projectId, Guid accountId, bool isActive, RedixDbContext db)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(row => row.Id == accountId && row.ProjectId == projectId);
        if (account is null)
        {
            return Results.NotFound();
        }
        account.IsActive = isActive;
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to add an account.</summary>
/// <param name="Name">The account's name.</param>
/// <param name="Type">What kind of account it is.</param>
/// <param name="OpeningBalance">What it held before its first entry.</param>
public record AddAccountRequest(string Name, AccountType Type, decimal OpeningBalance)
{
    /// <summary>The user who holds it; left out when nobody does.</summary>
    public Guid CustodianId { get; init; }
}

/// <summary>What the app sends to change an account.</summary>
/// <param name="Name">The account's name.</param>
/// <param name="OpeningBalance">What it held before its first entry.</param>
public record EditAccountRequest(string Name, decimal OpeningBalance)
{
    /// <summary>The user who holds it; left out when nobody does.</summary>
    public Guid CustodianId { get; init; }
}

/// <summary>An account as the app shows it.</summary>
/// <param name="Id">The account's id.</param>
/// <param name="Type">What kind of account it is.</param>
/// <param name="Name">The account's name.</param>
/// <param name="OpeningBalance">What it held before its first entry.</param>
/// <param name="Balance">What it holds now.</param>
/// <param name="CustodianId">The user who holds it; empty when nobody does.</param>
/// <param name="CustodianName">That user's name; empty when nobody does.</param>
/// <param name="IsMine">Whether the caller holds it, which the home screen shows (AC-22.1).</param>
/// <param name="IsActive">False once disabled.</param>
public record AccountView(Guid Id, AccountType Type, string Name, decimal OpeningBalance, decimal Balance, string CustodianId, string CustodianName, bool IsMine, bool IsActive)
{
    /// <summary>The view of <paramref name="account"/>, held by the user named <paramref name="custodianName"/>, for <paramref name="callerId"/>, after <paramref name="moved"/> of approved entries.</summary>
    public static AccountView Of(Account account, string custodianName, Guid callerId, decimal moved) => new(
        account.Id,
        account.Type,
        account.Name,
        account.OpeningBalance,
        account.OpeningBalance + moved,
        account.Custodians.Select(custodian => custodian.UserId.ToString()).FirstOrDefault(""),
        custodianName,
        account.IsHeldBy(callerId),
        account.IsActive);
}
