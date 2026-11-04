namespace Redix.Api.Projects;

/// <summary>
/// A user's place in one project (FR-36). An owner has none: being a financial supervisor in every project is what being an owner means (BR-29),
/// so an owner's role cannot be lost and a new project needs nothing added for owners.
/// </summary>
public class ProjectMember
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The user who works on it.</summary>
    public Guid UserId { get; init; }

    /// <summary>What the user is in this project.</summary>
    public ProjectRole Role { get; set; }
}
