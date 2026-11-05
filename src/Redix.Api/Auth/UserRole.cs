namespace Redix.Api.Auth;

/// <summary>A system-wide role held by a user, such as the admin's; a role in one project is a <see cref="Projects.ProjectMember"/>.</summary>
public class UserRole
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The user who holds the role.</summary>
    public Guid UserId { get; init; }

    /// <summary>The role.</summary>
    public Guid RoleId { get; init; }
}
