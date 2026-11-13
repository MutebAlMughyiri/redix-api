using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>
/// The rule that every project keeps an active person who manages its users (BR-29): whoever holds <see cref="Permission.ManageUsers"/> there and is not inactive there
/// (redix-api #126).
/// </summary>
public static class ProjectManagers
{
    /// <summary>Whether <paramref name="userId"/> is the last active person managing the users of <paramref name="projectId"/>, whom neither taking a permission nor being made inactive may take away.</summary>
    public static async Task<bool> IsTheLastAsync(Guid projectId, Guid userId, RedixDbContext db)
    {
        var managers = db.ProjectPermissions.Where(held => held.ProjectId == projectId && held.Permission == Permission.ManageUsers
            && !db.InactiveMembers.Any(inactive => inactive.ProjectId == projectId && inactive.UserId == held.UserId));
        return await managers.AnyAsync(held => held.UserId == userId) && !await managers.AnyAsync(held => held.UserId != userId);
    }

    /// <summary>Whether taking <paramref name="taken"/>, rows of one person in one project, would leave that project with no active person managing its users.</summary>
    public static async Task<bool> WouldLoseTheLastAsync(List<ProjectPermission> taken, RedixDbContext db)
    {
        var managing = taken.Where(held => held.Permission == Permission.ManageUsers).ToList();
        return managing.Count > 0 && await IsTheLastAsync(managing[0].ProjectId, managing[0].UserId, db);
    }
}
