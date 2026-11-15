namespace Redix.Api.Auth;

/// <summary>
/// A row a person's <see cref="UserAccess"/> is read from (redix-api #120 D3). Saving one makes <see cref="AccessChanges"/> forget that person's kept access, so the
/// change counts on their next request (BR-32).
/// </summary>
public interface IAccessRow
{
    /// <summary>The person whose access the row is part of.</summary>
    Guid UserId { get; }
}
