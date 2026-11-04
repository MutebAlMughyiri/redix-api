namespace Redix.Api.Auth;

/// <summary>
/// What a route can ask of its caller (BRS section 2). Which role holds each one is data, in <see cref="RolePermission"/>; a new one is code, because a route has
/// to name it. Each use case adds the ones its routes need.
/// </summary>
public enum Permission
{
    /// <summary>Starts a project, becoming its first project manager (BR-29).</summary>
    StartProject,

    /// <summary>Sees every login, and edits, resets, disables or enables any of them (BR-29).</summary>
    ManageLogins,

    /// <summary>Adds people to one project and gives their roles in it (FR-02, BR-29).</summary>
    ManageUsers,

    /// <summary>Sees one project and everything in it (BR-31).</summary>
    ReadProject,

    /// <summary>Sees every partner's capital in one project, not only their own (BR-31); UC-044 reads it.</summary>
    SeeAllCapital,

    /// <summary>Adds units to one project and closes it (FR-04).</summary>
    ManageProject,
}
