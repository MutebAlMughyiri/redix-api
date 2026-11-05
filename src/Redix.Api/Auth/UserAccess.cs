using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>What one user may do, read for each request so a change counts at once (BR-32). Nothing in the code tells one role from another.</summary>
/// <param name="everywhere">What the user's system-wide roles hold.</param>
/// <param name="byProject">What the user's roles in each project hold.</param>
public class UserAccess(List<Permission> everywhere, ILookup<Guid, Permission> byProject)
{
    /// <summary>What the user holds wherever they are.</summary>
    public IReadOnlyList<Permission> Everywhere => everywhere;

    /// <summary>What the user holds in <paramref name="projectId"/>. A role counts only in the project it was given for, so one project never opens another (BR-33), and only once that project is approved (BR-34).</summary>
    public List<Permission> In(Guid projectId) => everywhere.Union(byProject[projectId]).ToList();

    /// <summary>Reads what <paramref name="userId"/> holds.</summary>
    public static async Task<UserAccess> OfAsync(Guid userId, RedixDbContext db)
    {
        var everywhere = await db.UserRoles.Where(row => row.UserId == userId)
            .Join(db.RolePermissions, row => row.RoleId, held => held.RoleId, (_, held) => held.Permission).ToListAsync();
        var inProjects = await db.ProjectMembers
            .Where(member => member.UserId == userId
                && db.Projects.Any(project => project.Id == member.ProjectId && (project.Status == Projects.ProjectStatus.Running || project.Status == Projects.ProjectStatus.Finished)))
            .Join(db.RolePermissions, member => member.RoleId, held => held.RoleId, (member, held) => new { member.ProjectId, held.Permission }).ToListAsync();
        return new UserAccess(everywhere, inProjects.ToLookup(row => row.ProjectId, row => row.Permission));
    }
}
