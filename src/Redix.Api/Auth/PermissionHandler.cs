using Microsoft.AspNetCore.Authorization;

namespace Redix.Api.Auth;

/// <summary>
/// Answers each <see cref="PermissionRequirement"/> from the person's one access (FR-03, NFR-08, redix-api #120), so the server guards what a role may do whatever the app
/// shows. A refusal writes one log line naming the person, the project, the route and the permission it needed (#120 D6).
/// </summary>
/// <param name="db">The database the permissions live in.</param>
/// <param name="logger">Where a refusal is written.</param>
public partial class PermissionHandler(RedixDbContext db, ILogger<PermissionHandler> logger) : AuthorizationHandler<PermissionRequirement, HttpContext>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement, HttpContext resource)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return;
        }

        var userId = AccessTokens.UserIdOf(context.User);
        var projectId = resource.GetRouteValue("projectId") is string id ? Guid.Parse(id) : Guid.Empty;
        var access = await UserAccess.OfAsync(userId, db);
        if (access.In(projectId).Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }
        Refused(userId, projectId, resource.Request.Path, requirement.Permission);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Refused {UserId} in project {ProjectId} on {Path}: needs {Permission}")]
    private partial void Refused(Guid userId, Guid projectId, string path, Permission permission);
}
