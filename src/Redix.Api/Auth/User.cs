using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Redix.Api.Audit;

namespace Redix.Api.Auth;

/// <summary>Someone who signs in to REDIX (FR-01). It owns the sign-in rules: the password check and the lockout (AC-01.2).</summary>
public class User
{
    /// <summary>No user: what a lookup answers when there is none, so no caller holds a null.</summary>
    public static User None { get; } = new() { Username = "" };

    /// <summary>Wrong passwords in a row that lock the account (AC-01.2).</summary>
    public const int WrongPasswordsBeforeLock = 5;

    /// <summary>How long a locked account stays locked (AC-01.2).</summary>
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    /// <summary>The shortest password accepted when a password is changed (UC-003); the BRS sets no rule, so this is a recorded decision.</summary>
    public const int MinimumPasswordLength = 8;

    /// <summary>The longest name a user may have.</summary>
    public const int NameMaxLength = 100;

    /// <summary>The longest username a user may have.</summary>
    public const int UsernameMaxLength = 50;

    /// <summary>The longest mobile a user may have.</summary>
    public const int MobileMaxLength = 20;

    /// <summary>The <see cref="LockedUntil"/> of an account that is not locked.</summary>
    public static readonly DateTime NeverLocked = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>The user's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The name the user signs in with; unique.</summary>
    public required string Username { get; init; }

    /// <summary>The name as it shows on entries.</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>The user's mobile; empty when none.</summary>
    public string Mobile { get; set; } = "";

    /// <summary>The password, hashed by ASP.NET Core's <see cref="PasswordHasher{TUser}"/>; never the password itself.</summary>
    [NotAudited]
    public string PasswordHash { get; private set; } = "";

    /// <summary>True while the password is a temporary one the user must change before anything else.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>False once the user is disabled, and can no longer sign in (AC-01.3).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Wrong passwords in a row since the last correct one or the last lock.</summary>
    public int FailedSignIns { get; private set; }

    /// <summary>The moment the lock ends; <see cref="NeverLocked"/> when the account is not locked.</summary>
    public DateTime LockedUntil { get; private set; } = NeverLocked;

    /// <summary>When the user was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Replaces the password; only its hash is kept.</summary>
    public void SetPassword(string password, IPasswordHasher<User> hasher) => PasswordHash = hasher.HashPassword(this, password);

    /// <summary>Whether <paramref name="password"/> has at least <see cref="MinimumPasswordLength"/> characters.</summary>
    public static bool IsLongEnough(string password) => password.Length >= MinimumPasswordLength;

    /// <summary>Whether <paramref name="newPassword"/> may replace <paramref name="currentPassword"/>: long enough, and different.</summary>
    public static bool IsAcceptableNewPassword(string newPassword, string currentPassword) => IsLongEnough(newPassword) && newPassword != currentPassword;

    /// <summary>Whether <paramref name="mobile"/>, already trimmed, may be kept, for a user or a supplier: empty, or ASCII digits with a leading plus at most, in at most <see cref="MobileMaxLength"/> characters. The column holds nothing else, so Arabic digits would become question marks.</summary>
    public static bool IsAcceptableMobile(string mobile) => mobile.Length <= MobileMaxLength && Regex.IsMatch(mobile, @"\A(\+?[0-9]+)?\z");

    /// <summary>The refusal of a name and a mobile, already trimmed, as its code; empty when both are acceptable.</summary>
    public static string RefusalOfDetails(string displayName, string mobile)
    {
        if (displayName.Length is 0 or > NameMaxLength)
        {
            return "InvalidName";
        }
        return IsAcceptableMobile(mobile) ? "" : "InvalidMobile";
    }

    /// <summary>The refusal of a new user's fields, already trimmed, as its code; empty when all are acceptable.</summary>
    public static string RefusalOfNew(string displayName, string username, string mobile, string temporaryPassword)
    {
        if (username.Length is 0 or > UsernameMaxLength)
        {
            return "InvalidUsername";
        }
        return IsLongEnough(temporaryPassword) ? RefusalOfDetails(displayName, mobile) : "WeakPassword";
    }

    /// <summary>Replaces the password with a temporary one: the user must change it, and a lock from wrong passwords ends.</summary>
    public void ResetPassword(string temporaryPassword, IPasswordHasher<User> hasher)
    {
        SetPassword(temporaryPassword, hasher);
        MustChangePassword = true;
        FailedSignIns = 0;
        LockedUntil = NeverLocked;
    }

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
