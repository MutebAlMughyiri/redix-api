using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>The rule that every project keeps an active person who manages its users (BR-29): whoever holds <see cref="Permission.ManageUsers"/> there.</summary>
public static class ProjectManagers
{
    private static IQueryable<ProjectPermission> Active(RedixDbContext db) =>
        db.ProjectPermissions.Where(held => held.Permission == Permission.ManageUsers && db.Users.Any(user => user.Id == held.UserId && user.IsActive));

    /// <summary>Whether taking <paramref name="taken"/>, rows of one person in one project, would leave that project with no active person managing its users.</summary>
    public static async Task<bool> WouldLoseTheLastAsync(List<ProjectPermission> taken, RedixDbContext db)
    {
        var managing = taken.Where(held => held.Permission == Permission.ManageUsers).ToList();
        if (managing.Count == 0)
        {
            return false;
        }
        var (projectId, userId) = (managing[0].ProjectId, managing[0].UserId);
        var managers = Active(db).Where(held => held.ProjectId == projectId);
        return await managers.AnyAsync(held => held.UserId == userId) && !await managers.AnyAsync(held => held.UserId != userId);
    }

    /// <summary>Whether disabling <paramref name="userId"/> would leave any project with no active person managing its users.</summary>
    public static Task<bool> IsTheLastOfAProjectAsync(Guid userId, RedixDbContext db)
    {
        var managers = Active(db);
        return managers.Where(manager => manager.UserId == userId).AnyAsync(theirs => !managers.Any(other => other.ProjectId == theirs.ProjectId && other.UserId != userId));
    }
}
