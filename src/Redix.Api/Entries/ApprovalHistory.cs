using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;

namespace Redix.Api.Entries;

/// <summary>
/// The history of a project's approvals (Muteb, 12 Nov 2026), beside the accounts' and the partners' (redix-app #101): every entry approved or rejected, by whom and when, a
/// rejection with its reason, read from the History table newest first and a page at a time (<see cref="HistoryPage"/>), for whoever approves entries, as Approvals is.
/// </summary>
public static class ApprovalHistory
{
    /// <summary>Maps the route that reads it, for whoever approves entries.</summary>
    public static void MapApprovalHistory(this WebApplication app) =>
        app.MapGroup("/projects/{projectId:guid}/approvals").RequireAuthorization(nameof(Permission.ApproveEntries)).MapGet("/history", Read);

    /// <summary>
    /// A page of the project's approval history. <paramref name="kind"/> keeps the decisions on entries of one kind, as Approvals' filter does, and any other text keeps every
    /// kind; <paramref name="mine"/> keeps the caller's own decisions; <paramref name="text"/> keeps the lines whose item, party, people, reason or amount hold it (redix-app #104);
    /// <paramref name="before"/> starts after the last line of the page before.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the History table lives in.</param>
    /// <param name="kind">The kind of entry, such as <c>purchase</c>, or nothing for every kind.</param>
    /// <param name="mine">True for what the caller decided alone.</param>
    /// <param name="before">The id of the last line already shown, or 0 for the newest.</param>
    /// <param name="text">What was typed in the search line; empty for every line.</param>
    public static async Task<IResult> Read(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, string kind = "", bool mine = false, long before = 0, string text = "")
    {
        EntryKind[] kinds = Enum.TryParse<EntryKind>(kind, true, out var chosen) ? [chosen] : Enum.GetValues<EntryKind>();
        var decided = db.Entries.Where(entry => entry.ProjectId == projectId && kinds.Contains(entry.Kind)).Select(entry => entry.Id);
        var rows = db.History.Where(row => row.ProjectId == projectId && row.TableName == "Entries"
            && (row.Action == HistoryAction.Approve || row.Action == HistoryAction.Reject) && decided.Contains(row.RecordId));
        return Results.Ok(await HistoryPage.LinesAsync(rows, AccessTokens.UserIdOf(principal), mine, before, text, page => LinesAsync(projectId, page, db)));
    }

    private static async Task<List<ApprovalHistoryLine>> LinesAsync(Guid projectId, List<HistoryRecord> page, RedixDbContext db)
    {
        var ids = page.Select(row => row.RecordId).Distinct().ToList();
        var entries = await db.Entries.Where(entry => ids.Contains(entry.Id)).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var byId = entries.ToDictionary(entry => entry.Id);
        var rejections = await db.EntryDecisions.Where(decision => ids.Contains(decision.EntryId) && decision.Action == EntryDecisionAction.Reject).ToListAsync();
        var people = page.Select(row => row.UserId).Distinct().ToList();
        var names = await db.Users.Where(user => people.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        return [.. page.Select(row =>
        {
            var entry = byId[row.RecordId];
            var isRejection = row.Action == HistoryAction.Reject;
            var reason = isRejection ? string.Join("", rejections.Where(decision => decision.EntryId == entry.Id && decision.At == row.At).Select(decision => decision.Reason).Take(1)) : "";
            var item = facts.ItemNameOf(entry);
            return new ApprovalHistoryLine(row.Id, row.At, names.GetValueOrDefault(row.UserId, ""), isRejection ? ApprovalHistoryKind.Rejected : ApprovalHistoryKind.Approved,
                item.Length > 0 ? item : facts.PartnerNameOf(entry), facts.SupplierNameOf(entry), entry.Amount, reason);
        })];
    }
}

/// <summary>What a line of the approval history says was decided.</summary>
public enum ApprovalHistoryKind
{
    /// <summary>An entry was approved.</summary>
    Approved,

    /// <summary>An entry was rejected, with a reason.</summary>
    Rejected,
}

/// <summary>One line of the approval history.</summary>
/// <param name="Id">Its History row, which the next page starts after.</param>
/// <param name="At">When it was decided.</param>
/// <param name="By">Who decided it.</param>
/// <param name="Kind">What was decided.</param>
/// <param name="Item">The entry's item, or for a partner's money the partner.</param>
/// <param name="Party">The entry's supplier or contractor; empty for none.</param>
/// <param name="Amount">The entry's amount.</param>
/// <param name="Reason">A rejection's reason; empty for an approval.</param>
public record ApprovalHistoryLine(long Id, DateTime At, string By, ApprovalHistoryKind Kind, string Item, string Party, decimal Amount, string Reason) : IHistoryLine
{
    /// <summary>Whether <paramref name="text"/> is in its item, party, who decided it, its reason, or its amount.</summary>
    /// <param name="text">What was typed.</param>
    public bool Holds(string text) => ArabicSpelling.Holds(text, Amount, By, Item, Party, Reason);
}
