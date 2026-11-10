using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;

namespace Redix.Api.Partners;

/// <summary>
/// The capital ticks of a person's permissions and the partners chosen under them (BR-31, redix-api #106): one of <see cref="Permission.SeeAllCapital"/> and
/// <see cref="Permission.SeeChosenCapital"/> at a time, and partners chosen only while the second is held. <see cref="PartnerSight"/> reads what this writes.
/// </summary>
public static class CapitalSight
{
    /// <summary>The two capital ticks, which exclude each other.</summary>
    public static IReadOnlyList<Permission> Ticks { get; } = [Permission.SeeAllCapital, Permission.SeeChosenCapital];

    /// <summary>True when <paramref name="permissions"/> holds both capital ticks, which nobody may.</summary>
    public static bool HoldsBoth(IEnumerable<Permission> permissions) => Ticks.All(permissions.Contains);

    /// <summary>Chooses <paramref name="partnerId"/> for <paramref name="userId"/> in <paramref name="projectId"/>, with its history row; the caller saves.</summary>
    public static void Choose(Guid projectId, Guid userId, Guid partnerId, Guid actorId, DateTime now, RedixDbContext db)
    {
        var chosen = new ChosenPartner { ProjectId = projectId, UserId = userId, PartnerId = partnerId };
        db.ChosenPartners.Add(chosen);
        db.AddHistory(chosen, HistoryAction.AssignProject, actorId, now);
    }

    /// <summary>Takes every partner chosen for <paramref name="userId"/> in <paramref name="projectId"/>, or only <paramref name="partnerId"/> when it is given, with their history rows; the caller saves.</summary>
    public static async Task ClearAsync(Guid projectId, Guid userId, Guid actorId, DateTime now, RedixDbContext db, Guid partnerId = default)
    {
        var rows = await db.ChosenPartners.Where(row => row.ProjectId == projectId && row.UserId == userId && (partnerId == Guid.Empty || row.PartnerId == partnerId)).ToListAsync();
        foreach (var row in rows)
        {
            db.ChosenPartners.Remove(row);
            db.AddHistory(row, HistoryAction.RemoveProject, actorId, now);
        }
    }
}
