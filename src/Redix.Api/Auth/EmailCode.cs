using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>
/// A code sent by email, which proves the person reads that address (redix-api #127, the rule): REDIX sends six digits, keeps only their hash, and accepts them once, within
/// <see cref="Lifetime"/> and <see cref="TriesAllowed"/> tries; a new code ends the earlier one of the same purpose (D3, AC-02.8). The one owner that makes, checks and spends every code.
/// </summary>
public class EmailCode
{
    /// <summary>The name of the limit on asking for a code (D4): per caller, as registration's, and, for a forgotten password, per login typed.</summary>
    public const string AskingLimit = nameof(AskingLimit);

    /// <summary>How often one login may be typed into «نسيت كلمة المرور؟» in an hour, whoever types it and whether or not it names an account (D4, AC-02.8).</summary>
    public const int AsksPerLoginAnHour = 5;

    /// <summary>How long a code works after it is sent (D3).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    /// <summary>The tries a code allows; every try counts, and once they are spent even the right code is refused (D3).</summary>
    public const int TriesAllowed = 5;

    /// <summary>The <see cref="UsedAt"/> of a code not spent yet.</summary>
    public static readonly DateTime NotUsed = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>No code: what a check answers when none is live or the typed one is wrong, so no caller holds a null.</summary>
    public static EmailCode None { get; } = new();

    /// <summary>The code's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The person it was sent for.</summary>
    public Guid UserId { get; init; }

    /// <summary>What entering it does.</summary>
    public EmailCodePurpose Purpose { get; init; }

    /// <summary>The address it was sent to: the account's email for a reset, the new address for a confirmation.</summary>
    public string Email { get; init; } = "";

    /// <summary>The code, hashed by ASP.NET Core's <see cref="PasswordHasher{TUser}"/> as a password is; never the code itself.</summary>
    public string CodeHash { get; private init; } = "";

    /// <summary>The moment it stops working.</summary>
    public DateTime ExpiresAt { get; private init; }

    /// <summary>The wrong codes typed against it.</summary>
    public int Tries { get; private set; }

    /// <summary>When it was spent; <see cref="NotUsed"/> until then.</summary>
    public DateTime UsedAt { get; private set; } = NotUsed;

    /// <summary>
    /// Makes a new six-digit code for <paramref name="user"/> to enter for <paramref name="purpose"/>, to be sent to <paramref name="email"/>, and ends the earlier one of the same purpose (D3).
    /// Returns the code; only its hash is kept. The caller saves, then sends it.
    /// </summary>
    public static async Task<string> MakeAsync(User user, EmailCodePurpose purpose, string email, IPasswordHasher<User> hasher, DateTime now, RedixDbContext db)
    {
        db.EmailCodes.RemoveRange(await db.EmailCodes.Where(earlier => earlier.UserId == user.Id && earlier.Purpose == purpose).ToListAsync());
        var code = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        db.EmailCodes.Add(new EmailCode { UserId = user.Id, Purpose = purpose, Email = email, CodeHash = hasher.HashPassword(user, code), ExpiresAt = now + Lifetime });
        return code;
    }

    /// <summary>
    /// The code of <paramref name="purpose"/> <paramref name="user"/> was last sent, when <paramref name="typed"/> is it and it is unspent, unexpired and has tries left (D3, AC-02.8);
    /// <see cref="None"/> otherwise. Every try counts, and is saved before the code is checked: the row's version lets one of two tries sent at once count and refuses the other,
    /// so tries sent together cannot check more than <see cref="TriesAllowed"/> guesses.
    /// </summary>
    public static async Task<EmailCode> CheckAsync(User user, EmailCodePurpose purpose, string typed, IPasswordHasher<User> hasher, DateTime now, RedixDbContext db)
    {
        var last = await db.EmailCodes.Where(code => code.UserId == user.Id && code.Purpose == purpose).OrderByDescending(code => code.Id).FirstOrDefaultAsync() ?? None;
        if (last == None || last.UsedAt != NotUsed || last.ExpiresAt <= now || last.Tries >= TriesAllowed)
        {
            return None;
        }
        last.Tries++;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return None;
        }
        return hasher.VerifyHashedPassword(user, last.CodeHash, typed) == PasswordVerificationResult.Failed ? None : last;
    }

    /// <summary>Spends the code at <paramref name="now"/>, so it never works again (D3). The caller saves.</summary>
    public void Spend(DateTime now) => UsedAt = now;

    /// <summary>Ends every code sent for <paramref name="user"/>, whatever its purpose, as when their email changes (redix-api #127). The caller saves.</summary>
    public static async Task EndEveryOneOfAsync(User user, RedixDbContext db) => db.EmailCodes.RemoveRange(await db.EmailCodes.Where(code => code.UserId == user.Id).ToListAsync());
}
