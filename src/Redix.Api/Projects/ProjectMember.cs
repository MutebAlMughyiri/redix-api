namespace Redix.Api.Projects;

/// <summary>One role a user holds in one project (FR-36). A role counts only in its project (BR-33), and having no row is not being in the project.</summary>
public class ProjectMember
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The user who works on it.</summary>
    public Guid UserId { get; init; }

    /// <summary>What the user is in this project.</summary>
    public Guid RoleId { get; init; }
}
