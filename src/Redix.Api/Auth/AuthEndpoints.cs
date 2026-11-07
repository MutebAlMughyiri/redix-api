using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;

namespace Redix.Api.Auth;

/// <summary>Opens, renews, secures and ends sessions: sign-in (UC-002), renewal with a device's refresh token, changing the password (UC-003), sign-out (UC-062), and a phone that uses biometrics (UC-011).</summary>
public static class AuthEndpoints
{
    /// <summary>Maps <c>POST /auth/sign-in</c> and <c>POST /auth/refresh</c>, open without a session, <c>POST /auth/change-password</c> and <c>POST /auth/sign-out</c>, open to any session, and <c>POST /auth/biometrics</c>.</summary>
    public static void MapAuth(this WebApplication app)
    {
        app.MapPost("/auth/sign-in", SignIn).AllowAnonymous();
        app.MapPost("/auth/refresh", Refresh).AllowAnonymous();
        app.MapPost("/auth/change-password", ChangePassword).RequireAuthorization(SessionPolicies.MayChangePassword);
        app.MapPost("/auth/sign-out", SignOut).RequireAuthorization(SessionPolicies.MayChangePassword);
        app.MapPost("/auth/biometrics", SetBiometrics).RequireAuthorization(SessionPolicies.Working);
    }

    /// <summary>
    /// Signs a user in, opens a session on a new device (FR-01, AC-01.1) and records the sign-in in the audit trail (UC-005). A wrong username, a wrong
    /// password and a disabled user all answer <c>invalidCredentials</c>; a locked account answers <c>accountLocked</c> (AC-01.2). A refusal is recorded against the user
    /// whose name was typed; an unknown username has nobody to record it against.
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
            return Refusal.Of(StatusCodes.Status401Unauthorized, nameof(SignInOutcome.InvalidCredentials));
        }

        var outcome = CheckPassword(user, request.Password, db, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            await db.SaveChangesAsync();
            return Refusal.Of(StatusCodes.Status401Unauthorized, outcome.ToString());
        }

        var device = new Device { UserId = user.Id };
        var refreshToken = device.RenewRefreshToken(now);
        db.Devices.Add(device);
        db.Audit(user, AuditAction.SignIn, user.Id, now);
        await db.SaveChangesAsync();
        return Results.Ok(await Session.OfAsync(user, device, accessTokens.Create(user, device.Id), refreshToken, db));
    }

    /// <summary>
    /// Renews a session with a device's refresh token, which it replaces. An unknown, revoked or expired token (unused for 12 hours on a phone without biometrics, NFR-09),
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
            return Refusal.Of(StatusCodes.Status401Unauthorized, "SessionExpired");
        }

        var refreshToken = signedIn.Device.RenewRefreshToken(now);
        await db.SaveChangesAsync();
        return Results.Ok(await Session.OfAsync(signedIn.User, signedIn.Device, accessTokens.Create(signedIn.User, signedIn.Device.Id), refreshToken, db));
    }

    /// <summary>
    /// Replaces the signed-in user's password (UC-003, AC-02.1), records it in the audit trail (UC-005), and signs out the user's other devices; this device
    /// stays signed in with a renewed session.
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

        var outcome = CheckPassword(user, request.CurrentPassword, db, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            await db.SaveChangesAsync();
            return Refusal.Of(StatusCodes.Status400BadRequest, outcome.ToString());
        }
        if (!User.IsAcceptableNewPassword(request.NewPassword, request.CurrentPassword))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "WeakPassword");
        }

        user.SetPassword(request.NewPassword, hasher);
        user.MustChangePassword = false;
        await db.Devices.Where(device => device.UserId == userId && device.Id != deviceId).ExecuteUpdateAsync(device => device.SetProperty(d => d.IsRevoked, true));
        var thisDevice = await db.Devices.SingleAsync(device => device.Id == deviceId);
        var refreshToken = thisDevice.RenewRefreshToken(now);
        db.Audit(user, AuditAction.ChangePassword, user.Id, now);
        await db.SaveChangesAsync();
        return Results.Ok(await Session.OfAsync(user, thisDevice, accessTokens.Create(user, thisDevice.Id), refreshToken, db));
    }

    /// <summary>Ends the session on this device, and only this one (UC-062), and records it in the audit trail.</summary>
    /// <param name="principal">The session to end: its user and device.</param>
    /// <param name="db">The database the user and device live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> SignOut(ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var deviceId = AccessTokens.DeviceIdOf(principal);
        (await db.Devices.SingleAsync(device => device.Id == deviceId)).IsRevoked = true;
        db.Audit(await db.Users.SingleAsync(user => user.Id == userId), AuditAction.SignOut, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static SignInOutcome CheckPassword(User user, string password, RedixDbContext db, IPasswordHasher<User> hasher, DateTime now)
    {
        var lockedUntil = user.LockedUntil;
        var outcome = user.SignIn(password, hasher, now);
        if (outcome != SignInOutcome.Success)
        {
            db.Audit(user, user.LockedUntil == lockedUntil ? AuditAction.SignInFailed : AuditAction.AccountLocked, user.Id, now);
        }
        return outcome;
    }

    /// <summary>
    /// Turns fingerprint or face on or off for this phone (UC-011, FR-35), and records it in the audit trail. The check runs on the phone; the server only remembers
    /// that this phone asks for it, so its session stops timing out. The token is renewed, so turning it off starts the 12 hours from now.
    /// </summary>
    /// <param name="request">Whether it is on.</param>
    /// <param name="principal">The signed-in session: its user and device.</param>
    /// <param name="db">The database the user and device live in.</param>
    /// <param name="accessTokens">Issues the renewed access token.</param>
    /// <param name="time">The current time, for the renewed tokens and the audit record.</param>
    public static async Task<IResult> SetBiometrics(SetBiometricsRequest request, ClaimsPrincipal principal, RedixDbContext db, AccessTokens accessTokens, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var userId = AccessTokens.UserIdOf(principal);
        var deviceId = AccessTokens.DeviceIdOf(principal);
        var user = await db.Users.SingleAsync(user => user.Id == userId);
        var device = await db.Devices.SingleAsync(device => device.Id == deviceId);
        device.UsesBiometrics = request.On;
        var refreshToken = device.RenewRefreshToken(now);
        db.Audit(user, request.On ? AuditAction.TurnOnBiometrics : AuditAction.TurnOffBiometrics, user.Id, now);
        await db.SaveChangesAsync();
        return Results.Ok(await Session.OfAsync(user, device, accessTokens.Create(user, device.Id), refreshToken, db));
    }
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

/// <summary>What the app sends to turn fingerprint or face on or off for this phone.</summary>
/// <param name="On">Whether it is on.</param>
public record SetBiometricsRequest(bool On);

/// <summary>An open session, returned by sign-in, by every renewal, by a password change, and by turning fingerprint or face on or off.</summary>
/// <param name="AccessToken">Sent with each request; valid for 15 minutes.</param>
/// <param name="RefreshToken">Renews the session; replaced at each renewal and expired after 12 hours unused, unless this phone uses biometrics.</param>
/// <param name="MustChangePassword">True while the password is temporary and must be changed before anything else.</param>
/// <param name="Permissions">What the user holds everywhere; the app shows what they allow. What they hold in a project comes with the project.</param>
/// <param name="DefaultProjectId">The project sign-in opens directly, skipping the list (AC-37.1); empty when none, or when they can no longer read it (AC-37.2).</param>
/// <param name="UsesBiometrics">True when this phone asks for a fingerprint or face on opening (UC-011).</param>
/// <param name="UserId">Who this is, so that a phone's unsent entries go only with the person who entered them (UC-029).</param>
public record Session(string AccessToken, string RefreshToken, bool MustChangePassword, List<Permission> Permissions, string DefaultProjectId, bool UsesBiometrics, Guid UserId)
{
    /// <summary>The session of <paramref name="user"/> on <paramref name="device"/> with these tokens. One check covers every way of losing access to the default project.</summary>
    public static async Task<Session> OfAsync(User user, Device device, string accessToken, string refreshToken, RedixDbContext db)
    {
        var access = await UserAccess.OfAsync(user.Id, db);
        var chosen = await db.UserDefaultProjects.Where(row => row.UserId == user.Id).Select(row => row.ProjectId).SingleOrDefaultAsync();
        var defaultProjectId = access.In(chosen).Contains(Permission.ReadProject) ? chosen.ToString() : "";
        return new(accessToken, refreshToken, user.MustChangePassword, access.Everywhere.ToList(), defaultProjectId, device.UsesBiometrics, user.Id);
    }
}
