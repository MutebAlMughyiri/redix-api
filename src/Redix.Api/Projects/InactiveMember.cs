using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;

namespace Redix.Api.Projects;

/// <summary>
/// A person made inactive in one project by whoever manages its users (redix-api #126, D1): he keeps his permissions and entries there, and holds none of them until made active
/// again. No row means active, and each person has at most one row in a project.
/// </summary>
public class InactiveMember : IAccessRow
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The person inactive in it.</summary>
    public Guid UserId { get; init; }

    /// <summary>The projects <paramref name="userId"/> is inactive in.</summary>
    public static async Task<HashSet<Guid>> ProjectsOfAsync(Guid userId, RedixDbContext db) =>
        [.. await db.InactiveMembers.Where(row => row.UserId == userId).Select(row => row.ProjectId).ToListAsync()];

    /// <summary>Ends <paramref name="userId"/>'s being inactive in <paramref name="projectId"/>, if he is, with its History line of <paramref name="action"/> by <paramref name="by"/>; the caller saves.</summary>
    public static async Task EndAsync(Guid projectId, Guid userId, HistoryAction action, Guid by, DateTime now, RedixDbContext db)
    {
        foreach (var inactive in await db.InactiveMembers.Where(row => row.ProjectId == projectId && row.UserId == userId).ToListAsync())
        {
            db.InactiveMembers.Remove(inactive);
            db.AddHistory(inactive, action, by, now);
        }
    }
}
