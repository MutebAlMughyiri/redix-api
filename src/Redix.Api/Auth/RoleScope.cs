namespace Redix.Api.Auth;

/// <summary>Where a role counts (BR-33).</summary>
public enum RoleScope
{
    /// <summary>Across the whole system, through <see cref="UserRole"/>.</summary>
    System,

    /// <summary>In one project only, through <see cref="Projects.ProjectMember"/>.</summary>
    Project,
}
