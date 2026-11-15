using System.Security.Claims;
using Redix.Api.Auth;

namespace Redix.Api.Partners;

/// <summary>
/// Who sees which partner (BR-31): whoever holds <see cref="Permission.SeeAllCapital"/> in the project sees every partner; whoever holds <see cref="Permission.SeeChosenCapital"/> only the partners
/// chosen for them (redix-api #106); anyone else none. The one owner every reader of a partner's capital asks.
/// </summary>
/// <param name="SeesAll">True when the caller holds <see cref="Permission.SeeAllCapital"/> in the project.</param>
/// <param name="Chosen">The partners chosen for the caller; empty unless they hold <see cref="Permission.SeeChosenCapital"/>.</param>
public readonly record struct PartnerSight(bool SeesAll, List<Guid> Chosen)
{
    /// <summary>The sight of what is printed for whoever sees all capital: every partner.</summary>
    public static PartnerSight All { get; } = new(true, []);

    /// <summary>What the caller of <paramref name="principal"/> sees of the partners of <paramref name="projectId"/>.</summary>
    public static Task<PartnerSight> OfAsync(ClaimsPrincipal principal, Guid projectId, RedixDbContext db) => OfUserAsync(AccessTokens.UserIdOf(principal), projectId, db);

    /// <summary>What <paramref name="userId"/> sees of the partners of <paramref name="projectId"/>, from their one access (redix-api #120 D5).</summary>
    public static async Task<PartnerSight> OfUserAsync(Guid userId, Guid projectId, RedixDbContext db)
    {
        var access = await UserAccess.OfAsync(userId, db);
        var held = access.In(projectId);
        return new(held.Contains(Permission.SeeAllCapital), held.Contains(Permission.SeeChosenCapital) ? access.ChosenIn(projectId) : []);
    }

    /// <summary>True when the caller sees the partner <paramref name="partnerId"/>.</summary>
    public bool Sees(Guid partnerId) => SeesAll || Chosen.Contains(partnerId);

    /// <summary>True when the caller sees <paramref name="partner"/>.</summary>
    public bool Sees(Partner partner) => Sees(partner.Id);
}
