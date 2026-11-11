using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.History;

namespace Redix.Api.Accounts;

/// <summary>
/// The history of a project's accounts (redix-app #89, the first screen of UC-069): what was done to them and what moved their money, read from the History table by its
/// project alone (D11), so no line of another project can reach it, newest first and a page at a time. A partner's money shows only to whoever may see it (BR-31).
/// </summary>
public static class AccountHistory
{
    private static readonly string[] AccountTables = ["Accounts", "AccountCustodians", "Transfers", "Reconciliations"];

    /// <summary>Maps the route that reads it, for anyone who reads the project, as the accounts themselves (D9).</summary>
    public static void MapAccountHistory(this WebApplication app) =>
        app.MapGroup("/projects/{projectId:guid}/accounts").RequireAuthorization(nameof(Permission.ReadProject)).MapGet("/history", Read);

    /// <summary>
    /// A page of the project's account history (D10): accounts added, changed, disabled, enabled or deleted, a custodian changed, transfers, entries once approved or reversed,
    /// and actual-balances checks; never a draft, a send or a rejection. <paramref name="account"/> keeps one account's lines, and an account not in the project answers
    /// <c>unknownAccount</c>; <paramref name="mine"/> keeps the caller's own; <paramref name="text"/> keeps the lines whose accounts, people or amount hold it (redix-app #104);
    /// <paramref name="before"/> starts after the last line of the page before.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the History table lives in.</param>
    /// <param name="account">The id of one account of the project, or nothing for every account: read as text, since a query's <see cref="Guid"/> cannot default.</param>
    /// <param name="mine">True for what the caller did alone.</param>
    /// <param name="before">The id of the last line already shown, or 0 for the newest.</param>
    /// <param name="text">What was typed in the search line; empty for every line.</param>
    public static async Task<IResult> Read(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, string account = "", bool mine = false, long before = 0, string text = "")
    {
        var accountId = Guid.TryParse(account, out var id) ? id : Guid.Empty;
        if (accountId != Guid.Empty && !await db.Accounts.AnyAsync(row => row.Id == accountId && row.ProjectId == projectId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAccount");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var seen = (await EntriesEndpoints.SeenByAsync(projectId, userId, db)).Select(entry => entry.Id);
        var rows = db.History.Where(row => row.ProjectId == projectId
            && (AccountTables.Contains(row.TableName) || (row.TableName == "Entries" && (row.Action == HistoryAction.Approve || row.Action == HistoryAction.Reverse) && seen.Contains(row.RecordId))));
        if (accountId != Guid.Empty)
        {
            var transfers = db.Transfers.Where(transfer => transfer.FromAccountId == accountId || transfer.ToAccountId == accountId).Select(transfer => transfer.Id);
            var entries = db.Entries.Where(entry => entry.AccountId == accountId).Select(entry => entry.Id);
            var checks = db.Reconciliations.Where(check => check.Balances.Any(balance => balance.AccountId == accountId)).Select(check => check.Id);
            rows = rows.Where(row => row.RecordId == accountId || transfers.Contains(row.RecordId) || entries.Contains(row.RecordId) || checks.Contains(row.RecordId));
        }
        return Results.Ok(await HistoryPage.LinesAsync(
            rows, userId, mine, before, text, page => LinesAsync(projectId, page, db), line => ArabicSpelling.Holds(text, line.Amount, line.By, line.Account, line.OtherAccount, line.NameWas, line.Custodian)));
    }

    private static async Task<List<AccountHistoryLine>> LinesAsync(Guid projectId, List<HistoryRecord> page, RedixDbContext db)
    {
        var ids = page.Select(row => row.RecordId).ToList();
        var transfers = await db.Transfers.Where(transfer => ids.Contains(transfer.Id)).ToDictionaryAsync(transfer => transfer.Id);
        var entries = await db.Entries.Where(entry => ids.Contains(entry.Id)).ToDictionaryAsync(entry => entry.Id);
        var checks = await db.Reconciliations.Include(check => check.Balances).Where(check => ids.Contains(check.Id)).ToDictionaryAsync(check => check.Id);
        var accountNames = await AccountNamesAsync(projectId, db);
        var people = page.Select(row => row.UserId).Concat(page.Where(row => row.TableName == "AccountCustodians").Select(row => HistoryPage.IdIn(row.After, "UserId"))).Distinct().ToList();
        var names = await db.Users.Where(user => people.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        return page.Select(row =>
        {
            var line = new AccountHistoryLine(row.Id, row.At, names.GetValueOrDefault(row.UserId, ""), AccountHistoryKind.AccountChanged, "", "", 0, EntryKind.Purchase, "", "");
            return row.TableName switch
            {
                "Accounts" => line with { Kind = KindOf(row.Action), Account = accountNames.GetValueOrDefault(row.RecordId, ""), NameWas = row.Action == HistoryAction.Update ? HistoryPage.TextIn(row.Before, "Name") : "" },
                "AccountCustodians" => line with { Kind = AccountHistoryKind.CustodianChanged, Account = accountNames.GetValueOrDefault(row.RecordId, ""), Custodian = names.GetValueOrDefault(HistoryPage.IdIn(row.After, "UserId"), "") },
                "Transfers" => line with { Kind = AccountHistoryKind.Transfer, Account = accountNames.GetValueOrDefault(transfers[row.RecordId].FromAccountId, ""), OtherAccount = accountNames.GetValueOrDefault(transfers[row.RecordId].ToAccountId, ""), Amount = transfers[row.RecordId].Amount },
                "Reconciliations" => line with { Kind = AccountHistoryKind.Check, Amount = checks[row.RecordId].Balances.Sum(balance => balance.ActualBalance) },
                _ => EntryLine(line, row, entries[row.RecordId], accountNames),
            };
        }).ToList();
    }

    private static AccountHistoryLine EntryLine(AccountHistoryLine line, HistoryRecord row, Entry entry, Dictionary<Guid, string> accountNames) => line with
    {
        Kind = row.Action == HistoryAction.Reverse ? AccountHistoryKind.Reversed : entry.Kind.Signed(entry.Amount) >= 0 ? AccountHistoryKind.EntryIn : AccountHistoryKind.EntryOut,
        Account = accountNames.GetValueOrDefault(entry.AccountId, ""),
        Amount = Math.Abs(entry.Amount),
        EntryKind = entry.Kind,
    };

    private static AccountHistoryKind KindOf(HistoryAction action) => action switch
    {
        HistoryAction.Create => AccountHistoryKind.AccountCreated,
        HistoryAction.Disable => AccountHistoryKind.AccountDisabled,
        HistoryAction.Enable => AccountHistoryKind.AccountEnabled,
        HistoryAction.Delete => AccountHistoryKind.AccountDeleted,
        _ => AccountHistoryKind.AccountChanged,
    };

    /// <summary>Every account's name; a deleted one's as its deletion kept it, since a removed record keeps all its values (H7).</summary>
    private static async Task<Dictionary<Guid, string>> AccountNamesAsync(Guid projectId, RedixDbContext db)
    {
        var names = await db.Accounts.Where(account => account.ProjectId == projectId).ToDictionaryAsync(account => account.Id, account => account.Name);
        var deleted = await db.History.Where(row => row.ProjectId == projectId && row.TableName == "Accounts" && row.Action == HistoryAction.Delete).Select(row => new { row.RecordId, row.Before }).ToListAsync();
        foreach (var row in deleted)
        {
            names[row.RecordId] = HistoryPage.TextIn(row.Before, "Name");
        }
        return names;
    }
}

/// <summary>What a line of the account history says was done.</summary>
public enum AccountHistoryKind
{
    /// <summary>An account was added.</summary>
    AccountCreated,

    /// <summary>An account's name changed.</summary>
    AccountChanged,

    /// <summary>An account was disabled.</summary>
    AccountDisabled,

    /// <summary>A disabled account was enabled.</summary>
    AccountEnabled,

    /// <summary>An account nothing named was deleted.</summary>
    AccountDeleted,

    /// <summary>Someone else now holds an account, or nobody does.</summary>
    CustodianChanged,

    /// <summary>Money moved between two accounts.</summary>
    Transfer,

    /// <summary>An approved entry brought money into an account.</summary>
    EntryIn,

    /// <summary>An approved entry took money out of an account.</summary>
    EntryOut,

    /// <summary>An approved entry was reversed.</summary>
    Reversed,

    /// <summary>The actual balances were counted.</summary>
    Check,
}

/// <summary>One line of the account history.</summary>
/// <param name="Id">Its History row, which the next page starts after.</param>
/// <param name="At">When it was done.</param>
/// <param name="By">Who did it.</param>
/// <param name="Kind">What was done.</param>
/// <param name="Account">The account it names: a transfer's first, an entry's; empty for a check.</param>
/// <param name="OtherAccount">A transfer's second account; empty otherwise.</param>
/// <param name="Amount">How much moved, or the counted total of a check; zero for an account's own action.</param>
/// <param name="EntryKind">An entry's kind, for an entry's line.</param>
/// <param name="NameWas">An account's name before it changed; empty otherwise.</param>
/// <param name="Custodian">Who holds an account now, for a custodian's change; empty when nobody does.</param>
public record AccountHistoryLine(long Id, DateTime At, string By, AccountHistoryKind Kind, string Account, string OtherAccount, decimal Amount, EntryKind EntryKind, string NameWas, string Custodian);
