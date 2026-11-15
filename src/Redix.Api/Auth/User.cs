using System.Net.Mail;
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

    /// <summary>The longest email a user may have, the longest an address may be.</summary>
    public const int EmailMaxLength = 254;

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

    /// <summary>
    /// Where a code reaches the user (redix-api #127, R3, D1): their manager types it when adding them, and they change it in Settings once a code to the new address is entered;
    /// unique where set (R9). Empty while nobody has typed one, as for the accounts made before it, the admin and whoever registered a project, until they set it (D6).
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>The password, hashed by ASP.NET Core's <see cref="PasswordHasher{TUser}"/>; never the password itself.</summary>
    [NotInHistory]
    public string PasswordHash { get; private set; } = "";

    /// <summary>True while the password is a temporary one the user must change before anything else.</summary>
    public bool MustChangePassword { get; set; }

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

    /// <summary>
    /// The refusal of <paramref name="email"/>, already trimmed, as the email of <paramref name="userId"/> (redix-api #127, AC-07.8, R9): <c>emailInvalid</c> with 400 unless it is one
    /// well-formed address of at most <see cref="EmailMaxLength"/> characters, as .NET's <see cref="MailAddress"/> reads one, and <c>emailTaken</c> with 409 when another account holds it,
    /// whatever its case; empty when it may be kept.
    /// </summary>
    public static async Task<(int Status, string Refusal)> RefusalOfEmailAsync(string email, Guid userId, RedixDbContext db)
    {
        if (email.Length > EmailMaxLength || !MailAddress.TryCreate(email, out var address) || address.Address != email)
        {
            return (StatusCodes.Status400BadRequest, "EmailInvalid");
        }
        return await db.Users.AnyAsync(user => user.Email == email && user.Id != userId) ? (StatusCodes.Status409Conflict, "EmailTaken") : (0, "");
    }

    /// <summary>
    /// Makes <paramref name="email"/>, already checked, the person's email, by their manager (AC-07.8) or by themselves once its code is entered (AC-10.4). Another address ends every code
    /// sent before it, so none sent to the old address, nor to one never confirmed, works any more (redix-api #127); the same address ends none. The caller saves.
    /// </summary>
    public async Task ChangeEmailAsync(string email, RedixDbContext db)
    {
        if (email == Email)
        {
            return;
        }
        Email = email;
        await EmailCode.EndEveryOneOfAsync(this, db);
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

    /// <summary>
    /// Replaces a forgotten password: a lock from wrong passwords ends and every device is signed out (UC-007, redix-api #127 R7, AC-02.9). A temporary one, which a manager types, must be
    /// changed at the next sign-in (AC-02.1); one the person chose with a code by email need not (AC-02.6). The caller saves.
    /// </summary>
    public async Task ResetPasswordAsync(string password, bool mustChangePassword, IPasswordHasher<User> hasher, RedixDbContext db)
    {
        await db.Devices.Where(device => device.UserId == Id).ExecuteUpdateAsync(device => device.SetProperty(d => d.IsRevoked, true));
        SetPassword(password, hasher);
        MustChangePassword = mustChangePassword;
        FailedSignIns = 0;
        LockedUntil = NeverLocked;
    }

    /// <summary>
    /// Checks <paramref name="password"/> at <paramref name="now"/>. The fifth wrong password in a row locks the account for <see cref="LockDuration"/>; a correct one clears the count
    /// (AC-01.2). Someone inactive in his projects signs in as anyone does, to his project list (redix-api #126, D3).
    /// </summary>
    public SignInOutcome SignIn(string password, IPasswordHasher<User> hasher, DateTime now)
    {
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
