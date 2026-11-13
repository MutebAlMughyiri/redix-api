using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>What one user may do, read for each request so a change counts at once (BR-32).</summary>
/// <param name="everywhere">What the user holds across the system.</param>
/// <param name="byProject">What the user holds in each project.</param>
public class UserAccess(List<Permission> everywhere, ILookup<Guid, Permission> byProject)
{
    /// <summary>What the user holds wherever they are.</summary>
    public IReadOnlyList<Permission> Everywhere => everywhere;

    /// <summary>What the user holds in <paramref name="projectId"/>. A permission counts only in the project it was ticked in, so one project never opens another (BR-33), and only once that project is approved (BR-34).</summary>
    public List<Permission> In(Guid projectId) => everywhere.Union(byProject[projectId]).ToList();

    /// <summary>
    /// Reads what <paramref name="userId"/> holds. A project he is inactive in gives him nothing, so every route of it refuses him as for a project he is not in (redix-api #126, D1),
    /// and it is never his default (AC-37.2).
    /// </summary>
    public static async Task<UserAccess> OfAsync(Guid userId, RedixDbContext db)
    {
        var everywhere = await db.SystemPermissions.Where(row => row.UserId == userId).Select(row => row.Permission).ToListAsync();
        var inProjects = await db.ProjectPermissions
            .Where(row => row.UserId == userId
                && db.Projects.Any(project => project.Id == row.ProjectId && Projects.Project.ApprovedStatuses.Contains(project.Status))
                && !db.InactiveMembers.Any(inactive => inactive.ProjectId == row.ProjectId && inactive.UserId == userId))
            .Select(row => new { row.ProjectId, row.Permission }).ToListAsync();
        return new UserAccess(everywhere, inProjects.ToLookup(row => row.ProjectId, row => row.Permission));
    }
}
