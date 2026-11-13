using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>The policies every endpoint falls under besides its permission, and the check that ends a session at its next request.</summary>
public static class SessionPolicies
{
    /// <summary>The name of the policy that lets a session with a temporary password through: changing it is all such a session may do.</summary>
    public const string MayChangePassword = nameof(MayChangePassword);

    /// <summary>The fallback for every endpoint not open to all: signed in, with no temporary password left to change (AC-02.1).</summary>
    public static readonly AuthorizationPolicy Working = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(AccessTokens.MustChangePasswordClaim, "false")
        .Build();

    /// <summary>
    /// Fails the request's sign-in unless the access token's device is still open, so a revoked device is refused at the next request instead of when the 15-minute token expires.
    /// Someone inactive in a project keeps his session: that project's routes refuse him through <see cref="UserAccess"/> (redix-api #126).
    /// </summary>
    public static async Task CheckSessionIsOpen(TokenValidatedContext context)
    {
        var principal = context.Principal ?? throw new InvalidOperationException("A validated token has no principal.");
        var userId = AccessTokens.UserIdOf(principal);
        var deviceId = AccessTokens.DeviceIdOf(principal);
        var db = context.HttpContext.RequestServices.GetRequiredService<RedixDbContext>();
        var isOpen = await db.Devices.AnyAsync(device => device.Id == deviceId && device.UserId == userId && !device.IsRevoked);
        if (!isOpen)
        {
            context.Fail("The session has ended.");
        }
    }
}
