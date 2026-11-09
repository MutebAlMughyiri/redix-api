using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;

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

    /// <summary>Why <paramref name="roleName"/>, already trimmed, cannot be kept: <c>InvalidRoleName</c> past <see cref="MaxLength"/>; empty when it can.</summary>
    public static string RefusalOf(string roleName) => roleName.Length > MaxLength ? "InvalidRoleName" : "";

    /// <summary>
    /// Names what <paramref name="userId"/> is called in <paramref name="projectId"/>, with its audit record by <paramref name="by"/>: a new name adds the row, another name changes
    /// it, an empty one removes it, and the same name changes nothing. The caller saves.
    /// </summary>
    public static async Task NameAsync(Guid projectId, Guid userId, string roleName, Guid by, DateTime now, RedixDbContext db)
    {
        var named = await db.ProjectRoleNames.SingleOrDefaultAsync(row => row.ProjectId == projectId && row.UserId == userId) ?? new ProjectRoleName { ProjectId = projectId, UserId = userId };
        if (named.RoleName == roleName)
        {
            return;
        }
        var action = (named.RoleName.Length, roleName.Length) switch
        {
            (0, _) => AuditAction.Create,
            (_, 0) => AuditAction.Delete,
            _ => AuditAction.Update,
        };
        named.RoleName = roleName;
        if (action == AuditAction.Create)
        {
            db.ProjectRoleNames.Add(named);
        }
        else if (action == AuditAction.Delete)
        {
            db.ProjectRoleNames.Remove(named);
        }
        db.Audit(named, action, by, now);
    }
}
