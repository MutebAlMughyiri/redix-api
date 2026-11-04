namespace Redix.Api.Auth;

/// <summary>One permission held by one role (BR-32).</summary>
public class RolePermission
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The role that holds the permission.</summary>
    public Guid RoleId { get; init; }

    /// <summary>The permission it holds.</summary>
    public Permission Permission { get; init; }
}
