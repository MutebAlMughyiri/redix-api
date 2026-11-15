using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>
/// What one person may do, everywhere and in each project, and which partners they were chosen to see (redix-api #120 R1). It is read at most once per request and kept
/// between requests by <see cref="AccessCache"/>, which <see cref="AccessChanges"/> keeps true, so a change still counts on the person's next request (BR-32). Nothing in it
/// changes once read, so one kept access serves every request at once.
/// </summary>
/// <param name="everywhere">What the person holds across the system.</param>
/// <param name="byProject">What the person holds in each project.</param>
/// <param name="chosenPartners">The partners chosen for the person in each project (redix-api #106).</param>
public class UserAccess(List<Permission> everywhere, ILookup<Guid, Permission> byProject, ILookup<Guid, Guid> chosenPartners)
{
    /// <summary>What the person holds wherever they are.</summary>
    public IReadOnlyList<Permission> Everywhere => everywhere;

    /// <summary>What the person holds in <paramref name="projectId"/>. A permission counts only in the project it was ticked in, so one project never opens another (BR-33), and only once that project is approved (BR-34).</summary>
    public List<Permission> In(Guid projectId) => everywhere.Union(byProject[projectId]).ToList();

    /// <summary>The partners of <paramref name="projectId"/> chosen for the person; empty when none are.</summary>
    public List<Guid> ChosenIn(Guid projectId) => chosenPartners[projectId].ToList();

    /// <summary>What <paramref name="userId"/> holds: the kept access, or one read from <paramref name="db"/> (redix-api #120 D1).</summary>
    public static Task<UserAccess> OfAsync(Guid userId, RedixDbContext db) => db.Access.OfAsync(userId, () => ReadAsync(userId, db));

    /// <summary>
    /// Reads what <paramref name="userId"/> holds. A project he is inactive in gives him nothing, so every route of it refuses him as for a project he is not in (redix-api #126, D1),
    /// and it is never his default (AC-37.2).
    /// </summary>
    private static async Task<UserAccess> ReadAsync(Guid userId, RedixDbContext db)
    {
        var everywhere = await db.SystemPermissions.Where(row => row.UserId == userId).Select(row => row.Permission).ToListAsync();
        var inProjects = await db.ProjectPermissions
            .Where(row => row.UserId == userId
                && db.Projects.Any(project => project.Id == row.ProjectId && Projects.Project.ApprovedStatuses.Contains(project.Status))
                && !db.InactiveMembers.Any(inactive => inactive.ProjectId == row.ProjectId && inactive.UserId == userId))
            .Select(row => new { row.ProjectId, row.Permission }).ToListAsync();
        var chosen = await db.ChosenPartners.Where(row => row.UserId == userId).Select(row => new { row.ProjectId, row.PartnerId }).ToListAsync();
        return new UserAccess(everywhere, inProjects.ToLookup(row => row.ProjectId, row => row.Permission), chosen.ToLookup(row => row.ProjectId, row => row.PartnerId));
    }
}
