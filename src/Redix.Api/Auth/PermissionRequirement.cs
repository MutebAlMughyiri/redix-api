using Microsoft.AspNetCore.Authorization;

namespace Redix.Api.Auth;

/// <summary>What a route asks of its caller: this permission, in the project the route names, or everywhere when it names none.</summary>
/// <param name="Permission">The permission the route needs.</param>
public record PermissionRequirement(Permission Permission) : IAuthorizationRequirement;
