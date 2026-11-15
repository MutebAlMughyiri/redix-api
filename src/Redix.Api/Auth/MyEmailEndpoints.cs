using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Email;
using Redix.Api.History;

namespace Redix.Api.Auth;

/// <summary>
/// The signed-in person's own email, «بريدي» in Settings (redix-api #127, AC-10.4, R3): a code goes to the new address, and the address replaces the old only once its code is entered; until
/// then the old one holds. The admin sets his this way (D6).
/// </summary>
public static class MyEmailEndpoints
{
    /// <summary>Maps <c>POST /me/email</c>, limited by <see cref="EmailCode.AskingLimit"/>, and <c>POST /me/email/confirm</c>, for any working session.</summary>
    public static void MapMyEmail(this WebApplication app)
    {
        app.MapPost("/me/email", Ask).RequireAuthorization(SessionPolicies.Working).RequireRateLimiting(EmailCode.AskingLimit);
        app.MapPost("/me/email/confirm", Confirm).RequireAuthorization(SessionPolicies.Working);
    }

    /// <summary>
    /// Sends a six-digit code to the new address typed (AC-10.4), ending the earlier one (D3), in the language the phone asked in (AC-02.10); the email stays as it was. An address not well
    /// formed answers <c>emailInvalid</c>, and one another account holds <c>emailTaken</c> (R9).
    /// </summary>
    /// <param name="request">The new address.</param>
    /// <param name="principal">The signed-in session.</param>
    /// <param name="db">The database the person and the code live in.</param>
    /// <param name="hasher">Hashes the code, which is never kept.</param>
    /// <param name="sender">Sends the email.</param>
    /// <param name="time">The current time, for the code's fifteen minutes.</param>
    public static async Task<IResult> Ask(MyEmailRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, EmailSender sender, TimeProvider time)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var email = request.Email.Trim();
        var (status, refusal) = await User.RefusalOfEmailAsync(email, userId, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(status, refusal);
        }

        var user = await db.Users.SingleAsync(user => user.Id == userId);
        var code = await EmailCode.MakeAsync(user, EmailCodePurpose.ConfirmEmail, email, hasher, time.GetUtcNow().UtcDateTime, db);
        await db.SaveChangesAsync();
        await sender.SendAsync(email, "email.confirmSubject", "email.confirmBody", code, db);
        return Results.Accepted();
    }

    /// <summary>
    /// Makes the address the last code went to the person's email once that code is entered (AC-10.4), spending it and ending the codes sent before, with its History line. A code
    /// wrong, expired, spent, out of tries or never asked for answers <c>codeWrong</c>; an address another account took meanwhile, <c>emailTaken</c> (R9).
    /// </summary>
    /// <param name="request">The six digits the email carried.</param>
    /// <param name="principal">The signed-in session.</param>
    /// <param name="db">The database the person and the code live in.</param>
    /// <param name="hasher">Checks the code.</param>
    /// <param name="time">The current time, for the code's fifteen minutes and the history row.</param>
    public static async Task<IResult> Confirm(ConfirmMyEmailRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var user = await db.Users.SingleAsync(user => user.Id == AccessTokens.UserIdOf(principal));
        var code = await EmailCode.CheckAsync(user, EmailCodePurpose.ConfirmEmail, request.Code.Trim(), hasher, now, db);
        if (code == EmailCode.None)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "CodeWrong");
        }
        var (status, refusal) = await User.RefusalOfEmailAsync(code.Email, user.Id, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(status, refusal);
        }

        code.Spend(now);
        await user.ChangeEmailAsync(code.Email, db);
        db.AddHistory(user, HistoryAction.Update, user.Id, now);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

/// <summary>What «بريدي» sends to ask for a code to a new address.</summary>
/// <param name="Email">The new address.</param>
public record MyEmailRequest(string Email);

/// <summary>What «بريدي» sends to make the new address hold.</summary>
/// <param name="Code">The six digits the email to the new address carried.</param>
public record ConfirmMyEmailRequest(string Code);
