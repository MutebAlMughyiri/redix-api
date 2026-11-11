using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>One permission one user holds in one project (FR-36), ticked by whoever manages its users. It counts only in its project (BR-33); holding <see cref="Permission.ReadProject"/> is being in the project, and no row is not being in it.</summary>
public class ProjectPermission
{
    /// <summary>What can be ticked in a project: every permission but the system's own.</summary>
    public static IReadOnlyList<Permission> InAProject { get; } = [.. Enum.GetValues<Permission>().Where(permission => permission != Permission.ApproveProjects)];

    /// <summary>What whoever starts a project holds in it: every permission of a project but seeing chosen partners' capital, which seeing all of it excludes (redix-api #106).</summary>
    public static IReadOnlyList<Permission> OfWhoeverStarts { get; } = [.. InAProject.Where(permission => permission != Permission.SeeChosenCapital)];

    /// <summary>The permission of a project named <paramref name="name"/>, as the app names it in a route, whatever its case; false for any other name, the system's own included.</summary>
    public static bool TryOf(string name, out Permission permission) => Enum.TryParse(name, ignoreCase: true, out permission) && InAProject.Contains(permission);

    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The user who holds it.</summary>
    public Guid UserId { get; init; }

    /// <summary>What the user may do in this project.</summary>
    public Permission Permission { get; init; }
}
