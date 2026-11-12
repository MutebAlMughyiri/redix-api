using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;

namespace Redix.Api.Auth;

/// <summary>Someone who signs in to REDIX (FR-01). It owns the sign-in rules: the password check and the lockout (AC-01.2).</summary>
public class User
{
    /// <summary>No user: what a lookup answers when there is none, so no caller holds a null.</summary>
    public static User None { get; } = new() { Username = "" };

    /// <summary>
    /// Adds a new login from what was typed, for the caller to save and write to the History table. A field outside its limit answers its code with status 400, a username already taken <c>usernameTaken</c> with 409;
    /// the user is <see cref="None"/> then.
    /// </summary>
    public static async Task<(User User, int Status, string Refusal)> AddNewAsync(string displayName, string username, string mobile, string password, bool mustChangePassword, RedixDbContext db, IPasswordHasher<User> hasher, DateTime now)
    {
        var name = displayName.Trim();
        var login = username.Trim();
        var phone = mobile.Trim();
        var refusal = RefusalOfNew(name, login, phone, password);
        if (refusal.Length > 0)
        {
            return (None, StatusCodes.Status400BadRequest, refusal);
        }
        if (await db.Users.AnyAsync(user => user.Username == login))
        {
            return (None, StatusCodes.Status409Conflict, "UsernameTaken");
        }

        var added = new User { Username = login, DisplayName = name, Mobile = phone, MustChangePassword = mustChangePassword, CreatedAt = now };
        added.SetPassword(password, hasher);
        db.Users.Add(added);
        return (added, 0, "");
    }

    /// <summary>Wrong passwords in a row that lock the account (AC-01.2).</summary>
    public const int WrongPasswordsBeforeLock = 5;

    /// <summary>How long a locked account stays locked (AC-01.2).</summary>
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

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
    [NotInHistory]
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
        return PasswordPolicy.Holds(temporaryPassword, username) ? RefusalOfDetails(displayName, mobile) : "WeakPassword";
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
