using System.Security.Claims;
using Redix.Api.Auth;

namespace Redix.Api.Partners;

/// <summary>Who sees which partner (BR-31): whoever holds <see cref="Permission.SeeAllCapital"/> in the project sees every partner; anyone else only the partner whose login they are.</summary>
/// <param name="UserId">The caller.</param>
/// <param name="SeesAll">True when the caller holds <see cref="Permission.SeeAllCapital"/> in the project.</param>
public readonly record struct PartnerSight(Guid UserId, bool SeesAll)
{
    /// <summary>What the caller of <paramref name="principal"/> sees of the partners of <paramref name="projectId"/>.</summary>
    public static Task<PartnerSight> OfAsync(ClaimsPrincipal principal, Guid projectId, RedixDbContext db) => OfUserAsync(AccessTokens.UserIdOf(principal), projectId, db);

    /// <summary>What <paramref name="userId"/> sees of the partners of <paramref name="projectId"/>.</summary>
    public static async Task<PartnerSight> OfUserAsync(Guid userId, Guid projectId, RedixDbContext db) =>
        new(userId, (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.SeeAllCapital));

    /// <summary>True when the caller sees <paramref name="partner"/>.</summary>
    public bool Sees(Partner partner) => SeesAll || partner.UserId == UserId;
}
