using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Projects;

/// <summary>
/// What a person is called in one project, typed by whoever manages its users (Muteb, 9 Nov 2026): a label only, shown under the project's name. What the person may do there is
/// still their permissions alone. No row means no role name, and each person has at most one in a project.
/// </summary>
public class ProjectRoleName
{
    /// <summary>The longest role name.</summary>
    public const int MaxLength = 50;

    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The person.</summary>
    public Guid UserId { get; init; }

    /// <summary>What they are called there; empty when nothing was typed.</summary>
    public string RoleName { get; set; } = "";

    /// <summary>The role names <paramref name="userId"/> holds, by project.</summary>
    public static Task<Dictionary<Guid, string>> OfUserAsync(Guid userId, RedixDbContext db) =>
        db.ProjectRoleNames.Where(row => row.UserId == userId).ToDictionaryAsync(row => row.ProjectId, row => row.RoleName);
}
