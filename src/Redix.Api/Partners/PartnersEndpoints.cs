using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
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
        changes.MapPost("/partners/{partnerId:guid}", Edit);
        changes.MapPost("/partners/{partnerId:guid}/delete", Delete);
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
        var partners = (await db.Partners.Where(Partner.Kept).Where(partner => partner.ProjectId == projectId).ToListAsync()).Where(sight.Sees).ToList();
        var userIds = partners.Select(partner => partner.UserId).ToList();
        var names = await db.Users.Where(user => userIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        var deletable = await DeletableAsync(projectId, db);
        return partners.OrderBy(partner => partner.SortOrder).ThenBy(partner => partner.Id)
            .Select(partner => new PartnerView(partner.Id, partner.Name, Ids.Text(partner.UserId), names.GetValueOrDefault(partner.UserId, ""), deletable.Contains(partner.Id))).ToList();
    }

    /// <summary>
    /// Deletes a partner softly (redix-api #100): they leave Partners, Add entry, the item's tables and the capital lines, and their movements stay in the entries, the reports and the History
    /// table, which keeps the delete too. Only at zero capital with nothing waiting, drafted or rejected naming them, otherwise <c>partnerNotDeletable</c>; one deleted already is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid partnerId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var partner = await db.Partners.Where(Partner.Kept).SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
        if (partner is null)
        {
            return Results.NotFound();
        }
        if (!(await DeletableAsync(projectId, db)).Contains(partnerId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "PartnerNotDeletable");
        }
        partner.IsDeleted = true;
        db.AddHistory(partner, HistoryAction.Delete, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>The project's partners at zero capital that nothing unapproved names: the one answer of whether a partner may be deleted, which the list shows and the delete checks.</summary>
    private static async Task<HashSet<Guid>> DeletableAsync(Guid projectId, RedixDbContext db)
    {
        var balances = await CapitalEndpoints.BalancesAsync(projectId, db);
        var named = await db.CapitalMovementDetails.Where(details => db.Entries.Any(entry => entry.Id == details.EntryId && entry.ProjectId == projectId && entry.Status != EntryStatus.Approved))
            .Select(details => details.PartnerId).Distinct().ToListAsync();
        var partners = await db.Partners.Where(Partner.Kept).Where(partner => partner.ProjectId == projectId).Select(partner => partner.Id).ToListAsync();
        return [.. partners.Where(id => balances.GetValueOrDefault(id) == 0 && !named.Contains(id))];
    }

    /// <summary>
    /// Adds a partner as a table of the project's capital item, last among its tables (redix-api #109); a project has one (<c>capitalItemExists</c>), so nobody chooses it (Muteb, 11 Nov 2026).
    /// An unacceptable name answers <c>invalidName</c>; a capital item disabled, or no longer of capital, <c>itemNotOffered</c>; a login who is not in the project, <c>unknownUser</c>; a login that
    /// is already another partner's in this project, <c>userTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The partner's name, and their own login if they have one.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Add(Guid projectId, PartnerRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var itemId = await ProjectStructure.ItemsOf(projectId, db).Where(item => item.Type == ItemType.Capital).Select(item => item.Id).SingleOrDefaultAsync();
            if (await ProjectStructure.OfferedItemAsync(projectId, itemId, ItemType.Capital, db) == Item.None)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "ItemNotOffered");
            }
            var refusal = await LoginRefusalAsync(projectId, Partner.None, request.UserId, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            db.Partners.Add(new Partner { ProjectId = projectId, UserId = request.UserId, Name = name, ItemId = itemId, SortOrder = await ProjectStructure.NextTablePlaceAsync(itemId, db) });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>Renames a partner and sets their own login, answering as <see cref="Add"/> does about the name and the login.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="request">The partner's name, and their own login if they have one.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Edit(Guid projectId, Guid partnerId, PartnerRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var partner = await db.Partners.Where(Partner.Kept).SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
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
            partner.UserId = request.UserId;
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
        return await db.Partners.Where(Partner.Kept).AnyAsync(other => other.ProjectId == projectId && other.UserId == userId && other.Id != partnerId) ? "UserTaken" : "";
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
/// <param name="MayDelete">True at zero capital with nothing unapproved naming them, when Delete is offered (redix-api #100).</param>
public record PartnerView(Guid Id, string Name, string UserId, string UserName, bool MayDelete);
