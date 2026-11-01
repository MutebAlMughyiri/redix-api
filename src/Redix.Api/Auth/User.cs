using Microsoft.AspNetCore.Identity;
using Redix.Api.Audit;

namespace Redix.Api.Auth;

/// <summary>Someone who signs in to REDIX (FR-01). It owns the sign-in rules: the password check and the lockout (AC-01.2).</summary>
public class User
{
    /// <summary>Wrong passwords in a row that lock the account (AC-01.2).</summary>
    public const int WrongPasswordsBeforeLock = 5;

    /// <summary>How long a locked account stays locked (AC-01.2).</summary>
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    /// <summary>The shortest password accepted when a password is changed (UC-003); the BRS sets no rule, so this is a recorded decision.</summary>
    public const int MinimumPasswordLength = 8;

    /// <summary>The <see cref="LockedUntil"/> of an account that is not locked.</summary>
    public static readonly DateTime NeverLocked = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>The user's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The name the user signs in with; unique.</summary>
    public required string Username { get; init; }

    /// <summary>The password, hashed by ASP.NET Core's <see cref="PasswordHasher{TUser}"/>; never the password itself.</summary>
    [NotAudited]
    public string PasswordHash { get; private set; } = "";

    /// <summary>True while the password is a temporary one the user must change before anything else.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>An owner is a financial supervisor in every project and manages the global settings (BR-29).</summary>
    public bool IsOwner { get; init; }

    /// <summary>False once a financial supervisor disables the user, who can then no longer sign in (AC-01.3).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Wrong passwords in a row since the last correct one or the last lock.</summary>
    public int FailedSignIns { get; private set; }

    /// <summary>The moment the lock ends; <see cref="NeverLocked"/> when the account is not locked.</summary>
    public DateTime LockedUntil { get; private set; } = NeverLocked;

    /// <summary>When the user was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Replaces the password; only its hash is kept.</summary>
    public void SetPassword(string password, IPasswordHasher<User> hasher) => PasswordHash = hasher.HashPassword(this, password);

    /// <summary>Whether <paramref name="newPassword"/> may replace <paramref name="currentPassword"/>: at least <see cref="MinimumPasswordLength"/> characters, and different.</summary>
    public static bool IsAcceptableNewPassword(string newPassword, string currentPassword) =>
        newPassword.Length >= MinimumPasswordLength && newPassword != currentPassword;

    /// <summary>
    /// Checks <paramref name="password"/> at <paramref name="now"/>. A disabled user is refused like a wrong password, so the answer never tells which.
    /// The fifth wrong password in a row locks the account for <see cref="LockDuration"/>; a correct one clears the count.
    /// </summary>
    public SignInOutcome SignIn(string password, IPasswordHasher<User> hasher, DateTime now)
    {
        if (!IsActive)
        {
            return SignInOutcome.InvalidCredentials;
        }
        if (LockedUntil > now)
        {
            return SignInOutcome.AccountLocked;
        }

        var verification = hasher.VerifyHashedPassword(this, PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            FailedSignIns++;
            if (FailedSignIns == WrongPasswordsBeforeLock)
            {
                LockedUntil = now + LockDuration;
                FailedSignIns = 0;
            }
            return SignInOutcome.InvalidCredentials;
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            SetPassword(password, hasher);
        }
        FailedSignIns = 0;
        return SignInOutcome.Success;
    }
}
