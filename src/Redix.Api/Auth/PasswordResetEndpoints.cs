using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Email;
using Redix.Api.History;

namespace Redix.Api.Auth;

/// <summary>
/// A forgotten password, reset by the person himself from the sign-in screen (redix-api #127, AC-02.6): a code to the account's email, then the code and a new password; the admin's the
/// same way (R10). Both routes are open without a session. Asking is limited per caller and per login typed (D4); typing a code is bounded by the code's own
/// <see cref="EmailCode.TriesAllowed"/> tries alone (Muteb, 16 Nov 2026). Neither answer says whether an account or its email exists (R5, AC-02.7).
/// </summary>
public static class PasswordResetEndpoints
{
    /// <summary>Maps <c>POST /auth/forgot-password</c>, limited by <see cref="EmailCode.AskingLimit"/>, and <c>POST /auth/reset-with-code</c>, both open with no session.</summary>
    public static void MapPasswordReset(this WebApplication app)
    {
        app.MapPost("/auth/forgot-password", ForgotPassword).AllowAnonymous().RequireRateLimiting(EmailCode.AskingLimit);
        app.MapPost("/auth/reset-with-code", ResetWithCode).AllowAnonymous();
    }

    /// <summary>
    /// Answers <c>202</c> at once whatever was typed, and sends the code only once the answer has left (<see cref="SendCodeAsync"/>), so neither the answer nor the time it takes says
    /// whether an account or its email exists (R5, AC-02.7, G1). One login typed more than <see cref="EmailCode.AsksPerLoginAnHour"/> times in an hour, whatever its case and whether
    /// or not it names an account, answers <c>429</c>, as asking too often from one caller does (D4, AC-02.8).
    /// </summary>
    /// <param name="request">The username or email typed.</param>
    /// <param name="response">The answer, after which the code is sent.</param>
    /// <param name="askingPerLogin">The limit on asking per login typed.</param>
    /// <param name="db">The database the account and its code live in.</param>
    /// <param name="hasher">Hashes the code, which is never kept.</param>
    /// <param name="sender">Sends the email.</param>
    /// <param name="time">The current time, for the code's fifteen minutes.</param>
    public static IResult ForgotPassword(ForgotPasswordRequest request, HttpResponse response, [FromKeyedServices(EmailCode.AskingLimit)] PartitionedRateLimiter<string> askingPerLogin, RedixDbContext db, IPasswordHasher<User> hasher, EmailSender sender, TimeProvider time)
    {
        using var asking = askingPerLogin.AttemptAcquire(request.Login.Trim().ToUpperInvariant());
        if (!asking.IsAcquired)
        {
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);
        }
        var language = CultureInfo.CurrentUICulture;
        response.OnCompleted(() => SendCodeAsync(request.Login, language, db, hasher, sender, time));
        return Results.Accepted();
    }

    /// <summary>
    /// Sends a six-digit code to the email of the account <paramref name="login"/> names by its username or its email (D5, AC-02.6), ending its earlier one (D3), in
    /// <paramref name="language"/>, the one the phone asked in, which the request no longer sets once its answer has left (AC-02.10). An unknown account, and one with no email yet,
    /// get nothing (G1).
    /// </summary>
    private static async Task SendCodeAsync(string login, CultureInfo language, RedixDbContext db, IPasswordHasher<User> hasher, EmailSender sender, TimeProvider time)
    {
        CultureInfo.CurrentUICulture = language;
        var user = await AccountOfAsync(login, db);
        if (user.Email.Length == 0)
        {
            return;
        }
        var code = await EmailCode.MakeAsync(user, EmailCodePurpose.ResetPassword, user.Email, hasher, time.GetUtcNow().UtcDateTime, db);
        await db.SaveChangesAsync();
        await sender.SendAsync(user.Email, "email.resetSubject", "email.resetBody", code, db);
    }

    /// <summary>
    /// Replaces the password of the account whose username or email was typed with the new one once its code is right (AC-02.6): the code is spent, a lock from wrong passwords ends, every
    /// device is signed out (R7, AC-02.9) and the password is not temporary, with its History line. A code wrong, expired, spent, out of tries or never sent answers <c>codeWrong</c>, as an
    /// unknown account does (R5); a new password <see cref="PasswordPolicy"/> refuses answers <c>weakPassword</c>, and the code stays unspent.
    /// </summary>
    /// <param name="request">The username or email, the code and the new password.</param>
    /// <param name="db">The database the account, its code and its devices live in.</param>
    /// <param name="hasher">Checks the code and hashes the new password.</param>
    /// <param name="time">The current time, for the code's fifteen minutes and the history row.</param>
    public static async Task<IResult> ResetWithCode(ResetWithCodeRequest request, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var user = await AccountOfAsync(request.Login, db);
        var code = await EmailCode.CheckAsync(user, EmailCodePurpose.ResetPassword, request.Code.Trim(), hasher, now, db);
        if (code == EmailCode.None)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "CodeWrong");
        }
        if (!PasswordPolicy.Holds(request.NewPassword, user.Username))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "WeakPassword");
        }

        code.Spend(now);
        await user.ResetPasswordAsync(request.NewPassword, mustChangePassword: false, hasher, db);
        db.AddHistory(user, HistoryAction.ResetPassword, user.Id, now);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>The account <paramref name="login"/> names by its username or its email (D5), the username first when one account's username is another's email; <see cref="User.None"/> when none.</summary>
    private static async Task<User> AccountOfAsync(string login, RedixDbContext db)
    {
        var typed = login.Trim();
        return await db.Users.Where(user => user.Username == typed || (user.Email == typed && user.Email != ""))
            .OrderByDescending(user => user.Username == typed).FirstOrDefaultAsync() ?? User.None;
    }
}

/// <summary>What «نسيت كلمة المرور؟» sends to ask for a code.</summary>
/// <param name="Login">The username or the email typed.</param>
public record ForgotPasswordRequest(string Login);

/// <summary>What «نسيت كلمة المرور؟» sends to reset the password with the code.</summary>
/// <param name="Login">The username or the email typed when the code was asked for.</param>
/// <param name="Code">The six digits the email carried.</param>
/// <param name="NewPassword">The password that replaces the forgotten one.</param>
public record ResetWithCodeRequest(string Login, string Code, string NewPassword);
