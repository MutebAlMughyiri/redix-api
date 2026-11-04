using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>The rule that every project keeps an active project manager (BR-29). A manager is whoever holds <see cref="Permission.ManageUsers"/>, so it follows the data if roles change.</summary>
public static class ProjectManagers
{
    private static IQueryable<ProjectMember> Active(RedixDbContext db) =>
        db.ProjectMembers.Where(member => db.RolePermissions.Any(held => held.RoleId == member.RoleId && held.Permission == Permission.ManageUsers)
            && db.Users.Any(user => user.Id == member.UserId && user.IsActive));

    /// <summary>Whether taking <paramref name="member"/>'s role would leave its project with no active manager.</summary>
    public static async Task<bool> WouldLoseTheLastAsync(ProjectMember member, RedixDbContext db)
    {
        var managers = Active(db);
        return await managers.AnyAsync(manager => manager.Id == member.Id) && !await managers.AnyAsync(other => other.ProjectId == member.ProjectId && other.Id != member.Id);
    }

    /// <summary>Whether disabling <paramref name="userId"/> would leave any project with no active manager.</summary>
    public static Task<bool> IsTheLastOfAProjectAsync(Guid userId, RedixDbContext db)
    {
        var managers = Active(db);
        return managers.Where(manager => manager.UserId == userId).AnyAsync(theirs => !managers.Any(other => other.ProjectId == theirs.ProjectId && other.UserId != userId));
    }

    /// <summary>The role that makes someone a project's manager, given to whoever starts it: the first project role holding <see cref="Permission.ManageUsers"/>.</summary>
    public static Task<Guid> RoleIdAsync(RedixDbContext db) =>
        db.Roles.Where(role => role.Scope == RoleScope.Project && db.RolePermissions.Any(held => held.RoleId == role.Id && held.Permission == Permission.ManageUsers))
            .OrderBy(role => role.SortOrder).Select(role => role.Id).FirstAsync();
}
