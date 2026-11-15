namespace Redix.Api.Auth;

/// <summary>A permission one user holds across the system, such as the admin's <see cref="Permission.ApproveProjects"/> (BR-29); one held in a project is a <see cref="Projects.ProjectPermission"/>.</summary>
public class SystemPermission : IAccessRow
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The user who holds it.</summary>
    public Guid UserId { get; init; }

    /// <summary>What the user may do everywhere.</summary>
    public Permission Permission { get; init; }
}
