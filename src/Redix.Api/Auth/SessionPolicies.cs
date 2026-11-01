using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>Who may call what: the policies every endpoint falls under, and the check that ends a session at its next request.</summary>
public static class SessionPolicies
{
    /// <summary>The name of the policy that lets a session with a temporary password through: changing it is all such a session may do.</summary>
    public const string MayChangePassword = nameof(MayChangePassword);

    /// <summary>The name of the policy for what only an owner may do: <see cref="Working"/>, by a user who is an owner (BR-29).</summary>
    public const string Owner = nameof(Owner);

    /// <summary>The fallback for every endpoint not open to all: signed in, with no temporary password left to change (AC-02.1).</summary>
    public static readonly AuthorizationPolicy Working = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(AccessTokens.MustChangePasswordClaim, "false")
        .Build();

    /// <summary>
    /// Fails the request's sign-in unless the access token's device is still open and its user still active, so a revoked device or a
    /// disabled user (AC-01.3) is refused at the next request instead of when the 15-minute token expires.
    /// </summary>
    public static async Task CheckSessionIsOpen(TokenValidatedContext context)
    {
        var principal = context.Principal ?? throw new InvalidOperationException("A validated token has no principal.");
        var userId = AccessTokens.UserIdOf(principal);
        var deviceId = AccessTokens.DeviceIdOf(principal);
        var db = context.HttpContext.RequestServices.GetRequiredService<RedixDbContext>();
        var isOpen = await db.Devices.AnyAsync(device => device.Id == deviceId && device.UserId == userId && !device.IsRevoked
            && db.Users.Any(user => user.Id == userId && user.IsActive));
        if (!isOpen)
        {
            context.Fail("The session has ended.");
        }
    }
}
