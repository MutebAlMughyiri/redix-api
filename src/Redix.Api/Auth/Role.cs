namespace Redix.Api.Auth;

/// <summary>A role, which is data so the business can change it with no release (BR-32). What it may do is its <see cref="RolePermission"/> rows.</summary>
public class Role
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The key of the word that names this role.</summary>
    public required string WordKey { get; init; }

    /// <summary>Where this role counts.</summary>
    public RoleScope Scope { get; init; }

    /// <summary>This role's place in its list; a new person starts at the last project role.</summary>
    public int SortOrder { get; init; }
}
