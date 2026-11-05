namespace Redix.Api.Projects;

/// <summary>The project a user chose to land on (FR-37). No row means no default, and each user has at most one.</summary>
public class UserDefaultProject
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The user who chose it.</summary>
    public Guid UserId { get; init; }

    /// <summary>The project they chose.</summary>
    public Guid ProjectId { get; set; }
}
