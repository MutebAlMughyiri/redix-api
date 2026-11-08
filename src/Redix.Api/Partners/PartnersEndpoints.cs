using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>A project's partners (UC-016). There is no delete: a partner who withdrew stays, with a zero balance and their history.</summary>
public static class PartnersEndpoints
{
    /// <summary>Maps the route that reads a project's partners, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapPartners(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/partners", List).RequireAuthorization(nameof(Permission.ReadProject));

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/partners", Add);
        changes.MapPost("/partners/{partnerId:guid}", Edit);
    }

    /// <summary>The project's partners in their tables' order: all of them with <see cref="Permission.SeeAllCapital"/>, otherwise only the caller's own (BR-31).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<PartnerView>> List(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesAll = (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.SeeAllCapital);
        var partners = await db.Partners.Where(partner => partner.ProjectId == projectId && (seesAll || partner.UserId == userId)).ToListAsync();
        var userIds = partners.Select(partner => partner.UserId).ToList();
        var names = await db.Users.Where(user => userIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName);
        var items = await db.Items.Where(item => db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId) && item.Type == ItemType.Capital)
            .ToDictionaryAsync(item => item.Id, item => item.Name);
        return partners.OrderBy(partner => partner.SortOrder).ThenBy(partner => partner.Id)
            .Select(partner => PartnerView.Of(partner, names.GetValueOrDefault(partner.UserId, ""), items.GetValueOrDefault(partner.ItemId, ""))).ToList();
    }

    /// <summary>
    /// Adds a partner as a table of a capital item, last among its tables (redix-api #109). An unacceptable name answers <c>invalidName</c>; an item that is not one of the project's capital
    /// items, <c>itemNotOffered</c>; a login who is not in the project, <c>unknownUser</c>; a login that is already another partner's in this project, <c>userTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The partner's name, their capital item, and their login if they have one.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Add(Guid projectId, PartnerRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var refusal = await RefusalOfAsync(projectId, Guid.Empty, request, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            db.Partners.Add(new Partner { ProjectId = projectId, UserId = request.UserId, Name = name, ItemId = request.ItemId, SortOrder = await ProjectStructure.NextTablePlaceAsync(request.ItemId, db) });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>Renames a partner and sets their login. It answers as <see cref="Add"/> does.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="request">The partner's name, and their login if they have one.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Edit(Guid projectId, Guid partnerId, PartnerRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var partner = await db.Partners.SingleOrDefaultAsync(row => row.Id == partnerId && row.ProjectId == projectId);
            if (partner is null)
            {
                return Results.NotFound();
            }
            var refusal = await RefusalOfAsync(projectId, partnerId, request, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            if (partner.ItemId != request.ItemId)
            {
                partner.ItemId = request.ItemId;
                partner.SortOrder = await ProjectStructure.NextTablePlaceAsync(request.ItemId, db);
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

    private static async Task<string> RefusalOfAsync(Guid projectId, Guid partnerId, PartnerRequest request, RedixDbContext db)
    {
        if (await ProjectStructure.OfferedItemAsync(projectId, request.ItemId, ItemType.Capital, db) == Item.None)
        {
            return "ItemNotOffered";
        }
        var userId = request.UserId;
        if (userId == Guid.Empty)
        {
            return "";
        }
        if (!await ProjectUsersEndpoints.IsMemberAsync(projectId, userId, db))
        {
            return "UnknownUser";
        }
        return await db.Partners.AnyAsync(partner => partner.ProjectId == projectId && partner.UserId == userId && partner.Id != partnerId) ? "UserTaken" : "";
    }
}

/// <summary>What the app sends to add or change a partner.</summary>
/// <param name="Name">The partner's name.</param>
/// <param name="ItemId">The capital item whose table they are (redix-api #109).</param>
public record PartnerRequest(string Name, Guid ItemId)
{
    /// <summary>The login whose capital this is; left out for a partner with no login.</summary>
    public Guid UserId { get; init; }
}

/// <summary>A partner as the app shows it.</summary>
/// <param name="Id">The partner's id.</param>
/// <param name="Name">The partner's name.</param>
/// <param name="UserId">The login whose capital this is; empty for a partner with no login.</param>
/// <param name="UserName">That login's name; empty for a partner with no login.</param>
/// <param name="ItemId">The capital item whose table they are.</param>
/// <param name="ItemName">That item's name.</param>
public record PartnerView(Guid Id, string Name, string UserId, string UserName, Guid ItemId, string ItemName)
{
    /// <summary>The view of <paramref name="partner"/>, whose login is named <paramref name="userName"/> and whose capital item <paramref name="itemName"/>.</summary>
    public static PartnerView Of(Partner partner, string userName, string itemName) => new(partner.Id, partner.Name, Ids.Text(partner.UserId), userName, partner.ItemId, itemName);
}
