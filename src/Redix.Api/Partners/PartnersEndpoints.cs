using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>
/// A project's partners (UC-016): added, read, renamed, and deleted softly at zero capital (redix-api #100), their movements staying in the books. Who sees each is ticked on people's
/// permissions (redix-api #106).
/// </summary>
public static class PartnersEndpoints
{
    /// <summary>Maps the route that reads a project's partners, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapPartners(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/partners", List).RequireAuthorization(nameof(Permission.ReadProject));

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/partners", Add);
        changes.MapPut("/partners/{partnerId:guid}", Edit);
        changes.MapDelete("/partners/{partnerId:guid}", Delete);
    }

    /// <summary>
    /// The project's partners the caller sees (BR-31, <see cref="PartnerSight"/>), in their tables' order, each with the name of their own login and whether it may be deleted now; a deleted
    /// partner is not among them (redix-api #100).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<PartnerView>> List(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var sight = await PartnerSight.OfAsync(principal, projectId, db);
        var partners = (await db.Partners.Include(partner => partner.Users).Where(Partner.Kept).Where(partner => partner.ProjectId == projectId).ToListAsync()).Where(sight.Sees).ToList();
        var userIds = partners.Select(partner => partner.UserId).ToList();
        var names = await db.Users.Where(user => userIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        var userId = AccessTokens.UserIdOf(principal);
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var drafts = await DraftsAsync(projectId, db);
        var deletable = await DeletableAsync(projectId, userId, held, drafts, db);
        var seesWhoseDrafts = held.Contains(Permission.ManageSettings);
        return partners.OrderBy(partner => partner.SortOrder).ThenBy(partner => partner.Id)
            .Select(partner => new PartnerView(
                partner.Id,
                partner.Name,
                Ids.Text(partner.UserId),
                names.GetValueOrDefault(partner.UserId, ""),
                deletable.Contains(partner.Id),
                [.. drafts.Where(draft => seesWhoseDrafts && draft.PartnerId == partner.Id && draft.AuthorId != userId).Select(draft => draft.AuthorName).Distinct()],
                [.. drafts.Where(draft => draft.PartnerId == partner.Id && draft.AuthorId == userId).Select(draft => new PartnerDraftView(draft.EntryDate, draft.Amount))])).ToList();
    }

    /// <summary>
    /// Deletes a partner softly (redix-api #100): they leave Partners, Add entry, the item's tables and the capital lines, and their movements stay in the entries, the reports and the History
    /// table, which keeps the delete too; their login is theirs no more, so it may be another partner's (findings row 214). Only at zero capital with nothing waiting or rejected naming
    /// them, and no one else's draft (findings row 199), otherwise
    /// <c>partnerNotDeletable</c>; the caller's own drafts naming them are deleted with them, as a delete of each entry is, so only by whoever also holds
    /// <see cref="Permission.DeleteEntries"/> (findings row 202). One deleted already is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid partnerId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var partner = await db.Partners.Include(row => row.Users).Where(Partner.Kept).SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
        if (partner is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var drafts = await DraftsAsync(projectId, db);
        if (!(await DeletableAsync(projectId, userId, (await UserAccess.OfAsync(userId, db)).In(projectId), drafts, db)).Contains(partnerId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "PartnerNotDeletable");
        }
        var at = time.GetUtcNow().UtcDateTime;
        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var draftId in drafts.Where(draft => draft.PartnerId == partnerId).Select(draft => draft.EntryId).Distinct())
        {
            await EntryChanges.DeleteAsync(await db.Entries.SingleAsync(entry => entry.Id == draftId), userId, at, db);
        }
        partner.IsDeleted = true;
        partner.LinkTo(Guid.Empty);
        db.AddHistory(partner, HistoryAction.Delete, userId, at);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Results.Ok();
    }

    /// <summary>A draft naming a partner: who entered it, and its date and amount, for the partner's delete to say whose draft holds it, or to delete the caller's own (findings row 199).</summary>
    private sealed record PartnerDraft(Guid PartnerId, Guid EntryId, Guid AuthorId, string AuthorName, DateOnly EntryDate, decimal Amount);

    private static Task<List<PartnerDraft>> DraftsAsync(Guid projectId, RedixDbContext db) =>
        (from details in db.CapitalMovementDetails
         join entry in db.Entries on details.EntryId equals entry.Id
         join author in db.Users on entry.CreatedByUserId equals author.Id
         where entry.ProjectId == projectId && entry.Status == EntryStatus.Draft
         select new PartnerDraft(details.PartnerId, entry.Id, author.Id, author.DisplayName, entry.EntryDate, entry.Amount)).ToListAsync();

    /// <summary>
    /// The project's partners <paramref name="userId"/>, holding <paramref name="held"/>, may delete: at zero capital, with nothing waiting or rejected naming them, and no draft of anyone
    /// else's, since their own go with them, which only whoever holds <see cref="Permission.DeleteEntries"/> may delete (findings row 202): the one answer of whether a partner may be
    /// deleted, which the list shows and the delete checks.
    /// </summary>
    private static async Task<HashSet<Guid>> DeletableAsync(Guid projectId, Guid userId, List<Permission> held, List<PartnerDraft> drafts, RedixDbContext db)
    {
        var balances = await CapitalEndpoints.BalancesAsync(projectId, db);
        var waiting = await db.CapitalMovementDetails
            .Where(details => db.Entries.Any(entry => entry.Id == details.EntryId && entry.ProjectId == projectId && (entry.Status == EntryStatus.PendingApproval || entry.Status == EntryStatus.Rejected)))
            .Select(details => details.PartnerId).Distinct().ToListAsync();
        var heldByDrafts = drafts.Where(draft => draft.AuthorId != userId || !held.Contains(Permission.DeleteEntries)).Select(draft => draft.PartnerId).ToHashSet();
        var partners = await db.Partners.Where(Partner.Kept).Where(partner => partner.ProjectId == projectId).Select(partner => partner.Id).ToListAsync();
        return [.. partners.Where(id => balances.GetValueOrDefault(id) == 0 && !waiting.Contains(id) && !heldByDrafts.Contains(id))];
    }

    /// <summary>
    /// Adds a partner as a table of the project's capital item, last among its tables (redix-api #109); a project has one (<c>capitalItemExists</c>), so nobody chooses it (Muteb, 11 Nov 2026).
    /// An unacceptable name answers <c>invalidName</c>; a capital item disabled, or no longer of capital, <c>itemNotOffered</c>; a login who is not in the project, <c>unknownUser</c>; a login that
    /// is already another partner's in this project, <c>userTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The partner's name, and their own login if they have one.</param>
    /// <param name="principal">The caller's session, for the history row (redix-app #101).</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Add(Guid projectId, PartnerRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var refusal = await AddAsync(projectId, name, request.UserId, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db);
            return refusal.Length > 0 ? Refusal.Of(StatusCodes.Status400BadRequest, refusal) : Results.Ok();
        });

    /// <summary>
    /// Adds the partner <paramref name="name"/>, a name already accepted, with their own login <paramref name="loginId"/> if they have one, as the last table of the project's capital
    /// item, by <paramref name="userId"/> at <paramref name="at"/>, in the History table, the partner and their login alone, whatever else the save carries: the one way a
    /// partner is added, by hand and from a project file's load alike (redix-api #132 D6). It answers the refusals of <see cref="Add"/> but the name's; empty once added.
    /// </summary>
    public static async Task<string> AddAsync(Guid projectId, string name, Guid loginId, Guid userId, DateTime at, RedixDbContext db)
    {
        var itemId = await ProjectStructure.ItemsOf(projectId, db).Where(item => item.Type == ItemType.Capital).Select(item => item.Id).SingleOrDefaultAsync();
        if (await ProjectStructure.OfferedItemAsync(projectId, itemId, ItemType.Capital, db) == Item.None)
        {
            return "ItemNotOffered";
        }
        var refusal = await LoginRefusalAsync(projectId, Partner.None, loginId, db);
        if (refusal.Length > 0)
        {
            return refusal;
        }
        var partner = new Partner { ProjectId = projectId, Name = name, ItemId = itemId, SortOrder = await ProjectStructure.NextTablePlaceAsync(itemId, db) };
        partner.LinkTo(loginId);
        db.Partners.Add(partner);
        db.AddHistory(partner, HistoryAction.Create, userId, at);
        foreach (var login in partner.Users)
        {
            db.AddHistory(login, HistoryAction.Create, userId, at);
        }
        await db.SaveChangesAsync();
        return "";
    }

    /// <summary>Renames a partner and sets their own login, answering as <see cref="Add"/> does about the name and the login.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="request">The partner's name, and their own login if they have one.</param>
    /// <param name="principal">The caller's session, for the history row (redix-app #101).</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Edit(Guid projectId, Guid partnerId, PartnerRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var partner = await db.Partners.Include(row => row.Users).Where(Partner.Kept).SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
            if (partner is null)
            {
                return Results.NotFound();
            }
            var refusal = await LoginRefusalAsync(projectId, partner, request.UserId, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            partner.Name = name;
            partner.LinkTo(request.UserId);
            db.AddHistoryOfChanges(HistoryAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>
    /// The partners declared with a new project (redix-api #109), in the order typed, each to become a table of its رأس المال once the project is approved and has a structure
    /// (<see cref="ProjectStructure.ApproveAsync"/>); <c>invalidName</c> when a name is not acceptable, as a partner added later answers.
    /// </summary>
    public static (List<Partner> Partners, string Refusal) DeclaredWith(Guid projectId, IEnumerable<string> names)
    {
        var trimmed = names.Select(name => name.Trim()).ToList();
        return trimmed.All(Project.IsAcceptableName)
            ? ([.. trimmed.Select((name, place) => new Partner { ProjectId = projectId, Name = name, SortOrder = place + 1 })], "")
            : ([], nameof(ProjectChangeOutcome.InvalidName));
    }

    /// <summary>Why <paramref name="userId"/> cannot be the own login of <paramref name="partner"/>: not in the project, or already another partner's; empty when it can, and for no login.</summary>
    private static async Task<string> LoginRefusalAsync(Guid projectId, Partner partner, Guid userId, RedixDbContext db)
    {
        if (userId == Guid.Empty)
        {
            return "";
        }
        if (!await ProjectUsersEndpoints.IsMemberAsync(projectId, userId, db))
        {
            return "UnknownUser";
        }
        var partnerId = partner.Id;
        return await db.PartnerUsers.AnyAsync(login => login.ProjectId == projectId && login.UserId == userId && login.PartnerId != partnerId) ? "UserTaken" : "";
    }
}

/// <summary>What the app sends to add or change a partner.</summary>
/// <param name="Name">The partner's name.</param>
public record PartnerRequest(string Name)
{
    /// <summary>The partner's own login; left out for a partner with no login. It gives no sight of capital (redix-api #106).</summary>
    public Guid UserId { get; init; }
}

/// <summary>A partner as the app shows it.</summary>
/// <param name="Id">The partner's id.</param>
/// <param name="Name">The partner's name.</param>
/// <param name="UserId">Their own login; empty for a partner with no login.</param>
/// <param name="UserName">That login's name; empty for a partner with no login.</param>
/// <param name="MayDelete">True when the caller may delete them: at zero capital with nothing waiting or rejected naming them, no one else's draft, and none of the caller's own
/// unless they hold <see cref="Permission.DeleteEntries"/> (redix-api #100, findings rows 199 and 202).</param>
/// <param name="OthersDrafts">Who else holds a draft naming them, which keeps them from being deleted until it is sent or deleted: told only to whoever holds
/// <see cref="Permission.ManageSettings"/>, who may delete partners, since a draft is its author's alone (BR-42, findings row 203); empty for anyone else.</param>
/// <param name="MyDrafts">The caller's own drafts naming them, which deleting them deletes too.</param>
public record PartnerView(Guid Id, string Name, string UserId, string UserName, bool MayDelete, List<string> OthersDrafts, List<PartnerDraftView> MyDrafts);

/// <summary>One of the caller's own drafts naming a partner, which deleting the partner deletes too (findings row 199).</summary>
/// <param name="EntryDate">The draft's date.</param>
/// <param name="Amount">Its amount.</param>
public record PartnerDraftView(DateOnly EntryDate, decimal Amount);
