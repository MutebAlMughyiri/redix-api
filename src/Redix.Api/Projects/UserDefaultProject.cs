namespace Redix.Api.Projects;

/// <summary>The project a user chose to land on (FR-37). No row means no default, and each user has at most one: the user is the row's key (findings row 216).</summary>
public class UserDefaultProject
{
    /// <summary>The user who chose it.</summary>
    public Guid UserId { get; init; }

    /// <summary>The project they chose.</summary>
    public Guid ProjectId { get; set; }
}
