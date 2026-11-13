using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.History;

namespace Redix.Api.Partners;

/// <summary>
/// The history of a project's partners (redix-app #101), the accounts' history for partners: partners added, changed and deleted, and capital movements once approved or reversed,
/// read from the History table by its project alone, newest first and a page at a time (<see cref="HistoryPage"/>). Only the partners the caller sees show (BR-31, <see cref="PartnerSight"/>).
/// A login given, changed or taken in an edit is a row of <c>PartnerUsers</c> (findings row 214); before it, a row of <c>Partners</c> held it, and those rows read as they always did.
/// </summary>
public static class PartnerHistory
{
    /// <summary>Maps the route that reads it, for anyone who reads the project.</summary>
    public static void MapPartnerHistory(this WebApplication app) =>
        app.MapGroup("/projects/{projectId:guid}/partners").RequireAuthorization(nameof(Permission.ReadProject)).MapGet("/history", Read);

    /// <summary>
    /// A page of the project's partner history. <paramref name="partner"/> keeps one partner's lines, and a partner not in the project, or not seen, answers <c>unknownPartner</c>;
    /// <paramref name="mine"/> keeps the caller's own; <paramref name="text"/> keeps the lines whose partner, account, people or amount hold it (redix-app #104);
    /// <paramref name="before"/> starts after the last line of the page before.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the History table lives in.</param>
    /// <param name="partner">The id of one partner, or nothing for every partner: read as text, since a query's <see cref="Guid"/> cannot default.</param>
    /// <param name="mine">True for what the caller did alone.</param>
    /// <param name="before">The id of the last line already shown, or 0 for the newest.</param>
    /// <param name="text">What was typed in the search line; empty for every line.</param>
    public static async Task<IResult> Read(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, string partner = "", bool mine = false, long before = 0, string text = "")
    {
        var userId = AccessTokens.UserIdOf(principal);
        var sight = await PartnerSight.OfUserAsync(userId, projectId, db);
        var seen = (await db.Partners.Where(row => row.ProjectId == projectId).ToListAsync()).Where(sight.Sees).Select(row => row.Id).ToList();
        var partnerId = Guid.TryParse(partner, out var id) ? id : Guid.Empty;
        if (partnerId != Guid.Empty && !seen.Contains(partnerId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownPartner");
        }

        var shown = partnerId == Guid.Empty ? seen : [partnerId];
        var movements = db.CapitalMovementDetails
            .Where(details => shown.Contains(details.PartnerId) && db.Entries.Any(entry => entry.Id == details.EntryId && entry.Status != EntryStatus.Removed)).Select(details => details.EntryId);
        var rows = db.History.Where(row => row.ProjectId == projectId
            && (((row.TableName == "Partners" || (row.TableName == "PartnerUsers" && row.Action == HistoryAction.Update)) && shown.Contains(row.RecordId))
                || (row.TableName == "Entries" && (row.Action == HistoryAction.Approve || row.Action == HistoryAction.Reverse) && movements.Contains(row.RecordId))));
        return Results.Ok(await HistoryPage.LinesAsync(rows, userId, mine, before, text, page => LinesAsync(projectId, page, db)));
    }

    private static async Task<List<PartnerHistoryLine>> LinesAsync(Guid projectId, List<HistoryRecord> page, RedixDbContext db)
    {
        var ids = page.Select(row => row.RecordId).ToList();
        var entries = await db.Entries.Where(entry => ids.Contains(entry.Id)).ToDictionaryAsync(entry => entry.Id);
        var partnerOf = await db.CapitalMovementDetails.Where(details => ids.Contains(details.EntryId)).ToDictionaryAsync(details => details.EntryId, details => details.PartnerId);
        var partners = await db.Partners.Where(row => row.ProjectId == projectId).ToDictionaryAsync(row => row.Id, row => row.Name);
        var accounts = await db.Accounts.Where(account => account.ProjectId == projectId).ToDictionaryAsync(account => account.Id, account => account.Name);
        var people = page.Select(row => row.UserId).Concat(page.Where(row => row.TableName != "Entries").Select(row => HistoryPage.IdIn(row.After, "UserId"))).Distinct().ToList();
        var names = await db.Users.Where(user => people.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        return page.Select(row =>
        {
            var line = new PartnerHistoryLine(row.Id, row.At, names.GetValueOrDefault(row.UserId, ""), PartnerHistoryKind.PartnerCreated, "", "", 0, "", "");
            if (row.TableName != "Entries")
            {
                return line with { Kind = KindOf(row), Partner = partners.GetValueOrDefault(row.RecordId, ""), NameWas = HistoryPage.TextIn(row.Before, "Name"), Login = names.GetValueOrDefault(HistoryPage.IdIn(row.After, "UserId"), "") };
            }
            var entry = entries[row.RecordId];
            return line with
            {
                Kind = entry.Kind == EntryKind.CapitalOut ? PartnerHistoryKind.CapitalOut : PartnerHistoryKind.CapitalIn,
                Partner = partners.GetValueOrDefault(partnerOf.GetValueOrDefault(row.RecordId), ""),
                Account = accounts.GetValueOrDefault(entry.AccountId, ""),
                Amount = entry.Amount,
            };
        }).ToList();
    }

    /// <summary>What a row of the Partners or PartnerUsers table says was done: added, deleted, renamed, or given or losing an account, which is all a PartnerUsers row says.</summary>
    private static PartnerHistoryKind KindOf(HistoryRecord row) => row.Action switch
    {
        HistoryAction.Create => PartnerHistoryKind.PartnerCreated,
        HistoryAction.Delete => PartnerHistoryKind.PartnerDeleted,
        _ when HistoryPage.TextIn(row.Before, "Name").Length > 0 => PartnerHistoryKind.PartnerChanged,
        _ => PartnerHistoryKind.PartnerAccount,
    };
}

/// <summary>What a line of the partner history says was done.</summary>
public enum PartnerHistoryKind
{
    /// <summary>A partner was added.</summary>
    PartnerCreated,

    /// <summary>A partner's name changed.</summary>
    PartnerChanged,

    /// <summary>A partner was given an account, another one, or none.</summary>
    PartnerAccount,

    /// <summary>A partner was deleted, softly (redix-api #100).</summary>
    PartnerDeleted,

    /// <summary>Capital came in from a partner, approved.</summary>
    CapitalIn,

    /// <summary>Capital went out to a partner, approved.</summary>
    CapitalOut,
}

/// <summary>One line of the partner history.</summary>
/// <param name="Id">Its History row, which the next page starts after.</param>
/// <param name="At">When it was done.</param>
/// <param name="By">Who did it.</param>
/// <param name="Kind">What was done.</param>
/// <param name="Partner">The partner it names, by their name now.</param>
/// <param name="Account">A movement's account; empty otherwise.</param>
/// <param name="Amount">How much moved; zero for a partner's own action.</param>
/// <param name="NameWas">A partner's name before it changed; empty otherwise.</param>
/// <param name="Login">The account a partner was given; empty for none, and for any other line.</param>
public record PartnerHistoryLine(long Id, DateTime At, string By, PartnerHistoryKind Kind, string Partner, string Account, decimal Amount, string NameWas, string Login)
    : IHistoryLine
{
    /// <summary>Whether <paramref name="text"/> is in its partner, account, who did it, the name it had, the login, or its amount.</summary>
    /// <param name="text">What was typed.</param>
    public bool Holds(string text) => ArabicSpelling.Holds(text, Amount, By, Partner, Account, NameWas, Login);
}
