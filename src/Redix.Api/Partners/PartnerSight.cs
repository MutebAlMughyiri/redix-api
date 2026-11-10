using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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

    /// <summary>What <paramref name="userId"/> sees of the partners of <paramref name="projectId"/>.</summary>
    public static async Task<PartnerSight> OfUserAsync(Guid userId, Guid projectId, RedixDbContext db)
    {
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var chosen = held.Contains(Permission.SeeChosenCapital)
            ? await db.ChosenPartners.Where(row => row.ProjectId == projectId && row.UserId == userId).Select(row => row.PartnerId).ToListAsync()
            : [];
        return new(held.Contains(Permission.SeeAllCapital), chosen);
    }

    /// <summary>True when the caller sees the partner <paramref name="partnerId"/>.</summary>
    public bool Sees(Guid partnerId) => SeesAll || Chosen.Contains(partnerId);

    /// <summary>True when the caller sees <paramref name="partner"/>.</summary>
    public bool Sees(Partner partner) => Sees(partner.Id);
}
