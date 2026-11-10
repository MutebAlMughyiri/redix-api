using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;
using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>
/// A project's bank accounts and cash custodies (UC-015, FR-22, redix-app #89): add, see, change, disable or enable, and delete while nothing names one (BR-15). Every change is in
/// the History table, which the account history reads.
/// </summary>
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
        changes.MapPost("/accounts/{accountId:guid}/delete", Delete);
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
    /// Adds an account, which holds nothing until an entry or a transfer moves money into it. An unacceptable name answers <c>invalidName</c>; a cash custody with nobody
    /// holding it, <c>custodianRequired</c>; a custodian who is not in the project, <c>unknownCustodian</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The account's type and name, and who holds it.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Add(Guid projectId, AddAccountRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var name = request.Name.Trim();
        var refusal = await RefusalOfAsync(projectId, name, request.Type, request.CustodianId, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var account = new Account { ProjectId = projectId, Type = request.Type, Name = name };
        account.HandTo(request.CustodianId);
        db.Accounts.Add(account);
        db.AddHistory(account, HistoryAction.Create, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Changes an account's name and who holds it; its type never changes. It answers as <see cref="Add"/> does.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="request">Its name, and who holds it.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Edit(Guid projectId, Guid accountId, EditAccountRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == accountId && row.ProjectId == projectId);
        if (account is null)
        {
            return Results.NotFound();
        }

        var name = request.Name.Trim();
        var refusal = await RefusalOfAsync(projectId, name, account.Type, request.CustodianId, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        account.Name = name;
        account.HandTo(request.CustodianId);
        db.AddHistoryOfChanges(HistoryAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Disables an account: it disappears from entry and from every account picker, and stays in the reports (BR-15).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Disable(Guid projectId, Guid accountId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        SetActiveAsync(projectId, accountId, false, AccessTokens.UserIdOf(principal), db, time);

    /// <summary>Brings a disabled account back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Enable(Guid projectId, Guid accountId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        SetActiveAsync(projectId, accountId, true, AccessTokens.UserIdOf(principal), db, time);

    /// <summary>
    /// Deletes an account nothing names. One an entry in any status, an actual-balances check or a transfer names answers <c>accountInUse</c>, since they keep it: disabling it
    /// takes it out of entry instead (BR-15). Its history stays.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="accountId">The account.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid accountId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == accountId && row.ProjectId == projectId);
        if (account is null)
        {
            return Results.NotFound();
        }
        if ((await UsedAsync(projectId, db)).Contains(accountId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "AccountInUse");
        }
        db.Accounts.Remove(account);
        db.AddHistory(account, HistoryAction.Delete, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<List<AccountView>> ViewsAsync(Guid projectId, Guid callerId, RedixDbContext db, Func<Account, bool> shown)
    {
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId).Include(account => account.Custodians).ToListAsync();
        var custodianIds = accounts.Select(account => account.CustodianId).ToList();
        var names = await db.Users.Where(user => custodianIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        var balances = await AccountMoney.MovementsOfProjectAsync(projectId, db);
        var used = await UsedAsync(projectId, db);
        return accounts.Where(shown).OrderBy(account => account.Type).ThenBy(account => account.Id)
            .Select(account => AccountView.Of(account, names.GetValueOrDefault(account.CustodianId, ""), callerId, balances.GetValueOrDefault(account.Id), used.Contains(account.Id))).ToList();
    }

    /// <summary>The project's accounts that something names: an entry in any status, an actual-balances check, or a transfer either way (R15).</summary>
    private static async Task<HashSet<Guid>> UsedAsync(Guid projectId, RedixDbContext db)
    {
        var named = db.Entries.Where(entry => entry.ProjectId == projectId).Select(entry => entry.AccountId)
            .Union(db.Reconciliations.Where(check => check.ProjectId == projectId).SelectMany(check => check.Balances).Select(balance => balance.AccountId))
            .Union(db.Transfers.Where(transfer => transfer.ProjectId == projectId).Select(transfer => transfer.FromAccountId))
            .Union(db.Transfers.Where(transfer => transfer.ProjectId == projectId).Select(transfer => transfer.ToAccountId));
        return [.. await named.ToListAsync()];
    }

    private static async Task<string> RefusalOfAsync(Guid projectId, string name, AccountType type, Guid custodianId, RedixDbContext db)
    {
        var refusal = Account.RefusalOf(name, type, custodianId);
        return refusal.Length == 0 && custodianId != Guid.Empty && !await ProjectUsersEndpoints.IsMemberAsync(projectId, custodianId, db) ? "UnknownCustodian" : refusal;
    }

    private static async Task<IResult> SetActiveAsync(Guid projectId, Guid accountId, bool isActive, Guid userId, RedixDbContext db, TimeProvider time)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(row => row.Id == accountId && row.ProjectId == projectId);
        if (account is null)
        {
            return Results.NotFound();
        }
        account.IsActive = isActive;
        db.AddHistory(account, isActive ? HistoryAction.Enable : HistoryAction.Disable, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to add an account.</summary>
/// <param name="Name">The account's name.</param>
/// <param name="Type">What kind of account it is.</param>
public record AddAccountRequest(string Name, AccountType Type)
{
    /// <summary>The user who holds it; left out when nobody does.</summary>
    public Guid CustodianId { get; init; }
}

/// <summary>What the app sends to change an account.</summary>
/// <param name="Name">The account's name.</param>
public record EditAccountRequest(string Name)
{
    /// <summary>The user who holds it; left out when nobody does.</summary>
    public Guid CustodianId { get; init; }
}

/// <summary>An account as the app shows it.</summary>
/// <param name="Id">The account's id.</param>
/// <param name="Type">What kind of account it is.</param>
/// <param name="Name">The account's name.</param>
/// <param name="Balance">What it holds now: what its approved entries and transfers moved.</param>
/// <param name="CustodianId">The user who holds it; empty when nobody does.</param>
/// <param name="CustodianName">That user's name; empty when nobody does.</param>
/// <param name="IsMine">Whether the caller holds it, which the home screen shows (AC-22.1).</param>
/// <param name="IsActive">False once disabled.</param>
/// <param name="IsUsed">True once an entry, a check or a transfer names it, so it can only be disabled, never deleted (BR-15).</param>
public record AccountView(Guid Id, AccountType Type, string Name, decimal Balance, string CustodianId, string CustodianName, bool IsMine, bool IsActive, bool IsUsed)
{
    /// <summary>The view of <paramref name="account"/>, held by the user named <paramref name="custodianName"/>, for <paramref name="callerId"/>, holding <paramref name="balance"/>.</summary>
    public static AccountView Of(Account account, string custodianName, Guid callerId, decimal balance, bool isUsed) => new(
        account.Id,
        account.Type,
        account.Name,
        balance,
        account.Custodians.Select(custodian => custodian.UserId.ToString()).FirstOrDefault(""),
        custodianName,
        account.IsHeldBy(callerId),
        account.IsActive,
        isUsed);
}
