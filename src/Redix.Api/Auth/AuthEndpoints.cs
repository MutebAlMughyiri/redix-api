using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>Opens and renews sessions: sign-in with a username and password (UC-002), and renewal with a device's refresh token.</summary>
public static class AuthEndpoints
{
    /// <summary>Maps <c>POST /auth/sign-in</c> and <c>POST /auth/refresh</c>, both open without a session.</summary>
    public static void MapAuth(this WebApplication app)
    {
        app.MapPost("/auth/sign-in", SignIn);
        app.MapPost("/auth/refresh", Refresh);
    }

    /// <summary>
    /// Signs a user in and opens a session on a new device (FR-01, AC-01.1). A wrong username, a wrong password and a disabled user all answer
    /// <c>invalidCredentials</c>; a locked account answers <c>accountLocked</c> (AC-01.2).
    /// </summary>
    /// <param name="request">The username and password typed on the sign-in screen.</param>
    /// <param name="db">The database the user and the new device live in.</param>
    /// <param name="hasher">Checks the password against its hash.</param>
    /// <param name="accessTokens">Issues the session's access token.</param>
    /// <param name="time">The current time, for the lockout and the token's expiry.</param>
    public static async Task<IResult> SignIn(SignInRequest request, RedixDbContext db, IPasswordHasher<User> hasher, AccessTokens accessTokens, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var user = await db.Users.SingleOrDefaultAsync(user => user.Username == request.Username);
        if (user is null)
        {
            return Refused(nameof(SignInOutcome.InvalidCredentials));
        }

        var outcome = user.SignIn(request.Password, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            await db.SaveChangesAsync();
            return Refused(outcome.ToString());
        }

        var device = new Device { UserId = user.Id };
        var refreshToken = device.RenewRefreshToken(now);
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return Results.Ok(new Session(accessTokens.Create(user), refreshToken, user.MustChangePassword));
    }

    /// <summary>
    /// Renews a session with a device's refresh token, which it replaces. An unknown, revoked or expired token (unused for 12 hours, NFR-09),
    /// or a user disabled since (AC-01.3), answers <c>sessionExpired</c>.
    /// </summary>
    /// <param name="request">The device's current refresh token.</param>
    /// <param name="db">The database the device and its user live in.</param>
    /// <param name="accessTokens">Issues the renewed access token.</param>
    /// <param name="time">The current time, for the refresh token's expiry.</param>
    public static async Task<IResult> Refresh(RefreshRequest request, RedixDbContext db, AccessTokens accessTokens, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var refreshTokenHash = Device.HashOf(request.RefreshToken);
        var signedIn = await db.Devices
            .Where(device => device.RefreshTokenHash == refreshTokenHash)
            .Join(db.Users, device => device.UserId, user => user.Id, (device, user) => new { Device = device, User = user })
            .SingleOrDefaultAsync();
        if (signedIn is null || !signedIn.Device.CanRefresh(now) || !signedIn.User.IsActive)
        {
            return Refused("SessionExpired");
        }

        var refreshToken = signedIn.Device.RenewRefreshToken(now);
        await db.SaveChangesAsync();
        return Results.Ok(new Session(accessTokens.Create(signedIn.User), refreshToken, signedIn.User.MustChangePassword));
    }

    private static IResult Refused(string reason) => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        extensions: new Dictionary<string, object?> { ["code"] = JsonNamingPolicy.CamelCase.ConvertName(reason) });
}

/// <summary>What the sign-in screen sends.</summary>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="Password">The password as typed.</param>
public record SignInRequest(string Username, string Password);

/// <summary>What the app sends to renew its session.</summary>
/// <param name="RefreshToken">The device's current refresh token.</param>
public record RefreshRequest(string RefreshToken);

/// <summary>An open session, returned by sign-in and by every renewal.</summary>
/// <param name="AccessToken">Sent with each request; valid for 15 minutes.</param>
/// <param name="RefreshToken">Renews the session; replaced at each renewal and expired after 12 hours unused.</param>
/// <param name="MustChangePassword">True while the password is temporary and must be changed before anything else.</param>
public record Session(string AccessToken, string RefreshToken, bool MustChangePassword);
