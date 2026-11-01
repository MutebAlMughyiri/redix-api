using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>Opens, renews and secures sessions: sign-in (UC-002), renewal with a device's refresh token, and changing the password (UC-003).</summary>
public static class AuthEndpoints
{
    /// <summary>Maps <c>POST /auth/sign-in</c> and <c>POST /auth/refresh</c>, open without a session, and <c>POST /auth/change-password</c>, open to any session.</summary>
    public static void MapAuth(this WebApplication app)
    {
        app.MapPost("/auth/sign-in", SignIn).AllowAnonymous();
        app.MapPost("/auth/refresh", Refresh).AllowAnonymous();
        app.MapPost("/auth/change-password", ChangePassword).RequireAuthorization(SessionPolicies.MayChangePassword);
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
            return Refused(StatusCodes.Status401Unauthorized, nameof(SignInOutcome.InvalidCredentials));
        }

        var outcome = user.SignIn(request.Password, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            await db.SaveChangesAsync();
            return Refused(StatusCodes.Status401Unauthorized, outcome.ToString());
        }

        var device = new Device { UserId = user.Id };
        var refreshToken = device.RenewRefreshToken(now);
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return Results.Ok(new Session(accessTokens.Create(user, device.Id), refreshToken, user.MustChangePassword));
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
            return Refused(StatusCodes.Status401Unauthorized, "SessionExpired");
        }

        var refreshToken = signedIn.Device.RenewRefreshToken(now);
        await db.SaveChangesAsync();
        return Results.Ok(new Session(accessTokens.Create(signedIn.User, signedIn.Device.Id), refreshToken, signedIn.User.MustChangePassword));
    }

    /// <summary>
    /// Replaces the signed-in user's password (UC-003, AC-02.1) and signs out the user's other devices; this device stays signed in with a renewed session.
    /// A wrong current password answers <c>invalidCredentials</c> and counts toward the lockout (<c>accountLocked</c>); a new password shorter than
    /// <see cref="User.MinimumPasswordLength"/> or equal to the current one answers <c>weakPassword</c>.
    /// </summary>
    /// <param name="request">The current password and the new one.</param>
    /// <param name="principal">The signed-in session: its user and device.</param>
    /// <param name="db">The database the user and devices live in.</param>
    /// <param name="hasher">Checks the current password and hashes the new one.</param>
    /// <param name="accessTokens">Issues the renewed access token, which no longer asks for a password change.</param>
    /// <param name="time">The current time, for the lockout and the renewed tokens.</param>
    public static async Task<IResult> ChangePassword(ChangePasswordRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher,
        AccessTokens accessTokens, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var userId = AccessTokens.UserIdOf(principal);
        var deviceId = AccessTokens.DeviceIdOf(principal);
        var user = await db.Users.SingleAsync(user => user.Id == userId);

        var outcome = user.SignIn(request.CurrentPassword, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            await db.SaveChangesAsync();
            return Refused(StatusCodes.Status400BadRequest, outcome.ToString());
        }
        if (!User.IsAcceptableNewPassword(request.NewPassword, request.CurrentPassword))
        {
            return Refused(StatusCodes.Status400BadRequest, "WeakPassword");
        }

        user.SetPassword(request.NewPassword, hasher);
        user.MustChangePassword = false;
        await db.Devices.Where(device => device.UserId == userId && device.Id != deviceId).ExecuteUpdateAsync(device => device.SetProperty(d => d.IsRevoked, true));
        var thisDevice = await db.Devices.SingleAsync(device => device.Id == deviceId);
        var refreshToken = thisDevice.RenewRefreshToken(now);
        await db.SaveChangesAsync();
        return Results.Ok(new Session(accessTokens.Create(user, thisDevice.Id), refreshToken, user.MustChangePassword));
    }

    private static IResult Refused(int status, string reason) => Results.Problem(
        statusCode: status,
        extensions: new Dictionary<string, object?> { ["code"] = JsonNamingPolicy.CamelCase.ConvertName(reason) });
}

/// <summary>What the sign-in screen sends.</summary>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="Password">The password as typed.</param>
public record SignInRequest(string Username, string Password);

/// <summary>What the app sends to renew its session.</summary>
/// <param name="RefreshToken">The device's current refresh token.</param>
public record RefreshRequest(string RefreshToken);

/// <summary>What the change-password screen sends.</summary>
/// <param name="CurrentPassword">The password the user signs in with today.</param>
/// <param name="NewPassword">The password that replaces it.</param>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>An open session, returned by sign-in, by every renewal, and by a password change.</summary>
/// <param name="AccessToken">Sent with each request; valid for 15 minutes.</param>
/// <param name="RefreshToken">Renews the session; replaced at each renewal and expired after 12 hours unused.</param>
/// <param name="MustChangePassword">True while the password is temporary and must be changed before anything else.</param>
public record Session(string AccessToken, string RefreshToken, bool MustChangePassword);
