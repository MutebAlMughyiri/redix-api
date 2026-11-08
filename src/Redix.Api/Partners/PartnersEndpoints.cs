using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Projects;

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

    /// <summary>The project's partners in the order they were added: all of them with <see cref="Permission.SeeAllCapital"/>, otherwise only the caller's own (BR-31).</summary>
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
        return partners.OrderBy(partner => partner.Id).Select(partner => PartnerView.Of(partner, names.GetValueOrDefault(partner.UserId, ""))).ToList();
    }

    /// <summary>
    /// Adds a partner. An unacceptable name answers <c>invalidName</c>; a login who is not in the project, <c>unknownUser</c>; a login that is already another partner's in this project, <c>userTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The partner's name, and their login if they have one.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Add(Guid projectId, PartnerRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var refusal = await RefusalOfLoginAsync(projectId, Guid.Empty, request.UserId, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            db.Partners.Add(new Partner { ProjectId = projectId, UserId = request.UserId, Name = name });
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
            var refusal = await RefusalOfLoginAsync(projectId, partnerId, request.UserId, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
            partner.Name = name;
            partner.UserId = request.UserId;
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    private static async Task<string> RefusalOfLoginAsync(Guid projectId, Guid partnerId, Guid userId, RedixDbContext db)
    {
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
public record PartnerRequest(string Name)
{
    /// <summary>The login whose capital this is; left out for a partner with no login.</summary>
    public Guid UserId { get; init; }
}

/// <summary>A partner as the app shows it.</summary>
/// <param name="Id">The partner's id.</param>
/// <param name="Name">The partner's name.</param>
/// <param name="UserId">The login whose capital this is; empty for a partner with no login.</param>
/// <param name="UserName">That login's name; empty for a partner with no login.</param>
public record PartnerView(Guid Id, string Name, string UserId, string UserName)
{
    /// <summary>The view of <paramref name="partner"/>, whose login is named <paramref name="userName"/>.</summary>
    public static PartnerView Of(Partner partner, string userName) => new(partner.Id, partner.Name, Ids.Text(partner.UserId), userName);
}
