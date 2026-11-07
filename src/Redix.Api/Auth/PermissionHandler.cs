using Microsoft.AspNetCore.Authorization;

namespace Redix.Api.Auth;

/// <summary>Answers each <see cref="PermissionRequirement"/> from the database (FR-03, NFR-08), so the server guards what a role may do whatever the app shows.</summary>
/// <param name="db">The database the permissions live in.</param>
public class PermissionHandler(RedixDbContext db) : AuthorizationHandler<PermissionRequirement, HttpContext>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement, HttpContext resource)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return;
        }

        var projectId = resource.GetRouteValue("projectId") is string id ? Guid.Parse(id) : Guid.Empty;
        var access = await UserAccess.OfAsync(AccessTokens.UserIdOf(context.User), db);
        if (access.In(projectId).Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
